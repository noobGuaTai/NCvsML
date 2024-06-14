using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System.Linq;
using System;

public class Infer : MonoBehaviour
{
    // 存储CSV文件数据的列表
    private List<float> geneData = new List<float>();
    private int[] dims_list;
    (List<float[,]>, List<float[,]>) decoded;
    private float[] info;
    public GameObject player1;
    public GameObject player2;

    private PlayerMove p1m;
    private PlayerMove p2m;
    private PlayerAttribute p1a;
    private PlayerAttribute p2a;


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
        p1m = player1.GetComponent<PlayerMove>();
        p1a = player1.GetComponent<PlayerAttribute>();
        p2m = player2.GetComponent<PlayerMove>();
        p2a = player2.GetComponent<PlayerAttribute>();
        p1a.isInvincible = false;
        p2a.isInvincible = false;

        if (!p1m.isControl)
        {
            geneData = ReadCSV(filePath);
            decoded = Decode(geneData.ToArray(), dims_list);
        }
    }

    public void OnUpdate()
    {
        if (!p1m.isControl && geneData != null)
        {
            GetEnvInf(p1m, p2m, p1a, p2a, ref info);
            int[] output1 = Forward(info, decoded.Item1, decoded.Item2);
            p1m.RunAction(output1);
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

    int[] Forward(float[] x, List<float[,]> weightList, List<float[,]> biasList)
    {
        for (int i = 0; i < weightList.Count; i++)
        {
            x = Sigmoid(MatrixMultiply(weightList[i], x).Select((val, idx) => val + biasList[i][idx, 0]).ToArray());
        }
        int[] action = new int[x.Length];
        action[0] = x[0] > 0.5f ? 1 : 0;
        action[1] = x[1] > 0.5f ? 1 : 0;
        action[2] = (x[2] > 0.6f) ? 1 : ((x[2] < 0.4f) ? -1 : 0);

        return action;
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

    public void GetEnvInf(PlayerMove pm1, PlayerMove pm2, PlayerAttribute pa1, PlayerAttribute pa2, ref float[] info)
    {
        info[0] = pm1.transform.localScale.x;
        info[1] = 2 - pm1.bullets.Count; // 有改动
        info[2] = pm1.canJump ? 1 : 0;
        info[3] = pm1.transform.position.x - pm1.leftWall.transform.position.x;
        info[4] = pm1.rightWall.transform.position.x - pm1.transform.position.x;
        info[5] = pm2.transform.position.x - pm1.transform.position.x;
        info[6] = pm2.transform.position.y - pm1.transform.position.y;
        info[7] = pm2.bullets.Count > 0 ? 1 : 0;

        if (info[7] != 0 && pm2.bullets[0] != null)
        {
            info[8] = pm2.bullets[0].transform.position.x;
            info[9] = pm2.bullets[0].transform.position.y;
        }
        else
        {
            info[8] = 0;
            info[9] = 0;
        }

        info[10] = pm2.bullets.Count > 1 ? 1 : 0;
        if (info[10] != 0 && pm2.bullets[1] != null)
        {
            info[11] = pm2.bullets[1].transform.position.x;
            info[12] = pm2.bullets[1].transform.position.y;
        }
        else
        {
            info[11] = 0;
            info[12] = 0;
        }
        info[13] = pa1.isInvincible ? 1 : 0;
        info[14] = pa2.isInvincible ? 1 : 0;
    }


}

