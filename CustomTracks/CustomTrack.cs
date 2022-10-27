using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BaboonAPI.Hooks;
using Newtonsoft.Json;
using TrombLoader.Helpers;
using UnityEngine;

namespace TrombLoader.CustomTracks;

[Serializable]
public class CustomTrack: Tracks.TromboneTrack
{
    [JsonIgnore]
    public string folderPath { get; set; }

    public string trackRef;
    public string name;
    public string shortName;
    public string author;
    public string description;
    public int endpoint;

    [JsonProperty("year")]
    private int yearInt;
    
    public string genre { get; }
    public int difficulty { get; }
    public int tempo { get; }
    public string backgroundMovement { get; }

    // SavedLevel fields
    public int savednotespacing { get; }
    public int timesig { get; }
    public List<Lyric> lyrics { get; }
    public float[] note_color_start { get; }
    public float[] note_color_end { get; }
    public float[][] notes { get; }
    public float[][] bgdata { get; }
    
    [JsonIgnore]
    public string trackref => trackRef;
    [JsonIgnore]
    public string trackname_long => name;
    [JsonIgnore]
    public string trackname_short => shortName;
    [JsonIgnore]
    public string year => yearInt.ToString();
    [JsonIgnore]
    public string artist => author;
    [JsonIgnore]
    public string desc => description;
    [JsonIgnore]
    public int length => Mathf.FloorToInt(endpoint / (tempo / 60f));

    [JsonConstructor]
    public CustomTrack(string trackRef, string name, string shortName, string author, string description, int endpoint, int year, string genre, int difficulty, int tempo, string backgroundMovement, int savednotespacing, int timesig, List<Lyric> lyrics, float[] note_color_start, float[] note_color_end, float[][] notes, float[][] bgdata)
    {
        this.trackRef = trackRef;
        this.name = name;
        this.shortName = shortName;
        this.author = author;
        this.description = description;
        this.endpoint = endpoint;
        this.yearInt = year;
        this.genre = genre;
        this.difficulty = difficulty;
        this.tempo = tempo;
        this.backgroundMovement = backgroundMovement;
        this.savednotespacing = savednotespacing;
        this.timesig = timesig;
        this.lyrics = lyrics ?? new List<Lyric>();
        this.note_color_start = note_color_start ?? new[] { 0.5f, 0.5f, 0.5f };
        this.note_color_end = note_color_end ?? new[] { 1.0f, 1.0f, 1.0f };
        this.notes = notes;
        this.bgdata = bgdata ?? Array.Empty<float[]>();
    }

    [Serializable]
    public class Lyric
    {
        public string text { get; }
        public float bar { get; }

        [JsonConstructor]
        public Lyric(string text, float bar)
        {
            this.text = text;
            this.bar = bar;
        }
    }

    public SavedLevel LoadChart()
    {
        var level = new SavedLevel
        {
            savedleveldata = new List<float[]>(notes),
            bgdata = new List<float[]>(bgdata),
            endpoint = endpoint,
            lyricspos = lyrics.Select(lyric => new[] { lyric.bar, 0 }).ToList(),
            lyricstxt = lyrics.Select(lyric => lyric.text).ToList(),
            note_color_start = note_color_start,
            note_color_end = note_color_end,
            savednotespacing = savednotespacing,
            tempo = tempo,
            timesig = timesig
        };

        return level;
    }

    public Tracks.LoadedTromboneTrack LoadTrack()
    {
        return new LoadedCustomTrack(this);
    }

    public bool IsVisible()
    {
        return true;
    }

    public class LoadedCustomTrack : Tracks.LoadedTromboneTrack
    {
        private CustomTrack _parent;
        private AssetBundle _backgroundBundle;

        public LoadedCustomTrack(CustomTrack parent)
        {
            _parent = parent;
        }

        public Tracks.TrackAudio LoadAudio()
        {
            var songPath = Path.Combine(_parent.folderPath, "song.ogg");
            var e = Plugin.Instance.GetAudioClipSync(songPath);

            // TODO: is there a sync way of getting audio clips off disk
            while (e.MoveNext())
            {
                switch (e.Current)
                {
                    case AudioClip clip:
                        return new Tracks.TrackAudio(clip, 1.0f);
                    case string err:
                        Plugin.LogError(err);
                        return null;
                }
            }
            
            Plugin.LogError("Failed to load audio");
            return null;
        }

        public GameObject LoadBackground()
        {
            var songPath = _parent.folderPath;
            if (File.Exists(Path.Combine(songPath, "bg.trombackground")))
            {
                _backgroundBundle = AssetBundle.LoadFromFile(Path.Combine(songPath, "bg.trombackground"));

                // TODO attach events
                return _backgroundBundle.LoadAsset<GameObject>("assets/_background.prefab");
            }
            
            // TODO handle other background types

            Plugin.LogError("Failed to load background");
            return new GameObject();
        }

        public void Dispose()
        {
            if (_backgroundBundle != null)
            {
                _backgroundBundle.Unload(true);
            }
        }

        public string trackref => _parent.trackref;
    }
}