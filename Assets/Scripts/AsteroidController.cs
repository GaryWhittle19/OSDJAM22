using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidController : MonoBehaviour
{
    [Header( "RK_References" )]
    [SerializeField] private GameObject playerObject;
    [Header( "RK_Meshes" )]
    [SerializeField] private GameObject asteroidPrefab;
    [Header( "RK_Asteroids" )]
    [SerializeField] private int numAsteroids;
    [SerializeField] private float spawnRange;
    [SerializeField] private Vector2 scaleRange;
    [SerializeField] private Vector2 rotationSpeed;
    [SerializeField] private bool generateNewAsteroids;

    private List<GameObject> asteroidCollection = new List<GameObject>();
    private bool firstPass = true;
    private int firstPassCount = 0;

    // Start is called before the first frame update
    void Start()
    {
        float astRadius = asteroidPrefab.GetComponent<BoxCollider>().bounds.extents.x * 2;

        for ( int i = 0; i < numAsteroids; i++ )
        {
            GameObject asteroidInst = Instantiate( asteroidPrefab, Vector3.zero, Quaternion.identity );
            asteroidInst.SetActive( false );
            asteroidCollection.Add( asteroidInst );
        }
    }

    // Update is called once per frame
    void Update()
    {
        if ( !firstPass && !generateNewAsteroids ) { return; }

        // Prune existing asteroids
        foreach ( var asteroid in asteroidCollection )
        {
            if ( Vector3.Distance( asteroid.transform.position, playerObject.transform.position ) > spawnRange )
            {
                asteroid.SetActive( false );
            }
        }

        // Spawn new asteroids
        SpawnAsteroid();

        if ( firstPassCount++ > numAsteroids )
        {
            firstPass = false;
        }
    }

    void SpawnAsteroid()
    {
        GameObject asteroidInst = new GameObject();
        bool found = false;
        // Find inactive asteroid
        foreach ( var asteroid in asteroidCollection )
        {
            if ( asteroid.activeInHierarchy == false )
            {
                asteroidInst = asteroid;
                found = true;
            }
        }

        if ( found )
        {
            Vector3 spawnPosition = Random.insideUnitSphere;
            spawnPosition *= spawnRange;
            spawnPosition += playerObject.transform.position;
            spawnPosition.y = -10.0f;
            asteroidInst.transform.SetPositionAndRotation( spawnPosition, Quaternion.identity );
            float astScale = Random.Range( scaleRange.x, scaleRange.y );
            float instRad = 1.1f * astScale; // What does instRad mean? Setting lower for better asteroid placement
            Rigidbody rb = asteroidInst.GetComponent<Rigidbody>();
            Vector3 rotationAxis = new Vector3( Random.Range( 0.0f, 1.0f ), Random.Range( 0.0f, 1.0f ), Random.Range( 0.0f, 1.0f ) );
            rotationAxis.Normalize();
            rb.angularDrag = 0.0f;
            asteroidInst.transform.localScale = new Vector3( astScale, astScale, astScale );
            if ( Physics.CheckSphere( spawnPosition, instRad ) == false )
            {
                asteroidInst.SetActive( true );
                rotationAxis *= Random.Range( rotationSpeed.x, rotationSpeed.y ) * astScale;
                rb.AddRelativeTorque( rotationAxis, ForceMode.Impulse );
            }
        }
    }
}