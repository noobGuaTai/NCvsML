using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerEnum;
using System;
using Unity.Mathematics;
using TMPro;
using Google.Protobuf.WellKnownTypes;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.VisualScripting;
using System.Threading;

public enum RunMode
{
    Socket,
    DecisionTree,
    JuniorGA,
    Player,
}

public enum GameMode
{
    Live,
    Replay
}

[Serializable]
public class GameUI
{
    public TextMeshProUGUI player1HPUI;
    public TextMeshProUGUI player2HPUI;
    public TextMeshProUGUI roundNumUI;
    public TextMeshProUGUI roundTimeUI;
    public TextMeshProUGUI runLogUI;
    public GameObject saveLog;
    public GameObject player1Jump;
    public GameObject player1Shoot;
    public GameObject player1MoveLeft;
    public GameObject player1MoveRight;
    public GameObject player2Jump;
    public GameObject player2Shoot;
    public GameObject player2MoveLeft;
    public GameObject player2MoveRight;
    public TMP_InputField replayRound;
    public GameObject replayButton;
    public GameObject playerControlTips;
}

public class RunManager : MonoBehaviour
{
    public Manager manager;
    public GameUI gameUI;
    public RunSocket socket1;
    public RunSocket socket2;
    public DecisionTree decisionTree1;
    public DecisionTree decisionTree2;
    public AgentInfer agentInfer1;
    public AgentInfer agentInfer2;
    public ReplayByState replayByState;
    public GameObject player1;
    public GameObject player2;
    public RunMode runMode1;
    public RunMode runMode2;
    public GameMode gameMode;
    public bool isEnd = false;// 一场比赛结束标志
    public float roundTime;// 一场比赛剩余时间
    public float totalTime = 60f;// 一场比赛总时间
    public float timeSpeed = 1f;// 时间流速
    public float iterationStartTime;// 每场比赛开始时间
    public int iteration = 1;
    public int roundNum;
    public PlayerFSM player1FSM;
    public PlayerFSM player2FSM;
    public PlayerAttribute player1attribute;
    public PlayerAttribute player2attribute;
    public int player1HP;
    public int player2HP;
    public int isRestart = 0;
    public int isRestartThreshold = 0;
    public int player1WinNum = 0;
    public int player2WinNum = 0;
    public EnvInfo info1;
    public EnvInfo info2;
    public int socket1Port = 12345;
    public int socket2Port = 22345;
    public Vector3 player1InitPos;
    public Vector3 player2InitPos;
    public bool isStartGame = false;
    public List<string> runLog;
    public List<string> allLog;
    public Dictionary<string, bool> logContent = new Dictionary<string, bool>();// 打印哪些日志
    public bool isRoundStart = false;
    public Recorder recorder;
    public int recordNum = 50;// 保存最后50场

    public List<string> recordDataList = new List<string>();

    public delegate void RunTime();
    public RunTime runtime;

    void Awake()
    {
        player1FSM = player1.GetComponent<PlayerFSM>();
        player2FSM = player2.GetComponent<PlayerFSM>();
        player1attribute = player1.GetComponent<PlayerAttribute>();
        player2attribute = player2.GetComponent<PlayerAttribute>();
        player1FSM.parameters.isControl = false;
        player2FSM.parameters.isControl = false;
        logContent.Add("Log", true);
        logContent.Add("Error", true);
        logContent.Add("Score", true);
        logContent.Add("Hurt", true);
        logContent.Add("Action", true);

        recorder = new Recorder();

        gameUI.playerControlTips.SetActive(false);

        // QualitySettings.vSyncCount = 0;
        // Application.targetFrameRate = 50;
    }

