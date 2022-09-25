using System.IO;
using UnityEngine;
using BepInEx;

namespace TrombLoader.Helpers
{
    public static class Globals
    {
        public static readonly string defaultChartName = "song.tmb";
        public static readonly string defaultAudioName = "song.ogg";

        public static string GetCustomSongsPath()
        {
            return Path.Combine(Paths.BepInExRootPath, "CustomSongs");
        }

        //If there is no chart named trackReference.tmb in the streamingAssets/leveldata folder, then we are loading a custom chart
        public static bool IsCustomTrack(string trackReference)
        {
            return !File.Exists(Path.Combine(Application.dataPath, "StreamingAssets", "leveldata", $"{trackReference}.tmb"));
        }
    }
}