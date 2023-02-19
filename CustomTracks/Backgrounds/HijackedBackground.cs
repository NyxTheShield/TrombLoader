using BaboonAPI.Hooks.Tracks;
using UnityEngine;

namespace TrombLoader.CustomTracks.Backgrounds;

/// <summary>
///  Custom backgrounds which are based on hijacking a basegame background
/// </summary>
public abstract class HijackedBackground : AbstractBackground
{
    protected HijackedBackground() :
        base(AssetBundle.LoadFromFile($"{Application.streamingAssetsPath}/trackassets/ballgame"))
    {
    }

    public override GameObject Load(BackgroundContext ctx)
    {
        return Bundle.LoadAsset<GameObject>("BGCam_ballgame");
    }

    protected void DisableParts(GameObject bg)
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
}