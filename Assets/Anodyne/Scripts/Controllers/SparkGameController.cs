using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System;

public class SparkGameController : MonoBehaviour {

    [Header("Debug")]
    public bool debug_on = false;
    public bool dont_go_to_wormhole = false;
    public bool start_in_end = false;
    public bool use_debug_destScene = false;
    public Registry.GameScenes debug_destination = Registry.GameScenes.Cougher;

    TMPro.TMP_Text dimDiveText;

    bool isHorror = false;
    public static string SparkGameDestObjectName = "Entrance";
    public static Registry.GameScenes SparkGameDestScene = Registry.GameScenes.Cougher;
    public static Registry.GameScenes ReturnFrom2D_SourceScene = Registry.GameScenes.NanoAlbumen;
    public static string SceneWhereSparkGameEntered = "CCC";
    public static string DoorToMoveToIfLoseSparkGame = "TongueDoor";
    // Can eventually be set in Door.cs if there is more than one NPC model to show per destination scene.
    public static int SparkGameModelID = 0;
    public static bool Play2DBeamInEffect = false;

    Color SigilColor = new Color();
    float tm_sigilFade = 0.15f;
    float sigilScalePeriod = 0.4f;
    float t_sigilScale = 0;
    float sigilMaxScale = 2.7f;
    float sigilScaleFreq = 0.35f;
    int curSigilDir = -1;
    int sigilMode = 0;



    [Header("Other Properties")]
    float CameraDistance = 3.7f;
    public float exitTransitionStartTime = 810;
    float particleSpeed = 10f;
    float movementSpeed = -3.2f;
    float particleAmplitude = 4f;

    [Header("Model Things")]
    public GameObject CCC_Specific;
    public GameObject Ring_Specific;
    public GameObject Desert_Specific;

    public SparkGameModel[] models;

    [System.Serializable]
    public class SparkGameModel {
        public Registry.GameScenes NanopointScene = Registry.GameScenes.CCC;
        public GameObject ModelInScene = null;
        public int id = 0;
        public float particleInterval = 1f;
        public string patternString  = "0,1,2,3,4";
        public float _FailureZ = 220f;
    }

    //Vector3 forwardDir = new Vector3(0, 0, -1);
    float FailureZ = 198;
    float VictoryZ = 166;
    float StopZ = 157.37f;

    float StartFOV = 74;
    float EndFOV = 60;

    float startCamEulerX = 12;
    float endCamEulerX = -5;

    public Transform StartingVirtualCamera;
    CinemachineVirtualCamera activeVC;

    Animator anim;
    Transform player;
    Vector3 tempV = new Vector3();

    public UnityEngine.Playables.PlayableDirector endsceneDirector;
    public GameObject DustDodgePrefab;
    public GameObject DustyHit3DPrefab;
    public GameObject UpParticlePrefab;
    public GameObject RightParticlePrefab;
    public GameObject DownParticlePrefab;
    public GameObject LeftParticlePrefab;
    public GameObject FalseParticlePrefab;
    public GameObject ParticlePlanePrefab;
    public GameObject PlayerFloorSigil;

    List<Transform> particles = new List<Transform>();
    List<Transform> particlePlanes = new List<Transform>();
    List<int> particleDirs = new List<int>();
    YesNoPrompt yesno;

    DialogueBox dbox;




