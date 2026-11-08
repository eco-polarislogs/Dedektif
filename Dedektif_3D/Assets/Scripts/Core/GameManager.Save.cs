using System.Linq;
using Dedektif3D.World;
using UnityEngine;

namespace Dedektif3D.Core
{
    /// <summary>Soruşturma kaydı: otomatik kayıt ve kaldığı yerden devam.</summary>
    public partial class GameManager
    {
        const float AutosaveInterval = 30f;   // ani kapanmada en fazla bu kadar ilerleme kaybolur
        const float MinSaveGap = 2f;

        bool _saveDirty;
        bool _caseClosed;   // suçlama yapıldı: bu dava artık kaydedilmez
        bool _resumed;      // kayıttan açıldı: katil sunucuda değil kayıtta bulunur
        float _nextSaveAllowed, _nextPeriodicSave;

        /// <summary>Önemli bir olay oldu: ilk uygun anda kaydet.</summary>
        void MarkDirty() => _saveDirty = true;

        void TickAutosave()
        {
            float now = Time.unscaledTime;
            if (now >= _nextPeriodicSave) _saveDirty = true;
            if (_saveDirty && now >= _nextSaveAllowed) SaveCaseNow();
        }

        void SaveCaseNow()
        {
            if (Phase != GamePhase.Playing || _caseClosed || Town == null || Town.Data == null) return;
            _saveDirty = false;
            float now = Time.unscaledTime;
            _nextSaveAllowed = now + MinSaveGap;
            _nextPeriodicSave = now + AutosaveInterval;
            SaveSystem.Write(CaptureCase());
        }

        void OnApplicationQuit() => SaveCaseNow();
        void OnApplicationPause(bool paused) { if (paused) SaveCaseNow(); }
        void OnApplicationFocus(bool focused) { if (!focused) SaveCaseNow(); }

        CaseSave CaptureCase()
        {
            var dn = DayNightCycle.Instance;
            var s = new CaseSave
            {
                townId = Town.Data.Id,
                guiltyId = GuiltyId,
                labCount = LabCount,
                corpseLook = _corpseLook,
                notes = DetectiveNotes,
                autopsyTimerStarted = AutopsyTimerStarted,
                autopsyReady = AutopsyReady,
                autopsyTimeLeft = AutopsyTimeLeft,
                autopsyReport = AutopsyReport,
                hour = dn != null ? dn.Hour : DayNightCycle.CaseStartHour,
                day = dn != null ? dn.Day : 1,
                elapsedMinutes = dn != null ? dn.ElapsedMinutes : 0f,
                buildingNpcId = CurrentBuilding != null ? CurrentBuilding.NpcId : 0,
                playerPos = Player.transform.position,
                playerRot = Player.transform.rotation,
                bag = _bagOrder.ToList(),
                visited = _visited.ToList(),
                innocent = _innocent.ToList(),
                helperContexts = _shownHelperContexts.ToList(),
            };
            foreach (var c in _clues.Values)
            {
                s.clues.Add(new ClueSave
                {
                    id = c.Id,
                    inBag = c.InBag,
                    revealed = c.View != null && c.View.revealed,
                    bloodFound = c.BloodFound, printFound = c.PrintFound,
                    bloodChecked = c.BloodChecked, printChecked = c.PrintChecked,
                    bloodSent = c.BloodSent, printSent = c.PrintSent,
                });
            }
            foreach (var n in _npcs.Values)
            {
                var ns = new NpcSave { id = n.Id, stress = n.Stress, asked = n.Asked, wokenUp = n.WokenUp, askedQuestions = n.AskedQuestions.ToList() };
                foreach (var line in n.Log) ns.log.Add(new LineSave { fromPlayer = line.FromPlayer, speaker = line.Speaker, text = line.Text });
                s.npcs.Add(ns);
            }
            foreach (var spot in _spots)
                if (spot != null && spot.def != null)
                    s.spots.Add(new SpotSave { clueId = spot.def.ClueId, sensed = spot.Sensed, opened = spot.Opened });
            return s;
        }

        /// <summary>Kayıttan devam: kasabayı kurar, durumu geri yükler ve doğrudan oyuna girer (hikaye ve iniş atlanır).</summary>
        public void ResumeCase(string townId)
        {
            var save = SaveSystem.Read(townId);
            if (save == null)
            {
                SaveSystem.Delete(townId);
                UI.MessageBox("KAYIT OKUNAMADI", "Bu soruşturmanın kaydı bozulmuş. Davayı bölge haritasından yeniden açabilirsiniz.", true);
                return;
            }
            AudioManager.Instance?.StartRain();
            UI.FadeTransition(null, Color.black, () =>
            {
                ExitMap();
                LoadTown(townId);
                ResetCaseState();
                ApplySave(save);
                Director?.Stop();
                Player.SetVisible(true);
                SetPhase(GamePhase.Playing);
                _nextPeriodicSave = Time.unscaledTime + AutosaveInterval;
                UI.Toast($"{GameData.TownName}: soruşturma kaldığınız yerden sürüyor.");
            });
        }

