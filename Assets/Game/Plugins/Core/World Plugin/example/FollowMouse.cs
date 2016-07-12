using UnityEngine;
using System.Collections;

public class FollowMouse : MonoBehaviour, Action {
	private bool moving;

	public void Act (Entity e)
	{
		if (Input.GetKey (KeyCode.Q) && !moving) {
			e.LocalMove (1f, World.FindRotation (transform.position, Camera.main.ScreenToWorldPoint (Input.mousePosition)));
			e.LocalChangeTempPlayer ();
			moving = true;
		} 

		if (moving && !Input.GetKey (KeyCode.Q)){
			e.LocalStopMove ();
			e.LocalRemoveTempPlayer ();
			moving = false;
		}
	}
}
