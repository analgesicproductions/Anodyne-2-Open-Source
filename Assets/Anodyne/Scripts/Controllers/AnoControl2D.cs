using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using Anodyne;
using TMPro;
using System.Collections.Generic;

public class AnoControl2D : MonoBehaviour {

    public bool DEBUG_BEAM_IN = false;
    Rigidbody2D rb;
    Transform PickupRegion;
    Vector3 PickupRegionTVec;
    CircleCollider2D bc;
    Animator animator;
    SpriteRenderer sr;
    DialogueBox dbox;
    Camera cam;
    float roomsize = 10f;
    Vector2 currentRoom;
    int camMode = 0;
    Facing camScrollDir;
    public float camScrollSpeed = 7f;
    Transform SuckZoneParent;
    SpriteRenderer SuckZone_sr;
    Vector3 TempSuckZoneScale = new Vector3();
    Vector3 TempSuckZonePos = new Vector3();
    Color TempSuckZoneColor = new Color();
    float t_SuckZoneScale = 0;
    HealthBar healthbar;
    Vector2 BumpVelocity = new Vector2();
    [System.NonSerialized]
    public bool wrestleOn = false;
   // Vector3 checkpointPos;

   // float T_SpikeCooldown = 0;
   // float SpikeCooldown = 1f;
    float T_DamageCooldown = 0;
    float DamageCooldown = 1.25f;
    [System.NonSerialized]
    public bool CanShootSpark = false;

    TMP_Text AreaTitle_Bottom_Lower; 
    TMP_Text AreaTitle_Bottom_Top; 
    TMP_Text AreaTitle_Center;


    [System.NonSerialized]
    public bool poisoned = false;
    float tPoisonHurt = 0;

    [System.NonSerialized]
    public bool wrestleDodging = false;
    int wrestleDodgeMode = 0;
    [System.NonSerialized]
    public bool wrestleCountering = false;
    [System.NonSerialized]
    public Transform wrestleAxe;
    float t_wrestleDodge = 0;
    [System.NonSerialized]
    public bool isHorror = false;

    public RuntimeAnimatorController horrorAC;
    public void SwitchToHorror() {
        ui.turnHorrorUIOn();
        isHorror = true;
        vacAudioSource.mute = true;
        bc.offset = new Vector2(bc.offset.x, -0.5f);
        animator.runtimeAnimatorController = horrorAC;
        dbox.isHorror = true;
        GameObject.Find("PlayerInteractionIcon").transform.localPosition = new Vector3(0, 1.5f, 0);
        GameObject.Find("Dialog Box").GetComponent<RectTransform>().anchoredPosition = new Vector3(0, -102f, 0);
        GameObject.Find("fadetext").GetComponent<RectTransform>().anchoredPosition = new Vector3(1, 156.6f+39f, 0);

        DataLoader.instance.RefreshHorrorFonts();

        DataLoader.instance.silenceDSFlagsOnce = true;
        if (DataLoader._getDS("horrorbgstate") == 0) {
            DataLoader._setDS("horrorbgstate", 1);
        } else {
            DataLoader._setDS("horrorbgstate", 0);
        }
        if (DataLoader._getDS("horror-garg-4") == 1) {
            if (UnityEngine.Random.value <= 0.5f) {

                DataLoader.instance.silenceDSFlagsOnce = true;
                DataLoader._setDS("horrorbgstate", 2);
            }
        }
    }

    Vector3 screenShakeOffset = new Vector3();
    Vector3 tempVecShake = new Vector3();
    float t_screenShake = 0;
    float screenShakeMagnitude = 0;
    bool screenShakeStaysCentered = false;
    Vector3 screenShakeCenter;

    public void ScreenShake(float pixelMagnitude, float time, bool staysCentered = false) {
        if (!SaveManager.screenshake) {
            return;
        }
        t_screenShake = time;
        screenShakeOffset.Set(0, 0, 0);
        screenShakeMagnitude = pixelMagnitude;
        screenShakeStaysCentered = staysCentered;
        if (staysCentered) {
            screenShakeCenter = cam.transform.position;
        }
    }

    void SetAreaTitleBottom(string s) {
        AreaTitle_Bottom_Lower.text = s;
        AreaTitle_Bottom_Top.text = s;
    }

    [System.NonSerialized]
    public bool sparkAllowedForThisArea = false;
    void Start() {
        MedBigCam.forceRidescaleNextScene = false;
        string curSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        if (curSceneName.IndexOf("Pico") == 0) {
            isInPicoScene = true;
        }

        ui = GameObject.Find("2D UI").GetComponent<UIManager2D>();
        vacAudioSource = GameObject.Find("VacuumAudioSource").GetComponent<AudioSource>();


        healthbar = GetComponent<HealthBar>();
        rb = GetComponent<Rigidbody2D>();
        bc = GetComponent<CircleCollider2D>();
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        Registry.MoveObjectToDestinationDoor(gameObject);
        Registry.destinationDoorName = "";

        jumpDownShadow = GameObject.Find("JumpDownShadow").gameObject;
        jumpDownShadow.GetComponent<SpriteRenderer>().enabled = false;
        AreaTitle_Bottom_Lower = GameObject.Find("2DAreaTitle_Bottom_Lower").GetComponent<TMP_Text>();
        AreaTitle_Bottom_Top = GameObject.Find("2DAreaTitle_Bottom_Top").GetComponent<TMP_Text>();
        AreaTitle_Center = GameObject.Find("2DAreaTitle_Center").GetComponent<TMP_Text>();
        AreaTitle_Center.text = "";
        SetAreaTitleBottom("");

        PickupRegion = transform.Find("PickupRegion");
        SuckZoneParent = transform.Find("SuckZoneParent");
        if (isInPicoScene) {
            GameObject.Find("SuckZone").SetActive(false);
            GameObject.Find("PicoSuckZone").name = "SuckZone";
        } else {
            GameObject.Find("PicoSuckZone").SetActive(false);
        }
        SuckZone_sr = GameObject.Find("SuckZone").GetComponent<SpriteRenderer>();
        dbox = GameObject.Find("Dialogue").GetComponent<DialogueBox>();
        cam = GameObject.Find("2D 160px Camera").GetComponent<Camera>();

        SceneData2D sd2d = GameObject.Find("2D SceneData").GetComponent<SceneData2D>();
        if (sd2d.halfSizeCam) { // note, room size should still be set to 6 or whatever in unity
            cam.orthographicSize /= 2;

            // Need to change this later when using an 8px sprite
            transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            //SuckZoneParent.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            //bc.radius = 0.16f;
            //bc.offset = new Vector2(bc.offset.x, bc.offset.y / 2f);

            moveSpeed /= 2f;
            fullMoveSpeed /= 2f;
            camScrollSpeed /= 2f;
            // pickup zone?
        }

        roomsize = SceneData2D.RoomSize_X;


            TempSuckZoneColor = SuckZone_sr.color;
        TempSuckZoneColor.a = 0;
        SuckZone_sr.color = TempSuckZoneColor;


        if (Registry.justLoaded) {
            transform.position = Registry.enterGameFromLoad_Position;
            SetCheckpointPos();
            Registry.justLoaded = false;
        }
        Vector3 temp = transform.position; temp.z = 0; transform.position = temp;


        currentRoom.x = Mathf.FloorToInt(transform.position.x / roomsize);
        currentRoom.y = Mathf.FloorToInt(transform.position.y / roomsize);

        // debug
        //SparkGameController.SparkGameDestScene = Registry.GameScenes.NanoAlbumen;
        //SparkGameController.SparkGameDestObjectName = "Yolk1Door";
        //    SparkGameController.Play2DBeamInEffect = true;
        if (DEBUG_BEAM_IN) {
            SparkGameController.Play2DBeamInEffect = true;
        }
       
        // Logic for setting up whether beam in anim plays
        if (curSceneName == "NanoAlbumen") otherSceneInfo = 1;
        if (SparkGameController.SparkGameDestObjectName == "Yolk1Door") otherSceneInfo = 1;
        if (SparkGameController.SparkGameDestObjectName == "Yolk2Door") otherSceneInfo = 2;
        if (SparkGameController.SparkGameDestObjectName == "Yolk3Door") otherSceneInfo = 3;
        if (curSceneName == "NanoNexus") otherSceneInfo = 2;
        AreaTitle_Center.text = HF.GetSceneAssociatedText(HF.SceneNameToEnum(curSceneName), "areanames", otherSceneInfo);

        if (curSceneName == "NanoOcean" || curSceneName == "NanoFantasy") {
            mapHasWater = true;
        }
        
        string beamflagname = curSceneName + SparkGameController.SparkGameDestObjectName + "beamed";
        GameObject.Find("FakePlayerSprite").GetComponent<SpriteRenderer>().enabled = false;
        // Entry beam mode will set the correct display area name then immediately go to play mode if entryBeamMode is set to 4
        if (SparkGameController.Play2DBeamInEffect && DataLoader.instance.getDS(beamflagname) == 0) {
            SparkGameController.Play2DBeamInEffect = false;
            DataLoader.instance.setDS(beamflagname, 1);
            entryBeamMode = 0;
            AudioHelper.pausePlayingOfNewTracks = true;
            AreaTitle_Center.alpha = 0;
            animator.speed = 0;
        } else {
            SparkGameController.Play2DBeamInEffect = false;
            entryBeamMode = 4;
        }

        if (curSceneName == "NanoHorror" && transform.position.x > 24f) {
            SwitchToHorror();
        }

        if (curSceneName == "NanoOcean" || curSceneName == "NanoFantasy" || curSceneName == "NanoZera" || curSceneName.IndexOf("Pico") == 0) {
            if (!Ano2Stats.HasItem(Ano2Stats.ITEM_ID_NANOSPARK)) {
                Ano2Stats.GetItem(Ano2Stats.ITEM_ID_NANOSPARK);
            }
        }

        if (Ano2Stats.HasItem(Ano2Stats.ITEM_ID_NANOSPARK)) {
            sparkAllowedForThisArea = true;
        }
        if (curSceneName == "NanoHorror") {
            sparkAllowedForThisArea = false;
        }

    }

