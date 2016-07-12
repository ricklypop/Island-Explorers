using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChangeCache {
	public static readonly Queue<Change> CHANGEQUEUE = new Queue<Change>();

	public static void Enqueue (Change change)
	{
		CHANGEQUEUE.Enqueue (change);
	}

	public static int GetChangeCount(){
		return CHANGEQUEUE.Count;
	}

	public static void Clear(){
		CHANGEQUEUE.Clear ();
	}
}
