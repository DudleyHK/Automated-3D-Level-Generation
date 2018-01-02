using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : MonoBehaviour 
{
    [SerializeField]
    private List<GameObject> generatedTiles = new List<GameObject>();
    [SerializeField]
    private List<GameObject> tilePrefabs = new List<GameObject>();
    [SerializeField]
    private GameObject firstBlockPrefab;
    [SerializeField]
    private CSVManager csvManager;
    [SerializeField]
    private int maxHeight = 10;
    [SerializeField]
    private int maxWidth  = 10;
    [SerializeField]
    private int maxDepth  = 10;
    [SerializeField]
    private bool runGenerator = false;



    private List<Directions> directions = new List<Directions>(new Directions[] { Directions.Right, Directions.Forward });




    private void Start()
    {
        csvManager = FindObjectOfType<CSVManager>();
        if(!csvManager)
            Debug.Log("ERROR: CSVManager object is not active in Scene");

        if(tilePrefabs.Count <= 0)
        {
            Debug.Log("ERROR: TilePrefabs list in GeneratorManager is empty. This list must be full to generate a tile map.");
        }
    }



    private void Update()
    {
        if(runGenerator || Input.GetKeyDown(KeyCode.G))
        {
            StartCoroutine(Run());
            runGenerator = false;
        }
    }


    private IEnumerator Run()
    {
        var index = 0;
        var counter = 0;




        /* Initialise the Generation Process */
        // Instantiate the first Tile 
        var initialTile = Instantiate(firstBlockPrefab, Vector3.zero, Quaternion.identity);

        // Add the Tile to the list of all generated tiles.
        generatedTiles.Add(initialTile);

        /*Being the Main Generation Loop*/
        for(int i = 0; i < maxHeight; i++)
        {
            for(int j = 0; j < maxWidth; j++)
            {
                for(int k = 0; k < maxDepth; k++)
                {
                    Debug.Log("MESSAGE: Grid coordinate id is " + counter);
                    var tile = generatedTiles[counter];
                    var type = tile.tag;

                    foreach(var direction in directions)
                    {
                        var rowID = type.ToString() + " " + direction.ToString();
                        var probabilities = csvManager.ProbabiltiesOfRow(rowID);

                        Debug.Log("MESSAGE: Row ID is " + rowID);

                        ///var debug_probabilities = "";
                        ///foreach(var item in probabilities)
                        ///{
                        ///    debug_probabilities += item.ToString() + ", ";
                        ///}
                        ///Debug.Log("MESSAGE: Probabilities for rowID " + rowID + " is " + debug_probabilities); 

                        var tilePrefab = NextTile(probabilities);
                        Debug.Log("MESSAGE: Next tile type is " + tilePrefab.tag);


                        Vector3 newPosition = tile.GetComponent<Renderer>().bounds.center;

                        if(direction == Directions.Forward)
                        {
                            newPosition.z += tile.GetComponent<Renderer>().bounds.size.z;
                        }
                        else if(direction == Directions.Right)
                        {
                            newPosition.x += tile.GetComponent<Renderer>().bounds.size.x;
                        }

                        var newTile = Instantiate(tilePrefab, newPosition, Quaternion.identity);
                        generatedTiles.Add(newTile);
                        index++;
                    }
                    counter++;
                }
            }
        }
        yield return true;
    }

    /// <summary>
    /// First: Get the probabilty which will be used and the index of that probability.
    ///
    /// Second: Get the 'column' type from the CSV file and use it to reference the GameObject prefab
    ///     in the prefabs list. 
    ///     
    /// This is currently only taking into account a single parent tile. 
    /// </summary>
    /// <param name="probabilties"></param>
    /// <returns></returns>
    private GameObject NextTile(List<float> probabilties)
    {
        int index = 0;
        float probability = 0f;

        /*FIRST */
        index = 0;
        foreach(var probab in probabilties)
        {
            index++;
            
            probability = probab;
            if(FlipCoin(probability))
            {
                break;
            }
        }

        /*SECOND */
        var type = csvManager.NameOfColumn(index);
        GameObject tilePrefab = null;

        foreach(var tile in tilePrefabs)
        {
            if(tile.tag == type)
                tilePrefab = tile;
        }

        if(tilePrefab == null)
        {
            Debug.Log("ERROR: tilePrefab has not been given a TilePrefab.");
        }

        return tilePrefab;
    }

    /// <summary>
    /// Pass in a probability value between 0 - 1 which is checked against
    ///    a randomly generated probability between 0 - 1.
    /// </summary>
    /// <param name="probability"></param>
    /// <returns>
    /// Returns True if the probabilty is more than the generated value. 
    /// </returns>
    private bool FlipCoin(float probability)
    {
        var value = Random.Range(0f, 1f);
        //Debug.Log("MESSAGE: Probab is " + probability);
        //Debug.Log("MESSAGE: Random value is " + value);
        if(probability > value) return true;

        return false;
    }
}
