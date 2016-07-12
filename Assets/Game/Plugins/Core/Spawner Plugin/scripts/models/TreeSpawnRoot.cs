using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TreeSpawnRoot : MonoBehaviour {
	public static List<TreeSpawnRoot> roots = new List<TreeSpawnRoot> ();

	public float maxDistance;
	public int minSpawn;
	public int maxSpawn;
	public bool started;

	private float time;
	private int current;

	void Start(){
		roots.Add (this);
	}

	public void Generate(){
		started = true;
		GetComponent<TreeSpawnObject> ().SpawnTree (transform.position, maxDistance, maxSpawn, this);
	}

	void Update(){
		time += Time.deltaTime;
		if (time >= 0.1f && started && current < minSpawn) {
			Generate ();
		} else if (time >= 0.1f && started && current > minSpawn)
			started = false;
	}

	public void UpdateCurrent(){
		current++;
		time = 0;
	}

	public void Destroyed(){
		current--;
	}

	public int GetCurrent(){
		return current;
	}
}

public class Spawn{
	public Vector3 pos{ get; set; }
	public int spawn { get; set; }
	public TreeSpawnRoot root { get; set; }
	public float max { get; set; } 
	public int maxSpawn { get; set; }

	public Spawn(Vector3 pos,  float max, int maxSpawn, int spawn, TreeSpawnRoot root){
		this.pos = pos;
		this.spawn = spawn;
		this.root = root;
		this.max = max;
		this.maxSpawn = maxSpawn;
	}

}
