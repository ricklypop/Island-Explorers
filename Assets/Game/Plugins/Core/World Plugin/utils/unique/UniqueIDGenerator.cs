 using UnityEngine;
using System.Collections;
using System;

public static class UniqueIDGenerator {
	/// <summary>
	/// Gets a new unique Id.
	/// </summary>
	/// <returns>The unique Id.</returns>
	public static string GetUniqueID(){
		string uniqueID = "";
		uniqueID = string.Format("{0}_{1:N}", uniqueID, Guid.NewGuid());
		uniqueID = uniqueID.Substring (1, uniqueID.Length - 1);
		return uniqueID;
	}
}
