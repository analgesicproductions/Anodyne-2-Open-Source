using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

/*
 * Production notes: Probably don't ever REMOVE stuff from the [serializable] data class - the game will throw an error
 * if it finds PROPERTY_A in a save file but nothing to put it into in the game's data class.
 * On the other hand, if a property is added to the game and a save file doesn't have it, then reading that property
 * from the save file will give the default value of that type (0, None, null, etc)
 * */

// Needed for FileStream and File
using System.IO;

// Needed for BinaryFormatter
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

// Needed for [Serializable]
using System;



// This is a script with two classes, SaveManager and SaveData
// You can modify it for use in your game.
// SaveManager contains functions for saving and loading data, using the Serializable
//		SaveData class as the data to be saved.

public class SaveManager : MonoBehaviour  {

    public static bool dontChangeResOnLoadBCChangedInTitle = false;
	public static float playtime = 0;
    // "COUGHER" -> "11011|11010|00000|"
    public static Dictionary<string, string> SceneName_To_MinimapVisitedState_Dict = new Dictionary<string, string>();

    // Note, in-game map size will dictate these dimensions
    static public void Update_SceneMinimapVisitedState_With_Array(string sceneName, int[,] visitedState) {
        string visitedString = "";
        for (int y = 0; y < visitedState.GetLength(0); y++) {
            for (int x = 0; x < visitedState.GetLength(1); x++) {
                visitedString += visitedState[y, x].ToString();
            }
            if (y < visitedState.GetLength(0) - 1) {
                visitedString += "|";
            }
        }
        if (SceneName_To_MinimapVisitedState_Dict.ContainsKey(sceneName)) {
            SceneName_To_MinimapVisitedState_Dict[sceneName] = visitedString;
        } else {
            SceneName_To_MinimapVisitedState_Dict.Add(sceneName, visitedString);
        }
        //print("Save: " + visitedString);
    }

    // Handles mismatched sizes. if the dict string is too small, the output array will be zero
    // if the dict string is too big, the output array will be zero
    static public int[,] Get_SceneMinimapVisitedState_As_Array(string sceneName, int maxHeight, int maxWidth) {
        int[,] visitedState = new int[maxHeight, maxWidth];
        if (SceneName_To_MinimapVisitedState_Dict.ContainsKey(sceneName)) {
            string visitedstring = SceneName_To_MinimapVisitedState_Dict[sceneName];
            //print("Load: " + visitedstring);
            string[] rows = visitedstring.Split('|');
            int rowIdx = 0;
            foreach (string row in rows) {
                for (int i = 0; i < row.Length; i++) {
                    if (i == maxWidth) break;
                    visitedState[rowIdx, i] = int.Parse(row[i].ToString(), System.Globalization.CultureInfo.InvariantCulture);
                }
                rowIdx++;
                if (rowIdx == maxHeight) break;
            }
        }
        return visitedState;
    }

    // For saving only
    static public string Get_MinimapStateDict_As_String() {
        string s = "";
        foreach (string sceneName in SceneName_To_MinimapVisitedState_Dict.Keys) {
            s += sceneName + ",";
            s += SceneName_To_MinimapVisitedState_Dict[sceneName];
            s += "||";
        }
        return s;
    }

    // for loading only
    static public void Update_MinimapStateDict_With_String(string s) {
        //print("Loaded minimap string: " + s);
        SceneName_To_MinimapVisitedState_Dict = new Dictionary<string, string>();
        string[] sceneParts = s.Split(new string[] {"||"}, StringSplitOptions.RemoveEmptyEntries);
        foreach (string scenePart in sceneParts) {
            string[] lineData = scenePart.Split(',');
            string sceneName = lineData[0];
            string minimapData = lineData[1];
            SceneName_To_MinimapVisitedState_Dict.Add(sceneName, minimapData);
        }
    }

