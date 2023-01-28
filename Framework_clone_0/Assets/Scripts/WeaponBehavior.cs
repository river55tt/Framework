using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Rendering;
public class WeaponBehavior : MonoBehaviour
{
    public bool hitscan;
    public GameObject projectile;
    public Color projectilecolor;
    public float bulletspeed = 20f;

    public GaussianDistribution gauss;
    public float damage = 1f;
    public float armorpen;
    public float range = 1f; //explosive aoe
    public float fireRate = 1f;
    public float spread;
    public float cost;

    public int teamid = 0;
    public int weaponid;
    int mask = (1 << 6) | (1 << 7) | (1 << 8) | (1 << 10) | (1 << 12); //6 ground, 7 player, 8 bp, 10 building, 12 shield

    //shotgun;
    public float force;
    public int bullets;

    //explosive
    public bool explodeoncontact;
    //public float explosionradius;
    //public float explosionforce;
    public float timer;

    public Transform gun;
    public EntityProperties owner;

    public float nextFire;



   // public Vector3 testaimtarget;
   // public Vector3 testraydirection;
   // public float testdistance;
    // Update is called once per frame

    public void Shoot(Vector3 aimTarget)
    {
        float availablepower = owner.power;
        if(owner.contain != null && owner.contain.network != null)
        {
            availablepower = owner.contain.network.power;
        }
        if (Time.time >= nextFire && availablepower >= cost)
        {
            nextFire = Time.time + 1f / fireRate;
            owner.power -= cost;
            float totaldamage = 0;
            if (hitscan)
            {
                //CmdBullet(Vector3 position, List < Quaternion > rotation, List<float> distance, int weapon)
                List<Quaternion> bulletrotation = new List<Quaternion>();
                List<float> bulletdistance = new List<float>();

                List<int> entityIDs = new List<int>();
                List<float> damagelist = new List<float>();
                List<float> armorpenlist = new List<float>();
                List<Vector3> forcelist = new List<Vector3>();
                List<List<int>> partindexes = new List<List<int>>();
                List<bool> deadlist = new List<bool>();

                //List<Quaternion> rotation = new List
                //Debug.Log(gauss.Next());
                for (int i = bullets; i > 0; i--)
                {
                    RaycastHit hit;
                    float xrot = gauss.Next(0, .25f);
                    float zrot = gauss.Next(0, .25f);
                    float yrot = gauss.Next(0, .25f);
                    float distanceshot;
                    Vector3 raycastdirection = Vector3.Normalize(aimTarget -gun.position);
                    //raycastdirection = Quaternion.AngleAxis(xrot * spread, Vector3.right) * Quaternion.AngleAxis(zrot * spread, Vector3.up) * raycastdirection;
                    //raycastdirection = Quaternion.AngleAxis(xrot * spread, Vector3.right) + raycastdirection.y * (yrot * spread);
                    raycastdirection = Quaternion.Euler(xrot * spread, yrot * spread, zrot * spread) * raycastdirection;
                    //Debug.Log(raycastdirection);
                    if (Physics.Raycast(gun.position, raycastdirection, out hit, range))
                    {
                        EntityProperties entityproperties = hit.collider.GetComponentInParent<EntityProperties>();
                        //Debug.Log(hit.collider.transform.name);
                        if (entityproperties != null && entityproperties != owner)
                        {
                            entityproperties.TakeDamage(damage, armorpen, raycastdirection.normalized * force, hit.collider.gameObject, false, out List<int> partindex, out bool dead);

                            entityIDs.Add(entityproperties.entityID);
                            damagelist.Add(damage);
                            armorpenlist.Add(armorpen);
                            forcelist.Add(raycastdirection.normalized * force);
                            partindexes.Add(partindex);
                            deadlist.Add(dead);

                            totaldamage += damage;
                        }

                        distanceshot = hit.distance + .05f;
                    }
                    else
                    {
                        distanceshot = range;
                    }

                    bulletrotation.Add(Quaternion.LookRotation(raycastdirection, Vector3.up));
                    bulletdistance.Add(distanceshot);
                   /* GameObject shotprojectile = Instantiate(projectile, gun.transform.position, Quaternion.LookRotation(gun.transform.forward, gun.transform.up));
                    shotprojectile.transform.Rotate(new Vector3(xrot, 0, zrot) * spread);
                    shotprojectile.transform.position += -shotprojectile.transform.up * distanceshot;
                    shotprojectile.transform.localScale = new Vector3(400, 100 * distanceshot / .01f, 400);
                    shotprojectile.GetComponent<Renderer>().material.color = projectilecolor;
                    shotprojectile.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
                    shotprojectile.GetComponent<Renderer>().receiveShadows = false;
                    HitscanBullet projectilescript = shotprojectile.AddComponent<HitscanBullet>();
                    projectilescript.size = 100 * distanceshot / .01f;
                    projectilescript.speed = bulletspeed * 10000;
                    projectilescript.bullet = shotprojectile.transform;
                    projectilescript.bulletobject = shotprojectile;*/


                    GameObject shotprojectile = Instantiate(projectile, gun.transform.position, Quaternion.LookRotation(raycastdirection, Vector3.up));
                    shotprojectile.transform.Rotate(-90,0,0);
                    shotprojectile.transform.position += -shotprojectile.transform.up * distanceshot;
                    shotprojectile.transform.localScale = new Vector3(400, 100 * distanceshot / .01f, 400);
                    shotprojectile.GetComponent<Renderer>().material.color = projectilecolor;
                    shotprojectile.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
                    shotprojectile.GetComponent<Renderer>().receiveShadows = false;
                    HitscanBullet shotscript = shotprojectile.AddComponent<HitscanBullet>();
                    shotscript.size = 100 * distanceshot / .01f;
                    shotscript.speed = bulletspeed * 10000;
                    shotscript.bullet = shotprojectile.transform;
                    shotscript.bulletobject = shotprojectile;
                }

                //tempbuilding.TakeDamage(tempbuilding.maxhealth / 2, 100f, Vector3.zero, null, false, out List<int> partindex, out bool dead);

                NetworkClient.localPlayer.GetComponent<PlayerNetwork>().CmdBullet(owner.entityID, bulletrotation, bulletdistance, weaponid);
                NetworkClient.localPlayer.GetComponent<PlayerNetwork>().CmdDamageObject(entityIDs, damagelist, armorpenlist, forcelist, partindexes, deadlist);
            }
            else
            {
                Vector3 raycastdirection = aimTarget - gun.position;
                GameObject shotprojectile = Instantiate(projectile, gun.transform.position, Quaternion.LookRotation(raycastdirection, Vector3.up));
                Rigidbody projectileRB = shotprojectile.GetComponent<Rigidbody>();
                //projectileRB.velocity = Quaternion.LookRotation(aimTarget, Vector3.up).eulerAngles.normalized * bulletspeed);
                projectileRB.velocity = projectileRB.transform.forward * bulletspeed;

                //Debug.Log(aimTarget);
                //Debug.Log(Quaternion.LookRotation(aimTarget, Vector3.up).eulerAngles);

                Projectile shotscript = shotprojectile.GetComponent<Projectile>();
                shotscript.teamid = teamid;
                shotscript.mask = mask;
                shotscript.timer = timer;
                shotscript.explodeoncontact = explodeoncontact;
                shotscript.projectileRB = projectileRB;

                Explosion explosionscript = shotprojectile.GetComponent<Explosion>();
                explosionscript.teamid = teamid;
                explosionscript.mask = mask;
                explosionscript.explosionradius = range;
                explosionscript.explosionforce = force;
                explosionscript.damage = damage;
                explosionscript.armorpen = armorpen;


                //shotprojectile.transform.Rotate(-90, 0, 0);
            }
            //Debug.Log(totaldamage);
        }
       /* if (Input.GetKey(KeyCode.Alpha4))
        {


            //for (int i = bullets; i > 0; i--)
            //{
            RaycastHit hit;
            float xrot = gauss.Next(0, .25f);
            float zrot = gauss.Next(0, .25f);
            testaimtarget = aimTarget;
            testraydirection = testaimtarget - gun.transform.position;
            testraydirection = Quaternion.AngleAxis(xrot * spread, Vector3.right) * Quaternion.AngleAxis(zrot * spread, Vector3.up) * testraydirection;
            //Debug.Log(raycastdirection);
            if (Physics.Raycast(gun.transform.position, testraydirection, out hit, range))
            {
                testdistance = hit.distance;
            }
            else
            {
                testdistance = range;
            }
            //Debug.DrawLine(gun.transform.position, gun.transform.position + testraydirection * testdistance, Color.magenta);
            //}
        }*/
    }