    bool isInPicoScene = false;

    public void SetCheckpointPos(Vector3 pos) {
       // checkpointPos = pos;
    }
    public void SetCheckpointPos() {
        //checkpointPos = transform.position;
    }


    public bool InTheSameRoomAs(float x, float y) {
        if (currentRoom.x == HF.GetRoomX(x) && currentRoom.y == HF.GetRoomY(y)) return true;
        return false;
    }

    private void Awake() {
    }
    int mode = 0;
    int MODE_DYING = 2;
    int state_dying = 0;

    Vector2 newvel = new Vector2();

    public float moveSpeed = 10f;
    public float fullMoveSpeed = 10f;
    int sceneEntranceDelayFrames = 1;
    // Update is called once per frame

    public enum Facing { UP, LEFT, RIGHT, DOWN };
    [System.NonSerialized]
    public Facing facing = Facing.DOWN;
    Facing tempFacing_suck;


    void Pause() {
        animator.enabled = false;
        isPaused = true;
    }
    void Unpause() {
        animator.enabled = true;
        isPaused = false;
    }
    [System.NonSerialized]
    public bool pausedBySparkBar = false;
    public bool IsThereAReasonToPause() {
        return (DialogueAno2.AnyScriptIsParsing || DataLoader.instance.isPaused || SaveModule.saveMenuOpen  || pausedBySparkBar || CutsceneManager.deactivatePlayer || !dbox.isDialogFinished() || DataLoader.instance.isChangingScenes || CinemachineOn2D);
    }

    bool mapHasWater = false;
    Vector3 tempWaterPos = new Vector3();
    public static bool CinemachineOn2D = false;
    Vector3 jumpDownStartPos = new Vector3();
    int jumpDownMode = 0;
    float jumpDownArcTime = 0;
    GameObject jumpDownShadow;

    int vacAudioMode = 0;
    AudioSource vacAudioSource;
    public AudioClip clip_vacuumTurnOn;
    public AudioClip clip_vacuumLoop;
    float t_vacuumLoop = 0;
    float tm_vacuumLoop = 3f;
    TMP_Text dyingText;
    string baseDyingText;
  //  int dyingTextIndex = 0;
    float dyingSongPitch = 1f;
    // float t_DyingText = 0;



