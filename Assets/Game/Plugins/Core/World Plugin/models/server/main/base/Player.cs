using UnityEngine.Networking;

public class Player{
	public int connectionID{ get; set; }//The connection id for this player
	public Room room { get; set; }//The room this player is in
	public string deviceID;
	public bool saved {get; set;}

	#region Player Functions
	/// <summary>
	/// Updates the players load balancing information.
	/// </summary>
	public void UpdatePlayer(int i, int t, int c, int l, bool wasHost, string[] ids){
		Master.UpdatePlayers update = new Master.UpdatePlayers ();
		update.playerNumber = i;
		update.totalPlayers = t;
		update.connNumber = c;
		update.leftNumber = l;
		update.ids = CLZF2.Compress (System.Text.Encoding.ASCII.GetBytes(string.Join(" ", ids)));
		if (wasHost)
			update.wasHost = 1;
		else
			update.wasHost = 0;
		NetworkServer.SendToClient (connectionID, Master.UpdatePlayersId, update);
	}

	/// <summary>
	/// Sends the change message to the client.
	/// </summary>
	public void SendMessage(Master.SendChanges msg){
		NetworkServer.SendToClient (connectionID, Master.SendChangesId, msg);
	}
	#endregion
}