using UnityEngine;
using UnityEngine.Tilemaps;
namespace Anodyne {
    public class Vacuumable : MonoBehaviour {
        bool isATB_Fight = false;
        Transform ATB_Bar_Trans;
        public bool IsRooted = false;
        public bool Unrootable = false;
        int RootedMode = 0;
        [Tooltip("Set to -1 for this to shake but never be sucked up.")]
        public float RequiredUnrootingTime = 0.25f;
        Vector2 InitialPosition;
        [HideInInspector]
        public Vector2 RootedPosition;
        public float ShakeMagnitude = 1.1f / 16f;
        float t_Shake;
        float tm_Shake = 0.04f;
        float t_TimeUnrootedSoFar = 0f;
        float forceSuckingThreshold = 1.4f;

        public bool IsBreakable = false;
        public bool IsPickupable = false;
        public bool BreaksWhenShotOutsideRoom = true;
        [Tooltip("RespawnsWhenBrokenAndPlayerLeavesRoom.  OFTEN TRUE. If this thing breaks, respawns when you re-enter room. Unset for troublesome enemies or one-time Vacuumables.")]
        public bool RespawnsWhenBrokenAndPlayerLeavesRoom = true;
        bool NeedsToRespawn = false;
        [System.NonSerialized]
        public bool DOESNTRESPAWN = false;

        public string SignalDA2OnBreak = "";

        public float deceleration = 10f;
        public float shootVelocity = 15f;
        public float getSuckedInVel = 10f;
        public float getSuckedInAccelFactor = 2f;

        [HideInInspector]
        public bool CanBeSucked_SetExternally = true;

        SpriteAnimator spriteAnimator;

        [HideInInspector]
        public enum VacuumMode { Idle, PickedUp, Moving, Breaking, Broken }
        [HideInInspector]
        public VacuumMode state = 0;
        [HideInInspector]
        public bool IsBeingSuckedAndMoving = false;
        bool ForceSuckingBecauseBeingSuckedCloseToPlayer = false;

        Rigidbody2D rb;
        AnoControl2D player;
        Vector3 newpos;
        TilemapCollider2D tc1;
        CircleCollider2D cc1;
        BoxCollider2D bc1;
        Vector2 bc1_originalSize;
        float cc1_originalRadius;
        [HideInInspector]
        public Vector2Int initRoom;
        public bool usesDefaultSwallow = false;
        public bool hurtsPlayer = false;
        bool hurtsPlayerZeroidHack = false;
        float tHPZH = 0;
        // for wrestling
        public bool breaksWhenMovingAndShootReleased = false;

        // Used to disable and enable triggers for things when they are being sucked in close.
        bool colliderStartsAsTrigger = false;
        int initialLayer = 0;
        public bool respawnImmediately = false;

        public void updateRootedInitialPos(Vector3 v) {
            RootedPosition = InitialPosition = v;
        }

        [System.NonSerialized]
        public bool inPicoScene = false;
        void Start() {

            if (name == "ZeroidBloodGate") {
                hurtsPlayerZeroidHack = true;
            }

            originalScale = transform.localScale.x;

            string curSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if ("PicoFantasy" == curSceneName) {
                if (name.IndexOf("ATB_") != -1) {
                    isATB_Fight = true;
                    ATB_Bar_Trans = GameObject.Find("ATB_Bar").transform;
                    
                }
            }
            if (curSceneName.IndexOf("Pico") == 0) {
                inPicoScene = true;
                getSuckedInVel /= 2f;
                shootVelocity /= 2f;
                forceSuckingThreshold /= 2f;
                getSuckedInAccelFactor /= 2f;
                
            }

            initialLayer = gameObject.layer;

            if (transform.position.z != 0) {
                Vector3 zerod = transform.position;
                zerod.z = 0; transform.position = zerod;
            }
            InitialPosition = transform.position;
                
            RootedPosition = InitialPosition;
            HF.GetRoomPos(transform.position, ref initRoom);
            rb = GetComponent<Rigidbody2D>();

            if (GameObject.Find("TilemapDesign") != null) {
                tc1 = GameObject.Find("TilemapDesign").GetComponent<TilemapCollider2D>();
            }
            if (GameObject.Find("Tilemap") != null) {
                tc1 = GameObject.Find("Tilemap").GetComponent<TilemapCollider2D>();
            }
            cc1 = GetComponent<CircleCollider2D>();
            bc1 = GetComponent<BoxCollider2D>();
            if (cc1 != null) cc1_originalRadius = cc1.radius;
            if (bc1 != null) bc1_originalSize = bc1.size;

            if (cc1 != null) colliderStartsAsTrigger = cc1.isTrigger;
            if (bc1 != null) colliderStartsAsTrigger = bc1.isTrigger;

            player = GameObject.Find("2D Ano Player").GetComponent<AnoControl2D>();
            if (GetComponent<SpriteAnimator>() != null) {
                spriteAnimator = GetComponent<SpriteAnimator>();
            }
        }