    /*
    static public void TestMinimapStateStuff() {

        // save without visiting 2d
        string res = Get_MinimapStateDict_As_String();
        // load without visitng 2d
        Update_MinimapStateDict_With_String("");
        // cheeck state dict


        int[,] testVisitedData = new int[3, 3] { { 1, 0, 0 }, { 1, 0, 1 }, { 1, 1, 1 } };
        // Update cached data
        Update_MinimapStateDict("Test", testVisitedData);

        testVisitedData = new int[3, 4] { { 1, 1,1,1 }, { 0, 0,0, 1 }, { 0,1, 1, 1 } };

        Update_MinimapStateDict("Banana", testVisitedData);
        // "save" to disk
        string saveString = Get_MinimapStateDict_As_String();
        // "load" from disk
        Update_MinimapStateDict_With_String(saveString);
        // Load into uimanager
        int[,] testOutput = MinimapStateDict_To_Array("Test", 3, 3);
        print(testOutput);
        int[,] testOutputTooSmall = MinimapStateDict_To_Array("Banana", 2, 2);
        print(testOutputTooSmall);
        int[,] testOutputTooBig = MinimapStateDict_To_Array("Banana", 5,5);
        print(testOutputTooBig);

        int[,] testemptyOutput = MinimapStateDict_To_Array("EmptyScene", 4, 5);
        print(testemptyOutput);
    }
    */


