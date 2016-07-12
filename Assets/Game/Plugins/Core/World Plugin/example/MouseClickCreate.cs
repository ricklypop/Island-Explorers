using UnityEngine;
using System.Collections;

public class MouseClickCreate : MonoBehaviour {
	public Transform create;
	void Update () {
		if (Client.main != null && Client.main.gotWorld) {
			if (Input.GetMouseButtonDown (0)) {
				transform.position = Camera.main.ScreenToWorldPoint (Input.mousePosition);
				transform.position = new Vector3 (transform.position.x, transform.position.y, 0);
				transform.rotation = create.rotation;
				transform.localScale = create.localScale;
				SerializableTransform t = new SerializableTransform (transform);
				ObjectCommunicator.LocalCreateWorldObject (create, t, "");
			}

			if (Input.touchCount != 0 && Input.GetTouch (0).phase == TouchPhase.Began) {
				transform.position = Camera.main.ScreenToWorldPoint (Input.GetTouch (0).position);
				transform.position = new Vector3 (transform.position.x, transform.position.y, 0);
				transform.rotation = create.rotation;
				transform.localScale = create.localScale;
				SerializableTransform t = new SerializableTransform (transform);
				ObjectCommunicator.LocalCreateWorldObject (create, t, "");
			}
		}
	}
}
