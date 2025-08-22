using UnityEngine;
using System.IO;
using System.Diagnostics;

public class cciToCia {
	private ridconfig data = new ridconfig();
	private RIDToolsCore core = new RIDToolsCore();
	public void makecia()
	{
		data = core.LoadConfig();
		string boot9path = Path.Combine(Application.dataPath, "Editor/RID-Tools/ExtTools/boot9/boot9.bin");

		if (!File.Exists(boot9path))
		{
			UnityEngine.Debug.LogError("theres no boot9.bin on \"/Editor/RID-Tools/ExtTools/boot9/\" folder");
		}
		else
		{

			// Armar argumentos de línea de comandos
			string args = "--output=\"" + data.ciaOutputPath + "\" --overwrite ";
			args += " --boot9=\"" + Application.dataPath + "/Editor/RID-Tools/ExtTools/boot9/boot9.bin\"";
			//if (hasManual) args += " --hasManual";
			args += " \"" + data.cciPath + "\"";
			//UnityEngine.Debug.Log(data.cciPath);

			Process proc = new Process();
			proc.StartInfo.FileName = Application.dataPath + "/Editor/RID-Tools/ExtTools/3dsconv.exe";
			proc.StartInfo.Arguments = args;
			proc.StartInfo.UseShellExecute = false;
			proc.StartInfo.CreateNoWindow = true;
			proc.StartInfo.RedirectStandardOutput = true;
			proc.StartInfo.RedirectStandardError = true;

			proc.Start();
			string output = proc.StandardOutput.ReadToEnd();
			string error = proc.StandardError.ReadToEnd();
			proc.WaitForExit();
			if (proc.ExitCode == 0)
			{
				UnityEngine.Debug.Log("3dsconv output: \n" + output);
				//UnityEngine.Debug.Log("exit code: " + proc.ExitCode);
			}
			else
			{
				UnityEngine.Debug.LogError("Error building CIA.\n" + error);
				UnityEngine.Debug.Log("Output:\n" + output);
				UnityEngine.Debug.Log("exit code: " + proc.ExitCode);
			}
		}
	}
}