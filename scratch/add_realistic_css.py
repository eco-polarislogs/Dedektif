css_addition = """
/* ==============================================================
   SİSÖREN GERÇEKÇİ RESİMLİ BELGE / KAĞIT GÖRÜNÜMÜ
============================================================== */
.document-reader-content.sisoren-realistic-doc-container {
    background: transparent !important;
    border: none !important;
    outline: none !important;
    box-shadow: none !important;
    padding: 0 !important;
    display: flex !important;
    justify-content: center !important;
    align-items: center !important;
    max-width: 900px !important;
    width: 95% !important;
}

.realistic-doc-wrapper {
    position: relative;
    display: flex;
    justify-content: center;
    align-items: center;
    width: 100%;
    animation: fadeInDoc 0.3s ease-out;
}

.realistic-doc-image {
    max-width: 580px;
    width: 100%;
    height: auto;
    max-height: 85vh;
    object-fit: contain;
    border-radius: 4px;
    box-shadow: 0 25px 70px rgba(0, 0, 0, 0.95), 0 0 35px rgba(0, 0, 0, 0.6);
    filter: drop-shadow(0 15px 30px rgba(0, 0, 0, 0.8));
    transition: transform 0.2s ease;
}

.realistic-doc-image.realistic-wide {
    max-width: 820px;
}

@keyframes fadeInDoc {
    from {
        opacity: 0;
        transform: scale(0.95) translateY(10px);
    }
    to {
        opacity: 1;
        transform: scale(1) translateY(0);
    }
}
"""

style_path = r'c:\Users\solma\OneDrive\Desktop\Dedektiflik\wwwroot\css\style.css'
with open(style_path, 'r', encoding='utf-8') as f:
    content = f.read()

if "SİSÖREN GERÇEKÇİ RESİMLİ BELGE" not in content:
    with open(style_path, 'a', encoding='utf-8') as f:
        f.write("\n" + css_addition)
    print("REALISTIC_CSS_ADDED")
else:
    print("ALREADY_EXISTS")

