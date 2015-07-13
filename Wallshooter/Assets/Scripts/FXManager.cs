using UnityEngine;
using System.Collections;

public class FXManager : MonoBehaviour {

    public GameObject sniperBulletFXPrefab;
    public GameObject bloodSplatPrefab;
	
    [PunRPC]
    void SniperBulletFX(Vector3 startPos, Vector3 endPos)
    {
        if (sniperBulletFXPrefab != null)
        {


            GameObject sniperFX = (GameObject)Instantiate(sniperBulletFXPrefab, startPos, Quaternion.LookRotation(endPos - startPos));
            LineRenderer lr = sniperFX.transform.Find("LineFX").GetComponent<LineRenderer>();
            if (lr != null)
            {
                lr.SetPosition(0, startPos);
                lr.SetPosition(1, endPos);
                //AudioSource.PlayClipAtPoint(sniperBulletFXAudio, startPos);

            }
            else
            {
                Debug.LogError("Sniper bullet prefab had no line renderer!");
            }
        }
        else
        {
            Debug.LogError("No sniper bullet FX prefab found!");
        }
    }

    [PunRPC]
    void BloodFX(Vector3 position) {
        if (bloodSplatPrefab != null) {
            Instantiate(bloodSplatPrefab, position, Quaternion.identity);
        } else {
            Debug.LogError("Expected blood prefab!");
        }
    }

    
}
