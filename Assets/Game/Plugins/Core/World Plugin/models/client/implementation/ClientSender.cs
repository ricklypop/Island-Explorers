using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class ClientSender {
	private NetworkClient network;
	private ClientParser clientParser;
	private Client client;

	public ClientSender(ClientSession clientSession){
		this.client = clientSession.client;
		this.network = clientSession.network;
		this.clientParser = clientSession.clientParser;
	}


	#region Send World and Updates as Requested
	/// <summary>
	/// Sends the updates, by parsed piece.
	/// </summary>
	public void SendRequestedUpdates ()
	{
		
		foreach (int index in clientParser.currentRequestedUpdateIndexs.Keys) {


			if (!clientParser.RequestedUpdateConversionFinished(index)) {
				Master.UpdateObject upd = new Master.UpdateObject ();
				upd.updates = clientParser.ParseUpdates (index);

				if (clientParser.RequestedUpdateConversionComplete(index)) {
					
					upd.done = 1;

					client.recievedUpdateRequest.Remove (index);
					clientParser.RemoveRequestedUpdate (index);

				} else{
					upd.done = 0;
				}

				network.Send (Master.SendObjUpdateId, upd);
			}


		}

	}

	/// <summary>
	/// Sends the world, by parsed piece.
	/// </summary>
	public void SendWorld ()
	{
		Stack<int> remove = new Stack<int> ();

		foreach (int index in clientParser.currentWorldIndexs.Keys) {
			
			if (!WorldDatabase.gettingWorld) {
				
				Master.SendWorld msg = new Master.SendWorld ();

				msg.id = index;

				byte[] b = clientParser.ParseWorldSend (index);
				if (b.Length > 0) {
					
					msg.world = b;
					network.Send (Master.SendWorldId, msg);

				} else {
					
					msg.done = 2;

					network.Send (Master.SendWorldId, msg);
					remove.Push(index);

				}

			}

		}

		foreach (int key in remove) {
			clientParser.RemoveConvertedWorld (key);
		}
	}

	/// <summary>
	/// Requests the send changes.
	/// Request to send the current changes.
	/// If the list is not empty and there aren't any changes being streamed, set up a JSON string to stream
	/// Stream the current converted changes to other clients
	/// </summary>
	public void SendChanges ()
	{
		//If the list is not empty and there aren't any changes being streamed, set up a JSON string to stream
		if (ChangeCache.GetChangeCount() > 0 && clientParser.ChangeConversionFinished() ) {

			clientParser.ConvertNewChanges ();
			ChangeCache.Clear ();

		}

		//Stream the current converted changes to other clients
		if (!clientParser.ChangeConversionFinished()) {

			Master.SendChanges msg = new Master.SendChanges ();

			byte[] b = clientParser.ParseChangesSend ();
			msg.changes = b;

			if (clientParser.ChangeConversionCompleted ()) {
				msg.done = 1;
				clientParser.ResetConvertedChanges ();
			} else {
				msg.done = 0;
			}

			msg.id = LoadBalancer.player;
			network.Send (Master.SendChangesId, msg);

		}
	}
	#endregion

}