    public void StartGame()
    {
        isRestartThreshold = 0;
        decisionTree1 = null;
        decisionTree2 = null;
        agentInfer1 = null;
        agentInfer2 = null;
        runtime = null;
        roundTime = (int)totalTime;
        iterationStartTime = Time.time;
        LogMessage("Log", "Game Start");
        if (gameMode == GameMode.Live)
        {
            gameUI.replayRound.gameObject.SetActive(false);
            gameUI.replayButton.SetActive(false);
            switch (runMode1)
            {
                case RunMode.Socket:
                    player1FSM.parameters.isControl = false;
                    socket1 = new RunSocket(this, PlayerType.player1);
                    socket1.Start(socket1Port);
                    Time.timeScale = 0f;
                    isRestartThreshold++;
                    LogMessage("Log", "Player1 Waiting Connect...");
                    break;
                case RunMode.Player:
                    player1FSM.parameters.isControl = true;
                    break;
                case RunMode.DecisionTree:
                    player1FSM.parameters.isControl = false;
                    decisionTree1 = new DecisionTree(player1, player2, this);
                    break;
                case RunMode.JuniorGA:
                    player1FSM.parameters.isControl = false;
                    agentInfer1 = new AgentInfer(player1, player2, "/172.csv", this);
                    break;
            }

            switch (runMode2)
            {
                case RunMode.Socket:
                    player2FSM.parameters.isControl = false;
                    socket2 = new RunSocket(this, PlayerType.player2);
                    socket2.Start(socket2Port);
                    Time.timeScale = 0f;
                    isRestartThreshold++;
                    LogMessage("Log", "Player2 Waiting Connect...");
                    break;
                case RunMode.Player:
                    player2FSM.parameters.isControl = true;
                    break;
                case RunMode.DecisionTree:
                    player2FSM.parameters.isControl = false;
                    decisionTree2 = new DecisionTree(player2, player1, this);
                    break;
                case RunMode.JuniorGA:
                    player2FSM.parameters.isControl = false;
                    agentInfer2 = new AgentInfer(player2, player1, "/172.csv", this);
                    break;
            }
            if (runMode1 != RunMode.Socket && runMode2 != RunMode.Socket)
            {
                isStartGame = true;
                isRoundStart = true;
                Time.timeScale = timeSpeed;
                iteration = 1;
            }
            else
            {
                iteration = 0;
            }
            // if(runMode1 == RunMode.Player || runMode2 == RunMode.Player)
            //     gameUI.playerControlTips.SetActive(true);
            // else
            //     gameUI.playerControlTips.SetActive(false);
        }
        else
        {
            gameUI.replayRound.gameObject.SetActive(true);
            gameUI.replayButton.SetActive(true);
            replayByState = new ReplayByState(this, manager.platformParaInstance.recordSavePath.text);
            replayByState.OnStart();
        }

    }

    void SocketUpdate()
    {
        GetEnvInf(player1FSM, player2FSM, player1attribute, player2attribute, ref info1);
        GetEnvInf(player2FSM, player1FSM, player2attribute, player1attribute, ref info2);

        player1HP = player1attribute.HP;
        player2HP = player2attribute.HP;

        if (!isEnd && (roundTime <= 0 || player1attribute.HP <= 0 || player2attribute.HP <= 0))
        {
            isEnd = true;
            isRoundStart = false;
            if (player1attribute.HP > player2attribute.HP)
                player1WinNum++;
            else
                player2WinNum++;
            LogMessage("Score", "Score:" + player1WinNum + ":" + player2WinNum);

            Time.timeScale = 0f;
        }
        else
        {
            socket1?.SetEnvInfo(info1);// 结束了就不再更新环境信息
            socket2?.SetEnvInfo(info2);
        }

        if (isRestart == isRestartThreshold && isRestartThreshold != 0)
        {
            StartCoroutine(AddRecord());
            Reset();
            player1.transform.position = player1InitPos;
            player2.transform.position = player2InitPos;

            socket1?.SendMessage(socket1.runHandler, info1);
            socket2?.SendMessage(socket2.runHandler, info2);

            roundTime = (int)totalTime;
            iterationStartTime = Time.time;

            LogMessage("Log", "Round " + iteration + " start");
            isRoundStart = true;

            Time.timeScale = timeSpeed;
            isStartGame = true;
        }
    }

