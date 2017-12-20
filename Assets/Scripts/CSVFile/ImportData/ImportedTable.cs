/* Author philipp zupke, 2015 phil@bytestyles.net */
/*
Converts a CSV file into a 2D String array
*/

using UnityEngine;
using System.Collections;

public class ImportedTable 
{

	string[,] m_table;
	TablePointer currentPointer;
	static char splitCharCol = ',';
	static char splitCharRow = '\n';

	static int COLS;
	static int ROWS;

	public ImportedTable(string[,] table)
    {
		m_table = table;
		currentPointer = new TablePointer();
	}

	
	public class TablePointer
    {
		public int col = 0;
		public int row = 0;
		
		public override string ToString ()
		{
			return ("col:" + col + " | row:" + row);
		}
	}

	public void ResetPointer()
    {
		currentPointer.row = 0;
		currentPointer.col = 0;
	}

	//if possible, set the pointer to the next row
	public bool SetNextRow(bool resetCol){

		int targetCol = resetCol ? 0 : currentPointer.col;

		int tagetRow = currentPointer.row+1;

		if(IsValid( targetCol, tagetRow))
        {
			currentPointer.col = targetCol;
			currentPointer.row = tagetRow;
		
			return true;
		}
	
		return false;
	}

	//searches for the next header [a value in the first row of the table, and then sets the pointer to that cell]
	public bool SetNextHeader(){
		currentPointer.row = 0;

		for(int i = currentPointer.col+1; i < m_table.GetLength(0)-1 ; i++){

			if( !string.IsNullOrEmpty( m_table[i,0] )){
				currentPointer.col = i;
			

				return true;
			}
		}
	
		return false;
	}

	//Load the current pointer-string as the KEY, then advance +1 coloumn and get that string as a VALUE
	//note that the pointer is reset to the starting col after the value has been loaded
	//the pointer will be moved from the outside to advance
	public string[] GetKeyValuePair(){
	

		string[] pair = new string[2];

		pair[0] = GetStringFromPointer();
		currentPointer.col++;
		pair[1] = GetStringFromPointer();

		currentPointer.col--;

		if( string.IsNullOrEmpty( pair[0]) || string.IsNullOrEmpty( pair[1]) ){
			Debug.LogWarning("Couldnt Parse Key Value Pair at "+currentPointer.ToString());
			return null;
		}

		return pair;
		 
	}

	//returns if there is a cell at a specific row or col and if it contains a value
	bool IsValid(int c, int r){

		if( c >= COLS) return false;
		if( r >= ROWS) return false;

		if( string.IsNullOrEmpty( m_table[c,r] )) return false;

		return true;
	}

	//returns the string from current pointers position
	public string GetStringFromPointer(){

		if( IsValid(currentPointer.col, currentPointer.row)){
			return m_table[currentPointer.col,currentPointer.row];
		} 
		return null;
	}


	//loads a text asset and converts into a 2D-String array (ImportedTable)
	public static ImportedTable LoadFromFile(string fileName)
    {
		TextAsset textFile = (TextAsset) Resources.Load(fileName);

		if(textFile == null)
        {
			Debug.LogWarning(fileName + "  FILE NOT FOUND");
			return null;
		}

		Debug.Log("- Loading File: " + fileName);

		string[] rows = textFile.text.Split(splitCharRow);

		 COLS = GetLongest(rows);
		 ROWS = rows.Length;

		string[,] table = new string[COLS, ROWS];

		for(int r = 0; r < ROWS; r++){
	
			for(int c = 0; c < COLS; c++){

				string field = "";

				if(c < rows[r].Split(splitCharCol).Length)
                {
					field = rows[r].Split(splitCharCol)[c]; 
				}
				table[c, r] = field;
			}
		} 
		
		if(string.IsNullOrEmpty(table[0,0]))
        {
			Debug.LogWarning("First Cell is Empty, Importdata will not work");
			return null;
		}
		return new ImportedTable(table); 
	}

	//Finds the row with the most columns
	static int GetLongest(string[] arr)
	{
		int longest = 0;
		for (int i = 0; i < arr.Length; i++) {

			if(arr[i].Split(splitCharCol).Length > longest){

				longest = arr[i].Split(splitCharCol).Length;
			}
		}
		return longest;
	}
}
