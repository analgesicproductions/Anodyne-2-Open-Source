using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.IO;
using System.Xml;
#if !DISABLESTEAMWORKS
//using Steamworks;
#endif

public class DataLoader : MonoBehaviour {
	public static DataLoader instance;

	public List<DialogData> dialogDatas;
	Dictionary<string,int> dialogState;
    Dictionary<string, List<int>> dialogReadState;

	public bool autoSet = true;
	public string autoSetTo1_1 = "";
	public string autoSetTo1_2 = "";
	public string autoSetTo1_3 = "";
	public string autoSetTo1_4 = "";
	public string autoSetTo1_5 = "";
	public string autoSetTo1_6 = "";

    public Registry.ProgressVal progressSkip = Registry.ProgressVal.NONE;
    
    public TMPro.TMP_FontAsset Font_ru_kalam;
    public TMPro.TMP_FontAsset Font_ru_2D;
    public TMPro.TMP_FontAsset Font_ru_kreon;
    
    public TMPro.TMP_FontAsset Font_JpAreaName;
    public TMPro.TMP_FontAsset Font_JpKreon;
    public TMPro.TMP_FontAsset Font_JpForum;
    public TMPro.TMP_FontAsset Font_Jp2D;
    public TMPro.TMP_FontAsset Font_JpLiberationSans;
    public TMPro.TMP_FontAsset Font_JpKalam;
    public TMPro.TMP_FontAsset Font_ChineseForum;
    public TMPro.TMP_FontAsset Font_ChineseKreon;
    public TMPro.TMP_FontAsset Font_Chinese2D;

    public TMPro.TMP_FontAsset Font_TradChineseForum;
    public TMPro.TMP_FontAsset Font_TradChineseKreon;
    public TMPro.TMP_FontAsset Font_TradChinese2D;
    
    public TMPro.TMP_FontAsset Font_LiberationSans;
    public TMPro.TMP_FontAsset Font_2D_Euro;

    void Awake () {
        if (Application.isEditor) {
            Registry.DEV_MODE_ON = true;
        }
        if (Registry.DEV_MODE_ON) {
            print("Warning: Dev mode on.");
        } else {
            Cursor.visible = false;
        }
        if (instance != null ) {
			Destroy(gameObject);
            return;
		} else {
			instance = this;
            //GameObject g = new GameObject("SteamManager");
            //g.AddComponent<SteamManager>();

		}

		DontDestroyOnLoad(this);

		SceneManager.sceneLoaded += OnFirstSceneLoaded;
		getSceneSpecificRefs();

		SaveManager.winResX = (int) Screen.currentResolution.width;
		SaveManager.winResY = (int) Screen.currentResolution.height;
		SaveManager.fullscreen = Screen.fullScreen;

        if (SaveManager.GetRecentFileIndex() == -1) {
            SystemLanguage lang = Application.systemLanguage;
            if (lang == SystemLanguage.ChineseSimplified) {
                SaveManager.language = "zh-simp";
            } else if (lang == SystemLanguage.ChineseTraditional) {
                SaveManager.language = "zh-trad";
            } else if (lang == SystemLanguage.Portuguese) {
                SaveManager.language = "pt-br";
            } else if (lang == SystemLanguage.German) {
                SaveManager.language = "de";
            } else if (lang == SystemLanguage.Spanish) {
                SaveManager.language = "es";
            } else if (lang == SystemLanguage.French) {
                SaveManager.language = "fr";
            } else if (lang == SystemLanguage.Japanese) {
                SaveManager.language = "jp";
            } else if (lang == SystemLanguage.Russian) {
                SaveManager.language = "ru";
            } else {
                SaveManager.language = "en";
            }
            print("Setting default language during first startup to " + SaveManager.language);
        }
        //SaveManager.language = "ru"; // remove

        DialogHolder dialogHolder = DialogHolder.Load ("Dialogue/Dialogue Data");
		dialogDatas = dialogHolder.DialogDatas;
        needToRefreshFont = true;

		init_dialogState();

		if (autoSet && Registry.DEV_MODE_ON) {
			if (autoSetTo1_1 != "") {
                print("<color=red>Using autoset flag</color>");
				setDS(autoSetTo1_1,1);
            }
			if (autoSetTo1_2 != "") {
                print("<color=red>Using autoset flag</color>");
				setDS(autoSetTo1_2,1);
            }
            if (autoSetTo1_3 != "") {
                print("<color=red>Using autoset flag</color>");
                setDS(autoSetTo1_3, 1);
            }
            if (autoSetTo1_4 != "") {
                print("<color=red>Using autoset flag</color>");
                setDS(autoSetTo1_4, 1);
            }
            if (autoSetTo1_5 != "") {
                print("<color=red>Using autoset flag</color>");
                setDS(autoSetTo1_5, 1);
            }
            if (autoSetTo1_6 != "") {
                print("<color=red>Using autoset flag</color>");
                setDS(autoSetTo1_6, 1);
            }
        }
        if (Registry.DEV_MODE_ON) {
            if (progressSkip != Registry.ProgressVal.NONE) {
                print("<color=red>WARNING PROGRESSSKIP IS USED</color>");
                Registry.ProgressSkip(progressSkip);
                print("<color=red>WARNING PROGRESSSKIP IS USED</color>");
            }
        }
		
	}


    /// <summary>
    /// Reset game data - atm, just dialogue state.
    /// </summary>
	public void resetGameData() {
        SaveManager.playtime = 0;
        SaveManager.currentHealth = 6;
        SaveManager.healthUpgrades = 0;
        SaveManager.metacoins = 0;
        SaveManager.totalFoundCoins = 0;
        SaveManager.SceneName_To_MinimapVisitedState_Dict = new Dictionary<string, string>();
        init_dialogState();
	}

    public void ReloadDialogueText() {
        DialogHolder dialogHolder = DialogHolder.Load("Dialogue/Dialogue Data");
        dialogDatas = dialogHolder.DialogDatas;
    }

	void init_dialogState() {
        dialogReadState = new Dictionary<string, List<int>>();
		dialogState = new Dictionary<string, int>();
	}

