using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CapsuleController : MonoBehaviour
{
    // State of player ship
    private enum ShipState
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
    private bool resetRotation = false;
    // Boundary
    private Vector3 startingPosition;
    private float currentBoundaryDistance = 6.0f;
    private float currentBlackoutStart = 0.5f;
    // Mission Control Cam Transitions
    private float cameraTransitionTimer = 0.0f;
    private bool transitionLogicPerformed = false;
    //
    [Header( "RK_Particles" )]
    [SerializeField] ParticleSystem psThrust;
    //
    [Header( "RK_State" )]
    [SerializeField] private ShipState shipState;
    [SerializeField] private ShipState prevShipState = ShipState.COAST;
    //
    [Header( "RK_Movement" )]
    [SerializeField] float movementForce = 100.0f;
    [SerializeField] float maxSpeed = 2.0f;
    [SerializeField] [Range(0.95f, 1.0f)] float velocityTaper = 0.985f;
    [SerializeField] float rotateSpeedDegrees = 260.0f;
    [SerializeField] float controlAngleSpeedDegrees = 120.0f;
    [SerializeField] float lateralDriftCorrectionFactor = 100.0f;
    [SerializeField] float mainThrustDeadzone = 0.5f;
    [SerializeField] private GameObject directionalMesh;
    [SerializeField] private float directionalMeshDistance = 1.65f;
    //
    [Header( "RK_Collision" )]
    [SerializeField] private float collisionKnockback = 10.0f;
    [SerializeField] private float spinOutTimerValue = 3.0f;
    [SerializeField] private float collisionTimeoutValue = 3.0f;
    //
    [Header( "RK_Rendering" )]
    [SerializeField] private Material renderTex;
    [SerializeField] private Camera puzzleCamera;
    [SerializeField] private Camera replaceCamera;
    [SerializeField] private RenderTexture swapTex;
    // For handling background switches
    [SerializeField] private Material menuNebula;
    [SerializeField] private Material menuStarfield;
    [SerializeField] private Material storyNebula;
    [SerializeField] private Material storyStarfield;
    [SerializeField] private GameObject nebulaBackground;
    [SerializeField] private GameObject starfieldBackground;
    //
    [Header( "RK_Boundary" )]
    [SerializeField] private float menuBoundaryDistance = 6.0f;
    [SerializeField] private float storyBoundaryDistance = 90.0f;
    // NOTE: blackoutStart - ratio of boundary at which static starts (0.8 = 80% for example)
    [SerializeField] [Range( 0.0f, 1.0f )] private float menuBlackoutStart = 0.5f; 
    [SerializeField] [Range( 0.0f, 1.0f )] private float storyBlackoutStart = 0.8f;
    [SerializeField] private GameObject connectionText;
    //
    [Header( "RK_Menu" )]
    [SerializeField] private GameObject menuObject;
    [SerializeField] private GameObject missionControlCamera;

    [Header("RK_Spawning")]
    private SpawningController spawningController;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        controlVector = transform.forward;
        startingPosition = gameObject.transform.position;

        renderTex.SetFloat("_BlackoutAmount", 0.05f);
        currentBlackoutStart = menuBlackoutStart;
        connectionText.SetActive(false);
        ChangeState(ShipState.COAST);

        // Backgrounds need reset
        nebulaBackground.GetComponent<MeshRenderer>().material = menuNebula;
        starfieldBackground.GetComponent<MeshRenderer>().material = menuStarfield;

        spawningController = FindObjectOfType<SpawningController>();

        ClosePuzzleView();
    }

    // Update is called once per frame
    void Update()
    {
        // NOTE: Currently we deal with menu transitions in this Update call, could be refactored.
        if( !missionControlCamera.GetComponent<MissionControlCamera>().cameraTravelling )
        {
            buttonPressed = Input.GetKey( "space" );
            PlayerBoundaryCheck();
            // NOTE: Resetting these every frame is messy
            cameraTransitionTimer = 0.0f;
            transitionLogicPerformed = false;
        }
        else 
        {
            cameraTransitionTimer += Time.deltaTime;
            float currentSeconds = cameraTransitionTimer % 60;
            // NOTE: As above
            buttonPressed = false;
            switch ( missionControlCamera.GetComponent<MissionControlCamera>().travellingTo )
            {
                case Menu.selection.STORY:
                    if( currentSeconds > 2.0f && ! transitionLogicPerformed )
                    {
                        // Disable the menu
                        menuObject.SetActive( false );
                        // Stop player from moving
                        rb.velocity.Set( 0.0f, 0.0f, 0.0f );
                        // First, set the player to the start position
                        rb.MovePosition( startingPosition );
                        rb.rotation = Quaternion.Euler( new Vector3( 0, 0, 0 ));
                        // Then, swap out the backgrounds
                        nebulaBackground.GetComponent<MeshRenderer>().material = storyNebula;
                        starfieldBackground.GetComponent<MeshRenderer>().material = storyStarfield;
                        // Finally, change boundary distance
                        currentBoundaryDistance = storyBoundaryDistance;
                        currentBlackoutStart = storyBlackoutStart;
                        transitionLogicPerformed = true;
                        // Spawn asteroids and aliens
                        int numberOfAliens = FindObjectOfType<DialogueController>().dialogueInfoCount;
                        spawningController.InitializeAliens(transform.position, numberOfAliens);
                        spawningController.InitializeAsteroidField(transform.position);
                    }
                    break;
                case Menu.selection.RETURNING:
                    if ( currentSeconds > 2.0f && !transitionLogicPerformed )
                    {
                        // Enable the menu
                        menuObject.SetActive(true);
                        // Stop player from moving
                        rb.velocity.Set( 0.0f, 0.0f, 0.0f );
                        // First, set the player to the start position
                        rb.MovePosition( startingPosition );
                        rb.rotation = Quaternion.Euler( new Vector3( 0, 0, 0 ) );
                        // Then, swap out the backgrounds
                        nebulaBackground.GetComponent<MeshRenderer>().material = menuNebula;
                        starfieldBackground.GetComponent<MeshRenderer>().material = menuStarfield;
                        // Finally, change boundary distance
                        currentBoundaryDistance = menuBoundaryDistance;
                        currentBlackoutStart = menuBlackoutStart;
                        transitionLogicPerformed = true;
                        // Clean asteroids
                        spawningController.ResetSpawners();
                    }
                    break;
            }
        }
    }

    private void PlayerBoundaryCheck()
    {
        // Get distance from start position
        float distFromStart = Vector3.Distance( gameObject.transform.position, startingPosition );
        // Get distance where static should begin
        float staticStartDist = currentBoundaryDistance * currentBlackoutStart;
        // If player has passed that point
        if ( distFromStart - staticStartDist > 0.0f )
        {
            // Losing connection begin
            connectionText.SetActive( true );
            // Get amount of progression between the point of static starting and the boundary
            float boundaryProgression = Mathf.Min( (distFromStart - staticStartDist) / (currentBoundaryDistance - staticStartDist), 1.0f );
            // Set the static (or noise) amount
            renderTex.SetFloat( "_BlackoutAmount", boundaryProgression );
            // Return player to start position if they go off-screen
            if ( distFromStart > currentBoundaryDistance )
            {
                renderTex.SetFloat( "_BlackoutAmount", 0.0f );
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

        directionalMesh.transform.position = transform.position + controlVector * directionalMeshDistance;
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
        if (other.gameObject.CompareTag("Alien") && shipState != ShipState.ENCOUNTER)
        {
            var alienController = other.gameObject.GetComponent<AlienController>();
            if (alienController.WakeAlien(gameObject))
            {
                ChangeState(ShipState.ENCOUNTER);
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
                resetRotation = true;
                break;
            case ShipState.ENCOUNTER:
                if (FindObjectOfType<DialogueController>().DialogueExhausted())
                {
                    missionControlCamera.GetComponent<MissionControlCamera>().BeginTransition(Menu.selection.RETURNING);
                }

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
                rb.AddForce(transform.forward * movementForce * Time.fixedDeltaTime);
                psThrust.Play();
                break;
            case ShipState.SPIN_OUT:
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