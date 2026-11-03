import re

file_path = r'c:\Users\solma\OneDrive\Desktop\Dedektiflik\wwwroot\js\app.js'
with open(file_path, 'r', encoding='utf-8') as f:
    text = f.read()

# 2011'den 2132'ye kadar olan bloğu bul
start_marker = "        case 2011: // Şifreli Telgraf Şeridi"
end_marker = "            break;\n    }\n    overlay.classList.remove('hidden');"

idx_start = text.find(start_marker)
idx_end = text.find(end_marker)

print("idx_start:", idx_start, "idx_end:", idx_end)

sisoren_html_cases = """        case 2011: // Şifreli Telgraf Şeridi
            content.classList.add('threat-letter', 'sisoren-telegram');
            content.innerHTML = `
                <div class="letter-header" style="color: #064e3b; border-bottom: 2px dashed #059669;">T.C. POSTA VE TELGRAF İDARESİ — SİSÖREN HATTI</div>
                <div class="letter-body">
                    <p style="font-family: monospace; font-size: 1.15rem; letter-spacing: 2px; color: #047857;">--. . -.-. . / -.-- .- .-. .. ... .. / --. --- .-.. -.. .</p>
                    <p style="color: #b91c1c; font-weight: bold; margin-top: 8px;">[MORS ÇÖZÜMÜ]:</p>
                    <p>"...Halil şantaj defterini maden vagonuna saklamış. Gece yarısı gölde telleri keseceğiz, jandarmayı arayamaz..."</p>
                    <div class="letter-signature" style="color: #064e3b;">Rüstem</div>
                </div>
            `;
            break;
        case 2015: // Tehdit Mesajı Taslağı
            content.classList.add('threat-letter');
            content.innerHTML = `
                <div class="letter-header">KARALAMA KAĞIDI & İSİMSİZ TEHDİT</div>
                <div class="letter-body">
                    <p>Halil Efendi,</p>
                    <p>Madendeki kaçak sevkiyatları ve madenci ailelerinden çaldığın yetim haklarını bildiğimi inkâr etme. Savcılığa gidecek resmi telgraf kopyaları masamda duruyor.</p>
                    <p>Ya bu akşam telgrafhaneye 50.000 Lira nakit bırakırsın, ya da yarın şafakta jandarma kapını kırar!</p>
                    <div class="letter-signature">[İsim Karalanmış]</div>
                </div>
            `;
            break;
        case 2023: // Yırtık Borç Listesi (Kahvehane Açık İki Sayfalı Veresiye Defteri)
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
                        <div class="handwritten-note">• Halil kahvehanemi hacizle elimden alıp ocağımı söndürmek istiyor.</div>
                        <div class="handwritten-note">• Her gün gelip yüzüme sırıtıyor, bana hakaret ediyor. Sabrım tükendi.</div>
                        <div class="handwritten-note">• Altıpatları temizleyip yağladım. Bu hesap dağ yolunda kanla kapanacak!</div>
                        <div class="notebook-stamp" style="border-color:#ef4444; color:#ef4444;">KAHVEHANE HACİZLİ</div>
                    </div>
                </div>
            `;
            break;
        case 2032: // Şantajlı Afiş
            content.classList.add('confidential-file');
            content.innerHTML = `
                <div class="police-file-header" style="color: #701a75; border-bottom: 2px solid #701a75;">"GECENİN GÖLGELERİ" (1949) — AFİŞ ARKA YÜZÜ</div>
                <div class="letter-body" style="font-family: 'Caveat', 'Kalam', cursive; font-size: 1.5rem; line-height: 1.5; color: #3b0764;">
                    <p style="color: #701a75; font-weight: bold;">[Mor Mürekkeple Yazılmış Şantaj]:</p>
                    <p>"Nejat, projeksiyon odasındaki kaçak altınları Halil gözleriyle gördü. O karanlık işlerin 35 mm makaralara kaydedildi. Ya sus payını getirirsin ya da makaralar savcının masasında döner!"</p>
                    <p style="color: #b91c1c; font-weight: bold; margin-top: 15px;">[Nejat'ın cevabı]: Perde bu gece Halil'in üzerine kapanacak!</p>
                </div>
            `;
            break;
        case 2033: // Düşmüş Bilet Koçanı
            content.classList.add('threat-letter');
            content.innerHTML = `
                <div class="letter-header">SİSÖREN BELEDİYE SİNEMASI GİRİŞ BİLETİ</div>
                <div class="letter-body">
                    <p><strong>SEANS:</strong> Gece Yarısı Özel Gösterimi (01:30)</p>
                    <p><strong>YER:</strong> Balkon No: 13</p>
                    <p style="color: #b91c1c; margin-top: 10px;">"Saat 02:00'de makine dairesinde ol. Halil tek başına gelecek..."</p>
                </div>
            `;
            break;
        case 2042: // Bakkal Cemile'nin Yırtık Veresiye Sayfası
            content.classList.add('prescription');
            content.innerHTML = `
                <div class="prescription-header">
                    <h3>SİSÖREN DAĞ BAKKALI VERESİYE KAYDI</h3>
                    <p>Tarih: 18 Ekim 1952 | Kayıt No: #412</p>
                </div>
                <div class="prescription-body">
                    <p><strong>Müşteri:</strong> Halil Efendi (Maden Müteahhidi)</p>
                    <p>• Dükkân İpotek Borcu: 40.000 TL</p>
                    <hr>
                    <p class="torn-edge-text">
                        "Cemile, borcu kapatmazsan bakkala el koyuyorum..." <span class="faded-torn-text">[Yırtılmış / Kesilmiş]</span>
                    </p>
                    <p style="color: #047857; font-weight: bold; margin-top: 8px;">[Cemile'nin el yazısı]: Kilerdeki fare zehri bu akşam peynire karışacak!</p>
                </div>
            `;
            break;
        case 2045: // İsimli Senet
            content.classList.add('title-deed');
            content.innerHTML = `
                <div class="deed-header">T.C. SİSÖREN NOTERLİĞİ</div>
                <div class="deed-title">MÜLKİYET REHİN VE BORÇ SENEDİ</div>
                <p><strong>Borçlu:</strong> Bakkal Cemile Hanım</p>
                <p><strong>Alacaklı:</strong> Madenci Halil Efendi</p>
                <p><strong>Tutar:</strong> 40.000 TL</p>
                <div class="deed-warning">[VADESİ DOLMUŞTUR — HACİZ KARARI]</div>
            `;
            break;
        case 2051: // İçi Oyulmuş Ansiklopedi & Harita
            content.classList.add('confidential-file');
            content.innerHTML = `
                <div class="police-file-header" style="color: #78350f; border-bottom: 2px solid #78350f;">BÜYÜK OSMANLI TARİHİ (CİLT IV) — GİZLİ HARİTA</div>
                <div class="letter-body" style="font-family: 'Caveat', 'Kalam', cursive; font-size: 1.45rem; line-height: 1.5; color: #451a03;">
                    <p style="color: #9a3412; font-weight: bold;">"SİSÖREN ESKİ MADEN GALERİSİ KROKİSİ"</p>
                    <p>• Kuzey kuyusu 4. galeride kilitli demir sandık gizlidir.</p>
                    <p>• Halil haritayı çalmak için sahafı basacağını söyledi. Ecdat mirasını o haramiye teslim etmeyeceğim. Bu kalın ansiklopedi kafasını ezecek!</p>
                    <div class="letter-signature" style="color: #78350f;">Sahaf Hikmet</div>
                </div>
            `;
            break;
        case 2052: // Şifreli Kenar Notları
            content.classList.add('secret-note');
            content.innerHTML = `
                <div class="secret-note-title">KİTAP KENARI EL YAZISI</div>
                <p>"Kardeşim, maktulün gözü doymuyor. Dün gece dükkânı gözetledi. Saat 02:00 maden girişi kilitli olacak."</p>
                <div class="secret-note-sig">- H.</div>
            `;
            break;
        case 2055: // Sahte Vasiyetname
            content.classList.add('title-deed');
            content.innerHTML = `
                <div class="deed-header">VERASET VE İNTİKAL VASİYETNAMESİ</div>
                <div class="deed-title">VASİYET VE MÜLK TAKSİMİ</div>
                <p><strong>Miras Bırakan:</strong> Halil Efendi</p>
                <p><strong>Lehdar:</strong> Sahaf Hikmet (Tüm altın haritaları ve el yazmaları)</p>
                <div class="deed-warning">[SAHTE İMZA VE TAKLİT EL YAZISI]</div>
            `;
            break;
        case 2061: // Asitle Silinmiş Kütük
            content.classList.add('confidential-file');
            content.innerHTML = `
                <div class="police-file-header" style="color: #064e3b; border-bottom: 2px solid #064e3b;">SİSÖREN KÖYÜ NÜFUS KÜTÜK DEFTERİ #12</div>
                <div class="letter-body">
                    <p><strong>Sayfa:</strong> 184 | Hane No: 29</p>
                    <p style="background: rgba(0,0,0,0.85); color: #f87171; padding: 12px; border-radius: 4px; font-family: monospace;">
                        [SÜLFÜRİK ASİTLE YAKILMIŞ ALAN]<br>
                        Halil Efendi 840 Dönüm Mera Payı... [SİLİNMİŞ]<br>
                        -> Muhtar Meliha Hanım zimmetine tescil edilmiştir.
                    </p>
                    <p style="color: #b91c1c; font-style: italic; margin-top: 10px;">"Halil bu tahrifatı buldu. Müfettiş gelse de dinleyecek bir Halil bulamayacak!"</p>
                </div>
            `;
            break;
        case 2063: // Gizli Orman Tapusu
            content.classList.add('title-deed');
            content.innerHTML = `
                <div class="deed-header">T.C. TAPU VE KADASTRO GENEL MÜDÜRLÜĞÜ</div>
                <div class="deed-title">ARAZİ DEVİR SENEDİ</div>
                <p><strong>Mevkii:</strong> Sisören Çamtepe Yaylası No: 712</p>
                <p><strong>Hak Sahibi:</strong> Muhtar Meliha Hanım</p>
                <div class="deed-warning">[KÖY MÜLKİYETİNDEN ŞÜPHELİ DEVİR]</div>
            `;
            break;
        case 2065: // Parçalanmış Uyarı Mektubu
            content.classList.add('threat-letter');
            content.innerHTML = `
                <div class="letter-header">VİLAYET MÜFETTİŞLİĞİ ÖN İNCELEME İHBARI</div>
                <div class="letter-body">
                    <p>Sisören Köyü Muhtarlığına,</p>
                    <p>Halil Efendi'nin şikayeti üzerine köy meralarının kanunsuz devri soruşturulacaktır. Müfettiş heyeti yarın intikal edecektir.</p>
                    <div class="letter-signature">Valilik İdare Kurulu</div>
                </div>
            `;
            break;
        case 2072: // Şifreli Sigara Kâğıdı
            content.classList.add('secret-note');
            content.innerHTML = `
                <div class="secret-note-title">TÜTÜN FİLTRESİ İÇİ NOTU</div>
                <p>"Nermin Hanım, kaçak Virginia tütünleri dağ deposunda. Halil bu gece jandarmayı yollayacak. Bu adamı sisin içinde susturmak zorundayız."</p>
                <div class="secret-note-sig">- Dağ Kuryesi</div>
            `;
            break;
        case 2085: // Saplı Bıçak Notu
            content.classList.add('threat-letter');
            content.innerHTML = `
                <div class="letter-header" style="color: #7f1d1d;">DİRSEK TAHTASINA SAPLANMIŞ KANLI KAĞIT</div>
                <div class="letter-body">
                    <p style="font-size: 1.5rem; color: #991b1b; font-weight: bold;">HALİL HARAMİSİ!</p>
                    <p>Sürümün içtiği dereye maden zehri dökmeyi bırakacaksın dedik. Üç gebe koyunum can verdi.</p>
                    <p>Dağ yolunda karşıma çıkarsan o paslı nalbant çivisini kalbine çakarım!</p>
                    <div class="letter-signature" style="color: #7f1d1d;">Çoban Durmuş</div>
                </div>
            `;
            break;
        case 2093: // Sahte Teslimat Kaydı
            content.classList.add('confidential-file');
            content.innerHTML = `
                <div class="police-file-header">SİSÖREN AYGAZ BAYİİ TESLİMAT JURNALİ</div>
                <div class="letter-body">
                    <p><strong>Tarih:</strong> 24 Ekim Gece | 23:45</p>
                    <p>• Madenci Halil Efendi Konutu (1 Adet Sanayi Tüpü)</p>
                    <p style="color: #b91c1c; font-style: italic; margin-top: 10px;">"Valfi bilerek gevşettim... Bir kibrit çaktığında her şey kül olacak!" — Şevket</p>
                </div>
            `;
            break;
        case 2104: // Kanlı Tartım Fişi
            content.classList.add('threat-letter');
            content.innerHTML = `
                <div class="letter-header">SİSÖREN KANTAR İŞLETMESİ MAKBUZU #8921</div>
                <div class="letter-body">
                    <p><strong>Tartım Saati:</strong> 02:18 (Gece) | 420 Kg Çelik Hurda</p>
                    <p style="color: #991b1b; font-style: italic; margin-top: 10px;">"Halil presin yanına geldi. Çelik kapak kapandı... Artık hurdalardan farksız!"</p>
                    <div class="letter-signature" style="color: #991b1b;">Hurdacı Zehra</div>
                </div>
            `;
            break;
        case 2111: // Yırtık Mektup (Zeynep Teyze)
            content.classList.add('threat-letter');
            content.innerHTML = `
                <div class="letter-header">ESKİ YIRTIK MEKTUP PARÇASI</div>
                <div class="letter-body">
                    <p>Zeynep Hanım,</p>
                    <p>Yıllar önce kaybettiğin o oğlanın sırrını saklamamın bir bedeli var. Bu gece son altın bileziği de pencereye koyacaksın.</p>
                    <div class="letter-signature">Halil</div>
                </div>
            `;
            break;
        case 2121: // Eski Fotoğraf Albümü
            content.classList.add('confidential-file');
            content.innerHTML = `
                <div class="police-file-header">HATİCE NİNE'NİN ESKİ DAĞ ALBÜMÜ</div>
                <div class="letter-body" style="font-family: 'Caveat', 'Kalam', cursive; font-size: 1.45rem;">
                    <p>"Sisören Dağları bizim yurdumuzdur. Halil bu toprakları ecnebi şirketlere satmaya kalkarsa, dağın kara mantarları kaynar, eceli bir bardak çayda bulur."</p>
                    <div class="letter-signature">- Hatice Nine</div>
                </div>
            `;
            break;
        case 2125: // Sahte Arazi Tapusu
            content.classList.add('title-deed');
            content.innerHTML = `
                <div class="deed-header">ARAZİ SATIŞ VE FERAGAT SENEDİ</div>
                <div class="deed-title">SİSÖREN DAĞ KÖYÜ TOPLU SATIŞ PROTOKOLÜ</div>
                <p><strong>Satıcı:</strong> Halil Efendi (Yetkisiz Mümessil)</p>
                <p><strong>Alıcı:</strong> Doğu Madencilik T.A.Ş.</p>
                <div class="deed-warning">[ŞÖMİNEDE KISMEN YANMIŞ — GEÇERSİZDİR]</div>
            `;
            break;
        case 2132: // Gizli Çekmece Belgesi (Emine Hanım)
            content.classList.add('confidential-file');
            content.innerHTML = `
                <div class="police-file-header">ÇİFT KİMLİK VE TEHDİT DOSYASI</div>
                <div class="letter-body" style="font-family: 'Caveat', 'Kalam', cursive; font-size: 1.45rem;">
                    <p>• Halil dürbünle yamaç evlerini gözetliyor, şantaj kurguluyor.</p>
                    <p style="color: #047857;">"Dokuduğum en kalın urganı hazırladım. Köprüde karşıma çıkarsa o boğazı sıkar, intihar süsü veririm!" — Emine</p>
                </div>
            `;
            break;"""

if idx_start != -1 and idx_end != -1:
    new_text = text[:idx_start] + sisoren_html_cases + text[idx_end:]
    with open(file_path, 'w', encoding='utf-8') as f:
        f.write(new_text)
    print("REVERTED_AND_NATIVE_CLASSES_APPLIED")
else:
    print("INDEX_NOT_FOUND")

