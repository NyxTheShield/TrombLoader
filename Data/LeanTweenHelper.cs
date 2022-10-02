using System;
using UnityEngine;

namespace TrombLoader.Data;

public class LeanTweenHelper:MonoBehaviour
{
    public LeanTweenHelperType tweenType;
    public Vector3 targetValue;
    public float time;
    public LeanTweenType easeType = LeanTweenType.linear;
    public LeanTweenType loopType = LeanTweenType.clamp;
    public int loopCount = 0;
    public bool runOnStart = false;
    public LeanTweenHelper invokeOnComplete;
    
    
    private Action tweenAction;
    
    public void Start()
    {
        switch (tweenType)
        {
            case LeanTweenHelperType.MOVELOCAL:
                tweenAction = () => LeanTween.moveLocal(this.gameObject, targetValue, time).setEase(easeType).setLoopType(loopType).setOnComplete(() => invokeOnComplete.DoTween());
                break;
            case LeanTweenHelperType.MOVEGLOBAL:
                tweenAction = () => LeanTween.move(this.gameObject, targetValue, time).setEase(easeType).setLoopType(loopType).setOnComplete(() => invokeOnComplete.DoTween());
                break;
            case LeanTweenHelperType.ROTATELOCAL:
                tweenAction = () => LeanTween.rotateLocal(this.gameObject, targetValue, time).setEase(easeType).setLoopType(loopType).setOnComplete(() => invokeOnComplete.DoTween());
                break;
            case LeanTweenHelperType.ROTATEGLOBAL:
                tweenAction = () => LeanTween.rotate(this.gameObject, targetValue, time).setEase(easeType).setLoopType(loopType).setOnComplete(() => invokeOnComplete.DoTween());
                break;
            case LeanTweenHelperType.SCALE:
                tweenAction = () => LeanTween.scale(this.gameObject, targetValue, time).setEase(easeType).setLoopType(loopType).setOnComplete(() => invokeOnComplete.DoTween());
                break;
        }
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
        SCALE =4
    }
}

