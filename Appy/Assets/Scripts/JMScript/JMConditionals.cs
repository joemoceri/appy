using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using JM.Arithmetic;
using JM.PrimitiveTypes;
using System;
using System.Text;
using JM.Interpreter;

namespace JM.Conditionals
{
	
	public class JMConditionals : JMArithmetic
	{
		private JMInterpreter interpreter;

		public JMConditionals() { }

		public JMConditionals(JMInterpreter i) 
		{
			interpreter = i;
		}

		public bool CheckConditional(string exp) 
		{

			// Get our outer most boolean parenthetical expressions, and recursively keep calling until we reach the bottom
			checkBooleanParentheses(ref exp);

			string outsidePattern = @"(and)|(or)";
			List<string> subExpressions = new List<string>(), conditionalOperators = new List<string>();

			subExpressions = Regex.Split(exp, outsidePattern).toList();
			subExpressions.removeWhitespace();
			
			// Remove the parentheses if there are any
			removeOutsideParentheses(subExpressions);

			SeperateExpressionsAndOperators(subExpressions, conditionalOperators);

			EvaluateSubExpressions(subExpressions);

			bool temp = EvaluateEntireExpression(subExpressions, conditionalOperators);

			////Debug.Log ("Conditional is: " + temp);

			return temp;
		}

		private bool checkParenthesesCount(string str) 
		{
			int count = 0;
			for (int i = 0; i < str.Length; i++) 
			{
				if (str[i] == '(' || str[i] == ')') 
				{
					count++;
				}
			}
			return count % 2 == 0;
		}

		private void removeOutsideParentheses(List<string> list) 
		{
			for (int i = 0; i < list.Count; i++) 
			{
				int len = list[i].Length - 1;
				if(list[i][len] == ')' && list[i][0] == '(')
				{
					list[i] = list[i].Remove(len);
					list[i] = list[i].Remove(0, 1);
				}
				
			}
		}

		private void checkBooleanParentheses(ref string str) 
		{
			bool foundExpression = false;
			string pattern = @"(\()|(\))|(and)|(or)";
			List<string> list = Regex.Split(str, pattern).toList();
			list.removeExtraEntries();
			
			int leftCount = 0, first = 0;
			for (int i = 0; i < list.Count; i++) 
			{
				if (list[i] == "(") 
				{
					leftCount++;
					if (leftCount == 1) 
					{
						first = i;
					}
				}
				else if (list[i] == ")") 
				{
					leftCount--;
					if (leftCount == 0 && foundExpression) 
					{
						StringBuilder newExp = new StringBuilder();
						for (int j = first + 1; j < i; j++) 
						{
							newExp.Append(list[j]);
						}
						string newBoolean = CheckConditional(newExp.ToString()).ToString(), oldBoolean = "(" + newExp + ")";
						oldBoolean = oldBoolean.Replace(" ", "");
						newBoolean = newBoolean.Replace(" ", "");
						str = str.Replace(oldBoolean, newBoolean);
						
						newExp = null;
						foundExpression = false;
					}
				}
				else if (leftCount > 0) 
				{
					if (list[i] == "and" || list[i] == "or") 
					{
						foundExpression = true;
					}
				}
			}
		}

		// This should set our string lists for conditional operators and sub expressions
		private void SeperateExpressionsAndOperators(List<string> exp, List<string> conditionalOp) 
		{
			List<int> toRemove = new List<int>();
			for (int i = 0; i < exp.Count; i++) 
			{
				if (i % 2 != 0)
				{
					conditionalOp.Add(exp[i]);
					toRemove.Add(i);
				}
			}

			int tracker = 0;
			for(int i = 0; i < toRemove.Count; i++)
			{
				exp.RemoveAt(toRemove[i] - tracker);
				tracker++;
			}

		}

		private void EvaluateSubExpressions(List<string> exp)
		{
			string subPattern = @"(>=)|(<=)|(==)|(!=)|(>)|(<)";
			for(int i = 0; i < exp.Count; i++)
			{
				List<string> subExp = Regex.Split(exp[i], subPattern).toList();
				subExp.removeExtraEntries();
				subExp.removeWhitespace();
				exp[i] = computeBooleanExpression(subExp);
			}
		}

		private bool EvaluateEntireExpression(List<string> exp, List<string> operators)
		{
			bool ans = false;

			if(operators.Count > 0)
			{
				for(int i = 0; i < operators.Count; i++)
				{
					if(operators[i] == "and")
					{
						ans = performConditionalAnd(i == 0 ? bool.Parse(exp[i]) : ans, bool.Parse(exp[i+1]));
					}
					else if(operators[i] == "or")
					{
						ans = performConditionalOr(i == 0 ? bool.Parse(exp[i]) : ans, bool.Parse(exp[i+1]));
					}
				}
			}
			else
			{
				ans = bool.Parse(exp[0]);
			}

			return ans;
		}

