using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerEnum;

public class PlayerIdleState : IState
{
    private PlayerFSM playerFSM;
    private PlayerParameters parameters;

    public PlayerIdleState(PlayerFSM playerFSM)
    {
        this.playerFSM = playerFSM;
        this.parameters = playerFSM.parameters;
    }

    public void OnEnter()
    {
        parameters.rb.velocity = Vector2.zero;
    }

    public void OnExit()
    {

    }

    public void OnFixedUpdate()
    {

    }

    public void OnUpdate()
    {
        if (!parameters.playerAttribute.isInvincible)
        {
            if (parameters.isControl)
            {
                //键盘输入判断
                if (Input.GetButtonDown("Jump") && parameters.canJump)
                {
                    playerFSM.ChangeState(PlayerStateType.Jump);
                    return;
                }

                if (parameters.moveKeyboardInput.x != 0)
                {
                    playerFSM.ChangeState(PlayerStateType.Move);
                    return;
                }

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
                if (parameters.playerAction[0] == PlayerActionType.Jump)
                {
                    playerFSM.ChangeState(PlayerStateType.Jump);
                    return;
                }
                if (parameters.playerAction[2] != PlayerActionType.None)
                {
                    playerFSM.ChangeState(PlayerStateType.Move);
                    return;
                }
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
}
