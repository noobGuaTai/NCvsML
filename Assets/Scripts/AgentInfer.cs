using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System.Linq;
using System;
using PlayerEnum;
using Unity.Mathematics;

public class AgentInfer : MonoBehaviour
{
    // 存储CSV文件数据的列表
    private List<float> geneData = new List<float>();
    private int[] dims_list;
    (List<float[,]>, List<float[,]>) decoded;
    public float[] info;
    public GameObject self;
    public GameObject enemy;
    public RunManager runManager;

    private PlayerFSM selfFSM;
    private PlayerFSM enemyFSM;
    private PlayerAttribute selfAttribute;
    private PlayerAttribute enemyAttribute;


    // CSV文件的路径
    public string filePath = "/gene.csv";
    public string path = Application.streamingAssetsPath;
    public bool ready = false;


    public AgentInfer(GameObject self, GameObject enemy, string filePath, RunManager runManager)
    {
        this.self = self;
        this.enemy = enemy;
        this.filePath = path + filePath;
        this.runManager = runManager;
        selfFSM = self.GetComponent<PlayerFSM>();
        selfAttribute = self.GetComponent<PlayerAttribute>();
        enemyFSM = enemy.GetComponent<PlayerFSM>();
        enemyAttribute = enemy.GetComponent<PlayerAttribute>();
        selfAttribute.isInvincible = false;
        enemyAttribute.isInvincible = false;
        info = new float[15];
        geneData = ReadCSV(this.filePath);
        decoded = Decode(geneData.ToArray(), dims_list);
        runManager.runtime += OnUpdate;
        ready = true;
    }

    public void OnUpdate()
    {
        info = runManager.GetEnvInf(selfFSM, enemyFSM, selfAttribute, enemyAttribute);
        if (ready && !selfFSM.parameters.isControl)
        {
            PlayerActionType[] output1 = Forward(info, decoded.Item1, decoded.Item2);
            selfFSM.parameters.playerAction = output1;
        }
    }

    List<float> ReadCSV(string path)
    {
        List<float> geneData = new List<float>();
        dims_list = null;

        if (File.Exists(path))
        {
            string[] lines = File.ReadAllLines(path, Encoding.UTF8);
            if (lines.Length > 0)
            {
                // 使用空数据（连续两个逗号）作为分界符
                string[] parts = lines[0].Split(new string[] { ",," }, StringSplitOptions.None);
                if (parts.Length == 2)
                {
                    // 处理空数据左侧的部分
                    string[] values = parts[0].Split(',');
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

                    // 处理空数据右侧的部分
                    string[] dimsStrings = parts[1].Split(',');
                    dims_list = new int[dimsStrings.Length];
                    for (int i = 0; i < dimsStrings.Length; i++)
                    {
                        if (int.TryParse(dimsStrings[i], out int dimValue))
                        {
                            dims_list[i] = dimValue;
                        }
                        else
                        {
                            Debug.LogError("Invalid integer value for dims in CSV: " + dimsStrings[i]);
                        }
                    }
                }
                else
                {
                    Debug.LogError("CSV format error: Missing or incorrect ',,' delimiter");
                }
            }
            else
            {
                Debug.LogError("CSV file is empty");
            }
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
        actionArray[2] = intArray[2] == 1 ? PlayerActionType.MoveRight : intArray[2] == -1 ? PlayerActionType.MoveLeft : PlayerActionType.None;

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



}

