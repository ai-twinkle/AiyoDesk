param(
    [Parameter(Mandatory = $true)][string]$OutputDir
)

# ���� GPU �t��
function Get-GpuVendor {
    $gpu = Get-WmiObject Win32_VideoController | Select-Object -First 1 -ExpandProperty AdapterCompatibility
    if ($gpu -match 'NVIDIA') { return 'nvidia' }
    if ($gpu -match 'AMD')    { return 'amd' }
    if ($gpu -match 'Intel')  { return 'intel' }
    return 'cpu'
}

$vendor = Get-GpuVendor
Write-Host "������ GPU �t�ӡG$vendor"

# �w�˹������� PyTorch
switch ($vendor) {
    'nvidia' {
        Write-Host "�w�ˤ䴩 CUDA �� PyTorch..."
        pip install torch torchvision torchaudio --index-url https://download.pytorch.org/whl/cu121
    }
    'amd' {
        Write-Host "�w�ˤ䴩 AMD �� PyTorch..."
        pip install torch torchvision torchaudio --index-url https://download.pytorch.org/whl/rocm6.0
    }
    'intel' {
        pip install torch torchvision torchaudio --index-url https://download.pytorch.org/whl/xpu
    }
    default {
        Write-Host "�w�� CPU-only PyTorch..."
        pip install torch torchvision torchaudio --index-url https://download.pytorch.org/whl/cpu
    }
}

# �U���ø��� ComfyUI
$zipUrl    = 'https://github.com/comfyanonymous/ComfyUI/archive/refs/heads/master.zip'
$outputZip = "$PSScriptRoot\ComfyUI-main.zip"
$targetDir = "$PSScriptRoot\ComfyUI"

Write-Host "�U�� ComfyUI ��l�X ZIP..."
Invoke-WebRequest -Uri $zipUrl -OutFile $outputZip -UseBasicParsing -Verbose

Write-Host "�����Y ComfyUI �� $OutputDir"
New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
Expand-Archive -LiteralPath $outputZip -DestinationPath $OutputDir -Force

# ���� requirements.txt ���n�w��
$extracted = Join-Path $OutputDir 'ComfyUI-master'

# 3. �w�� requirements.txt
$reqFile = Join-Path $extracted 'requirements.txt'
if (Test-Path $reqFile) {
    Write-Host "`n�}�l���� pip install -r requirements.txt ..." -ForegroundColor Cyan
    Push-Location $extracted
    pip install -r $reqFile
    Pop-Location
    Write-Host "`n�Ҧ� Python �̮ۨM��w�w�˧����C" -ForegroundColor Green
}
else {
    Write-Host "`nĵ�i�G�䤣�� requirements.txt�A���ˬd ComfyUI �����C" -ForegroundColor Yellow
}

Write-Host "ComfyUI �w�˦��\"
