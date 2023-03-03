using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TrombLoader.Class_Patches;

[HarmonyPatch(typeof(CharSelectController))]
[HarmonyPatch(nameof(LevelSelectController.Start))]
public class CharSelectControllerPatch
{
    public static GameObject tromboneListVisualButton;
    public static Button tromboneListButton;
    
    static void Postfix(CharSelectControllerPatch __instance)
    {
        //Hijack an existing button
        var sfx0VisualButton = GameObject.Find("sfx1");
        var sfx0Button = GameObject.Find("sfx_btn1");
        
        //Duplicate it
        tromboneListVisualButton = GameObject.Instantiate( sfx0VisualButton, sfx0VisualButton.transform.parent );
        tromboneListButton = GameObject.Instantiate( sfx0Button, sfx0Button.transform.parent ).GetComponent<Button>();; 
        
        //Clean Events
        tromboneListButton.onClick = new Button.ButtonClickedEvent();
        tromboneListButton.onClick.AddListener( ()=>Debug.Log("Clicked my fancy new button!!!!!"));

        //Set new Position
        var newPos = tromboneListButton.gameObject.GetComponent<RectTransform>().localPosition;
        newPos += new Vector3(0, 222, 0);

        tromboneListButton.gameObject.GetComponent<RectTransform>().localPosition = newPos;
        tromboneListVisualButton.GetComponent<RectTransform>().localPosition = newPos;
        
        //Hover code
        var trigger = tromboneListButton.GetComponent<EventTrigger>();
        var hover = tromboneListVisualButton.gameObject.transform.GetChild(1).gameObject;

        trigger.triggers = new List<EventTrigger.Entry>();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerEnter;
        entry.callback.AddListener( (eventData) => { HoverButton(hover,true); } );
        trigger.triggers.Add(entry);
        
        entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerExit;
        entry.callback.AddListener( (eventData) => { HoverButton(hover, false); } );
        trigger.triggers.Add(entry);
        
        //Set Name
        var text = tromboneListVisualButton.gameObject.transform.GetChild(4).gameObject.GetComponent<Text>();
        text.text = "Custom";
    }

    public static void HoverButton(GameObject hover, bool show)
    {
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