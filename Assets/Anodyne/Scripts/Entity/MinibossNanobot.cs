using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Anodyne;
public class MinibossNanobot : MonoBehaviour {
    public enum MinibossNanobotID { Sanctuary, Rage, Tongue, Pig, Cougher }
    public MinibossNanobotID miniboss_id = MinibossNanobotID.Sanctuary;

    public GameObject rockSpawnPrefab;
    public GameObject bulletPrefab;
    public TriggerChecker trigger;

    GameObject deathFXPrefab;
    ParticleSystem deathFXExplosions;
    ParticleSystem deathFXDust;

    Vacuumable rockSpawn;

    public int health;
    float maxHealth;

    DialogueBox dbox;
    PositionShaker ps;
    Rigidbody2D rb;
    EntityState2D state;
    SpriteAnimator anim;
    AnoControl2D player;

    Vacuumable[] blockChildren;
    List<Bullet> bullets = new List<Bullet>();
    List<Vector3> waypoints = new List<Vector3>();
    float waypointThreshold = 0.2f;

    public Vector2 rampedWaypointWaitTime = new Vector2(1.25f, 0.5f);
    public Vector2 rampedBulletVel = new Vector2(2.3f, 4.5f);
    public Vector2 rampedMovementVel = new Vector2(2.5f, 6f);

    int mode = 0;
    int cached_mode = 0;
    int MODE_HURTSTUN = 12345;
    int MODE_DYING_SPEECH = 23456;
    int MODE_DYING_ANIM = 1231;
    int MODE_DEAD = 2345;

    Vector2 cached_vel = new Vector2();

    float hurt_timer = 0;
    float t_waitAtWaypoint;
    float t_shootAfterLeavingWaypoint = 0;
    float t_damagePlayer = 0;

    int nextWaypoint = 1;
    bool onPlayer = false;
    bool shotAfterLeavingWaypoint = false;
    string songFadedOut = "";
    string bossSongName = "NanobotFight";
    // Specific scene checklist
    // x Check to Die
    // x Intro  
    // x Set state on die
    // x Outro Speech
    // x Health
    // x Player Bullet
    // x Waypoint Bullet
    // _ Number of waypoints

    string dialogue_scene_sanctuary = "cc-nanobot";
    string dialogue_scene_pig = "pig-nanobot";
    string dialogue_scene_rage = "rage-nanobot";
    string dialogue_scene_cougher = "cougher-nanobot";
    string dialogue_scene_tongue = "tongue-nanobot";
    private float takeDamageCooldown;

