using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Rewired;

public class MyInput : MonoBehaviour {

	public static MyInput o;
    public static float joyMoveThreshold = 0.2f;

	public static bool invertX = false;
	public static bool invertY = false;
    public static bool invertYMove = false;
    public static bool useMouse = false;

	public static bool controllerActive = false;

    public static KeyCode KC_talk; // Serialize
    public static bool talk;
    public static bool jpTalk;


    public static bool jpHome = false;

	public static KeyCode KC_cancel;
	public static bool cancel;
	public static bool jpCancel;

	public static KeyCode KC_right;
	public static KeyCode KC_left;
	public static KeyCode KC_down;
	public static KeyCode KC_up;
	public static KeyCode KC_pause;

    public static KeyCode KC_camtoggle;

	public static KeyCode KC_jump;
    public static KeyCode KC_special;
    public static KeyCode KC_toggleRidescale;

    public static KeyCode KC_zoomIn;
    public static KeyCode KC_zoomOut;

    public static bool zoomIn;
    public static bool zoomOut;

    public static bool down;
	public static bool jpDown;
	public static bool jpUp;
	public static bool jrUp;
	public static bool up;
	public static bool left;
	public static bool jpLeft;
	public static bool jpRight;
	public static bool right;
    public static bool anyDir;

    static int jpUpMode = 0;
    static int jpDownMode = 0;
    static int jpRightMode = 0;
    static int jpLeftMode = 0;

    public static float moveX;
	public static float moveY;
	public static float camY;
    public static float camX;


	public static bool camRight;
	public static bool camLeft;


	public static bool jpPause;
	public static bool pause;

	public static bool camUp;
	public static bool camDown;
    public static bool jpCamToggle;

	public static bool shortcut;

	public static bool jump;
	public static bool jpJump;
	public static bool jrJump;

    public static bool confirm;
    public static bool jpConfirm;

    public static bool jpSpecial;
    public static bool special;
    public static bool jpToggleRidescale;
    public static bool toggleRidescale;

    public static bool jpRidescaleAccel;
    public static bool ridescaleAccel;
    public static bool jrRidescaleAccel;

    public static bool jpR3 = false;

    //public static Player rewiredPlayer;
    

	void Start () {
		if (o != null ) {
			Destroy(gameObject);
            return;
		} else {
			o = this;
		}

		DontDestroyOnLoad(this);
	}

    // System: Confirm, Cancel, Pause
    // Game: Jump/Confirm, Suck/Spark/Cruise, Talk, Cancel/Reverse, RidescaleToggle, CamToggle 

	void Awake () {

        if (o == null) {
            KC_zoomIn = KeyCode.E;
            KC_zoomOut = KeyCode.R;

            KC_cancel = KeyCode.Z;
            KC_jump = KeyCode.X;
            KC_special = KeyCode.C;
            KC_talk = KeyCode.Space;

            KC_pause = KeyCode.Return;

            KC_camtoggle = KeyCode.Q;
            KC_toggleRidescale = KeyCode.Tab;

            KC_up = KeyCode.UpArrow;
            KC_right = KeyCode.RightArrow;
            KC_down = KeyCode.DownArrow;
            KC_left = KeyCode.LeftArrow;
        }

        if (o == null) {
            /* REWIRED_OPENSOURCE
            print("Subscribing to ReWired events.");
             rewiredPlayer = ReInput.players.GetPlayer(0);
            ReInput.ControllerConnectedEvent += OnControllerConnected;
            ReInput.ControllerDisconnectedEvent += OnControllerDisconnected;
            ReInput.ControllerPreDisconnectEvent += OnControllerPreDisconnect;*/
        }

    }

