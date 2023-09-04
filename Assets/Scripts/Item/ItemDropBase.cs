using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDropBase : MonoBehaviour
{    
    float speed;
    public float Speed
    {
        get { return speed; }
        set { speed = value; }
    }

    bool isPicked;
    public bool IsPicked
    {
        get { return isPicked; }
        set { isPicked = value; }
    }

    private void LateUpdate()
    {
        if(isPicked)
        {
            transform.LookAt(GameObject.FindGameObjectWithTag("Player").transform.position + new Vector3(0, 1, 0));
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
    }


}
