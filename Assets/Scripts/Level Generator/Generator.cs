using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : MonoBehaviour 
{
    private enum TileChars
    {
        M,
        G,
        WT,
        WL,
        A
    }
    [SerializeField]
    private List<GameObject> generatedLevels = new List<GameObject>();
    [SerializeField]
    private List<GameObject> generatedTiles = new List<GameObject>();
    [SerializeField]
    private List<GameObject> tilePrefabs = new List<GameObject>();
    [SerializeField]
    private List<KeyValuePair<string, List<float>>> textLevel;
    [SerializeField]
    private List<Material> materials = new List<Material>();
    [SerializeField]
    private GameObject firstBlockPrefab;
    [SerializeField]
    private GameObject layerPrefab;
    [SerializeField]
    private GameObject generatedLevelPrefab;
    [SerializeField]
    private CSVManager csvManager;
    [SerializeField]
    private TileChars initialTileChar = TileChars.M;
    [SerializeField]
    private string textLevelOutput = "TextLevel.txt";
    [SerializeField]
    private int maxHeight = 10;
    [SerializeField]
    private int maxWidth  = 10;
    [SerializeField]
    private int maxDepth  = 10;
    [SerializeField]
    private bool runGenerator = false;

    public static int Height;
    public static int Width;
    public static int Depth;



    private List<Directions> directions = new List<Directions>(new Directions[] { Directions.Right, Directions.Forward });


    private void OnEnable()
    {
        GUIManager.typeSliderEvent += ChangeInitialTile;
        GUIManager.dimentionEvent += ChangeDimentions;
        GUIManager.generateEvent += Generate;
        GUIManager.changeLevelDisplay += DeactivateLastLevel;
    }

    private void OnDisable()
    {
        GUIManager.typeSliderEvent -= ChangeInitialTile;
        GUIManager.dimentionEvent -= ChangeDimentions;
        GUIManager.generateEvent -= Generate;
        GUIManager.changeLevelDisplay -= DeactivateLastLevel;
    }



    private string ChangeInitialTile(int value)
    {
        if(!System.Enum.IsDefined(typeof(TileChars), (TileChars)value)) 
            return "ERROR: Invalid value parsed";

        initialTileChar = ((TileChars)value);
        var type = ConvertToType(((TileChars)value).ToString());


        return type;
    }


    private void ChangeDimentions(string axis, int value)
    {
        if(value == 0) value = 1;

        if(axis == "Height")
        {
            maxHeight = value;
        }
        else if (axis == "Width")
        {
            maxWidth = value;
        }
        else if(axis == "Depth")
        {
            maxDepth = value;
        }
        else
        {
            Debug.Log("ERROR: Invalid axis " + axis + " passed");
        }
    }



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
        Height = maxHeight;
        Width  = maxWidth;
        Depth  = maxDepth;


        if(runGenerator || Input.GetKeyDown(KeyCode.G))
        {
           Generate();
        }

        if(Input.GetKeyDown(KeyCode.B))
        {
            if(textLevel != null)
            {
                StopAllCoroutines();
                StartCoroutine(Build());
            }
        }
    }
     
    private void Generate()
    {
        DeactivateLastLevel();

        textLevel = new List<KeyValuePair<string, List<float>>>();
        StartCoroutine(Run(flag =>
        {
            if(flag)
                StartCoroutine(Build());
        }));
        runGenerator = false;
    }


    private void DeactivateLastLevel()
    {
        if(generatedLevels.Count > 0)
            generatedLevels[generatedLevels.Count - 1].SetActive(false);
    }

    private IEnumerator Run(System.Action<bool> complete)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        complete(false);

        InitialiseStringLevel();
        GenerateStringLevel();
        PrintTextLevel();



        stopwatch.Stop();
        Debug.Log("Time taken to generate level " + stopwatch.Elapsed);

        complete(true);
        yield return true;
    }

    /// <summary>
    /// Use the textLevel as the blueprint to build the level out of equally sized tiles. 
    /// </summary>
    /// <returns></returns>
    private IEnumerator Build()
    {
        var tileSize = tilePrefabs[0].GetComponent<Renderer>().bounds.size;
        var initialPosition = tilePrefabs[0].GetComponent<Renderer>().bounds.center;
        var spawnPosition = Vector3.zero;

        var generatedLevelClone = Instantiate(generatedLevelPrefab, initialPosition, Quaternion.identity);

        for(int i = 0; i < maxHeight; i++)
        {
            var layer = Instantiate(layerPrefab, initialPosition, Quaternion.identity, generatedLevelClone.transform);
            for(int j = 0; j < maxDepth; j++)
            {
                for(int k = 0; k < maxWidth; k++)
                {
                    var index = (i * maxHeight + j) * maxWidth + k;

                    if(index % maxWidth == 0)
                    {
                        spawnPosition.x = initialPosition.x;
                    }
                    else
                    {
                        spawnPosition.x += tileSize.x;
                    }

                    var type = ConvertToType(textLevel[index].Key);
                    var tilePrefab = TilePrefab(type);

                    var clone = Instantiate(tilePrefab, spawnPosition, Quaternion.identity, layer.transform);

                    // generatedTiles.Add(clone);
                }
                spawnPosition.z += tileSize.z;
                spawnPosition.x = initialPosition.x;
            }
            spawnPosition.y += tileSize.y;
            spawnPosition.z = initialPosition.z;
        }

        generatedLevels.Add(generatedLevelClone);
        yield return true;
    }

    /// <summary>
    /// Initialise all positions to '*'
    /// </summary>
    private void InitialiseStringLevel()
    {
        /*Being the Main Generation Loop*/
        for(int i = 0; i < (maxHeight * maxDepth * maxWidth); i++)
        {
            textLevel.Add(new KeyValuePair<string, List<float>>("*", null));
        }
    }


    /// <summary>
    /// The way index is taken is shuffled due to using maxHeight where most people use width. 
    /// </summary>
    private void GenerateStringLevel()
    {
        Debug.Log("textLevel size " + textLevel.Count);

        /*Being the Main Generation Loop*/
        for(int i = 0; i < maxHeight; i++)
        {
            for(int j = 0; j < maxDepth; j++)
            {
                var baseIndex = (i * maxHeight + j);
                
                for(int k = 0; k < maxWidth; k++)
                {
                    var index = baseIndex * maxWidth + k;

                    if(index == 0)
                        AddTextTile(initialTileChar.ToString(), index);


                    //Debug.Log("i " + i);
                    //Debug.Log("maxHeight " + maxHeight);
                    //Debug.Log(" j " + j);
                    //Debug.Log("maxWidth " + maxWidth);
                    //Debug.Log(" k " + k);
                    //
                    //Debug.Log("Index: " + index);
                    WriteTextLevel(index);

                }
            }
        }
    }


    private void WriteTextLevel(int index)
    {
        var standardProbabilities = default(List<float>);
        var tileChar = textLevel[index].Key;
        var direction = "None";
        var tileType = "None";

        var upIndex = GetGridUp(index);
        var forwardIndex = GetGridForward(index);
        var rightIndex = GetGridRight(index);


        if(tileChar == "*") return;


        if(rightIndex >= 0)
        {
            direction = "Right";
            tileType = ConvertToType(tileChar) + " " + direction;
            standardProbabilities = csvManager.RowProbabilities(tileType);
            var newTileChar = NextTextTile(standardProbabilities);
            
            Debug.Log("MESSAGE: Adding a tile " + newTileChar + " ," + tileType);
            AddTextTile(newTileChar, rightIndex, direction);

        }
        if(upIndex >= 0)
        {
            direction = "Up";
            tileType = ConvertToType(tileChar) + " " + direction;
            standardProbabilities = csvManager.RowProbabilities(tileType);
            var newTileChar = NextTextTile(standardProbabilities);

            //Debug.Log("MESSAGE: Adding a tile " + newTileChar + " ," + tileType);
            AddTextTile(newTileChar, upIndex, direction);
        }
        if(forwardIndex >= 0)
        {
            direction = "Forward";
            tileType = ConvertToType(tileChar) + " " + direction;
            standardProbabilities = csvManager.RowProbabilities(tileType);
            var newTileChar = NextTextTile(standardProbabilities);

            //Debug.Log("MESSAGE: Adding a tile " + newTileChar + " ," + tileType);
            AddTextTile(newTileChar, forwardIndex, direction);
        }
    }

    // TODO: Update the tileChar after the 
    /// <summary>
    /// Adds a new text tile to the list. If space being added to is already occupied 
    /// this function will update the transition matrix for that space.
    /// </summary>
    /// <param name="tileChar"></param>
    /// <param name="direction"></param>
    private void AddTextTile(string tileChar, int index, string direction = "None")
    {
        var probabilitiesList = default(List<float>);

        if(textLevel[index].Key != "*")
        {
            // Update the probabilities matrix for the tile. 
            var tileType = ConvertToType(tileChar) + " " + direction;
            probabilitiesList = csvManager.RowProbabilities(tileType);

            if(probabilitiesList == null)
            {
                Debug.Log("ERROR: Probabilities has returned null");
                return;
            }

            //Debug.Log("MESSAGE: Updating the probabilty list of index " + index + "(tileChar "  + tileChar + ")");
            for(int i = 0; i < textLevel[index].Value.Count; i++)
            {
                var value = textLevel[index].Value[i];
                var probabilty = probabilitiesList[i];

                if(probabilty == float.NaN || value == float.NaN)
                {
                    continue;
                }
                else if(value == 0 && probabilty == 0)
                {
                    continue;
                }
                else if(value > 0 && probabilty == 0)
                {
                    continue;
                }
                else if(value == 0 && probabilty > 0)
                {
                    value = probabilty;
                }
                else
                {
                    value *= probabilty;
                }

                textLevel[index].Value[i] = value;
            }

            tileChar = NextTextTile(textLevel[index].Value);
            //Debug.Log("MESSAGE: Updating to " + tileChar);
        }
        // Set a basic probabilties matrix for the tile. 
        else
        {
            if(direction != "None")
            {
                var tileType = ConvertToType(tileChar) + " " + direction;
                probabilitiesList = csvManager.RowProbabilities(tileType);

                if(probabilitiesList == null)
                {
                    Debug.Log("ERROR: Probabilities has returned null");
                    return;
                }
            }
            else
            {
                //Debug.Log("MESSAGE: TextTile is being initialised without a direction");
                probabilitiesList = null;
            }
            textLevel[index] = new KeyValuePair<string, List<float>>(tileChar, probabilitiesList);
        }
    }

    /// <summary>
    /// Pass in a list of probabilities and return a Tile Character.
    /// </summary>
    /// <param name="probabilties"></param>
    /// <returns></returns>
    private string NextTextTile(List<float> probabilities)
    {
        var index = GetIndexOfProbability(probabilities);

       // Debug.Log("Index " + index); //+ " gets tileChar " + type);

        var type = csvManager.NameOfColumn(index);
        var tileChar = ConvertToChar(type);




        return tileChar;
    }

    private int GetIndexOfProbability(List<float> probabilities)
    {
        int counter = 0;
        int listID = 0;
        
        while(counter < 100)
        {
            if(listID >= probabilities.Count)
                listID = 0;


            if(FlipCoin(probabilities[listID++]))
            {
                break;
            }
            counter++;
        }
        return listID;
    }

    private string ConvertToType(string tileChar)
    {
        var type = "";
        
        if(tileChar == "M")
        {
            type = "Mud";
        }
        else if(tileChar == "G")
        {
            type = "Grass";
        }
        else if(tileChar == "WT")
        {
            type = "Water";
        }
        else if(tileChar == "WL")
        {
            type = "Wall";
        }
        else if(tileChar == "A")
        {
            type = "Air";
        }
        else
        {
            Debug.Log("ERROR: Invalid tileChar " + tileChar +  " passed to type converter");
        }
        
        return type;
    }

    private string ConvertToChar(string type)
    {
        var tileChar = "";

        if(type == "Mud")
        {
            tileChar = "M";
        }
        else if(type == "Grass")
        {
            tileChar = "G";
        }
        else if(type == "Water")
        {
            tileChar = "WT";
        }
        else if(type == "Wall")
        {
            tileChar = "WL";
        }
        else if(type == "Air")
        {
            tileChar = "A";
        }
        else
        {
            Debug.Log("ERROR: Invalid type " + type +  " passed to tileChar converter");
        }

        return tileChar;
    }


    /// <summary>
    /// Print the textLevel to a text file. 
    /// </summary>
    private void PrintTextLevel()
    {
        System.IO.StreamWriter streamWriter = new System.IO.StreamWriter(Application.dataPath + "/OutputData/" + textLevelOutput);
       // Debug.Log("MESSAGE: " + Application.dataPath + "/OutputData/" + textLevelOutput);

        streamWriter.Flush();
        for(int i = 0; i < maxHeight; i++)
        {
            if(i == 0)
            {
                streamWriter.WriteLine("Bottom Layer");
            }
            else if(i == (maxHeight - 1))
            {
                streamWriter.WriteLine("Top Layer");
            }
            else
            {
                streamWriter.WriteLine("Layer " + i);
            }

            for(int j = 0; j < maxDepth; j++)
            {
                var index2D = (i * maxHeight + j);
                var layerString = "";

                for(int k = 0; k < maxWidth; k++)
                {
                    var index3D = index2D * maxWidth + k;
                    var tile = textLevel[index3D].Key;

                    layerString += tile;
                    layerString += " ";
                }
                streamWriter.WriteLine(layerString);
            }
            streamWriter.WriteLine("\n");
        }

        streamWriter.Close();
    }


    private void ReadTextLevel(int index3D)
    {
        var upIndex = GetGridUp(index3D);
        var forwardIndex = GetGridForward(index3D);
        var rightIndex = GetGridRight(index3D);

        Debug.Log("Index " + index3D + " tile " + textLevel[index3D].Key);

        if(rightIndex >= 0)
        {
            Debug.Log("Right tile " + textLevel[rightIndex].Key);
        }
        if(upIndex >= 0)
        {
            Debug.Log("Up tile " + textLevel[upIndex].Key);
        }
        if(forwardIndex >= 0)
        {
            Debug.Log("Forward tile " + textLevel[forwardIndex].Key);
        }
    }


    private int GetGridUp(int index)
    {
        var nextIndex = index + (maxWidth * maxDepth);

        if(!SanityCheckIndex(index))
            return -1;

        if(!SanityCheckIndex(nextIndex))
        {
           // Debug.Log("INVALID: NextIndex is invalid");
            return -1;
        }


        return nextIndex;
    }


    private int GetGridRight(int index)
    {
        var nextIndex = (index + 1);

        if(!SanityCheckIndex(index))
            return -1;

        if(!SanityCheckIndex(nextIndex))
            return -1;


        var x = 0;
        var y = 0;

        var currentZ = 0;
        var nextZ = 0;

        if(!GetIndexCoordinates(index, out x, out y, out currentZ))
        {
            Debug.Log("ERROR: GetIndexCoordinates is returning false");
        }

        if(!GetIndexCoordinates(nextIndex, out x, out y, out nextZ))
        {
            Debug.Log("ERROR: GetIndexCoordinates is returning false");
        }

        if(currentZ != nextZ)
            return -1;


        /// TODO: Calculate this for x
        ///var currentX = index % maxWidth;
        ///var nextX = nextIndex % maxWidth;
        ///if(nextX != currentX)
        ///    return -1;



        return nextIndex;
    }


    private int GetGridForward(int index)
    {
        var nextIndex = (index + maxWidth);

        if(!SanityCheckIndex(index))
            return -1;

        if(!SanityCheckIndex(nextIndex))
            return -1;

        var currentY = index / (maxWidth * maxDepth);
        var nextY = nextIndex / (maxWidth * maxDepth);

        if(currentY != nextY)
            return -1;
        
        return nextIndex;
    }


    /// <summary>
    /// Pass in a 3D grid index and get all of the coordinates back. 
    /// TODO: Update this so it uses the standard way of getting cooridnates which i cant
    ///     work out at the moment. 
    /// </summary>
    /// <param name="index"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    private bool GetIndexCoordinates(int index, out int x, out int y, out int z)
    {
        x = -1;
        y = -1;
        z = -1;


        if(!SanityCheckIndex(index))
        {
            return false;
        }

        for(int i = 0; i < maxHeight; i++)
        {
            for(int j = 0; j < maxDepth; j++)
            {
                var index2D = (j * maxHeight + j);
                for(int k = 0; k < maxWidth; k++)
                {
                    var flatID = index2D * maxWidth + k;
                    if(flatID == index)
                    {
                        x = k;
                        y = i;
                        z = j;
                    }
                }
            }
        }
        return true;
    }


    private bool SanityCheckIndex(int index)
    {
        //Debug.Log("MESSAGE: Sanity checking " + index);

        if(index >= (maxWidth * maxHeight * maxDepth))
        {
            //Debug.Log("ERROR: Index is more than array size");
            return false;
        }

        if(index < 0)
        {
            //Debug.Log("ERROR: Index is less than zero");
            return false;
        }

        return true;
    }


    private bool ObjectAtPosition(Vector3 position, out GameObject tile)
    {
        GameObject outTile = null;
        tile = null;
        
        var objectAtPosition = generatedTiles.Exists(obj =>
        {
            outTile = obj;
            return (obj.GetComponent<Renderer>().bounds.center == position);
        });

        if(objectAtPosition) 
            tile = outTile;

        return objectAtPosition;
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
        GameObject tilePrefab = TilePrefab(type);

        return tilePrefab;
    }


    private GameObject TilePrefab(string type)
    {
        var tilePrefab = tilePrefabs.Find(tile => 
        {
           return tile.tag == type;
        });

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
        if(probability <= 0) return false;
        if(probability == float.NaN) return false;

        var value = Random.Range(0f, 1f);
        //Debug.Log("MESSAGE: Probab is " + probability);
        //Debug.Log("MESSAGE: Random value is " + value);
        if(probability > value) 
            return true;

        return false;
    }
}
