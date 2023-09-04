using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EXPDrop : ItemDropBase
{
    [SerializeField] float exp;
    [SerializeField] float destroyTimer;

    public float EXP
    {
        get { return exp; }
        set { exp = value; }
    }
    void Start()
    {
        Destroy(this.gameObject, destroyTimer);
    }

}
