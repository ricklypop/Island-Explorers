using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UpdateCache : MonoBehaviour {
	public static readonly Queue<int> UPDATEQUEUE = new Queue<int>();

	public static void Enqueue (int id)
	{
		UPDATEQUEUE.Enqueue (id);
	}

	public static int GetUpdateCount ()
	{
		return UPDATEQUEUE.Count;
	}
}
