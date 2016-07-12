using UnityEngine;
using System.Collections;

namespace DeviceConsole
{
	public class MouseDragDetector : DragDetector
	{
		private Vector2 m_startPos;
		private Vector2 m_currentPos;
		private Vector2 m_delta;
		
		#if UNITY_EDITOR
		void Update()
		{
			if (Input.GetMouseButtonDown(0))
			{
				m_startPos = Input.mousePosition;
			}
			
			if (Input.GetMouseButton(0))
			{
				m_currentPos = Input.mousePosition;
				m_delta = m_currentPos - m_startPos;
				NotifyOnDrag(m_startPos, m_currentPos, m_delta);
				
				m_startPos = m_currentPos;
			}
		}
		#endif
	}
}

