using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class EntityProperties : MonoBehaviour
{
    public bool debugtest;
    public int entityID;
    public int teamid;

    public float maxhealth;
    public float health;
    //public float defaultarmor;

    public float maxpower;  //if entitytype != 0
    public float power; //if entitytype != 0
    public float powerdelta; //if entitytype != 0

    public List<Transform> snappingfrom; //if snappingtype != 0
    public Transform[] snappedto; //if snappingtype != 0

    public Transform altsnappingfrom; //beacon or pump
    public Transform altsnappingto; //beacon or pump

    public List<Vector3> localbeampoints; //if entitytype == 2 or 3
    public List <EntityProperties> connectedproperties; //if snappingtype != 0
    public int buildingid;
    public int entitytype; //0 default, 1 power network, 2 functional building, 3 unit, 4 misc
    public int snappingtype; //1 pipe, 2 power rig, 3 beacon, 4 block, 5 platform, 6 ladder, 7 bridge, 8 wall
    //[SerializeField]
    //public PowerNetworks.Network network;
    public PowerNetworks.Contain contain; //entitytype == 1 or 2
    public AnchorSystem.Object anchordata;
    //public int networkid;
    public Transform powerscale; //entitytype == 1 or 2

    public GameObject[] parts; //if entitytype == 3
    public float[] armor; //if entitytype == 3
    public float[] damagemultiplier; //if entitytype == 3
    public bool dead;

    public Transform backpack; //entitytype == 3

    public int damagealpha;
    public Material damagemat;

    // Start is called before the first frame update
    void Start()
    {
        if (entitytype == 2 || entitytype == 3)
        {
            foreach (Transform alltransforms in GetComponentsInChildren<Transform>())
            {
                if (alltransforms.tag == "BeamPoint")
                {
                    if (snappedto.Length > 0)
                    {
                        localbeampoints.Add(alltransforms.localPosition / alltransforms.localScale.x);
                        alltransforms.gameObject.SetActive(false);
                        Destroy(alltransforms.gameObject);
                    }
                    else
                    {
                        localbeampoints.Add(alltransforms.localPosition / alltransforms.localScale.x);
                        backpack = alltransforms;
                    }
                }
            }

            if (localbeampoints.Count > 0 && transform.GetComponent<Distributor>() == null && transform.GetComponent<Fabricator>() == null)
            {
                DistributorSystem.DistributorManager.EditReceiver(this, true);
            }
        }
    }

    // Update is called once per frame
    //void Update()
    //{
    //     
    // }

    public void TakeDamage(float damage, float armorpen, Vector3 force, GameObject part, bool networkaction, out List<int> partindex, out bool destroyed)
    {
        destroyed = false;
        partindex = new List<int> { };
            /*AnchorSystem.AnchorManager.DebugStart(this);
            //StartCoroutine(AnchorSystem.AnchorManager.DebugTest(this));
            //debugtest = false;
            return;*/
        if(debugtest)
        {
            Debug.Log(anchordata.chunk.anchorID);
            Debug.Log(anchordata.chunk.physicsproperties.position);
            Debug.Log(transform.parent.position);
            Debug.Log(GetComponentInParent<Rigidbody>().position);
            return;
        }

        if (!dead && (armor.Length == 0 || armor[0] != -1))
        {
            int i = -1;
            float effectivearmor = 0;
            float targetarmor = 0;
            //Debug.Log(part);
            if (parts.Length > 0)
            {
                for (i = 0; i < parts.Length; i++)
                {
                    if (parts[i] == part)
                    {
                        targetarmor = armor[i+1];
                        damage *= damagemultiplier[i];
                        // Debug.Log("yes");
                        break;
                    }
                }
            }
            else
            {
                if(armor.Length > 0)
                {
                    targetarmor = armor[0];
                }
                effectivearmor = targetarmor - armorpen;
            }

            effectivearmor = targetarmor - armorpen;
            if (effectivearmor > 0f)
            {
                health -= damage / (effectivearmor * effectivearmor + 1);
                //Debug.Log(damage / (effectivearmor * effectivearmor + 1));
            }
            else
            {
                health -= damage;
                //Debug.Log(damage);
            }

            if (GameSystem.GameManager.isServer) //is server
            {
               /* if (buildingid == -10 && !GameSystem.GameManager.newobjects.Contains(entityID)) //damaged terrain
                {
                    GameSystem.GameManager.newobjects.Add(entityID);
                }*/
            }

            if (networkaction == false)
            {
                //PlayerNetwork localplayer = NetworkClient.localPlayer.GetComponent<PlayerNetwork>();
                partindex = new List<int>();

                if(part != null)
                {
                    while(!part.GetComponent<EntityProperties>())
                    {
                        partindex.Add(part.transform.GetSiblingIndex());
                        part = part.transform.parent.gameObject;
                    }
                }

                partindex.Reverse();
                destroyed = health <= 0;
                //localplayer.CmdDamageObject(entityID, damage, armorpen, force, partindex, health <= 0);
            }

            if (health <= 0)
            {
                dead = true;

                health = 0;

                /*if (GameSystem.GameManager.isServer) //is server
                {
                    //Debug.Log(entityID);
                    for (i = 0; i < GameSystem.GameManager.newobjects.Count; i++)
                    {
                        //Debug.Log(GameSystem.GameManager.newobjects[i]);
                    }
                    //GameSystem.GameManager.ServerRB(entityID, -1, out _);
                    if (GameSystem.GameManager.newobjects.Contains(entityID))
                    {
                        GameSystem.GameManager.newobjects.Remove(entityID);
                        //Debug.Log(entityID + " removed, " + GameSystem.GameManager.newobjects.Contains(entityID));
                    }
                    if(buildingid == -10)
                    {
                        GameSystem.GameManager.destroyedterrain.Add(entityID);
                    }
                }*/
                //Debug.Log("connected count: " + contain.connectedto.Count);

                //if (snappedto[0] != null) //if my bp exists
                //{
                for (int j = 0; j < snappedto.Length; j++) //re enable their snapping points
                {
                    Transform snapupdate = snappedto[j];
                    if (snapupdate != null)
                    {
                        snapupdate.gameObject.SetActive(true);
                        Blueprint bp = snapupdate.GetComponentInParent<Blueprint>();
                        EntityProperties properties = snapupdate.GetComponentInParent<EntityProperties>();
                        if (bp != null) //find their snappingfrom of our snappedto
                        {
                            int foundindex = bp.snappingfrom.FindIndex(x => x == snapupdate);
                            bp.snappingfrom[foundindex].gameObject.SetActive(true);
                            bp.snappedto[foundindex] = null;
                            //properties.snappingfrom[foundindex].gameObject.SetActive(true);
                        }
                        if (properties != null)
                        {
                            int foundindex = properties.snappingfrom.FindIndex(x => x == snapupdate);
                            properties.snappingfrom[foundindex].gameObject.SetActive(true);
                            properties.snappedto[foundindex] = null;


                            //properties.snappingfrom.Find(x => x == snapupdate).gameObject.SetActive(true);
                            //bp.snappingfrom.Find(x => x == snappingfrom[j]) = null;
                            //int foundindex = bp.snappingfrom.FindIndex(x => x == snapupdate);
                            //bp.snappingfrom[foundindex].gameObject.SetActive(true);
                        }
                    }
                }
                //Debug.Log(transform.GetComponent<Blueprint>());
                if(altsnappingto != null)
                {
                    altsnappingfrom = null;
                    altsnappingto.GetComponentInParent<EntityProperties>().altsnappingfrom.gameObject.SetActive(true);
                    altsnappingto.GetComponentInParent<EntityProperties>().altsnappingto = null;
                    if (altsnappingto.GetComponentInParent<Blueprint>() != null)
                    {
                        altsnappingto.GetComponentInParent<Blueprint>().altsnappingfrom.gameObject.SetActive(true);
                        altsnappingto.GetComponentInParent<Blueprint>().altsnappingto = null;
                    }
                }
                //}
                if (localbeampoints.Count > 0)
                {
                    DistributorSystem.DistributorManager.EditReceiver(this, false);
                }

                if (GetComponent<Blueprint>() != null)
                {
                    FabricatorSystem.FabricatorManager.EditBPList(transform.GetComponent<Blueprint>(), false);
                    //Debug.Log("bp");
                    if (transform.GetComponent<Blueprint>().altsnappingto != null)
                    {
                        Debug.Log("altsnap");
                        transform.GetComponent<Blueprint>().altsnappingto.GetComponentInParent<EntityProperties>().altsnappingfrom.gameObject.SetActive(true);
                        transform.GetComponent<Blueprint>().altsnappingto = null;
                    }
                    Destroy(gameObject);
                }
                if ((contain != null && contain.network != null) || anchordata != null)
                {
                    //Debug.Log("oop");
                    if (contain != null)
                    {
                        PowerNetworks.PowerManager.RemoveObject(contain);
                    }
                    if (anchordata != null)
                    {
                        //Debug.Log(anchordata.connectedto.Count);
                        foreach(AnchorSystem.Object connections in anchordata.connectedto)
                        {
                            //Debug.Log(connections.objectproperties.gameObject.name);
                        }
                        AnchorSystem.AnchorManager.RemoveObject(anchordata);
                    }
                    transform.SetParent(null);
                    Destroy(gameObject);
                }
                else
                {
                    if (GetComponent<Rigidbody>() != null)
                    {
                        GetComponent<Rigidbody>().AddForce(force);
                    }
                    else if (GetComponentInChildren<Rigidbody>() != null)
                    {
                        GetComponentInChildren<Rigidbody>().AddForce(force);
                    }
                }
            }
            else if(entitytype != 3 && entitytype != 4)
            {
                if (damagealpha == 0)
                {
                    Material checkformat = GetComponent<Renderer>().materials[0];
                    damagemat = Material.Instantiate<Material>(GetComponent<Renderer>().materials[0]);//Instantiate(GetComponent<Renderer>().materials[1]);
                    Material[] tempmats = GetComponent<Renderer>().materials;
                    foreach (Renderer allrenderers in GetComponentsInChildren<Renderer>())
                    {
                        if(allrenderers.material == checkformat)
                        {
                            allrenderers.sharedMaterials = tempmats;
                        }
                        if (allrenderers.materials.Length > 1)
                        {
                            Debug.Log("error materials: " + transform.name);
                            allrenderers.sharedMaterials = tempmats;
                            // allrenderers.materials[1] = damagemat;
                        }
                    }
                    damagealpha = Shader.PropertyToID("DamageAlpha");

                    damagemat = GetComponent<Renderer>().materials[0];
                    damagemat.SetFloat(damagealpha, 0);
                }

                if(health / maxhealth < .25f)
                {
                    damagemat.SetFloat(damagealpha, -1);
                    GameSystem.GameManager.UpdateChunkMesh(anchordata.chunk.chunkobject);
                }
                else if(health / maxhealth < .75f)
                {
                    damagemat.SetFloat(damagealpha, health * 2f / maxhealth - 1.5f); //-1 at .25, 0 at .75
                    GameSystem.GameManager.UpdateChunkMesh(anchordata.chunk.chunkobject);
                }
            }
            //Debug.Log(damage);
        }
    }
    private void OnDestroy()
    {
        if(GameSystem.GameManager && GameSystem.GameManager.entityIDs.ContainsKey(entityID))
        {
            GameSystem.GameManager.entityIDs.Remove(entityID);
        }
    }
}
