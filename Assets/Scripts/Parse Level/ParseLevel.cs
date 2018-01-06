using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Directions
{
    Right,
    Up,
    Forward
}


public class ParseLevel : MonoBehaviour
{
    public GameObject debug_originObject;

    [SerializeField]
    private GameObject level;
    [SerializeField]
    private List<GameObject> tiles;
    [SerializeField]
    private List<Directions> directions = new List<Directions>(new Directions[]
    {
        Directions.Right,
        Directions.Up,
        Directions.Forward
    });
    [SerializeField]
    private List<ArrayList> outputData = new List<ArrayList>();
    [SerializeField]
    private Vector3 startPosition = new Vector3(100f, 100f, 100f);
    [SerializeField]
    private int startIdx = -1;
    [SerializeField]
    private bool parseLevel = false;
  

    private CSVManager csvManager;

    


    private void OnEnable()
    {
        GUIManager.parseEvent += ParseWithLevel;
    }


    private void OnDisable()
    {
        GUIManager.parseEvent -= ParseWithLevel;
    }



    private void ParseWithLevel(GameObject level)
    {
        if(!level)
        {
            Debug.Log("ERROR: Level passed in is null");
            return;
        }

        this.level = level;
        Parse();
    }


    private void Update()
    {
        if(parseLevel || Input.GetKeyDown(KeyCode.P))
        {
           Parse();
        }
    }

    private void Parse()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        Init();
        Run();
        OutputResult();
        CalculateProbabilities();
        csvManager.Write();
        parseLevel = false;

        tiles.Clear();

        stopwatch.Stop();
        Debug.Log("Time taken to parse and write level " + stopwatch.Elapsed);
    }

    /// <summary>
    /// Add the chlid tile to a list and check if its positiion is lower in all directions than the current lowest.
    ///     This is to find out where the min position is.
    /// </summary>
    public void Init()
    {
        csvManager = FindObjectOfType<CSVManager>();
        if(!csvManager)
        {
            Debug.Log("ERROR: CSVManager object is not active in Scene");
            return;
        }

        if(!level)
        {
            Debug.Log("ERROR: Level object has not been set.");
            return;
        }

        int idx = 0;

        foreach(var renderer in level.GetComponentsInChildren<Renderer>())
        {
            var child = renderer.gameObject;
            tiles.Add(child);

            if(child.transform.position.x < startPosition.x &&
               child.transform.position.y < startPosition.y &&
               child.transform.position.z < startPosition.z)
            {
                startPosition = child.transform.position;
                startIdx = idx;
            }
            idx++;
        }
        
       
    }

    /// <summary>
    /// Currently ignoring startIdx
    /// </summary>
    public void Run()
    {
       outputData = new List<ArrayList>();

        foreach(var tile in tiles)
        {
            var centre = Centre(tile.GetComponent<Renderer>());
            foreach(var direction in directions)
            {
                if(tile.tag == "Untagged")
                {
                    Debug.Log("ERROR: Tile " + tile.name + " " + tile.tag);
                    continue;
                }

                var tag = tile.tag + " " + direction.ToString();

                var dir = GetDirectionVector(direction);

                var neighbourTag = GetTagOfNeighbour(centre, dir);
                if(neighbourTag == "")
                {
                    // Debug.Log("MESSAGE: Neighbour in direction " + direction.ToString() + " is nothingness");
                    continue;
                }

                // TODO: Make this turn on and offable
                //if(neighbourTag == "Air")
                //    continue;


                ArrayList list;
                if(AlreadyCreated(tag, neighbourTag, out list))
                {
                    var value = (int)list[2];
                    value++;

                    list[2] = value;
                    //Debug.Log("MESSAGE: Updated Cell From " + list[0] + " to " + list[1] + " with " + list[2]);

                }
                else
                {
                    ArrayList newList = new ArrayList(3);
                    newList.Add(tag);
                    newList.Add(neighbourTag);
                    newList.Add(1);

                    outputData.Add(newList);
                    //  outputData.Add(new ArrayList { tag, neighbourTag, 1});
                    //Debug.Log("MESSAGE: From " + newList[0] + " to " + newList[1] + " are being added to outputData with data of " + newList[2]);
                }
                DebugDrawLines(direction, centre, dir);
            }
            //var debugObj = Instantiate(debug_originObject, centre, Quaternion.identity);
        }
    }


    private bool AlreadyCreated(string from, string to, out ArrayList outList)
    {
        outList = null;

        foreach(var list in outputData)
        {
            outList = list;

            if(list[0] as string == from &&
                list[1] as string == to)
            {
                return true;
            }
        }
        return false;
    }


    public void OutputResult()
    {
        csvManager.SetTotalsValues(outputData);
    }

    /// <summary>
    /// Run the calculation the probability for each.
    /// </summary>
    private void CalculateProbabilities()
    {
        var rowTotals = csvManager.GetRowTotals();

        for(int i = 0; i < rowTotals.Count; i++)
        {
            var total = rowTotals[i];
            csvManager.SetProbabilityValue(i, total);
        }
    }

    private Vector3 GetDirectionVector(Directions dir)
    {
        Vector3 direction = Vector3.zero;
        switch(dir)
        {
            case Directions.Right:   direction = Vector3.right;   break;
            case Directions.Up:      direction = Vector3.up;      break;
            case Directions.Forward: direction = Vector3.forward; break;
        }
        return direction;
    }

    private string GetTagOfNeighbour(Vector3 origin, Vector3 dir)
    {
        RaycastHit hitInfo;
        var hit = Physics.Raycast(origin, dir, out hitInfo, 20f);
        if(hit)
        {
            return hitInfo.collider.tag;
        }
        else
        {
            //Debug.Log("Raycast at position " + origin + " in direction " + dir + " didn't hit anything");
            //Debug.DrawRay(origin, dir, Color.magenta, 100f);
            return "Air";
        }
    }

    private void DebugDrawLines(Directions directions, Vector3 origin, Vector3 dir)
    {        
        switch(directions)
        {
            case Directions.Right:
                Debug.DrawRay(origin, dir, Color.red, 100f);
                break;
            case Directions.Up:
                Debug.DrawRay(origin, dir, Color.green, 100f);
                break;
            case Directions.Forward:
                Debug.DrawRay(origin, dir, Color.blue, 100f);
                break;
            default:
                break;
        }
    }

    private Vector3 Centre(Renderer renderer)
    {
        return renderer.bounds.center;
    }
}