    [Header("Sigil Materials")]
    public Material Sigil_Shield_Up;
    public Material Sigil_Shield_Right;
    public Material Sigil_Shield_Down;
    public Material Sigil_Shield_Left;
    public MeshRenderer SigilMesh;
    public Material Attack_Up;
    public Material Attack_Right;
    public Material Attack_Down;
    public Material Attack_Left;
    public Material FalseFloorMat;
    public Material CCCSkybox;
    public Material RingSkybox;
    public Material DesertSkybox;
    public Color CCCAmbientColor;
    public Color RingAmbientColor;
    public Color DesertAmbientColor;
    int easiness = 0;
    bool showedDimDive = false;
    int dimDiveMode = 0;
    float tDimDive = 0;
    bool horrormusic = false;
    void Start () {

        if (Registry.DEV_MODE_ON == false) debug_on = false;

        if (debug_on) {
            print("<color=red>Error: </color> SPARKGAME DEBUG STILL ON. PLEASE REMEMBER TO TURN OFF.");
        }

        dimDiveText = GameObject.Find("DimDiveText").GetComponent<TMPro.TMP_Text>();
        dimDiveText.text = DataLoader.instance.getRaw("dimdive", 0);
        if(SaveManager.language == "jp") {
            dimDiveText.font = DataLoader.instance.Font_JpAreaName;
        }

        if (debug_on && use_debug_destScene) {
            SparkGameDestScene = debug_destination;
        }

        easiness = DataLoader.instance.getDS("SPARKGAME_EASINESS");


        // default ccc_specific is on

        if (SparkGameDestScene == Registry.GameScenes.NanoHorror) {
            isHorror = true;
            horrormusic = true;
        }
        foreach (SparkGameModel model in models) {
            if (model.ModelInScene != null && model.NanopointScene == SparkGameDestScene && model.id == SparkGameModelID) {
                model.ModelInScene.SetActive(true);
                string[] a = model.patternString.Split(',');
                int[] _pattern = new int[a.Length];
                for (int i = 0; i < a.Length; i++) {
                    _pattern[i] = int.Parse(a[i], System.Globalization.CultureInfo.InvariantCulture);
                }
                currentPattern = _pattern;
                SparkGameModelID = 0;
                tm_newParticle = model.particleInterval * 1.25f;
                FailureZ = model._FailureZ;
                break;
            }
        }
        List<string> ringDests = new List<string>{ "NanoClone", "NanoStalker", "NanoGolem" } ;
        List<string> desertDests = new List<string>{ "NanoNexus", "NanoOrb", "NanoSkeligum", "NanoHorror" } ;
        CCC_Specific.SetActive(false);
        Ring_Specific.SetActive(false);
        Desert_Specific.SetActive(false);
        if (ringDests.Contains(SparkGameDestScene.ToString())) {
            RenderSettings.ambientLight = RingAmbientColor;
            RenderSettings.skybox = RingSkybox;
            Ring_Specific.SetActive(true);
        } else if (desertDests.Contains(SparkGameDestScene.ToString())) {
            RenderSettings.ambientLight = DesertAmbientColor;
            RenderSettings.skybox = DesertSkybox;
            Desert_Specific.SetActive(true);
        } else {
            RenderSettings.ambientLight = CCCAmbientColor;
            RenderSettings.skybox = CCCSkybox;
            CCC_Specific.SetActive(true);
        }

        tempV =  GameObject.Find("FailLine").transform.position;
        tempV.z = FailureZ;
        GameObject.Find("FailLine").transform.position = tempV;

        if (currentPattern == null) {
            currentPattern = patternTest;
        }

        player = transform;
        tempV = player.position;
        tempV.z = FailureZ - 1;
        player.position = tempV;

        activeVC = StartingVirtualCamera.GetComponent<CinemachineVirtualCamera>();
        float ratio = (player.position.z - FailureZ) / (VictoryZ - FailureZ);
        ratio = Mathf.Clamp01(ratio);
        activeVC.m_Lens.FieldOfView = StartFOV + (EndFOV - StartFOV) * ratio;

        tempV = StartingVirtualCamera.localEulerAngles;
        tempV.x = startCamEulerX + (endCamEulerX - startCamEulerX) * ratio;
        StartingVirtualCamera.localEulerAngles = tempV;


        tempV = StartingVirtualCamera.position;
        tempV.z = player.position.z + CameraDistance;
        StartingVirtualCamera.position = tempV;

        startZ = player.position.z;
        anim = transform.Find("FBXContainer").Find("NovaFBX").GetComponent<Animator>();
        tempV = SigilMesh.transform.localEulerAngles;
        tempV.z = -180;
        SigilMesh.transform.localEulerAngles = tempV;

        novaRenderers = new Renderer[5];
        novaRenderers[0] = GameObject.Find("NovaFBX").transform.Find("Body").GetComponent<SkinnedMeshRenderer>();
        novaRenderers[1] = GameObject.Find("NovaFBX").transform.Find("Plane").GetComponent<SkinnedMeshRenderer>();
        novaRenderers[2] = GameObject.Find("NovaFBX").transform.Find("Armature").transform.Find("Bone").transform.Find("Body1").transform.Find("Body2").transform.Find("Neck").transform.Find("Head").transform.Find("HEAD").GetComponent<MeshRenderer>();
        novaRenderers[3] = GameObject.Find("NovaFBX").transform.Find("Armature").transform.Find("Bone").transform.Find("Body1").transform.Find("Body2").transform.Find("Neck").transform.Find("Head").transform.Find("Ear.L").transform.Find("EAR.L").GetComponent<MeshRenderer>();
        novaRenderers[4] = GameObject.Find("NovaFBX").transform.Find("Armature").transform.Find("Bone").transform.Find("Body1").transform.Find("Body2").transform.Find("Neck").transform.Find("Head").transform.Find("Ear.R").transform.Find("EAR.R").GetComponent<MeshRenderer>();
        newNovaMat = new Material(novaRenderers[0].material);
        foreach (Renderer r in novaRenderers) {
            r.material = newNovaMat;
        }


        anim.Play("Idle");

        HF.GetDialogueBox(ref dbox);

        if (debug_on && start_in_end) {
            mode = doingEntryAnimMode;
            tempV = transform.position;
            tempV.z = VictoryZ;
            transform.position = tempV;
        }
        mode = sceneEntryMode;
    }

