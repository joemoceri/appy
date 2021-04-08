using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using JM.PrimitiveTypes;
using JM.Variables;
using JM.FunctionType;
using JM.Math;
using JM.UserFunctions;
using JM.Queue;
using JM.Interpreter;

namespace JM.Functions
{

	using jmType = JM.PrimitiveTypes.JMType.IJMType;

	public enum Function
	{
		Print, ReadFile, Sin, Cos, Tan, Factorial, Pow, Abs,
		Clamp, Exp, Sqrt, Ceil, Floor, Round, Range, User
	};

	/// <summary>
	/// JM functions is what handles all of our function calls. It stores
	/// the enumeration for the functions and all calls go through here.
	/// 
	/// The S3 commands are called from here, based on what function the user
	/// is executing. SandFleaCommands and JMCommands both link from here.
	/// </summary>

	public class JMFunctionCalls
	{
		public string userReturnValue = "null";
		private JMInterpreter interpreter;
		public Dictionary<string, FunctionData> functionNames = new Dictionary<string, FunctionData>();
		public JMFunctionCalls(JMInterpreter i)
		{
			interpreter = i;
			functionNames.Add("print", new FunctionData(Function.Print, 1));
			functionNames.Add("readFile", new FunctionData(Function.ReadFile,1));
			functionNames.Add("sin", new FunctionData(Function.Sin, 1));
			functionNames.Add("cos", new FunctionData(Function.Cos, 1));
			functionNames.Add("tan", new FunctionData(Function.Tan, 1));
			functionNames.Add("fact", new FunctionData(Function.Factorial, 1));
			functionNames.Add("pow", new FunctionData(Function.Pow, 2));
			functionNames.Add("abs", new FunctionData(Function.Abs, 1));
			functionNames.Add("clamp", new FunctionData(Function.Clamp, 3));
			functionNames.Add("exp", new FunctionData(Function.Exp, 1));
			functionNames.Add("sqrt", new FunctionData(Function.Sqrt, 1));
			functionNames.Add("ceil", new FunctionData(Function.Ceil, 1));
			functionNames.Add("floor", new FunctionData(Function.Floor, 1));
			functionNames.Add("round", new FunctionData(Function.Round, 1));
			functionNames.Add("range", new FunctionData(Function.Range, 3));
		}

		public bool CheckFunctionNames(string firstWord) 
		{
			bool found = false;

			if (functionNames.ContainsKey(firstWord)) 
			{
				found = true;
			}
			else if (interpreter.jmUserFunctions.UserFunctions.ContainsKey(firstWord)) 
			{
				found = true;
			}

			return found;
		}

		/// <summary>
		/// Starting point for checking our functions. Line is our broken up command input.
		/// </summary>
		/// <returns>If the function was found, it continues to execute.</returns>
		/// <param name="line">Line was the user input previously broken up, called from JMQueue.</param>
		
		public string CheckFunctionCall(string lineCommand)
		{

			bool error = true;
			string pattern = @"(\()|(\))|(,)|(=)", returnValue = "void";
			List<string> line = Regex.Split(lineCommand, pattern, RegexOptions.IgnorePatternWhitespace).toList();
			List<string> varExp = new List<string>();

			////Debug.Log("lineCommand: " + lineCommand);
			////Debug.Log("line[0]: " + line[0]);

			FunctionF func;
			if (interpreter.jmUserFunctions.UserFunctions.TryGetValue(line[0], out func))
			{

				int paramAmount = line.Count < 4 ? 0 : line.Count / 2 - 1;
				error = false;
				if (CheckLine(line, varExp, paramAmount)) 
				{
					//Debug.Log("Adding func lineData: " + func.lineData);
					func.SetParameters(varExp);
					interpreter.jmQueue.AddFunctionLineData(func.lineData);
					interpreter.jmUserFunctions.curFunctionCall = func;

					////Debug.Log("INSIDE FUNCTION CALL: " + interpreter.jmQueue.Queue[0]);
					interpreter.jmQueue.RemoveFromQueueWithoutParsing();
					////Debug.Log("AFTER INSIDE FUNCTION CALL: " + interpreter.jmQueue.Queue[0]);

					////Debug.Log("queue is:");
					//interpreter.jmQueue.Queue.print();
					////Debug.Log("end queue.");

					//interpreter.jmQueue.ParseQueue();
					interpreter.jmQueue.ParseQueue(QueueRemoveState.Skip);
					
					returnValue = userReturnValue;
					userReturnValue = "null";
					//Debug.Log("return value is: " + returnValue);
				}

			}
			else if (functionNames.ContainsKey(line[0])) 
			{
				if (CheckLine(line, varExp, functionNames[line[0]].ParamAmount))
				{
					returnValue = ExecuteFunction(varExp, functionNames[line[0]].Func);
				}
			}

			
			
			if(error)
			{
				////Debug.Log("Error! Unknown expression '" + line[0] + "'.");
			}

			return returnValue;
		}

