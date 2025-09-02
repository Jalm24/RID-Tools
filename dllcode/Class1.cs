using Harmony;
using UnityEngine;
using UnityEditor;
using UnityEditor.Modules;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using System.Linq;

using Debug = UnityEngine.Debug;

public static class N3DSToolchain
{

    private static string Quote(string path) {return "\"" + path + "\"";}
    // public static void UnpackCCI(string pathTo3DS, string pathToExtractFolder)
    // {
    //     string ctrtoolPath = Path.Combine("Assets/Editor", "ctrtool.exe");

    //     if (!File.Exists(pathTo3DS)) {
    //         Debug.LogError("CCI file missing after CreateCCI: "+pathTo3DS);
    //     } else {
    //         Debug.Log("CCI file exists: "+pathTo3DS);
    //     }


    //     Directory.CreateDirectory(pathToExtractFolder);

    //     string exefsDir = Path.Combine(pathToExtractFolder, "exefs");
    //     string romfsDir = Path.Combine(pathToExtractFolder, "romfs");
    //     string exheaderFile = Path.Combine(pathToExtractFolder, "exheader.bin");
    //     string logoFile = Path.Combine(pathToExtractFolder, "logo.bin");
    //     string plainFile = Path.Combine(pathToExtractFolder, "plain.bin");
    //     string ncchFile = Path.Combine(pathToExtractFolder, "partition0.ncch");

    //     // Step 1: Extract raw NCCH partition from CCI (save to ncchFile)
    //     //ExtractNcchRaw(ctrtoolPath, pathTo3DS, ncchFile); // skip it

    //     // Step 2: Extract contents from the extracted NCCH file
    //     RunCtrTool(ctrtoolPath,
    //         "-v " +  // verbose output
    //         "--exefsdir=" + Quote(exefsDir) + " " +
    //         "--romfsdir=" + Quote(romfsDir) + " " +
    //         "--exheader=" + Quote(exheaderFile) + " " +
    //         "--logo=" + Quote(logoFile) + " " +
    //         "--plainrgn=" + Quote(plainFile) + " " +
    //         "--decompresscode " +
    //         Quote(pathTo3DS));

    //     RunPythonTool(pathToExtractFolder, Quote(pathTo3DS));
    // }

    // public static void RunPythonTool(string ExToolsDir, string pathTo3DSQuoted)
    // {
    //     //python3 generate_rsf.py mygame.3ds exheader.bin rsf_template.rsf --regionfree_icon icon.bin --spoof_firmware

    //     string ctrtoolPath = "makegen.py";
    //     string exheaderPath = Path.Combine(ExToolsDir, "exheader.bin");
    //     string iconBinPath = Path.Combine(ExToolsDir, "exefs/icon.bin");
    //     string dummyRsfPath = Path.Combine(Path.GetDirectoryName("Assets/Editor/N3DSExTools/tools"), "tools/dummy.rsf");
    //     string args = ctrtoolPath+" "+pathTo3DSQuoted+" "+Quote(exheaderPath)+" "+Quote(dummyRsfPath)+" --regionfree_icon "+Quote(iconBinPath)+" --spoof_firmware";
    //     ProcessStartInfo psi = new ProcessStartInfo
    //     {
    //         FileName = "python",
    //         Arguments = args,
    //         UseShellExecute = false,
    //         RedirectStandardOutput = true,
    //         RedirectStandardError = true,
    //         CreateNoWindow = true,
    //         WorkingDirectory = Path.GetDirectoryName("Assets/Editor/N3DSExTools/tools")
    //     };
    //     Debug.Log("Running python with args: "+args);

    //     using (var process = Process.Start(psi))
    //     {
    //         string output = process.StandardOutput.ReadToEnd();
    //         string error = process.StandardError.ReadToEnd();
    //         process.WaitForExit();

    //         Debug.Log("python stdout:\n" + output);
    //         Debug.Log("python stderr:\n" + error);

    //         if (process.ExitCode != 0)
    //             Debug.LogError("python failed (exit code "+process.ExitCode+"):\n"+error);
    //     }
    // }

