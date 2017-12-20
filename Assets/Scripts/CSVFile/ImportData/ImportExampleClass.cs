/* Author philipp zupke, 2015 phil@bytestyles.net *
* Example class on how to use the imported data tool
* Import Data has a lazy initiliasation, hence, the data only gets loaded, created, and stored when calling GetData
* 
* In your csv/ecxel you can define "DataContainers"
* each datacontainer has a key, called the id. In each container there are N KeyValuepairs, stored as strings
* To access and use they actualy values you need to get the right datacontainer and then access the stored datapoints with their key. 
* To get the right type you need to know what values you have put into the csv/excel in the first place!
* 
* Get the datacontainer from ImportData, then get you datapoint, and then chose the apprioate "ToType" method
* If needed, just add more types to the ImportedDataPoint class
*/

using UnityEngine;
using System.Collections;

public class ImportExampleClass : MonoBehaviour 
{

	public int      mInt;
	public float    mFloat;
	public string   mString;
	public int[]    mInts;
	public float[]  mFloats;
	public string[] mStrings; 


	void Start()
    {
		LoadMyData();
	}

	public void LoadMyData()
    {
		//Get the right Container
		ImportedDataContainer my_container = ImportData.GetContainer("example_id");

		//Now access all the data that you have stored there
		// !-- Keep in mind that you need to know what _actual_ type the value-string holds -- !

		mInts    = my_container.GetData("level_caps").ToIntArray();
		mFloats  = my_container.GetData("speed_by_level").ToFloatArray();
		mString  = my_container.GetData("name").ToString();
		mFloat   = my_container.GetData("reputation").ToFloat();
		mInt     = my_container.GetData("hit_points").ToInt();
		mStrings = my_container.GetData("friends").ToStringArray();

	}
}
