using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSVManager : MonoBehaviour 
{
    private static string filePath = Application.dataPath + "/CSV/Saved_Probabilities.csv";
    

    public static void SaveFile(string type, float value)
    {
        
        StreamWriter writer = new StreamWriter(filePath);
        writer.WriteLine("Water, Grass, Mud");
        
    }
}