using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI : MonoBehaviour
{
    private GameObject Player1HP;
    private GameObject Player2HP;
    private GameObject Player1;
    private GameObject Player2;
    void Start()
    {
        Player1HP = GameObject.Find("Player1HP");
        Player2HP = GameObject.Find("Player2HP");
        Player1 = GameObject.Find("Player1");
        Player2 = GameObject.Find("Player2");
    }

    void Update()
    {
        Player1HP.GetComponent<TextMeshProUGUI>().text = "Player1 HP:" + Player1.GetComponent<PlayerAttribute>().HP;
        Player2HP.GetComponent<TextMeshProUGUI>().text = "Player2 HP:" + Player2.GetComponent<PlayerAttribute>().HP;
    }


}
