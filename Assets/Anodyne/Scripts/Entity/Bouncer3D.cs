using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bouncer3D : MonoBehaviour {


    public Animator animator;
    public string bounceAnimName = "Armature|Bounce";
    public Vector3 bounceVel = new Vector3(0, 34f, 0);

    private void Start() {
        if (animator == null) {
            if (name == "SpringerCollider") {
                animator = transform.parent.GetComponent<Animator>();
            }
        }
    }

    private void OnTriggerEnter(Collider other) {
        //if (other.name == "MediumPlayer") {
         //   other.GetComponent<MediumControl>().DoBounce3D(bounceVel);
       // }
    }
    private void OnCollisionEnter(Collision collision) {
        if (collision.collider.name == "MediumPlayer") {
            collision.collider.GetComponent<MediumControl>().DoBounce3D(bounceVel);
            if (animator != null) {
                animator.Play(bounceAnimName);
            }
        }
    }
}
