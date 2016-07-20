using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

public class GameWorldPlayer : Entity {
	public float movementVelocity = BaseGameConstants.DEFAULTMOVEMENTVELOCITY;

	private float time;

	protected override void OnEntityStart(){
		if (!obj.vars.ContainsKey ("pX")) {
			obj.vars.Add ("pX", "");
			obj.vars.Add ("pY", "");
		}
	}

	#region Action implementation
	public override void Act ()
	{
		if (Input.GetMouseButtonUp (0)) {
			Vector3 point = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			LocalMovePoint (new Vector2 (point.x, point.z), movementVelocity);
		}
			
		if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended) {
			Vector3 point = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
			LocalMovePoint (new Vector2 (point.x, point.z), movementVelocity);
		}

	}
	#endregion

	protected override void OnEntityUpdate(){
		if (obj.vars ["pX"] != "" && obj.vars ["pY"] != "" && time >= BaseGameConstants.POINTUPDATETIME) {
			UpdatePoint ();
			time = 0;
		}else if(obj.vars ["pX"] != "" && obj.vars ["pY"] != "")
			time += Time.deltaTime;

		if (obj.vars ["pX"] != "" && obj.vars ["pY"] != "")
			CheckForStop ();

		if (obj.playerID == SystemInfo.deviceUniqueIdentifier)
			Camera.main.GetComponent<FollowCamera> ().follow = transform;
	}

	/// <summary>
	/// Move the local entity to point when the local player commands. Then sends this action to all other clients.
	/// </summary>
	/// <param name="newVelocity">New velocity.</param>
	/// <param name="direction">Direction.</param>
	public void LocalMovePoint(Vector2 point, float newVelocity){
		Dictionary<int, string> args = new Dictionary<int, string> ();
		args.Add ((int) WorldConstants.WorldVars.VELOCITY, newVelocity.ToString());
		args.Add ((int) WorldConstants.WorldVars.X, point.x.ToString());
		args.Add ((int) WorldConstants.WorldVars.Y, point.y.ToString());
		ClientMovePoint (args);
		obj.QueueChange (obj.id, (int) BaseGameConstants.BaseGameMethods.MOVE_CLIENT_POINT, args);
	}

	/// <summary>
	/// Moves the local entity to point when commanded from the ObjectCommunicator, as commanded by another client.
	/// </summary>
	/// <param name="par">Parameters.</param>
	public void ClientMovePoint(Dictionary<int, string> par){


		obj.vars[StringValue.GetStringValue(BaseGameConstants.BaseGameVars.POINT_X)] = 
			float.Parse (par [(int) WorldConstants.WorldVars.X]).ToString();

		obj.vars[StringValue.GetStringValue(BaseGameConstants.BaseGameVars.POINT_Y)] =
			float.Parse (par [(int) WorldConstants.WorldVars.Y]).ToString();
		
		obj.vars [StringValue.GetStringValue(WorldConstants.WorldVars.ROTATION)] = 
			JsonConvert.SerializeObject (
				
				new SerializableTransform (World.FindRotation(new Vector3(transform.position.x, 0, transform.position.z),
					
				new Vector3(float.Parse (par [(int) WorldConstants.WorldVars.X]), 
							0, float.Parse (par [(int) WorldConstants.WorldVars.Z]))).eulerAngles)
				
			);
		
		obj.vars[StringValue.GetStringValue(WorldConstants.WorldVars.VELOCITY)] =
			float.Parse (par [(int) WorldConstants.WorldVars.VELOCITY]).ToString();
		
	}

	void UpdatePoint(){


		obj.vars [StringValue.GetStringValue(WorldConstants.WorldVars.ROTATION)] =
			JsonConvert.SerializeObject (
				
				new SerializableTransform (World.FindRotation(new Vector3(transform.position.x, 0, transform.position.z),
					
				new Vector3(float.Parse (obj.vars [StringValue.GetStringValue(BaseGameConstants.BaseGameVars.POINT_X)]), 
					0, float.Parse (obj.vars [StringValue.GetStringValue(BaseGameConstants.BaseGameVars.POINT_Y)]))).eulerAngles)
				
			);


	}

	void CheckForStop(){
		
		Vector2 currentPos = new Vector2 (transform.position.x, transform.position.z);

		Vector2 goToPos = new Vector2 (float.Parse (obj.vars [StringValue.GetStringValue(BaseGameConstants.BaseGameVars.POINT_X)]), 
			float.Parse (obj.vars [StringValue.GetStringValue(BaseGameConstants.BaseGameVars.POINT_Y)]));
		
		if (Vector2.Distance (currentPos, goToPos) <= BaseGameConstants.POINTRELATIVEDISTANCE) {
			
			obj.vars [StringValue.GetStringValue(BaseGameConstants.BaseGameVars.POINT_X)] = "";
			obj.vars [StringValue.GetStringValue(BaseGameConstants.BaseGameVars.POINT_Y)] = "";
			obj.vars [StringValue.GetStringValue(WorldConstants.WorldVars.VELOCITY)] = "0";

		}

	}
}
