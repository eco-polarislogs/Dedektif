/**
 * GÖLGE ŞEHİR (SHADOW CITY) ENGINE MODULE v8.0
 * %100 Türkçe, Noir Zarf Animasyonu, Ayakta Duran Bekçi Rıfat Yardımcı Widget'ı
 */

window.GolgeSehirEngine = {
    currentActiveTown: 'gizemli',
    hasShownDualIntro: false,
    introDialogCompleted: false,
    visitedGolgeBuildings: new Set(),
    dualDialogStep: 0,

    init: function () {
        console.log("⚡ Gölge Şehir Motoru v8.0 Başlatıldı (%100 Türkçe)");
        this.registerGolgeSehirData();
        this.setupEventListeners();
        this.setupMapObserver();
        this.createBekciHelperWidget();
        this.createEnvelopeSuccessModal();
    },

    registerGolgeSehirData: function () {
        if (window.GOLGE_SEHIR_CONFIG && window.GOLGE_SEHIR_CONFIG.buildings) {
            window.NPC_DATA = window.NPC_DATA || {};
            window.SCENE_OBJECTS = window.SCENE_OBJECTS || {};

            window.GOLGE_SEHIR_CONFIG.buildings.forEach(bld => {
                window.NPC_DATA[bld.npcId] = bld.npc;
                window.SCENE_OBJECTS[bld.npcId] = bld.hotspots;
            });
        }
    },

    setupEventListeners: function () {
        const golgeTownBtn = document.querySelector('.region-town[data-town-name="Gölgeşehir"]') || document.querySelector('.region-town[data-town-id="golge_sehir"]');
        if (golgeTownBtn) {
            golgeTownBtn.setAttribute('data-town-id', 'golge_sehir');
        }

        const gizemliTownBtn = document.querySelector('.region-town[data-town-id="gizemli"]');
        if (gizemliTownBtn) {
            gizemliTownBtn.addEventListener('click', () => {
                this.resetGolgeState();
            });
        }
    },

    resetGolgeState: function () {
        this.currentActiveTown = 'gizemli';
        window.currentActiveTown = 'gizemli';
        window.hasEnteredGolgeSehir = false;
        this.hasShownDualIntro = false;
        this.introDialogCompleted = false;
        this.dualDialogStep = 0;
        this.visitedGolgeBuildings.clear();
        document.body.classList.remove('golge-sehir-theme');
        this.clearGolgeSehirMap();
        const successModal = document.getElementById('golge-success-modal');
        if (successModal) successModal.classList.add('hidden');
        if (window.golgeTypewriterTimer) clearTimeout(window.golgeTypewriterTimer);

        // Kasabaya özel otopsi ve lab durumunu tazele
        if (typeof window.checkAutopsyConditions === 'function') {
            window.checkAutopsyConditions();
        }
    },

    // 1. ESKİ KARANLIK NOİR DEDEKTİF TEBRİK ZARFI & KART ANİMASYONU
    createEnvelopeSuccessModal: function () {
        let modal = document.getElementById('golge-success-modal');
        if (modal) modal.remove();

        modal = document.createElement('div');
        modal.id = 'golge-success-modal';
        modal.className = 'envelope-modal-backdrop hidden';
        modal.innerHTML = `
            <div class="envelope-scene">
                <div class="envelope-wrapper" id="envelope-wrapper">
                    <div class="envelope-base">
                        <div class="envelope-top-flap" id="envelope-top-flap"></div>
                        <div class="envelope-pocket"></div>
                        
                        <!-- Zarfın İçinden Çıkan Eski Parşömen Görev Kartı -->
                        <div class="envelope-letter-card" id="envelope-letter-card">
                            <div class="envelope-stamp-badge"><i class="fa-solid fa-ribbon"></i> EMNİYET MÜDÜRLÜĞÜ GİZLİ VAKA DOSYASI</div>
                            <h2 class="letter-title">TEBRİKLER DEDEKTİF!</h2>
                            <div class="letter-subtitle">VAKA #104 (GİZEMLİ KASABA) BAŞARIYLA ÇÖZÜLDÜ</div>
                            <div class="letter-divider"></div>
                            <p class="letter-body">
                                Gizemli Kasaba cinayetini ve karanlık sırlarını üstün bir dedektiflik zekasıyla aydınlattınız. Yardımcı Dedektif Çetin ile sergilediğiniz ortaklık Emniyet Genel Merkezi tarafından tescillendi.<br><br>
                                <strong style="color:#d97706; font-size:1.1rem;">📁 YENİ GÖREV DOSYASI #201: GÖLGE ŞEHİR CİNAYETİ</strong><br>
                                Çam ormanlarının çevrelediği tekinsiz Gölge Şehir'de zengin tüccar Ekrem Bey dün gece katledildi. 8 yeni şüpheli ve karanlık sırlar sizi bekliyor.
                            </p>
                            <button id="golge-envelope-accept-btn" class="letter-accept-btn">
                                GÖREVİ KABUL ET VE GÖLGE ŞEHİR'E GİT <i class="fa-solid fa-arrow-right"></i>
                            </button>
                        </div>
                    </div>

                    <!-- Koyu Kırmızı Balmumu Mühür -->
                    <div class="wax-seal" id="envelope-wax-seal" title="Mührü Kır ve Zarfı Aç">
                        <i class="fa-solid fa-stamp"></i>
                        <span>AÇ</span>
                    </div>
                </div>

                <div class="envelope-prompt-text" id="envelope-prompt-text">
                    <i class="fa-solid fa-hand-pointer"></i> Mührün üzerine tıklayarak tebrik ve görev zarfını açın!
                </div>
            </div>
        `;
        document.body.appendChild(modal);

        const seal = document.getElementById('envelope-wax-seal');
        const flap = document.getElementById('envelope-top-flap');
        const card = document.getElementById('envelope-letter-card');
        const prompt = document.getElementById('envelope-prompt-text');

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
        document.getElementById('envelope-wrapper')?.addEventListener('click', (e) => {
            if (!flap?.classList.contains('opened')) {
                openEnvelope();
            }
        });

        document.getElementById('golge-envelope-accept-btn')?.addEventListener('click', (e) => {
            e.stopPropagation();
            modal.classList.add('hidden');
            this.showGolgeSehirStoryIntro();
        });
    },

    showSuccessModalBeforeStory: function () {
        window.currentActiveTown = 'golge_sehir';
        this.currentActiveTown = 'golge_sehir';
        document.body.classList.add('golge-sehir-theme');
        this.registerGolgeSehirData();

        // Kasabaya özel delil ve otopsi durumunu sıfırla / izole et
        if (Array.isArray(window.currentBag)) {
            window.currentBag = window.currentBag.filter(c => c.id >= 1000);
        }
        window.isAutopsyReady = false;
        window.isAutopsyTimerStarted = false;
        if (typeof isAutopsyReady !== 'undefined') isAutopsyReady = false;
        if (typeof isAutopsyTimerStarted !== 'undefined') isAutopsyTimerStarted = false;
        if (typeof autopsyTimer !== 'undefined' && autopsyTimer) {
            clearInterval(autopsyTimer);
            autopsyTimer = null;
        }

        // Her yeni Gölge Şehir açılışında katili anında rastgele sıfırla
        fetch('/api/golge-sehir/reset', { method: 'POST' })
            .then(res => res.json())
            .then(data => {
                if (data && data.guiltyNpcId) {
                    window.guiltyNpcId = data.guiltyNpcId;
                    if (typeof guiltyNpcId !== 'undefined') {
                        guiltyNpcId = data.guiltyNpcId;
                    }
                    console.log("🎲 Gölge Şehir Rastgele Yeni Katil Belirlendi:", data.guiltyNpcId);
                }
            })
            .catch(err => console.error("Gölge Şehir katil sıfırlama hatası:", err));

        let modal = document.getElementById('golge-success-modal');
        if (!modal) {
            this.createEnvelopeSuccessModal();
            modal = document.getElementById('golge-success-modal');
        }

        const seal = document.getElementById('envelope-wax-seal');
        const flap = document.getElementById('envelope-top-flap');
        const card = document.getElementById('envelope-letter-card');
        const prompt = document.getElementById('envelope-prompt-text');

        if (seal) seal.classList.remove('broken');
        if (flap) flap.classList.remove('opened');
        if (card) card.classList.remove('slid-out');
        if (prompt) prompt.style.display = 'block';

        if (modal) {
            modal.classList.remove('hidden');
            setTimeout(() => {
                const s = document.getElementById('envelope-wax-seal');
                if (s && !s.classList.contains('broken')) {
                    s.click();
                }
            }, 1000);
        } else {
            this.showGolgeSehirStoryIntro();
        }
    },

    // 2. GÖLGE ŞEHİR CİNAYET HİKAYESİ (Daktilo Efekti)
    // 2. GÖLGE ŞEHİR DAKTİLO HİKAYE EKRANI (ŞEFFAF LACİVERT & ALTIN/SARI TEMA)
    // 2. GÖLGE ŞEHİR CİNAYET HİKAYESİ (Gizemli Kasaba ile Birebir Uyumlu Daktilo Efekti)
    showGolgeSehirStoryIntro: function () {
        window.currentActiveTown = 'golge_sehir';
        this.currentActiveTown = 'golge_sehir';
        window.hasEnteredGolgeSehir = true;
        document.body.classList.add('golge-sehir-theme');
        this.registerGolgeSehirData();

        // Kasabaya özel çanta ve otopsi izolasyonu
        if (Array.isArray(window.currentBag)) {
            window.currentBag = window.currentBag.filter(c => c.id >= 1000);
            if (typeof currentBag !== 'undefined') currentBag = window.currentBag;
        }
        window.isAutopsyReady = false;
        window.isAutopsyTimerStarted = false;
        if (typeof isAutopsyReady !== 'undefined') isAutopsyReady = false;
        if (typeof isAutopsyTimerStarted !== 'undefined') isAutopsyTimerStarted = false;
        if (typeof autopsyTimer !== 'undefined' && autopsyTimer) {
            clearInterval(autopsyTimer);
            autopsyTimer = null;
        }
        if (typeof window.checkAutopsyConditions === 'function') {
            window.checkAutopsyConditions();
        }

        // Her yeni Gölge Şehir oturumunda 101-108 arası RASTGELE yeni katil belirle
        fetch('/api/golge-sehir/reset', { method: 'POST' })
            .then(res => res.json())
            .then(data => {
                if (data && data.guiltyNpcId) {
                    window.guiltyNpcId = data.guiltyNpcId;
                    if (typeof guiltyNpcId !== 'undefined') {
                        guiltyNpcId = data.guiltyNpcId;
                    }
                    console.log("🎲 Gölge Şehir Rastgele Yeni Katil Belirlendi:", data.guiltyNpcId);
                }
            })
            .catch(err => console.error("Gölge Şehir katil sıfırlama hatası:", err));

        const worldMapScreen = document.getElementById('world-map-screen');
        const storyIntroScreen = document.getElementById('story-intro-screen');
        const storyTextEl = document.getElementById('typewriter-text');
        const storyContinueBtn = document.getElementById('story-continue-btn');
        const skipStoryBtn = document.getElementById('skip-story-btn');
        const cursor = document.querySelector('.story-cursor');
        const storyBadge = document.querySelector('.story-badge');

        if (storyBadge) {
            storyBadge.innerHTML = '<i class="fa-solid fa-scroll"></i> VAKA DOSYASI #201 — GÖLGE ŞEHİR CİNAYETİ';
        }

        if (!storyIntroScreen || !storyTextEl) {
            this.playBekciWalkAnimation();
            return;
        }

        if (worldMapScreen) worldMapScreen.classList.add('hidden');
        storyIntroScreen.classList.remove('hidden');

        storyTextEl.textContent = '';
        if (cursor) cursor.style.display = 'inline-block';
        if (skipStoryBtn) skipStoryBtn.classList.remove('hidden');
        if (storyContinueBtn) storyContinueBtn.classList.add('hidden');

        const fullText = (window.GOLGE_SEHIR_CONFIG && window.GOLGE_SEHIR_CONFIG.storyIntroText) 
            ? window.GOLGE_SEHIR_CONFIG.storyIntroText 
            : "Sisli ve fırtınalı bir gece... Gölge Şehir'in çam ormanı girişinde bir ceset bulundu. Kurban, kasabanın en zengin ve tekinsiz tüccarı Ekrem Bey'di. Islak çam iğnelerinin üzerinde yatan cansız beden, gaz lambalarının altında solgun bir ışıkla aydınlanıyordu. Orman sınırında toplanan kasabalılar, birbirlerine derin bir şüpheyle bakıyordu. Deneyimli dedektif olarak bu karmaşık davayı çözmek için Gölge Şehir'e çağrıldınız. Sekiz şüpheli, sekiz bina, sayısız karanlık sır... Gerçeği ortaya çıkarabilecek misiniz?";

        let charIndex = 0;
        const speed = 35;
        if (window.golgeTypewriterTimer) clearTimeout(window.golgeTypewriterTimer);

        if (typeof window.playLoopSound === 'function' && window.typewriterSound) {
            window.playLoopSound(window.typewriterSound, 0.4);
        }

        const finishGolgeTypewriter = () => {
            if (window.golgeTypewriterTimer) clearTimeout(window.golgeTypewriterTimer);
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
                window.golgeTypewriterTimer = setTimeout(typeChar, speed);
            } else {
                finishGolgeTypewriter();
            }
        };

        typeChar();
    },

    // 3. BEKÇİ RIFAT FENER YÜRÜYÜŞÜ
    playBekciWalkAnimation: function () {
        const worldMapScreen = document.getElementById('world-map-screen');
        if (worldMapScreen) worldMapScreen.classList.add('hidden');

        let cutscene = document.getElementById('golge-bekci-cutscene');
        if (cutscene) cutscene.remove();

        cutscene = document.createElement('div');
        cutscene.id = 'golge-bekci-cutscene';
        cutscene.className = 'bekci-walk-cutscene';
        cutscene.innerHTML = `
            <div class="bekci-walk-bg"></div>
            <div class="bekci-lantern-glow"></div>
            <div id="bekci-mouth-bubble" class="bekci-side-bubble" style="
                position: absolute; top: 45%; right: 6%; left: auto; transform: translateY(-50%);
                background: rgba(15, 23, 42, 0.96); border: 2px solid #f59e0b; border-radius: 18px;
                padding: 24px 28px; color: #fbbf24; font-family: 'Cinzel', serif; font-size: 1.1rem;
                box-shadow: 0 0 45px rgba(245, 158, 11, 0.75), inset 0 0 15px rgba(245, 158, 11, 0.2);
                width: 440px; max-width: 90vw; z-index: 50; display: none; opacity: 0; transition: opacity 0.5s ease;
            ">
                <div style="font-weight: 700; color: #f59e0b; margin-bottom: 12px; font-size: 1.25rem; display:flex; align-items:center; gap:10px;">
                    <i class="fa-solid fa-person-military-pointing"></i> BEKÇİ RIFAT:
                </div>
                <div id="bekci-mouth-text" style="line-height: 1.6; color:#fef3c7; font-size:1.05rem;">
                    "Durun bakalım! Fenerimi gözünüze tutturmayın! Ha... Demek merkezden beklenen dedektif heyeti sizsiniz. 40 yıldır bu sokaklardayım, tüccar Ekrem'in öldüğü gece gibi uğursuz bir gece görmedim!"
                </div>
                <div style="text-align: right; margin-top: 20px;">
                    <button id="bekci-pass-to-town-btn" style="
                        background: linear-gradient(135deg, #d97706, #b45309); color: #fff;
                        border: 1px solid #f59e0b; padding: 12px 26px; border-radius: 8px;
                        font-weight: 700; font-family: 'Cinzel', serif; cursor: pointer; font-size: 1.05rem;
                        box-shadow: 0 0 20px rgba(245, 158, 11, 0.6); transition: transform 0.2s;
                    ">
                        KASABAYA GEÇ <i class="fa-solid fa-arrow-right"></i>
                    </button>
                </div>
            </div>
        `;
        document.body.appendChild(cutscene);
        cutscene.style.display = 'flex';

        if (typeof window.playSound === 'function' && window.chatterSound) {
            window.playSound(window.chatterSound, 0.3);
        }

        setTimeout(() => {
            const bubble = document.getElementById('bekci-mouth-bubble');
            if (bubble) {
                bubble.style.display = 'block';
                setTimeout(() => bubble.style.opacity = '1', 50);

                const passBtn = document.getElementById('bekci-pass-to-town-btn');
                if (passBtn) {
                    passBtn.onclick = () => {
                        cutscene.style.transition = 'opacity 0.5s ease';
                        cutscene.style.opacity = '0';
                        setTimeout(() => {
                            cutscene.remove();
                            this.loadGolgeSehirMap();
                        }, 500);
                    };
                }
            }
        }, 1800);
    },

    setupMapObserver: function () {
        const townMapScreen = document.getElementById('town-map-screen');
        if (!townMapScreen) return;

        const observer = new MutationObserver(() => {
            const isVisible = !townMapScreen.classList.contains('hidden');
            if (isVisible) {
                if (window.currentActiveTown === 'golge_sehir') {
                    document.body.classList.add('golge-sehir-theme');
                    if (!townMapScreen.classList.contains('golge-sehir-active')) {
                        this.applyGolgeSehirState();
                    }
                    this.updateGolgeSehirAutopsyUI();
                } else if (window.currentActiveTown === 'gizemli') {
                    document.body.classList.remove('golge-sehir-theme');
                    if (townMapScreen.classList.contains('golge-sehir-active')) {
                        this.clearGolgeSehirMap();
                    }
                }
            }
        });

        observer.observe(townMapScreen, { attributes: true, attributeFilter: ['class'] });
    },

    loadGolgeSehirMap: function () {
        window.currentActiveTown = 'golge_sehir';
        this.currentActiveTown = 'golge_sehir';
        document.body.classList.add('golge-sehir-theme');
        this.registerGolgeSehirData();

        // Kasabaya özel çanta ve otopsi durumunu izole et
        if (Array.isArray(window.currentBag)) {
            window.currentBag = window.currentBag.filter(c => c.id >= 1000);
            if (typeof currentBag !== 'undefined') currentBag = window.currentBag;
        }

        const townMapStage = document.getElementById('town-map-stage');
        const townMapScreen = document.getElementById('town-map-screen');
        const worldMapScreen = document.getElementById('world-map-screen');

        if (!townMapStage || !townMapScreen) return;

        if (typeof window.triggerTransition === 'function') {
            window.triggerTransition(() => {
                if (worldMapScreen) worldMapScreen.classList.add('hidden');
                townMapScreen.classList.remove('hidden');
                this.applyGolgeSehirState();
                if (typeof window.checkAutopsyConditions === 'function') {
                    window.checkAutopsyConditions();
                }
            });
        } else {
            if (worldMapScreen) worldMapScreen.classList.add('hidden');
            townMapScreen.classList.remove('hidden');
            this.applyGolgeSehirState();
            if (typeof window.checkAutopsyConditions === 'function') {
                window.checkAutopsyConditions();
            }
        }
    },

    applyGolgeSehirState: function () {
        this.registerGolgeSehirData();

        // Yağmur ve arka plan müziğinin devam ettiğinden emin ol
        const isGameMuted = (typeof window.isMuted !== 'undefined') ? window.isMuted : (localStorage.getItem('gameMuted') === 'true');
        if (!isGameMuted) {
            const bgMusic = document.getElementById('bg-music');
            const rainSound = document.getElementById('rain-sound');
            if (bgMusic && typeof window.playLoopSound === 'function') window.playLoopSound(bgMusic, 0.3);
            if (rainSound && typeof window.playLoopSound === 'function') window.playLoopSound(rainSound, 0.5);
        }

        const townMapScreen = document.getElementById('town-map-screen');
        const townMapStage = document.getElementById('town-map-stage');
        if (!townMapScreen || !townMapStage) return;

        if (!townMapScreen.classList.contains('golge-sehir-active')) {
            townMapScreen.classList.add('golge-sehir-active');
        }
        if (!townMapStage.classList.contains('golge-sehir-active')) {
            townMapStage.classList.add('golge-sehir-active');
        }

        const bekciBtn = document.getElementById('bekci-quick-tip-btn');
        if (bekciBtn) {
            bekciBtn.style.display = 'block';
        }

        const gizemliBuildings = townMapStage.querySelectorAll('.map-building:not([class*="building-golge-"])');
        gizemliBuildings.forEach(el => el.style.display = 'none');

        // Gölge Şehir 8 binası
        if (townMapStage.querySelectorAll('[class*="building-golge-"]').length === 0) {
            window.GOLGE_SEHIR_CONFIG.buildings.forEach(bld => {
                const buildingDiv = document.createElement('div');
                buildingDiv.className = `map-building building-golge-${bld.id}`;
                buildingDiv.setAttribute('data-npc-id', bld.npcId);
                buildingDiv.setAttribute('data-building-id', bld.id);
                buildingDiv.title = bld.title;

                Object.assign(buildingDiv.style, bld.style);

                const hoverTag = document.createElement('div');
                hoverTag.className = 'building-hover-tag';
                hoverTag.innerHTML = `<i class="${bld.icon}"></i> ${bld.hoverTag}`;
                buildingDiv.appendChild(hoverTag);

                const isVisited = this.visitedGolgeBuildings.has(bld.npcId) || (window.visitedBuildings && window.visitedBuildings.has(bld.npcId));
                if (isVisited) {
                    buildingDiv.classList.add('visited');
                    hoverTag.innerHTML = `<i class="fa-solid fa-lock"></i> İNCELEME TAMAMLANDI`;
                    buildingDiv.classList.add(`building-golge-${bld.id}`, 'building-golge-admin');
                }
                buildingDiv.setAttribute('data-npc-id', bld.npcId);

                buildingDiv.onclick = (e) => {
                    e.stopPropagation();
                    if (this.adminMode) {
                        console.log(`Admin Mode Tıklandı: ${bld.title} (ID: ${bld.npcId})`);
                        return;
                    }
                    if (this.visitedGolgeBuildings.has(bld.npcId) || (window.visitedBuildings && window.visitedBuildings.has(bld.npcId)) || buildingDiv.classList.contains('visited')) return;
                    if (!this.introDialogCompleted) {
                        if (typeof window.showCinematicHelper === 'function') {
                            window.showCinematicHelper("Önce Bekçi Rıfat amcayı ve kasaba bilgilendirmesini dinleyelim amirim!", false);
                        }
                        return;
                    }

                    // Kasabalı Evi (Fehmi Bey - ID 107) Özel 3 Vuruşlu Kapı Animasyonu ve İki Aşamalı Onay Akışı
                    if (bld.npcId === 107) {
                        this.triggerFehmiBeyDoorSequence(bld);
                        return;
                    }

                    if (typeof window.openDoorTransitionModal === 'function') {
                        this.registerGolgeSehirData();
                        window.openDoorTransitionModal(bld.npcId);
                    } else {
                        this.onBuildingClick(bld);
                    }
                };

                townMapStage.appendChild(buildingDiv);
            });
        } else {
            // Var olan Gölge binalarının kilit / ziyaret durumunu güncelle
            townMapStage.querySelectorAll('[class*="building-golge-"]').forEach(buildingDiv => {
                const npcId = parseInt(buildingDiv.getAttribute('data-npc-id'), 10);
                const isVisited = this.visitedGolgeBuildings.has(npcId) || (window.visitedBuildings && window.visitedBuildings.has(npcId));
                const hoverTag = buildingDiv.querySelector('.building-hover-tag');
                
                if (isVisited && !buildingDiv.classList.contains('visited')) {
                    buildingDiv.classList.add('visited');
                    if (hoverTag) {
                        hoverTag.innerHTML = `<i class="fa-solid fa-lock"></i> İNCELEME TAMAMLANDI`;
                    }
                }
                
                // Admin mod kontrolü veya visited kontrolü
                buildingDiv.onclick = (e) => {
                    e.stopPropagation();
                    if (this.adminMode) {
                        console.log(`Admin Mode Tıklandı: ID ${npcId}`);
                        return;
                    }
                    if (this.visitedGolgeBuildings.has(npcId) || (window.visitedBuildings && window.visitedBuildings.has(npcId)) || buildingDiv.classList.contains('visited')) return;
                    if (!this.introDialogCompleted) {
                        if (typeof window.showCinematicHelper === 'function') {
                            window.showCinematicHelper("Önce Bekçi Rıfat amcayı ve kasaba bilgilendirmesini dinleyelim amirim!", false);
                        }
                        return;
                    }
                    const bld = window.GOLGE_SEHIR_CONFIG.buildings.find(b => b.npcId === npcId);
                    if (bld) {
                        if (bld.npcId === 107) {
                            this.triggerFehmiBeyDoorSequence(bld);
                            return;
                        }
                        if (typeof window.openDoorTransitionModal === 'function') {
                            this.registerGolgeSehirData();
                            window.openDoorTransitionModal(bld.npcId);
                        } else {
                            this.onBuildingClick(bld);
                        }
                    }
                };
            });
        }


        this.updateGolgeSehirAutopsyUI();

        if (!this.hasShownDualIntro) {
            this.hasShownDualIntro = true;
            this.dualDialogStep = 0;
            setTimeout(() => this.playDualAssistantBubbleDialogue(), 500);
        }
    },

    updateGolgeSehirAutopsyUI: function () {
        const isGolge = (window.currentActiveTown === 'golge_sehir');
        if (!isGolge) return;

        // Count unique visited buildings >= 101
        let uniqueGolgeVisited = new Set();
        if (this.visitedGolgeBuildings) {
            this.visitedGolgeBuildings.forEach(id => uniqueGolgeVisited.add(id));
        }
        if (window.visitedBuildings) {
            window.visitedBuildings.forEach(id => {
                if (id >= 101) uniqueGolgeVisited.add(id);
            });
        }
        let golgeVisitedCount = uniqueGolgeVisited.size;
        const labCount = window.submittedForensicCountGolge || 0;
        const container = document.getElementById('autopsy-timer-container');
        const badgeText = document.getElementById('forensic-badge-text');

        if (badgeText) {
            if (labCount === 0) {
                badgeText.textContent = '0/4 LAB GEREKLİ';
            } else if (labCount < 4) {
                badgeText.textContent = `${labCount}/4 LAB GÖNDERİLDİ`;
            } else {
                badgeText.textContent = `✓ ${labCount} LAB GÖNDERİLDİ`;
            }
        }

        if (container && !window.isAutopsyTimerStarted && !window.isAutopsyReady) {
            container.innerHTML = `<i class="fa-solid fa-clock-rotate-left"></i> OTOPSİ: BİNA ${golgeVisitedCount}/4 | LAB ${labCount}/4`;
        }
    },

    // 4. ÇİFT YARDIMCI DİNAMİK KONUŞMA BALONU
    playDualAssistantBubbleDialogue: function () {
        const dialogs = window.GOLGE_SEHIR_CONFIG.assistants.introDialogue;

        const updateBubbleUI = (index) => {
            if (index < 0 || index >= dialogs.length) return;

            const current = dialogs[index];
            const isCetin = (current.speaker === 'Çetin');

            const speakerInfo = {
                speaker: isCetin ? 'cetin' : 'rifat',
                speakerName: isCetin ? 'YARDIMCI DEDEKTİF ÇETİN' : 'BEKÇİ RIFAT (GECE BEKÇİSİ)',
                avatar: isCetin ? 'images/dedektif_helper.png' : 'images/towns/golge_sehir/npcler/bekci_hasan_helper.png',
                theme: isCetin ? 'cetin-speaking' : 'rifat-speaking'
            };

            const box = document.getElementById('cinematic-helper-box');
            const avatarImg = document.querySelector('.cinematic-helper-avatar img');
            const nameEl = document.querySelector('.cinematic-helper-name');

            if (box) {
                box.classList.remove('rifat-speaking', 'cetin-speaking');
                box.classList.add(speakerInfo.theme);
            }
            if (avatarImg) avatarImg.src = speakerInfo.avatar;
            if (nameEl) nameEl.textContent = speakerInfo.speakerName;

            if (typeof window.showCinematicHelper === 'function') {
                window.showCinematicHelper(current.text, false, `golge_intro_${index}`, false, speakerInfo);
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
                box.classList.remove('rifat-speaking');
                box.classList.add('cetin-speaking');
            }
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

        const skipBtn = document.getElementById('cinematic-skip-btn');
        if (skipBtn) {
            skipBtn.onclick = (e) => {
                e.stopPropagation();
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
            };
        }

        const prevBtn = document.getElementById('cinematic-prev-btn');
        if (prevBtn) {
            prevBtn.onclick = (e) => {
                e.stopPropagation();
                if (this.dualDialogStep > 1) {
                    this.dualDialogStep -= 2;
                    showNext();
                }
            };
        }
    },

    // 5. BEKÇİ RIFAT AYAKTA DURAN YARDIMCI WIDGET'I (ÇETİN GİBİ ŞEFFAF & BÜYÜK GÖRÜNÜM)
    createBekciHelperWidget: function () {
        let widget = document.getElementById('bekci-quick-tip-btn');
        if (widget) widget.remove();

        widget = document.createElement('div');
        widget.id = 'bekci-quick-tip-btn';
        widget.className = 'helper-detective-widget golge-bekci-widget';
        widget.title = 'Bekçi Rıfat\'tan İpucu Al';
        widget.innerHTML = `
            <img src="images/towns/golge_sehir/npcler/bekci_hasan_helper.png" class="helper-detective-img" alt="Bekçi Rıfat">
            <div class="helper-detective-badge">BEKÇİ RIFAT</div>
        `;

        widget.addEventListener('click', (e) => {
            e.stopPropagation();
            this.triggerBekciTip();
        });

        document.body.appendChild(widget);
    },

    triggerBekciTip: function () {
        if (window.currentActiveTown !== 'golge_sehir') return;

        const currentNpc = window.activeNpcId;
        const box = document.getElementById('cinematic-helper-box');
        const avatarImg = document.querySelector('.cinematic-helper-avatar img');
        const nameEl = document.querySelector('.cinematic-helper-name');

        if (box && avatarImg && nameEl) {
            box.classList.remove('cetin-speaking');
            box.classList.add('rifat-speaking');
            avatarImg.src = 'images/towns/golge_sehir/npcler/bekci_hasan_helper.png';
            nameEl.textContent = 'BEKÇİ RIFAT (GECE BEKÇİSİ)';
        }

        const tips = {
            101: "Tahsin ile Muhtar Cevdet orman arazisi yüzünden Ekrem'e diş biliyordu. Baltasındaki reçineyi ve kan izlerini adli tıbba gönderin!",
            102: "Manav Ayşe'nin dükkânı Ekrem'e ipotekliydi. Ayşe, Bakkal Naciye'ye dert yanıp 'Ekrem dükkânımı alırsa yaşatmam' demişti! Cinayet gecesi peleriniyle kaçtığını gördüm!",
            103: "Demirci Kazım usta sessizdir ama Ekrem için özel şifreli bir çelik kasa kilidi yapmıştı. O kasanın içinde Muhtar ile Hekim'in evrakları vardı! Ocağın arkasını arayın!",
            104: "Bakkal Naciye kasabanın tüm dedikodularını bilir. Ekrem'in veresiye defterindeki sayfasını yırtmış! Ayrıca Hekim Sevgi'ye ilaç şişeleri vermişti!",
            105: "Hekim Sevgi şifalı otlar hazırlar ama Ekrem onun şantaj yapıyordu. Muayenehanedeki koyu mor şişe banotu zehri içeriyor, Ekrem'in tırnaklarındaki morluklara bakın!",
            106: "Muhtar Cevdet kasabayı parmağında oynatır. Sahte çam tapusu çıkarmıştı. Çekmecesindeki mühürleri kontrol edin!",
            107: "Fehmi Bey emekli muallimdir, Ekrem onun köstekli saatini çalmıştı. Cinayet gecesi saat 02:14'te Ekrem'in penceresinin altında kavga sesleri duymuş!",
            108: "Kunduracı Rasim kaçak deri ticaretinde Ekrem'e borçluydu. Cinayet gecesi göl kenarından gelen 42 numara çamurlu çizme izleri atölyesine uzanıyor!"
        };

        const msg = (currentNpc && tips[currentNpc])
            ? tips[currentNpc]
            : "Bu kasabada herkes birbirinin kuyusunu kazar! 8 şüphelinin her birinin maktul Ekrem Bey ile karanlık bir hesabı var. Bana danışmaktan çekinmeyin amirim!";

        if (typeof window.showCinematicHelper === 'function') {
            window.showCinematicHelper(msg, false, 'bekci_interactive_tip', false, {
                speaker: 'rifat',
                speakerName: 'BEKÇİ RIFAT (GECE BEKÇİSİ)',
                avatar: 'images/towns/golge_sehir/npcler/bekci_hasan_helper.png',
                theme: 'rifat-speaking'
            });
        }
    },

    clearGolgeSehirMap: function () {
        document.body.classList.remove('golge-sehir-theme');
        const bekciBtn = document.getElementById('bekci-quick-tip-btn');
        if (bekciBtn) {
            bekciBtn.style.display = 'none';
        }

        const townMapScreen = document.getElementById('town-map-screen');
        const townMapStage = document.getElementById('town-map-stage');
        
        if (townMapScreen) {
            townMapScreen.classList.remove('golge-sehir-active');
            townMapScreen.style.removeProperty('background-image');
        }
        if (townMapStage) {
            townMapStage.classList.remove('golge-sehir-active');
            townMapStage.style.removeProperty('background-image');
        }

        if (townMapStage) {
            const gizemliBuildings = townMapStage.querySelectorAll('.map-building:not([class*="building-golge-"])');
            gizemliBuildings.forEach(el => el.style.display = '');

            const golgeBuildings = townMapStage.querySelectorAll('[class*="building-golge-"]');
            golgeBuildings.forEach(el => el.remove());
        }
    },

    toggleAdminMode: function() {
        if (this.adminMode === undefined) this.adminMode = false;
        this.adminMode = !this.adminMode;
        console.log("Admin Mode: " + (this.adminMode ? "ON" : "OFF"));
        
        const golgeBuildings = document.querySelectorAll('.building-golge-admin');
        if (this.adminMode) {
            golgeBuildings.forEach(el => {
                el.style.border = '2px dashed red';
                el.style.backgroundColor = 'rgba(255,0,0,0.3)';
                el.draggable = true;
                
                el.ondragstart = (e) => {
                    const rect = el.getBoundingClientRect();
                    e.dataTransfer.setData('text/plain', el.getAttribute('data-npc-id'));
                    el.dataset.offsetX = e.clientX - rect.left;
                    el.dataset.offsetY = e.clientY - rect.top;
                };
            });
            
            const townMapStage = document.getElementById('town-map-stage');
            if (townMapStage) {
                townMapStage.ondragover = (e) => e.preventDefault();
                townMapStage.ondrop = (e) => {
                    e.preventDefault();
                    const npcId = e.dataTransfer.getData('text/plain');
                    const el = document.querySelector(`.building-golge-admin[data-npc-id="${npcId}"]`);
                    if (el) {
                        const rect = townMapStage.getBoundingClientRect();
                        const offsetX = parseFloat(el.dataset.offsetX || 0);
                        const offsetY = parseFloat(el.dataset.offsetY || 0);
                        
                        const leftPct = ((e.clientX - rect.left - offsetX) / rect.width) * 100;
                        const topPct = ((e.clientY - rect.top - offsetY) / rect.height) * 100;
                        
                        el.style.left = leftPct.toFixed(2) + '%';
                        el.style.top = topPct.toFixed(2) + '%';
                        
                        console.log(`Bina ID ${npcId} Yeni Koordinatları: left: '${leftPct.toFixed(2)}%', top: '${topPct.toFixed(2)}%'`);
                    }
                };
            }
        } else {
            golgeBuildings.forEach(el => {
                el.style.border = '';
                el.style.backgroundColor = '';
                el.draggable = false;
                el.ondragstart = null;
            });
            const townMapStage = document.getElementById('town-map-stage');
            if (townMapStage) {
                townMapStage.ondragover = null;
                townMapStage.ondrop = null;
            }
        }
    },

    // 5.5 KASABALI EVİ (FEHMİ BEY - ID 107) ÖZEL KAPI VİDEOSU VE İKİ AŞAMALI ONAY
    triggerFehmiBeyDoorSequence: function (bld) {
        if (typeof window.showGameMessageBox !== 'function') {
            this.onBuildingClick(bld);
            return;
        }

        window.showGameMessageBox({
            title: "KASABALI EVİNE GİRİŞ",
            message: "Emekli muallim Fehmi Bey yabancılardan pek hoşlanmaz. Kapıyı tıklatıp şansınızı denemek istediğinize emin misiniz?",
            showCancel: true,
            confirmText: "Kapıyı Çal",
            cancelText: "Vazgeç",
            onConfirm: () => {
                const animModal = document.getElementById('fehmi-door-animation-modal');
                const doorVideo = document.getElementById('fehmi-door-video');

                if (!animModal || !doorVideo) {
                    this.onBuildingClick(bld);
                    return;
                }

                animModal.classList.remove('hidden');
                doorVideo.currentTime = 0;
                doorVideo.muted = true; // Video içindeki tüm konuşma/bağırma sesini tamamen sustur

                const isGameMuted = (typeof window.isMuted !== 'undefined') ? window.isMuted : (localStorage.getItem('gameMuted') === 'true');

                // 1. AŞAMA: Kapı Tıklatma / Vurma Sesi (0. sn)
                if (!isGameMuted) {
                    const doorKnock = document.getElementById('door-knock');
                    if (doorKnock && typeof window.playSound === 'function') {
                        window.playSound(doorKnock, 0.7);
                    }
                }

                // 2. AŞAMA: Kapı Açılma Gıcırtısı (0.6 sn sonra kapı aralanırken)
                setTimeout(() => {
                    const isMutedCheck = (typeof window.isMuted !== 'undefined') ? window.isMuted : (localStorage.getItem('gameMuted') === 'true');
                    if (!isMutedCheck) {
                        const doorCreak = document.getElementById('door-creak');
                        if (doorCreak && typeof window.playSound === 'function') {
                            window.playSound(doorCreak, 0.7);
                        }
                    }
                }, 600);

                const proceedToWarning = () => {
                    animModal.classList.add('hidden');
                    doorVideo.pause();

                    // 3. AŞAMA: Kapı Yüze Çarpılma / Kapanma Sesi
                    const isMutedNow = (typeof window.isMuted !== 'undefined') ? window.isMuted : (localStorage.getItem('gameMuted') === 'true');
                    if (!isMutedNow) {
                        const doorClose = document.getElementById('door-close');
                        if (doorClose && typeof window.playSound === 'function') {
                            window.playSound(doorClose, 0.8);
                        }
                    }

                    window.showGameMessageBox({
                        title: "KAPI YÜZÜNÜZE KAPANDI!",
                        message: "Fehmi Bey öfkeyle kapıyı yüzünüze kapattı! Yine de içeri zorla girmek ve evi aramak istiyor musunuz?",
                        showCancel: true,
                        confirmText: "Evet, Zorla Gir",
                        cancelText: "Haritada Kal",
                        onConfirm: () => {
                            this.onBuildingClick(bld);
                        },
                        onCancel: () => {
                            console.log("Fehmi Bey evinden vazgeçildi.");
                        }
                    });
                };

                doorVideo.onended = () => {
                    proceedToWarning();
                };

                const playPromise = doorVideo.play();
                if (playPromise !== undefined) {
                    playPromise.catch(e => {
                        console.warn("Video otomatik oynatılamadı, tıklandığında geçiliyor:", e);
                        doorVideo.onclick = () => doorVideo.play();
                    });
                }
            }
        });
    },

    onBuildingClick: function (bld) {
        console.log(`🚪 Gölge Şehir İç Mekânı Açılıyor: ${bld.title}`);
        this.registerGolgeSehirData();
        window.activeNpcId = bld.npcId;

        const townMapScreen = document.getElementById('town-map-screen');
        const intScreen = document.getElementById('interior-screen');
        const stageCanvas = document.getElementById('interior-stage-canvas');

        if (townMapScreen) townMapScreen.classList.add('hidden');
        if (intScreen) intScreen.classList.remove('hidden');

        const talkNameEl = document.getElementById('talk-npc-name');
        const talkBtn = document.getElementById('talk-npc-btn');
        const isNpcInnocent = (window.innocentNpcIds && window.innocentNpcIds.has(bld.npcId)) || (typeof innocentNpcIds !== 'undefined' && innocentNpcIds.has(bld.npcId));

        if (talkNameEl) {
            talkNameEl.innerText = isNpcInnocent ? `${bld.npc.name} (Masum İlan Edildi)` : `${bld.npc.name} ile Konuş`;
        }
        if (talkBtn) {
            if (isNpcInnocent) {
                talkBtn.style.opacity = '0.5';
                talkBtn.style.cursor = 'not-allowed';
                talkBtn.title = 'Bu karakter masum ilan edildiği için sorgulanamaz.';
            } else {
                talkBtn.style.opacity = '1';
                talkBtn.style.cursor = 'pointer';
                talkBtn.title = '';
            }
        }

        if (typeof window.playSound === 'function' && window.doorCreak) {
            window.playSound(window.doorCreak, 0.7);
        }

        const imgUrl = `url('${bld.interiorImg}?v=${Date.now()}')`;

        if (intScreen) {
            intScreen.setAttribute('data-npc-id', bld.npcId);
            intScreen.style.setProperty('background-image', imgUrl, 'important');
            intScreen.style.setProperty('background-size', 'cover', 'important');
            intScreen.style.setProperty('background-position', 'center center', 'important');
            intScreen.style.setProperty('background-repeat', 'no-repeat', 'important');
        }

        if (stageCanvas) {
            stageCanvas.style.setProperty('background-image', 'none', 'important');
            stageCanvas.style.setProperty('background-color', 'transparent', 'important');
        }

        const container = document.getElementById('hotspots-container');
        if (container) container.innerHTML = '';

        this.renderBuildingClueHotspots(bld);
        this.playBuildingBanter(bld.npcId);
    },

    currentBanterTimer: null,
    currentBanterStep: 0,
    currentBanterList: [],

    playBuildingBanter: function (npcId) {
        if (typeof window.showCinematicHelper !== 'function') return;

        const banters = {
            101: [
                { speaker: 'rifat', text: 'Oduncu Tahsin bu kasabanın en gürültücü adamıdır! Gece gündüz demez ağaç keser. Geçen yıl orman sınırını ihlal edip Ekrem’le davalık olmuşlardı.' },
                { speaker: 'cetin', text: 'Dedikoduları bırakalım Bekçi amca. Bizim işimiz baltasındaki kan ve reçine izlerini laboratuvara göndermek. Somut delil lazım.' },
                { speaker: 'rifat', text: 'Sen ne anlarsın be çömez! İnsanların geçmişini bilmeden kanın kimden aktığını bulamazsın. Neyse, siz işinize bakın amirim.' }
            ],
            102: [
                { speaker: 'rifat', text: 'Manav Ayşe her daim güleryüzlüdür ama borcu boyunu aşmıştı. Ekrem onun dükkânına haciz getirecekti, zavallı kadın iki aydır uyku uyumuyordu.' },
                { speaker: 'cetin', text: 'Bu onun katil olduğunu göstermez Bekçi amca. Bize kırık kasadaki izler ve zehirli elma lazım, varsayımlar değil.' },
                { speaker: 'rifat', text: 'Varsayım değil gerçek bunlar genç! Ama belli ki senin kitabi bilgin benim 40 yıllık tecrübemle yarışacak. Hadi bakalım amirim, delilleri toplayın.' }
            ],
            103: [
                { speaker: 'rifat', text: 'Demirci Kazım... Çeliği döver gibi insanı da döver bu adam. Ekrem’le hep gizli gizli fısıldaşırlardı. Aralarında karanlık bir anlaşma var.' },
                { speaker: 'cetin', text: 'Odak noktamız çelik kasa ve demir tozu olmalı. Özel yapım o kilitte parmak izi bulabiliriz.' },
                { speaker: 'rifat', text: 'Parmak iziymiş! Adamın bakışlarındaki ateşi görmüyor musun? Neyse amirim, Çetin dedektifimiz laboratuvarda oyalana dursun, siz ipuçlarına bakın.' }
            ],
            104: [
                { speaker: 'rifat', text: 'Bakkal Naciye kasabanın ayaklı gazetesidir! Ama Ekrem söz konusu olunca ağzını bıçak açmazdı. Ekrem’in ona yüklü miktarda borç taktığı söylenir.' },
                { speaker: 'cetin', text: 'Dedikodular mahkemede işe yaramaz. O veresiye defterindeki yırtık sayfalar asıl kanıtımız olacak.' },
                { speaker: 'rifat', text: 'Mahkeme sizin olsun, kasabanın vicdanı benim! Sen defter sayfalarını birleştirirken ben de insanları okurum. Kolay gelsin amirim.' }
            ],
            105: [
                { speaker: 'rifat', text: 'Hekim Sevgi hanım... Otlarla mucizeler yaratır ama Ekrem Bey onun geçmişindeki karanlık bir sırrı biliyordu. Şantaja uğruyordu zavallı.' },
                { speaker: 'cetin', text: 'Şantaj büyük bir motiftir ama banotu zehri daha somut bir delil. Laboratuvar analizine odaklanmalıyız Bekçi amca.' },
                { speaker: 'rifat', text: 'Zehir şişeden değil, insanın içinden akar evlat! Sen şişeleri topla, amirimle biz de gerçekleri araştıralım.' }
            ],
            106: [
                { speaker: 'rifat', text: 'Muhtar Cevdet! Kasabanın sözde babası. Ekrem ile birlikte orman arazisine çöktüklerini sağır sultan bile duydu. Rüşvet, sahtecilik ne ararsan var.' },
                { speaker: 'cetin', text: 'Rüşveti kanıtlamak için sahte tapuları ve o mührü bulmamız gerek. Kişisel yargılara yer yok.' },
                { speaker: 'rifat', text: 'Kişisel yargı değil, gözümle gördüm diyorum! Sen mührü incele, ben de amirime yolu göstereyim.' }
            ],
            107: [
                { speaker: 'rifat', text: 'Fehmi Muallim yıllarca eğitime ömrünü verdi. Ama Ekrem, adamın dedesinden kalma köstekli saatini zorla elinden almıştı. Fehmi çok içerledi bu duruma.' },
                { speaker: 'cetin', text: 'Köstekli saat önemli bir ipucu. Cinayet saatinde duyduğu kavga seslerinin gerçekliğini sorgulamamız gerekiyor.' },
                { speaker: 'rifat', text: 'Adam onurundan oldu, sen hala saat diyorsun! Haklısın amirim, şu genç dedektife uymayın siz, etrafı iyice inceleyin.' }
            ],
            108: [
                { speaker: 'rifat', text: 'Kunduracı Rasim hep kaçak işler çevirir. Ekrem ona ucuz deri sağlardı ama Rasim parayı denkleştiremedi. İkisi fena kapışmıştı geçen hafta.' },
                { speaker: 'cetin', text: 'Kaçak deri ticareti ve 42 numara çamurlu çizme izleri. Aradığımız bağlantı bu dükkanda olabilir, dedikodularda değil.' },
                { speaker: 'rifat', text: 'Çamuru sadece çizmelerde mi sanırsın çömez? Bu kasabanın her yeri çamura batmış! Hadi amirim, siz delilleri çantaya atın da bitsin bu iş.' }
            ]
        };

        const banter = banters[npcId];
        if (!banter) return;

        if (this.currentBanterTimer) clearTimeout(this.currentBanterTimer);
        this.currentBanterList = banter;
        this.currentBanterStep = 0;

        const showBanterLine = (step) => {
            if (step < 0 || step >= banter.length) return;
            this.currentBanterStep = step;
            const b = banter[step];
            const isCetin = b.speaker === 'cetin';

            const prevBtn = document.getElementById('cinematic-prev-btn');
            if (prevBtn) {
                prevBtn.style.display = (step > 0) ? 'inline-block' : 'none';
            }

            window.showCinematicHelper(b.text, false, '', true, {
                speaker: b.speaker,
                speakerName: isCetin ? 'YARDIMCI DEDEKTİF ÇETİN' : 'BEKÇİ RIFAT (GECE BEKÇİSİ)',
                avatar: isCetin ? 'images/dedektif_helper.png' : 'images/towns/golge_sehir/npcler/bekci_hasan_helper.png',
                theme: isCetin ? 'cetin-speaking' : 'rifat-speaking'
            });

            if (step < banter.length - 1) {
                this.currentBanterTimer = setTimeout(() => {
                    showBanterLine(step + 1);
                }, 6000);
            }
        };

        // Bağlanabilir Skip & Prev kontrolleri
        const skipBtn = document.getElementById('cinematic-skip-btn');
        if (skipBtn) {
            skipBtn.onclick = (e) => {
                e.stopPropagation();
                if (window.isHelperTyping) {
                    window.isHelperTyping = false;
                    if (window.cinematicTypewriterTimeout) clearTimeout(window.cinematicTypewriterTimeout);
                    const textEl = document.getElementById('cinematic-helper-text');
                    if (textEl && window.currentHelperMessageText) {
                        textEl.textContent = window.currentHelperMessageText;
                        textEl.classList.add('typing-done');
                    }
                } else if (this.currentBanterStep < this.currentBanterList.length - 1) {
                    if (this.currentBanterTimer) clearTimeout(this.currentBanterTimer);
                    showBanterLine(this.currentBanterStep + 1);
                }
            };
        }

        const prevBtn = document.getElementById('cinematic-prev-btn');
        if (prevBtn) {
            prevBtn.onclick = (e) => {
                e.stopPropagation();
                if (this.currentBanterStep > 0) {
                    if (this.currentBanterTimer) clearTimeout(this.currentBanterTimer);
                    showBanterLine(this.currentBanterStep - 1);
                }
            };
        }

        setTimeout(() => showBanterLine(0), 800);
    },

    // 6. GİZLİ DELİL HOTSPOTLARI (VARSAYILAN OLARAK GİZLİ, HOVER'DA SARI NEON VE ETİKET)
    renderBuildingClueHotspots: function (bld) {
        const stageCanvas = document.getElementById('interior-stage-canvas');
        if (!stageCanvas) return;

        stageCanvas.querySelectorAll('.golge-clue-hotspot').forEach(el => el.remove());
        const container = document.getElementById('hotspots-container');
        if (container) container.innerHTML = '';

        bld.hotspots.forEach((clue) => {
            const spot = document.createElement('div');
            spot.className = 'golge-clue-hotspot clue-hotspot'; // app.js uyumluluğu için clue-hotspot da eklendi
            spot.title = clue.name;
            spot.style.top = clue.top;
            spot.style.left = clue.left;
            spot.style.width = '70px';
            spot.style.height = '70px';
            spot.style.position = 'absolute';
            spot.style.cursor = 'pointer';
            spot.style.zIndex = '9999'; // Tıklanabilirliği artırmak için yüksek z-index

            const img = document.createElement('img');
            img.src = clue.img;
            img.alt = clue.name;
            img.style.width = '100%';
            img.style.height = '100%';
            img.style.objectFit = 'contain';

            const label = document.createElement('span');
            label.className = 'golge-clue-label';
            label.textContent = clue.name;

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
    }
};

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => window.GolgeSehirEngine.init());
} else {
    window.GolgeSehirEngine.init();
}
