// ========================================================
// BANANA STUDIO & NASHI - DEDEKTIF ENTEGRASYON MODULU
// ========================================================

(function () {
    console.log("🎬 Banana Studio & Nashi Entegrasyon Modülü Yükleniyor...");

    // HTML Bileşenlerini Dinamik Ekle
    function injectStudioUI() {
        if (document.getElementById("studio-toggle-btn")) return;

        // 1. Üst Çubuk Butonu (Geliştirici modu ile açılır)
        const toggleBtn = document.createElement("button");
        toggleBtn.id = "studio-toggle-btn";
        toggleBtn.className = "studio-toggle-btn";
        toggleBtn.title = "Banana Studio & Nashi AI Medya Atölyesi (Geliştirici Modu)";
        toggleBtn.innerHTML = `<i class="fa-solid fa-wand-magic-sparkles"></i> <span>AI STÜDYO (DEV)</span>`;
        document.body.appendChild(toggleBtn);

        // URL'de ?studio=1 varsa veya localStorage'da açıksa göster
        const urlParams = new URLSearchParams(window.location.search);
        if (urlParams.has('studio') || localStorage.getItem('dev_studio_active') === 'true') {
            toggleBtn.classList.add('visible-dev');
        }

        // Kısayol: Ctrl + Shift + S tuşlarına basınca aç/kapat
        window.addEventListener('keydown', (e) => {
            if (e.ctrlKey && e.shiftKey && (e.key === 'S' || e.key === 's')) {
                toggleBtn.classList.toggle('visible-dev');
                const isVis = toggleBtn.classList.contains('visible-dev');
                localStorage.setItem('dev_studio_active', isVis ? 'true' : 'false');
                if (isVis) {
                    modal.classList.remove('hidden');
                }
            }
        });

        // 2. Modal Konteyneri
        const modal = document.createElement("div");
        modal.id = "studio-modal";
        modal.className = "studio-modal hidden";
        modal.innerHTML = `
        <div class="studio-modal-card">
            <!-- Modal Başlık -->
            <div class="studio-modal-header">
                <div class="studio-title-group">
                    <h2><i class="fa-solid fa-clapperboard"></i> DEDEKTİF AI MEDYA & SİNEMATİK ATÖLYESİ</h2>
                    <p>Banana Studio & Nashi Altyapısıyla Güçlendirilmiş Yapay Zekâ Görsel ve Video Üretimi</p>
                </div>
                <button id="studio-close-btn" class="studio-close-btn" title="Kapat">
                    <i class="fa-solid fa-xmark"></i>
                </button>
            </div>

            <!-- Sekmeler -->
            <div class="studio-tabs">
                <button class="studio-tab-btn active" data-tab="quick-generator">
                    <i class="fa-solid fa-bolt"></i> Hızlı Üretici
                </button>
                <button class="studio-tab-btn" data-tab="external-apps">
                    <i class="fa-solid fa-cubes"></i> Harici Stüdyolar (Banana & Nashi)
                </button>
                <button class="studio-tab-btn" data-tab="studio-gallery">
                    <i class="fa-solid fa-images"></i> Üretilen Medyalar
                </button>
            </div>

            <!-- Sekme İçerikleri -->
            <div class="studio-tab-content">
                <!-- 1. HIZLI ÜRETİCİ -->
                <div id="tab-quick-generator" class="studio-tab-pane active">
                    <div class="studio-form-grid">
                        <div class="studio-form-left">
                            <div class="studio-field">
                                <label><i class="fa-solid fa-tag"></i> Vaka ve Kategori</label>
                                <select id="studio-category" class="studio-select">
                                    <option value="character">🎭 Şüpheli Portresi (Kasap, Hekim, Muhtar...)</option>
                                    <option value="location">🏚️ Mekân & Olay Yeri (Eczane, Sisören, Orman...)</option>
                                    <option value="clue">🔍 Delil & Adli Tıp Nesnesi (Zehir Şişesi, Mektup...)</option>
                                    <option value="cinematic">🎬 Sinematik Giriş & Video Sahnesi</option>
                                </select>
                            </div>

                            <div class="studio-field">
                                <label><i class="fa-solid fa-lightbulb"></i> Hazır Dedektiflik Şablonları</label>
                                <select id="studio-presets" class="studio-select">
                                    <option value="">-- Şablon Seçin veya Kendiniz Yazın --</option>
                                    <option value="Gece vakti sisli çam ormanında gaz feneriyle yürüyen şüpheli gece bekçisi Rıfat">Gece Bekçisi Rıfat (Ormanda Fenerle)</option>
                                    <option value="1940'lar tarzı eski bir eczanede loş sarı ışık altında gizemli zehir ve banotu şişeleri">Eczacı Selma (Zehir Şişeleri Masada)</option>
                                    <option value="Sisören kasabasında yoğun sis altında tarihi taş kahvehane ve pencerelerden sızan sarı ışık">Sisören Tarihi Kahvehanesi (Sis Altında)</option>
                                    <option value="Masa üzerinde açılmış, kan lekesi ve parmak izi olan eski Osmanlıca veresiye defteri">Kan Lekeli Veresiye Defteri (Delil)</option>
                                    <option value="Öfkeli ve suçluluğunu gizlemeye çalışan demirci ustası, ocak başında elinde çekiçle">Demirci Kazım (Sorgulama Anı Portresi)</option>
                                </select>
                            </div>

                            <div class="studio-field">
                                <label><i class="fa-solid fa-pen-nib"></i> Sahne / Karakter Açıklaması</label>
                                <textarea id="studio-prompt" class="studio-textarea" placeholder="Örn: 1940'lar kasabasında cinayet mahallinde fenerle ipucu arayan yağmurluklu dedektif..."></textarea>
                            </div>

                            <div class="studio-btn-row">
                                <button id="studio-enhance-btn" class="btn-studio-enhance" title="Banana Studio'nun Enhance özelliğiyle promptu sinematikleştirir">
                                    <i class="fa-solid fa-wand-magic-sparkles"></i> Enhance (İstemi Güçlendir)
                                </button>
                                <button id="studio-generate-btn" class="btn-studio-generate">
                                    <i class="fa-solid fa-paintbrush"></i> Görsel Üret
                                </button>
                            </div>
                        </div>

                        <!-- Sağ: Önizleme -->
                        <div class="studio-form-right">
                            <label><i class="fa-solid fa-eye"></i> Stüdyo Çıktısı</label>
                            <div id="studio-preview" class="studio-preview-box">
                                <div class="studio-preview-placeholder">
                                    <i class="fa-solid fa-camera-retro"></i>
                                    <p>Henüz bir üretim yapılmadı.<br>Sol taraftan bir sahne girip "Görsel Üret"e basın.</p>
                                </div>
                            </div>
                            <div id="studio-output-actions" style="margin-top:12px; display:none; gap:10px;">
                                <a id="studio-download-btn" class="btn-studio-enhance" href="#" download style="text-decoration:none; display:inline-flex;">
                                    <i class="fa-solid fa-download"></i> İndir
                                </a>
                                <span id="studio-save-status" style="font-size:0.8rem; color:#4ade80; display:flex; align-items:center; gap:6px;">
                                    <i class="fa-solid fa-check-circle"></i> Oyuna Kaydedildi! (images/studio)
                                </span>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- 2. HARİCİ STÜDYOLAR (BANANA STUDIO & NASHI) -->
                <div id="tab-external-apps" class="studio-tab-pane">
                    <div class="studio-app-cards">
                        <!-- Banana Studio -->
                        <div class="studio-app-card">
                            <div>
                                <h3><i class="fa-solid fa-lemon" style="color:#ffd700;"></i> Banana Studio</h3>
                                <p>Orkun Işıtmak tarafından geliştirilen tam teşekküllü AI medya stüdyosu. Seçili alan boyama (inpainting), gelişmiş video modelleri (Seedance, MiniMax H3 Max) ve çoklu prompt üretimi sağlar.</p>
                                <ul>
                                    <li><i class="fa-solid fa-check"></i> Delil üstüne kan/leke ekleme (Area Edit)</li>
                                    <li><i class="fa-solid fa-check"></i> Metinden veya görselden Sinematik Video</li>
                                    <li><i class="fa-solid fa-check"></i> Gemini, OpenAI, fal.ai entegrasyonu</li>
                                </ul>
                            </div>
                            <button id="launch-banana-btn" class="btn-studio-generate" style="width:100%; justify-content:center;">
                                <i class="fa-solid fa-rocket"></i> Banana Studio'yu Başlat (Web)
                            </button>
                        </div>

                        <!-- Nashi App -->
                        <div class="studio-app-card">
                            <div>
                                <h3><i class="fa-solid fa-scissors" style="color:#60a5fa;"></i> Nashi Media Toolkit</h3>
                                <p>Üretilen video ve görselleri Dedektif oyununa aktarmadan önce kırpma (Trim In/Out), WebM/MP4 format dönüştürme ve görsel boyutlandırma için yerel Windows aracı.</p>
                                <ul>
                                    <li><i class="fa-solid fa-check"></i> Sinematik video sahnelerini saniyesine göre kırpma</li>
                                    <li><i class="fa-solid fa-check"></i> Web için optimize dosya dönüştürme</li>
                                    <li><i class="fa-solid fa-check"></i> Şeffaflık ve görsel oran ayarları</li>
                                </ul>
                            </div>
                            <button id="launch-nashi-btn" class="btn-studio-enhance" style="width:100%; justify-content:center; background:#1e3a8a; border-color:#3b82f6; color:#93c5fd;">
                                <i class="fa-solid fa-desktop"></i> Nashi Uygulamasını Aç (Windows)
                            </button>
                        </div>
                    </div>
                </div>

                <!-- 3. GALERİ -->
                <div id="tab-studio-gallery" class="studio-tab-pane">
                    <div id="studio-gallery-container" class="studio-gallery-grid">
                        <p style="color:#888; grid-column: 1 / -1; text-align:center;">Medyalar yükleniyor...</p>
                    </div>
                </div>
            </div>
        </div>
        `;

        document.body.appendChild(modal);

        // Olay Dinleyicileri Kur
        setupEvents();
    }

    function setupEvents() {
        const toggleBtn = document.getElementById("studio-toggle-btn");
        const modal = document.getElementById("studio-modal");
        const closeBtn = document.getElementById("studio-close-btn");

        toggleBtn.addEventListener("click", () => {
            modal.classList.remove("hidden");
        });

        closeBtn.addEventListener("click", () => {
            modal.classList.add("hidden");
        });

        // Sekme Değişimi
        const tabs = modal.querySelectorAll(".studio-tab-btn");
        tabs.forEach(tab => {
            tab.addEventListener("click", () => {
                tabs.forEach(t => t.classList.remove("active"));
                tab.classList.add("active");

                const target = tab.dataset.tab;
                modal.querySelectorAll(".studio-tab-pane").forEach(pane => pane.classList.remove("active"));
                const activePane = document.getElementById(`tab-${target}`);
                if (activePane) activePane.classList.add("active");

                if (target === "studio-gallery") {
                    loadGallery();
                }
            });
        });

        // Şablon Seçildiğinde Prompt Kutusuna Doldur
        const presetSelect = document.getElementById("studio-presets");
        const promptInput = document.getElementById("studio-prompt");
        presetSelect.addEventListener("change", () => {
            if (presetSelect.value) {
                promptInput.value = presetSelect.value;
            }
        });

        // 1. Enhance Butonu (Gemini Prompt İyileştirme)
        const enhanceBtn = document.getElementById("studio-enhance-btn");
        enhanceBtn.addEventListener("click", async () => {
            const rawPrompt = promptInput.value.trim();
            if (!rawPrompt) {
                alert("Lütfen önce bir sahne açıklaması veya şablon girin.");
                return;
            }

            enhanceBtn.disabled = true;
            enhanceBtn.innerHTML = `<i class="fa-solid fa-spinner fa-spin"></i> Zenginleştiriliyor...`;

            try {
                const res = await fetch("/api/studio/enhance-prompt", {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({
                        prompt: rawPrompt,
                        category: document.getElementById("studio-category").value
                    })
                });
                const data = await res.json();
                if (data.enhancedPrompt) {
                    promptInput.value = data.enhancedPrompt;
                }
            } catch (err) {
                console.error("Enhance hatası:", err);
            } finally {
                enhanceBtn.disabled = false;
                enhanceBtn.innerHTML = `<i class="fa-solid fa-wand-magic-sparkles"></i> Enhance (İstemi Güçlendir)`;
            }
        });

        // 2. Görsel Üret Butonu
        const generateBtn = document.getElementById("studio-generate-btn");
        const previewBox = document.getElementById("studio-preview");
        const outputActions = document.getElementById("studio-output-actions");
        const downloadBtn = document.getElementById("studio-download-btn");

        generateBtn.addEventListener("click", async () => {
            const prompt = promptInput.value.trim();
            if (!prompt) {
                alert("Lütfen bir prompt girin.");
                return;
            }

            generateBtn.disabled = true;
            generateBtn.innerHTML = `<i class="fa-solid fa-spinner fa-spin"></i> Üretiliyor...`;
            previewBox.innerHTML = `
                <div style="text-align:center; color:#ffd700;">
                    <i class="fa-solid fa-spinner fa-spin" style="font-size:3rem; margin-bottom:12px;"></i>
                    <p style="font-family:'Cinzel',serif; font-size:1.1rem;">Yapay Zekâ Sahneyi Çiziyor...</p>
                    <p style="font-size:0.8rem; color:#888;">Noir ışıklandırma ve dokular işleniyor...</p>
                </div>
            `;

            try {
                const res = await fetch("/api/studio/generate-image", {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({
                        prompt: prompt,
                        targetCategory: document.getElementById("studio-category").value
                    })
                });
                const data = await res.json();
                if (data.success && data.imageUrl) {
                    previewBox.innerHTML = `<img src="${data.imageUrl}?t=${Date.now()}" alt="Studio Output" />`;
                    downloadBtn.href = data.imageUrl;
                    downloadBtn.download = data.fileName;
                    outputActions.style.display = "flex";
                } else {
                    previewBox.innerHTML = `<p style="color:#ef4444;">Hata: ${data.error || 'Görsel üretilemedi.'}</p>`;
                }
            } catch (err) {
                console.error("Üretim hatası:", err);
                previewBox.innerHTML = `<p style="color:#ef4444;">Bağlantı hatası: ${err.message}</p>`;
            } finally {
                generateBtn.disabled = false;
                generateBtn.innerHTML = `<i class="fa-solid fa-paintbrush"></i> Görsel Üret`;
            }
        });

        // 3. Banana Studio Başlat Butonu
        const launchBananaBtn = document.getElementById("launch-banana-btn");
        launchBananaBtn.addEventListener("click", async () => {
            launchBananaBtn.disabled = true;
            launchBananaBtn.innerHTML = `<i class="fa-solid fa-spinner fa-spin"></i> Başlatılıyor...`;
            try {
                const res = await fetch("/api/studio/launch-banana", { method: "POST" });
                const data = await res.json();
                if (data.url) {
                    window.open(data.url, "_blank");
                }
            } catch (err) {
                alert("Banana Studio başlatılırken hata oluştu: " + err.message);
            } finally {
                launchBananaBtn.disabled = false;
                launchBananaBtn.innerHTML = `<i class="fa-solid fa-rocket"></i> Banana Studio'yu Başlat (Web)`;
            }
        });

        // 4. Nashi Başlat Butonu
        const launchNashiBtn = document.getElementById("launch-nashi-btn");
        launchNashiBtn.addEventListener("click", async () => {
            launchNashiBtn.disabled = true;
            launchNashiBtn.innerHTML = `<i class="fa-solid fa-spinner fa-spin"></i> Başlatılıyor...`;
            try {
                const res = await fetch("/api/studio/launch-nashi", { method: "POST" });
                const data = await res.json();
                if (data.success) {
                    alert("Nashi Medya Aracı başarıyla açıldı!");
                } else {
                    alert(data.error || "Açılamadı.");
                }
            } catch (err) {
                alert("Nashi başlatılırken hata oluştu: " + err.message);
            } finally {
                launchNashiBtn.disabled = false;
                launchNashiBtn.innerHTML = `<i class="fa-solid fa-desktop"></i> Nashi Uygulamasını Aç (Windows)`;
            }
        });
    }

    // Galeri Yükle
    async function loadGallery() {
        const container = document.getElementById("studio-gallery-container");
        if (!container) return;
        try {
            const res = await fetch("/api/studio/gallery");
            const items = await res.json();

            if (!items || items.length === 0) {
                container.innerHTML = `<p style="color:#888; grid-column:1/-1; text-align:center;">Henüz üretilmiş bir medya bulunmuyor.</p>`;
                return;
            }

            container.innerHTML = items.map(item => `
                <div class="studio-gallery-item" title="${item.fileName}">
                    ${item.isVideo 
                        ? `<video src="${item.url}" style="width:100%; height:120px; object-fit:cover;" controls></video>`
                        : `<img src="${item.url}" alt="${item.fileName}" onclick="window.open('${item.url}', '_blank')" />`
                    }
                    <div class="studio-gallery-info">${item.fileName}</div>
                </div>
            `).join("");
        } catch (err) {
            container.innerHTML = `<p style="color:#ef4444; grid-column:1/-1; text-align:center;">Galeri yüklenemedi: ${err.message}</p>`;
        }
    }

    // Sayfa Yüklendiğinde Başlat
    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", injectStudioUI);
    } else {
        injectStudioUI();
    }
})();
