using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerEnum;

public class PlayerJumpState : IState
{
    private PlayerFSM playerFSM;
    private PlayerParameters parameters;
    private int a = 0;

    public PlayerJumpState(PlayerFSM playerFSM)
    {
        this.playerFSM = playerFSM;
        this.parameters = playerFSM.parameters;
    }

    public void OnEnter()
    {
        parameters.rb.AddForce(new Vector2(0, parameters.playerAttribute.jumpSpeed), ForceMode2D.Impulse);
        parameters.canJump = false;
        Debug.Log(a);
        a++;
    }

    public void OnExit()
    {

    }

    public void OnFixedUpdate()
    {

    }

    public void OnUpdate()
    {
        if (parameters.isControl)
        {
            // 键盘输入判断
            playerFSM.KeyboardMove();
            if (parameters.canShoot && Input.GetButtonDown("Fire1"))
            {
                playerFSM.Shoot();
                parameters.shootTimer = parameters.shootCoolDown;
                parameters.canShoot = false;
            }
        }
        else
        {
            //智能体输入判断
            if (parameters.playerAction[2] == PlayerActionType.MoveLeft)
                playerFSM.MoveLeft();
            else if (parameters.playerAction[2] == PlayerActionType.MoveRight)
                playerFSM.MoveRight();
            else if (parameters.playerAction[2] == PlayerActionType.None)
                parameters.rb.velocity = new Vector2(0, parameters.rb.velocity.y);

            // 射击判断
            if (parameters.canShoot && parameters.playerAction[1] == PlayerActionType.Shoot)
            {
                playerFSM.Shoot();
                parameters.shootTimer = parameters.shootCoolDown;
                parameters.canShoot = false;
            }
        }
    }


}