		private string computeBooleanExpression(List<string> input) 
		{

			bool ans = false;

			if (input.Count == 1)
			{
				ans = bool.Parse(interpreter.jmArithmetic.CheckExpression(input[0]));
			}
			else
			{
				VarType type1 = VarType.Int;
				VarType type2 = VarType.Int;
				try
				{
					type1 = interpreter.jmArithmetic.updateType(input[0]);
					type2 = interpreter.jmArithmetic.updateType(input[2]);
				}
				catch (Exception e)
				{
					////Debug.Log(e + ": Update Type failed.");
				}
				VarType expType = SetExpType(type1, type2);

				BooleanExpression bExp = new BooleanExpression(input[0], input[2], type1, type2, expType);
				bExp.str1 = interpreter.jmArithmetic.CheckExpression(bExp.str1);
				bExp.str2 = interpreter.jmArithmetic.CheckExpression(bExp.str2);

				//Debug.Log("Performing " + bExp.str1 + " " + input[1] + " " + bExp.str2 + " as " + expType.ToString()); 

				string operation = input[1];

				if (operation == ">")
				{
					ans = performGreaterThan(bExp);
				}
				else if (operation == ">=")
				{
					ans = performGreaterThanOrEqualTo(bExp);
				}
				else if (operation == "<")
				{
					ans = performLessThan(bExp);
				}
				else if (operation == "<=")
				{
					ans = performLessThanOrEqualTo(bExp);
				}
				else if (operation == "==")
				{
					ans = performEqualTo(bExp);
				}
				else if (operation == "!=")
				{
					ans = performNotEqualTo(bExp);
				}
			}

			return ans.ToString();

		}

		private VarType SetExpType(VarType type1, VarType type2)
		{
			VarType type = VarType.Int;
			if(type1 == VarType.Float || type2 == VarType.Float)
			{
				type = VarType.Float;
			}
			else if(type1 == VarType.Boolean && type2 == VarType.Boolean)
			{
				type = VarType.Boolean;
			} 
			return type;
		}

		private bool performGreaterThan(BooleanExpression bExp)
		{
			bool ans = false;

			if(bExp.expType == VarType.Boolean)
			{
				ans = false;
			}
			else if(bExp.expType == VarType.Int)
			{
				ans = int.Parse(bExp.str1) > int.Parse(bExp.str2);
			}
			else if(bExp.expType == VarType.Float)
			{
				ans = float.Parse(bExp.str1) > float.Parse(bExp.str2);
			}
			else if(bExp.expType == VarType.String)
			{
				ans = false;
			}

			return ans;
		}

		private bool performGreaterThanOrEqualTo(BooleanExpression bExp)
		{
			bool ans = false;
			
			if(bExp.expType == VarType.Boolean)
			{
				ans = false;
			}
			else if(bExp.expType == VarType.Int)
			{
				ans = int.Parse(bExp.str1) >= int.Parse(bExp.str2);
			}
			else if(bExp.expType == VarType.Float)
			{
				ans = float.Parse(bExp.str1) >= float.Parse(bExp.str2);
			}
			else if(bExp.expType == VarType.String)
			{
				ans = false;
			}
			
			return ans;
		}

		private bool performLessThan(BooleanExpression bExp)
		{
			bool ans = false;

			if(bExp.expType == VarType.Boolean)
			{
				ans = false;
			}
			else if(bExp.expType == VarType.Int)
			{
				ans = int.Parse(bExp.str1) < int.Parse(bExp.str2);
			}
			else if(bExp.expType == VarType.Float)
			{
				ans = float.Parse(bExp.str1) < float.Parse(bExp.str2);
			}
			else if(bExp.expType == VarType.String)
			{
				ans = false;
			}

			return ans;
		}

		private bool performLessThanOrEqualTo(BooleanExpression bExp)
		{
			bool ans = false;
			
			if(bExp.expType == VarType.Boolean)
			{
				ans = false;
			}
			else if(bExp.expType == VarType.Int)
			{
				ans = int.Parse(bExp.str1) <= int.Parse(bExp.str2);
			}
			else if(bExp.expType == VarType.Float)
			{
				ans = float.Parse(bExp.str1) <= float.Parse(bExp.str2);
			}
			else if(bExp.expType == VarType.String)
			{
				ans = false;
			}
			
			return ans;
		}

		private bool performEqualTo(BooleanExpression bExp)
		{
			bool ans = false;
			
			if(bExp.expType == VarType.Boolean)
			{
				ans = false;
			}
			else if(bExp.expType == VarType.Int)
			{
				ans = int.Parse(bExp.str1) == int.Parse(bExp.str2);
			}
			else if(bExp.expType == VarType.Float)
			{
				ans = float.Parse(bExp.str1) == float.Parse(bExp.str2);
			}
			else if(bExp.expType == VarType.String)
			{
				ans = false;
			}
			
			return ans;
		}

		private bool performNotEqualTo(BooleanExpression bExp)
		{
			bool ans = false;
			
			if(bExp.expType == VarType.Boolean)
			{
				ans = false;
			}
			else if(bExp.expType == VarType.Int)
			{
				ans = int.Parse(bExp.str1) != int.Parse(bExp.str2);
			}
			else if(bExp.expType == VarType.Float)
			{
				ans = float.Parse(bExp.str1) != float.Parse(bExp.str2);
			}
			else if(bExp.expType == VarType.String)
			{
				ans = false;
			}
			
			return ans;
		}

		private bool performConditionalAnd(bool exp1, bool exp2)
		{
			return exp1 && exp2;
		}

		private bool performConditionalOr(bool exp1, bool exp2)
		{
			return exp1 || exp2;
		}

		private struct BooleanExpression
		{
			public string str1, str2;
			public VarType type1, type2, expType;

			public BooleanExpression(string s1, string s2, VarType t1, VarType t2, VarType eType)
			{
				str1 = s1;
				str2 = s2;
				type1 = t1;
				type2 = t2;
				expType = eType;
			}

		}

	}

}