using Assets;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;


public class Tween : MonoBehaviour
{
    // Start is called before the first frame update
    void Start() {

    }

    public enum TweenType
    {
        UNKNOWN = -1,
        FLOAT,
        VECTOR2,
        VECTOR3,
        VECTOR2INT,
        VECTOR3INT,
    }

    public enum TweenState
    {
        STOP,
        RUNNING,
        PAUSED,
    }

    public enum PlayMode
    {
        NORMAL,
        REPEAT,
        REVERSE,
    }

    public class TweeNodeBase
    {
        public TweenType type;
        public float time;
        public TransitionType transitionType = TransitionType.LINEAR;
        public EaseType easeType = EaseType.IN;
    }

    public class TweenNode<T> : TweeNodeBase
    {
        public Action<T> setter;
        public T start;
        public T end;


        public TweenNode(TweenType type, Action<T> setter, T start, T end, float time) {
            base.type = type;
            this.setter = setter;
            this.start = start;
            this.end = end;
            base.time = time;

        }

    }

    List<TweeNodeBase> tweenNodeList = new List<TweeNodeBase>();
    int tweenIndex = 0;
    float tweenTime = 0;
    TweenState _tweenState;
    PlayMode _playMode = PlayMode.NORMAL;

    public PlayMode playMode {
        set {
            _playMode = value;
        }
        get {
            return _playMode;
        }
    }

    public bool clearWhenEnd = false;


    public void Clear() {
        tweenNodeList.Clear();
        _tweenState = TweenState.STOP;
    }

    public TweenType GetTweenType<T>() {
        var type = typeof(T);
        if (type == typeof(float))
            return Tween.TweenType.FLOAT;
        else if (type == typeof(Vector2)) return Tween.TweenType.VECTOR2;
        else if (type == typeof(Vector3)) return Tween.TweenType.VECTOR3;
        else if (type == typeof(Vector2Int)) return Tween.TweenType.VECTOR2INT;
        else if (type == typeof(Vector3Int)) return Tween.TweenType.VECTOR3INT;
        else {
            return Tween.TweenType.UNKNOWN;
        }

    }

    public void AddTween<T>(
        Action<T> setter, T start, T end, float time,
        TransitionType transitionType = TransitionType.LINEAR, EaseType easeType = EaseType.IN) {
        if (_tweenState == Tween.TweenState.RUNNING) {
            Debug.LogError("Try to call AddTween while tween is running");
            return;
        }

        TweenType type = GetTweenType<T>();
        if (type == Tween.TweenType.UNKNOWN) {
            Debug.LogError("Try to AddTween with a unspport type");
            return;
        }
        var cTime = 0f;
        if (tweenNodeList.Count > 0)
            cTime = tweenNodeList[tweenNodeList.Count - 1].time;

        var tweenNode = new TweenNode<T>(type, setter, start, end, time + cTime);
        tweenNode.easeType = easeType;
        tweenNode.transitionType = transitionType;
        tweenNodeList.Add(tweenNode);
    }

    public void Play() {
        if(_tweenState == Tween.TweenState.RUNNING)
            return;

        _tweenState = Tween.TweenState.RUNNING;
        
        if (playMode == PlayMode.REVERSE) {
            tweenTime = tweenNodeList.Last().time;
            tweenIndex = tweenNodeList.Count - 1;
        }
        else {
            tweenIndex = 0;
            tweenTime = 0;
        }
    }

    public void Stop() {
        _tweenState = Tween.TweenState.STOP;
        if (playMode == PlayMode.REVERSE) {
            tweenTime = tweenNodeList.Last().time;
            tweenIndex = tweenNodeList.Count - 1;
        }
        else {
            tweenIndex = 0;
            tweenTime = 0;
        }
    }

    public void Pause() {
        _tweenState = Tween.TweenState.PAUSED;
    }

    //void tweenCall<T>(TweenNode<T> tweenNode, float alpha)
    //{
    //    var cntT = (tweenNode.end - tweenNode.start) * alpha + tweenNode.start;
    //    tweenNode.setter(cntT);
    //}

