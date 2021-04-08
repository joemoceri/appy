using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using JM.Variables;
using System;
using JM.Interpreter;

namespace JM.Arithmetic
{
	public class JMArithmetic : JMInterpreter
	{

		private JMInterpreter interpreter;
		public enum VarType { Boolean, Int, Float, String, Variable};
		private enum Operator { Multiply, Divide, Modulus, Addition, Subtraction};

		public JMArithmetic() { }

		// Make sure it returns the boolean representation if it's just a single parameter
		public JMArithmetic(JMInterpreter i)
		{
			interpreter = i;
		}

		public string CheckExpression(string line)
		{
			string pattern = @"(\+)|(\-)|(\*)|(\/)|(\%)";

			////Debug.Log("Before ." + line + ".");

			List<string> expressions = new List<string>();
			List<string> operators = new List<string>();

			VarType lineType = updateType(line);
			if (lineType == VarType.String)
			{
				//pattern = @"";
				pattern = @"(\+)";
				removeWhitespaceOutsideQuotes(ref line);
			}
			else 
			{
				line = line.Replace(" ", string.Empty);
			}
			
			string tLine = line;
			if (checkFunctionParams(ref line)) { /*Debug.Log("Arithmetic First Check: " + tLine + ": Found inside function parameters: " + line);*/ }
			else if (checkLocalVariables(ref line)) { /*Debug.Log("Arithmetic First Check: " + tLine + ": Found inside local variables: " + line);*/ }
			else if (checkGlobalVariables(ref line)) { /*Debug.Log("Arithmetic First Check: " + tLine + ": Found inside global variables: " + line);*/ }
			else if (checkIterators(ref line)) { /*//Debug.Log("Arithmetic First Check: " + tLine + ": Found inside loop iterators: " + line);*/ }
			else if (checkFunctionCall(ref line)) { /*Debug.Log("Arithmetic First Check: " + tLine + ": Found inside check function call: " + line);*/ }

			int outerAmount = 0, first = 0, second = 0, pAmt = checkParenthesesCount(line, ref outerAmount);
			
			for (int i = 0; i < outerAmount; i++) 
            {
                getOuterMostParentheticalExpression(ref line, ref first, ref second);
            }
            
			string returnAns = null;

			List<string> input = Regex.Split(line, pattern, RegexOptions.IgnorePatternWhitespace).toList();
			input.removeExtraEntries();

			if(pAmt == -1)
			{
				returnAns = null;
			}
			else if(input.Count == 1 || checkNegative(input))
			{
				returnAns = line;
			}
			else
			{
				expressions = generateExpressions(input, ref operators);
				returnAns = evaluateBigExpression(expressions, operators, pattern);
			}

			return returnAns == null ? line : returnAns;
		}

		private void getOuterMostParentheticalExpression(ref string str, ref int f, ref int s)
		{
            int leftCount = 0;
            for (int i = f; i < str.Length; i++)
            {
                if (str[i] == '(')
                {
                    if (leftCount == 0) 
                    {
                        f = i + 1;
                    }
                    leftCount++;
                    
                }
                else if (str[i] == ')')
                {
                    leftCount--;
                    if (leftCount == 0)
                    {
                        s = i;
                        break;
                    }
                    
                }
            }
          
			string temp = "";
			for(int i = f; i < s; i++)
			{
				temp += str[i];
			}

			if (temp != "") 
			{
				try
				{
					str = str.Replace("(" + temp + ")", CheckExpression(temp));
				}
				catch (Exception e)
				{
					//print (e + ": error thrown");
				}
			}
			
            f = 0; s = 0;

		}

		private int checkParenthesesCount(string str, ref int outer)
		{
            int count = 0, leftCount = 0;
			for(int i = 0; i < str.Length; i++)
			{
                if (str[i] == '(') 
                {
                    leftCount++;
                    count++;
                }
                else if (str[i] == ')') 
                {
                    leftCount--;
                    if (leftCount == 0) 
                    {
                        outer++;
                    }
                    count++;
                }
			}
			return count;
		}
		
		private bool checkNegative(List<string> list)
		{
			bool ans = false;
			if(list.Count == 2)
			{
				if (list[0] == "-")
				{
					ans = true;
				}
				else
				{
					ans = false;
				}
			}
			else
			{
				ans = false;
			}
			return ans;
		}
		
