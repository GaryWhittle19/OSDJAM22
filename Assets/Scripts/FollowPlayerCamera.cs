using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayerCamera : MonoBehaviour
{
    [SerializeField] GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if ( !player ) { return; };

        transform.position = new Vector3( player.transform.position.x, transform.position.y, player.transform.position.z );
    }
}
