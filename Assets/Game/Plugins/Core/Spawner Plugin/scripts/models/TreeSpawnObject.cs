using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class TreeSpawnObject : MonoBehaviour {
	public List<float> minDistance = new List<float>();
	public List<float> maxDistance = new List<float>();
	public int minSpawn;
	public int maxSpawn;
	public List<Transform> extentions = new List<Transform>();
	public List<int> canSpawnOn = new List<int> ();
	public bool slopeTilt = false;
	public bool spin = false;

	private Queue<Spawn> spawns = new Queue<Spawn> ();


	public void SpawnTree(Vector3 origin, float max, int maxSpawn, TreeSpawnRoot root){
		if ((!canSpawnOn.Contains (ProceduralMethods.GetTexture (transform.position)) || CheckTerrain ()) && root.gameObject != this.gameObject) {
			root.Destroyed ();
			OnDespawn ();
//			WORLD OBJECT SPAWN EXAMPLE
//			WorldObjectCache.Remove (GetComponent<WorldObject>().id);
			Destroy (this.gameObject);
		} 

		if (extentions.Count > 0) {
			int randomAmount = Random.Range (this.minSpawn, this.maxSpawn);
			int randomTree;
			Vector3 angle;
			Vector3 pos;
			float distance;

			for (int i = 0; i < randomAmount; i++) {
				if (root.GetCurrent () < maxSpawn) {
					System.Random rand = new System.Random ();
					randomTree = rand.Next (minDistance.Count);
					angle = new Vector3 (Random.Range (-1f, 1f), Random.Range (-1f, 1f), Random.Range (-1f, 1f));
					distance = Random.Range (minDistance[randomTree], maxDistance[randomTree]);
					pos = new Vector3 (
						transform.position.x + (angle.normalized.x * distance), 
						transform.position.y + (angle.normalized.y * distance),
						transform.position.z + (angle.normalized.z * distance));
					Vector3 finalPos = new Vector3 (Mathf.RoundToInt(pos.x), 
						Mathf.RoundToInt(Terrain.activeTerrain.SampleHeight (pos) + TerrainConstants.SPAWNHEIGHT),
						Mathf.RoundToInt(pos.z));
					if (Vector3.Distance (origin, finalPos) < max && finalPos.x < TerrainConstants.MAXMAPSIZE && finalPos.z < TerrainConstants.MAXMAPSIZE) {
						root.UpdateCurrent ();
						spawns.Enqueue (new Spawn(finalPos, max, maxSpawn, randomTree, root));
					}
				}
			}
		}
	}

	void Update(){
		for (int i = 0; i < spawns.Count; i ++) {
			Spawn spawn = spawns.Dequeue ();
			Transform t = SpawnObject (extentions[spawn.spawn], spawn.pos);
			if (t != null)
				t.GetComponent<TreeSpawnObject> ().SpawnTree (spawn.root.transform.position, spawn.max, spawn.maxSpawn, spawn.root);
		}
	}

	bool CheckTerrain(){
		if (Terrain.activeTerrain.SampleHeight (transform.position) >= transform.position.y)
			return false;
		else
			return true;
	}

	Transform SpawnObject ( Transform prefab, Vector3 worldPos  ){
		RaycastHit hit = new RaycastHit();
		if (Physics.Raycast(worldPos, Vector3.down, out hit)) {
			Transform spawn = null;
			if (prefab != null) {
				try{
					
					spawn = (Transform)Instantiate (prefab, hit.point, Quaternion.identity);
					if(spawn.GetComponent<TreeSpawnObject>().spin)
						spawn.eulerAngles = new Vector3 (spawn.eulerAngles.x, Random.Range (0, 359), spawn.eulerAngles.z);
					
					if(spawn.GetComponent<TreeSpawnObject>().slopeTilt)
						spawn.rotation = Quaternion.FromToRotation(spawn.up, hit.normal) * spawn.rotation;

					OnSpawn (spawn);
//					WORLD OBJECT SPAWN EXAMPLE
//					string id = UniqueIDGenerator.GetUniqueID ();
//					go.GetComponent<WorldObject> ().id = id;
//					WorldObjectCache.Add (id, go.GetComponent<WorldObject> ());

				}catch(System.Exception e){
					Debug.Log (spawn);
				}
			}
			return spawn;
		}
		return null;
		
	}

	public Texture getTerrainTextureAt( Vector3 position )
	{
		// Set up:
		Texture retval    =    new Texture();
		Vector3 TS; // terrain size
		Vector2 AS; // control texture size

		TS = Terrain.activeTerrain.terrainData.size;
		AS.x = Terrain.activeTerrain.terrainData.alphamapWidth;
		AS.y = Terrain.activeTerrain.terrainData.alphamapHeight;


		// Lookup texture we are standing on:
		int AX = (int)( ( position.x/TS.x )*AS.x + 0.5f );
		int AY = (int)( ( position.z/TS.z )*AS.y + 0.5f );
		float[,,] TerrCntrl = Terrain.activeTerrain.terrainData.GetAlphamaps(AX, AY,1 ,1);

		TerrainData TD = Terrain.activeTerrain.terrainData;

		for( int i = 0; i < TD.splatPrototypes.Length; i++ )
		{
			if( TerrCntrl[0,0,i] > .5f )
			{
				retval = TD.splatPrototypes[i].texture;
			}

		}


		return retval;
	}

	public abstract void OnDespawn ();

	public abstract void OnSpawn (Transform spawn);
}
