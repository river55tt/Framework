using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Explosion : MonoBehaviour
{
    public float explosionradius;
    public float explosionforce;
    public float damage;
    public float armorpen;
    public int teamid;
    public int mask;
    //public Rigidbody projectileRB;
    public GameObject explosionparticles;
    public void Explode(Vector3 explosionpos, EntityProperties hitobject)
    {
        Debug.Log("Explode");
        transform.position = explosionpos;
        Collider[] hits = Physics.OverlapSphere(explosionpos, explosionradius, mask);
        List<EntityProperties> exposed = new List<EntityProperties>();

        List<int> entityIDs = new List<int>();
        List<float> damagelist = new List<float>();
        List<float> armorpenlist = new List<float>();
        List<Vector3> forcelist = new List<Vector3>();
        List<List<int>> partindexes = new List<List<int>>();
        List<bool> deadlist = new List<bool>();


        if (hitobject != null)
        {
            exposed.Add(hitobject);
            Rigidbody directhitRB = hitobject.GetComponentInParent<Rigidbody>();
            if (!directhitRB.isKinematic)
            {
                directhitRB.velocity = Vector3.ClampMagnitude((directhitRB.position + directhitRB.centerOfMass - explosionpos).normalized * explosionforce / directhitRB.mass, 40); //max speed 40
            }
            hitobject.TakeDamage(damage, armorpen, (directhitRB.position - explosionpos).normalized * explosionforce, null, false, out List<int> partindex, out bool dead);

            entityIDs.Add(hitobject.entityID);
            damagelist.Add(damage);
            armorpenlist.Add(armorpen);
            forcelist.Add((directhitRB.position - explosionpos).normalized * explosionforce);
            partindexes.Add(partindex);
            deadlist.Add(dead);
        }

        if (hits.Length > 0)
        {
            for (int i = 0; i < hits.Length; i++) //all objects in radius
            {
                Ray ray = new Ray(explosionpos, hits[i].transform.position - explosionpos);
                Ray[] raysides = new Ray[7];
                raysides[0] = new Ray(explosionpos, new Vector3(hits[i].bounds.center.x, hits[i].bounds.center.y, hits[i].bounds.center.z) - explosionpos);

                raysides[1] = new Ray(explosionpos, new Vector3(hits[i].bounds.center.x + hits[i].bounds.extents.x, hits[i].bounds.center.y, hits[i].bounds.center.z) - explosionpos);
                raysides[2] = new Ray(explosionpos, new Vector3(hits[i].bounds.center.x - hits[i].bounds.extents.x, hits[i].bounds.center.y, hits[i].bounds.center.z) - explosionpos);

                raysides[3] = new Ray(explosionpos, new Vector3(hits[i].bounds.center.x, hits[i].bounds.center.y + hits[i].bounds.extents.y, hits[i].bounds.center.z) - explosionpos);
                raysides[4] = new Ray(explosionpos, new Vector3(hits[i].bounds.center.x, hits[i].bounds.center.y - hits[i].bounds.extents.y, hits[i].bounds.center.z) - explosionpos);

                raysides[5] = new Ray(explosionpos, new Vector3(hits[i].bounds.center.x, hits[i].bounds.center.y, hits[i].bounds.center.z + hits[i].bounds.extents.z) - explosionpos);
                raysides[6] = new Ray(explosionpos, new Vector3(hits[i].bounds.center.x, hits[i].bounds.center.y, hits[i].bounds.center.z - hits[i].bounds.extents.z) - explosionpos);


                for (int r = 0; r < raysides.Length; r++) //all sides of object
                {
                    ray = raysides[r];
                    //Debug.DrawLine(explosionpos, (hits[i].transform.position - explosionpos).normalized * explosionradius + explosionpos, Color.red, 2.0f);

                    //Debug.DrawLine(explosionpos, explosionpos + ray.direction.normalized * explosionradius, Color.red, 2.0f);

                    RaycastHit shieldtest;
                    float rangeleft = explosionradius;
                    Vector3 lastposition = explosionpos;
                    //Physics.Raycast(lastposition, ray.direction, out shieldtest, rangeleft, mask);

                    int x = 10;
                    while (rangeleft > 0 && x > 0) //keeps going until something hit or no range
                    {
                        x--;
                        if (x == 0)
                        {
                            Debug.LogWarning("WARNING: TOO MANY ITERATIONS");
                            break;
                        }

                        if (!Physics.Raycast(lastposition + ray.direction * .005f, ray.direction, out shieldtest, rangeleft, mask))
                        {
                            rangeleft = 0;
                            break;
                        }
                        rangeleft -= shieldtest.distance;
                        lastposition = shieldtest.point;
                        if (shieldtest.collider.gameObject.layer != 12 || shieldtest.collider.GetComponentInParent<EntityProperties>().teamid != teamid)
                        {
                            Debug.DrawLine(explosionpos, shieldtest.point, Color.red, 2.0f);
                            EntityProperties adding = shieldtest.collider.GetComponentInParent<EntityProperties>();
                            if (adding.dead)
                            {
                                //Debug.Log("continue: " + adding.health);
                                continue;
                            }
                            if (!exposed.Contains(adding))
                            {
                                float intensity = (-.2f + rangeleft / explosionradius);
                                if (intensity < 0) intensity = 0;

                                exposed.Add(adding);
                                //Debug.Log(adding.gameObject);
                                //Debug.Log(damage * intensity);
                                //Debug.Log((hits[i].ClosestPoint(explosionpos) - explosionpos).normalized * explosionforce * intensity);
                                //Debug.Log(((explosionpos - hits2[j].point).normalized * explosionforce * intensity).magnitude);
                                Rigidbody tempRB = adding.GetComponentInParent<Rigidbody>();
                                if (!tempRB.isKinematic)
                                {
                                    tempRB.velocity = Vector3.ClampMagnitude((tempRB.position + tempRB.centerOfMass - explosionpos).normalized * explosionforce * intensity / tempRB.mass, 40); //max speed 40
                                }
                                adding.TakeDamage(damage * (intensity + .2f), armorpen, (shieldtest.point - explosionpos).normalized * explosionforce * intensity, shieldtest.collider.gameObject, false, out List<int> partindex, out bool dead);

                                entityIDs.Add(adding.entityID);
                                damagelist.Add(damage * (intensity + .2f));
                                armorpenlist.Add(armorpen);
                                forcelist.Add((shieldtest.point - explosionpos).normalized * explosionforce * intensity);
                                partindexes.Add(partindex);
                                deadlist.Add(dead);

                                //adding.GetComponentInParent<Rigidbody>().AddExplosionForce(explosionforce, explosionpos, explosionradius,1);
                                if (adding.dead)
                                {
                                    //Debug.Log("continue");
                                    continue;
                                }
                            }
                            break;
                        }
                    }




                    /*
                    RaycastHit[] hits2 = Physics.RaycastAll(ray, explosionradius, mask);
                    //Debug.Log(mask == (mask |  1 << 12));
                    for (int j = 0; j < hits2.Length; j++) //check first valid hit of raycast
                    {
                        Debug.Log("ray object: " + hits2[j].collider.gameObject + ", " + j + ", " + r);
                        if ((mask == (mask | (1 << hits2[j].collider.gameObject.layer)) && hits2[j].collider.gameObject.layer != 12))
                        {
                            //Debug.Log("ray object: " + hits2[j].collider.gameObject + ", " + j);
                        }
                        else if (hits2[j].collider.GetComponentInParent<EntityProperties>().teamid != teamid)
                        {
                            Debug.Log("ray shield");
                        }
                    

                        if ((mask == (mask | (1 << hits2[j].collider.gameObject.layer)) && hits2[j].collider.gameObject.layer != 12) || hits2[j].collider.GetComponentInParent<EntityProperties>().teamid != teamid)
                        {
                            Debug.DrawLine(explosionpos, hits2[j].point, Color.red, 2.0f);
                            EntityProperties adding = hits2[j].collider.GetComponentInParent<EntityProperties>();
                            if(adding.dead)
                            {
                                Debug.Log("continue: " + adding.health);
                                continue;
                            }
                            if (!exposed.Contains(adding))
                            {
                                float intensity = (1.1f - hits2[j].distance / explosionradius);
                                if (intensity > 1) { intensity = 1; }

                                exposed.Add(adding);
                                Debug.Log(adding.gameObject);
                                //Debug.Log(damage * intensity);
                                //Debug.Log((hits[i].ClosestPoint(explosionpos) - explosionpos).normalized * explosionforce * intensity);
                                //Debug.Log(((explosionpos - hits2[j].point).normalized * explosionforce * intensity).magnitude);
                                Rigidbody tempRB = adding.GetComponentInParent<Rigidbody>();
                                if (!tempRB.isKinematic)
                                {
                                    tempRB.velocity = Vector3.ClampMagnitude((tempRB.position + tempRB.centerOfMass - explosionpos).normalized * explosionforce * intensity / adding.GetComponentInParent<Rigidbody>().mass, 40); //max speed 40
                                }
                                adding.TakeDamage(damage * intensity, armorpen, (hits2[j].point - explosionpos).normalized * explosionforce * intensity, hits2[j].collider.gameObject);
                                //adding.GetComponentInParent<Rigidbody>().AddExplosionForce(explosionforce, explosionpos, explosionradius,1);
                                if (adding.dead)
                                {
                                    Debug.Log("continue");
                                    continue;
                                }
                            }

                            break;
                        }
                    }*/
                }
            }

            /*if (exposed.Count > 0)
            {
                exposed.
                for (int k = 0; k < exposed.Count; k++) //all objects in radius
                {
                    exposed[k].TakeDamage(damage)
                }
            }*/
        }
        //Instantiate<explodeffect>
        Instantiate(explosionparticles, transform.position, Quaternion.identity, null);
        foreach (ParticleSystem particles in transform.GetComponentsInChildren<ParticleSystem>())
        {
            particles.transform.SetParent(null);
            particles.Stop();
        }

        //tempbuilding.TakeDamage(tempbuilding.maxhealth / 2, 100f, Vector3.zero, null, false, out List<int> partindex, out bool dead);
        //NetworkClient.localPlayer.GetComponent<PlayerNetwork>().CmdDamageObject(entityIDs, damage, armorpen, velocity, partindex, dead);
        NetworkClient.localPlayer.GetComponent<PlayerNetwork>().CmdDamageObject(entityIDs, damagelist, armorpenlist, forcelist, partindexes, deadlist);
        Destroy(gameObject);
    }
}
