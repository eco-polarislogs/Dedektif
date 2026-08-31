import psycopg2

conn = psycopg2.connect(host='localhost', port=5432, user='postgres', password='EcReN12854700456.', dbname='dedektiflik_rpg')
cur = conn.cursor()
cur.execute("SELECT table_name FROM information_schema.tables WHERE table_schema='public' ORDER BY table_name")
tabs = [r[0] for r in cur.fetchall()]
print("================= POSTGRESQL TUM TABLOLAR VE SATIR SAYILARI =================")
for t in tabs:
    cur.execute(f'SELECT count(*) FROM "{t}"')
    cnt = cur.fetchone()[0]
    print(f"  [TABLO] {t.ljust(25)} -> {cnt:,} kayit")
print("=============================================================================")
conn.close()
