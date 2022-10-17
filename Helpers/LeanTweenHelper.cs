using System;
using UnityEngine;

namespace TrombLoader.Data;

[Serializable]
public class LeanTweenHelper:MonoBehaviour
{
    public LeanTweenHelperType tweenType;
    public Vector3 vector3Value;
    public float floatValue;
    public float time;
    public LeanTweenType easeType = LeanTweenType.linear;
    public LeanTweenType loopType = LeanTweenType.clamp;
    public int loopCount = 0;
    public bool runOnStart = false;
    public LeanTweenHelper invokeOnComplete;
    
    public Action tweenAction;


    public void SetTweenType()
    {
        switch (tweenType)
        {
            case LeanTweenHelperType.MOVELOCAL:
                tweenAction = () => LeanTween.moveLocal(this.gameObject, vector3Value, time).setEase(easeType).setLoopCount(loopCount).setLoopType(loopType).setOnComplete(() => invokeOnComplete?.DoTween());
                break;
            case LeanTweenHelperType.MOVEGLOBAL:
                tweenAction = () => LeanTween.move(this.gameObject, vector3Value, time).setEase(easeType).setLoopCount(loopCount).setLoopType(loopType).setOnComplete(() => invokeOnComplete?.DoTween());
                break;
            case LeanTweenHelperType.ROTATELOCAL:
                tweenAction = () => LeanTween.rotateLocal(this.gameObject, vector3Value, time).setEase(easeType).setLoopCount(loopCount).setLoopType(loopType).setOnComplete(() => invokeOnComplete?.DoTween());
                break;
            case LeanTweenHelperType.ROTATEGLOBAL:
                tweenAction = () => LeanTween.rotate(this.gameObject, vector3Value, time).setEase(easeType).setLoopCount(loopCount).setLoopType(loopType).setOnComplete(() => invokeOnComplete?.DoTween());
                break;
            case LeanTweenHelperType.SCALE:
                tweenAction = () => LeanTween.scale(this.gameObject, vector3Value, time).setEase(easeType).setLoopCount(loopCount).setLoopType(loopType).setOnComplete(() => invokeOnComplete?.DoTween());
                break;
            case LeanTweenHelperType.MOVEX:
                tweenAction = () => LeanTween.moveX(this.gameObject, floatValue, time).setEase(easeType).setLoopCount(loopCount).setLoopType(loopType).setOnComplete(() => invokeOnComplete?.DoTween());
                break;
            case LeanTweenHelperType.MOVEY:
                tweenAction = () => LeanTween.moveY(this.gameObject, floatValue, time).setEase(easeType).setLoopCount(loopCount).setLoopType(loopType).setOnComplete(() => invokeOnComplete?.DoTween());
                break;
            case LeanTweenHelperType.MOVEZ:
                tweenAction = () => LeanTween.moveZ(this.gameObject, floatValue, time).setEase(easeType).setLoopCount(loopCount).setLoopType(loopType).setOnComplete(() => invokeOnComplete?.DoTween());
                break;
            case LeanTweenHelperType.MOVELOCALX:
                tweenAction = () => LeanTween.moveLocalX(this.gameObject, floatValue, time).setEase(easeType).setLoopCount(loopCount).setLoopType(loopType).setOnComplete(() => invokeOnComplete?.DoTween());
                break;
            case LeanTweenHelperType.MOVELOCALY:
                tweenAction = () => LeanTween.moveLocalY(this.gameObject, floatValue, time).setEase(easeType).setLoopCount(loopCount).setLoopType(loopType).setOnComplete(() => invokeOnComplete?.DoTween());
                break;
            case LeanTweenHelperType.MOVELOCALZ:
                tweenAction = () => LeanTween.moveLocalZ(this.gameObject, floatValue, time).setEase(easeType).setLoopCount(loopCount).setLoopType(loopType).setOnComplete(() => invokeOnComplete?.DoTween());
                break;
        }
    }

    public void Start()
    {
        SetTweenType();
        if (runOnStart) DoTween();
    }

    public void DoTween()
    {
        tweenAction.Invoke();
    }


    public enum LeanTweenHelperType
    {
        MOVELOCAL=0,
        MOVEGLOBAL=1,
        ROTATELOCAL=2,
        ROTATEGLOBAL=3,
        MOVEX=4,
        MOVEY=5,
        MOVEZ=6,
        MOVELOCALX=7,
        MOVELOCALY=8,
        MOVELOCALZ=9,
        SCALE=10
    }
}

