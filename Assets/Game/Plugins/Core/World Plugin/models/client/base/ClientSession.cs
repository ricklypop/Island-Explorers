using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class ClientSession {
	public NetworkClient network{ get; set; }
	public Client client { get; set; }
	public ClientParser clientParser{ get; set; }

	public ClientSession(NetworkClient network, Client client, ClientParser clientParser){
		this.network = network;
		this.client = client;
		this.clientParser = clientParser;
	}
}
