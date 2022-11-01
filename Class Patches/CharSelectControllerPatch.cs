using System;
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
    public static List<CustomTromboneSoundset> soundsetsList = new List<CustomTromboneSoundset>();
    public static List<VisualSoundsetButton> buttonList = new List<VisualSoundsetButton>();
    private static int numberOfButtons = 5;
    static void Postfix(CharSelectController __instance)
    {
        /*
        
        
        //Hover code
        var trigger = tromboneListButton.GetComponent<EventTrigger>();
        var hover = tromboneListVisualButton.gameObject.transform.GetChild(1).gameObject;

        trigger.triggers = new List<EventTrigger.Entry>();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerEnter;
        entry.callback.AddListener( (eventData) => { HoverButton(hover,true); } );
        trigger.triggers.Add(entry);
        
        EventTrigger exit = new EventTrigger.Entry();
        exit.eventID = EventTriggerType.PointerExit;
        exit.callback.AddListener( (eventData) => { HoverButton(hover, false); } );
        trigger.triggers.Add(entry);
        
        //Set Name
        var text = tromboneListVisualButton.gameObject.transform.GetChild(4).gameObject.GetComponent<Text>();
        text.text = "Custom";*/
    
        soundsetsList = new List<CustomTromboneSoundset>();
        buttonList = new List<VisualSoundsetButton>();
        
        //Hijack an existing button
        var sfx0VisualButton = GameObject.Find("sfx1");
        var sfx0Button = GameObject.Find("sfx_btn1");
        
        //Duplicate it
        var nextPageVisualButton = GameObject.Instantiate( sfx0VisualButton, sfx0VisualButton.transform.parent );
        var nextPageActualButton = GameObject.Instantiate( sfx0Button, sfx0Button.transform.parent ).GetComponent<Button>();; 
        
        //Clean Events
        nextPageActualButton.onClick = new Button.ButtonClickedEvent();
        nextPageActualButton.onClick.AddListener( ()=>
        {
            DeselectAllButtons();
            ScrollSoundset(true);
        });

        //Set new Position
        var newPos = nextPageActualButton.gameObject.GetComponent<RectTransform>().localPosition;
        newPos = new Vector3(600, 540, 0);

        nextPageActualButton.gameObject.GetComponent<RectTransform>().localPosition = newPos;
        
        newPos = new Vector3(610, 536, 0);
        nextPageVisualButton.GetComponent<RectTransform>().localPosition = newPos;
        
        //Set Name
        var text = nextPageVisualButton.gameObject.transform.GetChild(4).gameObject.GetComponent<Text>();
        text.text = "Next Page";
        
        //Create new list of soundsets
        for (int i = 0; i <= 12; i++)
        {
            var soundset = new CustomTromboneSoundset();
            soundset.soundsetID = i;
            soundset.path = "";
            soundset.isCustom = false;
            soundset.soundsetName =  "Custom" + i;
            soundsetsList.Add(soundset);
        }
        
        //Find default game buttons and set them up
        for (int i = 0 ; i < numberOfButtons; i++ ) {
            var visualButton = GameObject.Find("sfx"+i);
            var actualButton = GameObject.Find("sfx_btn"+i);
            var aux = new VisualSoundsetButton(visualButton, actualButton, soundsetsList[i]);
            aux.visualButton = visualButton;
            aux.actualButton = actualButton;
            
            //Quick Flash
            //aux.onSelect += __instance.quickFlash;
            
            buttonList.Add(aux);
        }
        
    }

    public static void DeselectAllButtons()
    {
        foreach (var but in CharSelectControllerPatch.buttonList)
        {
            but.SelectButton(false);
        }
    }

    public static void ScrollSoundset(bool down = true)
    {
        if (GlobalVariables.chosen_soundset == 0) return;
        var numberOfPages = ((soundsetsList.Count-1) / numberOfButtons) + 1;
        var currentPage = (GlobalVariables.chosen_soundset) / numberOfButtons;

        if (down)
        {
            Debug.Log($"==========================================");
            Debug.Log($"Current Page:{currentPage}");
            Debug.Log($"Number of Pages:{numberOfPages}");
            Debug.Log($"Current Soundset:{GlobalVariables.chosen_soundset}");
            Debug.Log($"Number of Soundsets:{soundsetsList.Count}");
            Debug.Log($"Number of Buttons:{numberOfButtons}");
            currentPage += 1;
            if (currentPage >= numberOfPages) currentPage = 0;
            Debug.Log($"Increasing page to:{currentPage}");
            //Configure page
            var buttonIndex = 0;
            for (int i = currentPage * numberOfButtons; i < (currentPage+1) * (numberOfButtons); i++)
            {
                if (i >= soundsetsList.Count)
                {
                    buttonList[buttonIndex].EnableButton(false);
                }
                else
                {
                    Debug.Log($"Setting button #{buttonIndex} text to:{soundsetsList[i].soundsetName }, with i being {i}");
                    buttonList[buttonIndex].EnableButton(true);
                    buttonList[buttonIndex].SetSoundSet( soundsetsList[i] );
                }

                if (buttonIndex == 0)
                {
                    Debug.Log($"Selecting default button");
                    buttonList[0].SelectButton(true);
                }

                buttonIndex++;
            }
        }
        else
        {
            if (currentPage > 0)
            {
                currentPage -= 1;
                //Configure page
            }
        }
    }
    
    public class CustomTromboneSoundset
    {
        public int soundsetID;
        public string soundsetName;
        public bool isCustom = true;
        public string path;
    }
    
    public class VisualSoundsetButton
    {
        public GameObject visualButton;
        public GameObject actualButton;
        public CustomTromboneSoundset soundset;
        
        public VisualSoundsetButton(GameObject newVisualButton, GameObject newActualButton, CustomTromboneSoundset newSoundset)
        {
            visualButton = newVisualButton;
            actualButton = newActualButton;
            
            var trigger = actualButton.GetComponent<EventTrigger>();

            trigger.triggers = new List<EventTrigger.Entry>();

            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerEnter;
            entry.callback.AddListener( (eventData) => { HoverButton(true); } );
            trigger.triggers.Add(entry);
        
            EventTrigger.Entry exit = new EventTrigger.Entry();
            exit.eventID = EventTriggerType.PointerExit;
            exit.callback.AddListener( (eventData) => { HoverButton( false); } );
            trigger.triggers.Add(exit);
            
            actualButton.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
            actualButton.GetComponent<Button>().onClick.AddListener(() =>
            {

                DeselectAllButtons();
                SelectButton(true);

            });
            
            SetSoundSet(newSoundset);
        }

        public void SetSoundSet(CustomTromboneSoundset newSoundset)
        {
            soundset = newSoundset;
            SetText(soundset.soundsetName);
        }

        public void SetText(string newText)
        {
            visualButton.gameObject.transform.GetChild(4).gameObject.GetComponent<Text>().text = newText;
        }

        public void EnableButton(bool enable)
        {
            visualButton.SetActive(enable);
            actualButton.SetActive(enable);
        }

        public void SelectButton(bool selected)
        {
            GameObject obj = visualButton;
            //Used to deselect all other buttons
            if (selected) GlobalVariables.chosen_soundset = soundset.soundsetID;
            obj.transform.GetChild(2).gameObject.SetActive(selected);
            obj.transform.GetChild(0).gameObject.SetActive(selected);
            LeanTween.cancel(obj.transform.GetChild(0).GetChild(0).gameObject);
            LeanTween.cancel(obj.transform.GetChild(0).GetChild(1).gameObject);
            obj.transform.GetChild(0).GetChild(0).transform.localScale = new Vector3(1f, 1.2f, 1f);
            obj.transform.GetChild(0).GetChild(1).transform.localScale = new Vector3(-1f, 0.1f, 1f);
            LeanTween.scaleY(obj.transform.GetChild(0).GetChild(0).gameObject, 0.1f, 0.15f).setEaseInOutQuad().setLoopPingPong();
            LeanTween.scaleY(obj.transform.GetChild(0).GetChild(1).gameObject, 1.2f, 0.15f).setEaseInOutQuad().setLoopPingPong();
        }
        
        public void HoverButton(bool show)
        {
            var hover = visualButton.gameObject.transform.GetChild(1).gameObject;
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

}