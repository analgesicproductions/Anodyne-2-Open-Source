using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class MediumControl : MonoBehaviour {

    ParticleSystem SplashParticles;
    ParticleSystem WaterRingParticles;
    ParticleSystem HoverParticles;

    int nojumpticks = 30;

    [Header("Hurt Properties")]
    [Range(0.2f, 1f)]
    public float hurtSlowDown = 0.4f;
    [Range(0.1f, 0.5f)]
    public float hurtAnimCrossfadeTime = 0.33f;

    public static float carAccelTimeOnSceneExit = 0;

    [Header("Regular Properties")]
    public Material novaUnlitMat;

    public bool IsRidescale = false;

    [System.NonSerialized]
    public bool CanShootSpark = false;

    [System.NonSerialized]
    public UIManagerAno2 ui;
	public Rigidbody rb;
	public float maxMedVel = 20f;
    public float maxMedFallVel = 20f;
    public float bigTurnSpeed = 90f;
	public float maxBigVel = 10f;
    public float Big_TimeToMaxVel = 4f;
    public Transform bigModelTransform;
    public Vector3 sceneStartPos;
    ParticleSystem doubleJumpParticles;
    public GameObject PREFAB_doubleJumpParticles;

    AudioSource ridescaleAudioSrc;
    float t_engineIdle;
    int engineSoundState = 0;
    public AudioClip engineIdleClip;
    float engineIdleVolume = 0.3f;
    float engineMoveVolume = 0.45f;

    DialogueBox dbox;

	CapsuleCollider cc;

	MedBigCam camControl;
	Animator animator;
    GameObject fixedHelperTransform;


    float forceFixedCamEulerY = 0;
    bool forceFixedCamEulerYTillNotPressingDirection = false;
    public static float nextMediumEulerY = -1;

    bool holdingForward = false;
    bool holdingBackward = false;

    public void SetFixedCamEulerYTillNotPressingDirection(float eulerY) {
        forceFixedCamEulerY = eulerY;
        forceFixedCamEulerYTillNotPressingDirection = true;
    }

    public bool OnMovingPlatform = false;
    Vector3 tempRot = new Vector3();
    float cur_rsXAngleTilt = 0;
    float target_rsXAngleTilt = 0;

    bool isHurt = false;
    bool useHurtMotion = false;

    void Start () {
        if (!IsRidescale) HoverParticles = transform.Find("HoverParticles").GetComponent<ParticleSystem>();
        ui = GameObject.Find("3D UI").GetComponent<UIManagerAno2>();
		rb = gameObject.GetComponent<Rigidbody>();	
		cc = GetComponent<CapsuleCollider>();

        string curSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (!IsRidescale && curSceneName == "nothing") {
            Renderer[] rs = transform.Find("NovaFBX").GetComponentsInChildren<Renderer>();
            foreach (Renderer r in rs) {
                r.material = novaUnlitMat;
            }
        }

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "CCC") {
            outofboundsYVal = -30f;
        } else if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "RingCave") {
            outofboundsYVal = -20f;
        } else if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "DesertSpireTop") {
            outofboundsYVal = 205f;
        } else if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "DesertShore") {
            outofboundsYVal = 173f;
        }

            dbox = GameObject.Find("Dialogue").GetComponent<DialogueBox>();
		camControl = GameObject.Find("MedBigCam").GetComponent<MedBigCam>();
		fixedHelperTransform = new GameObject();
        fixedHelperTransform.name = name + "fixedHelperTransform";
        isHurt = DustDropPoint.IsDisillusioned();
        if (IsRidescale) {
            if (isHurt) {
                useHurtMotion = true;
              //  engineIdleVolume = engineMoveVolume = 0.3f;
            }
            animator = bigModelTransform.Find("Ridescale").GetComponent<Animator>();
        } else {


            WaterRingParticles = transform.Find("WaterRing").GetComponent<ParticleSystem>();
            SplashParticles = transform.Find("Splash").GetComponent<ParticleSystem>();
            doubleJumpParticles = Instantiate(PREFAB_doubleJumpParticles, transform).GetComponent<ParticleSystem>();
            animator = transform.Find("NovaFBX").GetComponent<Animator>();

            if (isHurt) {
                animator.Play("Collapse");
                hurtMode = 10;
            }

            if (Registry.destinationDoorName == "USESAVEPT") {
                transform.position = Registry.enterGameFromLoad_Position;
                Registry.destinationDoorName = "";
            }
            Registry.MoveObjectToDestinationDoor(gameObject);

            tempV1 = transform.localEulerAngles;
            tempV1.x = 0; tempV1.z = 0;
            transform.localEulerAngles = tempV1;
            if (Registry.destinationDoorName != "") {
                GameObject lookGameObj = GameObject.Find(Registry.destinationDoorName);
                Transform lookTarget = null;
                if (lookGameObj != null) {
                    lookTarget = lookGameObj.transform.Find("LookTarget");
                }  else {
                    print("No door: " + Registry.destinationDoorName);
                }
                
                if (lookTarget == null) {
                    print("Warning, door " + Registry.destinationDoorName + " has no LookTarget");
                } else {
                    Vector3 oldEu = transform.localEulerAngles; transform.LookAt(lookTarget); oldEu.y = transform.localEulerAngles.y; transform.localEulerAngles = oldEu;
                    nextMediumEulerY = transform.localEulerAngles.y;
                    // Need both bc not sure whether or not 
                    camControl.setCameraEulerYRotation(nextMediumEulerY);
                }
                Registry.destinationDoorNameForPauseRespawn = Registry.destinationDoorName;
            } else {
                Registry.destinationDoorNameForPauseRespawn = "";
            }

            Registry.destinationDoorName = "";

            if (nextMediumEulerY != -1) {
                Vector3 eu = transform.localEulerAngles;
                eu.y = nextMediumEulerY; transform.localEulerAngles = eu;
            }
            nextMediumEulerY = -1;
        }

        MediumControl.ignoreMedInput = false;
        if (MedBigCam.forceRidescaleNextScene) {
            MediumControl.ignoreMedInput = true;
        }

        if (sizeSwitch_MRs == null) {
            ColorUtility.TryParseHtmlString("#4CCBCB", out emissiveCol);
            sizeSwitch_MRs = transform.GetComponentsInChildren<Renderer>();
        }

        AudioMixer mixer = GameObject.Find("Track 1-1").GetComponent<AudioSource>().outputAudioMixerGroup.audioMixer;
        ridescaleAudioSrc = gameObject.AddComponent<AudioSource>();
        ridescaleAudioSrc.outputAudioMixerGroup = mixer.FindMatchingGroups("Regular SFX")[0];


        if (!IsRidescale) {
            if (Registry.justLoaded) {
                transform.position = Registry.enterGameFromLoad_Position;
                Registry.justLoaded = false;
            }

            for (int i = 0; i < MediumControl.positionBuf.Length; i++) {
                MediumControl.positionBuf[i] = transform.position;
            }
            MediumControl.recentPosBufIdx = 0;
        }
        sceneStartPos = transform.position;
	}

    public void OnReturnToWalkscale() {
        if (cruiseControlOn) {
            cruiseControlOn = false;
            ui.TurnOffCruiseIcon();
        }
    }

    public void pushbackFromWall() {
        Vector3 newpos = transform.position;
        Ray r = new Ray(newpos, transform.forward);
        RaycastHit hit = new RaycastHit();
        int layermask = ~(1 << 9 | 1 << 21); // Everything but the player + audio
        float clearRadius = 3.0f;
        bool hitted = Physics.Raycast(r, out hit, clearRadius, layermask, QueryTriggerInteraction.Ignore);
        if (hitted) {
            float dis = Vector3.Distance(transform.position, hit.point);
            if (dis > clearRadius) dis = clearRadius;
            if (dis > 0 && dis < clearRadius) {
                float disToPushback = clearRadius - dis;
                Vector3 offset =  transform.forward * -1;
                offset *= disToPushback*2f;

                newpos += offset;
            }
        }
        transform.position = newpos;
    }

    public void setBigPlayerStartPosBasedOnMediumPlayer(GameObject mediumPlayer) {
        Vector3 newpos = mediumPlayer.transform.position;
        newpos.y += 1.52f;
        transform.position = newpos;
    }
    public void setBigPlayerStartVelBasedOnMed(Vector3 mediumVel) {
        accelTime = 5*bigBoostTime * (mediumVel.magnitude / maxBigVel);
    }
    public void setMediumPlayerStartPosBasedOnBigPlayer(GameObject bigPlayer) {
        CapsuleCollider big_cc = bigPlayer.GetComponent<CapsuleCollider>();
        Vector3 newpos = bigPlayer.transform.position;
        newpos.y -= big_cc.bounds.extents.y;
        newpos.y += big_cc.center.y;
        newpos.y += GetComponent<CapsuleCollider>().bounds.extents.y;
        transform.position = newpos;
    }

    Vector3 ccp1 = new Vector3();
    Vector3 ccp2 = new Vector3();
    Vector3 ccBottom = new Vector3();
    float ccRadiusWorld;
    RaycastHit capCastHit = new RaycastHit();
    void SetCapsuleCastVars() {
        float dis_ = cc.height * transform.localScale.y / 2 - cc.radius * transform.localScale.z;
        ccp1 = transform.position + cc.center * transform.localScale.y + Vector3.up * dis_;
        ccp2 = transform.position + cc.center * transform.localScale.y - Vector3.up * dis_;
        ccBottom = transform.position + cc.center * transform.localScale.y;
        ccBottom.y -= cc.bounds.extents.y;
        ccRadiusWorld = cc.radius * transform.localScale.z;
    }

    Ray raySlopeAngle = new Ray();
    RaycastHit hitSlopeAngle = new RaycastHit();
    float getRidescaleSlopeAngle() {
        raySlopeAngle.origin = transform.position;
        raySlopeAngle.direction = Vector3.down;
        if (Physics.Raycast(raySlopeAngle, out hitSlopeAngle, 4.5f, ~(1 << 21), QueryTriggerInteraction.Ignore)) {
            float slopeAngle = Vector3.Angle(hitSlopeAngle.normal, Vector3.up);
            return slopeAngle;
        }
        return -500f;

    }

	public bool isTouchingGround(float checkDistance=-1) {
        if (IsRidescale) {
            if (checkDistance == -1) checkDistance = 0.2f;
            SetCapsuleCastVars();
            ccp1.y += 0.1f; ccp2.y += 0.1f;
           // Debug.DrawRay(ccp1, transform.forward * ccRadiusWorld,Color.blue);
           // Debug.DrawRay(ccp2, transform.forward * ccRadiusWorld);
            // Cool trick! check the bottom of capsule to collide distance to get a good estimate of slopiness
            if (Physics.CapsuleCast(ccp1, ccp2, ccRadiusWorld, Vector3.down,out capCastHit, checkDistance,~(1 << 21),QueryTriggerInteraction.Ignore)) {
                if (Vector3.Distance(capCastHit.point, ccBottom) < 2f) {
                    return true;
                }
            }
            return false;
        } else {
            if (checkDistance == -1) checkDistance = 0.2f;
            //Debug.DrawRay(transform.position + cc.center * transform.localScale.y, -Vector3.up * (cc.bounds.extents.y + 0.2f));
            // note: might be wrong if the capsule ver has a nonzero x or z coordinate
            SetCapsuleCastVars();
            ccp1.y += 0.05f; ccp2.y += 0.05f;
            if (Physics.CapsuleCast(ccp1, ccp2, ccRadiusWorld, Vector3.down, out capCastHit, checkDistance,~(1 << 21),QueryTriggerInteraction.Ignore)) {
                //print(Vector3.Distance(capCastHit.point, ccBottom));
                if (Vector3.Distance(capCastHit.point, ccBottom) < .45f) {
                    return true;
                }
            }
            // Rather than assuming not being on the ground, this really makes sure the bottom of the capsule isn't near the ground
            return Physics.Raycast(transform.position + cc.center * transform.localScale.y, -Vector3.up, cc.bounds.extents.y + 0.05f, ~(1<<9 | 1 << 21), QueryTriggerInteraction.Ignore);



            /*
             * This hack will return accurater results for fucked up FBX imports, but we probably shouldnt use it
            Ray ray = new Ray(transform.position + cc.center * transform.localScale.y, -Vector3.up);
            RaycastHit hit = new RaycastHit();
            print(Physics.Raycast(ray, out hit, cc.bounds.extents.y + 0.05f, ~(1 << 9), QueryTriggerInteraction.Ignore));

            // if (hit.collider != null) print(hit.collider.gameObject.name);
            return Physics.Raycast(ray, out hit, cc.bounds.extents.y + 0.05f, ~(1 << 9), QueryTriggerInteraction.Ignore);
            */

        }
    }
    public bool pausedBySparkBar = false;
    public bool isThereAnyReasonToPause() {
        return CameraTrigger.PausedByCameraTrigger || pausedBySparkBar || DataLoader.instance.isChangingScenes|| DialogueAno2.AnyScriptIsParsing || DataLoader.instance.isPaused || SaveModule.saveMenuOpen || CutsceneManager.deactivatePlayer || MedBigCam.inCinemachineMovieMode || !dbox.isDialogFinished();
    }

    public void PlayAnimation(string name) {
        if (name == "idle") name = "Idle";
        if (name == "Idle" && isHurt) name = "IdleHurt";
        animator.Play(name);
    }


    bool ride_wheelTurnInitialized = false;
    Vector3 wheelEuler = new Vector3();
    float rotateRate = 700f;
    Transform[] wheelTransforms = new Transform[8];
    public void RidescaleInitWheelTurning() {
        if (ride_wheelTurnInitialized) return;
        ride_wheelTurnInitialized = true;
        string[] a = new string[] { "Wheel.Back.L", "Wheel.Back.L.001", "Wheel.Back.R", "Wheel.Back.R.001", "Wheel.Front.L", "Wheel.Front.L.001", "Wheel.Front.R", "Wheel.Front.R.001" };
        Transform[] children = transform.GetComponentsInChildren<Transform>();
        int j = 0;
        foreach (Transform t in children) {
            for (int i = 0; i < 8; i++) {
                if (a[i] == t.name) {
                    wheelTransforms[j] = t;
                    j++;
                    break;
                }
            }
        }
    }
    public void RidescaleUpdateRotating(float accelTime) {
        RidescaleInitWheelTurning();
        foreach (Transform t in wheelTransforms) {
            wheelEuler = t.transform.localEulerAngles;
            if (t.name == "Wheel.Front.R" || t.name == "Wheel.Front.R.001") {
                wheelEuler.y += rotateRate * Time.deltaTime * (accelTime / Big_TimeToMaxVel);
            } else {
                wheelEuler.y -= rotateRate * Time.deltaTime * (accelTime / Big_TimeToMaxVel);
            }
                
            t.transform.localEulerAngles = wheelEuler;
            // subtract to go forward
        }
    }

    [System.NonSerialized]
    public bool cruiseControlOn = false;

    [System.NonSerialized]
    public float accelTime = 0;
	bool wasPausedFixed =false;
    float horCutoff = 0.1f;
    Vector3 dummyV = new Vector3();
    float outofboundsYVal = -200f;
    int outofBoundsMode = 0;

    [System.NonSerialized]
    public static Vector3[] positionBuf = new Vector3[8];
    float positionBufferUpdateInterval = 0.5f;
    float t_updatePositionBuffer;
    [System.NonSerialized]
    public static int recentPosBufIdx = 0;
    float outOfBoundsWait = 0;

    bool switchingSize = false;
    int switchSizeMode = 0;
    public ParticleSystem shrinkSparkle;
    
    public void SwitchSizes(bool shrink = true) {
        switchingSize = true;

        // Unparent so sparkling fizzles out even after gameobject is deactivated by camera 
        if (!IsRidescale) {
            if (shrinkSparkle == null) shrinkSparkle = transform.Find("ShrinkSparkle").GetComponent<ParticleSystem>();
            shrinkSparkle.transform.parent = null;
            shrinkSparkle.transform.position = transform.position;
            WaterRingParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        } else {
            shrinkSparkle.transform.parent = null;
            shrinkSparkle.transform.position = bigModelTransform.position;
        }
        if (shrink) {
            switchSizeMode = 1;
        } else {
            if (!IsRidescale) transform.localScale = new Vector3(sizeSwitch_MinScaleWalk, sizeSwitch_MinScaleWalk, sizeSwitch_MinScaleWalk);
            if (IsRidescale) {
                bigModelTransform.localScale = new Vector3(sizeSwitch_MinScaleRide, sizeSwitch_MinScaleRide, sizeSwitch_MinScaleRide);
                t_engineIdle = 0;
            }
            switchSizeMode = 10;
        }


    }
    public bool DoneSwitchingSize() {
        return switchSizeMode == 0;
    }

    void FixedUpdate() {

        if (IsRidescale) {
            //print("Start:" + transform.position);
        }

        if (IsRidescale) {
            if (ignoreMedInput) {
                ignoreMedInput = false;
            }
        } else {
            if (ignoreMedInput) {
                return;
            }
        }


        if (!isThereAnyReasonToPause() && !doSpinOutAfterNano && (hurtMode == 0 || hurtMode == 10 || hurtMode == 3 || hurtMode == 4)) {
            jumpLogic();
        }
        myJPJump = false;


        if (outofBoundsMode == 0) {
            if (transform.position.y < outofboundsYVal) {
                ui.StartFade(0.5f, true);
                outofBoundsMode = 1;
                outOfBoundsWait = 1f;
            }
        } else if (outofBoundsMode == 1 && ui.fadeMode == 0) {
            if (outOfBoundsWait < 0) {
                ui.StartFade(0.5f, false);
                outofBoundsMode = 0;
            } else {
                if (outOfBoundsWait == 1f) {
                    int idx = MediumControl.recentPosBufIdx + 1;
                    if (idx == MediumControl.positionBuf.Length) idx = 0;
                    transform.position = MediumControl.positionBuf[idx];
                    if (camControl.GetBigPlayerControl() != null) {
                        camControl.GetBigPlayerControl().transform.position = transform.position;
                        camControl.GetBigPlayerControl().rs_lastY = transform.position.y + 0.5f;
                        camControl.GetBigPlayerControl().GetComponent<Rigidbody>().MovePosition(transform.position);
                    }
                    if (IsRidescale) {
                        accelTime = 0;
                        rb.velocity = Vector3.zero;
                        rs_lastY = transform.position.y + 0.5f;
                        camControl.GetMediumControl().outofBoundsMode = 0;
                    } else {
                        if (camControl.GetBigPlayerControl() != null) camControl.GetBigPlayerControl().outofBoundsMode = 0;
                    }
                    camControl.GetMediumControl().transform.position = transform.position;
                    camControl.transform.position = transform.position;
                }
                outOfBoundsWait -= Time.deltaTime;
                rb.velocity = Vector3.zero;
                return;
            }
        }


        if (doSpinOutAfterNanoDuringPause) {
            UpdateReturnFromNanoSpin();
            return;
        }

        bool controlsOn = true;
		if (isThereAnyReasonToPause()) {

            
            bool touchingGround_ = isTouchingGround(0.1f);

            if (!IsRidescale && (animator.GetCurrentAnimatorStateInfo(0).IsName("JumpUp") || animator.GetCurrentAnimatorStateInfo(0).IsName("JumpUpHurt"))) {
                animator.SetBool("Falling", true);
            }

            if (DialogueAno2.AnyScriptIsParsing && !IsRidescale) {
                HoverParticles.Stop();
                animator.SetBool("Running", false);
                if (isTouchingGround(1f)) {
                    animator.SetBool("Falling", false);
                }   
            }
            // Slow down player before starting any dialogue
            if (!(touchingGround_ && Mathf.Abs(rb.velocity.x) < 1f && Mathf.Abs(rb.velocity.z) < 1f) && DialogueAno2.AnyScriptIsParsing) {

                Vector3 newvel = rb.velocity;
                newvel.x *= 0.95f;
                newvel.y -= Time.deltaTime * 12f; if (newvel.y < -10f) newvel.y = -10f;
                if (touchingGround_) {
                    if (!IsRidescale) animator.SetBool("Falling", false);
                    newvel.y = 0;
                }
                newvel.z *= 0.95f;
                if (IsRidescale) {
                    newvel.x *= 0.85f; newvel.z *= 0.85f;
                    accelTime = 0;
                }
                rb.velocity = newvel;
                return;
            }
            controlsOn = false;
			rb.isKinematic = true;
			wasPausedFixed = true;
            CanShootSpark = false;
		} else {
			if (wasPausedFixed) {
				rb.isKinematic = false;
				wasPausedFixed = false;
			}
            if (HF.TimerDefault(ref t_updatePositionBuffer, positionBufferUpdateInterval)) {
                if (isTouchingGround(1f)) {
                    MediumControl.recentPosBufIdx++;
                    if (MediumControl.recentPosBufIdx == MediumControl.positionBuf.Length) MediumControl.recentPosBufIdx = 0;
                    MediumControl.positionBuf[MediumControl.recentPosBufIdx].Set(transform.position.x, transform.position.y, transform.position.z);
                }
            }
		}

        if (doSpinOutAfterNano) {
            UpdateReturnFromNanoSpin();
            return;
        }

        if (switchingSize) {
            if (IsRidescale) {
                UpdateSizeSwitch();
            } else {
                UpdateSizeSwitch();
                controlsOn = false;
                return;
            }
        }



        if (isHurt && !IsRidescale && controlsOn) {


            if (hurtMode == 10) {
                if (isTouchingGround(0.1f) && walkscaleJumpState == 0) {
                    hurtMode = 1;
                    CanShootSpark = false;
                    rb.velocity = Vector3.zero;
                }
            }
            // Collapse
            // CollapseIdle GetUp

            // Fall down after X seconds of idle, or Y seconds of regular movement.

            // if hurtmode is 4 (after getting up), the only difference is that if you continuously walk you can enter full speed in hurt mode
            // hurt mode only becomes 0 after walking too long in regular mode (hurt mode 3)
            if (hurtMode == 0 || hurtMode == 4) {
                t_hurtWalkWait += Time.fixedDeltaTime;
                t_hurtPoofer += Time.fixedDeltaTime;
                if (MyInput.jpToggleRidescale && t_hurtWalkWait < 15f) {
                    //t_hurtWalkWait = 30f;
                    if (t_hurtPoofer >= 0) {
                        t_hurtPoofer = -1f;
                        AudioHelper.instance.playOneShot("crystalHitPlayer");
                        tempV1 = transform.position; tempV1.y += 0.5f;
                        HF.SpawnDustyHit3D(tempV1);
                    }
                }
                if (!(MyInput.up || MyInput.down) || t_hurtWalkWait >3.5f) {
                    t_hurtMode += Time.fixedDeltaTime;
                    if (MyInput.jpSpecial) t_hurtMode = 0;
                    if  (t_hurtMode >= 2.5f || t_hurtWalkWait > 3.5f) {
                        if (walkscaleJumpState == 0) {
                            t_hurtMode = 0;
                            if (!MyInput.jpToggleRidescale && hurtMode == 4 && t_hurtWalkWait > 3.5f) {
                                hurtMode = 3;
                                if (animator.GetBool("Running")) {
                                    animator.CrossFadeInFixedTime("Run", hurtAnimCrossfadeTime);
                                } else {
                                    animator.CrossFadeInFixedTime("Idle", hurtAnimCrossfadeTime);
                                }
                            } else {
                                hurtMode = 1;
                                rb.velocity = Vector3.zero;
                                CanShootSpark = false;
                                animator.CrossFadeInFixedTime("Collapse", hurtAnimCrossfadeTime);
                            }
                            t_hurtWalkWait = 0;
                        }
                    }
                } else {
                    t_hurtMode = 0;
                }
                useHurtMotion = true;
            } else if (hurtMode == 3) {
                // after nova's gotten up, allow running around for X seconds normally, then go back to the hurt movement
                useHurtMotion = false;
                t_hurtMode += Time.fixedDeltaTime;
                if (MyInput.jpToggleRidescale) {
                    MyInput.jpToggleRidescale = false;
                    t_hurtMode = 30f;
                    t_hurtWalkWait = 30f; // force a transition in mode 0 straight to collapsing
                    AudioHelper.instance.playOneShot("crystalHitPlayer");
                    tempV1 = transform.position; tempV1.y += 0.5f;
                    HF.SpawnDustyHit3D(tempV1);
                }
                if (t_hurtMode > 15f && walkscaleJumpState == 0) {
                    t_hurtMode = 0;
                    hurtMode = 0;
                    if (animator.GetBool("Running")) {
                        animator.CrossFadeInFixedTime("WalkHurt", hurtAnimCrossfadeTime);
                    } else {
                        animator.CrossFadeInFixedTime("IdleHurt", hurtAnimCrossfadeTime);
                    }
                }
            }

            if (hurtMode == 1) {
                nextJumpVel = rb.velocity;
                nextJumpVel.y -= Time.deltaTime * 15f;
                if (nextJumpVel.y <= -10f) nextJumpVel.y = -10f;
                rb.velocity = nextJumpVel;
                t_hurtMode += Time.fixedDeltaTime;
                if (t_hurtMode >= 1.2f) {
                    if (MyInput.up || MyInput.down) {
                        hurtMode = 2;
                        t_hurtMode = 0;
                        animator.CrossFadeInFixedTime("GetUp", hurtAnimCrossfadeTime);
                    }
                }
                return;
            } else if (hurtMode == 2) {
                t_hurtMode += Time.fixedDeltaTime;
                if (t_hurtMode >= 1.3f) {
                    animator.CrossFadeInFixedTime("IdleHurt", hurtAnimCrossfadeTime);
                    t_hurtMode = 0;
                    hurtMode = 222;
                }
                return;
            } else if (hurtMode == 222) {
                t_hurtMode += Time.fixedDeltaTime;
                if (t_hurtMode >= 0.5f) {
                    t_hurtMode = 0;
                    hurtMode = 4;
                    CanShootSpark = true;
                }
                return;
            }

        }


		if (controlsOn) {
            CanShootSpark = true;
            // Movement controls for fixed camera ('hidden')
            if (camControl.fixedFollow && !IsRidescale) {
                holdingForward = false;
				float hor = MyInput.moveX;
				float vert = MyInput.moveY;
                if (MyInput.right) hor = 1;
                if (MyInput.left) hor = -1;

				Vector3 newvelhor = new Vector3();
				Vector3 newvelvert = new Vector3();
				Transform t = fixedHelperTransform.transform;
                // When camera enters fixed mode, the trigger that activated can force the velocity to still be 
                // relative to the entering camera rotation until player lets go of controller
                float threshold = 0.1f;
                if (Mathf.Abs(hor) <threshold && Mathf.Abs(vert) < threshold) {
                    forceFixedCamEulerYTillNotPressingDirection = false;
                }
                if (forceFixedCamEulerYTillNotPressingDirection) {
                    t.eulerAngles = new Vector3(0, forceFixedCamEulerY, 0);
                } else {
                    t.eulerAngles = new Vector3(0, camControl.transform.eulerAngles.y, 0);
                }

                if (hor < -threshold) {
					newvelhor = t.right*hor*maxMedVel;
				} else if (hor > threshold) {
					newvelhor = t.right*hor*maxMedVel;
				}
				if (vert < -threshold) {
					newvelvert = t.forward*vert*maxMedVel;
				} else if (vert > threshold) {
					newvelvert = t.forward*vert*maxMedVel;
				}


                if (Mathf.Abs(hor) >  threshold || Mathf.Abs(vert) > threshold) {
                    holdingForward = true;
                    animator.SetBool("Running", true);
                } else {
                    animator.SetBool("Running", false);
                }



                Vector3 newvel = newvelhor + newvelvert;
                newvel.y = rb.velocity.y;
                if (MyInput.shortcut) {
                    newvel.x = 6 * newvel.x;
                    newvel.z = 6 * newvel.z;
                }
                rb.velocity = newvel;



                if (IsRidescale) {
                    LimitRidescaleSpeed();
                } else {
                    AdjustWalkscaleVelocity();
                }

				// Get a quat with values equal to the velocity vector's XZ plane
				// Get the eulerangle of this and lerp the player's y-euler to this.
                // (FIXED CAM MODE)
				if (holdingForward) {
					newvel.Normalize();
					newvel.y = 0;
					Quaternion q = Quaternion.LookRotation(newvel);
					float eulerY = q.eulerAngles.y;
					transform.eulerAngles = new Vector3(0,Mathf.LerpAngle(transform.eulerAngles.y,eulerY,Time.fixedDeltaTime*15),0);



				}

				return;
			}

            // Movement controls for regular camera
            holdingForward = false;
            holdingBackward = false;

            // Hop = X, Cruise control = Z, accel = C

            if (IsRidescale) holdingForward = MyInput.ridescaleAccel || MyInput.moveY > 0.7f;
            if (IsRidescale) holdingBackward = MyInput.moveY < -0.7f || MyInput.cancel;

            if (!IsRidescale) holdingForward = MyInput.moveY > 0.1f;
            if (!IsRidescale) holdingBackward = MyInput.moveY < -0.1f;

            if (holdingBackward) holdingForward = false;

            // Set velocity
            if (IsRidescale) {

                if (engineSoundState == 0) {
                    if (t_engineIdle == 0) {
                        ridescaleAudioSrc.PlayOneShot(engineIdleClip);
                    }
                    t_engineIdle += Time.fixedDeltaTime;
                    if (t_engineIdle >= 5.7f) {
                        t_engineIdle = 0;
                    }
                    if (switchingSize) {
                        ridescaleAudioSrc.volume -= Time.fixedDeltaTime * 2f;
                    } else if (cruiseControlOn) {

                    } else if (holdingForward || holdingBackward) {
                        ridescaleAudioSrc.volume += Time.fixedDeltaTime / 3f;
                        if (ridescaleAudioSrc.volume > engineMoveVolume) ridescaleAudioSrc.volume = engineMoveVolume;
                    } else {
                        ridescaleAudioSrc.volume -= Time.fixedDeltaTime / 3f;
                        if (ridescaleAudioSrc.volume < engineIdleVolume) ridescaleAudioSrc.volume = engineIdleVolume;
                    }
                    if (camControl.fixedFollow) {
                        ridescaleAudioSrc.volume = 0;
                    }
                }

                Vector3 newvel = new Vector3();


                //animator.SetFloat("turn_dir", 0);
                RidescaleUpdateRotating(accelTime);

                if (!holdingBackward && !holdingBackward && accelTime == 0) {
                    tRidescaleTurnRightAngleAccel = 0;
                    tRidescaleTurnLeftAngleAccel = 0;
                    if (ridescaleIsAnimatingR || ridescaleIsAnimatingL) {
                        animator.CrossFadeInFixedTime("idle", 0.5f);
                    }
                    ridescaleIsAnimatingL = ridescaleIsAnimatingR = false;
                }

                if (holdingForward || holdingBackward || accelTime != 0) {
                    Vector3 newrot = transform.eulerAngles;
                    if (MyInput.right || MyInput.moveX > 0.33f) {
                        //animator.SetFloat("turn_dir", 1);
                        tRidescaleTurnLeftAngleAccel = 0;
                        if (!ridescaleIsAnimatingR && tRidescaleTurnRightAngleAccel > 0.05f) {
                            ridescaleIsAnimatingR = true;
                            animator.CrossFadeInFixedTime("turn_r", 0.6f);
                        }
                        tRidescaleTurnRightAngleAccel += Time.fixedDeltaTime;
                        if (tRidescaleTurnRightAngleAccel >= ridescaleAngleAccelTime) tRidescaleTurnRightAngleAccel = ridescaleAngleAccelTime;
                        newrot.y += Time.fixedDeltaTime * bigTurnSpeed * (tRidescaleTurnRightAngleAccel / ridescaleAngleAccelTime);
                    } else if (MyInput.left || MyInput.moveX < -0.33f) {
                        tRidescaleTurnRightAngleAccel = 0;

                        if (!ridescaleIsAnimatingL && tRidescaleTurnLeftAngleAccel > 0.05f) {
                            ridescaleIsAnimatingL = true;
                            animator.CrossFadeInFixedTime("turn_l", 0.6f);
                        }
                        tRidescaleTurnLeftAngleAccel += Time.fixedDeltaTime;
                        if (tRidescaleTurnLeftAngleAccel >= ridescaleAngleAccelTime) tRidescaleTurnLeftAngleAccel = ridescaleAngleAccelTime;
                        newrot.y -= Time.fixedDeltaTime * bigTurnSpeed * (tRidescaleTurnLeftAngleAccel / ridescaleAngleAccelTime);
                        //animator.SetFloat("turn_dir", -1);
                    } else {
                        tRidescaleTurnRightAngleAccel = 0;
                        tRidescaleTurnLeftAngleAccel = 0;
                        if (ridescaleIsAnimatingR || ridescaleIsAnimatingL) {
                            animator.CrossFadeInFixedTime("idle", 0.5f);
                        }
                        ridescaleIsAnimatingL = ridescaleIsAnimatingR = false;
                    }

                    transform.eulerAngles = newrot;
                    Vector3 xz = transform.forward.normalized * maxBigVel;

                    if (cruiseControlOn) {
                        // Exit cruise when wanting to accelerate or turn it off
                        if (MyInput.jpSpecial || MyInput.jpConfirm) {
                            MyInput.jpSpecial = false;
                            cruiseControlOn = false;
                            ui.TurnOffCruiseIcon();
                        }
                    } else {
                        if (MyInput.jpSpecial && accelTime > 0.25f) {
                            MyInput.jpSpecial = false;
                            ui.TurnOnCruiseIcon();
                            cruiseControlOn = true;
                        }
                        if (holdingBackward) {

                            accelTime -= 2 * Time.fixedDeltaTime;
                            if (accelTime > 0) accelTime -= 4 * Time.fixedDeltaTime;
                            if (accelTime < -2) accelTime = -2;
                            // Check backwards and up for obstacles, stop backing up if so (prevents model from clipping into walls because of how capsule collider is set up...
                            dummyV = Vector3.Lerp(transform.forward * -1, transform.up, 0.2f);
                            if (rb.SweepTest(dummyV, out tempHitInfo, 3.5f, QueryTriggerInteraction.Ignore)) {
                                accelTime = 0;
                            }
                        } else if (holdingForward) {
                            accelTime += Time.fixedDeltaTime;
                            if (accelTime < 1.3f) accelTime += 3 * Time.fixedDeltaTime;
                            if (accelTime > Big_TimeToMaxVel) accelTime = Big_TimeToMaxVel;
                        } else {
                            if (accelTime > 0) {
                                accelTime -= Time.fixedDeltaTime;
                                if (accelTime < 0) accelTime = 0;
                            } else if (accelTime < 0) {
                                accelTime += Time.fixedDeltaTime;
                                if (accelTime > 0) accelTime = 0;
                            }
                        }
                    }


                    newvel.x = xz.x * (accelTime / Big_TimeToMaxVel);
                    newvel.z = xz.z * (accelTime / Big_TimeToMaxVel);
                    newvel.y = rb.velocity.y;

                    MediumControl.carAccelTimeOnSceneExit = accelTime;
                    if (MediumControl.carAccelTimeOnSceneExit < 0) MediumControl.carAccelTimeOnSceneExit = 0;

                    float slopeAngle = getRidescaleSlopeAngle();
                    // When moving,  every .1s update the target vel for slopes, for smoother movement.
                    if (rb.velocity.magnitude > 1f && slopeAngle != -500f) {
                        if (HF.TimerDefault(ref t_updateRidePastY, 0.1f,Time.fixedDeltaTime)) {
                            float old_ridePastY = ridePastY;
                            ridePastY = transform.position.y;
                            if (old_ridePastY != -12345) {
                                float yChange = ridePastY - old_ridePastY;
                                float xzMag = Mathf.Sqrt(newvel.x * newvel.x + newvel.z * newvel.z);
                                // Slope velocity is only applied when y position is moving beyond a threshold.
                                if (Mathf.Abs(yChange) > 0.1f) {
                                    // Use trig to get a yVel that would make the entire vel parallel to slope.
                                    targ_rsAdjSlopeYVel = Mathf.Tan(Mathf.Deg2Rad * slopeAngle) * xzMag;
                                    // Apply this vel for this amount of time (outside this conditional)
                                    t_rsUseAdjSlopeYVel = 0.12f;
                                    if (yChange > 0.1f) {
                                        targ_rsAdjSlopeYVel = Mathf.Clamp(targ_rsAdjSlopeYVel, 0, 15f);
                                        rsMovingUpwards = true;
                                    } else if (yChange < -0.1f) {
                                        rsMovingUpwards = false;
                                        targ_rsAdjSlopeYVel = Mathf.Clamp(targ_rsAdjSlopeYVel, 0, 15f);
                                        targ_rsAdjSlopeYVel *= -1;
                                    }
                                }
                            }
                        }
                    } else {
                        // so the new slope yVel is never recalculated without two fresh values.
                        ridePastY = -12345;
                    }
                    
                    // This timer set during slope detection above.
                    if (t_rsUseAdjSlopeYVel > 0) {
                        // Only change the yVel for slopes if the current velocity is too small
                        if ((rsMovingUpwards && targ_rsAdjSlopeYVel > newvel.y) || (!rsMovingUpwards && targ_rsAdjSlopeYVel < newvel.y)) {
                            if (rsMovingUpwards && !isTouchingGround(0.2f)) {
                                targ_rsAdjSlopeYVel *= 0.5f;
                            }
                            cur_rsAdjSlopeYVel = Mathf.Lerp(cur_rsAdjSlopeYVel, targ_rsAdjSlopeYVel, 0.15f);
                            newvel.y = cur_rsAdjSlopeYVel;
                        }
                    }
                    if (t_rsUseAdjSlopeYVel > 0) t_rsUseAdjSlopeYVel -= Time.fixedDeltaTime;

                    // Rotate car. Backwards is janky. No rotation there.
                    target_rsXAngleTilt = Mathf.Clamp(slopeAngle, 0, 30f);
                    if (rsMovingUpwards) {
                        target_rsXAngleTilt *= -1;
                    }
                    if (holdingBackward) target_rsXAngleTilt = 0;
                    cur_rsXAngleTilt = Mathf.Lerp(cur_rsXAngleTilt, target_rsXAngleTilt, 0.043f);
                    tempRot = bigModelTransform.localEulerAngles;
                    tempRot.x = cur_rsXAngleTilt;
                    bigModelTransform.localEulerAngles = tempRot;

                    rb.velocity = newvel;
                }
                // Walkscale movement
            } else { 
                bool holdingLeft = MyInput.moveX < -horCutoff;
                bool holdingRight = MyInput.moveX > horCutoff;
                bool holdingHor = holdingLeft || holdingRight;

                if (camControl.t_YRotLerpR3 > 0) {
                    holdingLeft = holdingRight = holdingHor = false;
                    holdingForward = holdingBackward = false;

                }
                if (holdingForward || holdingBackward || holdingLeft || holdingRight) {
                    if (holdingForward || holdingBackward || (MyInput.controllerActive && (holdingLeft || holdingRight))) {
                        animator.SetBool("Running", true);
                    } else {
                        animator.SetBool("Running", false);
                    }
                    Vector3 newvel = rb.velocity;

                    // Get the normalized xz-plane vector of the caemra  - forward motion
                    Vector3 xz = new Vector3();
                    if (holdingForward) xz = Camera.main.transform.forward;
                    if (holdingBackward) xz = Camera.main.transform.forward * -1;

                    // Only changes velocity for controller. L/R on keyboard are rotation
                    Vector3 horPart = new Vector3();
                    if (holdingLeft) horPart = Camera.main.transform.right * -1;
                    if (holdingRight) horPart = Camera.main.transform.right;

                    // If controller is connected, the left stick should work a little weirder.
                    //if (Mathf.Abs(MyInput.moveX) > horCutoff) {
                    if (MyInput.controllerActive) {

                        // Right now, holding up-left gives you halfway between up and horizontal
                        if (Mathf.Abs(MyInput.moveX) > 0.05f && (holdingForward || holdingBackward)) {
                            xz = MyInput.moveX * Camera.main.transform.right + MyInput.moveY * Camera.main.transform.forward;
                            //xz = Vector3.Lerp(xz, horPart, Mathf.Abs(MyInput.moveX));
                        }
                        if (!(holdingForward || holdingBackward)) {
                            xz = horPart;
                        }
                    }

                    xz.y = 0;
                    xz.Normalize();
                    if (MyInput.shortcut) {
                        xz *= 10f;
                    }
                    
                    xz *= maxMedVel;

                    if (MyInput.controllerActive) {
                        float movementAmp = Mathf.Sqrt(MyInput.moveX * MyInput.moveX + MyInput.moveY * MyInput.moveY);
                        if (movementAmp > 1) movementAmp = 1;
                        float lowerCutoff = 0.4f;
                        float higherCutoff = 0.8f;
                        float slowestSpeed = 0.64f;
                        if (movementAmp < lowerCutoff) {
                            xz *= slowestSpeed;
                        } else if (movementAmp  < higherCutoff) {
                            xz *= slowestSpeed + (1 - slowestSpeed) * ((higherCutoff - movementAmp) / (higherCutoff - (higherCutoff-lowerCutoff)));
                        }
                    }
                    newvel.x = xz.x;
                    newvel.z = xz.z;
                    rb.velocity = newvel;

                    // Rotate player if moving forward/back
                    if (holdingForward || holdingBackward || holdingHor) {
                        float camEulerY = Camera.main.transform.eulerAngles.y;
                        // Forward, and maybe left/right w/ controller
                        if (MyInput.camX != 0) {
                            //print("camx not zero");
                        } else {
                            //print("camx zero");
                        }
                        if ((MyInput.controllerActive || (!MyInput.controllerActive && !holdingBackward)) && Mathf.Abs(MyInput.moveX) > 0.1f) {
                            Quaternion q = new Quaternion();
                            q.SetLookRotation(newvel); // Set q's look to the velocity of player
                            float yEulerFromVel = q.eulerAngles.y;
                            Vector3 newLook = Vector3.zero;
                            newLook.y = Mathf.LerpAngle(transform.eulerAngles.y, yEulerFromVel, Time.fixedDeltaTime * 10f);
                            transform.eulerAngles = newLook;
                        } else {
                            if (holdingBackward) {
                                camEulerY += 180f; if (camEulerY > 360f) camEulerY -= 360f;
                            }
                            transform.eulerAngles = new Vector3(0, camEulerY, 0);
                        }
                    }
                } else {
                    animator.SetBool("Running", false);
                }
            }


            if (IsRidescale) {
                LimitRidescaleSpeed();
            } else {
                AdjustWalkscaleVelocity();
            }

        }




    }

    float ridePastY = 0;
    float t_updateRidePastY = 0;
    float t_rsUseAdjSlopeYVel = 0;
    float targ_rsAdjSlopeYVel = 0;
    float cur_rsAdjSlopeYVel = 0;
    bool rsMovingUpwards = false;

    int walkscaleJumpState = 0;
    int ridescaleJumpState = 0;
    public float medJumpInitVel = 18f;
    public float medJumpHeight = 3f;
    public float bigJumpInitVel = 24f;
    public float bigJumpHeight = 4f;

    // time for boost to return to normal speed
    public float bigBoostTime = 1f;

    float FallDampenValue = 0.25f;
    Vector3 nextJumpVel = new Vector3(0, 0, 0);

    public int getJumpStateMed() { return walkscaleJumpState; }
    public int getJumpStateBig() { return ridescaleJumpState; }

    public float getVel(bool noY= false) {
        if (noY) {
            return Mathf.Sqrt(rb.velocity.x * rb.velocity.x + rb.velocity.z * rb.velocity.z);
        }
        return rb.velocity.magnitude;
    }

    float ignoreJumpLanding_Counter = 0;
    Vector3 tempbounceVel = new Vector3();
    public void DoBounce3D(Vector3 bounceVel) {
        if (hurtMode == 1 || hurtMode == 2 || hurtMode == 222) return;
        tempbounceVel = rb.velocity;
        tempbounceVel.y = 0;
        tempbounceVel += bounceVel;
        rb.velocity = tempbounceVel;
        nextJumpVel = rb.velocity;
        walkscaleJumpState = 1;
        camControl.startJumpCamMode();
        animator.SetTrigger("Jump");
        ignoreJumpLanding_Counter = 0.3f;
        doubleJumpParticles.Play();
        AudioHelper.instance.playOneShot("bounce3D");
    }
    bool myJPJump = false;
    bool myJump = false;
    void jumpLogic() {


        if (!IsRidescale) { 
            bool touchingGround = isTouchingGround();
         //   Debug.DrawRay(transform.position + cc.center * transform.localScale.y, -Vector3.up * (cc.bounds.extents.y + 0.05f));
            if (rb.velocity.y > 2) touchingGround = false;
            nextJumpVel = rb.velocity;
            float C_FallDampen = 1f;
            // When falling downwards, you can hold jump to fall slower
            if (myJump && rb.velocity.y < 0) {
                C_FallDampen = FallDampenValue ;
            }

            // Figure out how long to do half a jump based on initial vel and jump height
            float medJumpDeaccelTime = medJumpHeight * 2 / medJumpInitVel;
            // Calculate deceleration w/ that data
            float medJumpDeaccel = medJumpInitVel / medJumpDeaccelTime;

            // Stops you from floating weirdly when going up slopes
            if (walkscaleJumpState == 0) {
                rb.useGravity = true;
            } else {
                rb.useGravity = false;
            }
            if (ignoreJumpLanding_Counter > 0) {
                ignoreJumpLanding_Counter -= Time.fixedDeltaTime;
            }
            if (MyInput.shortcut && myJump) {
                if (walkscaleJumpState ==1 || walkscaleJumpState == 2) {
                    nextJumpVel.y = medJumpInitVel * 1.5f;
                }
            }
            if (walkscaleJumpState == 0) { // On ground
                if (nojumpticks > 0) nojumpticks--;
                if (myJPJump && nojumpticks <= 0) {
                    MyInput.jpJump = false;
                    if (!OnMovingPlatform) camControl.startJumpCamMode();
                    nextJumpVel.y = medJumpInitVel;
                    if (MyInput.shortcut) nextJumpVel.y *= 1.5f;
                    walkscaleJumpState = 1;
                    AudioHelper.instance.playOneShot("playerJump");
                    animator.SetTrigger("Jump");
                } else if (!touchingGround) {
                    nextJumpVel.y -= Time.fixedDeltaTime * medJumpDeaccel ;
                }
            } else if (walkscaleJumpState == 1) { //First jump, wait for gruond or double jump
                if (!(MyInput.shortcut && myJump)) {
                    nextJumpVel.y -= Time.fixedDeltaTime * medJumpDeaccel * C_FallDampen;
                }

                if (myJump && nextJumpVel.y < -1f) {
                    if (!HoverParticles.isPlaying) {
                        HoverParticles.Play();
                    }
                } else {
                    if (HoverParticles.isPlaying) {
                        HoverParticles.Stop();
                    }
                }

                if (ignoreJumpLanding_Counter <= 0 &&  touchingGround) {

                    HoverParticles.Stop();
                    camControl.stopJumpCamMode();
                    nextJumpVel.y = 0;
                    walkscaleJumpState = 0;
                } else if (myJPJump && !useHurtMotion) {
                    MyInput.jpJump = false;
                    doubleJumpParticles.Play();
                    HoverParticles.Stop();
                    AudioHelper.instance.playOneShot("playerJump",1,1.08f);
                    animator.SetTrigger("Jump");
                    nextJumpVel.y = medJumpInitVel;
                    walkscaleJumpState = 2;
                }
            } else if (walkscaleJumpState == 2) { // Second jump, wait for ground.
                if (!(MyInput.shortcut && myJump)) {
                    nextJumpVel.y -= Time.fixedDeltaTime * medJumpDeaccel * C_FallDampen;
                }
                if (myJump && nextJumpVel.y < -1f) {
                    if (!HoverParticles.isPlaying) {
                        HoverParticles.Play();
                    }
                } else {
                    if (HoverParticles.isPlaying) {
                        HoverParticles.Stop();
                    }
                }
                if (ignoreJumpLanding_Counter <= 0 && touchingGround) {
                    HoverParticles.Stop();
                    camControl.stopJumpCamMode();
                    nextJumpVel.y = 0;
                    walkscaleJumpState = 0;
                }
            }

            if (nextJumpVel.y < -0.5f && !isTouchingGround(1f)) {
                animator.SetBool("Falling", true);
            } else if ((animator.GetCurrentAnimatorStateInfo(0).IsName("JumpUp") || animator.GetCurrentAnimatorStateInfo(0).IsName("JumpUpHurt")) && nextJumpVel.y <= 0 && isTouchingGround(0.5f)) {
                // For reaching a platform but having tiny velocity
                animator.SetBool("Falling", true);
            } else {
                animator.SetBool("Falling", false);
            }

            if (myJump && nextJumpVel.y < -7f) nextJumpVel.y = -7f; // Allow gliding when jump is held
            rb.velocity = nextJumpVel;

            if (nextJumpVel.y < -maxMedFallVel) {
                nextJumpVel.y = -maxMedFallVel;
                rb.velocity = nextJumpVel;
            }
        } else if (IsRidescale) {
            //bool nearGroundAndFalling = isTouchingGround(0.75f) && rb.velocity.y <= -2;
            nextJumpVel = rb.velocity;
            if (!isTouchingGround()) {
                nextJumpVel.y -= Time.fixedDeltaTime * 40f;
            } else {
                nextJumpVel.y = 0;
            }
            if (nextJumpVel.y < -40) nextJumpVel.y = -40;
            rb.velocity = nextJumpVel;
            //print(rb.velocity);
        }


    }


    void Update () {
		bool playSounds = true;
		bool playAnimations = true;
        //bool controlsOn = true;
        if (MyInput.jpJump) {
            myJPJump = true;
        }
        myJump = MyInput.jump;

        if (doSpinOutAfterNano) {
            return;
        }
        if (isThereAnyReasonToPause()) {
            playAnimations = false;
			playSounds = false;
          //  controlsOn = false;
            //boostSrc1.volume -= Time.deltaTime; boostSrc2.volume -= Time.deltaTime;
        }


        //if (controlsOn && (hurtMode == 0 || hurtMode == 10 || hurtMode == 3 || hurtMode == 4)) {
        //    jumpLogic();
        //}


        if (playSounds) {
			updateSounds();
		}

		if (playAnimations) {
		}
	}


    Vector3 tempRideVel = new Vector3();
    void LimitRidescaleSpeed() {
        tempRideVel = rb.velocity;
        float cachedYVel = tempRideVel.y;
        tempRideVel.y = 0;
        if (tempRideVel.magnitude > maxBigVel) {
            tempRideVel.Normalize();
            tempRideVel *= maxBigVel;
        }
        if (useHurtMotion) tempRideVel *= 0.7f;
        float slopeAngle = getRidescaleSlopeAngle();
        // Stop ridescale from climbing > 60deg slopes
        // but allow some 'error time' for miscalclations
        if (slopeAngle == -500) {
            rs_t_withNoSlope += Time.fixedDeltaTime;
            if (rs_t_withNoSlope < 0.1f) {
                slopeAngle = 0;
            }
        } else {
            rs_t_withNoSlope = 0;
        }
        if (slopeAngle == -500f || slopeAngle > 50f) {
            if (cachedYVel > 0) cachedYVel = -3f;
            if (rs_lastY < transform.position.y) {
                tempPosRSLimit = transform.position;
                tempPosRSLimit.y = rs_lastY;
                transform.position = tempPosRSLimit;
            }
        }
        rs_lastY = transform.position.y;
        tempRideVel.y = cachedYVel;
        rb.velocity = tempRideVel;
    }
    float rs_t_withNoSlope = 0;
    float rs_lastY = 0;
    Vector3 tempPosRSLimit = new Vector3();
    Vector3 tempWalkVel = new Vector3();
    Vector3 tempWalkVel_NoY = new Vector3();
    Vector3 origWalkVel_NEVERCHANGES = new Vector3();
    Vector3 origWalkVel_NEVERCHANGES_NoY = new Vector3();


    void AdjustWalkscaleVelocity() {
        tempWalkVel = rb.velocity;
        origWalkVel_NEVERCHANGES = rb.velocity;
        origWalkVel_NEVERCHANGES_NoY = rb.velocity; origWalkVel_NEVERCHANGES_NoY.y = 0;

        // Slow down 
        if (Mathf.Abs(MyInput.moveY) < 0.2f) {
            tempWalkVel.z /= 1.2f;
            tempWalkVel.x /= 1.2f;
        }


        int layerMask = 1 | 1 << 10; // only detect these
        tempWalkVel_NoY.Set(tempWalkVel.x, 0, tempWalkVel.z);

        // Add a helper y-boost if almost at the lip of something and holding forward/jump.
        // Only do this if your bottom is hitting and your top is not, and you are hitting a vertical-ish wall
        bool gotYBoost = false;
        if (myJump && (tempWalkVel_NoY.x != 0 || tempWalkVel_NoY.z != 0)) {
            // From bottom of collider, towards velocity
            Ray yBoostray = new Ray(transform.position + new Vector3(0, -cc.bounds.extents.y, 0), tempWalkVel_NoY);
    
           // Debug.DrawRay(yBoostray.origin, yBoostray.direction * 3f, Color.blue);
           // Debug.DrawRay(yBoostray.origin + new Vector3(0, 0.8f, 0), yBoostray.direction * 3f, Color.blue);

            // Cast the above ray, then cast again 0.8 units up
            if (Physics.Raycast(yBoostray, out tempHitInfo, 1f, layerMask, QueryTriggerInteraction.Ignore)) {
                if (Vector3.Angle(tempHitInfo.normal, new Vector3(tempHitInfo.normal.x, 0, tempHitInfo.normal.z)) < 5f) {
                    yBoostray.origin = yBoostray.origin + new Vector3(0, 0.8f, 0);
                    if (!Physics.Raycast(yBoostray, 1f, layerMask, QueryTriggerInteraction.Ignore)) {
                        tempWalkVel = tempWalkVel + new Vector3(0, 1f, 0);
                        if (tempWalkVel.y <= 1) tempWalkVel.y = 1f;
                        tempWalkVel.x = 0; tempWalkVel.z = 0;
                        gotYBoost = true;
                    }
                }
            }
        }


        // Move transform backwards, then sweep test forwards.
        // Set x and z vel to zero if a hit detected. Should prevent issues with physics stopping all v movement, even
        // in the case where collider is exactly up against the wall.
        tempV1 = transform.position;
        tempV2 = transform.position;
        tempV1 -= transform.forward.normalized;
        transform.position = tempV1;

        if (rb.SweepTest(transform.forward, out tempHitInfo, 1.2f,QueryTriggerInteraction.Ignore)) {
            float slopeAngle = Vector3.Angle(tempHitInfo.normal, Vector3.up);
            if ((holdingForward  || holdingBackward) && slopeAngle > 45f && walkscaleJumpState != 0) {
                tempWalkVel.x = 0;
                tempWalkVel.z = 0;
            }
        }
        transform.position = tempV2;

        SetCapsuleCastVars();
        // Capsule cast to check if a wall is hit. If so, then change velocity to the closer of the normal's two perp vectors to push along wall
        float castDistance = 0.5f;
        
		if (!gotYBoost && Physics.CapsuleCast(ccp1,ccp2, ccRadiusWorld, origWalkVel_NEVERCHANGES,out tempHitInfo, castDistance,layerMask,QueryTriggerInteraction.Ignore)) {
			// Raise the capsule cast a little bit - if there's no hit that means this was probably a gentle slope
			ccp1.y += 0.86f;
			ccp2.y += 0.86f;
			if (Physics.CapsuleCast(ccp1,ccp2, ccRadiusWorld, origWalkVel_NEVERCHANGES, out tempHitInfo, castDistance,layerMask,QueryTriggerInteraction.Ignore)) {
                normalWithoutY = tempHitInfo.normal;
                normalWithoutY.y = 0;

                // if you are coming too straight-on at the wall dont slide along wall
                if (Vector3.Angle(normalWithoutY * -1, origWalkVel_NEVERCHANGES_NoY) < 20f) {
                    tempWalkVel.x = 0; tempWalkVel.z = 0;
                } else {
                    perp1.Set(tempHitInfo.normal.z, 0, -tempHitInfo.normal.x);
                    perp2.Set(-tempHitInfo.normal.z, 0, tempHitInfo.normal.x);
                   // Debug.DrawRay(hitInfo.point, hitInfo.normal * 3f, Color.green);
                    float p1v = Vector3.Angle(perp1, origWalkVel_NEVERCHANGES_NoY);
                    float p2v = Vector3.Angle(perp2, origWalkVel_NEVERCHANGES_NoY);
                    float velMag = origWalkVel_NEVERCHANGES_NoY.magnitude;
                    perp1.Normalize();
                    perp2.Normalize();
                    if (p1v < p2v) {
                        origWalkVel_NEVERCHANGES_NoY = velMag * perp1;
                    } else {
                        origWalkVel_NEVERCHANGES_NoY = velMag * perp2;
                    }
                    tempWalkVel.x = origWalkVel_NEVERCHANGES_NoY.x;
                    tempWalkVel.z = origWalkVel_NEVERCHANGES_NoY.z;
                }
            }
		}
        if (useHurtMotion) {
            tempWalkVel.x *= hurtSlowDown;
            tempWalkVel.z *= hurtSlowDown;
        }
		rb.velocity = tempWalkVel;
		
	}
    RaycastHit tempHitInfo = new RaycastHit();
    Vector3 tempV1 = new Vector3();
    Vector3 tempV2 = new Vector3();
    Vector3 perp1 = new Vector3();
    Vector3 perp2 = new Vector3();
    Vector3 normalWithoutY = new Vector3();
    void updateSounds() {
        if (camControl.fixedFollowTopDown) return;
	}

    float cccc = 1.5f;
    private void OnCollisionStay(Collision collision) {
        if (walkscaleJumpState != 0 && !IsRidescale && MyInput.moveY > 0) {
            rb.velocity = nextJumpVel;
            Vector3 newpos = transform.position;
            if (rb.velocity.y > 0) {
                newpos.y += cccc*Time.deltaTime;
            } else if (rb.velocity.y < -1) {
                newpos.y -= 2.5f*cccc * Time.deltaTime;
            }
            transform.position = newpos;
        }
    }

    public void PlaySparkAnim() {
        animator.Play("Spark", animator.GetLayerIndex("Spark Layer"));
    }

    float tSizeSwitch = 0;
    float tmSizeSwitch = 0.42f;
    float sizeSwitch_EulerStart = 0;
    float sizeSwitch_TotalRotation = 720f;
    float sizeSwitch_StartScaleWalk = 0.5f;
    float sizeSwitch_StartScaleRide = 1f;
    float sizeSwitch_MinScaleWalk = 0.11f;
    float sizeSwitch_MinScaleRide = 0.11f;
    Renderer[] sizeSwitch_MRs;
    Color tempCol = new Color();
    Color emissiveCol = new Color();
    void UpdateSizeSwitch() {
        if (switchSizeMode == 1) {
            shrinkSparkle.Play();
            if (!IsRidescale) {
                animator.CrossFadeInFixedTime("Armature|Spin", 0.1f);
                sizeSwitch_EulerStart = transform.localEulerAngles.y;
            }
            switchSizeMode = 2;
            tSizeSwitch = 0;

        } else if (switchSizeMode == 2) {
            tSizeSwitch += Time.deltaTime;
            float r = tSizeSwitch / tmSizeSwitch;
            // Scale
            if (IsRidescale) {
                tempV1 = bigModelTransform.localScale;
                tempV1.z = Mathf.Lerp(sizeSwitch_StartScaleRide, sizeSwitch_MinScaleRide, r);
                tempV1.x = tempV1.y = tempV1.z;
                bigModelTransform.localScale = tempV1;
            } else {
                tempV1 = transform.localScale;
                tempV1.z = Mathf.Lerp(sizeSwitch_StartScaleWalk, sizeSwitch_MinScaleWalk, r);
                tempV1.x = tempV1.y = tempV1.z;
                transform.localScale = tempV1;
            }
            // color
            tempCol = Color.Lerp(Color.black, emissiveCol, r);
            for (int i =0; i < sizeSwitch_MRs.Length; i++) {
                sizeSwitch_MRs[i].material.SetColor("_EmissionColor", tempCol);
            }
            // Rotation (Walk only)
            if (!IsRidescale) {
                tempV1 = transform.localEulerAngles;
                tempV1.y = Mathf.Lerp(sizeSwitch_EulerStart, sizeSwitch_EulerStart + sizeSwitch_TotalRotation, r);
                transform.localEulerAngles = tempV1;
            }
        

            if (tSizeSwitch > tmSizeSwitch) {
                switchSizeMode = 3;
                shrinkSparkle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        } else if (switchSizeMode == 3) {
            switchSizeMode = 0;
            switchingSize = false;
        }

        // Grow
        if (switchSizeMode == 10) {
            switchSizeMode = 11;
            tSizeSwitch = 0;
            switchSizeMode = 11;
            shrinkSparkle.Play();
            if (!IsRidescale) {
                animator.Play("Armature|Spin");
                sizeSwitch_EulerStart = Camera.main.transform.localEulerAngles.y;
            } else {
                rs_lastY = transform.position.y + 0.2f;
                animator.Play("transform");
            }
        } else if (switchSizeMode == 11) {
            tSizeSwitch += Time.deltaTime;
            float r = tSizeSwitch / tmSizeSwitch;
            tempV1 = transform.localScale;
            if (IsRidescale) tempV1 = bigModelTransform.localScale;
            if (!IsRidescale) tempV1.z = Mathf.Lerp(sizeSwitch_MinScaleWalk, sizeSwitch_StartScaleWalk, r);
            if (IsRidescale) tempV1.z = Mathf.Lerp(sizeSwitch_MinScaleRide, sizeSwitch_StartScaleRide, r);
            tempV1.x = tempV1.y = tempV1.z;
            if (IsRidescale) bigModelTransform.localScale = tempV1;
            if (!IsRidescale) transform.localScale = tempV1;


            // color
            tempCol = Color.Lerp(emissiveCol,Color.black, r);
            for (int i = 0; i < sizeSwitch_MRs.Length; i++) {
                sizeSwitch_MRs[i].material.SetColor("_EmissionColor", tempCol);
            }

            if (!IsRidescale) {
                tempV1 = transform.localEulerAngles;
                tempV1.y = Mathf.Lerp(sizeSwitch_EulerStart, sizeSwitch_EulerStart + sizeSwitch_TotalRotation, r);
                transform.localEulerAngles = tempV1;
            }

            if (tSizeSwitch > tmSizeSwitch) {
                switchSizeMode = 13;
                shrinkSparkle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        } else if (switchSizeMode == 13) {
            if (!IsRidescale) {
                if (isHurt) {
                    if (hurtMode == 10) {
                        animator.CrossFadeInFixedTime("CollapseIdle", 0.1f);
                    } else {
                        animator.CrossFadeInFixedTime("IdleHurt", 0.1f);
                    }
                } else {
                    animator.CrossFadeInFixedTime("Idle", 0.1f);
                }
            } 
            if (IsRidescale) animator.CrossFadeInFixedTime("idle", 0.6f);
            switchSizeMode = 0;
            switchingSize = false;
        }
    }

    int returnFromNanoMode = 0;
    Transform novaFBX;
    float rfnStartY = 6;
    Vector3 tempScaleRFN = new Vector3();
    Vector3 tempPosRFN = new Vector3();
    float rfnInitialWaitTime = 0.5f;
    float tmRFNSpin = 2f;
    public static bool doSpinOutAfterNano = false;
    public static bool doSpinOutAfterNanoDuringPause = false;
    Vector3 RFNStartOffset = new Vector3();
    [System.NonSerialized]
    public int hurtMode;
    [System.NonSerialized]
    public float t_hurtMode;
    private float t_hurtWalkWait;
    private bool ridescaleIsAnimatingL;
    private bool ridescaleIsAnimatingR;
    private float tRidescaleTurnLeftAngleAccel;
    private float tRidescaleTurnRightAngleAccel;

    public float ridescaleAngleAccelTime = 0.4f;
    public static bool ignoreMedInput = false;
    private float t_hurtPoofer;

    void UpdateReturnFromNanoSpin() {

        if (returnFromNanoMode == 0) {
            novaFBX = transform.Find("NovaFBX");
            tempScaleRFN = Vector3.zero;
            novaFBX.localScale = tempScaleRFN;
            returnFromNanoMode = 1;
            if (shrinkSparkle == null) shrinkSparkle = transform.Find("ShrinkSparkle").GetComponent<ParticleSystem>();
            shrinkSparkle.transform.parent = null;
        } else if (returnFromNanoMode == 1) {
            if (!ui.isFading()) {
                tempPosRFN = novaFBX.localPosition;
                tempPosRFN.y = rfnStartY;
                novaFBX.localPosition = tempPosRFN;
                RFNStartOffset = transform.forward * 5f;

                rfnInitialWaitTime -= Time.deltaTime;
                if (rfnInitialWaitTime < 0) {
                    rfnInitialWaitTime = 0.5f;
                    returnFromNanoMode = 10;
                }
            }
        } else if (returnFromNanoMode == 10) {
            returnFromNanoMode = 11;
            AudioHelper.instance.playOneShot("ridescaleTransform");
            tSizeSwitch = 0;
            shrinkSparkle.Play();
            animator.Play("Armature|Spin");
        } else if (returnFromNanoMode == 11) {
            tSizeSwitch += Time.deltaTime;
            float r = tSizeSwitch / tmRFNSpin;
            if (r > 1) r = 1;
            // scale - scale fbx
            tempV1 = novaFBX.localScale;
            tempV1.z = Mathf.SmoothStep(0, 1, r);
            tempV1.x = tempV1.y = tempV1.z;
            novaFBX.localScale = tempV1;

            // Reset local x and z of FBX to their initial state.
            tempPosRFN.Set(0, 0, 0);
            novaFBX.localPosition = tempPosRFN;

            // Set the FBX's world position with the offset-towards-return-target
            novaFBX.position += RFNStartOffset * (1 - r);

            // Convert to local coordinates.
            tempPosRFN.x = novaFBX.localPosition.x;
            tempPosRFN.z = novaFBX.localPosition.z;

            // Recalculate the local y coordinate
            tempPosRFN.y = Mathf.SmoothStep(rfnStartY, -1, r);

            novaFBX.localPosition = tempPosRFN;



            // color
            tempCol = Color.Lerp(emissiveCol,Color.black, r);
            for (int i = 0; i < sizeSwitch_MRs.Length; i++) {
                sizeSwitch_MRs[i].material.SetColor("_EmissionColor", tempCol);
            }
            // Angle - rotate root transform
            tempV1 = novaFBX.localEulerAngles;
            tempV1.y = Mathf.SmoothStep(0, sizeSwitch_EulerStart + sizeSwitch_TotalRotation*3f, r);
            novaFBX.localEulerAngles = tempV1;
            if (tSizeSwitch > tmRFNSpin) {
                returnFromNanoMode = 13;
                shrinkSparkle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }

            shrinkSparkle.transform.position = novaFBX.position;
        } else if (returnFromNanoMode == 13) {
            animator.CrossFadeInFixedTime("Idle", 0.5f);
            returnFromNanoMode = 14;
            tSizeSwitch = 0;
        } else if (returnFromNanoMode == 14) {
            tSizeSwitch += Time.deltaTime;
            if (tSizeSwitch > 0.5f) {
                tSizeSwitch = 0;
                MediumControl.doSpinOutAfterNano = false;
                MediumControl.doSpinOutAfterNanoDuringPause = false;
                returnFromNanoMode = 0;
            }
        }
    }

    public void StopRunning() {
        animator.SetBool("Running", false);
    }

    private void OnTriggerEnter(Collider other) {
        if (!IsRidescale) {
            if (other.gameObject.layer == 22) {
                AudioHelper.instance.playOneShot("splash",0.45f,0.9f,3);
                SplashParticles.Play();
                WaterRingParticles.Play();
            }
        }
    }
    private void OnTriggerExit(Collider other) {
        if (!IsRidescale) {
            if (other.gameObject.layer == 22) {
                if (rb.velocity.y > 2f) {
                    SplashParticles.Play();
                    AudioHelper.instance.playOneShot("splash", 0.35f, 0.8f, 3);
                }
                WaterRingParticles.Stop(true,  ParticleSystemStopBehavior.StopEmitting);
            }
        }
    }
}
