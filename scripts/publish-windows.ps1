# Publish the Avalonia app as a self-contained Windows zip.
#
# Usage (PowerShell 7+):
#   .\scripts\publish-windows.ps1
#   $env:VERSION="1.2.0"; .\scripts\publish-windows.ps1
#   $env:RUNTIME="win-arm64"; .\scripts\publish-windows.ps1
#
# Output: artifacts\zip\PromptAssistant-<VERSION>-<RUNTIME>.zip
#
# The zip extracts to a folder containing PromptAssistant.Desktop.exe.
# No installer is produced — drag the folder anywhere and run the .exe.
# Tested on PowerShell 7.4+. Does NOT require PowerShell to be on the target machine.
[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# ─── Config ─────────────────────────────────────────────────────────────────
$AppName       = "Prompt Assistant"
$ExecutableName = "PromptAssistant.Desktop"
$Version       = if ($env:VERSION) { $env:VERSION } else { "1.0.0" }
$Runtime       = if ($env:RUNTIME) { $env:RUNTIME } else { "win-x64" }
$Project       = "src\PromptAssistant.Desktop\PromptAssistant.Desktop.csproj"

# ─── Paths ──────────────────────────────────────────────────────────────────
$Root        = (Resolve-Path "$PSScriptRoot\..").Path
$PublishDir  = "$Root\artifacts\publish\$Runtime"
$ZipOut      = "$Root\artifacts\zip"
$ZipPath     = "$ZipOut\PromptAssistant-$Version-$Runtime.zip"

Write-Host "▶ Cleaning previous artifacts"
if (Test-Path "$Root\artifacts") { Remove-Item "$Root\artifacts" -Recurse -Force }
New-Item -ItemType Directory -Force -Path $PublishDir | Out-Null
New-Item -ItemType Directory -Force -Path $ZipOut    | Out-Null

# ─── 1. Publish self-contained binary ───────────────────────────────────────
Write-Host "▶ Publishing $Runtime (self-contained)"
dotnet publish "$Root\$Project" `
    -c Release `
    -r $Runtime `
    --self-contained true `
    -p:PublishSingleFile=false `
    -p:UseAppHost=true `
    -p:Version=$Version `
    -o $PublishDir

if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed with exit code $LASTEXITCODE" }

# ─── 2. Zip the publish output ───────────────────────────────────────────────
Write-Host "▶ Creating zip"
Compress-Archive -Path "$PublishDir\*" -DestinationPath $ZipPath -Force

$ZipSize = "{0:N1} MB" -f ((Get-Item $ZipPath).Length / 1MB)
Write-Host ""
Write-Host "✓ Done"
Write-Host "  Zip:     $ZipPath"
Write-Host "  Size:    $ZipSize"
Write-Host "  Runtime: $Runtime"
Write-Host "  Version: $Version"
Write-Host ""
Write-Host "  Install: extract the zip, run $ExecutableName.exe"
Write-Host "  Note:    Windows SmartScreen may warn on first run — click 'More info' → 'Run anyway'."
Write-Host "           For silent-install distribution, sign the exe with a code-signing cert."