    void Start () {

        deathFXPrefab = Resources.Load("Prefabs/BossDeathFX") as GameObject;
        GameObject fx = Instantiate(deathFXPrefab, transform);
        deathFXDust = fx.transform.Find("BossDustParticle").GetComponent<ParticleSystem>();
        deathFXExplosions = fx.transform.Find("BossExplodeParticle").GetComponent<ParticleSystem>();


        Transform BlocksTransform = transform.parent.Find("Blocks");
        int BlockssCount = BlocksTransform.childCount;
        blockChildren = new Vacuumable[BlockssCount];

        for (int i = 0;  i < BlockssCount; i++) {
            blockChildren[i] = BlocksTransform.GetChild(i).GetComponent<Vacuumable>();
        }

        // Turn self off if boss defeated
        if (miniboss_id == MinibossNanobotID.Sanctuary && DataLoader.instance.getDS(dialogue_scene_sanctuary) == 1) gameObject.SetActive(false);
        if (miniboss_id == MinibossNanobotID.Pig && DataLoader.instance.getDS(dialogue_scene_pig) == 1) gameObject.SetActive(false);
        if (miniboss_id == MinibossNanobotID.Rage && DataLoader.instance.getDS(dialogue_scene_rage) == 1) gameObject.SetActive(false);
        if (miniboss_id == MinibossNanobotID.Cougher && DataLoader.instance.getDS(dialogue_scene_cougher) == 1) gameObject.SetActive(false);
        if (miniboss_id == MinibossNanobotID.Tongue && DataLoader.instance.getDS(dialogue_scene_tongue) == 1) gameObject.SetActive(false);

        songFadedOut = "PollenArea";
        if (miniboss_id == MinibossNanobotID.Cougher) songFadedOut = "Cougher";
        if (miniboss_id == MinibossNanobotID.Pig) songFadedOut = "Pig";
        if (miniboss_id == MinibossNanobotID.Rage) songFadedOut = "Rage";
        if (miniboss_id == MinibossNanobotID.Tongue) songFadedOut = "Tongue";

        HF.GetPlayer(ref player);
        state = GetComponent<EntityState2D>();
        anim = GetComponent<SpriteAnimator>();
        rb = GetComponent<Rigidbody2D>();
        ps = GetComponent<PositionShaker>();
        ps.enabled = false;
        dbox = GameObject.Find("Dialogue").GetComponent<DialogueBox>();

        // Player tracking shots - 1, 1, 3, 3, 3
        // Waypoint shots - 0, 0, 4, 4, 8
        // Health - 6,6 8,8, 10
        CCCMinibossesDefeated = 0;
        if (DataLoader.instance.getDS(dialogue_scene_tongue) == 1) CCCMinibossesDefeated++;
        if (DataLoader.instance.getDS(dialogue_scene_cougher) == 1) CCCMinibossesDefeated++;
        if (DataLoader.instance.getDS(dialogue_scene_pig) == 1) CCCMinibossesDefeated++;
        if (DataLoader.instance.getDS(dialogue_scene_rage) == 1) CCCMinibossesDefeated++;

        if (CCCMinibossesDefeated == 1) health = 7;
        if (CCCMinibossesDefeated == 2) health = 7;
        if (CCCMinibossesDefeated == 3) health = 8;
        print("Defeated " + CCCMinibossesDefeated + " bosses, health: " + health);
        maxHealth = health;

        for (int i = 0; i < 10; i++) {
            Bullet b = Instantiate(bulletPrefab, transform.parent).GetComponent<Bullet>();
            b.Die();
            bullets.Add(b);
        }

        int nrWaypoints = 3;
        if (miniboss_id == MinibossNanobotID.Pig) nrWaypoints = 5;
        if (miniboss_id == MinibossNanobotID.Rage) nrWaypoints = 4;
        if (miniboss_id == MinibossNanobotID.Cougher) nrWaypoints = 6;
        if (miniboss_id == MinibossNanobotID.Tongue) nrWaypoints = 3;
        for (int i = 0; i < nrWaypoints; i++) {
            waypoints.Add(transform.parent.Find("Waypoint" + (i+1).ToString()).position);
        }
        transform.position = waypoints[0];
        nextWaypoint = 1;
	}

    public int CCCMinibossesDefeated = 0;

