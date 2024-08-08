using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
public struct EnvInfo// TODO：转成枚举类型
{
    public float Face_E;
    public float leftWall_XD;
    public float rightWall_XD;
    public float E_XD;// 相对距离
    public float E_YD;
    public float self_Bullet0_XD;
    public float self_Bullet0_YD;
    public float self_Bullet1_XD;
    public float self_Bullet1_YD;
    public float E_Bullet0_XD;
    public float E_Bullet0_YD;
    public float E_Bullet1_XD;
    public float E_Bullet1_YD;
    public float infoCode;// 0为正常，1为胜利，2为失败，3为击中对方，4为被击中，5为双方同时击中，
    public float time;
}