    void AgentUpdate()
    {
        if (runMode1 != RunMode.Socket)
        {
            if (player1FSM.parameters.beShot == true)
            {
                LogMessage("Hurt", "Player1 Hurt");
                player1FSM.parameters.beShot = false;
            }
        }
        if (runMode2 != RunMode.Socket)
        {
            if (player2FSM.parameters.beShot == true)
            {
                LogMessage("Hurt", "Player2 Hurt");
                player2FSM.parameters.beShot = false;
            }
        }
        if (runMode1 != RunMode.Socket && runMode2 != RunMode.Socket)
        {
            if (roundTime <= 0 || player1attribute.HP <= 0 || player2attribute.HP <= 0)
            {
                LogMessage("Log", "Round " + iteration + " end");
                Reset();
                player1.transform.position = player1InitPos;
                player2.transform.position = player2InitPos;
                isRoundStart = true;
                if (isRestartThreshold == 0)
                {
                    StartCoroutine(AddRecord());
                }
            }
        }
        runtime?.Invoke();
    }

    void Update()
    {
        // Time.timeScale = timeSpeed;

        if (isStartGame)
            roundTime = totalTime - (Time.time - iterationStartTime);

        gameUI.player1HPUI.text = "Player1HP:" + player1HP.ToString();
        gameUI.player2HPUI.text = "Player2HP:" + player2HP.ToString();
        gameUI.roundTimeUI.text = "Time:" + ((int)roundTime).ToString();
        if (gameMode == GameMode.Live)
            gameUI.roundNumUI.text = "Round:" + iteration.ToString();

        SocketUpdate();
        AgentUpdate();
        ShowActionUI();

        if (iteration > roundNum && isStartGame)
        {
            Time.timeScale = 0f;
            socket1?.Shutdown();
            socket2?.Shutdown();
            LogMessage("Log", "Game Over");
            isStartGame = false;

        }

        if (roundTime < -10)
        {
            roundTime = 0;
            isStartGame = false;
            LogMessage("Error", "Error:Connection Timeout with Socket.");
        }

    }

    void FixedUpdate()
    {
        if (isRoundStart)
        {
            recorder.AddState(player1FSM, player2FSM);
        }
    }

    public void GetEnvInf(PlayerFSM playerFSM1, PlayerFSM playerFSM2, PlayerAttribute playerAttribute1, PlayerAttribute playerAttribute2, ref EnvInfo info)
    {   
        info.Face_E = playerFSM1.transform.localScale.x * (playerFSM2.transform.position.x - playerFSM1.transform.position.x) / Math.Abs(playerFSM2.transform.position.x - playerFSM1.transform.position.x);
        info.leftWall_XD = playerFSM1.transform.position.x - playerFSM1.parameters.leftWall.transform.position.x;
        info.rightWall_XD = playerFSM1.parameters.rightWall.transform.position.x - playerFSM1.transform.position.x;
        info.E_XD = playerFSM2.transform.position.x - playerFSM1.transform.position.x;
        info.E_YD = playerFSM2.transform.position.y - playerFSM1.transform.position.y;
        if (playerFSM1.parameters.bullets[0] != null)
        {
            info.self_Bullet0_XD = playerFSM1.parameters.bullets[0].transform.position.x - playerFSM2.transform.position.x;
            info.self_Bullet0_YD = playerFSM1.parameters.bullets[0].transform.position.y - playerFSM2.transform.position.y;
        }
        else
        {
            info.self_Bullet0_XD = 13.7f;
            info.self_Bullet0_YD = 3.5f;
        }
        if (playerFSM1.parameters.bullets[1] != null)
        {
            info.self_Bullet1_XD = playerFSM1.parameters.bullets[1].transform.position.x - playerFSM2.transform.position.x;
            info.self_Bullet1_YD = playerFSM1.parameters.bullets[1].transform.position.y - playerFSM2.transform.position.y;
        }
        else
        {
            info.self_Bullet1_XD = 13.7f;
            info.self_Bullet1_YD = 3.5f;
        }
        if (playerFSM2.parameters.bullets[0] != null)
        {
            info.E_Bullet0_XD = playerFSM2.parameters.bullets[0].transform.position.x - playerFSM1.transform.position.x;
            info.E_Bullet0_YD = playerFSM2.parameters.bullets[0].transform.position.y - playerFSM1.transform.position.y;
        }
        else
        {
            info.E_Bullet0_XD = 13.7f;
            info.E_Bullet0_YD = 3.5f;
        }
        if (playerFSM2.parameters.bullets[1] != null)
        {
            info.E_Bullet1_XD = playerFSM2.parameters.bullets[1].transform.position.x - playerFSM1.transform.position.x;
            info.E_Bullet1_YD = playerFSM2.parameters.bullets[1].transform.position.y - playerFSM1.transform.position.y;
        }
        else
        {
            info.E_Bullet1_XD = 13.7f;
            info.E_Bullet1_YD = 3.5f;
        }
        info.time = roundTime;
    }

