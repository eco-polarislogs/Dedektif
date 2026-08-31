using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace DedektiflikRPG.Services.AI;

/// <summary>
/// %100 Türkçeye Duyarlı, Sokak Ağzı, Devrik Cümle, Kök ve Anlamsal Konu Analiz Motoru v5.0.
/// Türkçedeki tüm özel harfleri (ş, ç, ı, ü, ö, ğ, İ, Ş, Ç, Ü, Ö, Ğ) hem orijinal hem esnek işler.
/// Devrik cümleleri, karmaşık soru kalıplarını ve cinayet dışı alakasız/genel konuları derinlemesine ayrıştırır.
/// </summary>
public static class TurkishTextEngine
{
    private static readonly CultureInfo _cultureTr = new CultureInfo("tr-TR");

    public static string NormalizeToAscii(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return "";
        string s = text.ToLower(_cultureTr);
        StringBuilder sb = new StringBuilder(s.Length);
        foreach (char c in s)
        {
            switch (c)
            {
                case 'ç': sb.Append('c'); break;
                case 'ğ': sb.Append('g'); break;
                case 'ı': sb.Append('i'); break;
                case 'i': sb.Append('i'); break;
                case 'ö': sb.Append('o'); break;
                case 'ş': sb.Append('s'); break;
                case 'ü': sb.Append('u'); break;
                default:
                    if (char.IsLetterOrDigit(c) || char.IsWhiteSpace(c))
                        sb.Append(c);
                    break;
            }
        }
        return sb.ToString();
    }