    bool skipGame = false;

    float currentZ;
    float startZ;
    int mode = 0;
    int submode = 0;

    int tutorialMode = 0;
    int playingMode = 1;
    int reachedGoalMode = 2;
    int failureMode = 3;
    int enteringNanopointMode = 4;
    int doingEntryAnimMode = 5;
    int sceneEntryMode = 6;

    float curSigilAlpha = 0;
    public Color novaEmissionColor = new Color(0, 0, 0);
    Material newNovaMat;
    Renderer[] novaRenderers;
	void Update () {
        if (horrormusic) {
            horrormusic = false;
            AudioHelper.instance.setPitch("SparkGame", 0.75f);
        }

        newNovaMat.SetColor("_EmissionColor", novaEmissionColor);


        if (Registry.DEV_MODE_ON && Input.GetKeyDown(KeyCode.Escape)) {
            submode = 1;
            mode = enteringNanopointMode;
            t_animWait = 3;
        }

        // Shield logic
        int oldDir = curSigilDir;
        // Nothing held, fade to alpha zero
        if (sigilMode == 0) {
            SigilColor = SigilMesh.material.GetColor("_Color");
            curSigilAlpha -= Time.deltaTime * (1f / tm_sigilFade);
            if (curSigilAlpha < 0) curSigilAlpha = 0;
            SigilColor.a = curSigilAlpha;
            SigilMesh.material.SetColor("_Color", SigilColor);
            if (MyInput.anyDir && mode == playingMode && !gotHurt) {
                sigilMode = 1;
                curSigilDir = -1;
            }
        } else if (sigilMode == 1) {
            if (mode != playingMode || gotHurt) {
                // if get hurt iwth shield up, force an oscillation
                if (gotHurt) {
                    oldDir = -2;
                    curSigilAlpha = 1;
                    t_sigilScale = 0;
                }
                sigilMode = 0;
            } else {
                if (MyInput.up) {
                    curSigilDir = 0;
                    SigilMesh.material = Sigil_Shield_Up;
                } else if (MyInput.right) {
                    curSigilDir = 1;
                    SigilMesh.material = Sigil_Shield_Right;
                } else if (MyInput.down) {
                    curSigilDir = 2;
                    SigilMesh.material = Sigil_Shield_Down;
                } else if (MyInput.left) {
                    curSigilDir = 3;
                    SigilMesh.material = Sigil_Shield_Left;
                } else {
                    sigilMode = 0;
                }
                SigilColor = SigilMesh.material.GetColor("_Color");
                curSigilAlpha += Time.deltaTime * (1f / tm_sigilFade);
                if (curSigilAlpha > 1) curSigilAlpha = 1;
                SigilColor.a = curSigilAlpha;
                SigilMesh.material.SetColor("_Color", SigilColor);
            }
        }

        if (curSigilDir != oldDir) { // reset scaling if direction changed
            t_sigilScale = 0;
            AudioHelper.instance.playOneShot("sparkGameShield", 1);
        }
        t_sigilScale += Time.deltaTime;
        if (t_sigilScale >= sigilScalePeriod) t_sigilScale = sigilScalePeriod;
        float sigilScaleFactor = (1 + Mathf.Sin(Mathf.Deg2Rad * 360f * (t_sigilScale / sigilScalePeriod) * (1f / sigilScaleFreq))) / 2f; // from 0 to 1
        float nextScale = Mathf.Lerp(2, sigilMaxScale, sigilScaleFactor); // get scale based on sine wave value
        nextScale = Mathf.Lerp(nextScale, 2, (t_sigilScale / sigilScalePeriod)); // dampen the scaling based on elapsed time 
        tempV = SigilMesh.transform.localEulerAngles;
        tempV.Set(nextScale, nextScale, nextScale);
        SigilMesh.transform.localScale = tempV;


        if (mode == tutorialMode) {
            if (submode == 0) {
                //DataLoader._setDS(Registry.FLAG_RING_OPEN, 1);
                if (DataLoader._getDS(Registry.FLAG_RING_OPEN) == 1 && DataLoader._getDS("sparkgamehelp2") == 0) {
                    dbox.playDialogue("sparkgamehelp",1);
                    DataLoader.instance.setDS("sparkgamehelp2", 1);
                } else if (DataLoader.instance.getDS("sparkgamehelp") == 0) {
                    dbox.playDialogue("sparkgamehelp",0);
                    GameObject.Find("DimDiveImage").GetComponent<UnityEngine.UI.Image>().enabled = true;
                    //dbox.playDialogue("2d-vid-3");
                    DataLoader.instance.setDS("sparkgamehelp", 1);
                }
                submode = 1;
            } else if (submode == 1 && dbox.isDialogFinished()) {
                mode = playingMode;
                if (DataLoader._getDS("sparkgamehelp") == 1) {
                    DataLoader._setDS("sparkgamehelp", 2);
                    GameObject.Find("DimDiveImage").GetComponent<UnityEngine.UI.Image>().enabled = false;
                }
                if (isHorror) {
                    anim.Play("CollapseIdle");
                } else {
                    anim.Play("RunUnlinked");
                }
                submode = 0;
            }
        } else if (mode == sceneEntryMode) {
            UpdateSceneEntry();
        } else if (mode == playingMode) {
            if (!showedDimDive) {
                if (dimDiveMode == 0) {
                    dimDiveText.alpha = 1;
                    dimDiveMode = 1;
                    if (SaveManager.language == "jp") {
                        dimDiveText.font = DataLoader.instance.Font_JpAreaName;
                    }
                    AudioHelper.instance.playOneShot("cardUpgradeBoom",1,1,1);
                } else if (dimDiveMode == 1) {
                    tDimDive += Time.deltaTime; 
                    if (tDimDive > 0.7f) {
                        dimDiveText.CrossFadeAlpha(0, 0.8f,false);
                        showedDimDive = true;
                    }
                }
            }
            if (tmPush > 0) {
                tmPush -= Time.deltaTime;
                if (tmPush <= 0) {
                    anim.CrossFadeInFixedTime("RunUnlinked", 0.2f);
                }
            }

            UpdateParticles();

            // Dynamic diff scaling?  More you lose - slower bullets, phase out hard bullets

            currentZ = player.position.z;
            float ratio = (currentZ - FailureZ) / (VictoryZ - FailureZ);
            ratio = Mathf.Clamp01(ratio);
            activeVC.m_Lens.FieldOfView = StartFOV + (EndFOV - StartFOV) * ratio;

            bool shortcutHurt = MyInput.shortcut && MyInput.confirm;
            bool shortcutMove = !shortcutHurt && MyInput.shortcut;
            if (isHorror) {
                shortcutMove = true;
            }

            // If NOT holding only shortcut but not confirm:
            if ((gotHurt && !shortcutMove) || shortcutHurt) {
                if (shortcutHurt) t_gotHurt = 0;
                if (tmPush > 0.2f) {
                    // tmPush starts at 0.3f, freeze player for like 0.1f seconds
                } else if (HF.TimerDefault(ref t_gotHurt, tm_gotHurt)) {
                    gotHurt = false;

                } else {
                    tempV = player.position;
                    // move backwards slower the longer you've been hurt
                    tempV.z -= (Time.deltaTime * hurtSpeed) * (1 - (t_gotHurt / tm_gotHurt));
                    if (tempV.z >= FailureZ) {
                        tempV.z = FailureZ;
                        PoofAllAttacks();
                        ClearParticleData();
                        AudioHelper.instance.playOneShot("crystalHitPlayer", 1,1);
                        mode = failureMode;
                        GameObject.Find("3D UI").GetComponent<UIManagerAno2>().StartFade(0.5f, true);
                        submode = 0;
                    }
                    player.position = tempV;
                }
            } else {
                tempV = player.position;
                tempV.z += Time.deltaTime * movementSpeed;
                if (shortcutMove) {
                    tempV.z += Time.deltaTime * 5 * movementSpeed;
                }
                if (tempV.z <= VictoryZ) {
                    AudioHelper.instance.playOneShot("crystalHitPlayer", 1, 1);
                    tempV.z = VictoryZ;
                    PoofAllAttacks();
                    ClearParticleData();
                    mode = reachedGoalMode;
                }
                player.position = tempV;
            }

            tempV = StartingVirtualCamera.localEulerAngles;
            tempV.x = startCamEulerX + (endCamEulerX - startCamEulerX) * ratio;
            StartingVirtualCamera.localEulerAngles = tempV;

            tempV = StartingVirtualCamera.position;
            tempV.z = player.position.z + CameraDistance;
            StartingVirtualCamera.position = tempV;

        } else if (mode == reachedGoalMode) {
            if (debug_on && dont_go_to_wormhole) {
                if (MyInput.jpConfirm) {
                    ResetGameState();
                }
            } else {
                if (easiness > 0) {
                    easiness--;
                    DataLoader.instance.setDS("SPARKGAME_EASINESS", easiness);
                }

                if (isHorror) {
                    GameObject.Find("NovaFBX").transform.localScale = new Vector3(-1.5f, -1.5f, -1.5f);
                }
                mode = doingEntryAnimMode;
                submode = 0;
            }
        } else if (mode == failureMode) {
            if (submode == 0) {
                // Wait a bit before allowing restarting
                if (HF.TimerDefault(ref t_restartWait, 0.5f)) {
                    // Show yes no prompt
                    easiness++;
                    print("Difficulty now " + easiness);
                    yesno = new YesNoPrompt("TryAgainCursor", "TryAgainText", "tryagain", 0);
                    yesno.StartOnYes();
                    submode = 1;

                }
            } else if (submode == 1) {
                // Pick yes or no with up down confirm
                int retval = yesno.Update();
                if (retval == 1) {
                    ResetGameState();
                    GameObject.Find("3D UI").GetComponent<UIManagerAno2>().StartFade(0.6f, false);
                } else if (retval == 0) {
                    submode = 2;
                    DataLoader.instance.setDS("SPARKGAME_EASINESS",easiness);
                    DataLoader.instance.enterScene(DoorToMoveToIfLoseSparkGame, SceneWhereSparkGameEntered, 0);
                }
            } else if (submode == 2) {
            }
        } else if (mode == doingEntryAnimMode) {

            tempV = player.position;
            if (tempV.z >= StopZ) {
                tempV.z += Time.deltaTime * 1.4f* movementSpeed;
                player.position = tempV;
            }
            if (submode == 0) {
                endsceneDirector.enabled = true;
                endsceneDirector.Play();
                submode = 1;
            } else if (submode == 1) {

                if (endsceneDirector.time*60f > exitTransitionStartTime || MyInput.shortcut) {
                    submode = 0;
                    AudioHelper.instance.playSFX("sparkBarShrunk");
                    mode = enteringNanopointMode;
                }
            } else if (submode == 2) {

            }
        } else if (mode == enteringNanopointMode) {
            if (submode == 0) {
                if (skipGame) {
                    // cut to cam
                    Camera.main.GetComponent<CinemachineBrain>().m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.Cut;
                }
                t_animWait = 0;
                submode = 1;
            } else if (submode == 1) {


                if (skipGame && t_animWait == 0) {
                    // return cam back to normal lerping
                    Camera.main.GetComponent<CinemachineBrain>().m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.EaseInOut;
                }

                //AudioHelper.instance.playSFX("SparkGameEnterThing");
                float fadetime = 0.5f;
                if (Registry.DEV_MODE_ON && Input.GetKeyDown(KeyCode.Escape)) {
                    fadetime = 0.05f;
                }
                string flagname = SparkGameDestScene.ToString() + "SPARKGAMEDONE";
                if (SparkGameDestScene.ToString() == "NanoNexus" && SparkGameDestObjectName == "FantasyEntrance") flagname += "1";
                DataLoader.instance.setDS(flagname, 1);
                DataLoader.instance.enterScene("none", Registry.GameScenes.Wormhole,fadetime);
                submode = 2;
            } else if (submode == 2) {

            } else if (submode == 3) {

            }



        }
	}

