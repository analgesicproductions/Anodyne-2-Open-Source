using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Anodyne;
using UnityEngine.Tilemaps;
public class SlimeWanderer : MonoBehaviour {

    SpriteAnimator animator;
    public bool IsGas = false;
    int gasIndex = 0;
    public bool IsElectro = false;
    public bool IsInnocent = false;
    int submode = 0;
    float shotTimer = 0;
    public float shotTimerMax = 2f;
    public float MovementSpeed = 2f;
    Vacuumable vac;
    Rigidbody2D rb;
    public SlimeWandererMovement movementstyle = SlimeWandererMovement.Normal;
    public enum SlimeWandererMovement { Normal, DownOnly }
    Vector3 initpos;
    Vector2Int initRoom;
    Vector2 vel = new Vector2();
    bool broken = false;
    bool breaking = false;
    Vector3 nextpos;
    Vector3 tempScale = new Vector3();
    bool didinit = false;
    float movetimer = 1.5f;
    float maxmovetimer = 1.5f;
    float initSpawnTimer = 0f;
    public float spawnTimer = 0f;
    Vector2 targetVel = new Vector2();
    public List<GameObject> children;
    AnoControl2D player;
    public Element element = Element.None;

    public List<Bullet> bullets;
    public GameObject bulletPrefab;
    float sfxtimer = 0;
    Element initelement;
    public List<Vacuumable> vacProjectiles;
       
    bool hasShield = false;
    Transform shield;
    SpriteRenderer shieldSR;
   // SpriteAnimator shieldAnim;
    float tShield = 0;
    float tmShieldOn = 1;
    float tmShieldOff = 0.5f;

    void Start () {
        initpos = transform.position;
        nextpos = new Vector3();
        initSpawnTimer = spawnTimer;
        animator = GetComponent<SpriteAnimator>();
        rb = GetComponent<Rigidbody2D>();
        vac = GetComponent<Vacuumable>();
        vac.overrideIdleDeceleration = true;

        if (transform.parent != null) {
            if (transform.Find("Shield") != null) {
                shield = transform.Find("Shield");
                shieldSR = shield.GetComponent<SpriteRenderer>();
               // shieldAnim = shield.GetComponent<SpriteAnimator>();
                hasShield = true;
            }
        }

        player = GameObject.Find("2D Ano Player").GetComponent<AnoControl2D>();
        if (IsElectro) {
            element = Element.Electric;
            bullets = new List<Bullet>();
            bullets.Add(transform.parent.GetChild(0).gameObject.GetComponent<Bullet>());
            bullets.Add(transform.parent.GetChild(1).gameObject.GetComponent<Bullet>());
            bullets.Add(transform.parent.GetChild(2).gameObject.GetComponent<Bullet>());
            foreach (Bullet bullet in bullets) {
              //  bullet.nonEnemyLethal = true;
                bullet.gameObject.SetActive(false);
            }
        } else if (element == Element.Fire) {

        } else if (IsGas) {
            for (int i = 0; i < 8; i++) {
                GameObject g = Instantiate(bulletPrefab, transform.parent);
                bullets.Add(g.GetComponent<Bullet>());
                g.GetComponent<Bullet>().InitComponents();
                g.SetActive(false);
            }
        } else if (element == Element.Ice) {
            vacProjectiles = new List<Vacuumable>();
            for (int i = 0; i < 4; i++) {
                GameObject g = Instantiate(bulletPrefab, transform.position,Quaternion.identity,transform.parent);
                vacProjectiles.Add(g.GetComponent<Vacuumable>());
                HF.GetRoomPos(g.transform.position, ref vacProjectiles[i].initRoom);
            }
            hasShield = false;
            shieldSR.enabled = false;
            vac.IsPickupable = false;
            vac.IsBreakable = false;
        }  else if (element == Element.Snow) {
            for (int i = 0; i < 6; i++) {
                GameObject g = Instantiate(bulletPrefab, transform.parent);
                bullets.Add(g.GetComponent<Bullet>());
                g.GetComponent<Bullet>().InitComponents();
                g.SetActive(false);
            }
        }
        initelement = element;

        Reinitialize();
	}

    //float t_angle = 0;

    void Reinitialize() {
        transform.position = initpos;
        spawnTimer = initSpawnTimer;
        broken = breaking = false;
        rb.velocity = Vector2.zero;
            
        if (spawnTimer > 0) {
            vac.enabled = false;
            GetComponent<CircleCollider2D>().enabled = false;
            GetComponent<SpriteRenderer>().enabled = false;
        } else {
            vac.enabled = true;
            GetComponent<CircleCollider2D>().enabled = true;
            GetComponent<SpriteRenderer>().enabled = true;
        }
    }

