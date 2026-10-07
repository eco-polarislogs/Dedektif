/**
 * SİSÖREN MOTORU (v3.0)
 * Dağ Kasabası Sisören — Tam Kasaba Motoru
 * Gölge Şehir mimarisiyle uyumlu: NPC Data, Bina Giriş, Otopsi, Buldum Entegrasyonu
 */

window.SisorenEngine = {
    currentActiveTown: null,
    hasShownSisorenIntro: false,
    introDialogCompleted: false,
    hasShownDualIntro: false,
    dualDialogStep: 0,
    visitedSisorenBuildings: new Set(),

    normalizeNpcQuestions: function (npcId, name, questions) {
        const fallback = window.SISOREN_CONFIG && window.SISOREN_CONFIG.fallbackQuestions
            ? window.SISOREN_CONFIG.fallbackQuestions[npcId] || []
            : [];
        const normalized = (questions || []).map((question, index) => {
            if (typeof question === 'object') return question;
            const knownAnswer = fallback[index] && fallback[index].a;
            return {
                q: question,
                a: knownAnswer || `Amirim bu soruyu sorduğunuz iyi oldu... "${question}" demiştiniz değil mi? O gece etrafta çok fazla karanlık şey oldu. Bildiğim tek şey, bina çevresindeki izlerin dağ yoluna doğru gittiğidir. Başka bir şey görmedim.`,
                difficulty: index > 2 ? 2 : 1,
                category: index > 2 ? 'yuzlestirme' : 'tanisma'
            };
        });
        while (normalized.length < 4) {
            const index = normalized.length;
            normalized.push({
                q: `Olay gecesiyle ilgili başka hangi ayrıntıyı hatırlıyorsun?`,
                a: `*Başını sallayarak* Bunu daha önce anlatmadım ama... olay gecesi saat ${index + 1} sularında buralarda kısa bir hareketlilik oldu amirim. Gidip detayları kendiniz araştırmalısınız, ben pek bir şey göremedim.`,
                difficulty: 2,
                category: 'derinlesme'
            });
        }
        return normalized.slice(0, 4);
    },

    init: function () {
        console.log("🌲 Sisören Dağ Kasabası Motoru v3.0 Başlatıldı.");
        if (window.currentActiveTown) {
            this.currentActiveTown = window.currentActiveTown;
        }
        this.registerSisorenData();
        this.setupEventListeners();
        this.setupMapObserver();
        this.createTuccarHelperWidget();
        this.createSisorenEnvelopeModal();
    },

    // =============================
    // 1. NPC & DELİL VERİ KAYDI
    // =============================
    _dataRegistered: false,

    registerSisorenData: function () {
        if (this._dataRegistered) return;
        if (window.SISOREN_CONFIG && window.SISOREN_CONFIG.buildings) {
            window.NPC_DATA = window.NPC_DATA || {};
            window.SCENE_OBJECTS = window.SCENE_OBJECTS || {};

            // Ana 13 şüpheli binalarını kaydet
            window.SISOREN_CONFIG.buildings.forEach(bld => {
                if (bld.npc) {
                    bld.npc.questions = this.normalizeNpcQuestions(bld.npcId, bld.npc.name, bld.npc.questions);
                    window.NPC_DATA[bld.npcId] = bld.npc;
                }
                if (bld.hotspots && bld.hotspots.length > 0) {
                    window.SCENE_OBJECTS[bld.npcId] = bld.hotspots;
                }
                // Çocuk NPC'leri de kaydet (bakkalda gofret alan kız, sahafta kitap okuyan çocuklar, ahırda Kerem)
                if (bld.children && bld.children.length > 0) {
                    bld.children.forEach(child => {
                        const childNpc = {
                            id: child.numericId || child.id,
                            numericId: child.numericId || child.id,
                            name: child.name,
                            gender: child.gender,
                            building: bld.title,
                            role: child.role,
                            portrait: child.portrait || null,
                            bg: child.bg, talkBg: child.talkBg,
                            greeting: child.greeting,
                            questions: this.normalizeNpcQuestions(child.numericId || child.id, child.name, child.questions),
                            isExtra: true,
                            canBeGuilty: false,
                            isChild: true,
                            parentBuildingId: bld.id
                        };
                        window.NPC_DATA[child.id] = childNpc;
                        if (child.numericId) {
                            window.NPC_DATA[child.numericId] = childNpc;
                        }
                    });
                }
            });

            // Ekstra NPC'leri kaydet (kahvehane amcaları, sokak NPC'leri, bina içi yan karakterler)
            if (window.SISOREN_CONFIG.extraNpcs) {
                window.SISOREN_CONFIG.extraNpcs.forEach(extra => {
                    window.NPC_DATA[extra.numericId] = {
                        id: extra.numericId,
                        name: extra.name,
                        gender: extra.gender,
                        building: extra.buildingId ? (window.SISOREN_CONFIG.buildings.find(b => b.id === extra.buildingId)?.title || 'Sokak') : 'Sokak',
                        role: extra.role,
                        portrait: extra.portrait,
                        bg: extra.bg,
                        talkBg: extra.talkBg,
                        greeting: extra.greeting,
                        questions: this.normalizeNpcQuestions(extra.numericId, extra.name, extra.questions),
                        isExtra: true,
                        canBeGuilty: false,
                        age: extra.age,
                        extraId: extra.id
                    };
                });
            }

            // Konuşulan ekstra NPC'leri izleme seti
            if (!window.sisorenTalkedExtraNpcs) {
                window.sisorenTalkedExtraNpcs = new Set();
            }
            this._dataRegistered = true;
        }
    },

    // =============================
    // 2. EVENT LISTENER'LAR
    // =============================
    setupEventListeners: function () {
        const sisorenTownBtn = document.querySelector('.region-town[data-town-name="Sisören"]') ||
            document.querySelector('.region-town[data-town-id="sisoren"]') ||
            document.querySelector('.town-sisoren-btn');
        if (sisorenTownBtn) {
            sisorenTownBtn.setAttribute('data-town-id', 'sisoren');
        }

        const gizemliTownBtn = document.querySelector('.region-town[data-town-id="gizemli"]');
        if (gizemliTownBtn) {
            gizemliTownBtn.addEventListener('click', () => {
                this.resetSisorenState();
            });
        }
    },

    // =============================
    // 3. DURUM SIFIRLAMA
    // =============================
    resetSisorenState: function () {
        this.currentActiveTown = 'gizemli';
        window.currentActiveTown = 'gizemli';
        this.hasShownSisorenIntro = false;
        this.introDialogCompleted = false;
        this.hasShownDualIntro = false;
        this.dualDialogStep = 0;
        this.visitedSisorenBuildings.clear();
        document.body.classList.remove('sisoren-theme');
        const tuccar = document.getElementById('tuccar-quick-tip-btn');
        if (tuccar) tuccar.remove();
        this.clearSisorenMap();

        // Kasabaya özel otopsi ve lab durumunu tazele
        if (typeof window.checkAutopsyConditions === 'function') {
            window.checkAutopsyConditions();
        }
    },

    createTuccarHelperWidget: function () {
        let widget = document.getElementById('tuccar-quick-tip-btn');
        if (widget) widget.remove();

        widget = document.createElement('div');
        widget.id = 'tuccar-quick-tip-btn';
        widget.className = 'helper-detective-widget sisoren-tuccar-widget';
        widget.title = 'Tüccar İlyas\'tan İpucu Al';
        widget.style.cssText = 'position: fixed !important; left: auto !important; right: 20px !important; bottom: 15px !important; z-index: 120; display: none;';
        widget.innerHTML = `
            <img src="images/towns/sisoren/npcler/tuccar_ilyas_helper.png" class="helper-detective-img" alt="Tüccar İlyas">
            <div class="helper-detective-badge">TÜCCAR İLYAS</div>
        `;

        widget.addEventListener('click', (e) => {
            e.stopPropagation();
            this.triggerTuccarTip();
        });

        const mapStage = document.getElementById('town-map-stage');
        const interiorStage = document.getElementById('interior-stage');

        document.body.appendChild(widget);
    },

    triggerTuccarTip: function () {
        if (window.currentActiveTown !== 'sisoren') return;

        const currentNpc = window.activeNpcId;
        const box = document.getElementById('cinematic-helper-box');
        const avatarImg = document.querySelector('.cinematic-helper-avatar img');
        const nameEl = document.querySelector('.cinematic-helper-name');

        if (box && avatarImg && nameEl) {
            box.classList.remove('cetin-speaking');
            box.classList.add('ilyas-speaking');
            avatarImg.src = 'images/towns/sisoren/npcler/tuccar_ilyas_helper.png';
            nameEl.textContent = 'DAĞCI TÜCCAR İLYAS';
            nameEl.style.color = '';
        }

        const tips = {
            201: "Telgrafçı Rüstem o gece çok tuhaftı amirim. Mesajlarda bir şeyler saklıyor olabilir.",
            202: "Kahveci İrfan sürekli kulak misafiri olur. Fincanların arasına sakladığı sırlar var.",
            203: "Sinemacı Nejat makine dairesinde neler saklıyor bir baksanız iyi olur.",
            204: "Bakkal Cemile'nin veresiye defteri hiç de masum görünmüyor amirim.",
            205: "Sahaf Hikmet eski kitapların arasında zehir tarifleri okuyor, dikkat edin.",
            206: "Muhtar Meliha'nın sahte mühürleri olduğunu duydum amirim. Kendisine güven olmaz.",
            207: "Tütüncü Nermin o gece çok gerginmiş, birileriyle kavga etmiş diyorlar.",
            208: "Çoban Durmuş dağlarda sadece koyun gütmüyor amirim, gece işleri de var.",
            209: "Tüpçü Şevket'in anahtarı çok ağırdır amirim. Kimseye acımaz.",
            210: "Hurdacı Zehra kasadaki paraları nereye saklıyor sormak lazım.",
            211: "Zeynep Teyze'nin maden kayıtlarında usulsüzlük var amirim.",
            212: "Hatice Nine'nin sandığında neler gizli bir bilseniz...",
            213: "Emine Hanım otlarla sadece şifa dağıtmıyor amirim, bazen zehir de yapıyor."
        };

        const msg = (currentNpc && tips[currentNpc])
            ? tips[currentNpc]
            : "Dağların sisi gerçeği saklar ama benim katırlarım her dedikoduyu duyar! Şüphelilerin anlattıklarına dikkat et.";

        if (typeof window.showCinematicHelper === 'function') {
            window.showCinematicHelper(msg, false, 'ilyas_interactive_tip', false, {
                speaker: 'ilyas',
                speakerName: 'DAĞCI TÜCCAR İLYAS',
                avatar: 'images/towns/sisoren/npcler/tuccar_ilyas_helper.png',
                theme: 'ilyas-speaking'
            });
        }
    },

    createEnvelopeSuccessModal: function () {
        let modal = document.getElementById('sisoren-success-modal');
        if (modal) modal.remove();

        modal = document.createElement('div');
        modal.id = 'sisoren-success-modal';
        modal.className = 'envelope-modal-backdrop hidden';
        modal.innerHTML = `
            <div class="envelope-scene">
                <div class="envelope-wrapper" id="sisoren-envelope-wrapper">
                    <div class="envelope-base">
                        <div class="envelope-top-flap" id="sisoren-envelope-top-flap"></div>
                        <div class="envelope-pocket"></div>
                        
                        <div class="envelope-letter-card" id="sisoren-envelope-letter-card">
                            <div class="envelope-stamp-badge"><i class="fa-solid fa-ribbon"></i> EMNİYET MÜDÜRLÜĞÜ GİZLİ VAKA DOSYASI</div>
                            <h2 class="letter-title">TEBRİKLER DEDEKTİF!</h2>
                            <div class="letter-subtitle">VAKA #201 (GÖLGE ŞEHİR) BAŞARIYLA ÇÖZÜLDÜ</div>
                            <div class="letter-divider"></div>
                            <p class="letter-body">
                                Gölge Şehir cinayetini ve karanlık sırlarını aydınlattınız.<br><br>
                                <strong style="color:#d97706; font-size:1.1rem;">ğŸ“ YENİ GÖREV DOSYASI #301: SİSÖREN CİNAYETİ</strong><br>
                                Sarp yamaçlar arasında göz gözü görmeyen Sisören kasabasında Madenci Halil Efendi katledildi. Yeni şüpheliler ve dağların soğuk sırları sizi bekliyor.
                            </p>
                            <button id="sisoren-envelope-accept-btn" class="letter-accept-btn">
                                GÖREVİ KABUL ET VE SİSÖREN'E GİT <i class="fa-solid fa-arrow-right"></i>
                            </button>
                        </div>
                    </div>
                    <div class="wax-seal" id="sisoren-envelope-wax-seal" title="Mührü Kır ve Zarfı Aç">
                        <i class="fa-solid fa-stamp"></i>
                        <span>AÇ</span>
                    </div>
                </div>
                <div class="envelope-prompt-text" id="sisoren-envelope-prompt-text">
                    <i class="fa-solid fa-hand-pointer"></i> Mührün üzerine tıklayarak görevi açın!
                </div>
            </div>
        `;
        document.body.appendChild(modal);

        const seal = document.getElementById('sisoren-envelope-wax-seal');
        const flap = document.getElementById('sisoren-envelope-top-flap');
        const card = document.getElementById('sisoren-envelope-letter-card');
        const prompt = document.getElementById('sisoren-envelope-prompt-text');

        const openEnvelope = () => {
            if (seal) seal.classList.add('broken');
            if (flap) flap.classList.add('opened');
            if (prompt) prompt.style.display = 'none';
            if (typeof window.playSound === 'function' && window.doorCreak) window.playSound(window.doorCreak, 0.4);
            setTimeout(() => { if (card) card.classList.add('slid-out'); }, 600);
        };

        seal?.addEventListener('click', openEnvelope);
        document.getElementById('sisoren-envelope-wrapper')?.addEventListener('click', (e) => {
            if (!flap?.classList.contains('opened')) openEnvelope();
        });

        document.getElementById('sisoren-envelope-accept-btn')?.addEventListener('click', (e) => {
            e.stopPropagation();
            modal.classList.add('hidden');
            this.showSisorenStoryIntro();
        });
    },

    // showSisorenStoryIntro — tam versiyon satır 723'te tanımlı (kısa duplicate kaldırıldı)

    // =============================
    // 4. HARİTA GÖZLEMCISI
    // =============================
    setupMapObserver: function () {
        const townMapScreen = document.getElementById('town-map-screen');
        if (!townMapScreen) return;

        const observer = new MutationObserver(() => {
            const isVisible = !townMapScreen.classList.contains('hidden');
            if (isVisible) {
                if (window.currentActiveTown === 'sisoren') {
                    document.body.classList.remove('theme-golge', 'theme-gizemli', 'golge-sehir-theme');
                    document.body.classList.add('sisoren-theme');
                    if (!townMapScreen.classList.contains('sisoren-active')) {
                        this.applySisorenState();
                    }
                    this.updateSisorenAutopsyUI();
                } else if (window.currentActiveTown === 'gizemli') {
                    document.body.classList.remove('sisoren-theme');
                    if (townMapScreen.classList.contains('sisoren-active')) {
                        this.clearSisorenMap();
                    }
                }
            }
        });

        observer.observe(townMapScreen, { attributes: true, attributeFilter: ['class'] });
    },

    // =============================
    // 5. SİSÖREN HARİTASI YÜKLEME
    // =============================
    loadSisorenMap: function () {
        window.currentActiveTown = 'sisoren';
        this.currentActiveTown = 'sisoren';
        document.body.classList.remove('golge-sehir-theme');
        document.body.classList.remove('theme-golge', 'theme-gizemli', 'golge-sehir-theme');
        document.body.classList.add('sisoren-theme');
        this.registerSisorenData();

        // Kasabaya özel çanta ve otopsi durumunu izole et
        window.isAutopsyReady = false;
        window.isAutopsyTimerStarted = false;
        if (typeof isAutopsyReady !== 'undefined') isAutopsyReady = false;
        if (typeof isAutopsyTimerStarted !== 'undefined') isAutopsyTimerStarted = false;
        if (typeof autopsyTimer !== 'undefined' && autopsyTimer) {
            clearInterval(autopsyTimer);
            autopsyTimer = null;
        }

        const townMapStage = document.getElementById('town-map-stage');
        const townMapScreen = document.getElementById('town-map-screen');
        const worldMapScreen = document.getElementById('world-map-screen');
        const storyIntroScreen = document.getElementById('story-intro-screen');

        if (!townMapStage || !townMapScreen) return;

        if (worldMapScreen) worldMapScreen.classList.add('hidden');
        if (storyIntroScreen) storyIntroScreen.classList.add('hidden');
        townMapScreen.classList.remove('hidden');

        this.applySisorenState();
        if (typeof window.checkAutopsyConditions === 'function') {
            window.checkAutopsyConditions();
        }

        if (!this.hasShownDualIntro) {
            this.hasShownDualIntro = true;
            this.dualDialogStep = 0;
            setTimeout(() => this.playDualAssistantBubbleDialogue(), 500);
        }
    },

    playDualAssistantBubbleDialogue: function () {
        const dialogs = window.SISOREN_CONFIG.assistants.introDialogue;

        const updateBubbleUI = (index) => {
            if (index < 0 || index >= dialogs.length) return;

            const current = dialogs[index];
            const isCetin = (current.speaker === 'Çetin' || current.speaker.includes('etin'));

            const speakerInfo = {
                speaker: isCetin ? 'cetin' : 'ilyas',
                speakerName: isCetin ? 'YARDIMCI DEDEKTİF ÇETİN' : 'DAĞCI TÜCCAR İLYAS',
                avatar: isCetin ? 'images/dedektif_helper.png' : 'images/towns/sisoren/npcler/tuccar_ilyas_helper.png',
                theme: isCetin ? 'cetin-speaking' : 'ilyas-speaking'
            };

            const box = document.getElementById('cinematic-helper-box');
            const avatarImg = document.querySelector('.cinematic-helper-avatar img');
            const nameEl = document.querySelector('.cinematic-helper-name');

            if (box) {
                box.classList.remove('ilyas-speaking', 'cetin-speaking', 'rifat-speaking');
                box.classList.add(speakerInfo.theme);
            }
            if (avatarImg) avatarImg.src = speakerInfo.avatar;
            if (nameEl) {
                nameEl.textContent = speakerInfo.speakerName;
                nameEl.style.color = '';
            }

            if (typeof window.showCinematicHelper === 'function') {
                window.showCinematicHelper(current.text, false, `sisoren_intro_${index}`, false, speakerInfo);
            }

            const prevBtn = document.getElementById('cinematic-prev-btn');
            if (prevBtn) {
                prevBtn.style.display = (index > 0) ? 'inline-block' : 'none';
            }
        };

        const finishIntro = () => {
            this.introDialogCompleted = true;
            window.introDialogCompleted = true;
            const box = document.getElementById('cinematic-helper-box');
            if (box) {
                box.classList.remove('ilyas-speaking', 'rifat-speaking');
                box.classList.add('cetin-speaking');
            }
            window.customHelperSkipClick = null;
            window.customHelperPrevClick = null;
            const skipBtn = document.getElementById('cinematic-skip-btn');
            if (skipBtn) skipBtn.onclick = null;
            const prevBtn = document.getElementById('cinematic-prev-btn');
            if (prevBtn) prevBtn.onclick = null;
        };

        const showNext = () => {
            if (this.dualDialogStep < dialogs.length) {
                updateBubbleUI(this.dualDialogStep);
                this.dualDialogStep++;
            } else {
                finishIntro();
            }
        };

        showNext();

        window.customHelperSkipClick = (e) => {
            if (window.isHelperTyping) {
                window.isHelperTyping = false;
                if (window.cinematicTypewriterTimeout) clearTimeout(window.cinematicTypewriterTimeout);
                const textEl = document.getElementById('cinematic-helper-text');
                if (textEl && window.currentHelperMessageText) {
                    textEl.textContent = window.currentHelperMessageText;
                    textEl.classList.add('typing-done');
                }
            } else if (this.dualDialogStep < dialogs.length) {
                showNext();
            } else {
                finishIntro();
            }
            return true;
        };

        window.customHelperPrevClick = (e) => {
            if (this.dualDialogStep > 1) {
                this.dualDialogStep -= 2;
                showNext();
            }
            return true;
        };
    },

    // =============================
    // 6. SİSÖREN DURUMU UYGULA
    // =============================
    applySisorenState: function () {
        this.registerSisorenData();

        // Sisören'e özel ses durumunu merkezi fonksiyondan çek (syncAudioState)
        if (typeof window.syncAudioState === 'function') {
            window.syncAudioState();
        }
        const townMapScreen = document.getElementById('town-map-screen');
        const townMapStage = document.getElementById('town-map-stage');
        if (!townMapScreen || !townMapStage) return;

        // Diğer kasaba temalarını temizle
        townMapScreen.classList.remove('golge-sehir-active');
        townMapStage.classList.remove('golge-sehir-active');

        if (!townMapScreen.classList.contains('sisoren-active')) {
            townMapScreen.classList.add('sisoren-active');
        }
        if (!townMapStage.classList.contains('sisoren-active')) {
            townMapStage.classList.add('sisoren-active');
        }

        // Diğer kasaba binalarını gizle
        const otherBuildings = townMapStage.querySelectorAll('.map-building:not([class*="building-sisoren-"])');
        otherBuildings.forEach(el => el.style.display = 'none');

        // Gölge Şehir binalarını temizle (ama gizemli binaları tekrar açmasını engelle)
        if (window.GolgeSehirEngine) {
            document.body.classList.remove('golge-sehir-theme');
            const bekciBtn = document.getElementById('bekci-quick-tip-btn');
            if (bekciBtn) bekciBtn.style.display = 'none';
            const golgeBuildings = townMapStage.querySelectorAll('[class*="building-golge-"]');
            golgeBuildings.forEach(el => el.remove());
        }

        // Bütün Gizemli Kasaba ve diğer kasaba binalarını kesin olarak gizle
        const allOtherBuildings = townMapStage.querySelectorAll('.map-building:not([class*="building-sisoren-"])');
        allOtherBuildings.forEach(el => {
            el.style.display = 'none';
        });

        // Tüccar İlyas butonunu göster
        const tuccarBtn = document.getElementById('tuccar-quick-tip-btn');
        if (tuccarBtn) {
            tuccarBtn.style.display = 'flex';
        }

        // BULDUM! butonunu Sisören için kesin olarak görünür yap ve en öne getir
        const foundBtn = document.getElementById('found-btn');
        if (foundBtn) {
            foundBtn.classList.remove('hidden');
            foundBtn.style.display = 'block';
            foundBtn.style.visibility = 'visible';
            foundBtn.style.zIndex = '9999';
            foundBtn.style.pointerEvents = 'auto';
        }

        // Sisören binalarını her seferinde temiz ve güncel oluştur
        townMapStage.querySelectorAll('[class*="building-sisoren-"]').forEach(el => el.remove());
        window.SISOREN_CONFIG.buildings.forEach(bld => {
            const buildingDiv = document.createElement('div');
            buildingDiv.className = `map-building building-sisoren-${bld.id}`;
            buildingDiv.setAttribute('data-npc-id', bld.npcId);
            buildingDiv.setAttribute('data-building-id', bld.id);
            buildingDiv.title = bld.title;

            Object.assign(buildingDiv.style, bld.style);
            // Boyut ve görünürlük güvencesi
            buildingDiv.style.display = 'flex';
            buildingDiv.style.visibility = 'visible';
            buildingDiv.style.zIndex = '50';
            buildingDiv.style.pointerEvents = 'auto';
            buildingDiv.style.cursor = 'pointer';

            const hoverTag = document.createElement('div');
            hoverTag.className = 'building-hover-tag';
            hoverTag.innerHTML = `<i class="${bld.icon}"></i> ${bld.hoverTag}`;
            buildingDiv.appendChild(hoverTag);

            // Ziyaret edilmiş bina kontrolü
            const isVisited = this.visitedSisorenBuildings.has(bld.npcId) || (window.visitedBuildings && window.visitedBuildings.has(bld.npcId));
            if (isVisited) {
                buildingDiv.classList.add('visited');
                hoverTag.innerHTML = `<i class="fa-solid fa-lock"></i> İNCELEME TAMAMLANDI`;
            }

            // Bina tıklama olayı
            buildingDiv.onclick = (e) => {
                e.preventDefault();
                e.stopPropagation();
                this.enterSisorenBuilding(bld, buildingDiv);
            };

            townMapStage.appendChild(buildingDiv);
        });

        // Çetin yardımcı mesajı (ilk giriş)
        if (!this.hasShownSisorenIntro) {
            this.hasShownSisorenIntro = true;
            setTimeout(() => this.playDualAssistantBubbleDialogue(), 500);
        }

        // Sokak NPC konuşma noktaları
        document.querySelectorAll('.sisoren-street-npc').forEach(el => el.remove());
        const streetNpcs = (window.SISOREN_CONFIG.extraNpcs || []).filter(e => e.buildingId === null);
        streetNpcs.forEach(extra => {
            if (!extra.mapPos) return;
            const marker = document.createElement('div');
            marker.className = 'sisoren-street-npc sisoren-npc-bubble-marker clue-hotspot';
            marker.title = `${extra.name} ile Konuş`;
            marker.style.top = extra.mapPos.top;
            marker.style.left = extra.mapPos.left;
            marker.style.position = 'absolute';
            marker.style.zIndex = '100';
            marker.style.cursor = 'pointer';

            marker.innerHTML = `
                <div class="sisoren-npc-bubble" style="font-size: 24px; color: white; text-shadow: 0 0 10px rgba(0,0,0,0.8); text-align: center;"><i class="fa-solid fa-comment-dots"></i></div>
                <div class="sisoren-npc-tag" style="background: rgba(0,0,0,0.7); color: white; padding: 2px 6px; border-radius: 4px; font-size: 12px; white-space: nowrap; margin-top: 5px; text-align: center;">${extra.name}</div>
            `;

            marker.onclick = (e) => {
                e.preventDefault();
                e.stopPropagation();
                this.openExtraNpcTalk(extra);
            };
            townMapStage.appendChild(marker);
        });
    },



    // =============================
    // 6.4 SİSÖREN ÖZEL GÖREV ZARFI & KART ANİMASYONU
    // =============================
    createSisorenEnvelopeModal: function () {
        let modal = document.getElementById('sisoren-success-modal');
        if (modal) modal.remove();

        modal = document.createElement('div');
        modal.id = 'sisoren-success-modal';
        modal.className = 'envelope-modal-backdrop hidden';
        modal.innerHTML = `
            <div class="envelope-scene sisoren-envelope-scene">
                <div class="envelope-wrapper" id="sisoren-envelope-wrapper">
                    <div class="envelope-base">
                        <div class="envelope-top-flap" id="sisoren-envelope-top-flap"></div>
                        <div class="envelope-pocket"></div>
                        
                        <!-- Zarfın İçinden Çıkan Eski Parşömen Görev Kartı -->
                        <div class="envelope-letter-card sisoren-letter-card" id="sisoren-envelope-letter-card">
                            <div class="envelope-stamp-badge"><i class="fa-solid fa-ribbon"></i> EMNİYET MÜDÜRLÜĞÜ GİZLİ VAKA DOSYASI</div>
                            <h2 class="letter-title">TEBRİKLER DEDEKTİF!</h2>
                            <div class="letter-subtitle">VAKA #201 (GÖLGE ŞEHİR) BAŞARIYLA ÇÖZÜLDÜ</div>
                            <div class="letter-divider"></div>
                            <p class="letter-body">
                                Gölge Şehir cinayetini ve karanlık sırlarını üstün bir dedektiflik zekasıyla aydınlattınız. Katili bularak adaleti sağladınız.<br><br>
                                <strong style="color:#0f766e; font-size:1.1rem;">ğŸ“ YENİ GÖREV DOSYASI #301: SİSÖREN DAĞ KASABASI</strong><br>
                                Uçsuz bucaksız sisli dağların zirvesinde yeni bir ceset bulundu. Sarp yamaçlar ve yoğun sis altında 13 şüpheli ve saklı sırlar sizi bekliyor.
                            </p>
                            <button id="sisoren-envelope-accept-btn" class="letter-accept-btn" style="background-color: #0f766e;">
                                GÖREVİ KABUL ET VE SİSÖREN'E GİT <i class="fa-solid fa-arrow-right"></i>
                            </button>
                        </div>
                    </div>

                    <!-- Koyu Yeşil Balmumu Mühür -->
                    <div class="wax-seal sisoren-wax-seal" id="sisoren-envelope-wax-seal" title="Mührü Kır ve Zarfı Aç">
                        <i class="fa-solid fa-stamp"></i>
                        <span>AÇ</span>
                    </div>
                </div>

                <div class="envelope-prompt-text" id="sisoren-envelope-prompt-text">
                    <i class="fa-solid fa-hand-pointer"></i> Mührün üzerine tıklayarak tebrik ve görev zarfını açın!
                </div>
            </div>
        `;
        document.body.appendChild(modal);

        const seal = document.getElementById('sisoren-envelope-wax-seal');
        const flap = document.getElementById('sisoren-envelope-top-flap');
        const card = document.getElementById('sisoren-envelope-letter-card');
        const prompt = document.getElementById('sisoren-envelope-prompt-text');

        const openEnvelope = () => {
            if (seal) seal.classList.add('broken');
            if (flap) flap.classList.add('opened');
            if (prompt) prompt.style.display = 'none';

            if (typeof window.playSound === 'function' && window.doorCreak) {
                window.playSound(window.doorCreak, 0.4);
            }

            setTimeout(() => {
                if (card) card.classList.add('slid-out');
            }, 600);
        };

        seal?.addEventListener('click', openEnvelope);
        document.getElementById('sisoren-envelope-wrapper')?.addEventListener('click', (e) => {
            if (!flap?.classList.contains('opened')) {
                openEnvelope();
            }
        });

        document.getElementById('sisoren-envelope-accept-btn')?.addEventListener('click', (e) => {
            e.stopPropagation();
            modal.classList.add('hidden');
            this.showSisorenStoryIntro();
        });
    },

    showSisorenSuccessModalBeforeStory: function () {
        window.currentActiveTown = 'sisoren';
        this.currentActiveTown = 'sisoren';
        document.body.classList.add('sisoren-theme');
        this.registerSisorenData();

        // Kasabaya özel delil izolasyonu
        // Bag is isolated by app.js setter

        let modal = document.getElementById('sisoren-success-modal');
        if (!modal) {
            this.createSisorenEnvelopeModal();
            modal = document.getElementById('sisoren-success-modal');
        }

        const seal = document.getElementById('sisoren-envelope-wax-seal');
        const flap = document.getElementById('sisoren-envelope-top-flap');
        const card = document.getElementById('sisoren-envelope-letter-card');
        const prompt = document.getElementById('sisoren-envelope-prompt-text');

        if (seal) seal.classList.remove('broken');
        if (flap) flap.classList.remove('opened');
        if (card) card.classList.remove('slid-out');
        if (prompt) prompt.style.display = 'block';

        if (modal) {
            modal.classList.remove('hidden');
            setTimeout(() => {
                const s = document.getElementById('sisoren-envelope-wax-seal');
                if (s && !s.classList.contains('broken')) {
                    s.click();
                }
            }, 1000);
        } else {
            this.showSisorenStoryIntro();
        }
    },

    // =============================
    // 6.5 SİSÖREN DİĞER KASABALARDAN FARKLI HİKAYE GİRİŞİ (DAKTİLO EFEKTİ)
    // =============================
    showSisorenStoryIntro: function () {
        window.currentActiveTown = 'sisoren';
        this.currentActiveTown = 'sisoren';
        document.body.classList.remove('golge-sehir-theme');
        document.body.classList.remove('theme-golge', 'theme-gizemli', 'golge-sehir-theme');
        document.body.classList.add('sisoren-theme');
        this.registerSisorenData();

        // Kasabaya özel çanta izolasyonu (Gölge Şehir patternine uyumlu)
        // Bag is isolated by app.js setter

        // Kasabaya özel otopsi ve lab durumunu hazırla
        window.isAutopsyReady = false;
        window.isAutopsyTimerStarted = false;
        if (typeof isAutopsyReady !== 'undefined') isAutopsyReady = false;
        if (typeof isAutopsyTimerStarted !== 'undefined') isAutopsyTimerStarted = false;
        if (typeof autopsyTimer !== 'undefined' && autopsyTimer) {
            clearInterval(autopsyTimer);
            autopsyTimer = null;
        }

        // Her yeni Sisören oturumunda 201-213 arası rastgele yeni katil belirle
        fetch('/api/sisoren/reset', { method: 'POST' })
            .then(res => res.json())
            .then(data => {
                if (data && data.guiltyNpcId) {
                    window.guiltyNpcId = data.guiltyNpcId;
                    if (typeof guiltyNpcId !== 'undefined') {
                        guiltyNpcId = data.guiltyNpcId;
                    }
                    console.log("ğŸ² Sisören Rastgele Yeni Katil Belirlendi:", data.guiltyNpcId);
                }
            })
            .catch(err => console.error("Sisören katil sıfırlama hatası:", err));

        const worldMapScreen = document.getElementById('world-map-screen');
        const storyIntroScreen = document.getElementById('story-intro-screen');
        const storyTextEl = document.getElementById('typewriter-text');
        const storyContinueBtn = document.getElementById('story-continue-btn');
        const skipStoryBtn = document.getElementById('skip-story-btn');
        const cursor = document.querySelector('.story-cursor');
        const storyBadge = document.querySelector('.story-badge');

        if (storyBadge) {
            storyBadge.innerHTML = '<i class="fa-solid fa-mountain"></i> VAKA DOSYASI #301 — SİSÖREN DAĞ KASABASI';
        }

        if (!storyIntroScreen || !storyTextEl) {
            this.loadSisorenMap();
            return;
        }

        if (worldMapScreen) worldMapScreen.classList.add('hidden');
        storyIntroScreen.classList.remove('hidden');

        // Hide any lingering cinematic helper box (e.g. from world map) to prevent covering buttons
        const cinematicBox = document.getElementById('cinematic-helper-box');
        if (cinematicBox) cinematicBox.classList.add('hidden');

        storyTextEl.textContent = '';
        if (cursor) cursor.style.display = 'inline-block';
        if (skipStoryBtn) skipStoryBtn.classList.remove('hidden');
        if (storyContinueBtn) storyContinueBtn.classList.add('hidden');

        const fullText = (window.SISOREN_CONFIG && window.SISOREN_CONFIG.storyIntroText)
            ? window.SISOREN_CONFIG.storyIntroText
            : "Uçsuz bucaksız sisli dağların zirvesinde, çam ormanlarıyla çevrili tekinsiz bir dağ kasabası: Sisören...\n\nSarp yamaçlar arasında göz gözü görmeyen yoğun bir sis tabakası kasabanın üzerine çökmüş durumda. Bakkalın önündeki ıslak kasalar, tüpçüdeki gaz silindirleri, ahırın çamurlu çitleri arasındaki hayvanlar ve telgraf tellerinin vızıltısı... Bu dağ kasabasında sırlar sisin ardına gizlenir. 13 şüpheli, karanlık sırlar ve sarp yamaçlar... Sis perdesini aralamaya hazır mısınız?";

        let charIndex = 0;
        const speed = 35;
        if (window.sisorenTypewriterTimer) clearTimeout(window.sisorenTypewriterTimer);

        if (typeof window.playLoopSound === 'function' && window.typewriterSound) {
            window.playLoopSound(window.typewriterSound, 0.4);
        }

        const finishSisorenTypewriter = () => {
            if (window.sisorenTypewriterTimer) clearTimeout(window.sisorenTypewriterTimer);
            if (typeof window.stopSound === 'function' && window.typewriterSound) {
                window.stopSound(window.typewriterSound);
            }
            storyTextEl.textContent = fullText;
            if (cursor) cursor.style.display = 'none';
            if (skipStoryBtn) skipStoryBtn.classList.add('hidden');
            if (storyContinueBtn) {
                storyContinueBtn.classList.remove('hidden');
                storyContinueBtn.innerHTML = '<i class="fa-solid fa-magnifying-glass"></i> SORUŞTURMAYI BAŞLAT';
            }
        };

        const typeChar = () => {
            if (charIndex < fullText.length) {
                storyTextEl.textContent += fullText.charAt(charIndex);
                charIndex++;
                window.sisorenTypewriterTimer = setTimeout(typeChar, speed);
            } else {
                finishSisorenTypewriter();
            }
        };

        typeChar();
    },

    // =============================
    // 7. BİNA GİRİŞ İŞLEMİ (İÇ MEKAN GÖRSELİ & MEKANDAKİ KİŞİLER)
    // =============================
    enterSisorenBuilding: function (bld, buildingDiv) {
        // Zaten ziyaret edilmiş mi?
        const isVisited = this.visitedSisorenBuildings.has(bld.npcId) || (window.visitedBuildings && window.visitedBuildings.has(bld.npcId));
        if (isVisited) {
            if (typeof window.showCinematicHelper === 'function') {
                window.showCinematicHelper(`Amirims, ${bld.title} binasını zaten inceledik. Başka bir binaya bakalım!`, false, `sisoren_visited_${bld.id}`);
            }
            return;
        }

        // NPC verisini kaydet
        this.registerSisorenData();
        const npc = window.NPC_DATA[bld.npcId];
        if (!npc) return;

        // Binayı ziyaret edilmiş olarak işaretle
        this.visitedSisorenBuildings.add(bld.npcId);
        if (window.visitedBuildings) {
            window.visitedBuildings.add(bld.npcId);
        }

        // Hover tag'ı güncelle
        buildingDiv.classList.add('visited');
        const tag = buildingDiv.querySelector('.building-hover-tag');
        if (tag) {
            tag.innerHTML = `<i class="fa-solid fa-lock"></i> İNCELEME TAMAMLANDI`;
        }

        // NPC konuşma verileri (activeNpcId)
        window.activeNpcId = bld.npcId;

        // Bina içi ekstra NPC'leri bul
        const buildingExtras = (window.SISOREN_CONFIG.extraNpcs || []).filter(e => e.buildingId === bld.id);
        const buildingChildren = bld.children || [];

        // Bina iç mekan ekranını aç (interior-screen)
        this.openSisorenInteriorScreen(bld, npc, buildingExtras, buildingChildren);

        // Banter çal
        this.playBuildingBanter(bld.npcId);

        // Otopsi sayacını güncelle
        if (typeof window.checkAutopsyConditions === 'function') {
            setTimeout(() => window.checkAutopsyConditions(), 500);
        }
    },

    currentBanterTimer: null,
    currentBanterStep: 0,
    currentBanterList: [],

    playBuildingBanter: function (npcId) {
        if (typeof window.showCinematicHelper !== 'function') return;

        const banters = {
            201: [
                { speaker: 'tuccar', text: 'Celal amcanın kahvehanesi... Bütün dedikoduların, yalanların demlendiği yer.' },
                { speaker: 'cetin', text: 'Boş laflara karnımız tok İlyas. Bize somut kanıt lazım, dedikodu değil.' },
                { speaker: 'tuccar', text: 'Bazen en gerçek ipucu fısıltılar arasında saklıdır Çetin Bey. Gözünüzü dört açın.' }
            ],
            202: [
                { speaker: 'tuccar', text: 'Bakkal Cemile teyze... Hem dükkanı yönetir hem de mahallenin bekçiliğini yapar. Ekstra tüplerin nereye gittiğini en iyi o bilir.' },
                { speaker: 'cetin', text: 'Eksik hesap kayıtları ve borç listesi bizim için daha önemli.' }
            ],
            203: [
                { speaker: 'tuccar', text: 'Sahaf Hikmet, kasabanın hafızasıdır. Ama bazı kitapların sayfaları kasten yırtılmıştır.' },
                { speaker: 'cetin', text: 'Eski gazetelerdeki cinayet haberlerini mi kastediyorsun?' }
            ],
            204: [
                { speaker: 'tuccar', text: 'Eski Ahır. Çoban Durmuş\'un mekanı. Dağ yollarından gelen gizemli ayak izleri genelde burada son bulur.' }
            ],
            205: [
                { speaker: 'tuccar', text: 'Telgrafhane... Kasabanın dış dünyayla tek bağlantısı. Şifreli mesajlar buradan geçer.' }
            ],
            206: [
                { speaker: 'tuccar', text: 'Terkedilmiş sinema. Eskiden burası kasabanın kalbiydi, şimdi ise sadece toz ve gölgeler var.' }
            ],
            207: [
                { speaker: 'tuccar', text: 'Muhtarlık. Meliha hanım bu kasabayı demir yumrukla yönetir.' }
            ],
            208: [
                { speaker: 'tuccar', text: 'Tütüncü dükkanı. Dumanlı odaların ardında ne sırlar yatıyor kim bilir.' }
            ],
            209: [
                { speaker: 'tuccar', text: 'Tüpçü. Eksik tüp meselesi hala çözülemedi amirim. Burası kilit nokta olabilir.' }
            ],
            210: [
                { speaker: 'tuccar', text: 'Hurdacı Zehra. Paslı demirlerin ve kayıp eşyaların mezarlığı.' }
            ],
            211: [
                { speaker: 'tuccar', text: 'Zeynep Teyze\'nin evi. Penceresinden kasabadaki her hareketi izler.' }
            ],
            212: [
                { speaker: 'tuccar', text: 'Hatice Nine\'nin taş evi. O kadar yaşlı ki, kasabanın kurulduğu günleri bile hatırlıyor olabilir.' }
            ],
            213: [
                { speaker: 'tuccar', text: 'Emine Hanım\'ın yamaçtaki evi. Buradan tüm kasaba ayaklarınızın altındadır, özellikle de gece vakti.' }
            ]
        };

        if (!banters[npcId]) return;

        this.currentBanterList = banters[npcId];
        this.currentBanterStep = 0;
        this.advanceBanter();
    },

    advanceBanter: function () {
        if (this.currentBanterStep >= this.currentBanterList.length) {
            return;
        }

        const current = this.currentBanterList[this.currentBanterStep];
        const speakerInfo = {
            speaker: current.speaker, // 'tuccar' or 'cetin'
            speakerName: current.speaker === 'tuccar' ? 'TÜCCAR İLYAS (KASABALI)' : 'YARDIMCI DEDEKTİF ÇETİN',
            avatar: current.speaker === 'tuccar' ? 'images/towns/sisoren/npcler/tuccar_ilyas_helper.png' : 'images/dedektif_helper.png',
            theme: current.speaker === 'tuccar' ? 'ilyas-speaking' : 'cetin-speaking'
        };

        window.showCinematicHelper(current.text, false, `sisoren_banter_${this.currentBanterStep}`, false, speakerInfo);
        this.currentBanterStep++;
    },

    // =============================
    // 7.5 SİSÖREN BİNA İÇ MEKAN EKRANI
    // =============================
    openSisorenInteriorScreen: function (bld, npc, buildingExtras, buildingChildren) {
        const townMapScreen = document.getElementById('town-map-screen');
        const interiorScreen = document.getElementById('interior-screen');
        const stageCanvas = document.getElementById('interior-stage-canvas');
        const talkBtn = document.getElementById('talk-npc-btn');
        const talkNameEl = document.getElementById('talk-npc-name');
        const hotspotsContainer = document.getElementById('hotspots-container');

        if (!interiorScreen) return;

        // Haritayı gizle, bina içini göster
        if (townMapScreen) townMapScreen.classList.add('hidden');
        interiorScreen.classList.remove('hidden');
        interiorScreen.setAttribute('data-npc-id', bld.npcId);

        // İç mekan: tek görsel, tekrarsız ve tam ekran.
        this.clearInteriorMarkers();
        const interiorImgUrl = bld.interiorImg || bld.npc.bg || bld.npc.talkBg;
        interiorScreen.style.backgroundImage = 'none';
        interiorScreen.style.backgroundColor = '#030806';

        if (stageCanvas && interiorImgUrl) {
            stageCanvas.style.backgroundImage = `url('${interiorImgUrl}?v=${Date.now()}')`;
            stageCanvas.style.setProperty('background-size', 'cover', 'important');
            stageCanvas.style.backgroundPosition = 'center center';
            stageCanvas.style.backgroundRepeat = 'no-repeat';
            stageCanvas.style.backgroundColor = '#030806';
        }

        // Ana NPC Konuşma Butonu metni
        if (talkNameEl) {
            talkNameEl.innerText = `${npc.name} ile Konuş`;
        }
        if (talkBtn) {
            talkBtn.style.opacity = '1';
            talkBtn.style.cursor = 'pointer';
            talkBtn.onclick = () => {
                if (typeof window.openNpcTalk === 'function') {
                    window.openNpcTalk(bld.npcId);
                }
            };
        }

        if (hotspotsContainer) hotspotsContainer.innerHTML = '';

        // Bina içindeki diğer karakterler için panel göster (Celal Amca, Hamdi Dayı, Gofretli Kız vs.)
        const allExtrasInBuilding = [...buildingExtras, ...buildingChildren];
        // this.renderPrimaryNpc(npc, bld); // Disabled as per user request to remove photo button
        this.renderInteriorOccupants(allExtrasInBuilding, bld, npc);

        // DELİLLERİ RENDER ET
        this.renderSisorenBuildingClueHotspots(bld);

        // Ses efektleri
        if (typeof window.playSound === 'function' && window.doorCreak) {
            window.playSound(window.doorCreak, 0.7);
            setTimeout(() => {
                if (window.doorClose) window.playSound(window.doorClose, 0.5);
            }, 600);
        }

        // Çetin yardımcı bilgilendirmesi
        setTimeout(() => {
            if (typeof window.showCinematicHelper === 'function') {
                const extraNames = allExtrasInBuilding.map(e => e.name).join(', ');
                const extraMsg = allExtrasInBuilding.length > 0
                    ? `\n\n👥 Mekanda ayrıca konuşabileceğiniz kişiler var: ${extraNames}`
                    : '';
                window.showCinematicHelper(
                    `Amirims, ${bld.title} içine girdik. Hem mekan sahibi ${npc.name} ile hem de mekandaki diğer kişilerle konuşabilirsiniz!${extraMsg}`,
                    false, `sisoren_enter_${bld.id}`
                );
            }
        }, 1200);
    },

    // =============================
    // 7.6 BİNA İÇİ NPC KONUŞMA BALONLARI (KARAKTERİN ÜZERİNDE)
    // =============================
    clearInteriorMarkers: function () {
        document.querySelectorAll('.sisoren-interior-npc-marker').forEach(el => el.remove());
        document.querySelectorAll('.sisoren-primary-npc-marker').forEach(el => el.remove());
        const panel = document.getElementById('sisoren-extra-npc-panel');
        if (panel) panel.remove();
    },

    renderSisorenBuildingClueHotspots: function (bld) {
        const stageCanvas = document.getElementById('interior-stage-canvas');
        if (!stageCanvas) return;

        stageCanvas.querySelectorAll('.clue-hotspot').forEach(el => el.remove());
        const container = document.getElementById('hotspots-container');
        if (container) container.innerHTML = '';

        if (!bld.hotspots) return;

        bld.hotspots.forEach((clue) => {
            const spot = document.createElement('div');
            spot.className = 'sisoren-clue-hotspot clue-hotspot';
            spot.title = clue.name;
            spot.style.top = clue.top;
            spot.style.left = clue.left;
            spot.style.width = '70px';
            spot.style.height = '70px';
            spot.style.position = 'absolute';
            spot.style.cursor = 'pointer';
            spot.style.zIndex = '9999';

            const img = document.createElement('img');
            img.src = clue.img;
            img.alt = clue.name;
            img.style.width = '100%';
            img.style.height = '100%';
            img.style.objectFit = 'contain';

            const label = document.createElement('span');
            label.className = 'sisoren-clue-label';
            label.textContent = clue.name;
            label.style.display = 'block';
            label.style.background = 'rgba(0,0,0,0.8)';
            label.style.color = '#fff';
            label.style.padding = '2px 5px';
            label.style.borderRadius = '4px';
            label.style.fontSize = '12px';
            label.style.textAlign = 'center';
            label.style.marginTop = '5px';

            spot.appendChild(img);
            spot.appendChild(label);

            spot.addEventListener('click', (e) => {
                e.stopPropagation();
                if (typeof window.openBuildingClueModal === 'function') {
                    window.openBuildingClueModal(clue, bld.npcId);
                } else if (typeof window.openClueInspect === 'function') {
                    window.openClueInspect(clue, bld.npcId);
                }
            });

            stageCanvas.appendChild(spot);
        });
    },

    renderPrimaryNpc: function (npc, bld) {
        const stageCanvas = document.getElementById('interior-stage-canvas');
        if (!stageCanvas || !npc || !bld.primaryNpcPos) return;

        const marker = document.createElement('button');
        marker.type = 'button';
        marker.className = 'sisoren-primary-npc-marker';
        marker.style.top = bld.primaryNpcPos.top;
        marker.style.left = bld.primaryNpcPos.left;
        marker.title = `${npc.name} ile Konuş`;
        marker.innerHTML = `<span><i class="fa-solid fa-comment-dots"></i> ${npc.name}</span>`;
        marker.onclick = (event) => {
            event.stopPropagation();
            if (typeof window.openNpcTalk === 'function') window.openNpcTalk(npc.id);
        };
        stageCanvas.appendChild(marker);
    },

    getInteriorPos: function (extra, bldId) {
        if (extra.interiorPos) return extra.interiorPos;
        const layout = window.SISOREN_INTERIOR_LAYOUTS && window.SISOREN_INTERIOR_LAYOUTS[bldId];
        const layoutPos = layout && layout[extra.id || extra.extraId];
        if (layoutPos) return layoutPos;
        const fallback = (window.SISOREN_INTERIOR_POSITIONS || {})[extra.id || extra.extraId];
        if (fallback) return fallback;

        // Varsayılan pozisyonlar - binaya göre farklı konumlar
        const defaultPositions = {
            'kahvehane': { top: '45%', left: '60%' },
            'bakkal': { top: '55%', left: '65%' },
            'sahaf': { top: '50%', left: '70%' },
            'ahir': { top: '60%', left: '65%' },
            'telgrafhane': { top: '45%', left: '75%' },
            'sinema': { top: '50%', left: '80%' },
            'muhtarlik': { top: '40%', left: '70%' },
            'tutuncu': { top: '55%', left: '65%' },
            'tupcu': { top: '50%', left: '70%' },
            'hurdaci': { top: '45%', left: '75%' },
            'kasabali_evi_1': { top: '50%', left: '70%' },
            'kasabali_evi_2': { top: '55%', left: '65%' },
            'kasabali_evi_3': { top: '45%', left: '75%' }
        };

        return defaultPositions[bldId] || { top: '50%', left: '50%' };
    },

    renderInteriorOccupants: function (extras, bld, mainNpc) {
        if (!extras || extras.length === 0) return;

        const stageCanvas = document.getElementById('interior-stage-canvas');
        if (!stageCanvas) return;

        extras.forEach((extra, idx) => {
            const pos = this.getInteriorPos(extra, bld.id);
            const marker = document.createElement('div');
            marker.className = 'sisoren-interior-npc-marker';
            marker.style.top = pos.top;
            marker.style.left = pos.left;
            marker.title = `${extra.name} ile Konuş`;

            marker.setAttribute('aria-label', `${extra.name} ile konuş`);
            marker.innerHTML = `
                <span class="sisoren-npc-bubble"><i class="fa-solid fa-comment-dots"></i></span>
                <span class="sisoren-npc-tag">${extra.name}</span>
            `;

            marker.onclick = (e) => {
                e.stopPropagation();
                this.openExtraNpcTalk(extra);
            };

            stageCanvas.appendChild(marker);
        });
    },

    // =============================
    // 7.7 EKSTRA NPC KONUŞMA EKRANI
    // =============================
    openExtraNpcTalk: function (extra) {
        const numericId = extra.numericId || extra.id;

        // Konuşulan ekstra NPC'yi suçlama ekranına eklemek için izle
        if (!window.sisorenTalkedExtraNpcs) window.sisorenTalkedExtraNpcs = new Set();
        window.sisorenTalkedExtraNpcs.add(numericId);

        // NPC_DATA'ya ekstra NPC'yi kaydet (zaten registerSisorenData'da yapılıyor ama garanti olsun)
        if (!window.NPC_DATA[numericId]) {
            window.NPC_DATA[numericId] = {
                id: numericId,
                name: extra.name,
                gender: extra.gender,
                building: extra.buildingId || 'Sokak',
                role: extra.role,
                portrait: extra.portrait,
                bg: extra.bg,
                talkBg: extra.talkBg,
                greeting: extra.greeting,
                questions: extra.questions || [],
                isExtra: true,
                canBeGuilty: false
            };
        }

        // Aktif NPC'yi ayarla ve konuşma ekranını aç
        window.activeNpcId = numericId;
        if (typeof window.openNpcTalk === 'function') {
            window.openNpcTalk(numericId);
        }
    },

    // =============================
    // 8. SİSÖREN HARİTASI TEMİZLE
    // =============================
    clearSisorenMap: function () {
        document.body.classList.remove('sisoren-theme');
        const townMapScreen = document.getElementById('town-map-screen');
        const townMapStage = document.getElementById('town-map-stage');

        this.clearInteriorMarkers();
        document.querySelectorAll('.sisoren-street-npc').forEach(el => el.remove());

        if (townMapScreen) {
            townMapScreen.classList.remove('sisoren-active');
        }
        if (townMapStage) {
            townMapStage.classList.remove('sisoren-active');
            townMapStage.querySelectorAll('[class*="building-sisoren-"]').forEach(el => el.remove());

            // Gizemli kasaba binalarını geri göster
            const gizemliBuildings = townMapStage.querySelectorAll('.map-building:not([class*="building-golge-"]):not([class*="building-sisoren-"])');
            gizemliBuildings.forEach(el => el.style.display = '');
        }

        const tuccarBtn = document.getElementById('tuccar-quick-tip-btn');
        if (tuccarBtn) {
            tuccarBtn.style.display = 'none';
        }
    },

    resetSisorenState: function () {
        this.hasShownSisorenIntro = false;
        this.introDialogCompleted = false;
        this.hasShownDualIntro = false;
        this.dualDialogStep = 0;
        if (this.visitedSisorenBuildings) {
            this.visitedSisorenBuildings.clear();
        }
        if (window.sisorenTalkedExtraNpcs) {
            window.sisorenTalkedExtraNpcs.clear();
        }
        window.submittedForensicCountSisoren = 0;
        window.isAutopsyReadySisoren = false;
        window.isAutopsyTimerStartedSisoren = false;
        this.clearSisorenMap();
    },

    // =============================
    // 9. OTOPSİ UI GÜNCELLEMESİ
    // =============================
    updateSisorenAutopsyUI: function () {
        if (window.currentActiveTown !== 'sisoren') return;

        const reqBuildings = 5;
        const reqLabs = 5;

        let buildingCount = 0;
        let uniqueSisorenVisited = new Set();
        if (this.visitedSisorenBuildings) {
            this.visitedSisorenBuildings.forEach(id => uniqueSisorenVisited.add(id));
        }
        if (window.visitedBuildings) {
            window.visitedBuildings.forEach(id => {
                if (id >= 201 && id <= 213) uniqueSisorenVisited.add(id);
            });
        }
        buildingCount = uniqueSisorenVisited.size;

        const labCount = window.submittedForensicCountSisoren || 0;

        const badgeText = document.getElementById('forensic-badge-text');
        if (badgeText) {
            if (labCount === 0) {
                badgeText.textContent = `0/${reqLabs} LAB GEREKLİ`;
            } else if (labCount < reqLabs) {
                badgeText.textContent = `${labCount}/${reqLabs} LAB GÖNDERİLDİ`;
            } else {
                badgeText.textContent = `✓ ${labCount} LAB GÖNDERİLDİ`;
            }
        }

        const container = document.getElementById('autopsy-timer-container');
        if (!container) return;

        if (window.isAutopsyReadySisoren) {
            container.classList.remove('hidden', 'pending', 'active-timer');
            container.classList.add('ready');
            container.innerHTML = '<i class="fa-solid fa-file-signature"></i> ✓ OTOPSİ RAPORU HAZIR! (TIKLA)';
            return;
        }

        if (window.isAutopsyTimerStartedSisoren) return;

        container.classList.remove('hidden', 'ready', 'active-timer');
        container.classList.add('pending');
        container.innerHTML = `<i class="fa-solid fa-clock-rotate-left"></i> OTOPSİ: BİNA ${buildingCount}/${reqBuildings} | LAB ${labCount}/${reqLabs}`;
    },

};

// =============================
// BAŞLATMA
// =============================
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => window.SisorenEngine.init());
} else {
    window.SisorenEngine.init();
}