        void ApplySave(CaseSave s)
        {
            _resumed = true;
            GuiltyId = GetNpc(s.guiltyId) != null ? s.guiltyId : GuiltyId;
            LabCount = s.labCount;
            _corpseLook = s.corpseLook;
            DetectiveNotes = s.notes ?? "";
            AutopsyTimerStarted = s.autopsyTimerStarted;
            AutopsyReady = s.autopsyReady;
            AutopsyTimeLeft = s.autopsyTimeLeft;
            AutopsyReport = s.autopsyReport;
            DayNightCycle.Instance?.RestoreTime(s.hour, s.day, s.elapsedMinutes);

            // Oyun güncellemesiyle kaldırılmış delil veya şüpheliler sessizce atlanır.
            foreach (var cs in s.clues)
            {
                var c = GetClue(cs.id);
                if (c == null) continue;
                c.InBag = cs.inBag;
                c.BloodFound = cs.bloodFound; c.PrintFound = cs.printFound;
                c.BloodChecked = cs.bloodChecked; c.PrintChecked = cs.printChecked;
                c.BloodSent = cs.bloodSent; c.PrintSent = cs.printSent;
                c.View?.SetRevealed(cs.revealed, false);
            }
            foreach (var id in s.bag)
                if (GetClue(id) != null && !_bagOrder.Contains(id)) _bagOrder.Add(id);
            foreach (var ns in s.npcs)
            {
                var n = GetNpc(ns.id);
                if (n == null) continue;
                n.Stress = ns.stress; n.Asked = ns.asked; n.WokenUp = ns.wokenUp;
                foreach (var q in ns.askedQuestions) n.AskedQuestions.Add(q);
                // Eski satırlar yeniden daktilo edilmesin diye geçmiş bir zamana alınır.
                foreach (var l in ns.log) n.Log.Add(new DialogueLine { FromPlayer = l.fromPlayer, Speaker = l.speaker, Text = l.text ?? "", StartTime = -100000f });
            }
            foreach (var spot in _spots)
            {
                if (spot == null || spot.def == null) continue;
                var ss = s.spots.Find(x => x.clueId == spot.def.ClueId);
                if (ss != null) spot.RestoreState(ss.sensed, ss.opened);
            }
            foreach (var key in s.helperContexts) _shownHelperContexts.Add(key);
            foreach (var id in s.visited)
                if (GetNpc(id) != null) _visited.Add(id);
            foreach (var id in s.innocent)
                if (GetNpc(id) != null) _innocent.Add(id);

            foreach (var kv in Town.Buildings)
            {
                if (kv.Value.StatusLabel == null) continue;
                if (IsInnocent(kv.Key)) kv.Value.StatusLabel.text = $"{GetNpc(kv.Key).Def.Name.ToUpperInvariant()} MASUM";
                else if (IsVisited(kv.Key)) kv.Value.StatusLabel.text = "İNCELEME TAMAMLANDI";
            }

            // İçerideyse odanın girişinden, dışarıdaysa kaydedilen yerden devam eder.
            if (s.buildingNpcId > 0 && Town.Interiors.TryGetValue(s.buildingNpcId, out var inst))
            {
                CurrentBuilding = inst.Def;
                Player.Teleport(inst.Spawn.position, inst.Spawn.rotation);
                SetIndoor(true);
                Search?.OnEnterRoom();
            }
            else
            {
                CurrentBuilding = null;
                if (IsFreeSpot(s.playerPos)) Player.Teleport(s.playerPos, s.playerRot);
                else Player.Teleport(Town.PlayerSpawn, Town.PlayerSpawnRotation);
                SetIndoor(false);
            }
            if (Api.IsOnline) Api.StartSession(GuiltyId);
        }

        /// <summary>Güncelleme sonrası kasaba düzeni değiştiyse oyuncu bir binanın içinde başlamasın.</summary>
        static bool IsFreeSpot(Vector3 feet)
        {
            int mask = ~((1 << 2) | (1 << TownGenerator.NpcBarrierLayer));
            return !Physics.CheckCapsule(feet + Vector3.up * 0.6f, feet + Vector3.up * 1.5f, 0.3f, mask, QueryTriggerInteraction.Ignore);
        }
    }
}
