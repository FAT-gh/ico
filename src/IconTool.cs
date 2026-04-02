using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Linq;

public class IconTool
{
    #region Win32 API
    private const uint LOAD_LIBRARY_AS_DATAFILE = 0x00000002;
    private const uint LOAD_LIBRARY_AS_IMAGE_RESOURCE = 0x00000020;
    private const int RT_ICON = 3;
    private const int RT_GROUP_ICON = 14;

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, uint dwFlags);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool FreeLibrary(IntPtr hModule);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr FindResource(IntPtr hModule, IntPtr lpName, IntPtr lpType);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr LoadResource(IntPtr hModule, IntPtr hResInfo);

    [DllImport("kernel32.dll")]
    private static extern IntPtr LockResource(IntPtr hResData);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern uint SizeofResource(IntPtr hModule, IntPtr hResInfo);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool EnumResourceNames(IntPtr hModule, IntPtr lpszType, EnumResNameProc lpEnumFunc, IntPtr lParam);

    private delegate bool EnumResNameProc(IntPtr hModule, IntPtr lpszType, IntPtr lpszName, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct GRPICONDIR {
        public ushort idReserved;
        public ushort idType;
        public ushort idCount;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct GRPICONDIRENTRY {
        public byte bWidth;
        public byte bHeight;
        public byte bColorCount;
        public byte bReserved;
        public ushort wPlanes;
        public ushort wBitCount;
        public uint dwBytesInRes;
        public ushort nID;
    }
    #endregion

    public static void Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  Extract: IconTool.exe extract <file.exe/dll> [output_dir]");
            Console.WriteLine("  Create:  IconTool.exe create <output.ico> <image1.png> [image2.png ...]");
            return;
        }

        string mode = args[0].ToLower();

        try
        {
            if (mode == "extract" && args.Length >= 2)
            {
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string outputDir = args.Length > 2 ? args[2] : "extracted_" + timestamp;
                Extract(args[1], outputDir);
            }
            else if (mode == "create" && args.Length >= 3)
            {
                Create(args[1], args.Skip(2).ToArray());
            }
            else
            {
                Console.WriteLine("Invalid arguments.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }

    private static void Extract(string filePath, string outputDir)
    {
        if (!File.Exists(filePath)) throw new FileNotFoundException("File not found", filePath);
        if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);

        IntPtr hModule = LoadLibraryEx(filePath, IntPtr.Zero, LOAD_LIBRARY_AS_DATAFILE | LOAD_LIBRARY_AS_IMAGE_RESOURCE);
        if (hModule == IntPtr.Zero) throw new Exception("Failed to load file.");

        try
        {
            List<IntPtr> groupNames = new List<IntPtr>();
            EnumResourceNames(hModule, (IntPtr)RT_GROUP_ICON, (mod, type, name, param) => {
                groupNames.Add(name);
                return true;
            }, IntPtr.Zero);

            int groupIdx = 0;
            foreach (var groupName in groupNames)
            {
                IntPtr hResInfo = FindResource(hModule, groupName, (IntPtr)RT_GROUP_ICON);
                IntPtr hResData = LoadResource(hModule, hResInfo);
                IntPtr pResData = LockResource(hResData);

                GRPICONDIR dir = (GRPICONDIR)Marshal.PtrToStructure(pResData, typeof(GRPICONDIR));
                IntPtr pEntry = pResData + Marshal.SizeOf(typeof(GRPICONDIR));

                for (int i = 0; i < dir.idCount; i++)
                {
                    GRPICONDIRENTRY entry = (GRPICONDIRENTRY)Marshal.PtrToStructure(pEntry, typeof(GRPICONDIRENTRY));
                    pEntry += Marshal.SizeOf(typeof(GRPICONDIRENTRY));

                    IntPtr hIconResInfo = FindResource(hModule, (IntPtr)entry.nID, (IntPtr)RT_ICON);
                    uint size = SizeofResource(hModule, hIconResInfo);
                    IntPtr hIconResData = LoadResource(hModule, hIconResInfo);
                    IntPtr pIconData = LockResource(hIconResData);

                    byte[] data = new byte[size];
                    Marshal.Copy(pIconData, data, 0, (int)size);

                    string ext = IsPng(data) ? ".png" : ".bmp";
                    string name = groupName.ToInt64() < 0xFFFF ? groupName.ToString() : "icon";
                    string outPath = Path.Combine(outputDir, String.Format("group{0}_{1}_{2}x{3}_{4}{5}", groupIdx, name, entry.bWidth, entry.bHeight, i, ext));
                    
                    File.WriteAllBytes(outPath, data);
                    Console.WriteLine("Extracted: " + outPath);
                }
                groupIdx++;
            }
        }
        finally
        {
            FreeLibrary(hModule);
        }
    }

    private static void Create(string outputPath, string[] imagePaths)
    {
        using (FileStream fs = new FileStream(outputPath, FileMode.Create))
        using (BinaryWriter bw = new BinaryWriter(fs))
        {
            // ICONDIR
            bw.Write((ushort)0); // Reserved
            bw.Write((ushort)1); // Type (1 = Icon)
            bw.Write((ushort)imagePaths.Length); // Count

            int offset = 6 + (imagePaths.Length * 16);
            List<byte[]> imageDataList = new List<byte[]>();

            foreach (string path in imagePaths)
            {
                byte[] data = File.ReadAllBytes(path);
                imageDataList.Add(data);

                using (Image img = Image.FromFile(path))
                {
                    bw.Write((byte)(img.Width >= 256 ? 0 : img.Width));
                    bw.Write((byte)(img.Height >= 256 ? 0 : img.Height));
                    bw.Write((byte)0); // Color count
                    bw.Write((byte)0); // Reserved
                    bw.Write((ushort)1); // Planes
                    bw.Write((ushort)32); // Bit count (assumed)
                    bw.Write((uint)data.Length);
                    bw.Write((uint)offset);
                    offset += data.Length;
                }
            }

            foreach (byte[] data in imageDataList)
            {
                bw.Write(data);
            }
        }
        Console.WriteLine("Created ICO: " + outputPath);
    }

    private static bool IsPng(byte[] data)
    {
        if (data.Length < 8) return false;
        return data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47;
    }
}