    static public int _SaveRecent() {
        int fileindex = GetRecentFileIndex();
        if (fileindex != -1) {
            print("Saving to recent file #" + fileindex.ToString());
            _Save(fileindex);
            return fileindex;
        }
        print("No recent file. Saving to file 0.");
        _Save(0);
        return 0;
    }
    static public bool forceCreditsData = false;
	static public bool _Save(int filenumber) {



		// Create an instance of SaveData, fill out its properties.
		SaveData data = new SaveData ();
        bool HasPos = false;
        if (GameObject.Find("MediumPlayer") != null) {
            HasPos = true;
            data.position = new SaveVector3(GameObject.Find("MediumPlayer").transform.position);
        } else if (GameObject.Find("2D Ano Player") != null) {
            HasPos = true;
            data.position = new SaveVector3(GameObject.Find("2D Ano Player").transform.position);
            GameObject.Find("2D Ano Player").GetComponent<AnoControl2D>().SetCheckpointPos();
        }
        if (!HasPos) {
            data.position = new SaveVector3(new Vector3(0, 0, 0));
        }


        Ano2Stats.saveStatsToFlags();
		data.flags = DataLoader.instance.getDialogueStateString();
        data.readState = DataLoader.instance.getDialogueReadStateString();
		data.sceneName = SceneManager.GetActiveScene().name;
		data.playtime = (int) playtime;


        if (forceCreditsData) {
            data.position = new SaveVector3(new Vector3(0, 268, 28));
            data.sceneName = "RingCCC";
        }

        string infoo = MyInput.getTimeString((int)SaveManager.playtime);
        Vector3 savedpos = data.position.toVector3();
        print(data.sceneName + " " + infoo + " " + savedpos.x + "," + savedpos.y + "," + savedpos.z);
        data.infiniteFly = infiniteFly;
        data.doubleHealth = doubleHealth;
        data.invincibility = invincibility;
        data.currentHealth = currentHealth;
        data.healthUpgrades = healthUpgrades;
        data.metacoins = metacoins;
        data.totalFoundCoins = totalFoundCoins;
        data.cardsFound = Ano2Stats.CountTotalCards(false);
        data.minimapState = Get_MinimapStateDict_As_String();
        if (SavePoint.AutoSaveOn2D) {
            data.autosaveOn2D = 1;
        } else {
            data.autosaveOn2D = 2;
        }

		ConfigData config = new ConfigData();
		config.brightness = brightness;
		config.volume = volume;
		config.language = language;
		config.invertX = MyInput.invertX;
		config.invertY = MyInput.invertY;
        config.useMouse = MyInput.useMouse;
		config.fullscreen = fullscreen;
        if (winResX == 0) {
            config.winResX = Screen.currentResolution.width;
        } else {
            config.winResX = winResX;
        }
        if (winResY == 0) {
            config.winResY = Screen.currentResolution.height;
        } else {
            config.winResY = winResY;
        }

        print("Saved res: "+config.winResX + "," + config.winResY);

		config.sensitivity = sensitivity;
        config.fieldOfView = fieldOfView;
        config.cameraDistance = Mathf.FloorToInt(cameraDistance);
        config.controllerDisable = controllerDisable;
        config.dialogueSkip = dialogueSkip;
        config.shadowQuality = shadowQuality;
        config.terrainQuality = terrainQuality;
        config.screenshake = screenshake;

		config.keyUp = MyInput.KC_up; 
		config.keyRight = MyInput.KC_right; 
		config.keyDown = MyInput.KC_down; 
		config.keyLeft = MyInput.KC_left; 
		config.keyCancel= MyInput.KC_cancel; 
		config.keyJump= MyInput.KC_jump;
        config.keyTalk = MyInput.KC_talk;
		config.keyPause= MyInput.KC_pause;
        config.keyCamToggle = MyInput.KC_camtoggle;
        config.keyNano = MyInput.KC_special;
        config.keyChangeSize = MyInput.KC_toggleRidescale;

        config.keyZoomIn= MyInput.KC_zoomIn;
        config.keyZoomOut = MyInput.KC_zoomOut;

        config.fpsLock = QualitySettings.vSyncCount > 0 ? 0 : 1;
        config.buttonLabelType = buttonLabelType;
        config.invertConfirmCancel = invertConfirmCancel;
        config.extraCamRotWithControllerMoveStrength = extraCamRotWithControllerMoveStrength;
        config.customUIScaling = customUIScaling;
        config.invertYMove = MyInput.invertYMove;
        config.instantText = instantText;

        string gameDataPath = Application.persistentDataPath + "/save" + filenumber.ToString() + ".txt";
        string configDataPath = Application.persistentDataPath + "/config" + filenumber.ToString() + ".txt";
        string recentPath = Application.persistentDataPath + "/recent.txt";
        if (File.Exists(gameDataPath)) {
            File.Copy(gameDataPath, gameDataPath + "backup",true);
        }
        if (File.Exists(configDataPath)) {
            File.Copy(configDataPath, configDataPath+ "backup",true);
        }
        if (File.Exists(recentPath)) {
            File.Copy(recentPath, recentPath + "backup",true);
        }

        FileStream file = File.Create(gameDataPath);
        FileStream configfile = File.Create(configDataPath);
        FileStream recent = File.Create(recentPath);
        if (file == null || configfile == null || recent == null || !file.CanWrite || !configfile.CanWrite || !recent.CanWrite) {
            print("Save failed, couldn't create files.");
            return false;
        }
        bool saveWorked = false;
        try {
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(file, data);

            BinaryFormatter bfConfig = new BinaryFormatter();
            bfConfig.Serialize(configfile, config);


            // Lastly, save a file with the 'recent' filenumber, so the game on next startup
            // initially loads the correct config data.
            Registry.set_startedNewGameButDidntSave(false);
            BinaryFormatter bfRecent = new BinaryFormatter();
            bfRecent.Serialize(recent, filenumber);
            saveWorked = true;

        } catch (SerializationException e) {
            print(e.ToString());
            print("Serialization Exception! Saving failed."); 
            saveWorked = false;
            if (File.Exists(gameDataPath + "backup")) {
                print("Restoring backups.");
                File.Copy(gameDataPath + "backup", gameDataPath);
                File.Copy(configDataPath + "backup", configDataPath);
                File.Copy(recentPath + "backup", recentPath);
            }
        }

        file.Close();
        configfile.Close();
        recent.Close();
        return saveWorked;

    }

	// returns string for use in menu
	static public string getMetaData() {
		string s = "";
		for (int i = 0; i < SaveModule.MAX_SAVES; i++) {
			if (!File.Exists(Application.persistentDataPath + "/save"+i.ToString()+".txt")) {
				s += (i+1).ToString()+". 00:00:00\n---";
			} else {
				FileStream file = File.OpenRead (Application.persistentDataPath + "/save"+i.ToString()+".txt");
				BinaryFormatter b = new BinaryFormatter();
				SaveData data = (SaveData)b.Deserialize(file);
				file.Close();
				string time = MyInput.getTimeString(data.playtime);
				s += (i+1).ToString()+". "+ time + "\n";

                s += DataLoader.instance.getRaw("savePoint",11)+" "+data.cardsFound.ToString();
			}
			if (i != SaveModule.MAX_SAVES - 1) s += "\n\n";

		}
		return s;
	}

