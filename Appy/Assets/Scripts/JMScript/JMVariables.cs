using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using JM.PrimitiveTypes;
using JM.FunctionType;
using JM.Interpreter;

namespace JM.Variables
{
	/// <summary>
	/// JM Variables is what handles all of our variable declarations, 
	/// parameter verification, internally our primitive types. It contains
	/// the dictionary holding all of our user declared variables and their values.
	/// </summary>
	
	public class JMVariables 
	{
		private JMInterpreter interpreter;
		private string[] reservedKeywords = new string[] {"enter", "exit", "movementMode", "launchMode", "rotate", "move", "launch"};
		private Dictionary<string, string> globalVariables = new Dictionary<string, string>(), localVariables = new Dictionary<string, string>();
		public Dictionary<string, string> GlobalVariables{get{return globalVariables;}}
		public Dictionary<string, string> LocalVariables{get{return localVariables;} set{localVariables = value;}} 
		public List<FunctionVariables> functionVariables = new List<FunctionVariables>();

		public JMVariables(JMInterpreter i)
		{
			interpreter = i;
			functionVariables.Add(new FunctionVariables(new Dictionary<string, string>()));
		}

		/// <summary>
		/// Update helper called from JMInitializer
		/// </summary>
		
		public void Update()
		{
			
		}
		
		/// <summary>
		/// This is the starting point for JMVariables. Line is the 
		/// user input. 
		/// </summary>
		/// <returns><c>true</c>, if our variable was updated <c>false</c> otherwise.</returns>
		/// <param name="line">Line is the user input.</param>
		
		public void CheckVariable(string lineCommand)
		{
			CheckForShorthandOperations(ref lineCommand);
			string pattern = @"(=)";
			List<string> list = Regex.Split(lineCommand, pattern).toList();
			list.removeExtraEntries();
			//list[0].Replace(" ", string.Empty);
			//list.removeWhitespace(); // so the key can match up in the dictionary correctly every time
			
			bool t = true;

			if(list[1] != "=")
			{
				t = false;
			}
			else if(checkReservedKeywords(list[0]))
			{
				t = false;
			}
			
			if(t)
			{
				UpdateVariable(list);
			}

			if (localVariables.ContainsKey(list[0]))
			{
				//Debug.Log("Local Variable: " + list[0] + ": ." + localVariables[list[0]] + ".");
			}
			else if(globalVariables.ContainsKey(list[0]))
			{
				//Debug.Log("Global Variable: " + list[0] + ": ." + globalVariables[list[0]] + ".");
			}
			
		}

		/// <summary>
		/// Finds the variable and sets it equal to the stored value, represented as a string
		/// from JMArithmetic
		/// </summary>
		/// <param name="line">Line is the user input.</param>
		
		private void UpdateVariable(List<string> line)
		{
			int curFuncAmount = interpreter.jmQueue.CurFunctionAmount;
			line[0] = line[0].Replace(" ", string.Empty);
			if(line[1].Equals("="))
			{
				if (curFuncAmount > 0)
				{
					LocalVariableUpdate(line);
				}
				else 
				{
					GlobalVariableUpdate(line);
				}
			}
		}

		private void LocalVariableUpdate(List<string> line) 
		{
			if (localVariables.ContainsKey(line[0]))
			{
				if (interpreter.jmUserFunctions.checkIfUserDefinedFunction(line[2]))
				{
					interpreter.jmQueue.updateUserFunctionCallCheck();
				}
				localVariables[line[0]] = interpreter.jmArithmetic.CheckExpression(line[2]);
			}
			else
			{
				line[2] = interpreter.jmArithmetic.CheckExpression(line[2]);
				localVariables.Add(line[0], line[2]);
				if (interpreter.jmUserFunctions.checkIfUserDefinedFunction(line[2]))
				{
					interpreter.jmQueue.updateUserFunctionCallCheck();
				}
			}
		}

		private void GlobalVariableUpdate(List<string> line) 
		{
			if (globalVariables.ContainsKey(line[0]))
			{
				if (interpreter.jmUserFunctions.checkIfUserDefinedFunction(line[2]))
				{
					interpreter.jmQueue.updateUserFunctionCallCheck();
				}
				globalVariables[line[0]] = interpreter.jmArithmetic.CheckExpression(line[2]);
			}
			else
			{
				if (interpreter.jmUserFunctions.checkIfUserDefinedFunction(line[2]))
				{
					interpreter.jmQueue.updateUserFunctionCallCheck();
				}
				line[2] = interpreter.jmArithmetic.CheckExpression(line[2]);
				globalVariables.Add(line[0], line[2]);
			}
		}

