using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerNetworks : MonoBehaviour
{
    public static PowerNetworks PowerManager;
    //[SerializeField]
    public List<Network> allnetworks = new List<Network>();

    //public Contain objectdata;
    //public Network networkdata;
    //public List<Contain> contain;
    //public List<Transform> connections;

    public GameObject leakprefab;
    // Start is called before the first frame update
    void Awake()
    {
        if (PowerManager == null)
        {
            PowerManager = this;
            Debug.Log("Initialized Power Manager");
        }
    }
    public class Network
    {
        public int powerID;
        public List<Contain> Contents;
        public int networksize;
        public List<Transform> powerscale;
        public float powerstabledelta;
        public float powerstorage;

        public float power;

        public float powerunstabledelta;

        public List<ParticleSystem> leaklist;
    }
    public class Contain
    {
        public Transform pscale;
        public Network network;
        public List<Contain> connectedto;
        public EntityProperties objectproperties;
        public int powertype;
        
        public float pdelta;
        public float pstorage;

        public float power;

        public List<ParticleSystem> leaks;
        public List<int> leakindex;
    }

    
    // Update is called once per frame
    void Update() //power delta here
    {
        if(PowerManager == null)
        {
            PowerManager = this;
        }
        foreach(Network thisnetwork in allnetworks)
        {
            if(thisnetwork.networksize < 1)
            {
                allnetworks.Remove(thisnetwork);
                continue;
            }

            thisnetwork.power += thisnetwork.powerstabledelta * Time.deltaTime;
            
            bool leaking = false;
            if(thisnetwork.power > 0 || thisnetwork.powerstabledelta > 0)
            {
                leaking = true;
            }
            if (thisnetwork.leaklist.Count > 0 && thisnetwork.leaklist[0] != null)
            {
                thisnetwork.power -= thisnetwork.leaklist.Count * 5 * Time.deltaTime;
                foreach (ParticleSystem leakpoint in thisnetwork.leaklist)
                {
                    //Debug.Log(leakpoint.transform.parent);
                    if(leaking)
                    {
                        if (!leakpoint.isEmitting)
                        {
                            leakpoint.Play();
                        }
                    }
                    else
                    {
                        leakpoint.Stop();
                    }
                }
            }

            if (thisnetwork.power > thisnetwork.powerstorage)
            {
                thisnetwork.power = thisnetwork.powerstorage;
            }
            else if(thisnetwork.power < 0)
            {
                thisnetwork.power = 0;
                //Debug.Log("power less than 0");
            }

            float pscale = 0;
            if (thisnetwork.powerstorage > 0)
            {
                pscale = thisnetwork.power / thisnetwork.powerstorage;
            }
            if (thisnetwork.powerscale == null)
            {
                thisnetwork.powerscale = new List<Transform>();
            }
            foreach (Transform scalepower in thisnetwork.powerscale)
            {
                scalepower.localScale = new Vector3(1, pscale, 1);
            }
            foreach (Contain contents in thisnetwork.Contents)
            {
                contents.power = pscale * contents.pstorage;
            }
        }
    }
    public void AddObject(EntityProperties addedobject)
    {
        //Debug.Log(objectdata.thisobject);
        var objectdata = new Contain { };  //{thisobject = null, connectedto = null, objectproperties = null, powertype = 0, pdelta = 0, pstorage = 0, power = 0};
        var networkdata = new Network { };  //{Contents = null, powerstabledelta = 0, powerstorage = 0, power = 0, powerunstabledelta = 0};

        addedobject.contain = objectdata;
        //objectdata.thisobject = addedobject.transform;
        objectdata.objectproperties = addedobject;
        objectdata.pdelta = addedobject.powerdelta;
        objectdata.pstorage = addedobject.maxpower;
        objectdata.pscale = addedobject.powerscale;
        objectdata.power = addedobject.power;
        objectdata.connectedto = new List<Contain>();
        objectdata.leaks = new List<ParticleSystem>();
        objectdata.leakindex = new List<int>();
        bool existingnetwork = false;
        //Debug.Log("power network");
        foreach (Transform connected in addedobject.snappedto)
        {
            //Network connectednetwork = connected.GetComponentInParent<EntityProperties>().network;
            if (connected != null)// && connectednetwork != null)
            {
                //Network connectednetwork = connected.GetComponentInParent<Contain>().network;
                EntityProperties connectedproperties = connected.GetComponentInParent<EntityProperties>();
                Contain connecteddata = connected.GetComponentInParent<EntityProperties>().contain;
                if(connecteddata == null)
                {
                    continue;
                }
                Network connectednetwork = connecteddata.network;
                connecteddata.connectedto.Add(objectdata);
                if (connectednetwork != null)// && connectednetwork != networkdata)
                {
                    //Debug.Log(connected.parent);
                    //Debug.Log("connected network id: " + allnetworks.FindIndex(x => x == connectednetwork));
                    //Debug.Log("connected ID: " + connectedproperties.entityID);
                    for (int j = 0; j < connectedproperties.snappedto.Length; j++)
                    {
                        if (connectedproperties.snappedto[j] != null && connectedproperties.snappedto[j].GetComponentInParent<EntityProperties>())// && connectedproperties.snappedto[j].GetComponentInParent<EntityProperties>() == addedobject)
                        {
                            EntityProperties thisprop = connectedproperties.snappedto[j].GetComponentInParent<EntityProperties>();
                            /*if(!thisprop.enabled)
                            {
                                thisprop = thisprop.transform.parent.parent.GetComponentInParent<EntityProperties>();
                            }*/
                            //Debug.Log("leak object " + j + " id: " + thisprop.entityID + ", " + thisprop.gameObject.name + thisprop.transform.GetInstanceID() + thisprop.enabled);
                            //Debug.Log(j + ": " + thisprop);
                            if (thisprop == addedobject)
                            {
                                //foreach()
                                //Debug.Log("leaks: " + connecteddata.leaks.Count);
                                //Debug.Log(connecteddata.leakindex.FindIndex(x => x == j));
                                ParticleSystem leakremove = connecteddata.leaks[connecteddata.leakindex.FindIndex(x => x == j)];
                                connecteddata.leaks.Remove(leakremove);
                                connecteddata.leakindex.Remove(j);
                                connecteddata.network.leaklist.Remove(leakremove);
                                Destroy(leakremove.gameObject);
                            }
                        }

                        /*if (connectedproperties.snappedto[j] != null)// && connectedproperties.snappedto[j].GetComponentInParent<EntityProperties>() == addedobject)
                        {
                            foreach (EntityProperties thisprop in connectedproperties.snappedto[j].GetComponentsInParent<EntityProperties>())
                            {
                                if(thisprop == addedobject)
                                {
                                    ParticleSystem leakremove = connecteddata.leaks[connecteddata.leakindex.FindIndex(x => x == j)]; //ERROR HERE?
                                    connecteddata.leaks.Remove(leakremove);
                                    connecteddata.leakindex.Remove(j);
                                    connecteddata.network.leaklist.Remove(leakremove);
                                    Destroy(leakremove.gameObject);
                                }
                            }
                        }*/
                        /*else if(connectedproperties.snappedto[j] == null)
                        {
                            Debug.Log("fuck");
                        }*/

                       /* if (connectedproperties.snappedto[j] != null)
                        {
                            Debug.Log(connectedproperties.snappedto[j].GetComponentInParent<EntityProperties>().gameObject);
                            Debug.Log(addedobject.gameObject);
                            //Debug.Log(addedobject == connectedproperties.snappedto[j].GetComponentsInParent<EntityProperties>());
                        }*/
                    }
                    if (!existingnetwork)
                    {
                        existingnetwork = true;
                        networkdata = connectednetwork;
                        //addedobject.networkid = allnetworks.FindIndex(x => x == connectednetwork);
                    }
                    else if (connecteddata.network != networkdata)
                    {
                        Debug.Log("combine network");
                        CombineNetwork(networkdata, connectednetwork);
                        //connectednetwork = networkdata;  dont need?
                    }
                }
                objectdata.connectedto.Add(connecteddata);
                //connectednetwork = networkdata;
                //Debug.Log("b");
            }
        }
        //Debug.Log("here");
        for (int j = 0; j < addedobject.snappedto.Length; j++)
        {
            //Debug.Log("ping");
            //Debug.Log(addedobject.snappedto[j]);
            //Debug.Log("pong");
            if (addedobject.snappedto[j] == null || addedobject.snappedto[j].GetComponentInParent<Blueprint>() != null)
            {
                //Debug.Log("hi");
                //Debug.Log(addedobject.snappingfrom[j]);
                ParticleSystem leakadd = Instantiate(leakprefab, addedobject.snappingfrom[j].position, addedobject.snappingfrom[j].rotation).GetComponentInChildren<ParticleSystem>();
                leakadd.transform.SetParent(addedobject.transform);
                leakadd.transform.localScale = new Vector3(1, 1, 1);
                    //addedobject.snappingfrom[j].GetComponent<ParticleSystem>();
                objectdata.leaks.Add(leakadd);
                objectdata.leakindex.Add(j);
                //connecteddata.network.leaklist.Add(connectedproperties.snappingfrom[j].GetComponent<ParticleSystem>());
            }
            //Debug.Log("pong");
        }
        //Debug.Log("here2");
        //Debug.Log(objectdata.connectedto.Count);
        //objectdata.connectedto.Add(addedobject.transform);
        //Debug.Log(objectdata);


        //Debug.Log(networkdata);
        if (!existingnetwork)
        {
            networkdata.Contents = new List<Contain>();
            networkdata.powerscale = new List<Transform>();
            networkdata.leaklist = new List<ParticleSystem>();
            allnetworks.Add(networkdata);
            //Debug.Log("added network");
            //Debug.Log("current network id: " + allnetworks.FindIndex(x => x == networkdata));
            //addedobject.networkid = allnetworks.FindIndex(x => x == networkdata);
        }

        AddData(networkdata, objectdata);
        /*networkdata.Contents.Add(objectdata);
        networkdata.networksize++;
        networkdata.powerstabledelta += objectdata.pdelta;
        networkdata.powerstorage += objectdata.pstorage;
        networkdata.power += objectdata.power;
        addedobject.network = networkdata;*/
        /*if(addedobject.powerscale != null)
        {
            if(networkdata.powerscale == null)
            {
                networkdata.powerscale = new List<Transform>();
            }
            networkdata.powerscale.Add(addedobject.powerscale);
        }*/
        //Debug.Log("size: " + networkdata.networksize);
        //Debug.Log("network count: " + allnetworks.Count);
       // Debug.Log("leak count: " + networkdata.leaklist.Count);
    }
    void AddData(Network expandingnetwork, Contain addeddata)
    {
        expandingnetwork.Contents.Add(addeddata);
        expandingnetwork.networksize++;
        expandingnetwork.powerstabledelta += addeddata.pdelta;
        expandingnetwork.powerstorage += addeddata.pstorage;
        expandingnetwork.power += addeddata.power;
        addeddata.network = expandingnetwork;

        if (addeddata.pscale != null)
        {
            //Debug.Log(expandingnetwork.powerscale);
            expandingnetwork.powerscale.Add(addeddata.pscale);
        }
        if (addeddata.leaks != null)
        {
            //Debug.Log(expandingnetwork.powerscale);
            expandingnetwork.leaklist.AddRange(addeddata.leaks);
        }
    }
    public void CombineNetwork(Network network1, Network network2)
    {
        network1.powerstabledelta += network2.powerstabledelta;
        network1.powerunstabledelta += network2.powerunstabledelta;
        network1.powerstorage += network2.powerstorage;
        network1.power += network2.power;

        foreach (Contain content in network2.Contents)
        {
            content.network = network1;
            content.objectproperties.contain.network = network1;
        }
        network1.Contents.AddRange(network2.Contents);
        network1.networksize += network2.networksize;
        network1.powerscale.AddRange(network2.powerscale);
        network1.leaklist.AddRange(network2.leaklist);
        allnetworks.Remove(network2);                                                              //memory leak? LOOK INTO LATER
    }

    public void RemoveObject(Contain removedobject)
    {
        //Debug.Log("connected count: " + removedobject.connectedto.Count);
        var networkdata = removedobject.network;
        //var objectcontain = removedobject.contain;
        if (networkdata.networksize == 1)
        {
            networkdata.Contents.Remove(removedobject);
            allnetworks.Remove(networkdata);                                                           //memory leak? LOOK INTO LATER
            removedobject.network = null;
            removedobject.objectproperties.contain.network = null;
            removedobject.objectproperties.contain = null;
            //removedobject.objectproperties.contain.network = null;
            return;
        }
        foreach (Contain removeconnections in removedobject.connectedto) //remove og object connections
        {
            //Debug.Log("old " + removeconnections.connectedto.Count);
            removeconnections.connectedto.Remove(removedobject);
            //Debug.Log("new " + removeconnections.connectedto.Count);
        }
        var searchqueue = new List<Contain>();
        var searchfor = new List<Contain>();
        var searched = new List<Contain>();
        //Debug.Log("gamer");
        bool separated = false;
        searchfor.AddRange(removedobject.connectedto);
        //Debug.Log(searchfor.Count);
        searchqueue.AddRange(searchfor[0].connectedto); //set up first connection
        searched.Add(searchfor[0]);
        searchfor.RemoveAt(0);
        //Debug.Log("gamer2");
        int notfound = searchfor.Count;
        //Debug.Log("notfound: " + notfound);

        /*foreach (Contain removeconnections in removedobject.connectedto) //remove og object connections
        {
            removeconnections.connectedto.Remove(removedobject);
        }*/
        networkdata.Contents.Remove(removedobject); //remove og object
        foreach (ParticleSystem leakremove in removedobject.leaks)
        {
            networkdata.leaklist.Remove(leakremove);
        }
        if (removedobject.pscale != null)
        {
            networkdata.powerscale.Remove(removedobject.pscale);
        }

        for (int j = 0; j < removedobject.objectproperties.snappedto.Length; j++)
        {

            if (removedobject.objectproperties.snappedto[j] != null)// && connectedproperties.snappedto[j].GetComponentInParent<EntityProperties>() == addedobject)
            {

                EntityProperties thisprop = removedobject.objectproperties.snappedto[j].GetComponentInParent<EntityProperties>();
                // foreach (EntityProperties thisprop in removedobject.objectproperties.snappedto[j].GetComponentsInParent<EntityProperties>())
                //{
                //if (thisprop == removedobject.objectproperties)
                //{
                //Debug.Log("hi");
                for (int b = 0; b < thisprop.snappedto.Length; b++)
                {
                    if(thisprop.snappingfrom[b] == removedobject.objectproperties.snappedto[j])
                    {
                        ParticleSystem leakadd = Instantiate(leakprefab, thisprop.snappingfrom[b].position, thisprop.snappingfrom[b].rotation).GetComponentInChildren<ParticleSystem>();
                        leakadd.transform.SetParent(thisprop.transform);
                        leakadd.transform.localScale = new Vector3(1, 1, 1);

                        thisprop.contain.leaks.Add(leakadd);
                        thisprop.contain.leakindex.Add(b);
                        networkdata.leaklist.Add(leakadd);
                    }
                }

                    //}
               // }
            }
        }

        networkdata.networksize--;
        networkdata.powerstabledelta -= removedobject.pdelta;
        networkdata.powerstorage -= removedobject.pstorage;
        networkdata.power -= removedobject.power;


        //create temp network with first connection
        //Debug.Log("notfound: " + notfound);
        var tempnetwork = new Network { };
        tempnetwork.Contents = new List<Contain>();
        tempnetwork.powerscale = new List<Transform>();
        tempnetwork.leaklist = new List<ParticleSystem>();
        while (notfound > 0)
        {
            // Debug.Log("notfound: " + notfound);
            //Debug.Log("seachqueue: " + searchqueue.Count);
            if (searchqueue.Count == 0) //if no more options, finalize that network and go to next connection with new temp network
            {
                separated = true; //not all connected
                notfound--;

                foreach (Contain assemble in searched) //finalize network using searched
                {
                    AddData(tempnetwork, assemble);
                }
                allnetworks.Add(tempnetwork); //finalize network

                searched.Clear(); //set up next connection with new temp network
                searchqueue.AddRange(searchfor[0].connectedto);
                searched.Add(searchfor[0]);
                searchfor.RemoveAt(0);

                //Debug.Log("size: " + tempnetwork.networksize);

                //new network
                tempnetwork = new Network { };
                tempnetwork.Contents = new List<Contain>();
                tempnetwork.powerscale = new List<Transform>();
                tempnetwork.leaklist = new List<ParticleSystem>();
                if (searchqueue.Count == 0 && notfound == 0)
                {
                    break;
                }
            }
            else
            {
                searched.Add(searchqueue[0]); //search
                                              // Debug.Log("here " + searchqueue[0].connectedto.Count);
                var tempqueue = new List<Contain>();
                var tempsearchfor = new List<Contain>();
                foreach (Contain foundconnections in searchqueue[0].connectedto)
                {
                    if (searchfor.Contains(foundconnections)) //og connection found, remove it from search and continue
                    {
                        Debug.Log("Found og connection");
                        notfound--;
                        //searchfor.Remove(foundconnections);
                        tempsearchfor.Add(foundconnections);
                    }
                    if (!searched.Contains(foundconnections) && !searchqueue.Contains(foundconnections)) //add connection if not already added
                    {
                        tempqueue.Add(foundconnections);
                        //searchqueue.Add(foundconnections);
                    }
                    //Debug.Log("here " + searchqueue[0].connectedto.Count);
                }

                //searchfor.Remove(tempsearchfor);
                searchfor.RemoveAll(item => tempsearchfor.Contains(item));
                searchqueue.AddRange(tempqueue);
                searchqueue.RemoveAt(0);
                //Debug.Log("searchqueue: " + searchqueue.Count);
            }
            //Debug.Log("searchqueue: " + searchqueue.Count);
        }
        //Debug.Log("exit");
        if (separated) //not all og found, search for rest and finalize network
        {
            //Debug.Log("separated");
            while (searchqueue.Count > 0) //search for rest
            {
                //search
                searched.Add(searchqueue[0]);
                foreach (Contain foundconnections in searchqueue[0].connectedto) //search
                {
                    if (searchfor.Contains(foundconnections)) //og connection found, remove it from search and continue
                    {
                        notfound--;
                        searchfor.Remove(foundconnections);
                    }
                    if (!searched.Contains(foundconnections) && !searchqueue.Contains(foundconnections)) //add connection if not already added
                    {
                        searchqueue.Add(foundconnections);
                    }
                }
                searchqueue.RemoveAt(0);
            }
            if (searchqueue.Count == 0) //if no more options, finalize that network and go to next connection with new temp network
            {
                foreach (Contain assemble in searched)  //MIGHT NOT BE DONE
                {
                    AddData(tempnetwork, assemble);
                }
                allnetworks.Add(tempnetwork); //finalized
                allnetworks.Remove(networkdata);                                                           //memory leak? LOOK INTO LATER
                //Debug.Log("size: " + tempnetwork.networksize);
            }
        }
        Debug.Log("network count: " + allnetworks.Count);
    }
}
