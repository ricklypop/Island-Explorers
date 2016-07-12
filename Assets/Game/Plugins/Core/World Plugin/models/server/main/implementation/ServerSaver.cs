using UnityEngine;
using System.Collections;

public class ServerSaver : Update {
	private float time; 

	/// <summary>
	/// Update this instance. If config is set, listen on port.
	/// </summary>
	public override void OnUpdate(){
		time += Time.deltaTime;

		if (time > Master.saveTime / MasterServer.rooms.Count 
			&& MasterServer.rooms.Count > 0) {

			Room r = GetNextRoom ();
			r.Save ();

			time = 0;
		} 

	}

	/// <summary>
	/// Gets the next unsaved room.
	/// </summary>
	/// <returns>The next unsaved room.</returns>
	Room GetNextRoom(){

		foreach (Room r in MasterServer.rooms.Values) {
			if (!r.saved) {
				return r;
			}
		}

		ResetSaves ();

		return GetNextRoom ();

	}

	/// <summary>
	/// Resets the saves.
	/// </summary>
	void ResetSaves(){

		foreach (Room r in MasterServer.rooms.Values) {
			r.saved = false;
		}

	}

}
