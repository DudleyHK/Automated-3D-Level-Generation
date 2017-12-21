using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParseLevel : MonoBehaviour 
{
    private enum Directions
    {
        Right, 
        Up, 
        Forward
    }


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
    [SerializeField]
    private Material complete;

    private CSVManager csvManager;



    private void Update()
    {
        if(parseLevel)
        {
            Init();
            Run();
            OutputResult();
            parseLevel = false;
        }
    }

    /// <summary>
    /// Add the chlid tile to a list and check if its positiion is lower in all directions than the current lowest.
    ///     This is to find out where the min position is.
    /// </summary>
    public void Init()
    {
        csvManager = FindObjectOfType<CSVManager>();
        if(!csvManager) Debug.Log("ERROR: CSVManager object is not active in Scene");

        if(!level) Debug.Log("ERROR: Level object has not been set.");
        int idx = 0;
        foreach(Transform child in level.transform)
        {
            tiles.Add(child.gameObject);

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
        foreach(var tile in tiles)
        {
            tile.GetComponent<MeshRenderer>().material = complete;

            foreach(var direction in directions)
            {
                var tag = tile.tag + " " + direction.ToString();

                var dir = GetDirectionVector(direction);
                var neighbourTag = GetTagOfNeighbour(tile.transform.position, dir);
                if(neighbourTag == "")
                {
                    // Debug.Log("Neighbour in direction " + direction.ToString() + " is nothingness");
                    continue;
                }


                ArrayList list;
                if(AlreadyCreated(tag, neighbourTag, out list))
                {
                    var value = (float)list[2];
                    value++;

                    list[2] = value;
                    Debug.Log("Updated Cell From " + list[0] + " to " + list[1] + " with " + list[2]);

                }
                else
                {
                    ArrayList newList = new ArrayList(3);
                    newList.Add(tag);
                    newList.Add(neighbourTag);
                    newList.Add(1f);

                    outputData.Add(newList);
                    //  outputData.Add(new ArrayList { tag, neighbourTag, 1});
                    Debug.Log("From " + newList[0] + " to " + newList[1] + " are being added to outputData with data of " + newList[2]);
                }

            }
            
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
        csvManager.SetProbabilityValues(outputData);
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
        var hit = Physics.Raycast(origin, dir, out hitInfo);
        if(hit)
        {
            return hitInfo.collider.tag;
        }
        else
        {
         //   Debug.Log("Raycast at position " + origin + " didn't hit anything");
        }

        return "";
    }
}
