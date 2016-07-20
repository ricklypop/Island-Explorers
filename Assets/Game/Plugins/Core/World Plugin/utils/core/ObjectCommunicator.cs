using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

public class ObjectCommunicator {
	private static int current;

	#region Communicate With World
	/// <summary>
	/// Sends a message to the local entity, if it is contained within the sorted list, as defined in the change.
	/// </summary>
	/// <param name="change">Change.</param>
	public static void SendChangeMessage(Change change){
		if(WorldObjectCache.KeyCount() > change.id){
			WorldObjectCache.GetObjectByIndex(change.id).gameObject.SendMessage(
				StringValue.GetStringValue((WorldConstants.WorldMethods) change.func), change.args);
		}
	} 

	/// <summary>
	/// Create world object. Send it to other clients.
	/// </summary>
	/// <param name="create">Create.</param>
	/// <param name="c">The compressed transform.</param>
	/// <param name="ownership">Ownership.</param>
	public static void LocalCreateWorldObject(Transform create, SerializableTransform c, string ownership){
		Master.CreateObject obj = new Master.CreateObject ();
		obj.name = (int) TypesConverter.ConvertTransform (create);
		obj.trans = JsonConvert.SerializeObject (c);
		obj.id = UniqueIDGenerator.GetUniqueID ();
		obj.own = ownership;
		ClientCreateWorldObject (obj.name, obj.trans, obj.id, ownership);
		if(LoadBalancer.totalPlayers > YamlConfig.config.minTotalPlayers)
			Client.main.network.Send(Master.RequestCreateObjectId, obj);
	}

	/// <summary>
	/// Creates the world object for this client.
	/// </summary>
	/// <param name="name">Name.</param>
	/// <param name="s">The serialized object.</param>
	/// <param name="id">Identifier.</param>
	/// <param name="own">Ownership.</param>
	public static void ClientCreateWorldObject(int name, string s, string id, string own){
		if (!WorldObjectCache.HasKey(id)) {
			SerializableTransform c = JsonConvert.DeserializeObject<SerializableTransform> (s);
			Transform create = (Transform)GameObject.Instantiate (TypesConverter.ConvertType ((WorldObjectCache.Types)name), c.ToTransform ().position, c.ToTransform ().rotation);
			WorldObject worldObject = create.GetComponent<WorldObject> ();
			worldObject.type = (WorldObjectCache.Types) name;
			worldObject.vars = new Dictionary<string, string> ();
			worldObject.id = id;
			worldObject.playerID = own;
			WorldObjectCache.Add(id, worldObject);
			LoadBalancer.Balance ();
		}
	}

	/// <summary>
	/// Destroys a world object. Sends its destruction to other clients.
	/// </summary>
	/// <param name="key">The Key to Destroy.</param>
	public static void LocalDestroyWorldObject(string key){
		Master.DestroyObject obj = new Master.DestroyObject ();
		obj.destroyKey = key;
		if(LoadBalancer.totalPlayers > YamlConfig.config.minTotalPlayers)
			Client.main.network.Send(Master.DestroyObjectRequestId, obj);
	}

	/// <summary>
	/// Destroys the world object with key, on this client.
	/// </summary>
	/// <param name="key">The Destroy Key.</param>
	public static void ClientDestroyWorldObject(string key){
		if (Client.main.gotWorld)
			GameObject.Destroy (WorldObjectCache.GetObject(key));
		else
			DestroyCache.Enqueue(WorldObjectCache.GetObject(key));
	}
	#endregion

	#region Create Communication Messages
	/// <summary>
	/// Creates the change message.
	/// </summary>
	/// <returns>The change message.</returns>
	/// <param name="id">Identifier.</param>
	/// <param name="action">Action.</param>
	/// <param name="parameters">Parameters.</param>
	public static Change CreateMessage(string id, int action, Dictionary<int, string> parameters){
		//SerializableDictionary dic = new SerializableDictionary ();
		Change change = new Change ();
		change.id = WorldObjectCache.GetIndexOfKey (id);
		change.func = action;
		//dic.Serialize (parameters);
		change.args = parameters;
		return change;
	}
	#endregion
}
