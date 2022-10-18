using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace TrombLoader.Class_Patches
{
    // size of 100 is hard coded in this method
    [HarmonyPatch(typeof(PointSceneController), nameof(PointSceneController.checkScoreCheevos))]
    class PointSceneControllerCheckScoreCheevosPatch
    {
        static bool Prefix(PointSceneController __instance)
        {
            List<string[]> playedSongs = GlobalVariables.localsave.data_trackscores
                .Where(i => i != null && i[0] != null && int.Parse(i[2]) > 0)
                .ToList();

            if (playedSongs.Count == 20)
            {
                AchievementSetter.setAchievement("PLAY_ALL_SONGS"); // actually PLAY_20_SONGS
            }

            if (__instance.letterscore == "S" || __instance.letterscore == "SS")
            {
                int sScores = playedSongs.Where(i => i[1] == "S" || i[1] == "SS").Count();
                switch (sScores)
                {
                    case 1:
                        AchievementSetter.setAchievement("S_RANK_01");
                        break;
                    case 5:
                        AchievementSetter.setAchievement("S_RANK_05");
                        break;
                    case 10:
                        AchievementSetter.setAchievement("S_RANK_10");
                        break;
                    case 15:
                        AchievementSetter.setAchievement("S_RANK_15");
                        break;
                    case 20:
                        AchievementSetter.setAchievement("S_RANK_20");
                        break;
                }
            }

            if (__instance.totalscore == 0) AchievementSetter.setAchievement("LOW_SCORE");
            return false;
        }
    }
}