    [HideInInspector]
    public bool isPaused { get; private set; }
    SpriteRenderer deathFade;
    Vector3 tempDeathPos;
    Vector3 tempStartDeathPos;
    bool noclipon = false;
    void Update() {
        CanShootSpark = false;

        if (MyInput.shortcut  && Input.GetKeyDown(KeyCode.H)) {
            T_DamageCooldown = 0;
            Damage(1);
        }
        if (vacAudioMode == 0) {
            if (!isPaused && SuckAnimShouldPlay() && MyInput.special) {
                vacAudioMode = 1;
                vacAudioSource.PlayOneShot(clip_vacuumTurnOn);
                vacAudioSource.volume = 1f;
            }
        } else if (vacAudioMode == 1) {
            t_vacuumLoop += Time.deltaTime;
            if (t_vacuumLoop >= tm_vacuumLoop) {
                t_vacuumLoop = 0;
                vacAudioSource.PlayOneShot(clip_vacuumTurnOn);
            }
            if (isPaused || !SuckAnimShouldPlay() || !MyInput.special) {
                vacAudioSource.Stop();
                vacAudioMode = 0;
                t_vacuumLoop = 0;
            }
        }


        if (mode == MODE_DYING) {
            if (state_dying == 0) {
                deathFade = GameObject.Find("DeathFade").GetComponent<SpriteRenderer>();
                animator.speed = 1;
                sr.enabled = true;
                if (isInPicoScene) {
                    animator.Play("playerSpinDie_P");
                } else {
                    animator.Play("playerSpinDie");
                }
                AudioHelper.instance.StopAllSongs();
                AudioHelper.instance.PlaySong("DustCue",0,0);
                sr.sortingLayerName = "Foreground"; sr.sortingOrder = -11;
                state_dying = 1;
                tempStartDeathPos = transform.position;
                tempDeathPos.x = currentRoom.x * SceneData2D.RoomSize_X + SceneData2D.RoomSize_X / 2f;
                tempDeathPos.y = currentRoom.y * SceneData2D.RoomSize_Y + SceneData2D.RoomSize_Y / 2f;
                tempDeathPos.z = tempStartDeathPos.z;
                dyingText = GameObject.Find("fadetext").GetComponent<TMP_Text>();
                dyingText.enableWordWrapping = false;
                dyingText.alignment = TextAlignmentOptions.Center;
                dyingText.rectTransform.localPosition = new Vector2(0,0);
                dyingText.color = Color.white;
                dyingText.alpha = 0;
                dyingText.text = DataLoader.instance.getDialogLine("deathMessage", 0);
                rb.isKinematic = true;
                bc.isTrigger = true;
            } else if (state_dying == 1) {
                Color c = deathFade.color;
                c.a += Time.deltaTime;
                transform.position = Vector3.Lerp(tempStartDeathPos, tempDeathPos, c.a);
                if (c.a >= 1) {
                    dyingText.alpha = 1;
                }
                if (c.a >= 1 && MyInput.jpConfirm) {
                    state_dying = 2;
                }
                deathFade.color = c;
            } else if (state_dying == 2) {
                SaveManager._Load(SavePoint.currentInUseFileIndex,false);
                DataLoader.instance.enterNextSceneBasedOnLoadedData();
                state_dying = 3;
            } else if (state_dying == 3) {
            }

            if (state_dying >= 0) {
                dyingSongPitch -= 0.0167f/10f;
                if (dyingSongPitch <= 0.5f) dyingSongPitch = 0.5f;
                AudioHelper.instance.setPitch("DustCue", dyingSongPitch);
            }

            return;
        }

        if (IsThereAReasonToPause()) {
            UpdateScreenShake();
            if (!isPaused) {
                Pause();
            }
            Vector2 newvel = rb.velocity; newvel.x *= 0.65f; newvel.y *= 0.65f;
            if (Mathf.Abs(newvel.x) < 1f && Mathf.Abs(newvel.y) < 1f) newvel = Vector2.zero;
            rb.velocity = newvel;
            return;
        } else {
            if (isPaused) {
                Unpause();
            }
        }

        UpdateGraphicEffects();
        UpdateCameraLogic();
        UpdateScreenShake();

        if (CameraIsChangingRooms() && ridingRaft) {
            if (tempRaftVel.x >= 2.5f) tempRaftVel.x = 2.5f;
            if (tempRaftVel.x <= -2.5f) tempRaftVel.x = -2.5f;
            if (tempRaftVel.y >= 2.5f) tempRaftVel.y = 2.5f;
            if (tempRaftVel.y <= -2.5f) tempRaftVel.y = -2.5f;
            activeRaftRB.velocity = tempRaftVel;
        }

        if (camMode == 1) return;




        if (mode == 0) { // Scene entrance pause mode
            sceneEntranceDelayFrames--;
            if (sceneEntranceDelayFrames <= 0) {
                //Vector3 camnewpos = cam.transform.position;
                //camnewpos.x = currentRoom.x * roomsize + cam.orthographicSize;
                //camnewpos.y = currentRoom.y * roomsize + cam.orthographicSize;
                //cam.transform.position = camnewpos;
                SnapCameraToPlayerAndKeepInRoom();

                mode = MODE_ENTRY_BEAM;
                ui.RefreshGameMinimap(transform.position.x, transform.position.y);
            }
        } else if (mode == MODE_ENTRY_BEAM) {
            UpdateEntryBeamInLogic();
        } else if (mode == 1) {

            if (healthbar.IsDead()) {
                mode = MODE_DYING;
                rb.velocity = Vector2.zero;
                return;
            }
            if (T_DamageCooldown > 0) T_DamageCooldown -= Time.deltaTime;

            UpdateTileLogic();

            if (Registry.DEV_MODE_ON) {
                if (noclipon == false) {
                    if (Input.GetKeyDown(KeyCode.Tab)) {
                        bc.enabled = false;
                        noclipon = true;
                    }
                } else {
                    if (Input.GetKeyDown(KeyCode.Tab)) {
                        bc.enabled = true;
                        noclipon = false;
                    }
                }
            }

            bool inWater = false;
            if (!MovementIsEnabled) {
                newvel.x = 0; newvel.y = 0;
            }
            if (sparkAllowedForThisArea) CanShootSpark = true;
            if (MovementIsEnabled) {
                bool strafing = MyInput.jump || MyInput.special || MyInput.toggleRidescale;


                /*if (isInPicoScene) {
                    newvel.Set(0, 0);
                    if (MyInput.jpUp) picoLastInput = Facing.UP;
                    if (MyInput.jpLeft) picoLastInput = Facing.LEFT;
                    if (MyInput.jpRight) picoLastInput = Facing.RIGHT;
                    if (MyInput.jpDown) picoLastInput = Facing.DOWN;
                    if (MyInput.up || MyInput.right || MyInput.down || MyInput.left) {

                        bool curPicoLastInputWasReleased = false;
                        if (picoLastInput == Facing.UP && !MyInput.up) curPicoLastInputWasReleased = true;
                        if (picoLastInput == Facing.DOWN && !MyInput.down) curPicoLastInputWasReleased = true;
                        if (picoLastInput == Facing.RIGHT && !MyInput.right) curPicoLastInputWasReleased = true;
                        if (picoLastInput == Facing.LEFT && !MyInput.left) curPicoLastInputWasReleased = true;
                        if (curPicoLastInputWasReleased) {
                            if (MyInput.up) picoLastInput = Facing.UP;
                            if (MyInput.down) picoLastInput = Facing.DOWN;
                            if (MyInput.right) picoLastInput = Facing.RIGHT;
                            if (MyInput.left) picoLastInput = Facing.LEFT;
                        }

                        if (picoLastInput == Facing.UP) newvel.y = moveSpeed;
                        if (!strafing && picoLastInput == Facing.UP) facing = Facing.UP;
                        if (picoLastInput == Facing.DOWN) newvel.y = -moveSpeed;
                        if (!strafing && picoLastInput == Facing.DOWN) facing = Facing.DOWN;
                        if (picoLastInput == Facing.RIGHT) newvel.x = moveSpeed;
                        if (!strafing && picoLastInput == Facing.RIGHT) facing = Facing.RIGHT;
                        if (picoLastInput == Facing.LEFT) newvel.x = -moveSpeed;
                        if (!strafing && picoLastInput == Facing.LEFT) facing = Facing.LEFT;

                    }
                } else {
                */
                    if (MyInput.left && !MyInput.right) {
                        if (!strafing) facing = Facing.LEFT;
                        newvel.x = -moveSpeed;
                    } else if (MyInput.right && !MyInput.left) {
                        if (!strafing) facing = Facing.RIGHT;
                        newvel.x = moveSpeed;
                    } else {
                        newvel.x = 0;
                    }

                    bool ignoreUpDownController = false;
                    if (MyInput.controllerActive) {
                        if (MyInput.left || MyInput.right) {
                            if (MyInput.up || MyInput.down) {
                               if (Mathf.Abs(MyInput.moveY) < 0.35f) {
                                   ignoreUpDownController = true;
                               }
                            }
                        }
                    }

                    if (MyInput.up && !MyInput.down && !ignoreUpDownController) {
                        if (!strafing) facing = Facing.UP;
                        newvel.y = moveSpeed;
                    } else if (MyInput.down && !MyInput.up && !ignoreUpDownController) {
                        if (!strafing) facing = Facing.DOWN;
                        newvel.y = -moveSpeed;
                    } else {
                        newvel.y = 0;
                    }

                    if (MyInput.controllerActive) {
                        if (MyInput.up  || MyInput.down) {
                            if (MyInput.right || MyInput.left) {
                                if (Math.Abs(MyInput.moveX) < 0.35f) {
                                    newvel.x = 0;
                                }
                            }
                        }
                    }


                #region HorrorFacingLogic
                if (isHorror) {

                    bool horUp = MyInput.up;
                    bool horDown = MyInput.down;
                    bool horLeft = MyInput.left;
                    bool horRight = MyInput.right;

                    if (MyInput.controllerActive) {
                        horUp = horDown = horLeft = horRight = false;
                        if (MyInput.moveX < -0.15f && Mathf.Abs(MyInput.moveX) >= Mathf.Abs(MyInput.moveY)) {
                            horLeft = true;
                        } else if (MyInput.moveX > 0.15f && Mathf.Abs(MyInput.moveX) >= Mathf.Abs(MyInput.moveY)) {
                            horRight = true;
                        } else if (MyInput.moveY > 0.15f && Mathf.Abs(MyInput.moveY) >= Mathf.Abs(MyInput.moveX)) {
                            horUp = true;
                        } else if (MyInput.moveY < -0.15f && Mathf.Abs(MyInput.moveY) >= Mathf.Abs(MyInput.moveX)) {
                            horDown = true;
                        }
                    }
                    newvel.Set(0, 0);
                    if (horUp) {
                        newvel.Set(moveSpeed, 0.5f * moveSpeed);

                        facing = Facing.UP;
                    } else if (horDown) {
                        newvel.Set(-moveSpeed, -0.5f * moveSpeed);

                        facing = Facing.DOWN;
                    } else if (horLeft) {
                        newvel.Set(-moveSpeed, 0.5f * moveSpeed);

                        facing = Facing.LEFT;
                    } else if (horRight) {
                        newvel.Set(moveSpeed, -0.5f * moveSpeed);

                        facing = Facing.RIGHT;
                    }
                }
                #endregion

                #region RaftLogic
                if (mapHasWater) {
                    tempWaterPos = transform.position;
                    tempWaterPos.y += bc.offset.y;
                    inWater = tmm.IsWater(tempWaterPos);
                    if (noclipon) inWater = false;
                    // inraftzone set by raft
                    if (inRaftZone || (!inRaftZone && !inWater)) {
                        inWater = false;
                        if (lastNonWaterPosList.Count == 0) {
                            for (int i = 0; i < 4; i++) lastNonWaterPosList.Add(transform.position);
                        }
                        if (HF.TimerDefault(ref t_waterAddPos, 0.1f)) {
                            for (int i = 0; i < 4 - 1; i++) lastNonWaterPosList[i] = lastNonWaterPosList[i + 1];
                            lastNonWaterPosList[4 - 1] = transform.position;
                        }
                        t_waterHurt = 0;
                    } else if (inWater) { 
                        if (HF.TimerDefault(ref t_waterHurt, 0.5f)) {
                            transform.position = lastNonWaterPosList[4 - 1];
                            AudioHelper.instance.playOneShot("blockexplode",1,1.1f,2);
                            //Flicker(0.5f);
                        }
                    }
                }
                // Raft movement
                if (!ridingRaft) {
                    tempRaftVel.Set(0, 0);
                    t_raftJustShotTimer = 0;
                } else if (ridingRaft) {
                    bool raftMovingHor = false;
                    bool raftMovingVert = false;

                    // Accel with suck
                    if (pickupMode == 0 && SuckAnimShouldPlay()) {
                        // When hitting a wall, set vel to zero.
                        if (Mathf.Abs(tempRaftVel.y) > 0.5f && Mathf.Abs(rb.velocity.y) < 0.25f) tempRaftVel.y = 0;
                        if (Mathf.Abs(tempRaftVel.x) > 0.5f && Mathf.Abs(rb.velocity.x) < 0.25f) tempRaftVel.x = 0;

                        if (facing == Facing.UP) tempRaftVel.y = Mathf.Min(maxRaftVel, tempRaftVel.y + raftAccel * Time.deltaTime);
                        if (facing == Facing.DOWN) tempRaftVel.y = Mathf.Max(-maxRaftVel, tempRaftVel.y - raftAccel * Time.deltaTime);
                        if (facing == Facing.RIGHT) tempRaftVel.x = Mathf.Min(maxRaftVel, tempRaftVel.x + raftAccel * Time.deltaTime);
                        if (facing == Facing.LEFT) tempRaftVel.x = Mathf.Max(-maxRaftVel, tempRaftVel.x - raftAccel * Time.deltaTime);

                        if (facing == Facing.UP || facing == Facing.DOWN) {
                            raftMovingVert = true;
                        } else {
                            raftMovingHor = true;
                        }
                    }

                    // Shootingn boosts you
                    if (t_raftJustShotTimer > 0) {
                        t_raftJustShotTimer -= Time.deltaTime;
                        if (facing == Facing.UP) tempRaftVel.y = -raftShootVel;
                        if (facing == Facing.DOWN) tempRaftVel.y = raftShootVel;
                        if (facing == Facing.RIGHT) tempRaftVel.x = -raftShootVel;
                        if (facing == Facing.LEFT) tempRaftVel.x = raftShootVel;
                    }
                    // Slow down in direction not being moved in
                    if (!raftMovingHor && tempRaftVel.x > 0) tempRaftVel.x = Mathf.Max(0, tempRaftVel.x - raftDrag * Time.deltaTime);
                    if (!raftMovingHor && tempRaftVel.x < 0) tempRaftVel.x = Mathf.Min(0, tempRaftVel.x + raftDrag * Time.deltaTime);
                    if (!raftMovingVert && tempRaftVel.y > 0) tempRaftVel.y = Mathf.Max(0, tempRaftVel.y - raftDrag * Time.deltaTime);
                    if (!raftMovingVert && tempRaftVel.y < 0) tempRaftVel.y = Mathf.Min(0, tempRaftVel.y + raftDrag * Time.deltaTime);

                    if (CameraIsChangingRooms()) {
                        if (tempRaftVel.x >= 0.5f) tempRaftVel.x = 0.5f;
                        if (tempRaftVel.x <= -0.5f) tempRaftVel.x = -0.5f;
                        if (tempRaftVel.y >= 0.5f) tempRaftVel.y = 0.5f;
                        if (tempRaftVel.y <= -0.5f) tempRaftVel.y = -0.5f;
                    }
                    activeRaftRB.velocity = tempRaftVel;

                }
                #endregion

                #region JumpDownAnim Logic
                if (jumpDownMode == 0) {
                    if (JumpDownZone.overlapping && MyInput.down) {
                        jumpDownShadow.transform.parent = null;
                        jumpDownShadow.GetComponent<SpriteRenderer>().enabled = true;
                        jumpDownShadow.transform.position = transform.position - new Vector3(0, 0.45f + JumpDownZone.globaldistance, 0);
                        jumpDownShadow.GetComponent<SpriteAnimator>().Play("small");

                        jumpDownMode = 1;
                        AudioHelper.instance.playSFX("player_jump_up");
                        jumpDownStartPos = transform.position;
                        bc.isTrigger = true;
                    }
                } else if (jumpDownMode == 1) {
                    newvel.Set(0, 0);
                    float maxTime = 0.23f;
                    jumpDownArcTime += Time.deltaTime; if (jumpDownArcTime > maxTime) jumpDownArcTime = maxTime;
                    float yOff = 0.5f * Mathf.Sin(Mathf.Deg2Rad*180f * (jumpDownArcTime / maxTime));
                    jumpDownStartPos.y += yOff;
                    transform.position = jumpDownStartPos;
                    jumpDownStartPos.y -= yOff;
                    if (jumpDownArcTime >= maxTime) {
                        jumpDownArcTime = 0;
                        jumpDownMode = 2;
                    }
                } else if (jumpDownMode == 2) {
                    newvel.x = 0;
                    newvel.y = -moveSpeed;

                    if (jumpDownStartPos.y - transform.position.y > JumpDownZone.globaldistance*0.35f) {
                        jumpDownShadow.GetComponent<SpriteAnimator>().Play("getbig");
                    }
                    if (jumpDownStartPos.y - transform.position.y > JumpDownZone.globaldistance) {
                        jumpDownShadow.GetComponent<SpriteRenderer>().enabled = false;
                        jumpDownMode = 0;
                        bc.isTrigger = false;
                        AudioHelper.instance.playSFX("player_jump_down");
                    }
                }
                #endregion

                TempSuckZonePos = SuckZone_sr.transform.localPosition;
                TempSuckZonePos.y = -1.6f;
                if (facing == Facing.UP) {
                    SuckZoneParent.eulerAngles = new Vector3(0, 0, 180);
                    TempSuckZonePos.x = -.3f;
                    TempSuckZonePos.y = -1.2f;
                    if (isInPicoScene) TempSuckZonePos.x = 0f;
                    if (isInPicoScene) TempSuckZonePos.y = -2.1f;
                } else if (facing == Facing.RIGHT) {
                    SuckZoneParent.eulerAngles = new Vector3(0, 0, 90);
                    TempSuckZonePos.x = -.2f;
                    if (isInPicoScene) TempSuckZonePos.x = -.1f;
                    if (isInPicoScene) TempSuckZonePos.y = -2f;
                } else if (facing == Facing.DOWN) {
                    TempSuckZonePos.x = -.1f;
                    if (isInPicoScene) TempSuckZonePos.x = 0f;
                    if (isInPicoScene) TempSuckZonePos.y = -2.1f;
                    SuckZoneParent.eulerAngles = new Vector3(0, 0, 0);
                } else if (facing == Facing.LEFT) {
                    SuckZoneParent.eulerAngles = new Vector3(0, 0, -90);
                    TempSuckZonePos.x = .2f;
                    if (isInPicoScene) TempSuckZonePos.y = -2f;
                    if (isInPicoScene) TempSuckZonePos.x = .1f;
                }
                SuckZone_sr.transform.localPosition = TempSuckZonePos;

            }


            if (MyInput.anyDir && (newvel.x != 0 || newvel.y != 0)) {
                // Stop movement from not switching frames right away
                if (animator.speed == 0) {
                    float nt = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                    nt = Mathf.Floor(nt * 2) / 2 + 0.5f; // In a 2 frame anim this skips to the next frame
                    animator.Play(0, 0, nt); // Normalized time is NRLOOPS.PERCENTTHRUCURRENTANIM !!  Also if statenamehash is 0 it applies Play tot he current state
                }
                animator.speed = 1;
            } else {
                animator.speed = 0;
            }

            if (jumpDownMode != 0) {
                animator.Play("jumpdown");
            } else if (wrestleDodgeMode == 1) {
                // Do nothing
            } else if (JumpDownZone.ladderOverlapping) {
                animator.Play("ladder");
                t_SuckZoneScale = 0;
                if ((MyInput.up || MyInput.down || MyInput.left || MyInput.right) && HF.TimerDefault(ref tLadder, tmLadder)) {
                    ladderSFXState = (ladderSFXState + 1) % 2;
                    if (ladderSFXState == 0) {
                        AudioHelper.instance.playSFX("ladder_step_1");
                    } else {
                        AudioHelper.instance.playSFX("ladder_step_2");
                    }
                }
            } else if (!isHorror && SuckAnimShouldPlay()) {
                HF.TimerStayAtMax(ref t_SuckZoneScale, 0.24f);
                tempFacing_suck = facing;
                playDirectionalAnim("suck");
            } else if (sparkAllowedForThisArea && MyInput.cancel) {
                playDirectionalAnim("suck");
            } else {
                t_SuckZoneScale -= Time.deltaTime * 3; if (t_SuckZoneScale < 0) t_SuckZoneScale = 0;
                if (tempFacing_suck != facing) t_SuckZoneScale = 0;
                if (PickedUpSomething()) {
                    playDirectionalAnim("inflated");
                } else {
                    playDirectionalAnim("walk");
                }
            }

            #region WrestleDodge Logic
            if (wrestleDodging) {
                if (wrestleDodgeMode == 0) {
                    if (MyInput.jpSpecial) {
                        wrestleDodgeMode = 1;
                        animator.speed = 1;
                        AudioHelper.instance.playOneShot("swing_broom_1");
                        if (wrestleCountering) {
                            animator.Play("spincounter");
                        } else {
                            if (wrestleAxe != null && wrestleAxe.position.x <= transform.position.x) {
                                animator.Play("wrestlehit_l");
                            } else {
                                animator.Play("wrestlehit_r");
                            }
                        }
                    }
                } else if (wrestleDodgeMode == 1) { // Hold the pose
                    if (wrestleCountering) animator.speed = 1;
                    newvel = Vector2.zero;
                    if (HF.TimerDefault(ref t_wrestleDodge,0.5f)) {
                        wrestleDodgeMode = 2;
                        if (wrestleCountering) wrestleDodgeMode = 0;
                    }
                } else if (wrestleDodgeMode == 2) { // cooldown
                    if (HF.TimerDefault(ref t_wrestleDodge, 0.5f)) {
                        wrestleDodgeMode = 0;
                    }
                }
            } else {
                wrestleDodgeMode = 0;
            }
            #endregion

            TempSuckZoneColor = SuckZone_sr.color;
            TempSuckZoneColor.a = t_SuckZoneScale / 0.24f;
            if (isInPicoScene && t_SuckZoneScale > 0) TempSuckZoneColor.a = 1;
            SuckZone_sr.color = TempSuckZoneColor;

            TempSuckZoneScale = SuckZone_sr.transform.localScale;
            if (isInPicoScene) {
                if (t_SuckZoneScale <= 0) {
                    TempSuckZoneScale.x = 0f;
                } else {
                    TempSuckZoneScale.x = 2f;
                }
                TempSuckZoneScale.y = 2f;
            } else {
                TempSuckZoneScale.x = 0.625f * t_SuckZoneScale / 0.24f;
            }
            SuckZone_sr.transform.localScale = TempSuckZoneScale;

            newvel.Normalize();


            if (isHorror) {
                newvel.x *= 0.5f;
                newvel.y *= 0.5f;
            }

            if (PickedUpSomething()) newvel *= fullMoveSpeed;
            if (!PickedUpSomething()) newvel *= moveSpeed;
            if (jumpDownMode != 0) newvel *= 1.7f;
            if (ridingRaft) {
                newvel = tempRaftVel;
                // Change player's velocity with arrow keys so can move around raft
                if (MyInput.up) newvel.y += moveSpeed;
                if (MyInput.down) newvel.y -= moveSpeed;
                if (MyInput.right) newvel.x += moveSpeed;
                if (MyInput.left) newvel.x -= moveSpeed;
            }
            if (noclipon) newvel *= 3f;
            if (poisoned || inWater) newvel *= 0.25f;
            rb.velocity = newvel;

            if (BumpVelocity.magnitude > 1f) {
                HF.ReduceVec2To0(ref BumpVelocity, Time.deltaTime * bumpveldecelfactor);
                rb.velocity = BumpVelocity;
            }

            updatePickup();
        }

    }

