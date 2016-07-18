using UnityEngine;
using System.Collections;

public class InitWorldSystem : MonoBehaviour {
	void Start(){
		foreach (string s in Constants.VARS)
			Constants.vars.Add (s);
		foreach (string s in Constants.METHODS)
			Constants.methods.Add (s);

		StartCoroutine (World.GenerateWorld());
	}
}
