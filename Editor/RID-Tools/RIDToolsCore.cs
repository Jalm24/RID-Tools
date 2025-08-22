using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEngine;

[Serializable]
public class ridconfig
{
	//cia settings
	public string cciPath = "";
	public string ciaOutputPath = "";
	public string memoryMode = "64MB";
	public bool hasManual = false;
	public bool autoCia = false;
	public bool autoCiaPatcher = false;

	//exclusive snake settings
	public string memoryModeExt = "124MB";
	public bool superSpeed = false;
	public bool l2cache = false;
	public bool EnableCore2 = false;

	//api settings
	public string TitleID = "";
	public string TitleName = "";
	public string TitleDeveloper = "";
	public string API_KEY = "";
	public string API_URL = "";

	//other settings
	public bool sdmcAccess = false;
	public bool debugFlag = false;
	public bool forceCompress = false;
	public bool writeSharedPage = false;
	public bool PrivilegedPriority = false;
	public bool NonAlphabetAndNumber = false;
	public bool MainFunctionArgument = false;
	public bool sharedevicemem = false;
	public bool runOnSleep = false;
	public bool SpecialMem = false;
	public bool forceDebug = false;
	public bool disableDebug = false;
}

public class RIDToolsCore
{
	public string configPath = "Assets/Editor/RID-Tools/tools/config.dat";

	public void SaveConfig(ridconfig data)
	{
		try
		{
			BinaryFormatter formatter = new BinaryFormatter();
			using (FileStream stream = new FileStream(configPath, FileMode.Create))
			{
				formatter.Serialize(stream, data);
			}
		}
		catch (Exception e)
		{
			Debug.LogError("Error saving configuration: " + e.Message);
		}
	}

	public ridconfig LoadConfig()
	{
		if (!File.Exists(configPath))
		{
			Debug.LogWarning("Configuration file not found.");
			return new ridconfig(); // Devuelve una config por defecto
		}

		try
		{
			BinaryFormatter formatter = new BinaryFormatter();
			using (FileStream stream = new FileStream(configPath, FileMode.Open))
			{
				return (ridconfig)formatter.Deserialize(stream);
			}
		}
		catch (Exception e)
		{
			Debug.LogError("Error loading configuration: " + e.Message);
			return new ridconfig(); // En caso de error, devuelve config por defecto
		}
	}

	//get a string property from playersettings and change value
	public void SaveXSFields(string property, string value)
	{
		var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset");
		if (assets == null || assets.Length == 0)
		{
			Debug.LogError("Can't find file ProjectSettings/ProjectSettings.asset");
			return;
		}

		var asset = assets[0];
		var obj = new SerializedObject(asset);
		var prop = obj.FindProperty(property);
		if (prop == null)
		{
			Debug.LogError("cant find" + property +" in ProjectSettings.asset");
			return;
		}

		prop.stringValue = value;
		obj.ApplyModifiedProperties();

		EditorUtility.SetDirty(asset);
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();

		//Debug.Log("XS Settings saved directly in ProjectSettings.asset with titleID: " + id);
	}
	//get a bool property from playersettings and change value
	//public void SavePSBoolFields(string property, bool value)
	//{
	//	var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset");
	//	if (assets == null || assets.Length == 0)
	//	{
	//		Debug.LogError("Can't find file ProjectSettings/ProjectSettings.asset");
	//		return;
	//	}

	//	var asset = assets[0];
	//	var obj = new SerializedObject(asset);
	//	var prop = obj.FindProperty(property);
	//	if (prop == null)
	//	{
	//		Debug.LogError("cant find" + property + " in ProjectSettings.asset");
	//		return;
	//	}

	//	prop.boolValue = value;
	//	obj.ApplyModifiedProperties();

	//	EditorUtility.SetDirty(asset);
	//	AssetDatabase.SaveAssets();
	//	AssetDatabase.Refresh();

	//	//Debug.Log("XS Settings saved directly in ProjectSettings.asset with titleID: " + id);
	//}
}