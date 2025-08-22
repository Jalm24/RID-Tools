using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;
using System.Xml;

public static class ridsmalldb
{
	private const string xmlRelativePath = "Editor/RID-Tools/ExtTools/comercialreleases.xml";

	[Serializable]
	private class postInfo
	{
		public string name;
		public string titleID;
		public string dev;
		public string apikey;
	}

	private static string baseURL()
	{
		var core = new RIDToolsCore();
		var config = core.LoadConfig();
		if (string.IsNullOrEmpty(config.API_URL))
		{
			Debug.LogWarning("API_URL not set in config. Using localhost.");
			return "http://localhost:80/api/apps/";
		}
		return config.API_URL.EndsWith("/") ? config.API_URL : config.API_URL + "/";
	}

	private static UnityWebRequest SyncGetRequest(string url)
	{
		var request = UnityWebRequest.Get(url);
		var asyncOp = request.Send();
		while (!asyncOp.isDone) { }
		return request;
	}

	//check if the title id is already in use on rid small db
	private static bool TitleIDExistsInAPI(string titleID)
	{
		string url = baseURL() + titleID;
		var request = SyncGetRequest(url);

		if (!string.IsNullOrEmpty(request.error))
		{
			Debug.LogError("Error connecting to the API: " + request.error);
			return true;
		}

		return request.responseCode != 404;
	}

	//load local db with 3ds comercial games
	private static HashSet<string> LoadExistingTitleIDs(string relativePath)
	{
		string fullPath = Path.Combine(Application.dataPath, relativePath);
		HashSet<string> titleIDs = new HashSet<string>();

		if (!File.Exists(fullPath))
		{
			Debug.LogWarning("Archivo XML no encontrado: " + fullPath);
			return titleIDs;
		}

		XmlDocument doc = new XmlDocument();
		doc.Load(fullPath);

		XmlNodeList nodes = doc.SelectNodes("//release/titleid");
		foreach (XmlNode node in nodes)
		{
			if (!string.IsNullOrEmpty(node.InnerText))
			{
				titleIDs.Add(node.InnerText.Trim());
			}
		}

		//Debug.Log("Total de titleIDs cargados: " + titleIDs.Count);
		return titleIDs;
	}
	
	//check if titleid exist on local and rid small db
	public static void CheckTitleID(Action<bool> onResult)
	{
		string localTitleID = PlayerSettings.N3DS.applicationId;

		// Verificar en el XML local
		HashSet<string> existingTitleIDs = LoadExistingTitleIDs(xmlRelativePath);
		if (existingTitleIDs.Contains(localTitleID))
		{
			EditorUtility.DisplayDialog("CRITICAL ERROR", "Title ID already exists in local XML.\nUsed by: " + localTitleID, "OK");
			onResult(true);
			return;
		}

		// Verificar en la API remota
		if (TitleIDExistsInAPI(localTitleID))
		{
			EditorUtility.DisplayDialog("CRITICAL ERROR", "Title ID already used in RID SmallDB.\nUsed by: " + localTitleID, "OK");
			onResult(true);
			return;
		}

		// Si no existe en ninguno
		onResult(false);
	}

	//generate new titleid that doesn't exist
	public static string GenTitleID()
	{
		int min = 0x00300;
		int max = 0xf7fff;

		HashSet<string> existingTitleIDs = LoadExistingTitleIDs(xmlRelativePath);

		while (true)
		{
			string newID = "0x" + UnityEngine.Random.Range(min, max).ToString("X5");

			if (existingTitleIDs.Contains(newID)) continue;
			if (TitleIDExistsInAPI(newID)) continue;

			return newID;
		}
	}

	//submit request to registry titleid on rid small api
	public static void SubmitTitleID(string name, string titleID, string dev, string apikey, Action<bool> onResult)
	{
		string url = baseURL() + "submit";

		var payload = new postInfo
		{
			name = name,
			titleID = titleID,
			dev = dev,
			apikey = apikey
		};

		string jsonData = JsonUtility.ToJson(payload);

		var request = new UnityWebRequest(url, "POST");
		byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
		request.uploadHandler = new UploadHandlerRaw(bodyRaw);
		request.downloadHandler = new DownloadHandlerBuffer();
		request.SetRequestHeader("Content-Type", "application/json");

		var asyncOp = request.Send();
		while (!asyncOp.isDone) { }

		if (!string.IsNullOrEmpty(request.error))
		{
			Debug.LogError("Error registering TitleID: " + request.error);
		}
		else
		{
			switch (request.responseCode)
			{
				case 201:
					Debug.Log("TitleID successfully registered: " + titleID);
					break;
				case 403:
					Debug.LogWarning("Invalid API key. TitleID under manual review");
					break;
				default:
					Debug.LogWarning("Unexpected response: " + request.responseCode);
					break;
			}
		}

		onResult(true); // flujo, no estado
	}
}