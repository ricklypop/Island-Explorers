using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorldObject : MonoBehaviour {
	#region Getters and Setters
	public WorldObjectCache.Types type {get; set;}
	public string id { get; set; }
	public string playerID{ get; set; }
	public Dictionary<string, string> vars = new Dictionary<string, string>();
	#endregion

	#region Change Handler Methods
	/// <summary>
	/// Queues the change.
	/// </summary>
	/// <param name="id">Identifier.</param>
	/// <param name="message">Message.</param>
	/// <param name="args">Arguments.</param>
	public void QueueChange(string id, string message, Dictionary<int, string> args){
		if(LoadBalancer.totalPlayers > YamlConfig.config.minTotalPlayers)
			ChangeCache.Enqueue(ObjectCommunicator.CreateMessage(id, message, args));
	}
	#endregion

	#region Local Methods to Send Changes to Other Clients
	/// <summary>
	/// Adds or edits the index with the change.
	/// Sends to other clients.
	/// </summary>
	/// <param name="i">The index.</param>
	/// <param name="c">The change.</param>
	public void LocalChangeVars(string i, string c){
		if (vars.ContainsKey (i))
			vars [i] = c;
		else
			vars.Add (i, c);
		Dictionary<int, string> args = new Dictionary<int, string> ();
		args.Add (Constants.vars.Compress("x"), i);
		args.Add (Constants.vars.Compress("y"), c);
		QueueChange (id, "ClientChangeVars", args);
	}
	#endregion

	#region The Client Methods that Apply Changes on the Client's World Objects
	/// <summary>
	/// Adds or edits the index with the change.
	/// </summary>
	/// <param name="par">Parameters.</param>
	public void ClientChangeVars(Dictionary<int, string> par){
		string i = par [Constants.vars.Compress("x")];
		string c = par [Constants.vars.Compress("y")];
		if (vars.ContainsKey (i))
			vars [i] = c;
		else
			vars.Add (i, c);
	}
	#endregion

	#region Converion Methods
	/// <summary>
	/// Compresses the world object.
	/// </summary>
	/// <returns>The compressed object.</returns>
	public SerializableWorldObject CompressWorldObject(){
		return new SerializableWorldObject(transform, id, playerID,(int) type, vars);
	}
	#endregion
}
