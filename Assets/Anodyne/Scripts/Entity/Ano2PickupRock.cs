using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
public class Ano2PickupRock : MonoBehaviour {

    // In theory, multiple sprites and behaviors could go in here.
    // This allows a anocontrol2d to pick up, it's written in a way that
    // ANoControl2D doesn't Care what it's picking up


    Rigidbody2D rb;
    public float deceleration = 15f;
    enum MODE { Idle, CanBePickedUp, PickedUp, Moving }
    MODE state = 0;
    AnoControl2D player;
    Vector3 newpos;
    public float shootVelocity = 10f;
    public float getSuckedInVel = 5f;
    public float getSuckedInAccelFactor = 1f;
    bool IsNotBeingSucked = false;
    TilemapCollider2D tc1;
    CircleCollider2D cc1;
    void Start() {
        rb = GetComponent<Rigidbody2D>();
        tc1 = GameObject.Find("Tilemap").GetComponent<TilemapCollider2D>();
        cc1 = GetComponent<CircleCollider2D>();
        player = GameObject.Find("2D Ano Player").GetComponent<AnoControl2D>();
    }

    [HideInInspector]
    public Vector2 preCollisionVelocity;
    void Update() {


        Vector2 newvel = rb.velocity;
        if (IsNotBeingSucked) {
            if (newvel.x > 0) newvel.x -= Time.deltaTime * deceleration;
            if (newvel.x < 0) newvel.x += Time.deltaTime * deceleration;
            if (newvel.y > 0) newvel.y -= Time.deltaTime * deceleration;
            if (newvel.y < 0) newvel.y += Time.deltaTime * deceleration;
            if (newvel.magnitude < 1f) {
                newvel = Vector2.zero;
            }
        }
        rb.velocity = newvel;

        if (MODE.Idle == state) {
            IsNotBeingSucked = true;
            if (PickupRegionIsOverlapping && MyInput.special) {
                if (!player.isAbleToPickUp()) return;
                player.enterPickedupMode();
                GetComponent<SpriteRenderer>().enabled = false;
                cc1.enabled = false;
                state = MODE.PickedUp;
                player.ChangeSuckedItemSprite(GetComponent<SpriteRenderer>());
                // Kind of janky, but the other way to do this would be to wait a few frames on the player side
                // which is arguably worse. plus, the jpConfirm input isn't needed anywhere else in this context
                MyInput.jpSpecial = false;
            } else if (SuckZoneIsOverlapping && MyInput.special) {
                Vector2 diff = player.getCenterOfPickupRegion() - transform.position; diff.Normalize();
                diff *= getSuckedInVel;
                rb.velocity = Vector2.Lerp(rb.velocity, diff, getSuckedInAccelFactor * Time.deltaTime);
                IsNotBeingSucked = false;
            }
        } else if (MODE.PickedUp == state) {
            newpos = player.getCenterOfPickupRegion();
            transform.position = newpos;
            if (player.hasJustShot()) {
                if (canBeReleased()) {
                    player.confirmRelease();
                    GetComponent<SpriteRenderer>().enabled = true;
                    player.ChangeSuckedItemSprite(null);
                    cc1.enabled = true;
                    rb.velocity = shootVelocity * player.getFacingDirVector();
                    state = MODE.Moving;
                } else {
                    player.cancelRelease();
                }
            }

        } else if (MODE.Moving == state) {
            if (rb.velocity.magnitude < 1f) {
                state = MODE.Idle;
            }
        }

        preCollisionVelocity = rb.velocity;
    }

    bool canBeReleased() {
        if (tc1.IsTouching(cc1)) return false;
        return true;
    }

    public bool isMoving() {
        return state == MODE.Moving;
    }
    public void Stop() {
        rb.velocity = Vector2.zero;
    }

    bool PickupRegionIsOverlapping = false;
    bool SuckZoneIsOverlapping = false;
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.name == "PickupRegion") {
            PickupRegionIsOverlapping = true;
        }
        if (collision.name == "SuckZone") {
            SuckZoneIsOverlapping = true;
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

}
