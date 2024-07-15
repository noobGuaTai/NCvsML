using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System;
using PlayerEnum;

[Serializable]
public class MainParameters
{
    public TMP_Dropdown player1SelectDropdown;
    public TMP_Dropdown player2SelectDropdown;
    public TMP_InputField socket1PortInputField;
    public TMP_InputField socket2PortInputField;
    public TMP_Dropdown player1RobotSelectDropdown;
    public TMP_Dropdown player2RobotSelectDropdown;
}

[Serializable]
public class PlatformParameters
{
    public TMP_InputField recordSavePath;
    public TMP_InputField runSpeed;
    public TMP_InputField groundNum;
}

[Serializable]
public class GameParameters
{
    public TMP_InputField player1HP;
    public TMP_InputField player2HP;
    public TMP_InputField player1FireRate;
    public TMP_InputField player2FireRate;
    public TMP_InputField player1JumpSpeed;
    public TMP_InputField player2JumpSpeed;
    public TMP_InputField bulletSpeed;
    public TMP_InputField gravity;
    public TMP_InputField groundTime;
    public TMP_InputField configurePath;
}



public class Manager : MonoBehaviour
{
    public RunManager runManager;
    public PlayerFSM player1;
    public PlayerFSM player2;
    public RectTransform rectTransformAll;
    public Vector3 rightRectTransform;
    public Vector3 leftRectTransform;
    public Vector3 topRectTransform;
    public Vector3 bottomRectTransform;
    public GameObject envTransform;
    public GameObject playerTransform;

    public MainParameters mainParaInstance;
    public PlatformParameters platformParaInstance;
    public GameParameters gameParaInstance;

    public GameObject notice;
    public GameObject waitingConnect;

    void Start()
    {
        mainParaInstance.player1SelectDropdown.onValueChanged.AddListener(ChangePlayer1Mode);
        mainParaInstance.player2SelectDropdown.onValueChanged.AddListener(ChangePlayer2Mode);
    }

    void Update()
    {
        
    }

    // public void ChangeTrainSpeed()
    // {
    //     if (trainSpeed.text != null && runManager != null)
    //         runManager.GetComponent<Train2Manager>().timeSpeed = int.Parse(trainSpeed.text);
    //     if (trainSpeed.text != null && train1Manager != null)
    //         train1Manager.GetComponent<Train1Manager>().timeSpeed = int.Parse(trainSpeed.text);
    // }

    public void ChangePlayer1Mode(int value)
    {
        if (mainParaInstance.player1SelectDropdown.options[value].text == "Socket")
        {

        }
        if (mainParaInstance.player1SelectDropdown.options[value].text == "Robot")
        {

        }
        if (mainParaInstance.player1SelectDropdown.options[value].text == "You")
        {

        }
    }

    public void ChangePlayer2Mode(int value)
    {
        if (mainParaInstance.player2SelectDropdown.options[value].text == "Socket")
        {

        }
        if (mainParaInstance.player2SelectDropdown.options[value].text == "Robot")
        {

        }
        if (mainParaInstance.player2SelectDropdown.options[value].text == "You")
        {

        }
    }

    public void StartGame()
    {
        StartCoroutine(MovePlayerWithUI(new Vector3(-17.5f, 0f, 0f)));
        StartCoroutine(MoveUI(rightRectTransform));
        runManager.enabled = true;
        runManager.StartGame();
    }

    public void ResetGame()
    {
        player1.parameters.playerAction[0] = PlayerActionType.None;
        player1.parameters.playerAction[1] = PlayerActionType.None;
        player1.parameters.playerAction[2] = PlayerActionType.None;
        player2.parameters.playerAction[0] = PlayerActionType.None;
        player2.parameters.playerAction[1] = PlayerActionType.None;
        player2.parameters.playerAction[2] = PlayerActionType.None;
        StartCoroutine(ResetTrainProcess());
    }

    IEnumerator MoveUI(Vector3 target)
    {
        float duration = 1.0f;
        float elapsedTime = 0.0f;
        Vector3 startPosition = rectTransformAll.anchoredPosition;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            rectTransformAll.anchoredPosition = Vector3.Lerp(startPosition, target, t);
            yield return null;
        }
        rectTransformAll.anchoredPosition = target;
    }

    IEnumerator MovePlayerWithUI(Vector3 target)
    {
        float duration = 1.0f;
        float elapsedTime = 0.0f;
        Vector3 startPosition = envTransform.transform.position;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            envTransform.transform.position = Vector3.Lerp(startPosition, target, t);
            playerTransform.transform.position = Vector3.Lerp(startPosition, target, t);
            yield return null;
        }
        envTransform.transform.position = target;
        playerTransform.transform.position = target;
    }

    public void CloseNotice()
    {
        notice.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = "";
        notice.SetActive(false);
    }

    IEnumerator ResetTrainProcess()
    {
        waitingConnect.SetActive(false);
        runManager.GetComponent<Train2Manager>().socket1.isRunning = false;
        runManager.GetComponent<Train2Manager>().socket2.isRunning = false;
        yield return new WaitForSeconds(1f);
        runManager.GetComponent<Train2Manager>().socket1.Shutdown();
        runManager.GetComponent<Train2Manager>().socket2.Shutdown();
        runManager.GetComponent<Train2Manager>().Reset();
        runManager.GetComponent<Train2Manager>().iteration = 1;
        runManager.GetComponent<Train2Manager>().isStartTrain = false;
    }

}