		private bool CheckLine(List<string> line, List<string> varExp, int amount)
		{
			int max = (amount * 2) + 1;
			bool correct = true;
			for(int i = 1; i <= max; i++)
			{
				if(i == 1)
				{
					if(line[i] != "(")
					{
						//Debug.Log (line[i] + "<-Error! Must start function call with parentheses! '('");
						correct = false;
						break;
					}
				}
				else if(i == max)
				{
					if(line[i] != ")")
					{
						//Debug.Log (line[i] + "<-Error! You forgot to put an ending parentheses! ')'");
						correct = false;
						break;
					}
				}
				else if(i % 2 == 0)
				{
					string varAns = interpreter.jmArithmetic.CheckExpression(line[i]);
					varExp.Add(varAns);
				}
				else
				{
					if(line[i] != ",")
					{
						//Debug.Log (line[i] + "<-Error! Parameters must be comma separated! ','");
						correct = false;
						break;
					}
				}
			}

			return correct;
		}
		
		/// <summary>
		/// Executes the specified function using the broken up expression. Only gets to this
		/// point if validated with no parenthetical errors.
		/// </summary>
		/// <param name="exp">Exp is our user input broken up in a list.</param>
		/// <param name="choice">Choice is the function we're executing.</param>
		
		private string ExecuteFunction(List<string> exp, Function choice)
		{
			////Debug.Log("hereads;khas;dtha;sldkyh");
			string ans = string.Empty;

			List<jmType> par = new List<jmType>();
			bool verified = false;
			if (choice == Function.Print) 
			{
				par = interpreter.jmVariables.VerifyParameters(exp, JMFunctionType.Print, out verified, par);
				
				if (verified) { interpreter.jmStandard.print(par[0].toString()); }
				ans = "void";
			}
			
			else if(choice == Function.ReadFile)
			{
				verified = true;
				interpreter.jmQueue.Multi(interpreter.jmStandard.readFile(exp[0]) );
				ans = "void";
			}
			else if (choice == Function.Sin)
			{
				par = interpreter.jmVariables.VerifyParameters(exp, JMFunctionType.Sin, out verified, par);
				if(verified) { ans = JMMath.Sin(par[0].toFloat()).ToString();}
			}
			else if(choice == Function.Cos)
			{
				par = interpreter.jmVariables.VerifyParameters(exp, JMFunctionType.Cos, out verified, par);
				if(verified) {ans = JMMath.Cos(par[0].toFloat()).ToString();}
			}
			else if(choice == Function.Tan)
			{
				par = interpreter.jmVariables.VerifyParameters(exp, JMFunctionType.Tan, out verified, par);
				if (verified) { ans = JMMath.Tan(par[0].toFloat()).ToString(); }
			}
			else if(choice == Function.Factorial)
			{
				par = interpreter.jmVariables.VerifyParameters(exp, JMFunctionType.Factorial, out verified, par);
				if (verified) { ans = JMMath.Factorial(par[0].toInt()).ToString(); }
			}
			else if(choice == Function.Pow)
			{
				par = interpreter.jmVariables.VerifyParameters(exp, JMFunctionType.Pow, out verified, par);
				if (verified) { ans = JMMath.Pow(par[0].toFloat(), par[1].toInt()).ToString(); }
			}
			else if (choice == Function.Abs) 
			{
				par = interpreter.jmVariables.VerifyParameters(exp, JMFunctionType.Abs, out verified, par);
				if (verified) { ans = JMMath.Abs(par[0].toFloat()).ToString(); }
			}
			else if (choice == Function.Clamp) 
			{
				par = interpreter.jmVariables.VerifyParameters(exp, JMFunctionType.Clamp, out verified, par);
				if (verified) { ans = JMMath.Clamp(par[0].toFloat(), par[1].toFloat(), par[2].toFloat()).ToString(); }
			}
			else if (choice == Function.Exp) 
			{
				par = interpreter.jmVariables.VerifyParameters(exp, JMFunctionType.Exp, out verified, par);
				if (verified) { ans = JMMath.Exp(par[0].toFloat()).ToString(); }
			}
			else if (choice == Function.Sqrt) 
			{
				par = interpreter.jmVariables.VerifyParameters(exp, JMFunctionType.Sqrt, out verified, par);
				if (verified) { ans = JMMath.Sqrt(par[0].toFloat()).ToString(); }
			}
			else if (choice == Function.Ceil) 
			{
				par = interpreter.jmVariables.VerifyParameters(exp, JMFunctionType.Ceil, out verified, par);
				if (verified) { ans = JMMath.Ceil(par[0].toFloat()).ToString(); }
			}
			else if (choice == Function.Floor) 
			{
				par = interpreter.jmVariables.VerifyParameters(exp, JMFunctionType.Floor, out verified, par);
				if (verified) { ans = JMMath.Floor(par[0].toFloat()).ToString(); }
			}
			else if (choice == Function.Round) 
			{
				par = interpreter.jmVariables.VerifyParameters(exp, JMFunctionType.Round, out verified, par);
				if (verified) { ans = JMMath.Round(par[0].toFloat()).ToString(); }
			}
			else if(choice == Function.Range)
			{
				par = interpreter.jmVariables.VerifyParameters(exp, JMFunctionType.Range, out verified, par);
				if (verified) { ans = JMMath.Range(par[0].toInt(), par[1].toInt(), par[2].toInt()).ToString();}
			}

			return ans;
		}
		
		private void print(string s)
		{
			//Debug.Log(s);
		}

		public struct FunctionData 
		{
			private Function func;
			public Function Func { get { return func; } }
			private int paramAmount;
			public int ParamAmount { get { return paramAmount; } }

			public FunctionData(Function f, int pAmount) 
			{
				func = f;
				paramAmount = pAmount;
			}

		}

	}
}