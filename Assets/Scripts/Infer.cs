using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System.Linq;
using System;
using PlayerEnum;

public class Infer : MonoBehaviour
{
    // 存储CSV文件数据的列表
    private List<float> geneData = new List<float>();
    private int[] dims_list;
    (List<float[,]>, List<float[,]>) decoded;
    private float[] info;
    public GameObject player1;
    public GameObject player2;

    private PlayerFSM player1FSM;
    private PlayerFSM player2FSM;
    private PlayerAttribute player1Attribute;
    private PlayerAttribute player2Attribute;


    // CSV文件的路径
    public string filePath = "/gene.csv";
    public string path = Application.streamingAssetsPath;


    public Infer(GameObject self, GameObject enemy, string filePath)
    {
        player1 = self;
        player2 = enemy;
        this.filePath = path + filePath;
    }

    public void StartInfer()
    {
        player1FSM = player1.GetComponent<PlayerFSM>();
        player1Attribute = player1.GetComponent<PlayerAttribute>();
        player2FSM = player2.GetComponent<PlayerFSM>();
        player2Attribute = player2.GetComponent<PlayerAttribute>();
        player1Attribute.isInvincible = false;
        player2Attribute.isInvincible = false;

        if (!player1FSM.parameters.isControl)
        {
            geneData = ReadCSV(filePath);
            decoded = Decode(geneData.ToArray(), dims_list);
        }
    }

    public void OnUpdate()
    {
        if (!player1FSM.parameters.isControl && geneData != null)
        {
            GetEnvInf(player1FSM, player2FSM, player1Attribute, player2Attribute, ref info);
            PlayerActionType[] output1 = Forward(info, decoded.Item1, decoded.Item2);
            player1FSM.parameters.playerAction = output1;
        }
    }

    List<float> ReadCSV(string path)
    {
        List<float> geneData = new List<float>();
        if (File.Exists(path))
        {
            string[] lines = File.ReadAllLines(path, Encoding.UTF8);
            string[] values = lines[0].Split(',');
            foreach (string value in values)
            {
                if (float.TryParse(value, out float floatValue))
                {
                    geneData.Add(floatValue);
                }
                else
                {
                    Debug.LogError("Invalid float value in CSV: " + value);
                }
            }

            string[] dimsStrings = values.Skip(values.Length - 3).ToArray();
            dims_list = new int[dimsStrings.Length];
            for (int i = 0; i < dimsStrings.Length; i++)
            {
                dims_list[i] = (int)float.Parse(dimsStrings[i]);
            }

            info = new float[dims_list[dims_list.Length - 2]];
        }
        else
        {
            Debug.LogError("CSV file not found at path: " + path);
        }
        return geneData;
    }

    (List<float[,]>, List<float[,]>) Decode(float[] gene, int[] dimsList)
    {
        List<float[,]> weightList = new List<float[,]>();
        List<float[,]> biasList = new List<float[,]>();

        int weightPos = 0;
        int biasPos = dimsList.Take(dimsList.Length - 1).Select((dim, i) => dim * dimsList[i + 1]).Sum();

        for (int i = 0; i < dimsList.Length - 1; i++)
        {
            int weightSize = dimsList[i] * dimsList[i + 1];
            float[,] weights = new float[dimsList[i + 1], dimsList[i]];
            for (int j = 0; j < dimsList[i + 1]; j++)
            {
                for (int k = 0; k < dimsList[i]; k++)
                {
                    weights[j, k] = gene[weightPos++];
                }
            }
            weightList.Add(weights);

            float[,] biases = new float[dimsList[i + 1], 1];
            for (int j = 0; j < dimsList[i + 1]; j++)
            {
                biases[j, 0] = gene[biasPos++];
            }
            biasList.Add(biases);
        }

        return (weightList, biasList);
    }

    PlayerActionType[] Forward(float[] x, List<float[,]> weightList, List<float[,]> biasList)
    {
        for (int i = 0; i < weightList.Count; i++)
        {
            x = Sigmoid(MatrixMultiply(weightList[i], x).Select((val, idx) => val + biasList[i][idx, 0]).ToArray());
        }
        int[] intArray = new int[x.Length];
        intArray[0] = x[0] > 0.5f ? 1 : 0;
        intArray[1] = x[1] > 0.5f ? 1 : 0;
        intArray[2] = (x[2] > 0.6f) ? 1 : ((x[2] < 0.4f) ? -1 : 0);

        PlayerActionType[] actionArray = new PlayerActionType[x.Length];
        actionArray[0] = intArray[0] == 2 ? PlayerActionType.StartNextGround : intArray[0] == 1 ? PlayerActionType.Jump : PlayerActionType.None;
        actionArray[1] = intArray[1] == 1 ? PlayerActionType.Shoot : PlayerActionType.None;
        actionArray[2] = intArray[0] == 1 ? PlayerActionType.MoveRight : intArray[0] == -1 ? PlayerActionType.MoveLeft : PlayerActionType.None;

        return actionArray;
    }

    float[] MatrixMultiply(float[,] matrix, float[] vector)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);
        float[] result = new float[rows];

        for (int i = 0; i < rows; i++)
        {
            result[i] = 0;
            for (int j = 0; j < cols; j++)
            {
                result[i] += matrix[i, j] * vector[j];
            }
        }

        return result;
    }

    float[] Sigmoid(float[] x)
    {
        return x.Select(val => 1 / (1 + Mathf.Exp(-val))).ToArray();
    }

    public void GetEnvInf(PlayerFSM playerFSM1, PlayerFSM playerFSM2, PlayerAttribute playerAttribute1, PlayerAttribute playerAttribute2, ref float[] info)
    {
        info[0] = playerFSM1.transform.localScale.x;
        info[1] = 2 - playerFSM1.parameters.bullets.Count; // 有改动
        info[2] = playerFSM1.parameters.canJump ? 1 : 0;
        info[3] = playerFSM1.transform.position.x - playerFSM1.parameters.leftWall.transform.position.x;
        info[4] = playerFSM1.parameters.rightWall.transform.position.x - playerFSM1.transform.position.x;
        info[5] = playerFSM2.transform.position.x - playerFSM1.transform.position.x;
        info[6] = playerFSM2.transform.position.y - playerFSM1.transform.position.y;
        info[7] = playerFSM2.parameters.bullets.Count > 0 ? 1 : 0;

        if (info[7] != 0 && playerFSM2.parameters.bullets[0] != null)
        {
            info[8] = playerFSM2.parameters.bullets[0].transform.position.x;
            info[9] = playerFSM2.parameters.bullets[0].transform.position.y;
        }
        else
        {
            info[8] = 0;
            info[9] = 0;
        }

        info[10] = playerFSM2.parameters.bullets.Count > 1 ? 1 : 0;
        if (info[10] != 0 && playerFSM2.parameters.bullets[1] != null)
        {
            info[11] = playerFSM2.parameters.bullets[1].transform.position.x;
            info[12] = playerFSM2.parameters.bullets[1].transform.position.y;
        }
        else
        {
            info[11] = 0;
            info[12] = 0;
        }
        info[13] = playerAttribute1.isInvincible ? 1 : 0;
        info[14] = playerAttribute2.isInvincible ? 1 : 0;
    }


}

