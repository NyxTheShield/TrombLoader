using HarmonyLib;
using SimpleJSON;
using System.Collections;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Permissions;
using TrombLoader.Data;
using TrombLoader.Helpers;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Events;
using UnityEngine.PostProcessing;
using UnityEngine.UI;

namespace TrombLoader.Class_Patches
{
    [HarmonyPatch(typeof(GameController))]
	[HarmonyPatch("Start")] // if possible use nameof() here
	class GameControllerStartPatch
	{
		//rewrite of the original
		static bool Prefix(GameController __instance)
		{
			if (GlobalVariables.localsettings.calibrationscreen)
			{
				if (GlobalVariables.localsettings.mousecontrolmode == 0)
				{
					__instance.mouse_rel_pos = 0f;
				}
				else if (GlobalVariables.localsettings.mousecontrolmode == 1)
				{
					__instance.mouse_rel_pos = 1f;
				}
				else if (GlobalVariables.localsettings.mousecontrolmode == 2)
				{
					__instance.mouse_rel_pos = 1f;
				}
				else if (GlobalVariables.localsettings.mousecontrolmode == 3)
				{
					__instance.mouse_rel_pos = 0f;
				}
			}
			GlobalVariables.gameplay_allnotescores.Clear();

			//.... Dont even ask why
			__instance.toot_keys.Add(KeyCode.Space);
			__instance.toot_keys.Add(KeyCode.A);
			__instance.toot_keys.Add(KeyCode.B);
			__instance.toot_keys.Add(KeyCode.C);
			__instance.toot_keys.Add(KeyCode.D);
			__instance.toot_keys.Add(KeyCode.E);
			__instance.toot_keys.Add(KeyCode.F);
			__instance.toot_keys.Add(KeyCode.G);
			__instance.toot_keys.Add(KeyCode.H);
			__instance.toot_keys.Add(KeyCode.I);
			__instance.toot_keys.Add(KeyCode.J);
			__instance.toot_keys.Add(KeyCode.K);
			__instance.toot_keys.Add(KeyCode.L);
			__instance.toot_keys.Add(KeyCode.M);
			__instance.toot_keys.Add(KeyCode.N);
			__instance.toot_keys.Add(KeyCode.O);
			__instance.toot_keys.Add(KeyCode.P);
			__instance.toot_keys.Add(KeyCode.Q);
			__instance.toot_keys.Add(KeyCode.R);
			__instance.toot_keys.Add(KeyCode.S);
			__instance.toot_keys.Add(KeyCode.T);
			__instance.toot_keys.Add(KeyCode.U);
			__instance.toot_keys.Add(KeyCode.V);
			__instance.toot_keys.Add(KeyCode.W);
			__instance.toot_keys.Add(KeyCode.X);
			__instance.toot_keys.Add(KeyCode.Y);
			__instance.toot_keys.Add(KeyCode.Z);
			__instance.toot_keys.Add(KeyCode.LeftArrow);
			__instance.toot_keys.Add(KeyCode.RightArrow);

			__instance.latency_offset = (float)GlobalVariables.localsettings.latencyadjust * 0.001f;

			Debug.Log("latency_offset: " + __instance.latency_offset);
			Application.targetFrameRate = 144;

			if (!__instance.leveleditor && !__instance.devmode)
			{
				Cursor.lockState = CursorLockMode.Confined;
			}
			__instance.retrying = false;
			__instance.notescoreaverage = 0;
			__instance.notescoresamples = 0;
			__instance.curtains.SetActive(true);
			__instance.curtainc = __instance.curtains.GetComponent<CurtainController>();
			__instance.level_finshed = false;
			__instance.scores_A = 0;
			__instance.scores_B = 0;
			__instance.scores_C = 0;
			__instance.scores_D = 0;
			__instance.scores_F = 0;

			__instance.beatstoshow = Plugin.Instance.beatsToShow?.Value ?? 64; // By default, this is 16.

			if (GlobalVariables.scene_destination == "freeplay")
			{
				__instance.freeplay = true;
				__instance.backbtn.SetActive(true);
				__instance.champcontroller.hideChampText();
			}
			else
			{
				__instance.backbtn.SetActive(false);
			}
			__instance.puppetnum = GlobalVariables.chosen_character;
			__instance.textureindex = GlobalVariables.chosen_trombone;
			__instance.soundset = GlobalVariables.chosen_soundset;
			__instance.popuptextobj.transform.localScale = new Vector3(0f, 1f, 1f);
			__instance.multtextobj.transform.localScale = new Vector3(0f, 1f, 1f);
			__instance.ui_savebtn.onClick.AddListener(new UnityAction(__instance.tryToSaveLevel));
			__instance.ui_loadbtn.onClick.AddListener(new UnityAction(__instance.loadFromEditor));
			BloomModel.Settings settings = __instance.gameplayppp.bloom.settings;
			QualitySettings.shadows = ShadowQuality.Disable; // Default
			settings.bloom.intensity = 0f;
			__instance.gameplayppp.bloom.settings = settings;
			if (!__instance.freeplay && !__instance.leveleditor)
			{
				if (!__instance.devmode)
				{
					__instance.songtitle.text = GlobalVariables.chosen_track_data.trackname_long;
					__instance.songtitleshadow.text = GlobalVariables.chosen_track_data.trackname_long;
				}
				else
				{
					__instance.songtitle.text = "testing";
					__instance.songtitleshadow.text = "testing";
				}
			}
			else if (__instance.leveleditor)
			{
				__instance.songtitle.text = "LEVEL EDITOR";
				__instance.songtitleshadow.text = "LEVEL EDITOR";
			}
			else if (__instance.freeplay)
			{
				__instance.songtitle.text = "";
				__instance.songtitleshadow.text = "";
				__instance.ui_score.text = "";
				__instance.ui_score_shadow.text = "";
				__instance.highestcombo.text = "";
				__instance.highestcomboshad.text = "";
			}
			string baseGameChartPath = "/trackassets/";
			string trackReference = GlobalVariables.chosen_track;
			string customTrackReference = trackReference;
			bool isCustomTrack = false;
			if (!__instance.freeplay)
			{
				baseGameChartPath += trackReference;
			}
			else if (__instance.freeplay)
			{
				baseGameChartPath += "freeplay";
			}
			if (!File.Exists(Application.streamingAssetsPath + baseGameChartPath))
			{
				Plugin.LogDebug("Nyx: Cant load asset bundle, must be a custom song, hijacking Ball game!");
				baseGameChartPath = "/trackassets/ballgame";
				trackReference = "ballgame";
				isCustomTrack = true;
			}
			__instance.myLoadedAssetBundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + baseGameChartPath);
			if (__instance.myLoadedAssetBundle == null)
			{
				Plugin.LogDebug("Failed to load AssetBundle!");
				return false;
			}
			Plugin.LogDebug("LOADED ASSETBUNDLE: " + Application.streamingAssetsPath + baseGameChartPath);
			if (!__instance.freeplay)
			{
				AudioSource component = __instance.myLoadedAssetBundle.LoadAsset<GameObject>("music_" + trackReference).GetComponent<AudioSource>();
				__instance.musictrack.clip = component.clip;
				__instance.musictrack.volume = component.volume;
				if (isCustomTrack)
				{
					Plugin.LogDebug("Nyx: Trying to load ogg from file!");

					var songPath = Path.Combine(Globals.ChartFolders[customTrackReference], "song.ogg");
					IEnumerator e = Plugin.Instance.GetAudioClipSync(songPath);

					//Worst piece of code I have ever seen, but it does the job, I guess
					//Unity has forced my hand once again
					//Forces a coroutine to be held manually, basically removing the point of it being a coroutine
					while (e.MoveNext())
					{
						if (e.Current != null)
						{
							if (e.Current is string)
							{
								Plugin.LogError("Couldnt Load OGG FILE!!");
							}
							else
							{
								__instance.musictrack.clip = e.Current as AudioClip;
							}
						}
					}

					//AudioClip clip = WavUtility.ToAudioClip(Globals.GetCustomSongsPath() + customTrackReference + "/song.wav");
					//Debug.Log(__instance.musictrack.clip == null);

				}
			}
			__instance.StartCoroutine(__instance.loadAssetBundleResources());
			__instance.bgcontroller.songname = customTrackReference;
            GameObject gameObject = new GameObject();

