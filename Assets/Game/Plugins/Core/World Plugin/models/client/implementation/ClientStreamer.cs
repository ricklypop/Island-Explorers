using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

public class ClientStreamer {
	private Client client;
	private NetworkClient network;

	public ClientStreamer(ClientSession clientSession){
		this.client = clientSession.client;
		this.network = clientSession.network;
	}

	public void RegisterOnNetwork(){
		network.RegisterHandler (Master.SendChangesId, GetFromChangesStream);
		network.RegisterHandler (Master.SendWorldId, GetFromWorldStream);
		network.RegisterHandler (Master.SendObjUpdateId, GetFromUpdateStream);
	}

	/// <summary>
	/// Gets from update stream. When done, updates objects from the list.
	/// </summary>
	public void GetFromUpdateStream (NetworkMessage m)
	{
		
		var msg = m.ReadMessage<Master.UpdateObject> ();

		client.recievedUpdate = client.recievedUpdate.Concat(msg.updates).ToArray();

		if (msg.done == 1) {


			foreach (SerializableWorldObject c in JsonConvert.DeserializeObject<List<SerializableWorldObject>>(System.Text.Encoding.ASCII.GetString(
				CLZF2.Decompress(client.recievedUpdate)))) {

				WorldObjectCache.SetObject(c.id, c.DecompressObject ().GetComponent<WorldObject> (), true);

			}


			client.recievedUpdate = new byte[0];
		}

	}

	/// <summary>
	/// Gets from world stream. If reset, reset. If host, get from database. If set, create the world.
	/// </summary>
	public void GetFromWorldStream (NetworkMessage m)
	{
		
		var recieved = m.ReadMessage<Master.SendWorld> ();

		int r = recieved.done;
		byte[] b = recieved.world;

		if (r == 0) {
			
			client.recievedWorld = client.recievedWorld.Concat(b).ToArray();

		} else if (r == 1) {
			
			client.StartCoroutine (WorldDatabase.database.GetWorld (WorldDatabase.currentWorldID));

		} else if (r == 2) {
			
			WorldObjectCache.ClearCache ();
			client.DeserializeWorld ();

		}

	}

	/// <summary>
	/// Transfers the changes to the object communicator, so changes can be made to world objects.
	/// If still getting the world, enqueue the data lost, to request an update when world is set.
	/// </summary>
	public void GetFromChangesStream (NetworkMessage m)
	{
		
		var msg = m.ReadMessage<Master.SendChanges> ();
		int id = msg.id;

		client.recievedChanges [LoadBalancer.playerIDs[id-1]] = client.recievedChanges[LoadBalancer.playerIDs[id-1]].Concat (msg.changes).ToArray ();



		if (msg.done == 1) {
			
			List<Change> changes = JsonConvert.DeserializeObject<List<Change>> (System.Text.Encoding.ASCII.GetString(
				CLZF2.Decompress(client.recievedChanges[LoadBalancer.playerIDs[id-1]])));
			
			client.TransferChanges (changes);

			client.recievedChanges[LoadBalancer.playerIDs[id-1]] = new byte[0];

		}

	}

}
