using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Events;

namespace TrombLoader.Helpers;

public class BeatCounter:MonoBehaviour
{
    public List<float> targetBeats;
    public float currentBeat;
    public UnityEvent actionToExecute;
    private bool isInitialized = false;
    public float bpm;
    public bool runOnZeroBeat = false;
    public void IncrementBeat()
    {
        if (!isInitialized)
        {
            isInitialized = true;
            if (!runOnZeroBeat) return;
        }
        
        if (targetBeats.Count == 0) return;
        
        float EighthNoteInterval = 0.125f;
        
        //Checks for Beat
        CheckNote(currentBeat);
        //Checks for 1/8, 2/8 ....... 7/8 of a beat
        for (int i = 1; i < 8; i++)
        {
            var newTime = EighthNoteInterval * i;
            var timeInSeconds = newTime * 60 / bpm;
            LeanTween.value(0, 1, timeInSeconds).setOnComplete(() =>
            {
                CheckNote(currentBeat+ newTime);
            });
        }
        currentBeat += 1;
    }
    
    void CheckNote(float target)
    {
        if (targetBeats.Count == 0) return;
        
        if (target >= targetBeats[0])
        {
            actionToExecute?.Invoke();
            targetBeats.RemoveAt(0);
        }
    }


    [ContextMenu("Parse String")]
    public void ParseFromString(){
        string toParse = GUIUtility.systemCopyBuffer;
        targetBeats = new List<float>();
        foreach (string number in toParse.Replace("[", "").Replace("]", "").Split(','))
        {
            targetBeats.Add(float.Parse(number, CultureInfo.InvariantCulture));
        }
    }   
}