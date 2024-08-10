using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Anodyne;

public class HF {

    public static UIManagerAno2 Get3DUI() {
        return GameObject.Find("3D UI").GetComponent<UIManagerAno2>();
    }
    public static UIManager2D Get2DUI() {
        return GameObject.Find("2D UI").GetComponent<UIManager2D>();
    }

    public static Registry.GameScenes SceneNameToEnum(string s) {
        if (s == "NanoAlbumen") return Registry.GameScenes.NanoAlbumen;
        if (s == "Cougher") return Registry.GameScenes.Cougher;
        if (s == "NanoTongue") return Registry.GameScenes.NanoTongue;
        if (s == "NanoRage") return Registry.GameScenes.NanoRage;
        if (s == "Pig") return Registry.GameScenes.Pig;
        if (s == "NanoSanctuary") return Registry.GameScenes.NanoSanctuary;

        if (s == "NanoGolem") return Registry.GameScenes.NanoGolem;
        if (s == "NanoStalker") return Registry.GameScenes.NanoStalker;
        if (s == "NanoClone") return Registry.GameScenes.NanoClone;

        if (s == "NanoHandfruitHaven") return Registry.GameScenes.NanoHandfruitHaven;
        if (s == "NanoDustbound") return Registry.GameScenes.NanoDustbound;
        if (s == "NanoDB_Wrestling") return Registry.GameScenes.NanoDB_Wrestling;
        if (s == "NanoDB_Clean") return Registry.GameScenes.NanoDB_Clean;

        if (s == "NanoHorror") return Registry.GameScenes.NanoHorror;
        if (s == "NanoOrb") return Registry.GameScenes.NanoOrb;
        if (s == "NanoNexus") return Registry.GameScenes.NanoNexus;
        if (s == "NanoOcean") return Registry.GameScenes.NanoOcean;
        if (s == "PicoOcean") return Registry.GameScenes.PicoOcean;
        if (s == "NanoFantasy") return Registry.GameScenes.NanoFantasy;
        if (s == "PicoFantasy") return Registry.GameScenes.PicoFantasy;
        if (s == "NanoSkeligum") return Registry.GameScenes.NanoSkeligum;
        if (s == "NanoZera") return Registry.GameScenes.NanoZera;
        if (s == "PicoZera") return Registry.GameScenes.PicoZera;
        if (s == "PG_Drawer") return Registry.GameScenes.PG_Drawer;
        if (s == "PG_Ano1") return Registry.GameScenes.PG_Ano1;
        return Registry.GameScenes.Pig;
    }

