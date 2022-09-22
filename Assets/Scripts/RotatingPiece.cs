using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class RotatingPiece : MonoBehaviour
{

    private float rotatingSpeed =  30.0f;
    private float rotationDirection = 1.0f;
    private bool freeze = false;

    [SerializeField] private float radius = 2.0f;
    [SerializeField] private float depth = 1.0f;
    [SerializeField] private int degreesPerVertex = 5;
    [SerializeField] [Range(0.0f, 360.0f)] private float openingAngle = 30.0f;
    [SerializeField] [Range(0.0f, 1.0f)] private float innerRadiusPct = 0.8f;
    private MeshFilter meshFilter;
    private Vector3[] capVertices;
    private Vector3[] edgeVertices;
    private Vector3[] allVertices;

    // Start is called before the first frame update
    void Start()
    {
        Generate();
    }

    public void ResetVars()
    {
        gameObject.transform.Rotate(Vector3.forward, Random.Range(0.0f, 360.0f));
        freeze = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!freeze)
        {
            gameObject.transform.Rotate(Vector3.forward, rotationDirection * rotatingSpeed * Time.deltaTime);
        }
    }

    private void Init()
    {
        meshFilter = gameObject.GetComponent<MeshFilter>();

        float zPos = Vector3.zero.z;
        openingAngle -= openingAngle % 10;

        // Circle
        // Calculate number of vertices in new shape
        capVertices = new Vector3[((360 - (int)openingAngle) / degreesPerVertex)];
        edgeVertices = new Vector3[((360 - (int)openingAngle) / degreesPerVertex) * 2];

        for (int i = 0; i < capVertices.Length; i += 2)
        {
            // Generate point in 2D circle around origin
            Vector3 upperPoint = FindPointInCircle(i * degreesPerVertex, capVertices[0], radius);   // For outer edge of cap
            Vector3 innerPoint = FindPointInCircle(i * degreesPerVertex, capVertices[0], radius * innerRadiusPct);  // For inner edge of cap
            Vector3 lowerPointOuter = upperPoint;  // For bottom of outer wall of edge
            Vector3 lowerPointInner = innerPoint;  // For bottom of inner wall of edge
            lowerPointOuter.z -= depth;
            lowerPointInner.z -= depth;

            // Add two new vertices to array
            capVertices[i] = upperPoint;
            capVertices[i + 1] = innerPoint;

            edgeVertices[i] = upperPoint;
            edgeVertices[i + 1] = lowerPointOuter;
            edgeVertices[edgeVertices.Length - i - 1] = lowerPointInner;
            edgeVertices[edgeVertices.Length - i - 2] = innerPoint;
        }

        allVertices = new Vector3[capVertices.Length + edgeVertices.Length];
        edgeVertices.CopyTo(allVertices, 0);
        capVertices.CopyTo(allVertices, edgeVertices.Length);
    }

    public void SetRotationValues(float speed, float direction)
    {
        rotationDirection = speed;
        rotationDirection = direction;
    }

    public void Generate()
    {
        Init();

        // Functionality from https://forum.unity.com/threads/programmatically-create-shapes.299160/
        int edgeIndices = (3 * (edgeVertices.Length - 2)); // I use triangles so I have 3 * number of vertices to plot
        int capIndices = (3 * (capVertices.Length - 2)); // I use triangles so I have 3 * number of vertices to plot

        int C0 = 0;
        int C1 = 1;
        int C2 = 2;
        int C3 = 3;
        int[] newTriangles = new int[edgeIndices + capIndices];

        // Generate edge
        for (int i = 0; i < edgeIndices; i += 6)
        {
            newTriangles[i] = C2; // each triangle will start an end from the origin (centre) of my shape
            newTriangles[i + 1] = C1;
            newTriangles[i + 2] = C0;

            if (i + 5 < edgeIndices)
            {
                newTriangles[i + 3] = C3; // each triangle will start an end from the origin (centre) of my shape
                newTriangles[i + 4] = C1;
                newTriangles[i + 5] = C2;
            }

            C0 += 2;
            C1 += 2;
            C2 += 2;
            C3 += 2;
        }

        for (int i = edgeIndices; i < newTriangles.Length; i += 6)
        {
            newTriangles[i] = C0; // each triangle will start an end from the origin (centre) of my shape
            newTriangles[i + 1] = C1;
            newTriangles[i + 2] = C2;

            if (i + 5 < newTriangles.Length)
            {
                newTriangles[i + 3] = C2; // each triangle will start an end from the origin (centre) of my shape
                newTriangles[i + 4] = C1;
                newTriangles[i + 5] = C3;
            }

            C0 += 2;
            C1 += 2;
            C2 += 2;
            C3 += 2;
        }

        Mesh mesh = meshFilter.mesh; // get the MeshFilter component of this game object
        mesh.vertices = allVertices; // set the points in space
        mesh.SetIndices(newTriangles, MeshTopology.Triangles, 0); // set the order in which the vertices will be drawn
        gameObject.GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    private Vector3 FindPointInCircle(float angle, Vector3 origin, float radius)
    {
        Vector3 point = origin;

        point.x = radius * Mathf.Sin(Mathf.Deg2Rad * angle);
        point.y = radius * Mathf.Cos(Mathf.Deg2Rad * angle);

        return point;
    }

    public void Freeze()
    {
        freeze = true;
    }
    public void Flip()
    {
        rotationDirection *= -1.0f;
    }
}

[CustomEditor(typeof(RotatingPiece))]
public class RotatingPieceEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        RotatingPiece puzzleController = (RotatingPiece)target;
        if (GUILayout.Button("Generate Shape"))
        {
            puzzleController.Generate();
        }
    }
}