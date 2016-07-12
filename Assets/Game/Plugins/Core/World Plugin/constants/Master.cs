using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class Master {
	#region Server Settings
	public const int maxConnections = 1000;//Max connections
	public const int roomSize = 16;//Max room size
	public const int increaseSendBy = 17;//Parsing skip
	public const float saveTime = 500;
	#endregion

	#region RPC Call IDs and Objects for Both Client and Server
	//Ids
	public const short SendChangesId = 161;
	public const short RequestWorldId = 152;
	public const short SendWorldId = 162;
	public const short SendObjUpdateId = 170;
	public const short DestroyObjectRequestId = 171;

	//Objects
	public class SendWorld : MessageBase{
		public byte[] world;
		public int done;
		public int id;
	}

	public class DestroyObject: MessageBase{
		public string destroyKey;
	}

	public class SendChanges : MessageBase{
		public byte[] changes;
		public int id;
		public int done;
	}

	public class RequestWorld : MessageBase{
		public int id;
		public int host;
	}
	#endregion

	#region Directory Server
	public const short RequestServerId = 180;
	public const short ConnectServerId = 181;
	public const short UpdateServerId = 182;

	public class RequestServer: MessageBase{
		public string room;
		public string ip;
		public int port;
	}

	public class ConnectServer: MessageBase{
		public string ip;
		public int port;
	}

	public class UpdateServer: MessageBase{
		public string addRoom;
		public string removeRoom;
		public int numPlayers;
	}
	#endregion

	#region RPC Call IDs and Objects for Client
	//Ids
	public const short UpdatePlayersId = 160;
	public const short SetWorldId = 163;
	//Skip For WorldSetID
	public const short JoinedId = 164;
	public const short TestPingId = 165;
	public const short CreateObjectId = 166;
	public const short RoomFullId = 167;
	public const short RequestObjUpdateId = 169;
	public const short SaveWorldId = 173;

	//Objects
	public class UpdatePlayers : MessageBase{
		public int totalPlayers;
		public int playerNumber;
		public int connNumber;
		public int leftNumber;
		public int wasHost;
		public byte[] ids;
	}
	
	public class Joined : MessageBase{
	}

	public class RoomFull : MessageBase{
	}
	
	public class TestPing : MessageBase{
	}

	public class UpdateObject : MessageBase{
		public int conn;
		public int done;
		public byte[] updates;
	}

	public class CreateObject : MessageBase{
		public int name;
		public string trans;
		public string id;
		public string own;
	}

	public class RequestObjUpdate : MessageBase{
		public byte[] objUpdates;
		public int fromClient;
		public int done;
	}

	public class SaveWorld : MessageBase{
	}
	#endregion

	#region RPC Call IDs and Objects for Server
	//Ids
	public const short JoinRoomId = 150;
	public const short TestPingedId = 153;
	public const short ClientDisconnectId = 154;
	public const short RequestCreateObjectId = 155;

	//Objects
	public class JoinRoom : MessageBase{
		public string roomID;
		public string deviceID;
	}
	
	public class TestPinged : MessageBase{
	}

	public class ClientDisconnect : MessageBase{
	}
	#endregion
}
	