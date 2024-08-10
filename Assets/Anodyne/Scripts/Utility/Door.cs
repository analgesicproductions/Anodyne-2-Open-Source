using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Anodyne {
    public class Door : MonoBehaviour {
        public static void ResetHorrorStatics() {
            horrorChaseIndex = 0;
            horrorCounter = 0;
            horrorGlowCounter = 0;
            horrorMode = 0;
            horrorSpawnGlow = false;
        }
        public static bool horrorStopBecause_GargKilledPlayer = false;
        public static int horrorChaseIndex = 0;
        public static int horrorCounter = 0;
        public static int horrorGlowCounter = 0;
        public static int horrorMode = 0;
        public static bool horrorSpawnGlow = false;
        public static int[] horrorChaseArray = new int[] { 2, 1, 2, 3, 1,1,2 };

        bool isNexusPad;

        public bool entersOnOverlap = false;
        public bool goesToCurrentScene = false;
        [Tooltip("Assumes childed twice, like Room1->2D OVerlapDoor->Door")]
        public bool autoSetDestinationSibling = false;
        
        [Tooltip("Can be toggled so this door isn't active")]
        public bool isCurrentlyEnterable = true;
        [Tooltip("Set to true if this takes you to nano. This could overlap with an NPC so you have a talk + shrink option")]
        public bool isANanoDoor = false;
        public bool isPicoPoint = false;
        public bool exitsToWormholeFrom2D = false;
        public Registry.GameScenes destinationScene;
        public string destinationObjectName;
        public NanopointData nanopointData;
        public float nextEulerY = -1;
        public float nextSceneStartFadeHoldTime = -1;
        public string SetTo1OnEnter = "";
        public int SetToOnEnterVal = 1;
        [Header("Horror")]
        public bool IsHorror = false;
        [Range(0,1)]
        [Tooltip("0 for not a regular door, 1 for gravel")]
        public int gravelVal = 0;
        bool HorrorIsGlowing = false;

        // set when entering the nora apt hallway via a trigger,
        // or before approaching the boss?
        public static void SetHorrorMode(int m = 0) {
            //print("Horror mode "+m);
            horrorMode = m;
            if (m == 0) {
                horrorCounter = 6; 
            } else if (m == 1) {
                horrorCounter = 8;
            } else if (m == 2) {
                horrorChaseIndex = 0;
                horrorCounter = horrorChaseArray[horrorChaseIndex];
                horrorGlowCounter = 5;
                horrorSpawnGlow = false;
            }
        }

        YesNoPrompt yesno;
        //int yesNoState = 0;

        UIManagerAno2 ui;
        UIManager2D ui2d;

        bool dontAllowMoreTriggering = false;

        void Start() {
            horrorStopBecause_GargKilledPlayer = false;
            if (name.IndexOf("NexusPad") != -1) {
                isNexusPad = true;
            }

            if (IsHorror && horrorMode == 2) {

                if (horrorSpawnGlow) { // set when horrorcounter hits 0
                    int siblings = transform.parent.childCount;
                    int glowIndex = horrorChaseIndex % siblings;
                    if (glowIndex == transform.GetSiblingIndex()) {
                        HorrorIsGlowing = true;
                        GameObject.Find("grav" + transform.parent.name).GetComponent<SpriteAnimator>().Play(name.Split(' ')[2]);
                    }
                } else {
                    if (transform.GetSiblingIndex() == 0) {
                        GameObject.Find("grav" + transform.parent.name).GetComponent<SpriteRenderer>().enabled = false;
                    }
                }
            }

            if (autoSetDestinationSibling) {
                destinationObjectName = transform.parent.parent.name+"||"+transform.parent.name+"||"+transform.parent.GetChild(1).name;
            }
            if (GameObject.Find("3D UI") != null) {
                ui = GameObject.Find("3D UI").GetComponent<UIManagerAno2>();

            } else if (GameObject.Find("2D UI") != null) {
                ui2d = GameObject.Find("2D UI").GetComponent<UIManager2D>();
            }
        }

        [System.NonSerialized]
        public bool ext_ForceEnterDoor = false;
        // Update is called once per frame
        int mode = 0;
        void Update() {
            if (mode == 0) {
                if (ext_ForceEnterDoor) {
                    mode = 1;
                }
            } else if (mode == 10) {
                if (MyInput.jpTalk) {
                    yesNoDoor_Talked = true;
                    CutsceneManager.deactivatePlayer = true;
                    ui2d.setTalkAvailableIconVisibility(false);
                    mode = 11;
                    if (GameObject.Find(Registry.PLAYERNAME2D) != null) GameObject.Find(Registry.PLAYERNAME2D).GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                    yesno = new YesNoPrompt("2DGameYesNoCursor", "2DGameYesNoText", "exitNanoPortal", 0);
                    GameObject.Find("2D Game YesNo").GetComponent<UnityEngine.UI.Image>().enabled = true;
                }
            } else if (mode == 11) {
                int retval = yesno.Update();
                if (retval == 1) {
                    CutsceneManager.deactivatePlayer = false;
                    mode = 1;
                    entersOnOverlap = true;
                } else if (retval == 0) {
                    CutsceneManager.deactivatePlayer = false;
                    mode = 0;
                    yesNoDoor_Talked = false;
                    GameObject.Find("2D Game YesNo").GetComponent<UnityEngine.UI.Image>().enabled = false;
                }
            } else if (mode == 1) {
                if (entersOnOverlap || (!(isANanoDoor || isPicoPoint) && MyInput.jpTalk) || ((isANanoDoor || isPicoPoint)&& ext_ForceEnterDoor)) {
                    if (dontAllowMoreTriggering) {
                        print("<color=yellow>Duplicate door entering detected, ignoring.</color>");
                        return;
                    }
                    dontAllowMoreTriggering = true;

                    if (horrorStopBecause_GargKilledPlayer) {
                        print("Prevent horror door entry bc garg killed player.");
                        return;
                    }

                    if (isNexusPad) {
                        AudioHelper.instance.playSFX("teleport_up");
                    } else if (entersOnOverlap) {
                        AudioHelper.instance.playSFX("enter_Door");
                    } else if (ui2d != null) {
                        AudioHelper.instance.playSFX("enter_Door");
                    }

                    #region Horror door stuff
                    if (IsHorror) {
                        horrorSpawnGlow = false;
                        // during gravel mode only gravel doors progress
                        if (horrorMode == 2) {
                            GargoyleChase.Door_TurnOff = true;
                            horrorCounter--;
                            if (HorrorIsGlowing) {
                                AudioHelper.instance.playSFX("save3dglow", true, 1f);
                                AudioHelper.instance.playSFX("gateOpenAno2", true, 1f);
                                horrorGlowCounter--;
                            }
                        } else if (horrorMode == 1) {
                            if (gravelVal == 1) {
                                horrorCounter--;
                            }
                        } else {
                            horrorCounter--;
                        }
                       // print(horrorCounter + " counter");
                       // print(horrorGlowCounter + " " + "glowcounter");
                        if (horrorCounter == 0) {
                            if (horrorMode == 0) {
                                destinationObjectName = "BuildingExitPos";
                            } else if (horrorMode == 1) {
                                destinationObjectName = "GravelExitPos";
                            } else if (horrorMode == 2) {
                                if (horrorGlowCounter == 0) {
                                    destinationObjectName = "ChaseExitPos";
                                } else {
                                    // when counter reaches zero, next scene entry, one door in each room will glow.
                                    horrorChaseIndex++;
                                    horrorSpawnGlow = true;
                                    if (horrorChaseIndex == horrorChaseArray.Length) {
                                        horrorChaseIndex = 0;
                                    }
                                    horrorCounter = horrorChaseArray[horrorChaseIndex];
                                }
                            }
                        }
                    }
                    #endregion


                    ext_ForceEnterDoor = false;
                    if (isANanoDoor) {
                        Registry.InitializeNanopointData(nanopointData);
                    } else if (isPicoPoint) {
                        if (exitsToWormholeFrom2D) {
                            Registry.InitializeNanopointData(nanopointData);
                        } else { 
                            Registry.InitPicopoint();
                        }
                    }
                    if (autoSetDestinationSibling) {
                        Registry.DestinationDoorIsTwoDeep = true;
                    }
                    if (nextEulerY != -1) {
                        MediumControl.nextMediumEulerY = nextEulerY;
                        MedBigCam.initialCameraRotation = nextEulerY;
                    }
                    if (nextSceneStartFadeHoldTime != -1) {
                        UIManagerAno2.sceneStartFadeHoldTime = nextSceneStartFadeHoldTime;

                    }
                    if (exitsToWormholeFrom2D && isANanoDoor) {
                        isANanoDoor = false;
                        print("Warning isANanoDoor AND exitsToWormholeFrom2D both set");

                    }
                    if (SetTo1OnEnter.Length > 1) {
                        DataLoader.instance.setDS(SetTo1OnEnter, SetToOnEnterVal);
                    }
                    if (isANanoDoor) {
                        SparkGameController.SparkGameDestObjectName = destinationObjectName;
                        SparkGameController.SparkGameDestScene = destinationScene;

                        string flagname = SparkGameController.SparkGameDestScene.ToString() + "SPARKGAMEDONE";
                        if (destinationObjectName == "FantasyEntrance" && destinationScene == Registry.GameScenes.NanoNexus) {
                            flagname += "1";
                        }
                        bool hasSparkGameBeenFinished = DataLoader.instance.getDS(flagname) == 1;

                        if (hasSparkGameBeenFinished || destinationScene == Registry.GameScenes.NanoAlbumen || nanopointData.SkipsToWormhole) {
                            DataLoader.instance.enterScene("none", Registry.GameScenes.Wormhole, 0.4f);
                            if (nanopointData.SkipsEpisodeTitle) Wormhole.skipEpisodeTitle = true;
                            if (destinationScene != Registry.GameScenes.NanoAlbumen && destinationScene != Registry.GameScenes.NanoSanctuary) {
                                SparkGameController.Play2DBeamInEffect = false;
                            } else {
                                SparkGameController.Play2DBeamInEffect = true;
                            }
                            if (destinationScene == Registry.GameScenes.NanoNexus || destinationScene == Registry.GameScenes.NanoOrb) {
                                SparkGameController.Play2DBeamInEffect = true;
                            }
                        } else {

                            if (nanopointData.SkipsEpisodeTitle) Wormhole.skipEpisodeTitle = true;
                            DataLoader.instance.enterScene("none", Registry.GameScenes.SparkGame, 0.4f, 0.4f);
                            SparkGameController.Play2DBeamInEffect = true;
                            SparkGameController.SceneWhereSparkGameEntered = SceneManager.GetActiveScene().name;
                            SparkGameController.DoorToMoveToIfLoseSparkGame = nanopointData.loseSparkgameDoor;
                        }
                        print("Spark destination set to: " + destinationObjectName + " in " + destinationScene.ToString());
                    } else if (isPicoPoint) {
                        SparkGameController.SparkGameDestObjectName = destinationObjectName;
                        SparkGameController.SparkGameDestScene = destinationScene;
                        SparkGameController.ReturnFrom2D_SourceScene = HF.SceneNameToEnum(SceneManager.GetActiveScene().name);
                        Wormhole2D.ReturningFromPico = true;
                        Wormhole2D.nextTintName = nanopointData.picoColorID;
                        DataLoader.instance.enterScene("none", Registry.GameScenes.Wormhole2D, 0.2f, 0.2f);
                    } else if (exitsToWormholeFrom2D) {
                        // note this code is copy pasted to 'cleandb' in DA2
                        SparkGameController.ReturnFrom2D_SourceScene = HF.SceneNameToEnum(SceneManager.GetActiveScene().name);
                        DataLoader.instance.enterScene("none", Registry.GameScenes.Wormhole, 0.4f, 0.4f);
                        SparkGameController.SparkGameDestObjectName = destinationObjectName;
                        SparkGameController.SparkGameDestScene = destinationScene;
                        Wormhole.ReturningFrom2D = true;
                    } else if (goesToCurrentScene) {
                        DataLoader.instance.enterScene(destinationObjectName, SceneManager.GetActiveScene().name,0.4f,0.4f);
                    } else {
                        if (IsHorror) {
                            if (destinationObjectName.IndexOf(",") != -1) {
                                string[] dests = destinationObjectName.Split(',');
                                if (DataLoader._getDS("horrorbgstate") == 0) {
                                    destinationObjectName = dests[0];
                                } else {
                                    destinationObjectName = dests[1];
                                }
                            }
                            GargoyleChase.stopincremenitingrandomval = false;
                            DataLoader.instance.enterScene(destinationObjectName, destinationScene, 0.13f, 0.13f);
                        } else {
                            DataLoader.instance.enterScene(destinationObjectName, destinationScene, 0.4f, 0.4f);
                        }
                    }


                    mode = 2;
                }
            }

        }


        private void OnTriggerEnter(Collider other) {
            if (dontAllowMoreTriggering) {
                print("OnTriggerEnter Door - ignoring entertrigger");
                return;
            }
            if (isCurrentlyEnterable && other.CompareTag("Player") && other.name == "MediumPlayer") {
                if (!isANanoDoor && !entersOnOverlap) ui.setTalkAvailableIconVisibility(true,3);
                mode = 1;
            }
            if (isCurrentlyEnterable && other.name == "BigPlayer" && entersOnOverlap) {
                MedBigCam.forceRidescaleNextScene = true;
                mode = 1;
            }
        }
        private void OnTriggerEnter2D(Collider2D other) {
            if (dontAllowMoreTriggering) {
                print("OnTriggerEnter2D Door - ignoring entertrigge");
                return;
            }
            if (isCurrentlyEnterable && other.CompareTag("Player")) {
                mode = 1;
                if (isNexusPad) {
                    GetComponent<SpriteAnimator>().Play("flash-on");
                    AudioHelper.instance.playOneShot("menu_selectano1");
                }
                if (entersOnOverlap == false) {
                    ui2d.setTalkAvailableIconVisibility(true);
                    if (exitsToWormholeFrom2D) {
                        mode = 10;
                    }
                }
            }
        }

        bool yesNoDoor_Talked = false;

        private void OnTriggerExit2D(Collider2D other) {
            if (yesNoDoor_Talked) return;
            if (mode != 2 && isCurrentlyEnterable && other.CompareTag("Player")) {
                mode = 0;

                if (isNexusPad) {
                    GetComponent<SpriteAnimator>().Play("off");
                }
                if (entersOnOverlap == false) {
                    if (!gameObject.activeInHierarchy) return;
                    ui2d.setTalkAvailableIconVisibility(false);
                }
            }
        }

        private void OnTriggerExit(Collider other) {
            if (mode != 2 && isCurrentlyEnterable && !entersOnOverlap && other.CompareTag("Player")) {
                if (!isANanoDoor) ui.setTalkAvailableIconVisibility(false,3);
                mode = 0;
            }
        }
    }
}
