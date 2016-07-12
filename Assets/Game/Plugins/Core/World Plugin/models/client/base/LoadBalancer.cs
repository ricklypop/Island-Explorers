using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class LoadBalancer {
	#region Static Values for the Load Balancer
	public static int player{ get; set; }//The player number in the room
	public static int totalPlayers{ get; set; }//The total players
	public static int connectionID{ get; set; }//This client's connection id on the server
	public static string[] playerIDs;
	#endregion

	#region Static Methods for the Load Balancer
	/// <summary>
	/// Balance this Client. Turn on and off appropriate scripts.
	/// </summary>
	public static void Balance(){
		List<Entity> bal = WorldObjectCache.PrepareForBalance ();

		int total = bal.Count;
		int start = (total / totalPlayers) * (player - 1);
		int end = start + (total / totalPlayers);

		if (player == totalPlayers)
			end = total;

		for (int i = start; i < end; i ++)
			bal [i].active = true;
	}
	#endregion
}
