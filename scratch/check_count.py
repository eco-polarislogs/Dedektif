import psycopg2

conn = psycopg2.connect(host='localhost', port=5432, user='postgres', password='EcReN12854700456.', dbname='dedektiflik_rpg')
cur = conn.cursor()
cur.execute('SELECT count(*) FROM "NPCDialogues"')
print('POSTGRES_COUNT:' + str(cur.fetchone()[0]))
conn.close()
