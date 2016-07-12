using UnityEngine;
using System.Collections;
using Newtonsoft.Json;

public class SerializableTransform{
	#region Getters and Setters
	public float x { get; set; }
	public float y { get; set; }
	public float z { get; set; }
	public float xRotation { get; set; }
	public float yRotation { get; set; }
	public float xSize {get; set;}
	public float ySize{ get; set; }
	#endregion

	#region Constructors
	/// <summary>
	/// Initializes a new instance of the <see cref="CompressedTransform"/> class.
	/// </summary>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	/// <param name="z">The z coordinate.</param>
	/// <param name="xRotation">X rotation.</param>
	/// <param name="yRotation">Y rotation.</param>
	/// <param name="xSize">X size.</param>
	/// <param name="ySize">Y size.</param>
	[JsonConstructor]
	public SerializableTransform(float x, float y, float z, float xRotation, float yRotation, float xSize, float ySize){
		this.x = x;
		this.y = y;
		this.z = z;
		this.xRotation = xRotation;
		this.yRotation = yRotation;
		this.xSize = xSize;
		this.ySize = ySize;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="CompressedTransform"/> class.
	/// </summary>
	/// <param name="t">Transform.</param>
	public SerializableTransform(Transform t){
		x = t.position.x;
		y = t.position.y;
		xRotation = t.eulerAngles.x;
		yRotation = t.eulerAngles.y;
		xSize = t.localScale.x;
		ySize = t.localScale.y;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="CompressedTransform"/> class.
	/// </summary>
	/// <param name="v">Vector3.</param>
	public SerializableTransform(Vector3 v){
		x = v.x;
		y = v.y;
		z = v.y;
	}
	#endregion

	#region Convertion Methods
	/// <summary>
	/// Converts to a position.
	/// </summary>
	/// <returns>The Vector3 position.</returns>
	public Vector3 toPosition(){
		return new Vector3 (x, y, z);
	}

	/// <summary>
	/// Converts into a transform.
	/// </summary>
	/// <returns>The transform.</returns>
	public Transform ToTransform(){
		GameObject obj = new GameObject();
		Transform convert = obj.transform;
		convert.position = new Vector3 (x, y, 0);
		convert.eulerAngles = new Vector3 (xRotation, yRotation, 0);
		convert.localScale = new Vector3 (xSize, ySize, 0);
		GameObject.Destroy (obj);
		return convert;
	}
	#endregion
}
