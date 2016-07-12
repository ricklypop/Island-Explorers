using UnityEngine;
using System.Collections;

namespace DeviceConsole
{
	public static class GUIHelper
	{
		private static Color m_contentColor = Color.white;
		private static Color m_backgroundColor = Color.white;

		public static Color ContentColor
		{
			get {return m_contentColor;}
		}
		
		public static Color BackgroundColor
		{
			get {return m_backgroundColor;}
		}
		
		public static void SaveColors()
		{
			m_contentColor = GUI.contentColor;
			m_backgroundColor = GUI.backgroundColor;
		}
		
		public static void RestoreColors()
		{
			GUI.contentColor = m_contentColor;
			GUI.backgroundColor = m_backgroundColor;
		}
		
		public static void SetContentColor(Color color)
		{
			GUI.contentColor = color;
		}
		
		public static void RestoreContentColor()
		{
			GUI.contentColor = m_contentColor;
		}
		
		public static void SetBackgroundColor(Color color)
		{
			GUI.backgroundColor = color;
		}
		
		public static void RestorBackgroundColor()
		{
			GUI.backgroundColor = m_backgroundColor;
		}
		
		public static bool Toggle(bool flag, GUIContent content, params GUILayoutOption[] options)
		{
			Color bgColor = flag ? Color.black : m_backgroundColor;
			SetBackgroundColor(bgColor);
			
			if (GUILayout.Button(content, options))
			{
				flag = !flag;
			}
			
			RestorBackgroundColor();
			return flag;
		}
		
		public static bool AdsorbToFullScreen(ref Rect r, int padding)
		{
			bool flag = false;
			
			// left
			int left = (int)r.x;
			if (left != 0)
			{
				if (left <= padding)
				{
					r.x = 0;
					flag = true;
				}
			}
			
			// right
			int right = (int)(r.x + r.width);
			if (right != Screen.width)
			{
				if (Screen.width - right <= padding)
				{
					r.x = Screen.width - r.width;
					flag = true;
				}
			}
			
			// top
			int top = (int)r.y;
			if (top != 0)
			{
				if (top <= padding)
				{
					r.y = 0;
					flag = true;
				}
			}
			
			// bottom
			int bottom = (int)(r.y + r.height);
			if (bottom != Screen.height)
			{
				if (Screen.height - bottom <= padding)
				{
					r.y = Screen.height - r.height;
					flag = true;
				}
			}
			
			return flag;
		}
		
		public static Texture2D CreateIcon(Color c, int w, int h, int dw, int dh)
		{
			Texture2D tex = new Texture2D(w, h, TextureFormat.ARGB32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;

			Color emptyColor = new Color(0, 0, 0, 0);
			Color shadowColor = Color.black;
			Color highlightColor = Color.Lerp(c, Color.white, 0.25f);
			Color lowlightColor = Color.Lerp(c, Color.black, 0.25f);
			Color contentColor = Color.Lerp(c, Color.black, 0.1f);

			for (int x = 0; x < w; x++)
			{
				for (int y = 0; y < h; y++)
				{
					// left bottom corner and right up corner
					if ((x < dw && y < dh) || (x >= w - dw && y >= h - dh))
					{
						tex.SetPixel(x, y, emptyColor);
					}
					// shadow
					else if ((x >= dw && y < dh) || (x >= w - dw && y < h - dh))
					{
						tex.SetPixel(x, y, shadowColor);
					}
					// highlight
					else if (y == h - 1)
					{
						tex.SetPixel(x, y, highlightColor);
					}
					// lowlight
					else if (y == dh)
					{
						tex.SetPixel(x, y, lowlightColor);
					}
					// content
					else
					{
						tex.SetPixel(x, y, contentColor);
					}
				}
			}

			tex.Apply();
			return tex;
		}

        public static Texture2D CreateImage(Color c, int w, int h)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.ARGB32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;

            int num = w * h;
            Color[] colors = new Color[num];
            for (int i = 0; i < num; i++)
            {
                colors[i] = c;
            }
            tex.SetPixels(colors);

            tex.Apply();
            return tex;
        } 
	}
}


