using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;
using JM.Arithmetic;
using JM.Math;
using JM.Interpreter;

namespace JM.Standard
{
	/// <summary>
	/// JM Commands is used for all general commands not related to robots (S3).
	/// It handles things like file input, aborting an operation, displaying a help file.
	/// </summary>

	public class JMStandard : MonoBehaviour
	{
		public UserInterface ui;
		private JMInterpreter interpreter;
		public JMInterpreter Interpreter { set { interpreter = value; } }

		public JMStandard(JMInterpreter i)
		{
			interpreter = i;
		}

		public void print(string str) 
		{
			removeOutsideQuotes(ref str);
			ui.AddToConsoleHistory(str);
			//Debug.Log("---------> " + str);
		}

		private void removeOutsideQuotes(ref string str) 
		{
			StringBuilder sb = new StringBuilder(str);
			if (sb[0] == '"') 
			{
				sb.Remove(0, 1);
			}
			if (sb[sb.Length - 1] == '"') 
			{
				sb.Remove(sb.Length - 1, 1);
			}
			str = sb.ToString();
		}

		/// <summary>
		/// This is the starting point for where execution of file input from the console is.
		/// Returns the collection of commands extracted from the file, if any.
		/// </summary>
		/// <returns>The list of commands from the file.</returns>
		/// <param name="line">Line is the name of the file, with extension (.jms) at the end.</param>
		
		public string readFile(string line)
		{
			/*removeOutsideQuotes(ref line);

			#if UNITY_STANDALONE_OSX
			string dir = System.Environment.CurrentDirectory + "/" + line;
			#elif UNITY_STANDALONE_WIN
			string dir = System.Environment.CurrentDirectory + "\\" + line;
			#endif

			string temp = string.Empty;
			
			try 
			{
				// Create an instance of StreamReader to read from a file. 
				// The using statement also closes the StreamReader. 
				using (StreamReader sr = new StreamReader(dir)) 
				{
					string l;
					while ((l = sr.ReadLine()) != null) 
					{
						temp += l;
						temp += "\n";
					}
				}
			}
			catch 
			{
				////Debug.Log("The file could not be read.");
			}
			
			////Debug.Log("Temp is: \n" + temp);
			
			return temp;
			 * */
			return "";
		}

	}
}
