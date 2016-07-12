using UnityEngine;
using System.Collections;

public class WorldDataWriter : MonoBehaviour {
	public int terrain = -1;
	public string type = "";

	void Update () {
		//Get the world object of this terrain object
		WorldObject w = GetComponent<WorldObject> ();

		//If a world is being created, and the terrain var has been set, write the world id
		if (terrain != -1) {
			if (w.vars.ContainsKey ("t")) {
				w.vars ["t"] = terrain.ToString ();
				terrain = -1;
			} else {
				w.vars.Add ("t", terrain.ToString ());
				terrain = -1;
			}
		}

		//If type is not an empty string, write the biome type of the world
		if (type != "") {
			if (w.vars.ContainsKey ("b")) {
				w.vars ["b"] = type;
				type = "";
			} else {
				w.vars.Add ("b", type);
				type = "";
			}
		}

		//If this terrain object has been written to, and the world is empty, create the world
		if (World.currentTerrain == null && w.vars.ContainsKey ("t"))
			World.currentTerrain = (Transform)Instantiate (TerrainConverter.main.terrain [int.Parse(w.vars ["t"])]
				,new Vector3 (0, 0, 0), Quaternion.identity);
	}
}
