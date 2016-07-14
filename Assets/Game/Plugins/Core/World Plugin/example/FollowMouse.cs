using UnityEngine;
using System.Collections;

public class FollowMouse : Entity {
	private bool moving;

	public override void Act ()
	{
		if (Input.GetKey (KeyCode.Q) && !moving) {
			LocalMove (1f, World.FindRotation (transform.position, Camera.main.ScreenToWorldPoint (Input.mousePosition)));
			LocalChangeTempPlayer ();
			moving = true;
		} 

		if (moving && !Input.GetKey (KeyCode.Q)){
			LocalStopMove ();
			LocalRemoveTempPlayer ();
			moving = false;
		}
	}
}
