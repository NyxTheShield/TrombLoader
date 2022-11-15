using HarmonyLib;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using TrombLoader.Data;
using TrombLoader.Helpers;
using UnityEngine;

namespace TrombLoader.Class_Patches
{
    [HarmonyPatch(typeof(SaverLoader))]
    [HarmonyPatch(nameof(SaverLoader.loadLevelData))] // if possible use nameof() here
    class SaverLoaderPatch
    {
        static void Postfix(SaverLoader __instance)
        {
            try
            {
                Globals.ChartFolders.Clear();

                string path = Application.streamingAssetsPath + "/leveldata/songdata.tchamp";
                if (!File.Exists(path))
                {
                    Plugin.LogDebug("Couldnt load default tracks... could not find global data file");
                    return;
                }
                Plugin.LogDebug("Appending Custom Tracks to default track list");
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                FileStream fileStream = File.Open(path, FileMode.Open);
                SongData songData = (SongData)binaryFormatter.Deserialize(fileStream);
                fileStream.Close();

                CreateMissingDirectories();

                //Iterate over custom/songs directories, check for charts song.tmb, and if found, add it to the global track list
                List<string[]> fullTrackTitles = GlobalVariables.data_tracktitles.ToList();

                var songs = Directory.GetFiles(Globals.GetCustomSongsPath(), "song.tmb", SearchOption.AllDirectories).Select(i => Path.GetDirectoryName(i));
                songs = songs.Concat(Directory.GetFiles(BepInEx.Paths.PluginPath, "song.tmb", SearchOption.AllDirectories).Select(i => Path.GetDirectoryName(i)));

                var index = GlobalVariables.data_tracktitles.Length;
                foreach (var songFolder in songs)
                {
                    string chartPath = songFolder + "/" + Globals.defaultChartName;
                    if (File.Exists(chartPath))
                    {
                        var customLevel = new CustomSavedLevel(chartPath);
                        Plugin.LogDebug($"Found Custom Chart!: {customLevel.trackRef}");

                        var aux = new List<string>();
                        aux.Add(customLevel.name);
                        aux.Add(customLevel.shortName);
                        aux.Add(customLevel.trackRef);
                        aux.Add(customLevel.year.ToString());
                        aux.Add(customLevel.author);
                        aux.Add(customLevel.genre);
                        aux.Add(customLevel.description);
                        aux.Add(customLevel.difficulty.ToString());
                        aux.Add(Mathf.FloorToInt(customLevel.endpoint / (customLevel.tempo / 60f)).ToString());
                        aux.Add(Mathf.RoundToInt(customLevel.tempo).ToString());
                        aux.Add(index.ToString());

                        fullTrackTitles.Add(aux.ToArray());
                        if (!Globals.ChartFolders.ContainsKey(customLevel.trackRef))
                        {
                            Globals.ChartFolders.Add(customLevel.trackRef, songFolder);
                        }
                        index++;
                    }
                    else
                    {
                        Plugin.LogDebug("Folder has no chart, ignoring...");
                    }
                }

                GlobalVariables.data_tracktitles = fullTrackTitles.ToArray();

                Plugin.LogDebug("========================================");
                Plugin.LogDebug("Printing Full Track List:");
                Plugin.LogDebug("========================================");
                Plugin.LogDebug($"{"Reference",15} || {"Author",15} || {"BPM",3}");
                int i = 0;
                foreach (var trackRef in GlobalVariables.data_tracktitles)
                {
                    Plugin.LogDebug($"{trackRef,15} || {GlobalVariables.data_tracktitles[i][4],30} || {GlobalVariables.data_tracktitles[i][8],3}");

                    i += 1;
                }
                return;
            }
            catch (Exception ex)
            {
                Plugin.LogError(ex.ToString());
                Plugin.LogError("Disabling save creation/deletion out of an abundance of caution...");
                Globals.SaveCreationEnabled = false;
            }
        }

        public static void CreateMissingDirectories()
        {
            //If the custom folder doesnt exist, create it
            if (!Directory.Exists(Globals.GetCustomSongsPath()))
            {
                Directory.CreateDirectory(Globals.GetCustomSongsPath());
            }

            //If custom song list doesnt exist, create it
            if (!Directory.Exists(Globals.GetCustomSongsPath()))
            {
                Directory.CreateDirectory(Globals.GetCustomSongsPath());
            }
        }

        //Serializes a songdata into a readable json, for debugging purposes
        public static JSONNode Serialize(SongData data)
        {
            Plugin.LogDebug("=========================================================================");
            Plugin.LogDebug(" Serializing SongData");
            Plugin.LogDebug("=========================================================================");
            JSONObject jsonobject = new JSONObject();
            int num = 0;
            foreach (string text in data.data_tracktitles.Select(x => x[2]))
            {
                jsonobject[text]["trackRef"] = text;
                jsonobject[text]["name"] = data.data_tracktitles[num][0];
                jsonobject[text]["shortName"] = data.data_tracktitles[num][1];
                jsonobject[text]["year"] = data.data_tracktitles[num][2];
                jsonobject[text]["author"] = data.data_tracktitles[num][3];
                jsonobject[text]["genre"] = data.data_tracktitles[num][4];
                jsonobject[text]["description"] = data.data_tracktitles[num][5];
                jsonobject[text]["difficulty"] = data.data_tracktitles[num][6];
                jsonobject[text]["BPM"] = data.data_tracktitles[num][7];
                jsonobject[text]["UNK1"] = data.data_tracktitles[num][8];

                Plugin.LogDebug(jsonobject[text]["trackRef"]);

                num++;
            }
            Plugin.LogDebug("=========================================================================");
            return jsonobject;
        }

