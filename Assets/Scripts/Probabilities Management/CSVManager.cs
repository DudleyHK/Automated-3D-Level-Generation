using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mono.Csv;


public class CSVManager : MonoBehaviour
{
    public List<int> Totals          { get; internal set; }
    public List<float> Probabilities { get; internal set; }


    [SerializeField]
    private List<string> rowIds       = new List<string>();
    [SerializeField]
    private List<string> columnIds    = new List<string>();

    private List<List<string>> gridData = new List<List<string>>();
    private List<List<string>> probabilityData = new List<List<string>>();
    private string csvPath        = "..//CTPPrototype//Assets//OutputData//";
    private string csvTotalsFile  = "Totals.csv";
    private string csvProbabsFile = "Probabilities.csv";



    private void Start()
    {
        Totals        = new List<int>();
        Probabilities = new List<float>();

        Read();
    }


    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.W))
        {
            Write();
        }
    }

    private void Read()
    {
        InitRead();
        ReadTotals();
        ReadProbabilities();
    }

    private void InitRead()
    {
        var dataFile = CsvFileReader.ReadAll(csvPath + csvTotalsFile, System.Text.Encoding.GetEncoding("gbk"));

        for(int i = 0; i < gridData.Count; i++)
        {
            for(int j = 0; j < gridData[i].Count; j++)
            {
                var value = gridData[i][j];
                if(i == 0)
                {
                    columnIds.Add(value);
                }

                if(j % gridData.Count == 0)
                {
                    rowIds.Add(value);
                }
            }
        }
    }

    private void ReadTotals()
    {
        gridData = CsvFileReader.ReadAll(csvPath + csvTotalsFile, System.Text.Encoding.GetEncoding("gbk"));

        for(int i = 0; i < gridData.Count; i++)
        {
            for(int j = 0; j < gridData[i].Count; j++)
            {
                var value = gridData[i][j];
                if(i == 0)
                {
                    columnIds.Add(value);
                }
                
                if(j % gridData.Count == 0)
                {
                    rowIds.Add(value);
                }

                if(IsNumeric(value))
                {
                    Totals.Add(int.Parse(value));
                }
            }
        }
    }

    private void ReadProbabilities()
    {
        probabilityData = CsvFileReader.ReadAll(csvPath + csvProbabsFile, System.Text.Encoding.GetEncoding("gbk"));
        for(int i = 0; i < gridData.Count; i++)
        {
            for(int j = 0; j < gridData[i].Count; j++)
            {
                var value = gridData[i][j];
                if(IsNumeric(value))
                {
                    Probabilities.Add(float.Parse(value));
                }
            }
        }
    }


   

    /// <summary>
    /// Iterate through the output data and get the row and column index of the passed in data.
    ///  Use these indexs to find the position in the gridData list, then convert that idx to the 
    ///  probabilities list idx.
    /// </summary>
    /// <param name="outputData"></param>
    public void SetTotalsValues(List<ArrayList> outputData)
    {
        // Debug.Log("MESSAGE: SETTING NEW PROBABILITY VALUES");
         //Debug.Log("MESSAGE: OutputData size " + outputData.Count);

        // Clear first.
        for(int i = 0; i < Totals.Count; i++)
        {
            Totals[i] = 0;
            Probabilities[i] = 0;
        }


        foreach(var data in outputData)
        {
            var rowName    = (string)data[0];
            var columnName = (string)data[1];
            var total      = (int)   data[2];



            var rowID = rowIds.FindIndex(n => 
            { 
                return n == rowName;  
            });
            var colID = columnIds.FindIndex(n =>
            {
                return n == columnName;
            });
            
            //Debug.Log("MESSAGE: From " + rowName + " To " + columnName + " new probability is " + total);
            //Debug.Log("MESSAGE: RowID " + rowID + " colID " + colID + " total " + total);

            var listID = ConvertToListID(rowID, colID);

            Totals[listID] = total;
        }
    }


    /// <summary>
    /// Calculate the total of each row.
    /// </summary>
    /// <param name="rowID"></param>
    /// <returns></returns>
    public int RowTotal(int rowID)
    {
        var rowTotal = 0;
        for(var i = 0; i < rowIds.Count; i++)
        {
            if(i != rowID) continue;
            for(var j = 0; j < columnIds.Count; j++)
            {
                if(j == 0) continue;
                
                var listID = ConvertToListID(i, j);
                if(listID >= 0)
                    rowTotal += Totals[listID];
            }
        }
        //Debug.Log("Row Total " + rowTotal + " for row " + rowID);
        return rowTotal;
    }



    /// <summary>
    /// Return a list of probabilties given a RowID string. 
    /// </summary>
    /// <param name="rowID"></param>
    /// <returns></returns>
    public List<float> RowProbabilities(string rowID)
    {
        if(!rowIds.Contains(rowID))
        {
           // Debug.Log("ERROR: RowID " + rowID + " string invalid");
            return null;
        }

        var probabilityList = new List<float>();

        for(var i = 0; i < rowIds.Count; i++)
        {
            if(rowIds[i] != rowID) continue;
            for(var j = 0; j < columnIds.Count; j++)
            {
                if(j == 0) continue;

                var listID = ConvertToListID(i, j);
                probabilityList.Add(Probabilities[listID]);
            }
        }
        return probabilityList;
    }


    /// <summary>
    /// Get the name of a column given an ID. 
    /// </summary>
    /// <param name="columnID"></param>
    /// <returns></returns>
    public string NameOfColumn(int id)
    {
        if(id > columnIds.Count)
        {
            Debug.Log("ERROR: ColumnID passed to CSVManager is more than the amount of IDs in the list.");
            return "";
        }

        return columnIds[id];
    }



    /// <summary>
    /// Convert Row and Col ID to Csv file Index.
    /// </summary>
    /// <param name="idx"></param>
    /// <returns></returns>
    private int ConvertToListID(int rowID, int colID)
    { 
        var csvID = (rowID * columnIds.Count) + colID;
       //Debug.Log("MESSAGE: Csv - CsvID  " + csvID);

        colID -= 1;
        rowID -= 1;

        var listID = (rowID * (columnIds.Count - 1)) + colID;
       //Debug.Log("MESSAGE: List - ListID " + listID);

        if(listID < 0)
        {
            Debug.Log("WARNING: ListID " + listID + " is less than 0");
            return -1;
        }

        ConvertToCsvID(rowID, colID);

        return listID;
    }

    /// <summary>
    /// Convert Row and Col ID to C# List Index.
    /// </summary>
    /// <param name="rowID"></param>
    /// <param name="colID"></param>
    /// <returns></returns>
    private int ConvertToCsvID(int rowID, int colID)
    {
        var listID = (rowID * (columnIds.Count - 1)) + colID;
        //Debug.Log("MESSAGE: Csv - ListID " + listID);

        colID += 1;
        rowID += 1;

        var csvID = (rowID * columnIds.Count) + colID;
        //Debug.Log("MESSAGE: Csv - CsvID " + csvID);

        if(csvID < 0)
        {
            Debug.Log("WARNING: CsvID " + csvID + " is less than 0");
            return -1;
        }

        return csvID;


    }



    public void Write()
    {
        if(Totals.Count > 0)
        {
            WriteTotals();
            Debug.Log("MESSAGE: CSVManager Written Totals");
        }

        if(Probabilities.Count > 0)
        {
            WriteProbabilities();
            Debug.Log("MESSAGE: CSVManager Written Probabilities");
        }

    }


    private void WriteTotals()
    {
        for(int i = 0; i < rowIds.Count; i++)
        {
            for(int j = 0; j < columnIds.Count; j++)
            {
                var value = gridData[i][j];

                if(IsNumeric(value))
                { 
                    var listID = ConvertToListID(i, j);
                    var total = Totals[listID];

                    gridData[i][j] = total.ToString();
                }
            }
        }
        CsvFileWriter.WriteAll(gridData, csvPath + csvTotalsFile, System.Text.Encoding.GetEncoding("gbk"));
    }




    /// <summary>
    /// Run through each row calculating the row total and using it as the demoninator 
    ///     for each cell probability value. 
    /// </summary>
    private void WriteProbabilities()
    {
        for(int i = 0; i < rowIds.Count; i++)
        {
            for(int j = 0; j < columnIds.Count; j++)
            {
                var value = gridData[i][j];

                if(IsNumeric(value))
                {
                    var listID = ConvertToListID(i, j);
                    var probab = Probabilities[listID];

                    gridData[i][j] = probab.ToString();
                }
            }
        }
        CsvFileWriter.WriteAll(gridData, csvPath + csvProbabsFile, System.Text.Encoding.GetEncoding("gbk"));
    }


    /// <summary>
    /// Wrapper function for the float IsNumberic function.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private bool IsNumeric(string value)
    {
        float result;
        bool isNumeric = float.TryParse(value, out result);
        if(isNumeric)
        {
            return true;
        }
        return false;
    }

   
    /// <summary>
    /// Set the probabilities values for a specific row.
    /// </summary>
    /// <param name="rowID"></param>
    /// <param name="rowTotal"></param>
    public void SetProbabilityValue(int rowID, int rowTotal)
    {
        if(rowTotal == 0) return;

        for(var i = 0; i < rowIds.Count; i++)
        {
            if(i != rowID) continue;
            for(var j = 0; j < columnIds.Count; j++)
            {
                var listID = ConvertToListID(i, j);
                //Debug.Log("Row " + rowID + " listID " + listID);
                if(listID < 0) continue;
                
                var total = Totals[listID];
                if(total != 0)
                {
                    Probabilities[listID] = total / (float)rowTotal;
                } 
            }
        }
    }

    /// <summary>
    /// Return a list of all of the totals for each row. 
    /// </summary>
    /// <returns></returns>
    public List<int> GetRowTotals()
    {
        var rowTotals = new List<int>();
        for(int i = 0; i < rowIds.Count; i++)
        {
            //if(i == 0) continue;
            rowTotals.Add(RowTotal(i));
        }
        return rowTotals;
    }
}