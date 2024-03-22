using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    private Rigidbody2D rb;
    private PlayerAttribute attributes;

    public GameObject bulletPrefab;
    public float bulletSpeed = 10f; // 子弹的速度
    public float shootCooldown = 1f; // 射击的冷却时间，以秒为单位

    private float shootTimer = 0f; // 射击计时器

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        attributes = GetComponent<PlayerAttribute>();
    }

    void Update()
    {
        Move();
        if (Input.GetButtonDown("Jump"))
        {
            Jump();
        }
    }

    void Move()
    {
        float moveHorizontal = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(moveHorizontal * attributes.moveSpeed, rb.velocity.y);

        // 根据移动方向翻转玩家的朝向
        if (moveHorizontal != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(moveHorizontal), 1, 1);
        }
    }

    void Jump()
    {
        rb.AddForce(new Vector2(0, attributes.jumpSpeed), ForceMode2D.Impulse);
    }

    public void StartShoot()
    {
        if (Input.GetButtonDown("Fire1") && shootTimer <= 0)
        {
            Shoot();
            shootTimer = shootCooldown; // 重置射击计时器
        }

        if (shootTimer > 0)
        {
            shootTimer -= Time.deltaTime; // 更新射击计时器
        }
    }

    public void Shoot()
    {
        if (bulletPrefab != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            bullet.GetComponent<Bullet>().SetFather(gameObject);
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            if (bulletRb != null)
            {
                // 设置子弹的初速度方向和速度
                float direction = transform.localScale.x; // 使用玩家的朝向作为子弹的方向
                bulletRb.velocity = new Vector2(direction * bulletSpeed, 0);
            }
        }
    }
}
