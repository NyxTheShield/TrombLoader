using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace TrombLoader.CustomTracks;

[JsonObject]
public class CustomTrackData
{
    public string trackRef;
    public string name;
    public string shortName;
    public string author;
    public string description;
    public float endpoint;
    public int year;
    public string genre;
    public int difficulty;
    public float tempo;
    public string backgroundMovement;
    public int savednotespacing;
    public int timesig;
    public List<Lyric> lyrics;
    public float[] note_color_start;
    public float[] note_color_end;
    public float[][] notes;
    public float[][] bgdata;

    public SavedLevel ToSavedLevel()
    {
        return new SavedLevel
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
    }

    [JsonObject]
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
}
