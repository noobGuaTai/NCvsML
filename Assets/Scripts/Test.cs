using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;

public class Test : MonoBehaviour
{
    public NNModel modelAsset;
    public GameObject player1;
    public GameObject player2;
    public float distance;

    private Model runtimeModel;
    private IWorker worker;
    private Tensor input;
    private Tensor gameResult;
    private float timeGap = 0.1f;
    private float currentTime;

    void Start()
    {
        // // 加载模型并创建worker
        // runtimeModel = ModelLoader.Load(modelAsset);
        // worker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, runtimeModel);

        // // 初始化输入Tensor，应当是当前帧的游戏状态
        // input = new Tensor(1, 3, 224, 224);
    }

    void Update()
    {
        // if (gameResult != null)
        // {
        //     // gameResult是前一次模型运行的输出，并将其用作这一次的输入
        //     // 确保gameResult的尺寸和模型期望的输入尺寸匹配
        //     input = gameResult;
        // }

        // // 执行模型
        // worker.Execute(input);
        // // 获取模型输出
        // gameResult = worker.PeekOutput("output");

        // Run(gameResult);//根据模型输出运行游戏

        // // 处理完后释放Tensor资源
        // gameResult.Dispose();
        if(Time.time - currentTime > timeGap)
        {
            DecideAndAct(player1, player2);
            currentTime = Time.time;
        }

        
    }

    void Run(Tensor output)
    {

    }

    void OnDestroy()
    {
        // // 清理资源
        // worker.Dispose();
        // input.Dispose();
        // if (gameResult != null) gameResult.Dispose();
    }

    void DecideAndAct(GameObject player1, GameObject player2)
    {
        PlayerMove player1Move = player1.GetComponent<PlayerMove>();
        PlayerMove player2Move = player2.GetComponent<PlayerMove>();
        PlayerAttribute player1Attr = player1.GetComponent<PlayerAttribute>();
        PlayerAttribute player2Attr = player2.GetComponent<PlayerAttribute>();

        distance = Vector3.Distance(player1.transform.position, player2.transform.position);

        // 检测敌人的子弹是否靠近
        bool isEnemyBulletClosePlayer1 = false;
        bool isEnemyBulletClosePlayer2 = false;
        foreach (var bullet in player2Move.bullets)//2射的子弹对1的距离
        {
            if (bullet != null && Vector3.Distance(bullet.transform.position, player1.transform.position) < 3) // 假设距离小于3为“靠近”
            {
                isEnemyBulletClosePlayer1 = true;
                break;
            }
        }

        foreach (var bullet in player1Move.bullets)//1射的子弹对2的距离
        {
            if (bullet != null && Vector3.Distance(bullet.transform.position, player2.transform.position) < 3) // 假设距离小于3为“靠近”
            {
                isEnemyBulletClosePlayer2 = true;
                break;
            }
        }

        if (distance < 5) // 近距离
        {
            if (player1Move.canShoot)
                player1Move.StartShoot(); // 攻击
            if (player2Move.canShoot)
                player2Move.StartShoot(); // 攻击

            if (isEnemyBulletClosePlayer1) // 2的子弹靠近时，跳跃
            {
                if (player1Move.canJump)
                    player1Move.Jump();
            }
            else // 随机前后移动
            {
                if (Random.value > 0.5f)
                    player1Move.MoveLeft();
                else
                    player1Move.MoveRight();
            }
            if (isEnemyBulletClosePlayer2) // 1的子弹靠近时，跳跃
            {
                if (player2Move.canJump)
                    player2Move.Jump();
            }
            else // 随机前后移动
            {
                if (Random.value > 0.5f)
                    player2Move.MoveLeft();
                else
                    player2Move.MoveRight();
            }
        }
        else // 距离远，互相靠近
        {
            if (player1.transform.position.x < player2.transform.position.x)
                player1Move.MoveRight(); // 如果玩家1在玩家2的左侧，则向右移动靠近
            else
                player1Move.MoveLeft(); // 否则，向左移动靠近

            if (player2.transform.position.x < player1.transform.position.x)
                player2Move.MoveRight(); // 如果玩家1在玩家2的左侧，则向右移动靠近
            else
                player2Move.MoveLeft(); // 否则，向左移动靠近
        }
    }


}
