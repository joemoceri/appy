using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
using System.Text;
using JM.Conditionals;
using JM.Variables;
using JM.Interpreter;

namespace JM.Queue
{

	public enum QueueRemoveState { ParseAgain, WithoutParsing, Skip, SkipWithoutParsing };
	public enum Conditional { If, Elif, Else };
	public enum LoopType { For, While };
	/// <summary>
	/// JMQueue keeps track of which commands are currently being checked.
	/// It uses a list internally to represent the queue. Helper functions
	/// include removing, adding, reseting. 
	/// 
	/// Also handles parsing single and multiple commands. 
	/// </summary>
	
	public class JMQueue
	{
		private enum CommentState { None, Inside, Outside };
		private int curIndent = 0, curLoopAmount = 0, curFunctionAmount = 0;
		public int CurFunctionAmount { get { return curFunctionAmount; } set { curFunctionAmount = value; } }
		public int CurIndent { get { return curIndent; } set { curIndent = value; } }
		public int CurLoopAmount { get { return curLoopAmount; } set { curLoopAmount = value; } }
		private JMInterpreter interpreter;
		private List<string> queue = new List<string>();
		public List<string> Queue{get{return queue;}}
		private bool running = false, ifFound = false, curConditional = false;
		public bool Running{get{return running;}}
		public delegate void OnChange();
		public OnChange onChange;

		public JMQueue(JMInterpreter i)
		{
			interpreter = i;
		}

		/// <summary>
		/// Parses the queue, determining whether its a variable declaration or a function call.
		/// Makes call to JMVariables(CheckForVariables) or JMFunctions(CheckForFunctions)
		/// depending on circumstances.
		/// </summary>
		/// <param name="index">Index is which command in the queue to parse at, usually the first(0)</param>

		public void ParseQueue(string lineCommand, QueueRemoveState parseAgain)
		{
			
			//Debug.Log("Parsing ." + lineCommand + ".");
			////Debug.Log("cur loop amount: " + CurLoopAmount);
			if(false)//(lineCommand.Contains("main") && !lineCommand.Contains("def")) 
			{
				//Debug.Log("Queue is:");
				//queue.print();
				//Debug.Log("end queue.");
			}
			
			CheckForComments(ref lineCommand);

			bool ignore = false;
			CheckForEndOfUserFunction(lineCommand, ref ignore, ref parseAgain);
			CheckForBlankLines(lineCommand, ref ignore);
			CheckForLoopFillerIteration(lineCommand, ref ignore);
			if (!ignore){ UpdateIndents(ref lineCommand, ref ignore);}

			if (!ignore)
			{
				string pattern = @"(\()|(\s)|(:)";
				List<string> inputExp = Regex.Split(lineCommand, pattern, RegexOptions.IgnorePatternWhitespace).toList();
				inputExp.removeExtraEntries();

				string firstWord = inputExp[0], functionReturnAns = string.Empty;
				firstWord = firstWord.Replace(" ", string.Empty);

				if (ParseCheckForReturn(lineCommand, firstWord, ref parseAgain)) { /*//Debug.Log("Parsed Check for Return...");*/ }
				else if (ParseCheckFunctionDef(lineCommand, firstWord)) { /*//Debug.Log("Parsed Function Definition...");*/}
				else if(ParseCheckConditional(lineCommand, firstWord)){/*//Debug.Log("Parsed Conditional...");*/}
				else if(ParseCheckLoop(lineCommand, firstWord)){/*//Debug.Log("Parsed Loop...");*/}
				else if (ParseCheckForVariables(lineCommand)) { /*//Debug.Log("Parsed Variable Definition...");*/}
				else if(ParseCheckFunctionCall(lineCommand, firstWord, ref functionReturnAns)){/*//Debug.Log("Parsed Function Call...");*/}
				else if(ParseCheckForReservedKeywords(lineCommand)){/*//Debug.Log("Parsed Reserved Keyword...");*/}
				else{/*Debug.Log("."+lineCommand+". not found.");*/}
			}

			////Debug.Log("parseAgain: " + parseAgain.ToString());

			if(parseAgain == QueueRemoveState.ParseAgain)
			{
				RemoveFromQueue();
			}
			else if(parseAgain == QueueRemoveState.WithoutParsing)
			{
				RemoveFromQueueWithoutParsing();
			}
			else if (parseAgain == QueueRemoveState.Skip)
			{
				////Debug.Log("Skipping removing from queue");
				ParseQueue();
			}
			else if (parseAgain == QueueRemoveState.SkipWithoutParsing) 
			{
				////Debug.Log("Skipping Without Parsing from queue");
			}

		}

