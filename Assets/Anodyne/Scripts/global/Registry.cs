using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Game related global variables that are used by many scripts
public class Registry : MonoBehaviour {

    public static string destinationDoorNameForPauseRespawn = "";
	public static string destinationDoorName = "";
	public static string waitingSpaceExitSceneName;
	public static string waitingSpaceExitDoorName;

	public static string enterGameFromLoad_SceneName;
	public static Vector3 enterGameFromLoad_Position;
	public static bool justLoaded = false; // reset in Update() on the DataLoader instance.

	public static bool DEV_MODE_ON = false;
    public static bool CONSOLE_BUILD = false;
    public static string PLAYERNAME2D = "2D Ano Player";
    public static string PLAYERNAME3D_Walkscale = "MediumPlayer";
    public static string PLAYERNAME3D_Ridescale = "BigPlayer";
    public static bool startedNewGameButDidntSave = false;
    public static string FLAG_RING_OPEN = "ddp-open-ring1";
    public static string FLAG_DESERT_OPEN = "ddp-open-ring2";
    public static string FLAG_SAW_GOODEND = "sawGoodEnd";
    public static string FLAG_SAW_BADEND = "sawBadEnd";

    // Used for scene selection dropdowns
    // ONLY ADD TO THE END OF THIS LIST
    public enum GameScenes {ZZ_Test3D,test2D,NanoAlbumen,Albumen, Albumen2, CCC, R1N, GrowthChapel, CenterChamber, Pig, NanoTongue, Cougher, NanoRage, NanoSanctuary, CougherHome, SparkGame, Wormhole, NanoGolem, RingGolem, NanoDustbound, DesertSpireCave, NanoDB_Wrestling, NanoHandfruitHaven, RingCCC, DesertSpire, RingClone, DesertSpireTop, RingCave, NanoClone, RingHighway, PicoOcean, NanoStalker, NanoOcean, Wormhole2D, NanoDB_Clean, NanoHorror, NanoFantasy, PicoFantasy, NanoSkeligum, NanoOrb, DesertOrb, DesertShore, DesertField, NanoZera, PicoZera, NanoNexus, PG_Ano1, PG_Drawer, PicoPG, NanoPG, ZZ_CCC_Old_3, ZZ_JoniRing1Test1, DEBUG2DFAKE, ZZ_test3D, ZY_Sean_ArchTests, ZZ_Albumen_Old, ZZ_RingGolem_Prototype,ZZ_JoniFearTest02, ZZ_GIF_NanoAlbumen, ZZ_ChapelTrailer, ZZ_CenterChamber, ZZ_NanoDustboundOld, S_MarinaRing1Test, ZZ_CCC_Old_1, ZZ_CCC_Old_2, ZZ_R1N, ZZ_prototype }

    public static void resetStatics() {
        MediumControl.doSpinOutAfterNano = false;
        MediumControl.doSpinOutAfterNanoDuringPause = false;
        CameraTrigger.PausedByCameraTrigger = false;
        DialogueAno2.AnyScriptIsParsing = false;
        DataLoader.instance.isPaused = false;
        SaveModule.saveMenuOpen = false;
        CutsceneManager.deactivatePlayer = false;
        MedBigCam.inCinemachineMovieMode = false;
        MedBigCam.forceRidescaleNextScene = false;
        Ano2Stats.resetStatics();
        Wormhole.ReturningFrom2D = false;
        Wormhole2D.ReturningFromPico = false;
        ExitNanoCutscenes.nanoprefix = "";
        Registry.destinationDoorNameForPauseRespawn = "";
        Anodyne.Spark.ScaleSparkOn = false;
        Anodyne.Door.ResetHorrorStatics();
        Anodyne.GlandilockBoss.diedOnce = false;
    }

    public static void set_startedNewGameButDidntSave(bool v) {
        if (v != startedNewGameButDidntSave) {
            print("setting startedNewGameButDidntSave to " + v);
        }
        startedNewGameButDidntSave = v;
    }

