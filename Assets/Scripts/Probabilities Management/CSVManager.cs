using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mono.Csv;


public class CSVManager : MonoBehaviour
{
    public int Rows    { get; private set; }
    public int Columns { get; private set; }
    public List<float> Totals        { get; internal set; }
    public List<float> Probabilities { get; internal set; }


    [SerializeField]
    private List<string> rowIds       = new List<string>();
    [SerializeField]
    private List<string> columnIds    = new List<string>();

    private List<List<string>> gridData = new List<List<string>>();
    private List<List<string>> probabsData = new List<List<string>>();
    private string csvPath        = "..//CTPPrototype//Assets//OutputData//";
    private string csvTotalsFile  = "Totals.csv";
    private string csvProbabsFile = "Probabilities.csv";



    private void Start()
    {
        Totals        = new List<float>();
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
        ReadTotals();
        ReadProbabilities();
    }

    private void ReadTotals()
    {
        // TODO: Change this to read in the probabilities
        gridData = CsvFileReader.ReadAll(csvPath + csvTotalsFile, System.Text.Encoding.GetEncoding("gbk"));

        //Debug.Log("REading and grid data size " + gridData.Count);
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
                    Totals.Add(result);
                }

                cellIdx++;
            }
            rowIdx++;
            cellIdx = 0;
        }
        Rows = rowIds.Count;
        Columns = columnIds.Count;
    }

    private void ReadProbabilities()
    {
        // TODO: Change this to read in the probabilities
        probabsData = CsvFileReader.ReadAll(csvPath + csvProbabsFile, System.Text.Encoding.GetEncoding("gbk"));
        foreach(var row in probabsData)
        {
            foreach(var cell in row)
            {
                float result;
                bool isNumeric = float.TryParse(cell, out result);
                if(isNumeric)
                {
                    if(result == float.NaN)
                        result = 0f;
                    Probabilities.Add(result);
                }
            }
        }
    }


   

    // TODO: This should be done in the ParseLevel script where only a Total and ID is passed in here to set the value.
    /// <summary>
    /// Iterate through the output data and get the row and column index of the passed in data.
    ///  Use these indexs to find the position in the gridData list, then convert that idx to the 
    ///  probabilities list idx.
    /// </summary>
    /// <param name="outputData"></param>
    public void SetTotalsValues(List<ArrayList> outputData)
    {
        // Debug.Log("MESSAGE: SETTING NEW PROBABILITY VALUES");
        // Debug.Log("MESSAGE: OutputData size " + outputData.Count);

        foreach(var data in outputData)
        {
            var rowName    = (string)data[0];
            var columnName = (string)data[1];
            var total      = (float) data[2];

            // Debug.Log("MESSAGE: From " + rowName + " To " + columnName + " new probability is " + probability);

            for(var i = 0; i < rowIds.Count; i++)
            {
                for(var j = 0; j < columnIds.Count; j++)
                {
                    if(columnIds[j] == columnName &&
                        rowIds[i] == rowName)
                    {
                        //Debug.Log("MESSAGE: Row " + i + " and Col " + j);

                        // Debug.Log("From " + rowName + " To " + columnName + " new probability is " + probability);
                        // Debug.Log("Row ID " + j + " and Column ID " + i);
                        //Debug.Log("MESSAGE: Columns Count " + columnIds.Count);

                        var gridIdx   = (i * columnIds.Count) + j;
                        var totalsIdx = ConvertToProbabilitiesIdx(gridIdx);

                        //Debug.Log("MESSSAGE: GridIdx is " + gridIdx + " and probabilitiesIdx is " + probabilityIdx);
                        Totals[totalsIdx] = total;
                    }
                }
            }
        }
    }


    /// <summary>
    /// Calculate the total of each row.
    /// </summary>
    /// <param name="rowID"></param>
    /// <returns></returns>
    public float RowTotal(int rowID)
    {
        if(rowID == 0)
        {
            //Debug.Log("ERROR: RowID attempting to acces top row");
            return -1f;
        }

        var rowTotal = 0f;
        var prevId = -1f; // TODO: Make the getting of data from different lists more reliable.
        for(var i = 0; i < rowIds.Count; i++)
        {
            if(i != rowID) continue;
            for(var j = 0; j < columnIds.Count; j++)
            {
                var gridIdx = (i * columnIds.Count) + j;
                var totalsIdx = ConvertToProbabilitiesIdx(gridIdx);
                if(totalsIdx == prevId)
                {
                    continue;
                }

                rowTotal += Totals[totalsIdx];
                //Debug.Log("Totals id being added " + totalsIdx);

                prevId = totalsIdx;
            }
        }
        return rowTotal;
    }


    /// <summary>
    /// Return a list of probabilties given a RowID string. 
    /// </summary>
    /// <param name="rowID"></param>
    /// <returns></returns>
    public List<float> ProbabiltiesOfRow(string rowID)
    {
        if(!rowIds.Contains(rowID))
        {
            Debug.Log("ERROR: RowID " + rowID + " string invalid");
            return null;
        }

        var probabilityList = new List<float>();

        var prevId = -1f; // TODO: Make the getting of data from different lists more reliable... ??? What is this for?
        for(var i = 0; i < rowIds.Count; i++)
        {
            if(rowIds[i] != rowID) continue;
            for(var j = 0; j < columnIds.Count; j++)
            {
                var gridIdx = (i * columnIds.Count) + j;
                var probabilitiesIdx = ConvertToProbabilitiesIdx(gridIdx);
                if(probabilitiesIdx == prevId)
                {
                    continue;
                }

                probabilityList.Add(Probabilities[probabilitiesIdx]);
                prevId = probabilitiesIdx;
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
    /// Count up until the idx is hit. If the probabilitiesIdx has been 
    ///     incremented add one to bring it to counter the starting at -1.
    /// </summary>
    /// <param name="idx"></param>
    /// <returns></returns>
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



    private void Write()
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
        int rowIdx = 0;
        int cellIdx = 0;
        int totalsIdx = 0;

        foreach(var row in gridData)
        {
            foreach(var cell in row)
            {
                float result;
                bool isNumeric = float.TryParse(cell, out result);
                if(isNumeric)
                {
                    var total = Totals[totalsIdx];
                    if(total == float.NaN) 
                        total = 0f;

                    gridData[rowIdx][cellIdx] = total.ToString();
                    //Debug.Log("Probability idx is " + probabilityIdx + " and gridDataIdx is " + ((cellIdx * row.Count) + rowIdx));
                    totalsIdx++;
                }
                cellIdx++;
            }
            rowIdx++;
            cellIdx = 0;
        }
        CsvFileWriter.WriteAll(gridData, csvPath + csvTotalsFile, System.Text.Encoding.GetEncoding("gbk"));
    }


    /// <summary>
    /// Run through each row calculating the row total and using it as the demoninator 
    ///     for each cell probability value. 
    /// </summary>
    private void WriteProbabilities()
    {
        int rowIdx = 0;
        int cellIdx = 0;
        int probabsIdx = 0;

        foreach(var row in gridData)
        {
            foreach(var cell in row)
            {
                float result;
                bool isNumeric = float.TryParse(cell, out result);
                if(isNumeric)
                {
                   // Debug.Log("Probability idx is " + probabsIdx + " has a value of " + Probabilities[probabsIdx].ToString());
                   
                    if(Probabilities[probabsIdx] == float.NaN)
                    {
                        Probabilities[probabsIdx] = 0f;
                    }


                     
                       

                    gridData[rowIdx][cellIdx] = Probabilities[probabsIdx].ToString();
                    //Debug.Log("Probability idx is " + probabsIdx + " has a value of " + Probabilities[probabsIdx].ToString());
                    probabsIdx++;
                }
                cellIdx++;
            }
            rowIdx++;
            cellIdx = 0;
        }
        CsvFileWriter.WriteAll(gridData, csvPath + csvProbabsFile, System.Text.Encoding.GetEncoding("gbk"));
    }


    

    private void OnApplicationQuit()
    {
        Write();
    }
}