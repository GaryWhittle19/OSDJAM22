using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionControlCamera : MonoBehaviour
{
    private Camera missionControlCam;

    [System.Serializable]
    public struct PointEntry
    {
        public Vector3 position;
        public float time;
    }
    [System.Serializable]
    public struct CameraEntry
    {
        public Menu.selection situation;
        public List<PointEntry> points;
    }
    [SerializeField]
    public CameraEntry[] entries;

    public List<PointEntry> currentPoints = new List<PointEntry> { };

    private bool cameraTravelling = false;

    // Start is called before the first frame update
    void Start()
    {
        missionControlCam = GetComponent<Camera>();
    }

    private void Update()
    {
        if( !cameraTravelling && currentPoints.Count > 1 )
        {
            StartCoroutine( "Movement" );
            cameraTravelling = true;
        }
    }

    // Update is called once per frame
    IEnumerator Movement()
    {
        // if 0 or only 1 waypoint then it makes no sense to go on
        if ( currentPoints.Count <= 1 ) yield break;

        // pick the first item
        PointEntry fromPoint = currentPoints[0];
        // since you already have the first start the iteration with the second item
        for ( var i = 1; i < currentPoints.Count; i++ )
        {
            PointEntry toPoint = currentPoints[i];
            Vector3 fromPosition = fromPoint.position;
            Vector3 toPosition = toPoint.position;
            float duration = toPoint.time - fromPoint.time;

            // this executes and at the same time waits(yields) until the MoveWithinSeconds routine finished
            yield return MoveWithinSeconds( transform, fromPosition, toPosition, duration );

            // update fromPoint for the next step
            fromPoint = toPoint;
        }
    }

    // Moves a given object from A to B within given duration
    IEnumerator MoveWithinSeconds( Transform obj, Vector3 from, Vector3 to, float duration )
    {
        float timePassed = 0f;
        while ( timePassed < duration )
        {
            // will always be a factor between 0 and 1
            var factor = timePassed / duration;
            // optional ease-in and ease-out
            // factor = Mathf.SmoothStep(0,1,factor);

            // linear interpolate the position
            obj.position = Vector3.Lerp( from, to, factor );

            // increase timePassed by time since last frame
            // using Min to avoid overshooting
            timePassed += Mathf.Min( Time.deltaTime, duration - timePassed );

            // "pause" the routine here, render the frame
            // and continue from here in the next one
            yield return null;
        }

        // just to be sure apply the target position in the end and check if we reached the end
        obj.position = to;
        if( obj.position == currentPoints[currentPoints.Count - 1].position )
        {
            currentPoints.Clear();
            cameraTravelling = false;
            yield break;
        }
    }

    public void BeginTransition( Menu.selection type )
    {
        foreach ( CameraEntry entry in entries )
        {
            if ( entry.situation == type )
            {
                currentPoints = new List<PointEntry>(entry.points);
            }
        }
    }
}
