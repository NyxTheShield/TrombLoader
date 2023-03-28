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

        var seen = new HashSet<string>();
        foreach (var songFolder in songs)
        {
            var chartPath = Path.Combine(songFolder, Globals.defaultChartName);
            if (!File.Exists(chartPath)) continue;

            using var stream = File.OpenText(chartPath);
            using var reader = new JsonTextReader(stream);

            CustomTrackData customLevel;
            try
            {
                customLevel = _serializer.Deserialize<CustomTrackData>(reader);
            }
            catch (Exception exc)
            {
                Plugin.LogWarning($"Unable to deserialize JSON of custom chart: {chartPath}");
                Plugin.LogWarning(exc.Message);
                continue;
            }

            if (customLevel == null) continue;

            if (seen.Add(customLevel.trackRef))
            {
                Plugin.LogDebug($"Found custom chart: {customLevel.trackRef}");

                yield return new CustomTrack(songFolder, customLevel, this);
            }
            else
            {
                Plugin.LogWarning(
                    $"Skipping folder {chartPath} as its trackref '{customLevel.trackRef}' was already loaded!");
            }
        }
    }

    public SavedLevel ReloadTrack(CustomTrack existing)
    {
        var chartPath = Path.Combine(existing.folderPath, Globals.defaultChartName);
        using var stream = File.OpenText(chartPath);
        using var reader = new JsonTextReader(stream);

        var track = _serializer.Deserialize<CustomTrackData>(reader);

        return track?.ToSavedLevel();
    }

    public bool ShouldReloadChart()
    {
        return Plugin.Instance.DeveloperMode.Value;
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
