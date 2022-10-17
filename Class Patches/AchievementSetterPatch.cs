using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace TrombLoader.Class_Patches
{
    // size of 100 is hard coded in this method
    [HarmonyPatch(typeof(AchievementSetter), nameof(AchievementSetter.checkCheevo))]
    class AchievementSetterPatch
    {
        static bool Prefix(string cheevo_name)
        {
            if (cheevo_name == "PLAY_ALL_SONGS")
            {
                List<string[]> playedSongs = GlobalVariables.localsave.data_trackscores
                    .Where(i => i != null && i[0] != null && int.Parse(i[2]) > 0)
                    .ToList();

                if (playedSongs.Count == GlobalVariables.data_trackrefs.Length)
                {
                    AchievementSetter.setAchievement(cheevo_name);
                }
                return false;
            }
            else if (cheevo_name.StartsWith("S_RANK"))
            {
                int sRanks = GlobalVariables.localsave.data_trackscores
                    .Where(i => i != null && i[1] == "S")
                    .Count();

                if (sRanks >= 1 && cheevo_name == "S_RANK_01")
                {
                    AchievementSetter.setAchievement(cheevo_name);
                }
                else if (sRanks >= 5 && cheevo_name == "S_RANK_05")
                {
                    AchievementSetter.setAchievement(cheevo_name);
                }
                else if (sRanks >= 10 && cheevo_name == "S_RANK_10")
                {
                    AchievementSetter.setAchievement(cheevo_name);
                }
                else if (sRanks >= 15 && cheevo_name == "S_RANK_15")
                {
                    AchievementSetter.setAchievement(cheevo_name);
                }
                else if (sRanks >= 20 && cheevo_name == "S_RANK_20")
                {
                    AchievementSetter.setAchievement(cheevo_name);
                }
                return false;
            }
            return true;
        }
    }
}
