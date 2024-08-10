using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DoYSort2D : MonoBehaviour {


    SpriteRenderer sr;
    Vector2 playerpos;
    Transform playertransform;
    bool hasBoxCollider2D = false;
    bool hasCircleCollider2D = false;
    BoxCollider2D bc;
    CircleCollider2D cc;
    ParticleSystemRenderer ps;
    public bool useParticleSystemSort = false;
    public bool editsSortingGroup = false;
    UnityEngine.Rendering.SortingGroup sortingGroup;
    public SpriteRenderer useThisSR;
    private void Start() {

        if (useThisSR != null) {
            sr = useThisSR;
        } else {
            sr = GetComponent<SpriteRenderer>();
        }

        if (editsSortingGroup) {
            sortingGroup = GetComponent<UnityEngine.Rendering.SortingGroup>();
        }

        playertransform = GameObject.Find("2D Ano Player").transform;
        if (GetComponent<ParticleSystemRenderer>() != null) {
            ps = GetComponent<ParticleSystemRenderer>();
        } 
        if (GetComponent<BoxCollider2D>() != null) {
            hasBoxCollider2D = true;
            bc = GetComponent<BoxCollider2D>();
        } else if (GetComponent<CircleCollider2D>() != null) {
            hasCircleCollider2D = true;
            cc = GetComponent<CircleCollider2D>();
        }
    }

    // Update is called once per frame
    void Update() {
        if (useParticleSystemSort) {

            float topY = transform.position.y;
            if (topY < playertransform.position.y) {
                ps.sortingLayerName = "AbovePlayer1";
            } else {
                ps.sortingLayerName = "BelowPlayer1";
            }
        } else if (editsSortingGroup) {
            float topY = transform.position.y;
            if (topY < playertransform.position.y) {
                sortingGroup.sortingLayerName = "AbovePlayer1";
            } else {
                sortingGroup.sortingLayerName  = "BelowPlayer1";
            }
        } else if (hasBoxCollider2D) {
            float topy = transform.position.y + bc.offset.y * transform.localScale.y + (bc.size.y * transform.localScale.y / 2);
            if (topy + 0.1f < playertransform.position.y) {
                sr.sortingLayerName = "AbovePlayer1";
            } else {
                sr.sortingLayerName = "BelowPlayer1";
            }
        } else if (hasCircleCollider2D) {
            float topY = transform.position.y + cc.offset.y * transform.localScale.y + cc.radius * transform.localScale.y;
            if (topY < playertransform.position.y) {
                sr.sortingLayerName = "AbovePlayer1";
            } else {
                sr.sortingLayerName = "BelowPlayer1";
            }
        } else {
            float topY = transform.position.y;
            if (topY < playertransform.position.y) {
                sr.sortingLayerName = "AbovePlayer1";
            } else {
                sr.sortingLayerName = "BelowPlayer1";
            }

        }
    }
}
