 using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;


/// An entity is an object that can move and or contains it's own intelligence
/// </summary>
public abstract class Entity : MonoBehaviour {
	#region Public Entity Settings
	public bool active;
	public bool instantTurn{ get; set; }
	public WorldObject obj;
	#endregion

	#region Private Entity Values
	private float moveTime;
	private float rotateTime;

	private float deltaTime;
	#endregion

	#region Script Functions
	/// <summary>
	/// Start this instance.
	/// Check if required keys exist in the dictionary.
	/// "tP" stands for temporary player ownership.
	/// "mV" stands for movement velocity.
	/// "rV" stands for rotational velocity.
	/// "r" stands for rotation.
	/// </summary>
	void Start(){
		obj = GetComponent<WorldObject> ();
		LoadBalancer.Balance ();
		instantTurn = WorldConstants.DEFAULTINSTANTTURN;

		if(!obj.vars.ContainsKey("tP"))
			obj.vars.Add ("tP", "");
		if(!obj.vars.ContainsKey("mV"))
			obj.vars.Add ("mV", "0");
		if(!obj.vars.ContainsKey("rV"))
			obj.vars.Add ("rV", "0");
		if(!obj.vars.ContainsKey("r"))
			obj.vars.Add ("r", JsonConvert.SerializeObject(new SerializableTransform(Vector3.zero)));

		OnEntityStart ();
	}

	protected virtual void OnEntityStart(){

	}

	/// <summary>
	/// Update this instance.
	/// The entity will run it's seperate intelligence if it is active in its respective local device
	/// or if the temporary player ownership is set to this connection ID.
	/// Moves the entity.
	/// 
	/// </summary>
	void Update () {
		Vector3 rotation = JsonConvert.DeserializeObject<SerializableTransform> (obj.vars ["r"]).toPosition();
		float rotationVelocity = float.Parse(obj.vars["rV"]);
		float moveVelocity = float.Parse(obj.vars["mV"]);

		if((active && obj.vars["tP"] == "") 
			|| obj.vars["tP"] == LoadBalancer.connectionID.ToString())
			Act ();

		if (moveVelocity != 0) {
			if(instantTurn)
				transform.eulerAngles = new Vector3(rotation.x, rotation.y, rotation.z);
			transform.position += (transform.forward / WorldConstants.MOVEMENTFACTOR) * Time.deltaTime * moveVelocity;
			transform.position = new Vector3 (transform.position.x, transform.position.y, WorldConstants.DEFAULTZ);
			moveTime = 0;
		}

		if (rotateTime > rotationVelocity && rotationVelocity != 0) {
			transform.eulerAngles = new Vector3(transform.eulerAngles.x + rotation.normalized.x, 
				transform.eulerAngles.y + rotation.normalized.y, transform.eulerAngles.z + rotation.normalized.z);
			rotateTime = 0;
		}

		OnEntityUpdate ();
	}

	protected virtual void OnEntityUpdate(){

	}
	#endregion

	#region The Local Methods To Send Changes to Other Clients
	/// <summary>
	/// Move the local entity when the local player commands. Then sends this action to all other clients.
	/// </summary>
	/// <param name="newVelocity">New velocity.</param>
	/// <param name="direction">Direction.</param>
	public void LocalMove(float newVelocity, Quaternion direction){
		obj.vars ["mV"] = newVelocity.ToString ();
		obj.vars["r"] = JsonConvert.SerializeObject(new SerializableTransform(direction.eulerAngles));

		Dictionary<int, string> args = new Dictionary<int, string> ();
		args.Add ((int) WorldConstants.WorldVars.VELOCITY, newVelocity.ToString());
		args.Add ((int) WorldConstants.WorldVars.ROTATION_X, direction.eulerAngles.x.ToString());
		args.Add ((int) WorldConstants.WorldVars.ROTATION_Y, direction.eulerAngles.y.ToString());
		args.Add ((int) WorldConstants.WorldVars.ROTATION_Z, direction.eulerAngles.z.ToString());
		obj.QueueChange (obj.id, (int) WorldConstants.WorldMethods.MOVE_CLIENT, args);
	}


