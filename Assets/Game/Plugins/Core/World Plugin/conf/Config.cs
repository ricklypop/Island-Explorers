using UnityEngine;
using System.Collections;

public class Config {
	public string address { get; set; }
	public string ip { get; set; }
	public int port { get; set; }
	public int version {get; set;}

	public int maxBytes{ get; set;}
	public ushort packetSize{ get; set;}
	public ushort maxSendMessages{ get; set;}
	public int minTotalPlayers{ get; set;}
}
