using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AteroidController : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 3.5f;
    [SerializeField] private Vector3 rotationAxis;
    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rotationAxis = new Vector3(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
        rb = this.gameObject.GetComponent<Rigidbody>();
        rb.angularDrag = 0.0f;
        rb.AddTorque(rotationAxis * rotationSpeed, ForceMode.Impulse);
    }

    // Update is called once per frame
    void Update()
    {
    }
}
