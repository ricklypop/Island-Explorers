using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using System.Text;
using YamlDotNet;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using System;

public class YamlConfig : MonoBehaviour {
	public static Config config;//The static loaded config

	#region Config Defaults
	public static readonly int MAXBYTES = 1200;
	public static readonly ushort PACKETSIZE = 1400;
	public static readonly ushort MAXSENDMESSAGES = 128;
	public static readonly int MINTOTALPLAYER = 1;

	public static readonly string CONFIGADDRESS = "http://130.68.237.16:8080/WorldService/ws";
	public static readonly string DIRECTORYIP = "108.53.152.232";
	public static readonly int DIRECTORYPORT = 7070;
	public static readonly int VERSION = 1;
	#endregion

	public static readonly string WORLD_DATABASE_ADDRESS_ENV = "WORLD_DATABASE_ADDRESS";
	public static readonly string DIRECTORY_IP_ENV = "WORLD_DIRECTORY_ADDRESS";
	public static readonly string DIRECTORY_PORT_ENV = "WORLD_DIRECTORY_PORT";

	#region Script Functions
	void Start(){
		string load = LoadConfig ();
		if (load == null || load == "")
			CreateDefault ();
		else{
			StringReader input = new StringReader (load);
			Deserializer deserialize = new Deserializer (namingConvention: new CamelCaseNamingConvention());
			config = deserialize.Deserialize<Config> (input);
			SetEnviroment ();
			if (config.version != VERSION)
				CreateDefault ();
		}
	}
	#endregion

	void SetEnviroment(){
		if (Environment.GetEnvironmentVariable(WORLD_DATABASE_ADDRESS_ENV) == null) {
			Environment.SetEnvironmentVariable (WORLD_DATABASE_ADDRESS_ENV, config.address);
		}

		if (Environment.GetEnvironmentVariable(DIRECTORY_IP_ENV) == null) {
			Environment.SetEnvironmentVariable (DIRECTORY_IP_ENV, config.ip);
		}

		if (Environment.GetEnvironmentVariable(DIRECTORY_PORT_ENV) == null) {
			Environment.SetEnvironmentVariable (DIRECTORY_PORT_ENV, config.port.ToString());
		}
			
	}

	#region Config Functions
	/// <summary>
	/// Creates a default config.
	/// </summary>
	void CreateDefault(){
		config = new Config ();
		config.address = CONFIGADDRESS;
		config.ip = DIRECTORYIP;
		config.port = DIRECTORYPORT;
		config.version = VERSION;

		config.maxBytes = MAXBYTES;
		config.minTotalPlayers = MINTOTALPLAYER;
		config.packetSize = PACKETSIZE;
		config.maxSendMessages = MAXSENDMESSAGES;

		StringBuilder stringBuilder = new StringBuilder ();
		StringWriter stringWriter = new StringWriter (stringBuilder);

		Serializer serializer = new Serializer ();
		serializer.Serialize (stringWriter, config);
		SaveConfig (stringBuilder.ToString());
	}

	/// <summary>
	/// Loads the config.
	/// </summary>
	/// <returns>The config.</returns>
	string LoadConfig ()
	{
		try {
			XmlSerializer xml = new XmlSerializer (typeof(string));
			FileStream file;
			file = new FileStream (Application.persistentDataPath + "/config.ini", FileMode.Open, FileAccess.Read, FileShare.None);
			string con = xml.Deserialize (file).ToString (); 
			file.Close ();
			return con;
		} catch (Exception e) {
			Debug.Log ("File does not exist! Error: " + e.StackTrace);
		}
		return null;
	}

	/// <summary>
	/// Saves the config.
	/// </summary>
	void SaveConfig (string config)
	{
		XmlSerializer xml = new XmlSerializer (typeof(string));
		FileStream file = File.Create (Application.persistentDataPath + "/config.ini");

		xml.Serialize (file, config);
		file.Close ();
	}
	#endregion
}