    // private static void RunCtrTool(string ctrtoolPath, string args)
    // {
    //     ProcessStartInfo psi = new ProcessStartInfo
    //     {
    //         FileName = ctrtoolPath,
    //         Arguments = args,
    //         UseShellExecute = false,
    //         RedirectStandardOutput = true,
    //         RedirectStandardError = true,
    //         CreateNoWindow = true,
    //         WorkingDirectory = Path.GetDirectoryName(ctrtoolPath)
    //     };
    //     Debug.Log("Running ctrtool with args: "+args);

    //     using (var process = Process.Start(psi))
    //     {
    //         string output = process.StandardOutput.ReadToEnd();
    //         string error = process.StandardError.ReadToEnd();
    //         process.WaitForExit();

    //         Debug.Log("ctrtool stdout:\n" + output);
    //         Debug.Log("ctrtool stderr:\n" + error);

    //         if (process.ExitCode != 0)
    //             Debug.LogError("ctrtool failed (exit code "+process.ExitCode+"):\n"+error);
    //     }
    // }

    // public static void RebuildToCIA(string pathToCIA, bool encrypt, string pathToExtractedFolder)
    // {
    //     string makeromPath = Path.Combine("Assets/Editor", "makerom.exe");
    //     string exefsDir = Path.Combine(pathToExtractedFolder, "exefs");
    //     string romfsDir = Path.Combine(pathToExtractedFolder, "romfs");
    //     string rsfPath = Path.Combine(pathToExtractedFolder, "Application.rsf");

    //     ProcessStartInfo psi = new ProcessStartInfo
    //     {
    //         FileName = makeromPath,
    //         Arguments = "-f cia -o \"" + pathToCIA + "\" -rsf \"" + rsfPath + "\" " +
    //         "-icon \"" + Path.Combine(pathToExtractedFolder, "icon.icn") + "\" " +
    //         "-banner \"" + Path.Combine(pathToExtractedFolder, "banner.bnr") + "\" " +
    //         "-target t -ignoresign"/* +
    //         (encrypt ? "" : " -nocrypto")*/,
    //         UseShellExecute = false,
    //         RedirectStandardOutput = true,
    //         RedirectStandardError = true,
    //         CreateNoWindow = true
    //     };

    //     using (var process = Process.Start(psi))
    //     {
    //         process.WaitForExit();
    //         string output = process.StandardOutput.ReadToEnd();
    //         string error = process.StandardError.ReadToEnd();
    //         if (process.ExitCode != 0)
    //             Debug.LogError("makerom failed:\n"+error);
    //         else
    //             Debug.Log("makerom output:\n"+output);
    //     }
    // }

    // private static void InjectOrReplace(string key, string value)
    // {
    //     // Pad with two spaces for RSF style
    //     string entry = "  "+key+" : "+value;
    //     for (int i = 0; i < updatedLines.Count; i++)
    //     {
    //         if (updatedLines[i].TrimStart().StartsWith(key + " :"))
    //         {
    //             updatedLines[i] = entry;
    //             modified = true;
    //             injectedKeys.Add(key);
    //             return;
    //         }
    //     }
    //     // Not found, insert it
    //     updatedLines.Add(entry);
    //     modified = true;
    //     injectedKeys.Add(key);
    // }

    //private static List<string> updatedLines;
    //private static bool modified = false;
   // private static HashSet<string> injectedKeys;

    // public static void PatchRSFFile(string pathToRSF, string rsfToWriteTo)
    // {
    //     if (!File.Exists(pathToRSF))
    //     {
    //         Debug.LogError("[N3DSExTools] RSF file not found: "+pathToRSF);
    //         return;
    //     }

    //     var lines = File.ReadAllLines(pathToRSF).ToList();
    //     updatedLines = new List<string>();
    //     bool inSystemControlInfo = false;
    //     modified = false;

    //     // Track what we’ve already injected
    //     injectedKeys = new HashSet<string>();

    //     foreach (string line in lines)
    //     {
    //         string trimmed = line.Trim();