    public void setWaterRespawnPositions(Vector3 v) {
        for (int i = 0; i < lastNonWaterPosList.Count; i++) {
            lastNonWaterPosList[i] = v;
        }
    }
    void UpdateScreenShake() {
        if (t_screenShake > 0) {
            tempVecShake = cam.transform.position;
            tempVecShake -= screenShakeOffset;

            if (screenShakeStaysCentered) {
                cam.transform.position = screenShakeCenter;
                tempVecShake = cam.transform.position;
            }

            screenShakeOffset.x = UnityEngine.Random.value * screenShakeMagnitude;
            screenShakeOffset.y = UnityEngine.Random.value * screenShakeMagnitude;
            if (UnityEngine.Random.value > 0.5f) screenShakeOffset.x *= -1;
            if (UnityEngine.Random.value > 0.5f) screenShakeOffset.y *= -1;
            t_screenShake -= Time.deltaTime;
            if (t_screenShake > 0) {
                tempVecShake += screenShakeOffset;
            }
            cam.transform.position = tempVecShake;
        }
    }

    public void StopSuckingAnim() {
        TempSuckZoneScale = SuckZone_sr.transform.localScale;
        TempSuckZoneScale.x = 0;
        SuckZone_sr.transform.localScale = TempSuckZoneScale;
        playDirectionalAnim("walk");
        t_SuckZoneScale = 0;
        animator.speed = 1;
    }

