using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class ServerStreamer : MonoBehaviour {


	public void RegisterListeners(){
		NetworkServer.RegisterHandler (Master.RequestObjUpdateId, StreamUpdateRequest);
		NetworkServer.RegisterHandler (Master.SendObjUpdateId, StreamUpdate);
		NetworkServer.RegisterHandler (Master.SendWorldId, StreamWorld);
		NetworkServer.RegisterHandler (Master.SendChangesId, StreamChanges);
	}

	/// <summary>
	/// Streams the changes.
	/// </summary>
	void StreamChanges(NetworkMessage m){
		var msg = m.ReadMessage<Master.SendChanges> ();
		Room.allPlayers[m.conn.connectionId].room.SendMessage(Room.allPlayers[m.conn.connectionId], msg);
	}

	/// <summary>
	/// Streams the update.
	/// </summary>
	void StreamUpdate(NetworkMessage m){
		DisableLogging.Logger.Log ("Send update requested from: " + m.conn.connectionId, Color.yellow);

		var msg = m.ReadMessage<Master.UpdateObject> ();
		int conn = msg.conn;
		msg.conn = conn;

		NetworkServer.SendToClient (conn, Master.SendObjUpdateId, msg);
	}

	/// <summary>
	/// Streams the update.
	/// </summary>
	void StreamUpdateRequest(NetworkMessage m){
		DisableLogging.Logger.Log ("Send update requested from: " + m.conn.connectionId, Color.yellow);

		var msg = m.ReadMessage<Master.RequestObjUpdate> ();
		NetworkServer.SendToClient (Room.allPlayers[m.conn.connectionId].room.host.connectionID, Master.RequestObjUpdateId, msg);

	}

	/// <summary>
	/// Streams the world.
	/// </summary>
	void StreamWorld(NetworkMessage mess) {
		
		var msg = mess.ReadMessage<Master.SendWorld>();
		int id = msg.id;
		int done = msg.done;

		if (done == 2) {
			DisableLogging.Logger.Log("Got world from host: "+mess.conn.connectionId, Color.green);
		}

		msg.id = id;
		msg.done = done;

		NetworkServer.SendToClient (id, Master.SendWorldId, msg);
	}

}