    public float[] GetEnvInf(PlayerFSM playerFSM1, PlayerFSM playerFSM2, PlayerAttribute playerAttribute1, PlayerAttribute playerAttribute2)
    {
        float[] info = new float[15];
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
        return info;
    }

    public void RunAction(PlayerType player, PlayerActionType[] action)
    {
        PlayerFSM playerFSM = player == PlayerType.player1 ? player1FSM : player2FSM;

        UnityMainThreadDispatcher.Enqueue(() =>
        {
            if (action[0] == PlayerActionType.StartNextGround)
            {
                isRestart += 1;
            }
            if (playerFSM.parameters.isControl)
                return;

            playerFSM.parameters.playerAction = action;
        });
    }

    public void LogMessage(string type, string message)
    {
        if (logContent[type])
        {
            runLog.Add("[" + type + "]: " + "Time " + (int)roundTime + ":" + message);
            allLog.Add("[" + type + "]: " + "Time " + (int)roundTime + ":" + message);
            if (runLog.Count > 15)
            {
                runLog.RemoveAt(0);
            }
            gameUI.runLogUI.text = string.Join("\n", runLog);
        }
    }

    public void ClearLog()
    {
        runLog.Clear();
        gameUI.runLogUI.text = "";
    }

    public void SaveLog()
    {
        if (!Directory.Exists(Path.GetDirectoryName(manager.gameParaInstance.logSavePath.text)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(manager.gameParaInstance.logSavePath.text));
        }
        File.WriteAllLines(manager.gameParaInstance.logSavePath.text, allLog);
        if (gameMode == GameMode.Live)
            SaveRecord();
    }

    public void ShowActionUI()
    {
        gameUI.player1Jump.SetActive(player1FSM.parameters.playerAction[0] == PlayerActionType.Jump);
        gameUI.player1Shoot.SetActive(player1FSM.parameters.playerAction[1] == PlayerActionType.Shoot);
        gameUI.player1MoveLeft.SetActive(player1FSM.parameters.playerAction[2] == PlayerActionType.MoveLeft);
        gameUI.player1MoveRight.SetActive(player1FSM.parameters.playerAction[2] == PlayerActionType.MoveRight);

        gameUI.player2Jump.SetActive(player2FSM.parameters.playerAction[0] == PlayerActionType.Jump);
        gameUI.player2Shoot.SetActive(player2FSM.parameters.playerAction[1] == PlayerActionType.Shoot);
        gameUI.player2MoveLeft.SetActive(player2FSM.parameters.playerAction[2] == PlayerActionType.MoveLeft);
        gameUI.player2MoveRight.SetActive(player2FSM.parameters.playerAction[2] == PlayerActionType.MoveRight);
    }

    IEnumerator AddRecord()
    {
        yield return null;
        var data = new
        {
            Round = iteration,
            TimeSpeed = timeSpeed,
            Count = recorder.count,
            Player1Pos = recorder.player1Pos,
            Player2Pos = recorder.player2Pos,
            Player1Bullet1Pos = recorder.player1Bullet1Pos,
            Player1Bullet2Pos = recorder.player1Bullet2Pos,
            Player2Bullet1Pos = recorder.player2Bullet1Pos,
            Player2Bullet2Pos = recorder.player2Bullet2Pos,
            Player1HP = recorder.player1HP,
            Player2HP = recorder.player2HP,
            Player1LocalScaleX = recorder.player1LocalScaleX,
            Player2LocalScaleX = recorder.player2LocalScaleX,
        };
        string save = JsonConvert.SerializeObject(data, Formatting.Indented);
        recordDataList.Add(save);
        if (recordDataList.Count > recordNum)
        {
            recordDataList.RemoveAt(0);
        }
        recorder.ClearList();
    }

