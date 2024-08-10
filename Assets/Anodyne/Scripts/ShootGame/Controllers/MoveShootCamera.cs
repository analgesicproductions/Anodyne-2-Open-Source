using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveShootCamera : MonoBehaviour {

    MoveShootController playerController;
	bool lerpCamPos = false;
	Vector3 posFromTrigger;
	Vector3 STARTposFromTrigger;
	Vector3 rotFromTrigger;
	Vector3 STARTrotFromTrigger;
	Camera cam;

	float triggerTransitionTime;
	float maxTTT;


	public bool fixedFollow = false; // Set this to have the camera stay in one spot and track the target
	public bool fixedFollowTopDown = false; // Set this to have the camera follow the player like at opdown cam
	Vector3 camXZ;
	Vector3 playerXZ;
	public bool fixedToSpline = false;
	public bool initSplineStuff = true;
	BGCurveMetadata splineMeta;
	//float _bgCurDis = 0f;

//	SceneData sceneData;

	GameObject target;
	public float rotateSpeed = 5;
    Vector3 offset;
	public Vector3 mediumOffset;

    GameObject MediumPlayer;

	// Use this for initialization
	void Start () {
        offset = mediumOffset;
		camXZ = new Vector3();
		playerXZ = new Vector3();

        MediumPlayer = GameObject.Find("MoveShootPlayer");
        playerController = MediumPlayer.GetComponent<MoveShootController>();
        target = MediumPlayer;


		if (fixedFollow == false) {
			transform.position = target.transform.position + offset;
		}
		targetPos = transform.position;

		//sceneData = GameObject.Find("SceneData").GetComponent<SceneData>();
		cam = GetComponent<Camera>();

	}
	float lerpTime = 3f;
	float lerpTimer = 0f;
	int zRotate_State = 0;
	float yRotationAroundTarget = 0f;
	float unrotate_delay = 1.5f;
	float extra_z = 0f; // Set by Vertical axis = basically changing vertical camera

	float lowerVertAngleLimit = -60f;
	float upperVertAngleLimit = 10f;
	float verticalCamSpeedMultiplier = 0.32f;
	float horizontalCamSpeedMultiplier = 0.43f;

    // Variables that help with smoothing the camera movement when entering aim mode
    float t_aimingYLerp = 0f;
    float tm_aimingYLerp = 0.6f;
    float t_aimingYTargetOff = 1.2f;

    Vector3 targetPos;
	Vector3 nextTargetPos;


	void switchModeInitialization(bool _spline, bool _fixed, float _t) {
		fixedToSpline = _spline; fixedFollow = _fixed; 
		triggerTransitionTime = 0; 
		maxTTT = _t; 
		lerpCamPos = true;
		if (fixedToSpline) {
			initSplineStuff = true;
		}
	}

	public void switchToFollow(float _transitionTime) {
		if (!(fixedToSpline || fixedFollow)) return;

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


	bool didInit = false;
	void LateUpdate() {

        
		if (CutsceneManager.deactivateCameras || SaveModule.saveMenuOpen) { // || DataLoader.instance.isPaused) {
			return;
		}

		if(!didInit) {
			didInit = true;
			targetPos = target.transform.position; // Camera follows this
			// target always LERPs to this, nextTarget only updates when player leaves deadzone
			nextTargetPos = target.transform.position; 
		}

        offset = mediumOffset;



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

		nextTargetPos.x = target.transform.position.x;
		nextTargetPos.z = target.transform.position.z;

		Vector3 viewportPos = Camera.main.WorldToViewportPoint(target.transform.position);

		// When exiting the y-deadzone, set a flag...
		if (viewportPos.y < .4f || viewportPos.y > .5f) {
			//nextTargetPos.y = target.transform.position.y;
			//targetPos = Vector3.Lerp(targetPos,nextTargetPos,3*Time.deltaTime);
		}


		targetPos.y = target.transform.position.y;
        // If the player is aiming, slowly make the camera focus on a point above the head so you can see better
        if (playerController.isAiming()) {
            t_aimingYLerp += Time.deltaTime;
            if (t_aimingYLerp > tm_aimingYLerp) {
                t_aimingYLerp = tm_aimingYLerp;
            } 
            targetPos.y += Mathf.SmoothStep(0,t_aimingYTargetOff,t_aimingYLerp/tm_aimingYLerp);
        } else {
            if (t_aimingYLerp > 0) {
                t_aimingYLerp -= Time.deltaTime;
                targetPos.y += Mathf.SmoothStep(0, t_aimingYTargetOff, t_aimingYLerp / tm_aimingYLerp);
            }
        }

		// Remove these two lines to get a generally 'laggy' camera
		targetPos.x = nextTargetPos.x;
		targetPos.z = nextTargetPos.z;


		// Rotate camera slowly if holding fly

		if (zRotate_State == 0) {
			if (MyInput.jump) {
			//	zRotate_State = 1;
			}
		// rotate while jump held.	
		} else if (zRotate_State == 1) {
			if (!MyInput.jump) {
				zRotate_State = 2;
				unrotate_delay = 0.3f;
			} else {
				lerpTimer += Time.deltaTime;
				if (lerpTimer > lerpTime) lerpTimer = lerpTime;
			}
		} else if (zRotate_State == 2) {
			unrotate_delay -= Time.deltaTime;
			if (unrotate_delay <= 0) {
				lerpTimer -= Time.deltaTime;
				if (lerpTimer <= 0) {
					lerpTimer = 0;
					zRotate_State = 0;
				}
			} else if (MyInput.jump) {
				zRotate_State = 1;
			}
		}



		float horizontal = 0;
        if (MyInput.right) MyInput.moveX = 1;
        if (MyInput.left) MyInput.moveX = -1;
		if (Mathf.Abs(MyInput.moveX) > 0.2f) horizontal = MyInput.moveX * rotateSpeed * horizontalCamSpeedMultiplier;
		horizontal *= (SaveManager.sensitivity / 100f);

        if (playerController.isAiming()) horizontal = 0; // No rotation while aiming.

		yRotationAroundTarget += horizontal;


		float desiredYAngle = yRotationAroundTarget;


		float vertical = 0;
		if (MyInput.up) vertical = 1 * rotateSpeed * verticalCamSpeedMultiplier;
		if (MyInput.down) vertical = -1* rotateSpeed * verticalCamSpeedMultiplier;
		vertical *= (SaveManager.sensitivity / 100f);
        if (playerController.isAiming()) vertical = 0; // No rotation while aiming.

        extra_z += vertical;
		if (extra_z <= lowerVertAngleLimit) extra_z = lowerVertAngleLimit;
		if (extra_z >= upperVertAngleLimit)  extra_z = upperVertAngleLimit;

        Quaternion rotation = Quaternion.Euler(extra_z, desiredYAngle, 0);

		// Use lerpTimer to change the zoom, ypanning and FOV of the camera
		float zoomMultiplier = 1f;
		zoomMultiplier = Mathf.SmoothStep(1f,1.1f,lerpTimer/lerpTime);


		// Move camera away from player and look at it.
		transform.position = targetPos - (rotation * offset * zoomMultiplier);
		transform.LookAt(targetPos);

		// If there's a really low to ground angle move the camera up some
		float percentFromLowestAngle = (extra_z-lowerVertAngleLimit) / (upperVertAngleLimit-lowerVertAngleLimit);
		Vector3 _final = transform.position;
		_final.y += finalYOffset * (1 - percentFromLowestAngle);
		transform.position = _final;	


		// Move to player if wall in the way
		RaycastHit hit = new RaycastHit();
		Ray ray = new Ray (targetPos, transform.position - targetPos);
		Debug.DrawRay(ray.origin,10*ray.direction,Color.red);
		if (Physics.Raycast (ray,out hit,offset.magnitude,1<<0,QueryTriggerInteraction.Ignore)) {
			Vector3 diff = hit.point - targetPos;
			// Move the camera position a little away frmo the collision to help with avoiding clipping
			diff *= noClipFac;
			targetPos += diff;
			transform.position = targetPos;
		}

		
		//Vector3 rightMid = cam.ViewportToWorldPoint(new Vector3(1, 0.5, cam.nearClipPlane));
		Vector3 center = cam.ViewportToWorldPoint(new Vector3(0.5f,0.5f,cam.nearClipPlane));
		Vector3 leftMid = cam.ViewportToWorldPoint(new Vector3(0, 0.5f, cam.nearClipPlane));
		Vector3 rightMid = cam.ViewportToWorldPoint(new Vector3(1, 0.5f, cam.nearClipPlane));
		ray = new Ray(center,leftMid - center);
		if (Physics.Raycast(ray,out hit,Vector3.Distance(center,leftMid),1<<0,QueryTriggerInteraction.Ignore)) {
			Vector3 hitOffset = hit.point - leftMid;
			targetPos = transform.position;
			targetPos += hitOffset*1.25f;
			transform.position = targetPos;
		}


		ray = new Ray(center,rightMid - center);
		if (Physics.Raycast(ray,out hit,Vector3.Distance(center,rightMid ),1<<0,QueryTriggerInteraction.Ignore)) {
			Vector3 hitOffset = hit.point - rightMid;
			targetPos = transform.position;
			targetPos += hitOffset*1.25f;
			transform.position = targetPos;
		}

		
		



		if (lerpCamPos) {


			float _t = Mathf.SmoothStep(0,1,triggerTransitionTime/maxTTT);
			triggerTransitionTime += Time.deltaTime;
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

	float finalYOffset = 2.5f;
	float noClipFac = 0.77f;

}