    void TweenProcess(TweeNodeBase tweenNodeBase, float alpha) {
        switch (tweenNodeBase.type) {
            case TweenType.UNKNOWN:
                break;
            case TweenType.FLOAT:
                //tweenCall((TweenNode<float>)tweenNodeBase, alpha);
                //break;

                TweenCall.Call((TweenNode<float>)tweenNodeBase, alpha);
                break;
            case TweenType.VECTOR2:
                TweenCall.Call((TweenNode<Vector2>)tweenNodeBase, alpha);
                break;
            case TweenType.VECTOR3:
                TweenCall.Call((TweenNode<Vector3>)tweenNodeBase, alpha);
                break;
            case TweenType.VECTOR2INT:
                TweenCall.Call((TweenNode<Vector2Int>)tweenNodeBase, alpha);
                break;
            case TweenType.VECTOR3INT:
                TweenCall.Call((TweenNode<Vector3Int>)tweenNodeBase, alpha);
                break;
        }
    }


    // Update is called once per frame
    public void Update() {
        if (_tweenState != TweenState.RUNNING)
            return;

        var delTime = Time.deltaTime;
        if (playMode == PlayMode.REVERSE)
            tweenTime -= delTime;
        else
            tweenTime += delTime;

        Action loopProcess = () =>
        {
            if (playMode == PlayMode.REVERSE) {
                while (tweenIndex >= 0 && tweenTime <= (
                    tweenIndex == 0 ? 0 :
                    tweenNodeList[tweenIndex - 1].time)) {
                    TweenProcess(tweenNodeList[tweenIndex], 0);
                    tweenIndex--;
                }
                return;
            }
            while (tweenIndex < tweenNodeList.Count && tweenTime >= tweenNodeList[tweenIndex].time) {
                TweenProcess(tweenNodeList[tweenIndex], 1);
                tweenIndex++;
            }
        };
        loopProcess();
        Func<bool> endCondition = () => {
            if (playMode == PlayMode.REVERSE) {
                return tweenIndex == -1;
            }
            else {
                return tweenIndex == tweenNodeList.Count;
            }
        };

        if (endCondition()) {
            switch (_playMode) {
                case PlayMode.NORMAL:
                    _tweenState = TweenState.STOP;
                    if (clearWhenEnd)
                        Clear();
                    return;
                case PlayMode.REPEAT:
                    tweenIndex = 0;
                    tweenTime -= tweenNodeList.Last().time;
                    loopProcess();
                    return;
                case PlayMode.REVERSE:
                    _tweenState = TweenState.STOP;
                    if (clearWhenEnd)
                        Clear();
                    return;
            }

        }

        float preTime = 0;
        if (tweenIndex > 0)
            preTime = tweenNodeList[tweenIndex - 1].time;
        float cntAlpha = (tweenTime - preTime) / (tweenNodeList[tweenIndex].time - preTime + 1e-6f);
        var transitionType = tweenNodeList[tweenIndex].transitionType;
        var easeType = tweenNodeList[tweenIndex].easeType;

        cntAlpha = EaseAndTrainsitionProcess(cntAlpha, easeType, transitionType);

        TweenProcess(tweenNodeList[tweenIndex], cntAlpha);
    }

    public enum TransitionType
    {
        LINEAR,
        SIN,
        QUAD,
        CUBIC,
        QUART,
        BACK
    }

    public enum EaseType
    {
        IN,
        OUT,
        IN_OUT,
        OUT_IN,
    }
    public static float TransitionProcess(float alpha, TransitionType type) {
        switch (type) {
            case TransitionType.LINEAR:
                return alpha;
            case TransitionType.SIN:
                return Mathf.Sin(alpha * 0.5f * Mathf.PI);
            case TransitionType.QUAD:
                return alpha * alpha;
            case TransitionType.CUBIC:
                return alpha * alpha * alpha;
            case TransitionType.QUART:
                return alpha * alpha * alpha * alpha;
            case TransitionType.BACK:
                return -0.875f * alpha + 1.875f * alpha * alpha;
        }
        Debug.LogError("Unknow transition!");
        return -1;
    }

    public static float EaseAndTrainsitionProcess(
        float alpha, EaseType easeType, TransitionType transitionType) {
        switch (easeType) {
            case EaseType.IN:
                return TransitionProcess(alpha, transitionType);
            case EaseType.OUT:
                return 1 - TransitionProcess(1 - alpha, transitionType);
            case EaseType.IN_OUT:
                var alphaIn = TransitionProcess(alpha, transitionType);
                var alphaOut = 1 - TransitionProcess(1 - alpha, transitionType);
                return (1 - alpha) * alphaIn + alpha * alphaOut;
            case EaseType.OUT_IN:
                alphaIn = TransitionProcess(alpha, transitionType);
                alphaOut = 1 - TransitionProcess(1 - alpha, transitionType);
                return alpha * alphaIn + (1 - alpha) * alphaOut;
        }
        Debug.LogError("Unknow transition!");
        return -1;
    }
}