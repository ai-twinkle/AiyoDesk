param(
    [Parameter(Mandatory = $true)][string]$CondaPath
)

$ErrorActionPreference = "Stop"

function Download-File($url, $output) {
    Write-Host "�U��: $url"
    Invoke-WebRequest -Uri $url -OutFile $output -Headers @{ "User-Agent" = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36" }
}

function Install-Conda {
    $condaExe = Join-Path $CondaPath "Scripts\conda.exe"
    if (Test-Path $condaExe) {
        Write-Host "Conda �w�w�˩� $CondaPath"
        return
    }
    $pkgDir = "$PSScriptRoot\packages"
    New-Item -ItemType Directory -Path $pkgDir -Force | Out-Null
    $installerPath = "$pkgDir\Miniforge3-Windows-x86_64.exe"
    if (!(Test-Path $installerPath)) {
        Download-File "https://github.com/conda-forge/miniforge/releases/latest/download/Miniforge3-Windows-x86_64.exe" $installerPath
    }
    Write-Host "�}�l�w�� Miniforge �� $CondaPath..."
    Start-Process -FilePath $installerPath -ArgumentList "/InstallationType=JustMe", "/RegisterPython=0", "/S", "/D=$CondaPath" -Wait
}

function Create-Conda-Env {
    $condaExe = Join-Path $CondaPath "Scripts\conda.exe"
    Write-Host "�إ� Python 3.11 ����..."
    & $condaExe create -n aiyodesk python=3.11 -y
}

# Main
Install-Conda
Create-Conda-Env