    Vector2 cachedPauseVel;
    bool ispaused = false;
	void Update () {
        float delta = Time.deltaTime;

        if (ispaused) {
            if (!player.IsThereAReasonToPause()) {
                ispaused = false;
                rb.velocity = cachedPauseVel;
            }
            return;
        } else {
            if (player.IsThereAReasonToPause()) {
                cachedPauseVel = rb.velocity;
                rb.velocity = Vector2.zero;
                ispaused = true;
                return;
            }
        }

        if (Registry.DEV_MODE_ON && mode != 0) {
            if (health > 0 && Input.GetKeyDown(KeyCode.Alpha0)) {
                health = 1;
                TakeDamageRoutine();
            }
        }

        if (takeDamageCooldown > 0) takeDamageCooldown -= Time.deltaTime;

        // When not dead, hurt the player
        if (t_damagePlayer > 0) t_damagePlayer  -= delta;
        if (onPlayer && mode != MODE_DYING_ANIM && mode != MODE_DEAD && mode != MODE_DYING_SPEECH) {
            if (t_damagePlayer <= 0) {
                player.Damage(1);
                player.Bump(true, 10f);
                t_damagePlayer = 1f;
            }
        }

        // Spawned rock becomes breakable when sucked in. When it breaks, it gets destroyed so a new one can be spawned
        if (rockSpawn != null) {
            if (rockSpawn.isPickedUp()) {
                rockSpawn.IsBreakable = true;
            }
            if (rockSpawn.isBroken()) {
                Destroy(rockSpawn.gameObject);
                rockSpawn = null;
            }
        }

        if (mode == MODE_HURTSTUN) { // Wait a bit when hurt then return to previous state
            hurt_timer -= delta;
            if (hurt_timer < 0) {
                mode = cached_mode;
                rb.velocity = cached_vel;
                ps.enabled = false;
            }
        } else if (mode == 0) { // Wait for player to enter, play dialogue
            if (trigger.onPlayer2D) {
                mode = 1;
                if (songFadedOut != "") {
                    AudioHelper.instance.FadeSong(songFadedOut, 1, 0);
                } else {
                    AudioHelper.instance.StopAllSongs(true);
                }
                if (miniboss_id == MinibossNanobotID.Sanctuary) dbox.playDialogue(dialogue_scene_sanctuary, 0);
                if (miniboss_id == MinibossNanobotID.Rage) dbox.playDialogue(dialogue_scene_rage, 0);
                if (miniboss_id == MinibossNanobotID.Tongue) dbox.playDialogue(dialogue_scene_tongue, 0);
                if (miniboss_id == MinibossNanobotID.Cougher) dbox.playDialogue(dialogue_scene_cougher, 0);
                if (miniboss_id == MinibossNanobotID.Pig) dbox.playDialogue(dialogue_scene_pig, 0);
            }
        } else if (mode == 1 && dbox.isDialogFinished()) { // Start music
            mode = 2;
            AudioHelper.instance.PlaySong(bossSongName,0,42.857f);
            AudioHelper.instance.playOneShot("nanobotCry");
            HF.SendSignal(state.children, "unsignal"); // Close gates
        } else if (mode == 2) { // Wait to move

            if (HF.TimerDefault(ref t_waitAtWaypoint,getWaitTime())) {
                mode = 3;
                t_waitAtWaypoint = 0;
                t_shootAfterLeavingWaypoint = 0.3f;
                shotAfterLeavingWaypoint = false;
            }
        } else if (mode == 3) { // Moving, shoot once, when reach next waypoint, shoot.
            t_shootAfterLeavingWaypoint -= delta;
            if (t_shootAfterLeavingWaypoint < 0 && !shotAfterLeavingWaypoint) {
                int aliveblocks = 0;
                foreach (Vacuumable v in blockChildren) {
                    if (!v.isBroken()) {
                        aliveblocks++;
                    }
                }
                if (aliveblocks == 0 && rockSpawn == null) {
                    rockSpawn = Instantiate(rockSpawnPrefab, transform.parent).GetComponent<Vacuumable>();
                    rockSpawn.transform.position = transform.position;
                    rockSpawn.gameObject.layer = gameObject.layer; // Prevent from going on holes
                    rockSpawn.GetComponent<Rigidbody2D>().velocity = (player.transform.position - rockSpawn.transform.position).normalized * getBulletVel();
                } else {
                    anim.Play("attack");
                    anim.ScheduleFollowUp("idle");
                    ShootAtPlayer(getBulletVel());
                }
                shotAfterLeavingWaypoint = true;
            }

            rb.velocity = (waypoints[nextWaypoint] - transform.position).normalized * getHealthScaledVal(rampedMovementVel);
            if (Vector2.Distance(waypoints[nextWaypoint],transform.position) < waypointThreshold) {
                mode = 2;
                rb.velocity = Vector2.zero;
                nextWaypoint++;
                if (nextWaypoint >= waypoints.Count) nextWaypoint = 0;
                if (miniboss_id != MinibossNanobotID.Sanctuary && CCCMinibossesDefeated >= 1) {
                    anim.Play("attack");
                    anim.ScheduleFollowUp("idle");
                    if (CCCMinibossesDefeated == 3) {
                        ShootFourBurst(getBulletVel()*0.75f);
                    } else if (CCCMinibossesDefeated >= 1) {
                        ShootTwoHor(getBulletVel() * 0.75f);
                    }
                }
            }
        } else if (mode == MODE_DYING_SPEECH) {
            hurt_timer -= delta;
            if (dbox.isDialogFinished()) {
                hurt_timer = -1;
                AudioHelper.instance.playSFX("nanobotDie");
                mode = MODE_DYING_ANIM;
                deathFXExplosions.Play();
                deathFXDust.Play();
            }
            if (hurt_timer <= 0) {
                ps.enabled = false;
            }

        } else if (mode == MODE_DYING_ANIM) {


            if (dyingTimer > 0) {

                if (tDeathSound > 0.11f) {
                    tDeathSound = 0;
                    AudioHelper.instance.playOneShot("bluntExplosion",Mathf.Lerp(0.2f,1f,dyingTimer/3f),0.95f+0.1f*Random.value);
                } else {
                    tDeathSound += Time.deltaTime;
                }

                dyingTimer -= Time.deltaTime;
                if (dyingTimer < 0) dyingTimer = 0;
                ps.enabled = true;
                dyingCol = GetComponent<SpriteRenderer>().color;
                dyingCol.a = dyingTimer / 3f;
                dyingCol.g -= Time.deltaTime * 0.18f;
                dyingCol.b -= Time.deltaTime * 0.18f;
                GetComponent<SpriteRenderer>().color = dyingCol;
                tempScale = transform.localScale;
                tempScale.x -= Time.deltaTime * 0.18f;
                tempScale.y = tempScale.x;
                transform.localScale = tempScale;
                ps.amplitude.x = ps.amplitude.x + Time.deltaTime * 0.15f;
                ps.amplitude.y = ps.amplitude.y + Time.deltaTime * 0.15f;
            } else {
                deathFXExplosions.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                deathFXDust.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                mode = MODE_DEAD;
                HF.SendSignal(state.children);
                if (miniboss_id == MinibossNanobotID.Sanctuary) DataLoader.instance.setDS(dialogue_scene_sanctuary, 1);
                if (miniboss_id == MinibossNanobotID.Rage) DataLoader.instance.setDS(dialogue_scene_rage, 1);
                if (miniboss_id == MinibossNanobotID.Tongue) DataLoader.instance.setDS(dialogue_scene_tongue, 1);
                if (miniboss_id == MinibossNanobotID.Cougher) DataLoader.instance.setDS(dialogue_scene_cougher, 1);
                if (miniboss_id == MinibossNanobotID.Pig) DataLoader.instance.setDS(dialogue_scene_pig, 1);

                if (songFadedOut != "") {
                    AudioHelper.instance.FadeSong(songFadedOut, 1, 1);
                }

                GetComponent<SpriteRenderer>().enabled = false;
            }
        } else if (mode == MODE_DEAD) {

        }
	}
    Vector3 tempScale = new Vector3();
    Color dyingCol = new Color();
    float dyingTimer = 3f;
    float tDeathSound = 0;

