using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System.Linq;
using PlayerEnum;

public class InferManager : MonoBehaviour
{
    public Infer infer1;
    public Infer infer2;
    public GameObject player1;
    public GameObject player2;
    public PlayerFSM player1FSM;
    public PlayerFSM player2FSM;
    public PlayerAttribute player1Attribute;
    public PlayerAttribute player2Attribute;
    public string path1 = "D:\\Code\\Programs\\Python\\智能体对战python\\gene.csv";
    public string path2 = "D:\\Code\\Programs\\Python\\智能体对战python\\gene.csv";
    public GameObject UI;

    public float groundTime;
    public float groundStartTime;
    private int totalTime = 30;
    public Vector2 player1InitPos;
    public Vector2 player2InitPos;

    void Start()
    {
        player1FSM = player1.GetComponent<PlayerFSM>();
        player2FSM = player2.GetComponent<PlayerFSM>();
        player1Attribute = player1.GetComponent<PlayerAttribute>();
        player2Attribute = player2.GetComponent<PlayerAttribute>();
    }

    public void StartInfer()
    {
        Time.timeScale = 1f;
        infer1 = new Infer(player1, player2, path1);
        infer2 = new Infer(player2, player1, path2);
        infer1.StartInfer();
        infer2.StartInfer();
    }

    void Update()
    {
        infer1.OnUpdate();
        infer2.OnUpdate();
        groundTime = totalTime - (Time.time - groundStartTime);
        UI.GetComponent<UI>().time = (int)groundTime;
        if (player1Attribute.HP <= 0 || player2Attribute.HP <= 0 || groundTime <= 0)
        {
            Time.timeScale = 0f;
            UI.GetComponent<UI>().gameOver.SetActive(true);
        }
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

