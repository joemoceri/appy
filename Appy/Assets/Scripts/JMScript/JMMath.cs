using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using JM.Interpreter;

namespace JM.Math 
{
	public class JMMath : JMInterpreter
	{

		public JMInterpreter interpreter;
		public const float e = 2.71828f;
		public const float JMPI = 3.14159265359f;
		public const float Deg2Rad = JMPI / 180f, Rad2Deg = 180f / JMPI;

		public JMMath(JMInterpreter i) 
		{
			interpreter = i;
		}

		public static double Sin(double radians)
		{
			double r = ReduceAngle(radians);
			double result = r;
			int start = 3;
			for (int i = 0; i < 10; i++)
			{
				double n = Pow(r, start) / Factorial(start);
				result += i % 2 == 0 ? -n : n;
				start += 2;
			}
			//float result = radians - (Pow(radians, 3)/Factorial(3)) + (Pow(radians, 5)/Factorial(5)) - (Pow(radians, 7)/Factorial(7)) + (Pow(radians, 9)/Factorial(9));
			return result;
		}

		private static double ReduceAngle(double radians)
		{
			double angle = radians * Rad2Deg;
			return angle * Deg2Rad;
			//return (angle % 360) * Deg2Rad;
		}

		public static double Cos(double radians)
		{

			double r = ReduceAngle(radians), result = 1;
			int start = 2;
			for (int i = 0; i < 10; i++)
			{
				double n = Pow(r, start) / Factorial(start);
				result += i % 2 == 0 ? -n : n;
				start += 2;
			}
			//float result = 1 - (Pow(radians, 2)/Factorial(2)) + (Pow(radians, 4)/Factorial(4)) - (Pow(radians, 6)/Factorial(6)) + (Pow(radians, 8)/Factorial(8));
			return result;
		}

		public static double Tan(double angle)
		{
			return Sin(angle) / Cos(angle);
		}

		public static long Factorial(int num)
		{
			long result = num;

			for (int i = num; i-- > 1; )
			{
				result *= i;
			}
			return result;
		}

		public static float Pow(float n, float p)
		{
			float result = n;

			for (int i = 1; i++ < p; )
			{
				result *= n;
			}

			return result;
		}

		public static double Pow(double n, double p)
		{
			double result = n;

			for (int i = 1; i++ < p; )
			{
				result *= n;
			}
			return result;
		}

		public static int Abs(int n)
		{
			return n < 0 ? n * -1 : n;
		}

		public static float Abs(float n)
		{
			return n < 0.0f ? n * -1 : n;
		}

		public static double Abs(double n)
		{
			return n < 0.0 ? n * -1 : n;
		}


		public static int Clamp(int n, int min, int max)
		{
			return n > max ? max : n < min ? min : n;
		}

		public static float Clamp(float n, float min, float max)
		{
			return n > max ? max : n < min ? min : n;
		}

		public static double Clamp(double n, float min, float max)
		{
			return n > max ? max : n < min ? min : n;
		}

		public static int Ceil(float n)
		{
			return (int)(n > 0 ? n + 1 : n);
		}

		public static int Ceil(double n)
		{
			return (int)(n > 0 ? n + 1 : n);
		}

		public static int Floor(float n)
		{
			return (int)(n > 0 ? n : n - 1);
		}

		public static int Floor(double n)
		{
			return (int)(n > 0 ? n : n - 1);
		}

		public static int Round(float n)
		{
			float temp = (n - (int)n);
			return Abs(temp) > 0.5f ? (int)(n < 0 ? n - 1 : n + 1) : (int)n;
		}

		public static int Round(double n)
		{
			double temp = (n - (int)n);
			return temp >= 0.5f ? (int)(n + 1) : (int)n;
		}

		public static float Sqrt(float n)
		{
			float result = n;
			for (int i = 0; i < 6; i++)
			{
				result = 0.5f * (result + (n / result));
			}
			return result;
		}


		public static double Log(double n)
		{
			/*float start = 1f, result = Pow(e, start);
			for (int i = 0; i < 10; i++)
			{
				if(result > n)
				start++;
				result = Pow(e, start);
			}*/


			return n;
		}

		public static float Log(float n, float b)
		{
			return n;
		}

		public static double Log(double n, double b)
		{
			return n;
		}

		public static float Log10(float n)
		{
			return n;
		}

		public static double Log10(double n)
		{
			return n;
		}

		public static float Exp(float n)
		{
			float result = 1 + n;
			for (int i = 2; i < 20; i++)
			{
				result += Pow(n, i) / Factorial(i);
			}
			return result;
		}

		public static double Exp(double n)
		{
			double result = 1 + n;
			for (int i = 2; i < 20; i++)
			{
				result += Pow(n, i) / Factorial(i);
			}
			return result;
		}

		// Use when ready to implement IntList, FloatList, StringList, BoolList
		/*
		public static string Range(int start, int stop, int step)
		{
			StringBuilder temp = new StringBuilder();
			temp.Append('[');
			for(int i = start; i < stop; i+=step)
			{
				temp.Append(i + ", ");
			}
			temp.Replace(',', ']', temp.Length - 2, 1);
			Debug.Log("temp is: " + temp);
			return temp.ToString();
		}*/

		public static List<int> Range(int start, int stop, int step)
		{
			List<int> list = new List<int>();
			for(int i = start; i < stop; i+=step)
			{
				list.Add(i);
			}
			Debug.Log("list is: " + list);
			return list;
		}

	}

}