		private List<string> generateExpressions(List<string> list, ref List<string> operators)
		{
			
			List<string> temp = new List<string>();
			
			string exp = string.Empty;
			bool creatingHigher = false;
			
			if(list[1] == "*" || list[1] == "/" || list[1] == "%")
			{
				creatingHigher = true;
			}
			else
			{
				creatingHigher = false;
			}
			
			// This can only take in at least 3 parameters
			for(int i = 0; i < list.Count; i++)
			{
				if(i % 2 == 0) // if we're at a value not operator ..
				{
					exp += list[i]; // Add in the first number regardless of the expression 'type'
					if(creatingHigher) // If we're working with *, /, or %
					{
						
						try // Used for catching the end of the 'big' expression
						{
							if(list[i+1] == "+" || list[i+1] == "-") // if the next operator is the opposite (+ or -)
							{
								creatingHigher = false; // Switch to the other expression after
								if(exp.Contains("*") || exp.Contains("/") || exp.Contains("%") || exp.Contains("+") || exp.Contains("-")) // only add it in ..
								{ // .. if the expression has an operator (two or more parameters)
									temp.Add(addExp(ref exp));
								}
								else // otherwise add in the operator and continue building the expression
								{
									exp += list[i+1];
								}
							}
							else // if the operator stays the same, continue building the expression
							{
								exp += list[i+1];
							}
						}
						catch // If we're at the end, catch it, and add in the last expression
						{
							////Debug.Log("End of big expression");
							temp.Add(addExp(ref exp));
						}
					}
					else // Else if we're working with + or -
					{
						try // used for catching the end of the 'big' expression
						{
							if(list[i+1] == "*" || list[i+1] == "/" || list[i+1] == "%") // if the next operator is the opposite (*, / or %)
							{
								creatingHigher = true; // Switch to the other expression after
								if(exp.Contains("*") || exp.Contains("/") || exp.Contains("%") || exp.Contains("+") || exp.Contains("-")) // only add it in ..
								{ // .. if the expression has an operator (two or more parameters)
									temp.Add(addExp(ref exp));
								}
								else // otherwise add in the operator and continue building the expression
								{
									exp += list[i+1];
								}
							}
							else // If the operator stays the same .. 
							{
								if(i != 0) // If we're not at the starting index .. 
								{
									if( (list[i-1] == "+" || list[i-1] == "-") && (list[i+1] == "+" || list[i+1] == "-")) // Check the previous and next operator .. 
									{ // If both are lower precedence expressions, then we know it's a singular expression, and add it as a separate expression
										temp.Add(addExp(ref exp));
									}
									else // otherwise add the operator to the expression
									{
										exp += list[i+1];
									}
								}
								else if(i == 0) // If we are at the starting index .. 
								{
									if( (list[i+1] == "+" || list[i+1] == "-") && (list[i+3] == "*" || list[i+3] == "/" || list[i+3] == "%") ) // Check the next two operators .. 
									{ // If the first is lower, and the next is higher, then we know it's a singular expression, and add it as a separate expression
										temp.Add(addExp(ref exp));
									}
									else // Otherwise add the operator to the expression
									{
										exp += list[i+1];
									}
								}
							}
						}
						catch // If we're at the end, catch it, and add in the last expression
						{
							////Debug.Log("End of big expression");
							temp.Add(addExp(ref exp));
						}
					}
				}
				else // If we're at an operator for the 'big' expression ..
				{
					if(exp == string.Empty) // Confirm that it's a 'big' operator ..
					{ // and add it in
						operators.Add(list[i]);
					}
				}
			}
			
			return temp;
			
		}
		
		private string addExp(ref string exp)
		{
			string t = exp;
			exp = string.Empty;
			return t;
		}
		
		private string evaluateBigExpression(List<string> exp, List<string> operators, string pattern)
		{
			
			bool first = false;
			string sum = string.Empty;
			Operator op = Operator.Addition;
			
			for(int i = 0; i < exp.Count; i++)
			{
				exp[i] = evaluateSubExpression(Regex.Split(exp[i], pattern, RegexOptions.IgnorePatternWhitespace).toList());
			}
			
			if(exp.Count == 1)
			{
				sum = exp[0];
			}
			else
			{
				for(int i = 0; i < operators.Count; i++)
				{
					if(operators[i] == "*")
					{
						op = Operator.Multiply;
					}
					else if(operators[i] == "/")
					{ 
						op = Operator.Divide;
					}
					else if(operators[i] == "%")
					{
						op = Operator.Modulus;
					}
					else if(operators[i] == "+")
					{
						op = Operator.Addition;
					}
					else if(operators[i] == "-")
					{ 
						op = Operator.Subtraction;
					}
					
					sum = performOperation(updateFirstVar(ref first, exp[i], sum), exp[i+1], op);
					
					if(sum == null)
					{
						break;
					}
				}
			}
			
			
			return sum;
		}
		
