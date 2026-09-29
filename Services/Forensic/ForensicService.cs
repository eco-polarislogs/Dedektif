using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DedektiflikRPG.Core.Interfaces;
using DedektiflikRPG.Models;

namespace DedektiflikRPG.Services.Forensic;

public class ForensicService : IForensicService
{
    // Kasabaya özel adli bulgular listeleri (izole)
    private readonly List<string> _gizemliFindings = new();
    private readonly List<string> _golgeFindings = new();
    private readonly List<string> _sisorenFindings = new();

    public void ClearFindings()
    {
        lock (_gizemliFindings) { _gizemliFindings.Clear(); }
        lock (_golgeFindings) { _golgeFindings.Clear(); }
        lock (_sisorenFindings) { _sisorenFindings.Clear(); }
    }

    public void ClearGolgeFindings()
    {
        lock (_golgeFindings) { _golgeFindings.Clear(); }
    }

    public void ClearGizemliFindings()
    {
        lock (_gizemliFindings) { _gizemliFindings.Clear(); }
    }

    public void ClearSisorenFindings()
    {
        lock (_sisorenFindings) { _sisorenFindings.Clear(); }
    }

    public void SubmitFinding(int clueId, string clueName, string findingText, List<NPC> npcs, int guiltyId)
    {
        if (string.IsNullOrWhiteSpace(findingText)) return;

        bool isSisoren = (clueId >= 2000);
        bool isGolge = (!isSisoren && clueId >= 1000);
        int ownerNpcId = isSisoren ? (clueId / 10) : (isGolge ? (clueId / 10) : Math.Clamp((clueId - 1) / 3 + 1, 1, 5));
        bool isGuiltyClue = (ownerNpcId == guiltyId);
        bool isFingerprint = findingText.Contains("PARMAK İZİ", StringComparison.OrdinalIgnoreCase);
        bool isBlood = findingText.Contains("KAN LEKESİ", StringComparison.OrdinalIgnoreCase);

        string sampleCode = (isGolge || isSisoren) ? $"NUMUNE #{clueId} ({clueName})" : (clueId switch
        {
            1 => "NUMUNE #01 (Masif Çelik Kesici Alet)",
            2 => "NUMUNE #02 (Veresiye Belgesi / Defter)",
            3 => "NUMUNE #03 (Tekstil Dokuma Önlük)",
            4 => "NUMUNE #04 (Cam Solüsyon Şişesi)",
            5 => "NUMUNE #05 (Reçete Notu)",
            6 => "NUMUNE #06 (Organik Bitkisel Örnek)",
            7 => "NUMUNE #07 (Yazılı Mektup Belgesi)",
            8 => "NUMUNE #08 (Optik Polimer Çerçeve)",
            9 => "NUMUNE #09 (Metal Kasa Bölmesi)",
            10 => "NUMUNE #10 (Metal Simgesel Rozet)",
            11 => "NUMUNE #11 (Resmi Tahkikat Belgesi)",
            12 => "NUMUNE #12 (Palto Düğmesi)",
            13 => "NUMUNE #13 (Sentetik İplik Lif Bobini)",
            14 => "NUMUNE #14 (Yün Kumaş Parçası)",
            15 => "NUMUNE #15 (Astar İçi Dikiş Bölmesi)",
            _ => $"NUMUNE #{clueId:D2} (Olay Yeri Materyali)"
        });

        string entry;

        if (isFingerprint)
        {
            if (isGuiltyClue)
            {
                entry = $"[🔬 DAKTİLOGRAFİK İNCELEME - {sampleCode}]: Nesne yüzeyindeki gizli parmak izi, EMNİYET KAYITLARINDAKİ ŞÜPHELİ PROFİLİ İLE %99.8 EŞLEŞTİ. (Cinayet mahallindeki doğrudan şüpheli teması doğrulanmıştır).";
            }
            else
            {
                entry = $"[🔬 DAKTİLOGRAFİK İNCELEME - {sampleCode}]: Nesne yüzeyindeki izler incelendi. Parmak izi rutin günlük kullanıcı teması ile uyumludur. (Cinayet anına ait şüpheli temas saptanmamıştır).";
            }
        }
        else if (isBlood)
        {
            if (isGuiltyClue)
            {
                string victimName = isSisoren ? "Halil Efendi" : (isGolge ? "Ekrem Bey" : "Osman Bey");
                entry = $"[🧬 SEROLOJİK DNA ANALİZİ - {sampleCode}]: Numunedeki kan lekesi ve DNA serotipi, kurban {victimName}'in kan profili ile TAM EŞLEŞTİ. (Cinayet anı arbede/temas lekesi kesinleşmiştir).";
            }
            else
            {
                entry = $"[🧬 SEROLOJİK DNA ANALİZİ - {sampleCode}]: Serolojik test tamamlandı. Lekedeki biyolojik yapı kurbana ait DEĞİLDİR (Büyükbaş/hayvan kanı, boya veya rutin iş materyalidir - Cinayet ile ilişkisi saptanmamıştır).";
            }
        }
        else
        {
            entry = $"[🔬 ADLİ LAB İNCELEMESİ - {sampleCode}]: {findingText}";
        }

        // Kasabaya göre doğru listeye ekle
        var targetList = isSisoren ? _sisorenFindings : (isGolge ? _golgeFindings : _gizemliFindings);
        lock (targetList)
        {
            if (!targetList.Contains(entry))
            {
                targetList.Add(entry);
            }
        }
    }