		private void CheckForLoopFillerIteration(string lineCommand, ref bool ignore)
		{
			if (lineCommand == "<CURLOOPFILLERITERATION>") 
			{
				ignore = true;
			}
		}

		// #RETURN
		private bool ParseCheckForReturn(string lineCommand, string firstWord, ref QueueRemoveState parseAgain) 
		{
			bool ans = false;
			
			if (firstWord == "return") 
			{
				ans = true;
				string returnExp = lineCommand.Substring(6);
				lineCommand = lineCommand.Replace(" ", string.Empty);
				if (lineCommand.Length > 6) 
				{
					if (interpreter.jmUserFunctions.checkIfUserDefinedFunction(returnExp))
					{
						updateUserFunctionCallCheck();
					}
					
					interpreter.jmFunctionCalls.userReturnValue = interpreter.jmArithmetic.CheckExpression(returnExp);
				
				}
				else
				{
					interpreter.jmFunctionCalls.userReturnValue = "void";
				}
				RemoveRestOfFunctionLines();
				parseAgain = QueueRemoveState.WithoutParsing;
			}

			return ans;
		}

		private void RemoveRestOfFunctionLines() 
		{
			
			//Debug.Log("wtf");
			int i = 0;
			while (queue[i] != "<ENDJMUSERFUNCTION>") 
			{
				i++;
			}

			queue.RemoveRange(0, i - 1);

			bool t = false;
			QueueRemoveState qrs = QueueRemoveState.Skip;
			CheckForEndOfUserFunction("<ENDJMUSERFUNCTION>", ref t, ref qrs);
		}

		// #ENDUSERFUNCTION
		private void CheckForEndOfUserFunction(string lineCommand, ref bool ignore, ref QueueRemoveState parseAgain) 
		{
			if (lineCommand == "<ENDJMUSERFUNCTION>") 
			{
				ignore = true;
				parseAgain = QueueRemoveState.SkipWithoutParsing;
				
				CurFunctionAmount--;
				if (CurFunctionAmount > 0)
				{
					RemoveAndResetLocalVariablesFromFunctionVariablesList();
					RemoveAndResetLoopDataForCurrentLoop();
				}
				else if (CurFunctionAmount == 0)
				{
					interpreter.jmVariables.LocalVariables.Clear();
					interpreter.jmUserFunctions.curFunctionCall.ResetParameters();
				}
			}
		}

		private void RemoveAndResetLoopDataForCurrentLoop() 
		{
			CurLoopAmount = interpreter.jmLoops.RemoveLoopData(CurLoopAmount);
		}

		private void RemoveAndResetLocalVariablesFromFunctionVariablesList()
		{
			interpreter.jmVariables.functionVariables.RemoveAt(0);
			interpreter.jmVariables.LocalVariables = interpreter.jmVariables.functionVariables[0].Dict;
		}

		// #FUNCTIONCALL
		private bool ParseCheckFunctionCall(string lineCommand, string firstWord, ref string functionAns)
		{
			bool ans = false;

			if (interpreter.jmUserFunctions.checkIfUserDefinedFunction(lineCommand))
			{
				// increase our cur function amount and adds the local variables to the function variables list (if we have more than 1 function call)
				updateUserFunctionCallCheck();
			}
			
			if (interpreter.jmFunctionCalls.CheckFunctionNames(firstWord))
			{
				ans = true;
				int indentBefore = CurIndent;
				functionAns = interpreter.jmFunctionCalls.CheckFunctionCall(lineCommand);
				CurIndent = indentBefore;
			}

			return ans;
		}

		public void updateUserFunctionCallCheck() 
		{
			if (CurFunctionAmount > 0)
			{
				AddLocalVariablesToFunctionVariablesList();
				AddLoopDataForCurrentLoop();
			}
			CurFunctionAmount++;
		}

		private void AddLoopDataForCurrentLoop() 
		{
			// Reset our iterator dictionary in jmLoops and our CurLoopAmount to zero (0)
			CurLoopAmount = interpreter.jmLoops.AddLoopData(CurLoopAmount);
		}