        float SuckInScaleValue = 1f;
        [HideInInspector]
        public Vector2 preCollisionVelocity;
        [HideInInspector]
        public Vector2 preCollisionPosition;
        [HideInInspector]
        public bool overrideIdleDeceleration = false;

        [System.NonSerialized]
        public bool ext_DisablesOnPickupOverlap = false;
        public bool isOnHoleOrNull() {
            return TilemetaManager.instance.IsHole(transform.position) || TilemetaManager.instance.IsNull(transform.position);
        }
        public void Respawn() {
            GetComponent<SpriteRenderer>().enabled = true;
            SetColliderEnableState(true);
            if (!colliderStartsAsTrigger) {
                ChangeIsTriggerTo(false);
            }
            state = VacuumMode.Idle;
            t_Shake = 0; RootedMode = 0;
            spriteAnimator.PlayInitialAnim();
            transform.position = InitialPosition;
            rb.velocity = Vector2.zero;
        }

        Vector2 newvel = new Vector2();
        void Update() {

            
//            if (bc1 != null) {
               // Debug.DrawRay(transform.position, Vector3.down, Color.red, bc1.size.x * 0.5f * bc1.transform.localScale.x + bc1.edgeRadius + 0.1f);
                //Debug.DrawRay(transform.position, Vector3.up, Color.blue, bc1.size.x * 0.5f*bc1.transform.localScale.x + bc1.edgeRadius + 0.1f);
  //          }
            //if (cc1 != null) {
               // Debug.DrawRay(transform.position,Vector3.down, Color.red, cc1.radius * cc1.transform.localScale.x + 0.1f);
               // Debug.DrawRay(transform.position,Vector3.up, Color.blue, cc1.radius * cc1.transform.localScale.x + 0.1f);
           // }

            newvel = rb.velocity;
            if (!IsBeingSuckedAndMoving && !(overrideIdleDeceleration && VacuumMode.Idle == state)) {
                if (newvel.x > 0) newvel.x -= Time.deltaTime * deceleration;
                if (newvel.x < 0) newvel.x += Time.deltaTime * deceleration;
                if (newvel.y > 0) newvel.y -= Time.deltaTime * deceleration;
                if (newvel.y < 0) newvel.y += Time.deltaTime * deceleration;
                if (newvel.magnitude < 1f) {
                    newvel = Vector2.zero;
                }
            }
            rb.velocity = newvel;

            if (isATB_Fight) {
                if (!DataLoader.instance.isPaused && !atbShotOnce && state == VacuumMode.Idle && !IsBeingSucked()) {
                    newvel = transform.position;
                    newvel.x += Time.deltaTime * 0.6f;
                    transform.position = newvel;
                }
                
                if (state != VacuumMode.PickedUp && !player.InTheSameRoomAs(transform.position.x,transform.position.y)) {
                    Destroy(gameObject);
                    return;
                }
            }

            if (usesDefaultSwallow && state == VacuumMode.PickedUp) {
                if (player.CameraIsChangingRooms() || !player.InThisRoom(initRoom)) {
                    player.Swallow();
                    state = VacuumMode.Broken;
                }
            }

            if (NeedsToRespawn && !player.CameraIsChangingRooms() && !player.InSameRoomAs(InitialPosition) && VacuumMode.Idle == state) {
                NeedsToRespawn = false;
                Respawn();
            }
            if (!NeedsToRespawn && !DOESNTRESPAWN && player.InSameRoomAs(InitialPosition)) {
                NeedsToRespawn = true;
            }

            if (VacuumMode.Idle == state) {

                if (CanBeSucked_SetExternally) {
                    IsBeingSuckedAndMoving = false;
                    /*
                     * Rooted objects
                     * 
                     * */
                    if (IsRooted) {
                        if (RootedMode == 0) {
                            if (player.isAbleToPickUp() && SuckZoneIsOverlapping && player.InSameRoomAs(transform.position) && MyInput.special) {
                                float speedupfactor = (Vector2.Distance(transform.position, player.transform.position));
                                speedupfactor = Mathf.Lerp(1.2f, 1f, speedupfactor / 4.0f);
                                if (player.GetNameOfObjectInFront() == name) {
                                    speedupfactor *= 2.2f;
                                }
                                t_Shake += Time.deltaTime*speedupfactor;
                                if (t_Shake > tm_Shake) {
                                    t_Shake -= tm_Shake;
                                    newpos = RootedPosition;
                                    if (Random.value < 0.333f) {  newpos.x -= ShakeMagnitude; } else if (Random.value < 0.5f)  { newpos.x += ShakeMagnitude; }
                                    if (Random.value < 0.333f) {  newpos.y -= ShakeMagnitude; } else if (Random.value < 0.5f) { newpos.y += ShakeMagnitude;  }
                                    transform.position = newpos;
                                }

                                t_TimeUnrootedSoFar += Time.deltaTime*speedupfactor;
                                if (Unrootable) t_TimeUnrootedSoFar = 0;
                                if (t_TimeUnrootedSoFar > RequiredUnrootingTime) {
                                    RootedMode = 1;
                                    t_TimeUnrootedSoFar = 0;
                                    ChangeIsTriggerTo(true);
                                    player.enterUnrootWaitForObjectMode();
                                }
                            } else {
                                if (!MyInput.special) {
                                    SuckZoneIsOverlapping = false;
                                }
                                t_TimeUnrootedSoFar = 0;
                                transform.position = RootedPosition;
                            }
                        } else if (RootedMode == 1) {
                            MoveTowardsPickupRegion();
                            if (IsPickupable && PickupRegionIsOverlapping) {
                                rb.velocity = Vector2.zero;
                                if (ext_DisablesOnPickupOverlap) {
                                    //player.cancelUnrootWait();
                                    enabled = false;
                                    return;
                                }
                                if (SuckInScaleValue < 0.5f) {
                                    GetPickedUp();
                                    RootedMode = 0;
                                }
                            }
                        }
                    /**
                     *  Unrooted Objects
                     * 
                     * */
                    } else {
                        if (ForceSuckingBecauseBeingSuckedCloseToPlayer) {
                            MoveTowardsPickupRegion();
                            if (IsPickupable && PickupRegionIsOverlapping) {
                                rb.velocity = Vector2.zero;
                                if (SuckInScaleValue < 0.5f) {
                                    ForceSuckingBecauseBeingSuckedCloseToPlayer = false;
                                    GetPickedUp();
                                    if (isATB_Fight) {
                                        if (name.IndexOf("ATB_Heart") != -1) {
                                            player.GetComponent<HealthBar>().Heal();
                                            AudioHelper.instance.playOneShot("vacuumSuckDust");
                                            player.Swallow();
                                            state = VacuumMode.Broken;
                                            return;
                                        }
                                    }
                                }
                            }
                        } else if (player.isAbleToPickUp() && SuckZoneIsOverlapping && player.InSameRoomAs(transform.position) && MyInput.special) {
                            if (isATB_Fight) {
                                if (ATB_Bar_Trans.position.y >= transform.position.y) {
                                    return;
                                }
                            }

                            MoveTowardsPickupRegion();
                            if (IsPickupable && Vector3.Distance(player.getCenterOfPickupRegion(),transform.position) < forceSuckingThreshold) {
                                ForceSuckingBecauseBeingSuckedCloseToPlayer = true;
                                if (!colliderStartsAsTrigger) ChangeIsTriggerTo(true);
                                player.enterUnrootWaitForObjectMode();
                            }
                        } else {
                            if (!MyInput.special) {
                                SuckZoneIsOverlapping = false; // also done in rooted - bc after something is sucked in, the suck zone turnso ff so this variable cannot be reset in ontriggerexit.
                            }
                            // Not sucking -> Return scale to normal
                            if (SuckInScaleValue != originalScale) {
                                SuckInScaleValue += Time.deltaTime*3;
                                if (SuckInScaleValue > originalScale) SuckInScaleValue = originalScale;
                                framesLeftBeforeSuckInScalingStarts = 5;
                                SetScaleToSuckinVector();
                                if (!colliderStartsAsTrigger) ChangeIsTriggerTo(false);
                            }

                        }
                    }
                }
            } else if (VacuumMode.PickedUp == state) {
                newpos = player.getCenterOfPickupRegion();
                transform.position = newpos;
                if (player.hasJustShot()) {

                    if (canBeReleased()) {
                        if (hurtsPlayerZeroidHack) {
                            tHPZH = 0.2f;
                        }
                        newpos = player.transform.position;
                        Vector2 extraExtend = 1.1f*player.getFacingDirVector() * (player.GetWorldspaceRadius() + GetColliderHalfWidth());
                        newpos.x += extraExtend.x; newpos.y += extraExtend.y;
                        transform.position = newpos;
                        player.confirmRelease();
                        if (player.facing == AnoControl2D.Facing.UP || player.facing == AnoControl2D.Facing.DOWN) {
                            MoveAwayFromWall(true);
                        } else {
                            MoveAwayFromWall(false);

                        }

                        GetComponent<SpriteRenderer>().enabled = true;
                        player.ChangeSuckedItemSprite(null);
                        if (IsRooted) rb.bodyType = RigidbodyType2D.Dynamic;
                        SetColliderEnableState(true);
                        if (MakeCollideWhenThrown) ChangeIsTriggerTo(false);
                        if (MakeTriggerWhenThrown) ChangeIsTriggerTo(true);
                        if (ext_MakeNoncollidWhenThrown) ChangeIsTriggerTo(true);
                        rb.velocity = shootVelocity * player.getFacingDirVector();
                        state = VacuumMode.Moving; // Remove this for death movement bug
                        gameObject.layer = LayerMask.NameToLayer("Throwable");
                    } else {
                        player.cancelRelease();
                    }
                }

            } else if (VacuumMode.Moving == state) {
                if (hurtsPlayerZeroidHack) {
                    tHPZH -= Time.deltaTime;
                }
                if (isATB_Fight) {
                    if (name.IndexOf("ATB_Shield") != -1) {
                        atbShotOnce = true;
                        Vector2 tempVel = rb.velocity;
                        tempVel.x *= 0.5f;
                        tempVel.y *= 0.5f;
                        if (Mathf.Abs(tempVel.x) < 0.5f && Mathf.Abs(tempVel.y) < 0.5f) {
                            tempVel.Set(0, 0);
                            rb.velocity = tempVel;
                                
                            ChangeIsTriggerTo(false);
                            gameObject.layer = initialLayer;
                            state = VacuumMode.Idle;
                            tempVel = transform.position;
                            tempVel.y *= 2f; tempVel.y = Mathf.FloorToInt(tempVel.y); tempVel.y = tempVel.y / 2f;
                            transform.position = tempVel;
                            return;
                        }
                        rb.velocity = tempVel;
                    }
                    return;
                }

                if (rb.velocity.magnitude < 1f) {   
                    gameObject.layer = initialLayer;
                    state = VacuumMode.Idle;
                }
                if (BreaksWhenShotOutsideRoom) {
                    if (!HF.AreTheseInTheSameroom(transform, player.transform)) {
                        Break();
                    }
                }
                if (breaksWhenMovingAndShootReleased && !MyInput.special) {
                    Break();
                }
            } else if (VacuumMode.Breaking == state) {

                rb.velocity = Vector2.zero;
                if (spriteAnimator != null && spriteAnimator.isPlaying == false) {
                    state = VacuumMode.Broken;
                    if (IsRooted) rb.bodyType = RigidbodyType2D.Kinematic;
                    GetComponent<SpriteRenderer>().enabled = false;
                    SetColliderEnableState(false);
                }   
            } else if (VacuumMode.Broken == state) {
                if (isATB_Fight) {
                    Destroy(gameObject);
                    return;
                }
                if (respawnImmediately) {
                    Respawn();
                } else if (RespawnsWhenBrokenAndPlayerLeavesRoom && !player.CameraIsChangingRooms() && !player.InSameRoomAs(InitialPosition)) {
                    Respawn();
                }
            }

            preCollisionVelocity = rb.velocity;
            preCollisionPosition = transform.position;
        }