    public Task<string> GenerateAutopsyReportAsync(List<NPC> npcs, int guiltyId)
    {
        var guiltyNpc = npcs.FirstOrDefault(n => n.NPCId == guiltyId);
        string guiltyName = guiltyNpc?.Name ?? "Bilinmiyor";
        bool isSisoren = (guiltyId >= 200 && guiltyId < 300);
        bool isGolge = (!isSisoren && guiltyId >= 100);

        string victimName = isSisoren ? "Madenci Halil Efendi (52, Erkek)" : (isGolge ? "Ekrem Bey (62, Erkek)" : "Osman Bey (58, Erkek)");
        string deathTime = isSisoren ? "02:00 - 02:30 (Yoğun Sis - Maden Yolu)" : (isGolge ? "02:00 - 02:30 (Çam Ormanı Yolu / Göl Kenarı)" : "23:45 - 00:30 (Mantarlaşma & Rigor Mortis)");

        string reportHtml = $@"
        <div class='autopsy-dossier'>
            <div class='autopsy-paper-header'>
                <div class='autopsy-official-seal'>
                    <i class='fa-solid fa-scale-balanced'></i> T.C. ADLİ TIP KURUMU OTOPSİ VE ADLİ BİLİMLER BAŞKANLIĞI
                </div>
                <div class='autopsy-dossier-no'>RESMİ OTOPSİ PROTOKOL DOSYASI #{(isSisoren ? "301-S" : (isGolge ? "208-G" : "104-B"))}</div>
            </div>
            
            <div class='autopsy-meta-grid'>
                <div class='autopsy-meta-item'><strong>KURBAN:</strong> {victimName}</div>
                <div class='autopsy-meta-item'><strong>TAHMİNİ ÖLÜM SAATİ:</strong> {deathTime}</div>
                <div class='autopsy-meta-item'><strong>OTOPSİ UZMANI:</strong> Dr. Selim Karaca (Adli Tabip)</div>
                <div class='autopsy-meta-item'><strong>GİZLİLİK DERECESİ:</strong> <span class='autopsy-alert-tag'>🚨 ÇOK GİZLİ / İL MÜHÜRLÜ</span></div>
            </div>

            <div class='autopsy-divider-line'></div>

            <div class='autopsy-sec-title'><i class='fa-solid fa-microscope'></i> OTOPSİ & ADLİ PATOLOJİ BULGULARI</div>
            <div class='autopsy-main-finding'>";

        if (isSisoren)
        {
            switch (guiltyId)
            {
                case 201: // Telgrafçı
                    reportHtml += @"
                    <div class='autopsy-bullet'><span class='bullet-tag'>[ASFİKSİ ANOMALİSİ]</span> Kurbanın boyun çevresinde kalın, plastik dokulu bir kabloyla oluşan derin strangülasyon (boğulma) izi saptanmıştır.</div>
                    <div class='autopsy-bullet'><span class='bullet-tag'>[TRAVMA ANALİZİ]</span> Boğulmadan önce kafa arkasına ağır ve künt bir cisimle vurularak kurbanın sersemletildiği tespit edilmiştir.</div>
                    <div class='autopsy-bullet'><span class='bullet-tag'>[KİMYASAL İZ]</span> Kurbanın yaka kısmında telgraf mürekkebi pigmentleri ve failin kanlı eldivensiz parmak izleri izole edilmiştir.</div>";
                    break;
                case 202: // Kahveci
                    reportHtml += @"
                    <div class='autopsy-bullet'><span class='bullet-tag'>[PENETRASYON TRAVMASI]</span> Göğüs kafesi sol anterior bölgesinde yakın mesafeden ateşlenmiş kurşun giriş yarası saptanmıştır.</div>
                    <div class='autopsy-bullet'><span class='bullet-tag'>[KİMYASAL ANOMALİ]</span> Yaranın etrafındaki barut yanıklarının içinde yoğun kahve telvesi ve demlenmiş çay yaprağı kalıntıları bulunmuştur.</div>
                    <div class='autopsy-bullet'><span class='bullet-tag'>[MİKROSKOBİK TESPİT]</span> Olay yerinden elde edilen koku numunelerinde yanmış yün ve kumaş (kasket) dokusuna rastlanmıştır.</div>";
                    break;
                case 203: // Sinemacı
                    reportHtml += @"
                    <div class='autopsy-bullet'><span class='bullet-tag'>[MEKANİK TRAVMA]</span> Kafatasının arka kısmında çok ağır ve çelik bir silindir (bobin) ile tek seferde vurulmaya bağlı masif kırıklar tespit edilmiştir.</div>
                    <div class='autopsy-bullet'><span class='bullet-tag'>[FİZİKSEL BULGU]</span> Kurbanın boynunda selüloit film şeridinin sürtünmesinden kaynaklı kesikler izole edilmiştir.</div>
                    <div class='autopsy-bullet'><span class='bullet-tag'>[DOKU ANALİZİ]</span> Kurbanın tırnak aralarında faile ait pahalı deri eldiven dokusu bulunmuştur.</div>";
                    break;
                case 204: // Bakkal
                    reportHtml += @"
                    <div class='autopsy-bullet'><span class='bullet-tag'>[TOKSİKOLOJİK ROL]</span> Mide ve bağırsak sisteminde yüksek dozda arsenik (ağır endüstriyel fare zehri) kalıntısı saptanmıştır.</div>
                    <div class='autopsy-bullet'><span class='bullet-tag'>[POZOLOJİK UYGULAMA]</span> Zehrin kurbana doğrudan verilmediği, maktulün düzenli kullandığı vitamin kapsüllerinin içine ustalıkla yerleştirildiği belirlenmiştir.</div>
                    <div class='autopsy-bullet'><span class='bullet-tag'>[İZ ANALİZİ]</span> Kurbanın evinde bulunan borç senetlerinin üzerinde faile ait deri döküntüleri ve ter izleri bulunmuştur.</div>";
                    break;
                case 205: // Sahaf
                    reportHtml += @"
                    <div class='autopsy-bullet'><span class='bullet-tag'>[POSTÜRAL TRAVMA]</span> Başın parietal kemiğinde, içi oyulmuş kalın bir kitapla (içinde ağır bir nesne barındıran) vurulmaya bağlı çökme kırığı saptanmıştır.</div>
                    <div class='autopsy-bullet'><span class='bullet-tag'>[EPİDERMAL MİKROSKOBİ]</span> Yaranın içinde yüzlerce yıllık eski kağıt tozu, selüloz ve deri cilt kalıntıları izole edilmiştir.</div>
                    <div class='autopsy-bullet'><span class='bullet-tag'>[KİMYASAL İZ]</span> Olay yerinde bulunan kırık cep saatinin kapağında faile ait DNA örneği içeren kurumuş kan damlası tespit edilmiştir.</div>";
                    break;
                case 206: // Muhtar
                    reportHtml += @"
                    <div class='autopsy-bullet'><span class='bullet-tag'>[TRAVMA & PENETRASYON]</span> Kurbanın önce şakağına ağır pirinç bir nesne (mühür) ile vurulduğu, ardından yakın mesafeden tek el ateş edilerek öldürüldüğü saptanmıştır.</div>
                    <div class='autopsy-bullet'><span class='bullet-tag'>[KİMYASAL ANOMALİ]</span> Baş bölgesindeki künt yaranın etrafında kırılmış resmi mühürden dökülen pirinç parçacıkları ve ıslak mürekkep izole edilmiştir.</div>
                    <div class='autopsy-bullet'><span class='bullet-tag'>[BALİSTİK İNCELEME]</span> Olay yerindeki süpürgeliğin dibinde bulunan boş kovanın balistik izleri failin silahıyla tam eşleşmektedir.</div>";
                    break;
                case 207: // Tütüncü
                    reportHtml += @"
                    <div class='autopsy-bullet'><span class='bullet-tag'>[SOLUNUM PARALİZİ]</span> Post-mortem analizde kurbanın akciğerlerinde şiddetli solunum felcine yol açan toksik sıvı buharı kalıntısı saptanmıştır.</div>
                    <div class='autopsy-bullet'><span class='bullet-tag'>[POZOLOJİK UYGULAMA]</span> Toksik sıvının kurbanın puro tütününe emdirildiği ve puro yakıldığında ısı ile birlikte ölümcül bir gaza dönüştüğü anlaşılmıştır.</div>
                    <div class='autopsy-bullet'><span class='bullet-tag'>[FİZİKSEL BULGU]</span> Ölüm öncesi yaşanan kısa arbedede kurbanın failin gümüş tütün tabakasına vurarak onu yaraladığı ve tabakada kan izi bıraktığı saptanmıştır.</div>";
                    break;
                case 208: // Çoban
                    reportHtml += @"
                    <div class='autopsy-bullet'><span class='bullet-tag'>[HEMORAJİK ŞOK]</span> Kurbanın şah damarına saplanan çok keskin ve uzun, paslı bir metal (nalbant çivisi) nedeniyle anında kan kaybından öldüğü saptanmıştır.</div>
                    <div class='autopsy-bullet'><span class='bullet-tag'>[FİZİKSEL BULGU]</span> Maktulün at eyerinin kasten yarıya kadar kesildiği, atın düşürülmesi sonrası failin çiviyi eliyle doğrudan sapladığı belirlenmiştir.</div>
                    <div class='autopsy-bullet'><span class='bullet-tag'>[İZ ANALİZİ]</span> Olay yeri incelemesinde çevreye atılmış saplı bir çakının kabzasında faile ait deri DNA'sı bulunmuştur.</div>";
                    break;
                case 209: // Tüpçü
                    reportHtml += @"
                    <div class='autopsy-bullet'><span class='bullet-tag'>[KÜNT TRAVMA & KARBONMONOKSİT]</span> Kurbanın önce ağır bir İngiliz anahtarıyla kafasına vurularak bayıltıldığı, sonrasında ortama sızan propan gazı zehirlenmesinden hayatını kaybettiği tespit edilmiştir.</div>
                    <div class='autopsy-bullet'><span class='bullet-tag'>[KİMYASAL ANOMALİ]</span> Yaranın iç dokularında pas ve ağır makine yağı izole edilmiştir. Kurbanın ciğerlerinde yüksek konsantrasyonda ev tipi gaz bulunmuştur.</div>
                    <div class='autopsy-bullet'><span class='bullet-tag'>[EPİDERMAL KANIT]</span> Olay yerinde gaz vanasının yanında failin ceketinden koptuğu anlaşılan özel bir düğme tespit edilmiştir.</div>";
                    break;
                case 210: // Hurdacı
                    reportHtml += @"
                    <div class='autopsy-bullet'><span class='bullet-tag'>[MASİF PENETRASYON]</span> Göğüs bölgesinde yakın mesafeden ateşlenmiş, namlusu kesilmiş ve saçma ile doldurulmuş (seri numarası silinmiş) tüfek yarası saptanmıştır.</div>
                    <div class='autopsy-bullet'><span class='bullet-tag'>[DOKU SPEKTROSKOBİSİ]</span> Yaranın etrafındaki dokularda endüstriyel metal tozu, paslı demir oksit ve vagon gresi kalıntıları izole edilmiştir.</div>
                    <div class='autopsy-bullet'><span class='bullet-tag'>[İZ ANALİZİ]</span> Olay yerinde bulunan ve kurbanın kanının damladığı hurdacı tartım fişi üzerinde failin ter dokusu tespit edilmiştir.</div>";
                    break;
                case 211: // Zeynep
                    reportHtml += @"
                    <div class='autopsy-bullet'><span class='bullet-tag'>[KÜNT KAFA TRAVMASI]</span> Başın arka kısmında, çok ağır ve sivri uçlu tarihi bir demir madenci çekiciyle indirilmiş ölümcül bir darbe saptanmıştır.</div>
                    <div class='autopsy-bullet'><span class='bullet-tag'>[MEKANİK ANALİZ]</span> Darbenin bir anlık öfke patlamasıyla doğrudan kafatasına indirildiği anlaşılmıştır.</div>
                    <div class='autopsy-bullet'><span class='bullet-tag'>[EPİDERMAL MİKROSKOBİ]</span> Kurbanın kıyafetleri üzerinde faile ait uzun, beyaz/gri kadın saç telleri izole edilmiştir.</div>";
                    break;
                case 212: // Hatice
                    reportHtml += @"
                    <div class='autopsy-bullet'><span class='bullet-tag'>[KARDİYAK PENETRASYON]</span> Kurbanın boyun ve göğüs kafesi arasına giren, son derece ince ve uzun (baston içine gizlenmiş) bir bıçakla öldürüldüğü saptanmıştır.</div>
                    <div class='autopsy-bullet'><span class='bullet-tag'>[MİKROSKOBİK TESPİT]</span> Yaranın kenarlarında ahşap yuvanın (baston) ufalanmış çok ince ahşap kıymıkları ve cila kalıntıları bulunmuştur.</div>
                    <div class='autopsy-bullet'><span class='bullet-tag'>[EL YAZISI ANALİZİ]</span> Olay yerinde bulunan randevu notunun failin titrek ama kararlı el yazısıyla yazıldığı kanıtlanmıştır.</div>";
                    break;
                case 213: // Emine
                    reportHtml += @"
                    <div class='autopsy-bullet'><span class='bullet-tag'>[FİTOTOKSİK PARALİZİ]</span> Kurbanın kanında hızla yayılarak tam solunum durmasına neden olan çok güçlü bir zehir (Baldıran otu özütü) tespit edilmiştir.</div>
                    <div class='autopsy-bullet'><span class='bullet-tag'>[POZOLOJİK UYGULAMA]</span> Zehrin, zararsız görünen sıcak bir dağ çayının içine karıştırılarak kurbana içirildiği, midedeki bitki tortularından anlaşılmıştır.</div>
                    <div class='autopsy-bullet'><span class='bullet-tag'>[LİF PATOLOJİSİ]</span> Olay yerinde kapı eşiğine takılıp kopmuş, kırmızı-yeşil renkli el dokuması yün atkı lifleri izole edilmiştir.</div>";
                    break;
                default:
                    reportHtml += "<div class='autopsy-bullet'>• Otopsi ve laboratuvar analizleri devam ediyor.</div>";
                    break;
            }
        }
        else if (isGolge)
        {
            switch (guiltyId)
            {
                case 101: 
                    reportHtml += @"
                    <div class='autopsy-bullet'><span class='bullet-tag'>[PATOLOJİK KESİ]</span> Kafatasının sol frontal bölgesinde ağır masif kesici aletle açılmış 16cm derin yarıklı darbe lezyonu saptanmıştır.</div>
                    <div class='autopsy-bullet'><span class='bullet-tag'>[MEKANİK ANATOMİ]</span> Darbenin yüksek kuvvetle ve dikey açıyla uygulandığı, kemik dokuda derin çatlaklar bıraktığı saptanmıştır.</div>
                    <div class='autopsy-bullet'><span class='bullet-tag'>[EPİDERMAL MİKROSKOBİ]</span> Kurbanın ense kısmında mikronize bitkisel reçine ve odunsu kalıntılar tespit edilmiştir.</div>";
                    break;
                case 102: 
                    reportHtml += @"
                    <div class='autopsy-bullet'><span class='bullet-tag'>[PENETRASYON TRAVMASI]</span> Göğüs anterior mediastinal bölgesinde keskin tek kenarlı alet girişiyle (vulnus punctum) oluşan ölümcül yara saptanmıştır.</div>
                    <div class='autopsy-bullet'><span class='bullet-tag'>[İÇ PERFÜZYON]</span> Kesi çevresinde organik asit kalıntıları ile uyumlu eser miktarda asidik biyokimyasal izole edilmiştir.</div>
                    <div class='autopsy-bullet'><span class='bullet-tag'>[DOKU PATOLOJİSİ]</span> Kurbanın ceket liflerinde koyu renkli sentetik olmayan mikro-iplikler saptanmıştır.</div>";
                    break;
                case 103: 
                    reportHtml += @"
                    <div class='autopsy-bullet'><span class='bullet-tag'>[TRAVMA ANALİZİ]</span> Oksipital kemikte sıcak/kızgın silindirik sert cisim darbesi ile uyumlu dairesel yanık lezyonlu kırık saptanmıştır.</div>
                    <div class='autopsy-bullet'><span class='bullet-tag'>[METALOGRAFİK ANALİZ]</span> Darbe soketinde mikroskobik ölçekte inorganik metalik toz ve kömür kalıntıları saptanmıştır.</div>
                    <div class='autopsy-bullet'><span class='bullet-tag'>[PİGMENT KANITI]</span> Saç diplerinde yüksek ısı kaynağına maruziyetten kaynaklanan is ve endüstriyel yağ lekesi izole edilmiştir.</div>";
                    break;
                case 104: 
                    reportHtml += @"
                    <div class='autopsy-bullet'><span class='bullet-tag'>[İÇ TOKSİKOLOJİ]</span> Akut pulmoner ödem ve solunum depresyonu bulguları sabittir. Mekanik darbe izine rastlanmamıştır.</div>
                    <div class='autopsy-bullet'><span class='bullet-tag'>[TOKSİKOLOJİK PATOLOJİ]</span> Kurbanın gastrik sıvısında ve bronş çeperlerinde yüksek konsantrasyonda arsenik bazlı inorganik toksin tortuları izole edilmiştir.</div>
                    <div class='autopsy-bullet'><span class='bullet-tag'>[KİMYASAL BULGU]</span> Maktulün ağız içi mukozasında zehir karıştırılmış kurutulmuş bitki yaprağı lifleri tespit edilmiştir.</div>";
                    break;
                case 105: 
                    reportHtml += @"
                    <div class='autopsy-bullet'><span class='bullet-tag'>[EKSTERN BİLGİ]</span> Miyokardiyal aritmi ve kardiyovasküler felç bulguları saptanmıştır. Herhangi bir fiziki darbe veya penetrasyon saptanmamıştır.</div>
                    <div class='autopsy-bullet'><span class='bullet-tag'>[TOKSİKOLOJİK ROL]</span> Kan ve idrar tahlillerinde atropin ve skopolamin alkaloid bileşenleri izole edilmiştir.</div>
                    <div class='autopsy-bullet'><span class='bullet-tag'>[POZOLOJİK UYGULAMA]</span> Zehrin kurbanın günlük olarak tükettiği farmakolojik bir karışıma enjekte edildiği anlaşılmıştır.</div>";
                    break;
                case 106: 
                    reportHtml += @"
                    <div class='autopsy-bullet'><span class='bullet-tag'>[TRAVMA DETAYI]</span> Kafatasının temporal mevkisinde ağır pürüzsüz metal cisim darbesiyle oluşan çökme kırığı saptanmıştır.</div>
                    <div class='autopsy-bullet'><span class='bullet-tag'>[EPİDERMAL MİKROSKOBİ]</span> Kurbanın tırnak aralarında inorganik kırmızı balmumu (Rubrum Cera) pigmentleri saptanmıştır.</div>
                    <div class='autopsy-bullet'><span class='bullet-tag'>[KANTİTATİF ANALİZ]</span> Bel bölgesinde dekoratif halı lifleri ve optik polimer cam kıymıkları izole edilmiştir.</div>";
                    break;
                case 107: 
                    reportHtml += @"
                    <div class='autopsy-bullet'><span class='bullet-tag'>[POSTÜRAL TRAVMA]</span> Oksipital kemikte sert bir yapıya çarpmasıyla uyumlu doğrusal kırılma (fractura linearis) ve beyin kanaması saptanmıştır.</div>
                    <div class='autopsy-bullet'><span class='bullet-tag'>[MEKANİK ANALİZ]</span> Kurbanın elbisesinde saat 02:14 sularında durmuş olan mekanik bir aksama ait pirinç zincir halkası saptanmıştır.</div>
                    <div class='autopsy-bullet'><span class='bullet-tag'>[DOKU ANALİZİ]</span> Olay yerinde ve kurban elbiselerinde yüksek nem oranına sahip toprak yapısı ve petrokimyasal aydınlatma yakıtı kalıntıları izole edilmiştir.</div>";
                    break;
                case 108: 
                    reportHtml += @"
                    <div class='autopsy-bullet'><span class='bullet-tag'>[ASFİKSİ ANOMALİSİ]</span> Boyun trakeal kıkırdak çevresinde 0.4mm kalınlığında dayanıklı sentetik sicimle (ligatür boğulma) oluşan derin strangülasyon çizgisi saptanmıştır.</div>
                    <div class='autopsy-bullet'><span class='bullet-tag'>[MİKROSKOBİK TESPİT]</span> Strangülasyon çizgisinde izolasyon materyali (mumlu kaplama) ve endüstriyel yapıştırıcı kalıntıları izole edilmiştir.</div>
                    <div class='autopsy-bullet'><span class='bullet-tag'>[İZ ANALİZİ]</span> Kurbanın ceket omuzunda 42 ebatlarında organik dokulu ağır ayakkabı tabanı izi saptanmıştır.</div>";
                    break;
                default:
                    reportHtml += "<div class='autopsy-bullet'>• Otopsi ve laboratuvar analizleri devam ediyor.</div>";
                    break;
            }
        }
        else
        {
            switch (guiltyId)
            {
                case 1: 
                    reportHtml += @"
                    <div class='autopsy-bullet'><span class='bullet-tag'>[PATOLOJİK KESİ]</span> Boyun sol karotis arter mevkisinde enlemesine, geniş ağızlı masif kesici aletle (incisio vulnificus) açılmış 14cm derin yırtıklı yara saptanmıştır.</div>
                    <div class='autopsy-bullet'><span class='bullet-tag'>[MEKANİK ANATOMİ]</span> Darbenin yüksek kas aktivitesi ve kesici/bileyici alet kullanım tecrübesi olan bir şahıs tarafından tek hamlede uygulandığı değerlendirilmiştir.</div>
                    <div class='autopsy-bullet'><span class='bullet-tag'>[EPİDERMAL MİKROSKOBİ]</span> Kurbanın tırnak altında yapılan spektrofotometrik taramada inorganik bileme tozu izole edilmiştir.</div>";
                    break;
                case 2: 
                    reportHtml += @"
                    <div class='autopsy-bullet'><span class='bullet-tag'>[EKSTERN İNCELEME]</span> Post-mortem muayenede gövde üzerinde mekanik arbede veya fiziki darbe izine rastlanmamıştır. Miyokardial paralizi ve siyanoz bulguları sabittir.</div>
                    <div class='autopsy-bullet'><span class='bullet-tag'>[TOKSİKOLOJİK ROL]</span> Kan ve gastrik sıvı örneklerinde nörotoksik kardiyovasküler felce neden olan fitotoksin glikozit bileşeni tespit edilmiştir.</div>
                    <div class='autopsy-bullet'><span class='bullet-tag'>[POZOLOJİK UYGULAMA]</span> Toksik ajanın kurbanın düzenli tükettiği farmakolojik bir solüsyona enjekte edildiği anlaşılmıştır.</div>";
                    break;
                case 3: 
                    reportHtml += @"
                    <div class='autopsy-bullet'><span class='bullet-tag'>[TRAVMA ANALİZİ]</span> Kafatasının sağ parietal osteo-sutur bölgesinde ağır metalli nesne soketi ile uyumlu oval çökme kırığı (fractura depressa) saptanmıştır.</div>
                    <div class='autopsy-bullet'><span class='bullet-tag'>[DOKU SPEKTROSKOBİSİ]</span> Göğüs lezyonlarında selülozik kağıt lifleri, kırık optik polimer mikro-kıymıkları ve kalsiyum karbonat tespit edilmiştir.</div>
                    <div class='autopsy-bullet'><span class='bullet-tag'>[PİGMENT KANITI]</span> Elbise astarları ve tırnak aralarında kırmızı pigmentli baskı materyali (Rubrum Impressio) kalıntıları izole edilmiştir.</div>";
                    break;
                case 4: 
                    reportHtml += @"
                    <div class='autopsy-bullet'><span class='bullet-tag'>[DOĞRUSAL TRAVMA]</span> Sırt ve thorakal kaburga mevkisinde 32mm silindirik esnek yapılı bir aksesuara ait doğrusal ezik izleri (contusio rectilinearis) saptanmıştır.</div>
                    <div class='autopsy-bullet'><span class='bullet-tag'>[ASFİKSİ BULGUSU]</span> Boyun hyoid kemik çevresinde aksiller kıskac altında subkonjonktival asphyxia hematomu mevcuttur.</div>
                    <div class='autopsy-bullet'><span class='bullet-tag'>[KİMYASAL ANOMALİ]</span> Olay yerinde ve kurban elbisesinde adli incelemeyi engellemeye yönelik profesyonel endüstriyel solvent solüsyonu kalıntıları tespit edilmiştir.</div>";
                    break;
                case 5: 
                    reportHtml += @"
                    <div class='autopsy-bullet'><span class='bullet-tag'>[LİGATÜR BOĞULMA]</span> Boyun anterior larynx bölgesinde 0.4mm çapında çok ince, yüksek tensil mukavemetli sentetik lif (sulcus strangulationis) saptanmıştır.</div>
                    <div class='autopsy-bullet'><span class='bullet-tag'>[LİF PATOLOJİSİ]</span> Kurbanın posterior ceket astarlarında keskin lezyon ve organik dokulu mikronize yün lifleri izole edilmiştir.</div>
                    <div class='autopsy-bullet'><span class='bullet-tag'>[POSTÜRAL ANALİZ]</span> Saldırının kurbanın beklemediği bir anda arka açıdan dairesel traksiyon hamlesiyle gerçekleştirildiği kesinleşmiştir.</div>";
                    break;
                default:
                    reportHtml += "<div class='autopsy-bullet'>• Otopsi ve laboratuvar analizleri devam ediyor.</div>";
                    break;
            }
        }

        reportHtml += "</div>";

        // Kasabaya göre doğru bulgu listesini kullan
        var targetList = isSisoren ? _sisorenFindings : (isGolge ? _golgeFindings : _gizemliFindings);
        List<string> currentFindings;
        lock (targetList)
        {
            currentFindings = targetList.ToList();
        }

        if (currentFindings.Count > 0)
        {
            reportHtml += @"
            <div class='autopsy-divider-line'></div>
            <div class='autopsy-sec-title'><i class='fa-solid fa-vial-circle-check'></i> DEDEKTİF ADLİ LAB EŞLEŞTİRME RAPORLARI</div>
            <div class='autopsy-findings-list'>";

            foreach (var finding in currentFindings)
            {
                bool isNoMatch = finding.Contains("DEĞİLDİR", StringComparison.OrdinalIgnoreCase) ||
                                 finding.Contains("saptanmamıştır", StringComparison.OrdinalIgnoreCase) ||
                                 finding.Contains("ilişkisi saptanmamıştır", StringComparison.OrdinalIgnoreCase);

                bool isMatch = !isNoMatch && (finding.Contains("EŞLEŞTİ", StringComparison.OrdinalIgnoreCase) || finding.Contains("DOĞRULANMIŞTIR", StringComparison.OrdinalIgnoreCase) || finding.Contains("KESİNLEŞMİŞTİR", StringComparison.OrdinalIgnoreCase));

                string badgeClass;
                string badgeIcon;

                if (isNoMatch)
                {
                    badgeClass = "badge-nomatch";
                    badgeIcon = "<i class='fa-solid fa-triangle-exclamation'></i>";
                }
                else if (isMatch)
                {
                    bool isBloodOrPoison = finding.Contains("SEROLOJİK", StringComparison.OrdinalIgnoreCase) ||
                                           finding.Contains("DNA", StringComparison.OrdinalIgnoreCase) ||
                                           finding.Contains("KAN", StringComparison.OrdinalIgnoreCase) ||
                                           finding.Contains("ZEHİR", StringComparison.OrdinalIgnoreCase) ||
                                           finding.Contains("TOKSİN", StringComparison.OrdinalIgnoreCase);

                    if (isBloodOrPoison)
                    {
                        badgeClass = "badge-match-blood";
                        badgeIcon = "<i class='fa-solid fa-droplet'></i>";
                    }
                    else
                    {
                        badgeClass = "badge-match-written";
                        badgeIcon = "<i class='fa-solid fa-circle-check'></i>";
                    }
                }
                else
                {
                    badgeClass = "badge-nomatch";
                    badgeIcon = "<i class='fa-solid fa-triangle-exclamation'></i>";
                }

                reportHtml += $@"
                <div class='autopsy-finding-card {badgeClass}'>
                    <div class='finding-header'>{badgeIcon} {finding}</div>
                </div>";
            }

            reportHtml += "</div>";
        }
        else
        {
            reportHtml += @"
            <div class='autopsy-divider-line'></div>
            <div class='autopsy-sec-title'><i class='fa-solid fa-microscope'></i> LABORATUVAR İNCELEME DURUMU</div>
            <p style='color: #888; font-style: italic; font-size: 0.95rem;'>Henüz olay yerinden adli tıbba nesne incelemesi gönderilmedi. Binalardaki nesneleri büyüteç, UV veya toz fırçasıyla inceleyip 'ADLİ TIBBA GÖNDER' butonunu kullanabilirsiniz.</p>";
        }

        reportHtml += "</div>";

        return Task.FromResult(reportHtml);
    }

