using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using JM.Interpreter;

public class ScriptManager : MonoBehaviour {

	public UIScrollView scriptSv;
	public AudioSource buttonSound, clearSound, runSound;
	public JMInterpreter interpreter;
	public UIPopupList scriptPopupList;
	public UIInput scriptNameInput, scriptDataInput;
	private Dictionary<string, string> scripts = new Dictionary<string, string>();
	private List<string> scriptNameList = new List<string>();

	void Start ()
	{
		string str = "# JMScript is a scripting language built for the Unity player environment.\n" +
"# It is python-inspired, deriving most of its syntax from the language.\n" +
"# JMScript comes with a built-in print function.\n\n" +

"# Primitive types:\n" +
"# JMInt, JMFloat, JMBool, JMString \n\n" +

"# Currently supported features:\n" +
"# Built-in Function calls\n" +
"# Loops (for)\n" +
"# Conditionals (if, elif, else)\n" +
"# Variable declarations\n" +
"# Boolean, string and mathematical arithmetic\n" +
"# Comments (Start a comment with # sign)\n\n" +

"# Anytime you change the name of a script, a new one will be saved.\n" + 
"# You can access the list of all scripts using the button in the\n" + 
"# top left corner of this window.\n\n" +

"# Note: JMScript is in a very, very early alpha stage, and therefore\n" + 
"# is unstable. The scripts written here are for demonstrative purposes.\n" + 
"# You can write your own scripts, be cautious that they may not work.\n" +
"# Use the prewritten scripts as reference.\n";
		scripts.Add("Welcome", str);
		scriptNameList.Add("Welcome");

		str = "# Variables are declared like this:\n\n" +

"val = 5\n" +
"print(\"The variable val is: \" + val)\n\n" +

"# You can store integers like 5 inside of them\n" +
"# And also floating point values like 5.125\n\n" +

"val = 5.125\n" +
"print(\"Now the variable val is: \" + val)\n\n" +

"# Variables can store integers, floats, strings, bools\n\n" +

"val = \"A string inside\"\n" +
"print(\"Val becomes: \" + val)\n\n" +

"# You can also use variables in arithmetic expressions\n\n" +

"val = 5\n" +
"val = val + 3\n" +
"print(\"Val is now: \" + val)\n\n" +

"# And also using shorthand operators\n\n" +

"val++\n" +
"print(\"Val is now: \" + val)\n" +
"val++\n" +
"val/=5\n" +
"print(\"Val is now: \" + val)\n\n" +

"# Full support for mathematical arithmetic for integer and float expressions\n\n" +
"val = 5 * 2 + (val - 1) * (6.5/4)\n" +
"print(\"Val is now: \" + val)\n" +
"# End of variable demonstration\n";
		scripts.Add("Variables", str);
		scriptNameList.Add("Variables");

		str = "# Conditionals follow the syntax of Python.\n\n" + 

"if(5 > 3):\n" + 
"    print(\"5 is greater than 3\")\n\n" + 

"# The expressions need to be tab indented \n" + 
"# (equivalent of 4 spaces) in order to be recognized.\n\n" + 

"if(5 > 6):\n" + 
"    print(\"wrong!\")\n" + 
"elif(5 > 2):\n" + 
"    print(\"right!\")\n\n" + 

"# It will go through each conditional branch until it finds\n" + 
"# one that is boolean true\n\n" + 

"elif(5 > 3):\n" + 
"    print(\"will not work\")\n\n" + 

"else:\n" + 
"    print(\"will also not work\")\n\n" + 

"# Elif needs an if to start, and else needs either to follow from\n" + 

"# Conditionals can also be wrapped inside each other\n\n" + 
"if(5 > 3):\n" + 
"    if(3 > 2):\n" + 
"        if(1 > 0):\n" + 
"            print(\"right!\")\n" + 
"        else:\n" + 
"            print(\"will not reach\")\n\n" + 
			
"if(5 > 3):\n" + 
"    if(2 > 3):\n" + 
"        print(\"wrong\")\n" + 
"    else:\n" + 
"        print(\"right\")\n\n" + 
	
"    print(\"First one will continue until\")\n" + 
"    print(\"it finds a lesser tab indent.\")\n" + 
"    print(\"\")\n\n" + 

"print(\"After the statements\")\n" + 
"print(\"End of conditional demonstration\")\n"; 

		scripts.Add("Conditionals", str);
		scriptNameList.Add("Conditionals");

		str = "# Loops are supported to a lesser extent.\n" + 
"# And only for For loops\n" + 
"# They require an iterator, and a range function\n" + 
"# with three parameters \n\n" + 

"for i in range(0, 10, 1):\n" + 
"    print(\"i is: \" + i)\n\n" + 
	
"# Inside loops, you can use conditionals,\n" + 
"# variable declarations, other loops, whatever\n" + 
"# you can do outside of them you can do inside.\n\n" + 

"for i in range(0, 10, 1):\n" + 
"    if(i % 2 == 0):\n" + 
"        print(\"i mod 2 for: \" + i)\n" + 
"    elif(i % 3 == 0):\n" + 
"        print(\"i mod 3 for: \" + i)\n\n" + 
	
"    print(\"For each iteration this will show\")\n\n" + 
	
"    for j in range(0, 3, 1):\n" + 
"        print(\"j is: \" + j)\n\n" + 

"# After the loop, this will print\n\n" + 

"print(\"End of loop demonstration\")\n";

		scripts.Add("Loops", str);
		scriptNameList.Add("Loops");

		str = "# As you may have seen by now, JMScript comes with a\n" +
"# built in function called print(string str)\n\n" +

"print(\"This is a function call for print\")\n" +

"# You can use it to display items to the console\n\n" +

"num = 5\n" +

"print(num)\n" +
"print(\"Num is: \" + num)\n\n" +
"num *= 2\n\n" +
"print(\"Now num is: \" + num)\n\n" +
"num -= 3.250\n" +
"print(\"Finally num is: \" + num)\n\n" +

"# End of print demonstration\n";

		scripts.Add("Print", str);
		scriptNameList.Add("Print");

		str = "choice = 10\n\n" + 
"result = choice\n" + 
"for i in range(choice - 1, 1, -1):\n" + 
"    result *=  i\n\n" + 

"print(\"Factorial of \" + choice + \" is: \" + result)\n"; 

		scripts.Add("Factorial", str);
		scriptNameList.Add("Factorial");

		str = "base = 3\n" +
"exponent = 6\n\n" +

"result = base\n" +
"for i in range(1, exponent, 1):\n" +
"    result *= base	\n\n" +

"print(base + \" raised to \" + exponent + \" is: \" + result)\n";

		scripts.Add("Pow", str);
		scriptNameList.Add("Pow");

		str = "# choice must have a decimal place, to indicate float math\n" +
"choice = 5.0 \n\n" +

"result = choice\n" +
"for i in  range(0, 6, 1):\n" +
"    result = 0.5 * (result + (choice / result))\n\n" +

"print(\"The square root of \" + choice + \" is \" + result)\n";

		scripts.Add("Sqrt", str);
		scriptNameList.Add("Sqrt");

		scriptPopupList.items = scriptNameList;

	}

