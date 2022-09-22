using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
    public enum selection
    {
        // Main menu options.
        STORY,
        ENDLESS,
        OPTIONS,
        CREDITS,
        QUIT, 
        INTRO
    }
    [Header( "RK_Camera" )]
    [SerializeField] private GameObject missionControlCamObject;
    [Header( "RK_MenuSetup" )]
    [SerializeField] private List<selection> entries;               // List of menu entries
    [SerializeField] private GameObject triggerPrefab;              // Menu "button"/trigger prefab to use
    [SerializeField] private float menuDistance;                    // Distance of menu options from player
    private Vector3 triggerLocation;
    
    void Start()
    {
        triggerLocation = new Vector3( 0.0f, -10.0f, menuDistance );
        foreach ( selection entry in entries )
        {
            // Trigger for menu option
            Quaternion newDirectionRotation = Quaternion.AngleAxis( 360 / entries.Count, Vector3.up );
            triggerLocation = newDirectionRotation * triggerLocation;
            Vector3 spawnLocation = triggerLocation + transform.parent.transform.position;
            GameObject newOption = (GameObject)Instantiate( triggerPrefab, spawnLocation, Quaternion.identity );
            newOption.GetComponent<MenuOption>().type = entry;
            newOption.transform.SetParent(transform);
            // Text for menu option
            GameObject newText = new GameObject();
            TextMesh t = newText.AddComponent<TextMesh>();
            t.text = entry.ToString();
            t.fontSize = 250;
            t.transform.localEulerAngles += new Vector3( 90, 0, 0 );
            t.transform.localPosition += spawnLocation;
            t.transform.localScale = new Vector3(0.025f, 0.025f, 0.025f);
            t.anchor = TextAnchor.UpperCenter;
            t.color = Color.green;
        }
        missionControlCamObject.GetComponent<MissionControlCamera>().BeginTransition( selection.INTRO );
    }

    public void MenuOptionChosen( MenuOption chosenOption )
    {
        switch ( chosenOption.type )
        {
            case selection.STORY:
                missionControlCamObject.GetComponent<MissionControlCamera>().BeginTransition(selection.STORY);
                break;
            //case selection.ENDLESS:

            //    break;
            //case selection.CREDITS:

            //    break;
            case selection.OPTIONS:
                
                break;
            case selection.QUIT:
                Application.Quit();
                break;
        }
    }
}
