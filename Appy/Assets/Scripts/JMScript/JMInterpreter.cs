using UnityEngine;
using System.Collections;
using JM.Arithmetic;
using JM.Standard;
using JM.Conditionals;
using JM.Functions;
using JM.Loops;
using JM.Queue;
using JM.Variables;
using JM.Math;
using JM.UserFunctions;

namespace JM.Interpreter 
{
	public class JMInterpreter : MonoBehaviour
	{

		public JMMath jmMath;
		public JMArithmetic jmArithmetic;
		public JMVariables jmVariables;
		public JMStandard jmStandard;
		public JMQueue jmQueue;
		public JMFunctionCalls jmFunctionCalls;
		public JMConditionals jmConditionals;
		public JMLoops jmLoops;
		public JMUserFunctions jmUserFunctions;

		void Awake()
		{

			jmMath = new JMMath(this);
			jmStandard.Interpreter = this;
			jmVariables = new JMVariables(this);
			jmArithmetic = new JMArithmetic(this);
			jmQueue = new JMQueue(this);
			jmFunctionCalls = new JMFunctionCalls(this);
			jmConditionals = new JMConditionals(this);
			jmLoops = new JMLoops(this);
			jmUserFunctions = new JMUserFunctions(this);
			print("JMQueue: " + jmQueue);
		}
		void Start()
		{

			/*for (int i = 1; i <= 100; i++) 
			{
				if (i % 3 == 0 && i % 5 == 0) 
				{
					print("FizzBuzz");
				}
				else if (i % 3 == 0) 
				{
					print("Fizz");
				}
				else if (i % 5 == 0)
				{
					print("Buzz");
				}
				else 
				{
					print(i);
				}

			}*/

			//print(JMMath.Sin(30));
			//jmQueue.Single("readFile(\"TestFiles/StarterFiles/sqrt.jms\")");
			//jmQueue.Single("readFile(\"TestFiles/MathFiles/trig03.jmpy\")");
			//jmQueue.Single("readFile(\"TestFiles/MathFiles/factorial.jmpy\")");
			//jmQueue.Single("readFile(\"TestFiles/MathFiles/pow.jmpy\")");
			//jmQueue.Single("readFile(\"TestFiles/MathFiles/sqrt.jmpy\")");

			//jmQueue.Single("readFile(\"TestFiles/CondFiles/cond01.jmpy\")");
			//jmQueue.Single("readFile(\"TestFiles/ForFiles/for14.jmpy\")");
			//jmQueue.Single("readFile(\"TestFiles/DefFiles/def02.jms\")");
			//jmQueue.Single("readFile(\"TestFiles/StandardFiles/std01.jmpy\")");
			//jmQueue.Single("readFile(\"TestFiles/StringFiles/string02.jmpy\")");
			//jmQueue.Single("readFile(\"TestFiles/VarFiles/var01.jmpy\")");
		}

	}
}



