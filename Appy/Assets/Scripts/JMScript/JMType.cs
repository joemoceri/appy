using UnityEngine;
using System.Collections;

namespace JM.PrimitiveTypes
{
	/// <summary>
	/// JMType contains all of our JMScript primitive types
	/// int, float, string, bool
	/// 
	/// Each type has methods for converting between each of them.
	/// </summary>
	
	public class JMType 
	{
		
		/// <summary>
		/// Our interface allows us to create a list to contain all of our types.
		/// </summary>
		
		public interface IJMType
		{
			int toInt();
			float toFloat();
			string toString();
			bool toBool();
		};
		
		/// <summary>
		/// JMInt is our integer primitive type.
		/// </summary>
		
		sealed public class JMInt : IJMType 
		{
			public int num;
			public JMInt(int n)
			{
				num = n;
			}
			
			public int toInt()
			{
				return num;
			}
			
			public float toFloat()
			{
				return (float) num;
			}
			
			public string toString()
			{
				return num.ToString();
			}
			
			public bool toBool()
			{
				return num > 0;
			}
			
		}
		
		/// <summary>
		/// JMFloat is our float primitive type.
		/// </summary>
		
		sealed public class JMFloat : IJMType 
		{
			private float fl;
			public JMFloat(float f)
			{
				fl = f;
			}
			
			public int toInt()
			{
				return (int) fl;
			}
			
			public float toFloat()
			{
				return fl;
			}
			
			public string toString()
			{
				return fl.ToString();
			}
			
			public bool toBool()
			{
				return fl > 0.0f;
			}
		}
		
		/// <summary>
		/// JMString is our string primitive type.
		/// </summary>
		
		sealed public class JMString : IJMType 
		{
			private string str;
			public JMString(string s)
			{
				str = s;
			}
			
			public int toInt()
			{
				int temp = 0;
				return int.TryParse(str, out temp) ? temp : -1;
			}
			
			public float toFloat()
			{
				float temp = 0.0f;
				return  float.TryParse(str, out temp) ? temp : -1.0f;
			}
			
			public string toString()
			{
				return str.ToString();
			}
			
			public bool toBool()
			{
				return str == "" || str == string.Empty;
			}
		}
		
		/// <summary>
		/// JMBool is our boolean primitive type.
		/// </summary>
		
		sealed public class JMBool : IJMType
		{
			private bool b;
			public JMBool(bool tBool)
			{
				b = tBool;
			}
			
			public int toInt()
			{
				return b ? 1 : 0;
			}
			public float toFloat()
			{
				return b ? 1.0f : 0.0f;
			}
			public string toString()
			{
				return null;
			}
			
			public bool toBool()
			{
				return b;
			}
			
		}
		
	}
}