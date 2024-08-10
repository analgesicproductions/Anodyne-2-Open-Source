using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Anodyne;
public class PicoMover : MonoBehaviour {

    public int movementOffset = 0;
    [TextArea(5,20)]
    public string movementCode = "l1\nu2\nd2";
    float tileSize = 0.25f;
    public float moveInterval = 0.25f;
    Vacuumable vac;
    // pos in half grids from origin
    Vector2 initPos = new Vector2();
    Vector2 tempPos = new Vector2();
    List<string> movements = new List<string>();
    int movementIdx = 0;
    int yOff = 0;
    int xOff = 0;
	void Start () {
        initPos = transform.position;
        vac = GetComponent<Vacuumable>();
        RefreshMovements();
        HF.GetPlayer(ref player);
	}

    void RefreshMovements() {
        transform.position = initPos;
        tMove = 0f;
        movementCode = movementCode.Replace("\r", "");
        string[] movementArray = movementCode.Split('\n');
        movements.Clear();

        List<string> rawMovementList = new List<string>();
        foreach (string s in movementArray) {
            rawMovementList.Add(s);
        }
        for (int i = 0; i < movementOffset; i++) {
            string s = rawMovementList[0];
            rawMovementList.RemoveAt(0);
            rawMovementList.Add(s);
        }

        xOff = yOff = 0;
        // uN, ..., cw/ccLENGTH (expands to urdl.. or uldr
        foreach (string s in rawMovementList) {
            if (s.Length <= 0) continue;
            string moveType = s[0].ToString();
            if (moveType == "c") {
                string length = s[2].ToString();
                if (s[1] == 'c') { // counterClockwise
                    addMovements("u", length);
                    addMovements("l", length);
                    addMovements("d", length);
                    addMovements("r", length);
                } else { // clockwise
                    addMovements("u", length);
                    addMovements("r", length);
                    addMovements("d", length);
                    addMovements("l", length);
                }
            } else {
                string length = "";
                     
                if (s.Length > 2) {
                    length = s.Substring(1, 2);
                } else {
                   length = s[1].ToString();
                }
                addMovements(moveType, length);
            }
        }
        movementIdx = 0;
    }

    public bool suckingStopsThis = true;
    void addMovements(string type, string times) {

        for (int i = 0; i < int.Parse(times,System.Globalization.CultureInfo.InvariantCulture); i++) {
            movements.Add(type);
        }
    }

    float tRefresh = 0f;
    private void OnValidate() {
        if (Application.isPlaying && Application.isEditor) {
            //tRefresh = 1.2f;
        }
    }

    float tMove = 0f;
    AnoControl2D player;
    bool stopMovingTillLeaveRoom = false;
	void Update () {
      
        if (stopMovingTillLeaveRoom) {
            if (!player.InSameRoomAs(transform.position)) {
                stopMovingTillLeaveRoom = false;
            }
            return;
        }
        
        if (player.InSameRoomAs(transform.position) == false) {
            if (xOff != 0 || yOff != 0 || movementIdx == 0) {
                tMove = 0;
                movementIdx = 0;
                xOff = yOff = 0;
                transform.position = initPos;
            }
            return;
        }
        if (player.IsThereAReasonToPause()) return;
        if (suckingStopsThis && vac.IsBeingSucked() == true) {
            stopMovingTillLeaveRoom = true;
            return;
        }

        if (tRefresh > 0) {
            tRefresh -= Time.deltaTime;
            if (tRefresh <= 0) {
                RefreshMovements();
            }
        }

        tMove += Time.deltaTime;
        if (tMove >= moveInterval) {
            tMove -= moveInterval;
            string movement = movements[movementIdx];
            movementIdx++; if (movementIdx >= movements.Count) movementIdx = 0;
            if (movement == "u") {
                yOff++;
            } else if (movement == "d") {
                yOff--;
            } else if (movement == "r") {
                xOff++;
            } else if (movement == "l") {
                xOff--;
            } else if (movement == "w") { // wait
                return;
            }
            tempPos = initPos;
            tempPos.x += xOff * tileSize;
            tempPos.y += yOff * tileSize;
            transform.position = tempPos;
        }
	}

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.name == Registry.PLAYERNAME2D && vac != null && vac.isIdle()) {
            collision.GetComponent<AnoControl2D>().Damage(1);
        }
    }
}
