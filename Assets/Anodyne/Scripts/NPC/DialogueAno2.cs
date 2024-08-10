using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using Cinemachine;
public class DialogueAno2 : MonoBehaviour {

    DialogueBox box;

    [TextArea(6, 30)]
    public string RunOnStart;

    [TextArea(20, 100)]
    public string RunOnInteract;
    UIManagerAno2 ui;
    UIManager2D ui2d;
    AudioHelper audioHelper;
    CinemachineVirtualCamera ActiveVirtualCamera;
    float timeBeforeInteractionPromptsCanAppear = 0.2f;

    float timeOutAfterTalk = 0.5f;

    MediumControl walkscalePlayer;
    string playerDestAfterTrigger = "";
    string lookTargetAfterTrigger = "";
    bool blackoutAfterTrigger = false;
    float t_aftertrigger = 0;
    string altInteractIconObjectName = "";

    GameObject cached_GO_ref1;
    GameObject cached_GO_ref2;

    void Start() {
        box = GameObject.Find("Dialogue").GetComponent<DialogueBox>();
        audioHelper = GameObject.Find("AudioHelper").GetComponent<AudioHelper>();

        if (GameObject.Find("3D UI") != null) {
            ui = GameObject.Find("3D UI").GetComponent<UIManagerAno2>();

        }
        if (GameObject.Find("2D UI") != null) {
            ui2d = GameObject.Find("2D UI").GetComponent<UIManager2D>();
        }
        isParsingStart = true;
        UpdateParse();
        UpdateParse();
        OnInteractMode = 0;
        isParsingStart = false;
        HF.GetPlayer(ref walkscalePlayer);

    }

    public bool StartsImmediately = false;
    public bool MustBeInWalkscale = false;
    
    Vector3 tempAngle = new Vector3();
    public Transform turnToPlayerTransform;
    int turnToPlayerMode = 0;
    float t_TurnToPlayer = 0;
    float turnToPlayerStartAngle = 0;

    public bool debugScript = false;

    bool isWaitingForOverlap = false;
    bool isWaitingForFadeToFinish = false;
    bool isParsingStart = false;
    bool isParsingInteract = false;
    bool pauseForSay;
    bool pauseForChoice;
    bool pauseForCutscene;
    bool pauseForTimeline;
    PlayableDirector director;
    float directorEndTime;
    bool pauseForNamecard;
    NameCards activeNameCard;
    CutsceneManager cutscene;
    int OnInteractMode = 0;
    string[] lines;
    int InteractParseCurLine = 0;
    bool skipToEndIf = false;
    bool skipToNextConditional = false;
    bool insideIf = false;
    bool turnedOff = false; // True if stop is used in start. means that interact won't run

    int getFlag(string key) {
        return DataLoader.instance.getDS(key);
    }
    void setFlag(string key, int value) {
        DataLoader.instance.setDS(key, value);
    }

    [System.NonSerialized]
    public bool ext_ForceInteractScriptToParse = false;
    [System.NonSerialized]
    public bool ext_DoesNotReactToOverlap = false;
    public static bool AnyScriptIsParsing = false;
    [System.NonSerialized]
    public bool doesNotShowBubble = false;
    int updateMode = 0;
    bool doesntPauseAnything = false;

    // via trigger condition SCENE = 1
    bool hasEqualityTriggerCondition1 = false;
    bool hasInequalityTriggerCondition1 = false;
    string EqCond1Name = "";
    int EqCond1Val = 1;
    string IneqCond1Name = "";
    int IneqCond1Val = 0;

    public bool TriggerCanStartInAir = false;

    int UPDATE_MODE_WAIT_TO_BECOME_WALK = 58921;


    MediumControl ridescalePlayer;
    MedBigCam mbCam;
    void Update() {
        if (timeBeforeInteractionPromptsCanAppear > 0) timeBeforeInteractionPromptsCanAppear -= Time.deltaTime;

        if (updateMode == UPDATE_MODE_WAIT_TO_BECOME_WALK) {
            if (!mbCam.waitingForSizeSwitchConfirmation && ridescalePlayer.DoneSwitchingSize()) {
                updateMode = 0;
                 insideTrigger = true;
                // don't do else if here so that insideTrigger can be reset once walkscale player appears
            } else {
                return;
            }
        }

        if (isSuckable && !vac.isIdle()) {
            if (insideTrigger) {
                insideTrigger = false;
                turnOffTalkAvailableIcon();
            }
            return;
        }

        if (updateMode == 0) {
            // If dialogue is playing, add a timer so you can't interact with this
            // Prevents case of autodialogue and overlappign something righ taway
            if (!box.isDialogFinished()) {
                if (timeOutAfterTalk == 0) {
                    timeOutAfterTalk = 0.2f;
                }
                if (!isParsingInteract && !isParsingStart) {
                    return;
                }
            } else if (box.isDialogFinished()) {
                timeOutAfterTalk -= Time.deltaTime; if (timeOutAfterTalk < 0) timeOutAfterTalk = 0;
                if (!isParsingInteract && timeOutAfterTalk > 0) {
                    return;
                }
            }

            if (DataLoader.instance.isChangingScenes) {
            } else if (ext_ForceInteractScriptToParse || StartsImmediately || (!turnedOff && insideTrigger && !isParsingInteract)) {


                if (insideTrigger && MyInput.jpToggleRidescale) {
                    insideTrigger = false;
                    turnOffTalkAvailableIcon();
                    return;
                }


                if (insideTrigger && isWaitingForOverlap && !doesntPauseAnything) {
                    if (GameObject.Find("BigPlayer") != null) {
                        updateMode = UPDATE_MODE_WAIT_TO_BECOME_WALK;
                        print("Force walkscale "+name);
                        ridescalePlayer = GameObject.Find("BigPlayer").GetComponent<MediumControl>();
                        mbCam = Camera.main.GetComponent<MedBigCam>();
                        mbCam.EXT_FORCE_WALKSCALE = true;
                        return;
                    }
                }


                // So when talking to NPC, and done talking, box appears again.
                if (insideTrigger && !doesNotShowBubble && !isWaitingForOverlap && !isParsingInteract) {
                    turnOnTalkAvailableIcon();
                }

                if (ext_ForceInteractScriptToParse || MyInput.jpTalk || isWaitingForOverlap || StartsImmediately) {

                    if (hasEqualityTriggerCondition1) {
                        print("EQ check");
                        if (DataLoader.instance.getDS(EqCond1Name) == EqCond1Val) {
                            print("Passed");
                        } else {
                            print("Failed");
                            insideTrigger = false;
                            return;
                        }
                    }
                    if (hasInequalityTriggerCondition1) {
                        if (DataLoader.instance.getDS(IneqCond1Name) != IneqCond1Val) {

                        } else {
                            insideTrigger = false;
                            return;
                        }
                    }

                    if (!doesntPauseAnything && !TriggerCanStartInAir && insideTrigger && MyInput.jpTalk && walkscalePlayer != null) {
                        if (!walkscalePlayer.GetComponent<MediumControl>().isTouchingGround(0.1f)) {
                            return;
                        }
                    }
                    if (MyInput.jpTalk && walkscalePlayer != null && walkscalePlayer.isThereAnyReasonToPause()) {
                        MyInput.jpTalk = false;
                        return;
                    }
                    if (MyInput.jpTalk && GameObject.Find(Registry.PLAYERNAME2D)  != null && GameObject.Find(Registry.PLAYERNAME2D).GetComponent<AnoControl2D>().IsThereAReasonToPause()) {
                        MyInput.jpTalk = false;
                        return;
                    }


                    isWaitingForOverlap = false;
                    if (!StartsImmediately) turnOffTalkAvailableIcon();
                    isParsingInteract = true;
                    if (!doesntPauseAnything) AnyScriptIsParsing = true;
                    ext_ForceInteractScriptToParse = false;
                    updateMode = 1;
                    if (StartsImmediately || doesntPauseAnything) {
                        updateMode = 0; // skip vel check
                        StartsImmediately = false;
                    } else if (playerDestAfterTrigger != "" || (TriggerCanStartInAir && playerDestAfterTrigger != "")) {
                        updateMode = 2;
                        if (blackoutAfterTrigger) {
                            ui.CancelSceneEntryFade();
                            GameObject.Find("UI_FadeImage").GetComponent<Image>().color = new Color(0, 0, 0, 1);
                        }
                    } else {
                        updateMode = 1;
                    }
                }
            }
        } else if (updateMode == 1) {
            if (HF.PlayerHasZeroVelocity() || TriggerCanStartInAir) updateMode = 0;
            return;
        } else if (updateMode == 2) {
            if (blackoutAfterTrigger) {
                blackoutAfterTrigger = false;
            } else {
                ui.StartFade(0.5f, true);
            }
            updateMode = 3;
            return;
        } else if (updateMode == 3) {
            if (HF.TimerDefault(ref t_aftertrigger,0.5f)) {

                walkscalePlayer.transform.position = GameObject.Find(playerDestAfterTrigger).transform.position;

                Vector3 oldEuler = walkscalePlayer.transform.localEulerAngles;
                walkscalePlayer.transform.LookAt(GameObject.Find(lookTargetAfterTrigger).transform.position);
                oldEuler.y = walkscalePlayer.transform.eulerAngles.y;
                walkscalePlayer.transform.eulerAngles = oldEuler;
                playerDestAfterTrigger = lookTargetAfterTrigger = "";

                walkscalePlayer.GetComponent<Rigidbody>().velocity = Vector3.zero;

                updateMode = 1;
            }
            return;
        }
        if (t_drumbirdReset > 0) {
            if (pauseForSay) {
                t_drumbirdReset = 0;
            } else {
                t_drumbirdReset -= Time.deltaTime;
                if (t_drumbirdReset < 0) {
                    GetComponent<AudioSource>().Play();
                    GameObject.Find(drumbirdName).GetComponent<Animator>().CrossFadeInFixedTime(drumbirdAnim, 0.5f);
                }
            }
        }

        if (isParsingInteract) {
            UpdateParse();
        }
    }

