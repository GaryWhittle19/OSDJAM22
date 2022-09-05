#define DEBUG_MODE

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CapsuleController : MonoBehaviour
{
    Rigidbody rb;

    bool buttonPressed = false;
    bool engineFinish = false;
    bool engineStart = false;
    float controlAngle = 0.0f;
    Vector3 controlVector;

    [Header( "Variables" )]
    [SerializeField] ParticleSystem psThrust;
    [SerializeField] float movementForce = 100.0f;
    [SerializeField] float engineStartJerkFactor = 1.0f;
    [SerializeField] float maxSpeed = 2.5f;
    [SerializeField] [Range(0.95f, 1.0f)] float velocityTaper = 0.9885f;
    [SerializeField] float rotateSpeedDegrees = 260.0f;
    [SerializeField] float controlAngleSpeedDegrees = 120.0f;
    [SerializeField] float lateralDriftCorrectionFactor = 100.0f;
    [SerializeField] float mainThrustDeadzone = 0.5f;

#if DEBUG_MODE
    uint qsize = 4;  // number of messages to keep
    Queue myLogQueue = new Queue();
#endif // DEBUG_MODE

    // Start is called before the first frame update
    void Start()
    {
#if DEBUG_MODE
        Debug.Log( "Started up logging." );
#endif // DEBUG_MODE

        rb = GetComponent<Rigidbody>();
        controlVector = transform.forward;
    }

    // Update is called once per frame
    void Update()
    {
        if ( Input.GetKeyDown( "space" ) )
        {
            buttonPressed = true;
            engineStart = true;
        }
        else if ( Input.GetKeyUp( "space" ) )
        {
            buttonPressed = false;
            engineFinish = true;
        }
    }

    // Framerate independent update
    void FixedUpdate()
    {
        if ( engineFinish )
        {
            controlAngle = 0;
            psThrust.Stop();
            engineFinish = false;
        }

        if ( Vector3.Angle( transform.forward, controlVector ) < mainThrustDeadzone && buttonPressed ) // THRUST
        {
#if DEBUG_MODE
            Debug.Log( "BURN" );
#endif // DEBUG_MODE

            if ( engineStart )
            {
                rb.AddForce( transform.forward * movementForce * engineStartJerkFactor * Time.fixedDeltaTime );
                psThrust.Play();
                engineStart = false;
            }

            rb.AddForce( transform.forward * movementForce * Time.fixedDeltaTime );

            float lateralDriftCorrectionForce = -rb.velocity.x * transform.forward.z + rb.velocity.z * transform.forward.x;
            rb.AddForce( lateralDriftCorrectionForce * lateralDriftCorrectionFactor * transform.right * Time.fixedDeltaTime );
        }
        else if ( transform.forward != controlVector && buttonPressed ) // RCS
        {
#if DEBUG_MODE
            Debug.Log( "RCS" );
#endif // DEBUG_MODE

            Quaternion targetRotation = Quaternion.LookRotation( controlVector );
            Quaternion moveToRotation = Quaternion.RotateTowards( rb.rotation, targetRotation, Time.fixedDeltaTime * rotateSpeedDegrees );
            rb.MoveRotation( moveToRotation );
            rb.velocity *= velocityTaper;
        }
        else // COAST
        {
#if DEBUG_MODE
            Debug.Log( "COAST" );
#endif // DEBUG_MODE

            controlAngle += Time.fixedDeltaTime * controlAngleSpeedDegrees;
            if ( controlAngle >= 360.0f ) { controlAngle = 0.0f; }

            Quaternion newDirectionRotation = Quaternion.AngleAxis( controlAngle, Vector3.up );
            controlVector = newDirectionRotation * transform.forward;
            
            rb.velocity *= velocityTaper;
        }

        Vector3 maxPossibleVelocity = rb.velocity.normalized * maxSpeed;
        if ( rb.velocity.magnitude > maxPossibleVelocity.magnitude )
        {
            rb.velocity = maxPossibleVelocity;
        };

#if DEBUG_MODE
        Debug.DrawLine( transform.position, transform.position + transform.forward, Color.green, -1, false );
        Debug.DrawLine( transform.position, transform.position + controlVector, Color.red, -1, false );
        Debug.DrawLine( transform.position, transform.position + rb.velocity, Color.cyan, -1, false );
        Debug.Log( "forward: " + transform.forward.ToString() );
        Debug.Log( "controlAngle: " + controlAngle.ToString() );
        Debug.Log( "buttonPressed: " + buttonPressed.ToString() );
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

    void HandleLog( string logString, string stackTrace, LogType type )
    {
        myLogQueue.Enqueue( "[" + type + "] : " + logString );
        if ( type == LogType.Exception )
            myLogQueue.Enqueue( stackTrace );
        while ( myLogQueue.Count > qsize )
            myLogQueue.Dequeue();
    }

    void OnGUI()
    {
        GUI.skin.label.fontSize = 21;
        GUI.color = Color.white;
        GUILayout.BeginArea( new Rect( Screen.width - 400, 0, 400, Screen.height ) );
        GUILayout.Label( "\n" + string.Join( "\n", myLogQueue.ToArray() ) );
        GUILayout.EndArea();
    }
#endif // DEBUG_MODE
}