        Vector3 MoveAwayTemp = new Vector3();
        private void MoveAwayFromWall(bool facingUpOrDown) {
            MoveAwayTemp = transform.position;
            if (cc1 != null) {
                HF.MoveAwayFromWall(ref MoveAwayTemp, cc1,null,facingUpOrDown,inPicoScene);
                transform.position = MoveAwayTemp;
            } else {
                HF.MoveAwayFromWall(ref MoveAwayTemp, null, bc1, facingUpOrDown, inPicoScene);
                transform.position = MoveAwayTemp;
            }
        }

        private void ChangeIsTriggerTo(bool on) {
            if (cc1 != null) cc1.isTrigger = on;
            if (bc1 != null) bc1.isTrigger = on;
        }
        private float GetColliderHalfWidth() {
            if (cc1 != null) return cc1.radius*transform.localScale.x;
            if (bc1 != null) return (bc1.size.x * 0.5f + bc1.edgeRadius)*transform.localScale.x;
            return 0;
        }
        private void SetColliderEnableState(bool on) {
            if (cc1 != null) cc1.enabled = on;
            if (bc1 != null) bc1.enabled = on;
        }
        private bool TilemapIsTouchingCollider() {
            if (cc1 != null) return tc1.IsTouching(cc1);
            if (bc1 != null) return tc1.IsTouching(bc1);
            return false;
        }

