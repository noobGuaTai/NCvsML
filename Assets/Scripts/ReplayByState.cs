using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using PlayerEnum;
using UnityEngine;
public class TimeStateSequence
{
    public int Round { get; set; }
    public float TimeSpeed { get; set; }
    public int Count { get; set; }
    public List<float[]> Player1Pos { get; set; }
    public List<float[]> Player2Pos { get; set; }
    public List<float[]> Player1Bullet1Pos { get; set; }
    public List<float[]> Player1Bullet2Pos { get; set; }
    public List<float[]> Player2Bullet1Pos { get; set; }
    public List<float[]> Player2Bullet2Pos { get; set; }
    public List<int[]> Player1HP { get; set; }
    public List<int[]> Player2HP { get; set; }
    public List<int[]> Player1LocalScaleX { get; set; }
    public List<int[]> Player2LocalScaleX { get; set; }
}
public class ReplayByState
{
    private RunManager runManager;
    private List<TimeStateSequence> sequences;
    private string path;
    private PlayerFSM player1FSM;
    private PlayerFSM player2FSM;
    private int isEnd = 0;
    private int round = 0;
    public bool isReady = true;

    private int player1HPIndex = 0;
    private int player2HPIndex = 0;
    private int player1LocalScaleXIndex = 0;
    private int player2LocalScaleXIndex = 0;

    public ReplayByState(RunManager runManager, string path)
    {
        this.runManager = runManager;
        this.path = path;
        player1FSM = runManager.player1FSM;
        player2FSM = runManager.player2FSM;
    }

    public void OnStart()
    {
        sequences = LoadSequencesFromJson(path);
    }

    List<TimeStateSequence> LoadSequencesFromJson(string filePath)
    {
        if (File.Exists(filePath))
        {
            string jsonString = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<List<TimeStateSequence>>(jsonString);
        }
        else
        {
            Debug.LogError("JSON file not found: " + filePath);
            return null;
        }
    }

    public void Replay(int num)
    {
        Time.timeScale = runManager.timeSpeed;
        runManager.isStartGame = true;
        runManager.iteration = 1;
        isEnd = 0;
        player1HPIndex = 0;
        player2HPIndex = 0;
        player1LocalScaleXIndex = 0;
        player2LocalScaleXIndex = 0;
        runManager.StopAllCoroutines();
        runManager.StartCoroutine(ReplaySequences1(num));
        runManager.StartCoroutine(ReplaySequences2(num));
    }

    IEnumerator ReplaySequences1(int num)
    {
        TimeStateSequence sequence = sequences[num];
        runManager.gameUI.roundNumUI.text = "Round:" + sequence.Round.ToString();

        for (int i = 0; i < sequence.Player1Pos.Count; i++)
        {
            ExecuteAction(sequence, player1FSM, i);
            yield return new WaitForFixedUpdate();
        }
        runManager.Reset();
        runManager.player1.transform.position = runManager.player1InitPos;
        runManager.player2.transform.position = runManager.player2InitPos;

        isEnd++;
        if (isEnd == 2)
        {
            Time.timeScale = 0;
        }
    }

    IEnumerator ReplaySequences2(int num)
    {
        TimeStateSequence sequence = sequences[num];
        for (int i = 0; i < sequence.Player1Pos.Count; i++)
        {
            ExecuteAction(sequence, player2FSM, i);
            yield return new WaitForFixedUpdate();
        }

        isEnd++;
        if (isEnd == 2)
        {
            Time.timeScale = 0;
        }
    }

    void ExecuteAction(TimeStateSequence state, PlayerFSM playerFSM, int index)
    {
        playerFSM.transform.position = new Vector2(
            playerFSM == player1FSM ? state.Player1Pos[index][0] : state.Player2Pos[index][0],
            playerFSM == player1FSM ? state.Player1Pos[index][1] : state.Player2Pos[index][1]
        );

        if ((playerFSM == player1FSM ? state.Player1Bullet1Pos[index][0] : state.Player2Bullet1Pos[index][0]) != 0 ||
            (playerFSM == player1FSM ? state.Player1Bullet1Pos[index][1] : state.Player2Bullet1Pos[index][1]) != 0)
        {
            if (playerFSM.parameters.bullets[0] == null)
                playerFSM.Shoot();
            else
                playerFSM.parameters.bullets[0].transform.position = new Vector2(
                    playerFSM == player1FSM ? state.Player1Bullet1Pos[index][0] : state.Player2Bullet1Pos[index][0],
                    playerFSM == player1FSM ? state.Player1Bullet1Pos[index][1] : state.Player2Bullet1Pos[index][1]
                );
        }
        else
        {
            if (playerFSM.parameters.bullets[0] != null)
                GameObject.Destroy(playerFSM.parameters.bullets[0]);
        }

        if ((playerFSM == player1FSM ? state.Player1Bullet2Pos[index][0] : state.Player2Bullet2Pos[index][0]) != 0 ||
            (playerFSM == player1FSM ? state.Player1Bullet2Pos[index][1] : state.Player2Bullet2Pos[index][1]) != 0)
        {
            if (playerFSM.parameters.bullets[1] == null)
                playerFSM.Shoot();
            else
                playerFSM.parameters.bullets[1].transform.position = new Vector2(
                    playerFSM == player1FSM ? state.Player1Bullet2Pos[index][0] : state.Player2Bullet2Pos[index][0],
                    playerFSM == player1FSM ? state.Player1Bullet2Pos[index][1] : state.Player2Bullet2Pos[index][1]
                );
        }
        else
        {
            if (playerFSM.parameters.bullets[1] != null)
                GameObject.Destroy(playerFSM.parameters.bullets[1]);
        }

        if (state.Player1HP.Count > 0 && player1HPIndex < state.Player1HP.Count && state.Player1HP[player1HPIndex][0] == index)
        {
            player1FSM.parameters.playerAttribute.HP = state.Player1HP[player1HPIndex][1];
            player1HPIndex++;
        }

        if (state.Player2HP.Count > 0 && player2HPIndex < state.Player2HP.Count && state.Player2HP[player2HPIndex][0] == index)
        {
            player2FSM.parameters.playerAttribute.HP = state.Player2HP[player2HPIndex][1];
            player2HPIndex++;
        }

        if (state.Player1LocalScaleX.Count > 0 && player1LocalScaleXIndex < state.Player1LocalScaleX.Count && state.Player1LocalScaleX[player1LocalScaleXIndex][0] == index)
        {
            player1FSM.transform.localScale = new Vector3(state.Player1LocalScaleX[player1LocalScaleXIndex][1], player1FSM.transform.localScale.y, player1FSM.transform.localScale.z);
            player1LocalScaleXIndex++;

        }

        if (state.Player2LocalScaleX.Count > 0 && player2LocalScaleXIndex < state.Player2LocalScaleX.Count && state.Player2LocalScaleX[player2LocalScaleXIndex][0] == index)
        {
            player2FSM.transform.localScale = new Vector3(state.Player2LocalScaleX[player2LocalScaleXIndex][1], player2FSM.transform.localScale.y, player2FSM.transform.localScale.z);
            player2LocalScaleXIndex++;
        }
    }

    public void StopReplay()
    {
        runManager.StopAllCoroutines();
    }
}