    public static bool DestinationDoorIsTwoDeep = false;
    public static void MoveObjectToDestinationDoor(GameObject g) {
        if (DestinationDoorIsTwoDeep) {
            string[] parts = destinationDoorName.Split(new string[] { "||" },System.StringSplitOptions.None);
            GameObject dest = GameObject.Find(parts[0]).transform.Find(parts[1]).Find(parts[2]).gameObject;
            g.transform.position = dest.transform.position;
            DestinationDoorIsTwoDeep = false;
        } else if (GameObject.Find(destinationDoorName) != null) {
            g.transform.position = GameObject.Find(destinationDoorName).transform.position;
        }
    }

    // Called by a nanopoint when entering nanomode.
    public static void InitializeNanopointData(Anodyne.NanopointData data) {
        if (data.roomSizeY == -1) {
            SceneData2D.SetRoomSize(data.roomSizeX, data.roomSizeX);
        } else {
            SceneData2D.SetRoomSize(data.roomSizeX, data.roomSizeY);
        }
    }
    public static void InitPicopoint() {
        SceneData2D.SetRoomSize(6, 6);
    }

    public enum ProgressVal { NONE, MET_PAL_ENTERED_CCC, BEFORERINGOPEN, RING_OPENED, RING_AFTER_FIRST_UPGRADE, RING_BEFORE_HURT, RING_DURING_HURT, finalhavenscene, JUST_LEFT_DB, SAW_CLIFFTOP_SCENE, MET_CV, DESERT_250_BEFORE_MC, DESERT_300, DESERT_B4_LAST_FILL, DESERT_350, READY_FOR_CONFRONT, SAW_ANO, SAW_GOODEND, SAW_BOTHENDINGS };
    // Everything here must be initialized by the end of Registry's awake.
    public static bool usedProgressSkipThisPlaymode = false;
    public static void ProgressSkip(ProgressVal p) {
        if (usedProgressSkipThisPlaymode) {
            print("Skipping using progressskip since used once already");
            return;
        }
        usedProgressSkipThisPlaymode = true;
        if (p == ProgressVal.NONE) return;

        DataLoader._setDS("USINGPROGRESSSKIP", 1);

        DataLoader._setDS("cc-nanobot", 1);
        DataLoader._setDS("chapel-entry", 1);
        DataLoader._setDS("cc-after-miniboss", 1);
        for (int i = 0; i < 4; i++) {
            Ano2Stats.GetItem(i);
        }
        DataLoader._setDS("ccc-entry", 1);
        if (p == ProgressVal.MET_PAL_ENTERED_CCC) return;


        for (int i = 0; i < 4; i++) {
            Ano2Stats.GetCard(i);
        }
        Ano2Stats.PrismUpgrade();
        DataLoader._setDS("prism-pal-intro", 1);
        DataLoader._setDS("ridescaleprompt", 1);
        DataLoader._setDS("sparkgamehelp", 2);

        DataLoader._setDS("rage-LOOPCOUNT", 1);
        DataLoader._setDS("rage2", 1);
        DataLoader._setDS("NanoRageSPARKGAMEDONE", 1);
        DataLoader._setDS("NanoRageEntrancebeamed", 1);
        DataLoader._setDS("rage-nanobot", 1);

        DataLoader._setDS("tongue-state", 2);
        DataLoader._setDS("tongue-sawunlock", 1);
        DataLoader._setDS("NanoTongueSPARKGAMEDONE", 1);
        DataLoader._setDS("NanoTongueEntrancebeamed", 1);
        DataLoader._setDS("tongue-nanobot", 1);

        DataLoader._setDS("gwom-gwom", 1);
        DataLoader._setDS("gwom-sup", 1);
        DataLoader._setDS("gwom-chup", 1);
        DataLoader._setDS("gwom-pup", 1);
        DataLoader._setDS("pig-sawunlock", 1);
        DataLoader._setDS("PigSPARKGAMEDONE", 1);
        DataLoader._setDS("PigEntrancebeamed", 1);
        DataLoader._setDS("pig-nanobot", 1);

        DataLoader._setDS("cougher-bed", 1);
        DataLoader._setDS("cougher-exit", 1);
        DataLoader._setDS("cougher-LOOPCOUNT", 1);
        DataLoader._setDS("CougherSPARKGAMEDONE", 1);
        DataLoader._setDS("CougherEntrancebeamed", 1);
        DataLoader._setDS("cougher-nanobot", 1);
        Ano2Stats.GetItem(4);

        DataLoader._setDS("HEALTHCCC", 1);
        DataLoader._setDS("pal-card-1",1);
        DataLoader._setDS("pal-card-2",1);
        DataLoader._setDS("pal-card-3",1);
        DataLoader._setDS("pal-card-4",1);
        DataLoader._setDS("ddp-full-r0", 1);


        SaveManager.healthUpgrades = 1;

        if (p == ProgressVal.BEFORERINGOPEN) return;

        DataLoader._setDS("ddp-open-ring1", 1);

        if (p == ProgressVal.RING_OPENED) return;



        Ano2Stats.GetCard(-1);
        DataLoader._setDS("get-pal-card", 1);

        for (int i = 4; i < 8; i++) {
            Ano2Stats.GetCard(i);
        }
        Ano2Stats.PrismUpgrade();
        DataLoader._setDS("ring-pal-1", 1);
        if (p == ProgressVal.RING_AFTER_FIRST_UPGRADE) return;

        ////////////////
        for (int i = 8; i < 12; i++) {
            Ano2Stats.GetCard(i);
        }
        Ano2Stats.prismCurrentDust = 150;
        Ano2Stats.dust = 55;
        DataLoader._setDS("ring-pal-2", 1);


        DataLoader._setDS("carwashSwitch", 1);

        DataLoader._setDS("geof-intro", 3);
        DataLoader._setDS("geof-sawunlock", 3);
        DataLoader._setDS("golem-state", 1);
        DataLoader._setDS("NanoGolemSPARKGAMEDONE", 1);
        DataLoader._setDS("NanoGolemEntrancebeamed", 1);

        DataLoader._setDS("clone-enter", 1);
        DataLoader._setDS("clone-lonwei", 1);
        DataLoader._setDS("clone-sawunlock", 1);
        DataLoader._setDS("NanoCloneSPARKGAMEDONE", 1);
        DataLoader._setDS("NanoCloneEntrancebeamed", 1);


        DataLoader._setDS("stalker-1", 1);
        DataLoader._setDS("stalker-2", 1);
        DataLoader._setDS("stalker-sawunlock", 1);
        DataLoader._setDS("doornanocaveentry", 1);
        DataLoader._setDS("NanoStalkerSPARKGAMEDONE", 1);
        DataLoader._setDS("NanoStalkerEntrancebeamed", 1);

        DataLoader._setDS("HEALTHRingClone", 1);
        DataLoader._setDS("HEALTHEldi", 1);
        DataLoader._setDS("ring-health", 1);
        DataLoader._setDS("ring-health-2", 1);
        DataLoader._setDS("ring-health-3", 1);

        SaveManager.healthUpgrades = 3;

        if (p == ProgressVal.RING_BEFORE_HURT) return;



        //////////////////
        DataLoader._setDS("dustwallenter", 1); // saw enter desert scene
        DataLoader._setDS("pal-ring-3", 1);
        if (p == ProgressVal.RING_DURING_HURT) return;


        Ano2Stats.RemoveCard(-1);
        DataLoader._setDS("spire-eat", 1); // wolgali minigame - if true, hides handfruit. if false, top->cave door is hiddene
        DataLoader._setDS("db-field", 1); // entered db
        DataLoader._setDS("haven-first-time", 1);
        DataLoader._setDS("rites-done-1", 1);
        DataLoader._setDS("wrestle-1", 1);
        DataLoader._setDS("rites-done-2", 1);
        DataLoader._setDS("wrestle-2", 1);
        DataLoader._setDS("rites-done-3", 1);
        DataLoader._setDS("wrestle-3", 1);
        DataLoader._setDS("db-blowup", 1); 
        if (p == ProgressVal.finalhavenscene) return;


        DataLoader._setDS("rites-final", 1);

        if (p == ProgressVal.JUST_LEFT_DB) return;

        // post blowup spiretop scene
        DataLoader._setDS("postdbspire", 1);
        if (p == ProgressVal.SAW_CLIFFTOP_SCENE) return;

        Ano2Stats.dust = 0;
        Ano2Stats.PrismUpgrade();
        DataLoader._setDS("desert-sanc-1", 1);
        DataLoader._setDS("ddp-open-ring2", 1);
        if (p == ProgressVal.MET_CV) return;


        for (int i = 12; i < 16; i++) {
            Ano2Stats.GetCard(i);
        }
        Ano2Stats.PrismUpgrade();
        Ano2Stats.prismCurrentDust = 250;

        if (p == ProgressVal.DESERT_250_BEFORE_MC) return;

        DataLoader._setDS("desert-sanc-2", 1);

        for (int i = 16; i < 20; i++) {
            Ano2Stats.GetCard(i);
        }
        Ano2Stats.PrismUpgrade();
        Ano2Stats.prismCurrentDust = 300;

        Ano2Stats.GetItem(15);
        Ano2Stats.GetItem(16);
        Ano2Stats.GetItem(17);
        Ano2Stats.GetItem(18);

        if (p == ProgressVal.DESERT_300) return;

        for (int i = 20; i < 24; i++) Ano2Stats.GetCard(i);
        Ano2Stats.PrismUpgrade(); // -> 350
        Ano2Stats.prismCurrentDust = 350;

        if (p == ProgressVal.READY_FOR_CONFRONT || p == ProgressVal.SAW_GOODEND) {
            DataLoader._setDS("desert-dog", 2);
            DataLoader._setDS("desert-db-after", 1);
            Ano2Stats.GetCard(-2);
        }
        if (p == ProgressVal.READY_FOR_CONFRONT) {
            Ano2Stats.prismCurrentDust = 300;
            Ano2Stats.dust = 50;
        }
        if (p == ProgressVal.DESERT_B4_LAST_FILL) {
            Ano2Stats.prismCurrentDust = 300;
            Ano2Stats.dust = 89;
        }

        DataLoader._setDS("HEALTHNanoFantasy", 1);
        DataLoader._setDS("HEALTHStealer", 1);
        DataLoader._setDS("HEALTHDesertShore", 1);

        SaveManager.healthUpgrades = 6;
        if (p == ProgressVal.DESERT_350) return;
        if (p == ProgressVal.READY_FOR_CONFRONT) return;

        if (p == ProgressVal.SAW_GOODEND) {
            DataLoader._setDS(FLAG_SAW_GOODEND, 1);
        }
        if (p == ProgressVal.SAW_ANO) {
            DataLoader._setDS(FLAG_SAW_BADEND, 1);
        }
        if (p == ProgressVal.SAW_BOTHENDINGS) {
            DataLoader._setDS(FLAG_SAW_GOODEND, 1);
            DataLoader._setDS(FLAG_SAW_BADEND,1);
        }

    }