        private void SetScaleToSuckinVector() {
            SuckinScaleVector.Set(SuckInScaleValue, SuckInScaleValue, SuckInScaleValue);
            transform.localScale = SuckinScaleVector;
            if (cc1 != null) cc1.radius = cc1_originalRadius * SuckInScaleValue;
            if (bc1 != null) bc1.size = bc1_originalSize* SuckInScaleValue;
        }
        Vector3 SuckinScaleVector = new Vector3();
        int framesLeftBeforeSuckInScalingStarts = 5; // Used in MoveTowardsPickupRegion, reset when you stop sucking, or in GetPickedUp()

        public bool IsBeingSucked() {
            return SuckZoneIsOverlapping;
        }
        private void MoveTowardsPickupRegion() {
            IsBeingSuckedAndMoving = true;
            Vector2 diff = player.getCenterOfPickupRegion() - transform.position;
            framesLeftBeforeSuckInScalingStarts--;
            float suckinThreshold = GetColliderHalfWidth() + 0.3f + player.GetComponent<CircleCollider2D>().radius * player.transform.localScale.x;
            if (IsPickupable && diff.magnitude < suckinThreshold&& framesLeftBeforeSuckInScalingStarts <= 0) {
                if (!colliderStartsAsTrigger) ChangeIsTriggerTo(true);
                float lo = 0.25f; //Distance to have minimum scale
                float hi = 1.2f; // Distance to have maximum scale
                if (PickupRegionIsOverlapping) { // vac vel will be 0 so can't use distance to shrink it.
                    SuckInScaleValue *= 0.85f;
                } else {
                    SuckInScaleValue = Mathf.Lerp(0.33f, suckinThreshold, (diff.magnitude - lo) / (hi - lo));
                }
                if (transform.localScale.x - SuckInScaleValue > 0.05f) {
                    SuckInScaleValue = 0.8f * transform.localScale.x;
                }
                SetScaleToSuckinVector();
            }
            diff.Normalize();
            diff *= getSuckedInVel;
            rb.velocity = Vector2.Lerp(rb.velocity, diff, getSuckedInAccelFactor * Time.deltaTime);
            if (PickupRegionIsOverlapping) {
                diff = transform.position;
                diff = Vector2.Lerp(transform.position, player.getCenterOfPickupRegion(), 2 * Time.deltaTime);
                transform.position = diff;
            }
        }

