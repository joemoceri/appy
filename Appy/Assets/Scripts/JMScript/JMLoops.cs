using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
using JM.Arithmetic;
using JM.Math;
using JM.Queue;
using JM.Interpreter;

namespace JM.Loops
{
	public class JMLoops
	{
		
		private JMInterpreter interpreter;
		private Dictionary<string, string> userIter;
		public Dictionary<string, string> UserIter {get{return userIter;}}
		public List<string> entireBlockList = new List<string>();
		public List<LoopData> loopData = new List<LoopData>();
		public int nestedLoopIndex = 0;

		public JMLoops(JMInterpreter i)
		{
			interpreter = i;
			userIter = new Dictionary<string, string>();
		}

		public void CheckForLoop(string line, string loopBlock, int curLoopAmount) 
		{
			int start = 0, end = 0, iter = 0, step = 0;
			List<string> forExpList = new List<string>();
			string pattern = @"\s(?=.*?(?:\().*?(?:\)).*?)|(?<=(?:\().*?(?:\)).*?)|(:)\s";
			forExpList = Regex.Split(line, pattern).toList();

			setIterationParameters(ref start, ref end, ref step, forExpList[3]);
			iter = start;

			userIter.Add(forExpList[1], start.ToString());
			List<string> blockList = loopBlock.Split('\n').toList();
			
			if (curLoopAmount == 1)
			{
				entireBlockList = blockList;
			}

			ForLoop(iter, step, end, blockList, forExpList[1]);
			userIter.Remove(forExpList[1]);

		}

		private void setIterationParameters(ref int start, ref int end, ref int step, string str)
		{
			List<string> list = Regex.Split(str, @"(\()|(\))|(,)").toList();

			start = int.Parse(interpreter.jmArithmetic.CheckExpression(list[2]));
			end = int.Parse(interpreter.jmArithmetic.CheckExpression(list[4]));
			step = int.Parse(interpreter.jmArithmetic.CheckExpression(list[6]));

		}

		public void ForLoop(int iter, int step, int end, List<string> blockLines, string iterName)
		{
			bool keepLooping = step > 0 ? iter < end : iter > end;
			if (keepLooping)
			{
				bool breakFound = false;
				for(int i = 0; i < blockLines.Count; i++)
				{
					string keywordCheck = blockLines[i].Replace(" ", string.Empty);
					keywordCheck = keywordCheck.Replace("\t", string.Empty);

					if (keywordCheck == "break") 
					{
						breakFound = true;
						break;
					}
					else if (keywordCheck == "continue") 
					{
						break;
					}
					try
					{
						string lineToAdd = "<CURLOOPFILLERITERATION>#" + blockLines[i];
						////Debug.Log("ADDING: " + lineToAdd + "FROM JMLOOP");
						interpreter.jmQueue.AddToQueue(lineToAdd, 0);
					}
					catch(Exception e)
					{
						//Debug.Log(e);
					}
					if (blockLines[i].Contains("for")) 
					{
						nestedLoopIndex = i;
					}
					interpreter.jmQueue.ParseQueue(blockLines[i], QueueRemoveState.WithoutParsing);
				}

				if (breakFound) 
				{
					return;
				}

				int s = iter + step;
				string updatedIterValue = s.ToString();
				userIter[iterName] = updatedIterValue;
				ForLoop (s, step, end, blockLines, iterName);
			}
		}

		public string getIteratorValue(string key)
		{
			string temp = key;
			foreach (var v in userIter)
			{
				if (v.Key == key) { temp = v.Value; }
			}
			return temp;
		}

		public int AddLoopData(int curLoopAmount)
		{
			LoopData ld = new LoopData(curLoopAmount, userIter);
			loopData.Add(ld);
			curLoopAmount = 0;
			userIter.Clear();
			
			return curLoopAmount;
		}

		public int RemoveLoopData(int curLoopAmount)
		{
			/*//Debug.Log("AD;LFKGHDAS;LFGHA;LEKTHG;LAKSDHG;LASKDFHT;ASDKLHALHA'LSKDYA'SLKDHYAS'LKDYHASL'DKYHA'LSDKHY'ASLKDTS'AKT");
			for (int i = 0; i < loopData.Count; i++) 
			{
				//Debug.Log("loopDataAmount: " + loopData[i].LoopAmount);
			}*/
			
			curLoopAmount = loopData[0].LoopAmount;
			userIter = loopData[0].Iterators;
			loopData.RemoveAt(0);
			
			return curLoopAmount;
		}

		

	}

	public struct LoopData
	{
		private int loopAmount;
		public int LoopAmount { get { return loopAmount; } }
		private Dictionary<string, string> iterators;
		public Dictionary<string, string> Iterators { get { return iterators; } set{} }

		public LoopData(int loopAmt, Dictionary<string, string> iter) 
		{
			loopAmount = loopAmt;
			iterators = iter;
		}

	}
}