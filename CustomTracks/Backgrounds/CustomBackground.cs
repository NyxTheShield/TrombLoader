using System.Collections.Generic;
using System.IO;
using BaboonAPI.Hooks.Tracks;
using TrombLoader.Data;
using TrombLoader.Helpers;
using UnityEngine;
using UnityEngine.Video;

namespace TrombLoader.CustomTracks.Backgrounds;

/// <summary>
///  Custom .trombackgrounds, which are Unity assetbundles
/// </summary>
public class CustomBackground : AbstractBackground
{
    private string _songPath;

    public CustomBackground(AssetBundle bundle, string songPath) : base(bundle)
    {
        _songPath = songPath;
    }

    public override GameObject Load(BackgroundContext ctx)
    {
        var bg = Bundle.LoadAsset<GameObject>("assets/_background.prefab");

        // MacOS Shader Handling
        // May need to be expanded to other platforms eventually, but for now only check for invalid shaders on mac
        if (Application.platform == RuntimePlatform.OSXPlayer)
        {
            Dictionary<string, Shader> shaderCache = Plugin.Instance.ShaderHelper.ShaderCache;

            // bundle auto-built in TrombLoaderBackgroundProject for custom shaders
            // platform is null because we want to load EVERY shader bundle with EVERY name
            // TODO: cache this inbetween song runs
            var songSpecificShaderCache = Plugin.Instance.ShaderHelper.LoadShaderBundleFromPath(_songPath, null);
            foreach(var songSpecificShader in songSpecificShaderCache)
            {
                // shaders in the bundle should actually overwrite the global shaders temporarily, just in case it's a modified version.

                // for backgrounds that don't supply a custom macos shader file but still have custom shaders (eg, legacy songs),
                // the absolute endgame would be to have a "valve steam deck shader cache" type service where recompiled shaders can be automatically downloaded
                // however, it is debatable if it is worth developing this soley for the few legacy songs with custom shaders
                if(!shaderCache.ContainsKey(songSpecificShader.Key)) shaderCache.Add(songSpecificShader.Key, songSpecificShader.Value);
                else shaderCache[songSpecificShader.Key] = songSpecificShader.Value;
            }

            foreach (var renderer in bg.GetComponentsInChildren<Renderer>(true))
            {
                foreach (var material in renderer.sharedMaterials)
                {
                    if (material == null || material.shader == null || material.shader.isSupported) continue;

                    Shader shader;
                    if (shaderCache.TryGetValue(material.shader.name, out shader))
                    {
                        // Shader exists and is cached, so *hopefully* it's the same and can be swapped out with no ill effects.
                        material.shader = shader;
                    }
                    else
                    {
                        // TODO: Handle more gracefully. Maybe replace with a default shader.
                        Plugin.LogDebug($"Could not find shader on {renderer.gameObject.name} ({material.shader.name})");
                    }
                }
            }
        }

        var managers = bg.GetComponentsInChildren<TromboneEventManager>();
        foreach (var eventManager in managers)
        {
            eventManager.DeserializeAllGenericEvents();
        }

        var invoker = bg.AddComponent<TromboneEventInvoker>();
        invoker.InitializeInvoker(ctx.controller, managers);

        foreach (var videoPlayer in bg.GetComponentsInChildren<VideoPlayer>())
        {
            if (videoPlayer.url == null || !videoPlayer.url.Contains("SERIALIZED_OUTSIDE_BUNDLE")) continue;
            var videoName = videoPlayer.url.Replace("SERIALIZED_OUTSIDE_BUNDLE/", "");
            var clipURL = Path.Combine(_songPath, videoName);
            videoPlayer.url = clipURL;
        }

        // handle foreground objects
        var fgholder = bg.transform.GetChild(1);
        while (fgholder.childCount < 8)
        {
            var fillerObject = new GameObject("Filler");
            fillerObject.transform.SetParent(fgholder);
        }

        // handle two background images
        while (bg.transform.GetChild(0).GetComponentsInChildren<SpriteRenderer>().Length < 2)
        {
            var fillerObject = new GameObject("Filler");
            fillerObject.AddComponent<SpriteRenderer>();
            fillerObject.transform.SetParent(bg.transform.GetChild(0));
        }

        // add confetti holder if missing
        if (bg.transform.childCount < 3)
        {
            var fillerConfettiHolder = new GameObject("ConfettiHolder");
            fillerConfettiHolder.transform.SetParent(bg.transform);
        }

        // layering
        var breathCanvas = ctx.controller.bottombreath.transform.parent.parent.GetComponent<Canvas>();
        if (breathCanvas != null) breathCanvas.planeDistance = 2;

        var champCanvas = ctx.controller.champcontroller.letters[0].transform.parent.parent.parent
            .GetComponent<Canvas>();
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

        return bg;
    }

    public override void SetUpBackground(BGController controller, GameObject bg)
    {
        var gameController = controller.gamecontroller;

        foreach (var videoPlayer in bg.GetComponentsInChildren<VideoPlayer>())
        {
            videoPlayer.Prepare();
        }

        var puppetController = bg.AddComponent<BackgroundPuppetController>();

        // puppet handling
        foreach (var trombonePlaceholder in bg.GetComponentsInChildren<TrombonerPlaceholder>())
        {
            int trombonerIndex = trombonePlaceholder.TrombonerType == TrombonerType.DoNotOverride
                ? gameController.puppetnum
                : (int)trombonePlaceholder.TrombonerType;

            foreach (Transform child in trombonePlaceholder.transform)
            {
                if (child != null) child.gameObject.SetActive(false);
            }

            var sub = new GameObject("RealizedTromboner");
            sub.transform.SetParent(trombonePlaceholder.transform);
            sub.transform.SetSiblingIndex(0);
            sub.transform.localPosition = new Vector3(-0.7f, 0.45f, -1.25f);
            sub.transform.localEulerAngles = new Vector3(0, 0f, 0f);
            trombonePlaceholder.transform.Rotate(new Vector3(0f, 19f, 0f));
            sub.transform.localScale = Vector3.one;

            //handle male tromboners being slightly shorter
            if (trombonerIndex > 3 && trombonerIndex != 8)
                sub.transform.localPosition = new Vector3(-0.7f, 0.35f, -1.25f);

            var tromboneRefs = new GameObject("TromboneTextureRefs");
            tromboneRefs.transform.SetParent(sub.transform);
            tromboneRefs.transform.SetSiblingIndex(0);

            var textureRefs = tromboneRefs.AddComponent<TromboneTextureRefs>();
            // a bit of getchild action to mirror game behaviour
            textureRefs.trombmaterials = gameController.modelparent.transform.GetChild(0)
                .GetComponent<TromboneTextureRefs>().trombmaterials;

            // Copy the tromboners in
            var trombonerGameObject =
                Object.Instantiate(gameController.playermodels[trombonerIndex], sub.transform, true);
            trombonerGameObject.transform.localScale = Vector3.one;

            Tromboner tromboner = new(trombonerGameObject, trombonePlaceholder);

            // Store tromboners for later
            var customPuppetTrait = trombonerGameObject.AddComponent<CustomPuppetController>();
            customPuppetTrait.Tromboner = tromboner;
            puppetController.Tromboners.Add(tromboner);

            tromboner.controller.setTromboneTex(trombonePlaceholder.TromboneSkin == TromboneSkin.DoNotOverride
                ? gameController.textureindex
                : (int)trombonePlaceholder.TromboneSkin);

            if (GlobalVariables.localsave.cardcollectionstatus[36] > 9)
            {
                tromboner.controller.show_rainbow = true;
            }
        }
    }
}