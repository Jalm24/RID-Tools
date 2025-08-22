using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
using System;
using UnityEngine;

public class AutoBuildHook
{
	static bool yaEjecutado = false;

	[PostProcessBuild]
	public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
	{
		var core = new RIDToolsCore();
		var data = new ridconfig();
		var toCia = new cciToCia();
		var SetSM = new SetSM();


		if (yaEjecutado) return;
		yaEjecutado = true;

		if (!File.Exists(core.configPath)) return;

		data = core.LoadConfig();
		data.cciPath = pathToBuiltProject;
		core.SaveConfig(data);

		// Solo actuar si se genera un .cci
		if (!pathToBuiltProject.EndsWith(".cci")) return;
		try
		{
			Debug.Log(data.autoCiaPatcher.ToString());
			if (data.autoCiaPatcher)
			{
				Debug.Log("Invocando rebuildcia...");
				SetSM.rebuildcia();
			}
			else if (data.autoCia)
			{
				toCia.makecia();
			}
			else
			{
				return;
			}
			//UnityEngine.Debug.Log("path to build project: \"" + pathToBuiltProject + "\" and path to cci on data: " + data.cciPath);
		}
		catch (Exception e)
		{
			UnityEngine.Debug.LogError(e);
		}
	}
}