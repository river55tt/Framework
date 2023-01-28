using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserStrike : MonoBehaviour
{
    public static LaserStrike LSManager;
    public GameObject Laser;
    public GameObject Explosion;
    private GaussianDistribution gauss;
    public float spread;
    public float length;

    int mask = (1 << 6) | (1 << 7) | (1 << 8) | (1 << 10) | (1 << 12); //6 ground, 7 player, 8 bp, 10 building, 12 shield

    //public Vector3 fireposition;
    /* public float damage;
     public float radius;
     public int totalstrikes;
     public float separation;*/
    // Start is called before the first frame update

    // StartCoroutine(ClientTerrain(connectionToServer));
    void Awake()
    {
        if (LSManager == null)
        {
            LSManager = this;
            gauss = GetComponent<GaussianDistribution>();
            //StartCoroutine(Test());
            //Debug.Log("Initialized Power Manager");
        }
    }

    IEnumerator Test()
    {
        Debug.Log("waiting");
        yield return new WaitUntil(() => Mirror.NetworkClient.localPlayer != null);
        yield return new WaitUntil(() => Mirror.NetworkClient.localPlayer.GetComponentInChildren<EntityProperties>() != null);
        Transform player = Mirror.NetworkClient.localPlayer.GetComponentInChildren<EntityProperties>().GetComponentInChildren<Rigidbody>().transform;
        Debug.Log("waiting again");
        yield return new WaitForSeconds(5f);
        while (true)
        {
            StartCoroutine(InitiateLaserStrike(player.position, 400f, 10f, 4,  1.2f));
            yield return new WaitForSeconds(8f);
        }
    }

    IEnumerator InitiateLaserStrike(Vector3 position, float damage, float radius, int strikenumber, float separation)
    {
        for (int i = 0; i < strikenumber; i++)
        {
            //Instantiate
            RaycastHit hit;
            float xrot = gauss.Next(0, .25f);
            float zrot = gauss.Next(0, .25f);
            float yrot = gauss.Next(0, .25f);
            Vector3 raycastdirection = Vector3.down;
            //raycastdirection = Quaternion.AngleAxis(xrot * spread, Vector3.right) * Quaternion.AngleAxis(zrot * spread, Vector3.up) * raycastdirection;
            //Vector3 secondraycast = Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.forward) * Quaternion.AngleAxis(xrot * spread, Vector3.up) * Vector3.down;
            Vector3 secondraycast = Quaternion.AngleAxis(zrot * spread, Vector3.forward) * Quaternion.AngleAxis(xrot * spread, Vector3.right) * Vector3.up;
            //Debug.Log(zrot * spread);
            //Debug.Log(secondraycast);
            raycastdirection = Quaternion.Euler(xrot * spread, yrot * spread, zrot * spread) * raycastdirection;
            //Debug.Log(raycastdirection);
            //Debug.Log(position + raycastdirection * length);
            raycastdirection = secondraycast;
            //Debug.Log(secondraycast);

            Debug.DrawRay(position + raycastdirection * length, -raycastdirection * 400, Color.red, 2f);

            if (Physics.Raycast(position + raycastdirection * length, -raycastdirection, out hit, 1000f))
            {
                GameObject strike = Instantiate(Laser, hit.point - raycastdirection * .2f, Quaternion.LookRotation(raycastdirection), null);
                strike.transform.Rotate(90f, 0, 0, Space.Self);
                strike.transform.localScale = new Vector3(12500, 100 * hit.distance / .01f, 12500);
                Explosion explosionscript = strike.GetComponentInChildren<Explosion>();
                explosionscript.damage = damage;
                explosionscript.armorpen = 15f;
                explosionscript.explosionforce = 600;
                explosionscript.explosionradius = radius;
                explosionscript.teamid = -1;
                explosionscript.mask = mask;
                explosionscript.Explode(hit.point + hit.normal * .2f, hit.collider.GetComponentInParent<EntityProperties>());

                StartCoroutine(FireStrike(strike));
                //Instantiate(Explosion, hit.point, Quaternion.identity, null);
            }
            else
            {
                Debug.LogWarning("Missed Laser Strike!");
            }
            //GameObject laser = 
            //Instantiate(Explosion, transform.position, Quaternion.identity, null);
            yield return new WaitForSeconds(separation);
            //return null;
        }
        yield return null;
    }

    IEnumerator FireStrike(GameObject strike)
    {
        float timeleft = .3f;
        float timepassed = timeleft;
        yield return new WaitForSeconds(.15f);
        while (timepassed > 0)
        {
            yield return null;
            timepassed -= Time.deltaTime;
            float scaleportion = 12500 * timepassed / timeleft;
            strike.transform.localScale = new Vector3(scaleportion, strike.transform.localScale.y, scaleportion);
        }
        Destroy(strike);
    }
}