    int entrySubmode = 0;
    Vector3 entry_cachedVCPos = new Vector3();
    float t_entry = 0;
    float t_entryFade = 0;
    bool b_entry = false;
    public Transform entryLeftCamPos;
    public Transform entryRightCamPos;
    Vector3 entryTempV = new Vector3();
    int entrySubmode_PAN_TO_PLAYER = 2;
    float entry_cachedEulerY = 0;

    private void UpdateSceneEntry() {
        if (entrySubmode == 0) {
            // Frame 1: Warp to left pos , cache original pos
            entry_cachedVCPos = StartingVirtualCamera.position;

            entry_cachedEulerY = StartingVirtualCamera.localEulerAngles.y;
            entryTempV = StartingVirtualCamera.localEulerAngles;
            entryTempV.y = -180f;
            StartingVirtualCamera.localEulerAngles = entryTempV;


            StartingVirtualCamera.position = entryLeftCamPos.position;
            entrySubmode = 1;
        } else if (entrySubmode ==1) {
            // Lerp from left pos to right pos in 3 seconds
            // .5 seocnd from end, begin fade out
            // At end, move position to cachedpos, but somewhere near the enemey (victoryz)
            t_entry += Time.deltaTime;
            StartingVirtualCamera.position = Vector3.Lerp(entryLeftCamPos.position, entryRightCamPos.position, t_entry / 5f);
            t_entryFade += Time.deltaTime;
            if (MyInput.shortcut) {
                t_entry += 10 * Time.deltaTime;
                t_entryFade += 10 * Time.deltaTime;
            }
            if (t_entryFade > 4f && !b_entry) {
                b_entry = true;
                HF.Get3DUI().StartFade(1f, true);
            }
            if (t_entry > 5.8f) {
                entryTempV = StartingVirtualCamera.localEulerAngles;
                entryTempV.y = entry_cachedEulerY;
                StartingVirtualCamera.localEulerAngles = entryTempV;

                entryTempV = StartingVirtualCamera.position;
                StartingVirtualCamera.position = new Vector3(entry_cachedVCPos.x, entry_cachedVCPos.y, VictoryZ - 5f);

                entrySubmode = entrySubmode_PAN_TO_PLAYER;
                t_entry = 0;
                t_entryFade = 0;
                b_entry = false;
                HF.Get3DUI().StartFade(1f, false);
                float defaultFieldLength = 220f - VictoryZ;
                float curFieldLength = FailureZ - VictoryZ;
                entryPanToPlayerTime = (curFieldLength / defaultFieldLength) * 5f;

            }
        } else if (entrySubmode == entrySubmode_PAN_TO_PLAYER) {
            UpdateParticles();
            // in 3? seconds, move backwards to the cached position
            // when in position, go to tutorial mode.
            t_entry += Time.deltaTime;
            if (MyInput.shortcut) {
                t_entry += Time.deltaTime * 10f;
            }
            StartingVirtualCamera.position = Vector3.Lerp(entryTempV, entry_cachedVCPos, Mathf.SmoothStep(0, 1, t_entry / entryPanToPlayerTime));
            if (t_entry >= entryPanToPlayerTime) {
                t_entry = 0;
                entrySubmode = 3;
                mode = tutorialMode;
            }
        } 
    }
    float entryPanToPlayerTime;

