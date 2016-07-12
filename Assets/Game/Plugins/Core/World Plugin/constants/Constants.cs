using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Constants {
	public static readonly float MOVEMENTFACTOR = 1;

	public const float RETRYTIME = 4;
	public const float OPACADD = 0.05f;

	#region World Defining Values

	public static readonly bool DEFAULTINSTANTTURN = true;
	public static readonly int DEFAULTZ = 0;
	#endregion

	#region Server and Client Communication With World Values
	public static readonly string[] METHODS = {"ClientMove", "ClientStopMove", "ClientChangeTempPlayer"};
	public static readonly string[] VARS = {"velocity", "direction", "x", "y", "rX", "rY", "rZ", "z", "name"};
	public static readonly CompressionDictionary methods = new CompressionDictionary ();
	public static readonly CompressionDictionary vars = new CompressionDictionary ();
	#endregion

}