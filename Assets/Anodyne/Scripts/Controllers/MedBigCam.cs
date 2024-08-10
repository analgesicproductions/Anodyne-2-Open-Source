using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MedBigCam : MonoBehaviour {

    public bool mediumOnly = false;

    bool lerpCamPos = false;
	Vector3 posFromTrigger;
	Vector3 STARTposFromTrigger;
	Vector3 rotFromTrigger;
	Vector3 STARTrotFromTrigger;
	Camera cam;
    DialogueBox dbox;
    Cinemachine.CinemachineBrain brain;
    bool isHurt = false;

    int layermask = 1 | (1 << 11) | (1 << 17);

    // Set by doors so camera is rotated correctly when transitioning
    public static float initialCameraRotation = 0f;

	float triggerTransitionTime;
	float maxTTT;


    public static bool inCinemachineMovieMode = false;
	public bool fixedFollow = false; // Set this to have the camera stay in one spot and track the target
	public bool fixedFollowTopDown = false; // Set this to have the camera follow the player like at opdown cam
	Vector3 camXZ;
	Vector3 playerXZ;

//	SceneData sceneData;

	GameObject target;
	public float rotateSpeed = 320;
	public float bigRotateSpeed = 320;
	Vector3 offset;
    Vector3 tempCamPos = new Vector3();
    [Header("Camera Offsets")]
    [Tooltip("How much the camera looks below the player during a jump")]
    public float mediumYOff = 1.7f;
    [Tooltip("Default downwards angle")]
    public Vector3 mediumOffset;
    [Tooltip("A far, downwards angle")]
    public Vector3 mediumOffsetFar;
    [Tooltip("Default upwards angle")]
    public Vector3 mediumOffsetUp;
    public Vector3 mediumOffsetTopDown;

    public Vector3 bigOffset;
    public Vector3 bigOffsetLow;
    int offsetMode = 0;

    GameObject MediumPlayer;
    MediumControl MediumPlayerControl;
    MediumControl BigPlayerControl;
    GameObject BigPlayer;
    int sizeMode = 0;

    public float skyboxRotateSpeed = 0;

    public MediumControl GetMediumControl() {
        return MediumPlayerControl;
    }

    public MediumControl GetBigPlayerControl() {
        return BigPlayerControl;
    }

    private void Awake() {

        setCameraEulerYRotation(initialCameraRotation);
        initialCameraRotation = 0;
    }
    // Use this for initialization
    void Start () {
        brain = GetComponent<Cinemachine.CinemachineBrain>();
        isHurt = DustDropPoint.IsDisillusioned();
        offset = mediumOffset * SaveManager.cameraDistance / 100f;
        lerpOffsetCurrentVector = offset;
        offsetMode = 0;

        sizeMode = 0;
        camXZ = new Vector3();
        playerXZ = new Vector3();

        playershadow = GameObject.Find("PlayerShadow").GetComponent<PlayerShadowHelper>();
        dbox= GameObject.Find("Dialogue").GetComponent<DialogueBox>();

        string scenename = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (scenename == "Albumen" || scenename == "RingCave" || scenename == "DesertSpireCave" || scenename == "DesertSpireTop" || scenename == "CougherHome" ||scenename == "CenterChamber" || scenename == "CenterChamber CamTest" || scenename == "CenterChamberEndingWatch" || scenename == "GrowthChapel") {
            mediumOnly = true;
        }
        if (mediumOnly) {
            forceRidescaleNextScene = false;
        }
        if (mediumOnly == false) {
            BigPlayer = GameObject.Find("BigPlayer");
            BigPlayerControl = BigPlayer.GetComponent<MediumControl>();

            BigPlayer.SetActive(false);
        }
        MediumPlayer = GameObject.Find("MediumPlayer");
        MediumPlayerControl = MediumPlayer.GetComponent<MediumControl>();
        target = MediumPlayer;

        camXZ = new Vector3();
		playerXZ = new Vector3();

        curXOrbitAngle = 0f;

		if (fixedFollow == false) {
			transform.position = target.transform.position + offset;
		}
		targetPos = transform.position;

		//sceneData = GameObject.Find("SceneData").GetComponent<SceneData>();
		cam = GetComponent<Camera>();
        cam.fieldOfView = SaveManager.fieldOfView;
	}
	float target_yRotationAroundTarget = 0f;
    float cur_yRotationAroundTarget = 0f;
    float rs_extraTilt = 0;
	float curXOrbitAngle = 0f;

    public float lowerVertAngLimRidescale = -5f;
	public float lowerVertAngleLimit = -20f;
	public float upperVertAngleLimit = 40f;
	float verticalCamSpeedMultiplier = 0.32f;
	float horizontalCamSpeedMultiplier = 0.43f;
    bool jumpCam_HasFollowedFaster = false;

	Vector3 targetPos;
	Vector3 nextTargetPos;
    public static bool forceRidescaleNextScene = false;


	void switchModeInitialization(bool _spline, bool _fixed, float _t) {
		fixedFollow = _fixed; 
		triggerTransitionTime = 0; 
		maxTTT = _t; 
		lerpCamPos = true;
	}

    public void smoothLerpFromVCToNormal(float _transitionTime) {
        triggerTransitionTime = 0;
        maxTTT = _transitionTime;
        lerpCamPos = true;
        STARTposFromTrigger = transform.position;
        STARTrotFromTrigger = transform.eulerAngles;
    }

	public void switchToFollow(float _transitionTime) {
		if (!fixedFollow) return;

		switchModeInitialization(false,false,_transitionTime);
		STARTposFromTrigger = transform.position;
		STARTrotFromTrigger = transform.eulerAngles;
	}

	// Makes the camera transition to a fixed position view, lerping via SmoothStep for _transitionTime seconds.
	public void switchToFixed(Vector3 _newCamPos, float _transitionTime) {

		switchModeInitialization(false,true,_transitionTime);
		// position given from the trigger - destination of the camera.
		posFromTrigger = _newCamPos;
		STARTposFromTrigger = transform.position;
	}


    float t_hurtMode = 0;
    static float GetFieldOfView() {
        return SaveManager.fieldOfView;
    }
    int noRidescaleTicks = 30;

    PlayerShadowHelper playershadow;
	bool didInit = false;
    [System.NonSerialized]
    public bool waitingForSizeSwitchConfirmation = false;
    public bool EXT_FORCE_WALKSCALE = false;

    void LateUpdate() {
        if (noRidescaleTicks > 0) noRidescaleTicks--;
        if (noRidescaleTicks > 0) {
            MyInput.jpToggleRidescale = false;
        }

        if (mediumOnly && MediumControl.ignoreMedInput) {
            MediumControl.ignoreMedInput = false;
        }
        if (skyboxRotateSpeed != 0) {
            float skyRot = RenderSettings.skybox.GetFloat("_Rotation");
            RenderSettings.skybox.SetFloat("_Rotation", skyRot + skyboxRotateSpeed*Time.deltaTime);
        }

        if (brain.enabled) {
            fixedFollow = true;
        } else {
            fixedFollow = false;
        }
		if (inCinemachineMovieMode || CutsceneManager.deactivateCameras || SaveModule.saveMenuOpen || DataLoader.instance.isPaused) {
			return;
		}

		if(!didInit) {
			didInit = true;
			targetPos = target.transform.position; // Camera follows this
			// target always LERPs to this, nextTarget only updates when player leaves deadzone
			nextTargetPos = target.transform.position; 
		}

        if (DialogueAno2.AnyScriptIsParsing) MyInput.jpToggleRidescale = false;
        if (!dbox.isDialogFinished()) MyInput.jpToggleRidescale = false;
        // Handle switching between following walk and ridescale
        if (sizeMode == 0) { // Medium
            if (forceRidescaleNextScene) {
                forceRidescaleNextScene = false;
                target = BigPlayer;
                STARTrotFromTrigger = transform.eulerAngles;
                STARTposFromTrigger = transform.position;
                BigPlayerControl.accelTime = MediumControl.carAccelTimeOnSceneExit;
                MediumControl.carAccelTimeOnSceneExit = 0;
                BigPlayer.transform.eulerAngles = new Vector3(0, MediumPlayer.transform.localEulerAngles.y, 0); // rotate to current camera, NOT player
                BigPlayerControl.setBigPlayerStartPosBasedOnMediumPlayer(MediumPlayer);
                cam.fieldOfView = GetFieldOfView();
                setCameraEulerYRotation(BigPlayer.transform.localEulerAngles.y);
                Vector3 vvv = transform.localEulerAngles;
                vvv.y = target_yRotationAroundTarget;
                transform.localEulerAngles = vvv;
                offsetMode = 0; offset = bigOffset;
                lerpOffsetCurrentVector = offset * SaveManager.cameraDistance / 100f;
                t_JumpReturn = tm_JumpReturn; jumpCamMode = 0; inJumpCam = false;
                MediumPlayer.SetActive(false); BigPlayer.SetActive(true); playershadow.gameObject.SetActive(false);
                sizeMode = 1;
            }

            if (waitingForSizeSwitchConfirmation) {
                BigPlayerControl.setBigPlayerStartPosBasedOnMediumPlayer(MediumPlayer);
                if (MediumPlayerControl.DoneSwitchingSize()) {
                    // If hurt, put player in weak to collapse mode after done switching
                    // player only can enter ridescale while in hurtmode 3 (walking normal speed)
                    if (isHurt) {
                        MediumPlayerControl.hurtMode = 0;
                    }
                    BigPlayerControl.pushbackFromWall();
                    waitingForSizeSwitchConfirmation = false;
                    sizeMode = 1;
                    BigPlayer.SetActive(true);
                    BigPlayerControl.SwitchSizes(false);
                    playershadow.gameObject.SetActive(false);
                    inJumpCam = false;
                    MediumPlayer.SetActive(false);
                }
            } else if (!mediumOnly && (!isHurt || MediumPlayerControl.hurtMode == 3) && MyInput.jpToggleRidescale && !MediumControl.doSpinOutAfterNano && !MediumPlayerControl.OnMovingPlatform) {

                waitingForSizeSwitchConfirmation = true;
                MediumPlayerControl.SwitchSizes(true);
                AudioHelper.instance.playOneShot("ridescaleTransform");
                target = BigPlayer;
                STARTrotFromTrigger = transform.eulerAngles;
                STARTposFromTrigger = transform.position;
                BigPlayerControl.setBigPlayerStartVelBasedOnMed(MediumPlayer.GetComponent<Rigidbody>().velocity);
                if (fixedFollow) {
                    BigPlayer.transform.eulerAngles = new Vector3(0, MediumPlayer.transform.localEulerAngles.y, 0); // rotate to current camera, NOT player
                } else {
                    BigPlayer.transform.eulerAngles = new Vector3(0, transform.localEulerAngles.y, 0); // rotate to current camera, NOT player
                }
                BigPlayerControl.setBigPlayerStartPosBasedOnMediumPlayer(MediumPlayer);
                cam.fieldOfView = GetFieldOfView();
                lerpCamPos = true;
                triggerTransitionTime = 0; maxTTT = 1.5f;
                offsetMode = 0;
                offset = bigOffset;
                lerpOffsetCurrentVector = offset * SaveManager.cameraDistance / 100f;

                t_JumpReturn = tm_JumpReturn;
                jumpCamMode = 0;
            } else {
                offset = mediumOffset;
            }
        } else if (sizeMode == 1) { // Setting Walkscale acertive
            if (isHurt) {
                t_hurtMode += Time.deltaTime;
                if (t_hurtMode > 14f) {
                    t_hurtMode = 0;
                    MyInput.jpToggleRidescale = true;
                }
            }
            offset = bigOffset;
            if (waitingForSizeSwitchConfirmation) {
                if (BigPlayerControl.DoneSwitchingSize()) {
                    waitingForSizeSwitchConfirmation = false;
                    sizeMode = 0;
                    target = MediumPlayer;
                    target.SetActive(true);
                    playershadow.gameObject.SetActive(true);
                    target.GetComponent<Rigidbody>().velocity = Vector3.zero;
                    target.transform.eulerAngles = BigPlayer.transform.eulerAngles;
                    cam.fieldOfView = GetFieldOfView();
                    lerpCamPos = true;
                    triggerTransitionTime = 0; maxTTT = 1.5f;
                    STARTrotFromTrigger = transform.eulerAngles;
                    STARTposFromTrigger = transform.position;
                    MediumPlayerControl.setMediumPlayerStartPosBasedOnBigPlayer(BigPlayer);
                    MediumPlayerControl.SwitchSizes(false);
                    BigPlayerControl.OnReturnToWalkscale();
                    BigPlayer.SetActive(false);
                    offset = mediumOffset;
                    lerpOffsetCurrentVector = offset * SaveManager.cameraDistance/100f;
                }
            } else {
                if (MyInput.jpToggleRidescale || EXT_FORCE_WALKSCALE) {
                    t_hurtMode = 0;
                    if (EXT_FORCE_WALKSCALE) {
                        print("undo force walk");
                    }
                    EXT_FORCE_WALKSCALE = false;
                    AudioHelper.instance.playOneShot("ridescaleTransform");
                    waitingForSizeSwitchConfirmation = true;
                    BigPlayer.GetComponent<Rigidbody>().velocity = Vector3.zero;
                    BigPlayerControl.SwitchSizes(true);
                }
            }
        }

         
		if (fixedFollow) {
			if (lerpCamPos) {
				float _t = Mathf.SmoothStep(0,1,triggerTransitionTime/maxTTT);
				triggerTransitionTime += Time.deltaTime;
				if (triggerTransitionTime > maxTTT) {
					lerpCamPos = false;
				}
				transform.position = Vector3.Lerp(STARTposFromTrigger,posFromTrigger,_t);
			}

			nextTargetPos = target.transform.position;
			targetPos = Vector3.Lerp(targetPos,nextTargetPos,Time.deltaTime*3);

			if (fixedFollowTopDown) {
				camXZ.Set(targetPos.x,transform.position.y,targetPos.z-8);
				playerXZ.Set(targetPos.x,0,targetPos.z); // does nothing
				transform.position = camXZ;
			} 

			transform.LookAt(targetPos);
			return;
		}


        bool holdingForward = false;
        if (sizeMode == 1 && MyInput.ridescaleAccel) holdingForward = true;
        if (sizeMode == 0 && MyInput.moveY > 0) holdingForward = true;
                 
        /* Default Camera behavior */
		nextTargetPos.x = target.transform.position.x;
		nextTargetPos.z = target.transform.position.z;
        // When moving forward/back, slowly move the target position to slightly behind the player based on camera facing vector
        Vector3 camxzflipped = transform.forward;
        camxzflipped.y = 0; camxzflipped *= -1; camxzflipped.Normalize();
        Ray r = new Ray(target.transform.position, camxzflipped); // dont move the target pos through a wall
        if (holdingForward && sizeMode == 0 && !Physics.Raycast(r, 4f)) {
           t_xzCamLag += Time.deltaTime;
        } else {
            t_xzCamLag -= Time.deltaTime;
        }
        if (isHurt) t_xzCamLag = 0;
        float tm_xzCamLag = 1f;
        t_xzCamLag = Mathf.Clamp01(t_xzCamLag/tm_xzCamLag);
        nextTargetPos = Vector3.Lerp(nextTargetPos, nextTargetPos + camxzflipped * 2f, Mathf.SmoothStep(0,1,t_xzCamLag));

       
        targetPos.y = target.transform.position.y;
        if (sizeMode == 0 && !waitingForSizeSwitchConfirmation) targetPos.y += mediumYOff;

        // If in jumping mode camera should work a little differently along y-axis

        if (inJumpCam) {
            if (jumpCamMode == 0) {
                if (jumpCamTargetPos.y - target.transform.position.y < -3) {
                    jumpCamTargetPos.y = target.transform.position.y - 3;
                }
                targetPos = jumpCamTargetPos; // This is set when the player controller tells the camera to enter jump mode.
                // It's fixed to where the player starts the jump (to simulate a deadzone as the player initially jumps)
                float jyo = target.transform.position.y - jumpDeadStartY;
                if (jyo > jumpDeadYMargin || jyo < -1f) { 
                    jumpCamMode = 1;
                    timeJumpHeld = 0;
                    jumpCam_HasFollowedFaster = false;
                    jumpCameraLookYOff_Current = 0;
                }
            } else if (jumpCamMode == 1) {
                // when player is above margin of jump cam movement, make the camera slowly look further below it.
                float jyo = target.transform.position.y - jumpDeadStartY;
                if (jyo > jumpDeadYMargin) { jumpCameraLookYOff_Current += 1.5f*Time.deltaTime; }
                else { jumpCameraLookYOff_Current -= 1.5f*Time.deltaTime;  }
                float playerYVel = target.GetComponent<Rigidbody>().velocity.y;
                jumpCameraLookYOff_Current = Mathf.Clamp(jumpCameraLookYOff_Current, 0, jumpCameraLookYOff);
                bool followFasterBecauseMovingFast = false;
                if (playerYVel > 19f) followFasterBecauseMovingFast = true;
                if (followFasterBecauseMovingFast) {
                    jumpCam_HasFollowedFaster = true;
                }
                targetPos.y -= jumpCameraLookYOff_Current;

                // when bounced up really fast (with Bouncer3D, etc), then quickly lerp to following the player precisely.
                // The player will remain in the HasFollowedFaster state until the player has NOT been moving upwards really  fast for a seconds
                // this guarantees that transitioning to anything with the timeJumpHeld var won't lead to choppy transitions.
                if (jumpCam_HasFollowedFaster) {
                    if (!followFasterBecauseMovingFast) {
                        timeJumpHeld -= Time.deltaTime;
                    } else {
                        timeJumpHeld += Time.deltaTime;
                    }
                    jumpCamTargetPos = Vector3.Lerp(jumpCamTargetPos, targetPos, timeJumpHeld / 1.0f);
                    if (timeJumpHeld <= 0) {
                        jumpCam_HasFollowedFaster = false;
                    }

                    // Using the existing jumpCamTargetPos, we'll slowly lerp it to this new target position, then
                    // set targetPos to it, so the next camera calculations work
                } else if (playerYVel < 0) {
                    timeJumpHeld += Time.deltaTime;
                    jumpCamTargetPos = Vector3.Lerp(jumpCamTargetPos, targetPos, timeJumpHeld / 1.0f);
                } else {
                    // subtrackt timeJumpHeld to zero here so that in a double jump, if returning to th eplayerYVel< 0 block ,there's no choppiness
                    timeJumpHeld -= Time.deltaTime; if (timeJumpHeld < 0) timeJumpHeld = 0;
                    jumpCamTargetPos = Vector3.Lerp(jumpCamTargetPos, targetPos, Time.deltaTime / timerLerpToJumpCamera);
                }

                if (MyInput.shortcut) {
                    jumpCamTargetPos = targetPos;
                }
                targetPos = jumpCamTargetPos;
            } else if (jumpCamMode == 2) {
                // Lerp back from following beneath the foot to back to the player.
                t_JumpReturn += Time.deltaTime;
                if (targetPos.y - jumpCamTargetPos.y > 2f) {
                    t_JumpReturn += 0.88f * Time.deltaTime;
                }
                targetPos.y = Mathf.Lerp(jumpCamTargetPos.y, targetPos.y, t_JumpReturn / tm_JumpReturn);
                if (t_JumpReturn > tm_JumpReturn) {
                    t_JumpReturn = tm_JumpReturn;
                    jumpCamMode = 0;
                    inJumpCam = false;
                }
            }
        }

        // Addendum: If jump mode is on, the x and z positions will be a little messed up from calculations. Snap them to the player.
        targetPos.x = nextTargetPos.x;
		targetPos.z = nextTargetPos.z;


        // Determine the horizontal rotation of camera
        float targetHorizontal = 0;
        float deltaTime = Time.deltaTime;
        if (deltaTime > 0.035f) deltaTime = 0.035f;
        float rotateSpeedTimescaled = rotateSpeed * deltaTime;
        if (sizeMode == 1) rotateSpeedTimescaled = bigRotateSpeed * deltaTime;
        if (MyInput.controllerActive && sizeMode == 1) rotateSpeedTimescaled *= 1.5f;
        
        float camX = MyInput.camX;
        float moveX = MyInput.moveX;
        if (!MyInput.controllerActive && MyInput.left) camX = -1;
        if (!MyInput.controllerActive && MyInput.right) camX = 1;


        // (on keyboard: camera - right arrows and right stick - don't turn the ridescale camera
        if (!MyInput.controllerActive) {
            if (sizeMode == 1 && MyInput.ridescaleAccel) camX = 0;
            if (sizeMode == 1 && BigPlayerControl.getVel() > 1) camX = 0;
        }

        float controllerMoveLeftThreshold = 0.5f;
        // Moving camera with arrow keys and right stick
        if (Mathf.Abs(camX) > 0.1f) {
            targetHorizontal = camX * rotateSpeedTimescaled * horizontalCamSpeedMultiplier;
            targetHorizontal *= (SaveManager.sensitivity / 100f);

         // OR in ridescale with left stick while acceling
        } else if  (sizeMode == 1 && Mathf.Abs(moveX) > 0.15f) {
            if (!MyInput.controllerActive) {
                targetHorizontal = moveX * rotateSpeedTimescaled * horizontalCamSpeedMultiplier;
                targetHorizontal *= (SaveManager.sensitivity / 100f);
            }
        // Move camera with left stick in walkscale
        } else if (Mathf.Abs(MyInput.moveX) > controllerMoveLeftThreshold) {
            rotateSpeedTimescaled = Mathf.Lerp(0, rotateSpeedTimescaled, (Mathf.Abs(MyInput.moveX) - controllerMoveLeftThreshold) / (1 - controllerMoveLeftThreshold));
            if (MyInput.moveX > 0) targetHorizontal = rotateSpeedTimescaled * horizontalCamSpeedMultiplier * (.13f + (SaveManager.extraCamRotWithControllerMoveStrength/100f));
            if (MyInput.moveX < 0) targetHorizontal = -rotateSpeedTimescaled * horizontalCamSpeedMultiplier * (.13f + (SaveManager.extraCamRotWithControllerMoveStrength / 100f));
            targetHorizontal *= 1.6f;
        }

        // when moving diagonally, rotate camera slower or not at all
        if (!MyInput.controllerActive && (MyInput.up || MyInput.down)) {
            targetHorizontal *= 0.85f;
        }

        horizontalRotation = Mathf.Lerp(horizontalRotation, targetHorizontal, deltaTime * 6.5f);
        if (Mathf.Abs(camX) <= 0.2f) {
            horizontalRotation *= 0.875f; // Slow down faster towards zero if nothingh eld
        }
        if (t_YRotLerpR3 > 0) {
            horizontalRotation = 0;
        }

        target_yRotationAroundTarget += horizontalRotation;
        float desiredYAngle = 0f;

        if (sizeMode == 1 && (MyInput.controllerActive || MyInput.ridescaleAccel || MyInput.up || BigPlayerControl.getVel(true) > 1 || t_lerpOffset > 0)) {
            if (!wasJustDriving) {
                // When entering ridescale, model is aligned with player. Thus make sure the camrea doeesn't start wrong
                target_yRotationAroundTarget = 0;
            }
            float rs_extraYRotate = 0;
            if (MyInput.controllerActive) {
                rs_extraYRotate = target_yRotationAroundTarget;
                if (rs_extraYRotate < 0) rs_extraYRotate += 360f;
                if (rs_extraYRotate >= 360f) rs_extraYRotate -= 360f;
            } else {
                if (offsetMode == 0 || offsetMode == 1) rs_extraYRotate = 0;
                if (offsetMode == 2) rs_extraYRotate = 90f;
                if (offsetMode == 3) rs_extraYRotate = 165f;
            }
            if (BigPlayerControl.getVel(true) > 1 && t_YRotLerpR3 <= 0 && MyInput.controllerActive && (rs_extraYRotate < 25f || rs_extraYRotate >= 335f)) {
                rs_extraYRotate = Mathf.LerpAngle(rs_extraYRotate, 0, Time.deltaTime * 3f);
                target_yRotationAroundTarget = rs_extraYRotate;
            }

            // Since orbiting is free in controller mode, the player controls an angle offset (relative to behind the car). 
            // Since as the car turns, IT rotates its model, to keep the proper camera perspective, rs_extraYrotate must be set here.
            // It's the same idea as how setting the value above to 90 or 165 lets you move the car normally but fixes an angle
            if (t_lerpOffset > 0) {
                cur_yRotationAroundTarget = Mathf.LerpAngle(cur_yRotationAroundTarget, target.transform.eulerAngles.y + rs_extraYRotate, 1 - ((t_lerpOffset + Time.deltaTime) / tm_lerpOffset));
            } else {
                if (MyInput.controllerActive) {
                    // When turning left/right, don't move the cam as quickly to the desired rotation so you can still sort of see the side of the car as it moves
                    if (t_YRotLerpR3 > 0) {
                    } else if (Mathf.Abs(moveX) > 0.1f) {
                        cur_yRotationAroundTarget = Mathf.LerpAngle(cur_yRotationAroundTarget, target.transform.eulerAngles.y + rs_extraYRotate, deltaTime * 6f);
                    } else {
                        cur_yRotationAroundTarget = Mathf.LerpAngle(cur_yRotationAroundTarget, target.transform.eulerAngles.y + rs_extraYRotate, deltaTime * 7f);
                    }
                } else {
                    cur_yRotationAroundTarget = Mathf.LerpAngle(cur_yRotationAroundTarget, target.transform.eulerAngles.y + rs_extraYRotate, deltaTime * 3f);
                }
            }
            desiredYAngle = cur_yRotationAroundTarget;
            wasJustDriving = true;
        } else {
            if (wasJustDriving) {
                wasJustDriving = false;
                target_yRotationAroundTarget = cur_yRotationAroundTarget;
            }
            desiredYAngle = target_yRotationAroundTarget;
            cur_yRotationAroundTarget = desiredYAngle;
        }

        if (MyInput.jpR3) {
            t_YRotLerpR3 = 0.5f;
        }
        if (t_YRotLerpR3 > 0) {
            t_YRotLerpR3 -= Time.deltaTime;
            if (t_YRotLerpR3 <= 0) t_YRotLerpR3 = 0;
            float ___yRot = 0;
            if (sizeMode == 1) {
                ___yRot = Mathf.LerpAngle(cur_yRotationAroundTarget, target.transform.localEulerAngles.y, Mathf.SmoothStep(0, 1, (0.5f - t_YRotLerpR3) / 0.5f));
                target_yRotationAroundTarget = 0;
                cur_yRotationAroundTarget = desiredYAngle = ___yRot;
            } else {
                ___yRot = Mathf.LerpAngle(cur_yRotationAroundTarget, target.transform.localEulerAngles.y, Mathf.SmoothStep(0, 1, (0.5f - t_YRotLerpR3) / 0.5f));
               target_yRotationAroundTarget = cur_yRotationAroundTarget = desiredYAngle = ___yRot;
            }
        }

        // Now figure out the rotation about the player's local x-axis
        float xOrbitAngleDelta = 0;
         
		if (MyInput.camY > 0.25f) xOrbitAngleDelta = rotateSpeedTimescaled * verticalCamSpeedMultiplier * Mathf.Abs(MyInput.camY);
		if (MyInput.camY < -0.25f) xOrbitAngleDelta = -rotateSpeedTimescaled * verticalCamSpeedMultiplier * Mathf.Abs(MyInput.camY);

		xOrbitAngleDelta *= (SaveManager.sensitivity / 100f);
		curXOrbitAngle += xOrbitAngleDelta;
		if (curXOrbitAngle <= lowerVertAngleLimit) curXOrbitAngle = lowerVertAngleLimit;
        //if (sizeMode == 1 && curXOrbitAngle < lowerVertAngLimRidescale) curXOrbitAngle = lowerVertAngLimRidescale;
		if (curXOrbitAngle >= upperVertAngleLimit)  curXOrbitAngle = upperVertAngleLimit;
        // Choose offset


        if (MyInput.zoomOut) {
            SaveManager.cameraDistance += Time.deltaTime * 70f;
        } else if (MyInput.zoomIn) {
            SaveManager.cameraDistance -= Time.deltaTime * 70f;
        }
        SaveManager.cameraDistance = Mathf.Clamp(SaveManager.cameraDistance, 50f, 200f);

        if (waitingForSizeSwitchConfirmation) {
            lerpOffsetCurrentVector = offset* SaveManager.cameraDistance/100f;
        }
        if (!waitingForSizeSwitchConfirmation) {

            // If toggling camera angle - calculate the current offset (it gets reset a bit above this)
            if (MyInput.jpCamToggle || resetCamOffsetToDefault) {
                SetOffsetBasedOnOffsetMode();
                lerpOffsetInitialVector = offset * SaveManager.cameraDistance/100f;
            }

            // change offsetMode
            if (MyInput.jpCamToggle) {
                offsetMode++; if (offsetMode > 3) offsetMode = 0;
                if (MyInput.controllerActive == false) {
                    // There are no offsets with the controller so ignore this line of code
                    t_lerpOffset = tm_lerpOffset;
                }
            }
            if (resetCamOffsetToDefault) {
                resetCamOffsetToDefault = false;
                offsetMode = 0;
                t_lerpOffset = tm_lerpOffset;
            }

            // Recalc the offset
            SetOffsetBasedOnOffsetMode();
            offset *= (SaveManager.cameraDistance / 100f);


            // Set the extra x orbit (tilting camera up or down) based on camera offset
            if (sizeMode == 0) {
                if (!MyInput.controllerActive) {
                    if (offsetMode == 1) {
                        rs_extraTilt -= Time.deltaTime * 10f;
                        rs_extraTilt = Mathf.Max(rs_extraTilt, -20f);
                    } else {
                        rs_extraTilt += Time.deltaTime * 10f;
                        rs_extraTilt = Mathf.Min(rs_extraTilt, 0);
                    }
                    curXOrbitAngle = rs_extraTilt;
                }
            } else if (sizeMode == 1) {
                if (!MyInput.controllerActive) {
                    if (offsetMode > 0) {
                        rs_extraTilt -= Time.deltaTime * 15f;
                        if (rs_extraTilt < -28f) rs_extraTilt = -28f;
                    } else {
                        rs_extraTilt += Time.deltaTime * 10f;
                        if (rs_extraTilt > -20f) rs_extraTilt = -20f;
                    }
                    curXOrbitAngle = rs_extraTilt;
                }
            }

            // If camera angle was toggled, then start moving the camera to the next target offset
            if (t_lerpOffset > 0) {
                t_lerpOffset -= deltaTime;
                lerpOffsetCurrentVector = Vector3.Lerp(lerpOffsetInitialVector, offset, Mathf.SmoothStep(0, 1, 1 - (t_lerpOffset / tm_lerpOffset)));
                offset = lerpOffsetCurrentVector;
            }
        }

        // ExtraXTilt is an additional euler rotation added to the camera's final look orientation. It should only be negative (i.e. turning towards ceiling)
        float extraXTilt = 0;
        if (curXOrbitAngle < 0) {
            extraXTilt = curXOrbitAngle;
        }
        float xOrbitAngleForOffsetQuat = 0;
        // This is an additional rotation of the Offset Vector. Don't modify its x orbiting unless using a controller and needing to look towards the gruond.
        if (MyInput.controllerActive) { // from 0 to 65, curXOrbitAngle rotates towards the ceiling
            float controllerXTilt_BeginningAngle = -20;
            //if (sizeMode == 1) controllerXTilt_BeginningAngle = -20;
            // ride start at -5
            if (curXOrbitAngle > controllerXTilt_BeginningAngle) { // from -20 to 0, rotate towards the ground. Beyond -20, lock the offset rotation there and start to tilt camera
                xOrbitAngleForOffsetQuat = curXOrbitAngle;
                extraXTilt = 0; // cancel out the camera tilting
            } else {
                xOrbitAngleForOffsetQuat = controllerXTilt_BeginningAngle;
                extraXTilt = curXOrbitAngle - controllerXTilt_BeginningAngle;  // do this so the tilting starts from zero and goes higher (vs just starting at the cutoff)
            }
        } 


        // The euler rotation only has a minimum x of zero so the vector isn't rotated into the groun.
        // Afterwards, based on the current x orbit angle, the camera will be rotated higher
        Quaternion rotation = Quaternion.Euler(xOrbitAngleForOffsetQuat, desiredYAngle, 0);
        // Move camera away from player and look at it.
        transform.position = targetPos - (rotation * offset);
		transform.LookAt(targetPos);
        // 
        tempCamPos = transform.localEulerAngles;
        tempCamPos.x += extraXTilt;
        transform.localEulerAngles = tempCamPos;

        Vector3 uncollidedCameraPosition = transform.position;

        // Move to player if wall in the way
        // Use target's raw pos vs. targetpos, so the lerping targetpos during jumps wont get stuck in ground...
        tempRay.origin = target.transform.position;
        tempRay.direction = transform.position - target.transform.position;
        //Debug.DrawRay(ray.origin,10*ray.direction,Color.red);
        bool movedByBackCast = false;
        // 1.05f helps with issues where the camera is basically juuuust inside the wall so there's no hit detecton
		if (Physics.Raycast (tempRay,out tempHitInfo,offset.magnitude*1.26f,layermask,QueryTriggerInteraction.Ignore)) {

            // Obstruction avoidance:
            tempRay.origin = transform.position;
            tempRay.direction = target.transform.position - transform.position;
            RaycastHit obstructionCheck = new RaycastHit();
            // xor out the terrain because obstruction doesn't work right
            Physics.Raycast(tempRay, out obstructionCheck, offset.magnitude*0.9f, layermask ^ (1 << 11), QueryTriggerInteraction.Ignore);

            // If there's an obstruction, don't move the camera closer tot he player. stay where u r
            //Debug.DrawRay(ray.origin, offset.magnitude*0.9f*ray.direction, Color.red);
            if (obstructionCheck.point != Vector3.zero) {
                //Debug.DrawRay(obstructionCheck.point, Vector3.up*2f, Color.yellow);
            } else {
                normalNoY = tempHitInfo.normal;
                normalNoY.y = 0;
                // Reset camera offset if it's stuck in a slope unless using controller
                float slopeAngle = 90f - Vector3.Angle(tempHitInfo.normal, normalNoY);
                bool doneBackCasting = false;
                if (slopeAngle < 45f && slopeAngle > 5f && tempHitInfo.normal.y > 0) {
                    //if (!MyInput.controllerActive && sizeMode == 0) resetCamOffsetToDefault = true;
                    tempCamPos = target.transform.position - rotation*offset; // move cam to desired position
                    tempCamPos.y += 6f;
                    tempRay.origin = tempCamPos; // Start a ray above this position
                    tempRay.direction = Vector3.down;
                    Debug.DrawRay(tempRay.origin, tempRay.direction * 6f);
                    if (Physics.Raycast(tempRay, out tempHitInfo, 6f, layermask, QueryTriggerInteraction.Ignore)) {
                        tempCamPos = tempHitInfo.point;
                        tempCamPos.y += 0.5f;
                        transform.position = tempCamPos;
                        doneBackCasting = true;
                    }
                }
                if (!doneBackCasting) {
                    tempCamPos = target.transform.position;
                    Vector3 diff = tempHitInfo.point - tempCamPos;
                    // Move the camera position a little away frmo the collision to help with avoiding clipping
                    diff *= noClipFac;
                    tempCamPos += diff;
                    tempCamPos += tempHitInfo.normal * backcastHitNormal_OffsetDistance;
                    // preserve the original y position, so the camera doesn't move downwards into the player as much.
                    tempCamPos.y = transform.position.y;
                    transform.position = tempCamPos;
                }
                movedByBackCast = true;
            }
		}
        if (movedByBackCast) {
            lastPosAdjustedByBackcast = transform.position;
            movedByBackcastTimer = tm_camAdjustments;
        } else {
            lastPosWithoutBackcastAdjustment = transform.position;
            adjustToBackcastOnlyPosTimer = tm_camAdjustments;
        }


        // nearplane cast
        // Center now starts at the other side of the casting direction. basically makes less of an issue of clipping

        // Viewport values:
        // y 1|
        //    |
        //    0-------1 x
        bool movedByNearPlane = false;
        movedByNearPlane = NearplaneCastAndMove(1f, 0.5f, 0, 0.5f, cam); // right to left
        if (!movedByNearPlane) movedByNearPlane = NearplaneCastAndMove(0, 0.5f, 1, 0.5f, cam); // left to right
        if (!movedByNearPlane) movedByNearPlane = NearplaneCastAndMove(0.5f, 1f, 0.5f, 0, cam); // top to bottom
        if (!movedByNearPlane) movedByNearPlane = NearplaneCastAndMove(0.5f,0, 0.5f, 1, cam); // bottom to top

        if (nearPlaneTimer > 0) nearPlaneTimer -= Time.deltaTime;
        if (movedByBackcastTimer > 0) movedByBackcastTimer -= Time.deltaTime;
        if (adjustToBackcastOnlyPosTimer > 0) adjustToBackcastOnlyPosTimer -= Time.deltaTime;

        if (movedByBackCast && !movedByNearPlane) {
            // Written poorly, but this will gradually lerp from a non-adjusted to an adjusted position.
            transform.position = Vector3.Lerp(lastPosWithoutBackcastAdjustment, lastPosAdjustedByBackcast, 1- Mathf.SmoothStep(0,1,adjustToBackcastOnlyPosTimer/tm_camAdjustments));

        // If the nearplane moves the camera, then the next time it has NOT moved the camera, lerp to that position
        } else if (movedByNearPlane) {
            lastPosAdjustedByNearPlane = transform.position;
            if (movedByBackcastTimer > 0) {
                transform.position = Vector3.Lerp(lastPosAdjustedByBackcast, lastPosAdjustedByNearPlane, 1 - Mathf.SmoothStep(0,1,movedByBackcastTimer / tm_camAdjustments));
            } else {
                nearPlaneTimer = tm_camAdjustments;
            }
        } else {
            if (nearPlaneTimer > 0) {
                transform.position = Vector3.Lerp(lastPosAdjustedByNearPlane,transform.position, 1 - Mathf.SmoothStep(0,1,nearPlaneTimer / tm_camAdjustments));
            } else if (movedByBackcastTimer > 0) {
                transform.position = Vector3.Lerp(lastPosAdjustedByBackcast, transform.position, 1 - Mathf.SmoothStep(0, 1, movedByBackcastTimer / tm_camAdjustments));
            }
        }

        if (lerpCamPos) {
			float _t = Mathf.SmoothStep(0,1,triggerTransitionTime/maxTTT);
			triggerTransitionTime += deltaTime;
			if (triggerTransitionTime > maxTTT) {
				lerpCamPos = false;
			}

			// Need y rotation to lerp correctly
			Vector3 eulerLerp = Vector3.Lerp(STARTrotFromTrigger,transform.eulerAngles,_t);
			eulerLerp.x = Mathf.LerpAngle(STARTrotFromTrigger.x,transform.eulerAngles.x,_t);
			eulerLerp.y = Mathf.LerpAngle(STARTrotFromTrigger.y,transform.eulerAngles.y,_t);
			transform.rotation = Quaternion.Euler(eulerLerp);
			transform.position = Vector3.Lerp(STARTposFromTrigger,transform.position,_t);


		}

	}

    private void SetOffsetBasedOnOffsetMode() {
        if (MyInput.controllerActive) {
            offsetMode = 0;
            if (sizeMode == 0) {
                offset = mediumOffset;
                //if (offsetMode == 3) offsetMode = 0;
                //if (offsetMode == 0) offset = mediumOffset;
                //if (offsetMode == 1) offset = 1.35f * mediumOffset;
                //if (offsetMode == 2) offset = 0.65f * mediumOffset;
            } else {
                offset = bigOffset;
                //if (offsetMode == 0) offset = bigOffset;
                //if (offsetMode == 1) offset = bigOffsetLow;
                //if (offsetMode == 2) offset = bigOffsetLow;
                //if (offsetMode == 3) offset = bigOffsetLow;
            }
        } else {
            if (sizeMode == 0) {
                if (offsetMode == 0) offset = mediumOffset;
                if (offsetMode == 1) offset = mediumOffsetFar;
                if (offsetMode == 2) offset = mediumOffsetUp;
                if (offsetMode == 3) offset = mediumOffsetTopDown;
            } else {
                if (offsetMode == 0) offset = bigOffset;
                if (offsetMode == 1) offset = bigOffsetLow;
                if (offsetMode == 2) offset = bigOffsetLow;
                if (offsetMode == 3) offset = bigOffsetLow;
            }
        }
    }

    bool NearplaneCastAndMove(float originX, float originY, float destx,float desty, Camera mainCamera) {
        // Center now starts at the other side of the casting direction. basically makes less of an issue of clipping
        tempPlaneV.Set(originX, originY, mainCamera.nearClipPlane);
        planeCastStart = mainCamera.ViewportToWorldPoint(tempPlaneV);

        tempPlaneV.Set(destx, desty, mainCamera.nearClipPlane);
        planeCastEnd = mainCamera.ViewportToWorldPoint(tempPlaneV);


        tempRay.origin = planeCastStart;
        tempRay.direction = planeCastEnd - planeCastStart;

        if (Physics.Raycast(tempRay, out tempHitInfo, Vector3.Distance(planeCastStart, planeCastEnd), layermask, QueryTriggerInteraction.Ignore)) {
            Vector3 hitOffset = tempHitInfo.point - planeCastEnd;
            tempCamPos = transform.position;
            tempCamPos += hitOffset * 1.25f;
            transform.position = tempCamPos;
            return true;
        }

        return false;
    }

    bool resetCamOffsetToDefault = false;
    Vector3 normalNoY = new Vector3();
    Ray tempRay = new Ray();
    RaycastHit tempHitInfo = new RaycastHit();
    float backcastHitNormal_OffsetDistance = 0.8f;
    float tm_camAdjustments = 0.18f;
    float adjustToBackcastOnlyPosTimer = 0;
    Vector3 lastPosWithoutBackcastAdjustment = new Vector3();

    Vector3 tempPlaneV = new Vector3();
    Vector3 planeCastStart = new Vector3();
    Vector3 planeCastEnd = new Vector3();

    float movedByBackcastTimer = 0;
    Vector3 lastPosAdjustedByBackcast = new Vector3();
    float nearPlaneTimer = 0;
    Vector3 lastPosAdjustedByNearPlane = new Vector3();


    // Shifts the player-to-cam vector to left and right to check for collisions
    bool playerToCamIsClearOnCenterLeftAndRight() {

        Vector3 center = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, cam.nearClipPlane));
        Vector3 rightMid = cam.ViewportToWorldPoint(new Vector3(1, 0.5f, cam.nearClipPlane));
        Vector3 rightOffset = rightMid - center;
        Ray ray = new Ray(targetPos+rightOffset, transform.position - targetPos);
        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(ray, out hit, offset.magnitude, layermask, QueryTriggerInteraction.Ignore)) {
            return false;
        }
        ray.origin = targetPos - rightOffset;
        if (Physics.Raycast(ray, out hit, offset.magnitude, layermask, QueryTriggerInteraction.Ignore)) {
            return false;
        }

        return true;
    }

    public void setCameraNextSceneEulerY(float nextY) {
        initialCameraRotation = nextY;
    }

    public void setCameraEulerYRotation(float newY) {
        cur_yRotationAroundTarget = newY;
        target_yRotationAroundTarget = newY;
    }


    Vector3 jumpCamTargetPos = new Vector3(0, 0, 0);
    bool inJumpCam = false;
    int jumpCamMode = 0;
    float timeJumpHeld = 0f;

    public float jumpCameraLookYOff = 2f;
    float jumpCameraLookYOff_Current = 0;
    public float timerLerpToJumpCamera = 0.7f;

    float t_lerpOffset; // Timer for moving between offsets in medium mode
    float tm_lerpOffset = 0.4f;
    Vector3 lerpOffsetInitialVector = new Vector3();
    Vector3 lerpOffsetCurrentVector;

    float t_xzCamLag = 0;

    float t_JumpReturn = 0;
    public float tm_JumpReturn = 0.4f;
    public float jumpDeadYMargin = 3.5f;
    float jumpDeadStartY = 0f;

    public float finalYOffset = 2.5f;
	float noClipFac = 0.77f;

    float horizontalRotation = 0;
    private bool wasJustDriving;
    [System.NonSerialized]
    public float t_YRotLerpR3 = 0;

    public void startJumpCamMode() {
        // If the player touches the ground but the camera doesn't finish its 'return to normal' lerping, and
        // the player jumps again, then return back to the 'follow beneath the player mode.

        jumpDeadStartY = target.transform.position.y;
        jumpCamTargetPos = targetPos;
        inJumpCam = true;
        jumpCamMode = 0;
    }
    public void stopJumpCamMode() {
        jumpCamMode = 2;
        t_JumpReturn = 0;
    }

    /**
     * Makes the camera code stop running
     * */
    public void enterCinemachineMovieMode() {
       // GameObject.Find("vcam Main Imitator").GetComponent<MatchCMCamToMainCam>().enabled = false;
        GetComponent<Cinemachine.CinemachineBrain>().enabled = true;
        inCinemachineMovieMode = true;
    }

    public void exitCinemachineMovieMode() {
        GetComponent<Cinemachine.CinemachineBrain>().enabled = false;
        inCinemachineMovieMode = false;
    }
}
