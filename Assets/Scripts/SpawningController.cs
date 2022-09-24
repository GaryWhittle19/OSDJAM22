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
    
    private List<GameObject> asteroidCollection = new List<GameObject>();
    private float activeRangeSquared;

    [Header("RK_Aliens")]
    [SerializeField] private float minDistanceToAliens = 20.0f;
    [SerializeField] private float alienSpawnRange = 100.0f;
    private List<GameObject> alienCollection = new List<GameObject>();

    [Header("RK_Misc")]
    [SerializeField] private int spawnLoopAttemps = 100;


    private void Awake()
    {
        playerObject = FindObjectOfType<CapsuleController>().gameObject;
        
    }

    // Start is called before the first frame update
    void Start()
    {
        activeRangeSquared = asteroidActiveRange * asteroidActiveRange;
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

    public void InitializeAliens(Vector3 origin, int numberOfAliens)
    {
        for (int i = 0; i < numberOfAliens; i++)
        {
            // Get spawn parameters
            Vector3 spawnPosition;
            float alienScale = alienPrefab.GetComponent<Collider>().bounds.size.magnitude;
            Debug.Log("Alien scale: " + alienScale);
            GetSpawnPosition(out spawnPosition, alienScale, alienSpawnRange, origin);

            GameObject alienInstance = Instantiate(alienPrefab, spawnPosition, Quaternion.identity);
            alienCollection.Add(alienInstance);
        }
    }

    public void InitializeAsteroidField(Vector3 origin)
    {
        for (int i = 0; i < numAsteroids; i++)
        {
            // Instantiate asteroid
            GameObject asteroidInstance = Instantiate(asteroidPrefab, Vector3.zero, Quaternion.identity);

            // Choose random mesh and add mesh collider
            var asteroidMesh = asteroidMeshes[Random.Range(0, asteroidMeshes.Length - 1)];
            var meshFilter = asteroidInstance.GetComponent<MeshFilter>();
            meshFilter.sharedMesh = asteroidMesh;
            
            var meshCollider = asteroidInstance.GetComponent<MeshCollider>();
            meshCollider.sharedMesh = meshFilter.sharedMesh;

            // Set spawn position, scale, and rotation axis/speed
            Vector3 spawnPosition;
            float asteroidScale = Random.Range(asteroidScaleRange.x, asteroidScaleRange.y);
            Vector3 rotationAxis = new Vector3(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
            rotationAxis.Normalize();
            rotationAxis *= Random.Range(asteroidRotationSpeed.x, asteroidRotationSpeed.y) * asteroidScale;

            // Get an empty spawn position
            if (GetSpawnPosition(out spawnPosition, asteroidScale * 1.1f, asteroidSpawnRange, origin) == false)
                return;

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

    private bool GetSpawnPosition(out Vector3 spawnPosition, float checkAreaScale, float spawnRange, Vector3 origin)
    {
        // Get an empty spawn position
        int loopLimit = spawnLoopAttemps;
        do
        {
            spawnPosition = Random.insideUnitSphere;
            spawnPosition *= spawnRange;
            spawnPosition += origin;
            spawnPosition.y = -10.0f;

            loopLimit--;

            // Prevent infinite loop, if this happens there are likely too many objects in range
            if (loopLimit < 0)
            {
                Debug.LogWarning("Asteroid initialisation exited early: Couldn't find an appropriate spawn location.");
                return false;
            }
        }
        while (Physics.CheckSphere(spawnPosition, checkAreaScale));

        return true;
    }

    public void ResetSpawners()
    {
        foreach (var asteroid in asteroidCollection)
        {
            Destroy(asteroid);
        }

        asteroidCollection.Clear();

        foreach (var alien in alienCollection)
        {
            Destroy(alien);
        }

        alienCollection.Clear();
    }

}