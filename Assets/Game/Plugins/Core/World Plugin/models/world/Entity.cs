 using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

/// <summary>
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
		instantTurn = Constants.DEFAULTINSTANTTURN;

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
			transform.position += (transform.forward / Constants.MOVEMENTFACTOR) * Time.deltaTime * moveVelocity;
			transform.position = new Vector3 (transform.position.x, transform.position.y, Constants.DEFAULTZ);
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
		args.Add (Constants.vars.Compress("velocity"), newVelocity.ToString());
		args.Add (Constants.vars.Compress("rX"), direction.eulerAngles.x.ToString());
		args.Add (Constants.vars.Compress("rY"), direction.eulerAngles.y.ToString());
		args.Add (Constants.vars.Compress("rZ"), direction.eulerAngles.z.ToString());
		obj.QueueChange (obj.id, "ClientMove", args);
	}


	/// <summary>
	/// Stops the local entity, as commanded by the local player. Then sends this action to all other clients.
	/// </summary>
	public void LocalStopMove(){
		obj.vars ["mV"] = "0";
		Dictionary<int, string> args = new Dictionary<int, string> ();
		args.Add (Constants.vars.Compress("x"), transform.position.x.ToString());
		args.Add (Constants.vars.Compress("y"), transform.position.y.ToString());
		args.Add (Constants.vars.Compress("rX"), transform.eulerAngles.x.ToString());
		args.Add (Constants.vars.Compress("rY"), transform.eulerAngles.y.ToString());
		args.Add (Constants.vars.Compress("z"), transform.eulerAngles.z.ToString());
		obj.QueueChange (obj.id, "ClientStopMove", args);
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
		args.Add (Constants.vars.Compress("velocity"), newVelocity.ToString());
		args.Add (Constants.vars.Compress("x"), x.ToString());
		args.Add (Constants.vars.Compress("y"), y.ToString());
		args.Add (Constants.vars.Compress("z"), z.ToString());
		obj.QueueChange (obj.id, "ClientRotate", args);
	}

	/// <summary>
	/// Stops rotating the local entity, as commanded by the local player. Then sends to other clients.
	/// </summary>
	public void LocalStopRotate(){
		obj.vars ["rV"] = "0";

		Dictionary<int, string> args = new Dictionary<int, string> ();
		args.Add (Constants.vars.Compress("x"), transform.eulerAngles.x.ToString());
		args.Add (Constants.vars.Compress("y"), transform.eulerAngles.y.ToString());
		args.Add (Constants.vars.Compress("z"), transform.eulerAngles.z.ToString());
		obj.QueueChange(obj.id, "ClientStopRotate", args);
	}

	/// <summary>
	/// Changes the temporary player ownership of the local object, to the client's ID. Sends to other clients.
	/// </summary>
	public void LocalChangeTempPlayer(){
		if (obj.vars ["tP"] == "") {
			obj.vars ["tP"] = LoadBalancer.connectionID.ToString ();

			Dictionary<int, string> args = new Dictionary<int, string> ();
			args.Add (Constants.vars.Compress ("name"), LoadBalancer.connectionID.ToString ());
			ObjectCommunicator.CreateMessage (obj.id, "ClientChangeTempPlayer", args);
		}
	}

	/// <summary>
	/// Removes the temporary player ownership of the local entity. Sends to other clients.
	/// </summary>
	public void LocalRemoveTempPlayer(){
		if (obj.vars ["tP"] == LoadBalancer.connectionID.ToString ()) {
			obj.vars ["tP"] = "";

			Dictionary<int, string> args = new Dictionary<int, string> ();
			args.Add (Constants.vars.Compress ("name"), "");
			obj.QueueChange(obj.id, "ClientChangeTempPlayer", args);
		}
	}
	#endregion

	#region The Client Methods that Apply Changes on the Client's World Objects
	/// <summary>
	/// Moves the local entity when commanded from the ObjectCommunicator, as commanded by another client.
	/// </summary>
	/// <param name="par">Parameters.</param>
	public void ClientMove(Dictionary<int, string> par){
		float newVelocity = float.Parse (par [Constants.vars.Compress ("velocity")]);
		float x = float.Parse (par [Constants.vars.Compress ("rX")]);
		float y = float.Parse (par [Constants.vars.Compress ("rY")]);
		float z = float.Parse (par [Constants.vars.Compress ("rZ")]);
		obj.vars ["r"] = JsonConvert.SerializeObject (new SerializableTransform (new Vector3 (x, y, z)));
		obj.vars ["mV"] = newVelocity.ToString ();
	}

	/// <summary>
	/// Stop this entity from moving on this client.
	/// </summary>
	/// <param name="par">Parameters.</param>
	public void ClientStopMove(Dictionary<int, string> par){
		float x = float.Parse(par [Constants.vars.Compress("x")]);
		float y = float.Parse(par [Constants.vars.Compress("y")]);
		float rX = float.Parse(par [Constants.vars.Compress("rX")]);
		float rY = float.Parse(par [Constants.vars.Compress("rY")]);
		float z = float.Parse(par [Constants.vars.Compress("z")]);
		obj.vars ["mV"] = "0";
		transform.position = new Vector3 (x, y, Constants.DEFAULTZ);
		transform.eulerAngles = new Vector3 (rX, rY, z);
	}

	/// <summary>
	/// Rotates the local entity when commanded by the ObjectCommunicator, as commanded by another client.
	/// </summary>
	/// <param name="par">Parameters.</param>
	public void ClientRotate(Dictionary<int, string> par){
		float newVelocity = float.Parse(par [Constants.vars.Compress("velocity")]);
		float x = float.Parse(par [Constants.vars.Compress("x")]);
		float y = float.Parse(par [Constants.vars.Compress("y")]);
		float z = float.Parse(par [Constants.vars.Compress("direction")]);

		obj.vars ["rV"] = newVelocity.ToString ();
		obj.vars ["r"] = JsonConvert.SerializeObject(new SerializableTransform(new Vector3 (x, y, z)));
	}

	/// <summary>
	/// Stops rotating the local entity when commanded by the ObjectCommunicator, as commanded by another client.
	/// </summary>
	/// <param name="par">Parameters.</param>
	public void ClientStopRotate(Dictionary<int, string> par){
		float x = float.Parse(par [Constants.vars.Compress("x")]);
		float y = float.Parse(par [Constants.vars.Compress("y")]);
		float z = float.Parse(par [Constants.vars.Compress("z")]);
		obj.vars ["rV"] = "0";
		transform.eulerAngles = new Vector3 (x, y, z);
	}

	/// <summary>
	/// Changes the local temp player when commanded by the ObjectCommunicator, as commanded by another client.
	/// </summary>
	/// <param name="par">Parameters.</param>
	public void ClientChangeTempPlayer(Dictionary<int, string> par){
		string name = par [Constants.vars.Compress("name")];

		obj.vars ["tP"] = name;
	}
	#endregion

	public abstract void Act ();
}
