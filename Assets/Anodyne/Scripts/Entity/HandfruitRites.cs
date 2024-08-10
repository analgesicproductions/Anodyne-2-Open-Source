using Anodyne;
using System.Collections.Generic;
using UnityEngine;

public class HandfruitRites : MonoBehaviour {

    public bool IsBridge = false;
    public bool IsSlide = false;
    public bool IsColorDice = false;
    public bool IsLetterDice = false;

    DialogueBox dbox;
    Rigidbody2D rb;
    Vacuumable vac;
    SpriteAnimator anim;

    int currentRiteDay = 0;

    List<Transform> bridgePartTransforms;
    List<Vector3> bridgePartInitPositions;

    AnoControl2D player;

	void Start () {

        HF.GetDialogueBox(ref dbox);
        HF.GetPlayer(ref player);
        if (DataLoader.instance.getDS("rites-done-3") == 1) {
            currentRiteDay = 4;
        } else if (DataLoader.instance.getDS("rites-done-2") == 1) {
            currentRiteDay = 3;
        } else if (DataLoader.instance.getDS("rites-done-1") == 1) {
            currentRiteDay = 2;
        } else {
            currentRiteDay = 1;
        }


        if (ForceDay > -1) {
            print("Warning: force day on");
            currentRiteDay = ForceDay;
        }

        if (IsBridge) {
            bridgePartTransforms = new List<Transform>();
            bridgePartInitPositions = new List<Vector3>();
            for (int i = 0; i < 16; i++) {
                bridgePartTransforms.Add(transform.Find("BridgePiece" + " (" + i.ToString() + ")"));
                bridgePartInitPositions.Add(bridgePartTransforms[i].position);
            }
        }
        if (IsSlide) {
            if (currentRiteDay == 2) {
                soupTransform.GetComponent<SpriteAnimator>().Play("rose");
            } else if (currentRiteDay >= 3) {
                soupTransform.gameObject.SetActive(false);
            }
            if (DataLoader._getDS("haven-first-time") == 0) {
                soupTransform.GetComponent<SpriteRenderer>().enabled = false;
            }
        }
    }

    public int ForceDay = -1;

	void Update () {

        if (ForceDay > -1) {
            currentRiteDay = ForceDay;
        }

        if (ui2d == null) ui2d = GameObject.Find("2D UI").GetComponent<UIManager2D>();
        if (IsBridge) {
            UpdateBridge();
        } else if (IsSlide) {
            UpdateSlide();
        } else if (IsColorDice || IsLetterDice) {
            UpdateDice();
        }
	}

    Vector3 tempV = new Vector3();
    Vector2 tempVel = new Vector2();

    public Transform BridgeRetryPosition;
    bool playerInBridgeZone = false;
    public int stability = 1;
    int timesFailed = 0;
    float topFailureY = 77.32f;
    float bottomFailureY = 75.5f;
    float t_bridgeForceOsc = 0;
    float t_bridgePartOsc = 0;
    public float bridgeOscP = 2f;
    public float speedMul = 2f;
    int bridgeMode = 0;

    float t_bridgeFade = 0;

