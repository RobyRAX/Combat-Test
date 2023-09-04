using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPDrop : ItemDropBase
{
    [SerializeField] float hp;
    [SerializeField] float destroyTimer;
    public float HP
    {
        get { return hp; }
        set { hp = value; }
    }

    // Start is called before the first frame update
    void Start()
    {
        Destroy(this.gameObject, destroyTimer);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