    int MODE_ENTRY_BEAM = 30;
    float tmLadder = .18f;
    float tLadder = 0;
    float ladderSFXState = 0;

    float T_FlipFlicker = 0;
    float TimeToFlipFlicker = 0.056f;
    float TimeToRunFlicker = 0;
    int flickerMode = 0;
    Color poisonColor = new Color(.85f,.29f,.96f,1);
    private void UpdateGraphicEffects() {

        if (flickerMode == 0) {
            if (TimeToRunFlicker > 0) flickerMode = 1;
        } else if (flickerMode == 1) {
            T_FlipFlicker += Time.deltaTime;
            if (T_FlipFlicker > TimeToFlipFlicker) {
                sr.enabled = !sr.enabled;
                T_FlipFlicker -= TimeToFlipFlicker;
            }
            TimeToRunFlicker -= Time.deltaTime;
            if (TimeToRunFlicker < 0 && sr.enabled) {
                flickerMode = 0;
                T_FlipFlicker = 0;
            }
       }

        if (poisoned) {
            tPoisonHurt += Time.deltaTime;
            float f = Mathf.Sin(tPoisonHurt * 6.28f / 2.5f) + 1;
            f /= 2;
            poisonColor.g = 0.29f + f * 0.65f;
            sr.color = poisonColor;
            if (tPoisonHurt > 2.5f) {
                tPoisonHurt = 0;
                if (SaveManager.currentHealth > 1) {
                    Damage(1);
                    Bump(true, 5f);
                }

            }
         }
    }
    public void Flicker(float time) {
        if (time < TimeToRunFlicker) return;
        TimeToRunFlicker = time;
    }
    public bool CameraIsChangingRooms() {
        return camMode == 1;
    }

