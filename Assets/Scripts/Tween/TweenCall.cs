using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using static Tween;

namespace Assets
{
    struct TweenCall
    {
        public static void Call(TweenNode<float> tweenNode, float alpha) {
            var cntT = (tweenNode.end - tweenNode.start) * alpha + tweenNode.start;
            tweenNode.setter(cntT);
        }
        public static void Call(TweenNode<UnityEngine.Vector2> tweenNode, float alpha)
        {
            var cntT = (tweenNode.end - tweenNode.start) * alpha + tweenNode.start;
            tweenNode.setter(cntT);
        }
        public static void Call(TweenNode<UnityEngine.Vector3> tweenNode, float alpha)
        {
            var cntT = (tweenNode.end - tweenNode.start) * alpha + tweenNode.start;
            tweenNode.setter(cntT);
        }
        public static void Call(TweenNode<UnityEngine.Vector2Int> tweenNode, float alpha)
        {
            var cntT = ((UnityEngine.Vector2)tweenNode.end - tweenNode.start) * alpha + tweenNode.start;
            tweenNode.setter(new Vector2Int((int)cntT.x, (int)cntT.y));
        }
        public static void Call(TweenNode<UnityEngine.Vector3Int> tweenNode, float alpha)
        {
            var cntT = ((UnityEngine.Vector3)tweenNode.end - tweenNode.start) * alpha + tweenNode.start;
            tweenNode.setter(new Vector3Int((int)cntT.x, (int)cntT.y, (int)cntT.z));
        }
    }
}