    //         // Detect section headers
    //         if (trimmed.EndsWith(":"))
    //         {
    //             if (trimmed == "SystemControlInfo:")
    //             {
    //                 inSystemControlInfo = true;
    //                 updatedLines.Add(line);
    //                 continue;
    //             }
    //             else
    //             {
    //                 inSystemControlInfo = false;
    //             }
    //         }

    //         updatedLines.Add(line);
    //     }

    //     // Inject settings under SystemControlInfo
    //     if (!lines.Any(l => l.Trim() == "SystemControlInfo:"))
    //     {
    //         updatedLines.Add("");
    //         updatedLines.Add("SystemControlInfo:");
    //         inSystemControlInfo = true;
    //     }

    //     if (inSystemControlInfo)
    //     {
    //         // Memory Mode
    //         if (N3DSExTools.extendedMemoryMode != ExtendedMemoryMode.None)
    //         {
    //             string val = null;

    //             switch (N3DSExTools.extendedMemoryMode)
    //             {
    //                 case ExtendedMemoryMode.MB32:
    //                     val = "32MB";
    //                     break;
    //                 case ExtendedMemoryMode.MB64:
    //                     val = "64MB";
    //                     break;
    //                 case ExtendedMemoryMode.MB72:
    //                     val = "72MB";
    //                     break;
    //                 case ExtendedMemoryMode.MB80:
    //                     val = "80MB";
    //                     break;
    //                 case ExtendedMemoryMode.MB96:
    //                     val = "96MB";
    //                     break;
    //                 default:
    //                     val = null;
    //                     break;
    //             }

    //             if (val != null)
    //                 InjectOrReplace("SystemMode", val);
    //         }

    //         // Extended Memory Mode
    //         if (N3DSExTools.extendedOperatingMode != ExtendedMemoryModeNew.None)
    //         {
    //             string val = null;

    //             switch (N3DSExTools.extendedOperatingMode)
    //             {
    //                 case ExtendedMemoryModeNew.MB124:
    //                     val = "124MB";
    //                     break;
    //                 case ExtendedMemoryModeNew.MB178:
    //                     val = "178MB";
    //                     break;
    //                 default:
    //                     val = null;
    //                     break;
    //             }

    //             if (val != null)
    //                 InjectOrReplace("SystemModeExt", val);
    //         }

    //         // CPU Speed
    //         if (N3DSExTools.cpuSpeed != CPUSpeed.Default)
    //         {
    //             string val = null;

    //             switch (N3DSExTools.cpuSpeed)
    //             {
    //                 case CPUSpeed.MHz268:
    //                     val = "268MHz";
    //                     break;
    //                 case CPUSpeed.MHz804:
    //                     val = "804MHz";
    //                     break;
    //                 default:
    //                     val = null;
    //                     break;
    //             }

    //             if (val != null)
    //                 InjectOrReplace("CpuSpeed", val);
    //         }

    //         // L2 Cache
    //         InjectOrReplace("EnableL2Cache", N3DSExTools.enableL2Cache.ToString());

    //         // Core2 Access
    //         InjectOrReplace("CanAccessCore2", N3DSExTools.enableCore2.ToString());

    //         if (N3DSExTools.useHomebrewLogo)
    //         {
    //             InjectOrReplace("Logo", "Homebrew");
    //         }
    //     }

    //     if (modified)
    //     {
    //         File.WriteAllLines(rsfToWriteTo, updatedLines.ToArray());
    //         Debug.Log("[N3DSExTools] RSF successfully patched.");
    //     }
    //     else
    //     {
    //         Debug.Log("[N3DSExTools] No changes needed in RSF.");
    //     }
    // }
}


public static class N3DSExTools
{

    public static bool sdmcAccess = false;
    //public static ExtendedMemoryMode extendedMemoryMode;
    public static bool rsfDebugMode = false;
    //public static ExtendedMemoryModeNew extendedOperatingMode;
    //public static CPUSpeed cpuSpeed;
    //public static bool enableL2Cache;
    //public static bool enableCore2;
    public static bool FourGigMode = false;
    public static bool ForceCompressStaticMemory = false;
    //public static bool useOnSd = true;
    //public static bool encryptCia = false;
    //public static bool useHomebrewLogo = false;