        float originalScale = 1f;
        private void GetPickedUp() {
            if (isATB_Fight && player.PickedUpSomething()) {
                Destroy(gameObject);
                state = VacuumMode.Broken;
                return;
            }
            player.enterPickedupMode();
            framesLeftBeforeSuckInScalingStarts = 5;
            SuckInScaleValue = originalScale;
            SetScaleToSuckinVector();
            if (!colliderStartsAsTrigger) ChangeIsTriggerTo(false);
            GetComponent<SpriteRenderer>().enabled = false;
            if (GetComponent<SlimeWanderer>() != null) {
                GetComponent<SlimeWanderer>().OnSuckedIn();
            }

            if (IsRooted) ChangeIsTriggerTo(false);
            SetColliderEnableState(false);
            state = VacuumMode.PickedUp;
            player.ChangeSuckedItemSprite(GetComponent<SpriteRenderer>());
            // Kind of janky, but the other way to do this would be to wait a few frames on the player side
            // which is arguably worse. plus, the jpConfirm input isn't needed anywhere else in this context
            MyInput.jpSpecial = false;
        }


        bool canBeReleased() {
            if (TilemapIsTouchingCollider()) return false;
            return true;
        }

        public bool isBroken() {
            return state == VacuumMode.Broken;
        }
        public bool isIdle() {
            return state == VacuumMode.Idle;

        }
        public bool isMoving() {
            return state == VacuumMode.Moving;
        }
        public void Stop() {
            rb.velocity = Vector2.zero;
        }