    void ShootAtPlayer(float vel, float rotation=0) {
        Bullet b = Bullet.GetADeadBullet(bullets);
        if (b == null) return;
        b.StartVelocity = vel;
        b.LaunchAt(transform.position, player.transform.position,rotation);
    }

    void ShootTwoHor(float vel) {
        for (int i = 0; i < 2; i++) {
            Bullet b = Bullet.GetADeadBullet(bullets);
            if (b == null) continue;
            b.StartVelocity = vel;
            if (i == 0) b.LaunchInCardinalDir(transform.position, AnoControl2D.Facing.RIGHT);
            if (i == 1) b.LaunchInCardinalDir(transform.position, AnoControl2D.Facing.LEFT);
        }
    }

    void ShootFourBurst(float vel) {
        for (int i = 0; i < 4; i++) {
            Bullet b = Bullet.GetADeadBullet(bullets);
            if (b == null) continue;
            b.StartVelocity = vel;
            if (i == 0) b.LaunchInCardinalDir(transform.position, AnoControl2D.Facing.UP);
            if (i == 1) b.LaunchInCardinalDir(transform.position, AnoControl2D.Facing.RIGHT);
            if (i == 2) b.LaunchInCardinalDir(transform.position, AnoControl2D.Facing.DOWN);
            if (i == 3) b.LaunchInCardinalDir(transform.position, AnoControl2D.Facing.LEFT);
        }
    }
    void ShootRotatedFourBurst(float vel) {
        Vector2 pos = transform.position;
        for (int i = 0; i < 4; i++) {
            Bullet b = Bullet.GetADeadBullet(bullets);
            if (b == null) continue;
            b.StartVelocity = vel;
            if (i == 0) b.LaunchAt(pos, pos + Vector2.up, 45);
            if (i == 1) b.LaunchAt(pos, pos + Vector2.right, 45);
            if (i == 2) b.LaunchAt(pos, pos + Vector2.down, 45);
            if (i == 3) b.LaunchAt(pos, pos + Vector2.left, 45);
        }
    }

