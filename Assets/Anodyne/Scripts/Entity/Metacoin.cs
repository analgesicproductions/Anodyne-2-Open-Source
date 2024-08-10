using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Metacoin : MonoBehaviour {

    string sceneName;
    public bool spawnerCode = false;
    public GameObject mcPrefabForSpawning = null;
    [System.NonSerialized]
    public bool pickupable = true;
    int idx = 0;
    public static List<int> MC_Indices_In_This_Scene;
    public static string curMCIndicesScene = "";

    public static void ResetStatics() {
        MC_Indices_In_This_Scene = null;
        curMCIndicesScene = "";
    }
    void Start () {
        // make a new list
        sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        if (sceneName != curMCIndicesScene && MC_Indices_In_This_Scene == null) {
            MC_Indices_In_This_Scene = new List<int>();
            curMCIndicesScene = sceneName;
        }
        if (spawnerCode) {
            print("<color=red>WARNING Metacoin spawner on</color>");
            return;
        }
        if (name == "MetacoinPrefab") name += " (0)";

        if (transform.Find("MetacoinModel") != null) {
            Destroy(transform.Find("MetacoinModel").gameObject);
            Instantiate(Resources.Load("Prefabs/Metacoin"), transform);
        }

        if (transform.parent == null || transform.parent.name != "Metacoins" || name.IndexOf("(") == -1 || name.IndexOf(")") == -1) {
            print("Invalid metacoin: "+name);
            gameObject.SetActive(false);
            return;
        }
        string s = name.Split(' ')[1]; // MetacoinPrefab (1) -> (1)
        s = s.Replace("(", "");
        s = s.Replace(")", "");
        idx = int.Parse(s, System.Globalization.CultureInfo.InvariantCulture);
        if (MC_Indices_In_This_Scene.Contains(idx)) {
            print("<color=red>WARNING Metacoin duplicate index: </color>" + idx);
        } else {
            MC_Indices_In_This_Scene.Add(idx);
        }
		if (DataLoader._getDS(Registry.FLAG_DESERT_OPEN) == 0) {
            gameObject.SetActive(false);
        } else if (DataLoader.instance.hasLineBeenRead(sceneName+"-mcs",idx)) {
            gameObject.SetActive(false);
        }
	}

    public float spawnY_Offset = 2f;
	void Update () {
		if (spawnerCode) {
            if (Input.GetKeyDown(KeyCode.M)) {
                GameObject g = Instantiate(mcPrefabForSpawning, transform.parent, true);
                g.transform.position = GameObject.Find(Registry.PLAYERNAME3D_Walkscale).transform.position;
                Vector3 v = g.transform.position;
                v.y += spawnY_Offset;
                MC_Indices_In_This_Scene.Sort();
                g.transform.position = v;
                g.name = "MetacoinPrefab";
                g.GetComponent<Metacoin>().pickupable = false;
                int lastIndex = -1;
                if (MC_Indices_In_This_Scene.Count > 0) {
                    lastIndex = MC_Indices_In_This_Scene[MC_Indices_In_This_Scene.Count - 1];
                }
                g.name += " (" + (lastIndex + 1).ToString() + ")";
                g.transform.SetAsLastSibling();
            }
        }
	}

    private void OnTriggerEnter(Collider other) {
        if (!pickupable || spawnerCode) return;
        if (other.name == Registry.PLAYERNAME3D_Walkscale || other.name == Registry.PLAYERNAME3D_Ridescale) {
            HF.SpawnSparkyPoof3D(transform.position);
            AudioHelper.instance.playOneShot("blockExplode");
            SaveManager.metacoins++;
            SaveManager.totalFoundCoins++;
            if (SaveManager.totalFoundCoins >= 0) DataLoader.instance.unlockAchievement(DataLoader.achievement_id_COIN_1);
            if (SaveManager.totalFoundCoins >= 200) DataLoader.instance.unlockAchievement(DataLoader.achievement_id_COIN_200);
            if (SaveManager.totalFoundCoins >= 400) DataLoader.instance.unlockAchievement(DataLoader.achievement_id_COIN_400);
            if (SaveManager.totalFoundCoins >= 500) DataLoader.instance.unlockAchievement(DataLoader.achievement_id_COIN_500);
            DataLoader.instance.updateReadDialogueState(sceneName + "-mcs", idx, idx);
            Destroy(gameObject);
        }
    }
}