		private void AddLocalVariablesToFunctionVariablesList()
		{
			FunctionVariables funcVars = new FunctionVariables(interpreter.jmVariables.LocalVariables);
			interpreter.jmVariables.functionVariables.Insert(1, funcVars);
			interpreter.jmVariables.LocalVariables.Clear();
		}

		
		// #FUNCTIONDEF
		private bool ParseCheckFunctionDef(string lineCommand, string firstWord)
		{
			bool ans = false;
			try
			{
				if (firstWord == "def")
				{
					ans = true;
					increaseCurrentIndent();
					string lineBlocks = string.Empty;
					getAllFunctionDefStatements(ref lineBlocks);

					// Add our user defined function so we have it available to parse
					interpreter.jmUserFunctions.CheckForUserFunctions(lineCommand, lineBlocks);
					// And set our indent back to normal
					decreaseCurrentIndent();
				}
			}
			catch (Exception e) 
			{
				//Debug.Log("Error at ParseCheckFunctionDef");
				//Debug.Log(e);
			}
			
			return ans;
		}

		private void getAllFunctionDefStatements(ref string lineBlocks) 
		{
			StringBuilder sb = new StringBuilder();
			int amt = 1;
			for (int i = 1; i < queue.Count; i++) 
			{
				if (getIndentAmount(queue[i]) >= CurIndent)
				{
					StringBuilder next = new StringBuilder(queue[i]);
					next.Append("\n");
					if(next[0] == '\t')
					{
						next.Remove(0, 1);
					}
					sb.Append(next);
					amt++;
				}
				else 
				{
					break;
				}
			}
			sb.Append("<ENDJMUSERFUNCTION>\n");
			RemoveFromQueueWithoutParsing(0, amt > 0 ? amt - 1 : 0);
			lineBlocks = sb.ToString();
		}

		// #LOOP
		private bool ParseCheckLoop(string lineCommand, string firstWord)
		{
			bool ans = false;
			//try
			//{
				if (firstWord == "for")
				{
					ans = true;
					parseLoop(lineCommand, LoopType.For);
				}
			/*}
			catch(Exception e)
			{
				//Debug.Log("Error at ParseCheckLoop");
				//Debug.Log(e);
			}*/
			
			return ans;
		}

		private void parseLoop(string str, LoopType loopType)
		{

			int forIndent = CurIndent;
			increaseCurrentIndent();

			if (CurLoopAmount == 0)
			{
				RemoveFromQueueWithoutParsing();
			}
			
			string block = getAllLoopBlockStatements();
			CurLoopAmount++;

			interpreter.jmLoops.CheckForLoop(str, block, CurLoopAmount);
			CurLoopAmount--;
			CurIndent = forIndent--;

		}

		private string getAllLoopBlockStatements()
		{
			StringBuilder sb = new StringBuilder();
			int amt = 0;
			if (CurLoopAmount == 0)
			{ // outer most loop block statements
				try
				{
					for (int i = 0; i < queue.Count; i++)
					{
						if (getIndentAmount(queue[i]) >= CurIndent)
						{
							StringBuilder next = new StringBuilder(queue[i]);
							next.Append("\n");
							sb.Append(next);
							amt++;
						}
						else
						{
							break;
						}
					}
					RemoveFromQueueWithoutParsing(0, amt > 0 ? amt - 1 : 0);

				}
				catch (Exception e)
				{
					//Debug.Log("Error at getAllLoopBlockStatements");
					//Debug.Log(e);
				}
			}
			else 
			{
				List<string> list = interpreter.jmLoops.entireBlockList;
				int max = list.Count, start = interpreter.jmLoops.nestedLoopIndex + 1;

				for (int i = start; i < max; i++) 
				{
					if (getIndentAmount(list[i]) >= CurIndent)
					{
						StringBuilder next = new StringBuilder(list[i]);
						next.Append("\n");
						sb.Append(next);
					}
				}
			}

			return sb.ToString();
		}

		// #CONDITIONAL
		private bool ParseCheckConditional(string lineCommand, string firstWord)
		{
			bool ans = false;
			try
			{
				if (firstWord == "if")
				{
					ans = true;
					ifFound = false;
					curConditional = false;
					parseConditional(lineCommand, Conditional.If);
				}
				else if (firstWord == "elif")
				{
					ans = true;
					if (ifFound)
					{
						if (!curConditional)
						{
							parseConditional(lineCommand, Conditional.Elif);
						}
					}
					else
					{
						//Debug.Log("Error! Elif needs an If to start!");
					}
				}
				else if (firstWord == "else")
				{
					ans = true;
					if (ifFound)
					{
						if (!curConditional)
						{
							parseConditional(lineCommand, Conditional.Else);
							curConditional = true;
						}
					}
					else
					{
						//Debug.Log("Error! Else needs an Elif or If to start!");
					}
				}
			}
			catch (Exception e)
			{
				//Debug.Log("Error at ParseCheckConditional");
				//Debug.Log(e);
			}


			return ans;
		}