    float t_restartWait = 0;
    float t_animWait = 0;

    private void PoofAllAttacks() {
        foreach (Transform t in particles) {
            GameObject DustHit = Instantiate(DustyHit3DPrefab);
            DustHit.transform.position = t.position;
            DustHit.GetComponent<ParticleSystem>().Play();
            Destroy(DustHit, 1.25f);
        }
    }


    private void ClearParticleData() { 
        foreach (Transform t in particles) {
            Destroy(t.gameObject);
        }
        particles.Clear();
        foreach (Transform t in particlePlanes) {
            Destroy(t.gameObject);
        }
        particlePlanes.Clear();
        particleDirs.Clear();
        patternIndex = 0;
        t_newParticle = 0;
    }

    private void ResetGameState() {
        tempV = player.position;
        tempV.z = startZ;
        player.position = tempV;
        ClearParticleData();


        gotHurt = false;
        t_gotHurt = 0;
        mode = sceneEntryMode;
        entrySubmode = entrySubmode_PAN_TO_PLAYER;
        anim.Play("Idle");
        sigilMode = 0;
        curSigilDir = -1;
    }

    bool gotHurt = false;
    float t_gotHurt = 0;
    float tm_gotHurt = 0.6f;
    float hurtSpeed = -12f;
    float tm_newParticle = 0.5f;
    float t_newParticle;

