using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class configMenu : EditorWindow {

	private ridconfig data = new ridconfig();
	private RIDToolsCore core = new RIDToolsCore();
	private cciToCia toCia = new cciToCia();
	private SetSM rebuild = new SetSM();
	private Vector2 scrollPos;
	private bool isChecking = false;

	private string oldTitleID;
	[MenuItem("RID-Tools/Config Menu")]
	public static void ShowWindow()
	{
		var window = GetWindow<configMenu>();
		window.titleContent = new GUIContent("Config Menu", EditorGUIUtility.IconContent("d_SettingsIcon").image);
	}

	void OnEnable()
	{
		if (File.Exists(core.configPath))
		{
			data = core.LoadConfig();
		}
		var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset");
		if (assets != null && assets.Length > 0)
		{
			var asset = assets[0];
			var obj = new SerializedObject(asset);
			var prop = obj.FindProperty("n3dsApplicationId"); // Ajusta el nombre si es necesario
			if (prop != null)
			{
				data.TitleID = prop.stringValue;
			}
		}
	}
	void OnGUI()
	{
		//se declara lo que contendra toda la interfaz
		scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Width(position.width), GUILayout.Height(position.height));
		//GUILayout.Label("Post Build Patches", EditorStyles.boldLabel);
		Rect backgroundRect = GUILayoutUtility.GetRect(position.width - 15, 110, GUILayout.ExpandWidth(false));
		EditorGUI.DrawRect(backgroundRect, new Color(0.1f, 0.1f, 0.1f, 0.8f)); // Color negro semi-transparente

		GUILayout.Space(-105);
		GUILayout.BeginHorizontal();

		// Ícono grande
		Texture2D icon = EditorGUIUtility.Load("Assets/Editor/RID-Tools/UI/icons/rid logo.png") as Texture2D;
		GUILayout.Label(icon, GUILayout.Width(100), GUILayout.Height(100));

		// Label principal
		GUIStyle largeBoldLabel = new GUIStyle(EditorStyles.label);
		largeBoldLabel.fontSize = 14; // Cambia el tamaño aquí
		//largeBoldLabel.fontStyle = FontStyle.Bold;
		largeBoldLabel.normal.textColor = Color.white; // Opcional: color del texto
		largeBoldLabel.alignment = TextAnchor.MiddleLeft; // Alineación opcional
		//GUILayout.Space(5);
		GUILayout.BeginVertical();
		GUILayout.Space(10);
		GUILayout.Label("Post-build patches for 3DS", largeBoldLabel);
		GUILayout.Space(-5);

		GUILayout.Label("  Credits: \n   - Retro Indie Dev \n   - Dimolades 3DS Stand \n   - annoying \n   - and more...", EditorStyles.boldLabel);
		GUILayout.EndVertical();


		GUILayout.FlexibleSpace(); // Empuja los botones al extremo derecho
								   // Botón de menú desplegable
		GUIStyle flatStyle = new GUIStyle(GUIStyle.none);
		flatStyle.margin = new RectOffset(0, 15, 5, 0);
		flatStyle.padding = new RectOffset(0, 0, 0, 0);

		// Botón de ayuda (abre HTML)
		if (GUILayout.Button(EditorGUIUtility.IconContent("_Help"), flatStyle, GUILayout.Width(16), GUILayout.Height(16)))
		{
			Application.OpenURL("");
		}


		GUILayout.EndHorizontal();
		GUILayout.BeginVertical("box");
		GUILayout.BeginVertical("helpBox");
		GUILayout.Space(3);

		//first config paner
		GUILayout.BeginVertical("helpBox");
		GUILayout.Label("RID-API Panel", EditorStyles.boldLabel);
		
		//nombre del juego a registrar en la api
		data.TitleName = EditorGUILayout.TextField("Title Name: ", data.TitleName);

		//nombre de usuario del desarrollador
		data.TitleDeveloper = EditorGUILayout.TextField("Developer: ", data.TitleDeveloper);

		//api key para rid api, no es necesario para usar
		//solo para registrar informacion directamente en la db, sin tener que esperar revision manual
		data.API_KEY = EditorGUILayout.TextField("API Key: ", data.API_KEY);

		//url de la rid api, puede cambiar en el futuro, o se pueden usar apis compatibles
		data.API_URL = EditorGUILayout.TextField("API URL Endpoint: ", data.API_URL);
		//"https://rid-system.onrender.com/api/apps/"

		//boton para registrar en la db
		GUILayout.Space(5);
		data.TitleID = EditorGUILayout.TextField("TitleID: ", data.TitleID);

		//revisar si el titleid del juego ya esta ocupado por otro juego
		GUI.enabled = !isChecking;
		if (GUILayout.Button("Check Title ID"))
		{
			isChecking = true;

			try
			{
				ridsmalldb.CheckTitleID((result) =>
				{
					if (!result) EditorUtility.DisplayDialog("You can Use this", "This TitleID is not being used by any application registered in the database, you can use it without any problems!", "ok");
					isChecking = false;
					core.SaveXSFields("n3dsApplicationId", data.TitleID);
					Repaint();
				});
			}
			catch (Exception e)
			{
				Debug.Log("error" + e);
				isChecking = false;
				Repaint();
			}

		}

		//generar un titleid aleatorio que no este registrado en rid small db
		GUI.enabled = !isChecking;
		if (GUILayout.Button("Get Ramdom TitleID"))
		{
			isChecking = true;
			try
			{
				data.TitleID = ridsmalldb.GenTitleID();
				core.SaveConfig(data);
				core.SaveXSFields("n3dsApplicationId", data.TitleID);
			}
			catch (Exception e)
			{
				Debug.Log("error" + e);
			}
			isChecking = false;
			Repaint();
		}

		GUI.enabled = true;
		GUILayout.EndVertical();

		//espacio entre ajustes
		GUILayout.Space(2);

		//segunda caja de contenido
		GUILayout.BeginVertical("helpBox");
		GUILayout.Label("CIA Config panel", EditorStyles.boldLabel);

		//autogenerador de archivos .cia al compilar
		data.autoCia = EditorGUILayout.Toggle("AutoCIA (No Patches)", data.autoCia);
		data.autoCiaPatcher = EditorGUILayout.Toggle("AutoCIA Patcher", data.autoCiaPatcher);

		//incluir manual del juego
		data.hasManual = EditorGUILayout.Toggle("include manual", data.hasManual);

		//forzar un system mode diferente al por defecto
		string[] opciones = new[] { "32MB", "64MB", "72MB", "80MB", "96MB" };
		int index = Array.IndexOf(opciones, data.memoryMode);
		if (index == -1) index = 1;
		index = EditorGUILayout.Popup("Set SystemMode to: ", index, opciones);
		data.memoryMode = opciones[index];

		//ruta al archivo .cci
		EditorGUILayout.BeginHorizontal();
		data.cciPath = EditorGUILayout.TextField("Path to .cci File", data.cciPath);
		if (GUILayout.Button("Browse", GUILayout.Width(60)))
		{
			string ruta = EditorUtility.OpenFilePanel("Select .cci", Environment.CurrentDirectory, "cci");
			if (!string.IsNullOrEmpty(ruta)) data.cciPath = ruta;
		}
		EditorGUILayout.EndHorizontal();

		//carpeta donde guardar el archivo cia resultante
		EditorGUILayout.BeginHorizontal();
		data.ciaOutputPath = EditorGUILayout.TextField("Output .cia file", data.ciaOutputPath);
		if (GUILayout.Button("Browse", GUILayout.Width(60)))
		{
			string ruta = EditorUtility.OpenFolderPanel("Save .cia here", Environment.CurrentDirectory, "builds");
			if (!string.IsNullOrEmpty(ruta)) data.ciaOutputPath = ruta;
		}
		EditorGUILayout.EndHorizontal();
		GUILayout.EndVertical();

		//espacio entre ajustes
		GUILayout.Space(2);
		
		//tercera caja de configuracion, exclusivos de new 3ds
		GUILayout.BeginVertical("helpBox");
		GUILayout.Label("SNAKE Only Settings", EditorStyles.boldLabel);

		//habilitar 804Mhz en new3ds
		data.superSpeed = EditorGUILayout.Toggle("Enable 804MHz CPU", data.superSpeed);

		//habilitarl cache l2
		data.l2cache = EditorGUILayout.Toggle("Enable L2 Cache ", data.l2cache);

		//habilitar segundo nucleo
		data.EnableCore2 = EditorGUILayout.Toggle("Enable Core2 ", data.EnableCore2);

		//forzar un system mode ext diferente al default
		string[] options = new[] { "Legacy", "124MB", "178MB" };
		int indx = Array.IndexOf(options, data.memoryModeExt);
		if (indx == -1) indx = 1;
		indx = EditorGUILayout.Popup("Set SystemModeExt to: ", indx, options);
		data.memoryModeExt = options[indx];
		GUILayout.EndVertical();


		//funciones adicionales
		GUILayout.Space(2);
		GUILayout.BeginVertical("helpBox");
		GUILayout.Label("Other settings", EditorStyles.boldLabel);
		GUILayout.Space(2);

		GUILayout.BeginHorizontal();

		//izquierda
		GUILayout.BeginVertical();
		data.sdmcAccess = EditorGUILayout.Toggle("Direct SDMC Access", data.sdmcAccess);
		data.debugFlag = EditorGUILayout.Toggle("Debug", data.debugFlag);
		data.forceCompress = EditorGUILayout.Toggle("F. Compress StaticMem", data.forceCompress);
		data.writeSharedPage = EditorGUILayout.Toggle("Write Shared Page", data.writeSharedPage);
		data.PrivilegedPriority = EditorGUILayout.Toggle("Privileged Priority", data.PrivilegedPriority);
		data.NonAlphabetAndNumber = EditorGUILayout.Toggle("Allow Non-Alphanumeric", data.NonAlphabetAndNumber);
		GUILayout.EndVertical();

		GUILayout.BeginVertical();
		data.MainFunctionArgument = EditorGUILayout.Toggle("Main Function Arg", data.MainFunctionArgument);
		data.sharedevicemem = EditorGUILayout.Toggle("Share Device Memory", data.sharedevicemem);
		data.runOnSleep = EditorGUILayout.Toggle("Runnable on Sleep", data.runOnSleep);
		data.SpecialMem = EditorGUILayout.Toggle("Special Memory Arrange", data.SpecialMem);
		data.forceDebug = EditorGUILayout.Toggle("Force Debug", data.forceDebug);
		data.disableDebug = EditorGUILayout.Toggle("Disable Debug", data.disableDebug);

		GUILayout.EndVertical();

		GUILayout.EndHorizontal();
		GUILayout.Space(2);
		GUILayout.EndVertical();

		//GUILayout.EndVertical();
		GUILayout.Space(2);

		//caja de botones importantes
		GUILayout.BeginVertical("helpBox");

		//boton de guardar configuracion
		GUI.enabled = !isChecking;
		if (GUILayout.Button("Save Config"))
		{
			isChecking = true;
			core.SaveConfig(data);
			core.SaveXSFields("n3dsApplicationId", data.TitleID);
			//core.SavePSBoolFields("n3dsAllowDirectSDMC", data.sdmcAccess);
			isChecking = false;
			Repaint();
		}

		//boton de guardar configuracion y enviar titleid a la rid api
		GUI.enabled = !isChecking;
		if (GUILayout.Button("Save & Submit TitleID"))
		{
			try
			{
				isChecking = true;
				core.SaveConfig(data);
				core.SaveXSFields("n3dsApplicationId", data.TitleID);
				//core.SavePSBoolFields("n3dsAllowDirectSDMC", data.sdmcAccess);
				ridsmalldb.CheckTitleID((exists) =>
				{
					if (exists)
					{
						isChecking = false;
						Repaint();
						return;
					}
					else
					{
						ridsmalldb.SubmitTitleID(data.TitleName, data.TitleID, data.TitleDeveloper, data.API_KEY, (result) =>
						{
							EditorUtility.DisplayDialog("Successful registration", "TitleID was successfully sent to RID SmallDB", "OK");
							isChecking = false;
							Repaint();
						});
					}
					isChecking = false;
				});
			}
			catch (Exception e)
			{
				Debug.LogError(e);
				isChecking = false;
				Repaint();
			}
		}

		//boton para crear un archivo .cia
		if (GUILayout.Button("Create CIA from cci"))
		{
			toCia.makecia();
			//solo crea el .cia, no lo modifica
		}

		//boton para crear un archivo .cia con los parches indicados en la configuracion
		if (GUILayout.Button("Make CIA with Patches"))
		{
			rebuild.rebuildcia();
			//desarma el cci, extrae el exheader, lo modifica y vuelve a armar pero en cia
		}
		GUILayout.EndVertical();


		//cajas exteriores
		GUILayout.Space(3);
		GUILayout.EndVertical();
		GUILayout.EndVertical();

		//me dio la gana de poner un espacio aqui, algun problema? >:3
		GUILayout.EndScrollView();
	}

}