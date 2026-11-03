with open(r'c:\Users\solma\OneDrive\Desktop\Dedektiflik\wwwroot\js\app.js', 'r', encoding='utf-8') as f:
    text = f.read()

# Eski case'leri resimli ve gerçekçi dokulu hale getirme
old_start = "        case 2011: // Şifreli Telgraf Şeridi"
old_end = "        case 2132: // Gizli Çekmece Belgesi (Emine Hanım)\n            content.classList.add('confidential-file', 'sisoren-doc');\n            content.innerHTML = `\n                <div class=\"police-file-header\" style=\"background:#1f2937;\">ÇİFT KİMLİK VE TEHDİT DOSYASI</div>\n                <div class=\"letter-body\" style=\"font-family: var(--font-typewriter); padding: 15px;\">\n                    <p>Gizli bölmede bulunan sahte nüfus cüzdanı ve el yazısı takip günlüğü:</p>\n                    <hr style=\"border-color: #374151; margin: 10px 0;\">\n                    <p><strong>Gerçek Adı:</strong> Halil Efendi (Eski Maden Sabıkalısı)</p>\n                    <p>• Dürbünle yamaç evlerini izliyor, özellikle Emine Hanım'ı yalnız yaşadığı için taciz edip şantaj kurguluyor.</p>\n                    <p style=\"color: #10b981; font-style: italic; margin-top: 10px;\">\"Dokuduğum en kalın urganı hazırladım. Köprüde karşıma çıkarsa o boğazı sıkar, intihar süsü veririm!\" — Emine'nin günlüğü.</p>\n                </div>\n            `;\n            break;"

new_cases = """        case 2011: // Şifreli Telgraf Şeridi
            content.classList.add('sisoren-realistic-doc-container');
            content.innerHTML = `
                <div class="realistic-doc-wrapper">
                    <img src="images/towns/sisoren/deliller/doc_2011.png" class="realistic-doc-image" alt="Şifreli Telgraf Şeridi">
                </div>
            `;
            break;
        case 2015: // Tehdit Mesajı Taslağı
            content.classList.add('sisoren-realistic-doc-container');
            content.innerHTML = `
                <div class="realistic-doc-wrapper">
                    <img src="images/towns/sisoren/deliller/doc_2015.png" class="realistic-doc-image" alt="Tehdit Mesajı Taslağı">
                </div>
            `;
            break;
        case 2023: // Yırtık Borç Listesi
            content.classList.add('sisoren-realistic-doc-container');
            content.innerHTML = `
                <div class="realistic-doc-wrapper">
                    <img src="images/towns/sisoren/deliller/doc_2023.png" class="realistic-doc-image realistic-wide" alt="Yırtık Borç Listesi">
                </div>
            `;
            break;
        case 2032: // Şantajlı Afiş
            content.classList.add('sisoren-realistic-doc-container');
            content.innerHTML = `
                <div class="realistic-doc-wrapper">
                    <img src="images/towns/sisoren/deliller/doc_2032.png" class="realistic-doc-image" alt="Şantajlı Film Afişi">
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
            content.classList.add('sisoren-realistic-doc-container');
            content.innerHTML = `
                <div class="realistic-doc-wrapper">
                    <img src="images/towns/sisoren/deliller/doc_2042.png" class="realistic-doc-image" alt="Yırtılmış Veresiye Sayfası">
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
            content.classList.add('sisoren-realistic-doc-container');
            content.innerHTML = `
                <div class="realistic-doc-wrapper">
                    <img src="images/towns/sisoren/deliller/doc_2051.png" class="realistic-doc-image" alt="İçi Oyulmuş Ansiklopedi ve Harita">
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
            content.classList.add('sisoren-realistic-doc-container');
            content.innerHTML = `
                <div class="realistic-doc-wrapper">
                    <img src="images/towns/sisoren/deliller/doc_2061.png" class="realistic-doc-image" alt="Asitle Silinmiş Kütük Defteri">
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
            content.classList.add('sisoren-realistic-doc-container');
            content.innerHTML = `
                <div class="realistic-doc-wrapper">
                    <img src="images/towns/sisoren/deliller/doc_2085.png" class="realistic-doc-image" alt="Saplı Bıçak Tehdit Notu">
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
            content.classList.add('sisoren-realistic-doc-container');
            content.innerHTML = `
                <div class="realistic-doc-wrapper">
                    <img src="images/towns/sisoren/deliller/doc_2104.png" class="realistic-doc-image" alt="Kanlı Tartım Fişi">
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
            content.classList.add('sisoren-realistic-doc-container');
            content.innerHTML = `
                <div class="realistic-doc-wrapper">
                    <img src="images/towns/sisoren/deliller/doc_2125.png" class="realistic-doc-image" alt="Şöminede Kısmen Yanmış Sahte Köy Satış Tapusu">
                </div>
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

pos_start = text.find(old_start)
pos_end = text.find(old_end)

if pos_start != -1 and pos_end != -1:
    end_of_block = pos_end + len(old_end)
    new_text = text[:pos_start] + new_cases + text[end_of_block:]
    with open(r'c:\Users\solma\OneDrive\Desktop\Dedektiflik\wwwroot\js\app.js', 'w', encoding='utf-8') as f:
        f.write(new_text)
    print("APP_JS_UPDATED_WITH_IMAGE_DOCS")
else:
    print("CANNOT_FIND_BLOCK", pos_start, pos_end)