    public void updateDialogReadStateWithString(string s) {
        string[] rawLines = s.Split(new string[] { "\r\n", "\n" }, System.StringSplitOptions.None);

        //scene1|1,2,3,4\n
        //scene2|3\n
        foreach (string part in rawLines) {
            if (part.Length < 3) continue;
            string key = part.Split(new string[] { "|" }, System.StringSplitOptions.None)[0];
            string valueCSV = part.Split(new string[] { "|" }, System.StringSplitOptions.None)[1];
            List<int> l = new List<int>();
            foreach (string value in valueCSV.Split(',')) {
                l.Add(int.Parse(value, System.Globalization.CultureInfo.InvariantCulture));
            }

            
           // string debug = "";
           // foreach (int aa in l) { debug = debug + " " + aa.ToString(); }
           // print(key + " " + debug);
            dialogReadState[key] = l;
        }

    }
    public void updateDialogStateWithString(string s ){

		string[] rawLines = s.Split(new string[]{"\r\n","\n"},System.StringSplitOptions.None);

		foreach (string part in rawLines) {
			if (part.Length < 3) continue;
			string key = part.Split(new string[]{"|"},System.StringSplitOptions.None)[0];
			int value = int.Parse(part.Split(new string[]{"|"},System.StringSplitOptions.None)[1], System.Globalization.CultureInfo.InvariantCulture);
			dialogState[key] = value;
		}

	}


    public string getDialogueReadStateString() {
        string s = "";
        List<int> l = null;
        foreach (string _s in dialogReadState.Keys) {
            l = dialogReadState[_s];
            bool first = true;
            if (l.Count > 0) {
                string csv = "";

                foreach (int i in l) {
                    if (first) {
                        csv += i.ToString();
                    } else {
                        csv += "," + i.ToString();
                    }
                    first = false;
                }
                s += _s + "|" + csv + "\n";

            }
        }
        //print(s);
        return s;
    }

    public string getDialogueStateString() {
		string s = "";
		foreach (string _s in dialogState.Keys) {
			s += _s+"|"+dialogState[_s].ToString()+"\n";
		}
		return s;
	}


	public string getLine(string key, int index) {
		string s= getRaw(key,index);
		s = replaceTags(s);
		return s;
	}
	public string getRaw(string key, int index) {

		foreach (DialogData d in dialogDatas) {
			if (d.Name == key) {
				for (int i = 0; i < d.lines.Count; i++) {
					if (i == index) {
						LineData ld = d.lines[i];
						return ld.Dialog;
					}
				}
			}
		}

		return "No Dialogue "+key+" index "+index.ToString();
	}

    public string getDialogLine(string key, int onlythisline=-1) {
        return getDialogLines(key, onlythisline)[0];
    }

	public List<List<string>> lastDialogTags;
	public string[] getDialogLines(string key, int onlythisline=-1) {
		List<string> a = new List<string>();
		lastDialogTags = new List<List<string>>();

        int idx = 0;
		foreach (DialogData d in dialogDatas) {
			if (d.Name == key) {
				foreach (LineData ld in d.lines) {
                    if (onlythisline != -1 && idx != onlythisline) {
                        idx++; continue;
                    }
					List<string> metadata = new List<string>();
					if (ld.auto) metadata.Add("auto");
                    if (ld.Color != "") {
                        metadata.Add("color");
                        metadata.Add(ld.Color);
                    }
                    if (ld.SpeakerName != "") {
                        metadata.Add("speaker");
                        metadata.Add(ld.SpeakerName);
                    }
					lastDialogTags.Add(metadata);

					string replaced = replaceTags(ld.Dialog);
                    replaced = replaced.Replace("><", "> <");
					a.Add(replaced);
                    idx++;
				}
				return a.ToArray();
			}
		}
		Debug.Log("no scene: "+key);
		lastDialogTags.Add(new List<string>{"no dialogue"});	
		return new string[]{"No dialogue"};
	}


    public string[] getConstructedDialogueLines(string text,int color=0) {
        List<string> a = new List<string>();
        lastDialogTags = new List<List<string>>();
        List<string> metadata = new List<string>();
        if (color != 0) {
            metadata.Add("color");
            metadata.Add(color.ToString());
        }
        lastDialogTags.Add(metadata);
        a.Add(text);
        return a.ToArray();
    }

    public string replaceTags(string s) {
		s = MyInput.replaceTags(s);
		return s;
	}

    public static void _setDS(string key, int val) {
        instance.setDS(key, val);
    }
    public static int _getDS(string key) {
        return instance.getDS(key);
    }
    public bool existsDS(string key) {
        if (key.IndexOf(' ') != -1) print("<color=red>WARNING</color>key " + key + " has space");
        key = key.Replace(' ', '_');
        return (dialogState.ContainsKey(key));
    }
	public int getDS(string key) {
        if (key.IndexOf(' ') != -1) print("<color=red>WARNING</color>key " + key + " has space");
        key = key.Replace(' ', '_');
		if (dialogState.ContainsKey(key) == false) {
			dialogState.Add(key,0);
		}
//		print ("checking: "+key+" is "+dialogState[key].ToString());
		return dialogState[key];
	}
    public bool silenceDSFlagsOnce = false;
	public void setDS(string key, int value) {
        if (key.IndexOf(' ') != -1) print("<color=red>WARNING</color>key " + key + " has space");
        key = key.Replace(' ', '_');
        if (key == "") return;
        if (key == "read-a-metainfo" && value == 1) unlockAchievement(achievement_id_MCTALK);
        dialogState[key] = value;

		if (!silenceDSFlagsOnce) print("<color=green>"+key+" set to "+value.ToString()+"</color>");
        silenceDSFlagsOnce = false;
	}

