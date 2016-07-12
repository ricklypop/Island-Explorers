using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;

public class ClientListener {
	private NetworkClient network;
	private ClientParser clientParser;
	private Client client;

	public ClientListener(ClientSession clientSession){
		this.client = clientSession.client;
		this.network = clientSession.network;
		this.clientParser = clientSession.clientParser;
	}

	public void RegisterOnNetwork(){
		network.RegisterHandler (Master.SetWorldId, OnSendWorld);
		network.RegisterHandler (Master.JoinedId, OnJoined);
		network.RegisterHandler (Master.RequestObjUpdateId, OnUpdateRequest);
	}

	#region Server and Client Chain Methods

	/// <summary>
	/// Raises the joined event.
	/// Once joined, trash the current world and request the room world
	/// </summary>
	void OnJoined (NetworkMessage m)
	{
		
		DisableLogging.Logger.Log (network.connection.connectionId + ":Joined", Color.cyan);

		WorldDatabase.world = new List<SerializableWorldObject> ();
		WorldObjectCache.ClearCache ();
		
		client.gotWorld = false;

	}
	#endregion

	#region Recieve Request
	/// <summary>
	/// Recieve the update request, and grab the JSON string from the stream.
	/// When done, create a queue of compressed objects and add the objects based on the 
	/// keys from the desrialized list. Then serialize the queue, to start sending updates.
	/// </summary>
	void OnUpdateRequest (NetworkMessage m)
	{
		
		var msg = m.ReadMessage<Master.RequestObjUpdate> ();
		int index = msg.fromClient;

		if (!client.recievedUpdateRequest.ContainsKey (index)) {
			
			client.recievedUpdateRequest.Add (index, new byte[0]);
			clientParser.AddRequestedIndex (index);

		}

		client.recievedUpdateRequest[index] = client.recievedUpdateRequest[index].Concat(msg.objUpdates).ToArray();
		if (msg.done == 1) {
			
			Queue<SerializableWorldObject> compressed = new Queue<SerializableWorldObject> ();


			List<int> worldObjects = JsonConvert.DeserializeObject<List<int>>(
				System.Text.Encoding.ASCII.GetString(CLZF2.Decompress(client.recievedUpdateRequest[index])));


			foreach(int i in worldObjects){
				compressed.Enqueue (WorldObjectCache.GetObjectByIndex(i).CompressWorldObject ());
			}

			client.recievedUpdateRequest[index] = CLZF2.Compress(System.Text.Encoding.ASCII.GetBytes(JsonConvert.SerializeObject (compressed)));

		}

	}

	/// <summary>
	/// Raises the send world event. Save the id to send to.
	/// </summary>
	void OnSendWorld (NetworkMessage m)
	{
		
		DisableLogging.Logger.Log (network.connection.connectionId
			+ ":Reached Host Set World, Waiting...", Color.cyan);
		
		var recieved = m.ReadMessage<Master.RequestWorld> ();
		int id = recieved.id;

		clientParser.ConvertNewWorld (id);

	}

	#endregion

}
