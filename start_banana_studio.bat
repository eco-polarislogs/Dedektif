@echo off
title Banana Studio - Dedektiflik AI Medya Atolyesi
echo ========================================================
echo   BANANA STUDIO BASLATILIYOR (AI Gorsel & Video)
echo ========================================================
cd /d "%~dp0tools\banana-studio"
echo Web Sunucusu baslatiliyor: http://localhost:5173
start http://localhost:5173
npm run dev
pause