    public bool hasLineBeenRead(string key, int index) {
        if (key.IndexOf(' ') != -1) print("<color=red>WARNING</color>key " + key + " has space");
        if (dialogReadState.ContainsKey(key) && dialogReadState[key].Contains(index)) {
            return true;
        }
        return false;
    }
    public void updateReadDialogueState(string key, int start, int end) {
        if (key.IndexOf(' ') != -1) print("<color=red>WARNING</color>key " + key + " has space");
        if (dialogReadState.ContainsKey(key) == false) {
            dialogReadState.Add(key, new List<int>());
        }
        List<int> l = dialogReadState[key];
        for (int i = start; i <= end; i++) {
            if (!l.Contains(i)) {
                l.Add(i);
            }
        }
        dialogReadState[key] = l;
        //string debug = "";
        //foreach (int aa in l) { debug = debug + " " + aa.ToString(); }
        //print("Read state of " + key + " now " + debug);
    }
	// Called from a script on the saveModule GOs on the UI prefab.
	// Tells UIMAnager to do the fading, and loads the next scene.
	public void enterNextSceneBasedOnLoadedData() {
		if (Registry.enterGameFromLoad_SceneName != "Title") UIManagerAno2.FadeInOnNextSceneEnter = true;
		StartCoroutine(LevelLoad(Registry.enterGameFromLoad_SceneName));
        SetupSceneChangeFades();
	}

    // 2019-05 bc i see myself here a lot
    // the fade time is ALWAYS the default when entering a scene. hte fade time can be changed through DA2 when leaving a scene
    // so far htere's been no need to change the fade time for entering, except in 2D (see horror)
    // likewise ,pixelize time is always set to 1 for 3D, though 2D will inherit the pixelize time. 
    void SetupSceneChangeFades(float fadeTime=1.4f, float pixelizeTime=1f) {

        lastFadeTime = fadeTime;
        lastPixelizeTime = pixelizeTime;
        if (SceneManager.GetActiveScene().name == "Title") {
            // bc of dumb naming hack
            TitleScreen t = GameObject.Find("Dialogue").GetComponent<TitleScreen>();
            t.doExitFade();
        } else {
            if (GameObject.Find("2D UI") == null) {
                UIManagerAno2 ui = GameObject.Find("3D UI").GetComponent<UIManagerAno2>();
                if (fadeTime > 0) {
                    ui.fadeMode = 1;
                    ui.fadeTime = fadeTime;
                }
                GameObject.Find("UI Camera").GetComponent<PixelizePPE>().Pixelize(pixelizeTime);
            } else {
                UIManager2D ui = GameObject.Find("2D UI").GetComponent<UIManager2D>();
                ui.StartFade(new Color(0, 0, 0), 0, 1, fadeTime);
                GameObject.Find("UI Camera 2D").GetComponent<PixelizePPE>().Pixelize(pixelizeTime);
            }
        }
    }

    public void enterScene(string destname, Registry.GameScenes scene, float fadeTime = 1.4f, float pixelizeTime = 1f) {
        UIManagerAno2.FadeInOnNextSceneEnter = true;
        Registry.destinationDoorName = destname;
        //print("Next scene destination set to " + Registry.destinationDoorName);
        StartCoroutine(LevelLoad(scene.ToString(),Mathf.Max(fadeTime,pixelizeTime)));
        SetupSceneChangeFades(fadeTime, pixelizeTime);
    }
    public void enterScene(string destname, string scene, float fadeTime=1.4f, float pixelizeTime=1f) {
       if (scene != "Title") UIManagerAno2.FadeInOnNextSceneEnter = true;
        Registry.destinationDoorName = destname;
        StartCoroutine(LevelLoad(scene, Mathf.Max(fadeTime, pixelizeTime)));
        SetupSceneChangeFades(fadeTime,pixelizeTime);
    }

    public static float lastFadeTime = 1f;
    public static float lastPixelizeTime = 1f;
    public bool isChangingScenes = false;

    // Waits and then loads the scene.
    IEnumerator LevelLoad(string name, float fadeTime=1.4f) {

        DataLoader.instance.isChangingScenes = true;

		yield return new WaitForSeconds(fadeTime+0.1f);
        SceneManager.sceneLoaded += OnLevelLoaded;
        if (name == "Title") {
            Registry.resetStatics();
        }
        SceneManager.LoadScene(name);
	}

    public static void OnLevelLoaded(Scene scene, LoadSceneMode mode) {
        instance.noPauseOnSceneEnterTicks = 30;
        if (Ano2Stats.prismCurrentDust + Ano2Stats.dust >= 350 && Ano2Stats.CountTotalCards() >= 24) {
            instance.unlockAchievement(achievement_id_ReadyForAnodyne);
        }
        SaveModule.saveMenuOpen = false;
        DataLoader.instance.getSceneSpecificRefs();
        AudioHelper.instance.doSceneCrossfade = true;
        CameraTrigger.prevVCName = "";
        DataLoader.instance.isChangingScenes = false;
        CutsceneManager.ActivateCamAndPlayer();
        TilemetaManager.instance.refreshGrid();
        DataLoader.instance.SetTerrainQuality(SaveManager.terrainQuality);
        SceneManager.sceneLoaded -= OnLevelLoaded;
        DataLoader.instance.SetShadowQuality(SaveManager.shadowQuality);

        Metacoin.ResetStatics();

        string curScene = SceneManager.GetActiveScene().name;
        if (curScene == "RingCCC") {
            instance.unlockAchievement(achievement_id_RING);
        } else if (curScene == "DesertShore" || curScene == "DesertField" || curScene == "DesertOrb") {
            instance.unlockAchievement(achievement_id_DESERT);
        } else if (curScene == "GrowthChapel") {
            instance.unlockAchievement(achievement_id_INTRO);
        } else if (curScene == "DesertSpireCave") {
            instance.unlockAchievement(achievement_id_SPIRECAVE);
        } else if (curScene == "NanoAlbumen") {
            instance.unlockAchievement(achievement_id_SHRINK);
        }

        if (GameObject.Find("2D UI") != null) {
            UIManager2D ui = GameObject.Find("2D UI").GetComponent<UIManager2D>();
            ui.QueueColorlessFade(1, 0, lastFadeTime);
            GameObject.Find("UI Camera 2D").GetComponent<PixelizePPE>().Unpixelize(lastPixelizeTime);
        } else if (GameObject.Find("MedBigCam") != null) {
            if (SceneManager.GetActiveScene().name == "Wormhole2D") {
                GameObject.Find("UI Camera").GetComponent<PixelizePPE>().Unpixelize(lastPixelizeTime);
            } else {
                GameObject.Find("UI Camera").GetComponent<PixelizePPE>().Unpixelize(1f);
            }
        }
        instance.enFontDict.Clear();
        instance.needToRefreshFont = true;
    }

