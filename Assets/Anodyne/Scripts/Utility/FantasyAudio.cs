using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FantasyAudio : MonoBehaviour {

    // -2,5 is top

    string songsString = "000810910331\n061112210331\n066111113331\n066111110001\n166010000000\n100110001111\n601110011554\n000000000550\n777006660000\n777006660001\n011100111111";
    int[,] songIndices = new int[11,12];
    Vector2Int roomPos = new Vector2Int();
    AnoControl2D player;
    AudioHelper ah;
	void Start () {
        HF.GetPlayer(ref player);
        mode = 0;
        ah = AudioHelper.instance;
        string[] parts = songsString.Split('\n');
        int idx = 0;
        foreach (string part in parts) {
            for (int i = 0;  i < part.Length; i++) {
                songIndices[idx,i] = int.Parse(part[i].ToString(), System.Globalization.CultureInfo.InvariantCulture);
            }
            idx++;
        }
	}
    Vector2Int tempRoomPos = new Vector2Int();

    float tWait = 0;
    int mode = 1;
	void Update () {
		if (mode == 0) {
            tWait += 0.0167f;
            if (tWait > 0.1f) {
                tWait = 0;
                mode = 1;
            }
        } else if (mode == 1) {
            if (DataLoader.instance.isChangingScenes) {
                mode = 1000;
                return;
            }
            // after entered new screen
            HF.GetRoomPos(player.transform.position, ref roomPos);
            // player room x goes from -2 to 9
            // room y from 5 to -5
            roomPos.x += 2;
            roomPos.y -= 5; roomPos.y *= -1;
            // room pos is now in [0,11] (column) and [0,10] (row)
            //print(roomPos.x + "," + roomPos.y);
            bool needToSwitchSong = true;
            bool playerOnRaft = player.ridingRaft;
            int songID = 0;
            if (roomPos.y >= 11 || roomPos.y < 0 || roomPos.x < 0 || roomPos.x >= 12) {
                songID = 1;
            } else {
                songID = songIndices[roomPos.y, roomPos.x];
            }
            
            if (playerOnRaft) {
                if (ah.IsSongPlaying("fant-ocean")) {
                    if (songID == 6 && roomPos.y >= 7) {
                        // Switch song to dustdra area
                    } else {
                        needToSwitchSong = false;
                    }
                } else {
                    needToSwitchSong = false;
                    if (ah.IsSongPlaying("fant-slime") && roomPos.y >= 7) {

                    } else {
                        ah.StopAllSongs();
                        ah.PlaySong("fant-ocean", 0, 0);
                    }
                }
            }
            if (needToSwitchSong) {
                string songToPlay = "";
                switch (songID) {
                    case 0:
                        // do nothing
                        break;
                    case 1:
                        songToPlay = "fant-overworld";
                        break;
                    case 2:
                        songToPlay = "fant-castle";
                        break;
                    case 3:
                        songToPlay = "fant-crags";
                        break;
                    case 4:
                        songToPlay = "fant-kin";
                        break;
                    case 5:
                        songToPlay = "fant-kinisle";
                        break;
                    case 6:
                        songToPlay = "fant-slime";
                        break;
                    case 7:
                        songToPlay = "fant-woods";
                        break;
                    case 8:
                        if (DataLoader._getDS("fant-yolk-LOOPCOUNT") >= 1) {
                            songToPlay = "fant-yolk";
                        } else {
                            songToPlay = "fant-overworld";
                        }
                        break;
                    case 9:
                        if (DataLoader._getDS("fant-met-arteri") == 1) {
                            songToPlay = "fant-castle";
                        }
                        // castle, after talking to arteri once
                        break;
                }
                if (songToPlay != "" && ah.IsSongPlaying(songToPlay) == false) {
                    ah.StopAllSongs();
                    ah.PlaySong(songToPlay, 0, 0);
                }
            }
            mode = 2;
        } else if (mode == 2) {
            HF.GetRoomPos(player.transform.position, ref tempRoomPos);

            tempRoomPos.x += 2;
            tempRoomPos.y -= 5; tempRoomPos.y *= -1;
            if (tempRoomPos.x != roomPos.x || tempRoomPos.y != roomPos.y) {
                mode = 1;
            }
        }
	}
}
