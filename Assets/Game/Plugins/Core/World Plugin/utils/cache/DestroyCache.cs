using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DestroyCache : MonoBehaviour {
	public static readonly Queue<WorldObject> DESTROYQUEUE = new Queue<WorldObject>();

	public static void Enqueue (WorldObject worldObject)
	{
		DESTROYQUEUE.Enqueue (worldObject);
	}

	public static WorldObject Dequeue ()
	{
		return DESTROYQUEUE.Dequeue ();
	}

	public static int GetDestroyCount ()
	{
		return DESTROYQUEUE.Count;
	}
}
