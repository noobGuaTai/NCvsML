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
public class TrainpParameters
{
    public TMP_InputField socket1PortInputField;
    public TMP_InputField socket2PortInputField;
    public GameObject socket2PortGameObject;
    public TMP_InputField trainSpeed;
    public TMP_Dropdown agentNumDropdown;
    public TMP_Dropdown chooseAgentDropdown;
    public GameObject chooseAgentGameObject;
    public int agentNum = 1;

}


[Serializable]
public class InferParameters
{
    public TMP_InputField socket1CSVPath;
    public TMP_InputField socket2CSVPath;
    public bool isControlPlayer1 = true;
    public bool isControlPlayer2 = true;
    public TMP_Dropdown player1ControlModeDropdown;
    public TMP_Dropdown player2ControlModeDropdown;
}


public class Manager : MonoBehaviour
{
    public GameObject train2Manager;
    public GameObject train1Manager;
    public GameObject inferManager;
    public enum RunMode
    {
        Train,
        Infer
    }
    public RunMode gameRunMode = RunMode.Train;
    public GameObject settings;
    public TMP_Dropdown runModeDropDown;
    public GameObject UI;
    public GameObject trainSettingsUI;
    public GameObject inferSettingsUI;
    public GameObject player1;
    public GameObject player2;
    public GameObject trainUI;
    public GameObject inferUI;

    public TrainpParameters trainInstance;
    public InferParameters inferInstance;

    void Start()
    {
        runModeDropDown.onValueChanged.AddListener(ChangeRunMode);
        inferInstance.player1ControlModeDropdown.onValueChanged.AddListener(ChangePlayer1Mode);
        inferInstance.player2ControlModeDropdown.onValueChanged.AddListener(ChangePlayer2Mode);
        trainInstance.agentNumDropdown.onValueChanged.AddListener(ChangeGAAgentNum);
        trainInstance.chooseAgentDropdown.onValueChanged.AddListener(ChangeChooseGAAgent);
    }

    void Update()
    {
        if (gameRunMode == RunMode.Train)
        {
            trainSettingsUI.SetActive(true);
            inferSettingsUI.SetActive(false);
        }
        if (gameRunMode == RunMode.Infer)
        {
            trainSettingsUI.SetActive(false);
            inferSettingsUI.SetActive(true);
        }
    }

    public void OpenSettings()
    {
        settings.SetActive(true);
    }

    public void CloseSettings()
    {
        settings.SetActive(false);
    }

    public void ChangeRunMode(int value)
    {
        if (runModeDropDown.options[value].text == "Train")
        {
            gameRunMode = RunMode.Train;
        }
        if (runModeDropDown.options[value].text == "Infer")
        {
            gameRunMode = RunMode.Infer;
        }
    }

    public void ChangePlayer1Mode(int value)
    {
        if (inferInstance.player1ControlModeDropdown.options[value].text == "You")
        {
            inferInstance.isControlPlayer1 = true;
        }
        if (inferInstance.player1ControlModeDropdown.options[value].text == "Agent")
        {
            inferInstance.isControlPlayer1 = false;
        }
    }

    public void ChangePlayer2Mode(int value)
    {
        if (inferInstance.player2ControlModeDropdown.options[value].text == "You")
        {
            inferInstance.isControlPlayer2 = true;
        }
        if (inferInstance.player2ControlModeDropdown.options[value].text == "Agent")
        {
            inferInstance.isControlPlayer2 = false;
        }
    }

    public void ChangeGAAgentNum(int value)
    {
        if (trainInstance.agentNumDropdown.options[value].text == "1")
        {
            trainInstance.chooseAgentGameObject.SetActive(true);
            trainInstance.socket2PortGameObject.SetActive(false);
            trainInstance.agentNum = 1;
        }
        if (trainInstance.agentNumDropdown.options[value].text == "2")
        {
            trainInstance.chooseAgentGameObject.SetActive(false);
            trainInstance.socket2PortGameObject.SetActive(true);
            trainInstance.agentNum = 2;
        }
    }

    public void ChangeChooseGAAgent(int value)
    {
        if (trainInstance.chooseAgentDropdown.options[value].text == "Decision Tree")
        {

        }
        if (trainInstance.chooseAgentDropdown.options[value].text == "Junior Agent(GA)")
        {

        }
        if (trainInstance.chooseAgentDropdown.options[value].text == "Senior Agent(GA)")
        {

        }
    }

