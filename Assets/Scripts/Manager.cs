using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System;

[Serializable]
public class TrainGA
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
public class TrainRL
{
    public TMP_InputField socket1PortInputField;
    public TMP_InputField trainSpeed;
    public TMP_Dropdown chooseAgentDropdown;
    public GameObject chooseAgentGameObject;
}

[Serializable]
public class InferGA
{
    public TMP_InputField socket1CSVPath;
    public TMP_InputField socket2CSVPath;
    public bool isControlPlayer1 = true;
    public bool isControlPlayer2 = true;
    public TMP_Dropdown player1ControlModeDropdown;
    public TMP_Dropdown player2ControlModeDropdown;
}

[Serializable]
public class InferRL
{

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
    public enum TrainMode
    {
        GA,
        RL
    }
    public TrainMode trainMode = TrainMode.GA;
    public GameObject settings;
    public TMP_Dropdown runModeDropDown;
    public GameObject UI;
    public GameObject trainSettingsUI;
    public GameObject inferSettingsUI;
    public GameObject player1;
    public GameObject player2;
    public GameObject trainGAUI;
    public GameObject trainRLUI;
    public GameObject inferGAUI;
    public GameObject inferRLUI;
    public TMP_Dropdown trainModeDropdown;
    public TMP_Dropdown inferModeDropdown;

    public TrainGA trainGAInstance;
    public TrainRL trainRLInstance;
    public InferGA inferGAInstance;
    public InferRL inferRLInstance;

    void Start()
    {
        runModeDropDown.onValueChanged.AddListener(ChangeRunMode);
        inferGAInstance.player1ControlModeDropdown.onValueChanged.AddListener(ChangePlayer1Mode);
        inferGAInstance.player2ControlModeDropdown.onValueChanged.AddListener(ChangePlayer2Mode);
        trainGAInstance.agentNumDropdown.onValueChanged.AddListener(ChangeGAAgentNum);
        trainGAInstance.chooseAgentDropdown.onValueChanged.AddListener(ChangeChooseGAAgent);
        trainModeDropdown.onValueChanged.AddListener(ChangeTrainMode);
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
        if (inferGAInstance.player1ControlModeDropdown.options[value].text == "You")
        {
            inferGAInstance.isControlPlayer1 = true;
        }
        if (inferGAInstance.player1ControlModeDropdown.options[value].text == "Agent")
        {
            inferGAInstance.isControlPlayer1 = false;
        }
    }

    public void ChangePlayer2Mode(int value)
    {
        if (inferGAInstance.player2ControlModeDropdown.options[value].text == "You")
        {
            inferGAInstance.isControlPlayer2 = true;
        }
        if (inferGAInstance.player2ControlModeDropdown.options[value].text == "Agent")
        {
            inferGAInstance.isControlPlayer2 = false;
        }
    }

    public void ChangeTrainMode(int value)
    {
        if (trainModeDropdown.options[value].text == "GA")
        {
            trainGAUI.SetActive(true);
            trainRLUI.SetActive(false);
            trainMode = TrainMode.GA;
        }
        if (trainModeDropdown.options[value].text == "RL")
        {
            trainGAUI.SetActive(false);
            trainRLUI.SetActive(true);
            trainMode = TrainMode.RL;
        }
    }

    public void ChangeGAAgentNum(int value)
    {
        if (trainGAInstance.agentNumDropdown.options[value].text == "1")
        {
            trainGAInstance.chooseAgentGameObject.SetActive(true);
            trainGAInstance.socket2PortGameObject.SetActive(false);
            trainGAInstance.agentNum = 1;
        }
        if (trainGAInstance.agentNumDropdown.options[value].text == "2")
        {
            trainGAInstance.chooseAgentGameObject.SetActive(false);
            trainGAInstance.socket2PortGameObject.SetActive(true);
            trainGAInstance.agentNum = 2;
        }
    }

    public void ChangeChooseGAAgent(int value)
    {
        if (trainGAInstance.chooseAgentDropdown.options[value].text == "Decision Tree")
        {

        }
        if (trainGAInstance.chooseAgentDropdown.options[value].text == "Junior Agent(GA)")
        {

        }
        if (trainGAInstance.chooseAgentDropdown.options[value].text == "Senior Agent(GA)")
        {

        }
    }