    bool CinemachineOnButAllowingMovement = false;
    int cmMode = 0;
    string currentVCName = "";
    float t_ExitFixedCam = 0;
    Transform matcherVCTrans;
    Vector3 tempV = new Vector3();

    void SetVCPriority(string name,int priority) {
        GameObject.Find(name).GetComponent<Cinemachine.CinemachineVirtualCamera>().Priority = priority;
    }

    //Called from cutscene things when player needs to move while camera is fixed somewhere (bridge game)
    public void EnterFixedCamMode(string camToLerpTo) {
        cmMode = 1;
        CinemachineOnButAllowingMovement = true;
        currentVCName = camToLerpTo;
        SetVCPriority("matcherVC", 1000);
    }
    public void StartToExitFixedCamMode() {
        matcherVCTrans = GameObject.Find("matcherVC").transform;
        SetVCPriority(currentVCName, 0);
        SetVCPriority("matcherVC", 1000);
        cmMode = -1;
    }
    public bool IsCinemachineOnButAllowingMovement() {
        return CinemachineOnButAllowingMovement;
    }
    public bool IsExitingCinemachineMovementMode() {
        return cmMode == -1;
    }

    void TearFixCameraPos(Transform camTransform) {
        newCamPos = camTransform.position;
        newCamPos.x = Mathf.Round(newCamPos.x * 16) / 16; // Tearing fix
        newCamPos.y = Mathf.Round(newCamPos.y * 16) / 16;
        camTransform.position = newCamPos;
    }

    // ALso implemented in CinemachineBrain for when cinemachine's on
    private void LateUpdate() {
        tempV = cam.transform.position;
        tempV.z = -1;
        cam.transform.position = tempV;
        TearFixCameraPos(cam.transform);
    }