    static public int GetRecentFileIndex() {
        if (File.Exists(Application.persistentDataPath + "/recent.txt") == false) {
            print("No recent file exists.");
            return -1;
        }

        FileStream recentFile = File.OpenRead(Application.persistentDataPath + "/recent.txt");
        BinaryFormatter recentBF = new BinaryFormatter();
        int filenumber = (int)recentBF.Deserialize(recentFile);
        recentFile.Close();
        return filenumber;
    }

	static public bool _Load(int filenumber, bool recent,bool ifRecentOnlyGetMetadata=true) {
        if (Registry.startedNewGameButDidntSave) {
            // Check below this block to make sure nothing's missing!
            print("No active save file, returning to Albumenium.");
            DataLoader.instance.resetGameData();
            Registry.enterGameFromLoad_Position = new Vector3(0,-21.5f,199.5f);
            Ano2Stats.loadStatsFromFlags(); // Reset inventory things
            Registry.enterGameFromLoad_SceneName = "Albumen";
            return true;
        }
		if (recent) {
            filenumber = GetRecentFileIndex();
			if (filenumber == -1) {
				print("Loading recent file failed.");
				return false;
			}
			print("Loading config data from recent file "+filenumber);
		}

		if (File.Exists(Application.persistentDataPath + "/save"+filenumber.ToString()+".txt") == false) {
			print("File "+filenumber+" doesn't exist.");
			return false;
		}

		FileStream file = File.OpenRead (Application.persistentDataPath + "/save"+filenumber.ToString()+".txt");
		FileStream configfile = File.OpenRead(Application.persistentDataPath + "/config"+filenumber.ToString()+".txt");

		BinaryFormatter bf = new BinaryFormatter();
		BinaryFormatter bfConfig = new BinaryFormatter();

		// Basically the same as _SavE() but backwards.
		SaveData data = (SaveData)bf.Deserialize (file);
		ConfigData config = (ConfigData)bfConfig.Deserialize(configfile);

		file.Close ();
		configfile.Close();

        if (recent && ifRecentOnlyGetMetadata) {
			print("Only loading config of recent save. Not loading game state data.");
        } else {
            Registry.enterGameFromLoad_Position = data.position.toVector3 ();
            DataLoader.instance.resetGameData();
			DataLoader.instance.updateDialogStateWithString(data.flags);
            if (data.readState != null) DataLoader.instance.updateDialogReadStateWithString(data.readState);

            Ano2Stats.loadStatsFromFlags();
			Registry.enterGameFromLoad_SceneName = data.sceneName;
			playtime = data.playtime;
			infiniteFly = data.infiniteFly;
            doubleHealth = data.doubleHealth;
            currentHealth = data.currentHealth;
            healthUpgrades = data.healthUpgrades;
            invincibility = data.invincibility;
            if (data.minimapState != null) {
                Update_MinimapStateDict_With_String(data.minimapState);
            }
            metacoins = data.metacoins;
            if (metacoins < 0) metacoins = 0;
            totalFoundCoins = data.totalFoundCoins;
            if (totalFoundCoins < 0) totalFoundCoins = 0;
                    
            if (data.autosaveOn2D == 2) {
                SavePoint.AutoSaveOn2D = false;
            }
            // Validate health data
            if (healthUpgrades < 0) healthUpgrades = 0; if (healthUpgrades > 6) healthUpgrades = 6;
            if (currentHealth < 1) currentHealth = 1;
            if (doubleHealth) {
                if (currentHealth > 2 * (6 + healthUpgrades)) currentHealth = 2 * (6 + healthUpgrades);
            } else {
                if (currentHealth > 6+ healthUpgrades) currentHealth = (6 + healthUpgrades);
            }
		}

        fieldOfView = config.fieldOfView;
        if (fieldOfView < 50 || fieldOfView > 120) fieldOfView = 82;
        if (PauseMenu.language_changed_dont_change_on_load == false) {
            language = config.language;
            bool langFound = false;
            for (int i = 0; i < PauseMenu.languageArray.Length; i++) {
                if (language == PauseMenu.languageArray[i]) langFound = true;
            }
            if (!langFound) {
                print("Language not found on load, defaulting to english.");
                language = "en";
            } else {
                print("Loaded language: " + language);
            }
        } else {
            print("Language changed during game - reloads will now ignore.");
        }
		brightness = config.brightness;
        if (PauseMenu.volume_changed_dont_change_on_load == false) {
            volume = config.volume;
        }

        // Patch 15
        shadowQuality = config.shadowQuality;
        cameraDistance = config.cameraDistance;
        controllerDisable = config.controllerDisable;
        if (!keepSkipOnUntilLoadingFromTitle) {
            // set in pause menu
            dialogueSkip = config.dialogueSkip;
        }

        if (cameraDistance <= 0) {
            cameraDistance = 100;
            shadowQuality = 3;
        }


        // End patch 15

        // patch 17
        screenshake = config.screenshake;

        // patch 18
        terrainQuality = config.terrainQuality;
        if (terrainQuality <= 0) terrainQuality = 5;


        // 1.08
        if (config.fpsLock == 0) {
            QualitySettings.vSyncCount = 1;
            Application.targetFrameRate = -1;
        } else {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
            config.fpsLock = 1;
        }
        if (config.buttonLabelType >= 0 && config.buttonLabelType <= 5) {
            SaveManager.buttonLabelType = config.buttonLabelType;
        }

        // 1.08.1
        invertConfirmCancel = config.invertConfirmCancel;
        extraCamRotWithControllerMoveStrength = config.extraCamRotWithControllerMoveStrength;
        if (extraCamRotWithControllerMoveStrength < 0f) extraCamRotWithControllerMoveStrength = 0f;

        MyInput.invertYMove = config.invertYMove;
        customUIScaling = config.customUIScaling;
        if (customUIScaling < 2) customUIScaling = 2;

        AudioMixer am = GameObject.Find("Track 1-1").GetComponent<AudioSource>().outputAudioMixerGroup.audioMixer;
		if (volume == 0) am.SetFloat("MasterVolume",-80f);
		if (volume > 0) am.SetFloat("MasterVolume",Mathf.Lerp(-34f,0,SaveManager.volume/100f));

        if (dontChangeResOnLoadBCChangedInTitle == false) {
            print("Win: " + winResX + "," + winResY + " fs: " + fullscreen);
            print("Config: " + config.winResX + "," + config.winResY + " fs: " + config.fullscreen);
            if (winResX == config.winResX && winResY == config.winResY && fullscreen == config.fullscreen) {
                Screen.SetResolution(winResX, winResY, fullscreen);
            } else if (winResX != 0 && winResY != 0) {
                winResX = config.winResX;
                winResY = config.winResY;
                fullscreen = config.fullscreen;
                // old bug was - game would load in default 1280x720 resolution, fs = true, and then here, 
                // if fs was still set to true the game is set to fullscreen without escaling?
                if (!fullscreen) {
                    print("Screen.Setresolution to config's data");
                    Screen.SetResolution(winResX, winResY, fullscreen);
                } else {
                    print("Making fullscreen");
                    Screen.fullScreen = fullscreen;
                }
                print("Loaded res: "+winResX + "," + winResY + " fs: " + fullscreen);
                UIManagerAno2.force3DUIRefreshTicks = 5;
            }
        } else {
            print("Ignoring resolution change on load because res was changed from the title menu");
            dontChangeResOnLoadBCChangedInTitle = false;
        }

		sensitivity = config.sensitivity;
		MyInput.invertX = config.invertX;
		MyInput.invertY = config.invertY;
        MyInput.useMouse = config.useMouse;
            

		MyInput.KC_up = config.keyUp;
		MyInput.KC_right = config.keyRight;
		MyInput.KC_down = config.keyDown;
		MyInput.KC_left= config.keyLeft;

		MyInput.KC_cancel = config.keyCancel;
		MyInput.KC_jump = config.keyJump;
		MyInput.KC_pause = config.keyPause;
        MyInput.KC_talk = config.keyTalk;
        if (MyInput.KC_talk == KeyCode.None) MyInput.KC_talk = KeyCode.Space;

        MyInput.KC_zoomIn = config.keyZoomIn;
        MyInput.KC_zoomOut = config.keyZoomOut;
        // patch 18
        if (MyInput.KC_zoomIn == KeyCode.None) MyInput.KC_zoomIn = KeyCode.E;
        if (MyInput.KC_zoomOut == KeyCode.None) MyInput.KC_zoomOut = KeyCode.R;

        // 2020 02 19
        instantText = config.instantText;

        MyInput.KC_special = config.keyNano;
        MyInput.KC_toggleRidescale = config.keyChangeSize;
        MyInput.KC_camtoggle = config.keyCamToggle;

        Registry.set_startedNewGameButDidntSave(false);
        Registry.justLoaded = true;
		return true;

	}

