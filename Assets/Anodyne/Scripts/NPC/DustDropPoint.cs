using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DustDropPoint : MonoBehaviour {

    public bool IsThisTheTopPoint = false;
    public bool IsThisPrismConsole = false;
    public bool IsTriggerForPrismUpgrade = false;
    public bool IsTriggerForDustDrop = false;
    public bool IsTriggerForOpenRing = false;

    [Header("Debug")]
    public bool debugOn = false;
    public bool ignoreFirstMessage = false;
    public bool skipFirst4CardMsgs = false;
    public bool test4Cards = false;
    [Tooltip("Held, Prism Held, Capacity")]
    public bool useDustVals;
    public Vector3 dustVals = new Vector3(0,0,0);

    MediumControl player3D;

    DialogueBox dbox;
    bool skipPressConfirm = false;
    GameObject DustPrismUpgrade;
	void Start () {

        if (IsThisTheTopPoint && 0 == DataLoader.instance.getDS("ccc-entry")) {
            DataLoader.instance.setDS("chapel-entry", 1);
            DataLoader.instance.setDS("ccc-entry", 1);
        }
        if (!IsThisTheTopPoint  && UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "RingCCC") {
            SetShieldMeshPosBasedOnState(); // need PrismUpgradeParent
            SetDustMeshPosBasedOnState(); // need DustMesh
            MaybeTurnOffStormThings(true); // need outerWallMR
            gameObject.SetActive(false);
            return;
        }

        HF.GetPlayer(ref player3D);
        dbox = GameObject.Find("Dialogue").GetComponent<DialogueBox>();
        
        if (IsTriggerForPrismUpgrade) {
            DustPrismUpgrade = GameObject.Find("DustPrismUpgrade");
            DustPrismUpgrade.SetActive(false);
        }
        if (IsTriggerForPrismUpgrade || IsTriggerForDustDrop || IsTriggerForOpenRing) {
            disableTalkIcon = true;
        }

        if (IsTriggerForOpenRing) {
            MaybeTurnOffStormThings();
        }

        if (debugOn && Registry.DEV_MODE_ON) {
            if (useDustVals) {
                Ano2Stats.dust = (int)dustVals.x;
                Ano2Stats.prismCurrentDust = (int)dustVals.y;
                Ano2Stats.prismCapacity = (int)dustVals.z;
                if (Ano2Stats.prismCapacity > 200) {
                    DataLoader.instance.setDS("ddp-open-ring1", 1);
                    DataLoader.instance.setDS("ddp-open-ring2", 1);
                } else if (Ano2Stats.prismCapacity > 100) {
                    DataLoader.instance.setDS("ddp-open-ring1", 1);
                }
            }

            if (skipFirst4CardMsgs) {
                DataLoader.instance.setDS("pal-card-1", 1);
                DataLoader.instance.setDS("pal-card-2", 1);
                DataLoader.instance.setDS("pal-card-3", 1);
                DataLoader.instance.setDS("pal-card-4", 1);
            }
            if (test4Cards) {
                Ano2Stats.GetCard(0);
                Ano2Stats.GetCard(1);
                Ano2Stats.GetCard(2);
                Ano2Stats.GetCard(3);
            }
        } else {
            ignoreFirstMessage = false;
        }
	}

	
    enum Mode { WaitingButNotOverlapped, WaitingAndOverlapped, WaitForNotOverlap, WaitForDialogueThenEnd, WaitForDialogueThenEnterNextMode,
        CardResponse, Nothing,
        UpgradeAnimation,
        DustResponse,
        DustGreeting,
        LevelUp,
        DustAnimation,
        GelResponse,
        UnlockRing2,
        UnlockRing1,
        BadEnd,
        SceneChange,
        End
    }
    Mode mode;
    Mode nextmode;


    public static bool IsDisillusioned() {
        // check dataloader flags...
        return 1 == DataLoader._getDS("pal-ring-3") && 0 == DataLoader._getDS("db-field");
    }

    int submode = 0;
    void Update() {


        if (IsTriggerForPrismUpgrade) {
            UpdatePrismUpgrade();
            return;
        }
        if (IsTriggerForDustDrop) {
            UpdateDustDeposit();
            return;
           
        }
        if (IsTriggerForOpenRing) {
            UpdateOpenRing();
            return;
        }

        if (mode == Mode.WaitForDialogueThenEnd) {
            if (dbox.isDialogFinished()) {
                mode = Mode.End;
            }
        } else if (mode == Mode.End) {
            mode = Mode.WaitForNotOverlap;
        } else if (mode == Mode.WaitForDialogueThenEnterNextMode) {
            if (dbox.isDialogFinished()) {
                mode = nextmode;
            }
        } else if (mode == Mode.WaitForNotOverlap) {
            if (!overlappingplayer) {
                mode = Mode.WaitingButNotOverlapped;
            }
        } else if (mode == Mode.WaitingButNotOverlapped) {
            if (overlappingplayer) mode = Mode.WaitingAndOverlapped;
        } else if (mode == Mode.WaitingAndOverlapped) {
            if (!overlappingplayer) {
                mode = Mode.WaitingButNotOverlapped;
            } else {
                // Talking to console
                if (MyInput.jpTalk || skipPressConfirm) {
                    if (MyInput.jpTalk && player3D.getJumpStateMed() != 0) return;
                    player3D.StopRunning();
                    if (MyInput.jpTalk) {
                        GameObject.Find("3D UI").GetComponent<UIManagerAno2>().setTalkAvailableIconVisibility(false);
                    }
                    if (!ignoreFirstMessage && DataLoader.instance.getDS("chapel-entry") == 2) {
                        SayAndEnd("prism-come-here");
                        return;
                    }
                    if (IsThisPrismConsole) {
                        if (0 == DataLoader.instance.getDS("prism-pal-intro")) {
                            DataLoader.instance.setDS("prism-pal-intro", 1);
                            skipPressConfirm = true;
                            SayAndWait("prism-pal-intro", Mode.WaitingAndOverlapped);
                        } else {
                            skipPressConfirm = false;
                            SayAndWait("prism-greeting", Mode.CardResponse);
                        }
                    } else {
                        if (DataLoader.instance.getDS("ccc-entry") == 0) {
                            SayAndEnd("ddp-too-early");
                        } else if (IsDisillusioned()) {
                            SayAndEnd("ddp-disillusioned-repeat");
                        } else if (Ano2Stats.prismCurrentDust == Ano2Stats.prismCapacity) {
                            if (Ano2Stats.prismCapacity == 100 && dflag(Registry.FLAG_RING_OPEN) == 0) {
                                SayAndEnd("ddp-full-afterfilling", 3); // 'please speak with pal'
                            } else if (Ano2Stats.prismCapacity == 350) {
                                SayAndEnd("ddp-full-afterfilling", 4);  // please speak with visionary'
                            } else {
                                SayAndEnd("ddp-full-afterfilling", 2); // get more cards!
                            }
                        } else {
                            SayAndWait("ddp-greeting", Mode.DustResponse);
                        }
                    }
                }
            }
        } else if (mode == Mode.CardResponse) {
            if (Ano2Stats.prismCapacity == 350) {
                SayAndEnd("prism-maxed");
            } else if (Ano2Stats.CountUnusedCards() >= Ano2Stats.CARDS_PER_PRISM_UPGRADE) {
                SayAndWait("prism-upgrade", Mode.UpgradeAnimation);
            } else {
                SayAndEnd("prism-need-more-cards");
            }
        } else if (mode == Mode.UpgradeAnimation) {
            if (submode == 0) {
                AudioHelper.instance.SkipNextSceneCrossfade();
                DataLoader.instance.enterScene("PrismReinforcementEntry", "CCC");
                submode = 1;
            } else if (submode == 1) { 
            } else if (submode == 100) { // anim done, camerareturned to original pos
                submode = 0;
                if (Ano2Stats.prismCapacity == 200 && dflag("dustbound-done") == 0) {
                    SayAndEnd("prism-disillusioned");
                    DataLoader.instance.setDS("disillusioned", 1); // Make nova disillusioned, can't deposit dust
                }
            }
        } else if (mode == Mode.DustResponse) {

            if (IsDisillusioned()) {
                // Nova... what's that? You don't want to..
                SayAndEnd("ddp-disillusioned");
            } else if (Ano2Stats.dust == 0) {
                // Bring dust!
                SayAndEnd("ddp-no-dust");
            } else {
                mode = Mode.DustAnimation;
            }
        } else if (mode == Mode.DustAnimation) {
            if (submode == 0) {
                submode = 100;
                if (IsThisTheTopPoint) {
                    enteredFromRing = true;
                }
                DataLoader.instance.enterScene("DustDepositEntry", "CCC");
            }
        }
    }
    public static bool enteredFromRing = false;


    [Header("Upgrade - Timeline")]
    public Color tl_UpgradeShieldCol;
    public bool switchUpgradeShieldMat = false;
    public float timelineEndTime = 0;

    [Header("Upgrade - Internal")]
    public GameObject PrismUpgradeParent; // Holds the upgrade stuff, and eventually the final shields
    public MeshRenderer DustPrismUpgrade_Shields_MR; // Upgrade shields
    public Material DustPrismShieldFX2;
    public Cinemachine.CinemachineVirtualCamera InitUpgradeVC;
    public Cinemachine.CinemachineVirtualCamera Upgrade1Cam;
    public Cinemachine.CinemachineVirtualCamera Upgrade2Cam;
    public Cinemachine.CinemachineVirtualCamera Upgrade3Cam;
    public Cinemachine.CinemachineVirtualCamera Upgrade4Cam;
    public Cinemachine.CinemachineVirtualCamera Upgrade5Cam;
    public Cinemachine.CinemachineVirtualCamera Upgrade6Cam;
    UnityEngine.Playables.PlayableDirector director;
    float UpgradeSpacing = 40;
    //float UpgradeInitialShieldY = -335;
    Vector3 tempV = new Vector3();
    bool did_switchUpgradeShieldMat = false;
    void UpdatePrismUpgrade() {
        // Flipped in timeline, switches the upgrade shield material
        if (!did_switchUpgradeShieldMat) {
            if (switchUpgradeShieldMat) {
                did_switchUpgradeShieldMat = true;
                DustPrismUpgrade_Shields_MR.material = DustPrismShieldFX2;
            }
        }
        DustPrismUpgrade_Shields_MR.material.SetColor("_Color", tl_UpgradeShieldCol);

        // Move stuff into place based on game state, init stuff.
        if (submode == 0) {

            SetShieldMeshPosBasedOnState();
            director = GetComponent<UnityEngine.Playables.PlayableDirector>();
            int prismUpgrades = Ano2Stats.CountPrismUpgrades();
            // Move the initial virtual camera to another preset (only InitUpgradeVC is used as a shot clip in timeline)
            Cinemachine.CinemachineVirtualCamera camToCopyFrom = null;
            if (prismUpgrades == 0) camToCopyFrom = Upgrade1Cam;
            if (prismUpgrades == 1) camToCopyFrom = Upgrade2Cam;
            if (prismUpgrades == 2) camToCopyFrom = Upgrade3Cam;
            if (prismUpgrades == 3) camToCopyFrom = Upgrade4Cam;
            if (prismUpgrades == 4) camToCopyFrom = Upgrade5Cam;
            if (prismUpgrades >= 5) camToCopyFrom = Upgrade6Cam;
            InitUpgradeVC.transform.position = camToCopyFrom.transform.position;
            InitUpgradeVC.transform.localEulerAngles = camToCopyFrom.transform.localEulerAngles;
            InitUpgradeVC.m_Lens.FieldOfView = camToCopyFrom.m_Lens.FieldOfView;

            submode = 1;
        } else if (submode == 1) {
            if (overlappingplayer) {
                // Duplicate
                CutsceneManager.deactivatePlayer = true;                    
                Camera.main.GetComponent<Cinemachine.CinemachineBrain>().enabled = true;
                DustPrismUpgrade.SetActive(true);
                director.Play();
                submode = 2;
            }
        } else if (submode == 2) {
            if (director.time >= timelineEndTime) {
                submode = 3;
                Ano2Stats.PrismUpgrade();
                if (Ano2Stats.CountUnusedCards() >= Ano2Stats.CARDS_PER_PRISM_UPGRADE && Ano2Stats.prismCapacity != 350) {
                    dbox.playDialogue("prism-consec");
                } else {
                    dbox.playDialogue("prism-end");
                }
                submode = 3;
            }
        } else if (submode == 3) {
            if (dbox.isDialogFinished()) {
                submode = 4;
                CutsceneManager.deactivatePlayer = false;
                DataLoader.instance.enterScene("PrismEntrance", Registry.GameScenes.CenterChamber);
            }
        }
    }

    void debugSetDust(int prismdust, int prismcapacity, int dust) {
        Ano2Stats.prismCurrentDust = prismdust;
        Ano2Stats.prismCapacity = prismcapacity;
        Ano2Stats.dust = dust;
    }

    void SetShieldMeshPosBasedOnState() {
        tempV = PrismUpgradeParent.transform.position;
        int prismUpgrades = Ano2Stats.CountPrismUpgrades();
        tempV.y = tempV.y - UpgradeSpacing + UpgradeSpacing * prismUpgrades;
        PrismUpgradeParent.transform.position = tempV;
    }

    void SetDustMeshPosBasedOnState() {
        tempV = DustMesh.transform.localPosition;
        tempV.y = DustInitialY + (DustFinalY - DustInitialY) * (Ano2Stats.prismCurrentDust / (1.0f * Ano2Stats.ANODYNE_DUST_GOAL));
        DustMesh.transform.localPosition = tempV;
    }

    [Header("Dust - Internal")]
    public GameObject DustMesh;
    float DustInitialY = -280.5f;
    float DustFinalY = -0.5f;
    float dustToAdd = 0;
    float DustAnimEndY = 0;
    float tDepositDust = 0;
    int dtd = 0;
    int shd = 0;
    int spd = 0;
    void UpdateDustDeposit() {
        if (submode == 0) {

            SetDustMeshPosBasedOnState();
            submode = 1;

        } else if (submode == 1) {
            if (overlappingplayer) {

                shd = (int)Ano2Stats.dust;
                spd = Ano2Stats.prismCurrentDust;
                dustToAdd = Ano2Stats.DepositDust(1, true); // 1 to 60
                dtd = (int)dustToAdd;
                DataLoader.instance.StartDustDeposit(dtd, shd, spd);

                DustAnimEndY = DustInitialY + (DustFinalY - DustInitialY) * (Ano2Stats.prismCurrentDust / (1.0f * Ano2Stats.ANODYNE_DUST_GOAL));

                Cinemachine.CinemachineVirtualCamera camToCopyFrom = null;
                if (Ano2Stats.prismCurrentDust >= 0) camToCopyFrom = Upgrade1Cam;
                if (Ano2Stats.prismCurrentDust > 100) camToCopyFrom = Upgrade2Cam;
                if (Ano2Stats.prismCurrentDust > 150) camToCopyFrom = Upgrade3Cam;
                if (Ano2Stats.prismCurrentDust > 200) camToCopyFrom = Upgrade4Cam;
                if (Ano2Stats.prismCurrentDust > 250) camToCopyFrom = Upgrade5Cam;
                //if (Ano2Stats.prismCurrentDust >= 300) camToCopyFrom = Upgrade6Cam;
                InitUpgradeVC.Priority = 10000;
                InitUpgradeVC.transform.position = camToCopyFrom.transform.position;
                InitUpgradeVC.transform.localEulerAngles = camToCopyFrom.transform.localEulerAngles;
                InitUpgradeVC.m_Lens.FieldOfView = camToCopyFrom.m_Lens.FieldOfView;


                CutsceneManager.deactivatePlayer = true;
                Camera.main.GetComponent<Cinemachine.CinemachineBrain>().enabled = true;
                submode = 2;
            }
        } else if (submode ==2) {
            if (DataLoader.instance.IsDustDepositDone()) {
                submode = 10;
                tDepositDust = 0;
            }
        } else if (submode == 10) {
            tDepositDust += Time.deltaTime;
            if (tDepositDust >= 0.75f) {
                tempV = DustMesh.transform.localPosition;
                if (tempV.y < DustAnimEndY) {
                    tempV.y += Time.deltaTime * 15f;
                    if (tempV.y >= DustAnimEndY) {
                        tempV.y = DustAnimEndY;
                        submode = 11;
                        tDepositDust = 0;
                    }
                }
                DustMesh.transform.localPosition = tempV;
            }
        } else if (submode == 11 ) {
            tDepositDust += Time.deltaTime;
            if (tDepositDust >= 0.75f) {
                submode = 12;
                if (Ano2Stats.prismCapacity == 100 && Ano2Stats.prismCurrentDust == 100 && 0 == dflag(Registry.FLAG_RING_OPEN)) {
                    // ring can be opened, come talk to us
                    dbox.playDialogue("ddp-full-r0", 1);
                    DataLoader.instance.setDS("ddp-full-r0", 1);
                } else if (Ano2Stats.prismCapacity == 350 && Ano2Stats.prismCurrentDust == 350) {
                    dbox.playDialogue("bad-prism"); // Now, nova must speak with CP and CV to compelte the anodyne!
                } else {
                    // Console: ty for adding dust
                    dbox.playDialogue("ddp-dust-done", 0);
                }
                CutsceneManager.deactivatePlayer = false;
            }
        } else if (submode == 12) {
            if (dbox.isDialogFinished()) {
                if (enteredFromRing) {
                    enteredFromRing = false;
                    DataLoader.instance.enterScene("DustEntrance", Registry.GameScenes.RingCCC);
                } else {
                    DataLoader.instance.enterScene("DustEntrance", Registry.GameScenes.CenterChamber);
                }
                submode = 13;
            }
        }
    }

    private int dflag(string name) {
        return DataLoader.instance.getDS(name);
    }

    private void SayAndWait (string dialogue, Mode _nextmode) {
        dbox.playDialogue(dialogue);
        mode = Mode.WaitForDialogueThenEnterNextMode;
        nextmode = _nextmode;
    }
    private void SayAndEnd(string dialogue,int onlyline=-1) {
        dbox.playDialogue(dialogue,onlyline);
        mode = Mode.WaitForDialogueThenEnd;
    }

    [Header("Ring Open - Internal")]
    public Light sun;
    public MeshRenderer outerWallMR;
    public MeshRenderer[] stormMRs;

    Color sunStartColor = new Color();
    Color outerWallStartColor = new Color();
    Color ambientStartColor = new Color();
    Color ambientEndColor = new Color();
    Color sunEndColor = new Color();
    Color outerWallEndColor = new Color();
    float tOpenRing = 0;
    float tmOpenRing = 11f;
    float tmBeginPanCam = 4f;
    float tmEndPanCam = 9f;
    float stormFadeInterval = 1f;
    float stormFadeFac = 1.25f;
    Color tempCol = new Color();


    void MaybeTurnOffStormThings(bool RingCCC_Force=false) {
        if (DataLoader.instance.getDS("ddp-open-ring1") == 1 || RingCCC_Force) {
            InitStormColors();
            // Change wall emission, sun color + angle, and storm clouds set to off.
            outerWallMR.materials[1].SetColor("_EmissionColor", outerWallEndColor);
            if (RingCCC_Force) return;

            GameObject.Find("PostStormOuterThings").transform.Find("Mountains").gameObject.SetActive(true);
            GameObject.Find("PostStormOuterThings").transform.Find("CCCRocksEdge").gameObject.SetActive(true);
            sun.color = sunEndColor;
            tempV = sun.transform.localEulerAngles;
            tempV.x = 70;
            sun.transform.localEulerAngles = tempV;
            RenderSettings.ambientLight = ambientEndColor;
            for (int i = 0; i < stormMRs.Length; i++) {
                stormMRs[i].gameObject.SetActive(false);
            }
        }
    }

    void InitStormColors() {
        ColorUtility.TryParseHtmlString("#9F4C5CFF", out outerWallStartColor);
        ColorUtility.TryParseHtmlString("#59A384FF", out outerWallEndColor);
        ColorUtility.TryParseHtmlString("#ABCAC5FF", out ambientStartColor);
        ColorUtility.TryParseHtmlString("#87D4E8F1", out ambientEndColor);
        ColorUtility.TryParseHtmlString("#FF6A27FF", out sunStartColor);
        ColorUtility.TryParseHtmlString("#B49A7FFF", out sunEndColor);
    }

    void UpdateOpenRing() {
        if (submode == 0) {
            submode = 1;
        } else if (submode == 1) {
            if (overlappingplayer) {
                AudioHelper.instance.StopSongByName("CCC");
                AudioHelper.instance.PlaySong("CP_Harp", 0, 0, false);
                Ano2Stats.prismCurrentDust = 0;
                DataLoader.instance.setDS("ddp-open-ring1", 1);
                InitUpgradeVC.Priority = 150;
                CutsceneManager.deactivatePlayer = true;
                Camera.main.GetComponent<Cinemachine.CinemachineBrain>().enabled = true;
                submode = 20;
                InitStormColors();
            }
        } else if (submode == 20) {
            tOpenRing += Time.deltaTime;
            if (tOpenRing > 2f) {
                tOpenRing = 0;
                submode = 2;
            }
        } else if (submode == 2) {
            tOpenRing += Time.deltaTime;
            // if (tOpenRing > tmBeginColorChanges) {
            //  if (tOpenRing > stormFadeInterval*1)
                float r = tOpenRing / (stormFadeInterval * 4f);
                float smoothFac = Mathf.SmoothStep(0, 1, r);

                outerWallMR.materials[1].SetColor("_EmissionColor", Color.Lerp(outerWallStartColor, outerWallEndColor, smoothFac));

                sun.color = Color.Lerp(sunStartColor, sunEndColor, smoothFac);
                tempV = sun.transform.localEulerAngles;
                tempV.x = Mathf.Lerp(35, 70, smoothFac);
                sun.transform.localEulerAngles = tempV;

                RenderSettings.ambientLight = Color.Lerp(ambientStartColor, ambientEndColor, smoothFac);
           // }

            if (tOpenRing > tmBeginPanCam) {
                tempV = InitUpgradeVC.transform.localEulerAngles;
                tempV.x = Mathf.SmoothStep(-30, 15, (tOpenRing - tmBeginPanCam) / (tmEndPanCam - tmBeginPanCam));
                InitUpgradeVC.transform.localEulerAngles = tempV;
            }

            for (int i = 0; i < stormMRs.Length; i++) {
                // 0, 1, 2, 3 inner to outer
                tempCol = stormMRs[i].material.color;
                if (i == 0) {
                    tempCol.a -= stormFadeFac * Time.deltaTime;
                } else if (i == 1 && tOpenRing > stormFadeInterval * 1) {
                    tempCol.a -= stormFadeFac * Time.deltaTime;
                } else if (i == 2 && tOpenRing > stormFadeInterval * 2) {
                    tempCol.a -= stormFadeFac * Time.deltaTime;
                } else if (i == 3 && tOpenRing > stormFadeInterval * 3) {
                    tempCol.a -= stormFadeFac * Time.deltaTime;
                }
                if (tempCol.a < 0) tempCol.a = 0;
                // REMOVE
                if (tOpenRing > tmOpenRing) {
                    tempCol.a = 1;
                }
                stormMRs[i].material.color = tempCol;
                
            }
            if (tOpenRing > tmOpenRing) {
                tOpenRing = 0;
                dbox.playDialogue("open-ring-ending");
                InitUpgradeVC.m_Priority = 0;
                Camera.main.GetComponent<Cinemachine.CinemachineBrain>().m_DefaultBlend.m_Style = Cinemachine.CinemachineBlendDefinition.Style.EaseInOut;
                Upgrade1Cam.m_Priority = 150;
                submode = 3;
            }
        }  else if (submode == 3 && dbox.isDialogFinished()) {
            CutsceneManager.deactivatePlayer = false;
            DataLoader.instance.enterScene("chapel-pos", Registry.GameScenes.CenterChamber);
            submode = 4;
        }
    }

    bool overlappingplayer;
    bool disableTalkIcon = false;
    private void OnTriggerEnter(Collider other) {
        if (!other.CompareTag("Player")) return;
        if (!disableTalkIcon) GameObject.Find("3D UI").GetComponent<UIManagerAno2>().setTalkAvailableIconVisibility(true);
        overlappingplayer = true;
    }

    private void OnTriggerExit(Collider other) {
        if (!other.CompareTag("Player")) return;
        if (!disableTalkIcon) GameObject.Find("3D UI").GetComponent<UIManagerAno2>().setTalkAvailableIconVisibility(false);
        overlappingplayer = false;
    }

    void debugstuff() {
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            Ano2Stats.addDust(25);
            print("Current Dust: " + Ano2Stats.dust);
            print("---");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2)) {
            Ano2Stats.GetCard(0); Ano2Stats.GetCard(1);
            Ano2Stats.GetCard(2); Ano2Stats.GetCard(3);
            print("Total Cards: " + Ano2Stats.CountTotalCards());
            print("Unused Cards: " + Ano2Stats.CountUnusedCards());
            print("---");
        }

        if (Input.GetKeyDown(KeyCode.Alpha3)) {
            print("Dust" + Ano2Stats.dust);
            print("PrismDust" + Ano2Stats.prismCurrentDust);
            print("PrismCapacity" + Ano2Stats.prismCapacity);
            print("AllCards" + Ano2Stats.CountTotalCards());
            print("UnusedCards" + Ano2Stats.CountUnusedCards());
            print("---");
        }
        if (Input.GetKeyDown(KeyCode.Alpha4)) {
            Ano2Stats.prismCapacity = 100;
        }

    }
}
