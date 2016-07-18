using UnityEngine;
using System.Collections;

public class WorldTreeObject : TreeSpawnObject {
	#region implemented abstract members of TreeSpawnObject

	public override void OnDespawn ()
	{
		WorldObjectCache.Remove (GetComponent<WorldObject>().id);
	}

	public override void OnSpawn (Transform spawn)
	{
		string id = UniqueIDGenerator.GetUniqueID ();
		spawn.GetComponent<WorldObject> ().id = id;
		WorldObjectCache.Add (id, spawn.GetComponent<WorldObject> ());
	}

	#endregion


}
