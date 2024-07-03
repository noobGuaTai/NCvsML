using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System.Linq;
using PlayerEnum;
using Unity.Mathematics;
using System;

public enum InferMode
{
    player,
    decisionTree,
    agent,
}


public class InferManager : MonoBehaviour
{
    public AgentInfer agentInfer1;
    public AgentInfer agentInfer2;
    public DecisionTree decisionTree1;
    public DecisionTree decisionTree2;
    public GameObject player1;
    public GameObject player2;
    public PlayerFSM player1FSM;
    public PlayerFSM player2FSM;
    public PlayerAttribute player1Attribute;
    public PlayerAttribute player2Attribute;
    public string path1 = "D:\\Code\\Programs\\Python\\智能体对战python\\gene.csv";
    public string path2 = "D:\\Code\\Programs\\Python\\智能体对战python\\gene.csv";
    public GameObject UI;
    public InferMode player1InferMode;
    public InferMode player2InferMode;

    public float groundTime;
    public float groundStartTime;
    private int totalTime = 30;
    public Vector2 player1InitPos;
    public Vector2 player2InitPos;
    public float[] info1;
    public float[] info2;

    void Start()
    {
        player1FSM = player1.GetComponent<PlayerFSM>();
        player2FSM = player2.GetComponent<PlayerFSM>();
        player1Attribute = player1.GetComponent<PlayerAttribute>();
        player2Attribute = player2.GetComponent<PlayerAttribute>();
        info1 = new float[15];
        info2 = new float[15];
    }

    public void StartInfer()
    {
        Time.timeScale = 1f;
        if (player1InferMode == InferMode.agent)
        {
            agentInfer1 = new AgentInfer(player1, player2, path1, this);
        }
        if (player1InferMode == InferMode.decisionTree)
        {
            decisionTree1 = new DecisionTree(player1, player2, this);
        }

        if (player2InferMode == InferMode.agent)
        {
            agentInfer2 = new AgentInfer(player2, player1, path2, this);
        }
        if (player2InferMode == InferMode.decisionTree)
        {
            decisionTree2 = new DecisionTree(player2, player1, this);
        }

    }

    void Update()
    {
        GetEnvInf(player1FSM, player2FSM, player1Attribute, player2Attribute, ref info1);
        GetEnvInf(player2FSM, player1FSM, player2Attribute, player1Attribute, ref info2);
        if (player1InferMode == InferMode.agent)// 为什么这里使用if agentInfer1 != null，会进不去，也就是agentInfer1为null
        {
            agentInfer1.info = info1;
            agentInfer1.OnUpdate();
        }
        if (player2InferMode == InferMode.agent)
        {
            agentInfer2.info = info2;
            agentInfer2.OnUpdate();
        }
        if (player1InferMode == InferMode.decisionTree)
        {
            decisionTree1.info = info1;
            decisionTree1.OnUpdate();
        }
        if (player2InferMode == InferMode.decisionTree)
        {
            decisionTree2.info = info2;
            decisionTree2.OnUpdate();
        }
  
        groundTime = totalTime - (Time.time - groundStartTime);
        UI.GetComponent<UI>().time = (int)groundTime;
        if (player1Attribute.HP <= 0 || player2Attribute.HP <= 0 || groundTime <= 0)
        {
            Time.timeScale = 0f;
            UI.GetComponent<UI>().gameOver.SetActive(true);
        }
    }

    public void GetEnvInf(PlayerFSM playerFSM1, PlayerFSM playerFSM2, PlayerAttribute playerAttribute1, PlayerAttribute playerAttribute2, ref float[] info)
    {
        info[0] = playerFSM1.transform.localScale.x;
        info[1] = 2f;
        foreach (GameObject bullet in playerFSM1.parameters.bullets)
        {
            if (bullet != null)
            {
                info[1] -= 1f;
            }
        }
        info[2] = playerFSM1.parameters.canJump ? 1 : 0;
        info[3] = playerFSM1.transform.position.x - playerFSM1.parameters.leftWall.transform.position.x;
        info[4] = playerFSM1.parameters.rightWall.transform.position.x - playerFSM1.transform.position.x;
        info[5] = playerFSM2.transform.position.x - playerFSM1.transform.position.x;
        info[6] = playerFSM2.transform.position.y - playerFSM1.transform.position.y;
        // info[7] = playerFSM2.parameters.bullets[1] != null ? 1 : playerFSM2.parameters.bullets[0] != null ? 0.5f : 0;
        info[7] = playerFSM2.parameters.bullets[0] == null ? 0 : 1;
        if (playerFSM2.parameters.bullets[0] != null)
        {
            info[8] = playerFSM2.parameters.bullets[0].transform.position.x - playerFSM1.transform.position.x;
            info[9] = playerFSM2.parameters.bullets[0].transform.position.y - playerFSM1.transform.position.y;
        }
        else
        {
            info[8] = 0f;
            info[9] = 0f;
        }
        info[10] = playerFSM2.parameters.bullets[1] == null ? 0 : 1;
        if (playerFSM2.parameters.bullets[1] != null)
        {
            info[11] = playerFSM2.parameters.bullets[1].transform.position.x - playerFSM1.transform.position.x;
            info[12] = playerFSM2.parameters.bullets[1].transform.position.y - playerFSM1.transform.position.y;
        }
        else
        {
            info[11] = 0f;
            info[12] = 0f;
        }
        info[13] = playerAttribute1.isInvincible ? 1 : 0;
        info[14] = playerAttribute2.isInvincible ? 1 : 0;
    }

    public void Reset()
    {
        UI.GetComponent<UI>().gameOver.SetActive(false);
        player1FSM.transform.position = player1InitPos;
        player2FSM.transform.position = player2InitPos;
        player1.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        player2.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        player1FSM.ClearBullets();
        player2FSM.ClearBullets();
        player1FSM.transform.localScale = new Vector3(1, 1, 1);
        player2FSM.transform.localScale = new Vector3(-1, 1, 1);
        groundStartTime = Time.time;
        groundTime = 30;
        player1Attribute.HP = 10;
        player2Attribute.HP = 10;
    }
}