	/// <summary>
	/// Stops the local entity, as commanded by the local player. Then sends this action to all other clients.
	/// </summary>
	public void LocalStopMove(){
		obj.vars ["mV"] = "0";
		Dictionary<int, string> args = new Dictionary<int, string> ();
		args.Add ((int) WorldConstants.WorldVars.X, transform.position.x.ToString());
		args.Add ((int) WorldConstants.WorldVars.Y, transform.position.y.ToString());
		args.Add ((int) WorldConstants.WorldVars.Z, transform.eulerAngles.z.ToString());
		args.Add ((int) WorldConstants.WorldVars.ROTATION_X, transform.eulerAngles.x.ToString());
		args.Add ((int) WorldConstants.WorldVars.ROTATION_Y, transform.eulerAngles.y.ToString());
		obj.QueueChange (obj.id, (int) WorldConstants.WorldMethods.STOP_CLIENT, args);
	}

	/// <summary>
	/// Rotates the local entity, as commanded by the local player. Then sends to other clients.
	/// </summary>
	/// <param name="newVelocity">New velocity.</param>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	/// <param name="z">The z coordinate.</param>
	public void LocalRotate(float newVelocity, float x, float y, float z){
		obj.vars ["rV"] = newVelocity.ToString();
		obj.vars ["r"] = JsonConvert.SerializeObject(new SerializableTransform(new Vector3 (x, y, z)));

		Dictionary<int, string> args = new Dictionary<int, string> ();
		args.Add ((int) WorldConstants.WorldVars.VELOCITY , newVelocity.ToString());
		args.Add ((int) WorldConstants.WorldVars.X, x.ToString());
		args.Add ((int) WorldConstants.WorldVars.Y, y.ToString());
		args.Add ((int) WorldConstants.WorldVars.Z, z.ToString());
		obj.QueueChange (obj.id, (int) WorldConstants.WorldMethods.ROTATE_CLIENT, args);
	}

	/// <summary>
	/// Stops rotating the local entity, as commanded by the local player. Then sends to other clients.
	/// </summary>
	public void LocalStopRotate(){
		obj.vars ["rV"] = "0";

		Dictionary<int, string> args = new Dictionary<int, string> ();
		args.Add ((int) WorldConstants.WorldVars.X, transform.eulerAngles.x.ToString());
		args.Add ((int) WorldConstants.WorldVars.Y, transform.eulerAngles.y.ToString());
		args.Add ((int) WorldConstants.WorldVars.Z, transform.eulerAngles.z.ToString());
		obj.QueueChange(obj.id, (int) WorldConstants.WorldMethods.STOP_ROTATE_CLIENT, args);
	}

	/// <summary>
	/// Changes the temporary player ownership of the local object, to the client's ID. Sends to other clients.
	/// </summary>
	public void LocalChangeTempPlayer(){
		if (obj.vars ["tP"] == "") {
			obj.vars ["tP"] = LoadBalancer.connectionID.ToString ();

			Dictionary<int, string> args = new Dictionary<int, string> ();
			args.Add ((int) WorldConstants.WorldVars.NAME, LoadBalancer.connectionID.ToString ());
			ObjectCommunicator.CreateMessage (obj.id, (int) WorldConstants.WorldMethods.CHANGE_PLAYER_TEMP, args);
		}
	}

	/// <summary>
	/// Removes the temporary player ownership of the local entity. Sends to other clients.
	/// </summary>
	public void LocalRemoveTempPlayer(){
		if (obj.vars ["tP"] == LoadBalancer.connectionID.ToString ()) {
			obj.vars ["tP"] = "";

			Dictionary<int, string> args = new Dictionary<int, string> ();
			args.Add ((int) WorldConstants.WorldVars.NAME, "");
			obj.QueueChange(obj.id, (int) WorldConstants.WorldMethods.CHANGE_PLAYER_TEMP, args);
		}
	}
	#endregion

