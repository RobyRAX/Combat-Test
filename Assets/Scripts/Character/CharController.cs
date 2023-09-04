using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;



public class CharController : MonoBehaviour
{
    public static event Action<GameObject, float> OnPlayerGiveDamage;
    public static event Action<float, float> OnHpChange;
    public static event Action<float, float> OnExpChange;
    public static event Action<int, bool> OnLevelUp;
    public static event Action OnPlayerDead;

    [Serializable]
    public struct ComboParameter
    {
        public float comboStepDelay;
        public float comboStepDistance;
    }
    
    public CharStatsDatabase statDB;
      
    public CharStats baseStat;
    public CharStats upgradeStat;
    public CharStats currentStat;

    [Header("Current Stats")]
    //[SerializeField] int _level = 1;
    [SerializeField] float _hp;
    public float CurrentHP
    {
        get { return _hp; }
        set
        {
            _hp = value;
            OnHpChange(_hp, currentStat.MaxHp);
        }
    }
    [SerializeField] float _exp;
    public float CurrentEXP
    {
        get { return _exp; }
        set
        {
            _exp = value;
            OnExpChange(_exp, baseStat.MaxExp);
        }
    }


    [Header("Dash Parameter")]
    [SerializeField] float dashDistance;
    [SerializeField] float dashDuration;

    [Header("Attack Parameter")]
    [SerializeField] float comboBreakTimer;
    [SerializeField] ComboParameter[] combos;
    [SerializeField] Vector3 rayOffset;
    [SerializeField] float rayDistance = 5f; 
    [SerializeField] float rayAngle = 60f; 
    [SerializeField] int rayRes = 10;
    float _comboBreakTimer;
    float _comboStepDelay;
    int _comboStep;

    Vector2 inputValue;
    bool isDash;
    bool isMove;
    bool isAttack;
    bool isStep;
    bool canAttack = true;
    bool isDead;

    PlayerInputAction playerInput;
    Rigidbody rb;
    Animator anim;
    Collider col;

    private void Awake()
    {
        playerInput = new PlayerInputAction();
        playerInput.Enable();
        playerInput.Char.Dash.performed += DashPerformedHandler;
        playerInput.Char.Movement.performed += MovementHandler;
        playerInput.Char.Movement.canceled += MovementHandler;
        playerInput.Char.Attack.performed += AttackHandler;       

        GameManager.OnGameStateChange += GameStateChangeHandler;

        EnemyBase.OnEnemyGiveDamage += ReceiveDamage;
        //EnemyBase.OnEnemyDead += ReceiveExp;
    }

    void GameStateChangeHandler(GameState state)
    {
        if(state == GameState.GameInit)
        {
            SetBaseStats();
            SetCurrentStats();

            CurrentEXP = 0.0f;
        }
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        col = GetComponent<Collider>();

        //SetStats();

        currentStat.Level = 1;       
    }

