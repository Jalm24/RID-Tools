using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

public class SetSM {

	//apli patches from config file
	private ridconfig data = new ridconfig();
	private RIDToolsCore core = new RIDToolsCore();
	public void rebuildcia()
	{
		data = core.LoadConfig();
		//barra de progreso al 0%
		EditorUtility.DisplayProgressBar("Making CIA with Patches", "Initializing tools and working directory", 0);

		string tempFolder = Environment.CurrentDirectory + "/Temp/RIDTools";
		if (!Directory.Exists(tempFolder))
		{
			Directory.CreateDirectory(tempFolder); // Crea si no existe
		}

		//barra de progreso al 10%
		EditorUtility.DisplayProgressBar("Making CIA with Patches", "Extracting exefs, romfs and exheader.bin from .cci", 0.1f);

		// Armar argumentos
		//ctrtool --exefsdir=exefs --romfsdir=romfs --exheader=exheader.bin game.3ds
		string args = "--exefsdir=\"" + tempFolder + "/exefs\"";
		args += " --romfsdir=\"" + tempFolder + "/romfs\"";
		args += " --exheader=\"" + tempFolder + "/exheader.bin\"";
		args += " \"" + data.cciPath + "\"";
		//UnityEngine.Debug.Log("ctrtool args: \n\n" + args);

		Process proc = new Process();
		proc.StartInfo.FileName = Application.dataPath + "/Editor/RID-Tools/ExtTools/ctrtool.exe";
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
			//UnityEngine.Debug.Log("extracted rom\n" + output);
			//UnityEngine.Debug.Log("exit code: " + proc.ExitCode);
		}
		else
		{
			UnityEngine.Debug.LogError("error extracting rom\n" + error);
			UnityEngine.Debug.Log("output:\n" + output);
			UnityEngine.Debug.Log("exit code: " + proc.ExitCode);
		}

		//vaciar string args solo por si acaso
		args = "";

		EditorUtility.DisplayProgressBar("Making CIA with Patches", "Obtaining the original RSF file from .cci and exheader.bin using rsfgen3.exe with a dummy.rsf", 0.25f);

		// Armar argumentos
		//python rsfgen.py -r game.3ds -e exheader.bin -o dummy.rsf
		string rsfDir = Application.dataPath + "/Editor/RID-Tools/ExtTools/dummy.rsf";
		string rsfname = Path.GetFileName(rsfDir);
		string destino = Path.Combine(tempFolder, rsfname);
		if (File.Exists(destino))
		{
			File.Copy(rsfDir, destino, true);
		}
		else
		{
			File.Copy(rsfDir, destino, false);
		}

		args = "-r \"" + data.cciPath + "\"";
		args += " -e \"" + tempFolder + "/exheader.bin\"";
		args += " -o\"" + destino + "\"";
		//UnityEngine.Debug.Log("rsfgen3 args: \n\n" + args);

		proc = new Process();
		proc.StartInfo.FileName = Application.dataPath + "/Editor/RID-Tools/ExtTools/rsfgen3.exe";
		proc.StartInfo.Arguments = args;
		proc.StartInfo.UseShellExecute = false;
		proc.StartInfo.CreateNoWindow = true;
		proc.StartInfo.RedirectStandardOutput = true;
		proc.StartInfo.RedirectStandardError = true;

		proc.Start();
		output = proc.StandardOutput.ReadToEnd();
		error = proc.StandardError.ReadToEnd();
		proc.WaitForExit();
		if (proc.ExitCode == 0)
		{
			//UnityEngine.Debug.Log("rsf file obtained!\n" + output);
			//UnityEngine.Debug.Log("exit code: " + proc.ExitCode);
		}
		else
		{
			UnityEngine.Debug.LogError("error getting rsf file.\n" + error);
			UnityEngine.Debug.Log("output:\n" + output);
			UnityEngine.Debug.Log("exit code: " + proc.ExitCode);
		}

