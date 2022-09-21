using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlienController : MonoBehaviour
{

    private enum AlienState
    {
        IDLE,
        APPROACHING,
        PUZZLE,
        COMPLETE
    }


    [SerializeField] private GameObject puzzle;
    [SerializeField] private float dialogueTimeOutValue = 4.0f;
    
    private DialogueController dialogueController;
    private int dialogueCounter = 0;
    AlienState alienState = AlienState.IDLE;
    private float dialogueTimeOut = 0.0f;
    private GameObject playerObject;
    private string[] dialogueLines;

    // Start is called before the first frame update
    void Start()
    {
        dialogueController = FindObjectOfType<DialogueController>();
    }

    // Update is called once per frame
    void Update()
    {
        dialogueTimeOut -= Time.deltaTime;
        switch (alienState)
        {
            case AlienState.IDLE:   // Do nothing
                break;
            case AlienState.APPROACHING: // Wait for player to stop for next dialogue line

                if (playerObject.GetComponent<Rigidbody>().velocity.magnitude < 0.2f && dialogueTimeOut <= 0.0f)
                {
                    ChangeState(AlienState.PUZZLE);
                }
                break;
            case AlienState.PUZZLE: // Wait for puzzle complete

                if (dialogueTimeOut <= 0.0f)
                {
                    ChangeState(AlienState.COMPLETE);
                }

                break;
            case AlienState.COMPLETE:
                break;
            default:
                break;
        }
    }

    private void ChangeState(AlienState targetState)
    {
        Debug.Log("Changing state to: " + targetState);
        switch (targetState)
        {
            case AlienState.IDLE:
                break;
            case AlienState.APPROACHING: // Get next set of dialogue lines
                dialogueLines = dialogueController.RequestLines();
                Debug.Log("Lines received:");
                Debug.Log(dialogueLines);
                break;
            case AlienState.PUZZLE: // Spawn puzzle
                Instantiate(puzzle, gameObject.transform);
                break;
            case AlienState.COMPLETE: // Signal the puzzle is complete to the player
                playerObject.GetComponent<CapsuleController>().SignalPuzzleComplete();
                break;
            default:
                break;
        }

        if (targetState > AlienState.IDLE) { ProgressDialogue(); }
        alienState = targetState;
    }

    public bool WakeAlien(GameObject playerObj)
    {
        if (alienState == AlienState.COMPLETE)
        {
            return false;
        }

        playerObject = playerObj;
        ChangeState(AlienState.APPROACHING);
        
        return true;
    }

    private void ProgressDialogue()
    {
        if (dialogueCounter < dialogueLines.Length)
        {
            dialogueController.SetText(dialogueLines[dialogueCounter]);
            dialogueTimeOut = dialogueTimeOutValue;
            dialogueCounter++;
        }
    }
}