    private static uint Random1(uint seed)
    {
        seed = seed * 1103515245U + 12345U;
        return seed;
    }

    private static uint SdbmHash(string str)
    {
        uint num = 0U;
        byte[] bytes = Encoding.UTF8.GetBytes(str);
        foreach (byte b in bytes)
        {
            num = (uint)b + (num << 6) + (num << 16) - num;
        }
        return num;
    }

    public static void makeNetLibKey()
    {
        string str = Application.companyName.Trim();
        string str2 = Application.productName.Trim();
        uint seed = SdbmHash(str + "::" + str2);
        string a = "0x" + Random1(seed).ToString("X4");
        //PlayerSettings.N3DS.netLibKey = a;
        EditorUtility.DisplayDialog("Net Key", "Here's your net key: "+a, "OK");
    }
}

// patches

[HarmonyPatch(typeof(UnityEditor.PostProcessN3DS), "CreateCCI")]
public static class Patch_CreateCCI
{
    public static void Postfix(object __instance, bool developmentBuild, List<string> cfaList, string installPath, string stagingArea)
    {
        // Debug.Log("[N3DSExTools] Postfix: CreateCCI complete.");

        // var type = __instance.GetType();

        // var progressField = type.GetField("progress", BindingFlags.NonPublic | BindingFlags.Instance);
        // if (progressField == null)
        // {
        //     Debug.LogError("Field 'progress' not found");
        //     return;
        // }

        // ProgressHelper progress = (ProgressHelper)progressField.GetValue(__instance);
        // if (progress == null)
        // {
        //     Debug.LogError("progress is null");
        //     return;
        // }
        // progress.Step("N3DS build", "Doctoring the generated cci file!");

        // string ciaOutputPath = Path.GetDirectoryName(installPath)+"/"+Path.GetFileNameWithoutExtension(installPath)+".cia";
        // string extractPath = Path.Combine(stagingArea, "N3DSExTools");
        // N3DSToolchain.UnpackCCI(installPath, extractPath);
        // string rsfPath = Path.Combine(stagingArea+"/N3DSExTools/", "Application.rsf");
        // N3DSToolchain.PatchRSFFile(Path.Combine("Assets/Editor/N3DSExTools/tools", "dummy.rsf"),rsfPath);

        // // Rebuild using extracted content
        // try
        // {
        //     N3DSToolchain.RebuildToCIA(ciaOutputPath, N3DSExTools.encryptCia, stagingArea+"/N3DSExTools/");
        //     progress.Step("N3DS build", "Doctored the cci file! Generated CIA File!");
        //     Debug.Log("[N3DSExTools] CIA rebuilt successfully: " + ciaOutputPath);
        // }
        // catch (Exception ex)
        // {
        //     Debug.LogError("[N3DSExTools] Failed to rebuild CIA: " + ex.Message);
        // }
        // return;
    }
}

[HarmonyPatch(typeof(UnityEditor.PostProcessN3DS), "CreateRSF", new Type[] { typeof(bool), typeof(string) })]
public static class Patch_CreateRSF
{

