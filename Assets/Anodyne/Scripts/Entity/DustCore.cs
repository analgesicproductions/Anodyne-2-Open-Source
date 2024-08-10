using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Anodyne;
public class DustCore : MonoBehaviour {

    public int YolkID = 0;
    DialogueBox dbox;
    int mode = 0;
    [Tooltip("0-Tongue 1-Pig 2-Cougher 3-Rage 4-Geof 5-GeofChest 6-Clone 7-RingCloneChest 8-Stalker 9-StalkerChest 10-Carwash 11-RingHighway | 12 nanoorb crystal")]
    /*
     * 12. NanoOrb Crystal
     * DesertOrb Chest
     * 14 NanoHorror Crystal
     * DesertShore Chest
     * Ocean Sakura Man
     * Ocean Southern Women
     * ? Ocean Ghost Bridge
     * DesertField Chest
     * 20 Fantasy Crystal
     * Fantasy 2
     * Fantasy 3
     * Skeligum Crystal
     * */
    public int cardID = 0;

    public Registry.GameScenes SceneToExitTo;
    public string NextSceneDestinationName;
    public string SetFlagToOneOnDestruction;
    public GameObject child;
    public GameObject card;
    SpriteAnimator glandilockAnim;
    PositionShaker glandilockPS;
    ParticleSystem pulse;
    bool actuallyGetSuckedIn = false;
	void Start () {
        if (Ano2Stats.HasCard(cardID)) {
            if (transform.parent.name.IndexOf("DustCore") != -1) {
                Destroy(transform.parent.gameObject);
                return;
            }
        }

        pulse = GameObject.Find("GlandilockPulse").GetComponent<ParticleSystem>();
        dbox = GameObject.Find("Dialogue").GetComponent<DialogueBox>();
        glandilockAnim = GameObject.Find("GlandilockSeed").GetComponent<SpriteAnimator>();
        glandilockPS= GameObject.Find("GlandilockSeed").GetComponent<PositionShaker>();
        HF.GetPlayer(ref player);
        card.GetComponent<SpriteRenderer>().enabled = false;
        ps = child.GetComponent<PositionShaker>();
        ps.amplitude.Set(0, 0, 0);
        sr = child.GetComponent<SpriteRenderer>();
        ps.enabled = true;
        emissionModule = poofParticles.emission;
        emitRate = new ParticleSystem.MinMaxCurve(20f);

        if (cardID == 6) actuallyGetSuckedIn = true;

        string sn = HF.CurSceneName();
        if (sn == "NanoRage") {

        } else {
            GetComponent<CircleCollider2D>().offset *= -1;
        }
        if (sn == "NanoTongue") {
            transform.parent.Find("Mask").gameObject.SetActive(false);

        }
    }

    void UpdateHorrorStuff() {
        if (mode == 0) {
            if (HF.TimerDefault(ref t_horror, 4f)) {

                player.transform.position = GameObject.Find("BedPos").transform.position;
                player.ForceUpdateRoomPos();
                player.SnapCameraToPlayerAndKeepInRoom();
                player.SwitchToHorror();
                player.GetComponent<Animator>().enabled = true;
                player.GetComponent<Animator>().Play("idle_d");
                player.GetComponent<Animator>().speed = 1;
                player.facing = AnoControl2D.Facing.DOWN;
                // toggle horror ui
                GameObject.Find("2D UI").GetComponent<UIManager2D>().FadeColor(new Color(0.03f, 0.03f, 0.02f, 1f), 4f);
                mode = 1;
            }
        } else if (mode == 1 && HF.TimerDefault(ref t_horror, 8f)) {
            mode = 2;
            AudioHelper.instance.playOneShot("horrorVacuumLong");
        } else if (mode == 2 && HF.TimerDefault(ref t_horror, 3f)) {
            mode = 3;
            GameObject.Find("2D UI").GetComponent<UIManager2D>().StartFade(1, 0, 2f);
            // sfx at 4s, fade is gone
            // wait 3s. sfx maybe should playfor 6 seconds more
        } else if (mode == 3 && HF.TimerDefault(ref t_horror, 5)) {
            CutsceneManager.deactivatePlayer = false;
            dbox.playDialogue("horror-wake-1");
            // move player
            mode = 4;
        } else if (mode == 4 && HF.TimerDefault(ref t_horror, 2.5f)) {
            AudioHelper.instance.PlaySong("NanoHorror-Bedroom", 0, 0);
            mode = 5;
        }
    }

