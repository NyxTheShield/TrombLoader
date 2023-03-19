using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BaboonAPI.Hooks.Tracks;
using Newtonsoft.Json;
using TrombLoader.CustomTracks.Backgrounds;
using UnityEngine;

namespace TrombLoader.CustomTracks;

[Serializable]
public class CustomTrack : TromboneTrack
{
    /// <summary>
    ///  Folder path that this track can be found at
    /// </summary>
    [JsonIgnore] public string folderPath { get; set; }

    public string trackRef;
    public string name;
    public string shortName;
    public string author;
    public string description;
    public float endpoint;

    [JsonProperty("year")] private int _year;
    [JsonProperty("tempo")] private float _tempo;
    [JsonProperty("savednotespacing")] private float _savednotespacing;
    [JsonProperty("timesig")] private float _timesig;

    public string genre { get; }
    public int difficulty { get; }
    public string backgroundMovement { get; }

    // SavedLevel fields
    [JsonIgnore] public int savednotespacing => (int) _savednotespacing;
    [JsonIgnore] public int timesig => (int) _timesig;
    public List<Lyric> lyrics { get; }
    public float[] note_color_start { get; }
    public float[] note_color_end { get; }
    public float[][] notes { get; }
    public float[][] bgdata { get; }

    [JsonIgnore] public string trackref => trackRef;
    [JsonIgnore] public string trackname_long => name;
    [JsonIgnore] public string trackname_short => shortName;
    [JsonIgnore] public string year => _year.ToString();
    [JsonIgnore] public string artist => author;
    [JsonIgnore] public string desc => description;
    [JsonIgnore] public int tempo => (int) _tempo;
    [JsonIgnore] public int length => Mathf.FloorToInt(endpoint / (_tempo / 60f));

    [JsonConstructor]
    public CustomTrack(string trackRef, string name, string shortName, string author, string description, float endpoint,
        int year, string genre, int difficulty, float tempo, string backgroundMovement, float savednotespacing, float timesig,
        List<Lyric> lyrics, float[] note_color_start, float[] note_color_end, float[][] notes, float[][] bgdata)
    {
        this.trackRef = trackRef;
        this.name = name;
        this.shortName = shortName;
        this.author = author;
        this.description = description;
        this.endpoint = endpoint;
        this._year = year;
        this.genre = genre;
        this.difficulty = difficulty;
        this._tempo = tempo;
        this.backgroundMovement = backgroundMovement ?? "none";
        this._savednotespacing = savednotespacing;
        this._timesig = timesig;
        this.lyrics = lyrics ?? new List<Lyric>();
        this.note_color_start = note_color_start ?? new[] { 1.0f, 0.21f, 0f };
        this.note_color_end = note_color_end ?? new[] { 1.0f, 0.8f, 0.3f };
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
            tempo = _tempo,
            timesig = timesig
        };

        return level;
    }

    public LoadedTromboneTrack LoadTrack()
    {
        return new LoadedCustomTrack(this, LoadBackground());
    }

    private AbstractBackground LoadBackground()
    {
        if (File.Exists(Path.Combine(folderPath, "bg.trombackground")))
        {
            var bundle = AssetBundle.LoadFromFile(Path.Combine(folderPath, "bg.trombackground"));
            return new CustomBackground(bundle, folderPath);
        }
        
        var possibleVideoPath = Path.Combine(folderPath, "bg.mp4");
        if (File.Exists(possibleVideoPath))
        {
            return new VideoBackground(possibleVideoPath);
        }

        var spritePath = Path.Combine(folderPath, "bg.png");
        if (File.Exists(spritePath))
        {
            return new ImageBackground(spritePath);
        }

        Plugin.LogWarning($"No background for track {trackref}");
        return new EmptyBackground();
    }

    public bool IsVisible()
    {
        return true;
    }

    public class LoadedCustomTrack : LoadedTromboneTrack
    {
        private readonly CustomTrack _parent;
        private readonly AbstractBackground _background;

        public LoadedCustomTrack(CustomTrack parent, AbstractBackground background)
        {
            _parent = parent;
            _background = background;
        }

        public TrackAudio LoadAudio()
        {
            var songPath = Path.Combine(_parent.folderPath, "song.ogg");
            var e = Plugin.Instance.GetAudioClipSync(songPath);

            // TODO: is there a sync way of getting audio clips off disk
            while (e.MoveNext())
            {
                switch (e.Current)
                {
                    case AudioClip clip:
                        return new TrackAudio(clip, 1.0f);
                    case string err:
                        Plugin.LogError(err);
                        return null;
                }
            }

            Plugin.LogError("Failed to load audio");
            return null;
        }

        public GameObject LoadBackground(BackgroundContext ctx)
        {
            return _background.Load(ctx);
        }

        public void SetUpBackgroundDelayed(BGController controller, GameObject bg)
        {
            // Fix layering
            // Without this hack, video backgrounds render wrong
            var modelCam = GameObject.Find("3dModelCamera")?.GetComponent<Camera>();
            if (modelCam != null)
            {
                modelCam.clearFlags = CameraClearFlags.Depth;
            }

            _background.SetUpBackground(controller, bg);

            controller.tickontempo = false;

            // Apply background effect
            controller.doBGEffect(_parent.backgroundMovement);
        }

        public void Dispose()
        {
            _background.Dispose();
        }

        public string trackref => _parent.trackref;
    }
}