    public static bool Prefix(object __instance, ref string __result, bool developmentBuild, string stagingArea)
    {
        // You now have full control to reproduce and modify the original code.
        // Below is the original method body in C#.

        string text = "Application.rsf";
        string result = Path.Combine(stagingArea, text);

        // Access the private field `progress` using reflection
        var type = __instance.GetType();

        var progressField = type.GetField("progress", BindingFlags.NonPublic | BindingFlags.Instance);
        if (progressField == null)
        {
            Debug.LogError("Field 'progress' not found");
            return true;
        }

        ProgressHelper progress = (ProgressHelper)progressField.GetValue(__instance);
        if (progress == null)
        {
            Debug.LogError("progress is null");
            return true;
        }
        progress.Step("N3DS build", "Creating " + text + "With the assistance of the N3DS Ex Tools!");

        // Access private fields: mediaSizeNames, logoStyleNames, buildCategories, saveDataSizeNames
        string[] mediaSizeNames = (string[])__instance.GetType()
            .GetField("mediaSizeNames", BindingFlags.NonPublic | BindingFlags.Instance)
            .GetValue(__instance);
        string[] logoStyleNames = (string[])__instance.GetType()
            .GetField("logoStyleNames", BindingFlags.NonPublic | BindingFlags.Instance)
            .GetValue(__instance);
        string[] buildCategories = (string[])__instance.GetType()
            .GetField("buildCategories", BindingFlags.NonPublic | BindingFlags.Instance)
            .GetValue(__instance);
        string[] saveDataSizeNames = (string[])__instance.GetType()
            .GetField("saveDataSizeNames", BindingFlags.NonPublic | BindingFlags.Instance)
            .GetValue(__instance);

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("BasicInfo:");
        sb.Append("  Title: ");
        sb.AppendLine(PlayerSettings.N3DS.title);

        string productCode = PlayerSettings.N3DS.productCode;
        if (!string.IsNullOrEmpty(productCode))
        {
            sb.Append("  ProductCode: ");
            sb.AppendLine(productCode);
        }

        int mediaIndex = ((int)PlayerSettings.N3DS.mediaSize) & 255;
        int logoIndex = ((int)PlayerSettings.N3DS.logoStyle) & 255;
        sb.Append("  MediaSize: ");
        if (!N3DSExTools.FourGigMode)
            sb.AppendLine(mediaSizeNames[mediaIndex]);
        else
            sb.AppendLine("4GB");

        sb.Append("  Logo: ");
        sb.AppendLine(logoStyleNames[logoIndex]);
        sb.AppendLine();

        sb.AppendLine("TitleInfo:");
        sb.Append("  Category: ");
        sb.AppendLine(buildCategories[PlayerSettings.N3DS.buildCategory]);
        if (PlayerSettings.N3DS.buildCategory == 2)
        {
            sb.AppendLine("  DemoIndex: " + PlayerSettings.N3DS.demoIndex);
        }

        sb.Append("  UniqueId: ");
        sb.AppendLine(PlayerSettings.N3DS.applicationId.ToLower());
        sb.AppendLine();

        sb.AppendLine("Rom:");
        sb.AppendLine("  HostRoot: RomFS");
        if (PlayerSettings.N3DS.useSaveData)
        {
            sb.Append("  SaveDataSize: ");
            int saveIndex = ((int)PlayerSettings.N3DS.saveDataSize) & 255;
            sb.AppendLine(saveDataSizeNames[saveIndex]);
        }

        sb.AppendLine();
        sb.AppendLine("AccessControlInfo:");
        if (PlayerSettings.N3DS.useExtSaveData)
        {
            sb.AppendLine("  UseExtSaveData: True");
            sb.Append("  ExtSaveDataNumber: ");
            sb.AppendLine(PlayerSettings.N3DS.extSaveDataNumber);

            /*if (PlayerSettings.N3DS.allowDirectSDMC)
            {
                sb.AppendLine("  FileSystemAccess:");
                sb.AppendLine("   - DirectSdmc");
                if (developmentBuild)
                {
                    sb.AppendLine("   - Debug");
                }
            }*/

            if (N3DSExTools.sdmcAccess | N3DSExTools.rsfDebugMode)
            {
                sb.AppendLine("  FileSystemAccess:");
            }

            if (N3DSExTools.sdmcAccess)
            {

                sb.AppendLine("   - DirectSdmc");
            }
            if (N3DSExTools.rsfDebugMode)
            {
                sb.AppendLine("   - Debug");
            }
        }
        else
        {
            sb.AppendLine("  UseExtSaveData: False");
        }

        sb.AppendLine();
        sb.AppendLine("SystemControlInfo:");
        sb.Append("  StackSize: ");
        sb.AppendLine((PlayerSettings.N3DS.stackSizeK * 1024).ToString());
        sb.AppendLine();

        sb.AppendLine("Option:");
        sb.Append("  EnableCompress: ");
        sb.AppendLine(N3DSExTools.ForceCompressStaticMemory.ToString());

        string currentDirectory = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(stagingArea);
        File.WriteAllText(text, sb.ToString());
        Directory.SetCurrentDirectory(currentDirectory);

        __result = result;
        return false; // Skip original method
    }
}

