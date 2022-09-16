#define DEBUG_MODE

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlienEncounter : MonoBehaviour
{
    [SerializeField] GameObject player;
    [SerializeField] float encounterRadius = 2.5f;

    bool encounterActive = false;
    bool encounterComplete = false;

    // Update is called once per frame
    void Update()
    {
        if( encounterComplete ) { return; };

        Vector3 vectorToPlayer = player.transform.position - transform.position;
        vectorToPlayer.y = 0.0f;

        if( vectorToPlayer.magnitude < encounterRadius && !encounterActive )
        {
            encounterActive = true;
            player.GetComponent<CapsuleController>().ChangeState(CapsuleController.ShipState.ENCOUNTER);
        }

#if DEBUG_MODE
         Debug.DrawLine(transform.position, transform.position + vectorToPlayer, Color.green, -1, false);
#endif // DEBUG_MODE
    }
}
