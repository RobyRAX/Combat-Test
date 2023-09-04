using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillCollider : MonoBehaviour
{
    public static event Action<GameObject, float> OnGiveDamage;

    float speed;
    public float Speed
    {
        get { return speed; }
        set { speed = value; }
    }
    float damage;
    public float Damage
    {
        get { return damage; }
        set { damage = value; }
    }

    private void Start()
    {
        Destroy(this.gameObject, 5f);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Enemy")
        {
            OnGiveDamage(other.gameObject, Damage);
        }
    }
}
