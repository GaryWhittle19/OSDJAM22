using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RingsPuzzle : MonoBehaviour
{

    [SerializeField] private GameObject startObject;
    [SerializeField] private GameObject endObject;
    [SerializeField] private ParticleSystem ps;
    [SerializeField] private GameObject[] rings;
    [SerializeField] private float startingSpeed = 30.0f;

    private GameObject alienRef;
    private bool isPuzzleSolved = false;
    private bool buttonDown = false;
    private int solveCounter = 0;
    private float flipCountdown = 0.0f;
    private bool start = false;

    // Start is called before the first frame update
    void Start()
    {
        isPuzzleSolved = false;

        float rotateDir = 1.0f;

        foreach (var ring in rings)
        {
            var rotatingPiece = ring.GetComponent<RotatingPiece>();
            rotatingPiece.SetRotationValues(startingSpeed * 0.1f * rotateDir, rotateDir);
            rotateDir *= -1.0f;
        }

    }

    public void ResetVars(GameObject alien)
    {
        isPuzzleSolved = false;
        buttonDown = false;
        solveCounter = 0;
        flipCountdown = 0.0f;
        start = true;
        ps.startColor = Color.red;

        foreach (var ring in rings)
        {
            ring.GetComponent<RotatingPiece>().ResetVars();
        }

        alienRef = alien;
    }

    private void Update()
    {
        buttonDown = Input.GetKey("space") && start;
        if (flipCountdown > 0.0f) { flipCountdown -= Time.deltaTime; }
    }

    void FixedUpdate()
    {


        if (!isPuzzleSolved && alienRef != null)
        {
            RaycastHit hit;
            int layerMask = 1 << 8;
            layerMask = ~layerMask;

            Vector3 startPos = startObject.transform.position;
            Vector3 endPos = endObject.transform.position;

            if (Physics.Raycast(startPos, (endPos - startPos).normalized, out hit, Vector3.Distance(startPos, endPos), layerMask))
            {
                if (buttonDown && solveCounter < rings.Length && hit.transform.gameObject != rings[solveCounter])
                {
                    rings[solveCounter].GetComponent<RotatingPiece>().Freeze();
                    solveCounter++;
                }
                else if (hit.transform.gameObject == endObject)
                {
                    PuzzleSolved();
                }
                else if (buttonDown && flipCountdown <= 0.0f)
                {
                    flipCountdown = 1.0f;

                    if (solveCounter < rings.Length)
                    {
                        rings[solveCounter].GetComponent<RotatingPiece>().Flip();
                    }
                }
            }
        }
    }

    private void PuzzleSolved()
    {
        isPuzzleSolved = true;
        ps.startColor = Color.white;
        alienRef.GetComponent<AlienController>().SignalPuzzleComplete();
    }

}