	public void saveScript() 
	{
		string name = scriptNameInput.value.Replace(" ", string.Empty);
		if (scripts.ContainsKey(name))
		{
			scripts[name] = scriptDataInput.value;
		}
		else
		{
			scripts.Add(name, scriptDataInput.value);
			scriptNameList.Add(name);
		}
		scriptPopupList.items = scriptNameList;

		scriptSv.UpdateScrollbars(true);
	}

	public void loadScript() 
	{
		playButtonSound();
		// first save the current script
		saveScript();

		if (scripts[scriptPopupList.value].Length > 10000)
		{
			scriptDataInput.value = scripts[scriptPopupList.value].Substring(0, 10000);
		}
		else
		{
			scriptDataInput.value = scripts[scriptPopupList.value];
		}

		scriptNameInput.value = scriptPopupList.value;
		//curSelectedReceiptBundle = scripts[scriptPopupList.value];
		scriptNameInput.UpdateLabel();
		scriptDataInput.UpdateLabel();

		scriptSv.UpdateScrollbars(true);
		scriptSv.UpdatePosition();

	}

	public void runScript() 
	{
		//Debug.Log("hit");
		//Debug.Log(scriptDataInput.value);
		string str = scriptDataInput.value.Replace('\r', '\n');
		interpreter.jmQueue.Multi(str);
		//interpreter.jmQueue.AddToQueue(scriptDataInput.value);
		interpreter.jmQueue.ParseQueue();
		playRunSound();
		//interpreter.jmQueue.AddToQueue("print(\"End running\")");
	}

	public void playButtonSound() 
	{
		buttonSound.Stop();
		buttonSound.Play();
	}

	public void playClearSound() 
	{
		clearSound.Stop();
		clearSound.Play();
	}

	public void playRunSound() 
	{
		runSound.Stop();
		runSound.Play();
	}

}
