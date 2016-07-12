using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

public class SerializableWorldObject
{
	#region Getters and Setters
	public int type { get; set; }
	public string id { get; set; }
	public string playerID { get; set; }
	public SerializableTransform trans { get; set; } 
	public Dictionary<string, string> vars{ get; set; }
	#endregion

	#region Constructors
	/// <summary>
	/// Initializes a new instance of the <see cref="CompressedObject"/> class.
	/// </summary>
	/// <param name="trans">Transform.</param>
	/// <param name="id">Identifier.</param>
	/// <param name="type">Type.</param>
	/// <param name="vars">Variables.</param>
	[JsonConstructor]
	public SerializableWorldObject(SerializableTransform trans, string id, int type, Dictionary<string, string> vars){
		this.trans = trans;
		this.id = id;
		this.type = type;
		this.vars = vars;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="CompressedObject"/> class.
	/// </summary>
	/// <param name="trans">Transform.</param>
	/// <param name="id">Identifier.</param>
	/// <param name="playerID">Player ID.</param>
	/// <param name="type">Type.</param>
	/// <param name="vars">Variables.</param>
	public SerializableWorldObject(Transform trans, string id, string playerID,int type, Dictionary<string, string> vars){
		this.trans = new SerializableTransform (trans);
		this.id = id;
		this.type = type;
		this.vars = vars;
		this.playerID = playerID;
	}
	#endregion	

	#region Convertion Methods
	/// <summary>
	/// Decompresses the object.
	/// </summary>
	/// <returns>The world object transform.</returns>
	public Transform DecompressObject(){
		Transform t = trans.ToTransform ();
		Transform create = (Transform) GameObject.Instantiate(TypesConverter.ConvertType((WorldObjectCache.Types)type), t.position, t.rotation);
		GameObject.Destroy (t.gameObject);
		WorldObject worldObject = create.GetComponent<WorldObject> ();
		worldObject.type = (WorldObjectCache.Types)type;
		worldObject.vars = vars;
		worldObject.id = id;
		worldObject.playerID = playerID;
		return create;
	}
	#endregion
}