    void SaveRecord()
    {
        if (!Directory.Exists(Path.GetDirectoryName(manager.platformParaInstance.recordSavePath.text)))
            Directory.CreateDirectory(Path.GetDirectoryName(manager.platformParaInstance.recordSavePath.text));
        List<string> save = new List<string>(recordDataList);// 深拷贝 防止在保存时被修改
        SaveJsonArray(manager.platformParaInstance.recordSavePath.text, save);
    }

    void SaveJsonArray(string filePath, List<string> jsonString)
    {
        Thread fileWriteThread = new Thread(() =>
        {
            try
            {
                JArray jsonArray = new JArray();

                foreach (string json in jsonString)
                {
                    JObject newObject = JObject.Parse(json);
                    jsonArray.Add(newObject);
                }

                File.WriteAllText(filePath, jsonArray.ToString(Formatting.Indented));

                // // 完成后回调主线程
                // if (onComplete != null)
                // {
                //     // 使用Unity的MainThreadDispatcher来在主线程中执行回调
                //     MainThreadDispatcher.Instance().Enqueue(onComplete);
                // }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error writing file: " + e.Message);
            }
        });
        fileWriteThread.Start();
    }

    string[] ConvertPlayerAction(PlayerActionType[] actions)
    {
        string[] array = new string[3];

        array[0] = actions[0] == PlayerActionType.Jump ? "1" : "0";
        array[1] = actions[1] == PlayerActionType.Shoot ? "1" : "0";
        array[2] = actions[2] == PlayerActionType.MoveRight ? "1" : actions[2] == PlayerActionType.MoveLeft ? "-1" : "0";

        return array;
    }

    public void Replay()
    {
        Reset();
        player1.transform.position = player1InitPos;
        player2.transform.position = player2InitPos;
        replayByState.Replay(int.Parse(gameUI.replayRound.text));
    }

    public void Reset()
    {
        isRestart = 0;
        // yield return new WaitForSeconds(0.1f);
        player1attribute.HP = 10;
        player2attribute.HP = 10;
        roundTime = (int)totalTime;
        iterationStartTime = Time.time;
        player1.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        player2.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        player1FSM.ClearBullets();
        player2FSM.ClearBullets();
        player1attribute.isInvincible = false;
        player2attribute.isInvincible = false;
        player1.GetComponent<PlayerFSM>().parameters.beShot = false;
        player1.GetComponent<PlayerFSM>().parameters.isShot = false;
        player2.GetComponent<PlayerFSM>().parameters.beShot = false;
        player2.GetComponent<PlayerFSM>().parameters.isShot = false;
        player1.GetComponent<PlayerFSM>().currentState = player1.GetComponent<PlayerFSM>().state[PlayerStateType.Idle];
        player2.GetComponent<PlayerFSM>().currentState = player2.GetComponent<PlayerFSM>().state[PlayerStateType.Idle];
        player1FSM.parameters.canJump = true;
        player2FSM.parameters.canJump = true;
        info1.infoCode = 0;
        info2.infoCode = 0;
        player1.transform.localScale = new Vector3(1, 1, 1);
        player2.transform.localScale = new Vector3(-1, 1, 1);
        isEnd = false;
        if (socket1 != null)
            socket1.hasSendEndInfo = false;
        if (socket2 != null)
            socket2.hasSendEndInfo = false;
        iteration++;
        isRoundStart = false;

        GetEnvInf(player1FSM, player2FSM, player1attribute, player2attribute, ref info1);
        GetEnvInf(player2FSM, player1FSM, player1attribute, player2attribute, ref info2);
    }

}