    float tFlickerInterval = 0;
    float tmFlickerInterval = 0.05f;
    void Update() {
        if (!didinit) {
            didinit = true;
            HF.GetRoomPos(transform.position, ref initRoom);
        }

        if (!player.CameraIsChangingRooms() && !player.InThisRoom(initRoom)) {
            if (element != initelement) {
                ChangeElement(initelement, true);
            }
        }


        if (vac.state == Vacuumable.VacuumMode.PickedUp) {
            OnPlayer = false;
            if (player.CameraIsChangingRooms() || !player.InThisRoom(initRoom)) {
                player.Swallow();
                vac.state = Vacuumable.VacuumMode.Broken;
                broken = true;
            }
        } else if (element == Element.Water) {
            nextpos = transform.position;
            HF.ConstrainVecToRoom(ref nextpos, initRoom.x, initRoom.y);
            if (nextpos != transform.position) {
                rb.velocity = Vector2.zero;
                Break(true);
            }
            transform.position = nextpos;
        }

        if (broken || breaking) {
            if (IsGas) UpdateGas(true);
            if (element == Element.Ice) UpdateIce(true);
            if (element == Element.Snow) UpdateSnow(true);
        }
        if (broken) {
            if (vac.state == Vacuumable.VacuumMode.Idle) {
                broken = false;
            }
            return;
        }
        if (vac.state == Vacuumable.VacuumMode.Breaking) {
            breaking = true;
        }
        if (breaking) {
            if (animator.isPlaying == false) {
                breaking = false; broken = true;
                if (OnPlayer) OnPlayer = false;
                GetComponent<CircleCollider2D>().enabled = false;
                GetComponent<SpriteRenderer>().enabled = false;
                if (element != initelement) {
                    ChangeElement(initelement,true);
                }
                if (IsInnocent) {
                    HF.SendSignal(children,"unsignal");
                } else {
                    HF.SendSignal(children);
                }
            }

            return;
        }

        if (tFlicker > 0) {
            tFlicker -= Time.deltaTime;
            tFlickerInterval += Time.deltaTime;
            if (tFlickerInterval > tmFlickerInterval) {
                tFlickerInterval = 0;
                GetComponent<SpriteRenderer>().enabled = !GetComponent<SpriteRenderer>().enabled;
            }
            if (tFlicker <= 0) GetComponent<SpriteRenderer>().enabled = true;
        }


        if (damageCooldown > 0) damageCooldown -= Time.deltaTime;
        if (OnPlayer && !vac.IsBeingSuckedAndMoving) {
            if (damageCooldown <= 0) {
                player.Damage(damage);
                if (IsElectro) {
                    player.Bump(true,5f);
                } else {
                    player.Bump(true);
                    if (IsInnocent) {
                        Break();   
                    }
                }
                damageCooldown = maxDamageCooldown;
            }
        }
        if (spawnTimer > 0) {
            spawnTimer -= Time.deltaTime;
            if (spawnTimer < 0) {
                vac.enabled = true;
                GetComponent<CircleCollider2D>().enabled = true;
                GetComponent<SpriteRenderer>().enabled = true;
            } else {
                return;
            }
        }


        // Snow needs to be able to detect sucking and break
        if (element == Element.Snow) {
            UpdateSnow();
        }

        if (vac.isPickedUp() || vac.isMoving() || vac.IsBeingSuckedAndMoving) {
            if (vac.IsBeingSuckedAndMoving && element == Element.Ice) {
                UpdateIce();
            }
            return;
        }

        if (IsElectro) {
            if (!HF.AreTheseInTheSameroom(player.transform, transform)) {
                shotTimer = 0;
                submode = 0;
                foreach (Bullet b in bullets) {
                    if (!b.IsDead()) b.Die();
                }
            } else {

                if (submode == 0) {

                    if (HF.TimerDefault(ref shotTimer, shotTimerMax)) {
                        submode = 10;
                        animator.Play("charge");
                    }
                } else if (submode == 10) {
                    if (HF.TimerDefault(ref shotTimer, 0.65f)) {
                        submode = 1;
                        animator.Play("idle");
                    }
                } else if (submode == 1) {
                    for (int i = 0; i < 3; i++) {
                        bullets[i].gameObject.SetActive(true);
                    }

                    AudioHelper.instance.playOneShot("bluntExplosion");
                    bullets[0].LaunchAt(transform.position, player.transform.position);
                    bullets[1].LaunchAt(transform.position, player.transform.position, 80);
                    bullets[2].LaunchAt(transform.position, player.transform.position, -80);
                    submode = 2;
                } else if (submode == 2) {
                    int c = 0;
                    foreach (Bullet b in bullets) {
                        if (!b.IsDead()) c++;
                    }
                    if (c == 0) submode = 0;
                }
            }
        } else if (IsGas) {
            UpdateGas();
        } else if (element == Element.Ice) {
            UpdateIce();
        } else if (element == Element.Water) {
            if (transform.localScale.x < 1) {
                tempScale = transform.localScale;
                tempScale.x = tempScale.x + Time.deltaTime*0.33f;
                tempScale.y = tempScale.x;
                transform.localScale = tempScale;
            }
        }

        nextpos = transform.position;

        vel = rb.velocity;
		if (movementstyle == SlimeWandererMovement.DownOnly) {
            HF.ReduceVec2To0(ref vel, 3*Time.deltaTime);
            vel.y = -MovementSpeed;
        } else if (movementstyle == SlimeWandererMovement.Normal) {
            float f = 1;
            if (element == Element.Fire) f = 0.15f;
            if (HF.TimerDefault(ref movetimer,maxmovetimer*f)) { 
                HF.randomizeVec2ToOneDir(ref vel, MovementSpeed);
                targetVel = vel;
                if (element == Element.Fire) targetVel *= 5f;
            }
            HF.ReduceVec2ToVec(ref vel, targetVel, Time.deltaTime * 6);
        }
        if (!player.InSameRoomAs(transform.position)) {
            vel = Vector2.zero;
        }
        rb.velocity = vel;
        sfxtimer += Time.deltaTime;
        if (player.InSameRoomAs(transform.position) && !player.IsDying() && sfxtimer > 1) {
            sfxtimer = 0;
            if (vac.inPicoScene == false) AudioHelper.instance.playSFX("slime_walk", true, 1, true);
        }
        HF.ConstrainVecToRoom(ref nextpos, initRoom.x, initRoom.y);
        transform.position = nextpos;
    }

