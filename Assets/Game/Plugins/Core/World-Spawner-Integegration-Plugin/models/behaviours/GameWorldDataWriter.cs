using UnityEngine;
using System.Collections;

public class GameWorldDataWriter : MonoBehaviour {
	public int terrain = -1;
	public string type = "";

	void OnUpdate () {


		//Get the world object of this terrain object
		WorldObject w = GetComponent<WorldObject> ();

		//If a world is being created, and the terrain var has been set, write the world id
		if (terrain != -1) {


			if (w.vars.ContainsKey (StringValue.GetStringValue(BaseGameConstants.BaseGameVars.TERRAIN))) {
				
				w.vars [StringValue.GetStringValue(BaseGameConstants.BaseGameVars.TERRAIN)] = 
					terrain.ToString ();
				
				terrain = -1;
			
			} else {
				
				w.vars.Add (
					StringValue.GetStringValue(BaseGameConstants.BaseGameVars.TERRAIN), 
					terrain.ToString ()
				);

				terrain = -1;

			}


		}

		//If type is not an empty string, write the biome type of the world
		if (type != "") {
			
			if (w.vars.ContainsKey (StringValue.GetStringValue(BaseGameConstants.BaseGameVars.BIOME))) {
				
				w.vars [StringValue.GetStringValue(BaseGameConstants.BaseGameVars.BIOME)] = type;

				type = "";

			} else {
				
				w.vars.Add (StringValue.GetStringValue(BaseGameConstants.BaseGameVars.BIOME), type);

				type = "";

			}
				
		}

		//If this terrain object has been written to, and the world is empty, create the world
		if (World.currentTerrain == null && w.vars.ContainsKey (StringValue.GetStringValue (BaseGameConstants.BaseGameVars.TERRAIN))) {
			
			World.currentTerrain = (Transform)Instantiate (
				
				TerrainConverter.main.terrain [
					int.Parse (w.vars [StringValue.GetStringValue (BaseGameConstants.BaseGameVars.TERRAIN)])],
				
				new Vector3 (0, 0, 0), Quaternion.identity

			);

		}


	}
}
