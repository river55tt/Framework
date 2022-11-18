using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Distributor : MonoBehaviour
{
    public EntityProperties thisproperties;
    public PowerNetworks.Contain thiscontain;
    public PowerNetworks.Network thisnetwork;

    public List<EntityProperties> distributeto;
    public List<EntityProperties> activedistribute;
    public List<LineRenderer> activebeams;

    public GameObject beamprefab;
    public Vector3 beamorigin;
    EntityProperties playerstats;
    // Start is called before the first frame update
    void Start()
    {
        thisproperties = GetComponent<EntityProperties>();
        thiscontain = thisproperties.contain;
        thisnetwork = thiscontain.network;
        //distributeto.Add(GameObject.Find("Third Person Player").GetComponent<EntityProperties>());  //TEMPORARY

        if (thisproperties.localbeampoints.Count > 0)
        {
            beamorigin = thisproperties.localbeampoints[0];
            thisproperties.localbeampoints.Clear();
        }
        else
        {
            foreach (Transform alltransforms in GetComponentInChildren<Transform>())
            {
                if (alltransforms.tag == "BeamPoint")
                {
                    beamorigin = alltransforms.localPosition / alltransforms.localScale.x;
                    alltransforms.gameObject.SetActive(false);
                    Destroy(alltransforms.gameObject);
                    break;
                }
            }
        }
        //playerstats = distributeto[0].GetComponent<EntityProperties>();
    }

    // Update is called once per frame
    void Update()
    {
        thisnetwork = thiscontain.network;
        //if (NetworkClient.localPlayer.isServer)
        //{
            if (DistributorSystem.DistributorManager.receivers.Count > 0)
            {
                foreach (EntityProperties target in DistributorSystem.DistributorManager.receivers) //find valid receivers
                {
                    if(target == null)
                    {
                        continue;
                    }
                    bool samenetwork = false;
                    if (target.contain != null && target.contain.network == thisnetwork)
                    {
                        samenetwork = true;
                        //Debug.Log("bruh");
                    }
                    if (!samenetwork && Vector3.Distance(target.transform.position, this.transform.position) < 10)
                    {

                        int indextarget = activedistribute.FindIndex(x => x == target);
                        if (indextarget == -1)
                        {
                            activedistribute.Add(target);
                            GameObject createbeam = Instantiate(beamprefab, transform.position, transform.rotation);
                            createbeam.transform.SetParent(transform);
                            activebeams.Add(createbeam.GetComponent<LineRenderer>());
                            activebeams[activebeams.Count - 1].startWidth = .3f;
                            activebeams[activebeams.Count - 1].endWidth = .05f;
                        }
                    }
                    else
                    {
                        int indextarget = activedistribute.FindIndex(x => x == target);
                        if (indextarget == -1)
                        {
                            continue;
                        }
                        activebeams[indextarget].gameObject.SetActive(false);
                        activebeams[indextarget].enabled = false;
                        Destroy(activebeams[indextarget].gameObject);
                        activedistribute.RemoveAt(indextarget);
                        activebeams.RemoveAt(indextarget);
                    }
                }
                for (int j = 0; j < activedistribute.Count; j++) //distribute power
                {
                    if (activedistribute[j] == null)
                    {
                        activedistribute.RemoveAt(j);
                        Destroy(activebeams[j].gameObject);
                        activebeams.RemoveAt(j);
                        continue;
                    }
                    Vector3[] beampositions = new Vector3[2];
                    beampositions[0] = transform.position + transform.rotation * beamorigin;
                    beampositions[1] = activedistribute[j].transform.position + activedistribute[j].transform.rotation * activedistribute[j].localbeampoints[0];

                    activebeams[j].SetPositions(beampositions);

                    float powersend = 10 * Time.deltaTime;

                    if (powersend > thisnetwork.power)
                    {
                        powersend = thisnetwork.power;
                    }
                    if (activedistribute[j].contain != null && activedistribute[j].contain.network != thisnetwork)
                    {
                        PowerNetworks.Network transfernetwork = activedistribute[j].contain.network;
                        if (powersend > transfernetwork.powerstorage - transfernetwork.power)
                        {
                            powersend = transfernetwork.powerstorage - transfernetwork.power;
                        }
                    if (NetworkClient.localPlayer.isServer)
                    {
                        transfernetwork.power += powersend;
                    }
                    }
                    else
                    {
                        if (powersend > activedistribute[j].maxpower - activedistribute[j].power)
                        {
                            powersend = activedistribute[j].maxpower - activedistribute[j].power;
                        }
                    if (NetworkClient.localPlayer.isServer)
                    {
                        activedistribute[j].power += powersend;
                    }
                    }
                    if (thisnetwork.power > 0)
                    {
                        activebeams[j].enabled = true;
                    }
                    else
                    {
                        activebeams[j].enabled = false;
                    }
                if (NetworkClient.localPlayer.isServer)
                {
                    NetworkClient.localPlayer.GetComponent<PlayerNetwork>().CmdPowerExchange(thisproperties.entityID, activedistribute[j].entityID, powersend, false);
                }
                    //thisnetwork.power -= powersend;
                }
            }
        }
   // }
}