    public static string PreprocessSentence(string text)
    {
        string normalized = NormalizeToAscii(text);
        var tokens = normalized.Split(new[] { ' ', '.', ',', '?', '!', ';', ':', '-', '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
        var stopWords = new HashSet<string> { "mi", "mu", "mı", "mü", "miyim", "mısın", "musun", "müsün", "var", "yok", "bir", "ve", "ile", "için", "icin", "diye", "bu", "şu", "su", "da", "de", "ki", "işte", "iste", "yani", "ise", "ama", "fakat", "lakin" };

        List<string> processed = new List<string>();
        foreach (var t in tokens)
        {
            if (t.Length <= 2 && t != "ne") continue;
            if (stopWords.Contains(t)) continue;

            string stemmed = Stem(t);
            string mapped = MapSlang(stemmed);
            processed.Add(mapped);
        }
        return string.Join(" ", processed);
    }

    public static string Stem(string word)
    {
        if (word.Length <= 3) return word;

        // Türkçe Çekim ve Yapım Eklerini Temizleme
        string[] suffixes = {
            "yorsunuz", "yorsunuzdur", "yordunuz", "lardir", "lerdir", "lardan", "lerden", "larina", "lerine",
            "misiniz", "musunuz", "musunuzdur", "misinizdir", "mislersiniz", "diler", "dilar", "tilar", "tiler",
            "yorsun", "yordu", "miyor", "miyorlar", "miyor musun", "miyor musunuz", "miyor musun",
            "acak", "ecek", "iyor", "lar", "ler", "dan", "den", "tan", "ten", "nin", "nun", "nün", "nın",
            "yla", "yle", "siz", "suz", "süz", "sız", "sun", "sunuz", "siniz", "sin", "yim", "dik", "tik", "duk", "tuk",
            "di", "ti", "du", "tu", "yi", "ya", "ye", "in", "un", "ün", "ın", "im", "um", "üm", "ım", "miz", "muz", "müz", "mız"
        };

        foreach (var suffix in suffixes)
        {
            if (word.EndsWith(suffix) && word.Length - suffix.Length >= 3)
            {
                return word.Substring(0, word.Length - suffix.Length);
            }
        }

        if ((word.EndsWith("a") || word.EndsWith("e") || word.EndsWith("i") || word.EndsWith("u") || word.EndsWith("ü") || word.EndsWith("ı")) && word.Length >= 4)
        {
            return word.Substring(0, word.Length - 1);
        }

        return word;
    }

    public static string MapSlang(string word)
    {
        return word switch
        {
            "kanki" or "kral" or "abi" or "dayi" or "usta" or "aga" or "haci" or "bilader" or "sef" or "toprak" or "hocam" or "baskan" or "baskanim" or "kardes" or "kardesim" or "komutan" or "komutanim" => "amirim",
            "sikti" or "kesti" or "deldi" or "cizdi" or "vurdu" or "indirdi" or "desti" or "kiydi" or "gebertti" or "boctu" or "oldurdu" or "yapti" or "ett" or "bogdu" or "katletti" or "gecmis" => "oldur",
            "para" or "mangir" or "sakal" or "avanta" or "cukka" or "veresiye" or "alacak" or "senet" or "borcu" or "nakit" or "metelik" or "servet" => "borc",
            "cirkef" or "pislik" or "kavga" or "gurultu" or "dalas" or "husumet" or "itidal" or "anlasmazlik" or "dovus" or "niza" or "arbede" => "tartisma",
            "suphe" or "kusku" or "gizli" or "karanlik" or "supheli" or "zanli" => "suphe",
            "slm" or "s.a" or "sa" or "selamin" or "aleykum" or "selamlar" or "selam" or "selamun" or "hey" or "heyy" or "selamunaleykum" or "esselamu" or "aleykumselam" => "selam",
            "mrb" or "meraba" or "mrhb" or "merhaba" or "merhabalar" or "maraba" => "merhaba",
            "nbr" or "naber" or "naptin" or "napiyosun" or "napiyorsun" or "nabion" or "nasilsin" or "napiosun" or "nehaber" or "naberler" or "nasilsiniz" or "netiniz" => "nasilsin",
            "gunaydin" or "sabahlar" or "hayirli sabahlar" => "gunaydin",
            "iyi aksamlar" or "aksamlar" or "hayirli aksamlar" => "iyi aksamlar",
            "kolay gelsin" or "gelsin" or "bereketli" or "hayirli isler" or "rastgele" => "kolay gelsin",
            "kim" or "kimdir" or "katil" or "suclu" or "kimyapti" or "fail" or "canavar" or "zanli" => "kim",
            "neden" or "niye" or "nicin" or "sebep" or "nedendir" or "niyeki" or "ne diye" => "neden",
            "nasil" or "nasilki" or "ne sekilde" or "neyle" => "nasil",
            "ispat" or "kanit" or "delil" or "ispatla" or "kanitla" or "belge" or "tutanak" or "iz" => "kanit",
            _ => word
        };
    }

    public static bool ContainsAnyConcept(string rawTrLower, string normalizedAscii, params string[] concepts)
    {
        if (string.IsNullOrWhiteSpace(normalizedAscii)) return false;

        var inputTokens = normalizedAscii.Split(new[] { ' ', '.', ',', '?', '!', ';', ':', '-', '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
        string noSpaceInput = normalizedAscii.Replace(" ", "");

        // Köklenmiş token kümesi (Stemmed Token Set)
        var stemmedTokens = inputTokens.Select(Stem).Select(MapSlang).ToHashSet();

        foreach (var concept in concepts)
        {
            string conceptNorm = NormalizeToAscii(concept).Trim();
            if (string.IsNullOrEmpty(conceptNorm)) continue;

            // 1. TAM EŞLEŞME VEYA KELİME KÜMESİ İÇİNDE BULUNMA
            if (conceptNorm.Length <= 3)
            {
                if (inputTokens.Any(t => t == conceptNorm) || stemmedTokens.Any(s => s == conceptNorm))
                {
                    return true;
                }
                continue;
            }

            string noSpaceConcept = conceptNorm.Replace(" ", "");

            // 2. DOĞRUDAN ALT METİN / BİTİŞİK YAZIM KONTROLÜ
            if (rawTrLower.Contains(concept) || normalizedAscii.Contains(conceptNorm) || noSpaceInput.Contains(noSpaceConcept))
            {
                return true;
            }

            // 3. KÖK & SLANG EŞLEŞTİRMESİ
            var conceptTokens = conceptNorm.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var stemmedConceptTokens = conceptTokens.Select(Stem).Select(MapSlang).ToList();

            // Eğer konseptteki tüm kökler cümlede varsa eşleşir (Devrik cümle toleransı)
            if (stemmedConceptTokens.All(st => stemmedTokens.Contains(st) || inputTokens.Any(it => it.Contains(st))))
            {
                return true;
            }

            // 4. HARF HATASI & YAZIM YANLIŞI TOLERANSI (Levenshtein Fuzzy Matching)
            if (noSpaceConcept.Length >= 4)
            {
                // Cümledeki her token ile konsept tokenlerini kıyasla
                foreach (var inTok in inputTokens)
                {
                    if (inTok.Length < 3) continue;
                    foreach (var cTok in conceptTokens)
                    {
                        if (cTok.Length < 3) continue;
                        int maxTypos = (cTok.Length >= 6) ? 2 : 1;
                        if (Math.Abs(inTok.Length - cTok.Length) <= maxTypos)
                        {
                            int dist = LevenshteinDistance(inTok, cTok);
                            if (dist <= maxTypos) return true;
                        }
                    }
                }

                if (noSpaceConcept.Length >= 6)
                {
                    int maxOverallTypos = Math.Max(1, noSpaceConcept.Length / 4);
                    if (Math.Abs(noSpaceInput.Length - noSpaceConcept.Length) <= maxOverallTypos + 2)
                    {
                        int dist = LevenshteinDistance(noSpaceInput, noSpaceConcept);
                        if (dist <= maxOverallTypos) return true;
                    }
                }
            }
        }
        return false;
    }

    public static int LevenshteinDistance(string s, string t)
    {
        if (string.IsNullOrEmpty(s)) return string.IsNullOrEmpty(t) ? 0 : t.Length;
        if (string.IsNullOrEmpty(t)) return s.Length;

        int n = s.Length;
        int m = t.Length;
        int[,] d = new int[n + 1, m + 1];

        for (int i = 0; i <= n; d[i, 0] = i++) { }
        for (int j = 0; j <= m; d[0, j] = j++) { }

        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }
        return d[n, m];
    }
}
