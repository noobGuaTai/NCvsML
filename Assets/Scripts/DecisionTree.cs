using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System.Linq;
using System;
using PlayerEnum;
using Unity.Mathematics;

public class DecisionTree : MonoBehaviour
{
    public float[] info;
    public GameObject self;
    public GameObject enemy;

    private PlayerFSM selfFSM;
    private PlayerFSM enemyFSM;
    private PlayerAttribute selfAttribute;
    private PlayerAttribute enemyAttribute;
    private InferManager inferManager;
    public PlayerActionType[] actionArray;



    public DecisionTree(GameObject self, GameObject enemy, InferManager inferManager)
    {
        this.self = self;
        this.enemy = enemy;
        this.inferManager = inferManager;
        info = new float[15];

        selfFSM = self.GetComponent<PlayerFSM>();
        selfAttribute = self.GetComponent<PlayerAttribute>();
        enemyFSM = enemy.GetComponent<PlayerFSM>();
        enemyAttribute = enemy.GetComponent<PlayerAttribute>();
        selfAttribute.isInvincible = false;
        enemyAttribute.isInvincible = false;
        actionArray = new PlayerActionType[3];
    }

    public DecisionTree(GameObject self, GameObject enemy)
    {
        this.self = self;
        this.enemy = enemy;
        info = new float[15];

        selfFSM = self.GetComponent<PlayerFSM>();
        selfAttribute = self.GetComponent<PlayerAttribute>();
        enemyFSM = enemy.GetComponent<PlayerFSM>();
        enemyAttribute = enemy.GetComponent<PlayerAttribute>();
        selfAttribute.isInvincible = false;
        enemyAttribute.isInvincible = false;
        actionArray = new PlayerActionType[3];
    }

    public void OnUpdate()
    {
        actionArray[0] = PlayerActionType.None;
        if ((math.abs(info[8]) < 1f && math.abs(info[9]) < 0.6f) || (math.abs(info[11]) < 1f && math.abs(info[12]) < 0.6f))
        {
            actionArray[0] = PlayerActionType.Jump;
        }
        if(info[8] == 0 && info[11] == 0)
        {
            actionArray[0] = PlayerActionType.None;
        }
        if (math.abs(info[6]) > 0.6f)
        {
            actionArray[0] = PlayerActionType.Jump;
        }
        actionArray[1] = PlayerActionType.None;
        if(math.abs(info[6]) < 1f)
        {
            actionArray[1] = PlayerActionType.Shoot;
        }
        actionArray[2] = PlayerActionType.None;
        if (info[5] > 3f)
        {
            actionArray[2] = PlayerActionType.MoveRight;
        }
        if (info[5] < -3f)
        {
            actionArray[2] = PlayerActionType.MoveLeft;
        }
        if(info[0] != math.sign(info[5]))
        {
            actionArray[2] = info[5] > 0 ? PlayerActionType.MoveRight : PlayerActionType.MoveLeft;
        }
        selfFSM.parameters.playerAction = actionArray;
    }

}

