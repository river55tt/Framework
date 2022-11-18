using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistributorSystem : MonoBehaviour
{
    public static DistributorSystem DistributorManager;
    public List<Distributor> distributors = new List<Distributor>();
    public List<EntityProperties> receivers = new List<EntityProperties>();


    public GameObject beamprefab;
    public Vector3 beamorigin;
    EntityProperties playerstats;
    // Start is called before the first frame update
    private void Awake()
    {
        if (DistributorManager == null)
        {
            DistributorManager = this;
            Debug.Log("Initialized Distributor Manager");
        }
    }
    void Start()
    {
        //receivers.Add(GameObject.Find("Third Person Player").GetComponent<EntityProperties>());  //TEMPORARY
    }

    // Update is called once per frame
    /* void Update()
    {
        float powersend = 10 * Time.deltaTime;
        thisnetwork = thiscontain.network;
        foreach (EntityProperties target in distributeto)
        {
            if (Vector3.Distance(target.transform.position, this.transform.position) < 10)
            {
                /*float powersend = 10 * Time.deltaTime;
                thisnetwork = thiscontain.network;
                if (powersend > thisnetwork.power)
                {
                    powersend = thisnetwork.power;
                }
                if (powersend > playerstats.maxpower - playerstats.power)
                {
                    powersend = playerstats.maxpower - playerstats.power;
                }
                playerstats.power += powersend;
                thisnetwork.power -= powersend;*/               /*

                int indextarget = activedistribute.FindIndex(x => x == target);
                if (indextarget == -1)
                {
                    activedistribute.Add(target);
                    GameObject createbeam = Instantiate(beamprefab, transform.position, transform.rotation);
                    createbeam.transform.SetParent(transform);
                    activebeams.Add(createbeam.GetComponent<LineRenderer>());
                    activebeams[0].startWidth = .3f;
                    activebeams[0].endWidth = .05f;
                }
            }
            else
            {
                int indextarget = activedistribute.FindIndex(x => x == target);
                if(indextarget == -1)
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
        for (int j = 0; j < activedistribute.Count; j++)
        {
            Vector3[] beampositions = new Vector3[2];
            beampositions[0] = transform.position + transform.rotation * beamorigin;
            beampositions[1] = activedistribute[j].transform.position + activedistribute[j].transform.rotation * activedistribute[j].localbeampoints[0];

            activebeams[j].SetPositions(beampositions);

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
                transfernetwork.power += powersend;
            }
            else
            {
                if (powersend > activedistribute[j].maxpower - activedistribute[j].power)
                {
                    powersend = activedistribute[j].maxpower - activedistribute[j].power;
                }
                activedistribute[j].power += powersend;
            }
            if(thisnetwork.power>0)
            {
                activebeams[j].enabled = true;
            }
            else
            {
                activebeams[j].enabled = false;
            }
            thisnetwork.power -= powersend;
        }
}*/
    public void EditDistributor(Distributor distributor, bool adding)
    {
        if (adding)
        {
            distributors.Add(distributor);
        }
        else
        {
            distributors.Remove(distributor);
        }
    }
    public void EditReceiver(EntityProperties receiver, bool adding)
    {
        if (adding)
        {
            receivers.Add(receiver);
            //Debug.Log("added");
        }
        else
        {
            receivers.Remove(receiver);
        }
    }
}