using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Anodyne {
    [RequireComponent(typeof(Rigidbody2D),typeof(CircleCollider2D),typeof(SpriteRenderer))]
    [RequireComponent(typeof(Vacuumable), typeof(SpriteAnimator),typeof(EntityState2D))]
    public class Mole : MonoBehaviour {
        AnoControl2D player;

        EntityState2D state;
        Rigidbody2D rb;
        Vacuumable vac;
        CircleCollider2D cc;
        SpriteAnimator anim;
      //  SpriteRenderer sr;
        TriggerChecker bcTrig;
        public float movementVel = 3f;
        public Vector2 initialDirInOnes = new Vector2(1, 0);
        int mode = 0;
        ParticleSystem particles;

        void Start() {
            particles = transform.Find("MoleTrailParticlesOriginal").GetComponent<ParticleSystem>();
            state = GetComponent<EntityState2D>();
            rb = GetComponent<Rigidbody2D>();
         //   sr = GetComponent<SpriteRenderer>();
            HF.GetPlayer(ref player);
            vac = GetComponent<Vacuumable>();
            cc = GetComponent<CircleCollider2D>();
            anim = GetComponent<SpriteAnimator>();
            bcTrig = transform.Find("TriggerChecker").GetComponent<TriggerChecker>();

            vac.overrideIdleDeceleration = true;
            anim.PlayInitialAnim();
        }

        float tPopup = 0;
        Vector3 lastPos = new Vector3();
        void Update() {
            state.onPlayer = bcTrig.onPlayer2D;

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
                tPopup = 0;
                mode = 0;
                vac.Respawn();
                state.initializedEntity = false;

                particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            vac.IsRooted = vac.IsPickupable = vac.IsBreakable = false;
            }

            if (mode == 0) { // init
                if (HF.AreTheseInTheSameroom(transform, player.transform)) {

                    if (!state.initializedEntity) {
                        state.initializedEntity = true;
                        state.tempVel = initialDirInOnes.normalized * movementVel;
                    }
                    if (particles.isStopped) {
                        particles.Play(true);
                    }
                    mode = 1;
                    // A rooted body becomes kinematic when sucked in so needs to be reset here
                    rb.bodyType = RigidbodyType2D.Dynamic;
                    cc.isTrigger = false;
                    vac.IsRooted = vac.IsPickupable = vac.IsBreakable = false;
                }

            } else if (mode == 1) { // moving
                rb.velocity = state.tempVel;
                state.ConstrainPositionToCurrentRoom();
                if (doBounceHorSurface) {
                    doBounceHorSurface = false;
                    state.tempVel = rb.velocity; state.tempVel.y *= -1; rb.velocity = state.tempVel;
                }
                if (doBounceVertSurface) {
                    doBounceVertSurface = false;
                    state.tempVel = rb.velocity; state.tempVel.x *= -1; rb.velocity = state.tempVel;
                }
                if (state.onPlayer) {
                    anim.Play("steppedOn");
                    AudioHelper.instance.playOneShot("bluntExplosion", 1, 1.4f);
                    rb.velocity = Vector2.zero;
                    mode = 2;
                    cc.isTrigger = true; // So player can step on top of it
                }
            } else if (mode == 2) { // hit and waiting to pop
                if (HF.TimerDefault(ref tPopup,0.56f)) {
                    anim.Play("popUp");
                    if (state.onPlayer) {
                    //    mode = 3;
                        player.Bump(true);
                        player.Damage(1);
                    } 
                    vac.IsRooted = true;
                    vac.IsPickupable = true;
                    vac.IsBreakable = true;
                    vac.RequiredUnrootingTime = 0.2f;
                    vac.RootedPosition = transform.position;
                    mode = 4;
                    cc.isTrigger = false;
                }
            } else if (mode == 3) { // hurt player, wait a bit then return to moving
                rb.velocity = Vector2.zero;
                if (HF.TimerDefault(ref tPopup,0.5f)) {
                    mode = 1;
                    anim.PlayInitialAnim();
                }
            } else if (mode == 4) { // popped up, suckable and also collideable/hard
                if (vac.IsBeingSuckedAndMoving) {
                    mode = 5;
                    particles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                } else if (HF.TimerDefault(ref tPopup, 3f)) {
                    vac.IsRooted = vac.IsPickupable = vac.IsBreakable = false;
                    mode = 1;
                    anim.PlayInitialAnim();
                }
            } else if (mode == 5) { // sucked up
                if (vac.state == Vacuumable.VacuumMode.Broken) {
                    mode = 6;
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
        // Always called regardless of triggerness
        private void OnCollisionEnter2D(Collision2D collision) {
            if (mode == 4 && cc.isTrigger == false && collision.collider.CompareTag("Player")) {
                player.Bump(true);
                player.Damage(1);
            }
            if (collision.gameObject.GetComponent<Mole>() != null) {
                doBounceHorSurface = doBounceVertSurface = true;
                return;
            }
            HF.GetContactWallsCC(ref doBounceVertSurface, ref doBounceHorSurface, cc, collision);
            Vector3 v = rb.velocity * Time.fixedDeltaTime * 2;
            state.tempPos = transform.position - v;
            transform.position = state.tempPos;
        }

    }   
}