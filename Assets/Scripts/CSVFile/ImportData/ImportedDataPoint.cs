/* Author philipp zupke, 2015 phil@bytestyles.net */
/*
 * Datapoints are string key value pairs
 * There are conversion methods to return the Value as a specific type
 * e.g. call ImportedDataPoint.ToIntArray() if you _know_ that the string in the value property, can be converted into an int array
 * when returning an array, the value string is split with the arraysplit char
 * 
 * So if you want to store different types as value just keep in mind that all the information need to be inside of a thevalue-string,
 * and that you can then write a method to return that type
*/


using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ImportedDataPoint
{

	// This char determines how you seperate your ARRAYS in excel
	private static char arraySplitChar = '-';

	//Key Value Pair for this Data Point
	public string key;
	public string value;


	#region ConversionMethods

	public int ToInt(){
		int _val = 0;

		if(	int.TryParse( value, out _val)){
			return _val;
		} else {
			Debug.Log(value+" can not be parsed as INT!");
			return 0;
		}
	}


	public float ToFloat(){

		float _val = 0;

		if(	float.TryParse( value, out _val))
        {
			return _val;
		} else {
			Debug.Log(value+ "can not be parsed as FLOAT");
			return 0;
		}
	}
	
	public override string ToString()
    {
		return value;
	}
	
	public int[] ToIntArray()
   {
		string[] stringValues = value.Split(arraySplitChar);
		int[] intValues = new int[stringValues.Length ];
		
		for(int i = 0; i < stringValues.Length; i++)
        {
			if(!int.TryParse( stringValues[i], out intValues[i]))
            {
				Debug.Log( stringValues[i] + " can not be parsed as INT [for array]");
			}
		}
		
		return intValues;
	}
	
	public float[] ToFloatArray()
    {
		string[] _stringvalues = value.Split(arraySplitChar);
		float[] _floatvalues = new float[_stringvalues.Length ];
		
		for(int i = 0; i < _stringvalues.Length; i++){
			if(!float.TryParse( _stringvalues[i], out _floatvalues[i])){
				Debug.LogWarning(_stringvalues[i]+" can not be parsed as FLOAT [for array]");
			}
		}
		
		return _floatvalues;
	}
	
	public string [] ToStringArray() {
		return value.Split(arraySplitChar);
	}
	#endregion
}