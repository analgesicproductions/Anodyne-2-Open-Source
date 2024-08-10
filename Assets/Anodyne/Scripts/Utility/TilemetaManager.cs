using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
public class TilemetaManager : MonoBehaviour {

	public Dictionary<string,Dictionary<string,List<int>>> TilemapMetadatas;
    public static TilemetaManager instance;
    public static Grid grid;
    public static Tilemap CollisionTilemap;
   // public static Tilemap Tilemap2;
  //  public static Tilemap Tilemap3;
 //   public static Tilemap TilemapFG;
     static Tilemap HoleTilemap;
    bool hasHoleTilemap = false;
//    public static Tilemap TilemapWall;

    public void refreshGrid() {
        if (GameObject.Find("Grid") != null) {
            grid = GameObject.Find("Grid").GetComponent<Grid>();
            if (GameObject.Find("TilemapDesign") != null) {
                CollisionTilemap = GameObject.Find("TilemapDesign").GetComponent<Tilemap>();
                CollisionTilemap.GetComponent<TilemapRenderer>().sortingLayerName = "BottomTilemaps";
            }
            if (GameObject.Find("Tilemap") != null) {
                CollisionTilemap = GameObject.Find("Tilemap").GetComponent<Tilemap>();
                CollisionTilemap.GetComponent<TilemapRenderer>().sortingLayerName = "BottomTilemaps";
            }
            if (GameObject.Find("HoleTilemap") == null) {
                hasHoleTilemap = false;
            } else {
                HoleTilemap = GameObject.Find("HoleTilemap").GetComponent<Tilemap>();
                HoleTilemap.GetComponent<TilemapRenderer>().sortingLayerName = "BottomTilemaps";
            }
        }
    }
    void Start() {


        refreshGrid();
        instance = this;
        DontDestroyOnLoad(this);

        TilemapMetadatas = new Dictionary<string, Dictionary<string, List<int>>>();
        Metadata = new Dictionary<string, List<int>>();

        resetindices();
        SpikeIndices.AddRange(new int[] { 13, 12, 14 });
        addtometadata();
        TilemapMetadatas.Add("debug_tilemap", Metadata);

        resetindices();
        HoleIndices.AddRange(GetIndices("7,8,9,14-19,24-34"));
        addtometadata();
        TilemapMetadatas.Add("basic_tilemap", Metadata);

        resetindices();
        HoleIndices.AddRange(GetIndices("30-32,18"));
        addtometadata();
        TilemapMetadatas.Add("yolk_tilemap", Metadata);

        resetindices();
        addtometadata();
        TilemapMetadatas.Add("yolk_tilemap8x8", Metadata);


        resetindices();
        HoleIndices.AddRange(GetIndices("14"));
        addtometadata();
        TilemapMetadatas.Add("ccc_tongue_tilemap", Metadata);


        resetindices();
        HoleIndices.AddRange(GetIndices("5,77"));
        addtometadata();
        TilemapMetadatas.Add("ccc_pig_tilemap", Metadata);



    }

    int[] GetIndices(string s) {
        List<int> l = new List<int>();
        string[] bits = s.Split(',');
        foreach (string bit in bits) {
            if (bit.Contains("-")) {
                int low = int.Parse(bit.Split('-')[0], System.Globalization.CultureInfo.InvariantCulture);
                int high = int.Parse(bit.Split('-')[1], System.Globalization.CultureInfo.InvariantCulture);
                for (int i = low; i <= high; i++) {
                    l.Add(i);
                }
            } else {
                l.Add(int.Parse(bit, System.Globalization.CultureInfo.InvariantCulture));
            }
        }
        return l.ToArray();
    }
    void addtometadata() {
        Metadata = new Dictionary<string, List<int>>();
        Metadata.Add("spike", SpikeIndices);
        Metadata.Add("hole", HoleIndices);
    }
    Dictionary<string, List<int>> Metadata;
    List<int> SpikeIndices;
    List<int> HoleIndices;
    void resetindices() {
        SpikeIndices = new List<int>();
        HoleIndices = new List<int>();
    }

    string[] parts;
    Vector3Int testv;
    public bool IsSpike(Vector3 WorldPointToTest) {
        return IsType(WorldPointToTest, "spike");
    }

    public bool IsHole(Vector3 WorldPointToTest) {
        return IsType(WorldPointToTest, "hole",TMM_Layer.HoleTilemap);
    }
    public bool IsNull(Vector3 WorldPointToTest, TMM_Layer layer= TMM_Layer.Tilemap) {
        testv = grid.WorldToCell(WorldPointToTest);
        if (layer == TMM_Layer.HoleTilemap) {
            if (!hasHoleTilemap) return false;
            return HoleTilemap.GetTile(testv) == null;
        }
        return CollisionTilemap.GetTile(testv) == null;
    }


    public enum TMM_Layer { Tilemap, Tilemap2, Tilemap3, HoleTilemap, TilemapFG, TilemapWall}

    public bool IsHoleViaName(string tilename) {
        string type = "hole";
        parts = tilename.Split(new char[] { '_' });
        int index;
        if (!int.TryParse(parts[parts.Length - 1],out index)) {
            return false;
        }
        string tilemapname = "";
        for (int i = 0; i < parts.Length - 1; i++) {
            tilemapname += parts[i];
            if (i < parts.Length - 2) tilemapname += "_";
        }
        if (TilemapMetadatas[tilemapname][type].Contains(index)) return true;
        return false;
    }

