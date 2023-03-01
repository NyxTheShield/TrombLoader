using System;
using System.Collections.Generic;
using System.IO;
using BaboonAPI.Hooks.Tracks;
using BepInEx;
using TrombLoader.CustomTracks;

namespace TrombLoader.Helpers
{
    public static class Globals
    {
        public static readonly string defaultChartName = "song.tmb";
        public static readonly string defaultAudioName = "song.ogg";
        public static string GetCustomSongsPath()
        {
            return Path.Combine(Paths.BepInExRootPath, "CustomSongs/");
        }

        /// <summary>
        ///  Check if a track was loaded by TrombLoader
        /// </summary>
        /// <param name="trackReference">Track reference to check</param>
        /// <returns>True if this is a TrombLoader-provided track, false otherwise</returns>
        public static bool IsCustomTrack(string trackReference)
        {
            var track = TrackLookup.lookup(trackReference);
            return track is CustomTrack;
        }

        [Obsolete("No longer populated, use BaboonAPI to look up the track and cast to CustomTrack instead")]
        public static Dictionary<string, string> ChartFolders = new();
        
        [Obsolete("No longer controlled by TrombLoader")]
        public static bool SaveCreationEnabled = true;
    }
}
