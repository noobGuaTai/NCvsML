using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public GameObject father;
    public int index = 0;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player" && collision.gameObject != father && !collision.GetComponent<PlayerAttribute>().isInvincible)
        {
            if (father.GetComponent<PlayerFSM>().parameters.runManager.gameMode == GameMode.Live)
            {
                Vector2 knockBackDirection = (collision.transform.position - father.transform.position).normalized;
                knockBackDirection.y = 0;
                knockBackDirection = knockBackDirection.normalized;
                collision.GetComponent<PlayerAttribute>().ChangeHP(-1, knockBackDirection);
                collision.GetComponent<PlayerFSM>().parameters.beShot = true;
                father.GetComponent<PlayerFSM>().parameters.isShot = true;
                Destroy(gameObject);
                father.GetComponent<PlayerFSM>().parameters.bullets[index] = null;
            }
            if (father.GetComponent<PlayerFSM>().parameters.runManager.gameMode == GameMode.Replay)
            {
                Destroy(gameObject);
                father.GetComponent<PlayerFSM>().parameters.bullets[index] = null;
            }

        }
        if (collision.tag == "Wall")
        {
            Destroy(gameObject);
            father.GetComponent<PlayerFSM>().parameters.bullets[index] = null;
        }
    }

    public void SetFather(GameObject g, int index)
    {
        father = g;
        this.index = index;
        StartCoroutine(Die());
    }

    IEnumerator Die()
    {
        yield return new WaitForSeconds(4f);
        Destroy(gameObject);
        father.GetComponent<PlayerFSM>().parameters.bullets[index] = null;
    }
}
