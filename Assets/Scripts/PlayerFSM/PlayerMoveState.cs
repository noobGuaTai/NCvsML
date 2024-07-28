using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerEnum;

public class PlayerMoveState : IState
{
    private PlayerFSM playerFSM;
    private PlayerParameters parameters;

    public PlayerMoveState(PlayerFSM playerFSM)
    {
        this.playerFSM = playerFSM;
        this.parameters = playerFSM.parameters;
    }

    public void OnEnter()
    {

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
                // 键盘输入判断
                if (playerFSM.playerType == PlayerType.player1)
                {
                    if (Input.GetButtonDown("Jump1") && parameters.canJump)
                    {
                        playerFSM.ChangeState(PlayerStateType.Jump);
                        return;
                    }

                    if (parameters.moveKeyboardInput.x == 0)
                    {
                        playerFSM.ChangeState(PlayerStateType.Idle);
                        return;
                    }

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
                    if (Input.GetButtonDown("Jump2") && parameters.canJump)
                    {
                        playerFSM.ChangeState(PlayerStateType.Jump);
                        return;
                    }

                    if (parameters.moveKeyboardInput.x == 0)
                    {
                        playerFSM.ChangeState(PlayerStateType.Idle);
                        return;
                    }

                    playerFSM.KeyboardMove();

                    if (parameters.canShoot && Input.GetButtonDown("Fire2"))
                    {
                        playerFSM.Shoot();
                        parameters.shootTimer = parameters.shootCoolDown;
                        parameters.canShoot = false;
                    }
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

                if (parameters.playerAction[2] == PlayerActionType.None)
                {
                    playerFSM.ChangeState(PlayerStateType.Idle);
                    return;
                }
                else if (parameters.playerAction[2] == PlayerActionType.MoveLeft)
                    playerFSM.MoveLeft();
                else if (parameters.playerAction[2] == PlayerActionType.MoveRight)
                    playerFSM.MoveRight();

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
