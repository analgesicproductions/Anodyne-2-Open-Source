using System.Collections.Generic;
using UnityEngine;

public class Ano2Stats : MonoBehaviour {

    public static int ANODYNE_DUST_GOAL = 350;
    public static int DESERT_DUST_GOAL = 200;
    public static int RING_DUST_GOAL = 100;
    public static int CARDS_PER_PRISM_UPGRADE = 4;
    public static int MAX_CARDS = 24;
    public static int VOLUME_PER_PRISM_UPGRADE = 50;

    public static float dust = 0;
    public static int prismCapacity = 50;
    public static int prismCurrentDust = 0;

    public static float nanopointStartingDust = 0;
    public static float previousNanopointEarnedDust = 0;

    public static int ITEM_ID_VACUUM = 5;
    public static int ITEM_ID_SCALESPARK = 6;
    public static int ITEM_ID_NANOSPARK = 7;
    public static int ITEM_ID_CARDDETECTOR = 9;
    public static int ITEM_ID_TRYLOCK_COLLAR = 14;
    public static int ITEM_ID_TRYLOCK_RED = 15;
    public static int ITEM_ID_TRYLOCK_BLUE = 16;
    public static int ITEM_ID_TRYLOCK_GREEN = 17;
    public static int ITEM_ID_TRYLOCK_BASE = 18;
    public static int ITEM_ID_ADVENTURER = 19;

    public static int GetMaxDust() {
        if (DataLoader._getDS(Registry.FLAG_RING_OPEN) == 1) {
            return 90;
        } else {
            return 60;
        }
    }
    public static void resetStatics() {
        dust = 0;
        prismCapacity = 50;
        prismCurrentDust = 0;
    }

    // Saved through dialogue system, no need to create separate vars hooray
    public static void saveStatsToFlags() {
        DataLoader.instance.silenceDSFlagsOnce = true;
        DataLoader.instance.setDS("__dust", Mathf.RoundToInt(dust));

        DataLoader.instance.silenceDSFlagsOnce = true;
        DataLoader.instance.setDS("__prismcap", prismCapacity);

        DataLoader.instance.silenceDSFlagsOnce = true;
        DataLoader.instance.setDS("__prismdust", prismCurrentDust);
        print("[saved]: dust: " + dust + " prismCapacity: " + prismCapacity + " prismDust: " + prismCurrentDust);
        // print("Stats saved into dialogue flag database.");
    }
    public static void loadStatsFromFlags() {
        dust = DataLoader.instance.getDS("__dust");
        prismCapacity = DataLoader.instance.getDS("__prismcap");
        prismCurrentDust = DataLoader.instance.getDS("__prismdust");
        if (prismCapacity < 50) prismCapacity = 50;
        print("[loaded]: dust: " + dust + " prismCapacity: " + prismCapacity + " prismDust: " + prismCurrentDust);
        //print("Note: All stats set to saved values!");
    }

    // returns dust added
    public static float addDust(float dustToAdd) {
        float dustAdded = dustToAdd;
        if (dustToAdd + dust > GetMaxDust()) {
            dustAdded -= ((dustToAdd + dust) - GetMaxDust());
            dust = GetMaxDust();
        } else {
            dust += dustToAdd;
        }
        return dustAdded;
    }


    public static int CountTotalCards(bool includePal=false) {
        int r = 0;
        for (int i = 0; i < MAX_CARDS; i++) {
            if (DataLoader.instance.getDS("CARD" + i.ToString()) == 1) {
                r++;
            }
        }
        if (includePal && HasCard(-1)) r++;
        return r;
    }
    public static int CountUnusedCards() {
        int r = 0;
        for (int i = 0; i < MAX_CARDS; i++) {
            if (DataLoader.instance.getDS("CARD" + i.ToString()) == 1) {
                if (DataLoader.instance.getDS("CARD_USED" + i.ToString()) == 0) {
                    r++;
                }
            }
        }
        return r;
    }


    public static void ResetCards() {
        for (int i = 0; i < MAX_CARDS; i++) {
            RemoveCard(i);
        }
    }
    public static void GetCard(int id) {
        if (id == 0) print("Get Tongue card");
        if (id == 1) print("Get Pig card");
        if (id == 2) print("Get Cougher card");
        if (id == 3) print("Get Rage card");

        if (id == 4) print("Get Geof card");
        if (id == 5) print("Get Geof Village card");
        if (id == 6) print("Get Lonwei card");
        if (id == 7) print("Get RingClone card");
        if (id == 8) print("Get Iwasaki card");
        if (id == 9) print("Get NanoStalker chest card");
        if (id == 10) print("Get Carwash card");
        if (id == 11) print("Get Waterfall Mtn card");

        if (id == 12) print("Get NanoOrb Ending card");

        DataLoader.instance.setDS("CARD" + id.ToString(), 1);
    }

