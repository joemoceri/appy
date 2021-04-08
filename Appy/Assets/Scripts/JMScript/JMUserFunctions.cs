using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
using JM.PrimitiveTypes;
using JM.Interpreter;

namespace JM.UserFunctions 
{

	public class JMUserFunctions
	{

		private JMInterpreter interpreter;
		public FunctionF curFunctionCall;
		public Dictionary<string, FunctionF> UserFunctions = new Dictionary<string, FunctionF>();

		public JMUserFunctions(){}

		public JMUserFunctions(JMInterpreter i) 
		{
			interpreter = i;
		}

		public void CheckForUserFunctions(string lineCommand, string lineBlockData) 
		{
			
			string pattern = @"(\() | (\)) | (,) | (\s) | (:)", paramExpression = string.Empty;
			paramExpression = GetParameterExpression(lineCommand);
			List<string> list = Regex.Split(lineCommand, pattern, RegexOptions.IgnorePatternWhitespace).toList();
			list.removeWhitespace();
			AddFunctionToDictionary(list, lineBlockData, paramExpression);

		}

		private void AddFunctionToDictionary(List<string> list, string lineBlockData, string paramExpression) 
		{
			string name = list[1];
			List<FunctionParameters> tParams = GetParameters(paramExpression);
			FunctionF func = new FunctionF(name, tParams, lineBlockData);
			UserFunctions.Add(name, func);
			//UserFunctions.print();
		}

		private List<FunctionParameters> GetParameters(string paramExpression) 
		{
			string pattern = @"(\()|(\))|(,)";
			List<string> list = Regex.Split(paramExpression, pattern).toList();
			list.removeExtraEntries();
			list.removeWhitespace();
			List<FunctionParameters> funcList = new List<FunctionParameters>();
			if (list.Count > 2) 
			{
				for (int i = 0; i < list.Count; i++) // Always start at the third index '('
				{
					if (i % 2 != 0)
					{
						funcList.Add(new FunctionParameters(list[i], null));
					}
				}
			}

			return funcList;
		}

		private string GetParameterExpression(string str)
		{
			bool startFound = false;
			int leftCount = 0, start = 0, amt = 0;
			for (int i = 0; i < str.Length; i++)
			{
				if (str[i] == '(')
				{
					leftCount++;
					if (leftCount == 1)
					{
						start = i;
						startFound = true;
					}
				}
				else if (str[i] == ')')
				{
					leftCount--;
					if (leftCount == 0)
					{
						amt++;
						break;
					}
				}
				if (startFound)
				{
					amt++;
				}
			}
			return str.Substring(start, amt);

		}

		public string getCurFunctionParamValue(string key)
		{
			string temp = key;
			try
			{
				for(int i = 0; i < curFunctionCall.funcParameters.Count; i++)
				{
					if (curFunctionCall.funcParameters[i].paramValue != null) 
					{
						if (curFunctionCall.funcParameters[i].paramName == key)
						{
							temp = curFunctionCall.funcParameters[i].paramValue;
						}
					}

					
				}
			}
			catch{}//{Debug.Log("Caught at curFunctionParamValue");}
			/*catch(Exception e)
			{
				Debug.Log(e);
			}*/
			
			

			return temp;
		}

		public bool checkIfUserDefinedFunction(string lineCommand)
		{
			string pattern = @"(\()";
			List<string> list = Regex.Split(lineCommand, pattern).toList();
			list[0] = list[0].Replace(" ", string.Empty);

			bool ans = UserFunctions.ContainsKey(list[0]);

			return ans;
		}

	}

	public class FunctionF
	{
		public string name;
		public List<FunctionParameters> funcParameters;
		public int paramCount;
		public string lineData;

		public FunctionF(string tName, List<FunctionParameters> tFuncParameters, string tLineData) 
		{
			name = tName;
			funcParameters = tFuncParameters;
			paramCount = funcParameters.Count;
			lineData = tLineData;
		}

		public void SetParameters(List<string> newParams)
		{
			
			ResetParameters();
			newParams.removeWhitespace();
			for(int i = 0; i < paramCount; i++)
			{
				funcParameters[i] = new FunctionParameters(funcParameters[i].paramName, newParams[i]);
			}

		}

		public void ResetParameters() 
		{
			for (int i = 0; i < funcParameters.Count; i++) 
			{
				funcParameters[i] = new FunctionParameters(funcParameters[i].paramName, null);
			}
		}

	}

	public struct FunctionParameters
	{
		public string paramName, paramValue;
		public FunctionParameters(string tParamName, string tParamValue)
		{
			paramName = tParamName;
			paramValue = tParamValue;
		}

	}
	
	
}