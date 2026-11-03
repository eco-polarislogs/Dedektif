import re

sisoren_cases = """        case 2011: // Şifreli Telgraf Şeridi
            content.classList.add('threat-letter', 'sisoren-doc');
            content.innerHTML = `
                <div class="letter-header">T.C. POSTA VE TELGRAF İDARESİ — SİSÖREN HATTI</div>
                <div class="letter-body" style="font-family: var(--font-typewriter); padding: 12px;">
                    <p><strong>ALICI:</strong> [SİLİNMİŞ] | <strong>KOD:</strong> #TEL-882-SIS</p>
                    <hr style="border-color: #2ea44f; margin: 10px 0;">
                    <p style="font-family: monospace; letter-spacing: 2px;">--. . -.-. . / -.-- .- .-. .. ... .. / --. --- .-.. -.. .</p>
                    <p style="color: #10b981; font-weight: bold; margin-top: 10px;">[ÇÖZÜLMÜŞ MORS METNİ]:</p>
                    <p>"...Halil Efendi şantaj defterini maden yolundaki eski vagona saklamış. Gece yarısı gölde buluşup telleri keseceğiz, jandarmaya haber uçuramaz..."</p>
                    <div class="letter-signature" style="margin-top: 15px; font-style: italic;">- Rüstem</div>
                </div>
            `;
            break;
        case 2015: // Tehdit Mesajı Taslağı
            content.classList.add('threat-letter', 'sisoren-doc');
            content.innerHTML = `
                <div class="letter-header">KARALAMA KAĞIDI / İSİMSİZ NOT</div>
                <div class="letter-body" style="font-family: var(--font-typewriter); padding: 12px;">
                    <p>Halil Efendi,</p>
                    <p>Telgraf tellerinden geçen her sır benim elimden geçer. Madendeki kaçak sevkiyatları ve madenci ailelerinden çaldığın paraları bildiğimi sanma, kesin olarak biliyorum.</p>
                    <p>Ya bu akşam telgrafhaneye 50.000 Lira bırakırsın ya da yarın şafakta vilayet savcılığına resmi telgrafı çekerim. Ecelin bir telgraf şeridine bakar!</p>
                </div>
            `;
            break;
        case 2023: // Yırtık Borç Listesi
            content.classList.add('black-book', 'sisoren-doc');
            content.innerHTML = `
                <div class="open-notebook-wrapper">
                    <div class="notebook-spine"></div>
                    <div class="notebook-page left-page">
                        <div class="notebook-title">SİSÖREN KAHVESİ HESAPLARI</div>
                        <div class="notebook-date">Güz Dönemi Veresiyeleri</div>
                        <div class="ledger-row">
                            <span class="ledger-name">Çoban Durmuş:</span>
                            <span class="ledger-val">120 TL</span>
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
                            <span class="ledger-name">Tüpçü Şevket:</span>
                            <span class="ledger-val">310 TL</span>
                        </div>
                    </div>
                    <div class="notebook-page right-page">
                        <div class="notebook-title">İRFAN'IN EL NOTLARI</div>
                        <div class="handwritten-note">• Halil kahvehanemi ipotek ettirip ocağımı söndürmek istiyor. Üstelik her gün gelip yüzüme sırıtıyor.</div>
                        <div class="handwritten-note">• Sabrım bitti. Altıpatları temizledim. Bu borç kanla kapanacak.</div>
                        <div class="notebook-stamp" style="border-color:#10b981; color:#10b981;">KAHVEHANE HACİZLİ</div>
                    </div>
                </div>
            `;
            break;
        case 2032: // Şantajlı Afiş
            content.classList.add('confidential-file', 'sisoren-doc');
            content.innerHTML = `
                <div class="police-file-header" style="background:#0f766e;">ESKİ FİLM AFİŞİ ARKA YÜZÜ</div>
                <div class="letter-body" style="font-family: var(--font-typewriter); padding: 15px;">
                    <p style="font-size: 1.1rem; color: #f59e0b; font-weight: bold;">"GECENİN GÖLGELERİ" — VİZYON 1949</p>
                    <hr style="border-color: #0d9488; margin: 10px 0;">
                    <p>Afişin arkasına aceleyle mor mürekkeple yazılmış el yazısı:</p>
                    <blockquote style="background: rgba(0,0,0,0.3); border-left: 3px solid #14b8a6; padding: 10px; margin: 10px 0; font-style: italic;">
                        "Nejat, projeksiyon odasındaki kaçak film kutularında ne sakladığını Halil Efendi gördü. Sisin içinde yaptıkların o 35 milimetre makaralara kaydedildi. Ya sus payını getir ya da makaralar savcının masasında döner."
                    </blockquote>
                    <p style="color: #ef4444; font-weight: bold; margin-top: 10px;">[Nejat'ın notu]: Bu gece son seans... Perde kapanınca Halil'in de perdesi inecek!</p>
                </div>
            `;
            break;
        case 2033: // Düşmüş Bilet Koçanı
            content.classList.add('threat-letter', 'sisoren-doc');
            content.innerHTML = `
                <div class="letter-header">SİSÖREN BELEDİYE SİNEMASI GİRİŞ BİLETİ</div>
                <div class="letter-body" style="font-family: var(--font-typewriter); padding: 12px;">
                    <p><strong>SEANS:</strong> Gece Yarısı Özel Gösterimi (01:30)</p>
                    <p><strong>SALON:</strong> Balkon / Arka Sıra No: 13</p>
                    <hr style="border-color: #2ea44f; margin: 10px 0;">
                    <p>Biletin yırtık kenarında kurşun kalemle düşülmüş not:</p>
                    <p style="color: #34d399; font-style: italic;">"Saat 02:00'de makine dairesinde ol. Kimseye görünme. Halil tek başına gelecek."</p>
                </div>
            `;
            break;
        case 2042: // Kopuk Veresiye Sayfası
            content.classList.add('prescription', 'sisoren-doc');
            content.innerHTML = `
                <div class="prescription-header">
                    <h3 style="color:#0f766e;">SİSÖREN DAĞ BAKKALI — YIRTILMIŞ VERESİYE SAYFASI</h3>
                    <p>Tarih: 18 Ekim | Kayıt No: #412</p>
                </div>
                <div class="prescription-body">
                    <p><strong>Müşteri:</strong> Halil Efendi (Maden Müteahhidi)</p>
                    <hr>
                    <p>• 5 Kilo Çay, 10 Kilo Şeker, Gazyağı: 450 Lira (Ödenmedi)</p>
                    <p>• Dükkân İpoteği ve Faiz Borcu: 40.000 Lira</p>
                    <p class="torn-edge-text" style="color:#ef4444; font-weight:bold;">
                        "Cemile, bu borcu ödeyemezsen haftaya bakkalı devralıyorum, seni de sokakta bırakırım." — Halil Efendi'nin tehdit notu.
                    </p>
                    <p style="color:#10b981; font-style:italic; margin-top:8px;">[Altına Cemile'nin el yazısıyla]: Kilerdeki fare zehri bu akşam peynire karışacak!</p>
                </div>
            `;
            break;
        case 2045: // İsimli Senet
            content.classList.add('title-deed', 'sisoren-doc');
            content.innerHTML = `
                <div class="deed-header">T.C. SİSÖREN NOTERLİĞİ TASDİKLİ BORÇ SENEDİ</div>
                <div class="deed-title">REHİN VE MÜLKİYET DEVRİ SENEDİ</div>
                <p><strong>Borçlu:</strong> Bakkal Cemile Hanım</p>
                <p><strong>Alacaklı:</strong> Madenci Halil Efendi</p>
                <p><strong>Tutar:</strong> 40.000 Türk Lirası</p>
                <p><strong>Şart:</strong> Vadesinde ödenmediğinde Sisören Dağ Bakkalı'nın tüm tapu hakları bila kaydü şart Halil Efendi'ye devrolunacaktır.</p>
                <div class="deed-warning" style="border-color:#ef4444; color:#ef4444;">[BORÇ VADESİ DOLMUŞTUR — HACİZ KARARI BEKLENİYOR]</div>
            `;
            break;
        case 2051: // İçi Oyulmuş Ansiklopedi
            content.classList.add('confidential-file', 'sisoren-doc');
            content.innerHTML = `
                <div class="police-file-header" style="background:#1e3a5f;">OYULMUŞ CİLT: BÜYÜK OSMANLI TARİHİ (CİLT IV)</div>
                <div class="letter-body" style="font-family: var(--font-typewriter); padding: 15px;">
                    <p>Kitabın 340-410 sayfaları arası jiletle muntazam oyulmuş, içine sararmış bir el yazması harita sıkıştırılmış:</p>
                    <hr style="border-color: #2ea44f; margin: 10px 0;">
                    <p style="color: #f59e0b; font-weight: bold;">"SİSÖREN ESKİ MADEN GALERİSİ & KAYIP ALTIN SAKLANTISI"</p>
                    <p style="font-style: italic;">• Kuzey kuyusu 4. galeride demir sandık gizlidir. Halil haritayı ele geçirmek için sahafı basacağını söyledi. Bu kutsal mirası o haramiye teslim etmeyeceğim!</p>
                    <div class="letter-signature" style="margin-top:10px;">- Sahaf Hikmet</div>
                </div>
            `;
            break;
        case 2052: // Şifreli Kenar Notları
            content.classList.add('secret-note', 'sisoren-doc');
            content.innerHTML = `
                <div class="letter-header" style="font-weight:bold; color:#0f766e; border-bottom:1px solid #0f766e; padding-bottom:5px; margin-bottom:10px;">SARARMIŞ KİTAP KENARI EL YAZISI</div>
                <p style="line-height: 1.8;">"Kardeşim maktulün gözü doymuyor. Dün gece dükkânı gözetledi. 12. sayfa 4. satır: <em>Ölüm sisin içinde yürüyenlere yakındır</em>. Saat 02:00 maden girişi kilitli olacak."</p>
                <div class="secret-note-sig" style="color:#10b981;">- H.</div>
            `;
            break;
        case 2055: // Sahte Vasiyetname
            content.classList.add('title-deed', 'sisoren-doc');
            content.innerHTML = `
                <div class="deed-header">VERASET VE İNTİKAL VASİYETNAMESİ (MÜSVEDDE)</div>
                <div class="deed-title">VASİYET VE MÜLK TAKSİMİ</div>
                <p><strong>Miras Bırakan:</strong> Halil Efendi</p>
                <p><strong>Lehdar:</strong> Sahaf Hikmet (Tüm nadir el yazmaları ve altın haritaları)</p>
                <p style="color:#b91c1c; font-style:italic;">İmza bölümü kurbanın el yazısıyla açıkça taklit edilmiş, mürekkep kurumadan üzerine parmak izi bulaşmıştır.</p>
                <div class="deed-warning" style="border-color:#b91c1c; color:#b91c1c;">[SAHTE İMZA — İTİRAZLI EVRAK]</div>
            `;
            break;
        case 2061: // Asitle Silinmiş Kütük
            content.classList.add('confidential-file', 'sisoren-doc');
            content.innerHTML = `
                <div class="police-file-header" style="background:#065f46;">SİSÖREN KÖYÜ ESKİ NÜFUS KÜTÜK DEFTERİ #12</div>
                <div class="letter-body" style="font-family: var(--font-typewriter); padding: 15px;">
                    <p><strong>Sayfa:</strong> 184 — Hane No: 29</p>
                    <hr style="border-color: #047857; margin: 10px 0;">
                    <p style="background: #111; color: #94a3b8; padding: 10px; font-family: monospace;">
                        Madenci Halil Efendi... [ASİTLE KİMYASAL OLARAK SİLİNMİŞ VE DELİNMİŞ ALAN]<br>
                        Mirasçılar:... [SİLİNMİŞ]<br>
                        Köy Ortak Mera Arazisi Payı: 840 Dönüm — Muhtarlık tüzel kişiliğine intikal ettirilmiştir.
                    </p>
                    <p style="color: #ef4444; font-weight: bold; margin-top: 10px;">[Not]: Halil bu tahrifatı tespit etmişti ve Muhtar Meliha'yı rüşvet ve sahtecilikten içeri attırmak üzereydi.</p>
                </div>
            `;
            break;
        case 2063: // Gizli Orman Tapusu
            content.classList.add('title-deed', 'sisoren-doc');
            content.innerHTML = `
                <div class="deed-header">T.C. TAPU VE KADASTRO DAĞ BÖLGE MÜDÜRLÜĞÜ</div>
                <div class="deed-title">YAYLA VE ORMAN ÖZEL MÜLKİYET SENEDİ</div>
                <p><strong>Parsel:</strong> Sisören Çamtepe Mevkii No: 712</p>
                <p><strong>Hak Sahibi:</strong> Meliha Hanım (Muhtar)</p>
                <p><strong>Önceki Maliki:</strong> Köy Tüzel Kişiliği / Halil Efendi Hissesi</p>
                <div class="deed-warning" style="border-color:#10b981; color:#10b981;">[RESMİ MÜHÜRLE ONAYLANMIŞ — ŞÜPHELİ DEVİR]</div>
            `;
            break;
        case 2065: // Parçalanmış Uyarı Mektubu
            content.classList.add('threat-letter', 'sisoren-doc');
            content.innerHTML = `
                <div class="letter-header">VİLAYET MÜFETTİŞLİĞİ ÖN İNCELEME İHBARI</div>
                <div class="letter-body" style="font-family: var(--font-typewriter); padding: 12px;">
                    <p>Sisören Köyü Muhtarlığına,</p>
                    <p>Köy sakinlerinden Halil Efendi'nin başvurusu üzerine, köy meralarının kanunsuz devri ve şahsi zimmet iddiaları hakkında idari soruşturma başlatılmıştır. Müfettiş heyeti yarın sabah intikal edecektir.</p>
                    <p style="color: #ef4444; font-weight: bold; margin-top: 10px;">Meliha Hanım'ın kenara yazdığı not: "Müfettiş gelse de dinleyecek bir Halil bulamayacak!"</p>
                </div>
            `;
            break;
        case 2072: // Şifreli Sigara Kâğıdı
            content.classList.add('secret-note', 'sisoren-doc');
            content.innerHTML = `
                <div class="letter-header" style="font-weight:bold; color:#0f766e; border-bottom:1px solid #0f766e; padding-bottom:5px; margin-bottom:10px;">İNCE TÜTÜN SARMA KAĞIDI (FİLTRE İÇİ NOTU)</div>
                <p style="font-family: monospace; font-size: 0.95rem; line-height: 1.6;">
                    "Nermin Hanım, limandan gelen kaçak Virginia tütünleri dağ deposunda. Halil Efendi bu gece jandarmayı yollayacak. Eğer 20.000 Lira vermezsek depoyu ateşe verdirir. Bu adamı sisin içinde susturmak zorundayız."
                </p>
                <div class="secret-note-sig" style="color:#10b981;">- Dağ Kuryesi</div>
            `;
            break;
        case 2085: // Saplı Bıçak Notu
            content.classList.add('threat-letter', 'sisoren-doc');
            content.innerHTML = `
                <div class="letter-header">DİRSEK TAHTASINA SAPLANMIŞ KANLI KAĞIT</div>
                <div class="letter-body" style="font-family: var(--font-typewriter); padding: 12px;">
                    <p style="font-size: 1.2rem; color: #b91c1c; font-weight: bold;">HALİL!</p>
                    <p>Sürümün içtiği dereye kimyasal maden atığı dökmeyi bırakacaksın dedik, dinlemedin. Üç gebe koyunum zehirlenip can verdi.</p>
                    <p>Hayvanlarımın canı senin kirli canından daha kıymetli. Dağ yolunda karşıma çıkarsan o çiviyi kalbine çakarım!</p>
                    <div class="letter-signature" style="color:#0f766e;">Çoban Durmuş</div>
                </div>
            `;
            break;
        case 2093: // Sahte Teslimat Kaydı
            content.classList.add('confidential-file', 'sisoren-doc');
            content.innerHTML = `
                <div class="police-file-header" style="background:#1e293b;">SİSÖREN AYGAZ BAYİİ TESLİMAT JURNALİ</div>
                <div class="letter-body" style="font-family: var(--font-typewriter); padding: 15px;">
                    <p><strong>Tarih:</strong> 24 Ekim Gece | <strong>Plaka:</strong> 06 SIS 44</p>
                    <hr style="border-color: #334155; margin: 10px 0;">
                    <p>• 23:45 — Madenci Halil Efendi Konutu (1 Adet Sanayi Tüpü)</p>
                    <p style="color: #f59e0b; font-style: italic;">[Not]: Teslimatı bizzat Şevket yaptı. Teslim fişinde alıcı imzası bulunmuyor!</p>
                    <p style="color: #ef4444; font-weight: bold; margin-top: 10px;">"Valfi bilerek gevşettim... Bir kibrit çaktığında her şey kül olacak!" — Defterin sonuna düşülmüş karalama.</p>
                </div>
            `;
            break;
        case 2104: // Kanlı Tartım Fişi
            content.classList.add('threat-letter', 'sisoren-doc');
            content.innerHTML = `
                <div class="letter-header">SİSÖREN HURDA VE KANTAR İŞLETMESİ MAKBUZU</div>
                <div class="letter-body" style="font-family: var(--font-typewriter); padding: 12px;">
                    <p><strong>Fiş No:</strong> #8921 | <strong>Tartım Saati:</strong> 02:18 (Gece)</p>
                    <p><strong>Malzeme:</strong> 420 Kg Çelik Maden Rayı ve Ağır Hurda</p>
                    <hr style="border-color: #2ea44f; margin: 10px 0;">
                    <p>Fişin kenarında kurumuş koyu kırmızı kan lekesi ve arkasında kurşun kalemle not:</p>
                    <p style="color: #ef4444; font-style: italic;">"Halil pres makinesinin yanına geldi, parayı istedi. Presin kapağı kapandı... Artık hurdalardan farkı kalmadı."</p>
                    <div class="letter-signature">Hurdacı Zehra</div>
                </div>
            `;
            break;
        case 2111: // Yırtık Mektup (Zeynep Teyze)
            content.classList.add('threat-letter', 'sisoren-doc');
            content.innerHTML = `
                <div class="letter-header">ESKİ YIRTIK MEKTUP PARÇASI</div>
                <div class="letter-body" style="font-family: var(--font-typewriter); padding: 12px;">
                    <p>Zeynep Hanım,</p>
                    <p>Yıllar önce kaybettiğin o oğlanın aslında kimin yüzünden öldüğünü biliyorsun. Bu sırrı saklamamın bir bedeli var. Bu gece son altın bileziği de pencerenin pervazına koyacaksın.</p>
                    <p>Yoksa tüm Sisören senin geçmişini öğrenir.</p>
                    <div class="letter-signature">Halil</div>
                </div>
            `;
            break;
        case 2121: // Eski Fotoğraf Albümü
            content.classList.add('confidential-file', 'sisoren-doc');
            content.innerHTML = `
                <div class="police-file-header" style="background:#374151;">HATİCE NİNE'NİN ESKİ DAĞ ALBÜMÜ</div>
                <div class="letter-body" style="font-family: var(--font-typewriter); padding: 15px;">
                    <p>1952 yılına ait sararmış siyah-beyaz bir fotoğrafın arkasında yazılı el yazısı:</p>
                    <hr style="border-color: #4b5563; margin: 10px 0;">
                    <p style="font-style: italic; color: #f3f4f6;">
                        "Sisören Dağları bizim yurdumuzdur. Halil Efendi bu toprakları ecnebi şirketlere peşkeş çekmeye kalkarsa, dağın kara mantarları kaynar, eceli bir bardak çayda bulur."
                    </p>
                    <p style="color: #10b981; font-weight: bold; margin-top: 10px;">[Hatice Nine'nin notu]: Topraklarımızı kimseye vermeyiz oğul, gerekirse zehir oluruz.</p>
                </div>
            `;
            break;
        case 2125: // Sahte Arazi Tapusu (Maden Şirketi Satışı)
            content.classList.add('title-deed', 'sisoren-doc');
            content.innerHTML = `
                <div class="deed-header">ARAZİ SATIŞ VE MÜLKİYET FERAGAT SENEDİ</div>
                <div class="deed-title">SİSÖREN DAĞ KÖYÜ TOPLU SATIŞ PROTOKOLÜ</div>
                <p><strong>Satıcı Temsilcisi:</strong> Halil Efendi (Yetkisiz Vekil)</p>
                <p><strong>Alıcı:</strong> Doğu Madencilik ve Taşocağı Anonim Şirketi</p>
                <p><strong>Mevkii:</strong> Sisören Yaylası ve 40 Adet Köy Parseli</p>
                <div class="deed-warning" style="border-color:#ef4444; color:#ef4444;">[ŞÖMİNEDE KISMEN YANMIŞ — GEÇERSİZ VE SAHTE İMZALI]</div>
            `;
            break;
        case 2132: // Gizli Çekmece Belgesi (Emine Hanım)
            content.classList.add('confidential-file', 'sisoren-doc');
            content.innerHTML = `
                <div class="police-file-header" style="background:#1f2937;">ÇİFT KİMLİK VE TEHDİT DOSYASI</div>
                <div class="letter-body" style="font-family: var(--font-typewriter); padding: 15px;">
                    <p>Gizli bölmede bulunan sahte nüfus cüzdanı ve el yazısı takip günlüğü:</p>
                    <hr style="border-color: #374151; margin: 10px 0;">
                    <p><strong>Gerçek Adı:</strong> Halil Efendi (Eski Maden Sabıkalısı)</p>
                    <p>• Dürbünle yamaç evlerini izliyor, özellikle Emine Hanım'ı yalnız yaşadığı için taciz edip şantaj kurguluyor.</p>
                    <p style="color: #10b981; font-style: italic; margin-top: 10px;">"Dokuduğum en kalın urganı hazırladım. Köprüde karşıma çıkarsa o boğazı sıkar, intihar süsü veririm!" — Emine'nin günlüğü.</p>
                </div>
            `;
            break;"""

