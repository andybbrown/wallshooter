using UnityEngine;
using System.Collections;

public class TeamMember : MonoBehaviour {

    private int _teamID = 0;

    public int teamID {
        get { return _teamID; }
    }

    [PunRPC]
    void SetTeamID(int id) {
        _teamID = id;


        SkinnedMeshRenderer mySkin = this.transform.GetComponentInChildren<SkinnedMeshRenderer>();
        if (mySkin == null) {
            Debug.LogError("No skin was found on player!");
        } else {
            if (_teamID == 1)
                mySkin.material.color = Color.red;
            if (_teamID == 2)
                mySkin.material.color = new Color(.5f, 1f, .5f);
        }
    }
}
