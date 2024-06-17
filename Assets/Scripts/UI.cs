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
    public GameObject gameOver;
    public GameObject waitingConnect;

    public GameObject Player1HP;
    public GameObject Player2HP;
    public GameObject Player1;
    public GameObject Player2;

    void Update()
    {
        Player1HP.GetComponent<TextMeshProUGUI>().text = "Player1 HP:" + Player1.GetComponent<PlayerAttribute>().HP;
        Player2HP.GetComponent<TextMeshProUGUI>().text = "Player2 HP:" + Player2.GetComponent<PlayerAttribute>().HP;
        groundTime.GetComponent<TextMeshProUGUI>().text = time.ToString();
        Iteration.GetComponent<TextMeshProUGUI>().text = "Iteration:" + (train.GetComponent<Train2Manager>().iteration - 1).ToString();
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

}