    string drumbirdName = "";
    string drumbirdAnim = "";
    float t_drumbirdReset = 0;
    // Called from DialogueBox when some piece of dialogue is done
    public void unpauseSay() {
        pauseForSay = false;
    }
    public void unpauseChoice() {
        pauseForChoice = false;
    }

    bool pauseForPrismTut = false;
    bool pauseForWait = false;
    float waitTime = 0;
    float t_wait = 0;
    float rotateToPlayerOffset = 0;
    void UpdateParse() {
        if (OnInteractMode == 0) {
            lines = RunOnInteract.Split(new char[] { '\n' });
            if (isParsingStart) lines = RunOnStart.Split(new char[] { '\n' });
            OnInteractMode = 1;
            InteractParseCurLine = -1;
        } else if (OnInteractMode == 1) {

            if (pauseForSay && box.nameOfCameraToCutTo != "") {
                CmCutTo(box.nameOfCameraToCutTo);
                box.nameOfCameraToCutTo = "";
            }

            if (pauseForSay && box.nameOfCameraToSmoothTo != "") {
                CmCutTo(box.nameOfCameraToSmoothTo,true,box.cameraLerpTime);
                box.nameOfCameraToSmoothTo = "";
                box.cameraLerpTime = 1;
            }

            if (turnToPlayerMode == 1) {
                t_TurnToPlayer = 0;
                // get initial y rot
                turnToPlayerStartAngle = turnToPlayerTransform.localEulerAngles.y;
                turnToPlayerMode = 2;
            } else if (turnToPlayerMode == 2) {

                // Find target y look
                tempAngle = turnToPlayerTransform.localEulerAngles;
                turnToPlayerTransform.LookAt(walkscalePlayer.transform);
                float endEulerY = turnToPlayerTransform.localEulerAngles.y + rotateToPlayerOffset;
                turnToPlayerTransform.localEulerAngles = tempAngle;

                // rotate  towards player
                tempAngle = turnToPlayerTransform.localEulerAngles;
                t_TurnToPlayer += Time.deltaTime;
                tempAngle.y = Mathf.LerpAngle(turnToPlayerStartAngle, endEulerY, Mathf.SmoothStep(0, 1, t_TurnToPlayer / 0.35f));
                turnToPlayerTransform.localEulerAngles = tempAngle;
                if (t_TurnToPlayer > 0.35f) {
                    turnToPlayerMode = 0;
                }
                
            }

            if (isWaitingForYesNo) {
                int retval = yesno.Update();
                // no
                if (retval == 0) {
                    if (yesnoDoesStopOnNo) {
                        // Stop
                        OnInteractMode = 2;
                    }
                    lastYN_was_yes = false;
                // yes
                } else if (retval == 1) {
                    if (yesnoDoesStopOnNo) {
                    }
                    lastYN_was_yes = true;
                }
                if (retval == 0 || retval == 1) {
                    isWaitingForYesNo = false;
                    yesnoDoesStopOnNo = false;
                }
                return;
            }

            if (pauseForPrismTut) {
                if (DataLoader.instance.IsPauseTutorialDone()) {
                    pauseForPrismTut = false;
                }
                return;
            }
            if (pauseForNamecard) {
                if (activeNameCard.IsDone()) {
                    pauseForNamecard = false;
                }
                return;
            }
            if (pauseForWait) {
                t_wait += Time.deltaTime;
                if (t_wait > waitTime) {
                    t_wait = 0;
                    pauseForWait = false;
                }
                return;
            }
            if (pauseForTimeline) {
                if (director.time >= directorEndTime) {
                    pauseForTimeline = false;
                    director.Stop();

                }
                return;
            }
            if (pauseForSay) {
                if (box.isDialogFinished()) {
                    pauseForSay = false;
                }
                return;
            }
            if (isWaitingForCredits) {
                if (creditsEnd.IsFinished()) {
                    isWaitingForCredits = false;
                }
                return;
            }
            if (pauseForChoice) {
                return;
            }
            if (pauseForNanoUnlock) {
                // ?? Play particle effect and SFX on current NPC while dialogue box gone
                // play "unlocked dialogue"
                if (box.isDialogFinished()) {
                    pauseForNanoUnlock = false;
                }
                return;
            }
            if (pauseForCutscene) {
                if (cutscene.isCutsceneFinished()) {
                    pauseForCutscene = false;
                }
                return;
            }
            if (pauseforWaitForInput) {
                if (MyInput.jpConfirm || MyInput.jpCancel || MyInput.jpSpecial) {
                    pauseforWaitForInput = false;
                }
                return;
            }

            if (isWaitingForFadeToFinish) {
                if (ui2d != null) {
                    if (ui2d.FadeFinished(true)) {
                        isWaitingForFadeToFinish = false;
                    }
                } else if (ui != null) {
                    if (ui.isFading() == false) {
                        isWaitingForFadeToFinish = false;
                    }
                }
                return;
            }

            while (InteractParseCurLine < lines.Length - 1) {
                InteractParseCurLine++;
                string line = lines[InteractParseCurLine].Trim();

                if (line.IndexOf('\r') != -1) {
                    print("WARNING: \\r in command: " + line + "on " + name);
                    line = line.Replace("\r", "");
                    line = line.Replace("  ", " ");
                }
                if (line.Length < 2) continue;
                string[] parts = line.Split(new char[] { ' ' });
                string command = parts[0];
                if (command[0] == '/') continue;

                // Hit end of the if or elif block, skip to endif command
                if (insideIf && (command == "elif" || command == "else")) {
                    insideIf = false;
                    skipToEndIf = true;
                }

                if (skipToEndIf) {
                    if (command != "endif") {
                        continue;
                    }
                }

                // if sceneName is Value...
                if (command == "if" || command == "elif" || command == "andif" || command == "ifyes") {

                    if (debugScript) {
                        print(line+"-"+name);
                    }
                    if (command == "ifyes") {
                        if (!lastYN_was_yes) {
                            skipToNextConditional = true;
                            insideIf = false;
                            continue;
                        } else {
                            insideIf = true;
                            continue;
                        }
                    }

                    // e.g. if ... andif ..., if the first if is false, fall through to next conditional
                    if (command == "andif" && skipToNextConditional) {
                        continue;
                    }
                    skipToNextConditional = false;
                    string scene = parts[1];
                    int checkVal = safeIntParse(parts[3]);
                    if (parts[2] == ">=") {

                        if (scene == "CAPACITY") {
                            if (Ano2Stats.prismCapacity < checkVal) {
                                skipToNextConditional = true;
                                insideIf = false;
                                continue;
                            }
                        } else if (scene == "PRISMDUST") {
                            if (Ano2Stats.prismCurrentDust < checkVal) {
                                skipToNextConditional = true;
                                insideIf = false;
                                continue;
                            }
                        } else if (scene == "TOTALCOIN") {
                            if (SaveManager.totalFoundCoins < checkVal) {
                                skipToNextConditional = true;
                                insideIf = false;
                                continue;
                            }
                        } else if (!(getFlag(scene) >= checkVal)) {
                            skipToNextConditional = true;
                            insideIf = false;
                            continue;
                        }
                    } else if (parts[2] == "<") {
                        if (scene == "PRISMDUST") {
                            if (Ano2Stats.prismCurrentDust >= checkVal) {
                                skipToNextConditional = true;
                                insideIf = false;
                                continue;
                            }
                        } else {
                            if (!(getFlag(scene) < checkVal)) {
                                skipToNextConditional = true;
                                insideIf = false;
                                continue;
                            }
                        }
                    } else if (parts[2] == "!=") { 
                        if (checkVal == getFlag(scene)) {
                            skipToNextConditional = true;
                            insideIf = false;
                            continue;
                        }
                    } else if (checkVal != getFlag(scene)) {
                        skipToNextConditional = true;
                        insideIf = false;
                        continue;
                    }
                    insideIf = true;
                    continue;
                } else if (command == "else") {

                    if (debugScript) print(line);
                    skipToNextConditional = false;
                    continue;
                } else if (command == "endif") {

                    if (debugScript) print(line);
                    skipToNextConditional = false;
                    skipToEndIf = false;
                    continue;
                } else if (command == "stop" && !skipToNextConditional) {

                    if (debugScript) print(line);
                    OnInteractMode = 2;
                    break;
                } else if (command == "off"  && !skipToNextConditional) {
                    if (debugScript) print(line);
                    turnedOff = true;
                    OnInteractMode = 2;
                    if (parts.Length > 1) {
                        GameObject.Find(parts[1]).GetComponent<DialogueAno2>().ext_ForceInteractScriptToParse = true;
                    }
                    break;
                }

                if (skipToNextConditional) continue;

                if (debugScript) print(line);

                if (command == "set") {
                    // set emissve of A to B
                    if (parts[1] == "emissive") {
                        Material mat = GameObject.Find(parts[3]).GetComponent<MeshRenderer>().material;
                        Color col = mat.GetColor("_EmissionColor");
                        if (parts[5] == "trem") {
                            col.r = 1f;
                            col.g = 1f;
                            col.b = 1f;
                        } else if (parts[5] == "tremoff") {
                            col.r = 0;
                            col.g = 0;
                            col.b = 0;
                        } else {
                            float v = safeFloatParse(parts[5]);
                            col.r = v; col.g = v; col.b = v;
                            if (parts.Length > 7) {
                                col.g = safeFloatParse(parts[6]);
                                col.b = safeFloatParse(parts[7]);
                            }
                        }
                        mat.SetColor("_EmissionColor", col);
                    } else {
                        setFlag(parts[1], safeIntParse(parts[3]));
                    }
                } else if (command == "choice") {
                    pauseForChoice = true;
                    // Start choice command in DialogueBox
                    return;
                } else if (command == "wait") {
                    pauseForWait = true;
                    waitTime = safeFloatParse(parts[1]);
                    if (MyInput.shortcut) waitTime *= 0.05f;
                    return;
                } else if (command == "waitForInput") {
                    pauseforWaitForInput = true;
                    return;
                } else if (command == "say") {
                    // prevent two triggering at once? probably handle this in a higher priority
                    if (box.isDialogFinished() == false) {
                        InteractParseCurLine--;
                        return;
                    }
                    pauseForSay = true;
                    if (parts.Length > 3) {
                        box.playDialogue(parts[1], safeIntParse(parts[2]), safeIntParse(parts[3]));
                    } else if (parts.Length > 2) {
                        if (parts[1] == "looped") {
                            box.playLoopedDialogue(parts[2]);
                        } else {
                            box.playDialogue(parts[1], safeIntParse(parts[2]));
                        }
                    } else {
                        box.playDialogue(parts[1]);
                    }

                    if (walkscalePlayer != null && walkscalePlayer.gameObject.activeInHierarchy && turnToPlayerTransform != null) {
                        turnToPlayerMode = 1;
                    }

                    // Start say command in dialoguebox
                    return;
                } else if (command == "print") {
                    print(line);

                    // rotateEulerY of THING to THING2
                } else if (command.ToLower() == "rotateeulery") {
                    if (parts[4] == "EXITNANOPAL") {
                        parts[4] = "PALISADE" + ExitNanoCutscenes.nanoprefix;
                    }

                    GameObject lookTarget = GameObject.Find(parts[4]);
                    if (parts[2] == "p") parts[2] = "MediumPlayer";
                    GameObject rotatee = GameObject.Find(parts[2]);
                    Vector3 oldEuler = rotatee.transform.eulerAngles;
                    rotatee.transform.LookAt(lookTarget.transform.position);
                    oldEuler.y = rotatee.transform.eulerAngles.y;
                    rotatee.transform.eulerAngles = oldEuler;

                    // lerp p to DEST in TIME (parabola height)
                } else if (command == "lerp" || command == "lerplin" || command == "lerpparabola") {
                    if (parts[1] == "p") parts[1] = "MediumPlayer";
                    if (parts[1] == "p2d") parts[1] = Registry.PLAYERNAME2D;
                    GameObject gmv = GameObject.Find(parts[1]);
                    if (command == "lerp") {
                        StartCoroutine(moveObject(gmv.transform, GameObject.Find(parts[3]).transform.position, safeFloatParse(parts[5])));
                    } else if (command == "lerplin") {
                        StartCoroutine(moveObjectLinearly(gmv.transform, GameObject.Find(parts[3]).transform.position, safeFloatParse(parts[5])));
                    } else if (command == "lerpparabola") {
                        StartCoroutine(moveObjectParabola(gmv.transform, GameObject.Find(parts[3]).transform.position, safeFloatParse(parts[6]), safeFloatParse(parts[5])));
                    }

                    // 0          1     3              8
                    // lerplinUI OBJ to ix iy fx fy in TIME
                } else if (command == "lerplinUI") {
                    StartCoroutine(moveUIObjectLinearly(GameObject.Find(parts[1]).GetComponent<Image>().rectTransform, safeFloatParse(parts[3]), safeFloatParse(parts[4]), safeFloatParse(parts[5]), safeFloatParse(parts[6]), safeFloatParse(parts[8])));
                    // move to xxx , or move OBJ to xxx
                } else if (command == "2dsnapcam") {
                    GameObject.Find(Registry.PLAYERNAME2D).GetComponent<AnoControl2D>().ForceUpdateRoomPos();
                    GameObject.Find(Registry.PLAYERNAME2D).GetComponent<AnoControl2D>().SnapCameraToPlayerAndKeepInRoom();
                } else if (command == "move") {
                    if (parts.Length > 3) {
                        if (parts[1] == "cam2d") parts[1] = "2D 160px Camera";
                        if (parts[1] == "p") parts[1] = "MediumPlayer";
                        if (parts[1] == "p2d") parts[1] = Registry.PLAYERNAME2D;
                        GameObject moveToThisObject = GameObject.Find(parts[3]);
                        GameObject.Find(parts[1]).transform.position = moveToThisObject.transform.position;
                    } else {
                        GameObject moveToThisObject = GameObject.Find(parts[2]);
                        transform.position = moveToThisObject.transform.position;
                    }
                } else if (command == "trigger") {
                    // trigger condition SCENE = VAL
                    // trigger condition SCENE != val
                    if (parts.Length > 1 && parts[1] == "condition") {
                        isWaitingForOverlap = true;
                        if (parts[3] == "=" || parts[3] == "==") {
                            hasEqualityTriggerCondition1 = true;
                            EqCond1Name = parts[2];
                            EqCond1Val = safeIntParse(parts[4]);
                        } else if (parts[3] == "!=") {
                            hasInequalityTriggerCondition1 = true;
                            IneqCond1Name = parts[2];
                            IneqCond1Val = safeIntParse(parts[4]);
                        }
                    } else if (parts.Length > 1 && parts[1] == "reset") {
                        resetTriggerOnPlayerExit = true;
                    } else if (parts.Length > 1 && parts[1] == "capacity") {

                        int cap = safeIntParse(parts[2]);
                        if (Ano2Stats.prismCapacity >= cap) {
                            isWaitingForOverlap = true;
                        } else {
                            gameObject.SetActive(false);
                        }

                        if (parts.Length > 4) {
                            playerDestAfterTrigger = parts[3];
                            lookTargetAfterTrigger = parts[4];
                        }
                    } else if (parts.Length > 1 && parts[1] == "pal-ring-3") {
                        /*
                         for (int i = 0; i < 12; i++) {
                            Ano2Stats.GetCard(i);
                        }
                        Ano2Stats.prismCurrentDust = 150;
                        Ano2Stats.dust = 51;
                        */
                        if (DataLoader.instance.getDS("pal-ring-3") == 0 && Ano2Stats.CountTotalCards() >= 12 && Ano2Stats.prismCurrentDust + Ano2Stats.dust >= 200) {
                            print("Pal-ring-3 scene active");
                            isWaitingForOverlap = true;
                        } else {
                            gameObject.SetActive(false);
                        }
                    } else {
                        isWaitingForOverlap = true;
                        // trigger destination looktarget [blackout]
                        if (parts.Length > 2) {
                            playerDestAfterTrigger = parts[1];
                            lookTargetAfterTrigger = parts[2];
                        }
                        if (parts.Length > 3 && parts[3] == "blackout") {
                            blackoutAfterTrigger = true;
                        }
                    }
                } else if (command == "cutscene") {
                    cutscene = GetComponent<CutsceneManager>();
                    cutscene.StartCutscene();
                    pauseForCutscene = true;
                    return;
                } else if (command == "fadehold") {
                    UIManagerAno2.fadeHoldTime = safeFloatParse(parts[1]);
                } else if (command == "nanoUnlock" || command == "nanounlock") {
                    // 3 ways of nanounlocking
                    // 1. Force unlocks another object
                    // 2. Comes after a 'say looped' command, and will either unlock this object, or something else.
                    // 3. Comes anywhere, unlocks some object.
                    // Both the ID-LOOPCOUNT and ID-sawunlock flags will make the nanopoint become unlocked on next scene load.

                    // nanounlock force GAMEOBJ
                    if (parts[1] == "force") {
                        GameObject.Find(parts[2]).GetComponent<Anodyne.SparkReactor>().NanopointLocked = false;

                        // nanounlock ID [GAMEOBJ]
                    } else if (DataLoader.instance.getDS(parts[1] + "-LOOPCOUNT") == 1 && DataLoader.instance.getDS(parts[1] + "-LOOPINDEX") == 0) {
                        if (parts.Length > 1) {
                            GameObject.Find(parts[2]).GetComponent<Anodyne.SparkReactor>().NanopointLocked = false;
                        } else {
                            if (GetComponent<Anodyne.SparkReactor>() != null) {
                                GetComponent<Anodyne.SparkReactor>().NanopointLocked = false;
                            }
                        }
                        pauseForNanoUnlock = true;
                        box.playDialogue("nanounlock");

                        // nanoUnlock ID notlooped GAME_OBJ
                    } else if (parts.Length > 3 && parts[2] == "notlooped" && DataLoader.instance.getDS(parts[1] + "-sawunlock") == 0) {
                        DataLoader.instance.setDS(parts[1] + "-sawunlock", 1);
                        GameObject.Find(parts[3]).GetComponent<Anodyne.SparkReactor>().NanopointLocked = false;
                        pauseForNanoUnlock = true;
                        if (parts[1] == "wolgali") {
                            box.playDialogue("nanounlock-alt");
                        } else {
                            box.playDialogue("nanounlock");
                        }
                    }

                    return;
                } else if (command == "blackout") {
                    
                    // can be used to make blackout and cancellation of scene entry fade happen based on scene entrydistance from a point
                    // blackout distanceCancel2D thingtomeasurefrom
                    if (parts.Length > 2 && parts[1] == "distanceCancel2D") {
                        Vector2 pos = new Vector2();
                        if (Registry.destinationDoorName == "") {
                            pos = GameObject.Find(Registry.PLAYERNAME2D).transform.position;
                        } else {
                            pos = GameObject.Find(Registry.destinationDoorName).transform.position;
                        }
                        if (Vector2.Distance(pos,GameObject.Find(parts[2]).transform.position) < 1) {
                            doCancelUIEntryOnLate = true;
                            if (ui != null) ui.CancelSceneEntryFade();
                            if (ui2d != null) ui2d.CancelSceneEntryFade();
                        } else {
                            continue;
                        }
                    }
                    if (ui2d != null) {
                        GameObject.Find("Under Dialogue Fade Layer").GetComponent<Image>().color = new Color(0, 0, 0, 1);
                    } else {
                        GameObject.Find("UI_FadeImage").GetComponent<Image>().color = new Color(0, 0, 0, 1);
                    }
                    // fade to black in 4, fade to white in 4, fade out in 4, fade to 1 0.4 0.3
                } else if (command == "fade") {
                    isWaitingForFadeToFinish = true;
                    if (parts[1] == "out") {
                        if (ui2d != null) ui2d.StartFade(1, 0, safeFloatParse(parts[3]));
                        if (ui != null) ui.StartFade(safeFloatParse(parts[3]), false);
                    } else if (parts[1] == "makeblack") {
                        ui.MakeFadeUIBlack();
                    } else if (parts[2] == "black") {
                        if (ui2d != null) ui2d.StartFade(new Color(0, 0, 0), 0, 1, safeFloatParse(parts[4]));
                        if (ui != null) ui.StartFade(safeFloatParse(parts[4]), true);
                    } else if (parts[2] == "partial") {
                        if (ui2d != null) ui2d.StartFade(new Color(0, 0, 0), 0, 0.8f, safeFloatParse(parts[4]));
                    } else if (parts[1] == "partialout") {
                        if (ui2d != null) ui2d.StartFade(new Color(0, 0, 0), 0.8f, 0, safeFloatParse(parts[3]));
                    } else if (parts[2] == "white") {
                        if (ui2d != null) ui2d.StartFade(new Color(1, 1, 1), 0, 1, safeFloatParse(parts[4]));
                        if (ui != null) {
                            ui.MakeFadeUIWhite();
                            ui.StartFade(safeFloatParse(parts[4]), true);
                        }
                    } else if (parts[1] == "emissive") {
                        // fade emissive of A to B
                        StartCoroutine(fadeEmissive(GameObject.Find(parts[3]).GetComponent<MeshRenderer>().material, safeFloatParse(parts[5]), 1));
                    } else {
                        if (ui2d != null) ui2d.StartFade(new Color(safeFloatParse(parts[2]), safeFloatParse(parts[3]), safeFloatParse(parts[4])), 0, 1, safeFloatParse(parts[6]));
                    }
                    return;
                } else if (command == "doorOn") {
                    GetComponent<Anodyne.Door>().isCurrentlyEnterable = true;
                } else if (command == "doorOff") {
                    GetComponent<Anodyne.Door>().isCurrentlyEnterable = false;
                } else if (command == "lightIntensity") {
                    // name dest maxtime
                    StartCoroutine(CutsceneManager.lightIntensity(parts[1], safeFloatParse(parts[2]), safeFloatParse(parts[3])));

                    // enable/disable sr of THING
                } else if (command == "enable" || command == "disable") {
                    if (parts[3] == "this") parts[3] = name;
                    if (parts[3] == "p2d") parts[3] = Registry.PLAYERNAME2D;
                    GameObject g = GameObject.Find(parts[3]);
                    bool b = true;
                    if (command == "disable") b = false;
                    if (parts[1] == "sr") {
                        g.GetComponent<SpriteRenderer>().enabled = b;
                    } else if (parts[1] == "ps") {
                        g.GetComponent<PositionShaker>().enabled = b;
                    } else if (parts[1] == "rend") {
                        g.GetComponent<Renderer>().enabled = b;
                    } else if (parts[1] == "rotator") {
                        g.GetComponent<ObjectRotator>().enabled = b;
                    } else if (parts[1] == "image") {
                        g.GetComponent<Image>().enabled = b;
                    } else if (parts[1] == "light") {
                        g.GetComponent<Light>().enabled = b;
                    } else if (parts[1] == "col") {
                        g.GetComponent<Collider>().enabled = b;
                    } else if (parts[1] == "cols") {
                        Collider2D[] cols = g.GetComponents<Collider2D>();
                        foreach (Collider2D col in cols) {
                            col.enabled = false;
                        }
                    } else if (parts[1] == "controls2d") {
                        g.GetComponent<AnoControl2D>().enabled = false;
                    } else if (parts[1] == "vac") {
                        g.GetComponent<Anodyne.Vacuumable>().enabled = b;
                    } else if (parts[1] == "anim") {
                        g.GetComponent<Animator>().enabled = b;
                    }
                    // play2d ANIM on THING
                } else if (command == "play2d") {
                    GameObject.Find(parts[3]).GetComponent<Anodyne.SpriteAnimator>().Play(parts[1]);


                    // play idle anim on p [in 2.0]
                    // play idle (defaults to this obj)
                } else if (command == "play") {
                    if (parts.Length >= 5) {
                        if (parts[4] == "p") {
                            GameObject.Find("MediumPlayer").GetComponent<MediumControl>().PlayAnimation(parts[1]);
                        } else if (parts[4] == "p2d") {

                            AnoControl2D p2d = GameObject.Find("2D Ano Player").GetComponent<AnoControl2D>();
                            p2d.GetComponent<Animator>().enabled = true;
                            p2d.GetComponent<Animator>().Play(parts[1]);
                            p2d.GetComponent<Animator>().speed = 1;
                            if (parts[1] == "idle_r") p2d.facing = AnoControl2D.Facing.RIGHT;
                            if (parts[1] == "idle_d") p2d.facing = AnoControl2D.Facing.DOWN;
                            if (parts[1] == "idle_u") p2d.facing = AnoControl2D.Facing.UP;
                            if (parts[1] == "idle_l") p2d.facing = AnoControl2D.Facing.LEFT;
                        } else {
                            if (parts[4] == "this") parts[4] = name;
                            if (parts.Length >= 7) {
                                GameObject.Find(parts[4]).GetComponent<Animator>().CrossFadeInFixedTime(parts[1], safeFloatParse(parts[6]));
                            } else {
                                GameObject.Find(parts[4]).GetComponent<Animator>().Play(parts[1]);
                            }
                        }
                    } else {
                        Animator anim = GetComponent<Animator>();
                        anim.Play(parts[1]);
                    }
                } else if (command == "LinearMovingPlatform") {
                    LinearMovingPlatform lmp = GameObject.Find(parts[1]).GetComponent<LinearMovingPlatform>();
                    lmp._Enable();
                    // timeline X/off/stop
                } else if (command == "timeline") {
                    director = GetComponent<PlayableDirector>();
                    if (parts.Length > 1 && (parts[1] == "off" || parts[1] == "stop")) {
                        print("Stopping Timeline");
                        director.Stop();
                    } else {
                        directorEndTime = safeFloatParse(parts[1]);
                        pauseForTimeline = true;
                        if (!timelinestarted) {
                            print("Starting Timeline");
                            timelinestarted = true;
                            director.Play();
                        } else {
                            print("Resuming Timeline");
                            director.playableGraph.GetRootPlayable(0).SetSpeed(1);
                        }
                        return;
                    }
                    // Changes the player-follow euler y value to that of the current camera view so when returning to normal cam, player looks right
                } else if (command == "camEulerY") {
                    if (parts.Length > 1 && parts[1] == "p") {
                        Camera.main.GetComponent<MedBigCam>().setCameraEulerYRotation(GameObject.Find("MediumPlayer").transform.localEulerAngles.y);
                    } else {
                        Camera.main.GetComponent<MedBigCam>().setCameraEulerYRotation(Camera.main.transform.eulerAngles.y);
                    }
                } else if (command == "cm") {
                    if (parts[1] == "on") {
                        cachedFOV = Camera.main.fieldOfView;
                        if (parts.Length > 2 && parts[2] == "2d") {
                            AnoControl2D.CinemachineOn2D = true;
                            Camera.main.GetComponent<CinemachineBrain>().enabled = true;
                        } else {
                            Camera.main.GetComponent<MedBigCam>().enterCinemachineMovieMode();
                        }

                    } else if (parts[1] == "off") {
                        if (parts.Length > 2 && parts[2] == "EXITNANO") {
                            if (ExitNanoCutscenes.nanoprefix == "Cougher") {
                                MedBigCam.inCinemachineMovieMode = false;
                                return;
                            }
                        }

                        if (parts.Length > 2 && parts[2] == "2d") {
                            AnoControl2D.CinemachineOn2D = false;
                            Camera.main.GetComponent<CinemachineBrain>().enabled = false;
                        } else {
                            Camera.main.GetComponent<MedBigCam>().exitCinemachineMovieMode();
                            Camera.main.fieldOfView = cachedFOV;
                        }
                        if (ActiveVirtualCamera != null) ActiveVirtualCamera.Priority = 0;
                        // cm cut to XXX
                    } else if (parts[1] == "cut") {
                        CmCutTo(parts[3]);
                        // cm smooth to XXX in TIME
                    } else if (parts[1] == "smooth") {
                        CmCutTo(parts[3], true, safeFloatParse(parts[5]));
                    }
                } else if (command == "map") {
                    // map SCENE DESTINATION (fade time) (pixelize time)
                    if (delayReactivateSpinOut) {
                        delayReactivateSpinOut = false;
                        MediumControl.doSpinOutAfterNano = true;
                    }

                    if (parts[1] == "endZera") {
                        SparkGameController.ReturnFrom2D_SourceScene = Registry.GameScenes.CenterChamber;
                        DataLoader.instance.enterScene("none", Registry.GameScenes.Wormhole, 0, 0);
                        SparkGameController.SparkGameDestObjectName = "Entrance";
                        SparkGameController.SparkGameDestScene = Registry.GameScenes.NanoZera;
                        SparkGameController.Play2DBeamInEffect = true;
                        continue;
                    }

                    if (parts[1] == "cleandb") {
                        SparkGameController.ReturnFrom2D_SourceScene = Registry.GameScenes.NanoDB_Clean;
                        DataLoader.instance.enterScene("none", Registry.GameScenes.Wormhole, 0,0);
                        SparkGameController.SparkGameDestObjectName = "CleanEntrance";
                        SparkGameController.SparkGameDestScene = Registry.GameScenes.DesertSpireTop;
                        Wormhole.ReturningFrom2D = true;
                        continue;
                    }
                    if (parts[1] == "blowup") {
                        SparkGameController.ReturnFrom2D_SourceScene = Registry.GameScenes.NanoHandfruitHaven;
                        DataLoader.instance.enterScene("none", Registry.GameScenes.Wormhole, 0, 0);
                        SparkGameController.SparkGameDestObjectName = "BlowupEntrance";
                        SparkGameController.SparkGameDestScene = Registry.GameScenes.DesertSpireTop;
                        Wormhole.ReturningFrom2D = true;
                        continue;
                    }
                    if (parts[1] == "sparkdata") {
                        parts[2] = SparkGameController.SparkGameDestObjectName;
                        parts[1] = SparkGameController.SparkGameDestScene.ToString();
                    }
                    if (parts.Length > 4) {
                        DataLoader.instance.enterScene(parts[2], parts[1], safeFloatParse(parts[3]), safeFloatParse(parts[4]));
                    } else {
                        DataLoader.instance.enterScene(parts[2], parts[1]);
                    }
                    // name, start, end
                } else if (command == "simpletitle") {
                    GameObject.Find("SimpleTitleText").GetComponent<Image>().enabled = true;
                    if (SaveManager.language == "zh-simp") {
                        GameObject.Find("SimpleTitleText").GetComponent<Image>().sprite =
                            Resources.Load<Sprite>("Visual/Sprites/UI/Anodyne2Logo_title_zhSimp_hires");
                    } else if (SaveManager.language == "zh-trad") {
                        GameObject.Find("SimpleTitleText").GetComponent<Image>().sprite = Resources.Load<Sprite>("Visual/Sprites/UI/Anodyne2Logo_title_zhTrad_hires");
                    } 
                    // uiImageFade thing alpha duration
                } else if (command.ToLower() == "uiimagefade") {
                    StartCoroutine(fadeImageAlpha(GameObject.Find(parts[1]).GetComponent<Image>(), safeFloatParse(parts[2]), safeFloatParse(parts[3])));
                } else if (command == "simpletitlefadeout") {
                    GameObject.Find("SimpleTitleText").GetComponent<Image>().CrossFadeAlpha(0, 0.8f, false);
                } else if (command == "areatitle" || command == "areacard") {
                    string soundname = "default";
                    if (parts.Length > 2) soundname = parts[2];
                    if (parts.Length > 5) {
                        StartCoroutine(fadeAreaText(safeIntParse(parts[1]), soundname, safeFloatParse(parts[3]), safeFloatParse(parts[4]), safeFloatParse(parts[5])));
                    } else {
                        StartCoroutine(fadeAreaText(safeIntParse(parts[1]), soundname));
                    }
                } else if (command == "sfxsingle" || command == "sfx") {
                    if (parts.Length > 2) {
                        audioHelper.playOneShot(parts[1], safeFloatParse(parts[2]));
                    } else {
                        audioHelper.playSFX(parts[1]);
                    }

                } else if (command == "playsong") {
                    if (parts.Length > 4) {
                        audioHelper.PlaySong(parts[1], safeFloatParse(parts[2]), safeFloatParse(parts[3]), false, safeFloatParse(parts[4]));
                    } else {
                        audioHelper.PlaySong(parts[1], safeFloatParse(parts[2]), safeFloatParse(parts[3]));
                    }
                    // name, time , target
                } else if (command == "fadesongvolume") {
                    audioHelper.FadeSong(parts[1], safeFloatParse(parts[2]), safeFloatParse(parts[3]));
                } else if (command == "stopsong") {
                    audioHelper.StopSongByName(parts[1]);
                } else if (command == "stopallsongs") {
                    if (parts[1] == "0") {
                        audioHelper.StopAllSongs(false);
                    } else {
                        audioHelper.StopAllSongs(true);
                    }
                } else if (command.ToLower() == "setinactive") {
                    // setinactive child1 A2 of3 B4 to5 0/1-6
                    if (parts[1] == "child") {
                        bool activeState = safeIntParse(parts[6]) == 0;
                        GameObject.Find(parts[4]).transform.Find(parts[2]).gameObject.SetActive(activeState);
                    } else if (parts[1] == "late") {
                        doLateSetInactive = true;
                        go_to_SetInactiveInLateUpdate = GameObject.Find(parts[2]);
                    } else if (parts[1] == "cache") {
                        if (parts[2] == "1") cached_GO_ref1.SetActive(false);
                        if (parts[2] == "2") cached_GO_ref2.SetActive(false);
                    } else if (parts[1] == "this") {
                        gameObject.SetActive(false);
                    } else if (parts[1] != name) {
                        // setinactive
                        GameObject g = GameObject.Find(parts[1]);
                        if (g == null) {
                            print("Can't find " + parts[1]);
                        } else {
                            g.SetActive(false);
                        }
                    }
                } else if (command.ToLower() == "setactive") {
                    if (parts[1] == "this") {
                        gameObject.SetActive(true);
                    } else if (parts[1] == "cache") {
                        if (parts[2] == "1") cached_GO_ref1.SetActive(true);
                        if (parts[2] == "2") cached_GO_ref2.SetActive(true);
                    } else {
                        GameObject.Find(parts[1]).SetActive(true);
                    }
                    // setchildren of X to Y
                } else if (command.ToLower() == "setchildren") {
                    GameObject g = GameObject.Find(parts[2]);
                    int children = g.transform.childCount;
                    bool activeStatus = false;
                    if (parts[4] == "active") {
                        activeStatus = true;
                    }
                    for (int childIdx = 0; childIdx < children; childIdx++) {
                        g.transform.GetChild(childIdx).gameObject.SetActive(activeStatus);
                    }
                } else if (command.ToLower() == "dontpause" || command == "doesntpause") {
                    doesntPauseAnything = true;
                } else if (command.ToLower() == "cancelsceneentryfade") {
                    doCancelUIEntryOnLate = true;
                    if (ui != null) ui.CancelSceneEntryFade();
                    if (ui2d != null) ui2d.CancelSceneEntryFade();
                } else if (command == "prismtutorial") {
                    DataLoader.instance.StartPauseTutorial();
                    pauseForPrismTut = true;
                    return;
                } else if (command == "nextfadehold3D") {
                    UIManagerAno2.sceneStartFadeHoldTime = safeFloatParse(parts[1]);
                } else if (command == "namecard") {
                    activeNameCard = GameObject.Find(parts[1]).GetComponent<NameCards>();
                    pauseForNamecard = true;
                    activeNameCard.StartAnimating();
                    return;
                } else if (command == "setIdealUIScale") {
                    float scale = UIManager2D.getIdealScaleValue();
                    GameObject.Find(parts[1]).GetComponent<RectTransform>().localScale = new Vector3(scale, scale, scale);
                    // Signal GATENAME
                } else if (command == "signal") {
                    List<GameObject> l = new List<GameObject>();
                    if (parts[1] == "this") parts[1] = name;
                    GameObject l_obj = GameObject.Find(parts[1]);
                    l.Add(l_obj);
                    HF.SendSignal(l);
                } else if (command == "skipNextSceneAudioCrossfade") {
                    AudioHelper.instance.SkipNextSceneCrossfade();
                } else if (command == "collider") {
                    if (parts[2] == "p2d") parts[2] = Registry.PLAYERNAME2D;
                    GameObject _colObj = GameObject.Find(parts[2]);
                    bool _colState = false;
                    if (parts[1] == "on") {
                        _colState = true;
                    }
                    if (_colObj.GetComponent<BoxCollider2D>() != null) _colObj.GetComponent<BoxCollider2D>().enabled = _colState;
                    if (_colObj.GetComponent<CircleCollider2D>() != null) _colObj.GetComponent<CircleCollider2D>().enabled = _colState;
                    if (_colObj.GetComponent<BoxCollider>() != null) _colObj.GetComponent<BoxCollider>().enabled = _colState;
                    if (_colObj.GetComponent<SphereCollider>() != null) _colObj.GetComponent<SphereCollider>().enabled = _colState;
                    if (_colObj.GetComponent<CapsuleCollider>() != null) _colObj.GetComponent<CapsuleCollider>().enabled = _colState;

                    // get item 3 (gives glandilock)
                } else if (command == "get") {
                    if (parts[1] == "item") {
                        Ano2Stats.GetItem(safeIntParse(parts[2]));
                    }
                } else if (command.ToLower() == "dospinout") {
                    if (parts.Length > 2 && parts[2] == "delayed") {
                        print("Spin out scheduled to activate on map change by " + name);
                        delayReactivateSpinOut = true;
                    } else if (parts.Length > 2 && parts[2] == "duringpause") {
                        print("Spin out activated by " + name);
                        MediumControl.doSpinOutAfterNanoDuringPause = true;
                        MediumControl.doSpinOutAfterNano = true;
                    } else {
                        print("Spin out activated by " + name);
                        MediumControl.doSpinOutAfterNano = true;
                    }
                } else if (command == "cancelSpinOut") {
                    print("Spin out cancelled by " + name);
                    MediumControl.doSpinOutAfterNano = false;

                    // particle COMMAND THING
                } else if (command == "particle") {
                    ParticleSystem ps = GameObject.Find(parts[2]).GetComponent<ParticleSystem>();
                    if (parts[1] == "start") {
                        ps.Play();
                    } else if (parts[1] == "pause") {
                        ps.Pause();
                    } else if (parts[1] == "stop") {
                        ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                    } else if (parts[1] == "stopandclear") {
                        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    }
                    // fadeSpriteAlpha THING ALPHA TIME
                } else if (command.ToLower() == "fadespritealpha") {
                    StartCoroutine(fadeSpriteAlpha(GameObject.Find(parts[1]).GetComponent<SpriteRenderer>(), safeFloatParse(parts[2]), safeFloatParse(parts[3])));
                    // flicker THING TIME on/off
                } else if (command == "flicker") {
                    bool endsOn = false;
                    if (parts[1] == "this") parts[1] = name;
                    if (parts[3] == "on") endsOn = true;
                    StartCoroutine(flicker(GameObject.Find(parts[1]).GetComponent<SpriteRenderer>(), safeFloatParse(parts[2]), endsOn));
                } else if (command == "doesNotShowBubble" || command == "dontshowbubble") {
                    doesNotShowBubble = true;
                } else if (command.ToLower() == "specificcrap" || command == "misc") {
                    string cmd = parts[1];
                    if (parts[1] == "hideareaname") {
                        ui2d.setAreaNameText("");
                        // misc zoom2d orthosizedest time
                        // misc zoom2d 36 6
                    } else if (parts[1] == "credits") {
                        GameObject.Find("credits1").GetComponent<CreditsEnd>().MyActivate();
                        creditsEnd = GameObject.Find("credits1").GetComponent<CreditsEnd>();
                        isWaitingForCredits = true;
                        return;
                    } else if (parts[1] == "the-end") {
                        StartCoroutine(TheEnd());
                    } else if (parts[1] == "slash") {
                        StartCoroutine(Slash(parts[2]));
                    } else if (parts[1] == "off-freeze") {
                        if (parts[2] == "1") {
                            TextureOffsetter.globalfreeze = true;
                        } else {
                            TextureOffsetter.globalfreeze = false;
                        }
                    } else if (parts[1] == "allowspark2d") {
                        GameObject.Find(Registry.PLAYERNAME2D).GetComponent<AnoControl2D>().sparkAllowedForThisArea = true;
                    } else if (parts[1] == "sanc1prismupgrade") {
                        if (Ano2Stats.prismCapacity <= 150) {
                            Ano2Stats.PrismUpgrade();
                        }
                        Ano2Stats.prismCurrentDust = 0;
                        Ano2Stats.dust = 0;
                    } else if (parts[1] == "vac") {
                        isSuckable = true;
                        vac = GetComponent<Anodyne.Vacuumable>();
                    } else if (parts[1] == "shake") {
                        GameObject.Find(Registry.PLAYERNAME2D).GetComponent<AnoControl2D>().ScreenShake(0.08f, 0.5f);
                    } else if (parts[1] == "shakelong") {
                        // shakelong 3 pico
                        // shakelong 3
                        GameObject.Find(Registry.PLAYERNAME2D).GetComponent<AnoControl2D>().ScreenShake(0.08f, safeFloatParse(parts[2]),true);
                    } else if (cmd.ToLower() == "zoom2d") {
                        StartCoroutine(Zoom2D(safeFloatParse(parts[2]), safeFloatParse(parts[3])));
                    } else if (cmd.ToLower() == "lerplocalscale") {
                        // misc scale x y z TIME THING
                        // misc scale 3 3 3 10
                        StartCoroutine(MyScale(parts[6],safeFloatParse(parts[2]), safeFloatParse(parts[3]), safeFloatParse(parts[4]), safeFloatParse(parts[5])));
                        // misc ambientColor #RRGGBBAA TIME
                    } else if (cmd.ToLower() == "ambientcolor") {
                        Color newAmbCol = new Color();
                        ColorUtility.TryParseHtmlString(parts[2], out newAmbCol);
                        StartCoroutine(ambColor(newAmbCol, safeFloatParse(parts[3])));
                    } else if (cmd.ToLower() == "fogcolor") {
                        Color newfogcol = new Color();
                        ColorUtility.TryParseHtmlString(parts[2], out newfogcol);
                        RenderSettings.fogColor = newfogcol;
                    } else if (cmd.ToLower() == "skyboxcolor") {
                        Color newfogcol = new Color();
                        ColorUtility.TryParseHtmlString(parts[2], out newfogcol);
                        if (RenderSettings.skybox.name.IndexOf("(Instance)") == -1) {
                            Material mat = new Material(RenderSettings.skybox);
                            mat.name = mat.name + "(Instance)";
                            RenderSettings.skybox = mat;
                        }
                        RenderSettings.skybox.SetColor("_Tint", newfogcol);
                        // misc lightcolor #r30ur30 LIGHT
                    } else if (cmd.ToLower() == "lightcolor") {
                        Color newfogcol = new Color();
                        ColorUtility.TryParseHtmlString(parts[2], out newfogcol);
                        GameObject.Find(parts[3]).GetComponent<Light>().color = newfogcol;
                    } else if (parts[1] == "fashion") {
                        GameObject.Find("FashionController").GetComponent<FashionController>().Begin();
                    } else if (parts[1] == "horror") {
                        if (parts[2] == "0") Anodyne.Door.SetHorrorMode(0);
                        if (parts[2] == "1") Anodyne.Door.SetHorrorMode(1);
                        if (parts[2] == "2") Anodyne.Door.SetHorrorMode(2);
                        if (parts[2] == "startgarg") GameObject.Find("prechasegarg").GetComponent<Anodyne.GargoyleChase>().ActivateFromDA2();
                        if (parts[2] == "startgargf") GameObject.Find("finalgarg").GetComponent<Anodyne.GargoyleChase>().ActivateFromDA2();
                        if (parts[2] == "pause") GameObject.Find("finalgarg").GetComponent<Anodyne.GargoyleChase>().pausefromDA2();
                        if (parts[2] == "unpause") GameObject.Find("finalgarg").GetComponent<Anodyne.GargoyleChase>().unpauseFromDA2();
                    } else if (parts[1] == "horrorUI") {
                        // do nothing
                    } else if (parts[1] == "fruitspark") {
                        GameObject.Find("Spark1").transform.Find("SparkMesh").GetComponent<Anodyne.Spark>().isDisillusioned = false;
                        GameObject.Find("Spark2").transform.Find("SparkMesh").GetComponent<Anodyne.Spark>().isDisillusioned = false;
                        GameObject.Find("Spark3").transform.Find("SparkMesh").GetComponent<Anodyne.Spark>().isDisillusioned = false;
                    } else if (parts[1] == "poison") {
                        GameObject.Find(Registry.PLAYERNAME2D).GetComponent<AnoControl2D>().poisoned = true;
                    } else if (parts[1] == "poisonoff") {
                        GameObject.Find(Registry.PLAYERNAME2D).GetComponent<AnoControl2D>().poisoned = true;
                    } else if (parts[1] == "fullheal") {
                        GameObject.Find(Registry.PLAYERNAME2D).GetComponent<Anodyne.HealthBar>().FullHeal();
                    } else if (parts[1] == "wrestle") {
                        GameObject.Find(Registry.PLAYERNAME2D).GetComponent<AnoControl2D>().wrestleOn = true;
                    } else if (parts[1] == "beginwrestling") {
                        // specificCrap beginwrestling 1/2/3
                        GameObject.Find("WrestlingManager").GetComponent<WrestlingManager>().StartMatch(safeIntParse(parts[2]));
                    } else if (parts[1] == "angrypulse") {
                        ui2d.AngryPulse();
                    } else if (parts[1] == "dogspark") {
                        GameObject.Find("DogObjectDirty").GetComponent<Anodyne.DogSpark>().Init();
                    } else if (parts[1].ToLower() == "setinteracticon") {
                        altInteractIconObjectName = parts[2];
                    } else if (parts[1] == "stopattachedaudio") {
                        GetComponent<AudioSource>().Stop();
                        // ... NAME ANIM TIMEBEFORERESET
                        // plays the animation after waiting __ seconds, if this npc is not talked to again
                    } else if (parts[1] == "restartAudioAndAnim_WithDelay") {
                        drumbirdAnim = parts[3];
                        drumbirdName = parts[2];
                        t_drumbirdReset = safeFloatParse(parts[4]);
                    } else if (parts[1] == "setCloneFlash") {
                        if (parts[2] == "1") {
                            NPCHelper.CloneDroneFlashing = true;
                        } else {
                            NPCHelper.CloneDroneFlashing = false;
                        }
                    } else if (parts[1] == "setRotatePlayerAngleOffset") {
                        rotateToPlayerOffset = safeFloatParse(parts[2]);
                    } else if (parts[1] == "fadeColorMulOnSPP") {
                        // start alpha end alpha fad etime
                        // specificCrap fadeColorMulOnSPP NAME 1 0 0.5
                        GameObject.Find(parts[2]).GetComponent<ScrollParallaxPlane>().fadeColorMul(safeFloatParse(parts[3]), safeFloatParse(parts[4]), safeFloatParse(parts[5]));
                    } else if (parts[1] == "health") {
                        Ano2Stats.TryUpgradeHealth("HEALTH" + parts[2]);
                        if (GameObject.Find(Registry.PLAYERNAME2D) != null) {
                            GameObject.Find(Registry.PLAYERNAME2D).GetComponent<Anodyne.HealthBar>().FullHeal();
                        }
                    }
                    // rotateOverTime THING X Y Z
                } else if (command.ToLower() == "rotateovertime") {
                    StartCoroutine(rotateOverTime(GameObject.Find(parts[1]).transform, new Vector3(safeFloatParse(parts[2]), safeFloatParse(parts[3]), safeFloatParse(parts[4])), safeFloatParse(parts[5])));
                } else if (command == "makeDialogueChild") {
                    // makeDialogueChild uiSize thing
                    if (parts[1] == "uiSize") {
                        Image img = GameObject.Find(parts[2]).GetComponent<Image>();
                        img.rectTransform.sizeDelta = GameObject.Find("UI").GetComponent<CanvasScaler>().referenceResolution;
                        img.rectTransform.localScale = new Vector3(1, 1, 1);
                        img.transform.SetParent(GameObject.Find("Dialogue").transform);
                        img.transform.SetAsFirstSibling();
                    }  else {
                        // keep native size but scale alongside dialogue
                        Image img = GameObject.Find(parts[2]).GetComponent<Image>();
                        img.transform.SetParent(GameObject.Find("Dialogue").transform);
                        img.transform.SetAsFirstSibling();
                        img.rectTransform.localScale = new Vector3(1, 1, 1);
                    }
                } else if (command.ToLower() == "cancelrotatetoplayer") {
                    turnToPlayerTransform = null;
                }  else if (command.ToLower() == "cache") {
                    if (parts[2] == "1") cached_GO_ref1 = GameObject.Find(parts[1]);
                    if (parts[2] == "2") cached_GO_ref2 = GameObject.Find(parts[1]);
                } else if (command == "yesno" ||command == "yesno2d") {
                    // yesno scene index yes/no type [yScene yIdx nScene nidx]
                    string cursorName = "TryAgainCursor";
                    string textName = "TryAgainText";
                    if (command == "yesno2d") {
                        cursorName = "2DGameYesNoCursor";
                        textName = "2DGameYesNoText";
                    }
                    if (parts.Length > 5) {
                        yesno = new YesNoPrompt(cursorName,textName, parts[1], safeIntParse(parts[2]),parts[5],safeIntParse(parts[6]),parts[7],safeIntParse(parts[8]));
                    } else {
                        yesno = new YesNoPrompt(cursorName, textName, parts[1], safeIntParse(parts[2]));
                    }
                    if (parts[4] == "stopOnNo") {
                        yesnoDoesStopOnNo = true;
                    }

                    if (parts[3] == "yes") {
                        yesno.StartOnYes();
                    }
                    isWaitingForYesNo = true;
                    return;
                }

            }
            OnInteractMode = 2;
        } else if (OnInteractMode == 2) {
            skipToEndIf = false;
            skipToNextConditional = false;
            OnInteractMode = 3;
        } else if (OnInteractMode == 3) {
            OnInteractMode = 0;
            isParsingInteract = false;
            AnyScriptIsParsing = false;
            isParsingStart = false;
        }
    }

