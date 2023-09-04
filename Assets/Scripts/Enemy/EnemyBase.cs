using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    public enum State
    {
        Idle,
        Chase,
        Steady,
        Attack,
        Die,
    }

    public static event Action<GameObject, float> OnEnemyGiveDamage;
    public static event Action<GameObject, float> OnEnemyDead;

    [SerializeField] State currentState;

    public EnemyStatsDatabase statDB;
    public EnemyStats stat;

    [Header("Current Stats")]
    [SerializeField] float _hp;

    [Header("Idle Parameter")]
    [SerializeField] Vector2 idleTime;
    float _idleTime;

    [Header("Chase Parameter")]
    [SerializeField] Vector2 rotateSpeed;
    float _rotateSpeed;

    [Header("Attack Parameter")]
    [SerializeField] Vector3 rayOffset;
    [SerializeField] float scanDistance = 5f;
    [SerializeField] float attackDistance = 5f;
    [SerializeField] float rayAngle = 60f;
    [SerializeField] int rayRes = 10;
    [SerializeField] float attackDelay;
    [SerializeField] float stepDistance;

    [Header("Item Drop")]
    public GameObject HpDropPrefab;
    public GameObject ExpDropPrefab;
    [SerializeField] float dropTimer;

    bool isRun;

    GameObject player;
    Animator anim;
    Rigidbody rb;
    Collider col;

    private void Awake()
    {
        CharController.OnPlayerGiveDamage += ReceiveDamage;
        SkillCollider.OnGiveDamage += ReceiveDamage;
    }

    private void OnDestroy()
    {
        CharController.OnPlayerGiveDamage -= ReceiveDamage;
        SkillCollider.OnGiveDamage -= ReceiveDamage;
    }

    private void ReceiveDamage(GameObject enemy, float damage)
    {
        if(enemy == this.gameObject)
        {
            _hp -= damage;
            if(currentState == State.Idle || currentState == State.Chase)
            {
                anim.SetTrigger("Attacked");
            }

            if(_hp <= 0)
            {
                UpdateState(State.Die);
            }
        }
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        SetStat();        

        transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(-360f, 360f), 0);

        UpdateState(State.Chase);
    }

    private void Update()
    {        
        if(currentState == State.Chase)
        {
            Chase();
        }
        else if(currentState == State.Steady)
        {
            LookAtPlayer();
        }
        else if(currentState == State.Attack)
        {
            transform.Translate(Vector3.forward * stepDistance * Time.deltaTime);
        }
        else if(currentState == State.Die)
        {

        }
    }

    private void FixedUpdate()
    {
        rb.AddForce(Physics.gravity, ForceMode.Force);
    }

    /*private void LateUpdate()
    {
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);
    }*/

    void SetStat()
    {
        int random = UnityEngine.Random.Range(0, EnemySpawner.Instance.Wave);
        stat = statDB.Stats[random];

        gameObject.name = SetName();

        _hp = stat.MaxHp;
    }

    string SetName()
    {
        return $"{gameObject.name} - {stat.Level} - {EnemySpawner.Instance.Wave}";
    }

    public void UpdateState(State newState)
    {
        if(currentState != State.Die)
        {
            currentState = newState;

            switch (currentState)
            {
                case State.Idle:
                    IdleHandler();
                    break;
                case State.Chase:
                    ChaseHandler();
                    break;
                case State.Steady:
                    SteadyHandler();
                    break;
                case State.Attack:
                    AttackHandler();
                    break;
                case State.Die:
                    DieHandler();
                    break;
            }

            anim.SetBool("IsRun", isRun);
        }      
    }

    public void GiveDamage()
    {
        for (int i = -rayRes; i <= rayRes; i++)
        {
            float t = i / (float)rayRes;
            float currentAngle = t * (rayAngle / 2) * Mathf.Deg2Rad;
            Vector3 rayDirection = Quaternion.Euler(0f, currentAngle * Mathf.Rad2Deg, 0f) * transform.forward;

            Debug.DrawLine(transform.position + rayOffset, (transform.position + rayOffset) + rayDirection * attackDistance);

            RaycastHit hit;
            if (Physics.Raycast(transform.position + rayOffset, rayDirection, out hit, attackDistance))
            {
                if (hit.collider.tag == "Player")
                {
                    Debug.Log(hit.collider.name);
                    OnEnemyGiveDamage(this.gameObject, stat.Damage);
                    break;
                }
            }
        }        
    }    

    private void DieHandler()
    {
        anim.SetTrigger("Die");
        col.enabled = false;
        rb.useGravity = false;

        StartCoroutine(DropItemCo());
        OnEnemyDead(this.gameObject, stat.ExpDropped);
        Destroy(this.gameObject, 1.5f);
    }
    IEnumerator DropItemCo()
    {
        yield return new WaitForSeconds(dropTimer);

        float random = UnityEngine.Random.Range(0f, 100f);
        if (random <= 25)
        {
            GameObject hpDrop = Instantiate(HpDropPrefab, transform.position + new Vector3(0, 7.5f, 0), Quaternion.identity);
            hpDrop.GetComponent<HPDrop>().HP = stat.Damage / 2;
            hpDrop.GetComponent<Rigidbody>().AddForce(new Vector3(UnityEngine.Random.Range(-2, 2), 
                                                                    -Physics.gravity.y / 2, 
                                                                    UnityEngine.Random.Range(-2, 2)), 
                                                                    ForceMode.Impulse);
        }        

        yield return new WaitForSeconds(0.25f);

        random = UnityEngine.Random.Range(0f, 100f);
        GameObject expDrop = Instantiate(ExpDropPrefab, transform.position + new Vector3(0, 7.5f, 0), Quaternion.identity);
        expDrop.GetComponent<Rigidbody>().AddForce(new Vector3(UnityEngine.Random.Range(-2, 2),
                                                                    -Physics.gravity.y / 2,
                                                                    UnityEngine.Random.Range(-2, 2)),
                                                                    ForceMode.Impulse);
        if (random <= 50)
            expDrop.GetComponent<EXPDrop>().EXP = stat.ExpDropped * 2;
        else
        {
            expDrop.GetComponent<EXPDrop>().EXP = stat.ExpDropped;
            expDrop.transform.localScale *= 0.75f;
        }

    }


    void IdleHandler()
    {
        //isAttack = false;
        isRun = false;
        _idleTime = UnityEngine.Random.Range(idleTime.x, idleTime.y);
        StartCoroutine(IdleCo());
    }

    IEnumerator IdleCo()
    {
        yield return new WaitForSeconds(_idleTime);

        UpdateState(State.Chase);
    }

    void ChaseHandler()
    {
        isRun = true;
        _rotateSpeed = UnityEngine.Random.Range(rotateSpeed.x, rotateSpeed.y);
    }

    void Chase()
    {
        transform.Translate(Vector3.forward * stat.MovementSpeed * Time.deltaTime);

        LookAtPlayer();
        ScanPlayer();
    }

    void LookAtPlayer()
    {
        Vector3 relativePos = player.transform.position - transform.position;
        Quaternion targetRot = Quaternion.LookRotation(relativePos, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, new Quaternion(0, targetRot.y, 0, targetRot.w), _rotateSpeed * Time.deltaTime);
    }

    void ScanPlayer()
    {
        for (int i = -rayRes; i <= rayRes; i++)
        {
            float t = i / (float)rayRes;
            float currentAngle = t * (rayAngle / 2) * Mathf.Deg2Rad;
            Vector3 rayDirection = Quaternion.Euler(0f, currentAngle * Mathf.Rad2Deg, 0f) * transform.forward;

            Debug.DrawLine(transform.position + rayOffset, (transform.position + rayOffset) + rayDirection * scanDistance);

            RaycastHit hit;
            if (Physics.Raycast(transform.position + rayOffset, rayDirection, out hit, scanDistance))
            {               
                if (hit.collider.tag == "Player")
                {
                    Debug.Log(hit.collider.name);
                    UpdateState(State.Steady);
                    break;
                }
            }
        }
    }

    void SteadyHandler()
    {
        isRun = false;
        StartCoroutine(SteadyCo());
    }

    IEnumerator SteadyCo()
    {
        yield return new WaitForSeconds(attackDelay);
        if(currentState != State.Die)
            anim.SetTrigger("Attack");

        //UpdateStates() USING ANIMATION EVENT --> CHECK ANIMATION TAB
    }

    void AttackHandler()
    {
        //isAttack = true;       
    }

    /*private void OnDrawGizmos()
    {
        // Visualize the fan-shaped area using Gizmos
        Vector3 direction = transform.forward;
        float halfAngle = angle * 0.5f;

        for (int i = -res; i <= res; i++)
        {
            float t = i / (float)res;
            float currentAngle = t * halfAngle * Mathf.Deg2Rad;
            Vector3 offset = Quaternion.Euler(0f, currentAngle * Mathf.Rad2Deg, 0f) * direction * radius;
            Vector3 start = transform.position;
            Vector3 end = transform.position + offset;
            Gizmos.DrawLine(start, end);
        }
    }*/
}
