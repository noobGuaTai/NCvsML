using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public GameObject father;

    void Start()
    {
        Destroy(gameObject, 4f);
    }

    void Update()
    {
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player" && collision.gameObject != father && !collision.GetComponent<PlayerAttribute>().isInvincible)
        {
            Vector2 knockBackDirection = (collision.transform.position - father.transform.position).normalized;
            knockBackDirection.y = 0;
            knockBackDirection = knockBackDirection.normalized;
            collision.GetComponent<PlayerAttribute>().ChangeHP(-1, knockBackDirection);
            collision.GetComponent<PlayerMove>().beShot = true;
            father.GetComponent<PlayerMove>().isShot = true;
            Destroy(gameObject);
        }
    }

    public void SetFather(GameObject g)
    {
        father = g;
    }
}