    void getSceneSpecificRefs() {
		go = GameObject.Find("PauseMenu");
        if (go != null) {
            pm = go.GetComponent<PauseMenu>();
        }
        isTitle = false;
        if (SceneManager.GetActiveScene().name == "Title") isTitle = true;

    }

	void OnFirstSceneLoaded(Scene scene, LoadSceneMode mode) {
		getSceneSpecificRefs();
	}

    public void StartPauseTutorial() {
        pm.StartTutorialCutscene();
    }

    public void StartDustDeposit(int dtd, int shd, int spd) {
        pm.StartDustDeposit(dtd, shd, spd);
    }

    public bool IsPauseTutorialDone() {
        return pm.IsPauseTutorialDone();
    }
    public bool IsDustDepositDone() {
        return pm.IsDustDepositDone();
    }

    public bool isTitle = false;
	public bool isPaused;
    [System.NonSerialized]
	public PauseMenu pm;
	GameObject go;
    float tSec = 0;
	public void Update() {
        tSec += Time.deltaTime;
        if (tSec >= 1) {
            tSec -= 1;
            SaveManager.playtime++;
        }

        if (MyInput.jpHome) {
            MyInput.jpHome = false;
            /*if (SteamManager.Initialized) {
                Steamworks.SteamFriends.ActivateGameOverlay("Friends");
            }*/
        }
        if (forcePause) {
            forcePause = false;
            MyInput.jpPause = true;
        }
        if (noPauseOnSceneEnterTicks > 0) noPauseOnSceneEnterTicks--;
        if (!isTitle && MyInput.jpPause && noPauseOnSceneEnterTicks <= 0 && pm != null && !DialogueAno2.AnyScriptIsParsing && !MedBigCam.inCinemachineMovieMode && !SaveModule.saveMenuOpen && !CutsceneManager.deactivatePlayer && !DataLoader.instance.isChangingScenes) {
            if (GameObject.Find(Registry.PLAYERNAME2D) != null) {
                AnoControl2D ano2d = null;
                HF.GetPlayer(ref ano2d);
                if (HF.Get2DUI() != null && HF.Get2DUI().IsSparkBarVisible()) return;
                if (ano2d.pausedBySparkBar) return;
                if (ano2d.IsDying()) return;
            } else if (GameObject.Find(Registry.PLAYERNAME3D_Walkscale) != null) {
                MediumControl walkscale = null;
                if (HF.Get3DUI() != null && HF.Get3DUI().IsSparkBarVisible()) return;
                HF.GetPlayer(ref walkscale);
                if (walkscale != null && walkscale.pausedBySparkBar) return;
            }
            DialogueBox db = GameObject.Find("Dialogue").GetComponent<DialogueBox>();
            string currentSceneName = SceneManager.GetActiveScene().name;
            if (currentSceneName == "Wormhole" || currentSceneName == "SparkGame" || currentSceneName == "Wormhole2D") {
                // Don't turn on pause screen
            } else if (db.isDialogFinished() && !isPaused) {
				isPaused = !isPaused;
				pm.activate();
			} 
		}

		if (!isTitle && isPaused && pm != null) {
			if (pm.isActive() == false) {
				isPaused = false;
			}
		}

        if (Input.GetKey(KeyCode.D) && Input.GetKey(KeyCode.Alpha3) && Input.GetKeyDown(KeyCode.LeftShift)) {
            Registry.DEV_MODE_ON = !Registry.DEV_MODE_ON;
            print("Debug mode toggled to: " + Registry.DEV_MODE_ON);
        }
        if (Registry.DEV_MODE_ON) {
            if (Input.GetKey(KeyCode.D)  && Input.GetKey(KeyCode.Alpha1)) {

                if (GameObject.Find("2D Ano Player") != null) {
                    Anodyne.DustBar dustbar = GameObject.Find("2D Ano Player").GetComponent<Anodyne.DustBar>();
                    dustbar.AddDust(1);
                }
                Ano2Stats.addDust(1);
            } else if (Input.GetKey(KeyCode.D)  && Input.GetKey(KeyCode.Alpha2)) {
                if (Ano2Stats.dust > 0) {
                    if (GameObject.Find("2D Ano Player") != null) {
                        Anodyne.DustBar dustbar = GameObject.Find("2D Ano Player").GetComponent<Anodyne.DustBar>();
                        dustbar.AddDust(-1);
                    }
                    Ano2Stats.dust--;
                }

            } else if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Alpha1) ) {
                resetAchievements();
            } else if (Input.GetKey(KeyCode.A) && Input.GetKeyDown(KeyCode.Alpha1)) {
                unlockAchievement(1);
            } else if (Input.GetKey(KeyCode.A) && Input.GetKeyDown(KeyCode.Alpha2)) {
                unlockAchievement(2);
                unlockAchievement(3);
                unlockAchievement(4);
                unlockAchievement(5);
                unlockAchievement(6);
            } else if (Input.GetKey(KeyCode.A) && Input.GetKeyDown(KeyCode.Alpha3)) {
                unlockAchievement(7);
                unlockAchievement(8);
                unlockAchievement(9);
                unlockAchievement(10);
                unlockAchievement(11);
                unlockAchievement(12);
                unlockAchievement(13);
                unlockAchievement(14);
            }
        }

        if (Registry.DEV_MODE_ON) {

            if (Input.GetKeyDown(KeyCode.D) && Input.GetKey(KeyCode.LeftShift)) {
                if (isChangingScenes == false) {
                    CutsceneManager.deactivatePlayer = false;
                    DialogueAno2.AnyScriptIsParsing = false;
                    enterScene("Entrance", "Debug2D",0.1f,0.1f);
                }
            }

            if (Input.GetKeyDown(KeyCode.LeftShift)) {
                //StartTutorialCutscene();
                //RefreshFonts();
            }
        }
        if (needToRefreshFont) {
            needToRefreshFont = false;
            ReloadDialogueText();
            RefreshFonts();
        }

    }

    public void SetTerrainQuality(int level) {
        List<Terrain> tl = new List<Terrain>();
        GameObject g = GameObject.Find("Terrain");
        if (g != null && g.GetComponent<Terrain>() != null) tl.Add(g.GetComponent<Terrain>());
        g = GameObject.Find("Terrain2");
        if (g != null && g.GetComponent<Terrain>() != null) tl.Add(g.GetComponent<Terrain>());
        level -= 1;
        foreach (Terrain t in tl) {
            switch (level) {
                case 4:
                    t.heightmapPixelError = 5;
                    t.castShadows = true;
                    break;
                case 3:
                    t.heightmapPixelError = 8;
                    t.castShadows = true;
                    break;
                case 2:
                    t.heightmapPixelError = 12;
                    t.castShadows = true;
                    break;
                case 1:
                    t.heightmapPixelError = 12;
                    t.castShadows = false;
                    break;
                case 0:
                    t.heightmapPixelError = 100;
                    t.castShadows = false;
                    break;
            }
        }
    }

    public void SetShadowQuality(int level) {
        if (level <= 0) {
            QualitySettings.shadows = ShadowQuality.Disable;
            return;
        }

        Light[] lights = (Light[])GameObject.FindObjectsOfType(typeof(Light));
        QualitySettings.shadows = ShadowQuality.All;
        foreach (Light l in lights) {
            if (level == 1) {
                l.shadowResolution = UnityEngine.Rendering.LightShadowResolution.Medium;
            } else if (level == 2) {
                l.shadowResolution = UnityEngine.Rendering.LightShadowResolution.High;
            } else {
                l.shadowResolution = UnityEngine.Rendering.LightShadowResolution.VeryHigh;
            }
        }
    }
    bool needToRefreshFont = false;
    Dictionary<string, TMP_FontAsset> enFontDict = new Dictionary<string, TMP_FontAsset>();
    public void RefreshFonts() {
        List<TMP_Text[]> texts = new List<TMP_Text[]>();
        if (pm != null) {
            pm.GetTMP_Arrays(ref texts);
        }

        // do pause menu separately
        foreach (TMP_Text[] pm_texts in texts) {
            foreach (TMP_Text txt in pm_texts) {
                if (enFontDict.ContainsKey(txt.name) == false) {
                    enFontDict.Add(txt.name, txt.font);
                }
                if (SaveManager.language == "zh-simp") {
                    txt.font = DataLoader.instance.Font_ChineseKreon;
                    if (txt.name == "PauseText") {
                        txt.fontSize = 11f * (9.2f / 7.4f);
                        txt.lineSpacing = 0f;
                    } else if (txt.name == "MetaChoices" || txt.name == "MetaCosts") {
                        txt.fontSize = 12.4f;
                        txt.lineSpacing = 9.6f;
                    }
                } else if (SaveManager.language == "zh-trad") {
                    txt.font = DataLoader.instance.Font_TradChineseKreon;
                    if (txt.name == "PauseText") {
                        txt.fontSize = 11f * (9.2f / 7.4f);
                        txt.lineSpacing = 0f;
                    } else if (txt.name == "MetaChoices" || txt.name == "MetaCosts") {
                        txt.fontSize = 12.4f;
                        txt.lineSpacing = 9.6f;
                    }
                } else if (SaveManager.language == "jp") {
                    txt.font = instance.Font_JpKreon;
                    if (txt.name == "PauseText") {
                        txt.fontSize = 11f; 
                        txt.lineSpacing = -6.3f;
                    } else if (txt.name == "MetaChoices" || txt.name == "MetaCosts") {
                        txt.fontSize = 10.1f; 
                        txt.lineSpacing = 0f;
                    }
                } else if (SaveManager.language == "ru") {
                    txt.font = instance.Font_ru_kreon;
                    if (txt.name == "PauseText") {
                        txt.fontSize = 11f; 
                        txt.lineSpacing = 0;
                    } else if (txt.name == "MetaChoices" || txt.name == "MetaCosts") {
                        txt.fontSize = 8.4f; 
                        txt.lineSpacing = 34.5f;
                    }
                } else {
                    txt.font = enFontDict[txt.name];
                    if (txt.name == "PauseText") {
                        txt.fontSize = 11f;
                        txt.lineSpacing = 0f;
                    } else if (txt.name == "MetaChoices" || txt.name == "MetaCosts") {
                        txt.fontSize = 12.4f;
                        txt.lineSpacing = 0;
                    }
                }
            }
        }
        texts.Clear();

        bool is2D = false;
        if (GameObject.Find("2D UI") != null) {
            texts.Add(GameObject.Find("2D UI").GetComponentsInChildren<TMP_Text>());
            is2D = true;
        }
        if (GameObject.Find("3D UI") != null) {
            texts.Add(GameObject.Find("3D UI").GetComponentsInChildren<TMP_Text>());
        }

        if (GameObject.Find("Dialogue") != null) {
            texts.Add(GameObject.Find("Dialogue").GetComponentsInChildren<TMP_Text>());
        }

        // Adds namecards + credits
        if (GameObject.Find("NameCardZera") != null || GameObject.Find("NameCardAlb") != null) {
            if (SaveManager.language == "zh-simp" || SaveManager.language == "zh-trad" || SaveManager.language == "jp" || SaveManager.language == "ru") {
                if (SaveManager.language == "zh-simp" || SaveManager.language == "zh-trad") {
                    if (GameObject.Find("NameCardZera") != null) texts.Add(GameObject.Find("NameCardZera").GetComponentsInChildren<TMP_Text>());
                    if (GameObject.Find("NameCardAlb") != null) texts.Add(GameObject.Find("NameCardAlb").GetComponentsInChildren<TMP_Text>());

                    GameObject.Find("namecard_desc2").GetComponent<TMP_Text>().lineSpacing = 0;
                    GameObject.Find("namecard_desc1").GetComponent<TMP_Text>().lineSpacing = 0;
                    GameObject.Find("namecard_name1").GetComponent<TMP_Text>().lineSpacing = 0;
                    GameObject.Find("namecard_name2").GetComponent<TMP_Text>().lineSpacing = 0;
                    GameObject.Find("namecard_caption1").GetComponent<TMP_Text>().lineSpacing = 0;
                    GameObject.Find("namecard_desc2").GetComponent<TMP_Text>().characterSpacing = 0;
                    GameObject.Find("namecard_desc1").GetComponent<TMP_Text>().characterSpacing = 0;
                    GameObject.Find("namecard_name1").GetComponent<TMP_Text>().characterSpacing = 0;
                    GameObject.Find("namecard_name2").GetComponent<TMP_Text>().characterSpacing = 0;
                    GameObject.Find("namecard_caption1").GetComponent<TMP_Text>().characterSpacing = 0;
                } else if (SaveManager.language == "jp") {
                    TMP_Text[] jpTexts = null;
                    if (GameObject.Find("NameCardZera") != null) jpTexts= GameObject.Find("NameCardZera").GetComponentsInChildren<TMP_Text>();
                    if (GameObject.Find("NameCardAlb") != null) jpTexts = GameObject.Find("NameCardAlb").GetComponentsInChildren<TMP_Text>();
                    if (jpTexts != null) {
                        foreach (TMP_Text tt in jpTexts) {
                            if (tt.name.Contains("credits") == false) {
                                tt.font = instance.Font_JpKalam;
                                tt.lineSpacing = 0;
                                tt.characterSpacing = 0;
                            } else {
                                tt.font = instance.Font_JpKreon;
                            }
                        }
                    }
                } else if (SaveManager.language == "ru") {
                    TMP_Text[] ru_texts = null;
                    if (GameObject.Find("NameCardZera") != null) ru_texts= GameObject.Find("NameCardZera").GetComponentsInChildren<TMP_Text>();
                    if (GameObject.Find("NameCardAlb") != null) ru_texts = GameObject.Find("NameCardAlb").GetComponentsInChildren<TMP_Text>();
                    if (ru_texts != null) {
                        foreach (TMP_Text tt in ru_texts) {
                            if (tt.name.Contains("credits") == false) {
                                tt.font = instance.Font_ru_kalam;
                                tt.lineSpacing = 0;
                                tt.characterSpacing = 0;
                            } else {
                                tt.font = instance.Font_ru_kreon;
                            }
                        }
                    }
                    
                    GameObject.Find("namecard_name1").GetComponent<TMP_Text>().lineSpacing = -14f;
                    GameObject.Find("namecard_name2").GetComponent<TMP_Text>().lineSpacing = -14f;
                }

            }
        }


        foreach (TMP_Text[] childtexts in texts) {
            foreach (TMP_Text txt in childtexts) {
                // Cleared on scene load. Caches original font assets before changing them
                if (enFontDict.ContainsKey(txt.name) == false) {
                    enFontDict.Add(txt.name, txt.font);
                }
                if (SaveManager.language == "zh-simp") {

                    if (is2D) {
                        txt.font = DataLoader.instance.Font_Chinese2D;
                        if (txt.name == "2DAreaTitle_Center"|| txt.name == "SideText" || txt.name == "2DAreaTitle_Bottom_Lower" || txt.name == "2DAreaTitle_Bottom_Top" || txt.name == "SavingText_Text_Bottom" || txt.name == "SavingText_Text") {
                            txt.fontSize = 9f;
                        }
                    } else {
                        txt.font = DataLoader.instance.Font_ChineseKreon;
                        if (txt.name == "creatortext" || txt.name == "AreaNameText") {
                            txt.font = DataLoader.instance.Font_ChineseForum;
                        } else if (txt.name == "TryAgainText") {
                            txt.lineSpacing = 3f;
                        }
                    }
                } else if (SaveManager.language == "zh-trad") {
                    
                    if (is2D) {
                        txt.font = DataLoader.instance.Font_TradChinese2D;
                        if (txt.name == "2DAreaTitle_Center"|| txt.name == "SideText" || txt.name == "2DAreaTitle_Bottom_Lower" || txt.name == "2DAreaTitle_Bottom_Top" || txt.name == "SavingText_Text_Bottom" || txt.name == "SavingText_Text") {
                            txt.fontSize = 9f;
                        }
                    } else {
                        txt.font = DataLoader.instance.Font_TradChineseKreon;
                        if (txt.name == "creatortext" || txt.name == "AreaNameText") {
                            txt.font = DataLoader.instance.Font_TradChineseForum;
                        } else if (txt.name == "TryAgainText") {
                            txt.lineSpacing = 3f;
                        }
                    }
                } else if (SaveManager.language == "jp") {
                    if (is2D) {
                        txt.font = instance.Font_Jp2D;
                        if (txt.name == "2DAreaTitle_Center" || txt.name == "SideText" || txt.name == "2DAreaTitle_Bottom_Lower" || txt.name == "2DAreaTitle_Bottom_Top" || txt.name == "SavingText_Text_Bottom" || txt.name == "SavingText_Text" || txt.name == "2DGameYesNoText" ) {
                            txt.fontSize = 9f;
                            txt.lineSpacing = 9f; txt.characterSpacing = 0;
                        }

                    } else {
                        txt.font = instance.Font_JpKreon;
                        if (txt.name == "creatortext") {
                            txt.font = DataLoader.instance.Font_JpForum;
                        } else if (txt.name == "AreaNameText" || txt.name == "WormholeArea") {
                            txt.font = DataLoader.instance.Font_JpAreaName;
                        } else if (txt.name == "TryAgainText") {
                            txt.lineSpacing = 3f;
                        }
                    }
                
                } else if (SaveManager.language == "ru") {
                    if (is2D) {
                        txt.font = instance.Font_ru_2D;
                        if (txt.name == "2DAreaTitle_Center" || txt.name == "SideText" || txt.name == "2DAreaTitle_Bottom_Lower" || txt.name == "2DAreaTitle_Bottom_Top" || txt.name == "SavingText_Text_Bottom" || txt.name == "SavingText_Text" || txt.name == "2DGameYesNoText" ) {
                            txt.fontSize = 8f;
                            txt.lineSpacing = 8f; txt.characterSpacing = -2f;
                            if (txt.name == "SideText") {
                                txt.fontSize = 6f;
                            }
                        } else {
                            txt.fontSize = 8f;
                            txt.lineSpacing = 8f; txt.characterSpacing = -2f;
                        }

                    } else {
                        if (txt.name == "creatortext" || txt.name == "AreaNameText" || txt.name == "WormholeArea") {
                            txt.font = enFontDict[txt.name];
                        } else if (txt.name == "TryAgainText") {
                            txt.font = DataLoader.instance.Font_ru_kreon;
                        } else if (txt.name == "text_dialog" || txt.name == "text_skip" || txt.name == "nametext") {
                            txt.font = instance.Font_ru_kreon;
                        } else {
                            if (enFontDict[txt.name].name.IndexOf("Kreon") == 0) {
                                txt.font = instance.Font_ru_kreon;
                            } else {
                                txt.font = enFontDict[txt.name];
                            }
                        }
                    }
                } else if (SaveManager.language == "en") {
                    txt.font = enFontDict[txt.name];
                    if (txt.name == "2DAreaTitle_Bottom_Lower" || txt.name == "2DAreaTitle_Bottom_Top" || txt.name == "SavingText_Text_Bottom" || txt.name == "SavingText_Text") {
                        txt.fontSize = 8f;
                    }
                } else {
                    if (is2D) {
                        txt.font = Font_2D_Euro;
                        txt.fontSize = 16f;
                        if (txt.name == "SideText") {
                            txt.fontSize = 12f;
                        } else if (txt.name == "2DAreaTitle_Bottom_Lower" || txt.name == "2DAreaTitle_Bottom_Top" || txt.name == "SavingText_Text_Bottom" || txt.name == "SavingText_Text") {
                            txt.fontSize = 16f;
                        }
                    } else {
                        txt.font = enFontDict[txt.name];
                    }
                }
            }
        }
        // 2D and 3D dialogue (Fade Text, regular text + underlayer, Skip, name)
        if (GameObject.Find("Dialogue") != null && GameObject.Find("Dialogue").GetComponent<DialogueBox>() != null) {
            GameObject.Find("Dialogue").GetComponent<DialogueBox>().RefreshFonts();
        }

        // Title Screen-only Texts (Save module, ...)
        if (GameObject.Find("SaveMenu_Text") != null) {
            TMP_Text savemenu_text = GameObject.Find("SaveMenu_Text").GetComponent<TMP_Text>();
            if (SaveManager.language == "zh-simp") {
                savemenu_text.font = Font_ChineseKreon;
                savemenu_text.fontSize = 10.5f;
                savemenu_text.lineSpacing = 3f;
                savemenu_text.wordSpacing = 0f;
                savemenu_text.characterSpacing = 0.6f;
            } else if (SaveManager.language == "zh-trad") {
                savemenu_text.font = Font_TradChineseKreon;
                savemenu_text.fontSize = 10.5f;
                savemenu_text.lineSpacing = 3f;
                savemenu_text.wordSpacing = 0f;
                savemenu_text.characterSpacing = 0.6f;
            } else if (SaveManager.language == "jp") {
                savemenu_text.font = Font_JpKreon;
                savemenu_text.fontSize = 10.3f;
                savemenu_text.lineSpacing = -10.6f;
                savemenu_text.wordSpacing = 0f;
                savemenu_text.characterSpacing = 0f;
            }  else if (SaveManager.language == "ru") {
                savemenu_text.font = Font_ru_kreon;
                savemenu_text.fontSize = 9.4f;
                savemenu_text.lineSpacing = -6;
                savemenu_text.wordSpacing = 1.9f;
                savemenu_text.characterSpacing = 0f;
            } else {
                savemenu_text.fontSize = 11f;
                savemenu_text.lineSpacing = -6f;
                savemenu_text.wordSpacing = 1.9f;
                savemenu_text.characterSpacing = 0f;
            }
        }

        // Horror
        if (SceneManager.GetActiveScene().name == "NanoHorror" && GameObject.Find(Registry.PLAYERNAME2D).transform.position.x >= 24f) {
            RefreshHorrorFonts();
        }
    }

    public void RefreshHorrorFonts() {
        if (SaveManager.language != "zh-simp" && SaveManager.language != "zh-trad") {

            if (SaveManager.language == "jp") {
                GameObject.Find("2DGameYesNoText").GetComponent<TMP_Text>().font = DataLoader.instance.Font_JpLiberationSans;
                GameObject.Find("fadetext").GetComponent<TMP_Text>().font = DataLoader.instance.Font_JpLiberationSans;
                GameObject.Find("text_dialog").GetComponent<TMP_Text>().font = DataLoader.instance.Font_JpLiberationSans;
                GameObject.Find("text_dialog_under").GetComponent<TMP_Text>().font = DataLoader.instance.Font_JpLiberationSans;

                GameObject.Find("2DGameYesNoText").GetComponent<TMP_Text>().fontSize = 9f;
                GameObject.Find("fadetext").GetComponent<TMP_Text>().fontSize = 9f;
                GameObject.Find("text_dialog").GetComponent<TMP_Text>().fontSize = 9;
                GameObject.Find("text_dialog_under").GetComponent<TMP_Text>().fontSize = 9f;
            } else {
                GameObject.Find("2DGameYesNoText").GetComponent<TMP_Text>().font = DataLoader.instance.Font_LiberationSans;
                GameObject.Find("fadetext").GetComponent<TMP_Text>().font = DataLoader.instance.Font_LiberationSans;
                GameObject.Find("text_dialog").GetComponent<TMP_Text>().font = DataLoader.instance.Font_LiberationSans;
                GameObject.Find("text_dialog_under").GetComponent<TMP_Text>().font = DataLoader.instance.Font_LiberationSans;

                GameObject.Find("2DGameYesNoText").GetComponent<TMP_Text>().fontSize = 8f;
                GameObject.Find("fadetext").GetComponent<TMP_Text>().fontSize = 8f;
                GameObject.Find("text_dialog").GetComponent<TMP_Text>().fontSize = 8f;
                GameObject.Find("text_dialog_under").GetComponent<TMP_Text>().fontSize = 8f;
            }
        }
    }

    int noPauseOnSceneEnterTicks = 0;


    public static int achievement_id_SHRINK = 1; // done
    public static int achievement_id_INTRO = 2;// done
    public static int achievement_id_RING = 3;// done
    public static int achievement_id_SPIRECAVE = 4;// done
    public static int achievement_id_DESERT = 5;// done - OnLevelLoaded
    public static int achievement_id_GOODEND = 6; // sawGoodEnd
    public static int achievement_id_ReadyForAnodyne = 7; // done - OnLevelLoaded
    public static int achievement_id_MCBUY = 8; // done (pausemenu)
    public static int achievement_id_MCWARP = 9; // done (pausemenu)
    public static int achievement_id_MCTALK = 10; // done (setDS)
    public static int achievement_id_COIN_1 = 11; // done - metacoin
    public static int achievement_id_COIN_200 = 12; // done
    public static int achievement_id_COIN_400 = 13; // done
    public static int achievement_id_COIN_500 = 14; // done


    string achvShrink = "shrink"; // set in scene change in dataloader
    string achvIntro = "intro"; // set in scene change in dataloader
    string achvRing = "ring"; // set in dataoader when entering ringccc
    string achvSpireCave = "spirecave"; // set in dataoader when entering ringccc
    string achvDesert = "desert"; // set in dataloader when entering a desert area
    string achvGoodEnd = "goodend"; // set in credits i guess?
    string achvReadyForAnodyne = "badendready"; 
    string achvBuy = "buy";
    string achvWarp = "warp";
    string achvTalk = "talk";
    string achvCoin1 = "coin1";
    string achvCoin200 = "coin200";
    string achvCoin400 = "coin400";
    string achvCoin500 = "coin500";

    public void resendAchievementStats() {
        string achievementFlagPrefix = "achv";
        if (getDS(achievementFlagPrefix + achvShrink) == 1) { unlockAchievement(achievement_id_SHRINK,true); }
        if (getDS(achievementFlagPrefix + achvIntro) == 1) { unlockAchievement(achievement_id_INTRO, true); }
        if (getDS(achievementFlagPrefix + achvRing) == 1) { unlockAchievement(achievement_id_RING, true); }
        if (getDS(achievementFlagPrefix + achvSpireCave) == 1) { unlockAchievement(achievement_id_SPIRECAVE, true); }
        if (getDS(achievementFlagPrefix + achvDesert) == 1) { unlockAchievement(achievement_id_DESERT, true); }
        if (getDS(achievementFlagPrefix + achvGoodEnd) == 1) { unlockAchievement(achievement_id_GOODEND, true); }
        if (getDS(achievementFlagPrefix + achvReadyForAnodyne) == 1) { unlockAchievement(achievement_id_ReadyForAnodyne, true); }
        if (getDS(achievementFlagPrefix + achvBuy) == 1) { unlockAchievement(achievement_id_MCBUY, true); }
        if (getDS(achievementFlagPrefix + achvWarp) == 1) { unlockAchievement(achievement_id_MCWARP, true); }
        if (getDS(achievementFlagPrefix + achvTalk) == 1) { unlockAchievement(achievement_id_MCTALK, true); }
        if (getDS(achievementFlagPrefix + achvCoin1) == 1) { unlockAchievement(achievement_id_COIN_1, true); }
        if (getDS(achievementFlagPrefix + achvCoin200) == 1) { unlockAchievement(achievement_id_COIN_200, true); }
        if (getDS(achievementFlagPrefix + achvCoin400) == 1) { unlockAchievement(achievement_id_COIN_400, true); }
        if (getDS(achievementFlagPrefix + achvCoin500) == 1) { unlockAchievement(achievement_id_COIN_500, true); }
    }

    public void resetAchievements() {
        string[] achievementNames = new string[] { achvShrink, achvIntro, achvRing, achvSpireCave, achvDesert, achvGoodEnd, achvReadyForAnodyne, achvBuy, achvWarp, achvTalk, achvCoin1, achvCoin200, achvCoin400, achvCoin500 };
        foreach (string achname in achievementNames) {
            setDS("achv" + achname, 0);
            /*
            if (SteamManager.Initialized) {
                Steamworks.SteamUserStats.ClearAchievement(achname);
                Steamworks.SteamUserStats.StoreStats();
            }*/
        }
    }


    public void unlockAchievement(int id, bool forceResend=false) {
        string achievementFlagPrefix = "achv";

        string[] achievementNames = new string[] { achvShrink, achvIntro, achvRing, achvSpireCave, achvDesert, achvGoodEnd, achvReadyForAnodyne, achvBuy, achvWarp, achvTalk, achvCoin1, achvCoin200, achvCoin400, achvCoin500 };
        int[] achvIDs = new int[] { achievement_id_SHRINK, achievement_id_INTRO, achievement_id_RING, achievement_id_SPIRECAVE, achievement_id_DESERT, achievement_id_GOODEND, achievement_id_ReadyForAnodyne, achievement_id_MCBUY, achievement_id_MCWARP, achievement_id_MCTALK, achievement_id_COIN_1, achievement_id_COIN_200, achievement_id_COIN_400, achievement_id_COIN_500 };

        for (int i = 0; i < achievementNames.Length; i++) {
            if (id == achvIDs[i]) {
                if (getDS(achievementFlagPrefix + achievementNames[i]) == 0) {
                    setDS(achievementFlagPrefix + achievementNames[i], 1);
                } else if (!forceResend) {
                    return;
                }
                
                print("Sending stat " + achievementNames[i]);
                /*
                if (SteamManager.Initialized) {
                    Steamworks.SteamUserStats.SetAchievement(achievementNames[i]);
                    Steamworks.SteamUserStats.StoreStats();
                }*/
            }
        }
    }

    private void OnApplicationQuit() {
        if (!Application.isEditor) {
            if (File.Exists(Application.persistentDataPath+"/output_log.txt")) {
                string newname = "";
                newname = Application.persistentDataPath+"/"+ System.DateTime.Now.ToString("MM-dd_HH-mm-ss")+".log";
                File.Copy(Application.persistentDataPath + "/output_log.txt", newname);
            }
        }
    }

    [System.NonSerialized]
    public bool forcePause = false;

/*    protected Steamworks.Callback<Steamworks.GameOverlayActivated_t> m_GameOverlayActivated;

    private void OnEnable() {
        if (SteamManager.Initialized) {
            m_GameOverlayActivated = Steamworks.Callback<Steamworks.GameOverlayActivated_t>.Create(OnGameOverlayActivated);
       }
    }

    private void OnGameOverlayActivated(Steamworks.GameOverlayActivated_t pCallback) {
        if (pCallback.m_bActive != 0) {
            if (!isPaused) {
                forcePause = true;
            }
        } else {
            if (isPaused) {
                forcePause = true;
            }
        }
    }*/

}
