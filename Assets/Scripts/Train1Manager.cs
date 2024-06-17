using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public PlayerMove p1m;
    public PlayerMove p2m;
    public PlayerAttribute p1a;
    public PlayerAttribute p2a;
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

        p1m = player1.GetComponent<PlayerMove>();
        p2m = player2.GetComponent<PlayerMove>();
        p1a = player1.GetComponent<PlayerAttribute>();
        p2a = player2.GetComponent<PlayerAttribute>();
        p1m.isControl = false;
        p2m.isControl = false;
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
        GetEnvInf(p1m, p2m, p1a, p2a, ref info);
        player1HP = p1a.HP;
        player2HP = p2a.HP;

        infer.OnUpdate();

        if (isStartTrain)
            groundTime = totalTime - (Time.time - iterationStartTime);
        UI.GetComponent<UI>().time = (int)groundTime;

        if (groundTime <= 0 || p1a.HP <= 0 || p2a.HP <= 0)
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
            print("restart");
            Time.timeScale = timeSpeed;
        }

    }

    public void GetEnvInf(PlayerMove pm1, PlayerMove pm2, PlayerAttribute pa1, PlayerAttribute pa2, ref EnvInfo info)
    {
        info.direction = pm1.transform.localScale.x;
        info.shootable = 2 - pm1.bullets.Count;// 有改动
        info.jumpable = pm1.canJump ? 1 : 0;
        info.leftWall_XD = pm1.transform.position.x - pm1.leftWall.transform.position.x;
        info.rightWall_XD = pm1.rightWall.transform.position.x - pm1.transform.position.x;
        info.E_XD = pm2.transform.position.x - pm1.transform.position.x;
        info.E_YD = pm2.transform.position.y - pm1.transform.position.y;
        info.E_Bullet0 = pm2.bullets.Count > 0 ? 1 : 0;
        if (info.E_Bullet0 != 0 && pm2.bullets[0] != null)
        {
            info.E_Bullet0_XD = pm2.bullets[0].transform.position.x - pm1.transform.position.x;
            info.E_Bullet0_YD = pm2.bullets[0].transform.position.y - pm1.transform.position.y;
        }

        info.E_Bullet1 = pm2.bullets.Count > 1 ? 1 : 0;
        if (info.E_Bullet1 != 0 && pm2.bullets[1] != null)
        {
            info.E_Bullet1_XD = pm2.bullets[1].transform.position.x - pm1.transform.position.x;
            info.E_Bullet1_YD = pm2.bullets[1].transform.position.y - pm1.transform.position.y;
        }
        info.self_Invincible = pa1.isInvincible ? 1 : 0;
        info.E_Invincible = pa2.isInvincible ? 1 : 0;
        info.time = groundTime;
    }

    public void RunAction(int[] action)
    {
        UnityMainThreadDispatcher.Enqueue(() =>
        {
            if (action[0] == 2)
            {
                isStart += 1;
            }
            if (p1m.isControl)
                return;
            if (action[0] == 1)
            {
                p1m.Jump();
            }
            if (action[1] == 1)
            {
                p1m.StartShoot();
            }
            if (action[2] == -1)
            {
                p1m.MoveLeft();
            }
            if (action[2] == 1)
            {
                p1m.MoveRight();
            }
            if (action[2] == 0)
            {
                p1m.Idle();
            }
        });
    }

    public void Reset()
    {
        p1a.HP = 10;
        p2a.HP = 10;
        player1.transform.position = player1InitPos;
        player2.transform.position = player2InitPos;
        groundTime = (int)totalTime;
        iterationStartTime = Time.time;
        player1.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        player2.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        p1m.ClearBullets();
        p2m.ClearBullets();
        // p1m.canJump = true;
        // p2m.canJump = true;
        p1a.isInvincible = false;
        p2a.isInvincible = false;
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
