using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneData2D : MonoBehaviour {


    // Really, this exists for debugging. In game, entering a nanopoint would set this.
    public int RoomSizeSetInEditor = 12;
    // Other scripts can access this, contains most recent roomsize
    public static bool RoomSizeWasSetSomewhereElse = false;
    public static int RoomSize_X = 12; // currently rooms are square so Y does nothing
    public static int RoomSize_Y = 12;

    //public string npcDefaultTint = "FFFFFFFF";
    //public string npcAltTint1 = "FFFFFFFF";
    //public string npcAltTint2 = "FFFFFFFF";


    [TextArea(5,15)]
    public string gameMinimapCSV = "";
    public Vector2Int topLeftRoomCoordinates = new Vector2Int(0, 0);
    public bool hasMinimap = false;
    public bool halfSizeCam = false;

    // Only called when entering a Door object.
    // Otherwise, the room data size is set via the scene data object
    public static void SetRoomSize(int roomsize_x, int roomsize_y) {
        RoomSize_X = roomsize_x;
        RoomSize_Y = roomsize_y;
        RoomSizeWasSetSomewhereElse = true;
    }

    private void Awake() {
        Ano2Dust.initdusttable();
        if (RoomSizeWasSetSomewhereElse) {
            RoomSizeWasSetSomewhereElse = false;
        } else {
            RoomSize_X = RoomSizeSetInEditor;
            RoomSize_Y = RoomSizeSetInEditor;
        }
    }
}
