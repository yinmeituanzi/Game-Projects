using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    [Header("事件监听")]
    public SceneLoadEventSO sceneLoadEvent;
    public VoidEventSO afterSceneLoadedEvent;
    public VoidEventSO loadDataEvent;
    public VoidEventSO backToMenuEvent;

    public PlayerInputControl inputControl;
    private Rigidbody2D rb;
    public Vector2 inputDirection;
    private PhysicsCheck physicsCheck;
    private PlayeraAnimation playeraAnimation;
    private Collider2D coll;
    private Character character;

    [Header("基本参数")]
    public float speed;
    public float jumpForce;
    public float hurtForce;
    public float wallJumpForce;
    public float healingPower;

    [Header("技能冷却")]
    public float cd;
    [HideInInspector] public float cdCounter;
    public bool skillAvailable;
    public UnityEvent<Character> OnSkill;

    [Header("物理材质")]
    public PhysicsMaterial2D normal;
    public PhysicsMaterial2D wall;

    [Header("状态")]   
    public bool isHurt;
    public bool isDead;
    public bool isAttack;
    public bool wallJump;
    public bool isSkill;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        physicsCheck = GetComponent<PhysicsCheck>();
        playeraAnimation = GetComponent<PlayeraAnimation>();
        coll = GetComponent<CapsuleCollider2D>();
        character = GetComponent<Character>();

        inputControl = new PlayerInputControl();

        //跳跃
        inputControl.Gameplay.Jump.started += Jump;
        //攻击
        inputControl.Gameplay.Attack.started += PlayerAttack;
        //技能
        inputControl.Gameplay.Skill.started += Skill;

        inputControl.Enable();
    }

    private void OnEnable()
    {
        
        sceneLoadEvent.LoadRequestEvent += OnLoadEvent;
        afterSceneLoadedEvent.OnEventRaised += OnAfterSceneLoadedEvent;
        loadDataEvent.OnEventRaised += OnLoadDataEvent;
        backToMenuEvent.OnEventRaised += OnLoadDataEvent;
    }

    private void OnDisable()
    {
        inputControl.Disable();
        sceneLoadEvent.LoadRequestEvent -= OnLoadEvent;
        afterSceneLoadedEvent.OnEventRaised -= OnAfterSceneLoadedEvent;
        loadDataEvent.OnEventRaised -= OnLoadDataEvent;
        backToMenuEvent.OnEventRaised -= OnLoadDataEvent;
    }


    private void Update()
    {
        if(!skillAvailable)
        {
            cdCounter -= Time.deltaTime;
            if (cdCounter <= 0)
            {
                skillAvailable = true;
            }
        }

        inputDirection = inputControl.Gameplay.Move.ReadValue<Vector2>();
        inputDirection = inputDirection;
        CheckState();
    }

    private void FixedUpdate()
    {
        if(!isHurt&&!isAttack)
        Move();
    }

    //场景加载过程停止控制
    private void OnLoadEvent(GameSceneSO arg0, Vector3 arg1, bool arg2)
    {
        inputControl.Gameplay.Disable();
    }

    //读取游戏进度
    private void OnLoadDataEvent()
    {
        isDead = false;
        inputControl.Gameplay.Enable();
    }

    //场景加载结束启动控制
    private void OnAfterSceneLoadedEvent()
    {
        
        inputControl.Gameplay.Enable();
    }

    public void Move()
    {
        if(!wallJump)
        rb.velocity = new Vector2(inputDirection.x * speed * Time.deltaTime, rb.velocity.y);
        //人物翻转
        int facedir = (int)transform.localScale.x;
        if (inputDirection.x > 0)
            facedir = 1;
        if (inputDirection.x < 0)
            facedir = -1;

        transform.localScale = new Vector3(facedir,1,1);
    }

    private void Jump(InputAction.CallbackContext obj)
    {
        //Debug.Log("jump");
        if (physicsCheck.isGround)
        {
            rb.AddForce(transform.up * jumpForce, ForceMode2D.Impulse);
        }
        else if (physicsCheck.onWall)
        {
            rb.AddForce(new Vector2(-inputDirection.x, 2f) * wallJumpForce, ForceMode2D.Impulse);
            wallJump = true;
        }

        GetComponent<AudioDefination>()?.PlayAudioClip();
    }

    private void PlayerAttack(InputAction.CallbackContext obj)
    {
        playeraAnimation.PlayAttack();
        isAttack = true;
     
    }

    private void Skill(InputAction.CallbackContext obj)
    {
        if(skillAvailable&&physicsCheck.isGround)
        {
            isSkill = true;
            inputControl.Gameplay.Disable();
            rb.velocity = Vector2.zero;
            playeraAnimation.PlaySkill();
            if (character.currentHealth + healingPower < 90)
                character.currentHealth += healingPower;
            else
                character.currentHealth = 90;

            OnSkill?.Invoke(character);
            skillAvailable = false;
            cdCounter = cd;
        }
        
    }

    public void GetHurt(Transform attacker)
    {
        isHurt = true;
        rb.velocity = Vector2.zero;
        Vector2 dir = new Vector2((transform.position.x - attacker.position.x), 0).normalized;

        rb.AddForce(dir * hurtForce, ForceMode2D.Impulse);
    }

    public void PlayerDead()
    {
        isDead = true;
        inputControl.Gameplay.Disable();
        
    }

    private void CheckState()
    {
        coll.sharedMaterial = physicsCheck.isGround ? normal : wall;

        if(physicsCheck.onWall)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y / 2);
        }
        else
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y);
        }
            
        if(wallJump&&rb.velocity.y<0f)
        {
            wallJump = false;
        }
    }
}
