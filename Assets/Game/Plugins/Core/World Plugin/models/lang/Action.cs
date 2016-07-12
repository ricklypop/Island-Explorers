using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface Action{
	/// <summary>
	/// Act the specified entity.
	/// </summary>
	void Act(Entity e);
}