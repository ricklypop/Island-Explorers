using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace DeviceConsole
{
	public class LogToBuffer
	{
		private LinkedList<LogEntry> m_entries;
		private int m_maxCount = 200;
		
		public LogToBuffer()
		{
			m_entries = new LinkedList<LogEntry>();
		}
		
		internal void LogImpl(LogEntry entry)
		{
			m_entries.AddLast(entry);
			
			if (m_entries.Count > m_maxCount)
			{
				int n = m_entries.Count - m_maxCount;
				for (int i = 0; i < n; i++)
				{
					m_entries.RemoveFirst();
				}
			}
		}
		
		public void Clear()
		{
			m_entries.Clear();
		}
		
		public int Count
		{
			get {return m_entries.Count;}
		}
		
		public int MaxCount
		{
			get {return m_maxCount;}
			set {m_maxCount = value;}
		}
		
		public LinkedList<LogEntry> Entries
		{
			get {return m_entries;}
		}
	}
}