    float cachedFOV = 82;
    private void CmCutTo(string cameraName,bool smooth=false,float time=1) {
        if (cameraName == "EXITNANOVC") {
            cameraName = "exitnanoVC" + ExitNanoCutscenes.nanoprefix;
        }
        CinemachineBrain brain = Camera.main.GetComponent<CinemachineBrain>();
        if (smooth) {
            brain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.EaseInOut;
            brain.m_DefaultBlend.m_Time = time;
        } else {
            brain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.Cut;
        }
        if (ActiveVirtualCamera != null) ActiveVirtualCamera.Priority = 0;
        ActiveVirtualCamera = GameObject.Find(cameraName).GetComponent<CinemachineVirtualCamera>();
        if (!SaveManager.screenshake && ActiveVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>() != null) {
            ActiveVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 0;
        }
        ActiveVirtualCamera.Priority = 100;
    }

    bool timelinestarted = false;

    int safeIntParse(string s) {
        return int.Parse(s, System.Globalization.CultureInfo.InvariantCulture);
    }
    float safeFloatParse(string s) {
        return float.Parse(s, System.Globalization.CultureInfo.InvariantCulture);
    }

    bool insideTrigger = false;
    private bool pauseForNanoUnlock;
    private bool resetTriggerOnPlayerExit;
    private bool delayReactivateSpinOut;
    private bool doCancelUIEntryOnLate;
    private bool doLateSetInactive;
    private GameObject go_to_SetInactiveInLateUpdate;
    private YesNoPrompt yesno;
    private bool isWaitingForYesNo;
    private bool yesnoDoesStopOnNo;
    private bool lastYN_was_yes = false;
    private bool isSuckable = false;
    Anodyne.Vacuumable vac;
    private bool pauseforWaitForInput;
    private CreditsEnd creditsEnd;
    private bool isWaitingForCredits;

