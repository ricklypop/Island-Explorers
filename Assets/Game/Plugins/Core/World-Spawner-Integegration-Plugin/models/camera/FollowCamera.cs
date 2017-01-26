using UnityEngine;
using System.Collections;

public class FollowCamera : MonoBehaviour {
	public float height;
	public Transform follow;

	void Update () {
		if (Terrain.activeTerrain != null) {
			float y = Terrain.activeTerrain.SampleHeight (transform.position);
			if (transform != null)
				transform.position = new Vector3 (follow.position.x, y + height, follow.position.z);
		}
	}
}
