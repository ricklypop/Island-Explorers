using UnityEngine;
using System.Collections;

public class TestCamera : MonoBehaviour {
	public float height;

	void Update () {
		float y = Terrain.activeTerrain.SampleHeight (transform.position);

		if (Input.GetKey (KeyCode.W))
			transform.position += transform.forward;

		if (Input.GetKey (KeyCode.A))
			transform.position -= transform.right;

		if (Input.GetKey (KeyCode.S))
			transform.position -= transform.forward;

		if (Input.GetKey (KeyCode.D))
			transform.position += transform.right;

		transform.position = new Vector3 (transform.position.x, y + height, transform.position.z);
	}
}
