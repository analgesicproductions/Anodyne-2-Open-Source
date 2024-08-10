using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Anodyne;

[System.Serializable]

//[RequireComponent(typeof(Rigidbody2D))]
//[RequireComponent(typeof(Vacuumable))]
[RequireComponent(typeof(SpriteAnimator))]
[RequireComponent(typeof(SpriteRenderer))]
public class EntityState2D : MonoBehaviour {

    [HideInInspector]
    public bool initializedEntity = false;
    [HideInInspector]
    public Vector3 initPos;
    [HideInInspector]
    public Vector3 tempPos;

    [HideInInspector]
    public Vector2 tempVel = new Vector2();

    [HideInInspector]
    public Vector2Int curRoomPos;
    [HideInInspector]
    public Vector2Int initRoomPos;
    public List<GameObject> children;
    [HideInInspector]
    public float initSpawnTimer = 0f;
    public float spawnTimer = 0f;

    [HideInInspector]
    public float t_touchDmgCooldown = 0f;
    [HideInInspector]
    public float tm_touchDmgCooldown = 0.8f;
    public int touchDmg = 1;

    [HideInInspector]
    public bool onPlayer = false;
    [HideInInspector]
    public bool usesDefaultPlayerDetection = false;


    public void Start() {
        initPos = transform.position;
        tempPos = new Vector3();
        initSpawnTimer = spawnTimer;
        HF.GetRoomPos(transform.position, ref initRoomPos);
    }

    // Return true if need to do player bouncing
    public bool UpdateTouchDamage(AnoControl2D player) {
        if (t_touchDmgCooldown > 0) t_touchDmgCooldown -= Time.deltaTime;
        if (onPlayer) {
            if (t_touchDmgCooldown <= 0) {
                player.Damage(touchDmg);
                t_touchDmgCooldown = tm_touchDmgCooldown;
                return true;
            }
        }
        return false;
    }

    private Vector3 localTempPos = new Vector3();
    public void ConstrainPositionToCurrentRoom() {

        localTempPos = transform.position;
        HF.ConstrainVecToRoom(ref localTempPos, initRoomPos.x, initRoomPos.y);
        transform.position = localTempPos;
    }

    public bool NeedsToResetIfPlayerOutsideInitRoom(AnoControl2D player) {
        return (!player.CameraIsChangingRooms() && !player.InThisRoom(initRoomPos));
    }

    public bool NeedsToResetIfPlayerOutsideRoom(AnoControl2D player, Vacuumable vac) {
        return (!player.CameraIsChangingRooms() && !player.InSameRoomAs(transform.position) && !vac.IsMovingOrSucked());
    }

    public bool NeedsToResetIfPlayerOutsideRoom(AnoControl2D player) {
        return (!player.CameraIsChangingRooms() && !player.InSameRoomAs(transform.position));
    }


    /**
     * sfsdf
     * */
    public bool TrySwallow(AnoControl2D player, Vacuumable vac) {
        if (vac.state == Vacuumable.VacuumMode.PickedUp) {
            if (player.CameraIsChangingRooms() || !player.InThisRoom(initRoomPos)) {
                player.Swallow();
                vac.state = Vacuumable.VacuumMode.Broken;
                return true;
            }
        }
        return false;
    }

    public bool UpdateRoomIfVacIsNonIdle(Anodyne.Vacuumable vac, Transform t) {
        if (vac.isPickedUp() || vac.isMoving() || vac.IsBeingSuckedAndMoving) {
            HF.GetRoomPos(t.position, ref curRoomPos);
            return true;
        }
        return false;
    }

    public void SetVelocityTowardsDestination(Rigidbody2D rb, Transform destinationTransform, float magnitude) {
        localTempPos = destinationTransform.position - transform.position;
        localTempPos.Normalize();
        localTempPos *= magnitude;
        rb.velocity = localTempPos;
    }

    private void OnTriggerEnter2D(Collider2D collision) {

        if (!usesDefaultPlayerDetection) return;
        if (collision.CompareTag("Player")) {
            onPlayer = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision) {
        if (!usesDefaultPlayerDetection) return;
        if (collision.CompareTag("Player")) {
            onPlayer = false;
        }
    }

}
