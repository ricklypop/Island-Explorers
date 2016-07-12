using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

public class WorldPlayer : MonoBehaviour, Action {
	public float movementVelocity = GameConstants.DEFAULTMOVEMENTVELOCITY;

	private Entity e;
	private WorldObject obj;
	private float time;

	void Start(){
		e = GetComponent<Entity> ();
		obj = GetComponent<WorldObject> ();
		if (!obj.vars.ContainsKey ("pX")) {
			obj.vars.Add ("pX", "");
			obj.vars.Add ("pY", "");
		}
	}

	#region Action implementation
	public void Act (Entity e)
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

	void Update(){
		if (obj.vars ["pX"] != "" && obj.vars ["pY"] != "" && time >= GameConstants.POINTUPDATETIME) {
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
		args.Add (Constants.vars.Compress("velocity"), newVelocity.ToString());
		args.Add (Constants.vars.Compress("x"), point.x.ToString());
		args.Add (Constants.vars.Compress("z"), point.y.ToString());
		ClientMovePoint (args);
		obj.QueueChange (obj.id, "ClientMovePoint", args);
	}

	/// <summary>
	/// Moves the local entity to point when commanded from the ObjectCommunicator, as commanded by another client.
	/// </summary>
	/// <param name="par">Parameters.</param>
	public void ClientMovePoint(Dictionary<int, string> par){
		obj.vars["pX"] = float.Parse (par [Constants.vars.Compress ("x")]).ToString();
		obj.vars["pY"] = float.Parse (par [Constants.vars.Compress ("z")]).ToString();
		obj.vars ["r"] = JsonConvert.SerializeObject (
			new SerializableTransform (World.FindRotation(new Vector3(transform.position.x, 0, transform.position.z),
				new Vector3(float.Parse (par [Constants.vars.Compress ("x")]), 0, float.Parse (par [Constants.vars.Compress ("z")]))).eulerAngles));
		obj.vars["mV"] = float.Parse (par [Constants.vars.Compress ("velocity")]).ToString();
	}

	void UpdatePoint(){
		obj.vars ["r"] = JsonConvert.SerializeObject (
			new SerializableTransform (World.FindRotation(new Vector3(transform.position.x, 0, transform.position.z),
				new Vector3(float.Parse (obj.vars ["pX"]), 0, float.Parse (obj.vars ["pY"]))).eulerAngles));
	}

	void CheckForStop(){
		Vector2 currentPos = new Vector2 (transform.position.x, transform.position.z);
		Vector2 goToPos = new Vector2 (float.Parse (obj.vars ["pX"]), float.Parse (obj.vars ["pY"]));
		if (Vector2.Distance (currentPos, goToPos) <= GameConstants.POINTRELATIVEDISTANCE) {
			obj.vars ["pX"] = "";
			obj.vars ["pY"] = "";
			obj.vars ["mV"] = "0";
		}
	}
}
