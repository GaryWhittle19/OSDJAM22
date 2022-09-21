using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PuzzleController : MonoBehaviour
{
    [SerializeField] private float radius = 2.0f;
    [SerializeField] private float depth = 1.0f;
    [SerializeField] private int degreesPerVertex = 5;
    [SerializeField][Range(0.0f, 360.0f)] private float openingAngle = 30.0f;
    [SerializeField][Range(0.0f, 1.0f)] private float innerRadiusPct = 0.8f;
    private MeshFilter meshFilter;
    private Vector3[] capVertices;
    private Vector3[] edgeVertices;

    // Start is called before the first frame update
    void Start()
    {
        Init();
    }
    private void Init()
    {
        meshFilter = gameObject.GetComponent<MeshFilter>();

        float zPos = Vector3.zero.z;

        // Circle
        // Calculate number of vertices in new shape
        capVertices = new Vector3[((360 - (int)openingAngle) / degreesPerVertex)];
        edgeVertices = new Vector3[((360 - (int)openingAngle) / degreesPerVertex) * 2];

        for (int i = 0; i < capVertices.Length; i += 2)
        {
            // Generate point in 2D circle around origin
            Vector3 upperPoint = FindPointInCircle(i * degreesPerVertex, capVertices[0], radius);
            Vector3 innerPoint = FindPointInCircle(i * degreesPerVertex, capVertices[0], radius * innerRadiusPct);
            Vector3 lowerPoint = upperPoint;
            lowerPoint.z -= depth;

            // Add two new vertices to array
            capVertices[i] = upperPoint;
            capVertices[i + 1] = innerPoint;

            edgeVertices[i] = upperPoint;
            edgeVertices[i + 1] = lowerPoint;
            edgeVertices[edgeVertices.Length - i - 1] = upperPoint;
            edgeVertices[edgeVertices.Length - i - 2] = lowerPoint;


            GameObject varNext = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            varNext.transform.parent = gameObject.transform;
            varNext.transform.localPosition = upperPoint;
            varNext.transform.localScale = Vector3.one * 0.1f;

            varNext = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            varNext.transform.parent = gameObject.transform;
            varNext.transform.localPosition = innerPoint;
            varNext.transform.localScale = Vector3.one * 0.1f;
        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    public void GenerateCap()
    {
        Init();

        // Functionality from https://forum.unity.com/threads/programmatically-create-shapes.299160/
        int triangleIndices = (3 * (capVertices.Length - 2)); // I use triangles so I have 3 * number of vertices to plot
        int C0 = 0;
        int C1 = 1;
        int C2 = 2;
        int C3 = 3;
        int[] newTriangles = new int[triangleIndices];

        for (int i = 0; i < triangleIndices; i += 6)
        {
            newTriangles[i] = C0; // each triangle will start an end from the origin (centre) of my shape
            newTriangles[i + 1] = C1;
            newTriangles[i + 2] = C2;

            if (i + 5 < triangleIndices)
            {
                newTriangles[i + 3] = C2; // each triangle will start an end from the origin (centre) of my shape
                newTriangles[i + 4] = C1;
                newTriangles[i + 5] = C3;
            }

            C0+=2;
            C1+=2;
            C2+=2;
            C3+=2;
        }

        Mesh mesh = meshFilter.sharedMesh; // get the MeshFilter component of this game object
        mesh.vertices = capVertices; // set the points in space
        mesh.SetIndices(newTriangles, MeshTopology.Triangles, 0); // set the order in which the vertices will be drawn

    }

    public void GenerateEdge()
    {
        Init();

        // Functionality from https://forum.unity.com/threads/programmatically-create-shapes.299160/
        int triangleIndices = (3 * (edgeVertices.Length - 2)); // I use triangles so I have 3 * number of vertices to plot
        int C0 = 0;
        int C1 = 1;
        int C2 = 2;
        int C3 = 3;
        int[] newTriangles = new int[triangleIndices];

        for (int i = 0; i < triangleIndices; i += 6)
        {
            newTriangles[i] = C2; // each triangle will start an end from the origin (centre) of my shape
            newTriangles[i + 1] = C1;
            newTriangles[i + 2] = C0;

            if (i + 5 < triangleIndices)
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

        Mesh mesh = meshFilter.sharedMesh; // get the MeshFilter component of this game object
        mesh.vertices = edgeVertices; // set the points in space
        mesh.SetIndices(newTriangles, MeshTopology.Triangles, 0); // set the order in which the vertices will be drawn
    }

    private Vector3 FindPointInCircle(float angle, Vector3 origin, float radius)
    {
        Vector3 point = origin;

        point.x = radius * Mathf.Sin(Mathf.Deg2Rad * angle);
        point.y = radius * Mathf.Cos(Mathf.Deg2Rad * angle);

        return point;
    }
}

[CustomEditor(typeof(PuzzleController))]
public class PuzzleControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        PuzzleController puzzleController = (PuzzleController)target;
        if (GUILayout.Button("Generate Cap"))
        {
            puzzleController.GenerateCap();
        }

        if (GUILayout.Button("Generate Edge"))
        {
            puzzleController.GenerateEdge();
        }
    }
}