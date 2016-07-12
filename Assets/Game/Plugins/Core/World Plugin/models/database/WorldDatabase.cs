using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Networking;
using DisableLogging;
using System;
using System.Xml.Serialization;
using System.IO;
using System.Text;

public class WorldDatabase: MonoBehaviour{
	#region Static Database Values
	public static WorldDatabase database;
	public static string currentWorldID = SystemInfo.deviceUniqueIdentifier;
	public static List<SerializableWorldObject> world { get; set; }
	public static SortedList<CharString, string> worldList = new SortedList<CharString, string>();
	public static bool gettingWorld = false;
	public static bool getLastWorld = false;
	public static string checkID = "";
	public static string worldName = "";
	public static string address = null;
	#endregion

	#region Script Functions
	void Update ()
	{
		if (address == null && YamlConfig.config != null) {
			address = Environment.GetEnvironmentVariable (YamlConfig.WORLD_DATABASE_ADDRESS_ENV);
			if (address != null) {
				database = this;
				StartCoroutine (GetWorldList ());
				StartCoroutine(CheckID ());
			}
		}

		if (checkID == "true") {
			checkID = "";
			StartCoroutine (GetLastWorld ());
		} else if (checkID == "false") {
			checkID = "";
			World.GenerateWorld ();
			currentWorldID = SystemInfo.deviceUniqueIdentifier;
			StartCoroutine (PutID ());
		}

		if (getLastWorld) {
			getLastWorld = false;
			Client.main.StartClient (YamlConfig.config.ip, YamlConfig.config.port);
		}
	}

	public void StartPutWorld(){
		StartCoroutine (PutWorld ());
	}

	public void StartCheckID(){
		StartCoroutine (CheckID ());
	}
	#endregion

	#region Get Calls
	/// <summary>
	/// Gets the world list of ids.
	/// </summary>
	public IEnumerator GetWorldList ()
	{
		string url = address + "/getWorldList";
		WWWForm form = new WWWForm ();

		Dictionary<string, string> headers = form.headers;
		headers ["Authorization"] = "Basic " + System.Convert.ToBase64String (
			System.Text.Encoding.ASCII.GetBytes (SystemInfo.deviceUniqueIdentifier + ": "));

		WWW www = new WWW (url, null, headers);
		yield return www;
		Debug.Log (www.text);
		worldList.Clear ();
		Dictionary<string, string> response = new Dictionary<string, string> ();
		if(www.text.Replace(" ", "") != "" && www.text.Replace(" ", "") != "{}" )
			response = JsonConvert.DeserializeObject<Dictionary<string, string>>(www.text);
		if(response == null)
			response = new Dictionary<string, string>();
		foreach (string key in response.Keys)
			worldList.Add (new CharString(key), response [key]);
		if (www.text.Replace (" ", "") == "")
			DatabaseRetry.main.SetRetry (transform, "GetWorldList");
		else
			DatabaseRetry.main.show = false;
	}

	/// <summary>
	/// Gets the world based on a given id.
	/// </summary>
	public IEnumerator GetWorld (string id)
	{
		gettingWorld = true;
		string url = address + "/getWorld/" + id;
		WWWForm form = new WWWForm ();

		Dictionary<string, string> headers = form.headers;
		headers ["Authorization"] = "Basic " + System.Convert.ToBase64String (
			System.Text.Encoding.ASCII.GetBytes (SystemInfo.deviceUniqueIdentifier + ": "));

		WWW www = new WWW (url, null, headers);
		yield return www;
		Debug.Log (www.text);

		try{
			world = JsonConvert.DeserializeObject<List<SerializableWorldObject>> (www.text);
		}catch(Exception e){
			Debug.Log(e.StackTrace);
		}
		if(world == null)
			world = new List<SerializableWorldObject> ();
		World.CreateWorld ();
		gettingWorld = false;
	}

