using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttribute : MonoBehaviour
{
    public int HP = 10;
    public int moveSpeed = 5;
    public int jumpSpeed = 5;

    public void ChangeHP(int value)
    {
        HP += value;
        if(HP <= 0)
        {
            HP = 0;
        }
    }
}