    float getHealthScaledVal(Vector2 v) {
        return v.x + (v.y - v.x) * (1 - (health / maxHealth));
    }

    float getWaitTime() {
        return rampedWaypointWaitTime.x + (rampedWaypointWaitTime.y - rampedWaypointWaitTime.x) * (1 - (health / maxHealth));
    }
    float getBulletVel() {
        return rampedBulletVel.x + (rampedBulletVel.y - rampedBulletVel.x) * (1 - (health / maxHealth));
    }
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.name == "2D Ano Player") {
            onPlayer = true;
        } else if (collision.GetComponent<Vacuumable>() != null) {
            if (takeDamageCooldown > 0) {

            } else {
                Vacuumable v = collision.GetComponent<Vacuumable>();
                if (v.isMoving()) {
                    v.Break();
                    TakeDamageRoutine();

                }
            }
        }
    }
    void TakeDamageRoutine() {

        takeDamageCooldown = 1;
        health--;

        cached_vel = rb.velocity;
        cached_mode = mode;

        hurt_timer = 1f;
        rb.velocity = Vector2.zero;
        ps.enabled = true;

        if (health == 0) {
            anim.Play("hurt-loop");
            AudioHelper.instance.playSFX("nanobotHurt");
            AudioHelper.instance.StopSongByName(bossSongName);
            mode = MODE_DYING_SPEECH;
            Bullet.DieAll(bullets);
            if (miniboss_id == MinibossNanobotID.Sanctuary) dbox.playDialogue(dialogue_scene_sanctuary, 1);
            if (miniboss_id == MinibossNanobotID.Rage) dbox.playDialogue(dialogue_scene_rage, 1);
            if (miniboss_id == MinibossNanobotID.Tongue) dbox.playDialogue(dialogue_scene_tongue, 1);
            if (miniboss_id == MinibossNanobotID.Cougher) dbox.playDialogue(dialogue_scene_cougher, 1);
            if (miniboss_id == MinibossNanobotID.Pig) dbox.playDialogue(dialogue_scene_pig, 1);
        } else {
            anim.Play("hurt");
            anim.ScheduleFollowUp("idle");
            AudioHelper.instance.playSFX("nanobotHurt");
            mode = MODE_HURTSTUN;
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.name == "2D Ano Player") {
            onPlayer = false;
        }
    }
}
