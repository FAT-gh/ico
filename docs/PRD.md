# Product Requirements Document: Sanitary Icon Extractor (Local)

## 1. Overview
A lightweight, transparent, and "sanitary" command-line utility to extract icon resources (including high-resolution 256x256 PNG icons) from Windows executables (`.exe`) and libraries (`.dll`). This is intended as a safe alternative to third-party tools like BeCyIconGrabber.

## 2. Core Functionality
- **Extraction:** Accept a path to a Windows PE file (.exe, .dll, .ocx, etc.) and extract all icon groups.
- **High-Res Support:** Specifically identify and extract 256x256 icons (stored as PNGs).
- **Creation (Assembly):** Combine multiple image files (PNG/BMP) of varying sizes (e.g., 16x16, 32x32, 48x48, 256x256) into a single, standard Windows `.ico` file.
- **Deconstruction:** Extract individual image layers from an existing `.ico` file.
- **Output:** Save results to a local directory.
- **Zero-Dependency:** Build using standard Windows components (C# and the `csc` compiler).

## 3. Technical Implementation Strategy
### 3.1 Key Win32 APIs (Extraction)
... (existing APIs) ...

### 3.2 ICO File Structure (Creation)
To create an `.ico` file, the tool will manually construct the ICONDIR and ICONDIRENTRY structures:
- **ICONDIR:** 6 bytes (Reserved, Type=1 for Icon, Count).
- **ICONDIRENTRY:** 16 bytes per image (Width, Height, ColorCount, Reserved, Planes, BitCount, BytesInRes, ImageOffset).
- **Data Block:** The raw image data (PNG header for 256x256, or DIB for smaller sizes).

### 3.2 Icon Logic
- Icons in Windows are stored in **Groups**.
- Smaller icons (16x16, 32x32) use the DIB (Device Independent Bitmap) format.
- 256x256 icons (introduced in Windows Vista) are raw PNG files embedded directly in the resource stream.

## 4. Implementation References
- **Win32 Resource Management:** [Microsoft Docs - Resources](https://learn.microsoft.com/en-us/windows/win32/menurc/resources)
- **Icon Format Specification:** [Microsoft Docs - Icons](https://learn.microsoft.com/en-us/previous-versions/ms997538(v=msdn.10))
- **C# P/Invoke Signatures:** [pinvoke.net](https://www.pinvoke.net/)

## 5. Build & Execution (Local)
The app is designed to be compiled using the built-in C# compiler (`csc.exe`) found in every Windows installation.

### Step 1: Create the Source Code
Create a file named `IconExtractor.cs` with the provided implementation.

### Step 2: Compile
Run the following in PowerShell:
```powershell
Add-Type -AssemblyName System.Drawing
csc.exe /out:IconExtractor.exe /target:exe /reference:System.Drawing.dll IconExtractor.cs
```

### Step 3: Use
```powershell
.\IconExtractor.exe "C:\Path\To\Your\File.exe"
```

## 6. Security Considerations
- **No Execution:** By using `LOAD_LIBRARY_AS_DATAFILE`, we ensure the target EXE/DLL is never "run," only read as data.
- **Auditable Code:** The entire source is less than 200 lines of standard C#, allowing for easy manual review.
- **Local Scope:** All operations are performed within the user's Downloads folder.
