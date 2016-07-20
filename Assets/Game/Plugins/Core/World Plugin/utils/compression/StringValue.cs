using System;
using System.Reflection;

public class StringValue : System.Attribute
{
	public static string GetStringValue(Enum value){
		string output = null;
		Type type = value.GetType();

		//Check first in our cached results...

		//Look for our 'StringValueAttribute' 

		//in the field's custom attributes

		FieldInfo fi = type.GetField(value.ToString());
		StringValue[] attrs =
			fi.GetCustomAttributes(typeof(StringValue),
				false) as StringValue[];
		if (attrs.Length > 0)
		{ 
			output = attrs[0].Value;
		}

		return output;
	} 

	private readonly string _value;

	public StringValue(string value)
	{
		_value = value;
	}

	public string Value
	{
		get { return _value; }
	}

}