    void OnTriggerEnter(Collider other) {
        if (turnedOff) return;
        if (MustBeInWalkscale && other.name == "BigPlayer") return;
        if (other.CompareTag("Player")) {
            // Ridescale overlapping a press-talk trigger should be ignored.
            if (other.name == "BigPlayer" && !isWaitingForOverlap) return;

            // big playe doesnt spawn the talk icon
            if (other.name != "BigPlayer" && !insideTrigger && !isWaitingForOverlap) turnOnTalkAvailableIcon();
            insideTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player")) {
            insideTrigger = false;
            turnOffTalkAvailableIcon();
            if (resetTriggerOnPlayerExit) {
                isWaitingForOverlap = true;
                resetTriggerOnPlayerExit = false;
            }
        }
    }


    void OnTriggerEnter2D(Collider2D other) {
        if (ext_DoesNotReactToOverlap) return;
        if (turnedOff) return;
        if (isSuckable && !vac.isIdle()) return;
        if (other.CompareTag("Player")) {
            if (other.GetComponent<AnoControl2D>().IsDying()) return;
            if (!doesNotShowBubble && !insideTrigger && !isWaitingForOverlap) turnOnTalkAvailableIcon();
            insideTrigger = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (ext_DoesNotReactToOverlap) return;
        if (other.CompareTag("Player")) {
            insideTrigger = false;
            turnOffTalkAvailableIcon();
            if (resetTriggerOnPlayerExit) {
                isWaitingForOverlap = true;
                resetTriggerOnPlayerExit = false;
            }
        }
    }

    void turnOffTalkAvailableIcon() {

        if (timeBeforeInteractionPromptsCanAppear > 0) return;
        if (ui != null) {
            ui.setTalkAvailableIconVisibility(false);
        }

        if (ui2d != null) {
            ui2d.setTalkAvailableIconVisibility(false,false,altInteractIconObjectName);
        }
    }

    void turnOnTalkAvailableIcon() {

        if (timeBeforeInteractionPromptsCanAppear > 0) return;
        if (ui != null) {
            ui.setTalkAvailableIconVisibility(true);
        }

        if (ui2d != null) {
            ui2d.setTalkAvailableIconVisibility(true, false, altInteractIconObjectName);
        }
    }

    public void Unpause_YieldToTimeline() {
        pauseForTimeline = false;
    }


    IEnumerator fadeAreaText(int areaID, string soundName="default", float fadeInTime=1f, float holdTime=1f,float fadeOutTime=1f) {
        float time = 0;
        TMPro.TMP_Text text = GameObject.Find("AreaNameText").GetComponent<TMPro.TMP_Text>();
        text.text = DataLoader.instance.getRaw("areanames", areaID);

        Color c = text.color;
        c.a = 0;
        if (soundName == "default") {
            AudioHelper.instance.playSFX("areaCardAppear");
        } else if (soundName != "none") {
            AudioHelper.instance.playSFX(soundName);
        }


        while (time < fadeInTime) {
            time += Time.deltaTime;
            c.a = Mathf.SmoothStep(0, 1, time / fadeInTime);
            text.color = c;
            yield return new WaitForEndOfFrame();
        }
        time = 0;
        while (time < holdTime) {
            time += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        time = 0;
        while (time < fadeOutTime) {
            time += Time.deltaTime;
            c.a = Mathf.SmoothStep(1,0, time / fadeOutTime);
            text.color = c;
            yield return new WaitForEndOfFrame();
        }
    }
     
    IEnumerator fadeEmissive(Material mat, float target, float time) {
        float t = 0;
        Color col = mat.GetColor("_EmissionColor");
        float start = col.r;
        float cur = start;
        while (t < time) {
            t += Time.deltaTime;
            cur = Mathf.Lerp(start, target, t / time);
            col.r = cur; col.g = cur; col.b = cur;
            mat.SetColor("_EmissionColor", col);
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator fadeImageAlpha(Image sr, float endAlpha, float tm) {
        Color c = sr.color;
        float t = 0;
        float a = 0;
        float startAlpha = sr.color.a;
        while (t < tm) {
            if (MyInput.shortcut) t += 4 * Time.deltaTime;
            t += Time.deltaTime;
            a = Mathf.SmoothStep(startAlpha, endAlpha, t / tm);
            c.a = a;
            sr.color = c;
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator TheEnd() {
        float endAlpha = 1;
        float tm = 5f;
            
        TMPro.TMP_Text sr = GameObject.Find("TheEndText").GetComponent<TMPro.TMP_Text>();
        sr.text = DataLoader.instance.getRaw("the-end-only", 0);
        sr.color = Color.white;
        Color c = sr.color;
        float t = 0;
        float a = 0;
        float startAlpha = 0;
        while (t < tm) {
            if (MyInput.shortcut) t += 4 * Time.deltaTime;
            t += Time.deltaTime;
            a = Mathf.SmoothStep(startAlpha, endAlpha, t / tm);
            c.a = a;
            sr.color = c;
            yield return new WaitForEndOfFrame();
        }
    }

    //theEndText = GameObject.Find("TheEndText").GetComponent<TMPro.TMP_Text>();
    IEnumerator Slash(string anim) {
        Image img = GameObject.Find("DustAttackEnd").GetComponent<Image>();
        Color c = img.color;
        float t = 0;
        float a = 0;
        Vector3 startPos = img.rectTransform.anchoredPosition;
        img.GetComponent<Anodyne.SpriteAnimator>().Play(anim);
        Vector3 offset = new Vector3();
        float amp = 2f;   
        while (t < 1) {
            t += Time.deltaTime;
            a = Mathf.SmoothStep(1, 0, t / 1f);
            c.a = a;
            img.color = c;
            offset.x = -amp + 2 * amp * Random.value;
            offset.y = -amp + 2 * amp * Random.value;
            img.rectTransform.anchoredPosition = offset + startPos;
            yield return new WaitForEndOfFrame();
        }
        //"DustAttackEnd""
    }

    IEnumerator fadeSpriteAlpha(SpriteRenderer sr, float endAlpha, float tm) {
        Color c = sr.color;
        float t = 0;
        float a = 0;
        float startAlpha = sr.color.a;
        while (t < tm) {
            if (MyInput.shortcut) t += 4 * Time.deltaTime;
            t += Time.deltaTime;
            a = Mathf.SmoothStep(startAlpha, endAlpha, t / tm);
            c.a = a;
            sr.color = c;
            yield return new WaitForEndOfFrame();
        }
    }
    IEnumerator moveObjectLinearly(Transform obj, Vector3 dest, float timeToMove) {
        Vector3 start = obj.position;
        float time = 0;
        Vector3 newPos = new Vector3();
        while (time < timeToMove) {
            if (MyInput.shortcut) time += 4 * Time.deltaTime;
            time += Time.deltaTime;
            newPos = Vector3.Lerp(start, dest, time / timeToMove);
            obj.position = newPos;
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator moveUIObjectLinearly(RectTransform obj, float ix, float iy, float fx, float fy, float timeToMove) {
        Vector3 newPos = obj.anchoredPosition;
        newPos.Set(ix, iy, newPos.z);
        obj.anchoredPosition = newPos;

        Vector3 start = obj.anchoredPosition;
        Vector3 dest = new Vector3();
        dest.Set(fx, fy, newPos.z);

        float time = 0;
        while (time < timeToMove) {
            if (MyInput.shortcut) time += 4 * Time.deltaTime;
            time += Time.deltaTime;
            newPos = Vector3.Lerp(start, dest, time / timeToMove);
            obj.anchoredPosition = newPos;
            yield return new WaitForEndOfFrame();
        }
    }



    IEnumerator rotateOverTime(Transform obj, Vector3 dest, float maxTime) {
        Vector3 start = obj.localEulerAngles;
        float time = 0;
        Vector3 newRot = new Vector3();
        while (time < maxTime) {
            if (MyInput.shortcut) time += 4 * Time.deltaTime;
            time += Time.deltaTime;
            newRot = Vector3.Lerp(start, dest, Mathf.SmoothStep(0, 1, time / maxTime));
            obj.localEulerAngles= newRot;
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator moveObject(Transform obj, Vector3 dest, float maxTime) {
        Vector3 start = obj.position;
        float time = 0;
        Vector3 newPos = new Vector3();
        while (time < maxTime) {
            if (MyInput.shortcut) time += 4 * Time.deltaTime;
            time += Time.deltaTime;
            newPos = Vector3.Lerp(start, dest, Mathf.SmoothStep(0, 1, time / maxTime));
            obj.position = newPos;
            yield return new WaitForEndOfFrame();
        }
    }
    IEnumerator ambColor(Color newCol, float tm) {
        float t = 0;
        Color col = RenderSettings.ambientLight;
        Color startCol = col;
        while (t < tm) {
            t += Time.deltaTime;
            col = Color.Lerp(startCol, newCol, t / tm);
            RenderSettings.ambientLight = col;
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator moveObjectParabola(Transform obj, Vector3 dest, float height, float tm) {
        Vector3 start = obj.position;
        float t = 0;
        Vector3 newpos = new Vector3();
        while (t < tm) {
            t += Time.deltaTime;
            newpos = Vector3.Lerp(start, dest, t / tm);
            float angle = (t / tm) * 3.14f;
            angle = Mathf.Sin(angle);
            float y_offset = angle * height;
            newpos.y += y_offset;
            obj.position = newpos;
            yield return new WaitForEndOfFrame();
        }

    }

    IEnumerator flicker(SpriteRenderer sr, float tm, bool endsOn, float rate=0.05f) {
        float t = 0;
        float t_rate = 0;
        while (t < tm) {
            t += Time.deltaTime;
            t_rate += Time.deltaTime;
            if (t_rate > rate) {
                t_rate = 0;
                sr.enabled = !sr.enabled;
            }
            yield return new WaitForEndOfFrame();
        }
        if (endsOn) {
            sr.enabled = true;
        } else {
            sr.enabled = false;
        }
    }

    IEnumerator Zoom2D(float destSize, float time) {
        float t = 0;
        Camera cam = Camera.main;
        float v0 = cam.orthographicSize;
        float vf = destSize;
        while (t < time) {
            t += Time.deltaTime;
            cam.orthographicSize = Mathf.Lerp(v0, vf, t / time);
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator MyScale(string nameOfObjectToScale, float newX, float newY, float newZ, float time) {
        float t = 0;
        Transform tr = GameObject.Find(nameOfObjectToScale).transform;
        Vector3 v0 = tr.localScale;
        Vector3 vf = new Vector3(newX,newY,newZ);
        while (t < time) {
            t += Time.deltaTime;
            tr.localScale = Vector3.Lerp(v0, vf, t / time);
            yield return new WaitForEndOfFrame();
        }
    }

    private void LateUpdate() {

        if (doCancelUIEntryOnLate) {
            doCancelUIEntryOnLate = false;
            if (ui != null) ui.CancelSceneEntryFade();
            if (ui2d != null) ui2d.CancelSceneEntryFade();
        }
        if (doLateSetInactive) {
            doLateSetInactive = false;
            go_to_SetInactiveInLateUpdate.SetActive(false);
        }
    }
}

