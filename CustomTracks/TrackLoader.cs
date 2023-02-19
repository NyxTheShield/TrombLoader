using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BaboonAPI.Hooks.Tracks;
using Newtonsoft.Json;
using TrombLoader.Helpers;

namespace TrombLoader.CustomTracks;

public class TrackLoader: TrackRegistrationEvent.Listener
{
    private JsonSerializer _serializer = new();

    public IEnumerable<TromboneTrack> OnRegisterTracks()
    {
        CreateMissingDirectories();

        var songs = Directory.GetFiles(Globals.GetCustomSongsPath(), "song.tmb", SearchOption.AllDirectories)
            .Concat(Directory.GetFiles(BepInEx.Paths.PluginPath, "song.tmb", SearchOption.AllDirectories))
            .Select(i => Path.GetDirectoryName(i));

        foreach (var songFolder in songs)
        {
            var chartPath = Path.Combine(songFolder, Globals.defaultChartName);
            if (!File.Exists(chartPath)) continue;

            using var stream = File.OpenText(chartPath);
            using var reader = new JsonTextReader(stream);

            CustomTrack customLevel;
            try 
            {
                customLevel = _serializer.Deserialize<CustomTrack>(reader);
            }
            catch (Exception exc)
            {
                Plugin.LogWarning($"Unable to deserialize JSON of custom chart: {chartPath}");
                Plugin.LogWarning(exc.Message);
                continue;
            }

            if (customLevel == null) continue;

            Plugin.LogDebug($"Found custom chart: {customLevel.trackref}");

            customLevel.folderPath = songFolder;
            yield return customLevel;
        }
    }
    
    private static void CreateMissingDirectories()
    {
        //If the custom folder doesnt exist, create it
        if (!Directory.Exists(Globals.GetCustomSongsPath()))
        {
            Directory.CreateDirectory(Globals.GetCustomSongsPath());
        }
    }
}