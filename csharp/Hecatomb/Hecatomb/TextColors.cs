/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/10/2018
 * Time: 2:07 PM
 */
using System;
using System.Collections.Generic;

namespace Hecatomb
{
	/// <summary>
	/// Description of ColoredText.
	/// </summary>
	public class TextColors
	{
		public Dictionary<Tuple<int, int>, string> Colors;
		public TextColors(params object[] c)
		{
			Colors = new Dictionary<Tuple<int, int>, string>();
			int p = 0;
			int i = 0;
			int row, col;
			string color;
			while (i<c.Length)
			{	
				if (c[i] is string)
				{
					row = p;
					col = 0;
					color = (string) c[i];
				}
				else if (c[i] is int)
				{
					row = (int) c[i];
					if (c[i+1] is string)
					{
						col = 0;
						color = (string) c[i+1];
						i+=1;
					}
					else
					{
						col = (int) c[i+2];
						color = (string) c[i+2];
						i+=2;
					}
					p+=1;
					Colors[new Tuple<int, int>(row, col)] = color;
					i+=1;
				}
			}
		}
		
		public string this[int i, int j]
	   	{
			get
			{
				if (!Colors.ContainsKey(new Tuple<int, int>(i, j)))
				{
					return null;
				}
				return Colors[new Tuple<int, int>(i, j)];
			}
			set { Colors[new Tuple<int, int>(i, j)] = value; }
	   	}
		
		public Dictionary<Tuple<int, int>, string>.KeyCollection Keys
		{
			get {return Colors.Keys;}
			private set{}
		}
		
		public void Clear()
		{
			Colors.Clear();
		}
	}
}
