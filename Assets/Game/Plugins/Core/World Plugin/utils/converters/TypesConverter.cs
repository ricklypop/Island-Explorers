using UnityEngine;
using System.Collections.Generic;

public class TypesConverter: MonoBehaviour{
	public static TypesConverter main;

	#region Setting Lists For Converter
	public List<WorldObjectCache.Types> types = new List<WorldObjectCache.Types>();
	public List<Transform> convert = new List<Transform>();
	#endregion

	public void Start(){
		main = this;
	}

	#region Convertion Methods
	/// <summary>
	/// Converts the type to a Transform.
	/// </summary>
	/// <returns>The transform of the type.</returns>
	/// <param name="t">Type enum.</param>
	public static Transform ConvertType(WorldObjectCache.Types t){
		foreach(WorldObjectCache.Types type in main.types){
			if (type == t)
				return main.convert [main.types.IndexOf (type)];
		}
		return null;
	}

	/// <summary>
	/// Converts the transform to a type.
	/// </summary>
	/// <returns>The type enum.</returns>
	/// <param name="t">Transform.</param>
	public static WorldObjectCache.Types ConvertTransform(Transform t){
		foreach(Transform trans in main.convert){
			if (trans == t)
				return main.types [main.convert.IndexOf (trans)];
		}
		return WorldObjectCache.Types.NONE;
	}
	#endregion
}