    float tSnowShoot = 0f;
    int snowballsShot = 0;
    private void UpdateSnow(bool afterBreak = false) {
        if (submode == 0) {
            if (afterBreak) {
                submode = 1;
            } else if (vac.IsBeingSucked()) {
                player.cancelUnrootWait();
                submode = 1;
                tSnowShoot = 1;
                Break();
            }
        } else if (submode == 1) {
            if (snowballsShot < bullets.Count && HF.TimerDefault(ref tSnowShoot,0.15f)) {
                tSnowShoot = 0;
                snowballsShot++;
                Bullet b = Bullet.GetADeadBullet(bullets);
                b.gameObject.SetActive(true);
                b.boomerangSlowdownRate = 0.95f;
                b.boomerangInitSlowdownTime = 0.65f;
                b.diesWhenOutsideSpawnRoom = true;
                b.boomerangWaitToShootTime = 0.1f;
                b.LaunchAt(transform.position, transform.position + new Vector3(0,1,0), -35f + 70f * Random.value);
                b.boomerangInitialSpeedRatio = 1f;
                AudioHelper.instance.playOneShot("vacuumShoot", 1, 1);
            }
            // reset bullets
            if (!player.InThisRoom(initRoom)) {
                for (int i = 0; i < bullets.Count; i++) {
                    if (!bullets[i].IsDead()) {
                        bullets[i].Die();
                    }
                }
                submode = 0;
                snowballsShot = 0;
                return;
            }
        }
    }