    public static string CurSceneName() {
        return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
    }
    // also: episodeDescription, episodeTitle
    public static string GetSceneAssociatedText(Registry.GameScenes sceneID, string dialogueScene="areanames", int otherInfo=0) {
        if (sceneID == Registry.GameScenes.NanoAlbumen) {
            if (otherInfo == 1) return DataLoader.instance.getRaw(dialogueScene, 2);
            if (otherInfo == 2) return DataLoader.instance.getRaw(dialogueScene, 3);
            if (otherInfo == 3) return DataLoader.instance.getRaw(dialogueScene, 4);
        }
        if (sceneID == Registry.GameScenes.Cougher) return DataLoader.instance.getRaw(dialogueScene, 6);
        if (sceneID == Registry.GameScenes.NanoTongue) return DataLoader.instance.getRaw(dialogueScene, 7);
        if (sceneID == Registry.GameScenes.NanoRage) return DataLoader.instance.getRaw(dialogueScene, 8);
        if (sceneID == Registry.GameScenes.Pig) return DataLoader.instance.getRaw(dialogueScene, 9);

        if (sceneID == Registry.GameScenes.NanoSanctuary) return DataLoader.instance.getRaw(dialogueScene, 10);
        if (sceneID == Registry.GameScenes.NanoGolem) return DataLoader.instance.getRaw(dialogueScene, 11);
        if (sceneID == Registry.GameScenes.NanoDustbound) return DataLoader.instance.getRaw(dialogueScene, 12);
        if (sceneID == Registry.GameScenes.NanoHandfruitHaven) return DataLoader.instance.getRaw(dialogueScene, 13);
        if (sceneID == Registry.GameScenes.NanoDB_Wrestling) return DataLoader.instance.getRaw(dialogueScene, 14);

        if (sceneID == Registry.GameScenes.NanoStalker) return DataLoader.instance.getRaw(dialogueScene, 15);
        if (sceneID == Registry.GameScenes.NanoClone) return DataLoader.instance.getRaw(dialogueScene, 16);

        // Ocean and fantasy
        if (sceneID == Registry.GameScenes.NanoOrb) return DataLoader.instance.getRaw(dialogueScene, 17);
        if (sceneID == Registry.GameScenes.NanoNexus && otherInfo == 1) return DataLoader.instance.getRaw(dialogueScene, 18);
        if (sceneID == Registry.GameScenes.NanoFantasy) return DataLoader.instance.getRaw(dialogueScene, 18);
        if (sceneID == Registry.GameScenes.PicoFantasy) return DataLoader.instance.getRaw(dialogueScene, 18);
        if (sceneID == Registry.GameScenes.NanoNexus && otherInfo == 0) return DataLoader.instance.getRaw(dialogueScene, 19);
        if (sceneID == Registry.GameScenes.NanoOcean) return DataLoader.instance.getRaw(dialogueScene, 19);
        if (sceneID == Registry.GameScenes.PicoOcean) return DataLoader.instance.getRaw(dialogueScene, 19);
        if (sceneID == Registry.GameScenes.NanoSkeligum) return DataLoader.instance.getRaw(dialogueScene, 20);
        if (sceneID == Registry.GameScenes.NanoHorror) return DataLoader.instance.getRaw(dialogueScene, 21);
        if (sceneID == Registry.GameScenes.NanoNexus && otherInfo == 2) return DataLoader.instance.getRaw(dialogueScene, 22);
        if (sceneID == Registry.GameScenes.NanoDB_Clean) return DataLoader.instance.getRaw(dialogueScene, 23);
        if (sceneID == Registry.GameScenes.PicoZera) return DataLoader.instance.getRaw(dialogueScene, 24);
        if (sceneID == Registry.GameScenes.NanoZera) return DataLoader.instance.getRaw(dialogueScene, 24);

        if (sceneID == Registry.GameScenes.PG_Ano1) return DataLoader.instance.getRaw(dialogueScene, 33);

        return "No such scene";
    }

    public static void GetDialogueBox(ref DialogueBox dbox) {
        dbox = GameObject.Find("Dialogue").GetComponent<DialogueBox>();
    }

    public static void GetPlayer(ref AnoControl2D player) {
       player = GameObject.Find(Registry.PLAYERNAME2D).GetComponent<AnoControl2D>();
    }

    public static void GetPlayer(ref MediumControl player) {
        GameObject g = GameObject.Find(Registry.PLAYERNAME3D_Walkscale);
        if (g == null) return;
        player = g.GetComponent<MediumControl>();
    }

    public static void RotateVector2(ref Vector2 v, float degrees) {
        float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
        float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

        float tx = v.x;
        float ty = v.y;
        v.x = (cos * tx) - (sin * ty);
        v.y = (sin * tx) + (cos * ty);
    }

