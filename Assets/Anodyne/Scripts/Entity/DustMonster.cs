using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DustMonster : MonoBehaviour {

    AnoControl2D player;
    Rigidbody2D rb;
    public float vel = 1f;
    Vector2 velvec = new Vector2();
    // Use this for initialization
    Object DustFloaterPrefab;
    public float nrdusts = 6;
    public float DustValPerDust = 0.25f;
    
	void Start () {
        Ano2Stats.AddToUnscaledDustTotal(DustValPerDust* nrdusts, transform.parent.name);
        player = GameObject.Find("2D Ano Player").GetComponent<AnoControl2D>();
        rb = GetComponent<Rigidbody2D>();
        DustFloaterPrefab = Resources.Load("Prefabs/2D/DustEntities/DustFloater");
	}
	
	// Update is called once per frame
	void Update () {
        if (player.InTheSameRoomAs(transform.position.x, transform.position.y) == false) {
            rb.velocity = Vector2.zero;
            return;
        }
        velvec = player.transform.position - transform.position;
        velvec.Normalize();
        rb.velocity = velvec * vel;
		
	}

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.collider.GetComponent<Anodyne.Vacuumable>() != null) {
            if (collision.collider.GetComponent<Rigidbody2D>().velocity.magnitude > 2f) {
                gameObject.SetActive(false);
                bool gottenalready = false;
                /*
                if (Ano2Dust.dusttable.Contains(transform.parent.name + name)) {
                    gottenalready = true;
                } else { 
                    Ano2Dust.dusttable.Add(transform.parent.name + name);
                }*/
                for (int i = 0; i < nrdusts; i++) {
                    float dis = 1.5f;
                    float x = -dis + Random.value * dis*2;
                    float y = Mathf.Sqrt(dis*dis - x*x);
                    if (Random.value < 0.5f) y *= -1;
                    Vector3 offset = new Vector3(x, y, 0);
                    if (!gottenalready) {
                        Instantiate(DustFloaterPrefab, transform.position + offset, Quaternion.identity, transform.parent);
                    }
                }
            }
        }
    }
}
