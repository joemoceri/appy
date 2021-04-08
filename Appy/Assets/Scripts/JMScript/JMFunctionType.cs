using UnityEngine;
using System.Collections;

namespace JM.FunctionType
{

	/// <summary>
	/// JMFunctionType determines what primitive types JMScript supports.
	/// Int, Float, String, Bool
	/// 
	/// Each function call has a <c>Type[]</c> for it, matching each parameter for it's type. 
	/// </summary>

	public static class JMFunctionType 
	{
		public enum Type { Int, Float, String, Bool};

		public static Type[] Print = new Type[] { Type.String };

		public static Type[] ReadFile = new Type[] {Type.String};

		public static Type[] Sin = new Type[] {Type.Float};
		public static Type[] Cos = new Type[] {Type.Float};
		public static Type[] Tan = new Type[] {Type.Float};
		public static Type[] Factorial = new Type[] {Type.Int};
		public static Type[] Pow = new Type[] {Type.Float, Type.Int};
		public static Type[] Abs = new Type[] {Type.Float};
		public static Type[] Clamp = new Type[] { Type.Float, Type.Float, Type.Float };
		public static Type[] Exp = new Type[] { Type.Float };
		public static Type[] Sqrt = new Type[] { Type.Float };
		public static Type[] Ceil = new Type[] { Type.Float };
		public static Type[] Floor = new Type[] { Type.Float };
		public static Type[] Round = new Type[] { Type.Float };

		public static Type[] Range = new Type[] {Type.Int, Type.Int, Type.Int};
	}
}
