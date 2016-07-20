using UnityEngine;
using System.Collections;

public class BaseGameConstants {
	public enum BaseGameVars{
		[StringValue("pX")] POINT_X = WorldConstants.WORLD_VARS_LENGTH,
		[StringValue("pY")] POINT_Y,
		[StringValue("t")] TERRAIN,
		[StringValue("b")] BIOME
	}

	public enum BaseGameMethods{
		[StringValue("ClientMovePoint")] MOVE_CLIENT_POINT = WorldConstants.WORLD_METHODS_LENGTH,
	}

	public const int BASE_VARS_LENGTH = WorldConstants.WORLD_VARS_LENGTH + 4;
	public const int BASE_METHODS_LENGTH = WorldConstants.WORLD_METHODS_LENGTH + 1;

	public const float POINTRELATIVEDISTANCE = 0.02f;
	public const float POINTUPDATETIME = 0.25f;
	public const float DEFAULTMOVEMENTVELOCITY = 2f;
}
