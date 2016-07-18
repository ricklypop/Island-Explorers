using UnityEngine;
using System.Collections;

public class GameWorld: World {

	public override IEnumerator OnGenerateWorld(){
		//Generate a terrain id
		int terrain = Random.Range (0, TerrainConverter.main.terrain.Count);
		//Create a new world data object, for the world
		Transform t = (Transform) GameObject.Instantiate(TerrainConverter.main.worldData, new Vector3(0,0,0), Quaternion.Euler(0,0,0));
		//The world data object contains a writer
		//Write the world id and type using the writer
		t.GetComponent<GameWorldDataWriter> ().terrain = terrain;
		t.GetComponent<GameWorldDataWriter> ().type = TerrainConverter.main.type [terrain];
		//Add the world data to the new world
		WorldObjectCache.Add (UniqueIDGenerator.GetUniqueID(), t.GetComponent<WorldObject>());
		//Give a second to make sure the terrain is created
		yield return new WaitForSeconds (1);
		//Generate the world, based on its roots
		foreach (TreeSpawnRoot r in TreeSpawnRoot.roots)
			r.Generate ();
		//Wait while generating
		while(Generating())
			yield return new WaitForSeconds (1);
		//Save the world to the database
		SaveWorld ();
		//Check ID and start the client
		WorldDatabase.database.StartCheckID ();
	}

	private bool Generating(){
		foreach (TreeSpawnRoot r in TreeSpawnRoot.roots)
			if (r.started)
				return true;
		return false;
	}
}
