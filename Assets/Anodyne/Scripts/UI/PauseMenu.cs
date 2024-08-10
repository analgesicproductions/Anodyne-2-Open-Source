using Anodyne;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour {

    GameObject InformationMenu;
    GameObject CardsMenu;
    GameObject InventoryMenu;
    GameObject SettingsMenu;
    GameObject MetacleanMenu;
    DialogueBox dbox;
    TMP_Text BottomLeftText;
    TMP_Text text_inv_read;
    TMP_Text text_cards_read;

    TMP_Text choice_information;
    TMP_Text choice_cards;
    TMP_Text choice_inventory;
    TMP_Text choice_settings;
    TMP_Text choice_returntotitle;
    TMP_Text choice_quitgame;
    TMP_Text choice_metaclean;

    GameObject PrismInfo;
    GameObject Pausemenu_infoscreen_nomap;
    GameObject Pausemenu_infoscreen_prismonly;
    GameObject Pausemenu_infoscreen;
    GameObject prism_cardicons_tutorial;
    GameObject MinimapTilePrefab;

    Image menu_selector;
    Vector3 tempLocalPos = new Vector3();
    TopLevelMode mode = TopLevelMode.Closed;
    enum TopLevelMode { TopLevel, Information, Cards, Inventory, Settings, ReturnToTitle, QuitGame, Secret, OpeningMenu, ClosingMenu, Closed, PrismTutorial, DustDeposit, Metaclean }

    AudioSource audiosrcopen;
    AudioSource audiosrcmove;
    AudioSource audiosrccancel;
    AudioClip pauseMoveClip;
    AudioClip pauseOpenClip;
    AudioClip pauseCancelClip;
    AudioClip pauseSelectClip;
    AudioClip pauseCloseClip;
    //AudioSource[] audioSources = new AudioSource[4];

	TMP_Text textChoice;
	Image settingsCursor;
	Image settingsYesNoCursor;
	Image overlay;

	float tFade = 0;
	//float tmFade = 0.4f;
	int submode = 0;
	int cursorIndex = 0;
    int maxAddedW = 0;
    int maxAddedH = 0;
    int menuJokeCtr = 0;

	Vector3 settingsCur1InitPos;
	List<Resolution> resolutions;
    SpriteAnimator[,] prismCardAnims = new SpriteAnimator[6,4];

    List<Image> minimapTiles = new List<Image>();

    GameObject pauseMinimap;
    GameObject pauseMinimapPlayerPos;
    Sprite[] GameMinimapSpritesheet;

    TMP_Text text_prism_level;
    TMP_Text text_dust;
    TMP_Text text_prism;

    Transform PauseMenuTransform;
    DustBar dustbar;

    Sprite[] smallCardFrames;
    Sprite[] bigCardFrames;
    Sprite cardSpot;
    Image bigCard;
    Image[,] smallCards = new Image[4, 3];
    RectTransform cards_scrollbar;
    Image cardcursor;

    Sprite[] smallItemFrames;
    Image bigItem;
    Image[,] smallItems = new Image[5, 3];
    RectTransform inventory_scrollbar;
    Image itemcursor;
    Sprite itemSpot;
    TMP_Text text_item_name;

    RectTransform settings_scrollbar;
    RectTransform metaclean_scrollbar;
    Image[] metaicons = new Image[10];
    Sprite[] metaicon_Sprites;
    Image metacursor;
    TMP_Text text_metaChoices;
    TMP_Text text_metaCosts;
    int idx_MC_curTopChoice = 0;
    RectTransform MetashopBG_t;
    RectTransform MetacoinBG_t;
    List<int> MC_ChoiceList = new List<int>();
    Vector3 MC_tempPos = new Vector3();
    Vector3 MC_initcursorPos = new Vector3();



    Image mapPlayerPos;
    Image CCC_Map;
    bool InCCC = false;
    bool InOceanOrFantasy = false;
    bool InDesertOrRing3DArea = false;
    string curSceneName = "";

    Transform ItemProgressMarkers;

    static string MetazoneCachedScene;
    public static string MetazoneCachedEntrance;

    private void Awake() {
        if (SaveManager.winResX == 0 || SaveManager.winResY == 0) {
            print("PauseMenu Awake: Setting SaveManager.winRes to Screen.currentRes");
            if (SaveManager.winResX == 0) SaveManager.winResX = Screen.currentResolution.width;
            if (SaveManager.winResY == 0) SaveManager.winResY = Screen.currentResolution.height;
        }
    }

    void Start () {
        TMP_Text verText = null;
        if (GameObject.Find("versiontext2") != null) { verText = GameObject.Find("versiontext2").GetComponent<TMP_Text>(); }
        else if (GameObject.Find("version") != null) { verText = GameObject.Find("version").GetComponent<TMP_Text>(); }

        if (verText != null) {
            string platformPart = "";
            //if (SteamManager.Initialized) platformPart = "(Steam)";
            verText.text = DataLoader.instance.getRaw("controlLabels", 15) + " " + platformPart;
        }
        verText.text = "";

        if (hideVersionTextAndStuff) {
            if (GameObject.Find("UI Overlay") != null) {
                GameObject.Find("UI Overlay").GetComponent<Image>().enabled = false;
            }
        }


        ItemProgressMarkers = GameObject.Find("ItemProgressMarkers").transform;
        pauseMinimapPlayerPos = GameObject.Find("PauseMinimapPos");
        pauseMinimap = GameObject.Find("PauseMinimap");
        MinimapTilePrefab = GameObject.Find("PauseMinimapTile");
        BottomLeftText = GameObject.Find("text_playtimecontrol").GetComponent<TMP_Text>();

        if (GameObject.Find("Dialogue") != null) dbox = GameObject.Find("Dialogue").GetComponent<DialogueBox>();

        CCC_Map = GameObject.Find("CCC Map").GetComponent<Image>();
        mapPlayerPos = GameObject.Find("Map Player Pos").GetComponent<Image>();
        curSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (curSceneName == "CCC") {
            InCCC = true;
        } else if (curSceneName.IndexOf("Ring") == 0 || curSceneName.IndexOf("Desert") == 0) {
            CCC_Map.rectTransform.localEulerAngles = new Vector3(0, 0, 0);
            InDesertOrRing3DArea = true;
            if (DataLoader._getDS(Registry.FLAG_DESERT_OPEN) == 1) {
                CCC_Map.GetComponent<SpriteAnimator>().Play("desert");
            } else {
                CCC_Map.GetComponent<SpriteAnimator>().Play("ring");
            }
        } else if (curSceneName == "NanoFantasy") {
            InOceanOrFantasy = true;
            CCC_Map.rectTransform.localEulerAngles = new Vector3(0, 0, 0);
            CCC_Map.GetComponent<SpriteAnimator>().Play("fantasy");
            mapPlayerPos.enabled = false;
        } else if (curSceneName == "NanoOcean") {
            InOceanOrFantasy = true;
            CCC_Map.rectTransform.localEulerAngles = new Vector3(0, 0, 0);
            CCC_Map.GetComponent<SpriteAnimator>().Play("ocean");
        } else if (curSceneName == "NanoDustbound") {
            CCC_Map.rectTransform.localEulerAngles = new Vector3(0, 0, 0);
            CCC_Map.GetComponent<SpriteAnimator>().Play("db");
            mapPlayerPos.enabled = false;
        } else {
            GameObject.Find("PauseMaps").SetActive(false);
        }

        if (curSceneName.IndexOf("ZZ_") == 0 || curSceneName.IndexOf("ZY_") == 0 || curSceneName == "DEBUG2DFAKE" || curSceneName == "S_MarinaRing1Test") {
            inMetazone = true;
        }


        choice_cards = GameObject.Find("choice_cards").GetComponent<TMP_Text>();
        choice_inventory = GameObject.Find("choice_inventory").GetComponent<TMP_Text>();
        choice_information = GameObject.Find("choice_information").GetComponent<TMP_Text>();
        choice_metaclean = GameObject.Find("choice_metaclean").GetComponent<TMP_Text>();
        choice_settings = GameObject.Find("choice_settings").GetComponent<TMP_Text>();
        choice_quitgame = GameObject.Find("choice_quitgame").GetComponent<TMP_Text>();
        choice_returntotitle = GameObject.Find("choice_returntotitle").GetComponent<TMP_Text>();

        itemSpot = Resources.LoadAll<Sprite>("Visual/Sprites/UI/item_spot")[0];
        smallItemFrames = Resources.LoadAll<Sprite>("Visual/Sprites/UI/ItemSprites24");
        itemcursor = GameObject.Find("itemcursor").GetComponent<Image>();
        init_itemcursor_localpos = itemcursor.transform.localPosition;
        inventory_scrollbar = GameObject.Find("inventory scrollbar").GetComponent<RectTransform>();
        bigItem = GameObject.Find("big item").GetComponent<Image>();
        for (int i = 0; i < 5; i++) {
            for (int j = 0; j < 3; j++) {
                smallItems[i, j] = GameObject.Find("itemrow (" + (i + 1).ToString() + ")").transform.Find("item (" + (j + 1).ToString() + ")").GetComponent<Image>();
            }
        }
        text_item_name = GameObject.Find("text_item_name").GetComponent<TMP_Text>(); text_item_name.text = "";
        text_cards_read = GameObject.Find("text_cards_read").GetComponent<TMP_Text>(); text_cards_read.text = "";
        text_inv_read = GameObject.Find("text_inv_read").GetComponent<TMP_Text>(); text_inv_read.text = "";

        cards_scrollbar = GameObject.Find("cards scrollbar").GetComponent<RectTransform>();
        cardcursor = GameObject.Find("cardcursor").GetComponent<Image>();
        init_cardcursor_localpos = cardcursor.transform.localPosition;
        cardcursor.enabled = false;
        smallCardFrames = Resources.LoadAll<Sprite>("Visual/Sprites/UI/cards_small");
        bigCardFrames = Resources.LoadAll<Sprite>("Visual/Sprites/UI/cards_big");
        cardSpot = Resources.LoadAll<Sprite>("Visual/Sprites/UI/card_spot")[0];
        bigCard = GameObject.Find("BigCard").GetComponent<Image>();
        for (int i =0; i < 4; i++) {
            for (int j = 0; j < 3; j++) {
                smallCards[i,j] = GameObject.Find("maincardrow (" + (i + 1).ToString() + ")").transform.Find("card (" + (j + 1).ToString() + ")").GetComponent<Image>();
            }
        }


        metaclean_scrollbar = GameObject.Find("meta scrollbar").GetComponent<RectTransform>();
        settings_scrollbar = GameObject.Find("settings scrollbar").GetComponent<RectTransform>();
        metaicon_Sprites = Resources.LoadAll<Sprite>("Visual/Sprites/UI/Metaclean_Icons");
        MetashopBG_t = GameObject.Find("MetashopBG").GetComponent<RectTransform>();
        MetacoinBG_t = GameObject.Find("MetacoinBG").GetComponent<RectTransform>();
        for (int i = 0; i < metaicons.Length; i++) {
            metaicons[i] = GameObject.Find("metachoice (" + i + ")").GetComponent<Image>();
        }
        metacursor = GameObject.Find("MetaCursor").GetComponent<Image>();
        MC_initcursorPos = metacursor.transform.localPosition;
        text_metaChoices = GameObject.Find("MetaChoices").GetComponent<TMP_Text>();
        text_metaCosts = GameObject.Find("MetaCosts").GetComponent<TMP_Text>();


        PauseMenuTransform = GameObject.Find("PauseMenu").transform;
        TopLevel = GameObject.Find("TopLevel");
        PrismInfo = GameObject.Find("PrismInfo");
        Pausemenu_infoscreen_prismonly  = GameObject.Find("Pausemenu_infoscreen_prismonly");
        Pausemenu_infoscreen_nomap= GameObject.Find("Pausemenu_infoscreen_nomap");
        prism_cardicons_tutorial = GameObject.Find("prism_cardicons_tutorial");
        prism_cardicons_tutorial.SetActive(false);
        Pausemenu_infoscreen_prismonly.SetActive(false);
        Pausemenu_infoscreen_nomap.SetActive(false);
        Pausemenu_infoscreen = GameObject.Find("Pausemenu_infoscreen");
        PrismGoalLineRT = GameObject.Find("prism goal lines").GetComponent<RectTransform>();
        PrismCurrentLevelRT = GameObject.Find("prism current level").GetComponent<RectTransform>();
        PrismCurrentLevelMaskRT = GameObject.Find("prism level mask").GetComponent<RectTransform>();
        PrismCurrentDustRT = GameObject.Find("prism current dust").GetComponent<RectTransform>();
        PauseHealthBar = GameObject.Find("HealthBarMaskpm").GetComponent<HealthBar>();
        dustbar = GameObject.Find("DustMaskpm").GetComponent<DustBar>();

        text_prism_level = GameObject.Find("text prism level").GetComponent<TMP_Text>();
        text_dust= GameObject.Find("text dust").GetComponent<TMP_Text>();
        text_prism= GameObject.Find("text prism").GetComponent<TMP_Text>();

        for (int i = 0; i < 6; i++) {
            Transform prismcardrowt = GameObject.Find("card row " + (i+1).ToString()).transform;
            for (int j = 0; j < 4; j++) {
                prismCardAnims[i, j] = prismcardrowt.Find("card (" + (j+1).ToString() + ")").GetComponent<SpriteAnimator>();
            }
        }
        // Get a list of all 16:9 (and bigger than the max 16:9) resolutions for this game
        if (resolutions == null) {
			resolutions = new List<Resolution>();

			// Get the max native resolution for this computer
			int maxW = 0;
			int maxH = 0;
			foreach (Resolution r in Screen.resolutions) {
				if (r.width > maxW) maxW = r.width;
				if (r.height > maxH) maxH = r.height;
			}

			// Add in all fittable resolutions that have a 16:9 ratio to the 'resolutions' array.
			maxAddedW = 0;
			maxAddedH = 0;
			for (int i = 80; i < 400; i += 10) {
				if (i*16 <= maxW && i*9 <= maxH) {
					Resolution r = new Resolution();
					r.width = i*16;
					r.height = i*9;
					maxAddedW = r.width; maxAddedH = r.height;
					resolutions.Add(r);
				}
			}

			// Add in all resolutions above the max 16:9 resolution.
			List<string> l = new List<string>();
			// Find res for this cpu above the max 16:9
			foreach (Resolution r in Screen.resolutions) {
				string s = r.width.ToString()+"x"+r.height.ToString();
				if (l.Contains(s) == false) {
					if ((r.width > maxAddedW && r.height>= maxAddedH) || (r.width>= maxAddedW&& r.height>maxAddedH)) {
						Resolution rr = new Resolution();
						rr.width = r.width;
						rr.height = r.height;
                        maxAddedW = r.width;
                        maxAddedH = r.height;
						resolutions.Add(rr);
					}
					l.Add(s);
				}
			}

			// set the initial resIndex
			foreach (Resolution r in resolutions) {
				if (r.width == Screen.width && r.height == Screen.height) {
					break;
				}
				resIndex ++;
			}

		}
        InformationMenu = GameObject.Find("InformationMenu");
        CardsMenu= GameObject.Find("CardsMenu");
        InventoryMenu= GameObject.Find("InventoryMenu");
        SettingsMenu = GameObject.Find("SettingsMenu");
        MetacleanMenu = GameObject.Find("MetacleanMenu");

        menu_selector = GameObject.Find("menu_selector").GetComponent<Image>();


		settingsCursor = GameObject.Find("PauseCursor").GetComponent<Image>();
		settingsYesNoCursor = GameObject.Find("PauseCursor2").GetComponent<Image>();
		overlay = GameObject.Find("Pause Overlay").GetComponent<Image>();
		textChoice = GameObject.Find("PauseText").GetComponent<TMP_Text>();


        //string outputmixer = "Regular SFX";
        //AudioMixer mixer = GameObject.Find("Track 1-1").GetComponent<AudioSource>().outputAudioMixerGroup.audioMixer;
        //for (int i =0; i < audioSources.Length; i++) {
          //  AudioSource src = gameObject.AddComponent<AudioSource>();
           // src.outputAudioMixerGroup = mixer.FindMatchingGroups(outputmixer)[0];
            //audioSources[i] = src;
       // }
        pauseMoveClip = Resources.Load("Audio/Sound/menuMove") as AudioClip;
        pauseOpenClip = Resources.Load("Audio/Sound/menuOpenAno2") as AudioClip;
        pauseCancelClip = Resources.Load("Audio/Sound/menuCancel") as AudioClip;
        pauseCloseClip= Resources.Load("Audio/Sound/menuClose") as AudioClip;
        pauseSelectClip = Resources.Load("Audio/Sound/menuSelect") as AudioClip;

        setAlphaImage(settingsCursor,0);
		setAlphaImage(overlay,0);
		setAlphaImage(settingsYesNoCursor,0);

        setSettingsText();
        settingsCur1InitPos = settingsCursor.rectTransform.localPosition;

	}

    bool didinit = false;


	string getStrOn() {
		return DataLoader.instance.getRaw("pauseMenuWords",0);
	}
	string getStrOff() {
		return DataLoader.instance.getRaw("pauseMenuWords",1);
	}
	void setAlphaImage(Image i, float a) {
		Color c = i.color;
		c.a = a;
		i.color = c;
	}
    void PlaySound(AudioClip clip) {
        AudioHelper.instance.playOneShot(clip.name);
    }
    void cancelSound() {
        PlaySound(pauseCancelClip);
    }
    void moveSound() {
        PlaySound(pauseMoveClip);
    }
    void confirmSound() {
        PlaySound(pauseSelectClip);
    }
    void openSound() {
        PlaySound(pauseOpenClip);
    }
    void closeSound() {
        PlaySound(pauseCloseClip);
    }
    void SetMenuSelectorPosition(int index) {
        tempLocalPos = menu_selector.transform.localPosition; tempLocalPos.y = 86 + index*-18;
        menu_selector.transform.localPosition = tempLocalPos;
    }

    RectTransform PrismCurrentDustRT;
    RectTransform PrismGoalLineRT;
    RectTransform PrismCurrentLevelRT;
    RectTransform PrismCurrentLevelMaskRT;

    HealthBar PauseHealthBar;
    int currentPauseUIScale = 3;
    [System.NonSerialized]
    public bool useDarkenedHorrorUI = false;
    void RefreshPauseVisuals() {
        float scale = 1;
        if (Screen.fullScreen) {
            scale = UIManager2D.getIdealScaleValue(230,Screen.resolutions[Screen.resolutions.Length-1].height);
        } else {
            scale = UIManager2D.getIdealScaleValue();
        }
        currentPauseUIScale = Mathf.RoundToInt(scale);
        Vector3 scaleV = PauseMenuTransform.localScale; scaleV.Set(scale, scale, scale); PauseMenuTransform.localScale = scaleV;

        bool IsRingOpen = DataLoader.instance.getDS("ddp-open-ring1") == 1;
        bool IsDesertOpen = DataLoader.instance.getDS("ddp-open-ring2") == 1;
        int PrismUpgrades = Ano2Stats.CountPrismUpgrades();
        int PrismDust = Ano2Stats.prismCurrentDust;
        int totalCards = Ano2Stats.CountTotalCards();

        choice_information.text = RawD("topLevelPause", 0);
        choice_cards.text = RawD("topLevelPause", 1);
        choice_inventory.text = RawD("topLevelPause", 2);
        choice_settings.text = RawD("topLevelPause", 3);
        choice_returntotitle.text = "";
        choice_quitgame.text = RawD("topLevelPause", 5);
        choice_metaclean.text = RawD("topLevelPause", 6);

        if (SaveManager.language == "jp") {
            choice_information.characterSpacing = -7f;
            choice_information.fontSize = 10f;
        } else {
            choice_information.characterSpacing = 0f;
            choice_information.fontSize = 11f;
        }

        if (!IsDesertOpen && !MyInput.shortcut) choice_metaclean.text = "";
        if (useDarkenedHorrorUI) {
            choice_metaclean.text = "";
            choice_information.text = "";
            choice_cards.text = "";
            choice_inventory.text = "";
        }

        ItemProgressMarkers.gameObject.SetActive(false);

        if (InCCC) {
            Vector2 newPlayerMarkerPos = new Vector2(-1.47f, 4f);
            Vector3 playerPos = new Vector3();
            if (GameObject.Find("MediumPlayer") != null) {
                playerPos = GameObject.Find("MediumPlayer").transform.position;
            } else if (GameObject.Find("BigPlayer") != null) {
                playerPos = GameObject.Find("BigPlayer").transform.position;
            }

            if (DataLoader._getDS(Registry.FLAG_DESERT_OPEN) == 1) {
                ItemProgressMarkers.gameObject.SetActive(true);
                foreach (Image img in ItemProgressMarkers.GetComponentsInChildren<Image>()) {
                    img.enabled = false;
                    if (img.name == "HEALTHRingClone" && 0 == DataLoader._getDS("HEALTHEldi")) {
                        // -15 40
                        img.rectTransform.anchoredPosition = new Vector2(-15, 40);
                        img.enabled = true;
                    } else if (img.name == "HEALTHCCC" && 0 == DataLoader._getDS("HEALTHCCC")) {
                        // -56 39
                        img.rectTransform.anchoredPosition = new Vector2(-56, 39);
                        img.enabled = true;
                    }
                }
            }

            float scaleRatio = 210f / (31.8f - -1.24f);
            // The 0,0 here refer to where the initial newPlayerMarkerPos  position is in world space
            newPlayerMarkerPos.x += (playerPos.x - 0) / scaleRatio;
            newPlayerMarkerPos.y += (playerPos.z - 0) / scaleRatio;
            newPlayerMarkerPos.x = Mathf.RoundToInt(newPlayerMarkerPos.x);
            newPlayerMarkerPos.y = Mathf.RoundToInt(newPlayerMarkerPos.y);
            mapPlayerPos.rectTransform.localPosition = newPlayerMarkerPos;
        } else if (InOceanOrFantasy) {
            float xPercent = (GameObject.Find(Registry.PLAYERNAME2D).transform.position.x - (-12)) / (96 - (-12));
            float yPercent = (GameObject.Find(Registry.PLAYERNAME2D).transform.position.y - (-72)) / (60 - (-72)); // from bottom
            Vector2 newPlayerMarkerPos = new Vector2();
            newPlayerMarkerPos.x = xPercent * (54.5f - (-49f)) + (-49f);
            newPlayerMarkerPos.y = yPercent * (62.7f - (-62f)) + (-62f);
            newPlayerMarkerPos.x = Mathf.RoundToInt(newPlayerMarkerPos.x);
            newPlayerMarkerPos.y = Mathf.RoundToInt(newPlayerMarkerPos.y);
            mapPlayerPos.rectTransform.localPosition = newPlayerMarkerPos;
            ItemProgressMarkers.gameObject.SetActive(true);
            foreach (Image img in ItemProgressMarkers.GetComponentsInChildren<Image>()) {
                img.enabled = false;
                if (curSceneName == "NanoFantasy") {
                    if (img.name == "HEALTHNanoFantasy" && 0 == DataLoader._getDS("HEALTHNanoFantasy")) {
                        img.enabled = true;
                        img.rectTransform.anchoredPosition = new Vector2(-52, -28);
                    } else if (img.name == "CardDesertField" && 0 == DataLoader._getDS("CARD16")) {
                        img.enabled = true;
                        img.rectTransform.anchoredPosition = new Vector2(-8,-2);
                    } else if (img.name == "CardDesertOrb" && 0 == DataLoader._getDS("CARD17")) {
                        img.enabled = true;
                        img.rectTransform.anchoredPosition = new Vector2(-71,23);
                    }
                } else if (curSceneName == "NanoOcean") {
                    if (img.name == "HEALTHDesertShore" && 0 == DataLoader._getDS("HEALTHStealer")) {
                        img.enabled = true;
                        img.rectTransform.anchoredPosition = new Vector2(30, 65);
                    } else if (img.name == "CardDesertField" && 0 == DataLoader._getDS("CARD21")) {
                        img.enabled = true;
                        img.rectTransform.anchoredPosition = new Vector2(-55,24);
                    } else if (img.name == "CardDesertOrb" && 0 == DataLoader._getDS("CARD22")) {
                        img.enabled = true;
                        img.rectTransform.anchoredPosition = new Vector2(34,17);
                    } else if (img.name == "CardDesertShore" && 0 == DataLoader._getDS("CARD23")) {
                        img.enabled = true;
                        img.rectTransform.anchoredPosition = new Vector2(47,52);
                    }
                }
            }
        } else if (InDesertOrRing3DArea) {

            if (DataLoader._getDS(Registry.FLAG_DESERT_OPEN) == 1) {
                ItemProgressMarkers.gameObject.SetActive(true);
                if (true) ItemProgressMarkers.transform.Find("HEALTHCCC").GetComponent<Image>().enabled = false;
                if (1 == DataLoader._getDS("HEALTHRingClone")) ItemProgressMarkers.transform.Find("HEALTHRingClone").GetComponent<Image>().enabled = false;
                if (1 == DataLoader._getDS("HEALTHNanoFantasy")) ItemProgressMarkers.transform.Find("HEALTHNanoFantasy").GetComponent<Image>().enabled = false;
                if (1 == DataLoader._getDS("HEALTHDesertShore") && 1 == DataLoader._getDS("HEALTHStealer")) ItemProgressMarkers.transform.Find("HEALTHDesertShore").GetComponent<Image>().enabled = false;


                // 16 17 19 20
                if (Ano2Stats.HasCard(16) && Ano2Stats.HasCard(17) && Ano2Stats.HasCard(19) && Ano2Stats.HasCard(20)) ItemProgressMarkers.transform.Find("CardDesertField").GetComponent<Image>().enabled = false;
                // 12 13 14 15 18
                if (Ano2Stats.HasCard(12) && Ano2Stats.HasCard(13) && Ano2Stats.HasCard(14) && Ano2Stats.HasCard(15) && Ano2Stats.HasCard(18)) ItemProgressMarkers.transform.Find("CardDesertOrb").GetComponent<Image>().enabled = false;
                // 21 22 23
                if (Ano2Stats.HasCard(21) && Ano2Stats.HasCard(22) && Ano2Stats.HasCard(23)) ItemProgressMarkers.transform.Find("CardDesertShore").GetComponent<Image>().enabled = false;
                // cards..
            }

            Vector2 newPlayerMarkerPos = new Vector2();
            if (curSceneName == "RingCCC") {
                newPlayerMarkerPos.Set(4f, 4f);
            } else if (curSceneName == "RingGolem") {
                newPlayerMarkerPos.Set(-16f, 12f);
            } else if (curSceneName == "RingCave") {
                newPlayerMarkerPos.Set(31f, 4f);
            } else if (curSceneName == "RingHighway") {
                newPlayerMarkerPos.Set(1.4f, -11f);
            } else if (curSceneName == "RingClone") {
                newPlayerMarkerPos.Set(24f, 13f);
            } else if (curSceneName == "DesertOrb") {
                newPlayerMarkerPos.Set(39, -10f);
            } else if (curSceneName == "DesertField") {
                newPlayerMarkerPos.Set(27, 39);
            } else if (curSceneName == "DesertShore") {
                newPlayerMarkerPos.Set(-47, 20);
            } else if (curSceneName.IndexOf("DesertSpire") != -1) {
                newPlayerMarkerPos.Set(7f,-39f);
            }
            mapPlayerPos.rectTransform.localPosition = newPlayerMarkerPos;
        }

        // Amount of dust in the prism
        tempLocalPos = PrismCurrentDustRT.localPosition; tempLocalPos.y = -113 + Mathf.Floor(Mathf.Min(PrismDust, 300) / 50f) * 2 + (PrismDust / 350f) * 98; PrismCurrentDustRT.localPosition = tempLocalPos;

        // Show goal based on game state
        tempLocalPos = PrismGoalLineRT.localPosition;
        tempLocalPos.y = -3 - 16 * 6;
        if (IsRingOpen) tempLocalPos.y = -3 - 16*4;
        if (IsDesertOpen) tempLocalPos.y = -3 - 16;
        PrismGoalLineRT.localPosition = tempLocalPos;

        // Move the highlight of prism based on current prism level
        SetPrismLevelVisuals(PrismUpgrades);

        // Set little prism card state (Card rows go from bottom to top, columsn left to right)
        int prismCardsToAnimate = 4; if (IsRingOpen) prismCardsToAnimate = 12; if (IsDesertOpen) prismCardsToAnimate = 24;
        int possibleMaxPrismLevel = 1 + totalCards / 4;
        int counter = 0;
        for (int i = 0; i < 6; i++) {
            for (int j = 0; j < 4; j++) {
                SpriteAnimator prismCard = prismCardAnims[i, j];
                if (prismCardsToAnimate <= 0) {
                    prismCard.GetComponent<Image>().enabled = false;
                } else {
                    prismCard.GetComponent<Image>().enabled = true;
                    if (counter < PrismUpgrades*4) {
                        prismCard.Play("used");
                    }  else if (counter < (possibleMaxPrismLevel-1)*4) {
                        prismCard.ForcePlay("flash");
                    } else if (counter < totalCards) {
                        prismCard.Play("unused");
                    } else {
                        prismCard.Play("off");
                    }
                }
                prismCardsToAnimate--;
                counter++;
            }
        }
        
        // Change text on info menu

        int cardsLeftForLevel = 4 * (PrismUpgrades + 1) - totalCards;
        if (IsRingOpen && !IsDesertOpen && PrismUpgrades == 3) cardsLeftForLevel = 0;
        if (IsDesertOpen && PrismUpgrades == 6) cardsLeftForLevel = 0;
        if (cardsLeftForLevel < 0) cardsLeftForLevel = 0;

        text_prism.text = RawD("pause-info-text", 0);
        text_prism_level.text = RawD("pause-info-text", 2) + " " + (PrismUpgrades + 1).ToString();

        if (SaveManager.language == "jp") {
            text_prism_level.fontSize = 9f;
        } else if (SaveManager.language == "ru") {
            text_prism_level.fontSize = 7f;
        } else {
            text_prism_level.fontSize = 11f;
        }


        string dustprogress = GetDustProgressString();

        string levelprogress = RawD("pause-info-text", 1) + " "+cardsLeftForLevel.ToString();
        string goal = "";
        // Special messages (endings available, db stuff)
        if (DataLoader._getDS("ccc-entry") == 0) {
            goal = RawD("pause-info-goals", 10); // Explore the world!
        } else if (DataLoader._getDS("clean-db-seen") == 1 && Ano2Stats.prismCurrentDust + Ano2Stats.dust >= 150) {
            goal = RawD("pause-info-goals", 8); // Enter the prism and defeat the center!
        } else if (Ano2Stats.prismCurrentDust + Ano2Stats.dust >= Ano2Stats.ANODYNE_DUST_GOAL && Ano2Stats.prismCapacity >= 350) {
            goal = RawD("pause-info-goals", 6); // Return to do the anodyne!
        } else if (DataLoader._getDS("dustwallenter") == 1 && DataLoader._getDS("postdbspire") == 0) {
            goal = RawD("pause-info-goals", 4); // Warning, dust!
        } else if (DataLoader._getDS("dustwallenter") == 0 && DataLoader._getDS("pal-ring-3") == 1) {
            goal = RawD("pause-info-goals", 9); // Find pal

            // Conditional priority goals
        } else if (cardsLeftForLevel == 0 && possibleMaxPrismLevel > PrismUpgrades + 1) {
            goal = RawD("pause-info-goals", 1); //  upgrade the prism!
        } else if (Ano2Stats.dust >= Ano2Stats.GetMaxDust()) {
            goal = RawD("pause-info-goals", 0); //  deposit dust!

            // General goals
        } else if (1 == DataLoader._getDS("desert-sanc-2")) {
            goal = RawD("pause-info-goals", 7); //  Collect stuff to achieve the anodyne!
        } else if (IsDesertOpen) {
            goal = RawD("pause-info-goals", 5); //  Collect cards and dust! (before knowing ab tanodyne)
        } else if (IsRingOpen) {
            goal = RawD("pause-info-goals", 3); //  achieve open desert!
        } else {
            goal = RawD("pause-info-goals", 2); //  achieve open ring!
        }
        text_dust.text = dustprogress + "\n" + levelprogress + "\n" + goal;

        PauseHealthBar.SetHealth(SaveManager.currentHealth);

        dustbar.SetDust(Ano2Stats.dust);

        if (GameObject.Find("2D UI") != null) {
            UIManager2D ui2d = GameObject.Find("2D UI").GetComponent<UIManager2D>();
            if (!ui2d.HasMinimap()) {
                pauseMinimapPlayerPos.SetActive(false);
            } else if (ui2d.HasMinimap()) {
                int minimapWidth = ui2d.GetMinimapTileWidth();
                int minimapHeight = ui2d.GetMinimapTileHeight();
                int[,] visitedState = ui2d.GetMinimapVisitedState();
                int[,] visualState = ui2d.GetMinimapVisualState();
                float halfMinimapWidth = minimapWidth * 10f * 0.5f;
                float halfMinimapHeight = minimapHeight * 10f * 0.5f;
                Vector2Int minimapPlayerPos = ui2d.GetPlayerMinimapPos();
                pauseMinimapPlayerPos.GetComponent<RectTransform>().anchoredPosition = new Vector3(minimapPlayerPos.x * 10, minimapPlayerPos.y * -10, 0);
                Vector3 tempV3 = new Vector3();
                // ONLY ONCE
                // Add tiles, set their visual frames, position, size.
                if (GameMinimapSpritesheet == null) {
                    GameMinimapSpritesheet = ui2d.GetMinimapSpritesheet();
                    for (int row = 0; row < minimapHeight; row++) {
                        for (int col = 0; col < minimapWidth; col++) {
                            if (visualState[row, col] != 0) {
                                Image tile = Instantiate(MinimapTilePrefab, MinimapTilePrefab.transform.parent).GetComponent<Image>();
                                tile.sprite = GameMinimapSpritesheet[visualState[row, col]];
                                minimapTiles.Add(tile);
                                tempV3 = tile.rectTransform.anchoredPosition;
                                tempV3.x = col * 5f * 2;
                                tempV3.y = row * -5f * 2;
                                tempV3.z = 0;
                                tile.rectTransform.anchoredPosition = tempV3;
                                tempV3 = 2 * Vector3.one;
                                tile.rectTransform.localScale = tempV3;
                            }
                        }
                    }
                    tempV3 = pauseMinimap.GetComponent<RectTransform>().anchoredPosition;
                    tempV3.Set(-15f, 28f,0);
                    tempV3.x -= halfMinimapWidth;
                    tempV3.y += halfMinimapHeight;
                    pauseMinimap.GetComponent<RectTransform>().anchoredPosition = tempV3;
                    MinimapTilePrefab.SetActive(false);
                }
                // Each time pause menu opened/visuals refreshed:
                pauseMinimapPlayerPos.transform.SetAsLastSibling();
                int idx = 0;
                for (int row = 0; row < minimapHeight; row++) {
                    for (int col = 0; col < minimapWidth; col++) {
                        if (visualState[row, col] != 0) {
                            if (visitedState[row, col] == 1) {
                                minimapTiles[idx].gameObject.SetActive(true);
                            } else {
                                minimapTiles[idx].gameObject.SetActive(false);
                            }
                            idx++;
                        }
                    }
                }
            }
        } else {
            pauseMinimapPlayerPos.SetActive(false);
            MinimapTilePrefab.SetActive(false);
        }


        // Update Card Menu
        SetCardsVisibleStartingWithRow(abs_TopVisibleCardRow);
        TrySetBigcardVisibleBasedOnCursorPos(abs_TopVisibleCardRow, rel_CardCursorRow, rel_CardCursorCol);
        text_cards_read.text = DataLoader.instance.getDialogLine("pause-info-text", 6);

        // Update inventory
        if (!Ano2Stats.HasItem(5)) Ano2Stats.GetItem(5);
        if (!Ano2Stats.HasItem(13) && DataLoader._getDS(Registry.FLAG_DESERT_OPEN) == 1) Ano2Stats.GetItem(13);
        if (!Ano2Stats.HasItem(12) && DataLoader._getDS("desert-db-after") == 1) Ano2Stats.GetItem(12);

        if (invIndexToID == null) invIndexToID = new List<int>();
        invIndexToID.Clear();
        for (int i = 0; i < 30; i++) {
            if (i >= 15 && i <= 18 && DataLoader._getDS("desert-dog") == 2) {
            } else if (Ano2Stats.HasItem(i)) {
                invIndexToID.Add(i);
            }
        }
        while (invIndexToID.Count < 30) {
            invIndexToID.Add(100);
        }

        SetItemsVisibleStartingWithRow(abs_TopVisibleItemRow);
        TrySetBigitemVisibleBasedOnCursorPos(abs_TopVisibleItemRow, rel_ItemCursorRow, rel_ItemCursorCol);
        text_inv_read.text = DataLoader.instance.getDialogLine("pause-info-text", 6);

        // refresh metaclean info
        idx_MC_curTopChoice = 0;
        rel_MC_cursorRow = 0;
        RefreshMC_ChoiceList();
        RefreshMCGraphics();


        if (DataLoader.instance.isTitle) {
            // Turn off stuf
            TitleTurnOffer("Menu Choices");
            TitleTurnOffer("menu_selector");
        }

        if (useDarkenedHorrorUI) {

        }
    }

    void TitleTurnOffer(string name) {
        if (GameObject.Find(name) != null) GameObject.Find(name).SetActive(false);
    }

    string RawD(string scene, int linenumber) {
        return DataLoader.instance.getRaw(scene, linenumber);
    }

    [Header("Debug")]
    [Range(0,24)]
    public int DEBUG_CARDS_FOUND = 0;
    [Range(0f,1f)]
    public float RANDOM_CARD_CHANCE = 0;
    [Range(0,6)]
    public int DEBUG_PRISM_UPGRADES = 0;
    [Range(0,350)]
    public int DEBUG_PRISM_DUST = 0;
    [Range(0, 90)]
    public int DEBUG_CANISTER_DUST = 0;
    public bool DEBUG_DESERT_OPEN = false;
    public bool DEBUG_RING_OPEN = false;
    [Range(0,6)]
    public int DEBUG_SAVE_HEALTH_UPS = 0;
    public bool DEBUG_SAVE_DOUBLE_H = false;
    [Range(0, 1)]
    public float RANDOM_ITEM_CHANCE = 0;
    void update_debug_shortcuts() {
        if (Registry.DEV_MODE_ON && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Space)) {
            SaveManager.healthUpgrades = DEBUG_SAVE_HEALTH_UPS;
            SaveManager.doubleHealth = DEBUG_SAVE_DOUBLE_H;
            Ano2Stats.dust = DEBUG_CANISTER_DUST;
            DataLoader.instance.setDS("ddp-open-ring1", DEBUG_RING_OPEN ? 1 : 0);
            DataLoader.instance.setDS("ddp-open-ring2", DEBUG_DESERT_OPEN ? 1 : 0);
            Ano2Stats.prismCurrentDust = DEBUG_PRISM_DUST;
            Ano2Stats.prismCapacity = 50 + DEBUG_PRISM_UPGRADES * 50;
            if (!DEBUG_DESERT_OPEN && DEBUG_CARDS_FOUND > 12) DEBUG_CARDS_FOUND = 12;
            if (!DEBUG_RING_OPEN && DEBUG_CARDS_FOUND > 4) DEBUG_CARDS_FOUND = 4;
            Ano2Stats.ResetCards();
            for (int i = 0; i < DEBUG_CARDS_FOUND; i++) {
                Ano2Stats.GetCard(i);
            }
            for (int i = 0; i < DEBUG_PRISM_UPGRADES*4; i++) {
                Ano2Stats.UseCard(i);
            }

            if (RANDOM_CARD_CHANCE > 0) {
                for (int i =0; i < 24; i++) {
                    if (UnityEngine.Random.value <= RANDOM_CARD_CHANCE) {
                        Ano2Stats.GetCard(i);
                    }
                }
            }
            if (RANDOM_ITEM_CHANCE > 0) {
                for (int i = 0; i < 30; i++) {
                    Ano2Stats.RemoveItem(i);
                    if (UnityEngine.Random.value <= RANDOM_ITEM_CHANCE) {
                        Ano2Stats.GetItem(i);
                    }
                }
            }

            RefreshPauseVisuals();
        }
    }

    Vector3 FirstTopLevelChoice_Pos = new Vector3();
    void SetTopLevelChoicePositions(TopLevelMode[] choices) {
        if (FirstTopLevelChoice_Pos.x == 0) {
            FirstTopLevelChoice_Pos = choice_information.rectTransform.localPosition;
        }
        Vector3 tempTopLevelPos = new Vector3();
        float TopLevelChoice_Y_Offset = 18f;
        for (int i = 0; i < choices.Length; i++) {
            tempTopLevelPos = FirstTopLevelChoice_Pos;
            tempTopLevelPos.y -= TopLevelChoice_Y_Offset * i;
            if (choices[i] == TopLevelMode.Information) choice_information.rectTransform.localPosition = tempTopLevelPos;
            if (choices[i] == TopLevelMode.Cards) choice_cards.rectTransform.localPosition = tempTopLevelPos;
            if (choices[i] == TopLevelMode.Inventory) choice_inventory.rectTransform.localPosition = tempTopLevelPos;
            if (choices[i] == TopLevelMode.Metaclean) choice_metaclean.rectTransform.localPosition = tempTopLevelPos;
            if (choices[i] == TopLevelMode.Settings) choice_settings.rectTransform.localPosition = tempTopLevelPos;
            if (choices[i] == TopLevelMode.ReturnToTitle) choice_returntotitle.rectTransform.localPosition = tempTopLevelPos;
            if (choices[i] == TopLevelMode.QuitGame) choice_quitgame.rectTransform.localPosition = tempTopLevelPos;
        }
    }

    TopLevelMode[] topLevelChoices;
    int menu_selectorIndex;
	public void activate() {
        // DataLoader.getSceneSpecificRefs will do the scene entry deactivation of the pausemenu object to hide it.
        if (useDarkenedHorrorUI) {
            topLevelChoices = new TopLevelMode[] { TopLevelMode.Settings, TopLevelMode.QuitGame };
        } else if (DataLoader.instance.getDS("ddp-open-ring2") == 1 || MyInput.shortcut) {
            if (MyInput.shortcut) {
                print("<color=red>warning turned on mc</color>");
                DataLoader.instance.setDS("ddp-open-ring2", 1);
            }
            topLevelChoices = new TopLevelMode[] { TopLevelMode.Information, TopLevelMode.Cards, TopLevelMode.Inventory, TopLevelMode.Metaclean, TopLevelMode.Settings, TopLevelMode.QuitGame };
        } else {
            topLevelChoices = new TopLevelMode[] { TopLevelMode.Information, TopLevelMode.Cards, TopLevelMode.Inventory, TopLevelMode.Settings, TopLevelMode.QuitGame };
        }
        SetTopLevelChoicePositions(topLevelChoices);
        mode = TopLevelMode.OpeningMenu;
        SetVisibilityOfTopLevelUI(true);
        if (DataLoader.instance.isTitle) {
            SetTopLevelVisibleSubmenu(TopLevelMode.Settings);
            menu_selectorIndex = 3;
        } else {
            SetTopLevelVisibleSubmenu(topLevelChoices[0]);
            menu_selectorIndex = 0;
            openSound();
        }
        RefreshPauseVisuals();
        SetMenuSelectorPosition(menu_selectorIndex);
        // Settings stuff
		submode = 1;
		cursorIndex = 0;
		tFade = 0;
        setSettingsText();
    }

    GameObject TopLevel;
    void SetVisibilityOfTopLevelUI(bool visible) {
        TopLevel.SetActive(visible);
        if (useDarkenedHorrorUI && visible) {
            Color tempcol = new Color();
            ColorUtility.TryParseHtmlString("#050505FF", out tempcol);
            GameObject.Find("PauseMenuBG").GetComponent<Image>().color = tempcol;
            GameObject.Find("menu_selector_blocker").GetComponent<Image>().enabled = true;
        }
    }

	public void deactivate() {
	}

	int[] settingsChoices;
    /// <summary>
    /// Increment or decrement an index with up or down input
    /// </summary>
    /// <returns>Whether the index changed or not.</returns>
    bool ChangeUpDownIndex(ref int index, int min, int max) {
        int oldIndex = index;
        if (MyInput.jpUp) { index--; }
        if (MyInput.jpDown) { index++; }
        index = (int)Mathf.Clamp(index, min,max);
        return oldIndex != index;
    }

    void SetTopLevelVisibleSubmenu(TopLevelMode m) {
        InformationMenu.SetActive(m == TopLevelMode.Information);
        CardsMenu.SetActive(m == TopLevelMode.Cards);
        InventoryMenu.SetActive(m == TopLevelMode.Inventory);
        SettingsMenu.SetActive(m == TopLevelMode.Settings ||  m == TopLevelMode.QuitGame);
        MetacleanMenu.SetActive(m == TopLevelMode.Metaclean);
        if (m == TopLevelMode.QuitGame || m == TopLevelMode.Settings) {
            textChoice.alpha = 1;
        }
        if (m == TopLevelMode.QuitGame) {
            setQuitText();
        } else if (m == TopLevelMode.Settings) {
            setSettingsText();
        }
    }

    int tutorialMode = 0;
    public void StartTutorialCutscene() {
        mode = TopLevelMode.PrismTutorial;
    }
    int startingHeldDust = 0;
    int startingPrismDust = 0;
    public void StartDustDeposit(int _dustToDeposit,int _startingHeldDust, int _startingPrismDust) {
        dustToDeposit = _dustToDeposit;
        startingPrismDust = _startingPrismDust;
        startingHeldDust = _startingHeldDust;
            
        mode = TopLevelMode.DustDeposit;
    }

    bool doAutoCursorMoveUp = false;
    bool doAutoCursorMoveDown = false;
    public static bool hideVersionTextAndStuff = false;
    void Update() {


        if (Registry.DEV_MODE_ON && Input.GetKey(KeyCode.RightShift) && Input.GetKeyDown(KeyCode.V)) {
            if (GameObject.Find("UI Overlay") != null) {
                GameObject.Find("UI Overlay").GetComponent<Image>().enabled = false;
            }
        }

        doAutoCursorMoveDown = doAutoCursorMoveUp = false;
        if (!MyInput.down && !MyInput.up) {
            t_AutoMoveCursor = 0;
        } else {
            t_AutoMoveCursor += Time.deltaTime;
            if (t_AutoMoveCursor >= tm_AutoMoveCusorFirstTime) {
                t_AutoMoveCursor -= tm_AutoMoveCursorInterval;
                if (MyInput.up) {
                    doAutoCursorMoveUp = true;
                } else {
                    doAutoCursorMoveDown = true;
                }
            }
        }

        if (!didinit) {
            didinit = true;
            SetVisibilityOfTopLevelUI(false);
            SetTopLevelVisibleSubmenu(TopLevelMode.Closed);
        }

        if (mode != TopLevelMode.Closed) UpdateBottomLeftText();

        if (true && Registry.DEV_MODE_ON) {
            update_debug_shortcuts();
        }

        if (mode == TopLevelMode.Closed) {

        } else if (mode == TopLevelMode.PrismTutorial) {
            UpdatePrismTutorial();
        } else if (mode == TopLevelMode.DustDeposit) {
            UpdateDustDeposit();
        } else if (mode == TopLevelMode.OpeningMenu) {
            mode = TopLevelMode.TopLevel;
        } else if (mode == TopLevelMode.ClosingMenu) {
            mode = TopLevelMode.Closed;
            tBottomLeft = 0;
            SetTopLevelVisibleSubmenu(TopLevelMode.Closed);
            SetVisibilityOfTopLevelUI(false);
        } else if (mode == TopLevelMode.TopLevel) {
            if (dbox != null && !dbox.isDialogFinished()) {
                return;
            }
            #region menujoke
            if (menu_selectorIndex == 0 && MyInput.jpUp) {
                menuJokeCtr++;
                if (menuJokeCtr >= 14) {
                    menuJokeCtr = 0;
                    dbox.playDialogue("menujoke", 1);
                }
            } else if (menu_selectorIndex == 0 && MyInput.jpDown) {
                menuJokeCtr = 0;
            } else if (menu_selectorIndex == topLevelChoices.Length -1 && MyInput.jpDown) {
                menuJokeCtr++;
                if (menuJokeCtr >= 14) {
                    menuJokeCtr = 0;
                    dbox.playDialogue("menujoke", 0);
                }
            } else if (menu_selectorIndex == topLevelChoices.Length - 1 && MyInput.jpUp) {
                menuJokeCtr = 0;
            }
            #endregion

            if (doAutoCursorMoveUp) MyInput.jpUp = true;
            if (doAutoCursorMoveDown) MyInput.jpDown = true;

            if (ChangeUpDownIndex(ref menu_selectorIndex, 0, topLevelChoices.Length - 1)) {
                moveSound();
                SetTopLevelVisibleSubmenu(topLevelChoices[menu_selectorIndex]);
                SetMenuSelectorPosition(menu_selectorIndex);
            } else if (MyInput.jpCancel || MyInput.jpPause) {
                mode = TopLevelMode.ClosingMenu;
                menuJokeCtr = 0;
                closeSound();
            } else if (MyInput.jpConfirm || DataLoader.instance.isTitle) {
                mode = topLevelChoices[menu_selectorIndex];
                if (DataLoader.instance.isTitle) mode = TopLevelMode.Settings;
                if (mode == TopLevelMode.Information) mode = TopLevelMode.TopLevel;
                if (mode == TopLevelMode.Cards) OnEnterCards();
                if (mode == TopLevelMode.Inventory) OnEnterInventory();
                if (mode == TopLevelMode.Metaclean) OnEnterMetaclean();
                if (mode == TopLevelMode.Settings) OnEnterSettings();
                if (mode == TopLevelMode.QuitGame) OnEnterQuit();
                confirmSound();
            }

        } else if (mode == TopLevelMode.Cards) {
            UpdateCards();
        } else if (mode == TopLevelMode.Inventory) {
            UpdateInventory();
        } else if (mode == TopLevelMode.Metaclean) {
            UpdateMetaclean();
        } else if (mode == TopLevelMode.Settings) {
            UpdateSettings();
        } else if (mode == TopLevelMode.QuitGame) {
            UpdateQuit();
        }

    }

    bool fadingToTitle = false;

    float tBottomLeft;
    string time = "";
    string controlspart = "";
    void UpdateBottomLeftText() {

        tBottomLeft -= Time.deltaTime;
        if (tBottomLeft < 0) {
            tBottomLeft = 0.4f;
            controlspart = DataLoader.instance.getDialogLine("pause-info-text", 4) + "\n" + DataLoader.instance.getDialogLine("pause-info-text", 5);
        }


        time = MyInput.getTimeString((int)SaveManager.playtime);
        if (DataLoader.instance.isTitle) {
            BottomLeftText.text = controlspart;
        } else {
            BottomLeftText.text = time + "\n" + controlspart;
        }
        if (useDarkenedHorrorUI) {
            BottomLeftText.text = "";
        }
        

    }


    Vector3 init_cardcursor_localpos = new Vector3();
    int abs_TopVisibleCardRow = 0;
    int rel_CardCursorRow = 0;
    int rel_CardCursorCol = 0;
    int CardCursorHorSpacing = 34;
    int CardCursorVertSpacing = 42;
    void OnEnterCards() {
        menu_selector.GetComponent<SpriteAnimator>().Play("off");
        cardcursor.enabled = true;
    }
    void OnExitCards() {
        menu_selector.GetComponent<SpriteAnimator>().Play("idle");
        cardcursor.enabled = false;
        mustrelease = false;
    }

    // Starts with trying to show -1 onwards
    void SetCardsVisibleStartingWithRow(int topVisibleRow) {
        for (int i =0; i < 4; i++) {
            for (int j = 0; j < 3; j++) {
                int cardidx = topVisibleRow * 3 + i * 3 + j - 1;
                if (cardidx == 24 || cardidx == 25) {
                    smallCards[i, j].enabled = false;
                } else {
                    smallCards[i, j].enabled = true;
                }
                if (cardidx == -1 && Ano2Stats.HasCard(-2)) {
                    smallCards[i, j].sprite = smallCardFrames[28];
                } else if (Ano2Stats.HasCard(cardidx)) {
                    if (DataLoader.instance.hasLineBeenRead("cards",cardidx+1)) {
                        smallCards[i, j].sprite = smallCardFrames[topVisibleRow * 3 + i * 3 + j + 1 - 1];
                    } else {
                        smallCards[i, j].sprite = smallCardFrames[29];
                    }
                } else {
                    smallCards[i, j].sprite = cardSpot;
                }
            }
        }
    }

    void TrySetBigcardVisibleBasedOnCursorPos(int toprow, int cursorrow, int cursorcol) {
        int cardidx = toprow * 3 + cursorrow * 3 + cursorcol -1;
        bigCard.enabled = true;
        if (cardidx == -1 && Ano2Stats.HasCard(-2)) { // torn pal card
            bigCard.sprite = bigCardFrames[28];
        } else if (cardidx == 24 || cardidx == 25) {
            bigCard.enabled = false;
        } else if (Ano2Stats.HasCard(cardidx)) {
            bigCard.sprite = bigCardFrames[cardidx + 1];
            if (!DataLoader.instance.hasLineBeenRead("cards", cardidx + 1)) {
                bigCard.sprite = bigCardFrames[29];
            }
        } else {
            bigCard.sprite = bigCardFrames[29];
            bigCard.enabled = false;
        }
    }

    void UpdateCardScrollbarPos() {
        tempLocalPos = cards_scrollbar.transform.localPosition;
        tempLocalPos.y = 37 - abs_TopVisibleCardRow * (37 - -21)/5f;
        cards_scrollbar.transform.localPosition = tempLocalPos;
    }

    bool mustrelease = false;
    void UpdateCards() {
        if (mustrelease) {
            if (!MyInput.confirm && !MyInput.jpConfirm && dbox.isDialogFinished()) {
                mustrelease = false;
            } else {
                return;
            }
        }
        if (MyInput.jpCancel && dbox.isDialogFinished()) {
            cancelSound();
            OnExitCards();
            mode = TopLevelMode.TopLevel;
        } else if (MyInput.jpConfirm && !mustrelease && dbox.isDialogFinished()) {
            int cardidx = abs_TopVisibleCardRow * 3 + rel_CardCursorRow * 3 + rel_CardCursorCol - 1;
            if (cardidx == -1 && Ano2Stats.HasCard(-2)) {
                mustrelease = true;
                dbox.playDialogue("cards", 25);

                DataLoader.instance.updateReadDialogueState("cards", cardidx+1, cardidx+1);
                TrySetBigcardVisibleBasedOnCursorPos(abs_TopVisibleCardRow, rel_CardCursorRow, rel_CardCursorCol);
                SetCardsVisibleStartingWithRow(abs_TopVisibleCardRow);
            } else if (Ano2Stats.HasCard(cardidx)) {
                mustrelease = true;
                dbox.playDialogue("cards", abs_TopVisibleCardRow * 3 + rel_CardCursorRow * 3 + rel_CardCursorCol);
                if (cardidx >= 0 && cardidx <= 23) {
                    smallCards[rel_CardCursorRow, rel_CardCursorCol].sprite = smallCardFrames[cardidx+1];
                }

                DataLoader.instance.updateReadDialogueState("cards", cardidx + 1, cardidx + 1);
                TrySetBigcardVisibleBasedOnCursorPos(abs_TopVisibleCardRow, rel_CardCursorRow, rel_CardCursorCol);
                SetCardsVisibleStartingWithRow(abs_TopVisibleCardRow);
            }
        } else {
            if (MyInput.jpUp || doAutoCursorMoveUp) {
                if (rel_CardCursorRow > 0) {
                    rel_CardCursorRow--;
                } else if (abs_TopVisibleCardRow > 0) {
                    abs_TopVisibleCardRow--;
                } else {
                    if (!doAutoCursorMoveUp) {
                        abs_TopVisibleCardRow = 5;
                        rel_CardCursorRow = 3;
                    }
                    doAutoCursorMoveUp = false;
                }
            } else if (MyInput.jpDown || doAutoCursorMoveDown) {
                if (rel_CardCursorRow < 3) {
                    rel_CardCursorRow++;
                } else if (abs_TopVisibleCardRow < 5) {
                    abs_TopVisibleCardRow++;
                } else {
                    if (!doAutoCursorMoveDown) {
                        abs_TopVisibleCardRow = 0;
                        rel_CardCursorRow = 0;
                    }
                    doAutoCursorMoveDown = false;
                }
            } else if (MyInput.jpRight) {
                if (rel_CardCursorCol < 2) rel_CardCursorCol++;
            } else if (MyInput.jpLeft) {
                if (rel_CardCursorCol > 0) rel_CardCursorCol--;
            }
            if (doAutoCursorMoveDown || doAutoCursorMoveUp || MyInput.jpUp || MyInput.jpRight || MyInput.jpDown || MyInput.jpLeft) {
                moveSound();
                UpdateCardScrollbarPos();
                TrySetBigcardVisibleBasedOnCursorPos(abs_TopVisibleCardRow, rel_CardCursorRow, rel_CardCursorCol);
                SetCardsVisibleStartingWithRow(abs_TopVisibleCardRow);
                tempLocalPos = init_cardcursor_localpos;
                tempLocalPos.y -= rel_CardCursorRow * CardCursorVertSpacing;
                tempLocalPos.x += rel_CardCursorCol * CardCursorHorSpacing;
                cardcursor.transform.localPosition = tempLocalPos;
            }
        }
    }


    Vector3 init_itemcursor_localpos = new Vector3();
    int abs_TopVisibleItemRow = 0;
    int rel_ItemCursorRow = 0;
    int rel_ItemCursorCol = 0;
    int ItemCursorSpacing = 34;

    void OnEnterInventory() {

        menu_selector.GetComponent<SpriteAnimator>().Play("off");
        itemcursor.enabled = true;
    }
    void OnExitInventory() {
        menu_selector.GetComponent<SpriteAnimator>().Play("idle");
        itemcursor.enabled = false;
        mustrelease = false;
    }

    void SetItemsVisibleStartingWithRow(int topVisibleRow) {
        float itemScale = (currentPauseUIScale) / currentPauseUIScale;
        for (int i = 0; i < 5; i++) {
            for (int j = 0; j < 3; j++) {
                smallItems[i, j].enabled = true;
                if (i == 4 && j == 2 && topVisibleRow == 3) smallItems[i, j].enabled = false;
                tempLocalPos = smallItems[i, j].transform.localScale;
                int itemID = invIndexToID[topVisibleRow * 3 + i * 3 + j];
                if (Ano2Stats.HasItem(itemID)) {
                    smallItems[i, j].sprite = smallItemFrames[itemID];
                    tempLocalPos.Set(itemScale, itemScale, itemScale);
                } else {
                    smallItems[i, j].sprite = itemSpot;
                    tempLocalPos.Set(1, 1, 1);
                }
                smallItems[i, j].transform.localScale = tempLocalPos;
            }
        }
    }

    void TrySetBigitemVisibleBasedOnCursorPos(int toprow, int cursorrow, int cursorcol) {
        int cardidx = invIndexToID[toprow * 3 + cursorrow * 3 + cursorcol];
        if (Ano2Stats.HasItem(cardidx)) {
            bigItem.enabled = true;
            bigItem.sprite = smallItemFrames[cardidx];
            text_item_name.text = RawD("item-names", cardidx);
        } else {
            text_item_name.text = "";
            bigItem.enabled = false;
        }
    }

    void UpdateSettingsScrollbarPos() {
        tempLocalPos = settings_scrollbar.transform.localPosition;
        tempLocalPos.y = 47 - top_visible_setting_choice_index * (47 - -31) / (settingsChoices.Length - max_visible_settings_choices);
        settings_scrollbar.transform.localPosition = tempLocalPos;
    }
    void UpdateInventoryScrollbarPos() {
        tempLocalPos = inventory_scrollbar.transform.localPosition;
        tempLocalPos.y = 47 - abs_TopVisibleItemRow * (47 - -31) / 3f;
        inventory_scrollbar.transform.localPosition = tempLocalPos;
    }
    List<int> invIndexToID;
    bool doingInvVision = false;
    DialogueAno2 InvVisionDA2;

    float t_AutoMoveCursor = 0;
    float tm_AutoMoveCusorFirstTime = 0.4f;
    float tm_AutoMoveCursorInterval = .10f;
    bool twoTalkInv = false;
    void UpdateInventory() {
        if (twoTalkInv) {
            if (dbox.isDialogFinished()) {
                dbox.playDialogue("card-detector", 4, 10);
                twoTalkInv = false;
                mustrelease = true;
            }
            return;
        }
        if (mustrelease) {
            if (!MyInput.confirm && !MyInput.jpConfirm && dbox.isDialogFinished()) {
                mustrelease = false;
            } else {
                return;
            }
        }
        if (doingInvVision) {
            if (DialogueAno2.AnyScriptIsParsing == false) {
                doingInvVision = false;
            }
            return;
        }

      

        if (MyInput.jpCancel && dbox.isDialogFinished()) {
            cancelSound();
            OnExitInventory();
            mode = TopLevelMode.TopLevel;
        } else if (MyInput.jpConfirm && !mustrelease && dbox.isDialogFinished()) {
            int itemID = invIndexToID[abs_TopVisibleItemRow * 3 + rel_ItemCursorRow * 3 + rel_ItemCursorCol];
            if (Ano2Stats.HasItem(itemID)) {
                mustrelease = true;
                if (itemID == 6 && Spark.ScaleSparkOn) {
                    Spark.ScaleSparkOn = false;
                    dbox.playDialogue("spark-unequip");
                } else if (itemID == 8 && GameObject.Find("palvision1") != null) {
                    InvVisionDA2 = GameObject.Find("palvision1").GetComponent<DialogueAno2>();
                    InvVisionDA2.ext_ForceInteractScriptToParse = true;
                    doingInvVision = true;
                } else if (itemID == 10) {
                    if (GameObject.Find("Metacoins") != null) {
                        Metacoin[] coins = GameObject.Find("Metacoins").GetComponentsInChildren<Metacoin>();
                        int i = 0;
                        foreach (Metacoin coin in coins) {
                            if (!coin.spawnerCode && coin.gameObject.activeInHierarchy) {
                                i++;
                            }
                        }
                        print(i + " coins remaining");
                        if (i > 0) {
                            dbox.playDialogue("metaclean-messages", 12);
                        } else {
                            dbox.playDialogue("metaclean-messages", 13);
                        }
                    } else {
                        if (GameObject.Find(Registry.PLAYERNAME2D) == null) {
                            dbox.playDialogue("metaclean-messages", 13);
                        } else {
                            dbox.playDialogue("metaclean-messages", 14);
                        }
                    }
                } else if (itemID == Ano2Stats.ITEM_ID_CARDDETECTOR) {

                    twoTalkInv = true;
                    if (curSceneName == "NanoOrb" && !(Ano2Stats.HasCard(12) && Ano2Stats.HasCard(13))) {
                        dbox.playDialogue("card-detector", 2);
                    } else if (curSceneName == "NanoSkeligum" && !(Ano2Stats.HasCard(14) && Ano2Stats.HasCard(15))) {
                        dbox.playDialogue("card-detector", 2);
                    } else if ((curSceneName == "NanoFantasy") && !(Ano2Stats.HasCard(16) && Ano2Stats.HasCard(17))) {
                        dbox.playDialogue("card-detector", 2);
                        // clearer if pico consistently returns 'no cards' vs showing it in a pico place where you cant reach treacl
                    } else if ((curSceneName == "NanoOcean") && !(Ano2Stats.HasCard(21) && Ano2Stats.HasCard(22) && Ano2Stats.HasCard(23))) {
                        dbox.playDialogue("card-detector", 2);
                    } else if (curSceneName == "DesertOrb" && !Ano2Stats.HasCard(18)) {
                        dbox.playDialogue("card-detector", 3);
                    } else if (curSceneName == "DesertField" && !Ano2Stats.HasCard(19)) {
                        dbox.playDialogue("card-detector", 3);
                    } else {
                        twoTalkInv = false;
                        if (GameObject.Find(Registry.PLAYERNAME2D) == null) {
                            dbox.playDialogue("card-detector", 0);
                        } else {
                            dbox.playDialogue("card-detector", 1);
                        }
                    }
                } else {
                    if (itemID == 6) {
                        Spark.ScaleSparkOn = true;
                    }
                    dbox.playDialogue("item-descriptions", itemID);
                }
            }
        } else {
            if (MyInput.jpUp || doAutoCursorMoveUp) {
                if (rel_ItemCursorRow > 0) {
                    rel_ItemCursorRow--;
                } else if (abs_TopVisibleItemRow > 0) {
                    abs_TopVisibleItemRow--;
                } else {
                    if (!doAutoCursorMoveUp) {
                        abs_TopVisibleItemRow = 3;
                        rel_ItemCursorRow = 4;
                    }
                    doAutoCursorMoveUp = false;
                }
            } else if (MyInput.jpDown || doAutoCursorMoveDown) {
                if (rel_ItemCursorRow < 4) {
                    rel_ItemCursorRow++;
                } else if (abs_TopVisibleItemRow < 3) {
                    abs_TopVisibleItemRow++;
                } else {
                    if (!doAutoCursorMoveDown) {
                        rel_ItemCursorRow = 0;
                        abs_TopVisibleItemRow = 0;
                    }
                    doAutoCursorMoveDown = false;
                }
            } else if (MyInput.jpRight) {
                if (rel_ItemCursorCol < 2) rel_ItemCursorCol++;
            } else if (MyInput.jpLeft) {
                if (rel_ItemCursorCol > 0) rel_ItemCursorCol--;
            }
            if (doAutoCursorMoveDown || doAutoCursorMoveUp || MyInput.jpUp || MyInput.jpRight || MyInput.jpDown || MyInput.jpLeft) {
                moveSound();
                UpdateInventoryScrollbarPos();
                TrySetBigitemVisibleBasedOnCursorPos(abs_TopVisibleItemRow, rel_ItemCursorRow, rel_ItemCursorCol);
                SetItemsVisibleStartingWithRow(abs_TopVisibleItemRow);
                tempLocalPos = init_itemcursor_localpos;
                tempLocalPos.y -= rel_ItemCursorRow * ItemCursorSpacing;
                tempLocalPos.x += rel_ItemCursorCol * ItemCursorSpacing;
                itemcursor.transform.localPosition = tempLocalPos;
            }
        }
    }





    void OnEnterMetaclean() {
        menu_selector.GetComponent<SpriteAnimator>().Play("off");
        metacursor.enabled = true;
        UpdateMCCursor();
        metaSubmode = 0;
    }
    void OnExitMetaclean() {
        menu_selector.GetComponent<SpriteAnimator>().Play("idle");
        metacursor.enabled = false;
        mustrelease = false;
    }


    int MC_Type_Dialogue = 0;
    int MC_Type_Commentary = 1;
    int MC_Type_Warp = 2;
    //int MC_Type_Item = 3;

    // Note, used ones:
    // 0 - 2 - Audio logs 
    // 3 - card detector 
    // 10 - 16 - Dev Lore
    // 20 - 35 - Bonus Warps (
    // 36 - 39 -  PG Convenience warps 
    int[] Metaclean_Costs =    new int[] { 3,  3,  3,  25,  5,  3,  4,  3,  5,  4, 10, 10, 10, 10, 10, 10, 10,  5,  5,  7,  9, 11,  5, 15,  7, 13,  5, 15,  5, 15,  8, 12,  7, 13,  5, 15, 10, 10, 10, 10,  10,  5,  5,  5,  5,  5};
    int[] Metaclean_Order =    new int[] { 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39,  4,  5,  6,  7,  8,  9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20,  0,  1,  2,  3, 40, 41, 42, 43, 44, 45 }; // a[idx] is n, then the n+1th item in the metaitem list would show in pos idx, assuming it's been bought.
    //int[] Metaclean_Order =  new int[] {  0,  1,  2,  3,  4,  5,  6,  7,  8,  9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45 }; // a[idx] is n, then the n+1th item in the metaitem list would show in pos idx, assuming it's been bought.
    int[] Metaclean_IconType = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,  1,  1,  1,  1,  1,  1,  1,  1,  1,  2,  2,  2,  2,  2,  2,  2,  2,  2,  2,  2,  2,  2,  2,  2,  2,  2,  2,  2,  2,  2,  2,  2,  2,  2,  2,  2};


    void MaybeAddPurchasableMetaItem(ref List<int> l, int idx, int nrtoAdd=3) {
        if (DataLoader.instance.getDS("META" + idx) == 0) {
            if (l.Count < nrtoAdd) l.Add(idx);
        }
    }
    void AddPurchasableMetaItems(ref List<int> l, int nrToAdd) {

        //print("<color=red>MC DEBUG</color>");
        //DataLoader._setDS("VisitedDrawer", 1);
        //Ano2Stats.GetItem(Ano2Stats.ITEM_ID_SCALESPARK);
        //DataLoader._setDS(Registry.FLAG_SAW_GOODEND,1);
        

        if (DataLoader._getDS("VisitedDrawer") == 1) {
            MaybeAddPurchasableMetaItem(ref l, 25); //  debug2d
            MaybeAddPurchasableMetaItem(ref l, 1); // audio log 2
        }

        if (DataLoader._getDS(Registry.FLAG_SAW_GOODEND) == 1) {
            MaybeAddPurchasableMetaItem(ref l, 2); // audio log 3
            MaybeAddPurchasableMetaItem(ref l, 15); // Sands II info 
            MaybeAddPurchasableMetaItem(ref l, 36); // CCC Warp
            MaybeAddPurchasableMetaItem(ref l, 37); // RingHighway Warp
            MaybeAddPurchasableMetaItem(ref l, 38); // DesertField Warp
            MaybeAddPurchasableMetaItem(ref l, 39); // Nexus Warp
        //    MaybeAddPurchasableMetaItem(ref l, 16); // Finale Info
        }

        MaybeAddPurchasableMetaItem(ref l, 3); // Cardtector
        MaybeAddPurchasableMetaItem(ref l, 11); // CCC
        MaybeAddPurchasableMetaItem(ref l, 12); // Vale1
        MaybeAddPurchasableMetaItem(ref l, 4); // freewrite


        // Add other areas
        if (DataLoader._getDS("read-a-metainfo") == 1) {
            MaybeAddPurchasableMetaItem(ref l, 20); //  metazone: r1n
            if (DataLoader._getDS("visited-r1n") == 1) {
                MaybeAddPurchasableMetaItem(ref l, 19); //  
                MaybeAddPurchasableMetaItem(ref l, 21); //  
                MaybeAddPurchasableMetaItem(ref l, 22); // 
                MaybeAddPurchasableMetaItem(ref l, 5); // freewrite 
                MaybeAddPurchasableMetaItem(ref l, 23); //  
                MaybeAddPurchasableMetaItem(ref l, 24); //  

                MaybeAddPurchasableMetaItem(ref l, 6); // freewrite
                MaybeAddPurchasableMetaItem(ref l, 13); //  Vale2
                MaybeAddPurchasableMetaItem(ref l, 26); // 
                MaybeAddPurchasableMetaItem(ref l, 27); //  
                MaybeAddPurchasableMetaItem(ref l, 28); //  
                MaybeAddPurchasableMetaItem(ref l, 7); // freewrite

                MaybeAddPurchasableMetaItem(ref l, 29); //  
                MaybeAddPurchasableMetaItem(ref l, 30); //  
                MaybeAddPurchasableMetaItem(ref l, 14); //  sands1
                MaybeAddPurchasableMetaItem(ref l, 31); //  
                MaybeAddPurchasableMetaItem(ref l, 8); // freewrite
                MaybeAddPurchasableMetaItem(ref l, 32); //  
                //MaybeAddPurchasableMetaItem(ref l, 33); //  remove motion sick
                MaybeAddPurchasableMetaItem(ref l, 34); //  
                MaybeAddPurchasableMetaItem(ref l, 35); //  
                MaybeAddPurchasableMetaItem(ref l, 9); // freewrite

            }
        }


        while (l.Count < nrToAdd) {
            l.Add(-2); // corresponds to "---" 
        }
    }
    void RefreshMC_ChoiceList() {
        MC_ChoiceList.Clear();
        AddPurchasableMetaItems(ref MC_ChoiceList, MC_MaxPurchasableChoices);
        MC_ChoiceList.Add(-1); // -1 corresponds to metacoin section of menu
        /*
            - Go through itemorder array
            - use each index to check corresponding META(n)
            - if (1), set the ChoiceList thing to 1000 + n
            - if2, set to n
        */
        // Add purchased items

        // e.g. [2,1,0]
        // 
        int nrOfMetaitems = Metaclean_Order.Length;

        for (int idx = 0; idx < nrOfMetaitems; idx++) {
            for (int mc_id = 0; mc_id < nrOfMetaitems; mc_id++) {
                // for the 0th choice in our list, we need to find the index of the member of Order that is '0'. Then we take that things metaclean_id and add it to the choice list.
                if (Metaclean_Order[mc_id] == idx) {
                    int stateOfChoice = DataLoader.instance.getDS("META" + mc_id);
                    if (stateOfChoice == 1) {
                        MC_ChoiceList.Add(mc_id + 1000);
                        break;
                    } else if (stateOfChoice == 2) {
                        MC_ChoiceList.Add(mc_id);
                        break;
                    }
                }
            }
        }

        // Add empty slots if nothing bought yet
        if (MC_ChoiceList.Count < MC_maxVisibleChoices) {
            while (MC_ChoiceList.Count < MC_maxVisibleChoices) {
                MC_ChoiceList.Add(-2);
            }
        }
    }
    string metaShopColorOpen = "<color=#B4FFC7>";
    string metaShopColorClose = "</color>";
    int MC_MaxPurchasableChoices = 3;
    int MC_maxVisibleChoices = 10;
    int MC_defaultShopBG_Height = 16;
    int MC_defaultShopBG_ChunkHeight = 15;
    int rel_MC_cursorRow = 0;
    Vector2 MC_sizeDelta = new Vector2();
    // assumes idx_topmostMetaChoice + 9 is valid.
    void RefreshMCGraphics() {
        // Using the MetacleanChoiceList that was built in RefreshMetaChoices(), add the purchasables if needed
        text_metaCosts.text = "";
        text_metaChoices.text = "";
        int iconIndex = 0;
        int idx_MC = idx_MC_curTopChoice;
        if (idx_MC < MC_MaxPurchasableChoices) {
            text_metaCosts.text = metaShopColorOpen;
            text_metaChoices.text = metaShopColorOpen;
            for (int i = idx_MC; i < MC_MaxPurchasableChoices; i++) {
                if (MC_ChoiceList[i] == -2) {
                    text_metaCosts.text += "-\n";
                    text_metaChoices.text += "---\n";
                    metaicons[iconIndex].enabled = false;
                } else {
                    text_metaCosts.text += Metaclean_Costs[MC_ChoiceList[i]] + "\n";
                    text_metaChoices.text += RawD("metaclean-names", MC_ChoiceList[i]) + "\n";

                    metaicons[iconIndex].enabled = true;

                    int icontype = Metaclean_IconType[MC_ChoiceList[i]];
                    int spriteIdx = 1;
                    if (icontype == 1) spriteIdx = 3;
                    if (icontype == 2) spriteIdx = 5;
                    metaicons[iconIndex].sprite = metaicon_Sprites[spriteIdx];
                }
                iconIndex++;
                idx_MC++;
            }
            text_metaCosts.text += metaShopColorClose;
            text_metaChoices.text += metaShopColorClose;
        }
        // set height of metashop BG
        MC_sizeDelta = MetashopBG_t.sizeDelta;
        if (iconIndex == 0) {
            MetashopBG_t.GetComponent<Image>().enabled = false;
        } else {
            MetashopBG_t.GetComponent<Image>().enabled = true;
            if (iconIndex == 1) MC_sizeDelta.y = MC_defaultShopBG_Height;
            if (iconIndex == 2) MC_sizeDelta.y = MC_defaultShopBG_Height + MC_defaultShopBG_ChunkHeight;
            if (iconIndex == 3) MC_sizeDelta.y = MC_defaultShopBG_Height + 2 * MC_defaultShopBG_ChunkHeight;
            MetashopBG_t.sizeDelta = MC_sizeDelta;
        }

        // Default is set metacoin bg invisible
        MetacoinBG_t.GetComponent<Image>().enabled = false;

        // add the remaining 6-10 choices.
        for (int i = idx_MC; i < idx_MC_curTopChoice + MC_maxVisibleChoices; i++) {
            int idx_metaItemList= MC_ChoiceList[i];
            // move metacoin bg
            if (idx_metaItemList == -1) {
                MC_tempPos = MetacoinBG_t.anchoredPosition;
                MC_tempPos.y = 119 - 15 * iconIndex;
                MetacoinBG_t.anchoredPosition = MC_tempPos;
                MetacoinBG_t.GetComponent<Image>().enabled = true;
                text_metaChoices.text += RawD("metaclean-messages", 3);
                text_metaCosts.text += RawD("metaclean-messages", 2) + ": " + SaveManager.metacoins;
                metaicons[iconIndex].enabled = false;
            // No item, just show  '---'
            } else if (idx_metaItemList == -2) {
                text_metaChoices.text += "---";
                metaicons[iconIndex].enabled = false;
            // show item name
            } else {
                int choice = MC_ChoiceList[i];
                bool isNew = false;
                if (choice >= 1000) {
                    choice -= 1000;
                    isNew = true;
                }
                text_metaChoices.text += RawD("metaclean-names",choice);
                // Also set sprite!
                int icontype = Metaclean_IconType[choice];
                int spriteIdx = 1;
                if (icontype == 1) spriteIdx = 3;
                if (icontype == 2) spriteIdx = 5;
                if (isNew) spriteIdx -= 1;
                metaicons[iconIndex].enabled = true;
                metaicons[iconIndex].sprite = metaicon_Sprites[spriteIdx];

            }
            iconIndex++;
            text_metaChoices.text += "\n";
        }

        // resize scroll bar
        float scrollHeightAtMax = 158;
        float scrollHeightAtMin = 40;
        float minListSize = 10;
        float maxListSize = 20;


        int curHeight = Mathf.FloorToInt(scrollHeightAtMax - (scrollHeightAtMax - scrollHeightAtMin) * ((MC_ChoiceList.Count - minListSize) / (maxListSize - minListSize)));
        if (curHeight < scrollHeightAtMin) curHeight = (int)scrollHeightAtMin;
        MC_sizeDelta = metaclean_scrollbar.sizeDelta;
        MC_sizeDelta.y = curHeight;
        metaclean_scrollbar.sizeDelta = MC_sizeDelta;
        float maxTopChoice = MC_ChoiceList.Count - MC_maxVisibleChoices;
        if (maxTopChoice > 0) {
            float disToMove = (scrollHeightAtMax - curHeight) * (idx_MC_curTopChoice / maxTopChoice);
            disToMove = 37 - disToMove;
            MC_tempPos = metaclean_scrollbar.anchoredPosition;
            MC_tempPos.y = disToMove;
            metaclean_scrollbar.anchoredPosition = MC_tempPos;
        }

    }
    float metaCursorSpacing = 15f;
    int metaSubmode = 0;
    void UpdateMetaclean() {

        if (Registry.DEV_MODE_ON && Input.GetKey(KeyCode.RightShift) && Input.GetKeyDown(KeyCode.M)) {
            SaveManager.metacoins += 50;
            SaveManager.totalFoundCoins += 50;
        }
        if (Registry.DEV_MODE_ON && Input.GetKey(KeyCode.RightShift) && Input.GetKeyDown(KeyCode.N)) {
            DataLoader._setDS("read-a-metainfo",1);
            DataLoader._setDS("visited-r1n",1);
        }



        if (metaSubmode == 0) {
            if (MyInput.jpCancel) {
                cancelSound();
                OnExitMetaclean();
                mode = TopLevelMode.TopLevel;
            }
            #region Cursor Movement Code
            if (MyInput.jpUp || doAutoCursorMoveUp) {
                if (rel_MC_cursorRow > 0) {
                    rel_MC_cursorRow--;
                } else if (idx_MC_curTopChoice > 0) {
                    idx_MC_curTopChoice--;
                } else {
                    if (!doAutoCursorMoveUp) {
                        rel_MC_cursorRow = MC_maxVisibleChoices - 1;
                        idx_MC_curTopChoice = MC_ChoiceList.Count - MC_maxVisibleChoices;
                    }
                    doAutoCursorMoveUp = false;
                }
            } else if (MyInput.jpDown || doAutoCursorMoveDown) {
                if (rel_MC_cursorRow < MC_maxVisibleChoices - 1) {
                    rel_MC_cursorRow++;
                } else if (idx_MC_curTopChoice + MC_maxVisibleChoices < MC_ChoiceList.Count) {
                    idx_MC_curTopChoice++;
                } else {
                    if (!doAutoCursorMoveDown) {
                        rel_MC_cursorRow = 0;
                        idx_MC_curTopChoice = 0;
                    }
                    doAutoCursorMoveDown = false;
                }
            } else if (MyInput.jpLeft) {
                rel_MC_cursorRow -= MC_maxVisibleChoices - 1;
                if (rel_MC_cursorRow < 0) {
                    idx_MC_curTopChoice += rel_MC_cursorRow;
                    if (idx_MC_curTopChoice < 0) idx_MC_curTopChoice = 0;
                    rel_MC_cursorRow = 0;
                }
            } else if (MyInput.jpRight) {
                rel_MC_cursorRow += MC_maxVisibleChoices - 1;
                if (rel_MC_cursorRow >= MC_maxVisibleChoices) {
                    idx_MC_curTopChoice += (rel_MC_cursorRow - MC_maxVisibleChoices) + 1;
                    if (idx_MC_curTopChoice + MC_maxVisibleChoices >= MC_ChoiceList.Count) {
                        idx_MC_curTopChoice = MC_ChoiceList.Count - MC_maxVisibleChoices;
                    }
                    rel_MC_cursorRow = MC_maxVisibleChoices - 1;
                }
            }

            if (doAutoCursorMoveDown || doAutoCursorMoveUp || MyInput.jpUp || MyInput.jpRight || MyInput.jpDown || MyInput.jpLeft) {
                UpdateMCCursor();
                RefreshMCGraphics();
                moveSound();
            }
            #endregion

            // Code for playing dialogue or whatever when selecting an item
            if (MyInput.jpConfirm) {

                int idx_MC = idx_MC_curTopChoice + rel_MC_cursorRow;

                // Top 3 choices are always purchasable items
                if (idx_MC <= 2) {
                    // "Sold out!"
                    if (MC_ChoiceList[idx_MC] == -2) {
                        dbox.playDialogue("metaclean-messages", 4);
                        metaSubmode = 1;
                        // Ask if want to buy
                    } else if (SaveManager.metacoins >= Metaclean_Costs[MC_ChoiceList[idx_MC]]) {
                        metaYN = new YesNoPrompt("MetaCursor2", "Meta_YN", "metaclean-messages", 1);
                        GameObject.Find("MetaCursor2").GetComponent<Image>().enabled = true;
                        //Image Meta_YN_BG = GameObject.Find("Meta_YN_BG").GetComponent<Image>();
                        //Meta_YN_BG.enabled = true;
                        //MC_sizeDelta = Meta_YN_BG.rectTransform.sizeDelta;
                        //MC_sizeDelta.x = (int)GameObject.Find("Meta_YN").GetComponent<TMP_Text>().preferredWidth + 16;
                        //Meta_YN_BG.rectTransform.sizeDelta = MC_sizeDelta;
                        openSound();
                        metaSubmode = 2;
                        // Not enough money.
                    } else {
                        dbox.playDialogue("metaclean-messages", 0);
                        metaSubmode = 1;
                    }
                    // Third choice is always a dividerr
                } else if (idx_MC == 3) {
                    // nothing
                } else {
                    // Set META[N] to 2, to denote it has been read.
                    int itemID = MC_ChoiceList[idx_MC];
                    int icontypeidx = itemID >= 1000 ? itemID - 1000 : itemID;
                    if (icontypeidx >= Metaclean_IconType.Length) return;
                    if (icontypeidx < 0) return;
                    int itemType = Metaclean_IconType[icontypeidx];
                    // Distinguish the item type to decide what to do next.
                    if (itemType == MC_Type_Dialogue || itemType == MC_Type_Commentary) {
                        if (itemID >= 1000) {
                            DataLoader.instance.setDS("META" + (itemID - 1000), 2);
                            itemID -= 1000;
                        }
                        if (itemID >= 4 && itemID <= 9) {
                            dbox.playDialogue("metaclean-freewrite", (itemID - 4));
                            metaSubmode = 1;
                        } else {
                            dbox.playDialogue("metaclean-desc", itemID);
                            if (itemType == MC_Type_Commentary) {
                                metaSubmode = 90;
                            } else {
                                metaSubmode = 1;
                            }
                        }
                    } else if (itemType == MC_Type_Warp) {
                        // Block metawarps from 2D or Metazone

                        if (itemID >= 1000) itemID -= 1000;
                        // No metawarps while in 2D
                        if (GameObject.Find(Registry.PLAYERNAME2D) != null && itemID < 36) {
                            dbox.playDialogue("metaclean-messages", 8);
                            metaSubmode = 1;
                        // no metawarps in a metazone
                        } else if (inMetazone && itemID < 36) {
                            dbox.playDialogue("metaclean-messages", 10);
                            metaSubmode = 1;
                        // no warps if there's no recent 3D entrance
                        } else if (itemID < 36 && Registry.destinationDoorNameForPauseRespawn == "") {
                            dbox.playDialogue("metaclean-messages", 11);
                            metaSubmode = 1;
                        } else {
                            if (itemID >= 36 && itemID <= 39) {
                                metaYN = new YesNoPrompt("MetaCursor2", "Meta_YN", "metaclean-messages", 9);
                            } else {
                                metaYN = new YesNoPrompt("MetaCursor2", "Meta_YN", "metaclean-messages", 6);
                            }
                            GameObject.Find("MetaCursor2").GetComponent<Image>().enabled = true;
                            metaSubmode = 3;
                        }
                    }

                    RefreshMC_ChoiceList();
                    RefreshMCGraphics();

                }
            }
        } else if (metaSubmode == 90) { // "seek the knowledge and learn.."
            if (dbox.isDialogFinished()) {
                dbox.playDialogue("metaclean-messages", 7);
                metaSubmode = 1;
            }
        } else if (metaSubmode == 1) {
            if (dbox.isDialogFinished()) {
                if (!MyInput.jpConfirm && !MyInput.confirm) {
                    metaSubmode = 0;
                }
            }
        } else if (metaSubmode == 2) {
            int retval = metaYN.Update();
            if (retval == 1) { // yes
                int idx_MC = idx_MC_curTopChoice + rel_MC_cursorRow;
                SaveManager.metacoins -= Metaclean_Costs[MC_ChoiceList[idx_MC]];
                DataLoader.instance.unlockAchievement(DataLoader.achievement_id_MCBUY);
                DataLoader.instance.setDS("META" + MC_ChoiceList[idx_MC], 1);
                if (MC_ChoiceList[idx_MC] == 3) {
                    Ano2Stats.GetItem(Ano2Stats.ITEM_ID_CARDDETECTOR);
                    RefreshPauseVisuals();
                }
                RefreshMC_ChoiceList();
                RefreshMCGraphics();
                confirmSound();
            } else if (retval == 0) {
                //cancelSound();
            }
            if (retval > -1) {
                metaSubmode = 0;
                GameObject.Find("Meta_YN").GetComponent<TMP_Text>().text = "";
                GameObject.Find("Meta_YN_BG").GetComponent<Image>().enabled = false;
                GameObject.Find("MetaCursor2").GetComponent<Image>().enabled = false;
                if (retval == 1) {
                    dbox.playDialogue("metaclean-messages", 5); // Thanks!
                    metaSubmode = 1;
                }
            }
        } else if (metaSubmode == 3) { // YN when choosing to warp or  not
            int retval = metaYN.Update();
            if (retval == 1) {

                int idx_MC = idx_MC_curTopChoice + rel_MC_cursorRow;
                int itemID = MC_ChoiceList[idx_MC];
                if (itemID >= 1000) {
                    DataLoader.instance.setDS("META" + (itemID - 1000), 2);
                    itemID -= 1000;
                }
                metaSubmode = 4;


                string nextEntrance = "";
                Registry.GameScenes nextScene = Registry.GameScenes.CCC;
                if (itemID == 20) { nextEntrance = "Entrance"; nextScene = Registry.GameScenes.ZZ_R1N;
                } else if (itemID == 19) { nextEntrance = "Entrance"; nextScene =  Registry.GameScenes.ZZ_prototype;
                } else if (itemID == 21) { nextEntrance = "Entrance"; nextScene =  Registry.GameScenes.ZZ_CCC_Old_1;
                } else if (itemID == 22) { nextEntrance = "Entrance"; nextScene =  Registry.GameScenes.ZZ_CCC_Old_2;
                } else if (itemID == 23) { nextEntrance = "Entrance"; nextScene =  Registry.GameScenes.ZZ_CCC_Old_3;
                } else if (itemID == 24) { nextEntrance = "Entrance"; nextScene =  Registry.GameScenes.ZZ_JoniRing1Test1;
                } else if (itemID == 25) { nextEntrance = "Entrance"; nextScene =  Registry.GameScenes.DEBUG2DFAKE;
                } else if (itemID == 26) { nextEntrance = "Entrance"; nextScene =  Registry.GameScenes.ZZ_test3D;
                } else if (itemID == 27) { nextEntrance = "Entrance"; nextScene =  Registry.GameScenes.ZY_Sean_ArchTests;
                } else if (itemID == 28) { nextEntrance = "Entrance"; nextScene =  Registry.GameScenes.ZZ_Albumen_Old;
                } else if (itemID == 29) { nextEntrance = "Entrance"; nextScene =  Registry.GameScenes.ZZ_RingGolem_Prototype;
                } else if (itemID == 30) { nextEntrance = "Entrance"; nextScene =  Registry.GameScenes.ZZ_JoniFearTest02;
                } else if (itemID == 31) { nextEntrance = "Entrance"; nextScene =  Registry.GameScenes.ZZ_GIF_NanoAlbumen;
                } else if (itemID == 32) { nextEntrance = "Entrance"; nextScene = Registry.GameScenes.ZZ_ChapelTrailer;
                } else if (itemID == 33) { nextEntrance = "Entrance"; nextScene = Registry.GameScenes.ZZ_CenterChamber;
                } else if (itemID == 34) { nextEntrance = "Entrance"; nextScene = Registry.GameScenes.ZZ_NanoDustboundOld;
                } else if (itemID == 35) { nextEntrance = "Entrance"; nextScene = Registry.GameScenes.S_MarinaRing1Test;
                } else if (itemID == 36) { nextEntrance = "ElevatorEntrance"; nextScene =  Registry.GameScenes.CCC;
                } else if (itemID == 37) { nextEntrance = "GolemEntrance"; nextScene = Registry.GameScenes.RingHighway;
                } else if (itemID == 38) { nextEntrance = "CloneEntrance"; nextScene =  Registry.GameScenes.DesertField;
                } else if (itemID == 39) { nextEntrance = "OceanEntrance"; nextScene = Registry.GameScenes.NanoNexus;
                } else if (itemID == 40) { nextEntrance = "Exit1"; nextScene = Registry.GameScenes.Albumen;
                } else {
                    metaSubmode = 0;
                }
                if (metaSubmode == 4) {
                    MoveFadeToFront();
                    DataLoader.instance.unlockAchievement(DataLoader.achievement_id_MCWARP);
                    DataLoader.instance.enterScene(nextEntrance, nextScene, 0.7f, 0.7f);
                    MetazoneCachedScene = curSceneName;
                    MetazoneCachedEntrance = Registry.destinationDoorNameForPauseRespawn;
                }
            } else if (retval == 0) { // no
                metaSubmode = 0;
            }
            if (retval > -1) {
                GameObject.Find("Meta_YN").GetComponent<TMP_Text>().text = "";
                GameObject.Find("Meta_YN_BG").GetComponent<Image>().enabled = false;
                GameObject.Find("MetaCursor2").GetComponent<Image>().enabled = false;
            }
        } else if (metaSubmode == 4) {  // wait for scene

        }
    }

    void UpdateMCCursor() {
        MC_tempPos = MC_initcursorPos;
        MC_tempPos.y = MC_initcursorPos.y - metaCursorSpacing * rel_MC_cursorRow;
        metacursor.transform.localPosition = MC_tempPos;
    }
    YesNoPrompt metaYN;

    int[] quitChoices;
    void setQuitText() {
        quitChoices = new int[] { 0, 1, 2 };
        if (inMetazone) {
            quitChoices[0] = 3;
        }
        textChoice.text = "";
        string s = "";
        for (int i = 0; i < quitChoices.Length; i++) {
            s += DataLoader.instance.getRaw("quitMenuChoices", quitChoices[i]);
            //int choiceID = quitChoices[i];
            //if (choiceID == 0) {
             //   s += 
           // }
            s += "\n";
        }
        textChoice.text = s;
    }

    const int CHOICE_FULLSCREEN = 0;
    const int CHOICE_WINDOW_RES = 1;
    const int CHOICE_CAMVERT = 2;
    const int CHOICE_CAMHOR = 3;
    const int CHOICE_KEYBINDINGS = 4;

    const int CHOICE_VOLUME = 5;
    const int CHOICE_LANGUAGE = 6;
    const int CHOICE_SHADOW = 7;
    const int CHOICE_FASTTEXT = 8;
    const int CHOICE_MOUSE = 9;

    const int CHOICE_DOUBLEHEALTH = 10;
    const int CHOICE_SENSITIVITY = 11;
    const int CHOICE_FOV = 12;
    const int CHOICE_AUTOSAVE2D = 13;
    const int CHOICE_CAMDIST = 14;

    const int CHOICE_DISABLECONTROLLER = 15;
    const int CHOICE_SCREENSHAKE = 16;
    const int CHOICE_INVINCIBILITY = 17;
    const int CHOICE_TERRAIN = 18;
    const int CHOICE_FPS_LOCK = 19;
    const int CHOICE_BUTTONLABEL = 20;
    const int CHOICE_INVERTCONFIRM = 21;
    const int CHOICE_controllerRotationStrength = 22;
    const int CHOICE_MOVEVERT = 23;
    const int CHOICE_UISCALING = 24;
    const int CHOICE_INSTANTTEXT = 25;

    void setSettingsText() {

        if (DataLoader.instance.isTitle) {
            settingsChoices = new int[] { 0, 1, 24, 2, 3, 23, 4, 5, 6, 25, 7, 18, 14, 11, 12, 16, 10, 17, 13, 15, 8,19,20,21,22};
        } else {
            settingsChoices = new int[] { 0, 1, 24, 2, 3, 23, 4, 5, 6, 25, 7, 18, 14, 11, 12, 16, 10, 17, 13, 15, 8,19,20,21,22};
        }
        textChoice.text = "";

        /*Fullscreen: 
          Windowed Resolution:
          UI Scaling:
          Controller - Invert Vertical Camera Axis:
          Controller - Invert Horizontal Camera Axis:
          Controller - InvertVertical Controller Movement
          Controls

        language
          Volume:
          Instant Text
          Shadow Quality 7
          terrain qual 18

          Camera Distance 14
          Camera Speed:
          Minimum Field of View:
          screenshake
          Double Health:
          invincibility (17)

          Save when Overlap 2d:
          Disable Controller
          Fast Dialogue
          fps lock
          Controller: Flip Confirm/Cancel and Talk/Spark:
          Controller: Rotation Strength during Horizontal Movement:
          */

        string s = "";
        for (int i = top_visible_setting_choice_index; i < top_visible_setting_choice_index+ max_visible_settings_choices; i++) {
            int choiceID = settingsChoices[i];
            if ((choiceID == CHOICE_INVERTCONFIRM || choiceID == CHOICE_controllerRotationStrength) && SaveManager.language == "ru") {
                s += "<size=6.5>";
            }
            s += DataLoader.instance.getRaw("pauseMenuChoices", settingsChoices[i]);
            if (choiceID == 0) {
                if (SaveManager.fullscreen) s += " " + getStrOn();
                if (!SaveManager.fullscreen) s += " " + getStrOff();
            } else if (choiceID == 1) {
                s += " ";

                if (settingsChoices[cursorIndex] == CHOICE_WINDOW_RES && submode == 4) {
                    s += "<- " + SaveManager.winResX.ToString() + "x" + SaveManager.winResY.ToString() + " ->";
                } else {
                    s += SaveManager.winResX.ToString() + "x" + SaveManager.winResY.ToString();
                }
            } else if (choiceID == CHOICE_CAMVERT) {
                if (MyInput.invertY) s = s + " " + getStrOn();
                if (!MyInput.invertY) s = s + " " + getStrOff();
            } else if (choiceID == CHOICE_CAMHOR) {
                if (MyInput.invertX) s = s + " " + getStrOn();
                if (!MyInput.invertX) s = s + " " + getStrOff();
            } else if (choiceID == CHOICE_VOLUME) {
                int vol = (int)(SaveManager.volume);
                if (settingsChoices[cursorIndex] == 5 && submode == 4) {
                    s += " <- " + vol.ToString() + "% ->";
                } else {
                    s += " " + vol.ToString() + "%";
                }
            } else if (choiceID == CHOICE_LANGUAGE) {
                string langToDisplay = SaveManager.language;
                if (langToDisplay == "zh-simp") langToDisplay = "简体中文";
                if (langToDisplay == "zh-trad") langToDisplay = "繁體中文";
                if (settingsChoices[cursorIndex] == 6 && submode == 4) {
                    s += " <- " + langToDisplay + " ->";
                } else {
                    s += " " + langToDisplay;
                }
            } else if (choiceID == CHOICE_SHADOW) {
                if (settingsChoices[cursorIndex] == CHOICE_SHADOW && submode == 4) {
                    s += " <- " + RawD("shadow-quality", SaveManager.shadowQuality) + " ->";
                } else {
                    s += " " + RawD("shadow-quality", SaveManager.shadowQuality);
                }
            } else if (choiceID == CHOICE_TERRAIN) {
                if (settingsChoices[cursorIndex] == CHOICE_TERRAIN && submode == 4) {
                    s += " <- " + RawD("terrain-quality", SaveManager.terrainQuality) + " ->";
                } else {
                    // from 1 to 5
                    s += " " + RawD("terrain-quality", SaveManager.terrainQuality);
                }
            } else if (choiceID == CHOICE_CAMDIST) {
                if (settingsChoices[cursorIndex] == CHOICE_CAMDIST && submode == 4) {
                    s += " <- " + ((int)SaveManager.cameraDistance).ToString() + "% ->";
                } else {
                    s += " " + ((int)SaveManager.cameraDistance).ToString() + "%";
                }
            } else if (choiceID == CHOICE_controllerRotationStrength) {
                if (settingsChoices[cursorIndex] == CHOICE_controllerRotationStrength && submode == 4) {
                    s += " <- " + ((int)SaveManager.extraCamRotWithControllerMoveStrength).ToString() + "% ->";
                } else {
                    s += " " + ((int)SaveManager.extraCamRotWithControllerMoveStrength).ToString() + "%";
                }
            } else if (choiceID == CHOICE_MOUSE) {
                if (MyInput.useMouse) s = s + " " + getStrOn();
                if (!MyInput.useMouse) s = s + " " + getStrOff();
            } else if (choiceID == CHOICE_FPS_LOCK) {
                // // if > 0, then it is vsync every blank. off not, then the lockshould be on
                if (QualitySettings.vSyncCount > 0) s = s + " " + getStrOff();
                if (QualitySettings.vSyncCount <= 0) s = s + " " + getStrOn();
            } else if (choiceID == CHOICE_BUTTONLABEL) {
                if (SaveManager.buttonLabelType < 0) SaveManager.buttonLabelType = 0;
                if (SaveManager.buttonLabelType > 3) SaveManager.buttonLabelType = 3;
                s = s + " " + RawD("menu-button-types", SaveManager.buttonLabelType);
            } else if (choiceID == CHOICE_DOUBLEHEALTH) {
                if (SaveManager.doubleHealth) s = s + " " + getStrOn();
                if (!SaveManager.doubleHealth) s = s + " " + getStrOff();
            } else if (choiceID == CHOICE_SENSITIVITY) {
                if (settingsChoices[cursorIndex] == 11 && submode == 4) {
                    s += " <- " + SaveManager.sensitivity.ToString() + "% ->";
                } else {
                    s += " " + SaveManager.sensitivity.ToString() + "%";
                }
            } else if (choiceID == CHOICE_FOV) {
                if (settingsChoices[cursorIndex] == 12 && submode == 4) {
                    s += " <- " + ((int)SaveManager.fieldOfView).ToString() + " ->";
                } else {
                    s += " " + ((int)SaveManager.fieldOfView).ToString();
                }
            } else if (choiceID == CHOICE_AUTOSAVE2D) {
                if (SavePoint.AutoSaveOn2D) s = s + " " + getStrOn();
                if (!SavePoint.AutoSaveOn2D) s = s + " " + getStrOff();
            } else if (choiceID == CHOICE_FASTTEXT) {
                if (SaveManager.dialogueSkip) s = s + " " + getStrOn();
                if (!SaveManager.dialogueSkip) s = s + " " + getStrOff();
            } else if (choiceID == CHOICE_DISABLECONTROLLER) {
                if (SaveManager.controllerDisable) s = s + " " + getStrOn();
                if (!SaveManager.controllerDisable) s = s + " " + getStrOff();
            } else if (choiceID == CHOICE_SCREENSHAKE) {
                if (SaveManager.screenshake) s = s + " " + getStrOn();
                if (!SaveManager.screenshake) s = s + " " + getStrOff();
            } else if (choiceID == CHOICE_INVINCIBILITY) {
                if (SaveManager.invincibility) s = s + " " + getStrOn();
                if (!SaveManager.invincibility) s = s + " " + getStrOff();
            } else if (choiceID == CHOICE_INVERTCONFIRM) {
                if (SaveManager.invertConfirmCancel) s = s + " " + getStrOn();
                if (!SaveManager.invertConfirmCancel) s = s + " " + getStrOff();

            } else if (choiceID == CHOICE_MOVEVERT) {
                if (MyInput.invertYMove) s = s + " " + getStrOn();
                if (!MyInput.invertYMove) s = s + " " + getStrOff();
            } else if (choiceID == CHOICE_UISCALING) {
                if (settingsChoices[cursorIndex] == CHOICE_UISCALING && submode == 4) {
                    s += " <- ";
                }

                if (SaveManager.customUIScaling <= 2) {
                    s = s + " " + RawD("menu-button-types", 0);
                } else {
                    s = s +" "+ SaveManager.customUIScaling + "x";
                }

                if (settingsChoices[cursorIndex] == CHOICE_UISCALING && submode == 4) {
                    s += " ->";
                }
            } else if (choiceID == CHOICE_INSTANTTEXT) {
                if (SaveManager.instantText) s = s + " " + getStrOn();
                if (!SaveManager.instantText) s = s + " " + getStrOff();
            }


            if ((choiceID == CHOICE_INVERTCONFIRM || choiceID == CHOICE_controllerRotationStrength) && SaveManager.language == "ru") {
                s += "</size>";
            }
            s += "\n";
        }
        textChoice.text = s;
    }


    void OnEnterQuit() {
        menu_selector.GetComponent<SpriteAnimator>().Play("off");
        cursorIndex = 0;
        UpdateSettingsCursorPos();
        setAlphaImage(settingsCursor, 1);
        submode = 2;
    }

    void OnExitQuit() {
        menu_selector.GetComponent<SpriteAnimator>().Play("idle");
        setAlphaImage(settingsCursor, 0);
        if (DataLoader.instance.isTitle) mode = TopLevelMode.ClosingMenu;

    }

    void UpdateQuit() {

        if (submode == 2) {
            if (dbox.isDialogFinished() == false) {
                return;
            }
            if (MyInput.jpUp && cursorIndex == 0) {
                cursorIndex = quitChoices.Length - 1;
                UpdateSettingsCursorPos();
            } else if (MyInput.jpDown && cursorIndex == quitChoices.Length - 1) {
                cursorIndex = 0;
                UpdateSettingsCursorPos();
            } else if (ChangeUpDownIndex(ref cursorIndex, 0, quitChoices.Length - 1)) {
                UpdateSettingsCursorPos();
            }
            if (MyInput.jpConfirm) {
                submode = 4;
                choiceIndex = quitChoices[cursorIndex];
                savedCursorIndex = cursorIndex;
                if (choiceIndex == 0 || choiceIndex == 3) {
                    if (curSceneName == "Wormhole" || curSceneName == "Wormhole2D" || curSceneName == "SparkGame" || curSceneName == "Intro" || (GameObject.Find(Registry.PLAYERNAME2D) != null && !inMetazone)) {
                        submode = 2;
                        dbox.playDialogue("quitMenuChoices", 4);
                    } else if (curSceneName == "CenterChamber" && DataLoader._getDS("chapel-entry") == 1 && DataLoader._getDS("cc-after-miniboss") == 0) {
                        submode = 2;
                        dbox.playDialogue("quitMenuChoices", 4);
                    } else if (curSceneName == "DesertSpire" && DataLoader._getDS("dustwallenter") == 1 && DataLoader._getDS("spire-eat") == 0 && Registry.destinationDoorNameForPauseRespawn == "HighwayEntrance") {
                        submode = 2;
                        dbox.playDialogue("quitMenuChoices", 4);
                    } else if (curSceneName == "DesertSpireTop" && DataLoader._getDS("spire-eat") == 1 && DataLoader._getDS("db-field") == 0) {
                        submode = 2;
                        dbox.playDialogue("quitMenuChoices", 4);
                    } else if (inMetazone) {
                        refreshRegularYesNoText("pauseMenuWords", 5);
                    } else {
                        refreshRegularYesNoText("pauseMenuWords", 4);
                    }
                } else if (choiceIndex == 1) {
                    refreshRegularYesNoText("pauseMenuWords", 2);
                } else if (choiceIndex == 2) {
                    refreshRegularYesNoText("pauseMenuWords", 3);
                }
            } else if (MyInput.jpCancel) {
                submode = 3;
                cursorIndex = 0;
                cancelSound();
            }
        } else if (submode == 10) {
            if (!MyInput.confirm && !MyInput.cancel) {
                submode = 2;
            }
        } else if (submode == 3) {
            mode = TopLevelMode.TopLevel;
            OnExitQuit();
        } else if (submode == 4) {

            if (fadingToTitle) {

                tFade += Time.deltaTime;
                if (tFade < 1) {
                    Color col = overlay.color; col.a = tFade; overlay.color = col;
                }

                return;
            }

            int retval = update_yesno();
            if (retval == 0) {
                submode = 10;
            } else if (retval == 1) {
                if (choiceIndex == 2) {
                    Application.Quit();
                } else if (choiceIndex == 1) { 
                    fadingToTitle = true;
                    Registry.enterGameFromLoad_SceneName = "Title";
                    Registry.set_startedNewGameButDidntSave(false);
                    DataLoader.instance.enterNextSceneBasedOnLoadedData();
                } else if (choiceIndex == 0 || choiceIndex == 3) {
                    submode = 5;
                    MoveFadeToFront();
                    if (inMetazone) {
                        DataLoader.instance.enterScene(PauseMenu.MetazoneCachedEntrance, PauseMenu.MetazoneCachedScene,0.7f,0.7f);
                    } else {
                        if (Registry.destinationDoorNameForPauseRespawn == "") {
                            if (curSceneName == "Albumen") {
                                Registry.destinationDoorNameForPauseRespawn = "Exit1";
                            } else {
                                Registry.destinationDoorNameForPauseRespawn = "USESAVEPT";
                            }
                        }
                        DataLoader.instance.enterScene(Registry.destinationDoorNameForPauseRespawn, curSceneName, 0.7f, 0.7f);
                    }
                }
            }
        } else if (submode == 5) { // wait to exit scene mode

        }

        if (submode == 2 || submode == 4) {
            if (doAutoCursorMoveDown || doAutoCursorMoveUp || MyInput.jpUp || MyInput.jpDown || MyInput.jpLeft || MyInput.jpRight) {
                moveSound();
            }
            if (MyInput.jpCancel || MyInput.jpJump || MyInput.jpConfirm) {
                confirmSound();
            }
        }

    }

    void MoveFadeToFront() {
        if (GameObject.Find(Registry.PLAYERNAME2D) == null) {
            if (GameObject.Find("UI_FadeImage") != null) {
                GameObject.Find("UI_FadeImage").transform.SetParent(GameObject.Find("PauseMenu").transform, true);
                GameObject.Find("UI_FadeImage").transform.SetAsLastSibling();
            }
        } else {
            if (GameObject.Find("Under Dialogue Fade Layer") != null) {
                GameObject.Find("Under Dialogue Fade Layer").transform.SetParent(GameObject.Find("PauseMenu").transform, true);
                GameObject.Find("Under Dialogue Fade Layer").transform.SetAsLastSibling();
            }
        }
    }

    void OnEnterSettings() {
        menu_selector.GetComponent<SpriteAnimator>().Play("off");
        cursorIndex = 0;
        top_visible_setting_choice_index = 0;
        settings_visible_cursor_idx = 0;
        UpdateSettingsCursorPos(true);
        setAlphaImage(settingsCursor, 1);
        submode = 2;
    }
    void OnExitSettings() {
        menu_selector.GetComponent<SpriteAnimator>().Play("idle");
        setAlphaImage(settingsCursor, 0);
        if (DataLoader.instance.isTitle) mode = TopLevelMode.ClosingMenu;
    }

    void UpdateSettingsCursorPos(bool fromSettings = false) {
        if (SaveManager.language == "zh-simp" || SaveManager.language == "zh-trad") {
            settingsCursorSpacing = 13.7f;
            
        } else {
            settingsCursorSpacing = 13.9f;
        }
        tempLocalPos = settingsCur1InitPos;
        if (CHOICE_KEYBINDINGS == choiceIndex) {
            if (SaveManager.language == "ru") {
                settingsCursorSpacing = 9.64f;
            }
        }
        if (fromSettings) {
            tempLocalPos.y -= settingsCursorSpacing * (cursorIndex - top_visible_setting_choice_index);
        } else {
            tempLocalPos.y -= settingsCursorSpacing * cursorIndex;
        }
        settingsCursor.transform.localPosition = tempLocalPos;
    }
    // Assumes a font size of 11 using Forum font.
    float settingsCursorSpacing = 9.31f;

    int max_visible_settings_choices = 14;
    int top_visible_setting_choice_index = 0;
    int settings_visible_cursor_idx = 0;
    void UpdateSettings () {
        // Cursor index is the absolute cursor index (into the choices array)
        if (submode == 2) {
            bool cursored_but_not_at_ends = false;
            if (doAutoCursorMoveUp) {
                MyInput.jpUp = true;
            } else if (doAutoCursorMoveDown) {
                MyInput.jpDown = true;
            }
            if (MyInput.jpUp && cursorIndex == 0) {
                cursorIndex = settingsChoices.Length - 1;
                settings_visible_cursor_idx = max_visible_settings_choices - 1;
                top_visible_setting_choice_index = (settingsChoices.Length - 1) - (max_visible_settings_choices - 1);
                UpdateSettingsCursorPos(true);
                setSettingsText();
                UpdateSettingsScrollbarPos();
            } else if (MyInput.jpDown && cursorIndex == settingsChoices.Length - 1) {
                cursorIndex = 0;
                settings_visible_cursor_idx = 0;
                top_visible_setting_choice_index = 0;
                UpdateSettingsCursorPos(true);
                setSettingsText();
                UpdateSettingsScrollbarPos();
            } else if (MyInput.jpRight || MyInput.jpLeft) {
                if (MyInput.jpRight) cursorIndex += 5;
                if (MyInput.jpLeft) cursorIndex -= 5;
                if (cursorIndex < 0) cursorIndex = 0;
                if (cursorIndex > settingsChoices.Length - 1) cursorIndex = settingsChoices.Length - 1;
                cursored_but_not_at_ends = true;
            } else if (ChangeUpDownIndex(ref cursorIndex, 0, settingsChoices.Length-1)) {
                cursored_but_not_at_ends = true;
            }
            if (cursored_but_not_at_ends) {
                if (MyInput.jpUp || MyInput.jpLeft) {
                    if (cursorIndex < top_visible_setting_choice_index) {
                        settings_visible_cursor_idx = 0;
                        top_visible_setting_choice_index = cursorIndex;
                    } else {
                        if (MyInput.jpUp) settings_visible_cursor_idx -= 1;
                        if (MyInput.jpLeft) settings_visible_cursor_idx -= 5;
                    }
                } else if (MyInput.jpDown || MyInput.jpRight) { // bottom of visible list (but not at the bsolute bottom)
                    if (cursorIndex > top_visible_setting_choice_index + (max_visible_settings_choices - 1)) {
                        settings_visible_cursor_idx = max_visible_settings_choices - 1;
                        top_visible_setting_choice_index = cursorIndex - (max_visible_settings_choices - 1);
                    } else {
                        if (MyInput.jpDown) settings_visible_cursor_idx += 1;
                        if (MyInput.jpRight) settings_visible_cursor_idx += 5;
                    }
                }
                UpdateSettingsCursorPos(true);
                setSettingsText();
                UpdateSettingsScrollbarPos();
            }



			if (MyInput.jpConfirm) {
			    submode = 4;
                choiceIndex = settingsChoices[cursorIndex];
                if (choiceIndex == CHOICE_LANGUAGE) origLang = SaveManager.language;
                savedCursorIndex = cursorIndex;
                setSettingsText();
			} else if (MyInput.jpCancel) {
				submode = 3;
                cancelSound();
            }
		} else if (submode == 3) {

            //float lowpassCutoff = Mathf.Lerp(22000f, 1100f, t);
            //AudioMixer am = GameObject.Find("Track 1-1").GetComponent<AudioSource>().outputAudioMixerGroup.audioMixer;
            //am.SetFloat("BGMLowPass", lowpassCutoff);
            mode = TopLevelMode.TopLevel;
            OnExitSettings();
		} else if (submode == 4) {
            SettingsChoiceLogic();
		} else if (submode == 5) { // wait to exit scene mode

            //tFade += Time.deltaTime;
            //if (tFade > 1f) tFade = 1f;
            //float lowpassCutoff = Mathf.Lerp(1100f, 22000f, tFade);
            //AudioMixer am = GameObject.Find("Track 1-1").GetComponent<AudioSource>().outputAudioMixerGroup.audioMixer;
            //am.SetFloat("BGMLowPass", lowpassCutoff);

        }


        if (submode == 2 || submode == 4) {
            if (doAutoCursorMoveDown || doAutoCursorMoveUp || MyInput.jpUp || MyInput.jpDown || MyInput.jpLeft || MyInput.jpRight) {
                moveSound();
            }
            if (MyInput.jpCancel || MyInput.jpJump || MyInput.jpConfirm) {
                confirmSound();
            }
        }

    }

    // Based on current Screen.fullscreen and Screen.resolution states
    void RefreshUIScaling(float overrideH = 0,float overrideW = 0) {
        RefreshPauseVisuals();
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Title") UIManagerAno2.Update3DUIScale(overrideH,overrideW);
        if (GameObject.Find("3D UI") != null) UIManagerAno2.Update3DUIScale(overrideH,overrideW);
        if (GameObject.Find("2D UI") != null) GameObject.Find("2D UI").GetComponent<UIManager2D>().Set2DScreenResolution(0, true,overrideH);
    }

	int resIndex = 0;

    int choiceIndex = 0;
    int savedCursorIndex = 0;
    int doubleHealthIndex = 0;
	void SettingsChoiceLogic() {
		bool doneWithChoices = false;

		switch (choiceIndex) {
			case CHOICE_SENSITIVITY:
			if (MyInput.jpRight && SaveManager.sensitivity < 150) SaveManager.sensitivity += 5;
			if (MyInput.jpLeft && SaveManager.sensitivity > 25) SaveManager.sensitivity -= 5;
			if (MyInput.jpLeft || MyInput.jpRight) {
				setSettingsText();
			}
			if (MyInput.jpCancel || MyInput.jpConfirm) {
				setSettingsText();
				doneWithChoices = true;
			}
			break;


            case CHOICE_TERRAIN:
                if (MyInput.jpRight && SaveManager.terrainQuality < 5) SaveManager.terrainQuality++;
                if (MyInput.jpLeft && SaveManager.terrainQuality > 1) SaveManager.terrainQuality--;

                if (MyInput.jpLeft || MyInput.jpRight) {
                    setSettingsText();
                }
                if (MyInput.jpCancel || MyInput.jpConfirm) {
                    setSettingsText();
                    DataLoader.instance.SetTerrainQuality(SaveManager.terrainQuality);
                    doneWithChoices = true;
                }
                break;
            case CHOICE_SHADOW:
                if (MyInput.jpRight && SaveManager.shadowQuality < 3) SaveManager.shadowQuality ++;
                if (MyInput.jpLeft && SaveManager.shadowQuality> 0) SaveManager.shadowQuality --;
                
                if (MyInput.jpLeft || MyInput.jpRight) {
                    setSettingsText();
                }
                if (MyInput.jpCancel || MyInput.jpConfirm) {
                    setSettingsText();
                    DataLoader.instance.SetShadowQuality(SaveManager.shadowQuality);
                    doneWithChoices = true;
                }
                break;
            case CHOICE_CAMDIST:
                if (MyInput.jpRight && SaveManager.cameraDistance < 200) SaveManager.cameraDistance += 5;
                if (MyInput.jpLeft && SaveManager.cameraDistance > 50) SaveManager.cameraDistance -= 5;
                SaveManager.cameraDistance = Mathf.Clamp(SaveManager.cameraDistance, 50,200);
                if (MyInput.jpLeft || MyInput.jpRight) {
                    setSettingsText();
                }
                if (MyInput.jpCancel || MyInput.jpConfirm) {
                    setSettingsText();
                    doneWithChoices = true;
                }
                break;

            case CHOICE_controllerRotationStrength:
                if (MyInput.jpRight && SaveManager.extraCamRotWithControllerMoveStrength < 100) SaveManager.extraCamRotWithControllerMoveStrength += 5;
                if (MyInput.jpLeft && SaveManager.extraCamRotWithControllerMoveStrength >= 5) SaveManager.extraCamRotWithControllerMoveStrength -= 5;
                SaveManager.extraCamRotWithControllerMoveStrength = Mathf.Clamp(SaveManager.extraCamRotWithControllerMoveStrength, 0, 100);
                if (MyInput.jpLeft || MyInput.jpRight) {
                    setSettingsText();
                }
                if (MyInput.jpCancel || MyInput.jpConfirm) {
                    setSettingsText();
                    doneWithChoices = true;
                }
                break;
            case CHOICE_FOV:
                if (MyInput.jpRight && SaveManager.fieldOfView < 110) SaveManager.fieldOfView += 1;
                if (MyInput.jpLeft && SaveManager.fieldOfView > 60) SaveManager.fieldOfView -= 1;
                Camera.main.fieldOfView = SaveManager.fieldOfView;
                if (MyInput.jpLeft || MyInput.jpRight) {
                    setSettingsText();
                }
                if (MyInput.jpCancel || MyInput.jpConfirm) {
                    setSettingsText();
                    doneWithChoices = true;
                }
                break;

            case CHOICE_UISCALING:
                if (MyInput.jpRight && SaveManager.customUIScaling < 18) SaveManager.customUIScaling++;
                if (MyInput.jpLeft && SaveManager.customUIScaling > 2) SaveManager.customUIScaling--;
                if (MyInput.jpLeft || MyInput.jpRight) {
                    setSettingsText();
                    if (SaveManager.fullscreen) RefreshUIScaling(maxAddedH, maxAddedW);
                    if (!SaveManager.fullscreen) RefreshUIScaling(0, 0);
                }
                if (MyInput.jpCancel || MyInput.jpConfirm) {
                    setSettingsText();
                    doneWithChoices = true;
                }
                break;
            case CHOICE_FULLSCREEN:
                if (modeFullscren == 0) {
                    SaveManager.fullscreen = !SaveManager.fullscreen;
                    if (!SaveManager.fullscreen) {
                        Screen.SetResolution(SaveManager.winResX, SaveManager.winResY, false);
                    } else {
                        Screen.SetResolution(maxAddedW, maxAddedH, true);
                    }
                    tFullscreen = 1f;
                    modeFullscren = 1;
                } else if (modeFullscren == 1) {
                    tFullscreen -= Time.deltaTime;
                    if (tFullscreen < 0 || SaveManager.fullscreen == Screen.fullScreen) {
                        modeFullscren = 0;
                        if (SaveManager.fullscreen) RefreshUIScaling(maxAddedH,maxAddedW);
                        if (!SaveManager.fullscreen) RefreshUIScaling(0,0);
                        doneWithChoices = true;
                        if (DataLoader.instance.isTitle) {
                            SaveManager.dontChangeResOnLoadBCChangedInTitle = true;
                        }
                    } 
                }
                break;
			case CHOICE_WINDOW_RES:

                update_windowsres();
			if (MyInput.jpCancel || MyInput.jpConfirm) {
				doneWithChoices = true;
                    if (DataLoader.instance.isTitle) {
                        SaveManager.dontChangeResOnLoadBCChangedInTitle = true;
                    }
                }
				break;
			case CHOICE_CAMVERT:
				MyInput.invertY = !MyInput.invertY;
				doneWithChoices = true;
				break;
			case CHOICE_CAMHOR:
				MyInput.invertX = !MyInput.invertX;
				doneWithChoices = true;
				break;
            case CHOICE_MOVEVERT:
                MyInput.invertYMove= !MyInput.invertYMove;
                doneWithChoices = true;
                break;
            case CHOICE_AUTOSAVE2D:
                SavePoint.AutoSaveOn2D = !SavePoint.AutoSaveOn2D;
                doneWithChoices = true;
                break;
            case CHOICE_INSTANTTEXT:
                SaveManager.instantText = !SaveManager.instantText;
                doneWithChoices = true;
                break;
            case CHOICE_SCREENSHAKE:
                SaveManager.screenshake = !SaveManager.screenshake;
                doneWithChoices = true;
                break;
            case CHOICE_INVINCIBILITY:
                SaveManager.invincibility = !SaveManager.invincibility;
                doneWithChoices = true;
                break;
            case CHOICE_FASTTEXT:
                SaveManager.dialogueSkip = !SaveManager.dialogueSkip;
                SaveManager.keepSkipOnUntilLoadingFromTitle = SaveManager.dialogueSkip;
                doneWithChoices = true;
                break;
            case CHOICE_DISABLECONTROLLER:
                SaveManager.controllerDisable = !SaveManager.controllerDisable;
                doneWithChoices = true;
                break;
            case CHOICE_KEYBINDINGS:
			    if (update_keybindings() == 1) {
					doneWithChoices = true;
				}
				
				break;
			case CHOICE_VOLUME:
				update_volume();
			if (MyInput.jpCancel  || MyInput.jpConfirm) {
				doneWithChoices = true;
			}
				break;
            case CHOICE_MOUSE:
                MyInput.useMouse = !MyInput.useMouse;
                doneWithChoices = true;
                break;
            case CHOICE_FPS_LOCK:
                if (QualitySettings.vSyncCount <= 0) {
                    QualitySettings.vSyncCount = 1;
                    Application.targetFrameRate = -1;
                } else {
                    QualitySettings.vSyncCount = 0;
                    Application.targetFrameRate = 60;
                }
                doneWithChoices = true;
                break;
            case CHOICE_BUTTONLABEL:
                SaveManager.buttonLabelType++;
                if (SaveManager.buttonLabelType > 3) SaveManager.buttonLabelType = 0;
                doneWithChoices = true;
                break;
            case CHOICE_LANGUAGE:
				update_language();
                if (MyInput.jpCancel || MyInput.jpConfirm) {
                    doneWithChoices = true;
                }
                break;
            case CHOICE_INVERTCONFIRM:
                if (doubleHealthIndex == 0) {
                    SaveManager.invertConfirmCancel = !SaveManager.invertConfirmCancel;
                    doubleHealthIndex = 1;
                    if (dbox != null) {
                        dbox.playDialogue("doubleHealth", 2);
                    }
                } else if (doubleHealthIndex == 1) {
                    if (dbox == null || dbox.isDialogFinished()) {
                        doneWithChoices = true;
                        doubleHealthIndex = 0;
                    }
                }
                break;
            case CHOICE_DOUBLEHEALTH:
                if (doubleHealthIndex == 0) {
                    if (SaveManager.doubleHealth) {
                        if (SaveManager.currentHealth > 6 + SaveManager.healthUpgrades) {
                            SaveManager.currentHealth = 6 + SaveManager.healthUpgrades;
                            PauseHealthBar.SetHealth(SaveManager.currentHealth); // Do this to hide golden if needed when turning off double
                            if (GameObject.Find("2D Ano Player") != null) {
                                GameObject.Find("2D Ano Player").GetComponent<HealthBar>().SetHealth(SaveManager.currentHealth);
                            }
                        }
                    }
                    SaveManager.doubleHealth = !SaveManager.doubleHealth;
                    PauseHealthBar.SetHealth(SaveManager.currentHealth); // Now do this to move everything into place
                    if (GameObject.Find("2D Ano Player") != null) {
                        GameObject.Find("2D Ano Player").GetComponent<HealthBar>().SetHealth(SaveManager.currentHealth);
                    }
                    doubleHealthIndex = 1;
                    if (dbox != null) {
                        if (SaveManager.doubleHealth) {
                            dbox.playDialogue("doubleHealth", 0);
                        } else {
                            dbox.playDialogue("doubleHealth", 1);
                        }
                    }
                } else if (doubleHealthIndex == 1) {
                    if (dbox == null || dbox.isDialogFinished()) {
                        doneWithChoices = true;
                        doubleHealthIndex = 0;
                    }
                }
                break;
		}
		if (doneWithChoices) {
            MyInput.jpCancel = MyInput.jpConfirm = false;
			submode = 2;
            cursorIndex = savedCursorIndex;
            UpdateSettingsCursorPos(true);
			setSettingsText();
		}
	}


	// Change it to
	// o Move Forward: [UP]
	// Select a control, then press the desired key.
	// [Can't assign two controls the same key!]
	int kbMode = 0;
	int kbIndex = 0;

	void setControlText(int activeKey=-1,int additionalMsg=0) {
		textChoice.text = "";
        if (SaveManager.language == "ru") textChoice.text += "<size=7.5>";
		for (int i = 0; i < 13; i++) {
            if (i > 10) {
                textChoice.text += DataLoader.instance.getRaw("keybind", i+4) + " ";
            } else {
                textChoice.text += DataLoader.instance.getRaw("keybind", i) + " ";
            }
			if (activeKey == i) textChoice.text +="-";
			if (i == 0) textChoice.text += MyInput.KC_up;
			if (i == 1) textChoice.text += MyInput.KC_down;
			if (i == 2) textChoice.text += MyInput.KC_left;
			if (i == 3) textChoice.text += MyInput.KC_right;
            if (i == 4) textChoice.text += MyInput.KC_camtoggle;
            if (i == 5) textChoice.text += MyInput.KC_special;
			if (i == 6) textChoice.text += MyInput.KC_jump;
            if (i == 7) textChoice.text += MyInput.KC_talk;
			if (i == 8) textChoice.text += MyInput.KC_cancel;
			if (i == 9) textChoice.text += MyInput.KC_pause;
            if (i == 10) textChoice.text += MyInput.KC_toggleRidescale;
            if (i == 11) textChoice.text += MyInput.KC_zoomIn;
            if (i == 12) textChoice.text += MyInput.KC_zoomOut;


            if (i == 4 && MyInput.controllerActive) textChoice.text += "/"+MyInput.replaceTags("{CAMTOGGLE}");
            if (i == 5 && MyInput.controllerActive) textChoice.text += "/" + MyInput.replaceTags("{SPECIAL}");
            if (i == 6 && MyInput.controllerActive) textChoice.text += "/" + MyInput.replaceTags("{JUMP}");
            if (i == 7 && MyInput.controllerActive) textChoice.text += "/" + MyInput.replaceTags("{TALK}");
            if (i == 8 && MyInput.controllerActive) textChoice.text += "/" + MyInput.replaceTags("{CANCEL}");
            if (i == 9 && MyInput.controllerActive) textChoice.text += "/" + MyInput.replaceTags("{PAUSE}");
            if (i == 10 && MyInput.controllerActive) textChoice.text += "/" + MyInput.replaceTags("{RIDESCALE}");
            if (i == 11 && MyInput.controllerActive) textChoice.text += "/" + MyInput.replaceTags("{ZOOMIN}");
            if (i == 12 && MyInput.controllerActive) textChoice.text += "/" + MyInput.replaceTags("{ZOOMOUT}");


            if (activeKey == i) textChoice.text +="-";
			textChoice.text += "\n";
		}

		textChoice.text += DataLoader.instance.getRaw("keybind",11) + "\n";
        textChoice.text += DataLoader.instance.getRaw("keybind", 14) + "\n";
        if (additionalMsg == 1) {
			textChoice.text += DataLoader.instance.getRaw("keybind",12) + "\n";
		} else if (additionalMsg == 2) {
			textChoice.text += DataLoader.instance.getRaw("keybind",13) + "\n";
		}

        if (SaveManager.language == "ru") textChoice.text += "</size>";
    }

	List<KeyCode> validKeycodes;

	int update_keybindings() {

		if (validKeycodes == null) {
			validKeycodes = new List<KeyCode>();
			int[] a = (int[])System.Enum.GetValues(typeof(KeyCode));

			foreach (int rawKC in a) {
				KeyCode kc = (KeyCode) rawKC;
				string kcName = kc.ToString();
				if (kcName.IndexOf("Mouse") != -1) {
				} else if (kcName.IndexOf("Joystick") != -1) {
				} else if (kcName == "Escape") {
				} else {
					validKeycodes.Add(kc);
				}
			}
		}

		if (kbMode == 0) {
			setControlText();
			kbIndex = 0;
            settingsCursor.transform.localPosition = settingsCur1InitPos;
			kbMode = 1;
		} else if (kbMode == 1) {

            if (doAutoCursorMoveUp) MyInput.jpUp = true;
            if (doAutoCursorMoveDown) MyInput.jpDown = true;

            if (ChangeUpDownIndex(ref kbIndex, 0, 12)) {
                cursorIndex = kbIndex;
                UpdateSettingsCursorPos();
            }

			if (MyInput.jpConfirm && !MyInput.jpConfirmCONTROLLER) {
				setControlText(kbIndex);
				kbMode = 2;
			} else if (MyInput.jpCancel) {
				kbMode = 0;
                cursorIndex = 0;
                setSettingsText();
                // hack to get into right position
                UpdateSettingsCursorPos(true);
                return 1;
			}
		} else if (kbMode == 2) {
			foreach (KeyCode kc in validKeycodes) {
				if (Input.GetKeyDown(kc)) {

                    kbMode = 3;
                    keyexittimer = 5;

                    if (kc == MyInput.KC_up || kc == MyInput.KC_right|| kc == MyInput.KC_down|| kc == MyInput.KC_left) {
						setControlText(-1,1); break;
					}
					if (kc == MyInput.KC_jump || kc == MyInput.KC_talk|| kc == MyInput.KC_cancel|| kc == MyInput.KC_pause || kc == MyInput.KC_camtoggle || kc == MyInput.KC_special || kc == MyInput.KC_toggleRidescale || kc == MyInput.KC_zoomOut || kc == MyInput.KC_zoomIn) {
                        if ((kbIndex == 5 || kbIndex == 8) && (kc == MyInput.KC_cancel || kc == MyInput.KC_special)) {
                            // Allow accel/nano/cancel to be binded to same key
                        } else {
                            setControlText(-1, 1);  break;
                        }
					}

					if (kbIndex == 0) MyInput.KC_up = kc;
					if (kbIndex == 1) MyInput.KC_down = kc;
					if (kbIndex == 2) MyInput.KC_left = kc;
					if (kbIndex == 3) MyInput.KC_right = kc;
					if (kbIndex == 4) MyInput.KC_camtoggle = kc;
					if (kbIndex == 5) MyInput.KC_special = kc;
					if (kbIndex == 6) MyInput.KC_jump = kc;
					if (kbIndex == 7) MyInput.KC_talk = kc;
					if (kbIndex == 8) MyInput.KC_cancel = kc;
					if (kbIndex == 9) MyInput.KC_pause = kc;
					if (kbIndex == 10) MyInput.KC_toggleRidescale = kc;
					if (kbIndex == 11) MyInput.KC_zoomIn = kc;
					if (kbIndex == 12) MyInput.KC_zoomOut = kc;
					setControlText(-1,2); // DIsplay "done" message

                    break;
				}
			}
		} else if (kbMode == 3) {
            // Prevent a repeat use of this input in trying to change a key again or exiting the menu immediately
            keyexittimer--;
            MyInput.jpCancel = MyInput.jpConfirm = false;
            if (keyexittimer == 0) kbMode = 1;
        }
        return -1;
	}
    int keyexittimer = 0;
	YesNoPrompt yn;


    void refreshRegularYesNoText(string scene, int index) {
        yn = new YesNoPrompt("PauseCursor2", "PauseYesNo", scene, index);
    }
	int update_yesno() {
		if (yn == null) {
			yn = new YesNoPrompt("PauseCursor2","PauseYesNo","pauseMenuWords",2);
		} else {
			int retval = yn.Update();
			if (retval > -1) {
				yn = null;
				return  retval;
			}
		}
		return -1;
	}


	void update_windowsres() {
		if (MyInput.jpLeft) {
			if (resIndex > 0) resIndex--;
		} else if (MyInput.jpRight) {
			if (resIndex < resolutions.Count-1) resIndex++;
			if (resIndex > resolutions.Count-1) resIndex = resolutions.Count-1;
		}
		if (MyInput.jpLeft || MyInput.jpRight) {
            if (!SaveManager.fullscreen) {
                Screen.SetResolution(resolutions[resIndex].width, resolutions[resIndex].height, SaveManager.fullscreen);
            }
			SaveManager.winResX = resolutions[resIndex].width;
			SaveManager.winResY = resolutions[resIndex].height;
            print("Update winres to: "+SaveManager.winResX + "," + SaveManager.winResY);
            if (!SaveManager.fullscreen) {
                RefreshUIScaling();
            }
			setSettingsText();
        }
    }

    public static bool language_changed_dont_change_on_load = false;
    public static string[] languageArray = new string[] { "en", "jp", "de", "pt-br","zh-simp", "zh-trad", "es", "fr","ru"};
    public static string GetSavingStringHardcoded(string lang) {;
        if (lang == "de") return "Spiel wird gespeichert...";
        if (lang == "fr") return "Sauvegarde en cours...";
        if (lang == "es") return "Guardando...";
        if (lang == "zh-simp") return "保存中...";
        if (lang == "zh-trad") return "保存中...";
        if (lang == "pt-br") return "Salvando...";
        if (lang == "jp") return "セーブ中...";
        if (lang == "ru") return "Сохранение...";
        return "Saving...";
    }
    string origLang = "";
    void update_language() {
        int langIndex = -1;
        if (MyInput.jpLeft || MyInput.jpRight || MyInput.jpConfirm) {
            for (int i = 0; i < languageArray.Length; i++) {
                if (languageArray[i] == SaveManager.language) {
                    langIndex = i;
                    break;
                }
            }
            language_changed_dont_change_on_load = true;
        }

        if (MyInput.jpLeft) {
            langIndex--;
            if (langIndex < 0) langIndex = languageArray.Length - 1;
            SaveManager.language = languageArray[langIndex];
            setSettingsText();
        } else if (MyInput.jpRight) {
            langIndex++;
            if (langIndex >= languageArray.Length) langIndex = 0;
            SaveManager.language = languageArray[langIndex];
            setSettingsText();
        }
        if (MyInput.jpConfirm) {
            if (langIndex != -1) {
                DataLoader.instance.ReloadDialogueText();
                DataLoader.instance.RefreshFonts();
                setSettingsText();
                RefreshPauseVisuals();
                if (DataLoader.instance.isTitle) {
                    TitleScreen.ChangeGameLogoLanguage();
                }
                print("Switched to " + SaveManager.language);
            }
        } else if (MyInput.jpCancel) {
            SaveManager.language = origLang;
            setSettingsText();
        }
    }

    public static bool volume_changed_dont_change_on_load = false;
	void update_volume() {
		if (MyInput.jpLeft) {
            volume_changed_dont_change_on_load = true;
			if (SaveManager.volume > 0) SaveManager.volume -= 10f;
			if (SaveManager.volume < 0) SaveManager.volume = 0;
			setSettingsText();
		} else if (MyInput.jpRight) {
            volume_changed_dont_change_on_load = true;
            if (SaveManager.volume < 100) SaveManager.volume += 10f;
			if (SaveManager.volume > 100) SaveManager.volume = 100;
			setSettingsText();
		}

		if (MyInput.jpLeft || MyInput.jpRight) {
			AudioMixer am = GameObject.Find("Track 1-1").GetComponent<AudioSource>().outputAudioMixerGroup.audioMixer;
			if (SaveManager.volume == 0) am.SetFloat("MasterVolume",-80f);
			if (SaveManager.volume > 0) am.SetFloat("MasterVolume",Mathf.Lerp(-34f,0,SaveManager.volume/100f));

		}
	}

	public bool isActive() {
        return mode != TopLevelMode.Closed;
	}

    int dustDepositMode = 0;
    float tDustDeposit = 0;
    int dustToDeposit = 0;
    int dustDeposited = 0;
    void UpdateDustDeposit() {



        if (dustDepositMode == 0) {
            RefreshPauseVisuals();
            SetTopLevelVisibleSubmenu(TopLevelMode.Information);
            for (int i = 0; i < InformationMenu.transform.childCount; i++) {
                InformationMenu.transform.GetChild(i).gameObject.SetActive(false);
            }
            PrismInfo.SetActive(true);
            Pausemenu_infoscreen.SetActive(false);
            Pausemenu_infoscreen_nomap.SetActive(true);
            dustbar.gameObject.SetActive(true);
            dustbar.SetDust(startingHeldDust);
            dustDepositMode = 1;
            text_dust.text = GetDustProgressString(startingPrismDust);
            tDustDeposit = -1f;

            tempLocalPos = PrismCurrentDustRT.localPosition; tempLocalPos.y = -113 + Mathf.Floor(Mathf.Min(startingPrismDust + dustDeposited, 300) / 50f) * 2 + ((startingPrismDust + dustDeposited) / 350f) * 98; PrismCurrentDustRT.localPosition = tempLocalPos;
        } else if (dustDepositMode == -1) {
            mode = TopLevelMode.Closed;
            SetTopLevelVisibleSubmenu(mode);
            for (int i = 0; i < InformationMenu.transform.childCount; i++) {
                InformationMenu.transform.GetChild(i).gameObject.SetActive(true);
            }
            Pausemenu_infoscreen.SetActive(true);
            Pausemenu_infoscreen_prismonly.SetActive(false);

        } else if (dustDepositMode == 1) {

            tDustDeposit += Time.deltaTime;
            if (tDustDeposit > 0.07f) {
                tDustDeposit -= 0.07f;
                AudioHelper.instance.playOneShot("vacuumShoot", 1f, 1.05f - 0.1f*(dustDeposited/90f));
                dustDeposited++;

                // Update dust bar sprite
                dustbar.SetDust(startingHeldDust - dustDeposited);



                // update info view
                tempLocalPos = PrismCurrentDustRT.localPosition; tempLocalPos.y = -113 + Mathf.Floor(Mathf.Min(startingPrismDust + dustDeposited, 300) / 50f) * 2 + ((startingPrismDust + dustDeposited) / 350f) * 98; PrismCurrentDustRT.localPosition = tempLocalPos;
                text_dust.text = GetDustProgressString(startingPrismDust + dustDeposited);
                if (dustToDeposit == dustDeposited) {
                    dustDepositMode = 4;
                    if (startingHeldDust - dustToDeposit != 0) {
                        if (Ano2Stats.prismCapacity == 50) {
                            dbox.playDialogue("ddp-full-r0", 0); // Pal overfilled message
                        } else if (Ano2Stats.prismCapacity == 350) {
                            dbox.playDialogue("ddp-full-afterfilling", 1); // Console overfill (ending)
                        } else if (Ano2Stats.prismCapacity >= 100 && DataLoader._getDS(Registry.FLAG_RING_OPEN) == 1) {
                            dbox.playDialogue("ddp-full-afterfilling", 0); // Console overfill
                        }
                    }
                }
            }
        } else if (dustDepositMode == 4 && dbox.isDialogFinished()) {
            tDustDeposit += Time.deltaTime;
            if (tDustDeposit > 1f) {
                dustDepositMode = -1;
            }
        }
    }

    float t_tutFlashText = 0;
    float t_tutStuff = 0;
    int tutCounter = 0;
    float tutDboxPos;
    void UpdatePrismTutorial() {
        if (HF.TimerDefault(ref t_tutFlashText,0.5f)) {
            if (text_dust.alpha == 1) {
                text_dust.alpha = 0;
            } else {
                text_dust.alpha = 1;
            }
        }
        if (tutorialMode == 0) {
            RefreshPauseVisuals();

            RectTransform dboxTransform = GameObject.Find("Dialog Box").GetComponent<RectTransform>();
            tutDboxPos = dboxTransform.localPosition.x;
            tempLocalPos = dboxTransform.localPosition;
            tempLocalPos.x = -80;
            dboxTransform.localPosition = tempLocalPos;

            SetTopLevelVisibleSubmenu(TopLevelMode.Information);
            for (int i = 0; i < InformationMenu.transform.childCount; i++) {
                InformationMenu.transform.GetChild(i).gameObject.SetActive(false);
            }
            prismCardAnims[0, 0].GetComponent<Image>().enabled = false;
            prismCardAnims[0, 1].GetComponent<Image>().enabled = false;
            prismCardAnims[0, 2].GetComponent<Image>().enabled = false;
            prismCardAnims[0, 3].GetComponent<Image>().enabled = false;
            PrismInfo.SetActive(true);
            Pausemenu_infoscreen.SetActive(false);
            Pausemenu_infoscreen_prismonly.SetActive(true);
            prism_cardicons_tutorial.SetActive(false);
            tutorialMode = 1;
            dbox.playDialogue("ccc-entry", 1);
            text_dust.text = RawD("ccc-entry-captions", 0);
            t_tutStuff = 2f;
            SetPrismLevelVisuals(6);
            text_prism_level.text = RawD("pause-info-text", 2) + " 7";
        } 
        else if (tutorialMode == -1) {
            RectTransform dboxTransform = GameObject.Find("Dialog Box").GetComponent<RectTransform>();
            tempLocalPos = dboxTransform.localPosition;
            tempLocalPos.x = tutDboxPos;
            dboxTransform.localPosition = tempLocalPos;

            mode = TopLevelMode.Closed;
            SetTopLevelVisibleSubmenu(mode);
            for (int i = 0; i < InformationMenu.transform.childCount; i++) {
                InformationMenu.transform.GetChild(i).gameObject.SetActive(true);
            }
            Pausemenu_infoscreen.SetActive(true);
            Pausemenu_infoscreen_prismonly.SetActive(false);
            prism_cardicons_tutorial.SetActive(false);

            text_dust.alpha = 1;
            // Fill sfx, 2s long
        } 
        // Move prism dust level
        else if (tutorialMode == 1) {
            tempLocalPos = PrismCurrentDustRT.localPosition;
            t_tutStuff += Time.deltaTime;
            tempLocalPos.y = -113f + 110f * (Mathf.Clamp01(t_tutStuff / 1f));
            if (t_tutStuff >= 3f) {
                t_tutStuff = 0;
                if (dbox.isDialogFinished()) {
                    dbox.playDialogue("ccc-entry", 2);
                    text_dust.text = RawD("ccc-entry-captions", 1);
                    tutorialMode = 2;
                    t_tutStuff = 0;
                    tempLocalPos.y = -113f;
                    tutCounter = -3;
                } else {
                    AudioHelper.instance.playOneShot("tut_fill");
                    // play  fill sfx 2s
                }
            }
            PrismCurrentDustRT.localPosition = tempLocalPos;
        // Move shields downwards
        } else if (tutorialMode == 2) {

            if (HF.TimerDefault(ref t_tutStuff, 0.25f)) {
                tutCounter--;
                if (tutCounter < 0) {
                    if (tutCounter == -4) {
                        tutCounter = 6;
                        AudioHelper.instance.playOneShot("tut_shield");
                        // play deactivate sfx, 2.75 s (.25 per bloop)
                    }
                }

                if (tutCounter >= 0) {
                    text_prism_level.text = RawD("pause-info-text", 2) + " " + (tutCounter + 1).ToString();
                    SetPrismLevelVisuals(tutCounter);
                }
            }


            if (dbox.isDialogFinished() && tutCounter == -3) {
                tutCounter = 0;
                t_tutStuff = 0;
                tutorialMode = 3;
                dbox.playDialogue("ccc-entry", 3);
                text_dust.text = RawD("ccc-entry-captions", 2);
                prism_cardicons_tutorial.GetComponent<SpriteAnimator>().Play("idle");
                AudioHelper.instance.playOneShot("tut_card");
                prism_cardicons_tutorial.SetActive(true);
            }
        // Make cards animate and turn on a shield
        } else if (tutorialMode == 3) {
            if (tutCounter == 0) {
                if (t_tutStuff > 1f) {
                    text_prism_level.text = RawD("pause-info-text", 2) + " " + (2).ToString();
                    tutCounter = 1;
                    SetPrismLevelVisuals(1);
                }
            } else {
                if (t_tutStuff <= 1f) {
                    text_prism_level.text = RawD("pause-info-text", 2) + " " + (1).ToString();
                    tutCounter = 0;
                    SetPrismLevelVisuals(0);
                }
            }
            if (HF.TimerDefault(ref t_tutStuff, 26*(1/8f))) {
                if (dbox.isDialogFinished()) {
                    tutorialMode = -1;
                } else {
                    prism_cardicons_tutorial.GetComponent<SpriteAnimator>().ForcePlay("idle");
                    AudioHelper.instance.playOneShot("tut_card");
                }
            }
        }
    }

    public bool IsPauseTutorialDone() {
        return mode != TopLevelMode.PrismTutorial;
    }
    public bool IsDustDepositDone() {
        return mode != TopLevelMode.DustDeposit;
    }

    bool IsRingOpen() {
        return DataLoader.instance.getDS("ddp-open-ring1") == 1;
    }
    bool IsDesertOpen() {
        return DataLoader.instance.getDS("ddp-open-ring2") == 1;
    }

    string GetDustProgressString(int forcedust=-1) {
        bool isRingOpen = DataLoader.instance.getDS("ddp-open-ring1") == 1;
        bool isDesertOpen = DataLoader.instance.getDS("ddp-open-ring2") == 1;
        int dustgoal = Ano2Stats.RING_DUST_GOAL; if (isRingOpen) dustgoal = Ano2Stats.DESERT_DUST_GOAL; if (isDesertOpen) dustgoal = Ano2Stats.ANODYNE_DUST_GOAL;
        if (forcedust == -1) {
            return RawD("pause-info-text", 3) + " " + Ano2Stats.prismCurrentDust.ToString() + "/" + dustgoal.ToString();
        } else {
            return RawD("pause-info-text", 3) + " " + forcedust.ToString() + "/" + dustgoal.ToString();
        }
    }

    void SetPrismLevelVisuals(int PrismUpgrades) {
        int offset = 2 + (6 - PrismUpgrades) * 16;
        if (PrismUpgrades == 6) offset = 0;
        tempLocalPos = PrismCurrentLevelRT.localPosition;
        tempLocalPos.y = 3 + offset; PrismCurrentLevelRT.localPosition = tempLocalPos;
        tempLocalPos = PrismCurrentLevelMaskRT.localPosition;
        tempLocalPos.y = 19 - offset; PrismCurrentLevelMaskRT.localPosition = tempLocalPos;
    }

    float tFullscreen = 0.3f;
    int modeFullscren = 0;
    private bool inMetazone;

    public void GetTMP_Arrays(ref List<TMP_Text[]> array_of_childtexts) {
        array_of_childtexts.Add(TopLevel.GetComponentsInChildren<TMP_Text>());
        array_of_childtexts.Add(InformationMenu.GetComponentsInChildren<TMP_Text>());
        array_of_childtexts.Add(CardsMenu.GetComponentsInChildren<TMP_Text>());
        array_of_childtexts.Add(InventoryMenu.GetComponentsInChildren<TMP_Text>());
        array_of_childtexts.Add(MetacleanMenu.GetComponentsInChildren<TMP_Text>());
        array_of_childtexts.Add(SettingsMenu.GetComponentsInChildren<TMP_Text>());
        if (dbox != null) array_of_childtexts.Add(dbox.GetComponentsInChildren<TMP_Text>());
    }

}
