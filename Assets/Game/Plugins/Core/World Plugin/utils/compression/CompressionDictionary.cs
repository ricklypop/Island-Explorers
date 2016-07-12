using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CompressionDictionary {
	#region Getters and Setters
	public Dictionary<string, int> methods = new Dictionary<string, int>();
	public int current{ get; set; }

	/// <summary>
	/// Get the length of the dictionary.
	/// </summary>
	public int Length(){return methods.Keys.Count;}
	#endregion

	public CompressionDictionary(){}

	#region Add to the Compressed Dictionary
	/// <summary>
	/// Add the specified name.
	/// Creates a corresponding integer value.
	/// </summary>
	/// <param name="name">Name.</param>
	public void Add(string name){
		if (methods.ContainsKey (name)) {
			methods [name] = current;
			current++;
		} else {
			methods.Add (name, current);
			current++;
		}
	}
		
	/// <summary>
	/// Sets the dictionary equal to a string list with int convertions.
	/// </summary>
	/// <param name="e">The string list.</param>
	public void SetEquals(List<string> e){
		current = 0;
		methods = new Dictionary<string, int>();
		foreach (string s in e)
			Add (s);
	}

	/// <summary>
	/// Sets the dictionary equal to a comparable string list with int convertions.
	/// </summary>
	/// <param name="e">The comparable string list.</param>
	public void SetEquals(List<CharString> e){
		current = 0;
		methods = new Dictionary<string, int>();
		foreach (CharString c in e)
			Add (c.s);
	}
	#endregion

	#region Convertion Methods
	/// <summary>
	/// Gets the integer conversion.
	/// </summary>
	/// <returns>The integer conversion.</returns>
	public int Compress(string g){
		return methods [g];
	}

	/// <summary>
	/// Gets the string conversion.
	/// </summary>
	/// <param name="i">The integer to convert.</param>
	public string Decompress(int i){
		foreach (string s in methods.Keys)
			if (methods [s] == i)
				return s;
		return null;
	}
	#endregion
}