    void UpdateBridge() {
        HF.TimerDefault(ref t_bridgePartOsc, 1f);
        for (int i = 0; i < 16; i++) {
            float diff = Mathf.Abs(player.transform.position.x - bridgePartTransforms[i].position.x);
            //if (Mathf.Abs(player.transform.position.x) - bridgePartTransforms[i].position.x < 8) { // makes it like 'bad stairs'
            if (playerInBridgeZone && Mathf.Abs(player.transform.position.x - bridgePartTransforms[i].position.x) < 1f) {
                tempV = bridgePartInitPositions[i];
                tempV.y -= 0.25f * (1 - (diff / 1f));
                bridgePartTransforms[i].position = tempV;
            } else {
                bridgePartTransforms[i].position = bridgePartInitPositions[i];
            }

            if (playerInBridgeZone) {
                tempV = bridgePartTransforms[i].position;
                if (i % 2 == 0) {
                    tempV.y += Mathf.Sin((t_bridgePartOsc / 1) * 360f * Mathf.Deg2Rad) * 0.075f;
                } else {
                    tempV.y -= Mathf.Sin((t_bridgePartOsc / 1) * 360f * Mathf.Deg2Rad) * 0.075f;
                }
                bridgePartTransforms[i].position = tempV;
            }


        }

        if (bridgeMode == 0) {
            if (playerInBridgeZone) {
                if (player.IsCinemachineOnButAllowingMovement() == false) {
                    player.EnterFixedCamMode("bridgeVC");
                }
                t_bridgeForceOsc += Time.deltaTime;
                float difficultyReduction = Mathf.Lerp(1f, 0.2f, (stability - 1) / 15f);
                float sinVal = difficultyReduction * Mathf.Sin((t_bridgeForceOsc / bridgeOscP) * 360f * Mathf.Deg2Rad); // -1 to 1
                tempV = player.transform.position;
                tempV.y += Time.deltaTime * sinVal * speedMul;
                player.transform.position = tempV;

                if (player.transform.position.y > topFailureY || player.transform.position.y < bottomFailureY) {
                    bridgeMode = 1;
                    if (currentRiteDay == 3) {
                        dbox.playLoopedDialogue("haven-bridge-day3");
                    } else {
                        dbox.playLoopedDialogue("haven-bridge");
                    }
                    timesFailed++;
                    if (currentRiteDay == 1) stability = 1 + timesFailed * 2;
                    if (currentRiteDay == 2) stability = 3 + timesFailed * 2;
                    if (currentRiteDay == 3) stability = 5 + timesFailed * 2;
                    print("stabilty now " + stability);
                }

            } else {
                if (player.IsCinemachineOnButAllowingMovement() && !player.IsExitingCinemachineMovementMode()) {
                    player.StartToExitFixedCamMode();
                }
                t_bridgeForceOsc = 0;
            }
        } else if (bridgeMode == 1 && dbox.isDialogFinished()) {
            CutsceneManager.deactivatePlayer = true;
            ui2d.StartFade(0, 1, 0.25f);
            t_bridgeFade = 0;
            bridgeMode = 2;
        } else if (bridgeMode == 2) {
            if (HF.TimerDefault(ref t_bridgeFade,0.25f)) {
                CutsceneManager.deactivatePlayer = false;
                player.transform.position = BridgeRetryPosition.position;
                player.StartToExitFixedCamMode();
                t_bridgeFade = 0;
                bridgeMode = 3;
            }
        } else if (bridgeMode == 3) {
            player.transform.position = BridgeRetryPosition.position;
            if (HF.TimerDefault(ref t_bridgeFade,1f)) {
                t_bridgeFade = 0;
                bridgeMode = 4;
                ui2d.StartFade(1, 0, 0.25f);
            }
        } else if (bridgeMode == 4) {
            player.transform.position = BridgeRetryPosition.position;
            if (HF.TimerDefault(ref t_bridgeFade,0.25f)) {
                bridgeMode = 0;
            }
        }
    }