    public static void TryUpgradeHealth(string flag ) {
        if (DataLoader._getDS(flag) == 0) {
            SaveManager.healthUpgrades++;
            if (SaveManager.healthUpgrades > 6) SaveManager.healthUpgrades = 6;
            DataLoader._setDS(flag, 1);
        }
    }

    public static void GetItem(int id) {
        if (id == 0) print("Get Cereal");
        if (id == 1) print("Get YolkMilk");
        if (id == 2) print("Get Reguloid");
        if (id == 3) print("Get Glandilock Seed");
        if (id == 4) print("Get Mint Lamp");
        DataLoader.instance.setDS("ITEM" + id.ToString(), 1);
    }

    // 0 = cereal, 1 = milk, 2 = reguloid
    public static bool HasItem(int id) {
        return DataLoader.instance.getDS("ITEM" + id.ToString()) == 1;
    }
    public static void RemoveItem(int id ) {
        DataLoader.instance.setDS("ITEM" + id.ToString(), 0);
    }

    public static bool HasCard(int id) {
        if (id == -1) return DataLoader.instance.getDS("CARD_PAL") == 1;
        if (id == -2) return DataLoader.instance.getDS("CARD_PAL2") == 1;
        return DataLoader.instance.getDS("CARD" + id.ToString()) == 1;
    }
    public static void RemoveCard(int id) {
        if (id == -1) {
            DataLoader.instance.setDS("CARD_PAL", 0);
            return;
        }
        if (id == -2) {
            DataLoader.instance.setDS("CARD_PAL2", 0);
            return;
        }
        DataLoader.instance.setDS("CARD" + id.ToString(), 0);
        DataLoader.instance.setDS("CARD_USED" + id.ToString(), 0);
    }

    // Not sure if I'll need this. Maybe for debugging.
    public static void UseCard(int id) {
        if (id == -1) return;
        DataLoader.instance.setDS("CARD_USED" + id.ToString(), 1);
    }

    // Uses the first unused card. Return its ID. Return -1 if no card used.
    public static int UseUnusedCard() {
        for (int i = 0; i < MAX_CARDS; i++) {
            if (DataLoader._getDS("CARD" + i.ToString()) == 1) {
                if (DataLoader._getDS("CARD_USED" + i.ToString()) == 0) {
                    DataLoader._setDS("CARD_USED" + i.ToString(), 1);
                    return i;
                }
            }
        }
        return -1;
    }


    public static int DepositDust(int howMany = 1, bool all = false) {
        if (all) howMany = Mathf.CeilToInt(dust);
        int deposited = 0;
        for (int i = 0; i < howMany; i++) {
            if (dust > 0 && prismCurrentDust < prismCapacity) {
                dust--;
                prismCurrentDust++;
                deposited++;
            }
        }
        if (dust < 0) dust = 0;
        return deposited;
    }


    public static int CountPrismUpgrades() {
        return (prismCapacity / VOLUME_PER_PRISM_UPGRADE) - 1;
    }

    // changes the prism capacity, resets current cards reinforced with
    // Returns list of used cards - if empty then the prism upgrade didn't work
    // Use to determine what msg to show player when trying to upgrade prism
    // obviously check whether the story allows you to proceed before calling
    public static List<int> PrismUpgrade() {
        List<int> l = new List<int>();

        for (int i = 0; i < CARDS_PER_PRISM_UPGRADE; i++) {
            l.Add(UseUnusedCard());
        }
        print("cards spent: " + l);
        prismCapacity += VOLUME_PER_PRISM_UPGRADE;
        return l;
    }



    static Dictionary<string, float> UnscaledDustDict = new Dictionary<string, float>();
    public static void AddToUnscaledDustTotal(float rawAmount, string parentName) {
        if (UnscaledDustDict.ContainsKey(parentName)) {
            UnscaledDustDict[parentName] += rawAmount;
        } else {
            print("Creating dust group: " + parentName);
            UnscaledDustDict.Add(parentName, rawAmount);
        }
    }
    // Set how much a total dust group should be worth (e.g. NanoAlbumen-1 is the 1st dust group in NanoAlbumen scene)
    public static float GetScaledDustValue(float rawAmount,string parentName) {
        float currentLevelMaxDust = 10f;
        if (parentName == "NanoAlbumen-1") currentLevelMaxDust = 15f;
        if (parentName == "NanoAlbumen-2") currentLevelMaxDust = 25f;
        if (parentName == "CCC Test") currentLevelMaxDust = 10f;
        if (parentName == "TestDustGroup") currentLevelMaxDust = 10f;

        return (rawAmount / UnscaledDustDict[parentName])*currentLevelMaxDust;
    }

}