    float[] angles;
    float iceAngularVel = 300f;
    private void UpdateIce(bool afterBreak = false) {

        if (vac.isIdle()) { 
            if (shieldSR.enabled && HF.TimerDefault(ref tShield,tmShieldOn)) {
                //shieldSR.enabled = false;
            } else if (!shieldSR.enabled && HF.TimerDefault(ref tShield, tmShieldOff)) {
                //shieldSR.enabled = true;
            }
        }

        if (submode == 0) {
            angles = new float[4];
            submode = 1;
        } else if (submode == 1) {

            for (int i = 0; i < 4; i++) {
                tempv2_2 = Vector2.up * 2.4f;
                HF.RotateVector2(ref tempv2_2, 90 * i);
                angles[i] = 90 * i;
                tempv2_2.x += transform.position.x; tempv2_2.y += transform.position.y;
                if (vacProjectiles[i].isBroken()) vacProjectiles[i].Respawn();
                vacProjectiles[i].transform.position = tempv2_2;
                vacProjectiles[i].hurtsPlayer = true;
            }

            if (player.InThisRoom(initRoom)) {
                submode = 2;
            }
        } else if (submode == 2) {

            if (!player.InThisRoom(initRoom)) {
                submode = 0;

                iceshattered = false;
                vac.IsPickupable = false;
                vac.IsBreakable = false;
                return;
            }
            // dont rotate ice cubes after slime is broken
            if (afterBreak) {
                for (int i = 0; i < 4; i++) {
                    vacProjectiles[i].hurtsPlayer = false;
                }
                return;
            }

            bool pauseRotation = false;
            for (int i = 0; i < 4; i++) {
                if (vacProjectiles[i].IsBeingSucked() && !vacProjectiles[i].isBroken()) {
                    pauseRotation = true;
                }
            }

            int dead_ = 0;
            for (int i = 0; i < 4; i++) {
                tempv2_2 = Vector2.up * 2.4f;
                HF.RotateVector2(ref tempv2_2, angles[i]);
                if (!pauseRotation) angles[i] += Time.deltaTime * iceAngularVel;
                if (angles[i] >= 360f) angles[i] -= 360f;
                if ((vacProjectiles[i].isIdle() && !vacProjectiles[i].IsBeingSucked()) || vacProjectiles[i].isBroken()) {
                    tempv2_2.x += transform.position.x; tempv2_2.y += transform.position.y;
                    vacProjectiles[i].transform.position = tempv2_2;

                    if (vacProjectiles[i].isBroken()) {
                        dead_++;
                    }
                } else {
                    dead_++;
                    continue;
                }
            }
            if (dead_ == 4 && !iceshattered) {
                iceshattered = true;
                AudioHelper.instance.playOneShot("sparkBarHit");
                animator.Play("Shatter");
                animator.ScheduleFollowUp("idle_vulnerable");
                vac.IsPickupable = true;
                vac.IsBreakable = true;
            }
        }
    }
    bool iceshattered = false;

    private void UpdateGas(bool afterBreak = false) {
        if (submode == 0) {
            if (!player.InThisRoom(initRoom)) {
                shotTimer = 0;
                return;
            }
            if (afterBreak || HF.TimerDefault(ref shotTimer, shotTimerMax)) {
                submode = 1;
                gasIndex = Random.Range(0, 7);
            }
        } else if (submode >= 1 && submode <= 8) {
            if (HF.TimerDefault(ref shotTimer, 0.08f)) {
                int times = 1;
                if (afterBreak) times = 8;
                for (int i = 0; i < times; i++) {
                    submode++;
                    bullets[gasIndex].gameObject.SetActive(true);
                    tempv2_2 = transform.position;
                    tempv2_2 += Vector2.right * 10f;
                    bullets[gasIndex].LaunchAt(transform.position, tempv2_2, 45 * gasIndex);
                    gasIndex = (gasIndex + 1) % 8;
                }
            }
        } else if (submode == 9) {
            if (Bullet.AllDead(bullets)) {
                if (afterBreak) {
                    submode = 10;
                } else {
                    submode = 0;
                }
            }
        } else if (submode == 10) {
            if (!afterBreak) submode = 0;
        }
    }
   // Vector2 tempv2_1 = new Vector2();
    Vector2 tempv2_2 = new Vector2();
    public void SendSignal(string signal) {
        if (signal == "reset") {
            animator.Play("idle");
            Reinitialize();
        }
    }

    public void OnSuckedIn() {
        if (IsInnocent) {
           HF.SendSignal(children, "unsignal");
        }

        if (hasShield && shieldSR.enabled) {
            player.Damage();
            shieldSR.enabled = false;
        } else if (element == Element.Fire) {
            player.Damage();
            ChangeElement(Element.None);
        } else if (element == Element.Electric) {
            player.Damage();
        }
    }

