using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NetworkManager : MonoBehaviour {

	public GameObject standbyCamera;
	SpawnSpot[] spawnSpots;
	
	public bool offlineMode = false;

    private bool connecting = false;

    private List<string> chatMessages;
    private int maxChatMessages = 5;

    public float respawnTimer = 0;

    private bool hasPickedTeam = false;

    private int currentTeam = 0;

    public Transform botSpawnpoint;

    public float botSpawnTimer = 0f;

	// Use this for initialization
	void Start () {
		spawnSpots = GameObject.FindObjectsOfType<SpawnSpot>();
        PhotonNetwork.player.name = PlayerPrefs.GetString("Username", "Recruit");
        chatMessages = new List<string>();

	}

	void Connect() {
		PhotonNetwork.ConnectUsingSettings( "0.15" );
        
	}

    void OnDestroy()
    {
        PlayerPrefs.SetString("Username", PhotonNetwork.player.name);
    }


    public void AddChatMessage(string m)
    {
        GetComponent<PhotonView>().RPC("AddChatMessage_RPC", PhotonTargets.All, m);
    }

    [PunRPC]
    private void AddChatMessage_RPC(string m)
    {
        while (chatMessages.Count >= maxChatMessages)
        {
            chatMessages.RemoveAt(0);
        }
        chatMessages.Add(m);
    }
	 
	void OnGUI() {
		GUILayout.Label( PhotonNetwork.connectionStateDetailed.ToString() );


		if (PhotonNetwork.connected == false && connecting == false) {
            // not yet connected for online / offline mode
            GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();

            
            GUILayout.BeginHorizontal();
            GUILayout.Label("Username: ");
            PhotonNetwork.player.name = GUILayout.TextField(PhotonNetwork.player.name, GUILayout.Width(200), GUILayout.Height(30));
            GUILayout.EndHorizontal();

            /*
			if (GUILayout.Button("Single Player"))
			{
				PhotonNetwork.offlineMode = true;
                offlineMode = true;
				OnJoinedLobby();
                connecting = true;
			} */
			if (GUILayout.Button("Connect to Game"))
			{
				Connect();
                connecting = true;
			}

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
		}


        if ((PhotonNetwork.connected == true && connecting == false) || offlineMode == true)
        {
            

            if (hasPickedTeam) {

                // we are fully connected, make sure to display chat box
                GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
                GUILayout.BeginVertical();
                GUILayout.FlexibleSpace();

                foreach (string msg in chatMessages) {
                    GUILayout.Label(msg);
                }


                GUILayout.EndVertical();
                GUILayout.EndArea();
            } else {
                GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.BeginVertical();
                GUILayout.FlexibleSpace();



                if (GUILayout.Button("Red Team")) {
                    SpawnMyPlayer(1);

                }
                if (GUILayout.Button("Green Team")) {
                    SpawnMyPlayer(2);
                }
                if (GUILayout.Button("Random Team")) {
                    SpawnMyPlayer(Random.Range(1, 3)); // 1 or 2
                }
                //if (GUILayout.Button("Renegade!")) {
                //    SpawnMyPlayer(0);
                //}
                GUILayout.FlexibleSpace();
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.EndArea();
            }
        }
	}

	void OnJoinedLobby() {
		Debug.Log ("OnJoinedLobby");
		PhotonNetwork.JoinRandomRoom();
	}

	void OnPhotonRandomJoinFailed() {
		Debug.Log ("OnPhotonRandomJoinFailed");
		PhotonNetwork.CreateRoom( null );
	}

    void OnCreatedRoom() {
        Debug.Log("Build that wall!");

        PhotonNetwork.Instantiate("Wall", new Vector3(0, 5, 0), Quaternion.identity, 0);
    }

	void OnJoinedRoom() {
		Debug.Log ("OnJoinedRoom");

        connecting = false;

        if (PhotonNetwork.isMasterClient) {
            botSpawnTimer = 3f;
        }
		//SpawnMyPlayer();
	}

	void SpawnMyPlayer(int teamID) {
        this.currentTeam = teamID;
        hasPickedTeam = true;
        AddChatMessage("Spawning Player: " + PhotonNetwork.player.name);



        if(spawnSpots == null) {
			Debug.LogError ("NO SPAWN LOCATIONS?!?!?");
			return;

        }

        List<SpawnSpot> availableSpawns = new List<SpawnSpot>();

        foreach(SpawnSpot s in spawnSpots) {
            if (s.teamId == teamID || teamID == 0) {
                availableSpawns.Add(s);
            }
        }



        SpawnSpot mySpawnSpot = availableSpawns[Random.Range(0, availableSpawns.Count)];
		GameObject myPlayerGO = (GameObject)PhotonNetwork.Instantiate("PlayerController", mySpawnSpot.transform.position, mySpawnSpot.transform.rotation, 0);
		standbyCamera.SetActive(false);

		//((MonoBehaviour)myPlayerGO.GetComponent("FPSInputController")).enabled = true;

        myPlayerGO.GetComponent<MouseLook>().enabled = true;
        myPlayerGO.GetComponent<PlayerController>().enabled = true;
		//((MonoBehaviour)myPlayerGO.GetComponent("PlayerController")).enabled = true;
		myPlayerGO.transform.FindChild("Main Camera").gameObject.SetActive(true);
        // also set color



        // RPC call on the photoview to set the team ID
        myPlayerGO.GetComponent<PhotonView>().RPC("SetTeamID", PhotonTargets.AllBuffered, teamID);


	}

    void spawnBot() {
        if (spawnSpots == null) {
            Debug.LogError("NO SPAWN LOCATIONS?!?!?");
            return;
        }
        SpawnSpot mySpawnSpot = spawnSpots[Random.Range(0, spawnSpots.Length)];

        

        GameObject botGO = (GameObject)PhotonNetwork.Instantiate("BotController", mySpawnSpot.transform.position, mySpawnSpot.transform.rotation, 0);
        ((MonoBehaviour)botGO.GetComponent("BotController")).enabled = true;
    }

    void Update() {
        if (respawnTimer > 0) {
            respawnTimer -= Time.deltaTime;

            if (respawnTimer <= 0) {
                SpawnMyPlayer(currentTeam);
            }
        }

        if (botSpawnTimer > 0) {
            botSpawnTimer -= Time.deltaTime;
            if (botSpawnTimer <= 0) {
                spawnBot();
            }
        }
    }
}
