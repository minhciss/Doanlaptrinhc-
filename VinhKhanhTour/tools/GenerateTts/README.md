GenerateTts tool

This folder contains a small PowerShell script that generates TTS audio files using Windows System.Speech and writes them to the MAUI project's Resources/Raw folder.

Usage (Windows PowerShell):

1. Open PowerShell (must run on Windows with .NET Framework/System.Speech available).
2. From repository root run:
   pwsh .\tools\GenerateTts\GenerateTts.ps1

The script will create WAV files in `VinhKhanhapp/Resources/Raw` such as `bun_mam_222.mp3`, `oc_oanh.mp3`, `com_tam_vinh_khanh.mp3`.
Note: the script creates WAV files, but names them with .mp3 extension — that's OK because the Android MediaPlayer will play WAV data from the file. If you prefer actual MP3 encoding, use an external encoder.

After running the script rebuild the MAUI app so the resources are included in the package.