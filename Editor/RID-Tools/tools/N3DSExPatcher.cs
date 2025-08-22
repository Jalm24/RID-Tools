using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Reflection;
using Harmony;

public class N3DSExToolsMenu
{
    static bool injected = false;
    static Assembly loadedAssembly = null;
    static HarmonyInstance harmony = null;

    [MenuItem("RID-Tools/(RE)Inyect Extra Patcher")]
    public static void InjectOrReinject()
    {
        string dllPath = Path.Combine(Application.dataPath, "Plugins/Editor/Extra/N3DSExtraPatcher.dll");

        if (!File.Exists(dllPath))
        {
            EditorUtility.DisplayDialog("DLL Not Found", "Could not find DLL at:\n" + dllPath, "OK");
            return;
        }

        try
        {
            // Unpatch previous injection if it exists
            if (injected && harmony != null)
            {
                harmony.UnpatchAll("com.dimolade.n3dsextrapatch");
                injected = false;
                Debug.Log("Previous Harmony patches removed.");
            }

            // Reload DLL
            loadedAssembly = Assembly.LoadFile(dllPath);
            Type entryType = loadedAssembly.GetType("N3DSExNspace.N3DSEntry");

            if (entryType == null)
            {
                EditorUtility.DisplayDialog("Error", "Could not find class N3DSExNspace.N3DSEntry", "OK");
                return;
            }

            MethodInfo mainMethod = entryType.GetMethod("Main", BindingFlags.Public | BindingFlags.Static);

            if (mainMethod == null)
            {
                EditorUtility.DisplayDialog("Error", "Could not find static method Main in N3DSEntry", "OK");
                return;
            }

            // Set up Harmony again
            harmony = HarmonyInstance.Create("com.dimolade.n3dsextrapatch");

            // Call the entry method which should apply patches
            mainMethod.Invoke(null, null);

            EditorUtility.DisplayDialog("Success", "DLL (re)injected successfully.", "OK");
            injected = true;
        }
        catch (Exception ex)
        {
            EditorUtility.DisplayDialog("Injection Failed", ex.ToString(), "OK");
        }
	}
}