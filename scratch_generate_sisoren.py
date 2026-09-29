import json
import random

npcs = {
    201: {"Name": "Telgrafçı Rüstem", "Tone": "adult", "Profession": "Telgrafçı"},
    202: {"Name": "Kahveci İrfan", "Tone": "adult", "Profession": "Kahveci"},
    203: {"Name": "Sinemacı Nejat", "Tone": "adult", "Profession": "Sinemacı"},
    204: {"Name": "Bakkal Cemile", "Tone": "adult", "Profession": "Bakkal"},
    205: {"Name": "Sahaf Hikmet", "Tone": "adult", "Profession": "Sahaf"},
    206: {"Name": "Muhtar Meliha", "Tone": "adult", "Profession": "Muhtar"},
    207: {"Name": "Tütüncü Nermin", "Tone": "adult", "Profession": "Tütüncü"},
    208: {"Name": "Çoban Durmuş", "Tone": "adult", "Profession": "Çoban"},
    209: {"Name": "Tüpçü Şevket", "Tone": "adult", "Profession": "Tüpçü"},
    210: {"Name": "Hurdacı Zehra", "Tone": "adult", "Profession": "Hurdacı"},
    211: {"Name": "Zeynep Teyze", "Tone": "senior", "Profession": "Emekli"},
    212: {"Name": "Hatice Nine", "Tone": "senior", "Profession": "Emekli"},
    213: {"Name": "Emine Hanım", "Tone": "adult", "Profession": "Ev Hanımı"},
    301: {"Name": "Celal Amca", "Tone": "senior", "Profession": "Emekli"},
    302: {"Name": "Hamdi Dayı", "Tone": "senior", "Profession": "Emekli"},
    303: {"Name": "Kahveci Çırağı Salih", "Tone": "young", "Profession": "Çırak"},
    304: {"Name": "Şerife Teyze", "Tone": "senior", "Profession": "Emekli"},
    305: {"Name": "Oduncu Çırağı Cemal", "Tone": "young", "Profession": "Çırak"},
    306: {"Name": "Postacı Nuri Efendi", "Tone": "adult", "Profession": "Postacı"},
    307: {"Name": "Telgraf Çırağı Yusuf", "Tone": "young", "Profession": "Çırak"},
    308: {"Name": "Biletçi Fatma", "Tone": "adult", "Profession": "Gişe Memuru"},
    309: {"Name": "Kâtip Sami Efendi", "Tone": "adult", "Profession": "Kâtip"},
    310: {"Name": "Tütün Tiryakisi Osman", "Tone": "adult", "Profession": "Müşteri"},
    311: {"Name": "Hurda Toplayan Ali", "Tone": "child", "Profession": "Çocuk"},
    312: {"Name": "Tüp Dağıtıcısı Mehmet", "Tone": "adult", "Profession": "Dağıtıcı"},
    313: {"Name": "Küçük Ayşe", "Tone": "child", "Profession": "Çocuk"},
    314: {"Name": "Gece Bekçisi Recep", "Tone": "senior", "Profession": "Bekçi"},
    315: {"Name": "Küçük Elif", "Tone": "child", "Profession": "Çocuk"},
    316: {"Name": "Can", "Tone": "child", "Profession": "Çocuk"},
    317: {"Name": "Selin", "Tone": "child", "Profession": "Çocuk"},
    318: {"Name": "Kerem", "Tone": "child", "Profession": "Çocuk"}
}

questions = [
    "Cinayet gecesi binanda kim vardı?",
    "O gece saat iki civarında ne gördün?",
    "Bu olayla ilgili sakladığın belge ya da eşya nedir?",
    "Başka hangi kasabalı bu ayrıntıyı doğrulayabilir?"
]

categories = ["tanisma", "derinlesme", "yuzlestirme", "baski", "son"]

