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


    [SerializeField] private float dialogueTimeOutValue = 4.0f;

    private GameObject puzzle;
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
        puzzle = FindObjectOfType<RingsPuzzle>().gameObject;
        puzzle.GetComponent<RingsPuzzle>().ResetVars(this.transform.gameObject);
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

        CapsuleController capsuleController = null;
        if (playerObject)
        {
            capsuleController = playerObject.GetComponent<CapsuleController>();
        }

        switch (targetState)
        {
            case AlienState.IDLE:
                break;
            case AlienState.APPROACHING: // Get next set of dialogue lines
                dialogueLines = dialogueController.RequestLines();
                dialogueCounter = 0;
                break;
            case AlienState.PUZZLE: // Spawn puzzle
                Debug.Log("open");

                capsuleController.OpenPuzzleView();
                puzzle.GetComponent<RingsPuzzle>().ResetVars(this.transform.gameObject);
                break;
            case AlienState.COMPLETE: // Signal the puzzle is complete to the player
                Debug.Log("close");

                capsuleController.ClosePuzzleView();
                capsuleController.SignalPuzzleComplete();
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
        Debug.Log("wake");
        playerObject = playerObj;
        ChangeState(AlienState.APPROACHING);
        
        return true;
    }

    public void SignalPuzzleComplete()
    {
        ChangeState(AlienState.COMPLETE);
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
