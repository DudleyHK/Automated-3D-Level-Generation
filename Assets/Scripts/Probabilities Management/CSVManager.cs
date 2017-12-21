using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mono.Csv;


public class CSVManager : MonoBehaviour
{
    [SerializeField]
    private List<float> probabilities = new List<float>();
    [SerializeField]
    private List<string> rowIds   = new List<string>();
    [SerializeField]
    private List<string> columnIds = new List<string>();

    private List<List<string>> gridData = new List<List<string>>();
    private string csvPath = "..//CTPPrototype//Assets//OutputData//";
    private string csvFile = "Test.csv";



    private void Start()
    {
        Read();
    }

    private void Read()
    {
        gridData = CsvFileReader.ReadAll(csvPath + csvFile, System.Text.Encoding.GetEncoding("gbk"));

        Debug.Log("REading and grid data size " + gridData.Count);
        int rowIdx = 0;
        int cellIdx = 0;
        foreach(var row in gridData)
        {
            foreach(var cell in row)
            {
                if(rowIdx == 0)
                {
                    columnIds.Add(cell);
                }

                if(cellIdx == 0)
                {
                    rowIds.Add(cell);
                }
                
                float result;
                bool isNumeric = float.TryParse(cell, out result);
                if(isNumeric)
                {
                    probabilities.Add(result);
                }

                cellIdx++;
            }
            rowIdx++;
            cellIdx = 0;
        }
    }


    private void Write()
    {
        int rowIdx = 0;
        int cellIdx = 0;
        int probabilityIdx = 0;


        foreach(var row in gridData)
        {
            foreach(var cell in row)
            {
                float result;
                bool isNumeric = float.TryParse(cell, out result);
                if(isNumeric)
                {
                    gridData[rowIdx][cellIdx] = probabilities[probabilityIdx].ToString();
                    //Debug.Log("Probability idx is " + probabilityIdx + " and gridDataIdx is " + ((cellIdx * row.Count) + rowIdx));
                    probabilityIdx++;
                }
                cellIdx++;
            }
            rowIdx++;
            cellIdx = 0;
        }
        CsvFileWriter.WriteAll(gridData, csvPath + csvFile, System.Text.Encoding.GetEncoding("gbk"));
    }


    /// <summary>
    /// Iterate through the output data and get the row and column index of the passed in data.
    ///  Use these indexs to find the position in the gridData list, then convert that idx to the 
    ///  probabilities list idx.
    /// </summary>
    /// <param name="outputData"></param>
    public void SetProbabilityValues(List<ArrayList> outputData)
    {
        Debug.Log("SETTING NEW PROBABILITY VALUES");
        Debug.Log("OutputData size " + outputData.Count);

        foreach(var data in outputData)
        {
            var rowName = (string)data[0];
            var columnName = (string)data[1];
            var probability = (float)data[2];

            Debug.Log("From " + rowName + " To " + columnName + " new probability is " + probability);

            for(var i = 0; i < rowIds.Count; i++)
            {
                for(var j = 0; j < columnIds.Count; j++)
                {
                    if(columnIds[j] == columnName &&
                        rowIds[i] == rowName)
                    {
                        Debug.Log("Row " + i + " and Col " + j);

                        // Debug.Log("From " + rowName + " To " + columnName + " new probability is " + probability);
                        // Debug.Log("Row ID " + j + " and Column ID " + i);
                        Debug.Log("Columns Count " + columnIds.Count);

                        var gridIdx = (i * columnIds.Count) + j;
                        var probabilityIdx = ConvertToProbabilitiesIdx(gridIdx);

                        Debug.Log("GridIdx is " + gridIdx + " and probabilitiesIdx is " + probabilityIdx);
                        probabilities[probabilityIdx] = probability;
                    }
                }
            }
        }
    }

    private int ConvertToProbabilitiesIdx(int idx)
    {
        int counter = 0;
        int probabilityIdx = 0;

        foreach(var row in gridData)
        {
            foreach(var cell in row)
            {
                if(idx == counter)
                {
                    break;
                }

                float result;
                bool isNumeric = float.TryParse(cell, out result);
                if(isNumeric)
                {
                    probabilityIdx++;
                }
                counter++;
            }
        }
        return probabilityIdx;
    }
    

    private void OnApplicationQuit()
    {
        Write();
    }
}