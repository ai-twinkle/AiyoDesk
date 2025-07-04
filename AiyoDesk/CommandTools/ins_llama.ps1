param(
    [Parameter(Mandatory = $true)][string]$Backend,
    [Parameter(Mandatory = $true)][string]$OutputDir
)

$ErrorActionPreference = "Stop"

function Download-File($url, $output) {
    Write-Host "�U��: $url"
    Invoke-WebRequest -Uri $url -OutFile $output -Headers @{ "User-Agent" = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36" }
}

function Download-LLAMACPP {
    $release = Invoke-RestMethod 'https://api.github.com/repos/ggml-org/llama.cpp/releases/latest'
    $pattern = "llama-.*-bin-win-$Backend-x64\.zip"
    $assets = $release.assets
    $asset = $assets | Where-Object { $_.name -match $pattern } | Sort-Object name | Select-Object -Last 1

    if (-not $asset) {
        throw "�䤣��ŦX��� '$Backend' �� llama.cpp ���Y�� (�Ҧ�: $pattern)"
    }

    $zipPath = "$PSScriptRoot\llama.zip"
    Download-File $asset.browser_download_url $zipPath

    Write-Host "�����Y llama.cpp �� $OutputDir"
    New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
    Expand-Archive -LiteralPath $zipPath -DestinationPath $OutputDir -Force
}

# Main
Write-Host "======== �Ұ� llama.cpp �w�ˬy�{ ========"
Download-LLAMACPP
Write-Host "======== �w�˧��� ========"