    public static void MoveAwayFromWall(ref Vector3 pos, CircleCollider2D cc=null,BoxCollider2D bc=null, bool facingUpOrDown=false,bool isPico=false) {
        float dis = 0;
      //  Debug.Log("Facing up or down: " + facingUpOrDown);
        Vector2 castpos = pos;
        if (cc != null) {
            if (isPico) {
                dis = cc.radius * cc.transform.localScale.x + 0.05f;
            } else {
                dis = cc.radius * cc.transform.localScale.x + 0.1f;
            }
            castpos.x += cc.offset.x * cc.transform.localScale.x;
            castpos.y += cc.offset.y * cc.transform.localScale.y;
        }
        if (bc != null) {
            dis = bc.size.x * 0.5f * bc.transform.localScale.x + bc.edgeRadius + 0.07f;
            castpos.x += bc.offset.x * bc.transform.localScale.x;
            castpos.y += bc.offset.y * bc.transform.localScale.y;
        }
        int layermask = ~(1 << LayerMask.NameToLayer("Player") | 1 << LayerMask.NameToLayer("Holetilemap"));
        // Cast a ray in four directions -if there's a hit, nudge the collider based on the distance
        Vector2[] directions = new Vector2[] { Vector2.left, Vector2.right, Vector2.up, Vector2.down };

        RaycastHit2D hit = new RaycastHit2D();
        Vector2 castposWithOffset = new Vector2();

        // Create a list of offsets based on player facing dir, to check all 8x8 tiles 3 ahead and 3 behind
        // Makes shooting better when near some stuff
        Vector2[] offsets = new Vector2[4];
        for (int i = 0; i < 4; i++) {
            if (facingUpOrDown) {
                //Vector2 offset = new Vector2(0, -1.5f + 0.5f * i);
                Vector2 offset = new Vector2(0, 0.5f * i);
                offsets[i] = offset;
            } else {
                //Vector2 offset = new Vector2(-1.5f+0.5f*i,0);
                Vector2 offset = new Vector2(0.5f*i,0);
                offsets[i] = offset;
            }
        }
        int idx = 0;
        foreach (Vector2 offset in offsets) {
            castposWithOffset = castpos + offset;
            for (int i = 0; i < 4; i++) {
                hit = Physics2D.Raycast(castposWithOffset, directions[i], dis, layermask);
                if (hit.collider != null) {
                    if (hit.collider.GetComponent<UnityEngine.Tilemaps.TilemapCollider2D>() == null) return;
                    float diff = 0;
                    if (i == 0) {
                        diff = castposWithOffset.x - hit.point.x;
                        pos.x += dis - diff;
                    } else if (i == 1) {
                        diff = hit.point.x - castposWithOffset.x;
                        pos.x -= dis - diff;
                    } else if (i == 2) {
                        diff = hit.point.y - castposWithOffset.y;
                        pos.y -= dis - diff;
                    } else {
                        diff = castposWithOffset.y - hit.point.y; // pos, move up
                        pos.y += dis - diff;
                    }
                    //Debug.Log(idx + " " + i);
                    return;
                }
            }
            idx++;
        }
    }

    public static bool isMovingOutOfRoomX(Vector2 velocity, float x, Vector2Int room) {
        if (velocity.x < 0 && x <= room.x *SceneData2D.RoomSize_X) {
            return true;
        } else if (velocity.x >= 0 && x >= (1+room.x)*SceneData2D.RoomSize_X) {
            return true;
        }
        return false;
    }

    public static bool isMovingOutOfRoomY(Vector2 velocity, float y, Vector2Int room) {
        if (velocity.y < 0 && y <= room.y * SceneData2D.RoomSize_Y) {
            return true;
        } else if (velocity.y >= 0 && y >= (1+room.y) * SceneData2D.RoomSize_Y) {
            return true;
        }
        return false;
    }

    public static int GetRoomX(float x) {
        return Mathf.FloorToInt(x / SceneData2D.RoomSize_X);
    }
    public static int GetRoomY(float y) {
        return Mathf.FloorToInt(y / SceneData2D.RoomSize_Y);
    }

    public static void GetContactWallsCC(ref bool doBounceVertSurface, ref bool doBounceHorSurface, CircleCollider2D cc, Collision2D collision,bool debug=false) {
        ContactPoint2D[] pts = new ContactPoint2D[2];
        collision.GetContacts(pts);
        foreach (ContactPoint2D pt in pts) {
            if (cc.gameObject.name == "moletest") {

            }
            if (debug) Debug.Log(pt.point.x + "," + pt.point.y);
            if (pt.point.x == 0 && pt.point.y == 0) continue;
            // 707 from the 45-45-90 triangle's side with hypotenuse of the circle radii
            if (pt.point.x < cc.transform.position.x - cc.radius *.707 || pt.point.x > cc.transform.position.x + cc.radius*.707) {
                doBounceVertSurface = true;
            } else {
                doBounceHorSurface = true;
            }
        }
    }

    // Useful for timers that increase from 0 to timerMax and need to reset to 0, or 
    // trigger some kind of state change
    public static bool TimerDefault(ref float timer, float timerMax, float increment=0) {
        if (increment == 0) increment = Time.deltaTime;
        timer += increment;
        if (timer > timerMax) {
            timer -= timerMax;
            return true;
        }
        return false;
    }
    
    public static bool TimerStayAtMax(ref float timer, float timerMax, float increment = 0) {
        if (timer >= timerMax) {
            return true;
        }
        if (increment == 0) increment = Time.deltaTime;
        timer += increment;
        if (timer >= timerMax) {
            timer = timerMax;
            return true;
        }
        return false;
    }