	static public string language = "en";
	static public float brightness = 1.0f;
	static public float volume = 100f;
	static public bool fullscreen = true;
	static public int winResX = 0;
	static public int winResY = 0;
	static public bool infiniteFly = false;
	static public int sensitivity = 100;
    static public float fieldOfView = 82;
    static public bool invertConfirmCancel = false;
    static public int customUIScaling = 2;

    static public bool doubleHealth = false;
    static public int healthUpgrades = 0;
    static public int currentHealth = 6;
    static public int metacoins = 0;
    static public int totalFoundCoins = 0;


    static public bool dialogueSkip = false;
    static public bool keepSkipOnUntilLoadingFromTitle = false;
    static public bool controllerDisable = false;
    static public int shadowQuality = 3;
    static public float cameraDistance = 100;
    static public bool screenshake = true;
    static public bool invincibility = false;
    static public int terrainQuality = 5;
    static public int buttonLabelType = 0;
    static public float extraCamRotWithControllerMoveStrength = 0;
    static public bool instantText = false;
}


[Serializable]
class SaveData {
	public SaveVector3 position;
    public int cardsFound;
	public string sceneName;
	public string flags;
    public string readState;
    public string minimapState = "";
	public bool infiniteFly;
	public int playtime;
    public bool doubleHealth = false;
    public bool invincibility = false;
    public int healthUpgrades;
    public int currentHealth;
    public int autosaveOn2D;
    public int metacoins;
    public int totalFoundCoins;
}


