using UnityEngine;
using System.Collections;

public class DatabaseRetry: MonoBehaviour {
	public static DatabaseRetry main;

	public Transform display;

	public Transform currentDisplay{ get; set; }
	public bool show { get; set; }
	public Transform retry{ get; set; }
	public string message{ get; set; }

	private float time;
	private float opactity;

	void Start(){
		main = this;
	}

	void Update(){
		if (show && currentDisplay == null) {
			currentDisplay = Instantiate (display);
		} else if (show && currentDisplay != null) {
			if (opactity < 1)
				opactity += Constants.OPACADD;
			currentDisplay.GetComponent<CanvasGroup> ().alpha = opactity;
		}else if(!show && currentDisplay != null){
			if (opactity > 0)
				opactity -= Constants.OPACADD;
			currentDisplay.GetComponent<CanvasGroup> ().alpha = opactity;
			if (opactity == 0)
				Destroy (currentDisplay);
		}

		if (retry != null && Constants.RETRYTIME <= time)
			Retry ();
		else if(retry != null)
			time += Time.deltaTime;
	}

	void Retry(){
		retry.SendMessage (message);
		retry = null;
		message = "";
	}

	public void SetRetry(Transform t, string s){
		retry = t;
		message = s;
		show = true;
	}
}
