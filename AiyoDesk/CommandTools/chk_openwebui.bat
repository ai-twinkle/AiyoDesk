@echo off
setlocal

REM ���o pip list ���O�_�� open-webui
pip list | findstr open-webui >nul

if %errorlevel%==0 (
    echo YES
) else (
    echo NO
)

endlocal