    public object GetForensicState(int guiltyId)
    {
        int weaponId = 0;
        int fingerprintId = 0;

        if (guiltyId >= 200)
        {
            switch (guiltyId)
            {
                case 201: weaponId = 2014; fingerprintId = 2012; break; // Rüstem
                case 202: weaponId = 2025; fingerprintId = 2024; break; // İrfan
                case 203: weaponId = 2031; fingerprintId = 2034; break; // Nejat
                case 204: weaponId = 2041; fingerprintId = 2045; break; // Cemile
                case 205: weaponId = 2051; fingerprintId = 2053; break; // Hikmet
                case 206: weaponId = 2064; fingerprintId = 2062; break; // Meliha
                case 207: weaponId = 2075; fingerprintId = 2074; break; // Nermin
                case 208: weaponId = 2082; fingerprintId = 2085; break; // Durmuş
                case 209: weaponId = 2092; fingerprintId = 2095; break; // Şevket
                case 210: weaponId = 2103; fingerprintId = 2104; break; // Zehra
                case 211: weaponId = 2112; fingerprintId = 2115; break; // Zeynep
                case 212: weaponId = 2122; fingerprintId = 2123; break; // Hatice
                case 213: weaponId = 2132; fingerprintId = 2133; break; // Emine
            }
        }
        else if (guiltyId >= 100)
        {
            switch (guiltyId)
            {
                case 101: weaponId = 1011; fingerprintId = 1013; break; // Oduncu
                case 102: weaponId = 1023; fingerprintId = 1021; break; // Manav
                case 103: weaponId = 1031; fingerprintId = 1033; break; // Demirci
                case 104: weaponId = 1043; fingerprintId = 1042; break; // Bakkal
                case 105: weaponId = 1051; fingerprintId = 1054; break; // Hekim
                case 106: weaponId = 1064; fingerprintId = 1062; break; // Muhtar
                case 107: weaponId = 1073; fingerprintId = 1071; break; // Fehmi
                case 108: weaponId = 1084; fingerprintId = 1081; break; // Kunduracı
            }
        }
        else
        {
            switch (guiltyId)
            {
                case 1: weaponId = 1; fingerprintId = 2; break; // Kasap
                case 2: weaponId = 6; fingerprintId = 4; break; // Eczacı
                case 3: weaponId = 8; fingerprintId = 7; break; // Muhtar
                case 4: weaponId = 10; fingerprintId = 12; break; // Komiser
                case 5: weaponId = 13; fingerprintId = 14; break; // Terzi
            }
        }

