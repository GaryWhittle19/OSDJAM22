#define DEBUG_MODE

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CapsuleController : MonoBehaviour
{
    Rigidbody rb;
    Vector3 controlVector;
    float controlAngle = 0.0f;
    float speed;
    bool buttonPressed;
    bool resetControlAngle = false;
    
    public float maxSpeed;
    public float speedTaper;
    public float rotateSpeed;

#if DEBUG_MODE
    uint qsize = 6;  // number of messages to keep
    Queue myLogQueue = new Queue();
#endif // DEBUG_MODE

    // Start is called before the first frame update
    void Start()
    {
#if DEBUG_MODE
        Debug.Log("Started up logging.");
#endif // DEBUG_MODE

        rb = GetComponent<Rigidbody>();
        controlVector = transform.forward;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            buttonPressed = true;
        }
        else if(Input.GetKeyUp("space"))
        {
            buttonPressed = false;
            resetControlAngle = true;
        }
    }

    // Framerate independent update
    void FixedUpdate()
    {
        // Update angle of newDirection.
        if (buttonPressed) { speed = maxSpeed; }
        if (resetControlAngle) { controlAngle = 0; }
        speed *= speedTaper;

        // Once facing desired direction, fire main thruster
        if (Vector3.Angle(transform.forward, controlVector) < 1.0f && buttonPressed)
        {
            rb.AddForce(transform.forward * speed * Time.fixedDeltaTime);
            Debug.Log("BURN \n");
        }
        // Until facing desired direction, fire RCS thrusters (rotate)
        else if (transform.forward != controlVector && buttonPressed)
        {
            Quaternion targetRotation = Quaternion.LookRotation(controlVector);
            Quaternion moveToRotation = Quaternion.RotateTowards(rb.rotation, targetRotation, Time.fixedDeltaTime * rotateSpeed);
            rb.MoveRotation(moveToRotation);
            Debug.Log("RCS \n");
        }
        else 
        {
            // Update newDirection based on the control angle.
            if (++controlAngle >= 360.0f) { controlAngle = 0.0f; }
            Quaternion newDirectionRotation = Quaternion.AngleAxis(controlAngle, Vector3.up);
            controlVector = newDirectionRotation * transform.forward;
            // Update speed based on the taper.
            rb.velocity *= speedTaper;
            Debug.Log("COAST \n");
        }

        resetControlAngle = false;

#if DEBUG_MODE
        Debug.DrawLine(transform.position, transform.position + transform.forward, Color.green, -1, false);
        Debug.DrawLine(transform.position, transform.position + controlVector, Color.red, -1, false);
        Debug.Log("forward: " + transform.forward.ToString());
        Debug.Log("controlVector: " + controlVector.ToString());
        Debug.Log("controlAngle: " + controlAngle.ToString());
        Debug.Log("speed: " + speed.ToString());
        Debug.Log("buttonPressed: " + buttonPressed.ToString());
#endif // DEBUG_MODE
    }

#if DEBUG_MODE
    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        myLogQueue.Enqueue("[" + type + "] : " + logString);
        if (type == LogType.Exception)
            myLogQueue.Enqueue(stackTrace);
        while (myLogQueue.Count > qsize)
            myLogQueue.Dequeue();
    }

    void OnGUI()
    {
        GUI.skin.label.fontSize = 21;
        GUI.color = Color.red;
        GUILayout.BeginArea(new Rect(Screen.width - 400, 0, 400, Screen.height));
        GUILayout.Label("\n" + string.Join("\n", myLogQueue.ToArray()));
        GUILayout.EndArea();
    }
#endif // DEBUG_MODE
}