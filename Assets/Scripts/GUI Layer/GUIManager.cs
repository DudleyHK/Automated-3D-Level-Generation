using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;


public class GUIManager : MonoBehaviour 
{
    [SerializeField]
    private Slider  typeSlider;
	[SerializeField]
    private Text    typeLabel;
    [SerializeField]
    private InputField height;
    [SerializeField]
    private InputField width;
    [SerializeField]
    private InputField depth;
    [SerializeField]
    private Button parseButton;
    [SerializeField]
    private Button generateButton;
    [SerializeField]
    private Dropdown levelSelection;
    [SerializeField]
    private List<GameObject> levels;
    [SerializeField]
    private GameObject displayedLevel;
    [SerializeField]
    private GameObject generatedLevel;




    public delegate string TypeSliderEvent(int value);
    public static event TypeSliderEvent typeSliderEvent;

    public delegate void DimentionEvent(string axis, int value);
    public static event DimentionEvent dimentionEvent;

    public delegate void ParseEvent(GameObject level);
    public static event ParseEvent parseEvent;

    public delegate GameObject GenerateEvent();
    public static event GenerateEvent generateEvent;

    public delegate void ChangeLevelDisplay();
    public static event ChangeLevelDisplay changeLevelDisplay;


    private void Start()
    {
        generateButton.interactable = false;

        typeLabel.text = typeSliderEvent((int)typeSlider.value);
        levels = new List<GameObject>(Resources.LoadAll<GameObject>("Prebuilt Levels/"));

        var levelNames = new List<string>();
        foreach(var level in levels)
        {
            levelNames.Add(level.name);
        }
        
        levelSelection.AddOptions(levelNames);
        levelSelection.captionText.text = levelNames[0];
        ChangeDisplayLevel(0);


    }


    private void Update()
    {
        if(!height.isFocused)
            height.text = Generator.Height.ToString();

        if(!width.isFocused)
            width.text = Generator.Width.ToString();
        

        if(!depth.isFocused)
            depth.text = Generator.Depth.ToString();

        if(displayedLevel)
        {
            displayedLevel.transform.Rotate(displayedLevel.GetComponent<LevelData>().Centre, 5 * Time.deltaTime);
        }


        // Change this to change the Z axis of the Camera.
        //float feildOfView = Camera.main.fieldOfView;
        //Camera.main.transform.position += Input.GetAxis("Mouse ScrollWheel") * -25f;
        //feildOfView = Mathf.Clamp(feildOfView, 1, 10000);
        //Camera.main.fieldOfView = feildOfView;
    }


    public void DropMenuChange()
    {
        var index = levelSelection.value;
        ChangeDisplayLevel(index);
        changeLevelDisplay();

        generateButton.interactable = false;
    }


    public void ParseLevel()
    {
        var selected = levelSelection.captionText;
        var level    = levels.Find(obj => 
        { 
            return (selected.text == obj.name);
        });

        parseEvent(level);
        generateButton.interactable = true;
    }

    public void GenerateLevel()
    {
        Destroy(displayedLevel);
        displayedLevel = generateEvent();
    }

    public void SliderChanged()
    {
        var value = (int)typeSlider.value;

        //Debug.Log("MESSAGE: Slider value is " + value);
        typeLabel.text = typeSliderEvent(value);

    }


    public void SetHeight()
    {
        var value = int.Parse(height.text);
        dimentionEvent("Height", value);
    }

    public void SetWidth()
    {
        var value = int.Parse(width.text);
        dimentionEvent("Width", value);
    }

    public void SetDepth()
    {
        var value = int.Parse(depth.text);
        dimentionEvent("Depth", value);
    }


    private void ChangeDisplayLevel(int index)
    {
        Destroy(displayedLevel);
        displayedLevel = Instantiate(levels[index], levels[index].transform.position, levels[index].transform.rotation);
    }

}
