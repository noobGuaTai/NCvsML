using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System.Linq;

public class InferManager : MonoBehaviour
{
    public Infer infer1;
    public Infer infer2;
    public GameObject player1;
    public GameObject player2;
    public PlayerMove p1m;
    public PlayerMove p2m;
    public PlayerAttribute p1a;
    public PlayerAttribute p2a;
    public string path1 = "D:\\Code\\Programs\\Python\\智能体对战python\\gene.csv";
    public string path2 = "D:\\Code\\Programs\\Python\\智能体对战python\\gene.csv";
    public GameObject UI;

    public float groundTime;
    public float groundStartTime;
    private int totalTime = 30;
    private Vector2 player1InitPos;
    private Vector2 player2InitPos;

    void Start()
    {
        p1m = player1.GetComponent<PlayerMove>();
        p2m = player2.GetComponent<PlayerMove>();
        p1a = player1.GetComponent<PlayerAttribute>();
        p2a = player2.GetComponent<PlayerAttribute>();
    }

    public void StartInfer()
    {
        Time.timeScale = 1f;
        infer1 = new Infer(player1, player2, path1);
        infer2 = new Infer(player2, player1, path2);
        infer1.StartInfer();
        infer2.StartInfer();

        player1InitPos = player1.transform.position;
        player2InitPos = player2.transform.position;
    }

    void Update()
    {
        infer1.OnUpdate();
        infer2.OnUpdate();
        groundTime = totalTime - (Time.time - groundStartTime);
        UI.GetComponent<UI>().time = (int)groundTime;
        if (p1a.HP <= 0 || p2a.HP <= 0 || groundTime <= 0)
        {
            Time.timeScale = 0f;
            UI.GetComponent<UI>().gameOver.SetActive(true);
        }
    }

    public void Reset()
    {
        UI.GetComponent<UI>().gameOver.SetActive(false);
        p1m.transform.position = player1InitPos;
        p2m.transform.position = player2InitPos;
        player1.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        player2.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        p1m.ClearBullets();
        p2m.ClearBullets();
        p1m.transform.localScale = new Vector3(1, 1, 1);
        p2m.transform.localScale = new Vector3(-1, 1, 1);
        groundStartTime = Time.time;
        groundTime = 30;
        p1a.HP = 10;
        p2a.HP = 10;
    }
}

