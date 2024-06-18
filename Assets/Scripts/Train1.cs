using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using PlayerEnum;

public class Train1
{
    public Train1Manager train1Manager;
    public Socket mainListener;
    public Socket accListener;
    public bool recvFlag = false;
    public bool sendFlag = false;
    public int player;

    private PlayerActionType[] action;
    public EnvInfo info;// 环境信息
    public bool canRestart = false;
    public Socket RAShandler;
    private List<Thread> threads;
    public bool isRunning = true;


    public Train1(Train1Manager trainManager, int player)
    {
        train1Manager = trainManager;
        this.player = player;
        threads = new List<Thread>();
    }

    public void Start(int port)
    {
        info = new EnvInfo();
        mainListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        CreateConnection("127.0.0.1", port, mainListener);
    }

    void CreateConnection(string ip, int port, Socket listener)
    {
        IPAddress ipAddress = IPAddress.Parse(ip);
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);
        listener.Bind(localEndPoint);
        Debug.Log("Waiting for a connection...");
        listener.Listen(10);

        Thread acceptThread = new Thread(AcceptConnection);
        acceptThread.Start(listener);
        threads.Add(acceptThread);
    }

    void AcceptConnection(object obj)
    {
        accListener = (Socket)obj;
        while (isRunning)
        {
            try
            {
                Socket accHandler = accListener.Accept();
                Debug.Log("connected");
                // train2Instance.timeSpeed = ;
                train1Manager.isStartTrain = true;
                UnityMainThreadDispatcher.RunOnMainThread(() =>
                {
                    train1Manager.iterationStartTime = Time.time;
                    Time.timeScale = train1Manager.timeSpeed;
                    train1Manager.UI.GetComponent<UI>().waitingConnect.SetActive(false);
                });
                SendMessage(accHandler, info);
                Debug.Log("send init envInfo");

                Thread sendNrecvThread = new Thread(RecvAndSend);
                sendFlag = false;
                sendNrecvThread.Start(accHandler);
                threads.Add(sendNrecvThread);
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode == SocketError.Interrupted)
                {
                    Debug.Log("Socket accept interrupted by close.");
                    break;
                }
                Debug.Log("SocketException: " + ex.ToString());
            }
        }
    }

    public void RecvAndSend(object obj)
    {
        RAShandler = (Socket)obj;
        // action = recvInt(RAShandler);
        while (isRunning)
        {
            try
            {
                while (!recvFlag) ;
                // if (!isRunning) break;

                action = recvAction(RAShandler);

                train1Manager.RunAction(action);// 接受到信息，就执行操作
                // Debug.Log(player + "received.");
                // Debug.Log("action[0]: " + action[0] + " action[1]: " + action[1] + " action[2]: " + action[2]);
                recvFlag = false;

                while (!sendFlag) ;// 操作执行完后
                if (canRestart)
                {
                    if (train1Manager.isEnd)
                    {
                        Debug.Log("Ground" + (train1Manager.iteration - 1) + "End");
                        // info.infoCode = (train2Instance.p1a.HP >= train2Instance.p2a.HP) && (player == 1) ? 1f : 2f;

                        info.infoCode = train1Manager.player1HP > train1Manager.player2HP ? 2f : -2f;
                        info.infoCode = train1Manager.player1HP == train1Manager.player2HP ? -2f : info.infoCode;
                        // Debug.Log("player:" + player + "train2Instance.p1a.HP:" + train2Instance.player1HP + "train2Instance.p2a.HP:" + train2Instance.player2HP + "info.infoCode:" + info.infoCode);
                        SendMessage(RAShandler, info);
                        canRestart = false;
                        // Debug.Log("send end:"+info.infoCode);
                        // hasSendEndInfo = true;
                    }
                    else
                    {
                        info.infoCode = train1Manager.player1FSM.parameters.beShot == true ? -1f : 0f;// -1被击中
                        info.infoCode = train1Manager.player1FSM.parameters.isShot == true ? 1f : info.infoCode;// 1击中对方
                        // info.infoCode = train2Instance.p1m.isShot == true && train2Instance.p1m.beShot == true ? 5f : info.infoCode;
                        // if (info.infoCode == 3 || info.infoCode == 4)
                        // {
                        //     Debug.Log("player:" + player + " infoCode:" + info.infoCode);
                        // }
                        SendMessage(RAShandler, info);
                        // Debug.Log("send normal");
                        info.infoCode = 0f;
                        train1Manager.player1FSM.parameters.beShot = false;
                        train1Manager.player1FSM.parameters.isShot = false;
                    }
                }


                sendFlag = false;
                // Debug.Log("send info" + player.ToString());


            }
            catch (SocketException)
            {
                break;
            }
        }
        if (RAShandler != null)
        {
            RAShandler.Shutdown(SocketShutdown.Both);
            RAShandler.Close();
        }
    }

    PlayerActionType[] recvAction(Socket handler)
    {
        int len = 3;
        int[] intArray = new int[len];
        byte[] buffer = new byte[len * 4]; // 每个整数4个字节
        int bytesRec = handler.Receive(buffer);
        if (bytesRec != len * 4)
        {
            throw new Exception("Received data length mismatch.");
        }

        // 将字节转换为整数
        Buffer.BlockCopy(buffer, 0, intArray, 0, bytesRec);

        PlayerActionType[] actionArray = new PlayerActionType[len];
        actionArray[0] = intArray[0] == 2 ? PlayerActionType.StartNextGround : intArray[0] == 1 ? PlayerActionType.Jump : PlayerActionType.None;
        actionArray[1] = intArray[1] == 1 ? PlayerActionType.Shoot : PlayerActionType.None;
        actionArray[2] = intArray[0] == 1 ? PlayerActionType.MoveRight : intArray[0] == -1 ? PlayerActionType.MoveLeft : PlayerActionType.None;

        return actionArray;
    }

    public void SendMessage(Socket handler, EnvInfo info)
    {
        List<byte> byteStream = new List<byte>();

        byteStream.AddRange(BitConverter.GetBytes(info.direction));
        byteStream.AddRange(BitConverter.GetBytes(info.shootable));
        byteStream.AddRange(BitConverter.GetBytes(info.jumpable));
        byteStream.AddRange(BitConverter.GetBytes(info.leftWall_XD));
        byteStream.AddRange(BitConverter.GetBytes(info.rightWall_XD));
        byteStream.AddRange(BitConverter.GetBytes(info.E_XD));
        byteStream.AddRange(BitConverter.GetBytes(info.E_YD));

        byteStream.AddRange(BitConverter.GetBytes(info.E_Bullet0));
        if (info.E_Bullet0 > 0)
        {
            byteStream.AddRange(BitConverter.GetBytes(info.E_Bullet0_XD));
            byteStream.AddRange(BitConverter.GetBytes(info.E_Bullet0_YD));
        }
        else
        {
            byteStream.AddRange(BitConverter.GetBytes(0));
            byteStream.AddRange(BitConverter.GetBytes(0));
        }

        byteStream.AddRange(BitConverter.GetBytes(info.E_Bullet1));
        if (info.E_Bullet1 > 0)
        {
            byteStream.AddRange(BitConverter.GetBytes(info.E_Bullet1_XD));
            byteStream.AddRange(BitConverter.GetBytes(info.E_Bullet1_YD));
        }
        else
        {
            byteStream.AddRange(BitConverter.GetBytes(0));
            byteStream.AddRange(BitConverter.GetBytes(0));
        }
        byteStream.AddRange(BitConverter.GetBytes(info.self_Invincible));
        byteStream.AddRange(BitConverter.GetBytes(info.E_Invincible));
        byteStream.AddRange(BitConverter.GetBytes(info.infoCode));
        byteStream.AddRange(BitConverter.GetBytes(info.time));

        handler.Send(byteStream.ToArray());
    }

    public void SetEnvInfo(EnvInfo info)
    {
        this.info = info;
    }

    public void SetRecvFlag(bool flag)
    {
        this.recvFlag = flag;
    }

    public void SetSendFlag(bool flag)
    {
        this.sendFlag = flag;
    }

    public void Shutdown()
    {
        // foreach (var thread in threads)
        // {
        //     if (thread.IsAlive)
        //     {
        //         thread.Join();
        //     }
        // }
        // threads.Clear();

        if (mainListener != null)
        {
            try
            {
                mainListener.Close();
                mainListener = null;
                Debug.Log("Listener socket closed.");
            }
            catch (SocketException ex)
            {
                Debug.Log("SocketException during listener close: " + ex.ToString());
            }
        }

        if (accListener != null)
        {
            try
            {
                accListener.Close();
                accListener = null;
                Debug.Log("Listener socket closed.");
            }
            catch (SocketException ex)
            {
                Debug.Log("SocketException during listener close: " + ex.ToString());
            }
        }
    }


}