			// force kill all old puppets
			Globals.Tromboners.Clear();

			if (!__instance.freeplay)
			{
				gameObject = __instance.myLoadedAssetBundle.LoadAsset<GameObject>("BGCam_" + trackReference);

                if (isCustomTrack)
                {
					__instance.bgcontroller.tickontempo = false;

					var songPath = Globals.ChartFolders[customTrackReference];
					if (File.Exists(Path.Combine(songPath, "bg.trombackground")))
                    {
						var gameObjectOld = gameObject;
						gameObject = AssetBundleHelper.LoadObjectFromAssetBundlePath<GameObject>(Path.Combine(songPath, "bg.trombackground"));
						UnityEngine.Object.DontDestroyOnLoad(gameObject);

						var managers = gameObject.GetComponentsInChildren<TromboneEventManager>();
						foreach (var manager in managers) manager.DeserializeAllGenericEvents();

						var invoker = gameObject.AddComponent<TromboneEventInvoker>();
						invoker.InitializeInvoker(__instance, managers);
						UnityEngine.Object.DontDestroyOnLoad(invoker);

						foreach (var videoPlayer in gameObject.GetComponentsInChildren<VideoPlayer>())
                        {
							if (videoPlayer.url != null && videoPlayer.url.Contains("SERIALIZED_OUTSIDE_BUNDLE"))
							{
								var videoName = videoPlayer.url.Replace("SERIALIZED_OUTSIDE_BUNDLE/", "");
								var clipURL = Path.Combine(songPath, videoName);
								videoPlayer.url = clipURL;
							}
						}

						// puppet handling
						foreach(var trombonePlaceholder in gameObject.GetComponentsInChildren<TrombonerPlaceholder>())
                        {
							int trombonerIndex = trombonePlaceholder.TrombonerType == TrombonerType.DoNotOverride ? __instance.puppetnum : (int)trombonePlaceholder.TrombonerType;
							int tromboneSkinIndex = trombonePlaceholder.TromboneSkin == TromboneSkin.DoNotOverride ? __instance.textureindex : (int)trombonePlaceholder.TromboneSkin;
							// this specific thing could cause problems later but it's fine for now.
							trombonePlaceholder.transform.SetParent(gameObject.transform.GetChild(0));

							foreach(Transform child in trombonePlaceholder.transform)
                            {
								if (child != null) child.gameObject.SetActive(false);
                            }

							var sub = new GameObject();
							sub.transform.SetParent(trombonePlaceholder.transform);
							sub.transform.SetSiblingIndex(0);
							sub.transform.localPosition = new Vector3(-0.7f, 0.45f, -1.25f);
							sub.transform.localEulerAngles = new Vector3(0, 0f, 0f);
							trombonePlaceholder.transform.Rotate(new Vector3(0f, 19f, 0f));
							sub.transform.localScale = Vector3.one;

							//handle male tromboners being slightly shorter
							if(trombonerIndex > 3 && trombonerIndex != 8) sub.transform.localPosition = new Vector3(-0.7f, 0.35f, -1.25f);

							var placeHolder2 = new GameObject("TrombonePlaceHolder");
							placeHolder2.transform.position = trombonePlaceholder.transform.position;
							placeHolder2.transform.eulerAngles = trombonePlaceholder.transform.eulerAngles;
							placeHolder2.transform.localScale = trombonePlaceholder.transform.localScale;

							var tromboneRefs = new GameObject("TromboneTextureRefs");
							tromboneRefs.transform.SetParent(sub.transform);
							tromboneRefs.transform.SetSiblingIndex(0);

							var textureRefs = tromboneRefs.AddComponent<TromboneTextureRefs>();
							textureRefs.trombmaterials = __instance.modelparent.transform.GetChild(0).GetComponent<TromboneTextureRefs>().trombmaterials; // a bit of getchild action to mirror game behaviour

							var trombonerGameObject = Object.Instantiate<GameObject>(__instance.playermodels[trombonerIndex]);

							trombonerGameObject.transform.SetParent(placeHolder2.transform);
							trombonerGameObject.transform.localScale = Vector3.one;

							var reparent = trombonerGameObject.AddComponent<Reparent>();
							reparent.instanceID = trombonePlaceholder.InstanceID;

							Tromboner tromboner = new(trombonerGameObject, trombonePlaceholder);

							Globals.Tromboners.Add(tromboner);

							//LeanTween.scaleY(tromboner.gameObject, 0.01f, 0.01f); 
							tromboner.controller.setTromboneTex(trombonePlaceholder.TromboneSkin == TromboneSkin.DoNotOverride ? __instance.textureindex : (int)trombonePlaceholder.TromboneSkin);

							if (GlobalVariables.localsave.cardcollectionstatus[36] > 9)
							{
								tromboner.controller.show_rainbow = true;
							}
						}

						// very scuffed and temporary, this could probably be completely done on export

						// handle foreground objects
						while (gameObject.transform.GetChild(1).childCount < 8)
						{
							var fillerObject = new GameObject("Filler");
							fillerObject.transform.SetParent(gameObject.transform.GetChild(1));
						}

						// handle two background images
						while (gameObject.transform.GetChild(0).GetComponentsInChildren<SpriteRenderer>().Length < 2)
						{
							var fillerObject = new GameObject("Filler");
							fillerObject.AddComponent<SpriteRenderer>();
							fillerObject.transform.SetParent(gameObject.transform.GetChild(0));
						}

						// move confetti
						gameObjectOld.transform.GetChild(2).SetParent(gameObject.transform);

						// layering
						var breathCanvas = __instance.bottombreath?.transform.parent?.parent?.GetComponent<Canvas>();
						if (breathCanvas != null) breathCanvas.planeDistance = 2;

						var champCanvas = __instance.champcontroller.letters[0]?.transform?.parent?.parent?.parent?.GetComponent<Canvas>();
						if (champCanvas != null) champCanvas.planeDistance = 2;
						
						var gameplayCam = GameObject.Find("GameplayCam")?.GetComponent<Camera>();
						if (gameplayCam != null) gameplayCam.depth = 99;

						var removeDefaultLights = gameObject.transform.Find("RemoveDefaultLights");
						if (removeDefaultLights)
						{
							foreach (var light in GameObject.FindObjectsOfType<Light>()) light.enabled = false;
							removeDefaultLights.gameObject.AddComponent<SceneLightingHelper>();
						}

						var addShadows = gameObject.transform.Find("AddShadows");
						if (addShadows)
						{
							QualitySettings.shadows = ShadowQuality.All;
							QualitySettings.shadowDistance = 100;
						}
					}
				}
			}
			else if (__instance.freeplay)
			{
				gameObject = __instance.myLoadedAssetBundle.LoadAsset<GameObject>("BGCam_freeplay");
			}
			if (gameObject != null)
			{
				GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(gameObject, new Vector3(0f, 0f, 0f), Quaternion.identity, __instance.bgholder.transform);
				gameObject2.transform.localPosition = new Vector3(0f, 0f, 0f);
				__instance.bgcontroller.fullbgobject = gameObject2;
			}
			if (__instance.soundset == 0)
			{
				__instance.currentnotesound.volume = 1f;
			}
			else if (__instance.soundset == 1)
			{
				__instance.currentnotesound.volume = 0.77f;
			}
			else if (__instance.soundset == 2)
			{
				__instance.currentnotesound.volume = 0.34f;
			}
			else if (__instance.soundset == 3)
			{
				__instance.currentnotesound.volume = 0.25f;
			}
			else if (__instance.soundset == 4)
			{
				__instance.currentnotesound.volume = 0.25f;
			}
			else if (__instance.soundset == 5)
			{
				__instance.currentnotesound.volume = 0.75f;
			}
			string[] array = new string[]
			{
			"default",
			"bass",
			"muted",
			"eightbit",
			"club",
			"fart"
			};
			__instance.mySoundAssetBundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/soundpacks/soundpack" + array[__instance.soundset]);
			if (__instance.mySoundAssetBundle == null)
			{
				Plugin.LogDebug("Failed to load sound pack AssetBundle!");
				return false;
			}
			Plugin.LogDebug("LOADED <<<sound pack>>> ASSETBUNDLE");
			UnityEngine.Object.Instantiate<GameObject>(__instance.mySoundAssetBundle.LoadAsset<GameObject>("soundpack" + array[__instance.soundset]), new Vector3(0f, 0f, 0f), Quaternion.identity, __instance.soundSets.transform);
			__instance.StartCoroutine(__instance.loadSoundBundleResources());
			__instance.puppet_human = UnityEngine.Object.Instantiate<GameObject>(__instance.playermodels[__instance.puppetnum], new Vector3(0f, 0f, 0f), Quaternion.identity, __instance.modelparent.transform);
			__instance.puppet_human.transform.localPosition = new Vector3(0.7f, -0.38f, 1.3f);
			if (!__instance.leveleditor && !__instance.freeplay)
			{
				LeanTween.scaleY(__instance.puppet_human, 0.01f, 0.01f);
			}
			if (__instance.freeplay)
			{
				LeanTween.moveLocalX(__instance.puppet_human, 0.4f, 0.01f);
			}
			__instance.puppet_humanc = __instance.puppet_human.GetComponent<HumanPuppetController>();
			if (GlobalVariables.localsave.cardcollectionstatus[36] > 9)
			{
				__instance.puppet_humanc.show_rainbow = true;
			}
			__instance.puppet_humanc.setTromboneTex(__instance.textureindex);
			__instance.topbreathr = __instance.topbreath.GetComponent<RectTransform>();
			__instance.bottombreathr = __instance.bottombreath.GetComponent<RectTransform>();
			__instance.noteholderr = __instance.noteholder.GetComponent<RectTransform>();
			__instance.lyricsholderr = __instance.lyricsholder.GetComponent<RectTransform>();
			__instance.noteparticlesrect = __instance.noteparticles.transform.GetComponent<RectTransform>();
			__instance.leftboundsglow.transform.localScale = new Vector3(0.01f, 1f, 1f);
			for (int i = 0; i < 15; i++)
			{
				__instance.notelines[i] = __instance.notelinesholder.transform.GetChild(i).gameObject;
			}
			for (int j = 0; j < 8; j++)
			{
				GameObject gameObject3 = __instance.notelines[j].gameObject;
				float num = __instance.vbounds / 12f;
				if (j == 0)
				{
					gameObject3.transform.localPosition = new Vector3(0f, __instance.vbounds, 0f);
				}
				else if (j == 1)
				{
					gameObject3.transform.localPosition = new Vector3(0f, __instance.vbounds - num, 0f);
				}
				else if (j == 2)
				{
					gameObject3.transform.localPosition = new Vector3(0f, __instance.vbounds - num * 3f, 0f);
				}
				else if (j == 3)
				{
					gameObject3.transform.localPosition = new Vector3(0f, __instance.vbounds - num * 5f, 0f);
				}
				else if (j == 4)
				{
					gameObject3.transform.localPosition = new Vector3(0f, __instance.vbounds - num * 7f, 0f);
				}
				else if (j == 5)
				{
					gameObject3.transform.localPosition = new Vector3(0f, __instance.vbounds - num * 8f, 0f);
				}
				else if (j == 6)
				{
					gameObject3.transform.localPosition = new Vector3(0f, __instance.vbounds - num * 10f, 0f);
				}
				else if (j == 7)
				{
					gameObject3.transform.localPosition = new Vector3(0f, __instance.vbounds - num * 12f, 0f);
				}
			}
			for (int k = 0; k < 7; k++)
			{
				GameObject gameObject4 = __instance.notelines[k + 8].gameObject;
				float num2 = __instance.vbounds / 12f;
				if (k == 0)
				{
					gameObject4.transform.localPosition = new Vector3(0f, -num2, 0f);
				}
				else if (k == 1)
				{
					gameObject4.transform.localPosition = new Vector3(0f, num2 * -3f, 0f);
				}
				else if (k == 2)
				{
					gameObject4.transform.localPosition = new Vector3(0f, num2 * -5f, 0f);
				}
				else if (k == 3)
				{
					gameObject4.transform.localPosition = new Vector3(0f, num2 * -7f, 0f);
				}
				else if (k == 4)
				{
					gameObject4.transform.localPosition = new Vector3(0f, num2 * -8f, 0f);
				}
				else if (k == 5)
				{
					gameObject4.transform.localPosition = new Vector3(0f, num2 * -10f, 0f);
				}
				else if (k == 6)
				{
					gameObject4.transform.localPosition = new Vector3(0f, num2 * -12f, 0f);
				}
			}
			for (int l = 0; l < 15; l++)
			{
				__instance.notelinepos[l] = __instance.notelines[l].gameObject.transform.localPosition.y;
				__instance.notelines[l] = null;
				UnityEngine.Object.Destroy(__instance.notelines[l]);
			}
			__instance.pointerrect = __instance.pointer.GetComponent<RectTransform>();
			__instance.pointerrect.anchoredPosition3D = new Vector3(__instance.zeroxpos - (float)__instance.dotsize * 0.5f, 0f, 0f);
			__instance.noteparticlesrect.anchoredPosition3D = new Vector3(__instance.zeroxpos, 0f, 0f);
			__instance.leftbounds.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(__instance.zeroxpos, 60f, 0f);
			if (!__instance.leveleditor && !__instance.freeplay)
			{
				__instance.buildLevel();
				__instance.trackmovemult = __instance.tempo / 60f * (float)__instance.defaultnotelength;
				float num3 = __instance.zeroxpos - __instance.noteoffset * -__instance.trackmovemult;
				LeanTween.value(num3 + 1000f, num3, 1.5f).setEaseInOutQuad().setOnUpdate(delegate (float val)
				{
					__instance.noteholderr.anchoredPosition3D = new Vector3(val, 0f, 0f);
					__instance.lyricsholderr.anchoredPosition3D = new Vector3(val, 0f, 0f);
				});
			}
			if (!__instance.leveleditor)
			{
				__instance.editorcanvas.SetActive(false);
			}
			else if (__instance.leveleditor)
			{
				__instance.readytoplay = true;
				__instance.editorcanvas.SetActive(true);
				__instance.buildEditorGUI();
			}
			if (__instance.freeplay)
			{
				__instance.editorcanvas.SetActive(false);
				__instance.healthobj.SetActive(false);
			}
			if (GlobalVariables.testScreenRatio() == 1610)
			{
				__instance.healthobj.transform.localPosition = new Vector3(-5.3f, 4.32f, 10f);
			}
			__instance.pointer.transform.SetAsLastSibling();
			if (!GlobalVariables.localsettings.calibrationscreen)
			{
				__instance.curtainc.doSpotLightAnim();
				if (!__instance.freeplay && !__instance.leveleditor)
				{
					__instance.startSong(true);
				}
			}
			else
			{
				__instance.curtainc.doTutAnimation();
			}
			if (!__instance.freeplay)
			{
				__instance.musictrack = __instance.musicref.GetComponent<AudioSource>();
				return false;
			}
			if (__instance.freeplay)
			{
				__instance.tempo = 40f;
				__instance.Invoke("startDance", 1f);
			}

