using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class ClientRequestor {
	private NetworkClient network;
	private ClientParser clientParser;
	private Client client;

	public ClientRequestor(ClientSession clientSession){
		this.client = clientSession.client;
		this.network = clientSession.network;
		this.clientParser = clientSession.clientParser;
	}

	#region Server Client Intial Interactions
	/// <summary>
	/// Join a room, with the players on the chosen world.
	/// </summary>
	public void JoinRoom ()
	{
		
		DisableLogging.Logger.Log (network.connection.connectionId + ":Requested Join Room: " + WorldDatabase.currentWorldID, Color.cyan);
		var msg = new Master.JoinRoom ();

		msg.roomID = WorldDatabase.currentWorldID;
		msg.deviceID = SystemInfo.deviceUniqueIdentifier;
		network.Send (Master.JoinRoomId, msg);

	}

	/// <summary>
	/// Requests the world from host of room.
	/// </summary>
	public void RequestWorld ()
	{
		var msg = new Master.RequestWorld ();
		network.Send (Master.RequestWorldId, msg);
	}
	#endregion


	#region Request Using Update Methods
	/// <summary>
	/// Requests the update.
	/// Request to request needed updates, once finished notify the host
	/// </summary>
	public void RequestUpdate ()
	{
		
		if (client.gotWorld && UpdateCache.GetUpdateCount() > 0) {


			byte[] b = clientParser.ParseRequestUpdate ();

			if (b.Length != 0) {

				Master.RequestObjUpdate up = new  Master.RequestObjUpdate ();
				up.objUpdates = b;

				if (clientParser.UpdateRequestCompleted ()) {
					up.done = 1;
				}else {
					up.done = 0;
				}

				up.fromClient = LoadBalancer.connectionID;
				network.Send (Master.RequestObjUpdateId, up);

			}


		}

	}

	#endregion
}
