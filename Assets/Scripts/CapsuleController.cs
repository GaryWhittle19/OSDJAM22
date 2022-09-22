using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CapsuleController : MonoBehaviour
{
    public enum ShipState
    {
        THRUST,
        RCS,
        COAST,
        SPIN_OUT,
        ENCOUNTER
    }

    // Mesh
    private Rigidbody rb;
    // Movement
    private bool buttonPressed = false;
    private float controlAngle = 0.0f;
    private Vector3 controlVector;
    // Collision
    private Animator animator;
    private float spinOutTimer = 0.0f;
    private float collisionTimeout = 0.0f;
    // Boundary
    Vector3 startingPosition;

    [Header( "RK_Particles" )]
    [SerializeField] ParticleSystem psThrust;
    [Header( "RK_State" )]
    [SerializeField] private ShipState shipState;
    [SerializeField] private ShipState prevShipState = ShipState.COAST;
    [Header( "RK_Movement" )]
    [SerializeField] float movementForce = 100.0f;
    [SerializeField] float engineStartJerkFactor = 1.0f;
    [SerializeField] float maxSpeed = 2.0f;
    [SerializeField] [Range(0.95f, 1.0f)] float velocityTaper = 0.985f;
    [SerializeField] float rotateSpeedDegrees = 260.0f;
    [SerializeField] float controlAngleSpeedDegrees = 120.0f;
    [SerializeField] float lateralDriftCorrectionFactor = 100.0f;
    [SerializeField] float mainThrustDeadzone = 0.5f;
    [SerializeField] private GameObject directionalMesh;
    [Header( "RK_Collision" )]
    [SerializeField] private float collisionKnockback = 10.0f;
    [SerializeField] private float spinOutTimerValue = 3.0f;
    [SerializeField] private float collisionTimeoutValue = 3.0f;
    [SerializeField] private bool resetRotation = false;
    [Header( "RK_Rendering" )]
    [SerializeField] private Material renderTex;
    [SerializeField] private Camera puzzleCamera;
    [SerializeField] private Camera replaceCamera;
    [SerializeField] private RenderTexture swapTex;
    [Header( "RK_Boundary" )]
    [SerializeField] [Tooltip( "Boundary distance - how far from centre can player go?" )] private float boundaryDistance;
    [SerializeField] [Tooltip( "Progress towards boundary at which static will appear (1.0 - at boundary, 0.0 - in centre)" )] [Range(0.0f, 1.0f)]private float staticStart;
    [SerializeField] private GameObject connectionText;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        controlVector = transform.forward;
        collisionKnockback = 10.0f;
        startingPosition = gameObject.transform.position;

        renderTex.SetFloat("_NoiseAmount", 0.05f);
        connectionText.SetActive(false);
        ChangeState(ShipState.COAST);

        ClosePuzzleView();
    }

    // Update is called once per frame
    void Update()
    {
        buttonPressed = Input.GetKey( "space" );

        PlayerBoundaryCheck();
    }

    private void PlayerBoundaryCheck()
    {
        // Get distance from start position
        float distFromStart = Vector3.Distance( gameObject.transform.position, startingPosition );
        // Get distance where static should begin
        float staticStartDist = boundaryDistance * staticStart;
        // If player has passed that point
        if ( distFromStart - staticStartDist > 0.0f )
        {
            // Losing connection begin
            connectionText.SetActive( true );
            // Get amount of progression between the point of static starting and the boundary
            float boundaryProgression = Mathf.Min( (distFromStart - staticStartDist) / (boundaryDistance - staticStartDist), 1.0f );
            // Set the static (or noise) amount
            renderTex.SetFloat( "_NoiseAmount", boundaryProgression );
            // Return player to start position if they go off-screen
            if ( distFromStart > boundaryDistance )
            {
                renderTex.SetFloat( "_NoiseAmount", 0.0f );
                gameObject.transform.position.Set( startingPosition.x, startingPosition.y, startingPosition.z );
                rb.MovePosition( startingPosition );
            }
        }
        else
        {
            connectionText.SetActive( false );
        }
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

        directionalMesh.transform.position = transform.position + controlVector;
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Alien"))
        {
            var alienController = other.gameObject.GetComponent<AlienController>();
            if (alienController.WakeAlien(gameObject))
            {
                ChangeState(CapsuleController.ShipState.ENCOUNTER);
            }
        }
    }

    public void SignalPuzzleComplete()
    {
        ChangeState(ShipState.COAST);
    }

    public void OpenPuzzleView()
    {
        Debug.Log("Open view");
        puzzleCamera.targetTexture = swapTex;
        replaceCamera.targetTexture = null;
    }

    public void ClosePuzzleView()
    {
        Debug.Log("Close view");
        replaceCamera.targetTexture = swapTex;
        puzzleCamera.targetTexture = null;
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

            case ShipState.ENCOUNTER:
                rb.velocity *= velocityTaper;
                // TODO
                // Has puzzle been completed?
                // If so, change state back to coast
                break;

            default:
                break;
        }
    }
}