    public void SaveSettings()
    {
        if (gameRunMode == RunMode.Train)
        {
            if (trainMode == TrainMode.GA)
            {
                if (trainGAInstance.agentNum == 1)
                {
                    train1Manager.GetComponent<Train1Manager>().socket1Port = int.Parse(trainGAInstance.socket1PortInputField.text);
                    train1Manager.GetComponent<Train1Manager>().timeSpeed = int.Parse(trainGAInstance.trainSpeed.text);

                    settings.SetActive(false);
                    train1Manager.SetActive(true);
                    UI.GetComponent<UI>().TrainUI();
                    train1Manager.GetComponent<Train1Manager>().StartTrain();
                }
                if (trainGAInstance.agentNum == 2)
                {
                    train2Manager.GetComponent<Train2Manager>().socket1Port = int.Parse(trainGAInstance.socket1PortInputField.text);
                    train2Manager.GetComponent<Train2Manager>().socket2Port = int.Parse(trainGAInstance.socket2PortInputField.text);
                    train2Manager.GetComponent<Train2Manager>().timeSpeed = int.Parse(trainGAInstance.trainSpeed.text);

                    settings.SetActive(false);
                    train2Manager.SetActive(true);
                    UI.GetComponent<UI>().TrainUI();
                    train2Manager.GetComponent<Train2Manager>().StartTrain();
                }
            }
            if (trainMode == TrainMode.RL)
            {
                train1Manager.GetComponent<Train1Manager>().socket1Port = int.Parse(trainRLInstance.socket1PortInputField.text);
                train1Manager.GetComponent<Train1Manager>().timeSpeed = int.Parse(trainRLInstance.trainSpeed.text);

                settings.SetActive(false);
                train1Manager.SetActive(true);
                UI.GetComponent<UI>().TrainUI();
                train1Manager.GetComponent<Train1Manager>().StartTrain();
            }
        }
        if (gameRunMode == RunMode.Infer)
        {
            // infer.GetComponent<Infer>().filePath = inferGAInstance.socket1CSVPath.text;
            // infer.GetComponent<Infer>().filePath2 = inferGAInstance.socket2CSVPath.text;
            // player1.GetComponent<PlayerMove>().isControl = inferGAInstance.isControlPlayer1;
            // player2.GetComponent<PlayerMove>().isControl = inferGAInstance.isControlPlayer2;

            // settings.SetActive(false);
            // UI.GetComponent<UI>().InferUI();
            // infer.GetComponent<Infer>().groundStartTime = Time.time;
            // infer.SetActive(true);
            // infer.GetComponent<Infer>().StartInfer();
            inferManager.GetComponent<InferManager>().path1 = inferGAInstance.socket1CSVPath.text;
            inferManager.GetComponent<InferManager>().path2 = inferGAInstance.socket2CSVPath.text;
            player1.GetComponent<PlayerMove>().isControl = inferGAInstance.isControlPlayer1;
            player2.GetComponent<PlayerMove>().isControl = inferGAInstance.isControlPlayer2;

            settings.SetActive(false);
            UI.GetComponent<UI>().InferUI();
            inferManager.GetComponent<InferManager>().groundStartTime = Time.time;
            inferManager.SetActive(true);
            inferManager.GetComponent<InferManager>().StartInfer();
        }
    }

    public void ResetGame()
    {
        if (gameRunMode == RunMode.Train && train2Manager != null)
            StartCoroutine(ResetTrainProcess());

        if (gameRunMode == RunMode.Infer && inferManager != null)
        {
            inferManager.GetComponent<InferManager>().Reset();
            inferManager.SetActive(false);
        }
    }

    IEnumerator ResetTrainProcess()
    {
        // train.GetComponent<Train>().s1.StopListen();
        // train.GetComponent<Train>().s2.StopListen();
        UI.GetComponent<UI>().waitingConnect.SetActive(false);
        train2Manager.GetComponent<Train2Manager>().s1.isRunning = false;
        train2Manager.GetComponent<Train2Manager>().s2.isRunning = false;
        yield return new WaitForSeconds(0.2f);
        train2Manager.GetComponent<Train2Manager>().s1.Shutdown();
        train2Manager.GetComponent<Train2Manager>().s2.Shutdown();
        train2Manager.GetComponent<Train2Manager>().Reset();
        train2Manager.GetComponent<Train2Manager>().iteration = 1;
        train2Manager.GetComponent<Train2Manager>().generation = 1;
        train2Manager.GetComponent<Train2Manager>().isStartTrain = false;
        UI.GetComponent<UI>().groundTime.SetActive(false);
        UI.GetComponent<UI>().Iteration.SetActive(false);
        UI.GetComponent<UI>().Generation.SetActive(false);
        train2Manager.SetActive(false);
    }

}
