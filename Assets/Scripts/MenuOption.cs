using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuOption : MonoBehaviour
{
    public Menu.selection type;

    // Simple class, we just want to flag triggers to the parent Menu object.
    private void OnTriggerEnter( Collider other )
    {
        transform.parent.GetComponent<Menu>().MenuOptionChosen( this );
    }
}
