using System.IO;
using UnityEngine;

namespace MyFirstPlugin;

public static class Globals
{
    public static string defaultChartName = "song.tmb";
    public static string defaultAudioName = "song.ogg";

    public static string GetCustomContentPath()
    {
        return Application.dataPath + "/../custom/";
    }
    
    public static string GetCustomSongsPath()
    {
        return GetCustomContentPath() + "/songs/";
    }

    //If there is no chart named trackReference.tmb in the streamingAssets/leveldata folder, then we are loading a custom chart
    public static bool IsCustomTrack(string trackReference)
    {
        return !File.Exists(Application.dataPath + "/StreamingAssets/leveldata/"+trackReference+".tmb");
    }
}