	#region The Client Methods that Apply Changes on the Client's World Objects
	/// <summary>
	/// Moves the local entity when commanded from the ObjectCommunicator, as commanded by another client.
	/// </summary>
	/// <param name="par">Parameters.</param>
	public void ClientMove(Dictionary<int, string> par){
		float newVelocity = float.Parse (par [(int) WorldConstants.WorldVars.VELOCITY]);
		float x = float.Parse (par [(int) WorldConstants.WorldVars.ROTATION_X]);
		float y = float.Parse (par [(int) WorldConstants.WorldVars.ROTATION_Y]);
		float z = float.Parse (par [(int) WorldConstants.WorldVars.ROTATION_Z]);
		obj.vars [StringValue.GetStringValue(WorldConstants.WorldVars.ROTATION)] = JsonConvert.SerializeObject (new SerializableTransform (new Vector3 (x, y, z)));
		obj.vars [StringValue.GetStringValue(WorldConstants.WorldVars.VELOCITY)] = newVelocity.ToString ();
	}

	/// <summary>
	/// Stop this entity from moving on this client.
	/// </summary>
	/// <param name="par">Parameters.</param>
	public void ClientStopMove(Dictionary<int, string> par){
		float x = float.Parse(par [(int) WorldConstants.WorldVars.X]);
		float y = float.Parse(par [(int) WorldConstants.WorldVars.Y]);
		float z = float.Parse(par [(int) WorldConstants.WorldVars.Z]);
		float rX = float.Parse(par [(int) WorldConstants.WorldVars.ROTATION_X]);
		float rY = float.Parse(par [(int) WorldConstants.WorldVars.ROTATION_Y]);
		obj.vars [StringValue.GetStringValue(WorldConstants.WorldVars.VELOCITY)] = "0";
		transform.position = new Vector3 (x, y, WorldConstants.DEFAULTZ);
		transform.eulerAngles = new Vector3 (rX, rY, z);
	}

	/// <summary>
	/// Rotates the local entity when commanded by the ObjectCommunicator, as commanded by another client.
	/// </summary>
	/// <param name="par">Parameters.</param>
	public void ClientRotate(Dictionary<int, string> par){
		float newVelocity = float.Parse(par [(int) WorldConstants.WorldVars.VELOCITY]);
		float x = float.Parse(par [(int) WorldConstants.WorldVars.X]);
		float y = float.Parse(par [(int) WorldConstants.WorldVars.Y]);
		float z = float.Parse(par [(int) WorldConstants.WorldVars.DIRECTION]);

		obj.vars [StringValue.GetStringValue(WorldConstants.WorldVars.ROTATION_VELOCITY)] = newVelocity.ToString ();
		obj.vars [StringValue.GetStringValue(WorldConstants.WorldVars.ROTATION)] = JsonConvert.SerializeObject(new SerializableTransform(new Vector3 (x, y, z)));
	}

	/// <summary>
	/// Stops rotating the local entity when commanded by the ObjectCommunicator, as commanded by another client.
	/// </summary>
	/// <param name="par">Parameters.</param>
	public void ClientStopRotate(Dictionary<int, string> par){
		float x = float.Parse(par [(int) WorldConstants.WorldVars.X]);
		float y = float.Parse(par [(int) WorldConstants.WorldVars.Y]);
		float z = float.Parse(par [(int) WorldConstants.WorldVars.Z]);
		obj.vars [StringValue.GetStringValue(WorldConstants.WorldVars.ROTATION_VELOCITY)] = "0";
		transform.eulerAngles = new Vector3 (x, y, z);
	}

	/// <summary>
	/// Changes the local temp player when commanded by the ObjectCommunicator, as commanded by another client.
	/// </summary>
	/// <param name="par">Parameters.</param>
	public void ClientChangeTempPlayer(Dictionary<int, string> par){
		string name = par [(int) WorldConstants.WorldVars.NAME];

		obj.vars [StringValue.GetStringValue(WorldConstants.WorldVars.TEMP_PLAYER)] = name;
	}
	#endregion

	public abstract void Act ();
}
