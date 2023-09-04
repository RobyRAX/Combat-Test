using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpController : MonoBehaviour
{
    public CharController player;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "HPDrop" || other.gameObject.tag == "EXPDrop")
        {
            other.GetComponent<ItemDropBase>().IsPicked = true;
            other.GetComponent<ItemDropBase>().Speed = player.currentStat.MovementSpeed + 2f;
        }
    }
}