    // for use after goodend credits
    // badend doesn't save your data, so doesn't matter
    public static void ResetEndingFlags() {
        DataLoader._setDS("end-confront", 0);
        DataLoader._setDS("prismexploded", 0);
        DataLoader._setDS("end-ring", 0);
        DataLoader._setDS("end-channel", 0);

        DataLoader._setDS("zera-intro", 0);
        DataLoader._setDS("KeyNanoZera_KEY1", 0);
        DataLoader._setDS("KeyNanoZera_KEY2", 0);
        DataLoader._setDS("KeyNanoZera_KEY3", 0);
        DataLoader._setDS("KeyblockNanoZerareguloid", 0);
        DataLoader._setDS("KeyblockNanoZeracereal", 0);
        DataLoader._setDS("KeyblockNanoZeramilk", 0);
        DataLoader._setDS("cerealpp", 0);
        DataLoader._setDS("reguloidpp", 0);
        DataLoader._setDS("milkpp", 0);
        DataLoader._setDS("BossBulGS1", 0);
        DataLoader._setDS("BossBulGS2", 0);
        DataLoader._setDS("BossTileGS", 0);

        DataLoader._setDS("gs-phase", 0);
        DataLoader._setDS("end-zera-gs", 0);
    }

}

namespace Anodyne {
    [System.Serializable]
    public class NanopointData {
        public bool SkipsToWormhole = false;
        public bool SkipsEpisodeTitle = false;
        public string loseSparkgameDoor = "";
        public int roomSizeX = 12;
        public int roomSizeY = -1;
        public string picoColorID = "default";
    }
}
