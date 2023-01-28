using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Fabricator : MonoBehaviour
{
    public int teamid;
    public EntityProperties thisproperties;
    public PowerNetworks.Contain thiscontain;
    public PowerNetworks.Network thisnetwork;
    public List<Vector3> beamoutput;

    public List<LineRenderer> activebeams;
    public GameObject beamprefab;

    // Start is called before the first frame update
    void Start()
    {
        thisproperties = gameObject.GetComponent<EntityProperties>();
        //thiscontain = gameObject.GetComponent<EntityProperties>().contain;
        teamid = thisproperties.teamid;
        thiscontain = thisproperties.contain;
        thisnetwork = thiscontain.network;

        if(thisproperties.localbeampoints.Count > 0)
        {
            beamoutput.AddRange(thisproperties.localbeampoints);
            thisproperties.localbeampoints.Clear();
        }
        else
        {
            foreach (Transform alltransforms in GetComponentInChildren<Transform>())
            {
                if (alltransforms.tag == "BeamPoint")
                {
                    beamoutput.Add(alltransforms.localPosition / alltransforms.localScale.x);
                    alltransforms.gameObject.SetActive(false);
                    Destroy(alltransforms.gameObject);
                }
            }
        }
        GameObject createbeam = Instantiate(beamprefab, transform.position, transform.rotation);
        createbeam.transform.SetParent(transform);
        activebeams.Add(createbeam.GetComponent<LineRenderer>());
        activebeams[0].startWidth = .05f;
        activebeams[0].endWidth = .3f;
        // foreach(Vector)
    }

    // Update is called once per frame
    void Update()
    {
        //if (NetworkClient.localPlayer.isServer)
        //{
            bool active = false;
            thisnetwork = thiscontain.network;
            if (FabricatorSystem.FabricatorManager.teamdata[teamid].blueprints.Count > 0)
            {
                foreach (Blueprint buildingbp in FabricatorSystem.FabricatorManager.teamdata[teamid].blueprints)
                {
                    if (buildingbp == null)
                    {
                        //FabricatorSystem.FabricatorManager.blueprints.Remove(buildingbp);
                        continue;
                    }
                    //Blueprint buildingbp = FabricatorSystem.FabricatorManager.blueprints[0];
                    if (Vector3.Distance(buildingbp.transform.position, this.transform.position) < 20)
                    {
                        float powersend = 10 * Time.deltaTime;
                        bool finished = false;
                        thisnetwork = thiscontain.network;
                        if (powersend > thisnetwork.power)
                        {
                            powersend = thisnetwork.power;
                        }
                        if (powersend > buildingbp.maxpower - buildingbp.power)
                        {
                            powersend = buildingbp.maxpower - buildingbp.power;
                            finished = true;
                        }

                        if (NetworkClient.localPlayer.isServer)
                        {
                            NetworkClient.localPlayer.GetComponent<PlayerNetwork>().CmdPowerExchange(thisproperties.entityID, buildingbp.GetComponent<EntityProperties>().entityID, powersend, finished);
                            buildingbp.BlueprintFill(powersend);
                        }

                        float closestdistance = 20;
                        Vector3[] closestbeam = new Vector3[2];
                        closestbeam[1] = buildingbp.transform.position;
                        foreach (Vector3 checkbeam in beamoutput)
                        {
                            float currentdistance = Vector3.Distance(transform.rotation * checkbeam + transform.position, buildingbp.transform.position);
                            if (currentdistance < closestdistance)
                            {
                                closestdistance = currentdistance;
                                closestbeam[0] = transform.rotation * checkbeam + transform.position;
                            }
                        }
                        activebeams[0].SetPositions(closestbeam);
                        if (thisnetwork.power > 0.01)
                        {
                            activebeams[0].enabled = true;
                        }
                        else
                        {
                            activebeams[0].enabled = false;
                        }
                        //thisnetwork.power -= powersend;
                        active = true;
                        break;
                    }
                }
            }
            if (!active)
            {
                activebeams[0].enabled = false;
            }
        }
    //}
}
