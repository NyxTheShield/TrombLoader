using System.Collections;
using TrombLoader.Data;
using UnityEngine;
using UnityEngine.Video;

namespace TrombLoader.Helpers;

public class BackgroundHelper
{
    public static void ApplyBackgroundTransforms(GameController instance, GameObject bg)
    {
        // puppet handling
		foreach(var trombonePlaceholder in bg.GetComponentsInChildren<TrombonerPlaceholder>())
        {
			int trombonerIndex = trombonePlaceholder.TrombonerType == TrombonerType.DoNotOverride
				? instance.puppetnum
				: (int) trombonePlaceholder.TrombonerType;

			// this specific thing could cause problems later but it's fine for now.
			trombonePlaceholder.transform.SetParent(bg.transform.GetChild(0));

			foreach(Transform child in trombonePlaceholder.transform)
            {
				if (child != null) child.gameObject.SetActive(false);
            }

			var sub = new GameObject();
			sub.transform.SetParent(trombonePlaceholder.transform);
			sub.transform.SetSiblingIndex(0);
			sub.transform.localPosition = new Vector3(-0.7f, 0.45f, -1.25f);
			sub.transform.localEulerAngles = new Vector3(0, 0f, 0f);
			trombonePlaceholder.transform.Rotate(new Vector3(0f, 19f, 0f));
			sub.transform.localScale = Vector3.one;

			//handle male tromboners being slightly shorter
			if(trombonerIndex > 3 && trombonerIndex != 8) sub.transform.localPosition = new Vector3(-0.7f, 0.35f, -1.25f);

			var placeHolder2 = new GameObject("TrombonePlaceHolder");
			var transform = trombonePlaceholder.transform;
			placeHolder2.transform.position = transform.position;
			placeHolder2.transform.eulerAngles = transform.eulerAngles;
			placeHolder2.transform.localScale = transform.localScale;

			var tromboneRefs = new GameObject("TromboneTextureRefs");
			tromboneRefs.transform.SetParent(sub.transform);
			tromboneRefs.transform.SetSiblingIndex(0);

			var textureRefs = tromboneRefs.AddComponent<TromboneTextureRefs>();
			textureRefs.trombmaterials = instance.modelparent.transform.GetChild(0).GetComponent<TromboneTextureRefs>().trombmaterials; // a bit of getchild action to mirror game behaviour

			var trombonerGameObject = Object.Instantiate(instance.playermodels[trombonerIndex], placeHolder2.transform, true);
			trombonerGameObject.transform.localScale = Vector3.one;

			var reparent = trombonerGameObject.AddComponent<Reparent>();
			reparent.instanceID = trombonePlaceholder.InstanceID;

			Tromboner tromboner = new(trombonerGameObject, trombonePlaceholder);
			Globals.Tromboners.Add(tromboner);

			tromboner.controller.setTromboneTex(trombonePlaceholder.TromboneSkin == TromboneSkin.DoNotOverride ? instance.textureindex : (int)trombonePlaceholder.TromboneSkin);

			if (GlobalVariables.localsave.cardcollectionstatus[36] > 9)
			{
				tromboner.controller.show_rainbow = true;
			}
		}

		// handle foreground objects
		while (bg.transform.GetChild(1).childCount < 8)
		{
			var fillerObject = new GameObject("Filler");
			fillerObject.transform.SetParent(bg.transform.GetChild(1));
		}

		// handle two background images
		while (bg.transform.GetChild(0).GetComponentsInChildren<SpriteRenderer>().Length < 2)
		{
			var fillerObject = new GameObject("Filler");
			fillerObject.AddComponent<SpriteRenderer>();
			fillerObject.transform.SetParent(bg.transform.GetChild(0));
		}

		// layering
        var breathCanvas = instance.bottombreath.transform.parent.parent.GetComponent<Canvas>();
        if (breathCanvas != null) breathCanvas.planeDistance = 2;

        var champCanvas = instance.champcontroller.letters[0].transform.parent.parent.parent.GetComponent<Canvas>();
        if (champCanvas != null) champCanvas.planeDistance = 2;

        var gameplayCam = GameObject.Find("GameplayCam")?.GetComponent<Camera>();
        if (gameplayCam != null) gameplayCam.depth = 99;

        var removeDefaultLights = bg.transform.Find("RemoveDefaultLights");
        if (removeDefaultLights)
        {
            foreach (var light in Object.FindObjectsOfType<Light>()) light.enabled = false;
            removeDefaultLights.gameObject.AddComponent<SceneLightingHelper>();
        }

        var addShadows = bg.transform.Find("AddShadows");
        if (addShadows)
        {
            QualitySettings.shadows = ShadowQuality.All;
            QualitySettings.shadowDistance = 100;
        }
    }

    public static void DisableBackground(GameObject bg)
    {
	    var bgplane = bg.transform.GetChild(0).gameObject;
	    var img1fade = bg.transform.GetChild(0).GetChild(1).gameObject;
		var img2fade = bg.transform.GetChild(0).GetChild(0).gameObject;
		var fgholder = bg.transform.GetChild(1).gameObject;

		bgplane.SetActive(false);
		img1fade.SetActive(false);
		img2fade.SetActive(false);
		fgholder.SetActive(false);
    }

    public static void ApplyImage(GameObject bg, string path)
    {
	    DisableBackground(bg);

	    var bgplane = bg.transform.GetChild(0);
	    var renderer = bgplane.GetChild(0).GetComponent<SpriteRenderer>();
	    renderer.sprite = ImageHelper.LoadSpriteFromFile(path);

	    bgplane.gameObject.SetActive(true);
	    renderer.gameObject.SetActive(true);
    }

    public static void ApplyVideo(GameObject bg, BGController bgController, string videoPath)
    {
	    DisableBackground(bg);

	    var bgplane = bg.transform.GetChild(0);
	    var pc = bgplane.GetChild(0);

	    bgplane.gameObject.SetActive(true);
	    pc.gameObject.SetActive(true);
	    pc.GetComponent<SpriteRenderer>().color = Color.black;

	    var videoPlayer = pc.GetComponent<VideoPlayer>() ?? pc.gameObject.AddComponent<VideoPlayer>();

	    videoPlayer.url = videoPath;
	    videoPlayer.isLooping = true;
	    videoPlayer.playOnAwake = false;
	    videoPlayer.skipOnDrop = true;
	    videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
	    videoPlayer.targetCamera = bg.transform.GetComponent<Camera>();

	    videoPlayer.enabled = true;
	    videoPlayer.Pause();

	    bgController.StartCoroutine(PlayVideoDelayed(videoPlayer).GetEnumerator());
    }

    public static IEnumerable PlayVideoDelayed(VideoPlayer videoPlayer)
    {
	    yield return new WaitForSeconds(2.4f);

	    if (videoPlayer != null)
	    {
		    videoPlayer.Play();
	    }
    }
}
