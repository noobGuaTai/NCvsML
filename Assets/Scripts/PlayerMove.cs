using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public bool isControl = false;
    public bool canJump = true;
    public bool canShoot = true;
    public List<GameObject> bullets = new List<GameObject>(); // 存储所有发射的子弹
    public GameObject bulletPrefab;
    public float bulletSpeed = 10f; // 子弹的速度
    public float shootCooldown = 0.5f; // 射击的冷却时间，以秒为单位
    public GameObject train1ManagerGameObject;
    public GameObject train2ManagerGameObject;
    public Train2Manager train2Manager;
    public Train1Manager train1Manager;
    public bool beShot = false;// 被击中
    public bool isShot = false;// 是否击中
    public GameObject leftWall;
    public GameObject rightWall;

    private Rigidbody2D rb;
    private PlayerAttribute attributes;
    private float shootTimer = 0f; // 射击计时器


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        attributes = GetComponent<PlayerAttribute>();
        train2Manager = train2ManagerGameObject.GetComponent<Train2Manager>();
        train1Manager = train1ManagerGameObject.GetComponent<Train1Manager>();
    }

    void Update()
    {
        if (isControl)
        {

            Move();

            // if (Input.GetButtonUp("Horizontal"))
            // {
            //     Idle();
            // }

            if (Input.GetButtonDown("Jump") && canJump)
            {
                Jump();
            }
            if (Input.GetButtonDown("Fire1"))
            {
                StartShoot();
            }
        }


        canShoot = shootTimer <= 0 && bullets.Count < 2;



        // 更新射击计时器
        if (shootTimer > 0)
        {
            shootTimer -= Time.deltaTime;
        }
        UpdateBulletPosition();
        if (train2Manager.s1 != null)
        {
            train2Manager.s1.SetRecvFlag(true);
            train2Manager.s1.SetSendFlag(true);
        }
        if (train2Manager.s2 != null)
        {
            train2Manager.s2.SetRecvFlag(true);
            train2Manager.s2.SetSendFlag(true);
        }
        if (train1Manager.train != null)
        {
            train1Manager.train.SetRecvFlag(true);
            train1Manager.train.SetSendFlag(true);
        }

    }

    void Move()
    {
        if (!attributes.isInvincible)
        {
            float moveHorizontal = 0;

            // 检测左右按键的状态，并根据优先级决定移动方向
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                moveHorizontal = -1;

            }
            else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                moveHorizontal = 1;
            }
            else
            {
                rb.velocity = new Vector2(0, rb.velocity.y);
            }

            rb.velocity = new Vector2(moveHorizontal * attributes.moveSpeed, rb.velocity.y);

            if (moveHorizontal != 0)
            {
                transform.localScale = new Vector3(Mathf.Sign(moveHorizontal), 1, 1);
            }
        }
    }


    public void MoveLeft()
    {
        if (!attributes.isInvincible)
        {
            rb.velocity = new Vector2(-attributes.moveSpeed, rb.velocity.y);
            transform.localScale = new Vector3(-1, 1, 1);
        }

    }

    public void MoveRight()
    {
        if (!attributes.isInvincible)
        {
            rb.velocity = new Vector2(attributes.moveSpeed, rb.velocity.y);
            transform.localScale = new Vector3(1, 1, 1);
        }

    }

    public void Idle()
    {
        if (!attributes.isInvincible)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }

    }

    public void Jump()
    {
        if (canJump && !attributes.isInvincible)
        {
            rb.AddForce(new Vector2(0, attributes.jumpSpeed), ForceMode2D.Impulse);
            canJump = false;
        }
    }

    public void StartShoot()
    {
        if (canShoot && !attributes.isInvincible)
        {
            Shoot();
            shootTimer = shootCooldown;
            canShoot = false;
        }
    }

    void Shoot()
    {
        if (bulletPrefab != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            bullet.GetComponent<Bullet>().SetFather(gameObject);
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            if (bulletRb != null)
            {
                float direction = transform.localScale.x;
                bulletRb.velocity = new Vector2(direction * bulletSpeed, 0);
            }
            bullets.Add(bullet);
        }
    }

    public void RunAction(int[] action)
    {
        if (action[0] == 1)
        {
            Jump();
        }
        if (action[1] == 1)
        {
            StartShoot();
        }
        if (action[2] == -1)
        {
            MoveLeft();
        }
        if (action[2] == 1)
        {
            MoveRight();
        }
        if (action[2] == 0)
        {
            Idle();
        }
    }

    void UpdateBulletPosition()
    {
        // foreach (GameObject bullet in bullets)
        // {
        //     if (bullet != null)
        //     {
        //         // Debug.Log(bullet.transform.position);
        //     }
        // }
        bullets.RemoveAll(bullet => bullet == null);
    }

    public void ClearBullets()
    {
        foreach (GameObject bullet in bullets)
        {
            if (bullet != null)
            {
                Destroy(bullet);
            }
        }
        bullets.Clear();
        shootTimer = 0f;
        canShoot = true;
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.CompareTag("Ground"))
        {
            canJump = true;
        }
        // if (other.collider.CompareTag("Wall"))
        // {
        //     Vector2 knockBackDirection = (transform.position - other.collider.transform.position).normalized;
        //     knockBackDirection.y = 0;
        //     knockBackDirection = knockBackDirection.normalized;
        //     attributes.ChangeHP(-1, knockBackDirection);
        //     beShot = true;
        // }
    }

}
