using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Transceiver : MonoBehaviour
{
    public int teamid;
    public PowerNetworks.Contain thiscontain;
    public PowerNetworks.Network thisnetwork;
    public Vector3 beamoutput;

    public EntityProperties thisproperties;

    public List<LineRenderer> activebeams;
    public GameObject beamprefab;
    private TransceiverSystem Manager;
    // Start is called before the first frame update
    void Start()
    {
        thisproperties = gameObject.GetComponent<EntityProperties>();
        //thiscontain = gameObject.GetComponent<EntityProperties>().contain;
        teamid = thisproperties.teamid;
        thiscontain = thisproperties.contain;
        thisnetwork = thiscontain.network;
        Manager = TransceiverSystem.TransceiverManager;
        Manager.teamdata[teamid].transceivers.Add(this);
        /*if (properties.localbeampoints.Count > 0)
        {
            beamoutput = properties.localbeampoints[0];
            properties.localbeampoints.Clear();
        }
        else
        {*/
            foreach (Transform alltransforms in transform.GetComponentsInChildren<Transform>())
            {
                //Debug.Log(alltransforms.name);
                if (alltransforms.tag == "BeamPoint")
                {
                    beamoutput = alltransforms.localPosition;
                    alltransforms.gameObject.SetActive(false);
                    Destroy(alltransforms.gameObject);
                }
            }
       // }
        GameObject createbeam = Instantiate(beamprefab, transform.position, transform.rotation);
        createbeam.transform.SetParent(transform);
        activebeams.Add(createbeam.GetComponent<LineRenderer>());
        activebeams[0].startWidth = .3f;
        activebeams[0].endWidth = .3f;
    }

    // Update is called once per frame
    void Update()
    {
        thisnetwork = thiscontain.network;
        if (Manager.teamdata[teamid].transceivers.Count > 1)
        {
            activebeams[0].gameObject.SetActive(false);
            //Debug.Log(Manager.teamdata[teamid].transceivers.Count);
            foreach (Transceiver othertransceiver in Manager.teamdata[teamid].transceivers)
            {
                if (othertransceiver == null || othertransceiver == this)
                {
                    //FabricatorSystem.FabricatorManager.blueprints.Remove(buildingbp);
                    continue;
                }
                Vector3 beampos1 = transform.position + beamoutput;
                Vector3 beampos2 = othertransceiver.transform.position + othertransceiver.beamoutput;
                RaycastHit hit;
                //MASK
                //Debug.Log(transform.position + beamoutput);
               // Debug.Log(othertransceiver.transform.position + othertransceiver.beamoutput);

                Debug.DrawLine(beampos1, beampos1 + (beampos2 - beampos1) * 10f, Color.blue, 1.0f);
                if (othertransceiver.thisnetwork != thisnetwork && Physics.Raycast(beampos1, beampos2 - beampos1, out hit, 1000f)) // 200/300 + 1300/1700
                {
                    Debug.Log(hit.collider.transform.name);
                    //Debug.DrawLine(beamoutput, beamoutput + (othertransceiver.beamoutput - beamoutput) * 10f, Color.blue, 1.0f);
                    if (hit.collider.transform == othertransceiver.transform)
                    {
                        float powersend = 10 * Time.deltaTime;
                        thisnetwork = thiscontain.network;

                        float powerpercent = (othertransceiver.thisnetwork.power + thisnetwork.power) / (othertransceiver.thisnetwork.powerstorage + thisnetwork.powerstorage);
                        float powerequalize = (thisnetwork.power - thisnetwork.powerstorage * powerpercent); //math maybe isnt right
                        if (powersend > Mathf.Abs(powerequalize))
                        {
                            powersend = powerequalize;
                        }
                        else if (othertransceiver.thisnetwork.power > thisnetwork.power)
                        {
                            powersend *= -1;
                        }

                        //buildingbp.BlueprintFill(powersend);
                        othertransceiver.thisnetwork.power += powersend;
                        thisnetwork.power -= powersend;

                        activebeams[0].gameObject.SetActive(true);
                        Vector3[] powerbeam = new Vector3[2];
                        powerbeam[0] = beampos1;
                        powerbeam[1] = beampos2;
                        activebeams[0].SetPositions(powerbeam);
                        //if (thisnetwork.power > 0.01)
                        //{
                        //    activebeams[0].enabled = true;
                        //}
                        //else
                        //{
                        //    activebeams[0].enabled = false;
                        //}

                        //active = true;
                        //break;
                        if (NetworkClient.localPlayer.isServer)
                        {
                            NetworkClient.localPlayer.GetComponent<PlayerNetwork>().CmdPowerExchange(thisproperties.entityID, othertransceiver.thisproperties.entityID, powersend, false);
                        }
                    }
                }
            }
        }
        //if (!active)
        ///{
        //    activebeams[0].enabled = false;
        //}
    }

    private void OnDestroy()
    {
        Manager.teamdata[teamid].transceivers.Remove(this);
    }
}