    Vector3 tempRot = new Vector3();
    Vector3 tempPos = new Vector3();
    Vector3 tempScale = new Vector3();
    Color tempCol = new Color();
    AnoControl2D player;
    float t1 = 0;
    int glandilockState = 0;
    float glandilockTimer = 0;
    float t_suck = 0;
    float tm_suck = 3f;
    float t_sucksound = 0;
    PositionShaker ps;
    public ParticleSystem poofParticles;
    ParticleSystem.EmissionModule emissionModule;
    ParticleSystem.MinMaxCurve emitRate;
    Vector3 tempscale = new Vector3();
    Color tempcol = new Color();
    SpriteRenderer sr;
    Vector3 tempV2 = new Vector3();
    bool changedSong = false;
    bool doCloneDustStuff = false;
    bool doHorrorStuff = false;
    float t_horror = 0;
    void Update () {

        if (doCloneDustStuff) {
            UpdateCloneDustStuff();
            return;
        }
        if (doHorrorStuff) {
            UpdateHorrorStuff();
            return;
        }

        if (mode == 0) {

            if (glandilockState == 0) {
                if (player.InSameRoomAs(transform.position) && HF.AreTheseInTheSameroom(transform,player.transform)) {
                    glandilockAnim.Play("blinkon");
                    // organic gluu cheep!
                    //AudioHelper.instance.playOneShot("GlandilockEyeOpen");
                    glandilockState = 1;

                    if (!changedSong) {
                        changedSong = true;
                        AudioHelper.instance.FadeAllSongs(1,0);
                        AudioHelper.instance.PlaySong("DustCue", 0, 14.4f,false,0.45f);
                    }

                }
            } else if (glandilockState == 1) {
                glandilockTimer += Time.deltaTime;
                if (glandilockTimer > 3.5f) {
                    // organic glugluglglgugluglgug
                    //AudioHelper.instance.playOneShot("GlandilockGargle");
                    glandilockAnim.ForcePlay("blink");
                    glandilockTimer = 0;
                }
                if (!player.InSameRoomAs(transform.position)) {
                    glandilockAnim.Play("idle");
                    glandilockState = 0;
                    changedSong = false;
                    AudioHelper.instance.UnfadeAllSongsExceptDust(1, 1);
                    AudioHelper.instance.StopSongByName("DustCue");
                }
            }

            if (onplayer && MyInput.special) {
                t_suck += Time.deltaTime;
                if (MyInput.shortcut) t_suck += 10 * Time.deltaTime;
                child.GetComponent<Oscillate>().enabled = false;
                if (t_suck >= tm_suck) {
                    transform.parent.Find("Mask").gameObject.SetActive(false);
                    t_suck = tm_suck;
                    GameObject.Find("2D UI").GetComponent<UIManager2D>().setTalkAvailableIconVisibility(false);
                    CutsceneManager.deactivatePlayer = true;
                    mode = -10;
                    glandilockAnim.Play("flashtored");
                    glandilockPS.enabled = true;
                    AudioHelper.instance.playOneShot("BeamIn_Land",1,0.9f);
                    poofParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                    tempV2 = child.transform.position;
                    ps.amplitude.Set(0, 0, 0);
                    return;
                }
            } else {
                if (t_suck > 0) {
                    child.GetComponent<Oscillate>().enabled = true;
                    t_suck -= 2 * Time.deltaTime;
                }
            }
            if (t_suck < 0) {
                t_suck = 0;

            }

            // Scale the crystal, make it shake, change emissionrate, change suck pitch and volume.
            float r = t_suck / tm_suck;
            if (t_suck > 0 && HF.TimerDefault(ref t_sucksound, 0.10f)) {
                AudioHelper.instance.playOneShot("BigCrystalSuckPoof1", 1f, 1.3f + 1.2f * r);
            }
            tempscale = child.transform.localScale;
            float ss = 1f + 0.33f * r;
            tempscale.Set(ss, ss, ss);
            child.transform.localScale = tempscale;
            ps.amplitude.x = 0.03f + r * 0.23f;
            ps.amplitude.y = 0.03f + r * 0.23f;
            if (t_suck <= 0) {
                ps.amplitude.Set(0, 0, 0);
            }
            emitRate.constant = 3 + 20f * r;
            emissionModule.rateOverTime = emitRate;

            tempcol = sr.color;
            tempcol.r = 1f - 0.4f * r;
            tempcol.g = tempcol.b = tempcol.r;
            sr.color = tempcol;

        } else if (mode == -10) {
            // Scale the crystal to zero, move it to the player.
            tempscale = child.transform.localScale;
            float maxt = 0.4f;
            float ss = 1f + 0.2f - 1.2f * (t1 / (maxt - 0.05f));
            if (ss < 0) ss = 0;
            tempscale.Set(ss, ss, ss);
            child.transform.localScale = tempscale;
            t1 += Time.deltaTime;
            if (t1 >= maxt) {
                int dustAdded = 0;
                // 3 crystals = 108. 92 30-46 picksup
                if (DataLoader._getDS(Registry.FLAG_RING_OPEN) == 1) {
                    dustAdded = (int)Ano2Stats.addDust(36);
                } else {
                    dustAdded = (int)Ano2Stats.addDust(12);
                }
                if (dustAdded > 0) {
                    player.GetComponent<DustBar>().AddDust(dustAdded);
                }
                AudioHelper.instance.StopSongByName("DustCue");
                player.StopSuckingAnim();
                player.GetComponent<Animator>().speed = 1;
                player.GetComponent<Animator>().enabled = true;
                if (actuallyGetSuckedIn) {
                    AudioHelper.instance.playOneShot("vacuumSucked");
                    RectTransform trans = GameObject.Find("DustCrystalUI").GetComponent<RectTransform>();
                    trans.SetParent(GameObject.Find("Game Render Texture").transform);
                    trans.SetSiblingIndex(GameObject.Find("UI Overlay").transform.GetSiblingIndex() + 1);
                    trans.localScale = new Vector3(1, 1, 1);
                    // -144 -89
                    trans.localPosition = new Vector2(-144f, -89f);
                    trans.GetComponent<UnityEngine.UI.Image>().enabled = true;
                    CutsceneManager.deactivatePlayer = false;
                    doCloneDustStuff = true;
                    mode = 0;
                } else {
                    mode = -9;
                }


            }
            tempscale = Vector3.Lerp(tempV2, player.transform.position, t1 / maxt);
            child.transform.position = tempscale;

        } else if (mode == -9) {
            player.GetComponent<Animator>().speed = 0;
            player.GetComponent<Animator>().enabled = false;
            if (MyInput.shortcut) t1 = 1.3f;
            // wait as glandilock flashes, wait a beat, then play a sound as the pulse plays
            if (HF.TimerDefault(ref t1, 1.3f)) {
                pulse.Play();
                AudioHelper.instance.playOneShot("GlandPulse");
                player.GetComponent<HealthBar>().FullHeal();
                mode = -8;
            }
        } else if (mode == -8) {
            // wiat ofr pulse to finish, then fade to white.
            if (MyInput.shortcut) t1 = 1.5f;
            if (HF.TimerDefault(ref t1, 1.5f)) {
                if (MyInput.shortcut) {
                    GameObject.Find("2D UI").GetComponent<UIManager2D>().StartFade(new Color(0.97f, 0.97f, 0.97f), 0, 1, 0.1f);
                } else {
                    GameObject.Find("2D UI").GetComponent<UIManager2D>().StartFade(new Color(0.97f, 0.97f, 0.97f), 0, 1, 2f);
                }
                AudioHelper.instance.playOneShot("GlandPulseFade");
                mode = 2;
            }
        } else if (mode == 2) {
            // play card dialogue
            if (MyInput.shortcut) t1 = 2.5f;
            if (HF.TimerDefault(ref t1, 2.5f)) {

                if (HF.CurSceneName() == "NanoHorror") {
                    doHorrorStuff = true;
                    AudioHelper.instance.playOneShot("horrorCrystal");
                    mode = 0;
                    return;
                }
                AudioHelper.instance.PlaySong("CrystalMonologue", 0, 26.422f);
                glandilockAnim.Play("idle");
                mode = 2002;
            }
        } else if (mode == 2002) {
            if (MyInput.shortcut) t1 = 1.2f;
            if (HF.TimerDefault(ref t1, 1.2f)) {

                if (cardID == 2) {
                    Ano2Stats.GetItem(4);
                }
                ExitNanoCutscenes.setprefix(cardID);
                if (MyInput.shortcut) {
                    mode = 2003;
                    return;
                }
                //dbox.oneChunkNoBox = true;
                dbox.oneChunkNoUnderlayer = true;
                //dbox.oneChunkBoxY = 0;
                if (cardID == 0) dbox.playDialogue("tongue-crystal");
                if (cardID == 1) dbox.playDialogue("pig-crystal");
                if (cardID == 2) dbox.playDialogue("cougher-crystal");
                if (cardID == 3) dbox.playDialogue("rage-crystal");


                if (cardID == 4) dbox.playDialogue("golem-crystal");
                if (cardID == 6) dbox.playDialogue("clone-crystal");
                if (cardID == 8) dbox.playDialogue("stalker-crystal");

                if (cardID == 14) dbox.playDialogue("skel-crystal");
                if (cardID == 16) dbox.playDialogue("fantasy-crystal");
                mode = 2003;
            }
        } else if (mode == 2003 && dbox.isDialogFinished()) {
            // Kaching... swrrrrrrrrrrrrr
            glandilockPS.enabled = false; 
            AudioHelper.instance.playOneShot("sparkBarShatter");
            AudioHelper.instance.StopSongByName("CrystalMonologue");
            //AudioHelper.instance.playOneShot("CrystalDialogueDone");
            mode = 3;
        } else if (mode == 3) {

            if (HF.TimerDefault(ref t1, 0.5f)) {
                child.GetComponent<SpriteRenderer>().enabled = false;
                card.GetComponent<SpriteRenderer>().enabled = true;
                // wait, hide core sprite, spawn card sprite
                mode = 4;
                if (MyInput.shortcut) {
                    GameObject.Find("2D UI").GetComponent<UIManager2D>().StartFade(new Color(0.97f, 0.97f, 0.97f), 1, 0, 0.1f);
                } else {
                    GameObject.Find("2D UI").GetComponent<UIManager2D>().StartFade(new Color(0.97f, 0.97f, 0.97f), 1, 0, 2f);
                }
            }
        } else if (mode == 4) {
            if (MyInput.shortcut) t1 += 4 * Time.deltaTime;
            if (HF.TimerDefault(ref t1, 2f)) {
                mode = 6;
                if (!MyInput.shortcut) {
                    dbox.playDialogue("card-get");
                } else {
                    mode = 2123;
                }
            }
        } else if (mode == 6) {
            if (dbox.isDialogFinished()) {
                AudioHelper.instance.playOneShot("cardGet");
                card.GetComponent<Oscillate>().enabled = false;
                mode = 7;
            }
        } else if (mode == 7) {
            // card sprite rising up/off screen, scaling, rotating
            tempPos = card.transform.position;
            tempPos.y += Time.deltaTime * 1.8f; card.transform.position = tempPos;

            tempScale = card.transform.localScale;
            tempScale.x += Time.deltaTime * 2; tempScale.y += Time.deltaTime * 2; card.transform.localScale = tempScale;

            tempRot = card.transform.localEulerAngles;
            tempRot.z += Time.deltaTime * 360f * 0.65f; card.transform.localEulerAngles = tempRot;

            tempCol = card.GetComponent<SpriteRenderer>().color;
            tempCol.a -= Time.deltaTime * 0.5f; card.GetComponent<SpriteRenderer>().color = tempCol;

            if (tempCol.a <= 0) {
                mode = 2123;
            }
        } else if (mode == 2123) {
            // Get items and leave area
            if (YolkID == 0 && Ano2Stats.HasCard(cardID) == false) {
                Ano2Stats.GetCard(cardID);
            }
            if (SetFlagToOneOnDestruction != "") {
                DataLoader.instance.setDS(SetFlagToOneOnDestruction, 1);
            }

            SparkGameController.ReturnFrom2D_SourceScene = HF.SceneNameToEnum(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);

            SparkGameController.SparkGameDestScene = SceneToExitTo;
            SparkGameController.SparkGameDestObjectName = NextSceneDestinationName;
            Wormhole.ReturningFrom2D = true;
            DataLoader.instance.enterScene("none",Registry.GameScenes.Wormhole);
            CutsceneManager.deactivatePlayer = false;
            mode = 10000;
        } 
	}

    NPCHelper CloneDrone;
    void UpdateCloneDustStuff() {
        if (mode == 0) {
            GameObject.Find("endblocker").GetComponent<BoxCollider2D>().enabled = true;
            dbox.playDialogue("clone-dustfail",0,4);
            player.StopSuckingAnim();
            mode = 1;
            CloneDrone = GameObject.Find("CloneDrone").GetComponent<NPCHelper>();
            CloneDrone.CloneDroneMoveToEndPos();
            
        } else if (mode == 1) {
            if (CloneDrone.cloneFollowMode == 10) {
                doCloneDustStuff = false;
                mode = -8;
                CutsceneManager.deactivatePlayer = true;

            }
            // wait around until the clone scene stuff happens?

        }
    }

    bool onplayer = false;
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("Player")) {
            onplayer = true;
  //          GameObject.Find("2D UI").GetComponent<UIManager2D>().setTalkAvailableIconVisibility(true);
        }
    }
    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.CompareTag("Player")) {

            onplayer = false;
//            GameObject.Find("2D UI").GetComponent<UIManager2D>().setTalkAvailableIconVisibility(false);
        }
    }
}
