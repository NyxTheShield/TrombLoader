using TrombLoader.Helpers;
using UnityEngine;

namespace TrombLoader.CustomTracks.Backgrounds;

public class ImageBackground : HijackedBackground
{
    private readonly string _imagePath;

    public ImageBackground(string imagePath)
    {
        _imagePath = imagePath;
    }

    public override void SetUpBackground(BGController controller, GameObject bg)
    {
        DisableParts(bg);

        var bgplane = bg.transform.GetChild(0);
        var renderer = bgplane.GetChild(0).GetComponent<SpriteRenderer>();
        renderer.sprite = ImageHelper.LoadSpriteFromFile(_imagePath);

        bgplane.gameObject.SetActive(true);
        renderer.gameObject.SetActive(true);
    }
}