			return false;
		}

		static void Postfix(GameController __instance)
		{
			
		}

	}

	[HarmonyPatch(typeof(GameController))]
	[HarmonyPatch("tryToLoadLevel")] // if possible use nameof() here
	class GameControllerTryToLoadLevelPatch
	{
		//rewrite of the original
		static bool Prefix(GameController __instance, ref string filename, ref bool customtrack)
		{
			string baseChartName;
			if (filename == "EDITOR")
			{
				baseChartName = Application.streamingAssetsPath + "/leveldata/" + __instance.levelnamefield.text + ".tmb";
			}
			else
			{
				baseChartName = Application.streamingAssetsPath + "/leveldata/" + filename + ".tmb";
			}
			if (!File.Exists(baseChartName))
			{
				Plugin.LogError("File doesnt exist!! Try to load custom song, hijacking Ballgame!!!!!");
				baseChartName = Application.streamingAssetsPath + "/leveldata/ballgame.tmb";
				Plugin.LogDebug("Loading Chart:" + baseChartName);
				Plugin.LogDebug("NYX: HERE WE HOOK OUR CUSTOM CHART!!!!!!!!!!!");
				customtrack = true;
			}
			if (File.Exists(baseChartName))
			{
				Plugin.LogDebug("found level");

				BinaryFormatter binaryFormatter = new BinaryFormatter();
				FileStream fileStream = File.Open(baseChartName, FileMode.Open);
				SavedLevel savedLevel = (SavedLevel)binaryFormatter.Deserialize(fileStream);
				fileStream.Close();
				if (!customtrack)
				{
					Plugin.LogDebug("NYX: Printing Ingame Chart!!!!");
					//Plugin.LogDebug(savedLevel.Serialize().ToString());
				}

				CustomSavedLevel customLevel = new CustomSavedLevel(savedLevel);
				if (customtrack)
				{
					string customChartPath = Path.Combine(Globals.ChartFolders[filename], "song.tmb");
					Plugin.LogDebug("Loading Chart from:" + customChartPath); 

					string jsonString = File.ReadAllText(customChartPath);
					var jsonObject = JSON.Parse(jsonString);
					customLevel.Deserialize(jsonObject);
				}
				__instance.bgdata.Clear();
				__instance.bgdata = customLevel.bgdata;
				__instance.leveldata.Clear();
				__instance.leveldata = customLevel.savedleveldata;
				__instance.lyricdata_pos = customLevel.lyricspos;
				__instance.lyricdata_txt = customLevel.lyricstxt;

				//Debug.Log("Nyx: Serialize Custom level to get lyrics");
				//File.WriteAllText(customChartPath+".withlyrics", customLevel.Serialize().ToString());

				if (customLevel.note_color_start == null)
				{
					Plugin.LogDebug("no color data :-(");
				}
				else
				{
					__instance.note_c_start = customLevel.note_color_start;
					__instance.note_c_end = customLevel.note_color_end;
					if (__instance.leveleditor)
					{
						__instance.col_r_1.text = __instance.note_c_start[0].ToString();
						__instance.col_g_1.text = __instance.note_c_start[1].ToString();
						__instance.col_b_1.text = __instance.note_c_start[2].ToString();
						__instance.col_r_2.text = __instance.note_c_end[0].ToString();
						__instance.col_g_2.text = __instance.note_c_end[1].ToString();
						__instance.col_b_2.text = __instance.note_c_end[2].ToString();
						Plugin.LogDebug(__instance.col_r_1.text + __instance.col_g_1.text + __instance.col_b_1.text);
					}
				}
				__instance.levelendpoint = customLevel.endpoint;
				__instance.editorendpostext.text = "end: " + __instance.levelendpoint;
				__instance.tempo = customLevel.tempo;
				__instance.defaultnotelength = customLevel.savednotespacing;
				__instance.defaultnotelength = Mathf.FloorToInt((float)__instance.defaultnotelength * GlobalVariables.gamescrollspeed);
				__instance.beatspermeasure = customLevel.timesig;
				if (__instance.leveleditor)
				{
					__instance.buildAllBGNodes();
				}
				__instance.buildNotes();
				__instance.buildAllLyrics();
				__instance.changeEditorTempo(0);
				__instance.moveTimeline(0);
				__instance.changeTimeSig(0);
				__instance.levelendtime = 60f / __instance.tempo * __instance.levelendpoint;
				
				BGControllerPatch.BGEffect = customLevel.backgroundMovement;

				var modelCam = GameObject.Find("3dModelCamera")?.GetComponent<Camera>();
				if (modelCam != null) modelCam.clearFlags = CameraClearFlags.Depth;

				Plugin.LogDebug("Level end TIME: " + __instance.levelendtime);
				Plugin.LogDebug("Level Loaded!!");

				return false;
			}
			Plugin.LogDebug("No file exists at that filename!");
			return false;
		}
	}
}