def get_base_response(tone, prof, stage, is_guilty):
    if tone == "child":
        if is_guilty:
            return [
                "Ben bir şey yapmadım! Sadece karanlıkta bir adamın koştuğunu gördüm...",
                "O gece uyuyordum, yemin ederim! Ama dışarıdan bir tıkırtı gelmişti...",
                "Beni korkutuyorsunuz! O gölge... O gölge bana çok yakındı!",
                "Söyleyemem... Söylersem beni de bulurlar."
            ]
        else:
            return [
                "O gece sokakta oyun oynuyordum, sonra büyük bir gürültü duydum.",
                "Sis çok yoğundu amirim, sadece bir ışık gördüm.",
                "Annem o saatte dışarı çıkmamı yasaklamıştı ama camdan baktım...",
                "Büyükler hep bir şeyler saklıyor. O gece biri arka kapıdan çıktı."
            ]
    elif tone == "young":
        if is_guilty:
            return [
                "Ben sadece çırağım! Neden bana soruyorsunuz ki? Orada değildim!",
                "O saatte çalışıyordum. Yani... Sanırım çalışıyordum. Çok karanlıktı.",
                "Ustam bana bir şey söylemememi tembihledi. Lütfen beni sıkıştırmayın.",
                "Benim olayla ilgim yok! Belki de o paketi taşımamalıydım..."
            ]
        else:
            return [
                "O saatte ortalık çok sessizdi. Arka sokaktan ayak sesleri geldiğini duydum.",
                "Ustama sorsanız daha iyi olur, ben o gece depoyu düzenliyordum.",
                "Sisten dolayı kimseyi net göremedim ama uzun boylu biriydi.",
                "Dikkatimi çeken tek şey, köşedeki lambanın o gece sönük olmasıydı."
            ]
    elif tone == "senior":
        if is_guilty:
            return [
                "Bu yaştan sonra başımı belaya sokmamı mı bekliyorsunuz? Benim haberim yok.",
                "Benim gözlerim artık iyi görmüyor. Zaten o gece çok yorgundum.",
                "Geçmişte çok şey gördüm amirim, ama bu olanlar beni aşar. Sormayın.",
                "Eski defterleri açmanın kimseye faydası olmaz. Benden uzak durun."
            ]
        else:
            return [
                "Evladım, bu kasabanın sırları bitmez. O gece eski maden yolundan bir ses geldi.",
                "Gece uyku tutmadı, pencereden bakarken telaşlı birini gördüm.",
                "Rahmetli kocam da böyle sisli gecelerden korkardı. Sis iyi şeyler getirmez.",
                "Buralarda saat ikiden sonra sadece tekinsiz insanlar dolaşır amirim."
            ]
    else: # adult
        if is_guilty:
            return [
                "İşim gücüm var amirim. O saatte dükkanda yalnızdım, kimseyi görmedim.",
                "Sorguladığınız kişi ben olmamalıyım. Neden hep suçlanan ben oluyorum?",
                "Bu dükkanda yasadışı hiçbir şey olmaz! Gördüğünüz izler başka birine aittir.",
                "Size anlatacak bir şeyim yok. İsterseniz avukatımı çağırın."
            ]
        else:
            return [
                f"{prof} olarak kasabanın nabzını tutarım. O gece olağandışı bir sessizlik vardı.",
                "Ben o saatte hesapları kontrol ediyordum. Dışarıdan bağırışma sesleri duydum.",
                "Müşterilerimden biri o gece çok gergindi, adını veremem ama telaşlıydı.",
                "Benim dükkanımdan o tarafı görmek zor, ama kepenklerin kapandığını duydum."
            ]

dialogues = []

for npc_id, data in npcs.items():
    name = data["Name"]
    tone = data["Tone"]
    prof = data["Profession"]
    
    for stage, category in enumerate(categories):
        for button, question in enumerate(questions):
            
            innocent_pool = get_base_response(tone, prof, stage, False)
            guilty_pool = get_base_response(tone, prof, stage, True)
            
            innocent_text = f"{name}: {random.choice(innocent_pool)}"
            guilty_text = f"{name}: {random.choice(guilty_pool)}"
            
            dialogues.append({
                "NPCId": npc_id,
                "QuestionText": question,
                "ResponseText": innocent_text,
                "GuiltyResponseText": guilty_text,
                "Stage": stage,
                "ButtonIndex": button,
                "Difficulty": min(5, stage + 1),
                "Category": category
            })

with open("Data/sisoren_dialogues.json", "w", encoding="utf-8") as f:
    json.dump(dialogues, f, ensure_ascii=False, indent=2)
print(f"Generated {len(dialogues)} dialogues to Data/sisoren_dialogues.json")
