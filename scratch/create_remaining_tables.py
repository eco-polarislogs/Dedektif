# -*- coding: utf-8 -*-
import psycopg2

def create_remaining_tables():
    pg_host = "localhost"
    pg_port = 5432
    pg_user = "postgres"
    pg_pass = "EcReN12854700456."
    target_db = "dedektiflik_rpg"

    conn = psycopg2.connect(host=pg_host, port=pg_port, user=pg_user, password=pg_pass, dbname=target_db)
    conn.autocommit = True
    cur = conn.cursor()

    print("Creating SceneObjects, NPCRelationships, and ScenarioHints in PostgreSQL...")
    cur.execute("""
    CREATE TABLE IF NOT EXISTS "SceneObjects" (
        "ObjectId" SERIAL PRIMARY KEY,
        "NPCId" INTEGER NOT NULL,
        "ObjectName" VARCHAR(150) NOT NULL,
        "Description" TEXT NOT NULL,
        "ImageFile" VARCHAR(255) DEFAULT '',
        "PosTop" VARCHAR(20) DEFAULT '50%',
        "PosLeft" VARCHAR(20) DEFAULT '50%',
        "IsDiscovered" BOOLEAN DEFAULT FALSE
    );

    CREATE TABLE IF NOT EXISTS "NPCRelationships" (
        "RelationId" SERIAL PRIMARY KEY,
        "NPC1Id" INTEGER NOT NULL,
        "NPC2Id" INTEGER NOT NULL,
        "RelationType" VARCHAR(50) NOT NULL,
        "Description" TEXT NOT NULL
    );

    CREATE TABLE IF NOT EXISTS "ScenarioHints" (
        "HintId" SERIAL PRIMARY KEY,
        "GuiltyNPCId" INTEGER NOT NULL,
        "HintText" TEXT NOT NULL,
        "HintType" VARCHAR(50) NOT NULL,
        "RevealOrder" INTEGER DEFAULT 1
    );
    """)

    # Seed SceneObjects if empty
    cur.execute('SELECT count(*) FROM "SceneObjects"')
    if cur.fetchone()[0] == 0:
        cur.execute("""
        INSERT INTO "SceneObjects" ("NPCId", "ObjectName", "Description", "ImageFile", "PosTop", "PosLeft") VALUES
        (1, 'Kanlı Satır', 'Tezgahta duran, üzerinde yeni kurumuş kan lekeleri olan ağır bir kasap satırı. İncelemeye değer.', 'images/cleaver.png', '40%', '30%'),
        (1, 'Kara Defter', 'Hasan''ın veresiye defteri. Kurbanın isminin üzeri kırmızı kalemle defalarca çizilmiş.', 'images/butcher_ledger.png', '60%', '65%'),
        (1, 'Yırtık Önlük', 'Kancaya asılı, cebinde paslı bir anahtar olan lekeli kasap önlüğü. Kavga izi var gibi.', 'images/torn_apron.png', '25%', '70%'),
        (2, 'Zehirli Şişe', 'Tezgah altında gizlenmiş, etiketi sökülmüş koyu renkli cam şişe. Zehirli sarmaşık özü içeriyor.', 'images/poison_bottle.png', '65%', '40%'),
        (2, 'Reçete Defteri', 'Son sayfaları yırtılmış eski bir reçete defteri. Yırtılan sayfalar cinayet gecesine ait.', 'images/prescription_book.png', '35%', '60%'),
        (2, 'Kırık Cam Parçası', 'Yerde bulunan, üzerinde bilinmeyen kimyasal kalıntıları olan tüp parçası.', 'images/broken_glass.png', '80%', '25%'),
        (3, 'Tehdit Mektubu', 'Çekmecede bulunan, kurbana yazılmış ama gönderilmemiş öfkeli bir mektup.', 'images/threat_letter.png', '30%', '45%'),
        (3, 'Kırık Gözlük', 'Masanın altında bulunan, bir kavgada kırılmış gibi duran okuma gözlüğü.', 'images/broken_glasses.png', '50%', '70%'),
        (3, 'Gizli Kasa', 'Tablonun arkasında şifresi açık unutulmuş para dolu kasa. İçinde sahte belgeler de var.', 'images/hidden_safe.png', '80%', '30%'),
        (4, 'Polis Rozeti', 'Olay yerinde bulunan, numarası kazınmış bir polis rozeti. Kime ait olduğu belirsiz.', 'images/police_badge.png', '45%', '25%'),
        (4, 'Gizli Dosya', 'Komiser Güneş''in masasında kilitli çekmecede bulunan "GİZLİ" damgalı bir dosya.', 'images/evidence_file.png', '35%', '65%'),
        (4, 'Kayıp Düğme', 'Olay yerinden toplanan, pahalı bir paltonun kopmuş düğmesi.', 'images/missing_button.png', '70%', '45%'),
        (5, 'Kanlı İplik Makarası', 'Tezgahın altında bulunan, üzerinde kurumuş kan lekeleri olan iplik makarası.', 'images/thread_spool.png', '55%', '30%'),
        (5, 'Yırtık Kumaş', 'Atölyede bulunan, kurbanın ceketinden kopmuş olabilecek kumaş parçası.', 'images/torn_fabric.png', '40%', '75%'),
        (5, 'Gizli Cep', 'Yahya''nın son diktiği ceketin astarında gizli bir cep.', 'images/hidden_pocket.png', '75%', '50%');
        """)
        print("[OK] Seeded 15 SceneObjects.")

    print("\nALL POSTGRESQL SCHEMAS AND DATA SYNCHRONIZED PERFECTLY!")
    cur.close()
    conn.close()

if __name__ == "__main__":
    create_remaining_tables()
