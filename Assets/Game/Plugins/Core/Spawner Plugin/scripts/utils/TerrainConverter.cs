using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TerrainConverter : MonoBehaviour {
	public static TerrainConverter main;

	public List<Transform> terrain = new List<Transform>();
	public List<string> type = new List<string> ();
	public Transform worldData;

	void Start(){
		main = this;
	}
}
