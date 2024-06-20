using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerEnum;

public class Train1Manager : MonoBehaviour
{
    public Train1 socket1;
    private Infer infer;
    public GameObject player1;
    public GameObject player2;
    public bool isEnd = false;// 一场比赛结束标志
    public float groundTime;// 一场比赛剩余时间
    public float totalTime = 30f;// 一场比赛总时间
    public float timeSpeed = 1f;// 时间流速
    public float iterationStartTime;// 每场比赛开始时间
    public int iteration = 1;
    public PlayerFSM player1FSM;
    public PlayerFSM player2FSM;
    public PlayerAttribute player1attribute;
    public PlayerAttribute player2attribute;
    public int player1HP;
    public int player2HP;
    public int isStart = 0;
    public EnvInfo info;
    public EnvInfo info2;
    public int socket1Port = 12345;
    public Vector3 player1InitPos;
    public Vector3 player2InitPos;
    public GameObject UI;
    public bool isStartTrain = false;

    public void StartTrain()
    {
        socket1 = new Train1(this, 1);
        socket1.Start(socket1Port);

        player1FSM = player1.GetComponent<PlayerFSM>();
        player2FSM = player2.GetComponent<PlayerFSM>();
        player1attribute = player1.GetComponent<PlayerAttribute>();
        player2attribute = player2.GetComponent<PlayerAttribute>();
        player1FSM.parameters.isControl = false;
        player2FSM.parameters.isControl = false;
        info = new EnvInfo();
        info2 = new EnvInfo();
        // player1InitPos = player1.transform.position;
        // player2InitPos = player2.transform.position;
        UI.GetComponent<UI>().waitingConnect.SetActive(true);

        infer = new Infer(player2, player1, "/gene.csv");
        Time.timeScale = 0f;
        infer.StartInfer();
    }

    void Update()
    {
        // Time.timeScale = timeSpeed;
        GetEnvInf(player1FSM, player2FSM, player1attribute, player2attribute, ref info);
        player1HP = player1attribute.HP;
        player2HP = player2attribute.HP;

        infer.OnUpdate();

        if (isStartTrain)
            groundTime = totalTime - (Time.time - iterationStartTime);
        UI.GetComponent<UI>().time = (int)groundTime;

        if (groundTime <= 0 || player1attribute.HP <= 0 || player2attribute.HP <= 0)
        {
            isEnd = true;
        }
        else
        {
            socket1.SetEnvInfo(info);// 结束了就不再更新环境信息
        }

        if (isStart == 1)
        {
            Reset();
            iteration++;


            socket1.SendMessage(socket1.RAShandler, socket1.info);
            // print("restart");
            Time.timeScale = timeSpeed;
        }

    }

    public void GetEnvInf(PlayerFSM playerFSM1, PlayerFSM playerFSM2, PlayerAttribute playerAttribute1, PlayerAttribute playerAttribute2, ref EnvInfo info)
    {
        info.direction = playerFSM1.transform.localScale.x;
        info.shootable = 2 - playerFSM1.parameters.bullets.Count;// 有改动
        info.jumpable = playerFSM1.parameters.canJump ? 1 : 0;
        info.leftWall_XD = playerFSM1.transform.position.x - playerFSM1.parameters.leftWall.transform.position.x;
        info.rightWall_XD = playerFSM1.parameters.rightWall.transform.position.x - playerFSM1.transform.position.x;
        info.E_XD = playerFSM2.transform.position.x - playerFSM1.transform.position.x;
        info.E_YD = playerFSM2.transform.position.y - playerFSM1.transform.position.y;
        info.E_Bullet0 = playerFSM2.parameters.bullets.Count > 0 ? 1 : 0;
        if (info.E_Bullet0 != 0 && playerFSM2.parameters.bullets[0] != null)
        {
            info.E_Bullet0_XD = playerFSM2.parameters.bullets[0].transform.position.x - playerFSM1.transform.position.x;
            info.E_Bullet0_YD = playerFSM2.parameters.bullets[0].transform.position.y - playerFSM1.transform.position.y;
        }

        info.E_Bullet1 = playerFSM2.parameters.bullets.Count > 1 ? 1 : 0;
        if (info.E_Bullet1 != 0 && playerFSM2.parameters.bullets[1] != null)
        {
            info.E_Bullet1_XD = playerFSM2.parameters.bullets[1].transform.position.x - playerFSM1.transform.position.x;
            info.E_Bullet1_YD = playerFSM2.parameters.bullets[1].transform.position.y - playerFSM1.transform.position.y;
        }
        info.self_Invincible = playerAttribute1.isInvincible ? 1 : 0;
        info.E_Invincible = playerAttribute2.isInvincible ? 1 : 0;
        info.time = groundTime;
    }

    public void RunAction(PlayerActionType[] action)
    {
        UnityMainThreadDispatcher.Enqueue(() =>
        {
            if (action[0] == PlayerActionType.StartNextGround)
            {
                isStart += 1;
            }
            if (player1FSM.parameters.isControl)
                return;

            player1FSM.parameters.playerAction = action;
        });
    }

    public void Reset()
    {
        player1attribute.HP = 10;
        player2attribute.HP = 10;
        player1.transform.position = player1InitPos;
        player2.transform.position = player2InitPos;
        groundTime = (int)totalTime;
        iterationStartTime = Time.time;
        player1.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        player2.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        player1FSM.ClearBullets();
        player2FSM.ClearBullets();
        // p1m.canJump = true;
        // p2m.canJump = true;
        player1attribute.isInvincible = false;
        player2attribute.isInvincible = false;
        info.infoCode = 0;
        info2.infoCode = 0;
        player1.transform.localScale = new Vector3(1, 1, 1);
        player2.transform.localScale = new Vector3(-1, 1, 1);
        isEnd = false;
        isStart = 0;
        socket1.canRestart = true;
        print("reset");
    }

}
