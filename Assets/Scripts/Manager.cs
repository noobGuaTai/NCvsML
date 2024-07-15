using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System;
using PlayerEnum;
using System.Net.Sockets;

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

    public GameObject errorNotice;
    public GameObject waitingConnect;

    private Vector3 player1InitPos;
    private Vector3 player2InitPos;

    void Start()
    {
        mainParaInstance.socket1PortInputField.text = "12345";
        mainParaInstance.socket2PortInputField.text = "22345";

        platformParaInstance.recordSavePath.text = Application.streamingAssetsPath + "/Record";
        platformParaInstance.runSpeed.text = "1";
        platformParaInstance.groundNum.text = "10";

        gameParaInstance.player1HP.text = "10";
        gameParaInstance.player2HP.text = "10";
        gameParaInstance.player1FireRate.text = "0.8";
        gameParaInstance.player2FireRate.text = "0.8";
        gameParaInstance.player1JumpSpeed.text = "16";
        gameParaInstance.player2JumpSpeed.text = "16";
        gameParaInstance.bulletSpeed.text = "9";
        gameParaInstance.gravity.text = "4";
        gameParaInstance.groundTime.text = "30";
        gameParaInstance.configurePath.text = Application.streamingAssetsPath + "/cfg";
        player1InitPos = player1.transform.position;
        player2InitPos = player2.transform.position;
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
        runManager.runMode1 = mainParaInstance.player1SelectDropdown.options[value].text == "Socket" ? RunMode.Socket : mainParaInstance.player1SelectDropdown.options[value].text == "Robot" ? RunMode.DecisionTree : RunMode.Player;
        if (mainParaInstance.player1SelectDropdown.options[value].text == "Robot")
        {
            mainParaInstance.player1RobotSelectDropdown.gameObject.SetActive(true);
        }
        else
        {
            mainParaInstance.player1RobotSelectDropdown.gameObject.SetActive(false);
        }
    }

    public void ChangePlayer2Mode(int value)
    {
        runManager.runMode2 = mainParaInstance.player2SelectDropdown.options[value].text == "Socket" ? RunMode.Socket : mainParaInstance.player2SelectDropdown.options[value].text == "Robot" ? RunMode.DecisionTree : RunMode.Player;
        if (mainParaInstance.player2SelectDropdown.options[value].text == "Robot")
        {
            mainParaInstance.player2RobotSelectDropdown.gameObject.SetActive(true);
        }
        else
        {
            mainParaInstance.player2RobotSelectDropdown.gameObject.SetActive(false);
        }
    }

    public void ChangeRobot1Mode(int value)
    {
        runManager.runMode1 = mainParaInstance.player1RobotSelectDropdown.options[value].text == "DecisionTree" ? RunMode.DecisionTree : RunMode.JuniorGA;
    }

    public void ChangeRobot2Mode(int value)
    {
        runManager.runMode2 = mainParaInstance.player2RobotSelectDropdown.options[value].text == "DecisionTree" ? RunMode.DecisionTree : RunMode.JuniorGA;
    }

    public void StartGame()
    {
        SetGameSettings();
        StartCoroutine(MovePlayerWithUI(new Vector3(-17.8f, 0f, 0f)));
        StartCoroutine(MoveUI(rightRectTransform));
        StartCoroutine(StartGameCoroutine());
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        runManager.enabled = false;
        StartCoroutine(ResetTrainProcess());
        StartCoroutine(MovePlayerWithUI(new Vector3(0f, 0f, 0f)));
        BackToMain();
    }

    IEnumerator StartGameCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        runManager.timeSpeed = float.Parse(platformParaInstance.runSpeed.text);
        runManager.roundNum = int.Parse(platformParaInstance.groundNum.text);

        runManager.enabled = true;
        runManager.StartGame();
    }

    IEnumerator MoveUI(Vector3 target)
    {
        float duration = 0.5f;
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
        float duration = 0.5f;
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

    public void SetGameSettings()
    {
        player1.GetComponent<PlayerAttribute>().HP = int.Parse(gameParaInstance.player1HP.text);
        player2.GetComponent<PlayerAttribute>().HP = int.Parse(gameParaInstance.player2HP.text);
        player1.GetComponent<PlayerFSM>().parameters.shootCoolDown = float.Parse(gameParaInstance.player1FireRate.text);
        player2.GetComponent<PlayerFSM>().parameters.shootCoolDown = float.Parse(gameParaInstance.player2FireRate.text);
        player1.GetComponent<PlayerAttribute>().jumpSpeed = int.Parse(gameParaInstance.player1JumpSpeed.text);
        player2.GetComponent<PlayerAttribute>().jumpSpeed = int.Parse(gameParaInstance.player2JumpSpeed.text);
        player1.GetComponent<PlayerFSM>().parameters.bulletSpeed = float.Parse(gameParaInstance.bulletSpeed.text);
        player2.GetComponent<PlayerFSM>().parameters.bulletSpeed = float.Parse(gameParaInstance.bulletSpeed.text);
        player1.GetComponent<Rigidbody2D>().gravityScale = float.Parse(gameParaInstance.gravity.text);
        player2.GetComponent<Rigidbody2D>().gravityScale = float.Parse(gameParaInstance.gravity.text);
        runManager.totalTime = float.Parse(gameParaInstance.groundTime.text);
    }

    public void BackToMain()
    {
        StartCoroutine(MoveUI(new Vector3(0f, 0f, 0f)));
    }

    public void OpenPlatformSettings()
    {
        StartCoroutine(MoveUI(new Vector3(0f, -1080f, 0f)));
    }

    public void OpenGameSettings()
    {
        StartCoroutine(MoveUI(new Vector3(0f, 1080f, 0f)));
    }

    public void CloseNotice()
    {
        errorNotice.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = "";
        errorNotice.SetActive(false);
    }

    IEnumerator ResetTrainProcess()
    {
        // waitingConnect.SetActive(false);
        RunSocket socket1 = runManager.socket1;
        RunSocket socket2 = runManager.socket2;
        if (socket1 != null)
            socket1.isRunning = false;
        if (socket2 != null)
            socket2.isRunning = false;
        yield return new WaitForSeconds(1f);
        socket1?.Shutdown();
        socket2?.Shutdown();
        runManager.Reset();
        runManager.iteration = 1;
        runManager.isStartGame = false;
        runManager.player1WinNum = 0;
        runManager.player2WinNum = 0;
        player1.parameters.playerAction[0] = PlayerActionType.None;
        player1.parameters.playerAction[1] = PlayerActionType.None;
        player1.parameters.playerAction[2] = PlayerActionType.None;
        player2.parameters.playerAction[0] = PlayerActionType.None;
        player2.parameters.playerAction[1] = PlayerActionType.None;
        player2.parameters.playerAction[2] = PlayerActionType.None;
        player1.transform.position = player1InitPos;
        player2.transform.position = player2InitPos;
        player1.transform.localScale = new Vector3(1f, 1f, 1f);
        player2.transform.localScale = new Vector3(-1f, 1f, 1f);
    }

}
