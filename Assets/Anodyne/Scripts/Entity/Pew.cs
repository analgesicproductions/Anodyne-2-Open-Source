using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Anodyne;

public class Pew : MonoBehaviour {

    AnoControl2D player;
    Rigidbody2D rb;
    //CircleCollider2D cc;
    SpriteAnimator anim;
    //SpriteRenderer sr;

    public GameObject bulletPrefab;
    // bullet prefab has the animations
    public int maxBullets = 0;
    public float shootInterval = 1;
    public float shootSpeed = 10f;
    public float trackSpeed = 5f;
    float tShoot = 0;
    Vector3 tempPos = new Vector3();
    Vector2 tempVel = new Vector2();


    public bool tracksPlayer = false;
    public bool tracksVertically = false;
    public bool tracksHorizontally = false;
    public AnoControl2D.Facing shootDirection = AnoControl2D.Facing.UP;
    public bool shootsAtPlayer = false;
    public bool bulletsPassThruWalls = false;
    public bool noWindupWait = false;
    public float homingStrength = 1.6f;
    int mode = 0;

    List<Bullet> bullets = new List<Bullet>();

    Vector2 initpos;
    void Start() {
        rb = GetComponent<Rigidbody2D>();
       // sr = GetComponent<SpriteRenderer>();
        HF.GetPlayer(ref player);
        //cc = GetComponent<CircleCollider2D>();
        anim = GetComponent<SpriteAnimator>();
        initpos = transform.position;
        if (shootsAtPlayer) {
        } else if (shootDirection == AnoControl2D.Facing.UP) {
            transform.localEulerAngles = new Vector3(0, 0, 90);
        } else if (shootDirection == AnoControl2D.Facing.LEFT) {
            Vector3 v = transform.localScale;
            v.x *= -1; transform.localScale = v;
        } else if (shootDirection == AnoControl2D.Facing.DOWN) {
            transform.localEulerAngles = new Vector3(0, 0, 270);
        }


        for (int i = 0; i < maxBullets; i++) {
            GameObject g = Instantiate(bulletPrefab, transform.parent);
            bullets.Add(g.GetComponent<Bullet>());
            g.GetComponent<Bullet>().InitComponents();
            g.transform.position = new Vector3(3000, 3000, 0);
            if (bulletsPassThruWalls) {
                g.GetComponent<Bullet>().passesThroughWalls = true;
            }
        }
        if (tracksHorizontally) {
            rb.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
        } else if (tracksVertically) {
            rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        }
        HF.GetRoomPos(transform.position, ref initroom);
    }

    Vector2Int initroom = new Vector2Int();

    float threshold = 0.5f;
    int trackMode = 0;

    int deathMode = 0;

    public void MyBreak() {
        anim.Play("break");
        deathMode = 1;
        AudioHelper.instance.playOneShot("blockExplode");
    }

    public bool IsDead() {
        return deathMode != 0;
    }
	void Update () {
        if (player.isPaused) {
            rb.velocity = Vector2.zero;
            return;
        }

        // break called
        if (deathMode == 1) {
            if (anim.isPlaying == false) {
                deathMode = 2;
                rb.velocity = Vector2.zero;
                GetComponent<SpriteRenderer>().enabled = false;
                transform.position = initpos;
            }
            return;
        } else if (deathMode == 2) {
            transform.position = initpos;
            if (!player.InThisRoom(initroom)) {
                anim.Play("idle");
                deathMode = 0;
                GetComponent<SpriteRenderer>().enabled = true;
            }
            return;
        }

        if (player.InThisRoom(initroom)) {
            tempPos = transform.position;
            float px = player.transform.position.x;
            float py = player.transform.position.y;
            tempVel.Set(0, 0);

            if (trackMode == 0) {
                if (tracksVertically) {
                    if (tempPos.y < py) {
                        tempVel.y = trackSpeed;
                    } else {
                        tempVel.y = -trackSpeed;
                    }
                    if (Mathf.Abs(py-tempPos.y) < 0.25f) {
                        trackMode = 1;
                    }
                    
                } else if (tracksHorizontally) {
                    if (tempPos.x < px) {
                        tempVel.x = trackSpeed;
                    } else {
                        tempVel.x = -trackSpeed;
                    }
                    if (Mathf.Abs(px - tempPos.x) < 0.25f) {
                        trackMode = 1;
                    }
                }
            } else if (trackMode == 1) {
                if (tracksVertically && Mathf.Abs(tempPos.y - player.transform.position.y) > threshold) {
                    trackMode = 0;
                } else if (tracksHorizontally && Mathf.Abs(tempPos.x - player.transform.position.x) > threshold) {
                    trackMode = 0;
                }
            }
            rb.velocity = tempVel;
        }

        if (!player.InThisRoom(initroom) || player.CameraIsChangingRooms()) {
            rb.velocity = Vector2.zero;
            return;
        }
	    if (mode == 0) {
            if (tShoot > 0.426f*shootInterval) {
                anim.Play("windup");
            }
            if (HF.TimerDefault(ref tShoot,shootInterval)) {
                foreach (Bullet bullet in bullets) {
                    if (bullet.IsDead()) {
                        AudioHelper.instance.playSFX("bluntExplosion");
                        bullet.StartVelocity = shootSpeed;
                        if (shootsAtPlayer) {
                            if (bullet.type == Bullet.BulletType.Homing) {
                                bullet.diesWhenOutsideSpawnRoom = false;
                                bullet.LaunchHoming(player.transform, homingStrength, transform.position, player.transform.position, 0);
                            } else {
                                bullet.LaunchAt(transform.position, player.transform.position);
                            }
                        } else {
                            bullet.LaunchInCardinalDir(transform.position, shootDirection);
                        }
                        mode = 1;
                        anim.Play("shoot");
                        GetComponent<PositionShaker>().enabled = true;
                        break;
                    }
                }
            }
        } else if (mode == 1) {
            rb.velocity = Vector2.zero;
            if (anim.isPlaying == false || noWindupWait || anim.CurrentAnimationName() == "idle") {
                anim.Play("idle");
                mode = 0;
                GetComponent<PositionShaker>().enabled = false;
            }
        } else if (mode == 2) {
            tPause -= Time.deltaTime;
            tflicker += Time.deltaTime;
            if (tflicker > 0.05f) {
                tflicker = 0;
                GetComponent<SpriteRenderer>().enabled = !GetComponent<SpriteRenderer>().enabled;
            }
            rb.velocity = Vector2.zero;
            if (tPause < 0) {
                GetComponent<SpriteRenderer>().enabled = true;
                anim.paused = false;
                mode = cachedMode;
            }
        } 	

       
	}
    float tflicker = 0;
    int cachedMode = 0;
    float tPause = 0;
    private void OnTriggerEnter2D(Collider2D collision) {
        if (mode != 2 && null != collision.GetComponent<Vacuumable>()) {
            if (collision.GetComponent<Ano2Dust>() != null) return;
            cachedMode = mode;
            mode = 2;
            anim.paused = true;
            tPause = 1.5f;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (mode != 2 && null != collision.collider.GetComponent<Vacuumable>()) {
            if (collision.collider.GetComponent<Ano2Dust>() != null) return;
            cachedMode = mode;
            mode = 2;
            anim.paused = true;
            tPause = 1.5f;
        }
    }

    public void DieBullets() {
        Bullet.DieAll(bullets);
    }

}
