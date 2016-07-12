using UnityEngine;
using System.Collections;

public class Spin : MonoBehaviour {
	public int speed;
	void Update () {
		transform.eulerAngles -= new Vector3 (0, 0, speed);
	}
}