    void ChangeElement(Element e,bool skipIdleAnim=false) {
        if (element == e) return;

        IsElectro = false;
        element = e;

        if (e == Element.Fire) {
            // Update animations
            HF.SendSignal(children, "becamefire");
            animator.UpdateSpritesheet("Entity", "EntitySprites24");
            animator.ChangeAnimationData("idle", 16,"20,21,22,23",true);
            animator.AddAnimation("flash", 16, "18,19,18,19,18,19,18,19", false);
            if (GetComponent<SpriteRenderer>().enabled) {
                AudioHelper.instance.playOneShot("enemyFlamed");
            }
            if (!skipIdleAnim) {
                animator.ForcePlay("flash");
                animator.ScheduleFollowUp("idle");
            }
        } else if (e == Element.Electric) {
            IsElectro = true;
        } else if (e == Element.None) {
            animator.UpdateSpritesheet("Prototyping", "SeanPlaceholder16px");
            animator.ChangeAnimationData("idle", 4,"28,29",true);
            if (!skipIdleAnim) animator.ForcePlay("idle");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (!player.InThisRoom(initRoom)) return;
        if (vac.inPicoScene && vac.isIdle()) {
            if (collision.name.IndexOf("2DSpark") == 0 && (collision.GetComponent<Spark2D>().justBroke || collision.GetComponent<Spark2D>().IsAlive())) {
                sparkHealth--;
                AudioHelper.instance.playOneShot("swing_broom_1", 0.7f, 0.95f + 0.1f * Random.value, 3);
                tFlicker = 0.3f;
                if (sparkHealth == 0) {
                    Break();
                    tFlicker = tFlickerInterval = 0;
                    sparkHealth = 3;
                    return;
                }
            }
        }

        if (element == Element.Water) {
            if (collision.gameObject.GetComponent<NonsuckRegrowHurter>() != null) {
                if (collision.gameObject.GetComponent<NonsuckRegrowHurter>().element == Element.Fire) {
                    tempScale = transform.localScale;
                    tempScale.x *= 0.8f; tempScale.y *= 0.8f;
                    transform.localScale = tempScale;
                }
            }
        }
        if ((vac.isMoving()|| vac.isIdle()) && collision.GetComponent<Bullet>() != null) {
            Bullet b = collision.GetComponent<Bullet>();
            
            if (b.element == Element.Fire) {
                if (element == Element.Water) {
                    Break(true);
                    return;
                }
                if (element != Element.Fire) ChangeElement(Element.Fire);
            } else {
                if (element == Element.Ice && !iceshattered) return;
                if (element == Element.Snow && submode == 0 && b.name.IndexOf("GasSlime") != -1) {
                    submode = 1;
                    tSnowShoot = 1;
                    Break();
                }
                if (b.siblingEnt.name == name) return;
                if (b.IsDead()) return;
                if (!b.nonEnemyLethal) Break();
            }
            return;
        }
    }

    float tFlicker = 0;

    private void OnCollisionEnter2D(Collision2D collision) {
        bool dobreak = false;

        

        float requiredVelToBreak = 8f;
        if (vac.inPicoScene) requiredVelToBreak = 4f;
        if (collision.gameObject.GetComponent<Vacuumable>() != null || collision.gameObject.GetComponent<TilemapCollider2D>() != null) {

            if (IsInnocent && collision.collider.name.IndexOf("Snowman") != -1) {
                dobreak = true;
            }
            if (Mathf.Abs(collision.relativeVelocity.magnitude) > requiredVelToBreak) {
                dobreak = true;
            }
            if (collision.gameObject.GetComponent<Boomer>() != null) {
                dobreak = true;
            }
        } else if (collision.collider.CompareTag("Player")) {
            if (!vac.isMoving()) {
                OnPlayer = true;
            }
        } else if (Mathf.Abs(collision.relativeVelocity.magnitude) > requiredVelToBreak) {
            dobreak = true;
        }


        if (element == Element.Ice && !iceshattered) dobreak = false;

        if (dobreak) {
            Break();
        }
    }

    int sparkHealth = 3;

    [System.NonSerialized]
    public bool wasShotIntoSomthing;
    public void Break(bool force = false) {
        //print("Break");
        if (element == Element.Water && vac.isOnHoleOrNull()) {
        } else if (!force && element == Element.Water) {
            return;
        }

        if (hasShield) shieldSR.enabled = false;
        if (IsGas) submode = 0; // reset the gas shooting routine
        animator.Play("break");
        if (vac.inPicoScene) {
            //SeanHF.SpawnDustPoof(transform.position,0.5f);
        } else {
            HF.SpawnDustPoof(transform.position);
        }
        //vacuumable.enabled = false;
        if (vac.isIdle() == false) wasShotIntoSomthing = true;
        vac.Break(true);
        wasShotIntoSomthing = false;
        rb.velocity = Vector2.zero;
        breaking = true;
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.name == Registry.PLAYERNAME2D) {
            //print(123); handles bug when jumpzone turns you into a trigger
            OnPlayer = false;
        }
    }

    float damageCooldown = 0f;
    float maxDamageCooldown = 0.8f;
    public int damage = 1;
    bool OnPlayer = false;
    private void OnCollisionExit2D(Collision2D collision) {
        if (collision.collider.CompareTag("Player")) {
            OnPlayer = false;
        }
    }
}