    private void Update()
    {
        if(!isDead)
        {
            Walk();
            AttackDelay();
        }
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "HPDrop")
        {
            ReceiveHP(collision.gameObject.GetComponent<HPDrop>().HP);

            Destroy(collision.gameObject);
        }
        if (collision.gameObject.tag == "EXPDrop")
        {
            ReceiveExp(collision.gameObject.GetComponent<EXPDrop>().EXP);

            Destroy(collision.gameObject);
        }
    }

    public void SetBaseStats()
    {
        baseStat.Level = statDB.stats[currentStat.Level - 1].Level;
        baseStat.MaxExp = statDB.stats[currentStat.Level - 1].MaxExp;
        baseStat.MaxHp = statDB.stats[currentStat.Level - 1].MaxHp;
        baseStat.Damage = statDB.stats[currentStat.Level - 1].Damage;
        baseStat.CritChance = statDB.stats[currentStat.Level - 1].CritChance;
        baseStat.MovementSpeed = statDB.stats[currentStat.Level - 1].MovementSpeed;

        if (currentStat.Level != 1)
        {
            CurrentHP += (baseStat.MaxHp - statDB.stats[currentStat.Level - 2].MaxHp);
        }
    }

    public void SetCurrentStats()
    {
        currentStat.MaxExp = baseStat.MaxExp;
        currentStat.MaxHp = baseStat.MaxHp + upgradeStat.MaxHp;
        currentStat.Damage = baseStat.Damage + upgradeStat.Damage;
        currentStat.CritChance = baseStat.CritChance + upgradeStat.CritChance;
        currentStat.MovementSpeed = baseStat.MovementSpeed + upgradeStat.MovementSpeed;      

        if (currentStat.Level == 1)
            CurrentHP = currentStat.MaxHp;           
    }

    private void ReceiveDamage(GameObject enemy, float damage)
    {
        CurrentHP -= damage;

        if (CurrentHP <= 0 && !isDead)
        {
            isDead = true;
            anim.SetTrigger("Dead");

            col.enabled = false;
            rb.useGravity = false;

            OnPlayerDead();
        }
    }

    private void ReceiveExp(float expIn)
    {
        CurrentEXP += expIn;
        if(CurrentEXP >= currentStat.MaxExp)
        {
            if(currentStat.Level < statDB.stats.Length)
            {
                LevelUp();
            }
            else
            {
                CurrentEXP = currentStat.MaxExp;
            }
        }
    }

    void ReceiveHP(float hpIn)
    {
        CurrentHP += hpIn;

        if (CurrentHP > currentStat.MaxHp)
            CurrentHP = currentStat.MaxHp;
    }

    void LevelUp()
    {
        float subtract = CurrentEXP - currentStat.MaxExp;
        currentStat.Level++;
        SetBaseStats();
        SetCurrentStats();

        CurrentEXP = subtract;
        OnLevelUp(currentStat.Level, currentStat.Level == statDB.stats.Length);
    }

    private void AttackHandler(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if(canAttack && !isDash)
            {
                canAttack = false;
                isAttack = true;
                _comboBreakTimer = comboBreakTimer;
                _comboStepDelay = combos[0].comboStepDelay;               
                Combo();
            }                 
        }
    }

    void AttackDelay()
    {
        anim.SetBool("IsAttack", isAttack);

        if (isAttack)
        {
            _comboStepDelay -= Time.deltaTime;
            if (_comboStepDelay <= 0)
            {
                canAttack = true;
            }

            if(canAttack)
            {
                _comboBreakTimer -= Time.deltaTime;
                if (_comboBreakTimer <= 0)
                {
                    isAttack = false;
                    _comboStep = 0;
                }
            }     
            
            if(isStep)
            {
                transform.Translate(Vector3.forward * combos[_comboStep].comboStepDistance * Time.deltaTime);
            }
        }
    }

    public void EnableStep()
    {
        isStep = true;
    }
    public void DisableStep()
    {
        isStep = false;
    }

    void Combo()
    {
        _comboStepDelay = combos[_comboStep].comboStepDelay;
        _comboStep++;

        anim.SetInteger("ComboStep", _comboStep);

        if (_comboStep == combos.Length)
        {
            _comboStep = 0;
        }
    }

    public void GiveDamage()
    {
        for (int i = -rayRes; i <= rayRes; i++)
        {
            float t = i / (float)rayRes;
            float currentAngle = t * (rayAngle / 2) * Mathf.Deg2Rad;
            Vector3 rayDirection = Quaternion.Euler(0f, currentAngle * Mathf.Rad2Deg, 0f) * transform.forward;

            Debug.DrawLine(transform.position + rayOffset, (transform.position + rayOffset) + rayDirection * rayDistance, Color.green);

            RaycastHit hit;
            if (Physics.Raycast(transform.position + rayOffset, rayDirection, out hit, rayDistance))
            {
                //Debug.Log(hit.collider.name);
                if (hit.collider.tag == "Enemy")
                {
                    if(CalculateCritical() == true)
                    {
                        OnPlayerGiveDamage(hit.collider.gameObject, currentStat.Damage * 2);
                        Debug.Log($"{hit.collider.name} - CRITICAL");
                    }                        
                    else
                    {
                        OnPlayerGiveDamage(hit.collider.gameObject, currentStat.Damage);
                        Debug.Log($"{hit.collider.name}");
                    }                        
                    break;
                }
            }
        }       
    }

    bool CalculateCritical()
    {
        float random = UnityEngine.Random.Range(0f, 100f);

        if(random <= currentStat.CritChance)
        {
            return true;
        }
        else
        {
            return false;
        }    
    }

    void Walk()
    {
        if(!isDash)
        {
            float angle = Mathf.Atan2(inputValue.x, inputValue.y) * Mathf.Rad2Deg;
            Quaternion rotationTarget = Quaternion.Euler(new Vector3(0, angle + 45, 0));
            transform.rotation = Quaternion.Slerp(transform.rotation, rotationTarget, 35 * Time.deltaTime);
        }
        
        if (isMove && !isDash && !isAttack)
        {           
            transform.Translate(Vector3.forward * currentStat.MovementSpeed * Time.deltaTime);
        }    
    }

    private void MovementHandler(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            isMove = true;
            inputValue = playerInput.Char.Movement.ReadValue<Vector2>();
        }
        else if(context.canceled)
        {
            isMove = false;
        }
        anim.SetBool("IsMove", isMove);
    }

    void DashPerformedHandler(InputAction.CallbackContext context)
    {
        if(!isDash)
        {
            //RESET COMBO
            canAttack = true;
            isAttack = false;
            _comboStep = 0;

            StartCoroutine(DashCo());
            anim.SetTrigger("IsDash");
        }
            
    }

    private IEnumerator DashCo()
    {
        isDash = true;
        if(isDash)
        {
            Vector3 dashDirection = transform.forward; // Dash in the direction the player is facing
            Vector3 dashVelocity = dashDirection * (dashDistance / dashDuration);
            rb.velocity = dashVelocity;
        }
        
        yield return new WaitForSeconds(dashDuration);

        rb.velocity = Vector3.zero;
        isDash = false;
    }
}