[Serializable] 
class ConfigData {
    public bool invertConfirmCancel = false;
	public string language = "en";
	public float brightness = 1.0f;
	public float volume = 1.0f;
    public int vsync = 0;
    public bool instantText = false;
    public bool screenshake = true;
	public bool invertX = false;
	public bool invertY = false;
    public bool invertYMove = false;
    public int customUIScaling = 2;
    public bool useMouse = false;
	public bool fullscreen = true;
	public int winResX = 0;
	public int winResY = 0;
    public bool dialogueSkip = false;
    public bool controllerDisable = false;
    public int shadowQuality = 3; // 3 2 1 0 - VeryHigh, high, med, off
    public int terrainQuality = 5;
    public int cameraDistance = 100;
    public float fieldOfView = 82;
	public int sensitivity = 100;
    public int fpsLock = 0;
    public int buttonLabelType = 0;
    public float extraCamRotWithControllerMoveStrength = 0f;
    public KeyCode keyUp = KeyCode.UpArrow;
	public KeyCode keyRight = KeyCode.RightArrow;
	public KeyCode keyDown = KeyCode.DownArrow;
	public KeyCode keyLeft = KeyCode.LeftArrow;
	public KeyCode keyCancel = KeyCode.Z;
	public KeyCode keyJump = KeyCode.X; // KC_jump
    public KeyCode keyNano = KeyCode.C; // KC_Special
    public KeyCode keyTalk = KeyCode.Space;

    public KeyCode keyPause = KeyCode.Return;

    public KeyCode keyCamToggle = KeyCode.Q;
    public KeyCode keyChangeSize = KeyCode.Tab;

    public KeyCode keyZoomIn = KeyCode.E;
    public KeyCode keyZoomOut = KeyCode.R;

    public KeyCode keyConfirm = KeyCode.X; // Not used, just for save data compatibility
}

[Serializable]
class SaveVector3 {
	public SaveVector3(Vector3 v) {
		x = v.x;
		y = v.y;
		z = v.z;
	}

	public Vector3 toVector3() {
		return new Vector3(x,y,z);
	}
	float x;
	float y;
	float z;
}