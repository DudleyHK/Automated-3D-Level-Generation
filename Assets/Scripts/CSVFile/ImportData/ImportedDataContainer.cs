using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ImportedDataContainer 
{
	public string ID; 
	private List<ImportedDataPoint> data = new List<ImportedDataPoint>();

	public void AddDataPoint(ImportedDataPoint dataPoint)
    {
		data.Add(dataPoint);
	}
	
	public ImportedDataPoint GetData(string id)
    {
		for(int i = 0; i < data.Count; i++)
        {
			if(data[i].key == id) return data[i];
		}
		Debug.LogWarning("Could not find Value for Key " + id);
		return null;
	}


    public void SetData(string id, string value)
    {
        for(int i = 0; i < data.Count; i++)
        {
            if(data[i].key == id)
            {
                Debug.Log("setting key id " + id + " which has a current value of " + data[i].value + ", to " + value);
                data[i].value = value;
            }
        }
    }
}
