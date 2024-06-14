using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Train2Manager : MonoBehaviour
{
    public Train2 s1;
    public Train2 s2;
    public GameObject player1;
    public GameObject player2;
    public bool isEnd = false;// 一场比赛结束标志
    public float groundTime;// 一场比赛剩余时间
    public float totalTime = 60f;// 一场比赛总时间
    public float timeSpeed = 1f;// 时间流速
    public float iterationStartTime;// 每场比赛开始时间
    public int iteration = 1;
    public int generation = 1;
    public PlayerMove p1m;
    public PlayerMove p2m;
    public PlayerAttribute p1a;
    public PlayerAttribute p2a;
    public int player1HP;
    public int player2HP;
    public int isStart = 0;
    public EnvInfo info1;
    public EnvInfo info2;
    public int socket1Port = 12345;
    public int socket2Port = 22345;
    public Vector3 player1InitPos;
    public Vector3 player2InitPos;
    public GameObject UI;
    public bool isStartTrain = false;

    public void StartTrain()
    {
        s1 = new Train2(this, 1);
        s1.Start(socket1Port);
        s2 = new Train2(this, 2);
        s2.Start(socket2Port);
        p1m = player1.GetComponent<PlayerMove>();
        p2m = player2.GetComponent<PlayerMove>();
        p1a = player1.GetComponent<PlayerAttribute>();
        p2a = player2.GetComponent<PlayerAttribute>();
        p1m.isControl = false;
        p2m.isControl = false;
        info1 = new EnvInfo();
        info2 = new EnvInfo();
        // player1InitPos = player1.transform.position;
        // player2InitPos = player2.transform.position;
        UI.GetComponent<UI>().waitingConnect.SetActive(true);

    }

    void Update()
    {
        Time.timeScale = timeSpeed;
        GetEnvInf(p1m, p2m, p1a, p2a, ref info1);
        GetEnvInf(p2m, p1m, p1a, p2a, ref info2);
        player1HP = p1a.HP;
        player2HP = p2a.HP;

        if (isStartTrain)
            groundTime = totalTime - (Time.time - iterationStartTime);
        UI.GetComponent<UI>().time = (int)groundTime;
        generation = (iteration / 190) + 1;
        if (groundTime <= 0 || p1a.HP <= 0 || p2a.HP <= 0)
        {
            isEnd = true;
        }
        else
        {
            s1.SetEnvInfo(info1);// 结束了就不再更新环境信息
            s2.SetEnvInfo(info2);
        }

        if (isStart == 2)
        {
            Reset();
            iteration++;

            s1.SendMessage(s1.RAShandler, s1.info);
            s2.SendMessage(s2.RAShandler, s2.info);
            print("restart");
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

    public void RunAction(int player, int[] action)
    {
        PlayerMove pm = player == 1 ? p1m : p2m;

        UnityMainThreadDispatcher.Enqueue(() =>
        {
            if (action[0] == 2)
            {
                isStart += 1;
            }
            if (pm.isControl)
                return;
            if (action[0] == 1)
            {
                pm.Jump();
            }
            if (action[1] == 1)
            {
                pm.StartShoot();
            }
            if (action[2] == -1)
            {
                pm.MoveLeft();
            }
            if (action[2] == 1)
            {
                pm.MoveRight();
            }
            if (action[2] == 0)
            {
                pm.Idle();
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
        p1a.isInvincible = false;
        p2a.isInvincible = false;
        // p1m.canJump = true;
        // p2m.canJump = true;
        info1.infoCode = 0;
        info2.infoCode = 0;
        player1.transform.localScale = new Vector3(1, 1, 1);
        player2.transform.localScale = new Vector3(-1, 1, 1);
        isEnd = false;
        isStart = 0;
        s1.hasSendEndInfo = false;
        s2.hasSendEndInfo = false;
        print("reset");
    }

}
