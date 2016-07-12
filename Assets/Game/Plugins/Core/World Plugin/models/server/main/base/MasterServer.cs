using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Collections.Generic;

public class MasterServer : MonoBehaviour {
	public static SortedList<string, Room> rooms = new SortedList<string, Room> ();//The server's rooms

	private ServerStreamer serverStreamer;
	private ServerSaver serverSaver;

	public int port;

	private bool serverStarted;//If the server as started

	private Room room;
	private float time;

	#region Server Functions
	/// <summary>
	/// Start this instance and the server. Registers handlers.
	/// </summary>
	void Start(){
		serverStreamer = new ServerStreamer ();
		serverSaver = new ServerSaver ();

		ConnectionConfig config = new ConnectionConfig();
		config.AddChannel(QosType.ReliableSequenced);
		config.AddChannel(QosType.ReliableFragmented);
		config.MaxSentMessageQueueSize = YamlConfig.config.maxSendMessages;
		config.PacketSize = YamlConfig.config.packetSize;
		config.FragmentSize = (ushort)(config.PacketSize - ((ushort)129));
		config.AddChannel(QosType.Unreliable);
		config.ConnectTimeout = 5000;
		config.MinUpdateTimeout = 10;
		config.DisconnectTimeout = 2000;
		config.PingTimeout = 500;
		NetworkServer.Configure(config, Master.maxConnections);
		NetworkServer.RegisterHandler(MsgType.Connect, OnClientConnected);
		NetworkServer.RegisterHandler(MsgType.Disconnect, OnClientDisconnected);

		NetworkServer.RegisterHandler(Master.JoinRoomId,  JoinRoom);
		NetworkServer.RegisterHandler (Master.RequestWorldId, GetWorld);
		NetworkServer.RegisterHandler (Master.RequestCreateObjectId, SendCreateObject);
		NetworkServer.RegisterHandler (Master.DestroyObjectRequestId, SendDestroyObject);

		serverStreamer.RegisterListeners ();

		NetworkServer.Listen (port);
	}

	/// <summary>
	/// Raises the client connected event.
	/// </summary>
	void OnClientConnected(NetworkMessage mess) {
		DisableLogging.Logger.Log("Connected: " + mess.conn.connectionId, Color.green);
	}

	/// <summary>
	/// Raises the client disconnected event. Remove player from room, and remove room if room is empty.
	/// </summary>
	void OnClientDisconnected(NetworkMessage mess) {
		DisableLogging.Logger.Log("Player left: "+mess.conn.connectionId, Color.red);
		if (Room.allPlayers.ContainsKey (mess.conn.connectionId)) {
			Room r = Room.allPlayers [mess.conn.connectionId].room;
			r.RemovePlayer (Room.allPlayers [mess.conn.connectionId]);
			DisableLogging.Logger.Log("Player remove from room id: " + r.ID, Color.yellow);
		}
	}
	#endregion

	#region Server and Client Connecting
	/// <summary>
	/// Joins a room. If room is already created, 
	/// . If it isn't created, create the room.
	/// If the room is full, tell the client to disconnect.
	/// </summary>
	void JoinRoom(NetworkMessage m){
		DisableLogging.Logger.Log("Connection Id " + m.conn.connectionId + " requested join room.", Color.yellow);
		var msg = m.ReadMessage<Master.JoinRoom>();
		string roomID = msg.roomID;
		if (rooms.ContainsKey (roomID) && rooms [roomID].totalPlayers <= Master.roomSize) {
			Player player = new Player ();
			player.deviceID = msg.deviceID;
			player.connectionID = m.conn.connectionId;
			player.room = rooms [roomID];
			rooms [roomID].AddPlayer (player);
			DisableLogging.Logger.Log ("Joined Room: " + roomID, Color.green);
			m.conn.Send (Master.JoinedId, new Master.Joined ());
			MasterServerClient.main.UpdateServer (Room.allPlayers.Count, "", "");
		} else if (!rooms.ContainsKey (roomID)) {
			rooms.Add (roomID, new Room ());
			rooms [roomID].ID = roomID;
			Player player = new Player ();
			player.deviceID = msg.deviceID;
			player.connectionID = m.conn.connectionId;
			player.room = rooms [roomID];
			rooms [roomID].AddPlayer (player);
			DisableLogging.Logger.Log ("Room Created: " + roomID, Color.green);
			m.conn.Send (Master.JoinedId, new Master.Joined ());
			MasterServerClient.main.UpdateServer (Room.allPlayers.Count, roomID, "");
		} else {
			DisableLogging.Logger.Log ("Room is full: " + roomID, Color.red);
			m.conn.Send (Master.RoomFullId, new Master.RoomFull ());
		}
	}

	/// <summary>
	/// Gets the world from client. If its the clients turn, get the world. If the client is the host,
	/// tell the client to download the world from the database. If it isn't the players turn, tell them to
	/// wait. If host left in the middle of stream, reset the the stream.
	/// </summary>
	void GetWorld(NetworkMessage mess) {
		DisableLogging.Logger.Log("World set requested from: "+mess.conn.connectionId, Color.green);
		int id = Room.allPlayers [mess.conn.connectionId].room.host.connectionID;
		var r =  mess.ReadMessage<Master.RequestWorld>();

		if (mess.conn.connectionId != id && (r.host == 0 || r.host == id)) {
			Master.RequestWorld setW = new Master.RequestWorld ();
			setW.id = mess.conn.connectionId;
			setW.host = id;
			NetworkServer.SendToClient (id, Master.SetWorldId, setW);
		} else if (mess.conn.connectionId == id) {
			DisableLogging.Logger.Log ("Host is client. Skipping request...", Color.yellow);
			Master.SendWorld set = new Master.SendWorld ();
			set.done = 1;
			set.world = new byte[0];
			set.id = mess.conn.connectionId;
			mess.conn.Send (Master.SendWorldId, set);
		} else if (r.host != id ) {
			DisableLogging.Logger.Log ("Host left, restarting stream...", Color.yellow);
			Master.SendWorld set = new Master.SendWorld ();
			set.done = 3;
			set.world = new byte[0];
			set.id = mess.conn.connectionId;
			mess.conn.Send (Master.SendWorldId, set);
		}
	}
	#endregion

	#region Server Redirect to other Client

	/// <summary>
	/// Sends the create object.
	/// </summary>
	void SendCreateObject(NetworkMessage m){
		var msg = m.ReadMessage<Master.CreateObject> ();
		DisableLogging.Logger.Log ("Create Object Requested: " + msg.trans, Color.green);
		foreach (Player p in Room.allPlayers[m.conn.connectionId].room.players)
			if(m.conn.connectionId != p.connectionID)
				NetworkServer.SendToClient (p.connectionID, Master.CreateObjectId, msg);
	}

	/// <summary>
	/// Sends the destroy object.
	/// </summary>
	void SendDestroyObject(NetworkMessage m){
		var msg = m.ReadMessage<Master.DestroyObject> ();
		DisableLogging.Logger.Log ("Destroy Object Requested: " + msg.destroyKey, Color.green);
		foreach (Player p in Room.allPlayers[m.conn.connectionId].room.players)
			if(m.conn.connectionId != p.connectionID)
				NetworkServer.SendToClient (p.connectionID, Master.DestroyObjectRequestId, msg);
	}
	#endregion
}
	