using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;

class FrontendFixer
{
    public static void Fix()
    {
        string wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        if (!Directory.Exists(wwwrootPath)) return;
        
        var files = Directory.GetFiles(wwwrootPath, "*.*", SearchOption.AllDirectories)
                             .Where(f => f.EndsWith(".js") || f.EndsWith(".html") || f.EndsWith(".css"));
                             
        var replacements = new Dictionary<string, string>
        {
            {"Åž", "Ş"}, {"ÅŸ", "ş"}, {"Ã§", "ç"}, {"Ã‡", "Ç"},
            {"Ä±", "ı"}, {"Ä°", "İ"}, {"Ã¶", "ö"}, {"Ã–", "Ö"},
            {"Ã¼", "ü"}, {"Ãœ", "Ü"}, {"ÄŸ", "ğ"}, {"Äž", "Ğ"},
            {"Ã¢â€ ’", "→"}, {"Ã¢Å““", "✓"}
        };

        foreach (var file in files)
        {
            try
            {
                var content = File.ReadAllText(file, Encoding.UTF8);
                bool changed = false;
                foreach (var kvp in replacements)
                {
                    if (content.Contains(kvp.Key))
                    {
                        content = content.Replace(kvp.Key, kvp.Value);
                        changed = true;
                    }
                }
                if (changed)
                {
                    File.WriteAllText(file, content, new UTF8Encoding(true));
                    Console.WriteLine("C# Frontend Fixer: " + file);
                }
            }
            catch { }
        }
    }
}
