using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveShootController : MonoBehaviour {

    CapsuleCollider cc;
    MoveShootCamera camControl;
    Camera cam;
    Animator animator;
    public Rigidbody rb;
    bool wasPaused = false;

    public float speedMultiplier = 3f;
	public float jumpSpeed = 20f;
	public float boostSpeed = 20f;
	public float maxVertVel = 20f;
	public float maxFallVel = 20f;
	public float maxHorVel = 10f;
	float tm_jetpack = 0.25f;
	float t_jetpack = 0f;

    int fixedUpdateMode = 0;


    void Start () {
		rb = gameObject.GetComponent<Rigidbody>();	
		cc = GetComponent<CapsuleCollider>();

		camControl = GameObject.Find("Main Camera").GetComponent<MoveShootCamera>();
        cam = camControl.GetComponent<Camera>();

		// If a door script from the previous scene has a destination door, move the player
		if (Registry.destinationDoorName != "" ) {
			Transform t = GameObject.Find(Registry.destinationDoorName).GetComponent<Transform>();
			transform.position = t.position;
			Registry.destinationDoorName = "";
		}

		if (Registry.justLoaded) {
			transform.position = Registry.enterGameFromLoad_Position;
			Registry.justLoaded = false;
		}
	}



	void FixedUpdate() {
		bool controlsOn = true;
		if (CutsceneManager.deactivatePlayer) controlsOn = false;
		if (SaveModule.saveMenuOpen) controlsOn = false;
		if (DataLoader.instance.isPaused) {
			controlsOn = false;
			rb.isKinematic = true;
			wasPaused = true;
		} else {
			if (wasPaused) {
				rb.isKinematic = false;
				wasPaused = false;
			}
		}


		if (controlsOn) {
            if (fixedUpdateMode == 0) {
                updateGroundMovement();
            } else if (fixedUpdateMode == 1) {
            }
		}
	}
    

    void updateGroundMovement() {

        

        bool touchingGround = isTouchingGround();
        // Jumping
        if (MyInput.jump && !camControl.fixedFollowTopDown) {
            t_jetpack += Time.fixedDeltaTime;
            if (t_jetpack <= tm_jetpack) {
                rb.AddForce(0, jumpSpeed, 0);
            }
        } else {
            // When not boosting upwards but in the air, increase downwards vel
            if (t_jetpack > 0 || !touchingGround) {
                rb.AddForce(0, -jumpSpeed * 0.55f, 0);
            }
        }

        // Reset jump timer when on ground
        if (!MyInput.jump && touchingGround) {
            t_jetpack = 0;
        }

        if (MyInput.up || MyInput.down) {
            Vector3 newvel = rb.velocity;
            // Get the normalized xz-plane vector of the caemra 
            Vector3 xz = Camera.main.transform.forward;
            xz.y = 0;
            xz.Normalize();
            xz *= boostSpeed;
            if (MyInput.down) {
                xz *= -1;
            }
            newvel.x = xz.x;
            newvel.z = xz.z;
            rb.velocity = newvel;
            float camEulerY = Camera.main.transform.eulerAngles.y;
            transform.eulerAngles = new Vector3(0, Mathf.LerpAngle(transform.eulerAngles.y, camEulerY, Time.fixedDeltaTime * 5), 0);
        }
        limitSpeed();

    }

    RectTransform crosshairRectT;
    GameObject bullet;
    Rigidbody bulletRB;
    GameObject targetMarker;
    Vector3 playerToBulletOffset = new Vector3(1.5f, 0, 0);

    public bool isAiming() {
        return fixedUpdateMode == 1;
    }

    float tRampCrosshairSpeed = 0f;
    void updateShooting() {


        float camEulerY = Camera.main.transform.eulerAngles.y;
        transform.eulerAngles = new Vector3(0, Mathf.LerpAngle(transform.eulerAngles.y, camEulerY, Time.fixedDeltaTime * 5), 0);

        if (crosshairRectT == null) {
            crosshairRectT = GameObject.Find("Crosshair").GetComponent<RectTransform>();
        }

        // Logic for moving crosshair
        Vector3 newCrosshairPos = crosshairRectT.position;
        float crosshairSpeed = 0.75f * Screen.width; // how much of the screen it moves

        if (MyInput.up || MyInput.down || MyInput.right || MyInput.left) {
            tRampCrosshairSpeed += Time.deltaTime;
            if (tRampCrosshairSpeed >= 1f) tRampCrosshairSpeed = 1f;
            crosshairSpeed *= tRampCrosshairSpeed;
        } else {
            tRampCrosshairSpeed = 0;
        }

        // slow down if both held
        if ((MyInput.down || MyInput.up) && (MyInput.right || MyInput.left)) crosshairSpeed *= .717f;

        if (MyInput.up) {
            newCrosshairPos.y += crosshairSpeed * Time.deltaTime;
        } else if (MyInput.down) {
            newCrosshairPos.y -= crosshairSpeed * Time.deltaTime;
        }

        if (MyInput.right) {
            newCrosshairPos.x += crosshairSpeed * Time.deltaTime;
        } else if (MyInput.left) {
            newCrosshairPos.x -= crosshairSpeed * Time.deltaTime;
        }

        newCrosshairPos.x = Mathf.Clamp(newCrosshairPos.x, 0, Screen.width);
        newCrosshairPos.y = Mathf.Clamp(newCrosshairPos.y, 0, Screen.height);
        crosshairRectT.position = newCrosshairPos;

        // Move bullet
        bullet.transform.localPosition = playerToBulletOffset;


        // Get vector from camera to the cursor point, use this to raycast and look for the bullet target.
        Vector3 cursorScreenPos = crosshairRectT.position;
        cursorScreenPos.z = cam.nearClipPlane;
        Vector3 cursorWorldPos = cam.ScreenToWorldPoint(cursorScreenPos); // z coord is distanec from camera
        Vector3 shootDir = cursorWorldPos - cam.transform.position;
        float maxDistance = 50f;
        Ray ray = new Ray(cam.transform.position, shootDir);
        RaycastHit hit;
        int layermask = ~(1 << 9 | 1 << 2); // Everything except player and ignores
        Physics.Raycast(ray, out hit, maxDistance,layermask);
        // move target confirmation to the point it hits (up to max distance of some #)
        Vector3 newTargetPos = new Vector3();
        if (hit.collider != null) {
            newTargetPos = hit.point;
            GameObject.Find("TargetMarker").transform.position = newTargetPos;
        } 

        // If the shoot key is pressed then move the bullet to the point in space
        if (MyInput.jpCancel) {
            fixedUpdateMode = 0;
        } else if (MyInput.jpConfirm) {
            fixedUpdateMode = 0;
            crosshairRectT.GetComponent<Image>().enabled = false;

            Vector3 newBulletVelocity = new Vector3();
            float bulletSpeed = 50f;

            if (hit.collider != null) {
                shootDir = hit.point - bullet.transform.position;
                newBulletVelocity = Vector3.Normalize(shootDir) * bulletSpeed;
            } else {
                // move the cam-to-cursor vector 100 units from the camera, then make that vector point from the bullet.
                shootDir = 100f * Vector3.Normalize(shootDir) + cam.transform.position - bullet.transform.position;
                // then shoot the bullet.
                newBulletVelocity = Vector3.Normalize(shootDir) * bulletSpeed;
            }

            bulletRB = bullet.GetComponent<Rigidbody>();
            bulletRB.isKinematic = false;
            bulletRB.useGravity = false;
            bullet.transform.parent = null;
            print(newBulletVelocity);
            bulletRB.velocity = newBulletVelocity;
            bulletRB = null;
            bullet = null;
        }

    }

    bool transitionCameraFOVForShoot = false;
    float transitionCameraFOVForShoot_OriginalFOV = 0f;
	void Update () {



		bool playSounds = true;
		//bool playAnimations = true;

		if (CutsceneManager.deactivatePlayer || SaveModule.saveMenuOpen || DataLoader.instance.isPaused) {
			//playAnimations= false;
			playSounds = false;
		}

		if (playSounds) {
			updateSounds();
		}


        if (fixedUpdateMode == 0) {

            if (transitionCameraFOVForShoot) {
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, transitionCameraFOVForShoot_OriginalFOV, Time.deltaTime*1.5f);
                if (Mathf.Abs(cam.fieldOfView - transitionCameraFOVForShoot_OriginalFOV) < 0.1f) {
                    cam.fieldOfView = transitionCameraFOVForShoot_OriginalFOV;
                    transitionCameraFOVForShoot = false;
                }
            }

            if (MyInput.jpConfirm) {
                fixedUpdateMode = 1;
                bullet = GameObject.Find("Bullet");
                bullet.transform.parent = this.transform;
                bullet.transform.localPosition = playerToBulletOffset;
                bullet.GetComponent<Rigidbody>().isKinematic = true;
                GameObject.Find("Crosshair").GetComponent<Image>().enabled = true;
                Vector2 resettedPos = new Vector2();
                GameObject.Find("Crosshair").GetComponent<RectTransform>().anchoredPosition = resettedPos;
                transitionCameraFOVForShoot_OriginalFOV = cam.fieldOfView;
                transitionCameraFOVForShoot = true;
                return;
            }
        } else if (fixedUpdateMode == 1) {
            if (transitionCameraFOVForShoot) {
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 89f, Time.deltaTime*3.4f);
            }
            updateShooting();
        }

    }

	public float getRemainingTime() {
		if (t_jetpack >= tm_jetpack) {
			return 0f;
		}
		float r = tm_jetpack - t_jetpack;
		return r;
	}

    bool isTouchingGround() {
        return Physics.Raycast(transform.position, -Vector3.up, cc.bounds.extents.y + 0.2f);
    }

    void limitSpeed() {

		// Limit speed
		Vector3 newvel = rb.velocity;

		if (rb.velocity.y > maxVertVel) {
			newvel.y = maxVertVel;
		} else if (rb.velocity.y < -maxFallVel) {
			newvel.y = -maxFallVel;
		}

        // Slow player down faster
        if (!MyInput.up) {
            newvel.z /= 1.1f;
            newvel.x /= 1.1f;
		}

		// Capsule cast to stop movement (so collision doesnt stop y movement with walls)
		float dis = cc.height / 2 - cc.radius;
		Vector3 p1 = transform.position + cc.center + Vector3.up * dis;
		Vector3 p2 = transform.position + cc.center - Vector3.up * dis;
		float radius = cc.radius*.95f;
		float castDistance = 0.5f;
		RaycastHit hitInfo;
		int layerMask = 1 | 1<<10;
		if (Physics.CapsuleCast(p1,p2,radius,newvel,out hitInfo, castDistance,layerMask,QueryTriggerInteraction.Ignore)) {
			// Raise the capsule cast a little bit - if there's no hit that means this was probably a gentle slope
			p1.y += 0.45f;
			p2.y += 0.45f;
			if (Physics.CapsuleCast(p1,p2,radius,newvel,out hitInfo, castDistance,layerMask,QueryTriggerInteraction.Ignore)) {
				newvel.x = 0;
				newvel.z = 0;
			}
		}

		rb.velocity = newvel;
		
	}

	void updateSounds() {
	}

}
