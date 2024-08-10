using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Anodyne;

public class UIManager2D : MonoBehaviour {

    [HideInInspector]
    public int fadeMode = 0;
    float tFade = 0;
    [HideInInspector]
    public float tmFade = 1;
    float defaultFadeTime = 1;
    Image UnderDialogueLayer;
    Color destinationColor = new Color();
    float endAlpha = 1;
    float startAlpha = 0;
    CanvasScaler canvasScaler;
    string curScene;
    Transform SuckedItemT;
    Vector3 tempScale = new Vector3();
    int SuckedItem_mode = 0;
    [System.NonSerialized]
    public bool ext_ItemWasSucked = false;
    float t_SuckedItemEffect = 0;
    Vector3 initialSuckedItemPos = new Vector3();
    Vector3 tempSuckedItemPos = new Vector3();
    Vector2 tempSuckedItemVel = new Vector2();
    float maxSuckedItemDistanceFromInitPos = 10f;
    public GameObject minimapTilePrefab;
    Image[,] minimapTiles;
    AnoControl2D player;

    TMPro.TMP_Text savingtext_loc;
    TMPro.TMP_Text savingtext_loc_bottom;


    SpriteAnimator glandilockAnim;
    PositionShaker glandilockPS;
    ParticleSystem pulse;

    // Use this for initialization

    // MUST look good on 1920x1080, 1366x768 to start
    // Arbitrarily the range of ... is (230 (min), .. (max)), that is to display at min*n height, thhen scr_h >=min*n. An integer scale ideal is the highest n for which min*n <= scr_h. the min was chosen by showing all of the main 2D UI and a bit of the margins to not feel cramped. the max by ??
    // Probably start with 230 for now
    // Canvas scaler is set to monitor resolution. So any width/height of a canvas object fits the monitor pixels perfectly
    // scale of game render object is set to that "n" value
    // User can also set to one level lower if desired.

    // Game Render Texture: always 160x160
    // UI Background: 
    // Detect monitor resolution, 
    public Material GlandilockAngryMat;