    Vector3 newCamPos = new Vector3();
    private void UpdateCameraLogic() {

        if (CinemachineOnButAllowingMovement) {
            if (cmMode == 0) {
                // Off.
            // Turn on the cinemachine brain, set the blend time, wait a tick so things propagate
            } else if (cmMode == 1) {
                Camera.main.GetComponent<Cinemachine.CinemachineBrain>().enabled = true;
                Camera.main.GetComponent<Cinemachine.CinemachineBrain>().m_DefaultBlend.m_Time = 1;
                cmMode = 2;
            } else if (cmMode == 2) {
                SetVCPriority(currentVCName, 1000);
                SetVCPriority("matcherVC", 0);
                cmMode = 3;
            } else if (cmMode == 3) {
                // Fixed in place. Let player move.

            // Move the matcher cam to player (since its MatchCM script isn't running right now)
            } else if (cmMode == -1 ) {
                tempV = transform.position;
                tempV.z = -1;
                matcherVCTrans.position = tempV;
                // Move towards matcher cam.
                t_ExitFixedCam += Time.deltaTime;
                if (t_ExitFixedCam > 1) {
                    t_ExitFixedCam = 0;
                    Camera.main.GetComponent<Cinemachine.CinemachineBrain>().enabled = false;
                    CinemachineOnButAllowingMovement = false;
                    cmMode = 0;
                }
            }



            return;
        }
        // Camera logic
        if (mode != 0) {
            float _x = transform.position.x;
            float _y = transform.position.y;
            if (camMode == 0) {
                // Detect need to scroll camera
                if (_x <= currentRoom.x * roomsize) { camScrollDir = Facing.LEFT; camMode = 1; }
                if (_x >= (currentRoom.x + 1) * (roomsize)) { camScrollDir = Facing.RIGHT; camMode = 1; }

                if (_y <= currentRoom.y * roomsize) { camScrollDir = Facing.DOWN; camMode = 1; }
                if (_y >= (currentRoom.y + 1) * (roomsize)) { camScrollDir = Facing.UP; camMode = 1; }


                SnapCameraToPlayerAndKeepInRoom();
                
                if (camMode == 1) {
                    rb.velocity = Vector2.zero;
                }
                // Scroll the camera
            } else if (camMode == 1) {
                Vector3 newCamPos = cam.transform.position;
                float nextEdge = 0;
                if (noclipon) camScrollSpeed *= 64;
                if (camScrollDir == Facing.LEFT) {
                    newCamPos.x -= camScrollSpeed * Time.deltaTime;
                    nextEdge = (currentRoom.x) * roomsize - cam.orthographicSize;
                    if (newCamPos.x <= nextEdge) { newCamPos.x = nextEdge;  currentRoom.x -= 1; camMode = 0; }
                } else if (camScrollDir == Facing.RIGHT) {
                    newCamPos.x += camScrollSpeed * Time.deltaTime;
                    nextEdge = (currentRoom.x + 1) * roomsize + cam.orthographicSize;
                    if (newCamPos.x >= nextEdge) { newCamPos.x = nextEdge;  currentRoom.x += 1; camMode = 0; }
                } else if (camScrollDir == Facing.UP) {
                    newCamPos.y += camScrollSpeed * Time.deltaTime;
                    nextEdge = (currentRoom.y + 1) * roomsize + cam.orthographicSize;
                    if (newCamPos.y >= nextEdge) { newCamPos.y = nextEdge;  currentRoom.y += 1; camMode = 0; }
                } else if (camScrollDir == Facing.DOWN) {
                    newCamPos.y -= camScrollSpeed * Time.deltaTime;
                    nextEdge = (currentRoom.y) * roomsize - cam.orthographicSize;
                    if (newCamPos.y <= nextEdge) { newCamPos.y = nextEdge;  currentRoom.y -= 1; camMode = 0; }
                }
                cam.transform.position = newCamPos;
                if (noclipon) camScrollSpeed /= 64;

                if (camMode == 0) {
                    ui.RefreshGameMinimap(transform.position.x, transform.position.y);
                }
            }
        }
    }

    public void ForceUpdateRoomPos() {
        currentRoom.x = Mathf.FloorToInt(transform.position.x / roomsize);
        currentRoom.y = Mathf.FloorToInt(transform.position.y / roomsize);
    }

    public void SnapCameraToPlayerAndKeepInRoom() {
        // Keep camera in bounds
        newCamPos = transform.position; newCamPos.z = cam.transform.position.z;
        newCamPos.y = Mathf.Clamp(newCamPos.y, currentRoom.y * roomsize + cam.orthographicSize, (currentRoom.y + 1) * roomsize - cam.orthographicSize);
        newCamPos.x = Mathf.Clamp(newCamPos.x, currentRoom.x * roomsize + cam.orthographicSize, (currentRoom.x + 1) * roomsize - cam.orthographicSize);
        //newCamPos.x = Mathf.Round(newCamPos.x*16)/16; // Tearing fix
        //newCamPos.y = Mathf.Round(newCamPos.y*16)/16;
        cam.transform.position = newCamPos;
    }

    int entryBeamMode = 0;
    float t_entryBeam = 0;
    int otherSceneInfo = 0;
    private void UpdateEntryBeamInLogic() {
        if (entryBeamMode == 0) {
            sr.enabled = false;
            if (HF.TimerDefault(ref t_entryBeam,0.6f)) {
                jumpDownShadow.GetComponent<SpriteRenderer>().enabled = true;
                AudioHelper.instance.playOneShot("BeamIn");
                GameObject.Find("FakePlayerSprite").GetComponent<SpriteRenderer>().enabled = true;
                beamInDirector = GameObject.Find("AreaTitle_BG").GetComponent<UnityEngine.Playables.PlayableDirector>();
                GameObject.Find("BeamSceneStuff").GetComponent<UnityEngine.Playables.PlayableDirector>().Play();
                AreaTitle_Center.alpha = 1;
                beamInDirector.Play();

                entryBeamMode = 1;
            }
        } else if (entryBeamMode == 1) {

            if (beamInDirector.time > 5.11f && AudioHelper.pausePlayingOfNewTracks) {
                AudioHelper.pausePlayingOfNewTracks = false;
            }
            if (beamInDirector.time > 6f) {
                beamInDirector.Stop();
                GameObject.Find("BeamSceneStuff").GetComponent<UnityEngine.Playables.PlayableDirector>().Stop();
                entryBeamMode = 4;
                jumpDownShadow.GetComponent<SpriteRenderer>().color = new Color(0, 0, 0, 1);
                jumpDownShadow.GetComponent<SpriteRenderer>().enabled = false;
                GameObject.Find("FakePlayerSprite").GetComponent<SpriteRenderer>().enabled = false;
                sr.enabled = true;
            }
        } else if (entryBeamMode == 4) {
            SetAreaTitleBottom(AreaTitle_Center.text.ToUpper());
            AreaTitle_Center.text = "";
            mode = 1;
        }
    }
    UnityEngine.Playables.PlayableDirector beamInDirector;
    TilemetaManager tmm;
    //float spikebumpvel = 10f;
    float bumpveldecelfactor = 45f;
    float defaultbumpvel = 10f;
    private void UpdateTileLogic() {
        if (tmm == null) {
            tmm = TilemetaManager.instance;
        }
    //    if (T_SpikeCooldown <= 0) {
            /*
            if (tmm.IsSpike(transform.position)) {
                Damage(1);
                Flicker(0.3f);
                SetBumpedBackwardsVel(spikebumpvel);
                T_SpikeCooldown = SpikeCooldown;
            }
            */
      //  } else {
       //     T_SpikeCooldown -= Time.deltaTime;
       // }
    }

    public void Damage(int amount=1) {
        if (isPaused) return;
        if (amount == 0) return;
        if (T_DamageCooldown <= 0) {
            AudioHelper.instance.playSFX("playerHurt");
            T_DamageCooldown = DamageCooldown;
            Flicker(DamageCooldown * 0.8f);
            if (!SaveManager.invincibility) {
                healthbar.Damage(amount);
            }
        }
    }

