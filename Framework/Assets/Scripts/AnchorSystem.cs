using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class AnchorSystem : MonoBehaviour
{
    public static AnchorSystem AnchorManager;
    //[SerializeField]
    //public List<AnchorGrid> allnetworks = new List<AnchorGrid>();
    public List<Chunk> allchunks = new List<Chunk>();
    public List<NetworkIdentity> unassignedchunks = new List<NetworkIdentity>();
    public List<uint> netIDs = new List<uint>();
    public List<int> replaceIDs = new List<int>();
    public GameObject terrain;
    public GameObject networkchunkobject;

    public List<int> tempchunkIDs = new List<int>();
    public List<NetworkIdentity> decomissionedchunks = new List<NetworkIdentity>();

    //public List<Chunk> roguechunks = new List<Chunk>;
    //chunks
    //objects

    System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

    void Awake()
    {
        if (AnchorManager == null)
        {
            AnchorManager = this;
            Debug.Log("Initialized Anchor Manager");
        }
        else
        {
            if (!GameSystem.GameManager || !GameSystem.GameManager.isServer)
            {
                NetworkIdentity serverchunk = GetComponent<NetworkIdentity>();
                if (serverchunk)
                {
                    /* if(serverchunk.isServer)
                     {
                         Debug.Log("Im server");
                     }*/
                    Debug.Log("Unassigned chunk: " + GetComponent<NetworkIdentity>().netId);
                    AnchorManager.unassignedchunks.Add(serverchunk);
                }
                else
                {
                    Debug.LogWarning("ERROR: network identity not found for: " + transform.name);
                }
            }
            Destroy(this);
        }
        /*AnchorManager.CreateObject(terrain.GetComponent<EntityProperties>());
        terrain.GetComponent<EntityProperties>().anchordata.anchor = true;
        AnchorManager.UpdateAnchor(terrain.GetComponent<EntityProperties>().anchordata.chunk);*/

        //Debug.Log(terrain.GetComponent<EntityProperties>().anchordata.chunk.contents);
    }


    public class AnchorGrid
    {
        public List<Chunk> contents;
        public int gridsize;

        public bool anchored;
    }
    public class Chunk
    {
        public int anchorID;
        public List<Object> contents;
        public int chunksize;

        public bool anchorsource;
        public bool anchored;

        public List<Chunk> connectedto;

        public bool rigidbodysource;
        public Transform chunkobject;
        public Rigidbody physicsproperties;
        public float mass;
    }
    public class Object
    {
        public bool searched;

        public Chunk chunk;
        public List<Object> connectedto;
        public Transform objecttransform;
        public EntityProperties objectproperties;

        public bool anchor;

        //public Rigidbody physicsproperties;
        public float mass;
    }

    public void CreateObject(EntityProperties addedobject, List<Object> optionalconnections = null, Chunk optionalchunk = null) //find connected to
    {
        Object objectdata = new Object { };
        objectdata.connectedto = new List<Object>();
        Chunk chunkdata = new Chunk { };
        bool existingnetwork = false;

        addedobject.anchordata = objectdata;
        objectdata.objectproperties = addedobject;
        objectdata.objecttransform = addedobject.transform;
        Rigidbody temprigidbody = addedobject.transform.GetComponentInParent<Rigidbody>();
        //Debug.Log(temprigidbody);
        objectdata.mass = temprigidbody.mass;
        Destroy(temprigidbody);

        if(optionalchunk != null)
        {
            //Debug.Log("added to chunk");
            existingnetwork = true;
            chunkdata = optionalchunk;
            objectdata.chunk = chunkdata;
            //AddObject(optionalchunk, objectdata);
            //return;
        }

        List<BoxCollider> checkbox = new List<BoxCollider>();
        foreach (Transform transformfind in addedobject.transform.GetComponentsInChildren<Transform>())
        {
            if(transformfind.tag == "WeldingBox")
            {
                checkbox.AddRange(transformfind.GetComponentsInChildren<BoxCollider>());
                //Debug.Log("welding");
            }
        }

        int mask2 = (1 << 6) | (1 << 10); //building and ground
        foreach (BoxCollider checkhitbox in checkbox)
        {
            if (true)
            {
                Vector3 worldCenter = checkhitbox.transform.TransformPoint(checkhitbox.center);
                Vector3 worldHalfExtents = checkhitbox.size * 0.5f; // only necessary when collider is scaled by non-uniform transform
                Collider[] overlapcol = Physics.OverlapBox(worldCenter, worldHalfExtents, checkhitbox.transform.rotation, mask2, QueryTriggerInteraction.Ignore);


                /*GameObject cube1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube1.transform.position = worldCenter;
                cube1.transform.localScale = worldHalfExtents * 2;
                cube1.transform.rotation = checkhitbox.transform.rotation;
                cube1.GetComponent<Renderer>().material.color = Color.red;
                */

                foreach (Collider intersect in overlapcol)
                {
                    EntityProperties collidedproperties = intersect.GetComponentInParent<EntityProperties>();
                    Object anchordata = null;
                    BoxCollider boxintersect = intersect as BoxCollider;
                    if (collidedproperties != null && collidedproperties.anchordata != null)
                    {
                        anchordata = collidedproperties.anchordata;
                        if (anchordata != objectdata && !objectdata.connectedto.Contains(anchordata))
                        {
                            objectdata.connectedto.Add(anchordata);
                        }
                    }
                }
            }
        }
        for (int j = 0; j < checkbox.Count; j++) //clean up welding box
        {
            if (checkbox[j] != null)
            {
                Destroy(checkbox[j].gameObject);
                checkbox[j] = null;
            }
        }

        if (optionalconnections != null && optionalconnections.Count > 0)
        {
            existingnetwork = true;
            chunkdata = optionalconnections[0].chunk;
            objectdata.chunk = chunkdata;

            for (int n = 0; n < optionalconnections.Count; n++) //clean up welding box
            {
                if (optionalconnections[n] != objectdata && !objectdata.connectedto.Contains(optionalconnections[n]))
                {
                    Debug.Log("added connection: " + optionalconnections[n].objecttransform.name);
                    objectdata.connectedto.Add(optionalconnections[n]);

                    Chunk connectedchunk = optionalconnections[n].chunk;

                    if (connectedchunk != null)
                    {
                        if (connectedchunk != chunkdata)
                        {
                            if (true) //connectedchunk.chunksize < 10)
                            {
                                Debug.Log("combine network");
                                if (chunkdata.chunksize > connectedchunk.chunksize)
                                {
                                    CombineChunk(chunkdata, connectedchunk);
                                }
                                else
                                {
                                    CombineChunk(connectedchunk, chunkdata);
                                    chunkdata = connectedchunk;
                                    objectdata.chunk = connectedchunk;
                                }
                            }
                            else if (!chunkdata.connectedto.Contains(connectedchunk))
                            {
                                chunkdata.connectedto.Add(connectedchunk);
                                connectedchunk.connectedto.Add(chunkdata);
                            }
                        }
                    }
                }
            }
        }
        
        foreach (Object connected in objectdata.connectedto)
        {
            if (connected != null)
            {
                Chunk connectedchunk = connected.chunk;
                connected.connectedto.Add(objectdata);
                if (connectedchunk != null)
                {
                    if (!existingnetwork)
                    {
                        existingnetwork = true;
                        chunkdata = connectedchunk;
                        objectdata.chunk = connectedchunk;
                    }
                    else if (connectedchunk != chunkdata)
                    {
                        if (true) //connectedchunk.chunksize < 10)
                        {
                            //Debug.Log("combine network");
                            if (chunkdata.chunksize > connectedchunk.chunksize)
                            {
                                CombineChunk(chunkdata, connectedchunk);
                            }
                            else
                            {
                                CombineChunk(connectedchunk, chunkdata);
                                chunkdata = connectedchunk;
                                objectdata.chunk = connectedchunk;
                            }
                        }
                        else if(!chunkdata.connectedto.Contains(connectedchunk))
                        {
                            chunkdata.connectedto.Add(connectedchunk);
                            connectedchunk.connectedto.Add(chunkdata);
                        }
                    }
                }
            }
        }

        if (!existingnetwork) //hellohi
        {
            chunkdata.connectedto = new List<Chunk>();
            chunkdata.contents = new List<Object>();
            chunkdata.rigidbodysource = true;

            if (GameSystem.GameManager.isServer)
            {
                GameObject chunkRB = null;

                objectdata.objectproperties.anchordata.chunk = chunkdata;

                GameSystem.GameManager.GenerateID(objectdata.objectproperties, -1, 3);

                //GameSystem.GameManager.anchorprefab
                chunkRB = Instantiate(GameSystem.GameManager.anchorprefab);
                NetworkServer.Spawn(chunkRB, NetworkClient.localPlayer.connectionToClient);
                //GameNetworkManager.

                AnchorManager.netIDs.Add(chunkRB.GetComponent<NetworkIdentity>().netId);

                Debug.Log("NEW CHUNK");
                chunkdata.rigidbodysource = true;
                chunkdata.chunkobject = chunkRB.transform;
                chunkdata.physicsproperties = chunkRB.GetComponent<Rigidbody>();


                chunkdata.anchored = true;
                chunkdata.anchorsource = true;
                UpdateAnchor(chunkdata);
            }
            else
            {
                Debug.Log("client chunk");
                GameObject rigidbodyholder = new GameObject("Chunk");
                //GameObject rigidbodyholder = Instantiate(networkchunkobject, Vector3.zero, Quaternion.identity);
                chunkdata.chunkobject = rigidbodyholder.transform;
                chunkdata.physicsproperties = rigidbodyholder.AddComponent<Rigidbody>();
            }

            //chunkdata.physicsproperties.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

            allchunks.Add(chunkdata);
           // Debug.Log("stuff");
        }
        //chunkdata.physicsproperties.position = -chunkdata.physicsproperties.position;
        //chunkdata.physicsproperties.position = -chunkdata.physicsproperties.position;

        /*foreach (Object connectionsthing in objectdata.connectedto)
        {
            Debug.Log(connectionsthing.objectproperties.gameObject.name);
        }*/

        AddObject(chunkdata, objectdata);

        //Debug.Log("network count: " + allnetworks.Count);
        //Debug.Log("leak count: " + networkdata.leaklist.Count);
    }
    public void AddObject(Chunk chunkdata, Object addedobject)
    {
        //Debug.Log(addedobject.objecttransform.gameObject);
        //Debug.Log(chunkdata.chunkobject);
        chunkdata.contents.Add(addedobject);
        //Debug.Log(addedobject.objecttransform);
        addedobject.objecttransform.SetParent(chunkdata.chunkobject.transform);
        chunkdata.chunksize++;
        chunkdata.mass += addedobject.mass;
        chunkdata.physicsproperties.mass += addedobject.mass;
        if(addedobject.anchor)
        {
            UpdateAnchor(chunkdata);
            //chunkdata.anchorsource = true;
        }
        if (chunkdata.rigidbodysource)
        {
            //chunkdata.physicsproperties.mass = chunkdata.mass; //+connected mass
        }
        addedobject.chunk = chunkdata;
        if (chunkdata.contents.Count > 30) //if chunk too big, split into 2
        {
            RestructureChunk(); //doesnt do shit right now
        }
        if(!chunkdata.anchored)
        {
            chunkdata.physicsproperties.WakeUp();
        }
    }
    public void CombineChunk(Chunk chunk1, Chunk chunk2)
    {
        /* if(chunk2.anchorsource)
         {
             chunk1.anchorsource = true;
             chunk1.anchored = true;
         }
         else if(chunk2.anchored)
         {
             chunk1.anchored = true;
         }*/
        chunk1.physicsproperties.velocity = (chunk1.physicsproperties.velocity * chunk1.mass + chunk2.physicsproperties.velocity * chunk2.mass) / (chunk1.mass + chunk2.mass);

        foreach (Object content in chunk2.contents)
        {
            AddObject(chunk1, content);
            //content.chunk = chunk1;
        }
        //chunk1.contents.AddRange(chunk2.contents);
        chunk1.connectedto.Remove(chunk2);
        foreach (Chunk checkchunk in chunk2.connectedto)
        {
            checkchunk.connectedto.Remove(chunk2);
            if(!checkchunk.connectedto.Contains(chunk1))
            {
                checkchunk.connectedto.Add(chunk1);
            }
            if (!chunk1.connectedto.Contains(checkchunk) && checkchunk != chunk1)
            {
                chunk1.connectedto.Add(checkchunk);
            }
        }
        //chunk1.chunksize += chunk2.chunksize;
        //chunk1.mass += chunk2.mass;
        if(GameSystem.GameManager.isServer)
        {
            Destroy(chunk2.chunkobject.gameObject);
            /*decomissionedchunks.Add(chunk2.chunkobject.GetComponent<NetworkIdentity>());
            chunk2.physicsproperties.isKinematic = true;*/
            netIDs.Remove(chunk2.chunkobject.GetComponent<NetworkIdentity>().netId);
        }
        else
        {
            Destroy(chunk2.chunkobject.gameObject);
        }
        //Destroy(chunk2.chunkobject.gameObject);
        allchunks.Remove(chunk2);
    }
    public void RemoveObject(Object removedobject)
    {
        var chunkdata = removedobject.chunk;

        if (chunkdata.chunksize == 1)
        {
            Debug.Log("delete chunk");
            chunkdata.contents.Remove(removedobject);
            foreach (Chunk removeconnections in chunkdata.connectedto) //remove old chunk connections
            {
                removeconnections.connectedto.Remove(chunkdata);
                chunkdata.connectedto.Remove(removeconnections);
            }
            allchunks.Remove(chunkdata);                                                           //memory leak? LOOK INTO LATER
            Destroy(chunkdata.chunkobject.gameObject);
            removedobject.objectproperties.anchordata = null;
            removedobject.chunk = null;
            return;
        }
        foreach (Object removeconnections in removedobject.connectedto) //remove og object connections
        {
            //Debug.Log("next: "+ removeconnections.objecttransform.name + ", " + removeconnections.objecttransform.position);
            foreach (Object removeconnections2 in removeconnections.connectedto) //remove og object connections
            {
                //Debug.Log(removeconnections2.objecttransform.name + ", " + removeconnections2.objecttransform.position);
            }
            removeconnections.connectedto.Remove(removedobject); //ISSUE
        }
       //Debug.Log("CONNECTIONS DONE");
        removedobject.objecttransform.SetParent(null);
        var searchqueue = new List<Object>();
        var searchfor = new List<Object>();
        var searched = new List<Object>();

        bool separated = false;
        bool chunkreused = false;
        searchfor.AddRange(removedobject.connectedto);
        Debug.Log(removedobject.connectedto.Count);

        searchqueue.Add(searchfor[0]); //set up first connection

        //searchqueue.AddRange(searchfor[0].connectedto); //set up first connection
        //searched.Add(searchfor[0]);
        //searchfor.RemoveAt(0);

        int notfound = searchfor.Count;
        //Debug.Log("notfound: " + notfound);

        chunkdata.contents.Remove(removedobject); //remove og object

        chunkdata.chunksize--;
        chunkdata.mass -= removedobject.mass;
        //chunkdata.physicsproperties.mass -= removedobject.mass;

        chunkdata.physicsproperties.WakeUp();

        //create temp network with first connection
        var tempchunk = new Chunk { };
        tempchunk.contents = new List<Object>();
        tempchunk.connectedto = new List<Chunk>();

        foreach (Object searchobject in searchfor)
        {
            //Debug.Log(searchobject.objecttransform.name + ", " + searchobject.objecttransform.position);
        }

        foreach (Object searchobject2 in searchqueue)
        {
            notfound--;
            searchfor.Remove(searchobject2);
            //Debug.Log("Found og connection: " + searchobject2.objecttransform.name + ", " + searchobject2.objecttransform.position);
        }
        //Debug.Log(notfound);
        foreach (Object searchobject in searchfor)
        {
            //Debug.Log(searchobject.objecttransform.name + " connections:");
            foreach (Object searchobjectconnections in searchobject.connectedto)
            {
               //Debug.Log(searchobjectconnections.objecttransform.name);
            }
        }
        while (notfound > 0)
        {
            if (searchqueue.Count == 0) //if no more options, finalize that network and go to next connection with new temp network
            {
                //Debug.Log("blank");
                separated = true; //not all connected
                notfound--;

                if (searched.Count * 2 > chunkdata.contents.Count)
                {
                    Debug.Log("chunk reuse");
                    chunkreused = true;

                    tempchunk.rigidbodysource = true;
                    tempchunk.chunkobject = chunkdata.chunkobject;
                    tempchunk.physicsproperties = chunkdata.physicsproperties;
                    tempchunk.physicsproperties.mass = .01f;
                    tempchunk.mass = 0f;
                    tempchunk.anchorID = chunkdata.anchorID;

                    foreach (Object assemble in searched) //finalize network using searched
                    {
                        AddObject(tempchunk, assemble);
                    }

                    tempchunk.anchored = true;
                    tempchunk.anchorsource = true;
                    UpdateAnchor(tempchunk);
                    allchunks.Add(tempchunk);
                }
                else
                {
                    NewChunkRB(tempchunk, chunkdata, searched);
                }

                searched.Clear(); //set up next connection with new temp network
                searchqueue.AddRange(searchfor[0].connectedto);
                searched.Add(searchfor[0]);
                searchfor.RemoveAt(0);

                //new network
                tempchunk = new Chunk { };
                tempchunk.contents = new List<Object>();
                tempchunk.connectedto = new List<Chunk>();

                if (searchqueue.Count == 0 && notfound == 0)
                {
                    break;
                }
            }
            else
            {
                searched.Add(searchqueue[0]); //search

                var tempqueue = new List<Object>();
                var tempsearchfor = new List<Object>();
                //Debug.Log(searchqueue[0]);
                //Debug.Log(searchqueue[0].connectedto);
                foreach (Object foundconnections in searchqueue[0].connectedto) //ISSUE
                {
                    if (searchfor.Contains(foundconnections)) //og connection found, remove it from search and continue
                    {
                        //Debug.Log("Found og connection: " + foundconnections.objecttransform.name + ", " + foundconnections.objecttransform.position);
                        foreach (Object searchobject in searchfor)
                        {
                           // Debug.Log(searchobject.objecttransform.name + ", " + searchobject.objecttransform.position + " left");
                        }

                        notfound--;
                        tempsearchfor.Add(foundconnections);
                        if(!searched.Contains(foundconnections) && !searchqueue.Contains(foundconnections))
                        {
                            tempqueue.Add(foundconnections);
                        }
                    }
                    else if (!searched.Contains(foundconnections) && !searchqueue.Contains(foundconnections)) //add connection if not already added
                    {
                        tempqueue.Add(foundconnections);
                    }
                }
                foreach(Object found in tempsearchfor)
                {
                    searchfor.Remove(found);
                }
                //searchfor.RemoveAll(item => tempsearchfor.Contains(item));
                //Debug.Log(searchqueue[0].objecttransform.name + ", " + searchqueue[0].objecttransform.position);
                searchqueue.AddRange(tempqueue);
                searchqueue.RemoveAt(0);
                //tempchunk = null;
            }
        }
        //Debug.Log("exit");
        if (separated) //not all og found, search for rest and finalize network
        {
            Debug.Log("separated");
            while (searchqueue.Count > 0) //search for rest
            {
                //search
                searched.Add(searchqueue[0]);
                foreach (Object foundconnections in searchqueue[0].connectedto) //search
                {
                    if (searchfor.Contains(foundconnections)) //og connection found, remove it from search and continue
                    {
                        notfound--;
                        searchfor.Remove(foundconnections);
                        if(!searched.Contains(foundconnections) && !searchqueue.Contains(foundconnections))
                        {
                            searchqueue.Add(foundconnections);
                        }
                    }
                    if (!searched.Contains(foundconnections) && !searchqueue.Contains(foundconnections)) //add connection if not already added
                    {
                        searchqueue.Add(foundconnections);
                    }
                }
                searchqueue.RemoveAt(0);
            }
            if (searchqueue.Count == 0) //finalize last network
            {

               /* foreach (Object assemble in searched) //finalize network using searched
                {
                    AddObject(tempchunk, assemble);
                }*/

                if(chunkreused)
                {
                    NewChunkRB(tempchunk, chunkdata, searched);
                }
                else
                {
                    tempchunk.rigidbodysource = true;
                    tempchunk.chunkobject = chunkdata.chunkobject;
                    tempchunk.physicsproperties = chunkdata.physicsproperties;
                    tempchunk.physicsproperties.mass = .01f;
                    tempchunk.mass = 0f; 
                    tempchunk.anchorID = chunkdata.anchorID;

                    foreach (Object assemble in searched) //finalize network using searched
                    {
                        AddObject(tempchunk, assemble);
                    }

                    tempchunk.anchored = true;
                    tempchunk.anchorsource = true;
                    UpdateAnchor(tempchunk);
                    allchunks.Add(tempchunk);
                }


                foreach (Chunk removeconnections in chunkdata.connectedto) //remove old chunk connections
                {
                    removeconnections.connectedto.Remove(chunkdata);
                    chunkdata.connectedto.Remove(removeconnections);
                }
                allchunks.Remove(chunkdata);                                                           //memory leak? LOOK INTO LATER
                //Destroy(chunkdata.chunkobject.gameObject);


               // NewChunkRB(tempchunk, chunkdata, searched);
            }
        }
        else
        {
            //Debug.Log(chunkdata.mass + ", " + chunkdata.physicsproperties.mass);
            UpdateAnchor(chunkdata);
        }
        //Debug.Log("network count: " + allchunks.Count);
        Physics.SyncTransforms();
    }
    public void UpdateAnchor(Chunk anchorchunk) //search chunk for anchor, update anchor source
    {
        //Chunk tempchunk = anchorobject.chunk;
        //Debug.Log("Anchor update");
        bool anchorfound = false;

        anchorchunk.physicsproperties.mass = anchorchunk.mass;

        anchorchunk.physicsproperties.MovePosition(anchorchunk.chunkobject.position);
        anchorchunk.physicsproperties.MoveRotation(anchorchunk.chunkobject.rotation);

        foreach (Object objectcheck in anchorchunk.contents)
        {
            if (objectcheck.anchor)
            {
                anchorfound = true;
                //Debug.Log("anchor found: " + objectcheck.objecttransform.name);
                //anchorchunk.anchorsource = true;
                break;
            }
        }
        if (!anchorchunk.anchorsource)
        {

        }

        if (!anchorchunk.anchored && anchorfound) //unanchored -> anchored
        {
            anchorchunk.anchorsource = true;
            anchorchunk.anchored = true;

            anchorchunk.physicsproperties.isKinematic = true; //ANCHORED
            //anchorchunk.physicsproperties.useGravity = false;
            List<Chunk> tempchunks = new List<Chunk>();
            tempchunks.AddRange(anchorchunk.connectedto); //recursively anchor connected chunks
            while (tempchunks.Count > 0)
            {
                if (!tempchunks[0].anchored)
                {
                    tempchunks[0].anchored = true;
                    foreach (Chunk checkchunk in tempchunks[0].connectedto)
                    {
                        if (!checkchunk.anchored)
                        {
                            tempchunks.Add(checkchunk);
                        }
                    }
                }
                tempchunks.RemoveAt(0);
            }
        }
        else if (anchorchunk.anchorsource && !anchorfound) //anchored -> unanchored
        {
            anchorchunk.anchorsource = false;

            bool chunkanchor = false;
            List<Chunk> searchedchunks = new List<Chunk>();
            List<Chunk> tempchunk = new List<Chunk>();
            tempchunk.Add(anchorchunk); //search connected chunks for anchor
            while (tempchunk.Count > 0 && !chunkanchor)
            {
                if (tempchunk[0].anchorsource)
                {
                    chunkanchor = true;
                    anchorchunk.anchored = true;
                }
                else
                {
                    foreach (Chunk checkchunk in tempchunk[0].connectedto)
                    {
                        if (!tempchunk.Contains(checkchunk) && !searchedchunks.Contains(checkchunk))
                        {
                            tempchunk.Add(checkchunk);
                        }
                    }
                }
                searchedchunks.Add(tempchunk[0]);
                tempchunk.RemoveAt(0);
            }
            if (!chunkanchor)
            {
                anchorchunk.physicsproperties.isKinematic = false; //UNANCHORED
                //anchorchunk.physicsproperties.useGravity = true;
                foreach (Chunk unanchorchunk in searchedchunks)
                {
                    unanchorchunk.anchored = false;
                }
            }
        }
    }
    public void AddConnections(Object objectdata, List<Object> connections)
    {
        //objectdata.connectedto = new List<Object>();
        Chunk chunkdata = objectdata.chunk;

    

       /* foreach (Object connectionsthing in objectdata.connectedto)
        {
            //Debug.Log(connectionsthing.objectproperties.gameObject.name);
        }*/

        foreach (Object anchordata in connections)
        {
            //Debug.Log(anchordata.objectproperties.gameObject.name);
            if (anchordata != objectdata && !objectdata.connectedto.Contains(anchordata))
            {
                objectdata.connectedto.Add(anchordata);

                Chunk connectedchunk = anchordata.chunk;
                if (!anchordata.connectedto.Contains(objectdata))
                {
                    anchordata.connectedto.Add(objectdata);
                }
                if (connectedchunk != null)
                {
                    if (connectedchunk != chunkdata)
                    {
                        if (true) //connectedchunk.chunksize < 10)
                        {
                            Debug.Log("combine network");
                            if (chunkdata.chunksize > connectedchunk.chunksize)
                            {
                                CombineChunk(chunkdata, connectedchunk);
                            }
                            else
                            {
                                CombineChunk(connectedchunk, chunkdata);
                                chunkdata = connectedchunk;
                                objectdata.chunk = connectedchunk;
                            }
                        }
                        else if (!chunkdata.connectedto.Contains(connectedchunk))
                        {
                            chunkdata.connectedto.Add(connectedchunk);
                            connectedchunk.connectedto.Add(chunkdata);
                        }
                    }
                }
            }
        }

        foreach (Object connectionsthing in objectdata.connectedto)
        {
            //Debug.Log(connectionsthing.objectproperties.gameObject.name);
        }
    }
    private void RestructureChunk() //idk
    {

    }

    private void NewChunkRB(Chunk tempchunk, Chunk chunkdata, List<Object> searched)//, out GameObject chunkRB)  REVIEW
    {
        GameObject chunkRB = null;
        bool clientchunk = false;

        if (GameSystem.GameManager.isServer)
        {
            Debug.Log("SERVER");

            //int chunkentityid = searched[0].objectproperties.entityID; //chunkdata.contents[0].objectproperties.entityID;
            EntityProperties chunkentity = searched[0].objectproperties; // GameSystem.GameManager.entityIDs[chunkentityid];

            Debug.Log(chunkdata.anchorID + ", " + chunkdata.contents[0].chunk.anchorID + ": " + (chunkdata.contents[0].chunk == chunkdata));


            int oldid = searched[0].chunk.anchorID; //.chunkdata.anchorID;
            GameSystem.GameManager.GenerateID(chunkentity, -1, 3);
            //Debug.Log(searched[0].chunk.anchorID + ", " + chunkentity.anchordata.chunk.anchorID);
            //Debug.Log(oldid + ", " + tempchunk.anchorID);
            tempchunk.anchorID = searched[0].chunk.anchorID;
            GameSystem.GameManager.anchorIDs.Remove(chunkentity.anchordata.chunk.anchorID);
            GameSystem.GameManager.anchorIDs.Add(tempchunk.anchorID, tempchunk);
            searched[0].chunk.anchorID = oldid;
            replaceIDs.Add(tempchunk.anchorID);
            Debug.Log(tempchunk.anchorID);
            //Debug.Log(chunkentity.anchordata.chunk.anchorID);
            //Debug.Log(oldid + ", " + tempchunk.anchorID);
            //Debug.Log(tempchunk == searched[0].chunk);

            //GameSystem.GameManager.anchorprefab
            chunkRB = Instantiate(GameSystem.GameManager.anchorprefab);
            NetworkServer.Spawn(chunkRB, NetworkClient.localPlayer.connectionToClient);
            if (NetworkClient.localPlayer)
            {
               // chunkRB.GetComponent<NetworkIdentity>().AssignClientAuthority(NetworkClient.connection);
            }
            //GameNetworkManager.

            netIDs.Add(chunkRB.GetComponent<NetworkIdentity>().netId);

            //GameSystem.GameManager.ServerRB(chunkdata.contents[0].objectproperties.entityID, -1, out _);
            //chunkRB = unassignedchunks[0].gameObject;
        }
        else
        {
            foreach (NetworkIdentity networkchunk in unassignedchunks)
            {
                //Debug.Log(networkchunk.netId + ", " + netIDs[0]);
                if (netIDs != null && netIDs.Count > 0 && networkchunk.netId == netIDs[0]) //search unassigned chunks for netid in order
                {
                    chunkRB = networkchunk.gameObject;
                    tempchunk.anchorID = replaceIDs[0];
                    GameSystem.GameManager.anchorIDs.Add(tempchunk.anchorID, tempchunk);
                    replaceIDs.RemoveAt(0);
                    netIDs.RemoveAt(0);
                    break;
                }
            }

            if (chunkRB == null)
            {
                chunkRB = new GameObject("Chunk");
                chunkRB.AddComponent<Rigidbody>();
                clientchunk = true;
                Debug.Log("no chunk");
            }
            else
            {
                Debug.Log("remove unassigned");
                unassignedchunks.Remove(chunkRB.GetComponent<NetworkIdentity>());
                netIDs.Remove(chunkRB.GetComponent<NetworkIdentity>().netId);
            }
        }

        Debug.Log("NEW CHUNK");
        tempchunk.rigidbodysource = true;
        tempchunk.chunkobject = chunkRB.transform;
        tempchunk.physicsproperties = chunkRB.GetComponent<Rigidbody>();

        Vector3 serverposition = tempchunk.physicsproperties.position;
        Quaternion serverrotation = tempchunk.physicsproperties.rotation;

        chunkdata.physicsproperties.MovePosition(tempchunk.physicsproperties.position);
        chunkdata.physicsproperties.MoveRotation(tempchunk.physicsproperties.rotation);
        //tempchunk.physicsproperties.MovePosition(chunkdata.physicsproperties.position);
        //tempchunk.physicsproperties.MoveRotation(chunkdata.physicsproperties.rotation);

        foreach (Object assemble in searched) //finalize network using searched
        {
            AddObject(tempchunk, assemble);
            chunkdata.mass -= assemble.mass;
            chunkdata.chunksize--;
            chunkdata.contents.Remove(assemble);
        }
        chunkdata.physicsproperties.WakeUp();

        if (tempchunk.chunkobject.GetComponent<NetworkIdentity>())
        {
            //tempchunk.physicsproperties.MovePosition(serverposition);
           // tempchunk.physicsproperties.MoveRotation(serverrotation);
            //enable ownership
        }

        tempchunk.physicsproperties.AddForce(chunkdata.physicsproperties.velocity * tempchunk.physicsproperties.mass);
        tempchunk.physicsproperties.velocity = chunkdata.physicsproperties.velocity;
        tempchunk.anchored = true;
        tempchunk.anchorsource = true;
        UpdateAnchor(tempchunk);
        allchunks.Add(tempchunk);
        if(clientchunk)
        {
            GameSystem.GameManager.GenerateID(tempchunk.contents[0].objectproperties, -1, 3);
            tempchunkIDs.Add(tempchunk.anchorID);
        }
        Debug.Log(tempchunk.anchorID + tempchunk.contents[0].objecttransform.name);
    }






    /*public void DebugStart(EntityProperties objecttest)
    {
        StartCoroutine(DebugTest(objecttest));
    }

    public IEnumerator DebugTest(EntityProperties objecttest)
    {
        stopwatch.Start();


        GameObject chunkRB = new GameObject("Chunk");
        chunkRB.AddComponent<Rigidbody>();

        Chunk chunkdata2 = objecttest.anchordata.chunk;
        Chunk tempchunk2 = new Chunk { };
        tempchunk2.contents = new List<Object>();
        tempchunk2.connectedto = new List<Chunk>();

        tempchunk2.rigidbodysource = true;
        tempchunk2.chunkobject = chunkRB.transform;
        tempchunk2.physicsproperties = chunkRB.GetComponent<Rigidbody>();

        Vector3 serverposition = tempchunk2.physicsproperties.position;
        Quaternion serverrotation = tempchunk2.physicsproperties.rotation;

        tempchunk2.physicsproperties.MovePosition(chunkdata2.physicsproperties.position);
        tempchunk2.physicsproperties.MoveRotation(chunkdata2.physicsproperties.rotation);

        if (tempchunk2.chunkobject.GetComponent<NetworkIdentity>())
        {
            tempchunk2.physicsproperties.MovePosition(serverposition);
            tempchunk2.physicsproperties.MoveRotation(serverrotation);
            //enable ownership
        }

        List<Object> searched2 = chunkdata2.contents;
        foreach (Object assemble in searched2) //finalize network using searched
        {
            AddObject(tempchunk2, assemble);
        }

        tempchunk2.physicsproperties.AddForce(chunkdata2.physicsproperties.velocity * tempchunk2.physicsproperties.mass);
        tempchunk2.physicsproperties.velocity = chunkdata2.physicsproperties.velocity;
        tempchunk2.anchored = false;
        UpdateAnchor(tempchunk2);
        allchunks.Add(tempchunk2);

        foreach (Chunk removeconnections in chunkdata2.connectedto) //remove old chunk connections
        {
            chunkdata2.connectedto.Remove(chunkdata2);
        }
        allchunks.Remove(chunkdata2);                                                           //memory leak? LOOK INTO LATER
        Destroy(chunkdata2.chunkobject.gameObject);


        Debug.Log("Time to move rigidbody: " + string.Format("{0:0}.{1:00}", stopwatch.Elapsed.Seconds, stopwatch.Elapsed.Milliseconds / 10) + " seconds");
       // yield return StartCoroutine(WaitFrame(stopwatch));
        stopwatch.Reset();
        yield return null;
        stopwatch.Start();


        for (int i = 0; i < 10; i++)
        {
            Object removedobject = objecttest.anchordata;
            var chunkdata = removedobject.chunk;

            var searchqueue = new List<Object>();
            var searchfor = new List<Object>();
            var searched = new List<Object>();

            searchfor.AddRange(removedobject.connectedto);


            searchqueue.Add(searchfor[0]); //set up first connection

            //searchqueue.AddRange(searchfor[0].connectedto); //set up first connection
            //searched.Add(searchfor[0]);
            //searchfor.RemoveAt(0);

            int notfound = searchfor.Count;
            //Debug.Log("notfound: " + notfound);

            chunkdata.physicsproperties.WakeUp();

            //create temp network with first connection
            var tempchunk = new Chunk { };
            tempchunk.contents = new List<Object>();
            tempchunk.connectedto = new List<Chunk>();

            foreach (Object searchobject in searchfor)
            {
                //Debug.Log(searchobject.objecttransform.name + ", " + searchobject.objecttransform.position);
            }

            foreach (Object searchobject2 in searchqueue)
            {
                notfound--;
                searchfor.Remove(searchobject2);
                //Debug.Log("Found og connection: " + searchobject2.objecttransform.name + ", " + searchobject2.objecttransform.position);
            }
            while (true)
            {
                if (searchqueue.Count == 0)
                {
                    break;
                }
                else
                {
                    searched.Add(searchqueue[0]); //search

                    var tempqueue = new List<Object>();
                    var tempsearchfor = new List<Object>();
                    //Debug.Log(searchqueue[0]);
                    //Debug.Log(searchqueue[0].connectedto);
                    foreach (Object foundconnections in searchqueue[0].connectedto) //ISSUE
                    {
                        //Debug.Log("search");
                        if (searchfor.Contains(foundconnections)) //og connection found, remove it from search and continue
                        {
                            notfound--;
                            tempsearchfor.Add(foundconnections);
                            if (!searched.Contains(foundconnections) && !searchqueue.Contains(foundconnections))
                            {
                                tempqueue.Add(foundconnections);
                            }
                        }
                        else if (!searched.Contains(foundconnections) && !searchqueue.Contains(foundconnections)) //add connection if not already added
                        {
                            tempqueue.Add(foundconnections);
                        }
                    }
                    foreach (Object found in tempsearchfor)
                    {
                        searchfor.Remove(found);
                    }
                    //searchfor.RemoveAll(item => tempsearchfor.Contains(item));
                    //Debug.Log(searchqueue[0].objecttransform.name + ", " + searchqueue[0].objecttransform.position);
                    searchqueue.AddRange(tempqueue);
                    searchqueue.RemoveAt(0);
                    //tempchunk = null;
                }
            }
            //Debug.Log("exit");


            UpdateAnchor(chunkdata);

            //Debug.Log("network count: " + allchunks.Count);
            Physics.SyncTransforms();
        }
        Debug.Log("Time to search chunk: " + string.Format("{0:0}.{1:00}", stopwatch.Elapsed.Seconds, stopwatch.Elapsed.Milliseconds / 10) + " seconds");
        //yield return StartCoroutine(WaitFrame(stopwatch));

        stopwatch.Reset();
        stopwatch.Stop();
    }

    IEnumerator WaitFrame(System.Diagnostics.Stopwatch stopwatch)
    {
        //Debug.Log((float)stopwatch.Elapsed.Milliseconds / 1000);
        Debug.Log("Time taken: " + string.Format("{0:0}.{1:00}", stopwatch.Elapsed.Seconds, stopwatch.Elapsed.Milliseconds / 10) + " seconds");
        stopwatch.Reset();
        yield return null;
        stopwatch.Start();
    }*/
}
/*ServerRB(out GameObject rigidbodyholder); //Sets new chunk RB to server RB

// GameObject rigidbodyholder = Instantiate(networkchunkobject, chunkdata.physicsproperties.position, chunkdata.physicsproperties.rotation);
Debug.Log("NEW CHUNK");
tempchunk.rigidbodysource = true;
tempchunk.chunkobject = rigidbodyholder.transform;
//tempchunk.physicsproperties = rigidbodyholder.GetComponent<Rigidbody>();
tempchunk.physicsproperties = rigidbodyholder.GetComponent<Rigidbody>();

Vector3 serverposition = tempchunk.physicsproperties.position;
Quaternion serverrotation = tempchunk.physicsproperties.rotation;

tempchunk.physicsproperties.MovePosition(chunkdata.physicsproperties.position);
tempchunk.physicsproperties.MoveRotation(chunkdata.physicsproperties.rotation);

if (tempchunk.chunkobject.GetComponent<NetworkIdentity>())
{
    tempchunk.physicsproperties.MovePosition(serverposition);
    tempchunk.physicsproperties.MoveRotation(serverrotation);
    //enable ownership
}
//tempchunk.physicsproperties.velocity = chunkdata.physicsproperties.velocity;
//tempchunk.physicsproperties.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
//chunkdata.physicsproperties.position = -chunkdata.physicsproperties.position;
//chunkdata.physicsproperties.position = -chunkdata.physicsproperties.position;

foreach (Object assemble in searched) //finalize network using searched
{
    AddObject(tempchunk, assemble);
}

tempchunk.physicsproperties.AddForce(chunkdata.physicsproperties.velocity * tempchunk.physicsproperties.mass);
tempchunk.physicsproperties.velocity = chunkdata.physicsproperties.velocity;
tempchunk.anchored = false;
UpdateAnchor(tempchunk);
allchunks.Add(tempchunk); //finalize network*/