    public static void ConstrainVecToRoom(ref Vector3 pos, int room_x, int room_y) {
        pos.x = constrainValtoRoom(pos.x, room_x);
        pos.y = constrainValtoRoom(pos.y, room_y);
    }

    public static float constrainValtoRoom(float x, int room_x) {
        if (x < room_x * SceneData2D.RoomSize_X) return room_x*SceneData2D.RoomSize_X;
        if (x > (room_x + 1) * SceneData2D.RoomSize_X) return (room_x +1 )* SceneData2D.RoomSize_X;
        return x;
    }

    public static void jitterVector2(ref Vector2 v, float magnitude) {
        v.x -= magnitude; v.y -= magnitude;
        v.x += 2 * magnitude * Random.value;
        v.y += 2 * magnitude * Random.value;
    }


    public static string getDirAnimBasedOnOneDirVel(Vector2 vel, string prefix) {

        if (vel.x > 0 && vel.y == 0) {
            return prefix + "_r";
        } else if (vel.x < 0 && vel.y == 0) {
            return prefix + "_l";
        } else if (vel.x == 0 && vel.y < 0) {
            return prefix + "_d";
        } else if (vel.x == 0 && vel.y > 0) {
            return prefix + "_u";
        }
        return prefix + "_u";

    }

    public static void randomizeVec2(ref Vector2 v, float magnitude) {
        v.x = -magnitude + 2 * magnitude * Random.value;
        v.y = Mathf.Sqrt(magnitude * magnitude - v.x * v.x);
        if (Random.value < 0.5f) v.y *= -1;
    }

    public static void randomizeVec2ToOneDir(ref Vector2 v, float magnitude) {
        v.x = 0; v.y = 0;
        if (Random.value > 0.5f) {
            if (Random.value > 0.5f) {
                v.x = magnitude;
            } else {
                v.x = -magnitude;
            }
        } else {
            if (Random.value > 0.5f) {
                v.y = magnitude;
            } else {
                v.y = -magnitude;
            }
        }
    }

    public static void GetRoomPos(Vector2 pos, ref Vector2Int roompos) {
        roompos.x = GetRoomX(pos.x);
        roompos.y = GetRoomY(pos.y);
    }

    public static bool IsInRoom(Vector2 pos, int roomX, int roomY) {
        if (roomX != GetRoomX(pos.x)) return false;
        if (roomY != GetRoomY(pos.y)) return false;
        return true;
    }
    public static bool AreTheseInTheSameroom(Transform t1, Transform t2) {
        return GetRoomX(t1.position.x) == GetRoomX(t2.position.x) && GetRoomY(t1.position.y) == GetRoomY(t2.position.y);
    }
    public static bool AreTheseInTheSameroom(Vector2 t1, Vector2 t2) {
        return GetRoomX(t1.x) == GetRoomX(t2.x) && GetRoomY(t1.y) == GetRoomY(t2.y);
    }

    // The default empty signal is the 'reduction' signal for opening things
    public static void SendSignal(List<GameObject> children, string signal = "") {
        if (children == null || children.Count == 0) return;
        foreach (GameObject g in children) {
            if (g == null) {
                continue;
            }
            if (g.GetComponent<Gate>() != null) {
                g.GetComponent<Gate>().SendSignal(signal);
            }
            if (g.GetComponent<NumDisplay>() != null) {
                g.GetComponent<NumDisplay>().SendSignal(signal);
            }
            if (g.GetComponent<Enabler>() != null) {
                g.GetComponent<Enabler>().SendSignal(signal);
            }
        }
    }

    public static void ReduceVec2ToVec(ref Vector2 v, Vector2 target, float r) {
        v.x = ReduceToVal(v.x, target.x, r);
        v.y = ReduceToVal(v.y, target.y, r);
    }
    public static void ReduceVec2To0(ref Vector2 v,float r) {
        v.x = ReduceTo0(v.x, r);
        v.y = ReduceTo0(v.y, r);
    }
    public static float ReduceTo0(float val, float reduction) {
        return ReduceToVal(val, 0, reduction);
    }
    public static float ReduceToVal(float val, float targetval, float reduction) {
        if (val > targetval) {
            val -= reduction;
            if (val <= targetval) val = targetval;
        } else if (val < targetval) {
            val += reduction;
            if (val >= targetval) val = targetval;
        }
        return val;
    }

