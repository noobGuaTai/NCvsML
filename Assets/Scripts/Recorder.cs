using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class Recorder
{
    public int count = 0;
    public List<float[]> player1Pos;
    public List<float[]> player2Pos;
    public List<float[]> player1Bullet1Pos;
    public List<float[]> player1Bullet2Pos;
    public List<float[]> player2Bullet1Pos;
    public List<float[]> player2Bullet2Pos;
    public List<int[]> player1HP;
    public List<int[]> player2HP;
    public List<int[]> player1LocalScaleX;
    public List<int[]> player2LocalScaleX;

    private int previousPlayer1HP = 10;
    private int previousPlayer2HP = 10;
    private int previousPlayer1LocalScaleX = 1;
    private int previousPlayer2LocalScaleX = -1;

    public Recorder() { player1Pos = new List<float[]>(); player2Pos = new List<float[]>(); player1Bullet1Pos = new List<float[]>(); player1Bullet2Pos = new List<float[]>(); player2Bullet1Pos = new List<float[]>(); player2Bullet2Pos = new List<float[]>(); player1HP = new List<int[]>(); player2HP = new List<int[]>(); player1LocalScaleX = new List<int[]>(); player2LocalScaleX = new List<int[]>(); }

    public void AddState(PlayerFSM player1, PlayerFSM player2)
    {
        count++;
        // Pos
        player1Pos.Add(player1.transform.position.ToVector2().ToFloatArray());
        player2Pos.Add(player2.transform.position.ToVector2().ToFloatArray());
        player1Bullet1Pos.Add((player1.parameters.bullets[0] == null ? Vector3.zero : player1.parameters.bullets[0].transform.position).ToVector2().ToFloatArray());
        player1Bullet2Pos.Add((player1.parameters.bullets[1] == null ? Vector3.zero : player1.parameters.bullets[1].transform.position).ToVector2().ToFloatArray());
        player2Bullet1Pos.Add((player2.parameters.bullets[0] == null ? Vector3.zero : player2.parameters.bullets[0].transform.position).ToVector2().ToFloatArray());
        player2Bullet2Pos.Add((player2.parameters.bullets[1] == null ? Vector3.zero : player2.parameters.bullets[1].transform.position).ToVector2().ToFloatArray());
        // HP
        int currentPlayer1HP = player1.parameters.playerAttribute.HP;
        int currentPlayer2HP = player2.parameters.playerAttribute.HP;
        if (currentPlayer1HP != previousPlayer1HP)
        {
            player1HP.Add(new int[] { count, currentPlayer1HP });
            previousPlayer1HP = currentPlayer1HP;
        }

        if (currentPlayer2HP != previousPlayer2HP)
        {
            player2HP.Add(new int[] { count, currentPlayer2HP });
            previousPlayer2HP = currentPlayer2HP;
        }
        // Local Scale X
        int currentPlayer1LocalScaleX = player1.transform.localScale.x > 0 ? 1 : -1;
        int currentPlayer2LocalScaleX = player2.transform.localScale.x > 0 ? 1 : -1;
        if (currentPlayer1LocalScaleX != previousPlayer1LocalScaleX)
        {
            player1LocalScaleX.Add(new int[] { count, currentPlayer1LocalScaleX });
            previousPlayer1LocalScaleX = currentPlayer1LocalScaleX;
        }
        if (currentPlayer2LocalScaleX != previousPlayer2LocalScaleX)
        {
            player2LocalScaleX.Add(new int[] { count, currentPlayer2LocalScaleX });
            previousPlayer2LocalScaleX = currentPlayer2LocalScaleX;
        }
    }

    public void ClearList()
    {
        count = 0;
        player1Pos.Clear();
        player2Pos.Clear();
        player1Bullet1Pos.Clear();
        player1Bullet2Pos.Clear();
        player2Bullet1Pos.Clear();
        player2Bullet2Pos.Clear();
        player1HP.Clear();
        player2HP.Clear();
        player1LocalScaleX.Clear();
        player2LocalScaleX.Clear();
        previousPlayer1HP = 10;
        previousPlayer2HP = 10;
        previousPlayer1LocalScaleX = 1;
        previousPlayer2LocalScaleX = -1;
    }
}

public static class Vector3Extensions
{
    public static float[] ToFloatArray(this Vector2 vector)
    {
        return new float[] { vector.x, vector.y };
    }

    public static Vector2 ToVector2(this Vector3 vector)
    {
        return new Vector2(vector.x, vector.y);
    }
}