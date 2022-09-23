using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawningController : MonoBehaviour
{
    [Header( "RK_References" )]
    private GameObject playerObject;
    [Header( "RK_Prefabs" )]
    [SerializeField] private GameObject asteroidPrefab;
    [SerializeField] private GameObject alienPrefab;

    [Header( "RK_Asteroids" )]
    [SerializeField] private int numAsteroids;
    [SerializeField] private float asteroidSpawnRange;
    [SerializeField] private float asteroidActiveRange = 10.0f;
    [SerializeField] private Vector2 asteroidScaleRange;
    [SerializeField] private Vector2 asteroidRotationSpeed;
    [SerializeField] private Mesh[] asteroidMeshes;

    [Header("RK_Aliens")]
    [SerializeField] private float minDistanceToAliens = 20.0f;

    private List<GameObject> asteroidCollection = new List<GameObject>();
    private float activeRangeSquared;

    private void Awake()
    {
        playerObject = FindObjectOfType<CapsuleController>().gameObject;
        
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Starting");
        activeRangeSquared = asteroidActiveRange * asteroidActiveRange;
        InitializeAsteroidField(playerObject.transform.position);
    }

    public int avgFrameRate;

    // Update is called once per frame
    void Update()
    {
        UpdateAsteroids();
    }

    private void UpdateAsteroids()
    {
        foreach (var asteroid in asteroidCollection)
        {
            Vector3 playerDistance = playerObject.transform.position - asteroid.transform.position;
            asteroid.SetActive(playerDistance.sqrMagnitude < activeRangeSquared);
        }
    }

    public void InitializeAsteroidField(Vector3 origin)
    {
        Debug.Log("Init: " + numAsteroids);

        for (int i = 0; i < numAsteroids; i++)
        {
            Debug.Log("Init: " + i);

            // Instantiate asteroid
            GameObject asteroidInstance = Instantiate(asteroidPrefab, Vector3.zero, Quaternion.identity);

            // Choose random mesh and add mesh collider
            asteroidInstance.GetComponent<MeshFilter>().mesh = asteroidMeshes[Random.Range(0, asteroidMeshes.Length - 1)];
            var meshCollider = asteroidInstance.AddComponent<MeshCollider>();
            meshCollider.convex = true;

            // Set spawn position, scale, and rotation axis/speed
            Vector3 spawnPosition;
            float asteroidScale = Random.Range(asteroidScaleRange.x, asteroidScaleRange.y);
            Vector3 rotationAxis = new Vector3(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
            rotationAxis.Normalize();
            rotationAxis *= Random.Range(asteroidRotationSpeed.x, asteroidRotationSpeed.y) * asteroidScale;

            // Get an empty spawn position
            int loopLimit = 100;
            do
            {
                spawnPosition = Random.insideUnitSphere;
                spawnPosition *= asteroidSpawnRange;
                spawnPosition += playerObject.transform.position;
                spawnPosition.y = -10.0f;

                loopLimit--;

                // Prevent infinite loop, if this happens there are likely too many objects in range
                if (loopLimit < 0)
                {
                    Debug.LogWarning("Asteroid initialisation exited early: Couldn't find an appropriate spawn location.");
                    return;
                }
            }
            while (Physics.CheckSphere(spawnPosition, asteroidScale * 1.1f));

            // Apply position, scale, rotation
            asteroidInstance.SetActive(true);   // Activate asteroid so it comes up in future loop's spawn checking
            Rigidbody rb = asteroidInstance.GetComponent<Rigidbody>();
            rb.angularDrag = 0.0f;

            rb.AddRelativeTorque(rotationAxis, ForceMode.Impulse);
            asteroidInstance.transform.localScale = Vector3.one * asteroidScale;
            asteroidInstance.transform.SetPositionAndRotation(spawnPosition, Quaternion.identity);

            asteroidCollection.Add(asteroidInstance);
        }

        UpdateAsteroids();
    }
}