	public static bool PointInOABB (Vector3 point, BoxCollider box )
	{
		point = box.transform.InverseTransformPoint( point ) - box.center;

		float halfX = (box.size.x * 0.5f);
		float halfY = (box.size.y * 0.5f);
		float halfZ = (box.size.z * 0.5f);
		if( point.x < halfX && point.x > -halfX && 
			point.y < halfY && point.y > -halfY && 
			point.z < halfZ && point.z > -halfZ )
			return true;
		else
			return false;
	}

    static Vector3 HealthProb = new Vector3();


    public static GameObject SparkyPoof3DPrefab;
    public static void SpawnSparkyPoof3D(Vector3 pos) {

        if (SparkyPoof3DPrefab == null) {
            SparkyPoof3DPrefab = Resources.Load("Prefabs/SparkyPoof3D") as GameObject;
        }
        ParticleSystem ps = Object.Instantiate(SparkyPoof3DPrefab).GetComponent<ParticleSystem>();
        ps.transform.position = pos;
        ps.Play();
        Object.Destroy(ps.gameObject, 2f);
    }

    public static GameObject DustyHit3DPrefab;
    public static void SpawnDustyHit3D(Vector3 pos) {

        if (DustyHit3DPrefab == null) {
            DustyHit3DPrefab = Resources.Load("Prefabs/DustyHit3D_R") as GameObject;
        }
        ParticleSystem ps = Object.Instantiate(DustyHit3DPrefab).GetComponent<ParticleSystem>();
        ps.transform.position = pos;
        ps.Play();
        Object.Destroy(ps.gameObject, 2f);
    }

    public static GameObject DustyPoofPrefab;
    public static void SpawnDustPoof(Vector3 pos,float scale=1f) {

        if (DustyPoofPrefab == null) {
            DustyPoofPrefab = Resources.Load("Prefabs/DustyPoofParticle") as GameObject;
        }
        ParticleSystem ps = Object.Instantiate(DustyPoofPrefab).GetComponent<ParticleSystem>();
        Vector3 locScale = ps.transform.localScale;
        locScale *= scale;
        ps.transform.localScale = locScale;
        ps.transform.position = pos;
        ps.Play();
        Object.Destroy(ps.gameObject, 0.8f);
    }

    public static float forceHealthScale = 1f;
    public static void SpawnHealthDust(Vector3 pos,float chance=0.75f,float guaranteedEvery=3f) {
        float r = Random.value;
        //Vector3 locScale = new Vector3();
        // xyz = Success, Tries, ConsecutiveFails
        // x and y not used now... but who knows lol
        string prefabName = "Prefabs/HealthDust";
        if (forceHealthScale == 0.5f) {
            prefabName = "Prefabs/PicoHealth";
        }
        //GameObject g = null;
        if (r <= chance) {
            //g = 
                Object.Instantiate(Resources.Load(prefabName), pos, Quaternion.identity);
            HealthProb.x++;
            HealthProb.y++;
            HealthProb.z = 0;
        } else {
            HealthProb.y++;
            HealthProb.z++;
            if (HealthProb.z >= guaranteedEvery) {
                HealthProb.z = 0;
                HealthProb.x++;
                //g = 
                    Object.Instantiate(Resources.Load(prefabName), pos, Quaternion.identity);
            }
        }
        /*if (g != null) {
            locScale = g.transform.localScale;
            locScale *= forceHealthScale;
            g.transform.localScale = locScale;
            g = g.transform.Find("ItemDustParticle").gameObject;
            locScale = g.transform.localScale;
            locScale *= forceHealthScale;
            g.transform.localScale = locScale;
        }*/
        //Debug.Log(HealthProb);
    }

    public static bool PlayerHasZeroVelocity() {
        if (GameObject.Find("MediumPlayer") != null) {
            Rigidbody rb = GameObject.Find("MediumPlayer").GetComponent<Rigidbody>();
            if (rb.velocity.magnitude < 0.1f) return true;
        } else if (GameObject.Find("BigPlayer") != null) {
            Rigidbody rb = GameObject.Find("BigPlayer").GetComponent<Rigidbody>();
            if (rb.velocity.magnitude < 0.1f) return true;
        } else if (GameObject.Find("2D Ano Player") != null) {
            Rigidbody2D rb2 = GameObject.Find("2D Ano Player").GetComponent<Rigidbody2D>();
            if (rb2.velocity.x == 0 && rb2.velocity.y == 0) return true;

        }
        return false;
    }
}