file_path = r'c:\Users\solma\OneDrive\Desktop\Dedektiflik\wwwroot\js\app.js'
with open(file_path, 'r', encoding='utf-8') as f:
    text = f.read()

target = """        case 1074: // Altı Çizili Eski Kitap
            content.classList.add('confidential-file');
            content.innerHTML = `
                <div class="police-file-header" style="background:#5b21b6;">"GECENİN SIRLARI" ROMAN SAYFASI</div>
                <div class="letter-body" style="font-family: var(--font-typewriter); padding: 15px;">
                    <p style="font-style: italic; line-height: 1.8;">"...ve saat tam 02:14'ü gösterdiğinde, fırtınanın ve gök gürültüsünün ortasında, göl kenarındaki o eski evden boğuk bir çığlık duyuldu. Katil, parmak uçlarında yürüyerek gecenin karanlığına karıştı..."</p>
                    <hr>
                    <p style="color:#f59e0b; font-weight:bold; margin-top: 15px;">[Fehmi Bey'in kurşun kalemle düştüğü el yazısı not]:</p>
                    <p style="font-style: italic; color:#fef3c7;">"O gece tam saat 02:14'te penceremin hemen altından geçen o sert çizme seslerini duydum. Adımlar göl kenarındaki deri atölyesine doğru ilerliyordu."</p>
                </div>
            `;
            break;"""

if target in text:
    new_text = text.replace(target, target + '\n' + sisoren_cases, 1)
    with open(file_path, 'w', encoding='utf-8') as f:
        f.write(new_text)
    print("SUCCESS")
else:
    print("TARGET_NOT_FOUND")