		private string evaluateSubExpression(List<string> list)
		{
			list.removeExtraEntries();
			bool first = false;
			string sum = string.Empty;
			Operator op = Operator.Multiply;
			
			if(list.Count == 1)
			{
				sum = list[0];
			}
			else
			{
				for(int i = 0; i < list.Count; i++)
				{
					if(i % 2 != 0)
					{
						if(list[i] == "*")
						{
							op = Operator.Multiply;
						}
						else if(list[i] == "/")
						{ 
							op = Operator.Divide;
						}
						else if(list[i] == "%")
						{
							op = Operator.Modulus;
						}
						else if(list[i] == "+")
						{
							op = Operator.Addition;
						}
						else if(list[i] == "-")
						{ 
							op = Operator.Subtraction;
						}
						
						sum = performOperation(updateFirstVar(ref first, list[i-1], sum), list[i+1], op);
						
						if(sum == null)
						{
							break;
						}
					}
				}
			}

			return sum;
		}
		
		private string updateFirstVar(ref bool first, string fVar, string sum)
		{
			string temp = string.Empty;
			temp = !first ? fVar : sum;
			if(temp == fVar) first = true;
			return temp;
		}
		
		private string performOperation(string var1, string var2, Operator op)
		{
			float num1 = 0.0f, num2 = 0.0f, s = -1.0f;
			VarType type1, type2;

			string tVar1 = var1, tVar2 = var2, ans = null;
			if (checkFunctionParams(ref var1)) { /*Debug.Log("Arithmetic Second Check: " + tVar1 + ": Found inside function parameters: " + var1);*/ }
			else if (checkLocalVariables(ref var1)) { /*Debug.Log("Arithmetic Second Check: " + tVar1 + ": Found inside local variables: " + var1);*/ }
			else if (checkGlobalVariables(ref var1)) { /*Debug.Log("Arithmetic Second Check: " + tVar1 + ": Found inside global variables: " + var1);*/ }
			else if (checkIterators(ref var1)) { /*Debug.Log("Arithmetic Second Check: " + tVar1 + ": Found inside loop iterators: " + var1);*/ }

			if (checkFunctionParams(ref var2)) { /*Debug.Log("Arithmetic Second Check: " + tVar2 + ": Found inside function parameters: " + var2);*/ }
			else if (checkLocalVariables(ref var2)) { /*Debug.Log("Arithmetic Second Check: " + tVar2 + ": Found inside local variables: " + var2);*/ }
			else if (checkGlobalVariables(ref var2)) { /*Debug.Log("Arithmetic Second Check: " + tVar2 + ": Found inside global variables: " + var2);*/ }
			else if (checkIterators(ref var2)) { /*Debug.Log("Arithmetic Second Check: " + tVar2 + ": Found inside loop iterators: " + var2);*/ }

			try
			{
				type1 = updateType(var1);
				type2 = updateType(var2);
			}
			catch
			{
				//Debug.Log("Update Type failed.");
				return null;
			}

			VarType expType = VarType.Int;

			checkExpType(ref expType, type1, type2);
			setParameter(ref num1, var1, type1);
			setParameter(ref num2, var2, type2);
			setParameter(ref var1, string.Empty, type1);
			setParameter(ref var2, string.Empty, type2);
			
			//Debug.Log("Performing operation: " + var1 + " " + op.ToString() + " " + var2 + " as " + expType.ToString());
			
			MathExpression exp = new MathExpression(num1, num2, var1, var2, type1, type2, expType);
			
			if(op == Operator.Multiply)
			{
				multiply(exp, ref s);
			}
			else if(op == Operator.Divide && (type1 != VarType.String || type2 != VarType.String) )
			{
				divide(exp, ref s);
			}
			else if(op == Operator.Modulus)
			{
				modulus(exp, ref s);
			}
			else if(op == Operator.Addition)
			{
				string temp = addition(exp, ref s);
				if(temp != null)
				{
					ans = temp;
				}
			}
			else if(op == Operator.Subtraction && (type1 != VarType.String || type2 != VarType.String) )
			{
				subtraction(exp, ref s);
			}

			if (ans == null)
			{
				ans = s.ToString();
			}
			else 
			{
				ans = "\"" + ans + "\"";
			}
			
			//Debug.Log("Arithmetic answer is: " + ans);

			return ans;
		}
		
		private void multiply(MathExpression exp, ref float s)
		{
			if(exp.expType == VarType.Int)
			{
				s = (int) (exp.par1 * exp.par2);
			}
			else
			{
				s = exp.par1 * exp.par2;
			}
		}
		
		private void divide(MathExpression exp, ref float s)
		{
			if (exp.expType == VarType.Int)
			{
				s = (int) (exp.par1 / exp.par2);
			}
			else
			{
				s = exp.par1 / exp.par2;
			}
		}
		
		private void modulus(MathExpression exp, ref float s)
		{
			s = exp.par1 % exp.par2;
		}
		
		private string addition(MathExpression exp, ref float s)
		{
			if(exp.type1 == VarType.String || exp.type2 == VarType.String)
			{
				return exp.str1 + exp.str2;
			}
			else
			{
				if (exp.expType == VarType.Int)
				{
					s = (int) (exp.par1 + exp.par2);
				}
				else
				{
					s = exp.par1 + exp.par2;
				}
			}
			return null;
		}
		
