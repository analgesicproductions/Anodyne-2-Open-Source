using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Anodyne {
    [RequireComponent(typeof(Rigidbody2D),typeof(CircleCollider2D),typeof(SpriteRenderer))]
    [RequireComponent(typeof(Vacuumable), typeof(SpriteAnimator),typeof(EntityState2D))]
    public class OceanMole : MonoBehaviour {
        AnoControl2D player;

        EntityState2D state;
        Rigidbody2D rb;
        Vacuumable vac;
        CircleCollider2D cc;
        SpriteAnimator anim;
        public float movementVel = 3f;
        public Vector2 initialDirInOnes = new Vector2(1, 0);
        int mode = 0;
        ParticleSystem particles;

        void Start() {
            particles = transform.Find("MoleTrailParticlesOriginal").GetComponent<ParticleSystem>();
            state = GetComponent<EntityState2D>();
            rb = GetComponent<Rigidbody2D>();
            HF.GetPlayer(ref player);
            vac = GetComponent<Vacuumable>();
            cc = GetComponent<CircleCollider2D>();
            anim = GetComponent<SpriteAnimator>();

            vac.overrideIdleDeceleration = true;
            anim.PlayInitialAnim();

            vac.IsRooted = false;
            vac.IsPickupable = true;
            vac.IsBreakable = true;

            vac.enabled = false;
        }

        float tDamage = 0;
        Vector3 lastPos = new Vector3();
        void Update() {

            if (player.isPaused) {
                transform.position = lastPos;
                return;
            }
            lastPos = transform.position;

            if (vac.state == Vacuumable.VacuumMode.PickedUp) {
                if (player.CameraIsChangingRooms() || !player.InThisRoom(state.initRoomPos)) {
                    player.Swallow();
                    vac.state = Vacuumable.VacuumMode.Broken;
                }
            }

            if (HF.isMovingOutOfRoomX(rb.velocity, transform.position.x, state.initRoomPos)) {
                doBounceVertSurface = true;
            } else if (HF.isMovingOutOfRoomY(rb.velocity, transform.position.y, state.initRoomPos)) {
                doBounceHorSurface = true;
            } 
            if (state.initializedEntity && state.NeedsToResetIfPlayerOutsideInitRoom(player)) {
                tDamage = 0;
                mode = 0;
                vac.Respawn();

                vac.enabled = false;
                state.initializedEntity = false;

                particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            }

            if (mode == 0) { // init
                if (player.InSameRoomAs(transform.position)) { 

                    if (!state.initializedEntity) {
                        state.initializedEntity = true;
                        state.tempVel = initialDirInOnes.normalized * movementVel;
                    }
                    if (particles.isStopped) {
                        particles.Play(true);
                    }
                    vac.enabled = true;
                    cc.isTrigger = true;
                    mode = 1;
                    rb.bodyType = RigidbodyType2D.Dynamic;
                }

            } else if (mode == 1) { // moving
                state.ConstrainPositionToCurrentRoom();
                if (!vac.IsBeingSucked()) {
                    rb.velocity = state.tempVel;
                }
                if (doBounceHorSurface) {
                    doBounceHorSurface = false;
                    state.tempVel = rb.velocity; state.tempVel.y *= -1; rb.velocity = state.tempVel;
                }
                if (doBounceVertSurface) {
                    doBounceVertSurface = false;
                    state.tempVel = rb.velocity; state.tempVel.x *= -1; rb.velocity = state.tempVel;
                }

                if (tDamage> 0) {
                    tDamage -= Time.deltaTime;
                }
                if (!vac.IsBeingSucked()) {
                    if (transform.localScale.x <= 1) {
                        transform.localScale = Vector3.one;
                    }
                }
                if (vac.state == Vacuumable.VacuumMode.PickedUp) {
                    mode = 5;
                    cc.isTrigger = false;
                    particles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                }
            } else if (mode == 5) { // sucked up
                if (vac.state == Vacuumable.VacuumMode.Broken) {
                    mode = 6;
                    cc.isTrigger = true;
                    state.tempVel = initialDirInOnes.normalized * movementVel;
                }
            } else if (mode == 6) { // Dead
                if (vac.state == Vacuumable.VacuumMode.Idle) {
                    mode = 0;
                }
            }

            if (vac.JustBrokeResetExternally) {
                vac.JustBrokeResetExternally = false;
                HF.SendSignal(state.children);
                if (mode == 4) {
                    particles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                    mode = 6;
                }
            }
        }


        private bool doBounceVertSurface = false;
        private bool doBounceHorSurface = false;
        private void OnTriggerEnter2D(Collider2D collision) {
            if (collision.name.IndexOf("Raft") != -1) return;
            if (vac.isMoving()) return;
            if (vac.IsBeingSucked() == false && tDamage <= 0 && collision.CompareTag("Player")) {
                tDamage = 1;
                player.Bump(true);
                player.Damage(1);
                AudioHelper.instance.playOneShot("bluntExplosion", 1, 1.4f);
                return;
            }
            if (collision.gameObject.layer == 9) return;
            if (collision.gameObject.GetComponent<OceanMole>() != null) {
                doBounceHorSurface = doBounceVertSurface = true;
                return;
            }



            Vector3 v = rb.velocity * Time.fixedDeltaTime * 2;
            state.tempPos = transform.position - v;
            transform.position = state.tempPos;

            int layerMask = ~(1 << 13 | 1 << 9);
            hit = Physics2D.Raycast(transform.position, Vector2.left, .75f,layerMask);
            if (v.x <= 0 && hit.collider != null && hit.collider.name.IndexOf("Tile") != -1) {
                doBounceVertSurface = true;
            }
            hit = Physics2D.Raycast(transform.position, Vector2.up, .75f, layerMask);
            if (v.y >= 0 && hit.collider != null && hit.collider.name.IndexOf("Tile") != -1) {
                doBounceHorSurface = true;
            }
            hit = Physics2D.Raycast(transform.position, Vector2.right, .75f, layerMask);
            if (v.x >= 0 && hit.collider != null && hit.collider.name.IndexOf("Tile") != -1) {
                doBounceVertSurface = true;
            }
            hit = Physics2D.Raycast(transform.position, Vector2.down, .75f, layerMask);
            if (v.y <= 0 && hit.collider != null && hit.collider.name.IndexOf("Tile") != -1) {
                doBounceHorSurface = true;
            }
            if (doBounceHorSurface == false && doBounceVertSurface == false) {
                doBounceVertSurface = doBounceHorSurface = true;
            }
        }
        RaycastHit2D hit = new RaycastHit2D();

    }   
}