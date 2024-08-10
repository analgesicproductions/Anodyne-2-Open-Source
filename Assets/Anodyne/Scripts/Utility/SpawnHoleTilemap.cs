using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SpawnHoleTilemap : MonoBehaviour {

    Tilemap tilemap;
    Tilemap holeTilemap;
	// Use this for initialization
	void Start () {

        GetComponent<TilemapRenderer>().enabled = false;
        init = true;

    }



    bool init = false;
    TilemetaManager tmm;
	// Update is called once per frame
	void Update () {

        if (init) return;

        tmm = TilemetaManager.instance;
        init = true;
        holeTilemap = GameObject.Find("HoleTilemap").GetComponent<Tilemap>();
        tilemap = GetComponent<Tilemap>();
        BoundsInt bounds = tilemap.cellBounds;
        TileBase[] allTiles = tilemap.GetTilesBlock(bounds);
        holeTilemap.origin = tilemap.origin;
        holeTilemap.size = tilemap.size;
        holeTilemap.ResizeBounds();

        
        for (int x = 0; x < bounds.size.x; x++) {
            for (int y = 0; y < bounds.size.y; y++) {
                TileBase tile = allTiles[x + y * bounds.size.x];
                if (tile != null) {
                    
                    if (tmm.IsHoleViaName(tile.name)) {
                        Vector3Int pos = new Vector3Int(x+tilemap.cellBounds.position.x, y+tilemap.cellBounds.position.y, 0);
                        holeTilemap.SetTile(pos, tile);
                        tilemap.SetTile(pos, null);
                        tilemap.RefreshTile(pos);
                        holeTilemap.RefreshTile(pos);
                    }
                }
            }
        }
    }
}
