using System.Collections;
using UnityEngine;

public class Train : MonoBehaviour
{
    public GameObject player1;

    private bool allowFrameUpdate = false;
    private bool updateExecuted = false;

    // 初始化时暂停游戏时间
    void Start()
    {
        Time.timeScale = 0f;
    }

    void Update()
    {
        //接收到下一帧指令，就执行一帧
        if (Next() && !allowFrameUpdate)
        {
            allowFrameUpdate = true;
        }
    }

    void LateUpdate()
    {
        if (allowFrameUpdate && !updateExecuted)
        {
            Time.timeScale = 1f;
            Run();
            updateExecuted = true;
        }
        else if (updateExecuted)
        {
            Time.timeScale = 0f;
            allowFrameUpdate = false;
            updateExecuted = false;
        }
    }

    void Run()
    {
        //这里放模型的输出的指令
        player1.GetComponent<PlayerMove>().StartShoot();

    }

    bool Next()
    {
        //如果接收到模型的下一帧请求，就进行下一帧
        if(Input.GetKey(KeyCode.N))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
