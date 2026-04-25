#Requires -Version 5.1
<#
.SYNOPSIS
    Unity batch build (WebGL + Windows64) then Butler push to itch.io.

.DESCRIPTION
    Expects Unity 6000.3.12f1 (see ProjectSettings/ProjectVersion.txt) or set UNITY_EDITOR_PATH to Unity.exe.
    Pushes Builds/WebGL -> sprint53/sew-it-goes:html5 and Builds/Windows64 -> sprint53/sew-it-goes:windows.
    Butler must be logged in (butler login) or BUTLER_API_KEY set for non-interactive use.

.NOTES
    Game page: https://sprint53.itch.io/sew-it-goes
#>
$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $RepoRoot

$expectedUnityVersionFolder = "6000.3.12f1"
$Unity = $env:UNITY_EDITOR_PATH
if ([string]::IsNullOrWhiteSpace($Unity)) {
    $versionFolder = $expectedUnityVersionFolder
    $tryPaths = @(
        (Join-Path ${env:ProgramFiles} "Unity\Hub\Editor\$versionFolder\Editor\Unity.exe"),
        (Join-Path ${env:ProgramFiles(x86)} "Unity\Hub\Editor\$versionFolder\Editor\Unity.exe")
    )
    foreach ($p in $tryPaths) {
        if (Test-Path $p) {
            $Unity = $p
            break
        }
    }
}

if ([string]::IsNullOrWhiteSpace($Unity) -or -not (Test-Path -LiteralPath $Unity)) {
    Write-Error "Unity.exe not found. Install Unity Hub editor $expectedUnityVersionFolder or set UNITY_EDITOR_PATH to the full path of Unity.exe."
}

$buildsDir = Join-Path $RepoRoot "Builds"
New-Item -ItemType Directory -Force -Path $buildsDir | Out-Null
$logFile = Join-Path $buildsDir "unity-build.log"

$unityArgs = @(
    "-quit",
    "-batchmode",
    "-nographics",
    "-projectPath", $RepoRoot,
    "-executeMethod", "SewItGoes.Editor.ItchBuildCommand.BuildWebGLAndWindowsForItch",
    "-logFile", $logFile
)

Write-Host "Running Unity: $Unity"
$proc = Start-Process -FilePath $Unity -ArgumentList $unityArgs -Wait -PassThru -NoNewWindow
if ($proc.ExitCode -ne 0) {
    Write-Error "Unity build failed with exit code $($proc.ExitCode). See log: $logFile"
}

$butler = Get-Command butler -ErrorAction SilentlyContinue
if ($null -eq $butler) {
    $butlerFallback = "D:\itch\Apps\butler\butler.exe"
    if (Test-Path $butlerFallback) {
        $butlerExe = $butlerFallback
    }
    else {
        Write-Error "butler not found on PATH and not at $butlerFallback. Install from https://itch.io/docs/butler/"
    }
}
else {
    $butlerExe = $butler.Source
}

$itchTarget = "sprint53/sew-it-goes"
$webPath = Join-Path $buildsDir "WebGL"
$winPath = Join-Path $buildsDir "Windows64"

if (-not (Test-Path (Join-Path $webPath "index.html"))) {
    Write-Error "WebGL output missing index.html under $webPath"
}

Write-Host "Butler push WebGL -> ${itchTarget}:html5"
& $butlerExe push $webPath "${itchTarget}:html5" --if-changed

Write-Host "Butler push Windows64 -> ${itchTarget}:windows"
& $butlerExe push $winPath "${itchTarget}:windows" --if-changed

Write-Host "Done. Page: https://sprint53.itch.io/sew-it-goes"