    float BaseUIBGHeight = 230f;
	void Start () {
        //SaveManager.TestMinimapStateStuff();
        curScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        initSparkBarThings();
        HF.GetPlayer(ref player);
        GameObject.Find("PauseMenu").transform.SetAsLastSibling();
        GameObject.Find("Dialogue").transform.SetParent(transform.parent,true);
        GameObject.Find("Dialogue").transform.SetAsLastSibling();
        scaleableUIStuffTransform = GameObject.Find("Game Render Texture").transform;

        savingtext_loc_bottom = GameObject.Find("SavingText_Text_Bottom").GetComponent<TMPro.TMP_Text>();
        savingtext_loc = GameObject.Find("SavingText_Text").GetComponent<TMPro.TMP_Text>();
        savingtext_loc.alpha = savingtext_loc_bottom.alpha = 0;

        pulse = GameObject.Find("GlandilockPulse").GetComponent<ParticleSystem>();
        glandilockAnim = GameObject.Find("GlandilockSeed").GetComponent<SpriteAnimator>();
        glandilockPS = GameObject.Find("GlandilockSeed").GetComponent<PositionShaker>();

        if (curScene == "NanoZera" || curScene == "PicoZera") {
            glandilockAnim.Play("angry");
        }

        Set2DScreenResolution(0,true);
           
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.IndexOf("Pico") != -1) {
            GameObject.Find("PlayerInteractionIcon").name = "ignoredPlayerinteract";
            GameObject.Find("PicoPlayerInteractionIcon").name = "PlayerInteractionIcon";
        }

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "NanoAlbumen") {
            GameObject.Find("GlandilockSeed").SetActive(false);
        }

        UnderDialogueLayer = GameObject.Find("Under Dialogue Fade Layer").GetComponent<Image>();
        transform.parent.GetComponent<Canvas>().worldCamera = GameObject.Find("UI Camera 2D").GetComponent<Camera>();

        SuckedItemT = GameObject.Find("SuckedItem").transform;
        initialSuckedItemPos = SuckedItemT.transform.localPosition;

        if (QUEUED_ColorlessFade) {
            StartFade(startAlpha, endAlpha, tmFade);
            QUEUED_ColorlessFade = false;
        }

        SceneData2D sd2d = GameObject.Find("2D SceneData").GetComponent<SceneData2D>();
        if (sd2d.hasMinimap) {
            hasMinimap = true;
            // Create tiles and position them
            GameObject GameMinimap = GameObject.Find("GameMinimap");
            minimapTiles = new Image[5, 5];
            topLeftRoomCoord = sd2d.topLeftRoomCoordinates;
            for (int i = 0; i < 5; i++) { // row
                for (int j = 0; j < 5; j++) { // col
                    minimapTiles[i, j] = Instantiate(minimapTilePrefab, GameMinimap.transform).GetComponent<Image>();
                    minimapTiles[i, j].rectTransform.localPosition = new Vector3(-10 + j * 5, 10 - i * 5, 0);
                    minimapTiles[i, j].rectTransform.SetAsFirstSibling();
                }
            }

            // Parse CSV into tilemap data.
            string[] rows = sd2d.gameMinimapCSV.Split('\n');
            minimapValues = new int[rows.Length, rows[0].Split(',').Length];
            minimapVisitedState = SaveManager.Get_SceneMinimapVisitedState_As_Array(curScene, rows.Length, rows[0].Split(',').Length);
            // width = minimapValues dim  2, 
            for (int rowIndex = 0; rowIndex < rows.Length; rowIndex++) {
                string[] tileIndices = rows[rowIndex].Split(',');
                for (int i = 0; i < tileIndices.Length; i++) {
                    minimapValues[rowIndex, i] = int.Parse(tileIndices[i], System.Globalization.CultureInfo.InvariantCulture);
                }
            }
            GameMinimapSpritesheet = Resources.LoadAll<Sprite>("Visual/Sprites/UI/minimap_tiles");
        } else {
            GameObject.Find("MinimapPlayerPosition").SetActive(false);
        }


        if (UIManagerAno2.sceneStartFadeHoldTime != -1) {
            UIManagerAno2.fadeHoldTime = UIManagerAno2.sceneStartFadeHoldTime;
            UIManagerAno2.sceneStartFadeHoldTime = -1;
        }
    }

    public Vector2Int GetPlayerMinimapPos() {
        Vector2Int v = new Vector2Int();
        Vector2Int vp = new Vector2Int();
        HF.GetRoomPos(player.transform.position, ref vp);
        v.x = vp.x - topLeftRoomCoord.x;
        v.y = topLeftRoomCoord.y - vp.y;
        return v;
    }
    public bool HasMinimap() {
        return hasMinimap;
    }
    public int GetMinimapTileWidth() {
        return minimapValues.GetLength(1);
    }
    public int GetMinimapTileHeight() {
        return minimapValues.GetLength(0);
    }

    public int[,] GetMinimapVisualState() {
        return minimapValues;
    }
    public int[,] GetMinimapVisitedState() {
        return minimapVisitedState;
    }
    public Sprite[] GetMinimapSpritesheet() {
        return GameMinimapSpritesheet;
    }


    int[,] minimapVisitedState;

    public void RefreshGameMinimap(float playerWorldX, float playerWorldY) {
        if (!hasMinimap) return;

        Vector2Int curPlayerRoomCoord = new Vector2Int();
        curPlayerRoomCoord.x = HF.GetRoomX(playerWorldX);
        curPlayerRoomCoord.y = HF.GetRoomY(playerWorldY);
        Vector2Int curPlayerAbsMinimapPos = new Vector2Int();
        curPlayerAbsMinimapPos.x = curPlayerRoomCoord.x - topLeftRoomCoord.x;
        curPlayerAbsMinimapPos.y = topLeftRoomCoord.y - curPlayerRoomCoord.y;
        if (curPlayerAbsMinimapPos.x >= 0 && curPlayerAbsMinimapPos.y >= 0 && curPlayerAbsMinimapPos.x < minimapValues.GetLength(1) && curPlayerAbsMinimapPos.y < minimapValues.GetLength(0)) {
            minimapVisitedState[curPlayerAbsMinimapPos.y, curPlayerAbsMinimapPos.x] = 1;
            SaveManager.Update_SceneMinimapVisitedState_With_Array(curScene, minimapVisitedState);
        }


        // if top left is 0,4, then if player at 2,2, offset is 2,2

        // Offsetting with this gives a value into 
        for (int relY = 0; relY < minimapTiles.GetLength(0); relY++) {
            for(int relX = 0; relX < minimapTiles.GetLength(1); relX++) {
                // x . . . .
                // . . . . .
                // . . p . .

                // If player at 2,2 then top left of minimap is at 0,4, 
                // Top left to bottom right
                int absY = relY - 2  + curPlayerAbsMinimapPos.y;
                int absX = relX - 2 + curPlayerAbsMinimapPos.x;
                if (absY < 0 || absX < 0 || absY >= minimapValues.GetLength(0) || absX >= minimapValues.GetLength(1)) {
                    minimapTiles[relY, relX].enabled = false;
                } else if (minimapVisitedState[absY,absX] == 0) {
                    minimapTiles[relY, relX].enabled = false;
                } else {
                    minimapTiles[relY, relX].enabled = true;
                    minimapTiles[relY, relX].sprite = GameMinimapSpritesheet[minimapValues[absY,absX]];
                }
            }
        }

    }

    Sprite[] GameMinimapSpritesheet;
    bool hasMinimap = false;
    Vector2Int topLeftRoomCoord;
    int[,] minimapValues;

    Transform scaleableUIStuffTransform;
    Vector3 newscale = new Vector3();
    public void Set2DScreenResolution(float scalingValue=3f,bool calculateit=false,float overrideHeight=0) {

        //Update Canvas Scaler
        canvasScaler = transform.parent.GetComponent<CanvasScaler>();
        canvasScaler.enabled = true;
        UpdateCanvasScalerSizeToDefault(canvasScaler);
    
        /*
        // start debug
        print("WARNING DEBUG FOR FORCING 720P");
        UpdateCanvasScalerSizeToDefault(canvasScaler,1280,720);
        scalingValue = 3;
        overrideHeight = 720;
        calculateit = false;
        // end debug
        */

        // Set UI Element scaling
        if (calculateit) {
            if (Screen.fullScreen) {
                scalingValue = getIdealScaleValue(BaseUIBGHeight, Screen.resolutions[Screen.resolutions.Length-1].height);
            } else {
                scalingValue = getIdealScaleValue(BaseUIBGHeight, overrideHeight);
            }
        }
        newscale.Set(scalingValue, scalingValue, scalingValue);
        scaleableUIStuffTransform.localScale = newscale;

        // Set dialogue box scaling
        GameObject.Find("Dialogue").transform.localScale = newscale;

        //print("Using UI scale of " + scalingValue);
    }


    // Updates the Canvas scaler size
    // If no w/h is specified, then defaults to current windowed resolution or the max fullscreen resolution
    public static void UpdateCanvasScalerSizeToDefault(CanvasScaler canvasScaler, float forceWidth = 0, float forceheight = 0) {

        Vector2 newScreenRes = new Vector2();
        if (forceWidth != 0 && forceheight != 0) {
            newScreenRes.Set(forceWidth, forceheight);
            canvasScaler.referenceResolution = newScreenRes;
            //print("Updating canvas scaler to:" + forceWidth + "x" + forceheight);
            return;
        }
        if (Screen.fullScreen) {
            newScreenRes.Set(Screen.resolutions[Screen.resolutions.Length - 1].width, Screen.resolutions[Screen.resolutions.Length - 1].height);
            //print("Updating canvas scaler to:" + Screen.resolutions[Screen.resolutions.Length - 1].width + "x" + Screen.resolutions[Screen.resolutions.Length - 1].height);
        } else {
            newScreenRes.Set(SaveManager.winResX, SaveManager.winResY);
            //print("Updating canvas scaler to:" + SaveManager.winResX+ "x" + SaveManager.winResY);
        }
        canvasScaler.referenceResolution = newScreenRes;
    }


    public static float getIdealScaleValue(float baseUIVal=230f,float overrideHeight=0) {
        float screenHeight = SaveManager.winResY;
        if (SaveManager.winResX / (float) SaveManager.winResY < 1.76f) {
            print("Squarer aspect ratio - calculating scale based on a truncated height.");
            screenHeight = SaveManager.winResX / (16f / 9f);
            screenHeight = Mathf.FloorToInt(screenHeight);
            print("new screen height for calc purposes: " + screenHeight);
        }
     //   print("Current screenHeight: " + screenHeight);
        if (overrideHeight != 0) {
            screenHeight = overrideHeight;
        }
        float scalingValue = screenHeight / baseUIVal;
        scalingValue = Mathf.Floor(scalingValue);
        //print(scalingValue);
        if (SaveManager.customUIScaling > 2) return SaveManager.customUIScaling;
        return scalingValue;
    }

    Color saveColor = new Color();
    int SavingTextMode = 0;
    Image SavingTextImage;
    float t_SavingTextFade = 0;
    bool using_loc_save = false;
    public void StartSavingTextFade() {
        SavingTextImage = GameObject.Find("SavingText").GetComponent<Image>();
        using_loc_save = false;
        if (SaveManager.language != "en" ) {
            savingtext_loc.text = PauseMenu.GetSavingStringHardcoded(SaveManager.language);
            savingtext_loc_bottom.text = savingtext_loc.text;
            using_loc_save = true;
        }
        SavingTextMode = 1;
    }

    bool fadeCancelled = false;
    public void CancelSceneEntryFade() {
        fadeCancelled = true;
    }

    float t_glandShake = 0;
    public void AngryPulse() {
        glandMode = 1;
        t_glandShake = 2f;
        pulse.GetComponent<ParticleSystemRenderer>().material = GlandilockAngryMat;
        pulse.Play();
        glandilockPS.enabled = true;
        glandilockAnim.Play("angryflash");
        AudioHelper.instance.playOneShot("GlandPulseAngry");
    }
    int glandMode = 0;


	void Update () {

        updateSparkBar();
        if (glandMode == 0) {

        } else if (glandMode == 1) {
            t_glandShake -= Time.deltaTime;
            if (t_glandShake < 0) {
                glandMode = 0;
                glandilockAnim.Play("angry");
                glandilockPS.enabled = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
          //  UpdateCanvasScalerSizeToDefault(1920,1080);
           // Set2DScreenResolution(0,true,1080);
        }
        if (Input.GetKeyDown(KeyCode.Q)) {
        //    UpdateCanvasScalerSizeToDefault(1600,900);
         //   Set2DScreenResolution(0, true, 900);
        }

        if (SavingTextMode == 1) {
            if (using_loc_save) {
                savingtext_loc.alpha += Time.deltaTime * 2f;
                if (savingtext_loc.alpha >= 1) {
                    savingtext_loc.alpha = 1;
                    SavingTextMode = 2;
                    t_SavingTextFade = 0;
                }
                savingtext_loc_bottom.alpha = savingtext_loc.alpha;
            } else {
                saveColor = SavingTextImage.color;
                saveColor.a += Time.deltaTime * 2f;
                if (saveColor.a >= 1) {
                    saveColor.a = 1;
                    SavingTextMode = 2;
                    t_SavingTextFade = 0;
                }
                SavingTextImage.color = saveColor;
            }
        } else if (SavingTextMode == 2) {
            t_SavingTextFade += Time.deltaTime;
            if (t_SavingTextFade > 1) {
                SavingTextMode = 3;
            } 
        } else if (SavingTextMode == 3) {
            if (using_loc_save) {
                savingtext_loc.alpha -= Time.deltaTime;
                if (savingtext_loc.alpha <= 0) {
                    savingtext_loc.alpha = 0;
                    SavingTextMode = 0;
                }
                savingtext_loc_bottom.alpha = savingtext_loc.alpha;
            } else {
                saveColor = SavingTextImage.color;
                saveColor.a -= Time.deltaTime * 2f;
                if (saveColor.a <= 0) {
                    saveColor.a = 0;
                    SavingTextMode = 0;
                }
                SavingTextImage.color = saveColor;
            }
        }
  
        if (playerInteractionIconMode == 1) {

        } else if (playerInteractionIconMode == 2) {
            if (interactIconAnim.isPlaying == false) {
                interactIconAnim.GetComponent<SpriteRenderer>().enabled = false;
                playerInteractionIconMode = 0;
            }
        }

        if (t_colorfade >= 0) {
            t_colorfade += Time.deltaTime;
            UnderDialogueLayer.color = Color.Lerp(colorfade_start, colorfade_end, t_colorfade / tm_colorfade);
            if (t_colorfade > tm_colorfade) {
                t_colorfade = -1;
            }
        }

        if (fadeCancelled) {
            //print("Fade cancelled");
            fadeCancelled = false;
            fadeMode = 0;
            tFade = 0;
            tmFade = defaultFadeTime;
        }

        if (fadeMode == 1) {
            if (UIManagerAno2.fadeHoldTime > 0) {
                UIManagerAno2.fadeHoldTime -= Time.deltaTime;
                destinationColor.a = 1;
                UnderDialogueLayer.color = destinationColor;
            } else {
                tFade += Time.deltaTime;
                if (MyInput.shortcut) {
                    tFade += 5 * Time.deltaTime;
                }
                destinationColor.a = Mathf.SmoothStep(startAlpha, endAlpha, tFade / tmFade);
                UnderDialogueLayer.color = destinationColor;
                //print(destinationColor.a);
                if (tFade > tmFade) {
                    tFade = 0;
                    tmFade = defaultFadeTime;
                    fadeMode = 2;
                }
            }
        } else if (fadeMode == 2) {

        }

        if (ext_ItemWasSucked) {
            ext_ItemWasSucked = false;
            SuckedItem_mode = 1;
            ScaleRange.y = initialMaxScaleRange;
            HF.randomizeVec2(ref tempSuckedItemVel, 9f);
            SuckedItemT.localPosition = initialSuckedItemPos;
        }

        tempSuckedItemPos = SuckedItemT.localPosition;
        tempSuckedItemPos.x += Time.deltaTime * tempSuckedItemVel.x;
        tempSuckedItemPos.y += Time.deltaTime * tempSuckedItemVel.y;

        if (Vector3.Distance(tempSuckedItemPos,initialSuckedItemPos) > maxSuckedItemDistanceFromInitPos) {
            HF.RotateVector2(ref tempSuckedItemVel, 135 + 90 * Random.value);
            ScaleRange.y = 1.1f;
            SuckedItem_mode = 1;
        } else {
            SuckedItemT.localPosition = tempSuckedItemPos;
        }


        if (SuckedItem_mode == 1) {
            tempScale = SuckedItemT.localScale;
            // Makes the cos wave go faster over time
            float timescale = Mathf.Lerp(TimescaleRange.x, TimescaleRange.y, t_SuckedItemEffect / EffectTimespan);
            t_SuckedItemEffect += Time.deltaTime * timescale;
            // 
            float maxScaling = Mathf.Lerp(ScaleRange.y, ScaleRange.x, t_SuckedItemEffect / EffectTimespan) - 1;
            float waveFactor = (Mathf.Cos(t_SuckedItemEffect * 6.28f*rateSpeedup) + 0.8f) / 2f; // Get cos in -.2 to .8
            // if t_sucked.. is at max, then maxscale will  always be (1-1) = 0 so the final obj scale is back at 1.
            nextScale = 1 + (waveFactor * maxScaling);
            tempScale.Set(nextScale, nextScale, nextScale);
            SuckedItemT.localScale = tempScale;
            if (t_SuckedItemEffect > EffectTimespan) {
                t_SuckedItemEffect = 0;
                SuckedItem_mode = 0; 
            }
        }

    }

    void SetAlpha(Image image, Color col, float alpha) {
        col = image.color;
        col.a = alpha;
        image.color = col;
    }


    #region SPARKBAR CODE
    PositionShaker SparkBarShaker;
    Image SparkBar;
    Image SparkBarBG;
    Color SparkBarColor = new Color();
    int modeSparkBar = 0;
    Vector2 tempSize;


    void initSparkBarThings() {
        SparkBarBG = GameObject.Find("SparkBarBG").GetComponent<Image>();
        SparkBar = GameObject.Find("SparkBar").GetComponent<Image>();
        SparkBarShaker = SparkBarBG.GetComponent<PositionShaker>();

        SetAlpha(SparkBar, SparkBarColor, 0);
        SetAlpha(SparkBarBG, SparkBarColor, 0);
    }

    public void setSparkBarSize(float cur, float max, bool noPauseAtEnd = false) {
        if (cur <= 0) cur = 0;
        if (cur >= max) cur = max;
        if (modeSparkBar >= 4) return;
        tempSize = SparkBar.rectTransform.sizeDelta;
        tempSize.x = (int)94f * (1 - (cur / max));
        SparkBar.rectTransform.sizeDelta = tempSize;
        // Fade in
        if (cur < max && modeSparkBar == 0) {
            modeSparkBar = 1;
            // Fade out
        } else if (cur == max && modeSparkBar == 2) {
            modeSparkBar = 3;
            // Ending animation + sound
        } else if (cur == 0 & modeSparkBar == 2) {
            if (DialogueAno2.AnyScriptIsParsing) return; // prevent same frame bugs
            setTalkAvailableIconVisibility(false);
            if (noPauseAtEnd) {
                modeSparkBar = 3;
            } else {
                AudioHelper.instance.playOneShot("sparkBarShatter");
                player.pausedBySparkBar = true;
                modeSparkBar = 4;
            }
        }
        SparkBarShaker.amplitude.x = 3 * (1 - (cur / max));
        SparkBarShaker.amplitude.y = 3 * (1 - (cur / max));
    }

    private void updateSparkBar() {
        if (modeSparkBar == 0) {

        } else if (modeSparkBar == 1) {
            SparkBarColor = SparkBar.color;
            SparkBarColor.a += 4 * Time.deltaTime;
            SparkBar.color = SparkBarColor;
            SparkBarBG.color = SparkBarColor;
            if (SparkBarColor.a >= 1) {
                modeSparkBar = 2;
            }
        } else if (modeSparkBar == 2) {

        } else if (modeSparkBar == 3) {
            SparkBarColor = SparkBar.color;
            SparkBarColor.a -= 4 * Time.deltaTime;
            SparkBar.color = SparkBarColor;
            SparkBarBG.color = SparkBarColor;
            if (SparkBarColor.a <= 0) {
                modeSparkBar = 0;
            }
        } else if (modeSparkBar == 4) {
            SparkBarShaker.amplitude.Set(1, 1, 1);
            t_SparkDeg += 2*Time.deltaTime;
            t_SparkDeg *= SparkMul;
            if (t_SparkDeg > tm_SparkDeg) {
                t_SparkDeg = tm_SparkDeg;
                modeSparkBar = 5;
                AudioHelper.instance.playSFX("sparkBarShrunk");
            }
            SparkDeg = 55 + (270 - 55f) * (t_SparkDeg / tm_SparkDeg);
            float v = (1 + Mathf.Sin(Mathf.Deg2Rad * SparkDeg)) / 2f;
            float newWidth = v * 100f * 1.1f;
            tempSize = SparkBarBG.rectTransform.sizeDelta;
            tempSize.x = newWidth;
            SparkBarBG.rectTransform.sizeDelta = tempSize;

            if (newWidth < 12) v = 0;
            newWidth = v * 94f * 1.1f;
            tempSize = SparkBar.rectTransform.sizeDelta;
            tempSize.x = newWidth;
            SparkBar.rectTransform.sizeDelta = tempSize;
        } else if (modeSparkBar == 5) {

        }
    }
    float tm_SparkDeg = 7.5f;
    float SparkMul = 1.053f;
    float SparkDeg = 55;
    float t_SparkDeg = 0;

    public bool IsSparkBarVisible() {
        return (modeSparkBar != 0);
    }
    public bool IsSparkBarClosingAnimDone() {
        return modeSparkBar == 5;
    }

    #endregion

    float EffectTimespan = 2f;
    float initialMaxScaleRange = 1.4f;
    Vector2 ScaleRange = new Vector2(1, 1.4f);
    Vector2 TimescaleRange = new Vector2(1, 2f);
    float nextScale = 1;
    float rateSpeedup = 3.2f;

    public bool FadeFinished(bool reset = false) {
        if (fadeMode == 2) {
            if (reset) fadeMode = 0;
            return true;
        }
        return false;
    }

    Color colorfade_start;
    Color colorfade_end;
    float t_colorfade = -1;
    float tm_colorfade;

    public void FadeColor(Color _c, float fadetime) {
        colorfade_start = UnderDialogueLayer.color;
        colorfade_end = _c;
        t_colorfade = 0;
        tm_colorfade = fadetime;
    }

    public void StartFade(Color _c, float _startAlpha, float _endAlpha, float fadetime) {
        if (fadetime == 0) {
            _c.a = _endAlpha;
            UnderDialogueLayer.color = _c;
            return;
        }
        destinationColor = _c;
        tmFade = fadetime;
        startAlpha = _startAlpha;
        endAlpha = _endAlpha;
        fadeMode = 1; 
    }

    bool QUEUED_ColorlessFade = false;
    public void QueueColorlessFade(float _start, float end, float time) {
        //print("Queue colorless fade");
        QUEUED_ColorlessFade = true;
        startAlpha = _start;
        endAlpha = end;
        tmFade = time;
    }

    public void StartFade(float _startAlpha, float _endAlpha, float fadetime) {
        //print("Start fade");
        destinationColor = Color.black;
        tmFade = fadetime;
        startAlpha = _startAlpha;
        endAlpha = _endAlpha;
        fadeMode = 1;
    }

    public void setNanoEntryIconVisibility(bool turnOn) {
        GameObject.Find("Nano Available Icon").gameObject.GetComponent<Image>().enabled = turnOn;
    }


    Anodyne.SpriteAnimator interactIconAnim;
    int playerInteractionIconMode = 0;
    public void setTalkAvailableIconVisibility(bool turnOn,bool read=false,string otherObject="") {
        GameObject icon = null;
        
        if (otherObject != "") {
            icon = GameObject.Find(otherObject).gameObject;
        } else {
            icon = GameObject.Find("PlayerInteractionIcon").gameObject;
        }
        // If turned off ignore playing turned off anim again
        if (!turnOn && !icon.GetComponent<SpriteRenderer>().enabled) return;
        if (!turnOn && playerInteractionIconMode == 0) return;
        icon.GetComponent<SpriteRenderer>().enabled = true;
        interactIconAnim= icon.GetComponent<Anodyne.SpriteAnimator>();
        string anima = "";
        if (turnOn) anima = "on_";
        if (!turnOn) anima = "off_";
        if (read) anima += "read";
        if (!read) anima += "unread";
        interactIconAnim.Play(anima);
        if (turnOn) playerInteractionIconMode = 1;
        if (!turnOn) playerInteractionIconMode = 2;
    }

    TMPro.TMP_Text areatitlelower;
    TMPro.TMP_Text areatitletop;
    public void setAreaNameText(string s) {
        if (areatitlelower == null) {
            areatitlelower = GameObject.Find("2DAreaTitle_Bottom_Lower").GetComponent<TMPro.TMP_Text>();
            areatitletop = GameObject.Find("2DAreaTitle_Bottom_Top").GetComponent<TMPro.TMP_Text>();
        }
        areatitlelower.text = s;
        areatitletop.text = s;
    }

    public Sprite UI_Full_Black;
    public void turnHorrorUIOn() {

        UnderDialogueLayer = GameObject.Find("Under Dialogue Fade Layer").GetComponent<Image>();
        GameObject.Find("UI Overlay").GetComponent<Image>().sprite = UI_Full_Black;
        GameObject.Find("UI Overlay").transform.SetSiblingIndex(UnderDialogueLayer.transform.GetSiblingIndex()-1);
        DataLoader.instance.pm.useDarkenedHorrorUI = true;
    }
}