		private void CheckForShorthandOperations(ref string lineCommand)
		{
			string pattern = @"(\+\+)|(\+\=)|(\-\-)|(\-\=)|(\*\=)|(\/\=)";
			List<string> list = Regex.Split(lineCommand, pattern, RegexOptions.IgnorePatternWhitespace).toList();

			if (list.Count > 1)
			{
				if (list[1] == "++")
				{
					lineCommand = list[0] + "=" + list[0] + " + 1";
				}
				else if (list[1] == "+=")
				{
					lineCommand = list[0] + "=" + list[0] + "+ (" + list[2] + ")";
				}
				else if (list[1] == "--")
				{
					lineCommand = list[0] + "=" + list[0] + " - 1";
				}
				else if (list[1] == "-=")
				{
					lineCommand = list[0] + "=" + list[0] + "- (" + list[2] + ")";
				}
				else if (list[1] == "*=")
				{
					lineCommand = list[0] + "=" + list[0] + "* (" + list[2] + ")";
				}
				else if (list[1] == "/=")
				{
					lineCommand = list[0] + "=" + list[0] + "/ (" + list[2] + ")";
				}
			}
		}

		/// <summary> 
		/// Checks if the variable the user is trying to declare is taken.
		/// </summary>
		/// <returns><c>true</c>, if reserved keywords was found, <c>false</c> otherwise.</returns>
		/// <param name="line">Line is the variable 'word' the user is trying to declare.</param>
		
		private bool checkReservedKeywords(string line)
		{
			for(int i = 0; i < reservedKeywords.Length; i++)
			{
				if(line.Equals(reservedKeywords[i]))
				{
					//Debug.Log("Error. '" + line[0] + "' is a reserved keyword!");
					return true;
				}
			}
			return false;
		}
		
		/// <summary>
		/// Verifies the parameters based on the function parameter type, and the user input specified.
		/// </summary>
		/// <returns>A generic JMScript primitive type list containing all of the values.</returns>
		/// <param name="exp">Exp is the sub expressions to be checked.</param>
		/// <param name="type">Type is the function parameter types to check against.</param>
		/// <param name="verified">Verified is used to determine whether or not it all user parameters match with their type.</param>
		/// <param name="list">List is what is returned, after being added to.</param>
		
		public List<JMType.IJMType> VerifyParameters(List<string> exp, JMFunctionType.Type[] type, out bool verified, List<JMType.IJMType> list)
		{
			verified = false;
			
			for(int i = 0; i < exp.Count; i++)
			{
				exp[i] = interpreter.jmArithmetic.CheckExpression(exp[i]);
			}
			 
			for(int i = 0; i < type.Length; i++)
			{
				if(type[i] == JMFunctionType.Type.Int)
				{
					int temp = 0;
					verified = VerifyVariables (exp[i], out temp);
					if(!verified) temp = -1;
					list.Add(new JMType.JMInt(temp));
				}
				else if(type[i] == JMFunctionType.Type.Float)
				{
					float temp = 0.0f;
					verified = VerifyVariables(exp[i], out temp);
					if(!verified) temp = -1.0f;
					list.Add(new JMType.JMFloat(temp));
				}
				else if(type[i] == JMFunctionType.Type.String)
				{
					string temp = string.Empty;
					verified = true;
					if(!verified){ temp = null;} else{ temp = exp[i];}
					list.Add(new JMType.JMString(temp));
				}
				else if(type[i] == JMFunctionType.Type.Bool)
				{
					bool temp;
					verified = VerifyVariables(exp[i], out temp);
					if(!verified) {temp = false;}
					list.Add(new JMType.JMBool(temp));
				}
			}
			
			
			return list;
		}
		
		/// <summary>
		/// Verifies the variables for booleans.
		/// Has a series of checks to make sure the type matches up correctly, if the 
		/// function type is of boolean.
		/// </summary>
		/// <returns><c>true</c>, if variables was verified, <c>false</c> otherwise.</returns>
		/// <param name="varName">Variable name is the string representation of what should be a boolean.</param>
		/// <param name="val">Value is what the expression evaluates to.</param>
		
		private bool VerifyVariables(string varName, out bool val)
		{
			bool temp = false;
			temp = checkParameter(varName, temp);
			bool flag = false;
			flag = temp ? true : false;
			flag = !flag ? bool.TryParse(varName, out temp) : true; 
			val = temp;
			return flag;
		}
		
		/// <summary>
		/// Checks the parameter if it is a variable the user declared. 
		/// </summary>
		/// <returns><c>true</c>, if the variable was found, <c>false</c> otherwise.</returns>
		/// <param name="choice">Choice is the key to check against.</param>
		/// <param name="varValue">varValue is used both as a container and as the return value if found.</param>
		
		private bool checkParameter(string choice, bool varValue)
		{
			foreach(var val in globalVariables)
			{
				if(val.Key == choice){ return bool.TryParse(val.Value, out varValue) ? varValue : false;}
			} 
			return false;
		}
		
		/// <summary>
		/// Verifies the variables for ints.
		/// Has a series of checks to make sure the type matches up correctly, if the 
		/// function type is of int. 
		/// </summary>
		/// <returns><c>true</c>, if variables was verified, <c>false</c> otherwise.</returns>
		/// <param name="varName">Variable name is the string representation of what should be a int.</param>
		/// <param name="val">Value is what the expression evaluates to.</param>
		
