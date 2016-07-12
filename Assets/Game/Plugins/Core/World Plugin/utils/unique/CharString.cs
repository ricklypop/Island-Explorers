using UnityEngine;
using System.Collections;
using System;

//A string object, in which can be ordered by it's char numerical values

public class CharString : IComparable {
	public string s{ get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="ComparableString"/> class.
	/// </summary>
	/// <param name="s">A string to create from.</param>
	public CharString(string s){
		this.s = s;
	}
		
	/// <summary>
	/// Compares to another object.
	/// </summary>
	/// <returns>The comparison.</returns>
	/// <param name="obj">Object.</param>
	public int CompareTo (object obj)
	{
		string id1 = s;
		CharString compare = (CharString) obj;
		string id2 = compare.s;
		for(int i = 0; i < id1.Length; i++){
			int char1 = char.ToUpper (id1[i]);
			int char2 = char.ToUpper (id2[i]);
			if (char1 != char2) {
				if (char1 < char2)
					return -1;
				else
					return 1;
			}
		}
		return 0;
	}

	/// <summary>
	/// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="ComparableString"/>.
	/// </summary>
	/// <param name="o">The <see cref="System.Object"/> to compare with the current <see cref="ComparableString"/>.</param>
	/// <returns><c>true</c> if the specified <see cref="System.Object"/> is equal to the current <see cref="ComparableString"/>;
	/// otherwise, <c>false</c>.</returns>
	public override bool Equals(System.Object o){
		if (((CharString)o).s == s)
			return true;
		else
			return false;
	}
}
