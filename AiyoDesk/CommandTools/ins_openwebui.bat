setlocal enabledelayedexpansion

set "CondaPath=%~1"
set "CondaEnv=%~2"

echo [�T��] �Ұ� Conda ���Ҥ�...
cd /d "%CondaPath%"
call activate.bat "%CondaEnv%"

python -m pip install open-webui