    public void SaveSettings()
    {
        if (gameRunMode == RunMode.Train)
        {
            if (trainInstance.agentNum == 1)
            {
                train1Manager.GetComponent<Train1Manager>().socket1Port = int.Parse(trainInstance.socket1PortInputField.text);
                train1Manager.GetComponent<Train1Manager>().timeSpeed = int.Parse(trainInstance.trainSpeed.text);

                settings.SetActive(false);
                train1Manager.SetActive(true);
                UI.GetComponent<UI>().TrainUI();
                train1Manager.GetComponent<Train1Manager>().StartTrain();
            }
            if (trainInstance.agentNum == 2)
            {
                train2Manager.GetComponent<Train2Manager>().socket1Port = int.Parse(trainInstance.socket1PortInputField.text);
                train2Manager.GetComponent<Train2Manager>().socket2Port = int.Parse(trainInstance.socket2PortInputField.text);
                train2Manager.GetComponent<Train2Manager>().timeSpeed = int.Parse(trainInstance.trainSpeed.text);

                settings.SetActive(false);
                train2Manager.SetActive(true);
                UI.GetComponent<UI>().TrainUI();
                train2Manager.GetComponent<Train2Manager>().StartTrain();
            }

        }
        if (gameRunMode == RunMode.Infer)
        {
            inferManager.GetComponent<InferManager>().path1 = inferInstance.socket1CSVPath.text;
            inferManager.GetComponent<InferManager>().path2 = inferInstance.socket2CSVPath.text;
            player1.GetComponent<PlayerFSM>().parameters.isControl = inferInstance.isControlPlayer1;
            player2.GetComponent<PlayerFSM>().parameters.isControl = inferInstance.isControlPlayer2;

            settings.SetActive(false);
            UI.GetComponent<UI>().InferUI();
            inferManager.GetComponent<InferManager>().groundStartTime = Time.time;
            inferManager.SetActive(true);
            inferManager.GetComponent<InferManager>().StartInfer();
        }
    }

    public void ResetGame()
    {
        player1.GetComponent<PlayerFSM>().parameters.playerAction[0] = PlayerActionType.None;
        player1.GetComponent<PlayerFSM>().parameters.playerAction[1] = PlayerActionType.None;
        player1.GetComponent<PlayerFSM>().parameters.playerAction[2] = PlayerActionType.None;
        player2.GetComponent<PlayerFSM>().parameters.playerAction[0] = PlayerActionType.None;
        player2.GetComponent<PlayerFSM>().parameters.playerAction[1] = PlayerActionType.None;
        player2.GetComponent<PlayerFSM>().parameters.playerAction[2] = PlayerActionType.None;

        if (gameRunMode == RunMode.Train)
            StartCoroutine(ResetTrainProcess());

        if (gameRunMode == RunMode.Infer)
        {
            inferManager.GetComponent<InferManager>().Reset();
            inferManager.SetActive(false);
        }
    }

    IEnumerator ResetTrainProcess()
    {
        if (trainInstance.agentNum == 1)
        {
            UI.GetComponent<UI>().waitingConnect.SetActive(false);
            train1Manager.GetComponent<Train1Manager>().socket1.isRunning = false;
            yield return new WaitForSeconds(1f);
            train1Manager.GetComponent<Train1Manager>().socket1.Shutdown();
            train1Manager.GetComponent<Train1Manager>().Reset();
            train1Manager.GetComponent<Train1Manager>().iteration = 1;
            train1Manager.GetComponent<Train1Manager>().isStartTrain = false;
            UI.GetComponent<UI>().groundTime.SetActive(false);
            UI.GetComponent<UI>().Iteration.SetActive(false);
            train1Manager.SetActive(false);
        }
        if (trainInstance.agentNum == 2)
        {
            UI.GetComponent<UI>().waitingConnect.SetActive(false);
            train2Manager.GetComponent<Train2Manager>().socket1.isRunning = false;
            train2Manager.GetComponent<Train2Manager>().socket2.isRunning = false;
            yield return new WaitForSeconds(1f);
            train2Manager.GetComponent<Train2Manager>().socket1.Shutdown();
            train2Manager.GetComponent<Train2Manager>().socket2.Shutdown();
            train2Manager.GetComponent<Train2Manager>().Reset();
            train2Manager.GetComponent<Train2Manager>().iteration = 1;
            train2Manager.GetComponent<Train2Manager>().isStartTrain = false;
            UI.GetComponent<UI>().groundTime.SetActive(false);
            UI.GetComponent<UI>().Iteration.SetActive(false);
            train2Manager.SetActive(false);
        }

    }

}
