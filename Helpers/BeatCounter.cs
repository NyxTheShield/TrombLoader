using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TrombLoader.Helpers;

public class BeatCounter:MonoBehaviour
{
    public List<int> targetBeats;
    public int currentBeat;
    public UnityEvent actionToExecute;

    public void IncrementBeat()
    {
        currentBeat += 1;
        foreach (int targetBeat in targetBeats)
        {
            if (currentBeat == targetBeat)
            {
                actionToExecute?.Invoke();
            }
        }
    }

    [ContextMenu("Parse String")]
    public void ParseFromString(){
        string toParse = GUIUtility.systemCopyBuffer;
        targetBeats = new List<int>();
        foreach (string number in toParse.Replace("[", "").Replace("]", "").Split(','))
        {
            targetBeats.Add(int.Parse(number));
        }
    }   
}