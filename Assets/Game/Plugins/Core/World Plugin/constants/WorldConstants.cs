using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;

public class WorldConstants {
	public enum WorldVars{
		[StringValue("v")] VELOCITY,  
		[StringValue("d")] DIRECTION,  
		[StringValue("x")] X, 
		[StringValue("y")] Y, 
		[StringValue("z")] Z, 
		[StringValue("z")] ROTATION,
		[StringValue("rV")] ROTATION_VELOCITY,
		[StringValue("rX")] ROTATION_X , 
		[StringValue("rY")] ROTATION_Y, 
		[StringValue("rZ")] ROTATION_Z, 
		[StringValue("tP")] TEMP_PLAYER,
		[StringValue("n")] NAME
	}

	public enum WorldMethods{
		[StringValue("ClientChangeVars")] CHANGE_CLIENT_VARS,
		[StringValue("ClientRotate")] ROTATE_CLIENT,
		[StringValue("ClientStopRotate")]STOP_ROTATE_CLIENT,
		[StringValue("ClientMove")] MOVE_CLIENT, 
		[StringValue("ClientStop")] STOP_CLIENT, 
		[StringValue("ClientChangeTempPlayer")] CHANGE_PLAYER_TEMP
	}

	public const int WORLD_VARS_LENGTH = 12;
	public const int WORLD_METHODS_LENGTH = 6;

	public static readonly float MOVEMENTFACTOR = 1;

	public const float RETRYTIME = 4;
	public const float OPACADD = 0.05f;

	#region World Defining Values

	public static readonly bool DEFAULTINSTANTTURN = true;
	public static readonly int DEFAULTZ = 0;
	#endregion


}