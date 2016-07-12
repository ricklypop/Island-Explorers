using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class ClientDirectory {
	private NetworkClient network;
	private Client client;

	public bool wentToDirectory{ get; set; }

	public ClientDirectory(ClientSession clientSession){
		this.client = clientSession.client;
		this.network = clientSession.network;
	}

	public void RegisterOnNetwork ()
	{
		network.RegisterHandler (Master.RequestServerId, GetServer);
	}

	/// <summary>
	/// Reset this instance of the client.
	/// </summary>
	public IEnumerator ConnectToServer (string ip, int port)
	{
		
		client.StopClient (false);

		yield return new WaitForSeconds (1);

		wentToDirectory = true;
		client.StartClient (ip, port);

	}

	public void GetServer(NetworkMessage m){
		
		var msg = m.ReadMessage<Master.RequestServer> ();

		string ip = msg.ip;
		int port = msg.port;

		DisableLogging.Logger.Log ("Connecting to: " + ip + ":" + port);
		client.StartCoroutine(ConnectToServer (ip, port));

	}

	public void RequestServer(){
		
		Master.RequestServer server = new Master.RequestServer ();

		server.room = WorldDatabase.currentWorldID;
		network.Send (Master.RequestServerId, server);

	}
}