	/// <summary>
	/// Gets the name of the world.
	/// </summary>
	public IEnumerator GetName (string id)
	{
		string url = address + "/getWorldName/" + id;
		WWWForm form = new WWWForm ();

		Dictionary<string, string> headers = form.headers;
		headers ["Authorization"] = "Basic " + System.Convert.ToBase64String (
			System.Text.Encoding.ASCII.GetBytes (SystemInfo.deviceUniqueIdentifier + ": "));

		WWW www = new WWW (url, null, headers);
		yield return www;
		Debug.Log (www.text);

		worldName = www.text;
	}

	/// <summary>
	/// Gets the last world the unique id was on.
	/// </summary>
	public IEnumerator GetLastWorld()
	{
		getLastWorld = false;
		string url = address + "/getLastWorld";
		WWWForm form = new WWWForm ();

		Dictionary<string, string> headers = form.headers;
		headers ["Authorization"] = "Basic " + System.Convert.ToBase64String (
			System.Text.Encoding.ASCII.GetBytes (SystemInfo.deviceUniqueIdentifier + ": "));

		WWW www = new WWW (url, null, headers);
		yield return www;
		Debug.Log (www.text);

		currentWorldID = www.text;
		getLastWorld = true;
	}

	/// <summary>
	/// Checks the ID to see if it exists.
	/// </summary>
	public IEnumerator CheckID ()
	{
		string url = address + "/checkID/" + SystemInfo.deviceUniqueIdentifier;
		WWWForm form = new WWWForm ();

		Dictionary<string, string> headers = form.headers;

		WWW www = new WWW (url, null, headers);
		yield return www;
		Debug.Log (www.text);
		checkID = www.text;
		if (www.text.Replace (" ", "") == "")
			DatabaseRetry.main.SetRetry (transform, "CheckID");
		else
			DatabaseRetry.main.show = false;
	}
	#endregion

	#region Put Calls
	/// <summary>
	/// Puts the device ID in the database.
	/// </summary>
	public IEnumerator PutID ()
	{
		string url = address + "/putID";
		WWWForm form = new WWWForm();

		form.AddField("id", SystemInfo.deviceUniqueIdentifier);
		Dictionary<string, string> headers = form.headers;
		byte[] rawData = form.data;

		WWW www = new WWW(url, rawData, headers);
		yield return www;
		StartCoroutine (PutLastWorld ());
	}

	/// <summary>
	/// Puts the world in the database.
	/// </summary>
	public IEnumerator PutWorld ()
	{
		string url = address + "/putWorld";
		WWWForm form = new WWWForm();

		form.AddField("world", JsonConvert.SerializeObject(world));
		form.AddField("id", currentWorldID);
		Dictionary<string, string> headers = form.headers;
		headers ["Authorization"] = "Basic " + System.Convert.ToBase64String (
			System.Text.Encoding.ASCII.GetBytes (SystemInfo.deviceUniqueIdentifier + ": "));
		byte[] rawData = form.data;

		WWW www = new WWW(url, rawData, headers);
		yield return www;
	}

	/// <summary>
	/// Puts the last world id, the unique device id was on.
	/// </summary>
	public IEnumerator PutLastWorld (){
		string url = address + "/putLastWorld";
		WWWForm form = new WWWForm();

		form.AddField("lastWorld", currentWorldID);
		Dictionary<string, string> headers = form.headers;
		headers ["Authorization"] = "Basic " + System.Convert.ToBase64String (
			System.Text.Encoding.ASCII.GetBytes (SystemInfo.deviceUniqueIdentifier + ": "));
		byte[ ] rawData = form.data;

		WWW www = new WWW(url, rawData, headers);
		yield return www;
	}

	/// <summary>
	/// Puts the name of the world in the database.
	/// </summary>
	public IEnumerator PutWorldName (string name)
	{
		string url = address + "/putWorldName";
		WWWForm form = new WWWForm();

		form.AddField("worldName", name);
		Dictionary<string, string> headers = form.headers;
		headers ["Authorization"] = "Basic " + System.Convert.ToBase64String (
			System.Text.Encoding.ASCII.GetBytes (SystemInfo.deviceUniqueIdentifier + ": "));
		byte[] rawData = form.data;

		WWW www = new WWW(url, rawData, headers);
		yield return www;
	}
	#endregion
}
