using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public GameObject TileObject { get; set; }
    
    [SerializeField]
    private List<string> parents = new List<string>();
    [SerializeField]
    private List<float> transitionMatrix = new List<float>();
    [SerializeField]
    private List<Material> materials = new List<Material>();

    private CSVManager csvManager;
    private string currentParent;


    public bool Run(string parent, CSVManager csvManager, List<Material> materials)
    {
        currentParent = parent;
        this.csvManager = csvManager;
        this.materials = materials;


        if(!AddParent()) return false;
       
        
        UpdateTransitionMatrix();
        UpdateTileType();

        return true;
    }


    private bool AddParent()
    {
        if(!parents.Contains(currentParent))
        {
            parents.Add(currentParent);
            return true;
        }
        return false;
    }


    /// <summary>
    /// If only one parent has been added use their probabilities list. For more than one parent update the 
    ///     transition Matrix accordingly.
    /// </summary>
    private void UpdateTransitionMatrix()
    {
        var probabilities = csvManager.ProbabiltiesOfRow(currentParent);

        if(parents.Count == 1)
        {
            transitionMatrix = probabilities;
            return;
        }
      
        for(int i = 0; i < transitionMatrix.Count; i++)
        {
            var value      = transitionMatrix[i];
            var probabilty = probabilities[i];

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
            
            transitionMatrix[i] = value;
        }
    }

    /// <summary>
    /// Get the type to update to. Dont update if its the same as the current type.
    /// </summary>
    private void UpdateTileType()
    {
        var index = 0; 
        var probability = 0f;

        foreach(var probab in transitionMatrix)
        {
            index++;

            probability = probab;
            if(FlipCoin(probability))
            {
                break;
            }
        }

        var type = csvManager.NameOfColumn(index);
        if(this.gameObject.tag == type)
        {
            return;
        }
        this.gameObject.tag = type;

        foreach(var mat in materials)
        {
            if(mat.name == type)
            {
                this.GetComponent<Renderer>().material = mat;
                gameObject.name = mat.name;
            }
        }
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
        if(probability > value)
            return true;

        return false;
    }
}
