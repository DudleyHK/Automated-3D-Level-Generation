using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelData : MonoBehaviour 
{
    public List<GameObject> generatedTiles = new List<GameObject>();
    public Vector3 Centre { get; set; }


    private void Start()
    {
        foreach(var renderer in GetComponentsInChildren<Renderer>())
        {
            var tile = renderer.gameObject;
            generatedTiles.Add(tile);
        }
        SetLevelData();  
    }

    private void SetLevelData()
    {
        Vector3 centroid = Vector3.zero;

        foreach(var tile in generatedTiles)
        {
            var centre = tile.GetComponent<Renderer>().bounds.center;
            centroid += centre;
        }

        centroid /= generatedTiles.Count;

        Centre = centroid;
    }

}
