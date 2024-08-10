using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;
using Anodyne;

public class Ano2Dust : MonoBehaviour {

    //public static List<string> dusttable;
    Vacuumable vacuumable;

    public bool IsHealthDust = false;
    public bool destroysWhenPlayerGone = true;
    GameObject contentsSprite;
    AnoControl2D player;
    Vector2Int initRoom;
    public static void initdusttable() {
        //if (dusttable == null) dusttable = new List<string>();
    }
    Transform ItemDustT;


    void Start() {
        HF.GetRoomPos(transform.position, ref initRoom);

        HF.GetPlayer(ref player);
        vacuumable = GetComponent<Vacuumable>();
        vacuumable.DOESNTRESPAWN = true;
        vacuumable.ext_DisablesOnPickupOverlap = true;
        contentsSprite = null;
        if (transform.parent.Find("Content") != null) contentsSprite = transform.parent.Find("Content").gameObject;
        ItemDustT = transform.parent.Find("ItemDustParticle").transform;
	}
    [System.NonSerialized]
    public bool ext_destroysWhenPlayerGone = true;
    
    /// <summary>
    /// Note, only works for a period of time after sucked up.
    /// </summary>
    /// <returns>Whether it got sucked up in the past second since being sucked in</returns>
    public bool JustSuckedUp() {
        return tDestroy > 0;
    }

    public int dustValue = 1;
    Vector3 tempPos = new Vector3();
	void Update () {



        if (destroysWhenPlayerGone && player.InThisRoom(initRoom) == false && ext_destroysWhenPlayerGone) {
            Destroy(transform.parent.gameObject);
            return;
        }

        if (tDestroy > 0) {
            tDestroy -= Time.deltaTime;
            if (tDestroy <= 0) {
                if (transform.parent != null && transform.parent.name.IndexOf("Dust") != -1) {
                    Destroy(transform.parent.gameObject);
                } else {
                    if (transform.parent.name.IndexOf("PicoHealth") != -1) {
                        Destroy(transform.parent.gameObject);
                    } else {
                        print("Destruction bug in " + name);
                    }
                }
            }
            return;
        }


        tempPos = transform.position;
        if (contentsSprite != null) {
            contentsSprite.transform.position = tempPos;
        }
        ItemDustT.position = transform.position;

        if (pickupMovementState == 0) {
            if (vacuumable.IsBeingSuckedAndMoving) {
                // Set this bool, so if obj stops being sucked, it goes to state 1 and moves to the player to be picked up
                forceMoveToPlayer = true;
                if (vacuumable.PickupRegionIsOverlapping) {
                    doPickup = true;
                    pickupMovementState = 2;
                }
            } else {
                if (forceMoveToPlayer) {
                    pickupMovementState = 1;
                }
            }
        } else if (pickupMovementState == 1) {
            tempVec = player.transform.position - transform.position;
            tempVec.z = player.transform.position.z;
            tempVec.Normalize(); tempVec *= 10f;
            transform.position = transform.position + Time.deltaTime * tempVec;
            if (Vector3.Distance(transform.position, player.transform.position) < 0.25f) {
                doPickup = true;
                pickupMovementState = 2;
            }
        } else if(pickupMovementState == 2) {
            // do nothing
        }

        if (doPickup) {
            doPickup = false;
            AudioHelper.instance.playOneShot("vacuumSuckDust");
            if (IsHealthDust) {
                player.GetComponent<HealthBar>().Heal(1);
            }
            if (vacuumable.IsRooted) {
                player.cancelUnrootWait();
            }


            int dustAdded = 0;
            if (Ano2Stats.CountTotalCards() >= 24) {
                dustAdded = (int)Ano2Stats.addDust(dustValue + 2  + randomness % 3);
            } else if (DataLoader._getDS(Registry.FLAG_DESERT_OPEN) == 1) {
                // average of 2.66 dust
                if (randomness % 3 == 1) {
                    dustAdded = (int)Ano2Stats.addDust(dustValue + 1);
                } else {
                    dustAdded = (int)Ano2Stats.addDust(dustValue + 2);
                }
            } else if (DataLoader._getDS(Registry.FLAG_RING_OPEN) == 1) {
                // average of 2.33 dust
                if (randomness % 3 == 1) {
                    dustAdded = (int)Ano2Stats.addDust(dustValue+2);
                } else {
                    dustAdded = (int)Ano2Stats.addDust(dustValue + 1);
                }
            } else {
                dustAdded = (int)Ano2Stats.addDust(dustValue);
            }
            randomness++;

            if (transform.parent != null && transform.parent.name.IndexOf("PicoHealth") != -1) {
                //dustAdded = 0;
            }

            if (dustAdded > 0) {
                DustBar dustbar = GameObject.Find("2D Ano Player").GetComponent<DustBar>();
                dustbar.AddDust(dustAdded);
            }


            ItemDustT.GetComponent<ParticleSystem>().Stop(false, ParticleSystemStopBehavior.StopEmitting);
            tDestroy = 1;
            if (contentsSprite != null) {
                contentsSprite.GetComponent<SpriteRenderer>().enabled = false;
            }
        }
	}
    public static int randomness = 0;

    Vector3 tempVec = new Vector3();
    bool doPickup = false;
    bool forceMoveToPlayer = false;
    float tDestroy = 0;
    int pickupMovementState = 0;
}

[System.Serializable]
public class DustInfo {
    public float unscaledDustValue = 1;
   // string dustID;
    private bool gotScaledValue;
    private float scaledDustValue;
    string parentName;
    [FormerlySerializedAs("usesDustTable")]
    public bool AddsToUnscaledDustTotal = true;
    public void Init(Transform transform, string name) {
        //Ano2Dust.initdusttable();
//dustID = transform.parent.name + name;
        parentName = transform.parent.name;
        if (AddsToUnscaledDustTotal) Ano2Stats.AddToUnscaledDustTotal(unscaledDustValue, transform.parent.name);
    }
    public void GetScaled() {
        if (!gotScaledValue) {
            gotScaledValue = true;
            scaledDustValue = Ano2Stats.GetScaledDustValue(unscaledDustValue, parentName);
        }
    }
   // public bool WasObtained() {
   //     return usesDustTable && Ano2Dust.dusttable.Contains(dustID);
  //  }
    public void Obtain() {
        float dustAdded = 0;
       // if (!usesDustTable) {
            dustAdded = Ano2Stats.addDust(scaledDustValue);
        /*} else if (Ano2Dust.dusttable.Contains(dustID) == false) {
            dustAdded = Ano2Stats.addDust(scaledDustValue);
            Ano2Dust.dusttable.Add(dustID);
        }*/
        Debug.Log("Got " + dustAdded + " dust, current: " + Ano2Stats.dust);
        if (dustAdded > 0) {
            DustBar dustbar = GameObject.Find("2D Ano Player").GetComponent<DustBar>();
            dustbar.AddDust(dustAdded);
        }
    }
}