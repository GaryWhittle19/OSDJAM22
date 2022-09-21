using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RIngsPuzzle : MonoBehaviour
{

    [SerializeField] private GameObject startObject;
    [SerializeField] private GameObject endObject;
    [SerializeField] private ParticleSystem ps;

    private bool isPuzzleSolved = false;

    // Start is called before the first frame update
    void Start()
    {
        isPuzzleSolved = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!isPuzzleSolved)
        {
            RaycastHit hit;
            int layerMask = 1 << 8;
            layerMask = ~layerMask;

            Vector3 startPos = startObject.transform.position;
            Vector3 endPos = endObject.transform.position;

            if (Physics.Raycast(startPos, (endPos - startPos).normalized, out hit, Vector3.Distance(startPos, endPos), layerMask))
            {
                if (hit.transform.gameObject == endObject)
                {
                    Debug.DrawRay(startPos, transform.TransformDirection((endPos - startPos).normalized) * hit.distance, Color.yellow);
                    PuzzleSolved();
                }
                else
                {
                    Debug.DrawRay(startPos, transform.TransformDirection((endPos - startPos).normalized) * 1000, Color.white);
                }
            }
        }
    }

    private void PuzzleSolved()
    {
        isPuzzleSolved = true;
        ps.startColor = Color.white;
    }

}
