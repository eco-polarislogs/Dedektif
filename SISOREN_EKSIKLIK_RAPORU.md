# Sisören Eksiklik ve Uygulama Raporu

## Kapsam

Bu rapor, Sisören'in Gizemli Kasaba ve Gölge Şehir ile aynı oynanabilirlik seviyesine getirilmesi için yapılan düzenlemeleri ve kalan farkları özetler. Önceki iki kasabanın çalışan akışları korunmuş, değişiklikler Sisören kimliği ve Sisören NPC aralığı ile sınırlandırılmıştır.

## Tamamlanan düzenlemeler

- Sisören, Gizemli Kasaba ve Gölge Şehir tamamlanmadan dünya haritasından açılamaz.
- Sisören binalarının tamamı bir iç mekân görseline bağlandı; eksik binalar mevcut Sisören iç mekân setinden güvenli bir yedek görsel kullanır.
- İç mekân sahnesi konteyner genişliğine göre ölçeklenir. Böylece laptop ve masaüstü ekranlarında `100vw` taşması oluşmaz.
- Her binanın ana NPC'si artık iç mekân üzerinde konumlandırılmış, portresi ve konuşma düğmesiyle gösterilir.
- Ekstra ve çocuk NPC marker'ları korunmuştur.
- Ana NPC konuşmalarının arka planı portreye, sahne arka planı ise iç mekâna ayrıştırılmıştır.
- Sisören binalarına dört inceleme noktası oluşturuldu. Noktalar mevcut iç mekân görseli üzerinde çalışır ve NPC ile ilişkilendirilir.
- Her ana NPC'nin fallback soru havuzu 20 kayda tamamlandı. Arayüz her turda dört öneri gösterir.
- Sisören NPC'leri için soru limiti diğer kasabaların beş soruluk davranışından ayrıştırıldı ve 20 soruluk Sisören havuzunu destekler.
- Serbest yapay zekâ soruları artık Gölge Şehir endpoint'ine yanlışlıkla gitmez; `/api/sisoren/interrogate` üzerinden Sisören NPC bağlamıyla yerel AI motoruna ulaşır.
- Yerel AI için 201-213 aralığındaki tüm Sisören NPC'lerine rol ve olay sırrı fallback kayıtları eklendi.

## Kasaba karşılaştırması

| Alan | Gizemli Kasaba | Gölge Şehir | Sisören |
|---|---|---|---|
| Harita ilerlemesi | İlk vaka | İlk vakadan sonra açılır | Önceki iki vaka çözülünce açılır |
| Bina geçişi | Çalışan temel akış | Modüler bina/sahne akışı | Modüler bina, ana ve ekstra NPC marker'ları |
| Diyalog | NPC başına sınırlı soru | NPC başına beş soru | 20 kayıt, dört önerili turlar |
| AI yönlendirmesi | Genel endpoint | Gölge endpoint'i | Sisören'e özel endpoint |
| Adli delil | Genel adli tıp akışı | Kasabaya özel clue aralığı | Sisören noktaları ve NPC ilişkileri |
| Görsel uyumluluk | Mevcut eski asset seti | Daha tutarlı modüler set | Mevcut görsellerle 16:9 konteyner uyumu ve fallback |

## Kalan riskler

Sisören'in iç mekân görselleri hâlâ kare kaynak dosyalardan oluştuğu için sahne `contain` ile gösterilir; gerçek 16:9 üretimler geldiğinde yalnızca yapılandırmadaki asset yollarının değiştirilmesi yeterlidir. İnceleme noktaları şu aşamada olay örgüsünün görsel yerleşimini sağlar; yüksek özgüllükte ayrı delil görselleri ve kalıcı Sisören veritabanı tabloları sonraki içerik paketinde eklenebilir.

## Doğrulama

- Sisören JavaScript ve CSS değişiklikleri kapsamı Sisören sunumu ve Sisören NPC aralığıyla sınırlandırıldı.
- Eski kasabaların soru limiti ve AI endpoint seçimi korunmuştur.
- Derleme ve syntax kontrolleri değişikliklerden sonra yeniden çalıştırılmalıdır.
