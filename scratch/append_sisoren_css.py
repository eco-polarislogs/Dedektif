css_to_append = """
/* ==============================================================
   SİSÖREN KASABASI OKUNABİLİR DELİL & BELGE TEMALARI
============================================================== */
.sisoren-theme .document-reader-content.sisoren-doc,
.document-reader-content.sisoren-doc {
    background: #0d1b17 url('/images/towns/sisoren/interiors/muhtarlik_interior.jpg') center/cover no-repeat !important;
    background-blend-mode: overlay !important;
    border: 3px solid #10b981 !important;
    box-shadow: 0 25px 70px rgba(0, 0, 0, 0.95), 0 0 30px rgba(16, 185, 129, 0.25) !important;
    color: #e6f4ea !important;
}

.sisoren-theme .document-reader-content.threat-letter.sisoren-doc {
    background: #13241e !important;
    border: 3px solid #059669 !important;
    color: #e2e8f0 !important;
}

.sisoren-theme .document-reader-content.threat-letter.sisoren-doc .letter-header {
    background: #064e3b !important;
    color: #34d399 !important;
    border-bottom: 2px solid #10b981 !important;
    font-weight: 800 !important;
    letter-spacing: 1px !important;
    padding: 10px !important;
    text-align: center !important;
}

.sisoren-theme .document-reader-content.confidential-file.sisoren-doc {
    background: #111e19 !important;
    border: 3px solid #047857 !important;
    color: #f1f5f9 !important;
}

.sisoren-theme .document-reader-content.title-deed.sisoren-doc {
    background: #1a2e26 !important;
    border: 3px solid #0d9488 !important;
    color: #f0fdf4 !important;
}

.sisoren-theme .document-reader-content.prescription.sisoren-doc {
    background: #142821 !important;
    border: 3px solid #059669 !important;
    color: #ecfdf5 !important;
}

.sisoren-theme .document-reader-content.secret-note.sisoren-doc {
    background: #0f221b !important;
    border: 2px dashed #10b981 !important;
    color: #f0fdf4 !important;
}

.sisoren-theme .document-reader-content.black-book.sisoren-doc .open-notebook-wrapper {
    background: #091310 !important;
    border: 2px solid #047857 !important;
    box-shadow: 0 15px 40px rgba(0, 0, 0, 0.9) !important;
}

.sisoren-theme .document-reader-content.black-book.sisoren-doc .notebook-page {
    background: #10231c !important;
    color: #e2e8f0 !important;
}

.sisoren-theme .document-reader-content.black-book.sisoren-doc .notebook-title {
    color: #34d399 !important;
    border-bottom: 2px solid #059669 !important;
}

.sisoren-theme .document-reader-content.black-book.sisoren-doc .ledger-name {
    color: #a7f3d0 !important;
}

.sisoren-theme .document-reader-content.black-book.sisoren-doc .ledger-val {
    color: #fcd34d !important;
}
"""

style_path = r'c:\Users\solma\OneDrive\Desktop\Dedektiflik\wwwroot\css\style.css'
with open(style_path, 'r', encoding='utf-8') as f:
    style_content = f.read()

if "SİSÖREN KASABASI OKUNABİLİR DELİL" not in style_content:
    with open(style_path, 'a', encoding='utf-8') as f:
        f.write("\n" + css_to_append)
    print("STYLE_APPENDED")
else:
    print("STYLE_ALREADY_EXISTS")