		private void parseConditional(string str, Conditional cond) 
		{
			bool found = false;
			string conditional = str.Replace(" ", "");
			if (checkParenthesesCount(conditional))
			{
				if (cond == Conditional.If || cond == Conditional.Elif)
				{
					found = interpreter.jmConditionals.CheckConditional(getEntireConditionalExpression(conditional));
				}
				else 
				{
					found = true;
				}

				if (found)
				{
					curConditional = true;
					increaseCurrentIndent();
				}
				
				ifFound = true;

			}
			else
			{
				//Debug.Log("Too many parentheses!");
				found = false;
			}
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

		/// <summary>
		/// Gets the entire conditional expression, within the quotes
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public string getEntireConditionalExpression(string str)
		{
			string exp = string.Empty;
			int first = str.IndexOf('(') + 1;
			int len = str.LastIndexOf(')') - first;

			exp = str.Substring(first, len);
			return exp;
		}

		/// <summary>
		/// Gets the amount of indents, tab and 4 spaced, that are in the line.
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		private int getIndentAmount(string str) 
		{
			////Debug.Log("str is: ." + str + ".");
			int count = 0, spaceCount = 0;
			bool curCountingSpace = false;
			for (int i = 0; i < str.Length; i++) 
			{
				////Debug.Log("str[i]: " + str[i]);
				if (str[i] == '\t')
				{
					////Debug.Log("here");
					count++;
				}
				else if (str[i] == ' ') 
				{
					spaceCount++;
					if (spaceCount % 4 == 0) 
					{
						////Debug.Log("hit hithit hit");
						count++;
						spaceCount = 0;
					}
				}
				else
				{
					break;
				}
			}
			////Debug.Log("curIndent: " + curIndent);
			////Debug.Log("count indent: " + count);
			
			return count;
		}

		private bool ParseCheckForVariables(string lineCommand)
		{
			bool ans = false;

			if (lineCommand.Contains("=") || lineCommand.Contains("++") || lineCommand.Contains("--"))
			{
				ans = true;
				interpreter.jmVariables.CheckVariable(lineCommand);
			}

			return ans;
		}

		private bool ParseCheckForReservedKeywords(string lineCommand)
		{
			bool ans = false;

			return ans;
		}

		private void CheckForBlankLines(string lineCommand, ref bool ignore)
		{
			if (Regex.IsMatch(lineCommand, @"^\s*$"))
			{
				ignore = true;
			}
		}

		private void UpdateIndents(ref string lineCommand, ref bool ignore)
		{
			int indentAmount = getIndentAmount(lineCommand);

			if (indentAmount > CurIndent)
			{
				//Debug.Log("Error! Too many indents. At: " + lineCommand);
				ignore = true;
			}
			else if (indentAmount < CurIndent)
			{
				CurIndent = indentAmount;
				convertWhitespaceToTab(ref lineCommand);
				lineCommand = lineCommand.Replace("\t", "");
				//removeWhitespaceOutsideQuotes(ref lineCommand);
			}
			else if (indentAmount == CurIndent)
			{
				convertWhitespaceToTab(ref lineCommand);
				lineCommand = lineCommand.Replace("\t", "");
				//removeWhitespaceOutsideQuotes(ref lineCommand);
			}
		}

		private void convertWhitespaceToTab(ref string line) 
		{
			StringBuilder sb = new StringBuilder(line);
			int count = 0, amount = 0;
			for (int i = 0; i < line.Length; i++) 
			{
				if (line[i] == ' ')
				{
					count++;
				}
				else 
				{
					break;
				}
				if (count == 4) 
				{
					count = 0;
					amount++;
					//sb.Remove(0, 4);
					//sb.Insert(0, '\t');
				}
			}

			if (amount > 0) 
			{
				sb.Remove(0, 4 * amount);
				StringBuilder tabs = new StringBuilder('\t');
				for (int i = 0; i < amount; i++) 
				{
					tabs.Append('\t');
				}
				sb.Insert(0, tabs.ToString());
			}
			

			line = sb.ToString();
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

		private void CheckForComments(ref string lineCommand)
		{

			CommentState state = CommentState.None;
			for (int i = 0; i < lineCommand.Length; i++) 
			{
				if (lineCommand[i] == '"')
				{
					if (state == CommentState.None || state == CommentState.Outside)
					{
						state = CommentState.Inside;
					}
					else if (state == CommentState.Inside)
					{
						state = CommentState.Outside;
					}

				}
				else if(state == CommentState.None)
				{
					state = CommentState.Outside;
				}
				
				if (state == CommentState.Outside) 
				{
					////Debug.Log("Index of: " + lineCommand.IndexOf('#'));
					////Debug.Log("i is: " + i);
					if (lineCommand.IndexOf('#') >= i) 
					{
						if (lineCommand.Contains("#"))
						{
							////Debug.Log("Comment Found");
							lineCommand = lineCommand.Substring(0, lineCommand.IndexOf("#"));
							break;
						}
					}
					
				}
			}
		}

		private void increaseCurrentIndent()
		{
			CurIndent++;
		}

		private void decreaseCurrentIndent()
		{
			CurIndent--;
		}

		/// <summary>
		/// Single takes in the single command, raw, unformatted, and adds it to the queue.
		/// </summary>
		/// <param name="pIn">pIn is the player input.</param>

		public void Single(string pIn)
		{
			AddToQueue(pIn);
			if (queue.Count > 0 && !running)
			{
				running = true;
				ParseQueue(queue[0], QueueRemoveState.ParseAgain);
			}
		}

		/// <summary>
		/// Multi takes in a single string, broken up by (\n) line breaks, and adds
		/// those commands into the queue sequentially.
		/// </summary>
		/// <param name="pIn">pIn is the player input.</param>

		public void Multi(string pIn)
		{
			string pattern = @"(\n)";
			List<string> list = Regex.Split(pIn, pattern, RegexOptions.IgnorePatternWhitespace).toList();
			//Debug.Log("list is: ");
			//list.print();
			AddToQueue(list);

			if (queue.Count > 0 && !running)
			{
				running = true;
				ParseQueue(queue[0], QueueRemoveState.ParseAgain);
			}
		}

		public void AddFunctionLineData(string pIn)
		{
			string pattern = @"(\n)";
			List<string> list = Regex.Split(pIn, pattern, RegexOptions.IgnorePatternWhitespace).toList();
			AddToQueue(list, 1);
		}

		public void ParseQueue()
		{
			if (queue.Count > 0) 
			{
				ParseQueue(queue[0], QueueRemoveState.ParseAgain);
			}
			
		}

		public void ParseQueue(QueueRemoveState choice) 
		{
			ParseQueue(queue[0], choice);
		}

		public void AddToQueue(string pIn, int loc)
		{
			queue.Insert(loc, pIn);
		}

		/// <summary>
		/// String variation for adding to the queue.
		/// Queue is a list, using the Add function of Lists.
		/// </summary>
		/// <param name="pIn">pIn is the user input.</param>
		
		public void AddToQueue(string pIn)
		{
			queue.Add(pIn);
		}
		
		/// <summary>
		/// List<string> variation for adding to the queue.
		/// Makes calls to the string variation internally.
		/// Queue is a list, using the Add function of Lists.
		/// </summary>
		/// <param name="pIn">pIn is the user input.</param>
		
		public void AddToQueue(List<string> pIn)
		{
			for(int i = 0; i < pIn.Count; i++){ queue.Add(pIn[i]);}
		}

		public void AddToQueue(List<string> pIn, int index)
		{
			////Debug.Log("pIn is:");
			//pIn.print();
			for(int i = pIn.Count - 1; i >= 0; i--){ queue.Insert(index, pIn[i]);}
		}
		
		/// <summary>
		/// Removes the item from the queue if there is something in it.
		/// If there is something left after removing, parse the queue again
		/// at it's latest parameter (0).
		/// Internally uses list functions, RemoveAt.
		/// </summary>
		
		public void RemoveFromQueue()
		{
			if(queue.Count != 0)
			{
				queue.RemoveAt(0);
				if (queue.Count > 0) { ParseQueue(queue[0], QueueRemoveState.ParseAgain); }
				else{ running = false;}
				//onChange();
			}
		}

		public void RemoveFromQueueWithoutParsing()
		{
			////Debug.Log("removing: " + queue[0]);
			if(queue.Count != 0)
			{
				queue.RemoveAt(0);
				//onChange();
			}
		}

		public void RemoveFromQueueWithoutParsing(int index)
		{
			if(queue.Count != 0)
			{
				queue.RemoveAt(index);
				//onChange();
			}
		}

		public void RemoveFromQueueWithoutParsing(int index, int count)
		{
			if(queue.Count != 0 && count != 0)
			{
				queue.RemoveRange(index, count);
				//onChange();
			}
		}

		/// <summary>
		/// Resets the queue by removing all entries from the queue.
		/// Using internally list functions, RemoveRange.
		/// And finally sets running to false.
		/// </summary>
		
		public void ResetQueue()
		{
			queue.RemoveRange(0, queue.Count);
			running = false;
		}

	}
}