using UnityEngine;
using System.Collections;

namespace DeviceConsole
{
	public class DragDetector : MonoBehaviour
	{
		public event System.Action<Vector2, Vector2, Vector2> onDrag;
		
		protected void NotifyOnDrag(Vector2 startPos, Vector2 endPos, Vector2 delta)
		{
			if (onDrag != null)
			{
				onDrag(startPos, endPos, delta);
			}
		}
	}
}

