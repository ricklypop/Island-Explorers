using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using DisableLogging;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System;

public class Client: MonoBehaviour
{
	public static Client main;	//This is a static method, used to reference the most recent instantiation of the Client

	#region Visible Public Vars Regarding Client

	public NetworkClient network;//The network
	public bool gotWorld { get; set; }//Used to see if the world has been loaded yet
	public int connectionAttemptCount { get; set; }//How many time the client has tryed to reconnect

	#endregion

	private ClientParser clientParser = new ClientParser();
	public ClientRequestor clientRequestor{get; set;}
	public ClientDirectory clientDirectory{ get; set; }
	public ClientSender clientSender{ get; set; }
	public ClientStreamer clientStreamer{get; set;}
	public ClientListener clientListener{get; set;}

	//Recieving World
	public byte[] recievedWorld {get; set;}//The recieved JSON world string that will be converted when stream has ended

	//Recieving Updates
	public byte[] recievedUpdate { get; set; }
	public Dictionary<int, byte[]> recievedUpdateRequest{get; set;}//The recieved population, a JSON string representing a list of int ids

	//Recieiving Changes
	public Dictionary<string, byte[]> recievedChanges{get; set;}

	#region System Functions

	//Start this behaviour
	void Start ()
	{
		main = this;

		ClientSession clientSession = new ClientSession (network, this, clientParser);
		clientRequestor = new ClientRequestor (clientSession);
		clientDirectory = new ClientDirectory (clientSession);
		clientSender = new ClientSender (clientSession);
		clientStreamer = new ClientStreamer (clientSession);
		clientListener = new ClientListener (clientSession);

		recievedUpdate= new byte[0];
		recievedUpdateRequest =  new Dictionary<int, byte[]>();
		recievedWorld = new byte[0];
		recievedChanges =  new Dictionary<string, byte[]>();

	}

	/// <summary>
	/// Starts the client. 
	/// Create network, create a connection config, configure network, register handlers
	/// </summary>
	public void StartClient (string ip, int port)
	{
		DisableLogging.Logger.Log ("Starting Client...", Color.cyan);
		network = new NetworkClient ();
		ConnectionConfig config = new ConnectionConfig ();
		config.AddChannel (QosType.ReliableSequenced);
		config.AddChannel (QosType.ReliableFragmented);
		config.MaxSentMessageQueueSize = YamlConfig.config.maxSendMessages;
		config.PacketSize = YamlConfig.config.packetSize;
		config.FragmentSize = (ushort)(config.PacketSize - ((ushort)129));
		config.AddChannel (QosType.Unreliable);
		config.ConnectTimeout = 5000;
		config.MinUpdateTimeout = 10;
		config.DisconnectTimeout = 2000;
		config.PingTimeout = 500;
		network.Configure (config, Master.maxConnections);
		network.Connect (ip, port);

		network.RegisterHandler (MsgType.Connect, OnClientConnect);
		network.RegisterHandler (MsgType.Disconnect, OnClientDisconnect);

		network.RegisterHandler (Master.UpdatePlayersId, UpdatePlayer);
		network.RegisterHandler (Master.TestPingId, TestPing);
		network.RegisterHandler (Master.CreateObjectId, CreateObject);
		network.RegisterHandler (Master.RoomFullId, RoomFullDisconnect);
		network.RegisterHandler (Master.DestroyObjectRequestId, DestroyObject);
		network.RegisterHandler (Master.SaveWorldId, SaveWorld);

		clientStreamer.RegisterOnNetwork ();

		clientDirectory.RegisterOnNetwork ();
	}

	/// <summary>
	/// Stops the client.
	/// Stop the client and network. Unregister handlers and shutdown everything
	/// </summary>
	public void StopClient (bool save)
	{
		if(save)
			World.SaveWorld ();

		print ("Stopping client: hostid=" + (connectionAttemptCount));
		NetworkTransport.RemoveHost (0);

		network.UnregisterHandler (MsgType.Connect);
		network.UnregisterHandler (MsgType.Disconnect);

		network.UnregisterHandler (Master.UpdatePlayersId);
		network.UnregisterHandler (Master.SendChangesId);
		network.UnregisterHandler (Master.SendWorldId);
		network.UnregisterHandler (Master.SetWorldId);
		network.UnregisterHandler (Master.JoinedId);
		network.UnregisterHandler (Master.TestPingId);
		network.UnregisterHandler (Master.CreateObjectId);
		network.UnregisterHandler (Master.RoomFullId);
		network.UnregisterHandler (Master.DestroyObjectRequestId);
		network.UnregisterHandler (Master.SendObjUpdateId);
		network.UnregisterHandler (Master.SaveWorldId);
		network.UnregisterHandler (Master.RequestServerId);

		clientParser.ResetWorldParsing ();
		network.Shutdown ();
		network = null;
		connectionAttemptCount += 1;
		gotWorld = false;
		clientDirectory.wentToDirectory = false;
		recievedChanges = new Dictionary<string, byte[]> ();
	}

	/// <summary>
	/// Connect this instance of the client.
	/// </summary>
	public static void Connect ()
	{
		main.StartClient (Environment.GetEnvironmentVariable(YamlConfig.DIRECTORY_IP_ENV), 
			int.Parse(Environment.GetEnvironmentVariable(YamlConfig.DIRECTORY_PORT_ENV)));
	}

	/// <summary>
	/// Disconnect this instance of the client.
	/// </summary>
	public static void Disconnect (bool save)
	{
		main.StopClient (save);
	}

