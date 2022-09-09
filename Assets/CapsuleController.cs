#define DEBUG_MODE

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CapsuleController : MonoBehaviour
{
    enum ShipState
    {
        THRUST,
        RCS,
        COAST,
        SPIN_OUT
    }

    // Private vars
    private Rigidbody rb;

    [SerializeField]
    private bool buttonPressed = false;
    private float controlAngle = 0.0f;
    private Vector3 controlVector;
    private Animator animator;
    private float spinOutTimer = 0.0f;
    private float collisionTimeout = 0.0f;


    // Private editor vars
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
    [SerializeField] private float collisionKnockback;
    [SerializeField] private float spinOutTimerValue = 3.0f;
    [SerializeField] private float collisionTimeoutValue = 3.0f;
    [SerializeField] private bool resetRotation = false;
    [SerializeField] private ShipState shipState;
    [SerializeField] private ShipState prevShipState = ShipState.COAST;


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

        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        controlVector = transform.forward;
        collisionKnockback = 10.0f;

        ChangeState(ShipState.COAST);
    }

    // Update is called once per frame
    void Update()
    {
        buttonPressed = Input.GetKey("space");
    }

    // Framerate independent update
    void FixedUpdate()
    {
        UpdateState(shipState);

        collisionTimeout -= Time.deltaTime;

        Vector3 maxPossibleVelocity = rb.velocity.normalized * maxSpeed;
        if ( rb.velocity.magnitude > maxPossibleVelocity.magnitude )
        {
            rb.velocity = maxPossibleVelocity;
        };

#if DEBUG_MODE
        Debug.DrawLine(transform.position, transform.position + transform.forward, Color.green, -1, false);
        Debug.DrawLine(transform.position, transform.position + controlVector, Color.red, -1, false);
        Debug.DrawLine(transform.position, transform.position + rb.velocity, Color.cyan, -1, false);
        //Debug.Log( "forward: " + transform.forward.ToString() );
        //Debug.Log( "controlAngle: " + controlAngle.ToString() );
        //Debug.Log( "buttonPressed: " + buttonPressed.ToString() );
#endif // DEBUG_MODE
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject other = collision.gameObject;
        if (other.CompareTag("Asteroid") && collisionTimeout < 0.0f)
        {
            Vector3 collisionDirection = Vector3.Normalize(other.transform.position - this.gameObject.transform.position);
            collisionDirection.y = 0.0f;
            collisionTimeout = collisionTimeoutValue;
            Debug.Log("Performing collision:");
            Debug.Log("Direction: " + collisionDirection.ToString());
            Debug.Log("Knockback: " + collisionKnockback.ToString());

            Vector3 rbVel = rb.velocity;
            rbVel /= 10.0f;
            rb.velocity = -rbVel;

            foreach (ContactPoint contact in collision.contacts)
            {
                print(contact.thisCollider.name + " hit " + contact.otherCollider.name);
                // Visualize the contact point
                Debug.DrawRay(contact.point, contact.normal, Color.white);
            }

            ChangeState(ShipState.SPIN_OUT);
            Debug.DrawLine(transform.position, transform.position + (collisionDirection * 4), Color.white, -1, false);
        }
    }

    private void ChangeState(ShipState targetState)
    {
        prevShipState = shipState;

        switch (prevShipState)
        {
            case ShipState.COAST:
                break;
            case ShipState.RCS:
                break;
            case ShipState.THRUST:
                controlAngle = 0;
                psThrust.Stop();
                break;
            case ShipState.SPIN_OUT:
                animator.Play("Idle");
                resetRotation = true;
                break;
            default:
                break;
        }

        switch (targetState)
        {
            case ShipState.COAST:
                break;
            case ShipState.RCS:
                break;
            case ShipState.THRUST:
                rb.AddForce(transform.forward * movementForce * engineStartJerkFactor * Time.fixedDeltaTime);
                psThrust.Play();
                break;
            case ShipState.SPIN_OUT:
                animator.Play("Blink");
                resetRotation = true;
                spinOutTimer = spinOutTimerValue;
                break;
            default:
                break;
        }

        shipState = targetState;
#if DEBUG_MODE
        Debug.Log("Ship state: " + shipState.ToString());
#endif
    }

    private void UpdateState(ShipState applyState)
    {
        switch (applyState)
        {
            case ShipState.COAST:
                controlAngle += Time.fixedDeltaTime * controlAngleSpeedDegrees;
                if (controlAngle >= 360.0f) { controlAngle = 0.0f; }

                Quaternion newDirectionRotation = Quaternion.AngleAxis(controlAngle, Vector3.up);
                controlVector = newDirectionRotation * transform.forward;

                rb.velocity *= velocityTaper;

                if (buttonPressed)
                {
                    ChangeState(ShipState.RCS);
                }
                break;

            case ShipState.RCS:
                if (buttonPressed)
                {
                    if (resetRotation)
                    {
                        rb.angularVelocity = Vector3.zero;
                    }

                    Quaternion targetRotation = Quaternion.LookRotation(controlVector);
                    Quaternion moveToRotation = Quaternion.RotateTowards(rb.rotation, targetRotation, Time.fixedDeltaTime * rotateSpeedDegrees);
                    rb.MoveRotation(moveToRotation);
                    rb.velocity *= velocityTaper;

                    if (Vector3.Angle(transform.forward, controlVector) < mainThrustDeadzone)
                    {
                        ChangeState(ShipState.THRUST);
                    }
                }
                else
                {
                    ChangeState(ShipState.COAST);
                }
                break;

            case ShipState.THRUST:
                if (buttonPressed)
                {
                    rb.AddForce(transform.forward * movementForce * Time.fixedDeltaTime);

                    float lateralDriftCorrectionForce = -rb.velocity.x * transform.forward.z + rb.velocity.z * transform.forward.x;
                    rb.AddForce(lateralDriftCorrectionForce * lateralDriftCorrectionFactor * transform.right * Time.fixedDeltaTime);
                }
                else
                {
                    ChangeState(ShipState.COAST);
                }
                break;

            case ShipState.SPIN_OUT:
                spinOutTimer -= Time.deltaTime;
                if (spinOutTimer < 0.0f)
                    ChangeState(ShipState.COAST);
                break;

            default:
                break;
        }
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