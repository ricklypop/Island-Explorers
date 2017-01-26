using UnityEngine;
using System.Collections;

public class TestSpawning : MonoBehaviour {
	bool gen;
	void Update () {
		if (!gen) {
			foreach (TreeSpawnRoot r in TreeSpawnRoot.roots) {
				Debug.Log ("Generating " + r);
				r.Generate ();
			}
			gen = true;
		}
	}
}
