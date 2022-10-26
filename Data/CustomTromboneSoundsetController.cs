using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TrombLoader.Data;

public class CustomTromboneSoundsetController:MonoBehaviour
{
    
}

public class CustomSoundsetUI
{
    public EventTrigger eventTrigger;
    public GameObject visualObject;
    public int tromboneID;

    public void SetText(string newText)
    {
        visualObject.gameObject.transform.GetChild(4).gameObject.GetComponent<Text>().text = newText;
    }
    
    public void HoverButton(bool show)
    {
        var hover = visualObject.gameObject.transform.GetChild(1).gameObject;
        if (show)
        {
            hover.SetActive(true);
            hover.transform.localScale = new Vector3(0.92f, 0.92f, 0.92f);
            LeanTween.scale(hover, new Vector3(1f, 1f, 1f), 0.15f).setEaseOutQuart();
        }
        else
        {
            hover.SetActive(false);
            hover.transform.localScale = new Vector3(0.45f, 0.45f, 1f);
        }
    }
}