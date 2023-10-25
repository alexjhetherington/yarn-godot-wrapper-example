using Godot;
using System.Collections.Generic;
using System;
using YarnDonut;

public partial class variable_storage_wrapper : VariableStorageBehaviour
{
	[Export]
	public Node storyState;

	// Duplicate only because Generic Try Get method used by Yarn itself has real troubles dealing with Variants
	// We use the godot one as the source of truth, and the generic collections one *only* for the try get method :l
	private Dictionary<string, object> variablesYarn = new Dictionary<string, object>();
	private Godot.Collections.Dictionary<string, Variant> variablesGodot = new Godot.Collections.Dictionary<string, Variant>();

	private Dictionary<string, Type> variableTypes = new Dictionary<string, Type>(); // needed for serialization

	private bool dirty = false;

	public void _PropagateToStoryState(){
		if(dirty){
			storyState.Set("variables", variablesGodot);
			storyState.Call("notify_new_variables");
			dirty = false;
		}
	}

	public void SetDirty(){
		if(!dirty) {
			dirty = true;
			CallDeferred(nameof(_PropagateToStoryState));
		}
	}

	#region Extra set value methods because Godot does not understand how to deal with method overloads correctly :(
	public void SetValueString(string variableName, string stringValue)
	{
		SetValue(variableName, stringValue);
	}

	public void SetValueFloat(string variableName, float floatValue)
	{
		SetValue(variableName, floatValue);
	}

	public void SetValueBool(string variableName, bool boolValue)
	{
		SetValue(variableName, boolValue);
	}
	#endregion

	#region For Dialogue Runner
	public override void Clear()
	{
		variablesGodot.Clear();
		variablesYarn.Clear();
		variableTypes.Clear();
	}

	public override bool Contains(string variableName)
	{
		return variablesGodot.ContainsKey(variableName);
	}

	public override (Dictionary<string, float>, Dictionary<string, string>, Dictionary<string, bool>) GetAllVariables()
	{
		Dictionary<string, float> floatDict = new Dictionary<string, float>();
		Dictionary<string, string> stringDict = new Dictionary<string, string>();
		Dictionary<string, bool> boolDict = new Dictionary<string, bool>();

		foreach (var variable in variablesGodot)
		{
			var type = variableTypes[variable.Key];

			if (type == typeof(float))
			{
				float value = System.Convert.ToSingle(variable.Value);
				floatDict.Add(variable.Key, value);
			}
			else if (type == typeof(string))
			{
				string value = System.Convert.ToString(variable.Value);
				stringDict.Add(variable.Key, value);
			}
			else if (type == typeof(bool))
			{
				bool value = System.Convert.ToBoolean(variable.Value);
				boolDict.Add(variable.Key, value);
			}
			else
			{
				GD.Print($"{variable.Key} is not a valid type");
			}
		}

		return (floatDict, stringDict, boolDict);
	}

	public override void SetAllVariables(Dictionary<string, float> floats, Dictionary<string, string> strings, Dictionary<string, bool> bools, bool clear = true)
	{
		if (clear)
		{
			variablesGodot.Clear();
			variablesYarn.Clear();
			variableTypes.Clear();
		}

		foreach (var value in floats)
		{
			SetValue(value.Key, value.Value);
		}
		foreach (var value in strings)
		{
			SetValue(value.Key, value.Value);
		}
		foreach (var value in bools)
		{
			SetValue(value.Key, value.Value);
		}

		GD.Print($"bulk loaded {floats.Count} floats, {strings.Count} strings, {bools.Count} bools");
	}

	public override void SetValue(string variableName, string stringValue)
	{
		variablesYarn[variableName] = stringValue;
		variablesGodot[variableName] = stringValue;
		variableTypes[variableName] = typeof(string);

		SetDirty();
	}

	public override void SetValue(string variableName, float floatValue)
	{
		variablesYarn[variableName] = floatValue;
		variablesGodot[variableName] = floatValue;
		variableTypes[variableName] = typeof(float);

		SetDirty();
	}

	public override void SetValue(string variableName, bool boolValue)
	{
		variablesYarn[variableName] = boolValue;
		variablesGodot[variableName] = boolValue;
		variableTypes[variableName] = typeof(bool);

		SetDirty();
	}

	public override bool TryGetValue<T>(string variableName, out T result)
	{
		// If we don't have a variable with this name, return the null
		// value
		if (variablesYarn.ContainsKey(variableName) == false)
		{
			result = default;
			return false;
		}

		var resultObject = variablesYarn[variableName];

		if (typeof(T).IsAssignableFrom(resultObject.GetType()))
		{
			result = (T)resultObject;
			return true;
		}
		else
		{
			throw new System.InvalidCastException($"Variable {variableName} exists, but is the wrong type (expected {typeof(T)}, got {resultObject.GetType()}");
		}
	}
	#endregion
}


