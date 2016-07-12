using UnityEngine.Networking;
using System.Collections.Generic;

public class Room{
	public static Dictionary<int, Player> allPlayers = new Dictionary<int, Player> ();

	#region Values of this Room
	public List<Player> players = new List<Player> ();
	public Player host;
	public string ID{ get; set; }
	public int totalPlayers{ get; set; }
	public bool saved{ get; set; }
	#endregion

	#region Room Functions
	/// <summary>
	/// Adds the player to the room.
	/// </summary>
	public void AddPlayer(Player player){
		players.Add (player);
		allPlayers.Add (player.connectionID, player);
		host = players [0];

		string[] plays = new string[players.Count];
		for (int i = 0; i < players.Count; i++)
			plays [i] = players [i].deviceID;

		for (int i = 0; i < players.Count; i++)
			players [i].UpdatePlayer (i + 1, players.Count, players[i].connectionID, -1, false, plays);
		totalPlayers = players.Count;
	}

	/// <summary>
	/// Removes the player from the room.
	/// </summary>
	public void RemovePlayer(Player player){
		bool wasHost = false;
		if (player == host)
			wasHost = true;
		int left = player.connectionID;
		players.Remove (player);
		allPlayers.Remove (player.connectionID);
		totalPlayers = players.Count;
		if (players.Count != 0) {
			host = players [0];

			string[] plays = new string[players.Count];
			for (int i = 0; i < players.Count; i++)
				plays [i] = players [i].deviceID;

			for (int i = 0; i < players.Count; i++)
				players [i].UpdatePlayer (i + 1, players.Count, players[i].connectionID, left, wasHost, plays);
			MasterServerClient.main.UpdateServer (allPlayers.Count, "", "");
		}
		else {
			MasterServer.rooms.Remove (ID);
			MasterServerClient.main.UpdateServer (allPlayers.Count, "", ID);
		}
	}

	/// <summary>
	/// Sends the change message.
	/// </summary>
	public void SendMessage(Player player, Master.SendChanges msg){
		foreach(Player p in players)
			if(player != p)
				p.SendMessage(msg);
	}

	/// <summary>
	/// Save this room world.
	/// </summary>
	public void Save(){
		Player p = NextSavePlayer ();
		NetworkServer.SendToClient (p.connectionID, Master.SaveWorldId, new Master.SaveWorld());
		saved = true;
	}

	/// <summary>
	/// Gets the next save player.
	/// </summary>
	/// <returns>The save player.</returns>
	Player NextSavePlayer(){
		foreach (Player p in players)
			if (!p.saved) {
				p.saved = true;
				return p;
			}
		ResetSaves ();
		return NextSavePlayer();
	}

	/// <summary>
	/// Resets the saves.
	/// </summary>
	void ResetSaves(){
		foreach (Player p in players)
			p.saved = false;
	}
	#endregion
}