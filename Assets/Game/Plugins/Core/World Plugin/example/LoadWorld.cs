using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LoadWorld : MonoBehaviour {
	public Text load;
	public InputField field;
	public void pressed(){
		WorldDatabase.currentWorldID = load.text;
		Client.main.StartClient (YamlConfig.config.ip, YamlConfig.config.port);
		Destroy(GameObject.Find("Selection Screen(Clone)"));
	}

	public void enter(){
		WorldDatabase.currentWorldID = field.text;
		Client.main.StartClient (YamlConfig.config.ip, YamlConfig.config.port);
		Destroy(GameObject.Find("Selection Screen(Clone)"));
	}
}
