using UnityEngine;
using System.Collections;

public class AddCamera : MonoBehaviour {
	void Start () {
		GetComponent<CanvasGroup> ().alpha = 0f;
		GetComponent<Canvas> ().worldCamera = Camera.main;
	}

	void OnGUI(){
		this.GetComponent<Canvas> ().renderMode = RenderMode.WorldSpace;
		GetComponent<CanvasGroup> ().alpha = 1f;
		transform.position = new Vector3 (transform.position.x, transform.position.y, 0);
		Destroy (this);
	}
}
