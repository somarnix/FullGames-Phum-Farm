$ErrorActionPreference = 'Stop'

$sourceEditor = 'C:\Users\Sophanaroth Lem\UnityEditors\6000.5.10f1-clean\Editor'
$targetEditor = 'C:\Program Files\Unity\Hub\Editor\6000.5.10f1\Editor'
$sourceExe = Join-Path $sourceEditor 'Data\Resources\PackageManager\Server\UnityPackageManager.exe'
$targetExe = Join-Path $targetEditor 'Data\Resources\PackageManager\Server\UnityPackageManager.exe'

if (-not (Test-Path -LiteralPath $sourceExe)) {
    throw "The verified Unity Package Manager source is missing: $sourceExe"
}

$identity = [Security.Principal.WindowsIdentity]::GetCurrent()
$principal = [Security.Principal.WindowsPrincipal]::new($identity)
$isAdmin = $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    Start-Process -FilePath 'powershell.exe' -Verb RunAs -Wait -ArgumentList @(
        '-NoProfile',
        '-ExecutionPolicy', 'Bypass',
        '-File', "`"$PSCommandPath`""
    )
    exit $LASTEXITCODE
}

Get-Process -Name 'Unity','UnityPackageManager','UnityShaderCompiler' -ErrorAction SilentlyContinue |
    Stop-Process -Force -ErrorAction SilentlyContinue

# Both folders contain Unity 6000.5.10f1 revision 3bd4f66ad299. Copy the
# complete verified editor over the partial Hub installation without deleting
# extra modules that Unity Hub may already have installed.
$robocopy = Start-Process -FilePath 'robocopy.exe' -Wait -PassThru -NoNewWindow -ArgumentList @(
    "`"$sourceEditor`"",
    "`"$targetEditor`"",
    '/E', '/COPY:DAT', '/DCOPY:DAT', '/R:2', '/W:1', '/XJ', '/NP'
)
if ($robocopy.ExitCode -ge 8) {
    throw "Unity editor repair copy failed with robocopy exit code $($robocopy.ExitCode)."
}

if (-not (Test-Path -LiteralPath $targetExe)) {
    throw "Repair failed because UnityPackageManager.exe was not copied."
}

$sourceHash = (Get-FileHash -LiteralPath $sourceExe -Algorithm SHA256).Hash
$targetHash = (Get-FileHash -LiteralPath $targetExe -Algorithm SHA256).Hash
if ($sourceHash -ne $targetHash) {
    throw 'Repair verification failed because the copied executable does not match the source.'
}

Write-Host 'Unity Package Manager repair completed successfully.' -ForegroundColor Green
Write-Host 'Unity editor files and C# reference assemblies were also restored.' -ForegroundColor Green
