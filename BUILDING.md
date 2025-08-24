AjoibotBio — Building and Sharing the App

This document shows how to build the app locally and how to create distributable artifacts you can share with others.

Prerequisites
- .NET 6 SDK installed
- Windows 10/11
- Optional: 7-Zip installed (for ZIP packaging from script), or use built-in Compress-Archive
- WebView2 Evergreen Runtime installed on target machines (the app checks for it). If not installed, run MicrosoftEdgeWebView2RuntimeInstallerX64/X86.exe.

Project overview
- Main app: AjoibotBio (WPF, net6.0-windows)
- Device libraries: ZKFingerprint, ZKFaceId
- Native fingerprint DLLs are automatically copied into RID-specific folders in the output (runtimes\win-x64\native and runtimes\win-x86\native) via .csproj rules.
- Logging uses log4net.config (copied to output).

Quick build (Debug)
1. Open a terminal in repository root.
2. Run: dotnet build AjoibotBio\AjoibotBio.csproj -c Debug
3. Launch: AjoibotBio\bin\Debug\net6.0-windows\AjoibotBio.exe

Release build (for local run)
1. Run: dotnet build AjoibotBio\AjoibotBio.csproj -c Release
2. Output: AjoibotBio\bin\Release\net6.0-windows

Publish (recommended for sharing)
Publishing collects required files into a clean folder. Choose the architecture that matches the target PCs.

Option A: Scripts (recommended)
- Publish both x64 and x86:
  powershell -ExecutionPolicy Bypass -File .\scripts\publish.ps1
- Package into ZIP files:
  powershell -ExecutionPolicy Bypass -File .\scripts\package.ps1

After running these, you will get folders and ZIP files under publish\win-x64 and publish\win-x86.

Option B: Manual commands
- x64 (framework-dependent):
  dotnet publish AjoibotBio\AjoibotBio.csproj -c Release -r win-x64 -o publish\win-x64 --self-contained false
- x86 (framework-dependent):
  dotnet publish AjoibotBio\AjoibotBio.csproj -c Release -r win-x86 -o publish\win-x86 --self-contained false

Notes:
- Framework-dependent means the target PC needs .NET 6 Desktop Runtime. You can switch to self-contained by adding --self-contained true (larger download), but WebView2 runtime still needs to be installed separately.
- The project already copies:
  - log4net.config
  - Assets (ReadMe.txt, zkdriver.exe, release reg edit.reg, and logo)
  - Native ZK fingerprint DLLs per architecture

What to share
- Share the ZIP created in publish\win-x64 or publish\win-x86 depending on the target machine.
- Ask recipients to:
  1) Unzip to a folder with write permission (logs will be created in logs\main.log).
  2) Ensure WebView2 runtime is installed (the app will log an error if missing).
  3) If using the fingerprint device, install drivers (Assets\zkdriver.exe).
  4) Run AjoibotBio.exe.

Troubleshooting
- Missing WebView2: The app logs an error at startup. Install the Evergreen runtime from Microsoft.
- Fingerprint device errors: Check logs\main.log for detailed messages about device connect/disconnect and capture.
- Log file missing: Ensure the app has permission to create the logs folder next to the EXE.

Advanced (self-contained publish)
- x64 self-contained:
  dotnet publish AjoibotBio\AjoibotBio.csproj -c Release -r win-x64 -o publish\win-x64-sc --self-contained true
- x86 self-contained:
  dotnet publish AjoibotBio\AjoibotBio.csproj -c Release -r win-x86 -o publish\win-x86-sc --self-contained true

If you need single-file, you can add: /p:PublishSingleFile=true /p:IncludeAllContentForSelfExtract=true. Note that WPF apps may unzip at first run, and native device DLLs remain as separate files.

CI integration
You can automate these commands in CI (e.g., GitHub Actions) to produce release artifacts when tagging a release.