        //TODO: Remove this, craft from folders instead
        public static SongData DeserializeCustomSongsAndAppendToCurrentSongData(JSONNode customSongsJson, SongData currentSongData)
        {
            Plugin.LogDebug("=========================================================================");
            Plugin.LogDebug(" Deserializing songlist.json");
            Plugin.LogDebug("=========================================================================");
            List<string> trackRefs = new List<string>();
            List<List<string>> trackTitles = new List<List<string>>();
            int num = 0;
            foreach (KeyValuePair<string, JSONNode> keyValuePair in customSongsJson)
            {
                JSONNode value = keyValuePair.Value;
                List<string> currentTrackInfo = new List<string>();
                currentTrackInfo.Add(value["name"].Value);
                currentTrackInfo.Add(value["shortName"]);
                currentTrackInfo.Add(value["trackRef"]);
                currentTrackInfo.Add(value["year"]);
                currentTrackInfo.Add(value["author"]);
                currentTrackInfo.Add(value["genre"]);
                currentTrackInfo.Add(value["description"]);
                currentTrackInfo.Add(value["difficulty"]);
                currentTrackInfo.Add(value["BPM"]);
                currentTrackInfo.Add(value["UNK1"]);
                trackTitles.Add(currentTrackInfo);
                num++;

                Plugin.LogDebug(value["trackRef"]);
            }


            //Append everything together
            var fulltrackRefsList = new List<string>();
            var fulltrackTitles = new List<List<string>>();

            foreach (var reference in trackRefs)
            {
                fulltrackRefsList.Add(reference);
            }

            foreach (var reference in currentSongData.data_tracktitles)
            {
                fulltrackTitles.Add(reference.ToList());
            }
            foreach (var reference in trackTitles)
            {
                fulltrackTitles.Add(reference);
            }

            currentSongData.data_tracktitles = (from l in fulltrackTitles select l.ToArray()).ToArray();
            Plugin.LogDebug("=========================================================================");
            return currentSongData;
        }
    }

    // size of 100 is hard coded in this method
    [HarmonyPatch(typeof(SaverLoader), nameof(SaverLoader.grabHighestScore))]
    class SaverLoaderGrabHigestScoresPatch
    {
        static bool Prefix(ref int __result, string songtag)
        {
            var trackScores = Plugin.Instance.GetTrackScores();
            __result = trackScores.TryGetValue(songtag, out string[] vals) ? int.Parse(vals[2]) : 0;
            return false;
        }
    }

    // size of 100 is hard coded in this method
    [HarmonyPatch(typeof(SaverLoader), nameof(SaverLoader.checkForUpdatedScore))]
    class SaverLoaderCheckForUpdatedScorePatch
    {
        static bool Prefix(string songtag, int newscore, string newletterscore)
        {
            Plugin.LogDebug("Checking for updated score: " + songtag + "," + newscore + "," + newletterscore);
            string[] trackScore = GlobalVariables.localsave.data_trackscores
                .Where(i => i != null && i[0] == songtag).FirstOrDefault();
            if (trackScore == null) return false;

            trackScore[1] = getBestLetterScore(trackScore[1], newletterscore);
            List<int> bestScores = getBestScores(newscore, trackScore);
            for (int i = 0; i < trackScore.Length - 2 && i < bestScores.Count; i++)
            {
                trackScore[i + 2] = bestScores[i].ToString();
            }
            Plugin.LogDebug("[Best score] " + songtag + ": " + trackScore[1] + " " + trackScore[2]);
            return false;
        }

        private static string getBestLetterScore(string oldScore, string newScore)
        {
            string[] allScores = new string[7] { "-", "F", "D", "C", "B", "A", "S" };
            int oldScoreIndex = 0, newScoreIndex = 0;
            for (int i = 0; i < allScores.Length; i++)
            {
                if (allScores[i] == oldScore) oldScoreIndex = i;
                if (allScores[i] == newScore) newScoreIndex = i;
            }
            return allScores[Math.Max(oldScoreIndex, newScoreIndex)];
        }

        private static List<int> getBestScores(int newScore, string[] trackScore)
        {
            List<int> bestScores = new List<int>();
            bestScores.Add(newScore);
            for (int i = 2; i < trackScore.Length; i++)
            {
                bestScores.Add(int.Parse(trackScore[i]));
            }
            return bestScores.OrderByDescending(i => i).ToList();
        }
    }
}
