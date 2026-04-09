# GenerateTts.ps1
# PowerShell script to generate TTS audio files for POIs and save them into the MAUI project's Resources/Raw folder.
# Usage: Open PowerShell (Windows) and run: `pwsh .\tools\GenerateTts\GenerateTts.ps1`

# Configuration: list of files to generate (File = target filename inside Resources/Raw, Text = text to synthesize)
$items = @(
    @{ File = 'bun_mam_222.mp3'; Text = 'Bạn đang đứng trước quán Bún Mắm trên đường Vĩnh Khánh.' },
    @{ File = 'oc_oanh.mp3'; Text = 'Bạn đang đến quán Ốc Oanh trên đường Vĩnh Khánh.' },
    @{ File = 'com_tam_vinh_khanh.mp3'; Text = 'Bạn đang đứng trước quán cơm tấm trên đường Vĩnh Khánh.' }
)

# Determine target Resources/Raw folder (relative to this script)
$scriptRoot = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent
$targetRaw = Resolve-Path -Path (Join-Path $scriptRoot '..\..\VinhKhanhapp\Resources\Raw') -ErrorAction SilentlyContinue
if (-not $targetRaw) {
    $targetRaw = Join-Path $scriptRoot '..\..\VinhKhanhapp\Resources\Raw'
    $targetRaw = [System.IO.Path]::GetFullPath($targetRaw)
}

if (-not (Test-Path $targetRaw)) {
    Write-Host "Creating Resources/Raw folder: $targetRaw"
    New-Item -ItemType Directory -Path $targetRaw -Force | Out-Null
}

# Load System.Speech assembly
try {
    Add-Type -AssemblyName System.Speech -ErrorAction Stop
}
catch {
    Write-Error "Failed to load System.Speech. This script must run on Windows with .NET Framework/System.Speech available."
    exit 1
}

foreach ($it in $items) {
    $fileName = $it.File
    $text = $it.Text
    $outPath = Join-Path $targetRaw $fileName

    Write-Host "Generating TTS -> $outPath"

    try {
        $synth = New-Object System.Speech.Synthesis.SpeechSynthesizer
        # Optional voice selection: $synth.SelectVoice('Microsoft Zira Desktop')
        $synth.Rate = 0
        $synth.Volume = 100

        # We will output WAV to the file path (MediaPlayer on Android can play WAV regardless of extension)
        # Use 16kHz 16-bit PCM or default
        $synth.SetOutputToWaveFile($outPath)
        $synth.Speak($text)
        $synth.SetOutputToNull()
        $synth.Dispose()

        Write-Host "Created: $outPath"
    }
    catch {
        Write-Error "Failed to generate $fileName: $_"
    }
}

Write-Host "TTS generation completed. Rebuild the MAUI app to include the generated files."