        return new { success = true, guiltyId, weaponId, fingerprintId };
    }

    public string GetDynamicClueDetail(int clueId, int guiltyId)
    {
        return clueId switch
        {
            1 => "Üzerindeki kan lekeleri Osman Bey'e ait gibi görünüyor. " + (guiltyId == 1 ? "Sapındaki el izi net bir şekilde Kasap Hasan'ı işaret ediyor." : "Ancak satırın sapında kasaba ait olmayan, eldivenle tutulmuş gibi garip izler var."),
            2 => "Sayfalarda Osman Bey'in adı kırmızıyla çizilmiş. Yanında bir not: " + (guiltyId == 1 ? "'Borcunu ödemedi, cezasını çekecek.'" : "'Bu borç sadece başlangıç.'"),
            3 => "Kavga izleri taşıyan önlük... Osman Bey'in ceketinin düğmesi önlüğün cebinde bulunuyor. " + (guiltyId == 1 ? "Hasan o gece kurbanla boğuşmuş olmalı." : "Önlük üzerindeki leke iş yeri lekesi gibi duruyor."),
            4 => "Zehirli bir ilacın boş şişesi. Reçetede Osman Bey'in adı var. " + (guiltyId == 2 ? "Etiketin arkasında Selma'nın el yazısıyla 'Son Doz' yazıyor." : "Şişe aceleyle alınmış gibi, kapağı zorlanmış."),
            5 => "Osman Bey'e verilen ilaçların listesi. Sayfa ortadan dikey yırtılmış. " + (guiltyId == 2 ? "Yırtık sayfanın kenarında 'Zehir' kelimesi okunabiliyor." : "Birisi kanıtları yok etmek için defteri zorla yırtmış."),
            6 => "Bu bitkinin özü, Osman Bey'in kanında bulunan zehirle aynı. " + (guiltyId == 2 ? "Selma bunu kasten hazırlamış." : "Birisi Selma'nın dükkanından bu otu gizlice almış olabilir."),
            7 => "Mektupta 'Osman, o araziler benim, sonun yaklaşıyor' yazıyor. " + (guiltyId == 3 ? "Muhtar Kemal açıkça kurbanı tehdit etmiş ve bunu gerçekleştirmiş." : "Ancak mektup asla postalanmamış, sadece bir sinir anında yazılmış."),
            8 => "Osman Bey'in kırık gözlüğü... " + (guiltyId == 3 ? "Muhtarın odasında şiddetli bir kavga yaşanmış." : "Gözlük bir başka yerde kırılıp buraya bırakılmış olabilir."),
            9 => "Kasada Osman Bey'in arazilerine ait sahte tapular var. " + (guiltyId == 3 ? "Kemal her şeyi planlamış, cinayet sebebi bu tapular." : "Bu tapular sadece muhtarın açgözlülüğünü gösteriyor, cinayeti değil."),
            10 => "Rozetin numarası kazınmış. Osman Bey'in cesedinin hemen yanında bulundu. " + (guiltyId == 4 ? "Güneş, kurbanla olay yerinde boğuşurken rozetini düşürmüş." : "Rozet oraya özellikle bir polisi suçlamak için bırakılmış."),
            11 => "Dosyada Osman Bey'in gizli geçmişi var. " + (guiltyId == 4 ? "Komiser Güneş, bu geçmişi kullanarak kurbanı şantaj yapıyordu." : "Dosya sadece prosedür gereği tutulmuş."),
            12 => "Pahalı bir palto düğmesi. Osman Bey'in cebinden çıktı. " + (guiltyId == 4 ? "Güneş'in paltosundan kopmuş, arbede sırasında Osman Bey onu tutmuş." : "Bu düğme terzinin bir müşterisine de ait olabilir."),
            13 => "İplik, Osman Bey'in ceketinin dikişleriyle aynı. Üzerindeki kan... " + (guiltyId == 5 ? "Kurbanın kanı. Yahya kurbanı öldürürken makara elindeydi." : "Terzinin dikiş yaparken kendi elini kestiği bir kaza olabilir."),
            14 => "Osman Bey'in ceketinden kopan kumaş. " + (guiltyId == 5 ? "Yahya kurbanla boğuşurken kumaş yırtıldı." : "Kumaş sadece bir terzi artığı olabilir."),
            15 => "Cepteki notta 'Osman, bugün hava kararınca gel konuşalım' yazıyor. " + (guiltyId == 5 ? "Yahya onu çağırdı ve tuzağa düşürdü." : "Yahya çağırdı ama gittiğinde onu ölü buldu."),
            
            // Gölge Şehir Delilleri (1011 - 1084)
            1011 => "Ağır balta üzerinde çam reçinesi ve koyu lekeler var. " + (guiltyId == 101 ? "Baltanın sapındaki el izi ve kan, Oduncu Tahsin'in Ekrem Bey'e indirdiği ölümcül darbeyi doğruluyor." : "Baltadaki lekeler sadece reçine ve ağaç özsuyundan ibaret."),
            1012 => "Gece kesim defterinde Ekrem Bey'in adı ve tehdit notu var. " + (guiltyId == 101 ? "Tahsin, Ekrem'in şantajını bitirmek için o gece ormanda pusu kurmuş." : "Defter sadece rutin kereste teslimatlarını gösteriyor."),
            1013 => "Kalın deri iş eldiveni... " + (guiltyId == 101 ? "Eldivenin avuç içinde kurbanın saç telleri ve kan izi izole edildi." : "Eldiven odun kıymıklarından korunmak için kullanılmış."),
            1014 => "Çam kütüğü üzerinde kazınmış harfler: 'E.B. 14 KASIM'. " + (guiltyId == 101 ? "Tahsin cinayeti önceden planlamış." : "Tarih sadece kereste kesim gününü işaret ediyor."),

            1021 => "Meyve kasasının dibinde gizlenmiş kanlı bıçak kılıfı. " + (guiltyId == 102 ? "Manav Ayşe'nin Ekrem'in göğsüne sapladığı bıçağın kılıfı buraya saklanmış." : "Sıradan bir meyve kasası."),
            1022 => "Tezgâhtaki elma üzerinde siyah kumaş lifleri var. " + (guiltyId == 102 ? "Ayşe cinayet gecesi giydiği siyah pelerini aceleyle çıkarırken lifler buraya dökülmüş." : "Meyve tezgâhında olağan dışı bir iz yok."),
            1023 => "Yırtık siyah pelerin kumaşı... " + (guiltyId == 102 ? "Ekrem can verirken Ayşe'nin pelerininden bu parçayı koparmış." : "Kumaş sıradan bir çuval bezi artığı."),
            1024 => "Buruşturulmuş kâğıt parçasında ipotek senedi ve Ayşe'nin el yazısı notu var: " + (guiltyId == 102 ? "'Ekrem'in burayı almasına asla izin vermeyeceğim!'" : "Sadece eski bir hesap notu."),

            1031 => "Ağır demirci çekici ve demir çubuk. " + (guiltyId == 103 ? "Kurbanın şakağındaki kırıkla çekicin köşesi tam olarak örtüşüyor." : "Çekiç sadece demir dövmek için kullanılmış."),
            1032 => "Özel yapım çelik asma kilit... " + (guiltyId == 103 ? "Kazım, Ekrem'in evraklarını sakladığı kasanın kilidini zorla açmaya çalışmış." : "Dükkânın standart kapı kilidi."),
            1033 => "Körük tozu ve kükürt numunesi. " + (guiltyId == 103 ? "Olay yerinde ve Ekrem'in ceketinde bulunan inorganik kükürt tozu ile %100 eşleşti." : "Atölyede olağan demir tozu kalıntısı."),
            1034 => "Deri iş önlüğü üzerinde kan sıçrama lekesi. " + (guiltyId == 103 ? "Kazım cinayet anında bu önlüğü giyiyordu." : "Önlük üzerindeki lekeler pas ve yanık izi."),

            1041 => "Veresiye defterinde Ekrem Bey'in 35.000 Liralık borcu kırmızıyla çizilmiş. " + (guiltyId == 104 ? "Naciye'nin 'Hak ettiğini bulacak' notu açık bir cinayet itirafı." : "Sıradan bir alacak-verecek kaydı."),
            1042 => "Boş cam şişede zehir tortusu saptandı. " + (guiltyId == 104 ? "Naciye fare zehrini bu şişeden Ekrem'in tütününe aktarmış." : "Şişe eski sirke şişesi."),
            1043 => "İşlemeli tütün kesesi... " + (guiltyId == 104 ? "İçindeki tütün yapraklarında ölümcül dozda inorganik arsenik toksini izole edildi." : "Sıradan kurutulmuş tütün yaprakları."),
            1044 => "Küçük pirinç anahtar... " + (guiltyId == 104 ? "Naciye zehri sakladığı ecza dolabının anahtarını cebinde unutmuş." : "Bakkal kasanın yedek anahtarı."),

            1051 => "Koyu renkli cam şişede banotu kökü özü var. " + (guiltyId == 105 ? "Hekim Sevgi, Ekrem'in mide ilacına bu ölümcül iksiri karıştırmış." : "Tıbbi amaçla saklanan bitkisel tentür."),
            1052 => "Yırtık reçete sayfasında silinmiş yazı: 'Banotu Kökü Özü (3cc Lethal Doz)'. " + (guiltyId == 105 ? "Sevgi ölümcül dozu bizzat reçete etmiş ve sayfayı yırtarak delili karartmak istemiş." : "Eski bir tedavi notu."),
            1053 => "Kurutulmuş banotu bitkisi... " + (guiltyId == 105 ? "Maktulün tırnaklarındaki mor lekelerle bitkinin alkaloid yapısı birebir uyumlu." : "Zararsız şifalı ot demeti."),
            1054 => "Pirinç saplı tıbbi kesici alet (neşter)... " + (guiltyId == 105 ? "Üzerindeki kan DNA'sı Ekrem Bey'in savunma yarasıyla eşleşiyor." : "Standart muayenehane aleti."),

            1061 => "Resmi mühürlü tehdit mektubu: " + (guiltyId == 106 ? "Muhtar Cevdet 'Aksi takdirde sonuçlarına katlanırsınız' diyerek kurbana açıkça gözdağı vermiş." : "Resmi bir köy işleri yazışması."),
            1062 => "Sahte tapu devir senedi... " + (guiltyId == 106 ? "Ekrem bu sahtekârlığı ihbar etmek istediği için Cevdet tarafından öldürüldü." : "Eski bir arazi anlaşmazlığı evrakı."),
            1063 => "Uzun çelik kasa anahtarı... " + (guiltyId == 106 ? "Muhtar cinayetten sonra Ekrem'in kasasından tapuları çalmak için bu anahtarı kullanmış." : "Muhtarlık arşiv dolabının anahtarı."),
            1064 => "Sol camı çatlamış altın çerçeveli gözlük! " + (guiltyId == 106 ? "Olay yerindeki arbedede Cevdet'in gözlüğü düşmüş, kırık cam parçası halıda kalmış." : "Eski bir okuma gözlüğü."),

            1071 => "Zincirli altın cep saati... " + (guiltyId == 107 ? "Tam saat 02:14'te durmuş. Fehmi Bey cinayet anında kurbanın cebinden babasının saatini geri almış." : "Antika bir köstekli saat."),
            1072 => "Tehdit içerikli not: 'O saati bir daha asla göremeyeceksin.' " + (guiltyId == 107 ? "Ekrem'in bu alaycı notu Fehmi Bey'in öfke krizine girip kurbanı şömineye itmesine yol açmış." : "Eski bir husumet mektubu."),
            1073 => "Gaz feneri üzerinde parmak izleri ve is lekesi. " + (guiltyId == 107 ? "Fehmi Bey gece saat 02:00'de Ekrem'in evine giderken bu feneri kullandı." : "Ev aydınlatmasında kullanılan sıradan fener."),
            1074 => "Kalın ciltli romanda altı çizili satır ve el yazısı not: " + (guiltyId == 107 ? "'O gece saat 02:14'te sesler duydum...' Fehmi Bey kendi suçunu örtbas etmek için sahte tanıklık kurgulamış." : "Edebi bir roman sayfası."),

            1081 => "42 numara çamurlu kışlık deri çizme... " + (guiltyId == 108 ? "Olay yerindeki göl kenarı çamur izleriyle çizmenin taban deseni %100 örtüşüyor." : "Kunduracının kendi atölye çizmesi."),
            1082 => "Mumlu dayanıklı ayakkabı ipi yumağı... " + (guiltyId == 108 ? "Ekrem Bey'in boynundaki 0.4mm strangülasyon izi bu mumlu iple birebir eşleşiyor." : "Deri dikiminde kullanılan standart mumlu ip."),
            1083 => "Eğri deri kesme bıçağı... " + (guiltyId == 108 ? "Bıçağın kabzasındaki parmak izi Rasim'in cinayet gecesi olay yerinde olduğunu kanıtlıyor." : "Deri traşlamada kullanılan usta aleti."),
            1084 => "Ahşap ayakkabı kalıbı... " + (guiltyId == 108 ? "Kalıbın taban numarası ile kurbanın göğsündeki darbe izi eşleşti." : "Ayakkabı şekillendirme kalıbı."),

            2011 => "Yırtık telgraf şeridi... " + (guiltyId == 201 ? "Rüstem, sırrının açığa çıkmasından korkarak şeridi çöpe attı." : "Sadece atılmış eski bir mesaj."),
            2012 => "Mors tuşu altında kanlı parmak izi... " + (guiltyId == 201 ? "Rüstem'in kurbanı öldürdükten sonra ellerindeki kanı silmeyi unuttuğu bariz." : "İz, kurbanın son çırpınışlarına ait olabilir."),
            2013 => "Çekmecedeki rüşvet kesesi... " + (guiltyId == 201 ? "Bu rüşveti saklamak Rüstem için kurbanı öldürmeye yetti." : "Telgrafçının şüpheli gelir kaynakları."),
            2014 => "Arka odada kasten kesilmiş yedek hat kablosu... " + (guiltyId == 201 ? "Rüstem bu kabloyla kurbanı boğarak öldürdü." : "Kablolar sadece sabotaj amaçlı kesilmiş."),
            2015 => "Karalanmış tehdit mesajı taslağı... " + (guiltyId == 201 ? "Taslağın el yazısı açıkça Rüstem'e ait." : "Kimliği belirsiz bir tehdit."),
            
            2021 => "Altına buluşma saati kazınmış fincan... " + (guiltyId == 202 ? "İrfan kurbanı kahvehanenin arka odasına bu işaretle çekti." : "Müşterilerin bıraktığı sıradan bir iz."),
            2022 => "Örgüt sembolü kazınmış okey taşı... " + (guiltyId == 202 ? "İrfan'ın derin bağlarını gösteren bu taş cinayetin asıl motifi." : "Kahvehanede unutulmuş işaretli taş."),
            2023 => "Yırtık borç listesi... " + (guiltyId == 202 ? "Kırmızıyla çizilmiş isimler İrfan'ın infaz listesi gibi." : "Sıradan alacak-verecek kaydı."),
            2024 => "Sobada tam yanmamış kanlı kasket... " + (guiltyId == 202 ? "İrfan olay anında kasketine sıçrayan kanı yok etmek için onu ateşe atmış." : "Kazara düşüp yanmış bir şapka."),
            2025 => "Zulalanmış, bir mermisi eksik ruhsatsız altıpatlar... " + (guiltyId == 202 ? "Silahın balistik incelemesi kurbanın göğsündeki kurşunla tam eşleşti." : "Kahvecinin kendini korumak için sakladığı silah."),

            2031 => "Kasten kesilmiş film bobini... " + (guiltyId == 203 ? "Nejat, kurbanın kafasını ezmek için bu ağır çelik bobini kullandı." : "Makinede kopmuş sıradan bir bobin."),
            2032 => "Çekmecede şantajlı eski film afişi... " + (guiltyId == 203 ? "Nejat, şantaja boyun eğmemek için kurbanı ortadan kaldırdı." : "Sadece geçmişe ait karanlık bir not."),
            2033 => "Cinayet saatine ait düşmüş bilet koçanı... " + (guiltyId == 203 ? "Nejat'ın alibisi çöktü, bilet cinayet anında kesilmişti." : "Müşterinin düşürdüğü bilet."),
            2034 => "Perde arkasında kanlı deri eldiven... " + (guiltyId == 203 ? "Pahalı domuz derisi eldiven sadece Nejat gibi gösterişli birine ait olabilir." : "Olay yerine bırakılmış yanıltıcı bir eşya."),
            2035 => "Aynaya rujla yazılmış rıhtım adresi... " + (guiltyId == 203 ? "Bu adres Nejat'ın cinayet sonrası kaçış planıydı." : "Eski bir aktrisin bıraktığı not."),

            2041 => "Tezgâh arkasında ezilmiş fare zehri (arsenik)... " + (guiltyId == 204 ? "Cemile kurbanı zehirlemek için bu kutudaki arseniği kullandı." : "Dükkanda zararlılar için bulundurulan zehir."),
            2042 => "Veresiye defterinde kopuk sayfa... " + (guiltyId == 204 ? "Cemile cinayet gecesi nerede olduğunu saklamak için sayfayı koparmış." : "Müşteri borcunu ödeyince sayfa yırtılmış."),
            2043 => "Kasa yanına düşmüş yabancı gümüş sikke... " + (guiltyId == 204 ? "Cemile'nin dışarıdan tuttuğu bir tetikçiden seken para." : "Koleksiyonluk eski bir para."),
            2044 => "Zulalanmış faturasız kaçak tütün... " + (guiltyId == 204 ? "Cemile kaçakçılık sırrını korumak için cinayet işledi." : "Sadece bakkalın yasadışı ticareti."),
            2045 => "Yerdeki buruşuk isimli borç senedi... " + (guiltyId == 204 ? "Senedin üzerindeki deri döküntüleri Cemile'yi cinayet anına bağlıyor." : "Ödenmemiş bir borç kâğıdı."),

            2051 => "İçi oyulmuş kalın ansiklopedi... " + (guiltyId == 205 ? "Hikmet bu ağır kitabı ve içindeki metal bloku kurbanın kafasına vurdu." : "Sadece değerli eşyaları saklamak için oyulmuş."),
            2052 => "Kırmızı mürekkepli şifreli kenar notları... " + (guiltyId == 205 ? "Bu şifreler Hikmet'in kurbanla olan gizli hesaplaşmasını gösteriyor." : "Eski bir okuyucunun notları."),
            2053 => "Kırık cep saati kapağında kurumuş kan... " + (guiltyId == 205 ? "DNA testi kanın kurbana değil, arbedede yaralanan Hikmet'e ait olduğunu kanıtladı." : "Saat tamircisinden kalan eski bir leke."),
            2054 => "Yerde çekilmiş ağır kasa izi... " + (guiltyId == 205 ? "Hikmet cinayetten sonra cesedi gizlemek için kasayı sürüklemiş." : "Dükkan temizliğinde yeri değiştirilen kasa."),
            2055 => "Sahte vasiyetname taslağı... " + (guiltyId == 205 ? "Hikmet sahte vasiyetnameden pay almak için gerçeği bilen kurbanı susturdu." : "Sadece taslak halinde bir sahtekarlık."),

            2061 => "Asitle silinmiş nüfus kütük sayfası... " + (guiltyId == 206 ? "Meliha Hanım kurbanın geçmişini ve haklarını yok etmek için sahtecilik yaptı." : "Belge kazara tahrip olmuş."),
            2062 => "Kenarı çatlamış resmi kasaba damgası (mühür)... " + (guiltyId == 206 ? "Meliha Hanım ateş etmeden önce mühürle kurbanın kafasına şiddetle vurdu." : "Sert basıldığı için kırılmış eski mühür."),
            2063 => "Gizli orman tapusu... " + (guiltyId == 206 ? "Meliha bu tapuların sırrını korumak için kurbanı harcadı." : "Arşive kaldırılmış eski bir arazi haritası."),
            2064 => "Süpürgelikteki boş mermi kovanı... " + (guiltyId == 206 ? "Balistik inceleme, bu kovanın Meliha'nın tabancasından çıktığını kanıtladı." : "Daha önceden kalma bir yorgun mermi kovanı."),
            2065 => "Çöpteki parçalanmış resmi uyarı mektubu... " + (guiltyId == 206 ? "Meliha görevden alınacağını öğrenince kurbanı suçlamak için panikledi." : "Önemsiz bir bürokratik yazışma."),

            2071 => "İthal yarım içilmiş puro izmariti... " + (guiltyId == 207 ? "Nermin, kurbana lüks bir puro ikram edip içine toksin enjekte etmiş." : "Zengin bir müşterinin izmariti."),
            2072 => "Şifreli sigara sarma kâğıdı... " + (guiltyId == 207 ? "Isıtıldığında Nermin'in kurbanı tuzağa düşürdüğü adres listesi ortaya çıktı." : "Kâğıt üzerinde anlamsız lekeler."),
            2073 => "İçi boşaltılıp temizlenmiş lüle taşı pipo... " + (guiltyId == 207 ? "Nermin, piponun içindeki zehir kalıntılarını yok etmeye çalışmış." : "Sadece temizlenmiş eski bir pipo."),
            2074 => "Temizlenmeye çalışılmış kanlı gümüş tabaka... " + (guiltyId == 207 ? "Arbede sırasında kurban Nermin'in tabakasına çarparak kanını bulaştırdı." : "Tabaka üzerindeki pas lekesi."),
            2075 => "Tütün kokulu zehirli sıvı şişesi... " + (guiltyId == 207 ? "Nermin'in kurbanın purosunu felç edici bir toksinle doldurduğu kanıtlandı." : "Sıradan bir aromatik yağ şişesi."),

            2081 => "Kasten yarıya kadar kesilmiş at eyeri kayışı... " + (guiltyId == 208 ? "Durmuş, kurbanın uçurumdan düşmesi için eyeri kurnazca kesti." : "Kayış sadece aşınmış."),
            2082 => "Samanlarda bulunan kanlı at nalı çivisi... " + (guiltyId == 208 ? "Durmuş, eyer planı işlemeyince kurbanın şah damarına bu çiviyi sapladı." : "Hayvan yaralanmasından kalma kanlı çivi."),
            2083 => "Kopuk altın cep saati zinciri... " + (guiltyId == 208 ? "Durmuş cinayet anında kurbanın değerli saatini çekip kopardı." : "Su yalağına düşmüş sıradan zincir."),
            2084 => "Yabancı sivri burunlu bot izi... " + (guiltyId == 208 ? "Durmuş cinayeti başka birine atmak için bu bot izini kasten yaptı." : "Ahıra izinsiz giren birinin izi."),
            2085 => "Direğe saplı paslı çakı... " + (guiltyId == 208 ? "Kabzasındaki DNA, Durmuş'un bu çakıyı olay yerinde bıraktığını kanıtlıyor." : "Hayvan bağlamak için saplanmış çakı."),

            2091 => "Valfi kasten bozulmuş küçük piknik tüpü... " + (guiltyId == 209 ? "Şevket, gaz sızıntısı yaratarak olaya kaza süsü vermek istedi." : "Sadece arızalı eski bir valf."),
            2092 => "İngiliz anahtarı üzerindeki kurumuş kan... " + (guiltyId == 209 ? "Şevket, kurbanı gazla zehirlemeden önce bu ağır anahtarla kafasına vurdu." : "Anahtardaki pas ve makine yağı izi."),
            2093 => "Sahte teslimat müşteri kaydı... " + (guiltyId == 209 ? "Şevket, kurbanın evine gece gitmesini açıklamak için defterde sahtecilik yaptı." : "Sadece hatalı tutulmuş bir kayıt."),
            2094 => "Boş tüpün içine zulalanmış çalıntı kolye... " + (guiltyId == 209 ? "Şevket'in borç batağından kurtulmak için kurbanı soyduğu ortada." : "Gizlenmiş, kime ait olduğu belirsiz mücevher."),
            2095 => "Kopuk özel ceket düğmesi... " + (guiltyId == 209 ? "Arbede sırasında Şevket'in ceketinden kopan bu düğme onu olay yerine bağlıyor." : "Sıradan bir plastik düğme."),

            2101 => "Preslenmek üzere olan çalıntı çelik kasa... " + (guiltyId == 210 ? "Zehra, kurbanın evinden çaldığı kasayı hurdada yok etmeye çalıştı." : "Eski bir hurda yığını."),
            2102 => "Üzeri zımparalanmış yeni araç plakası... " + (guiltyId == 210 ? "Zehra'nın yasadışı işlerini gizlemek için kestiği çalıntı plaka." : "Metal geri dönüşümü için hazırlanmış plaka."),
            2103 => "Seri numarası silinmiş, namlusu kesik tüfek... " + (guiltyId == 210 ? "Zehra, kurbanın göğsüne yakın mesafeden bu silahla ateş etti." : "Hurdaya atılmış bozuk bir silah."),
            2104 => "Kanlı tartım fişi... " + (guiltyId == 210 ? "Kurban kan revan içinde kantarın üzerine düştüğünde fişe kan sıçramış." : "Dikkatsizce damlatılmış kırmızı boya."),
            2105 => "Eritilecek teller içindeki kurbana ait yüzük... " + (guiltyId == 210 ? "Zehra cinayet delilini bakır tellerle birlikte eritmeye çalıştı." : "Hurda bakırların içine düşmüş yüzük."),

            2111 => "Tozlu eski maden kayıtları belgeleri... " + (guiltyId == 211 ? "Zeynep Teyze, kocasının intikamını bu belgelerdeki sır yüzünden aldı." : "Tarihi geçmiş tozlu evraklar."),
            2112 => "Paslı ağır demir madenci çekici... " + (guiltyId == 211 ? "Çekiç sapındaki lekenin kurbanın kanı olduğu anlaşıldı. Zeynep kurbanın başına bununla vurdu." : "Sıradan eski bir maden aleti."),
            2113 => "Kasten kırılmış eski sandık asma kilidi... " + (guiltyId == 211 ? "Kurban belgeleri çalmak için sandığı kırınca Zeynep onu suçüstü yakaladı." : "Sadece paslandığı için kırılmış kilit."),
            2114 => "Bekçi Recep'in tuttuğu gölge raporu... " + (guiltyId == 211 ? "O gece dışarı çıkan gölge bizzat Zeynep Teyzeydi." : "Bekçi sisli havada yanılmış olabilir."),
            2115 => "Olay yerindeki uzun beyaz/gri kadın saç teli... " + (guiltyId == 211 ? "Zeynep cinayet anında saç telini olay yerinde düşürdü." : "Kurbanın yaşlı bir akrabasına ait olabilir."),

            2121 => "30 yıl önceki maden kazasını gösteren albüm... " + (guiltyId == 212 ? "Hatice Nine bu fotoğraflarla kurbana şantaj yapan sırrı koruyordu." : "Eski hatıra fotoğrafları."),
            2122 => "İçinden bıçak çıkan ahşap baston... " + (guiltyId == 212 ? "Hatice Nine kurbanın boynuna kocasının yadigarı olan bu gizli bıçağı sapladı." : "Sadece antika bir savunma aleti."),
            2123 => "Titrek el yazılı randevu notu... " + (guiltyId == 212 ? "Hatice Nine'nin el yazısı incelemesinde notu kendisinin yazdığı kesinleşti." : "Sıradan bir buluşma notu."),
            2124 => "Siyah kolye boncuğu... " + (guiltyId == 212 ? "Hatice Nine'nin boynundaki siyah tespihten/kolyeden koptuğu belirlendi." : "Eski halı desenlerine karışmış sıradan boncuk."),
            2125 => "Dağ çamuru bulaşmış el örgüsü yün şal... " + (guiltyId == 212 ? "Hatice Nine kurbanın evinden çıkarken şalını kapıya taktı." : "Yoldan geçen birine ait yırtık parça."),

            2131 => "Kavanoza gizlenmiş baldıran otu... " + (guiltyId == 213 ? "Emine Hanım'ın kurbanı felç edip öldürmek için hazırladığı ölümcül zehir." : "Sadece kurumuş şifalı ot."),
            2132 => "Dibinde bitki tortusu olan zehirli çay fincanı... " + (guiltyId == 213 ? "Emine Hanım kurbana barış çayı bahanesiyle baldıran özütünü içirdi." : "Sıradan bir dağ çayı tortusu."),
            2133 => "Kırmızı-yeşil yün atkı püskülü... " + (guiltyId == 213 ? "Olay yerindeki bu atkı parçası Emine Hanım'ın tezgâhındaki iplerle %100 aynı." : "Kopmuş sıradan bir yün parçası."),
            2134 => "Zehirli bitkiler bölümü çizilmiş kitap... " + (guiltyId == 213 ? "Emine cinayet planını adım adım bu kitaptan öğrenerek kurguladı." : "Sıradan bir botanik kitabı."),
            2135 => "Yamaç toprağına ait çamur izi... " + (guiltyId == 213 ? "Emine kurbanın evine pencereden girerken dağ çamurunu oraya taşıdı." : "Herhangi bir çiftçinin ayakkabısındaki çamur."),

            _ => "Bu nesne karanlık sırlar barındırıyor..."
        };
    }
}