    [Header("Slide Stuff")]
    public TriggerChecker slideTriggerChecker;
    public BoxCollider2D slideExitBlocker;
    public Vacuumable soupVac;
    public Transform soupTransform;
    int slideMode = 0;
    public Transform slideStartPos;
    public Transform slideEndPos;
    float t_slideOsc = 0;
    public float slideOscAmp = 0.44f;
    float slideOscPeriod = 2.3f;
    float slideOscLength = 3.9375f;
    void UpdateSlide() {
        if (slideMode == 1) {
            slideExitBlocker.enabled = true;
            slideTriggerChecker.ThingToCheckFor = "Soup";
            soupVac.enabled = true;
            slideMode = 2;
        } else if (slideMode == 2) {
            // Stop the thing from moving once it overlaps the slide entry trigger
            if (soupVac.isMoving() && Vector2.Distance(soupTransform.position,player.transform.position) > 3f) {
                soupVac.Respawn();
            }
            if (slideTriggerChecker.onThingToCheckFor || (currentRiteDay == 3 && slideTriggerChecker.onPlayer2D)) {
                
                slideMode = 3;
                if (currentRiteDay == 3) {
                    tempV = player.transform.position;
                    player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                    player.GetComponent<CircleCollider2D>().enabled = false;
                } else {
                    tempV = soupTransform.position;
                    soupTransform.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                    soupTransform.GetComponent<CircleCollider2D>().enabled = false;
                }
            }
        } else if (slideMode == 3) {
            // Lerp it to the start point of the slide
            t_slideOsc += Time.deltaTime;
            if (currentRiteDay == 3) {
                player.transform.position = Vector3.Lerp(tempV, slideStartPos.position, t_slideOsc / 0.4f);
            } else {
                soupTransform.position = Vector3.Lerp(tempV, slideStartPos.position, t_slideOsc / 0.4f);
            }
            if (t_slideOsc >= 0.4f) {
                slideMode = 4;
                t_slideOsc = 0;

                if (currentRiteDay == 1) {
                    dbox.playDialogue("haven-slide-1", 0, 2);
                } else if (currentRiteDay == 2) {
                    dbox.playDialogue("haven-slide-2", 0, 1);
                } else {
                    dbox.playDialogue("haven-slide-3", 0);
                }
            }
        } else if (slideMode == 4 && dbox.isDialogFinished()) {
            // Oscillate it downwards
            t_slideOsc += Time.deltaTime;
            if (t_slideOsc > slideOscPeriod) t_slideOsc = slideOscPeriod;
            tempV = slideStartPos.position;
            tempV.y -= slideOscLength * (t_slideOsc / slideOscPeriod);
            tempV.x -= Mathf.Sin((t_slideOsc / slideOscPeriod) * 360f * Mathf.Deg2Rad) * slideOscAmp;
            if (currentRiteDay == 3) {
                player.transform.position = tempV;
             } else {
                soupTransform.position = tempV;
            }
            if (t_slideOsc >= slideOscPeriod) {
                slideMode = 5;
                t_slideOsc = 0;
            }

        } else if (slideMode == 5) {
            t_slideOsc += Time.deltaTime;
            if (currentRiteDay == 3) {
                player.transform.position = Vector3.Lerp(tempV, slideEndPos.position, t_slideOsc / 0.5f);
            } else {
                soupTransform.position = Vector3.Lerp(tempV, slideEndPos.position, t_slideOsc / 0.5f);
            }
            if (t_slideOsc >= 0.5f) {
                slideMode = 6;
                dbox.dontFadeOutAtEndOfFadeText = true;
                if (currentRiteDay == 1) {
                    dbox.playDialogue("haven-slide-1", 3,5);
                } else if (currentRiteDay == 2) {
                    dbox.playDialogue("haven-slide-2", 2,3);
                } else {
                    dbox.playDialogue("haven-slide-3", 1,5);
                }
            }
        } else if (slideMode == 6 && dbox.isDialogFinished()) {
            DataLoader.instance.setDS("dice-done", 0);
            DataLoader.instance.setDS("dice-color-done", 0);
            DataLoader.instance.setDS("dice-letter-done", 0);
            if (currentRiteDay == 1) {
                DataLoader.instance.setDS("rites-done-1", 1);
                DataLoader.instance.enterScene("EntranceNovaBed", Registry.GameScenes.NanoDustbound,0);
            } else if (currentRiteDay == 2) {
                DataLoader.instance.setDS("rites-done-2", 1);
                UIManagerAno2.sceneStartFadeHoldTime = 0.5f;
                DataLoader.instance.enterScene("EntranceFromHaven", Registry.GameScenes.NanoDB_Wrestling,0);
            } else if (currentRiteDay == 3) {
                DataLoader.instance.setDS("rites-done-3", 1);
                UIManagerAno2.sceneStartFadeHoldTime = 0.5f;
                DataLoader.instance.enterScene("EntranceFromHaven", Registry.GameScenes.NanoDB_Wrestling,0);
            }
            slideMode = 7;

        }
    }

