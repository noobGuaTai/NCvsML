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
    public GameObject none1;
    public GameObject none2;
    public GameObject gameMode;
}

[Serializable]
public class PlatformParameters
{
    public TMP_InputField recordSavePath;
    public TMP_InputField recordSaveNum;
    public TMP_InputField runSpeed;
    public TMP_InputField groundNum;
    public GameObject GameLogButton;
    public GameObject ScoreButton;
    public GameObject ErrorButton;
    public GameObject HurtButton;
    public GameObject ActionButton;

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
    public TMP_InputField logSavePath;
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

    public Tween tween1;
    public Tween tween2;

    private Vector3 player1InitPos;
    private Vector3 player2InitPos;

    void Start()
    {
        mainParaInstance.socket1PortInputField.text = "12345";
        mainParaInstance.socket2PortInputField.text = "22345";
        mainParaInstance.player1RobotSelectDropdown.options[0].text = "<size=100>Mr.Tree</size><size=40>(Made by BehaviorTree)</size>";
        mainParaInstance.player1RobotSelectDropdown.options[1].text = "<size=100>Darwin</size><size=40>(Made by Genetic Algorithm)</size>";
        mainParaInstance.player2RobotSelectDropdown.options[0].text = "<size=100>Mr.Tree</size><size=40>(Made by BehaviorTree)</size>";
        mainParaInstance.player2RobotSelectDropdown.options[1].text = "<size=100>Darwin</size><size=40>(Made by Genetic Algorithm)</size>";

        platformParaInstance.recordSavePath.text = Application.streamingAssetsPath + "/Record/record.json";
        platformParaInstance.recordSaveNum.text = "50";
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
        gameParaInstance.logSavePath.text = Application.streamingAssetsPath + "/log/log.txt";
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

        if (mainParaInstance.player1SelectDropdown.options[value].text == "You")
            mainParaInstance.none1.SetActive(true);
        else
            mainParaInstance.none1.SetActive(false);
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
        if (mainParaInstance.player2SelectDropdown.options[value].text == "You")
            mainParaInstance.none2.SetActive(true);
        else
            mainParaInstance.none2.SetActive(false);
    }

    public void ChangeRobot1Mode(int value)
    {
        runManager.runMode1 = value == 0 ? RunMode.DecisionTree : RunMode.JuniorGA;
    }

    public void ChangeRobot2Mode(int value)
    {
        runManager.runMode2 = value == 0 ? RunMode.DecisionTree : RunMode.JuniorGA;
    }

    public void StartGame()
    {
        SetGameSettings();
        if(MovePlayerWithUI(new Vector3(-17.8f, 0f, 0f)) == 0)
            return;
        MoveUI(rightRectTransform);
        StartCoroutine(StartGameCoroutine());
    }

    public void QuitRound()
    {
        if(MovePlayerWithUI(new Vector3(0f, 0f, 0f)) == 0)
            return;
        BackToMain();  
        Time.timeScale = 1f;
        runManager.recordDataList.Clear();
        runManager.replayByState?.StopReplay();
        runManager.enabled = false;
        StartCoroutine(ResetTrainProcess());
        
        
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    IEnumerator StartGameCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        runManager.timeSpeed = float.Parse(platformParaInstance.runSpeed.text);
        runManager.roundNum = int.Parse(platformParaInstance.groundNum.text);
        runManager.recordNum = int.Parse(platformParaInstance.recordSaveNum.text);

        runManager.enabled = true;
        runManager.StartGame();
    }

    void MoveUI(Vector3 target)
    {
        Vector3 startPosition = rectTransformAll.anchoredPosition;
        if (tween1._tweenState == Tween.TweenState.STOP)
        {
            tween1.AddTween((Vector3 a) =>
            {
                rectTransformAll.anchoredPosition = a;
            }, startPosition, target, 0.5f, Tween.TransitionType.QUART, Tween.EaseType.OUT);
            tween1.Play();
        }
    }

    int MovePlayerWithUI(Vector3 target)
    {
        Vector3 startPosition = envTransform.transform.position;
        if (tween1._tweenState == Tween.TweenState.STOP)
        {
            tween2.AddTween((Vector3 a) =>
            {
                playerTransform.transform.position = a;
                envTransform.transform.position = a;
            }, startPosition, target, 0.5f, Tween.TransitionType.QUART, Tween.EaseType.OUT);
            tween2.Play();
            return 1;// 正常
        }
        else
            return 0;// 有tween在运行，终止
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
        MoveUI(new Vector3(0f, 0f, 0f));
    }

    public void OpenPlatformSettings()
    {
        MoveUI(new Vector3(0f, -1080f, 0f));
    }

    public void OpenGameSettings()
    {
        MoveUI(new Vector3(0f, 1080f, 0f));
    }

    public void OpenHelpSettings()
    {
        MoveUI(new Vector3(1920f, 0f, 0f));
    }

    public void CloseNotice()
    {
        errorNotice.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = "";
        errorNotice.SetActive(false);
    }

    public void GameLogOnClick()
    {
        runManager.logContent["Log"] = !runManager.logContent["Log"];
    }
    public void ScoreOnClick()
    {
        runManager.logContent["Score"] = !runManager.logContent["Score"];
    }

    public void ErrorOnClick()
    {
        runManager.logContent["Error"] = !runManager.logContent["Error"];
    }

    public void HurtOnClick()
    {
        runManager.logContent["Hurt"] = !runManager.logContent["Hurt"];
    }

    public void ActionOnClick()
    {
        runManager.logContent["Action"] = !runManager.logContent["Action"];
    }

    public void ChangeGameMode()
    {
        if (runManager.gameMode == GameMode.Live)
        {
            runManager.gameMode = GameMode.Replay;
            mainParaInstance.gameMode.GetComponentInChildren<TextMeshProUGUI>().text = "Replay";
            mainParaInstance.gameMode.GetComponent<ui_2_color_image>().color = new Color32(212, 48, 48, 255);
        }
        else
        {
            runManager.gameMode = GameMode.Live;
            mainParaInstance.gameMode.GetComponentInChildren<TextMeshProUGUI>().text = "Live";
            mainParaInstance.gameMode.GetComponent<ui_2_color_image>().color = new Color32(40, 125, 218, 255);
        }

    }

    public void ResetGameSettings()
    {
        gameParaInstance.player1HP.text = "10";
        gameParaInstance.player2HP.text = "10";
        gameParaInstance.player1FireRate.text = "0.8";
        gameParaInstance.player2FireRate.text = "0.8";
        gameParaInstance.player1JumpSpeed.text = "16";
        gameParaInstance.player2JumpSpeed.text = "16";
        gameParaInstance.bulletSpeed.text = "9";
        gameParaInstance.gravity.text = "4";
        gameParaInstance.groundTime.text = "30";
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
        yield return new WaitForSeconds(0.5f);
        socket1?.Shutdown();
        socket2?.Shutdown();
        if (runManager.socket1 != null)
            runManager.socket1 = null;
        if (runManager.socket2 != null)
            runManager.socket2 = null;
        runManager.Reset();
        runManager.iteration = 1;
        runManager.isStartGame = false;
        runManager.player1WinNum = 0;
        runManager.player2WinNum = 0;
        runManager.recorder.ClearList();
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