    public bool IsWater(Vector3 WorldPointtoTest) {
        testv = grid.WorldToCell(WorldPointtoTest);
        _TileBase_check = CollisionTilemap.GetTile(testv);
        if (_TileBase_check == null) return false;
        string prefix = "";
        string index_s = "";
        string prefix2 = "";
        int index = 0;
        string tilename = _TileBase_check.name;
        int m = 0;
        for (int i = 0; i < _TileBase_check.name.Length; i++) {
            if (m == 0) {
                if (tilename[i] == '_') {
                    m = 1;
                } else {
                    prefix += tilename[i];
                }
            } else if (m == 1) {
                if (tilename[i] == '_') {
                    m = 2;
                } else {
                    prefix2 += tilename[i];
                }
            } else if (m == 2) {
                index_s += tilename[i];
            }
        }
        if (prefix == "Waterfall") {
            if (prefix2 == "above") return true;
            return false;
        }
        index = int.Parse(index_s, System.Globalization.CultureInfo.InvariantCulture);

        if (prefix == "debug") {
            if (index == 15) {
                return true;
            }
        } else if (prefix == "AnimTile") {
            if (prefix2 == "Dock" || prefix2 == "Ocean") {
                return true;
            }
        } else if (prefix == "fantasy" && ((index >= 70 && index <= 79) || (index >= 90 && index <= 99) || (index >= 110 && index <= 119))) {
            if (index != 114) return true;
        } else if (prefix == "ocean" && ((index % 20 >= 10 && index < 80) || (index >= 108 && index <= 110))) {
            return true;
        }

            return false;
    }

    TileBase _TileBase_check;
    Tilemap tilemapCheckRef;
    bool IsType(Vector3 WorldPointToTest, string type, TMM_Layer layer=TMM_Layer.Tilemap) {
        testv = grid.WorldToCell(WorldPointToTest);
        if (layer == TMM_Layer.Tilemap) tilemapCheckRef = CollisionTilemap;
        if (layer == TMM_Layer.HoleTilemap) {
            if (!hasHoleTilemap) return false;
            tilemapCheckRef = HoleTilemap;
        }
        _TileBase_check = tilemapCheckRef.GetTile(testv);
        if (_TileBase_check == null) return false;
        string tilename = _TileBase_check.name;

        // OPTIMIZATION: hand parse with a loop to avoid split

        // "debug_tilemap_1" -> debug,tilemap,1
        parts = tilename.Split(new char[] { '_' });
        // get "1"
        int index = 0;
        if (!int.TryParse(parts[parts.Length - 1], out index)) {
            return false;
        }
        string tilemapname = "";
        // get "debug_tilemap"
        for (int i = 0; i < parts.Length - 1; i++) {
            tilemapname += parts[i];
            if (i < parts.Length - 2) tilemapname += "_";
        }
        if (TilemapMetadatas[tilemapname][type].Contains(index)) return true;
        return false;

    }

    TileBase recentHoleTilebase;

    public void restoreHoleTiles(List<Vector3> removedPositions) {
        foreach (Vector3 v in removedPositions) {
            if (v.z == -1) continue;
            Vector3Int cell = grid.WorldToCell(v);
            HoleTilemap.SetTile(cell, recentHoleTilebase);
        }
    }

    // returns a list of removed holes for use in positioning sprites
    public List<Vector3> RemoveHolesForSkeligumBridge(Vector3 gumBlockPos, AnoControl2D.Facing dir, Vector2Int playerRoomPos) {

        Vector3Int startCell = grid.WorldToCell(gumBlockPos);
        Vector3Int offset = new Vector3Int();
        if (dir == AnoControl2D.Facing.UP) {
            offset.y = 1;
        } else if (dir == AnoControl2D.Facing.DOWN) {
            offset.y = -1;
        } else if (dir == AnoControl2D.Facing.RIGHT) {
            offset.x = 1;
        } else if (dir == AnoControl2D.Facing.LEFT) {
            offset.x = -1;
        }
        List<Vector3> removedHolePositions = new List<Vector3>();
        Vector3 cellWorldPos = new Vector3();
        for (int i = 0; i < 4; i++) {
            startCell += offset;
            cellWorldPos = grid.GetCellCenterWorld(startCell);
            if (startCell.x % 12 == 0 || startCell.x % 12 == 11 || startCell.x % 12 == -1) continue;
            if (startCell.y % 12 == 0 || startCell.y % 12 == 11 || startCell.y % 12 == -1) continue;
            if (HF.IsInRoom(cellWorldPos, playerRoomPos.x, playerRoomPos.y)) {
                if (HoleTilemap.GetTile(startCell) != null) {
                    recentHoleTilebase = HoleTilemap.GetTile(startCell);
                    HoleTilemap.SetTile(startCell, null);
                    removedHolePositions.Add(cellWorldPos);
                } else {
                    cellWorldPos.z = -1;
                    removedHolePositions.Add(cellWorldPos);
                }
            }
        }
        return removedHolePositions;
    }

}