    bool diceResultFixed = true;
    int diceState = 0;
    Transform diceTalk;
    DialogueAno2 diceTalkDA2;
    UIManager2D ui2d;
    void UpdateDice() {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (vac == null) vac = GetComponent<Vacuumable>();
        if (anim == null) anim = GetComponent<SpriteAnimator>();
        if (diceTalk == null) {
            if (IsColorDice) diceTalk = GameObject.Find("ColorDiceTalk").transform;
            if (IsLetterDice) diceTalk = GameObject.Find("LetterDiceTalk").transform;
            diceTalkDA2 = diceTalk.GetComponent<DialogueAno2>();
        }

        diceTalk.position = transform.position;
        if (diceState == 0) {
            if (vac.isIdle()) {
                diceTalkDA2.enabled = true;
                diceTalkDA2.doesNotShowBubble = false;
            } else {
                diceTalkDA2.enabled = false;
                ui2d.setTalkAvailableIconVisibility(false);
            }
            if (vac.IsBeingSucked() || !vac.isIdle()) {
                diceTalkDA2.doesNotShowBubble = true;
            }

            if (vac.isMoving()) {
                diceState = 1;
                if (1 == DataLoader.instance.getDS("dice-done")) {
                    diceResultFixed = false;
                }
                anim.Play("roll");
            }
        } else if (diceState == 1) {
            tempVel = rb.velocity;
            tempVel *= 0.9f;
            rb.velocity = tempVel;
            if (rb.velocity.magnitude < 0.5f) {
                rb.velocity = Vector2.zero;
                diceState = 2;
                // Play fixed end animation and set corresponding dice flag
                if (diceResultFixed) {
                    if (IsColorDice) {
                        if (currentRiteDay == 3) {
                            anim.Play("red");
                        } else if (currentRiteDay == 2) {
                            anim.Play("yellow");
                        } else {
                            anim.Play("blue");
                        }
                        DataLoader.instance.setDS("dice-color-done", 1);
                    } else {
                        if (currentRiteDay == 3) {
                            anim.Play("d");
                        } else if (currentRiteDay == 2) {
                            anim.Play("a");
                        } else {
                            anim.Play("c");
                        }
                        DataLoader.instance.setDS("dice-letter-done", 1);
                    }
                // Random dice colors
                } else {
                    if (IsColorDice) {
                        if (Random.value > 0.66f) {
                            anim.Play("green");
                        } else {
                            anim.Play("yellow");
                        }
                    } else {
                        if (Random.value > 0.66f) {
                            anim.Play("b");
                        } else {
                            anim.Play("a");
                        }
                    }
                }
            }
        } else if (diceState == 2) {
            if (DataLoader.instance.getDS("dice-color-done") == 1 && DataLoader.instance.getDS("dice-letter-done") == 1) {
                if (0 == DataLoader.instance.getDS("dice-done")) {
                    GameObject.Find("BridgeEntryBlocker").GetComponent<BoxCollider2D>().isTrigger = true;
                    DataLoader.instance.setDS("dice-done", 1);
                    GameObject.Find("DiceGate1").GetComponent<Gate>().SendSignal();
                    GameObject.Find("DiceGate2").GetComponent<Gate>().SendSignal();
                    if (currentRiteDay == 3) {
                        dbox.playDialogue("haven-dice", 2);
                    } else if (currentRiteDay == 2) {
                        dbox.playDialogue("haven-dice", 1);
                    } else {
                        dbox.playDialogue("haven-dice", 0);
                    }
                }
            }
            diceState = 0;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("Player")) {
            if (IsBridge) {
                playerInBridgeZone = true;
            } else if (IsSlide) {
                if (slideMode== 0) slideMode = 1;
            }
        }
    }
    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.CompareTag("Player")) {
            if (IsBridge) {
                playerInBridgeZone = false;
            }
        }
    }
}