		EditorUtility.DisplayProgressBar("Making CIA with Patches", "Adding patches to RSF", 0.4f);
		//modificar archivo rsf para habilitar funciones adicionales
		if (File.Exists(destino))
		{
			try
			{
				List<string> lineas = new List<string>(File.ReadAllLines(destino));

				lineas[13] = data.forceCompress ? "  EnableCompress          : true" : "  EnableCompress          : false";

				if (data.debugFlag)
					lineas[33] = "   - Debug";

				if (data.sdmcAccess)
					lineas[37] = "   - DirectSdmc";

				// Reemplazos directos
				//lineas[57] = "  MaxCpu                        : 0 # Let system decide"; -> esta porqueria crashea en hardware, no implementar!

				//esto funciona bien
				lineas[59] = data.disableDebug ? "  DisableDebug                  : true" : "  DisableDebug                  : false";
				lineas[60] = data.forceDebug ? "  EnableForceDebug              : true" : "  EnableForceDebug              : false";
				lineas[61] = data.writeSharedPage ? "  CanWriteSharedPage            : true" : "  CanWriteSharedPage            : false";
				lineas[62] = data.PrivilegedPriority ? "  CanUsePrivilegedPriority      : true" : "  CanUsePrivilegedPriority      : false";
				lineas[63] = data.NonAlphabetAndNumber ? "  CanUseNonAlphabetAndNumber    : true" : "  CanUseNonAlphabetAndNumber    : false";
				lineas[64] = data.MainFunctionArgument ? "  PermitMainFunctionArgument    : true" : "  PermitMainFunctionArgument    : false";
				lineas[65] = data.sharedevicemem ? "  CanShareDeviceMemory          : true" : "  CanShareDeviceMemory          : false";
				lineas[66] = data.runOnSleep ? "  RunnableOnSleep               : true" : "  RunnableOnSleep               : false";
				lineas[67] = data.SpecialMem ? "  SpecialMemoryArrange          : true" : "  SpecialMemoryArrange          : false";



				int sysmodeline = 76;

				// Insertar SystemMode
				switch (data.memoryMode)
				{
					case "64MB": lineas.Insert(sysmodeline, "  SystemMode                    : 64MB"); break;
					case "96MB": lineas.Insert(sysmodeline, "  SystemMode                    : 96MB"); break;
					case "80MB": lineas.Insert(sysmodeline, "  SystemMode                    : 80MB"); break;
					case "72MB": lineas.Insert(sysmodeline, "  SystemMode                    : 72MB"); break;
					case "32MB": lineas.Insert(sysmodeline, "  SystemMode                    : 32MB"); break;
				}

				// Insertar SystemModeExt
				sysmodeline++;
				switch (data.memoryModeExt)
				{
					case "Legacy": lineas.Insert(sysmodeline, "  SystemModeExt                 : Legacy"); break;
					case "124MB": lineas.Insert(sysmodeline, "  SystemModeExt                 : 124MB"); break;
					case "178MB": lineas.Insert(sysmodeline, "  SystemModeExt                 : 178MB"); break;
				}

				// Configuraciones adicionales
				lineas.Insert(78, data.superSpeed ? "  CpuSpeed                      : 804MHz" : "  CpuSpeed                      : 268MHz");
				lineas.Insert(79, data.l2cache ? "  EnableL2Cache                 : true" : "  EnableL2Cache                 : false");
				lineas.Insert(80, data.EnableCore2 ? "  CanAccessCore2                : true" : "  CanAccessCore2                : false");
				lineas[164] = "   - fs:USER";
				// Escritura forzada con CRLF y UTF-8
				using (StreamWriter writer = new StreamWriter(destino, false, new System.Text.UTF8Encoding(false)))
				{
					foreach (string linea in lineas)
					{
						writer.Write(linea);
						writer.Write("\r\n");
					}
				}
			}
			catch (Exception e)
			{
				UnityEngine.Debug.LogError("Error al leer o escribir el archivo: " + e.Message);
			}
		}

		EditorUtility.DisplayProgressBar("Making CIA with Patches", "Rebuilding romfs using 3dstool with romfs directory", 0.65f);

		//armando argumento para reconstruir romfs
		//3dstool -cvtf romfs romfs.bin --romfs-dir romfs/
		args = "";

		args = "-cvtf romfs \"" + tempFolder + "/romfs.bin\"";
		args += " --romfs-dir \"" + tempFolder + "/romfs/\"";
		//UnityEngine.Debug.Log("argumentos de 3dstool: \n\n" + args);

		proc = new Process();
		proc.StartInfo.FileName = Application.dataPath + "/Editor/RID-Tools/ExtTools/3dstool.exe";
		proc.StartInfo.Arguments = args;
		proc.StartInfo.UseShellExecute = false;
		proc.StartInfo.CreateNoWindow = true;
		proc.StartInfo.RedirectStandardOutput = true;
		proc.StartInfo.RedirectStandardError = true;

		proc.Start();
		output = proc.StandardOutput.ReadToEnd();
		error = proc.StandardError.ReadToEnd();
		proc.WaitForExit();
		if (proc.ExitCode == 0)
		{
			//UnityEngine.Debug.Log("romfs rebuilded!\n" + output);
			//UnityEngine.Debug.Log("exit code: " + proc.ExitCode);
		}
		else
		{
			UnityEngine.Debug.LogError("error rebuilding romfs\n" + error);
			UnityEngine.Debug.Log("output:\n" + output);
			UnityEngine.Debug.Log("exit code: " + proc.ExitCode);
		}


		args = "";

