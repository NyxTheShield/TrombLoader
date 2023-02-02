using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace TrombLoader.CustomTracks.Backgrounds;

public class VideoBackground : HijackedBackground
{
    private readonly string _videoPath;

    public VideoBackground(string videoPath)
    {
        _videoPath = videoPath;
    }
    
    public override void SetUpBackground(BGController controller, GameObject bg)
    {
        DisableParts(bg);

        var bgplane = bg.transform.GetChild(0);
        var pc = bgplane.GetChild(0);

        bgplane.gameObject.SetActive(true);
        pc.gameObject.SetActive(true);
        pc.GetComponent<SpriteRenderer>().color = Color.black;

        var videoPlayer = pc.GetComponent<VideoPlayer>() ?? pc.gameObject.AddComponent<VideoPlayer>();

        videoPlayer.url = _videoPath;
        videoPlayer.isLooping = true;
        videoPlayer.playOnAwake = false;
        videoPlayer.skipOnDrop = true;
        videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
        videoPlayer.targetCamera = bg.transform.GetComponent<Camera>();

        videoPlayer.enabled = true;
        videoPlayer.Pause();

        controller.StartCoroutine(PlayVideoDelayed(videoPlayer).GetEnumerator());
    }
    
    public static IEnumerable<YieldInstruction> PlayVideoDelayed(VideoPlayer videoPlayer)
    {
        yield return new WaitForSeconds(2.4f);

        if (videoPlayer != null)
        {
            videoPlayer.Play();
        }
    }
}