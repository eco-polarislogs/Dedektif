/**
 * SİSÖREN MOTORU (v3.0)
 * Dağ Kasabası Sisören — Tam Kasaba Motoru
 * Gölge Şehir mimarisiyle uyumlu: NPC Data, Bina Giriş, Otopsi, Buldum Entegrasyonu
 */

window.SisorenEngine = {
    currentActiveTown: 'gizemli',
    hasShownSisorenIntro: false,
    introDialogCompleted: false,
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
                a: knownAnswer || `${name} bir an duraksıyor: "${question}" sorusunun cevabını olay gecesindeki ayrıntıları hatırlayarak anlatıyor. Bu konuda bildiğim tek şey, izlerin bina çevresinden dağ yoluna doğru devam ettiğidir.`,
                difficulty: index > 2 ? 2 : 1,
                category: index > 2 ? 'yuzlestirme' : 'tanisma'
            };
        });
        while (normalized.length < 4) {
            const index = normalized.length;
            normalized.push({
                q: `${name}, olay gecesiyle ilgili başka hangi ayrıntıyı hatırlıyorsun?`,
                a: `${name} başını sallıyor: "Bunu daha önce anlatmadım; olay gecesi ${index + 1}. saatte bina çevresinde kısa bir hareketlilik vardı. Ayrıntıyı araştırmanız gerekiyor."`,
                difficulty: 2,
                category: 'derinlesme'
            });
        }
        return normalized.slice(0, 4);
    },

    init: function () {
        console.log("🌲 Sisören Dağ Kasabası Motoru v3.0 Başlatıldı.");
        this.registerSisorenData();
        this.setupEventListeners();
        this.setupMapObserver();

    },

    // =============================
    // 1. NPC & DELİL VERİ KAYDI
    // =============================
    registerSisorenData: function () {
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
                            bg: null,
                            talkBg: null,
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
                        bg: null,
                        talkBg: null,
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
        this.visitedSisorenBuildings.clear();
        document.body.classList.remove('sisoren-theme');
        this.clearSisorenMap();

        // Kasabaya özel otopsi ve lab durumunu tazele
        if (typeof window.checkAutopsyConditions === 'function') {
            window.checkAutopsyConditions();
        }
    },

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
    },

    // =============================
    // 6. SİSÖREN DURUMU UYGULA
    // =============================
    applySisorenState: function () {
        this.registerSisorenData();

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
            setTimeout(() => {
                if (typeof window.showCinematicHelper === 'function') {
                    window.showCinematicHelper(
                        'Amirims, Sisören dağ kasabasına hoş geldiniz! Yoğun sis ve karanlık dağ yamaçları arasında gizlenen kasabada en az 5 binayı incelemeli ve 5 delili Adli Tıbba göndermeliyiz. Binalara tıklayarak soruşturmaya başlayalım!\n\n👥 Binalara girip hem dükkan sahipleriyle hem de içerideki kasabalılarla konuşabilirsiniz!',
                        false, 'sisoren_intro'
                    );
                }
            }, 1200);
        }

        // Sokak NPC konuşma noktaları şimdilik kapalı (bina hotspotlarıyla çakışıyordu)
    },

    // =============================
    // 6.5 SİSÖREN DİĞER KASABALARDAN FARKLI HİKAYE GİRİŞİ (DAKTİLO EFEKTİ)
    // =============================
    showSisorenStoryIntro: function () {
        window.currentActiveTown = 'sisoren';
        this.currentActiveTown = 'sisoren';
        document.body.classList.remove('golge-sehir-theme');
        document.body.classList.add('sisoren-theme');
        this.registerSisorenData();

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
                    console.log("🎲 Sisören Rastgele Yeni Katil Belirlendi:", data.guiltyNpcId);
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
        if (typeof window.activeNpcId !== 'undefined') window.activeNpcId = bld.npcId;

        // Bina içi ekstra NPC'leri bul
        const buildingExtras = (window.SISOREN_CONFIG.extraNpcs || []).filter(e => e.buildingId === bld.id);
        const buildingChildren = bld.children || [];

        // Bina iç mekan ekranını aç (interior-screen)
        this.openSisorenInteriorScreen(bld, npc, buildingExtras, buildingChildren);

        // Otopsi sayacını güncelle
        if (typeof window.checkAutopsyConditions === 'function') {
            setTimeout(() => window.checkAutopsyConditions(), 500);
        }
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

        // İç mekan: tek görsel, tekrarsız, görüntünün tamamını ekrana doldur.
        this.clearInteriorMarkers();
        const interiorImgUrl = bld.interiorImg || bld.npc.bg || bld.npc.talkBg;
        interiorScreen.style.backgroundImage = 'none';
        interiorScreen.style.backgroundColor = '#030806';

        if (stageCanvas && interiorImgUrl) {
            stageCanvas.style.backgroundImage = `url('${interiorImgUrl}?v=${Date.now()}')`;
            stageCanvas.style.backgroundSize = '100% 100%';
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
        this.renderPrimaryNpc(npc, bld);
        this.renderInteriorOccupants(allExtrasInBuilding, bld, npc);

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

    renderPrimaryNpc: function (npc, bld) {
        const stageCanvas = document.getElementById('interior-stage-canvas');
        if (!stageCanvas || !npc || !bld.primaryNpcPos) return;

        const marker = document.createElement('button');
        marker.type = 'button';
        marker.className = 'sisoren-primary-npc-marker';
        marker.style.top = bld.primaryNpcPos.top;
        marker.style.left = bld.primaryNpcPos.left;
        marker.title = `${npc.name} ile Konuş`;
        marker.innerHTML = `<img src="${npc.portrait}" alt="${npc.name}"><span>${npc.name}</span>`;
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
        return { top: '50%', left: '50%' };
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
                bg: null,
                talkBg: null,
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
