with open(r'c:\Users\solma\OneDrive\Desktop\Dedektiflik\wwwroot\js\app.js', 'r', encoding='utf-8') as f:
    text = f.read()

old_2023 = """        case 2023: // Yırtık Borç Listesi
            content.classList.add('sisoren-realistic-doc-container');
            content.innerHTML = `
                <div class="realistic-doc-wrapper">
                    <img src="images/towns/sisoren/deliller/doc_2023.png" class="realistic-doc-image realistic-wide" alt="Yırtık Borç Listesi">
                </div>
            `;
            break;"""

new_2023 = """        case 2023: // Yırtık Borç Listesi (Kahvehane Açık İki Sayfalı Veresiye Defteri)
            content.classList.add('black-book');
            content.innerHTML = `
                <div class="open-notebook-wrapper">
                    <div class="notebook-spine"></div>
                    <div class="notebook-page left-page">
                        <div class="notebook-title">SİSÖREN KAHVEHANESİ VERESİYE</div>
                        <div class="notebook-date">1952 Güz Dönemi Hesapları</div>
                        <div class="ledger-row">
                            <span class="ledger-name">Çoban Durmuş:</span>
                            <span class="ledger-val">120 TL (Ödendi)</span>
                        </div>
                        <div class="ledger-row">
                            <span class="ledger-name">Tüpçü Şevket:</span>
                            <span class="ledger-val">310 TL</span>
                        </div>
                        <div class="ledger-row">
                            <span class="ledger-name">Hurdacı Zehra:</span>
                            <span class="ledger-val">480 TL</span>
                        </div>
                        <div class="ledger-row strike-row">
                            <span class="ledger-name victim-strike" style="color:#ef4444;">Madenci Halil Efendi:</span>
                            <span class="ledger-val victim-strike-val">65.000 TL</span>
                            <div class="hand-strike-line"></div>
                        </div>
                        <div class="ledger-row">
                            <span class="ledger-name">Sinemacı Nejat:</span>
                            <span class="ledger-val">250 TL</span>
                        </div>
                        <div class="ledger-row">
                            <span class="ledger-name">Sahaf Hikmet:</span>
                            <span class="ledger-val">85 TL</span>
                        </div>
                    </div>
                    <div class="notebook-page right-page">
                        <div class="notebook-title">İRFAN'IN KİŞİSEL NOTLARI</div>
                        <div class="handwritten-note">• Halil kahvehanemi ipotek ettirip ocağımı söndürmek istiyor.</div>
                        <div class="handwritten-note">• Her gün gelip yüzüme sırıtıyor, beni kasabada küçük düşürüyor. Sabrım kalmadı.</div>
                        <div class="handwritten-note">• Altıpatları temizleyip yağladım. Bu hesap dağ yolunda kanla kapanacak!</div>
                        <div class="notebook-stamp" style="border-color:#ef4444; color:#ef4444;">KAHVEHANE HACİZLİ</div>
                    </div>
                </div>
            `;
            break;"""

if old_2023 in text:
    text = text.replace(old_2023, new_2023, 1)
    with open(r'c:\Users\solma\OneDrive\Desktop\Dedektiflik\wwwroot\js\app.js', 'w', encoding='utf-8') as f:
        f.write(text)
    print("SUCCESS_2023_FIXED")
else:
    print("NOT_FOUND")