    private void OnDrawGizmos()
    {
        //Debug.DrawLine(gun.transform.position, gun.transform.position + testraydirection * testdistance, Color.magenta);
    }

}










/*if (Physics.Raycast(gun.transform.position, raycastdirection, out hit, range))
{
    EntityProperties entityproperties = hit.transform.GetComponentInParent<EntityProperties>();
    if (entityproperties != null)
    {
        entityproperties.TakeDamage(damage, 0);
        totaldamage += damage;
    }

    distanceshot = hit.distance;
    /* GameObject shotprojectile = Instantiate(projectile, gun.transform.position, Quaternion.LookRotation(gun.transform.forward, gun.transform.up));
     //GameObject shotprojectile = Instantiate(projectile);
     //shotprojectile.transform.Rotate(new Vector3(gauss.Next(0, .25f),0, gauss.Next(0, .25f))*spread);
     shotprojectile.transform.Rotate(new Vector3(xrot, 0, zrot) * spread);
     shotprojectile.transform.position += -shotprojectile.transform.up * hit.distance;
     shotprojectile.transform.localScale = new Vector3(400, 100 * hit.distance / .01f, 400);
     shotprojectile.GetComponent<Renderer>().material.color = projectilecolor;
     HitscanBullet projectilescript = shotprojectile.AddComponent<HitscanBullet>();
     projectilescript.size = 100 * hit.distance / .01f;
     projectilescript.speed = bulletspeed * 10000;
     projectilescript.bullet = shotprojectile.transform;
     projectilescript.bulletobject = shotprojectile;


else {
    /*GameObject shotprojectile = Instantiate(projectile, gun.transform.position, Quaternion.LookRotation(gun.transform.forward, gun.transform.up));
    //shotprojectile.transform.Rotate(new Vector3(gauss.Next(0, .25f), 0, gauss.Next(0, .25f)) * spread);
    shotprojectile.transform.Rotate(new Vector3(xrot, 0, zrot) * spread);
    shotprojectile.transform.position += -shotprojectile.transform.up * range;
    shotprojectile.transform.localScale = new Vector3(400, 100 * range / .01f, 400);
    shotprojectile.GetComponent<Renderer>().material.color = projectilecolor;
    HitscanBullet projectilescript = shotprojectile.AddComponent<HitscanBullet>();
    projectilescript.size = 100 * range / .01f;
    projectilescript.speed = bulletspeed * 10000;
    projectilescript.bullet = shotprojectile.transform;
    projectilescript.bulletobject = shotprojectile;

}*/