    int pickupMode = 0;
    void updatePickup() {
        PickupRegionTVec = bc.transform.position;
        if (facing == Facing.UP) {
            PickupRegionTVec.y += bc.radius;// + PickupRegion.GetComponent<BoxCollider2D>().bounds.size.y;
        } else if (facing == Facing.RIGHT) {
            PickupRegionTVec.x += bc.radius;// + PickupRegion.GetComponent<BoxCollider2D>().bounds.size.x;
        } else if (facing == Facing.DOWN) {
            PickupRegionTVec.y -= bc.radius;// + PickupRegion.GetComponent<BoxCollider2D>().bounds.size.y;
        } else if (facing == Facing.LEFT) {
            PickupRegionTVec.x -= bc.radius;// + PickupRegion.GetComponent<BoxCollider2D>().bounds.size.x;
        }
        PickupRegion.position = PickupRegionTVec;
        // In this mode, pickupable objects are able to be picked up
        if (pickupMode == 0) {
            if (NeedToReleaseSuckKeyToStartSuckingAgain && !MyInput.special) NeedToReleaseSuckKeyToStartSuckingAgain = false;

            // press button to release
        } else if (pickupMode == 1) {
            if (MyInput.jpSpecial && !JumpDownZone.ladderOverlapping) {
                pickupMode = 2;
                t_raftJustShotTimer = 0.25f;
            }
            // Has just shot, waiting for shot object to say it has started moving
        } else if (pickupMode == 2) {

        // Moving to player, either after waiting for rooted object or a normal object got within definite-suck radius
        } else if (pickupMode == 3) {
            freezeSafetyTimer -= Time.deltaTime;
            if (freezeSafetyTimer < 0) {
                cancelUnrootWait();
            }
        }

    }

    [System.NonSerialized]
    public bool NeedToReleaseSuckKeyToStartSuckingAgain = false;
    public bool PickedUpSomething() {
        return (pickupMode == 1 || pickupMode == 2);
    }
    public bool isAbleToPickUp() {
        if (NeedToReleaseSuckKeyToStartSuckingAgain) return false;
        return pickupMode == 0 && pickupMode != 3;
    }

    public bool SuckAnimShouldPlay() {
        if (wrestleDodging) return false;
        return !NeedToReleaseSuckKeyToStartSuckingAgain && ((pickupMode == 3) || (pickupMode == 0 && MyInput.special));
    }

    // Call if some pickupable object can't be released due to colliders
    public void cancelRelease() {
        if (pickupMode == 2) pickupMode = 1;
    }
    // Called from a pickable object when that object has been moved
    public void confirmRelease() {
        if (pickupMode == 2) {
            NeedToReleaseSuckKeyToStartSuckingAgain = true;
            pickupMode = 0;
            AudioHelper.instance.playOneShot("vacuumShoot");
        }

    }

    
    public string GetNameOfObjectInFront() {
        RaycastHit2D rh = new RaycastHit2D();
        rh = Physics2D.Raycast(transform.position, getFacingDirVector(),20,~(1 << gameObject.layer));
        if (rh.collider != null) return rh.collider.name;
        return "";
    }
    public float GetWorldspaceRadius() {
        return bc.radius * transform.localScale.x;
    }

    public bool hasShot() {
        return false;
    }
    public bool hasJustShot() {
        return pickupMode == 2;
    }

    float freezeSafetyTimer = 0.75f;
    public void enterUnrootWaitForObjectMode() {
        freezeSafetyTimer = 0.75f;
        MovementIsEnabled = false;
        pickupMode = 3;
    }
    public void cancelUnrootWait() {
        MovementIsEnabled = true;
        pickupMode = 0;
    }
    public void enterPickedupMode() {
        if (pickupMode == 3) {
            MovementIsEnabled = true;
        }
        pickupMode = 1;
        AudioHelper.instance.playOneShot("vacuumSucked");
    }

    public Vector2 getFacingDirVector() {
        Vector2 v = new Vector2(0,0);
        if (facing == Facing.RIGHT) v.x = 1;
        if (facing == Facing.LEFT) v.x = -1;
        if (facing == Facing.DOWN) v.y = -1;
        if (facing == Facing.UP) v.y = 1;
        return v;
    }
    public Vector3 getCenterOfPickupRegion() {
        return PickupRegion.position;
    }
 
    void playDirectionalAnim(string prefix) {

        string wrestlePrefix = "";
        if (wrestleOn) {
            wrestlePrefix = "_W";
        }
        if (isInPicoScene) {
            if (prefix == "inflated") prefix = "suck";
            if (prefix == "suck" || prefix == "walk") wrestlePrefix = "_P";
        }
       // print(animator.GetCurrentAnimatorStateInfo(0).normalizedTime % animator.GetCurrentAnimatorClipInfo(0)[0].clip.length);
        if (facing == Facing.UP) {
            animator.Play(prefix + "_u"+wrestlePrefix);
        } else if (facing == Facing.RIGHT) {
            animator.Play(prefix + "_r" + wrestlePrefix);
        } else if (facing == Facing.DOWN) {
            animator.Play(prefix + "_d" + wrestlePrefix);
        } else if (facing == Facing.LEFT) {
            animator.Play(prefix + "_l" + wrestlePrefix);
        }

    }

    void SetBumpedBackwardsVel(float vel) {
        SetBumpedVel(-vel);
    }
    void SetBumpedVel(float vel) { 
        if (facing == Facing.UP) {
            BumpVelocity.Set(0, vel);
        } else if (facing == Facing.RIGHT) {
            BumpVelocity.Set(vel,0);
        } else if (facing == Facing.DOWN) {
            BumpVelocity.Set(0, -vel);
        } else if (facing == Facing.LEFT) {
            BumpVelocity.Set(-vel, 0);
        }
    }
    public void BumpInDir(float x, float y) {
        BumpVelocity.Set(x, y);
    }
    public void Bump(bool backwards, float vel=0) {

        float flip = 1;
        if (backwards) flip = -1;
        if (vel == 0) {
            SetBumpedVel(defaultbumpvel*flip);
        } else {
            SetBumpedVel(vel*flip);
        }
    }

    public void Swallow() {
        if (pickupMode == 1) {
            ChangeSuckedItemSprite(null);
            pickupMode = 0;
        }
    }


    Color SuckedItemOriginalColor;
    UIManager2D ui;

    [HideInInspector]
    bool MovementIsEnabled = true;
    internal bool ridingRaft;

    float maxRaftVel = 6f;
    float raftAccel = 7.75f;
    float raftShootVel = 4.5f;
    float raftDrag = 4.82f;
    internal bool inRaftZone;

    Vector2 tempRaftVel = new Vector2();
    private float t_waterHurt;
    private float t_waterAddPos;
    List<Vector3> lastNonWaterPosList = new List<Vector3>();
    private float t_raftJustShotTimer;
    [System.NonSerialized]
    public Rigidbody2D activeRaftRB;

    public void ChangeSuckedItemSprite(SpriteRenderer sr) {
        Image i = GameObject.Find("SuckedItem").GetComponent<Image>();
        if (sr == null) {
            i.overrideSprite = null;
            i.color = SuckedItemOriginalColor;
        } else {
            ui.ext_ItemWasSucked = true;
            float w = 2 * sr.sprite.bounds.extents.x * sr.sprite.pixelsPerUnit;
            float h = 2 * sr.sprite.bounds.extents.y * sr.sprite.pixelsPerUnit;
            i.rectTransform.sizeDelta = new Vector2(w, h);
            i.overrideSprite = sr.sprite;
            SuckedItemOriginalColor = i.color;
            i.color = new Color(sr.color.r, sr.color.g, sr.color.b, sr.color.a);
        }
    }


    public bool InSameRoomAs(Vector2 pos) {
        return currentRoom.x == HF.GetRoomX(pos.x) && currentRoom.y == HF.GetRoomY(pos.y);
    }
    public bool InThisRoom(Vector2Int roompos) {
        return currentRoom.x == roompos.x && currentRoom.y == roompos.y;
    }

    public bool IsDying() {
        return MODE_DYING == mode;
    }
}
