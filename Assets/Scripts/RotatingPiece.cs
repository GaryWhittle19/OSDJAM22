using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingPiece : MonoBehaviour
{

    [SerializeField] [Range(1.0f, 100.0f)] private float rotatingSpeed;
    private float rotationDirection = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        rotationDirection = Random.Range(-1.0f, 1.0f);
        if (rotationDirection < 0.0f) { rotationDirection = -1.0f; } else { rotationDirection = 1.0f; }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        gameObject.transform.Rotate(Vector3.forward, rotationDirection * rotatingSpeed * Time.deltaTime);
    }
}
