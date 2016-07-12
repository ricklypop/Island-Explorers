using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Change {
	/// <summary>
	/// Gets or sets the identifier.
	/// </summary>
	/// <value>The identifier.</value>
	public int id { get; set; }

	/// <summary>
	/// Gets or sets the function.
	/// </summary>
	/// <value>The funcion.</value>
	public int func { get; set; }

	/// <summary>
	/// Gets or sets the arguments.
	/// </summary>
	/// <value>The arguments.</value>
	public Dictionary<int, string> args {get;set;}
}
