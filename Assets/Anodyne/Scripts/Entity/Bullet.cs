using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Anodyne {
    public class Bullet : MonoBehaviour {

        public enum BulletType {  Straight, Curve, BoomerangPassthru, Homing }
        public BulletType type = BulletType.Straight;
        public float StartVelocity = 10f;
        // Every 1 second, this much of the current vel gets added, perpendicular (1 = 90 deg right)
        public float CurveAddFactor = 1f;
        [System.NonSerialized]
        public Transform homingTarget;
        [System.NonSerialized]
        public float homingStrength;
        int mode = 0;
        int submode = 0;
        Rigidbody2D rb;
        SpriteAnimator anim;
        Vector2 nextvel = new Vector2();
        public int damage = 1;
        Vector2Int room;
        //Vector3 nextpos = new Vector3();
        [System.NonSerialized]
        public GameObject siblingEnt;
        public bool nonEnemyLethal = false;
        // if > 0 slows down this much each frame
        public float slowDownFactor = -1;
        public bool diesWhenVelNearZero = false;
        // If > 0, then this bullet can't hurt for a while
        public float initNoHurtTime = 0f;
        public bool diesWhenOutsideSpawnRoom = false;
        float t_windup = 0;
        float tm_windup = 0;
        float t_dieWhenActive = 0;
        float tm_dieWhenActive = -1;
        float t_travel = 0;
        [System.NonSerialized]
        public bool destroyWhenDead = false;
        bool isATB = false;
        public bool knockbackplayer = false;


        AnoControl2D player;

        // How fast the boomerang initially moves
        [System.NonSerialized]
        public float boomerangInitialSpeedRatio = 0.3f;
        [System.NonSerialized]
        public float boomerangSlowdownRate = 1f;
        [System.NonSerialized]
        public float boomerangInitSlowdownTime = 0.33f;
        [System.NonSerialized]
        public float boomerangWaitToShootTime = 0.8f;
        // slowdown ratio for boomerang

        Vector2 tempVel = new Vector2();

        public Element element = Element.None;
        private void Awake() {
            InitComponents();
        }

        public void InitComponents() {
            anim = GetComponent<SpriteAnimator>();
            rb = GetComponent<Rigidbody2D>();
            HF.GetPlayer(ref player);
        }

        Vector2Int spawnRoomPos = new Vector2Int();

        private void Start() {
            if (name.IndexOf("ATB_") != -1) {
                isATB = true;
            }
            if (transform.parent != null && transform.parent.Find("Entity") != null) {
                siblingEnt = transform.parent.Find("Entity").gameObject;
            }

        }
        public bool DiesWhenIdleAnimDone = false;

        Vector3 pausePos = new Vector3();
        void Update() {


            if (DataLoader.instance.isPaused || tPauseBeforeMove > 0) {
                if (tPauseBeforeMove > 0) tPauseBeforeMove -= Time.deltaTime;
                transform.position = pausePos;
                return;
            } else {
                pausePos = transform.position;
            }

            if (mode == 0) {

            } else if (mode == 1) {

                if (initNoHurtTime > 0) {
                    initNoHurtTime -= Time.deltaTime;
                }

                if (type == BulletType.Curve) {
                    nextvel = rb.velocity;
                    Vector2 perpvel = nextvel;
                    HF.RotateVector2(ref perpvel, 90);
                    nextvel += Time.deltaTime * CurveAddFactor * perpvel;
                    rb.velocity = nextvel;
                }

                if (type == BulletType.Homing) {
                    nextvel = homingTarget.position - transform.position;
                    nextvel.Normalize();
                    nextvel *= StartVelocity;
                    nextvel = Vector2.Lerp(rb.velocity, nextvel, Time.deltaTime * homingStrength);
                    rb.velocity = nextvel;
                    if (Vector2.Distance(transform.position,homingTarget.position) < 0.25f) {
                        Die();
                        submode = 0;
                    }
                }

                if (type == BulletType.BoomerangPassthru) {
                    if (submode == 0) {
                        tempVel = rb.velocity;
                        rb.velocity = tempVel * boomerangInitialSpeedRatio;
                        submode = 1;
                    } else if (submode == 1) {
                        nextvel = rb.velocity; nextvel *= boomerangSlowdownRate; rb.velocity = nextvel;
                        if (HF.TimerDefault(ref t_travel, boomerangInitSlowdownTime)) {
                            submode = 2;
                            // snowslimes dont go back to normal speed, booomeranger boomerangs do
                            if (boomerangSlowdownRate == 1) rb.velocity = tempVel;
                        }
                    } else if (submode == 2) { 
                        if (HF.TimerDefault(ref t_travel, boomerangWaitToShootTime)) {
                            submode = 3;
                            nextvel = (player.transform.position - transform.position).normalized * StartVelocity*1.5f;
                            rb.velocity = nextvel;
                        }
                    } else if (submode == 3) {
                        if (HF.TimerDefault(ref t_travel, 1.5f)) {
                            submode = 0;
                            Die();
                        }
                    }
                }

                if (slowDownFactor > 0 && slowDownFactor < 1) {
                    nextvel = rb.velocity; nextvel *= slowDownFactor; rb.velocity = nextvel;
                }
                if (DiesWhenIdleAnimDone && anim.isPlaying == false) {
                    Die();
                }
                if (diesWhenVelNearZero && Mathf.Abs(rb.velocity.magnitude) < 0.1f) {
                    Die();
                }
                if (diesWhenOutsideSpawnRoom && !HF.IsInRoom(transform.position,spawnRoomPos.x,spawnRoomPos.y)) {
                    if (type == BulletType.BoomerangPassthru) {
                        AudioHelper.instance.playOneShot("blockexplode");
                    }
                    Die();
                }

                if (tm_dieWhenActive > 0) {
                    if (Vector3.Distance(transform.position,player.transform.position) < 0.25f) {
                        player.Damage(damage);
                        Die();
                    }
                    if (HF.TimerDefault(ref t_dieWhenActive,tm_dieWhenActive)) {
                        Die();
                    }
                }

                if (type != BulletType.BoomerangPassthru) {
                    if (HF.IsInRoom(transform.position, room.x, room.y) == false) {
                        Die();
                    }
                }
            } else if (mode == 2 && anim.isPlaying == false) {
                mode = 0;
                GetComponent<SpriteRenderer>().enabled = false;
                if (destroyWhenDead) {
                    Destroy(gameObject);
                    return;
                }
            } else if (mode == 100) {
                if (HF.TimerDefault(ref t_windup,tm_windup)) {
                    if (anim.GetAnimation("emerge") != null) {
                        anim.Play("emerge");
                        mode = 101;
                        EnableCollider();
                    } else {
                        mode = 1;
                        anim.Play("idle");
                    }
                }
            } else if (mode == 101 && !anim.isPlaying) {
                mode = 1;
                anim.Play("idle");
                EnableCollider();
            }
        }

        public static bool AllDead(List<Bullet> bullets) {
            bool b = true;
            foreach (Bullet bul in bullets) {
                if (!bul.IsDead()) {
                    b = false;
                }
            }
            return b;
        }
        public static void DieAll(List<Bullet> bullets) {
            foreach (Bullet bul in bullets) {
                bul.Die();
            }
        }

        public bool IsDead() {
            return mode == 0;
        }
        public void Die() {
            if (mode == 2) return;
            submode = 0;
            t_travel = 0;
            if (tm_windup > 0) DisableCollider();
            if (!DiesWhenIdleAnimDone) {
                transform.localEulerAngles = new Vector3(0, 0, 0);
                anim.Play("break");
            }
            mode = 2;
            rb.velocity = Vector2.zero;
            if (startAndStopChildParticles) GetComponentInChildren<ParticleSystem>().Stop();
        }


        void LaunchInitialization() {
            mode = 1;
            rb.velocity = nextvel;
            preColVel = nextvel;
            GetComponent<SpriteRenderer>().enabled = true;
            anim.ForcePlay("idle");
            HF.GetRoomPos(transform.position, ref room);
            HF.GetRoomPos(transform.position, ref spawnRoomPos);

            if (startAndStopChildParticles) GetComponentInChildren<ParticleSystem>().Play();
        }

        // idle windup emerge break
        public void LaunchWithWindupIdle(Vector2 startpos, float _tm_windup, float _tm_diewhenActive,int predictive=0) {
            transform.position = startpos;
            if (predictive ==1) {
                Vector2 playerVel = player.GetComponent<Rigidbody2D>().velocity;
                float t = tm_windup + 0.06f;
                playerVel = t * playerVel * 0.5f;
                transform.position = startpos + playerVel;
            }

            t_windup = t_dieWhenActive = 0;
            tm_windup = _tm_windup;
            tm_dieWhenActive = _tm_diewhenActive;
            anim.Play("windup");
            DisableCollider();
            mode = 100;
            rb.velocity = Vector2.zero;
            GetComponent<SpriteRenderer>().enabled = true;
            HF.GetRoomPos(transform.position, ref room);
        }

        public static Bullet GetADeadBullet(List<Bullet> bullets) {
            foreach (Bullet bul in bullets) {
                if (bul.IsDead()) return bul;
            }
            return null;
        }

        public bool rotateDefault = false;

        [System.NonSerialized]
        public float tPauseBeforeMove = 0;
        Vector3 tempEuler = new Vector3();
        // Assumes default sprite dir is RIGHT. so DOWN, LEFT, UP is 270 180 90
        public void LaunchInCardinalDir(Vector2 startpos, AnoControl2D.Facing dir) {
            transform.position = startpos;
            pausePos = transform.position;
            tempEuler = transform.localEulerAngles;
            if (dir == AnoControl2D.Facing.UP) {
                nextvel.Set(0, StartVelocity);
                tempEuler.z = 90f;
            } else if (dir == AnoControl2D.Facing.RIGHT) {
                tempEuler.z = 0;
                nextvel.Set(StartVelocity,0);
            } else if(dir == AnoControl2D.Facing.DOWN) {
                tempEuler.z = 270;
                nextvel.Set(0, -StartVelocity);
            } else if (dir == AnoControl2D.Facing.LEFT) {
                tempEuler.z = 180;
                nextvel.Set(-StartVelocity,0);
            }
            if (rotateDefault) transform.localEulerAngles = tempEuler;
            LaunchInitialization();
        }

        void DisableCollider() {
            if (GetComponent<BoxCollider2D>() != null) GetComponent<BoxCollider2D>().enabled = false;
            if (GetComponent<CircleCollider2D>() != null) GetComponent<CircleCollider2D>().enabled = false;
        }
        void EnableCollider() {
            if (GetComponent<BoxCollider2D>() != null) GetComponent<BoxCollider2D>().enabled = true;
            if (GetComponent<CircleCollider2D>() != null) GetComponent<CircleCollider2D>().enabled = true;
        }

        public void LaunchHoming(Transform homingTarget, float homingStrength, Vector2 start, Vector2 target, float angleOffset) {
            LaunchAt(start, target, angleOffset);
            type = Bullet.BulletType.Homing;
            this.homingTarget = homingTarget;
            this.homingStrength = homingStrength;
        }

        public void LaunchAt(Vector2 start, Vector2 target, float angleOffset = 0) {
            transform.position = start;
            nextvel = (target - start).normalized * StartVelocity;
            if (angleOffset != 0) {
                HF.RotateVector2(ref nextvel, angleOffset); 
            }
            LaunchInitialization(); 
        }
        
        public bool startAndStopChildParticles = false;

        private void OnTriggerEnter2D(Collider2D collision) {
            if (mode != 1) return;
            TriggerEnterStuff(collision);
        }
        [System.NonSerialized]
        public string ignoreTrigName = "";
        [System.NonSerialized]
        public Vector2 preColVel;
        [System.NonSerialized]
        public bool passesThroughWalls = false;
        void TriggerEnterStuff(Collider2D collision) {
            preColVel = rb.velocity;
            if (collision.isTrigger) return;
            if (collision.gameObject == siblingEnt) return;
            if (collision.name == ignoreTrigName) return;
            if (isATB) {
                if (collision.name != "2D Ano Player" && collision.name.IndexOf("ATB_Shield") == -1) {
                    return;
                }
            }
            if (collision.transform != transform.parent && collision.gameObject.GetComponent<Bullet>() == null) {
                if (initNoHurtTime > 0) {
                    return;
                }
                if (type != BulletType.BoomerangPassthru && !passesThroughWalls) {
                    Die();
                }
                if (collision.name == "2D Ano Player") {
                    AnoControl2D player = collision.GetComponent<AnoControl2D>();
                    if (!player.CameraIsChangingRooms() && player.InThisRoom(room)) {
                        player.Damage(damage);
                        if (knockbackplayer) {
                            player.Bump(true,6f);
                        }
                    }
                }
            }
        }

    }

}