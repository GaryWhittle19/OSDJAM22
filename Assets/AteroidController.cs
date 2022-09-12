using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AteroidController : MonoBehaviour
{
    [SerializeField] private GameObject asteroidPrefab;
    [SerializeField] private GameObject playerObject;
    [SerializeField] private int numAsteroids;
    [SerializeField] private float spawnRange;
    [SerializeField] private Vector2 scaleRange;
    [SerializeField] private float rotationSpeed = 3.5f;

    private List<GameObject> asteroidCollection = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {

        float astRadius = asteroidPrefab.GetComponent<BoxCollider>().bounds.extents.x * 2;

        for (int i = 0; i < numAsteroids; i++)
        {
            GameObject asteroidInst = Instantiate(asteroidPrefab, Vector3.zero, Quaternion.identity);
            asteroidInst.SetActive(false);
            asteroidCollection.Add(asteroidInst);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Prune existing asteroids
        foreach (var asteroid in asteroidCollection)
        {
            if (Vector3.Distance(asteroid.transform.position, playerObject.transform.position) > spawnRange)
            {
                asteroid.SetActive(false);
            }
        }

        // Spawn new asteroids
        SpawnAsteroid();
    }

    void SpawnAsteroid()
    {
        GameObject asteroidInst = new GameObject();
        bool found = false;
        // Find inactive asteroid
        foreach (var asteroid in asteroidCollection)
        {
            if (asteroid.activeInHierarchy == false)
            {
                asteroidInst = asteroid;
                found = true;
            }
        }

        if (found)
        {
            Vector3 spawnPosition = Random.insideUnitSphere;
            spawnPosition *= spawnRange;
            spawnPosition += playerObject.transform.position;
            spawnPosition.y = -10.0f;
            asteroidInst.transform.SetPositionAndRotation(spawnPosition, Quaternion.identity);
            float astScale = Random.Range(scaleRange.x, scaleRange.y);
            float instRad = 2.0f * astScale;
            Rigidbody rb = asteroidInst.GetComponent<Rigidbody>();
            Vector3 rotationAxis = new Vector3(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
            rotationAxis.Normalize();
            rb.angularDrag = 0.0f;
            asteroidInst.transform.localScale = new Vector3(astScale, astScale, astScale);
            if (Physics.CheckSphere(spawnPosition, instRad) == false)
            {
                asteroidInst.SetActive(true);
                rotationAxis *= Random.Range(0.01f, 0.2f) * astScale;
                Debug.Log("Torque: " + rotationAxis);
                rb.AddRelativeTorque(rotationAxis, ForceMode.Impulse);
            }
        }

    }

}