	/// <summary>
	/// Raises the client connect event.
	/// When connected, join a room.
	/// </summary>
	void OnClientConnect (NetworkMessage m)
	{
		DisableLogging.Logger.Log (network.connection.connectionId + ":Client Started", Color.cyan);
		if (clientDirectory.wentToDirectory) {
			clientRequestor.JoinRoom ();
		}else {
			clientDirectory.RequestServer ();
		}
	}

	/// <summary>
	/// Raises the client disconnect event.
	/// When disconnect, try to reconnect.
	/// </summary>
	void OnClientDisconnect (NetworkMessage m)
	{
		StartCoroutine (Reset ());
	}
		
	void Update ()
	{
		clientSender.SendWorld ();//This allows the host (This client) to send the world to other clients
		clientSender.SendRequestedUpdates ();//Send requested updates
		clientSender.SendChanges ();//This sends the current change requests, limited by a periodic byte size

		clientRequestor.RequestUpdate ();//This allows the client to request an entire update on an object from the host

		DestroyObjects ();//Destroys objects in which could not be destroyed while downloading the world
	}

	/// <summary>
	/// Raises the application quit event.
	/// Saves the world.
	/// </summary>
	void OnApplicationQuit(){
		if(gotWorld)
			World.SaveWorld ();
	}

	/// <summary>
	/// Reset this instance of the client.
	/// </summary>
	IEnumerator Reset ()
	{
		StopClient (false);
		yield return new WaitForSeconds (1);
		clientDirectory.wentToDirectory = false;
		StartClient (Environment.GetEnvironmentVariable(YamlConfig.DIRECTORY_IP_ENV), 
			int.Parse(Environment.GetEnvironmentVariable(YamlConfig.DIRECTORY_PORT_ENV)));
	}

	#endregion

	#region Server Functions

	/// <summary>
	/// The room is full. Disconnect.
	/// </summary>
	void RoomFullDisconnect (NetworkMessage m)
	{
		DisableLogging.Logger.Log (network.connection.connectionId + ":Room is full. Disconnecting...", Color.red);
		Disconnect (false);
	}

	/// <summary>
	/// Test the pings the server.
	/// </summary>
	void TestPing (NetworkMessage m)
	{
		var msg = new Master.TestPinged ();
		network.Send (Master.TestPingedId, msg);
	}
		
	void SaveWorld(NetworkMessage m){
		if(gotWorld)
			World.SaveWorld ();
	}

	#endregion

	/// <summary>
	/// Updates the player.
	/// Update the total amount of players, the player number. Good for load balancing scripts and reseting stuff
	/// </summary>
	void UpdatePlayer (NetworkMessage m)
	{
		var msg = m.ReadMessage<Master.UpdatePlayers> ();
		int totalPlayers = msg.totalPlayers;
		int player = msg.playerNumber;

		LoadBalancer.totalPlayers = totalPlayers;
		LoadBalancer.player = player;
		LoadBalancer.connectionID = msg.connNumber;

		LoadBalancer.playerIDs = System.Text.Encoding.ASCII.GetString (CLZF2.Decompress (msg.ids)).Split (' ');
		foreach (string playerID in LoadBalancer.playerIDs)
			if (!recievedChanges.ContainsKey (playerID)) 
				recievedChanges.Add (playerID, new byte[0]);

		LoadBalancer.Balance ();
		if (!gotWorld) {
			if (WorldDatabase.world != null)
				WorldDatabase.world.Clear ();
			clientRequestor.RequestWorld ();
		}

		WorldObjectCache.RemovePlayerTemp (msg.leftNumber); 
		if (msg.wasHost == 1) {
			recievedWorld = new byte[0];
			recievedUpdate = new byte[0];
		}
		DisableLogging.Logger.Log ("Updated Player:" + totalPlayers + ": " + player, Color.cyan);
	}


	#region Regards Creating and Deleting World Objects

	/// <summary>
	/// Creates the object.
	/// </summary>
	void CreateObject (NetworkMessage m)
	{
		var msg = m.ReadMessage<Master.CreateObject> ();
		DisableLogging.Logger.Log ("Received create object: " + msg.name + ":" + msg.trans, Color.cyan);
		ObjectCommunicator.ClientCreateWorldObject (msg.name, msg.trans, msg.id, msg.own);
	}
		
	/// <summary>
	/// Destroys or enqueues the object.
	/// </summary>
	void DestroyObject (NetworkMessage m)
	{
		var msg = m.ReadMessage<Master.DestroyObject> ();
		ObjectCommunicator.ClientDestroyWorldObject (msg.destroyKey);
	}
		
	//NOTE: THIS IS A PRIVATE METHOD THAT REQUIRES UPDATES
	/// <summary>
	/// Destroys the objects.
	/// </summary>
	void DestroyObjects ()
	{
		if (gotWorld && DestroyCache.GetDestroyCount() > 0)
			GameObject.Destroy (DestroyCache.Dequeue ());
	}
	#endregion

	public void DeserializeWorld(){
		WorldDatabase.world = 
			JsonConvert.DeserializeObject <List<SerializableWorldObject>> (
				System.Text.Encoding.ASCII.GetString(CLZF2.Decompress(recievedWorld)));
		World.CreateWorld ();
	}

	public void TransferChanges(List<Change> changes){
		while (changes.Count > 0) {
			Change c = changes [0];
			if (gotWorld) {
				ObjectCommunicator.SendChangeMessage (c);
			} else {
				UpdateCache.Enqueue (c.id);
				clientParser.ConvertUpdateRequest ();
			}
			changes.Remove (c);
		}
	}

}