		private void subtraction(MathExpression exp, ref float s)
		{
			if (exp.expType == VarType.Int)
			{
				s = (int) (exp.par1 - exp.par2);
			}
			else
			{
				s = exp.par1 - exp.par2;
			}
		}
		
		private void setParameter(ref float num, string val, VarType type)
		{
			if(type == VarType.Int)
			{
				num = (int) int.Parse(val);
			}
			else if(type == VarType.Float)
			{
				num = float.Parse(val);
			}
		}
		
		private void setParameter(ref string var, string val, VarType type)
		{
			if(type == VarType.String)
			{
				var = var.Replace("\"", val);
			}
		}

		private bool checkLocalVariables(ref string variable) 
		{
			bool found = false;
			string str = interpreter.jmVariables.getLocalVariableValue(variable);
			if (str != null && str != variable) 
			{
				variable = str;
				found = true;
			}
			return found;
		}

		private bool checkGlobalVariables(ref string variable)
		{
			bool found = false;
			string str = interpreter.jmVariables.getGlobalVariableValue(variable);

			if (str != null && str != variable)
			{
				variable = str;
				found = true;
			}
			return found;
		}

		private bool checkIterators(ref string variable) 
		{
			bool found = false;
			string str = interpreter.jmLoops.getIteratorValue(variable);
			if (str != null && str != variable)
			{
				variable = str;
				found = true;
			}

			return found;
		}

		private bool checkFunctionParams(ref string variable)
		{
			bool found = false;
			string str = interpreter.jmUserFunctions.getCurFunctionParamValue(variable);
			if(str != null && str != variable) 
			{
				variable = str;
				found = true;
			}
			return found;
		}

		// #CHECKFUNCTIONCALL
		private bool checkFunctionCall(ref string variable) 
		{
			bool found = false;
			int indentBefore = interpreter.jmQueue.CurIndent;
			string str = interpreter.jmFunctionCalls.CheckFunctionCall(variable);
			////Debug.Log("yup");
			if (str != null && str != "void") 
			{
				variable = str;
				found = true;
			}
			interpreter.jmQueue.CurIndent = indentBefore;
			////Debug.Log("yup2");
			
			return found;
		}
		private void checkExpType(ref VarType expType, VarType type1, VarType type2)
		{
			if(expType != VarType.String)
			{
				if(type1 == VarType.String || type2 == VarType.String)
				{
					expType = VarType.String;
				}
				else if(type1 == VarType.Float || type2 == VarType.Float)
				{
					expType = VarType.Float;
				}
			}
		}
		
		private string multiplyString(string var1, string var2, int choice)
		{
			int amount = 0;
			string str = string.Empty;
			if(choice == 1)
			{
				amount = int.Parse(var2);
				for(int i = 0; i < amount; i++)
				{
					str += var1;
				}
			}
			else
			{
				amount = int.Parse(var1);
				for(int i = 0; i < amount; i++)
				{
					str += var2;
				}
			}
			
			str = "\"" + str + "\"";
			
			return str;
		}
		
		public VarType updateType(string var)
		{
			int iNum = 0; float fNum = 0.0f;
			VarType temp = VarType.Int;

			if(int.TryParse(var, out iNum))
			{
				temp = VarType.Int; 
			}
			else if(float.TryParse(var, out fNum))
			{
				temp = VarType.Float; 
			}
			else if(var.Contains("\"") || var.Contains("'"))
			{ 
				temp = VarType.String;
			}
			else
			{
				temp = VarType.Variable;
			}
			return temp;
		}
		
		private struct MathExpression 
		{
			
			public float par1, par2;
			public string str1, str2;
			public VarType type1, type2;
			public VarType expType;

			public MathExpression(float p1, float p2, string s1, string s2, VarType t1, VarType t2, VarType tempExpType)
			{
				par1 = p1;
				par2 = p2;
				str1 = s1;
				str2 = s2;
				type1 = t1;
				type2 = t2;
				expType = tempExpType;
			}
			
		}

		private void removeWhitespaceOutsideQuotes(ref string line)
		{
			StringBuilder sb = new StringBuilder(line);
			bool inQuote = false;
			int amountRemoved = 0;
			for (int i = 0; i < line.Length; i++)
			{
				if (line[i] == '\"')
				{
					inQuote = !inQuote;
				}
				if (!inQuote)
				{
					if (line[i] == ' ')
					{
						sb.Remove(i - amountRemoved, 1);
						amountRemoved++;
					}
				}
			}
			line = sb.ToString();

		}

		private void print(string str)
		{
			//Debug.Log(str);
		}

	}

}