    public static int activeControllers = 0;
    /* REWIRED_OPENSOURCE
    void OnControllerConnected(ControllerStatusChangedEventArgs args) {
        Debug.Log("A controller was connected! Name = " + args.name + " Id = " + args.controllerId + " Type = " + args.controllerType);
        if (args.name.ToLower().IndexOf("dualshock") != -1) alwaysUsePS4 = true;
        activeControllers++;
    }

    void OnControllerDisconnected(ControllerStatusChangedEventArgs args) {
        Debug.Log("A controller was disconnected! Name = " + args.name + " Id = " + args.controllerId + " Type = " + args.controllerType);
        activeControllers--;
        if (DataLoader.instance != null) {
            DataLoader.instance.forcePause = true;
        }
        if (activeControllers < 0) activeControllers = 0;
    }
    // This function will be called when a controller is about to be disconnected
    // You can get information about the controller that is being disconnected via the args parameter
    // You can use this event to save the controller's maps before it's disconnected
    void OnControllerPreDisconnect(ControllerStatusChangedEventArgs args) {
        Debug.Log("A controller is being disconnected! Name = " + args.name + " Id = " + args.controllerId + " Type = " + args.controllerType);
    }

    void OnDestroy() {
        ReInput.ControllerConnectedEvent -= OnControllerConnected;
        ReInput.ControllerDisconnectedEvent -= OnControllerDisconnected;
        ReInput.ControllerPreDisconnectEvent -= OnControllerPreDisconnect;
    }
        */