public enum ExtendedMemoryMode
{
    None,
    MB64,
    MB96,
    MB80,
    MB72,
    MB32
}

public enum CPUSpeed
{
    Default,
    MHz268,
    MHz804
}

public enum ExtendedMemoryModeNew
{
    None,
    MB124,
    MB178
}

[HarmonyPatch(typeof(UnityEditor.N3DS.SettingsEditorExtension), "SplashSectionGUI")]
public static class Patch_SplashSectionGUI
{
    // Prefix: Return false to skip original method entirely
    public static bool Prefix(UnityEditor.N3DS.SettingsEditorExtension __instance)
    {
        try
        {
            GUILayout.BeginVertical(); // Safely wrap GUI

            GUILayout.Space(10);
            GUILayout.Label("N3DS Ex Tools Prebuild patches by Dimolade", EditorStyles.boldLabel);
            //GUILayout.Space(-8);
            //GUILayout.Label("Prebuild patches by Dimolade");
            //GUILayout.Space(5);
            //GUILayout.Label("SNAKE & CTR", EditorStyles.boldLabel);
            //N3DSExTools.useHomebrewLogo = EditorGUILayout.Toggle("Homebrew Animation", N3DSExTools.useHomebrewLogo);
            N3DSExTools.sdmcAccess = EditorGUILayout.Toggle("SDMC Access", N3DSExTools.sdmcAccess);
            N3DSExTools.rsfDebugMode = EditorGUILayout.Toggle("(RSF) FS Debug", N3DSExTools.rsfDebugMode);
            N3DSExTools.FourGigMode = EditorGUILayout.Toggle("Use 4 GiB Cart Size", N3DSExTools.FourGigMode);
            //N3DSExTools.useOnSd = EditorGUILayout.Toggle("Use On SD (Recommended)", N3DSExTools.useOnSd);
            //  UseOnSD                 : true
            N3DSExTools.ForceCompressStaticMemory = EditorGUILayout.Toggle("Force Compress Static Memory", N3DSExTools.ForceCompressStaticMemory);
            //N3DSExTools.encryptCia = EditorGUILayout.Toggle("Encrypt CIA (Unrecommended)", N3DSExTools.encryptCia);

            //GUILayout.Space(5);
           //GUILayout.Label("CTR", EditorStyles.boldLabel);
            //N3DSExTools.extendedMemoryMode = (ExtendedMemoryMode)EditorGUILayout.EnumPopup("System Mode", N3DSExTools.extendedMemoryMode);

            //GUILayout.Space(5);
            //GUILayout.Label("SNAKE", EditorStyles.boldLabel);
            //N3DSExTools.extendedOperatingMode = (ExtendedMemoryModeNew)EditorGUILayout.EnumPopup("System Mode Ext", N3DSExTools.extendedOperatingMode);
            //N3DSExTools.cpuSpeed = (CPUSpeed)EditorGUILayout.EnumPopup("CPU Speed", N3DSExTools.cpuSpeed);
            //N3DSExTools.enableL2Cache = EditorGUILayout.Toggle("Enable L2 Cache", N3DSExTools.enableL2Cache);
            //N3DSExTools.enableCore2 = EditorGUILayout.Toggle("Enable Core2 Access", N3DSExTools.enableCore2);
            /*CpuSpeed                      : 268MHz # 268MHz(Default)/804MHz
  EnableL2Cache                 : false # false(default)/true
  CanAccessCore2                : false
*/
        }
        finally
        {
            GUILayout.EndVertical(); // Prevent GUIClip mismatch
        }

        // Returning false prevents the original method from executing
        return false;
    }
}

namespace N3DSExNspace
{
    public static class N3DSEntry
    {
        public static void Main()
        {
            var harmony = HarmonyInstance.Create("com.dimolade.n3dsextrapatch");
            harmony.PatchAll();
        }
    }
}