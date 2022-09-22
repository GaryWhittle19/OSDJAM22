using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuOption : MonoBehaviour
{
    public Menu.selection type;

    private void OnTriggerEnter( Collider other )
    {
        transform.parent.GetComponent<Menu>().MenuOptionChosen( this );
    }
}
