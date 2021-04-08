using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using JM.PrimitiveTypes;
using JM.UserFunctions;
using JM.Variables;

/// <summary>
/// Extension methods.
/// </summary>

public static class ExtensionMethods 
{

	public static void print(this List<FunctionVariables> list)
	{
		for(int i = 0; i < list.Count; i++)
		{
			Debug.Log("Dict[" + i + "]: ");
			list[i].Dict.print();
		}
	}

	public static void removeWhitespace(this string[] input)
	{
		for(int i = 0; i < input.Length; i++)
		{
			input[i].Replace(" ", "");
		}
	}

	/// <summary>
	/// Initialize a array of booleans one way or the other.
	/// </summary>
	/// <param name="array">Array is what we're affecting.</param>
	/// <param name="choice">Choice is set to true by default, can explicitly set it to false.</param>

	public static void setBools(this bool[] array, bool choice = true)
	{
		for(int i = 0; i < array.Length; i++){ array[i] = choice;}
	}

	/// <summary>
	/// Print the specified array.
	/// </summary>
	/// <param name="array">Array.</param>

	public static void print(this bool[] array)
	{
		for(int i = 0; i < array.Length; i++)
		{
			Debug.Log(i + ":" + array[i] + "\n");
		}
	}

	/// <summary>
	/// Print the specified list.
	/// </summary>
	/// <param name="list">List to modify.</param>

	public static void print(this List<string> list)
	{
		for(int i = 0; i < list.Count; i++)
		{
			Debug.Log(list[i] + "\n");
		}
	}

	/// <summary>
	/// Convert the list to a string with line breaks (\n).
	/// </summary>
	/// <returns>The string.</returns>
	/// <param name="list">List to modify.</param>

	public static string toString(this List<string> list)
	{
		string temp = ""; // to store our list
		for(int i = 0; i < list.Count; i++){temp += list[i] + "\n";} // in string friendly format
		return temp;
	}

	/// <summary>
	/// Converts to string, separate from ToString.
	/// </summary>
	/// <returns>The string.</returns>
	/// <param name="list">List to modify.</param>

	public static string _ToString(this List<string> list)
	{
		string temp = "";
		for(int i = 0; i < list.Count; i++)
		{
			temp += list[i];
		}
		return temp;
	}

	/// <summary>
	/// Print the specified input.
	/// </summary>
	/// <param name="input">Input to modify.</param>

	public static void print(this string[] input)
	{
		for(int i = 0; i < input.Length; i++){ Debug.Log("." + input[i] + ".");}
	}

	/// <summary>
	/// Print the specified <c>Dictionary<string, string></c>.
	/// </summary>
	/// <param name="dict">Dictionary to modify.</param>

	public static void print(this Dictionary<string, string> dict)
	{
		foreach (var val in dict) { Debug.Log("." + val.Key + ". : " + "." + val.Value + "."); }
	}

	public static void print(this Dictionary<string, FunctionF> dict) 
	{
		foreach (var val in dict)
		{
			//Debug.Log("." + val.Key + ". : " + "." + val.Value + ".");
			Debug.Log("Name: " + val.Value.name);
			Debug.Log("LineData: \n" + val.Value.lineData);
			Debug.Log("Parameters: \n");
			val.Value.funcParameters.print();
		}
	}

	public static void print(this List<FunctionParameters> list)
	{
		for(int i = 0; i < list.Count; i++)
		{
			Debug.Log(list[i].paramName + ": ." + list[i].paramValue + ".\n"); 
		
		}
	}

	/// <summary>
	/// Print the specified <c>List<JMType.IJMType></c>.
	/// </summary>
	/// <param name="list">List to modify.</param>

	public static void print(this List<JMType.IJMType> list)
	{
		for(int i = 0; i < list.Count; i++){ Debug.Log(list[i].toString());}
	}

	/// <summary>
	/// Prints the queue, can be applied to any <c>List<string></c>
	/// </summary>
	/// <param name="queue">Queue to modify.</param>

	public static void printQueue(this List<string> queue)
	{
		string temp = "";
		for(int i = 0; i < queue.Count; i++){ temp += i + ": " + queue[i] + "\n";}
		Debug.Log(temp);
	}

	/// <summary>
	/// Converts a <c>string[]</c> to a <c>List<string></c>, with options
	/// to remove extra entries and whitespace.
	/// </summary>
	/// <returns>The the newly formed list.</returns>
	/// <param name="str">Str is the string array to convert.</param>
	/// <param name="remove">If set to <c>true</c> remove, otherwise just add it.</param>

	public static List<string> toList(this string[] str, bool remove = true){
		List<string> temp = new List<string>();
		for(int i = 0; i < str.Length; i++)
		{
			if(remove)
			{
				if(str[i] == "\n" || str[i] == " " || str[i] == "")
				{
					continue;
				}
				temp.Add(str[i]);
			}
			else
			{ 
				temp.Add(str[i]); 
			}

		}
		return temp;
	}

	/// <summary>
	/// Removes all whitespace from the specified <c>List<string> elements.</c>
	/// </summary>
	/// <param name="output">Output is the <c>List<string></c> to modify.</param>

	public static void removeWhitespace(this List<string> output)
	{
		for(int i = 0; i < output.Count; i++)
		{
			//Debug.Log("Removing: ." + output[i] + ".");
			output[i] = output[i].Replace(" ", string.Empty);
		}
	}

	/// <summary>
	/// Removes the extra entries from the list directly.
	/// </summary>
	/// <param name="output">Output is the <c>List<string></c> to modify.</param>

	public static void removeExtraEntries(this List<string> output)
	{
		for(int i = 0; i < output.Count; i++)
		{
			if(output[i] == "" || output[i] == " " || output[i] == "\n")
			{
				output.RemoveAt(i);
			}
		}
	}

}