    public bool kbHasPriority = false;
    public bool hadControllerInput = false;
    public static bool jpConfirmCONTROLLER = false;
	void Update () {

        zoomIn = zoomOut = false;
		confirm = jpConfirm = pause = jpPause = cancel = jpCancel = jump = jpJump = jrJump = false;
        jpConfirmCONTROLLER = false;
        jpTalk = talk = false;
		up = right = down = left = jpUp = jrUp = jpRight = jpDown = jpLeft = false;
		camUp = camRight = camDown = camLeft = false;
        jpCamToggle = jpSpecial = special = toggleRidescale =  jpToggleRidescale = false;
		moveX = moveY = 0;
		camY = 0;
        camX = 0;
        jpR3 = false;


        controllerActive = false;
		/*REWIRED_OPENSOURCE
		 if (!SaveManager.controllerDisable && activeControllers > 0) {
            controllerActive = true;

            #region controllerDetection REGION
            jump = rewiredPlayer.GetButton("JumpConfirmAccel");
            jpJump = jpConfirmCONTROLLER = rewiredPlayer.GetButtonDown("JumpConfirmAccel");
			jrJump = rewiredPlayer.GetButtonUp("JumpConfirmAccel");
            talk = rewiredPlayer.GetButton("Talk");
            jpTalk = rewiredPlayer.GetButtonDown("Talk");
            pause = rewiredPlayer.GetButton("Pause");
            jpPause = rewiredPlayer.GetButtonDown("Pause");
            cancel = rewiredPlayer.GetButton("CancelSkipReverse");
            jpCancel = rewiredPlayer.GetButtonDown("CancelSkipReverse");
            toggleRidescale= rewiredPlayer.GetButton("RideScale");
            jpToggleRidescale = rewiredPlayer.GetButtonDown("RideScale");
            special = rewiredPlayer.GetButton("Special");
            jpSpecial= rewiredPlayer.GetButtonDown("Special");
            zoomIn = rewiredPlayer.GetAxis("ZoomIn") > 0.1f;
            zoomOut = rewiredPlayer.GetAxis("ZoomOut") > 0.1f;
            jpHome = rewiredPlayer.GetButtonDown("Home");
            jpR3 = rewiredPlayer.GetButtonDown("R3");

            if (SaveManager.invertConfirmCancel) {
                confirm = jump = cancel;
                jpConfirm = jpConfirmCONTROLLER = jpJump = jpCancel;
                jrJump = rewiredPlayer.GetButtonUp("CancelSkipReverse");

                cancel = rewiredPlayer.GetButton("JumpConfirmAccel");
                jpCancel = rewiredPlayer.GetButtonDown("JumpConfirmAccel");

                talk = special;
                jpTalk = jpSpecial;

                special = rewiredPlayer.GetButton("Talk");
                jpSpecial = rewiredPlayer.GetButtonDown("Talk");

            }

            if (invertY) {
                camDown= rewiredPlayer.GetAxis("CamVert") > 0.1f;
                camUp = rewiredPlayer.GetAxis("CamVert") < -0.1f;
            } else {
                camUp = rewiredPlayer.GetAxis("CamVert") > 0.1f;
                camDown = rewiredPlayer.GetAxis("CamVert") < -0.1f;
            }
            if (invertX) {
                camLeft = rewiredPlayer.GetAxis("CamHor") > 0.1f;
                camRight = rewiredPlayer.GetAxis("CamHor") < -0.1f;
            } else {
                camRight = rewiredPlayer.GetAxis("CamHor") > 0.1f;
                camLeft = rewiredPlayer.GetAxis("CamHor") < -0.1f;
            }

            jpCamToggle = rewiredPlayer.GetButtonDown("CamToggle");

            camX = rewiredPlayer.GetAxis("CamHor");
            if (invertX) camX *= -1;
            camY = -rewiredPlayer.GetAxis("CamVert");
            if (invertY) camY *= -1;
            // add the negative sign so by default we have the normal axis (up is look up, etc)

            float hor = rewiredPlayer.GetAxis("MoveHor");
            float vert = rewiredPlayer.GetAxis("MoveVert");

            if (rewiredPlayer.GetButton("DU")) {
                vert = 1f;
            } else if (rewiredPlayer.GetButton("DD")) {
                vert = -1f;
            }

            if (invertYMove) vert *= -1;

            if (rewiredPlayer.GetButton("DR")) {
                hor = 1f;
            } else if (rewiredPlayer.GetButton("DL")) {
                hor = -1f;
            }

            left =  hor < -0.3f;
            right = hor  > 0.3f;

            down = vert < -0.3f;
            up = vert > 0.3f;

            if (jpLeftMode == 0 && hor < -0.9f) {
                jpLeft = true;
                jpLeftMode = 1;
            } else if (jpLeftMode == 1 && !left) jpLeftMode = 0;


            if (jpRightMode == 0 && hor > 0.9f) {
                jpRight = true;
                jpRightMode = 1;
            } else if (jpRightMode == 1 && !right) jpRightMode = 0;

            if (jpUpMode == 0 && vert > 0.9f) {
                jpUp = true;
                jpUpMode = 1;
            } else if (jpUpMode == 1 && !up) {
                jpUpMode = 0;
                jrUp = true;
            }

            if (jpDownMode == 0 && vert < -0.9f) {
                jpDown= true;
                jpDownMode= 1;
            } else if (jpDownMode == 1 && !down) jpDownMode = 0;

            moveX = hor;
            moveY = vert;

            #endregion


            hadControllerInput = false;
            if (left || right || up || down || camLeft || zoomOut || zoomIn ||camRight || camUp || camDown || jpJump || jpCancel || jpSpecial ||jpTalk || jpToggleRidescale || jpCamToggle || jpPause) {
                hadControllerInput = true;
                if (kbHasPriority) {
                    kbHasPriority = false;
                    print("Controller has priority");
                }
            }
            // When controller plugged in, controllerActive is always set to true here.
            // But it can be set to false if controller had no input, and if there is key input below
		}*/
        #region kbDetection REGION
        cancel |= Input.GetKey(KC_cancel);
		pause |= Input.GetKey(KC_pause)|| Input.GetKey(KeyCode.Escape);
		jump |= Input.GetKey(KC_jump);
        special |= Input.GetKey(KC_special);
        talk |= Input.GetKey(KC_talk);
        toggleRidescale |= Input.GetKey(KC_toggleRidescale);

        jpTalk |= Input.GetKeyDown(KC_talk);
        jpSpecial |= Input.GetKeyDown(KC_special);
        jpToggleRidescale |= Input.GetKeyDown(KC_toggleRidescale);
		jpCancel |= Input.GetKeyDown(KC_cancel);
		jpPause |= Input.GetKeyDown(KC_pause) || Input.GetKeyDown(KeyCode.Escape);
		jpJump |= Input.GetKeyDown(KC_jump);
		jrJump |= Input.GetKeyUp(KC_jump);

        if (SaveManager.invertConfirmCancel && controllerActive) {
            confirm |= Input.GetKey(KC_jump);
            jpConfirm |= Input.GetKeyDown(KC_jump);
        } else {
            confirm = jump;
            jpConfirm = jpJump;
        }

		// If there is no controller input, then allow the keyboard to change the movement values used to move the player
		if (!up && !down) {
			if (Input.GetKey(KC_up)) {
				moveY = 1;
			} else if (Input.GetKey(KC_down)) {
				moveY = -1;
			}
		}

		if (!left && !right && !camLeft && !camRight) {
			if (Input.GetKey(KC_left)) {
				camX = -1;
			} else if (Input.GetKey(KC_right)) {
				camX = 1;
			}
		}


		up |= Input.GetKey(KC_up);
		down |= Input.GetKey(KC_down);
		left |= Input.GetKey(KC_left);
		right |= Input.GetKey(KC_right);

		jpUp |= Input.GetKeyDown(KC_up);
		jpDown |= Input.GetKeyDown(KC_down);
		jpLeft |= Input.GetKeyDown(KC_left);
		jpRight |= Input.GetKeyDown(KC_right);
		jrUp|= Input.GetKeyUp(KC_up);


        anyDir = false;
        if (up || down || right || left) {
            anyDir = true;
        }


        jpCamToggle |= Input.GetKeyDown(KC_camtoggle);

        ridescaleAccel = jump;
        jpRidescaleAccel= jpJump;
        jrRidescaleAccel = jrJump;

        zoomIn |= Input.GetKey(KC_zoomIn);
        zoomOut |= Input.GetKey(KC_zoomOut);

        #endregion

        if (!hadControllerInput && !kbHasPriority) {
            if (left || right || up || down || zoomIn || zoomOut || jpJump || jpCancel || jpSpecial || jpTalk || jpToggleRidescale || jpCamToggle || jpPause) {
                kbHasPriority = true;
                print("KB has priority");
            }
        }
        if (kbHasPriority) {
            controllerActive = false;
        }

        shortcut = false;
		if (Registry.DEV_MODE_ON) {
			shortcut = Input.GetKey(KeyCode.LeftControl);
		}

	}

