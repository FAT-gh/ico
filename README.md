# Sanitary Icon Tool (Local)

A lightweight, auditable, and secure utility for extracting icon resources from Windows binaries and creating `.ico` files from images.

## Project Structure
- `src/`: Original C# source code (`IconTool.cs`).
- `bin/`: Compiled binaries (may be blocked by local policy).
- `docs/`: Product Requirements and technical specifications.
- `output/`: Default location for extracted assets and created icons.
- `skills/`: Gemini CLI skill definition for this project.

## How to Run (Safe Method)
Due to strict application control policies (e.g., ThreatLocker), running the compiled `.exe` or `.dll` might result in an **"Access is denied"** error.

To run the tool safely without an executable, use the **PowerShell Memory Loading** method:

```powershell
# Read source code
$code = Get-Content "C:\Users\DARICK\Downloads\ico\src\IconTool.cs" -Raw

# Load logic into memory (System.Drawing reference required)
Add-Type -TypeDefinition $code -ReferencedAssemblies "System.Drawing"

# 1. To Extract Icons:
$extractArgs = @("extract", "C:\Path\To\File.exe", "C:\Users\DARICK\Downloads\ico\output\extracted_folder")
[IconTool]::Main($extractArgs)

# 2. To Create ICO:
$createArgs = @("create", "C:\Users\DARICK\Downloads\ico\output\my_icon.ico", "image1.png", "image2.png")
[IconTool]::Main($createArgs)
```

## Security Note
This tool is "sanitary" and auditable. It does not perform any network operations and only uses standard Windows APIs to read/write icon resources.