    int[] patternTest = new int[] { 0,1,2,3,4,10,11,12,13 };
    int[] currentPattern;
    int patternIndex = 0;

    public Transform particleStartingPoint;
    void UpdateParticles() {
        float particleInterval = tm_newParticle * (1 + easiness * 0.23f);
        if (HF.TimerDefault(ref t_newParticle,particleInterval)) {
            int particleDir = currentPattern[patternIndex];
            patternIndex++;
            if (patternIndex == currentPattern.Length) patternIndex = 0;
            if (particleDir != 4) {
                GameObject newParticle = null;
                Transform newplane = null;
                if (particleDir == 0) {
                    newParticle = Instantiate(UpParticlePrefab, particleStartingPoint.position, Quaternion.identity);
                    newplane = Instantiate(ParticlePlanePrefab).transform;
                    newplane.GetComponent<MeshRenderer>().material = Attack_Up;
                }
                if (particleDir == 1) {
                    newParticle = Instantiate(RightParticlePrefab, particleStartingPoint.position, Quaternion.identity);
                    newplane = Instantiate(ParticlePlanePrefab).transform;
                    newplane.GetComponent<MeshRenderer>().material = Attack_Right;
                }
                if (particleDir == 2) {
                    newParticle = Instantiate(DownParticlePrefab, particleStartingPoint.position, Quaternion.identity);
                    newplane = Instantiate(ParticlePlanePrefab).transform;
                    newplane.GetComponent<MeshRenderer>().material = Attack_Down;
                }
                if (particleDir == 3) {
                    newParticle = Instantiate(LeftParticlePrefab, particleStartingPoint.position, Quaternion.identity);
                    newplane = Instantiate(ParticlePlanePrefab).transform;
                    newplane.GetComponent<MeshRenderer>().material = Attack_Left;
                }
                if (particleDir >= 10 && particleDir <= 13) {
                    newParticle = Instantiate(FalseParticlePrefab, particleStartingPoint.position, Quaternion.identity);
                    newplane = Instantiate(ParticlePlanePrefab).transform;
                    newplane.GetComponent<MeshRenderer>().material = FalseFloorMat;
                }
                tempV = newplane.position;
                tempV.x = newParticle.transform.position.x;
                tempV.z = newParticle.transform.position.z;
                newplane.position = tempV;

                particlePlanes.Add(newplane);
                particles.Add(newParticle.transform);
                particleDirs.Add(particleDir);


                AudioHelper.instance.playOneShot("crystalShoot", 1,0.9f+0.1f*UnityEngine.Random.value);
            }
        }

        int i = 0;
        Transform particleToKill = null;
        foreach (Transform t in particles) {
            tempV = t.position;
            tempV.z += particleSpeed * Time.deltaTime;
            float ratio = (tempV.z -(player.position.z+1.5f) ) / (particleStartingPoint.position.z - (player.position.z+1.5f));
            ratio = Mathf.Clamp01(ratio);
            if (ratio == 0) {
                particleToKill = t;
            }
            float degs = ratio * 180f;
            float perpendicularOffset = Mathf.Sin(Mathf.Deg2Rad * degs) * particleAmplitude;
            //X+ = left 3 , X- = right 1 , Y+  = up 0 , Y- = down 2
            int idx = particleDirs[i];
            if (idx % 10 == 0) tempV.y = particleStartingPoint.position.y + perpendicularOffset;
            if (idx % 10 == 1) tempV.x = particleStartingPoint.position.x - 1.75f*perpendicularOffset;
            if (idx % 10 == 2) tempV.y = particleStartingPoint.position.y - 1;
            if (idx % 10 == 3) tempV.x = particleStartingPoint.position.x + 2*perpendicularOffset;
            t.position = tempV;

            tempV = t.localEulerAngles;
            tempV.z += Time.deltaTime * 540f;
            t.localEulerAngles = tempV;

            // Rotate orientation to movement tangent
            if (idx % 10 != 2) {
                tempV = t.localEulerAngles;
                // Ratio goes from 1 to 0
                // up, x from -45 to 45
                if (idx % 10 == 0) {
                    tempV.x = Mathf.SmoothStep(45, -25, ratio);
                } else {
                    if (idx % 10 == 1) {// right, y from -50 to 50
                        tempV.y = Mathf.SmoothStep(50, -50, ratio);
                    } else if (idx % 10 == 3) {
                        tempV.y = Mathf.SmoothStep(-50, 50, ratio);
                    }
                }
                t.localEulerAngles = tempV;
            }
           

            tempV = particlePlanes[i].position;
            tempV.z = t.position.z;
            particlePlanes[i].position = tempV;

            bool canBreak = false;
            if (player.position.z < t.position.z ) {
                canBreak = true;
            }
            //if (ratio == 0) {
            if (canBreak) {

                int heldDown = 0;
                if (MyInput.up) heldDown++;
                if (MyInput.down) heldDown++;
                if (MyInput.right) heldDown++;
                if (MyInput.left) heldDown++;

                bool successfullyDefended = false;
                if (idx == 0 && MyInput.up) successfullyDefended = true;
                if (idx == 1 && MyInput.right) successfullyDefended = true;
                if (idx == 2 && MyInput.down) successfullyDefended = true;
                if (idx == 3 && MyInput.left) successfullyDefended = true;

                if (heldDown > 2) successfullyDefended = false;

                bool gotHitLocal = false;

                if (ratio == 0 && idx >= 10 && idx <= 13) {
                    if (!MyInput.anyDir) {
                        successfullyDefended = true;
                    }
                }
                if (ratio == 0 && !successfullyDefended) {
                    if (idx == 0 && !MyInput.up) gotHitLocal = true;
                    if (idx == 1 && !MyInput.right) gotHitLocal = true;
                    if (idx == 2 && !MyInput.down) gotHitLocal = true;
                    if (idx == 3 && !MyInput.left) gotHitLocal = true;
                    if (idx >= 10 && idx <= 13) gotHitLocal = true;
                    if (heldDown > 2) gotHitLocal = true;
                }
                float randPitch = 0.95f + 0.12f * UnityEngine.Random.value;
                if (gotHitLocal) {
                    gotHurt = true;
                    t_gotHurt = 0;
                    anim.Play("Pushed");
                    tmPush = 0.3f;
                    AudioHelper.instance.playOneShot("crystalHitPlayer", 1, randPitch);
                } else if (successfullyDefended) {
                    particleToKill = t;
                    t_gotHurt = 0;
                    if (idx % 10 == 0) AudioHelper.instance.playOneShot("crystalHitU",0.7f,randPitch);
                    if (idx % 10 == 1) AudioHelper.instance.playOneShot("crystalHitR", 0.7f, randPitch);
                    if (idx % 10 == 2) AudioHelper.instance.playOneShot("crystalHitD", 0.7f, randPitch);
                    if (idx % 10 == 3) AudioHelper.instance.playOneShot("crystalHitL", 0.7f, randPitch);
                }
            }

            i++;

        }
        if (particleToKill != null) {
            tempV = particleToKill.transform.position;
            tempV.z = player.transform.position.z;
            particleToKill.transform.position = tempV;
            if (gotHurt) {
                GameObject DustHit = Instantiate(DustyHit3DPrefab);
                DustHit.transform.position = particleToKill.transform.position;
                DustHit.GetComponent<ParticleSystem>().Play();
                Destroy(DustHit, 1.25f);
            } else {
                GameObject DustDodge = Instantiate(DustDodgePrefab);
                DustDodge.transform.position = particleToKill.transform.position;
                tempV = DustDodge.transform.position;
                tempV.z -= particleSpeed * 0.13f;
                DustDodge.transform.position = tempV;
                DustDodge.GetComponent<ParticleSystem>().Play();
                Destroy(DustDodge, 0.6f);
            }

            int idx = particles.IndexOf(particleToKill);
            particles.Remove(particleToKill);

            Destroy(particlePlanes[idx].gameObject);
            particlePlanes.RemoveAt(idx);

            particleDirs.RemoveAt(idx);
            Destroy(particleToKill.gameObject);
        }
    }

    float tmPush = 0;
}