        [HideInInspector]
        public bool PickupRegionIsOverlapping = false;
        [HideInInspector]
        public bool SuckZoneIsOverlapping = false;
        private void OnTriggerEnter2D(Collider2D collision) {
            if (isATB_Fight && name.IndexOf("ATB_Shield") != -1 && collision.name.IndexOf("ATB_Bullet") != -1) {
                if (collision.name.IndexOf("dontbreak") == -1) {
                    Break();
                    return;
                }
            }

            if (collision.name == "PickupRegion") {
                PickupRegionIsOverlapping = true;
            }
            if (collision.name == "SuckZone") {
                SuckZoneIsOverlapping = true;
            }
        }

        public bool isPickedUp() {
            return state == VacuumMode.PickedUp;
        }


        public bool isBreaking() {
            return state == VacuumMode.Breaking;
        }
        [System.NonSerialized]
        public bool ext_UsesDustyPoof = true;
        public bool UsesDefaultBreakSound = true;
        public void Break(bool noAnimation=false) {
            if (SignalDA2OnBreak != "") {
                GameObject.Find(SignalDA2OnBreak).GetComponent<DialogueAno2>().ext_ForceInteractScriptToParse = true;
            }
            JustBrokeResetExternally = true;
            ForceSuckingBecauseBeingSuckedCloseToPlayer = false;
            state = VacuumMode.Breaking;
            TrySpawnHealthDust();
            gameObject.layer = initialLayer;
            if (UsesDefaultBreakSound) {
                AudioHelper.instance.playSFX("blockExplode",true,1f);
            }
            if (spriteAnimator != null && !noAnimation) {
                spriteAnimator.Play("break");
                if (ext_UsesDustyPoof) {
                    if (inPicoScene) {
                        //SeanHF.SpawnDustPoof(transform.position,0.5f);
                    } else {
                        HF.SpawnDustPoof(transform.position);
                    }
                }
            }
            ChangeIsTriggerTo(true);
        }

