using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class DirectoryServer : MonoBehaviour {
	public Dictionary<int, Server> servers = new Dictionary<int, Server> ();

	private bool serverStarted;

	#region Script Base Functions
	void Start(){
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

		NetworkServer.RegisterHandler(MsgType.Disconnect, OnDisconnect);

		NetworkServer.RegisterHandler(Master.RequestServerId, SendServerConnection);
		NetworkServer.RegisterHandler(Master.ConnectServerId, ConnectServer);
		NetworkServer.RegisterHandler(Master.UpdateServerId, UpdateServer);
	}
		
	void Update(){
		if (YamlConfig.config != null && !serverStarted) {
			NetworkServer.Listen (YamlConfig.config.port);
			serverStarted = true;
		}
	}
	#endregion

	#region Directory Server Tools
	Server CheckForRoom(string room){
		foreach (Server s in servers.Values)
			if (s.rooms.Contains (room))
				return s;
		return null;
	}

	Server LeastPopulatedServer(){
		Server index = null;
		foreach (Server s in servers.Values)
			if (index == null) {
				index = s;
			} else if (s.numPlayers < index.numPlayers) {
				index = s;
			}
		return index;
	}
	#endregion

	#region Server Functions
	void OnDisconnect(NetworkMessage m){
		if (servers.ContainsKey (m.conn.connectionId))
			servers.Remove (m.conn.connectionId);
	}

	void SendServerConnection(NetworkMessage m){
		var msg = m.ReadMessage<Master.RequestServer> ();
		Server sendTo = CheckForRoom (msg.room);
		if (sendTo == null)
			sendTo = LeastPopulatedServer ();

		Master.RequestServer message = new Master.RequestServer ();
		message.ip = sendTo.ip;
		message.port = sendTo.port;
		NetworkServer.SendToClient (m.conn.connectionId, Master.RequestServerId, message);
	}
		
	void ConnectServer(NetworkMessage m){
		var msg = m.ReadMessage<Master.ConnectServer> ();
		Server server = new Server ();
		server.ip = msg.ip;
		server.port = msg.port;
		server.connectionID = m.conn.connectionId;
		servers.Add (m.conn.connectionId, server);
		Debug.Log ("Connected Server: " + server.ip + ":" + server.port);
	}

	void UpdateServer(NetworkMessage m){
		var msg = m.ReadMessage<Master.UpdateServer> ();
		servers [m.conn.connectionId].numPlayers = msg.numPlayers;
		if(msg.addRoom != "" && msg.addRoom != null )
			servers [m.conn.connectionId].rooms.Add (msg.addRoom);
		if(msg.removeRoom != "" && msg.removeRoom != null )
			servers [m.conn.connectionId].rooms.Remove (msg.removeRoom);
	}
	#endregion
}

public class Server{
	public List<string> rooms = new List<string> ();
	public string ip{ get; set; }
	public int port { get; set; }
	public int numPlayers { get; set; }
	public int connectionID{ get; set; }
}