    public static string getTimeString(int seconds) {
        string s = ""; // 6059
        int sec = seconds % 60; // 59
        seconds -= sec; // 6000
        int min = seconds / 60; // 100
        min = min % 60; // 40
        int hour = seconds - min * 60; // 1 = 6000 - 40*60
        hour /= 3600;
        if (hour > 99) {
            s = string.Format("{0:D4}:{1:D2}:{2:D2}", hour,min,sec); 
        } else {
            s = string.Format("{0:D2}:{1:D2}:{2:D2}", hour, min, sec);
        }
        return s;
    }

    public static bool tagForcePS4 = false;
    public static bool tagForceXB1 = false;
    public static bool tagForceSwitch = false;
    static bool alwaysUsePS4 = false;
    static bool alwaysUseSwitch = false;
	public static string replaceTags(string s) {


        s = s.Replace("{RIGHTICON}", "<sprite=20>");
        s = s.Replace("{LEFTICON}", "<sprite=21>");
        s = s.Replace("{DOWNICON}", "<sprite=22>");
        s = s.Replace("{UPICON}", "<sprite=23>");
        if (s.IndexOf("{TIME}") != -1) {
            s = s.Replace("{TIME}", getTimeString((int) SaveManager.playtime));
        }
        s = s.Replace("{COINS}", SaveManager.totalFoundCoins.ToString());

        tagForcePS4 = tagForceXB1 = tagForceSwitch = false;
        if (SaveManager.buttonLabelType == 1) {
            tagForcePS4 = true;
        } else if (SaveManager.buttonLabelType == 2) {
            tagForceXB1 = true;
        } else if (SaveManager.buttonLabelType == 3) {
            tagForceSwitch = true;
        }

        if (!SaveManager.controllerDisable && activeControllers > 0) {
            s = s.Replace("{CAMERA}", DataLoader.instance.getRaw("controlLabels", 3));
            s = s.Replace("{MOVEMENT}", DataLoader.instance.getRaw("controlLabels", 0));
            // 9-12, triangle, circle, cross, Square

            bool isPS4 = false;
            bool isSwitch = false;
            if (alwaysUsePS4 && SaveManager.buttonLabelType == 0) isPS4 = true;
            if (alwaysUseSwitch && SaveManager.buttonLabelType == 0) isSwitch = true;
            /* REWIRED_OPENSOURCE
            if (activeControllers > 0 && !SaveManager.controllerDisable && rewiredPlayer.controllers.GetLastActiveController() != null && rewiredPlayer.controllers.GetLastActiveController().name.ToLower().IndexOf("dualshock") != -1) {
                if (SaveManager.buttonLabelType == 0) {
                    isPS4 = true;
                    alwaysUsePS4 = true;
                }
            } else if (activeControllers > 0 && !SaveManager.controllerDisable && rewiredPlayer.controllers.GetLastActiveController() != null) {
                string controlname = rewiredPlayer.controllers.GetLastActiveController().name;
                if (controlname.IndexOf("Joy-Con") != -1 || controlname.IndexOf("Nintendo Switch") != -1 || controlname.IndexOf("Pro Controller") != -1) {
                    if (SaveManager.buttonLabelType == 0) {
                        isSwitch = true;
                        alwaysUseSwitch = true;
                    }
                }
            }*/
            if (SaveManager.invertConfirmCancel) {
                s = s.Replace("{TALK}", "{SPECIAL2}");
                s = s.Replace("{CANCEL}", "{CONFIRM2}");
                s = s.Replace("{CONFIRM}", "{CANCEL}");
                s = s.Replace("{JUMP}", "{CANCEL}");
                s = s.Replace("{SPECIAL}", "{TALK}");
                s = s.Replace("{SPECIAL2}", "{SPECIAL}"); 
                s = s.Replace("{CONFIRM2}", "{CONFIRM}"); 
            }

            if (tagForcePS4 || isPS4) {
                s = s.Replace("{TALK}", "<sprite=1>"); // TopFace Triangle/Y
                s = s.Replace("{CANCEL}", "<sprite=2>");  // Circle/B
                s = s.Replace("{CONFIRM}", "<sprite=3>"); // Cross/A
                s = s.Replace("{JUMP}", "<sprite=3>");
                s = s.Replace("{SPECIAL}", "<sprite=0>"); // Square/X
                s = s.Replace("{PAUSE}", DataLoader.instance.getRaw("controlLabels", 20)); // options
                s = s.Replace("{RIDESCALE}", "<sprite=5>"); // r1
                s = s.Replace("{ZOOMOUT}", "<sprite=7>"); // R r2
                s = s.Replace("{ZOOMIN}", "<sprite=6>"); // E l2
            } else if (tagForceSwitch || isSwitch) {
                s = s.Replace("{TALK}", "<sprite=18>"); // TopFace Triangle/X
                s = s.Replace("{CANCEL}", "<sprite=8>");  // Circle/A
                s = s.Replace("{CONFIRM}", "<sprite=9>"); // Cross/B
                s = s.Replace("{JUMP}", "<sprite=9>");
                s = s.Replace("{SPECIAL}", "<sprite=19>"); // Square/Y
                s = s.Replace("{PAUSE}", DataLoader.instance.getRaw("controlLabels", 22)); // options
                s = s.Replace("{RIDESCALE}", "<sprite=25>"); // r1
                s = s.Replace("{ZOOMOUT}", "<sprite=27>"); // R r2
                s = s.Replace("{ZOOMIN}", "<sprite=26>"); // E l2
            } else {
                s = s.Replace("{TALK}", "<sprite=13>"); // Y
                s = s.Replace("{CANCEL}", "<sprite=11>"); // B
                s = s.Replace("{CONFIRM}", "<sprite=10>"); // A
                s = s.Replace("{JUMP}", "<sprite=10>");
                s = s.Replace("{SPECIAL}", "<sprite=12>"); // X
                s = s.Replace("{PAUSE}", DataLoader.instance.getRaw("controlLabels", 13)); // menu
                s = s.Replace("{RIDESCALE}", "<sprite=15>"); // RB
                s = s.Replace("{ZOOMIN}", "<sprite=16>"); // E lt
                s = s.Replace("{ZOOMOUT}", "<sprite=17>"); // R rt
            }

            s = s.Replace("{ZOOMINK}", replaceKeyCodeNames(KC_zoomIn));
            s = s.Replace("{ZOOMOUTK}", replaceKeyCodeNames(KC_zoomOut));
            s = s.Replace("{CAMTOGGLE}", "---");
            s = s.Replace("{CAMTOGGLEK}", replaceKeyCodeNames(KC_camtoggle));

            return s;
		}

        // confirm, cancel, pause, up down left right, camera_up, camera_down
        // camera, movement

        s = s.Replace("{CAMERA}", DataLoader.instance.getRaw("controlLabels", 2));
        s = s.Replace("{MOVEMENT}", DataLoader.instance.getRaw("controlLabels", 1));
        s = s.Replace("{CONFIRM}",replaceKeyCodeNames(KC_jump));
		s = s.Replace("{CANCEL}",replaceKeyCodeNames(KC_cancel));
		s = s.Replace("{PAUSE}",replaceKeyCodeNames(KC_pause));
        s = s.Replace("{JUMP}", replaceKeyCodeNames(KC_jump));
        s = s.Replace("{SPECIAL}", replaceKeyCodeNames(KC_special));
        s = s.Replace("{RIDESCALE}", replaceKeyCodeNames(KC_toggleRidescale));
        s = s.Replace("{TALK}", replaceKeyCodeNames(KC_talk));

        s = s.Replace("{ZOOMINK}", replaceKeyCodeNames(KC_zoomIn));
        s = s.Replace("{ZOOMOUTK}", replaceKeyCodeNames(KC_zoomOut));

        s = s.Replace("{CAMTOGGLEK}", replaceKeyCodeNames(KC_camtoggle));
        s = s.Replace("{CAMTOGGLE}", replaceKeyCodeNames(KC_camtoggle));

        s = s.Replace("{UP}",replaceKeyCodeNames(KC_up));
		s = s.Replace("{RIGHT}",replaceKeyCodeNames(KC_right));
		s = s.Replace("{DOWN}",replaceKeyCodeNames(KC_down));
		s = s.Replace("{LEFT}",replaceKeyCodeNames(KC_left));




        return s;
	}

	public static string replaceKeyCodeNames(KeyCode k) {
		if (k == KeyCode.UpArrow) return DataLoader.instance.getRaw("controlLabels",4);
		if (k == KeyCode.RightArrow) return DataLoader.instance.getRaw("controlLabels",5);
		if (k == KeyCode.DownArrow) return DataLoader.instance.getRaw("controlLabels",6);
		if (k == KeyCode.LeftArrow) return DataLoader.instance.getRaw("controlLabels",7);
		return k.ToString();
	}
}
