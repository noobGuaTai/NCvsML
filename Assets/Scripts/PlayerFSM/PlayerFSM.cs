using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerEnum;


[Serializable]
public class PlayerParameters
{
    public bool isControl = false;// 判断玩家是否可以控制该角色
    public bool canJump = true;
    public bool canShoot = true;
    public GameObject[] bullets;
    public Vector2 bullet1Pos;
    public Vector2 bullet2Pos;
    public GameObject bulletPrefab;
    public float bulletSpeed = 10f;
    public float shootCoolDown = 0.5f;
    public RunManager runManager;
    public bool beShot = false;// 被击中
    public bool isShot = false;// 是否击中
    public GameObject leftWall;
    public GameObject rightWall;

    public Rigidbody2D rb;
    public PlayerAttribute playerAttribute;
    public float shootTimer = 0f;
    public Vector2 moveKeyboardInput;// 键盘输入
    public PlayerActionType[] playerAction = new PlayerActionType[3];
}

public class PlayerFSM : MonoBehaviour
{
    public PlayerParameters parameters;
    public IState currentState;
    public Dictionary<PlayerStateType, IState> state = new Dictionary<PlayerStateType, IState>();

    void Start()
    {
        parameters.rb = GetComponent<Rigidbody2D>();
        parameters.playerAttribute = GetComponent<PlayerAttribute>();
        parameters.playerAction[0] = PlayerActionType.None;
        parameters.playerAction[1] = PlayerActionType.None;
        parameters.playerAction[2] = PlayerActionType.None;
        parameters.bullets = new GameObject[2];

        state.Add(PlayerStateType.Idle, new PlayerIdleState(this));
        state.Add(PlayerStateType.Move, new PlayerMoveState(this));
        state.Add(PlayerStateType.Jump, new PlayerJumpState(this));
        ChangeState(PlayerStateType.Idle);
    }


    void Update()
    {
        // parameters.moveKeyboardInput.x = Input.GetAxisRaw("Horizontal");
        parameters.canShoot = parameters.shootTimer <= 0 && (parameters.bullets[0] == null || parameters.bullets[1] == null);

        if (parameters.shootTimer > 0)
        {
            parameters.shootTimer -= Time.deltaTime;
        }
        UpdateBulletState();

        if (parameters.runManager.socket1 != null)
        {
            parameters.runManager.socket1.SetRecvFlag(true);
            parameters.runManager.socket1.SetSendFlag(true);
        }
        if (parameters.runManager.socket2 != null)
        {
            parameters.runManager.socket2.SetRecvFlag(true);
            parameters.runManager.socket2.SetSendFlag(true);
        }

        parameters.moveKeyboardInput.x = Input.GetAxisRaw("Horizontal");

        currentState.OnUpdate();
    }

    void FixedUpdate()
    {
        currentState.OnFixedUpdate();
    }

    public void ChangeState(PlayerStateType stateType)
    {
        if (currentState != null)
        {
            currentState.OnExit();
        }
        currentState = state[stateType];
        currentState.OnEnter();
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.CompareTag("Ground") && currentState == state[PlayerStateType.Jump])
        {
            ChangeState(PlayerStateType.Idle);
            parameters.canJump = true;
        }
    }

    void UpdateBulletState()
    {
        // parameters.bullets.RemoveAll(bullet => bullet == null);
        if(parameters.bullets[0]!=null)
        {
            parameters.bullet1Pos = parameters.bullets[0].transform.position;
        }
        if(parameters.bullets[1]!=null)
        {
            parameters.bullet2Pos = parameters.bullets[1].transform.position;
        }
    }

    public void ClearBullets()
    {
        foreach (GameObject bullet in parameters.bullets)
        {
            if (bullet != null)
            {
                Destroy(bullet);
            }
        }
        parameters.bullets[0] = null;
        parameters.bullets[1] = null;
        parameters.shootTimer = 0f;
        parameters.canShoot = true;
    }

    public void Shoot()
    {
        if (parameters.bulletPrefab != null)
        {
            GameObject bullet = GameObject.Instantiate(parameters.bulletPrefab, transform.position, Quaternion.identity);
            bullet.GetComponent<Bullet>().SetFather(gameObject, parameters.bullets[0] == null ? 0 : 1);
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            if (bulletRb != null)
            {
                float direction = transform.localScale.x;
                bulletRb.velocity = new Vector2(direction * parameters.bulletSpeed, 0);
            }
            if (parameters.bullets[0] == null)
                parameters.bullets[0] = bullet;
            else if (parameters.bullets[1] == null)
                parameters.bullets[1] = bullet;
        }
    }

    public void KeyboardMove()
    {
        parameters.rb.velocity = new Vector2(parameters.moveKeyboardInput.x * parameters.playerAttribute.moveSpeed, parameters.rb.velocity.y);
        if(parameters.moveKeyboardInput.x != 0)
            transform.localScale = new Vector3(Mathf.Sign(parameters.moveKeyboardInput.x) * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    public void MoveLeft()
    {
        parameters.rb.velocity = new Vector2(-parameters.playerAttribute.moveSpeed, parameters.rb.velocity.y);
        transform.localScale = new Vector3(-1, 1, 1);
    }

    public void MoveRight()
    {
        parameters.rb.velocity = new Vector2(parameters.playerAttribute.moveSpeed, parameters.rb.velocity.y);
        transform.localScale = new Vector3(1, 1, 1);
    }


}
