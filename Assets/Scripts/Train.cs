using System.Collections;
using UnityEngine;

public class ManualFrameController : MonoBehaviour
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
        player1.GetComponent<PlayerMove>().Shoot();
    }

    bool Next()
    {
        if(Input.GetKeyDown(KeyCode.N))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