		EditorUtility.DisplayProgressBar("Making CIA with Patches", "Rebuilding game executable (cxi) using makerom with RSF file, exefs/code.bin, exefs/icon.bin, exefs/banner.bin, exheader.bin and romfs.bin", 0.8f);

		//reconstruir ejecutable
		//makerom -f cxi -o game.cxi -rsf game.rsf -code exefs/code.bin -icon exefs/icon.bin -banner exefs/banner.bin -exheader exheader.bin -romfs romfs.bin

		args = "-f cxi -o \"" + tempFolder + "/game.cxi\" ";
		args += "-rsf \"" + tempFolder + "/dummy.rsf\" ";
		args += "-code \"" + tempFolder + "/exefs/code.bin\" ";
		args += "-icon \"" + tempFolder + "/exefs/icon.bin\" ";
		args += "-banner \"" + tempFolder + "/exefs/banner.bin\" ";
		args += "-exheader \"" + tempFolder + "/exheader.bin\" ";
		args += "-romfs \"" + tempFolder + "/romfs.bin\" ";

		//UnityEngine.Debug.Log("makerom args para cxi: \n\n" + args);

		proc = new Process();
		proc.StartInfo.FileName = Application.dataPath + "/Editor/RID-Tools/ExtTools/makerom.exe";
		proc.StartInfo.Arguments = args;
		proc.StartInfo.UseShellExecute = false;
		proc.StartInfo.CreateNoWindow = true;
		proc.StartInfo.RedirectStandardOutput = true;
		proc.StartInfo.RedirectStandardError = true;

		proc.Start();
		output = proc.StandardOutput.ReadToEnd();
		error = proc.StandardError.ReadToEnd();
		proc.WaitForExit();
		if (proc.ExitCode == 0)
		{
			//UnityEngine.Debug.Log("cxi rebuilded!\n" + output);
			//UnityEngine.Debug.Log("exit code: " + proc.ExitCode);
		}
		else
		{
			UnityEngine.Debug.LogError("error rebuilding cxi\n" + error);
			UnityEngine.Debug.Log("output:\n" + output);
			UnityEngine.Debug.Log("exit code: " + proc.ExitCode);
		}


		args = "";

		//	empaquetar cia 

		//sin manual
		//#makerom -f cia -o game.cia -content game.cxi:0:0

		//con manual
		//#makerom -f cia -o game.cia -content game.cxi:0:0 -content manual.cfa:1:1 -content dlpchild.cfa:2:2

		if (data.hasManual)
		{
			EditorUtility.DisplayProgressBar("Making CIA with Patches", "Rebuilding CIA using makerom with CXI, manual.cfa and dlpchild.cfa (dlpchild.cfa isn't necesary but... just in case)", 0.95f);

			args = "-f cia -o \"" + Path.GetFileNameWithoutExtension(data.cciPath) + "-patched.cia\" ";
			args += "-content \"" + tempFolder + "/game.cxi:0:0\" ";
			args += "-content \"" + tempFolder + "/manual.cfa:1:1\" ";
			args += "-content \"" + tempFolder + "/dlpchild.cfa:2:2\"";

			//UnityEngine.Debug.Log("argumentos de makerom para reconstruir cia: \n\n" + args);

		}
		else
		{
			EditorUtility.DisplayProgressBar("Making CIA with Patches", "Rebuilding CIA using makerom with game executable (cxi)", 0.95f);

			args = "-f cia -o \"" + Path.GetFileNameWithoutExtension(data.cciPath) + "-patched.cia\" ";
			args += "-content \"" + tempFolder + "/game.cxi:0:0\" ";

			//UnityEngine.Debug.Log("argumentos de makerom para reconstruir cia: \n\n" + args);
		}
		proc = new Process();
		proc.StartInfo.FileName = Application.dataPath + "/Editor/RID-Tools/ExtTools/makerom.exe";
		proc.StartInfo.Arguments = args;
		proc.StartInfo.UseShellExecute = false;
		proc.StartInfo.CreateNoWindow = true;
		proc.StartInfo.RedirectStandardOutput = true;
		proc.StartInfo.RedirectStandardError = true;

		proc.Start();
		output = proc.StandardOutput.ReadToEnd();
		error = proc.StandardError.ReadToEnd();
		proc.WaitForExit();
		if (proc.ExitCode == 0)
		{
			//UnityEngine.Debug.Log("cia rebuilded!\n" + output);
			//UnityEngine.Debug.Log("exit code: " + proc.ExitCode);
		}
		else
		{
			UnityEngine.Debug.LogError("error rebuilding cia\n" + error);
			UnityEngine.Debug.Log("output:\n" + output);
			UnityEngine.Debug.Log("exit code: " + proc.ExitCode);
		}
		EditorUtility.DisplayProgressBar("Making CIA with Patches", "Finish", 1);
		EditorUtility.ClearProgressBar();
	}
}