        [System.NonSerialized]
        public bool JustBrokeResetExternally = false;

        [Tooltip("Set for things that strat as triggers but you want to be thrown as colliders (Bats, etc)")]
        public bool MakeCollideWhenThrown = false;

        public bool MakeTriggerWhenThrown = false;
        [System.NonSerialized]
        public bool DoesntBreakWhenHitByOtherThings_External = false;

        [System.NonSerialized]
        public bool ext_MakeNoncollidWhenThrown;
        private bool atbShotOnce;

        private void OnCollisionEnter2D(Collision2D collision) {
            if (hurtsPlayer && collision.collider.name == Registry.PLAYERNAME2D && isIdle()) {
                player.Damage(1);
            }
            if (hurtsPlayerZeroidHack && collision.collider.name == Registry.PLAYERNAME2D && tHPZH < 0) {
                player.Damage(1);
                Break();
                return;
            }

            if (IsBreakable && !collision.collider.CompareTag("Player") && state == VacuumMode.Moving) {
                Break();
            } else if (IsBreakable && (state == VacuumMode.Idle)) {
                Vacuumable thingThatHitThis = collision.gameObject.GetComponent<Vacuumable>();
                if (thingThatHitThis != null && (thingThatHitThis.state == VacuumMode.Moving || thingThatHitThis.state == VacuumMode.Breaking || thingThatHitThis.state == VacuumMode.Broken)) {
                    if (DoesntBreakWhenHitByOtherThings_External) return;
                    // the check for Broken is because there was a case where two blocks hit each other but the idle one
                    // didn't explode? Theoretically depending on the scheduling of these two events, maybe the moving one
                    // could have updated another frame and entered Broke mode? 
                    Break();
                }
            }
        }


        private void OnTriggerExit2D(Collider2D collision) {

            if (collision.name == "PickupRegion") {
                PickupRegionIsOverlapping = false;
            }
            if (collision.name == "SuckZone") {
                SuckZoneIsOverlapping = false;
            }
        }

        public bool IsMovingOrSucked() {
            return state == VacuumMode.Moving || state == VacuumMode.PickedUp;
        }

        void TrySpawnHealthDust() {
            if (inPicoScene) HF.forceHealthScale = 0.5f;
            if (GetComponent<SlimeWanderer>() != null) {
                SlimeWanderer slime = GetComponent<SlimeWanderer>();
                if (GetComponent<SlimeWanderer>().IsInnocent || slime.element == Element.Snow) {
                    HF.SpawnHealthDust(transform.position, 0.15f, 9);
                    // elc slims killed by sucking and shooting shouldnt drop dust
                } else if (GetComponent<SlimeWanderer>().element == Element.Electric && GetComponent<SlimeWanderer>().wasShotIntoSomthing) {
                    HF.SpawnHealthDust(transform.position,0,100);
                } else {
                    HF.SpawnHealthDust(transform.position);
                }
            }
            if (GetComponent<Mole>() != null) HF.SpawnHealthDust(transform.position,0.6f,4);
            if (GetComponent<SpikyChaser>() != null) {
                if (GetComponent<SpikyChaser>().isSnowman) {
                    HF.SpawnHealthDust(transform.position, 0.25f, 8);
                } else {
                    HF.SpawnHealthDust(transform.position, 0.5f, 4);
                }
            }
            HF.forceHealthScale = 1f;
        }
    }

}