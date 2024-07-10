using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI : MonoBehaviour
{
    public GameObject train;
    public GameObject Infer;
    public GameObject groundTime;
    public GameObject Iteration;
    public GameObject Manager;
    public float time;
    public int iteration;
    public GameObject gameOver;
    public GameObject waitingConnect;

    public GameObject Player1HP;
    public GameObject Player2HP;
    public GameObject Player1;
    public GameObject Player2;
    public GameObject notice;

    void Update()
    {
        Player1HP.GetComponent<TextMeshProUGUI>().text = "Player1 HP:" + Player1.GetComponent<PlayerAttribute>().HP;
        Player2HP.GetComponent<TextMeshProUGUI>().text = "Player2 HP:" + Player2.GetComponent<PlayerAttribute>().HP;
        groundTime.GetComponent<TextMeshProUGUI>().text = time.ToString();
        Iteration.GetComponent<TextMeshProUGUI>().text = "Iteration:" + iteration.ToString();
    }

    public void TrainUI()
    {
        Iteration.SetActive(true);
        groundTime.SetActive(true);
    }

    public void InferUI()
    {
        Iteration.SetActive(false);
        groundTime.SetActive(true);
    }

    void OnEnable()
    {
        // 订阅日志回调
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        // 取消订阅日志回调
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        // 如果是错误日志，激活Notice对象并显示日志信息
        if (type == LogType.Error || type == LogType.Exception)
        {
            if (notice != null)
            {
                notice.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = logString + "\n" + stackTrace;
                notice.SetActive(true);
            }
        }
    }
}