		private bool VerifyVariables(string varName, out int val)
		{
			int temp = 0;
			temp = checkParameter(varName, temp);
			bool flag = false;
			flag = temp != -1 ? true : false;
			flag = !flag ? int.TryParse(varName, out temp) : true; 
			val = temp;
			return flag;
		}
		
		/// <summary>
		/// Checks the parameter if it is a variable the user declared. 
		/// </summary>
		/// <returns><c>varValue</c>, if the variable was found, <c>-1</c> otherwise.</returns>
		/// <param name="choice">Choice is the key to check against.</param>
		/// <param name="varValue">varValue is used both as a container and as the return value if found.</param>
		
		private int checkParameter(string choice, int varValue)
		{
			foreach(var val in globalVariables)
			{
				if(val.Key == choice){ return int.TryParse(val.Value, out varValue) ? varValue : -1;}
			} 
			return -1;
		}
		
		/// <summary>
		/// Verifies the variables for floats.
		/// Has a series of checks to make sure the type matches up correctly, if the 
		/// function type is of float. 
		/// </summary>
		/// <returns><c>true</c>, if variables was verified, <c>false</c> otherwise.</returns>
		/// <param name="varName">Variable name is the string representation of what should be a float.</param>
		/// <param name="val">Value is what the expression evaluates to.</param>
		
		private bool VerifyVariables(string varName, out float val)
		{
			float temp = 0.0f;
			temp = checkParameter(varName, temp);
			bool flag = false;
			flag = temp != -1.0f ? true : false;
			flag = !flag ? float.TryParse(varName, out temp) : true; 
			val = temp;
			return flag;
		}
		
		/// <summary>
		/// Checks the parameter if it is a variable the user declared. 
		/// </summary>
		/// <returns><c>varValue</c>, if the variable was found, <c>-1.0f</c> otherwise.</returns>
		/// <param name="choice">Choice is the key to check against.</param>
		/// <param name="varValue">varValue is used both as a container and as the return value if found.</param>
		
		private float checkParameter(string choice, float varValue)
		{
			foreach(var val in globalVariables)
			{
				if(val.Key == choice){ return float.TryParse(val.Value, out varValue) ? varValue : -1.0f;}
			}
			return -1.0f;
		}
		
		/// <summary>
		/// Verifies the variables for strings.
		/// Has a series of checks to make sure the type matches up correctly, if the 
		/// function type is of string. 
		/// </summary>
		/// <returns><c>true</c>, if variables was verified, <c>false</c> otherwise.</returns>
		/// <param name="varName">Variable name is the string representation of what should be a string.</param>
		/// <param name="val">Value is what the expression evaluates to.</param>
		
		private bool VerifyVariables(string varName, out string val)
		{
			string temp = string.Empty;
			temp = checkParameter(varName, temp);
			if(temp == null) temp = varName;
			val = temp;
			return true;
		}
		
		/// <summary>
		/// Checks the parameter if it is a variable the user declared. 
		/// </summary>
		/// <returns><c>value at the key</c>, if the variable was found, <c>null</c> otherwise.</returns>
		/// <param name="choice">Choice is the key to check against.</param>
		/// <param name="varValue">varValue is used both as a container.</param>
		
		private string checkParameter(string choice, string varValue)
		{
			foreach(var val in globalVariables)
			{
				if(val.Key == choice){ return val.Value; }
			} 
			return null;
		}
		
		private void print(string s)
		{
			//Debug.Log(s);
		}
		
		/// <summary>
		/// Gets the variable value.
		/// </summary>
		/// <returns>The variable value.</returns>
		/// <param name="key">Key.</param>
		
		public void getGlobalVariableValue(ref string key)
		{
			foreach(var v in globalVariables)
			{
				if(v.Key == key){key = v.Value;}
			}

		}

		/// <summary>
		/// Gets the variable value.
		/// </summary>
		/// <returns>The variable value.</returns>
		/// <param name="key">Key.</param>
		
		public string getGlobalVariableValue(string key)
		{
			string temp = key;
			foreach(var v in globalVariables)
			{
				if(v.Key == key){ temp = v.Value;}
			}
			return temp;
		}

		public string getLocalVariableValue(string key) 
		{
			////Debug.Log("Local Variables are:");
			//localVariables.print();
			string temp = key;
			foreach (var v in localVariables) 
			{
				if (v.Key == key) { temp = v.Value; }
			}
			return temp;
		}
		
	}

	public struct FunctionVariables
	{
		private Dictionary<string, string> dict;
		public Dictionary<string, string> Dict{get{return dict;}}

		public FunctionVariables(Dictionary<string, string> tDict)
		{
			dict = new Dictionary<string, string>(tDict);
		}

	}

}