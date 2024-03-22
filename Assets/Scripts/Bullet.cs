using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public GameObject father;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(123);
        if(collision.tag == "Player" && collision.gameObject != father)
        {
            collision.GetComponent<PlayerAttribute>().ChangeHP(-1);
            Destroy(gameObject);
        }
    }

    public void SetFather(GameObject g)
    {
        father = g;
    }
}
