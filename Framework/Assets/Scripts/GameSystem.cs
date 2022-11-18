using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GameSystem : NetworkBehaviour
{
    public static GameSystem GameManager;
    public string playername { get; set; }
    public int chosenteam { get; set; }
    public GameObject username;
    public GameObject teamUI;
    public List<GameObject> playernetworkobject;
    public List<bool> eliminated;
    public List<Team> teamdata;
    public GameObject playerprefab;
    public GameObject anchorprefab;
    public GameObject testball;
    public List<GameObject> buildlist;
    public List<GameObject> terrainlist;
    public Material glassbpmat;
    public Material blueprintmaterial;
    //public List<GameObject> bullet;

    public GameObject defaultcamera;

   // public List<int> newobjects = new List<int>();
   // public List<int> destroyedterrain = new List<int>();
    public Dictionary<int, EntityProperties> entityIDs = new Dictionary<int, EntityProperties>();
    public Dictionary<int, PowerNetworks.Network> powerIDs = new Dictionary<int, PowerNetworks.Network>();
    public Dictionary<int, AnchorSystem.Chunk> anchorIDs = new Dictionary<int, AnchorSystem.Chunk>();
    //public Dictionary<int, EntityProperties> tempIDs;

    //server -> generate terrain, accept players after

    //player -> get terrain seed, generate terrain, ask to be spawned
    public void Awake()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("Cursor Visible");
    }
    public override void OnStartServer()
    {
        if (GameManager == null)
        {
            GameManager = this;
            StartCoroutine("InitializeMatch");
            //TerrainSystem.TerrainManager.StartMatch();
        }
    }
   // NetworkBehaviour
   //OnClient
    public override void OnStartClient()
    {
        if (GameManager == null && !isServer)
        {
            GameManager = this;           
        }
        username.SetActive(false);
    }

    //[Command]
    public void GenerateID(EntityProperties entityobject, int tempid, int type)
    {
        int newID = Random.Range(10000, 1000000);
        while ((type == 1 && entityIDs.ContainsKey(newID)) || (type == 2 && powerIDs.ContainsKey(newID)) || (type == 3 && anchorIDs.ContainsKey(newID)) || newID == tempid)
        {
            newID = Random.Range(10000, 1000000);
        }

        if(type == 1)
        {
            Debug.Log(newID + ", already exists: " + entityIDs.ContainsKey(newID));
            entityIDs.Add(newID, entityobject);
            entityobject.entityID = newID;
        }
        else if(type == 2)
        {
            Debug.Log(newID + ", already exists: " + powerIDs.ContainsKey(newID));
            PowerNetworks.Network powernetwork = entityobject.contain.network;
            //entityobject.contain.network
            powerIDs.Add(newID, powernetwork);
            powernetwork.powerID = newID;
        }
        else if(type == 3)
        {
            Debug.Log(newID + ", already exists: " + anchorIDs.ContainsKey(newID));
            AnchorSystem.Chunk anchorchunk = entityobject.anchordata.chunk;
            anchorIDs.Add(newID, anchorchunk);
            anchorchunk.anchorID = newID;
        }
    }
    //[TargetRpc]
    public void ReceiveID()
    {

    }
    public void NetworkObject(int teamID, int objecttype, List<Vector3> chunkposition, List<Quaternion> chunkrotation, int chunkID,
        List<int> snappedfrom, List<int> snappedto, List<int> snapobjectIDs, List<int> anchorobjectIDs, int entityID, bool sync, out int serverID)
    {
        //Debug.Log(anchorobjectIDs.Count);
        //Debug.Log(entityID);
        serverID = 0;
        GameObject newobject = null;  //if server generate id, if not then server id
        //Debug.Log(buildlist);
        //Debug.Log(buildlist.blueprintlist + " " + objecttype);
        if (objecttype < 0)
        {
            //Debug.Log("Terrain " + (objecttype + 100));
            //Debug.Log(anchorIDs[chunkID]);
            newobject = Instantiate(terrainlist[objecttype + 100], chunkposition[0], chunkrotation[0], anchorIDs[chunkID].chunkobject);
            Rigidbody objectrigidbody = newobject.GetComponent<Rigidbody>();

            newobject.transform.localPosition = chunkposition[0];
            newobject.transform.localRotation = chunkrotation[0];
            objectrigidbody.MovePosition(newobject.transform.position);
            objectrigidbody.MoveRotation(newobject.transform.rotation);

            EntityProperties newobjectproperties = newobject.GetComponent<EntityProperties>();

            foreach (Transform child in newobject.GetComponentsInChildren<Transform>())
            {
                if (child.tag == "Check" || child.tag == "WeldingBox")
                {
                    Destroy(child.gameObject);
                }
                //child.gameObject.layer = 8;
            }
            newobjectproperties.teamid = teamID;
            newobjectproperties.buildingid = objecttype;

            newobjectproperties.entityID = entityID;
            entityIDs.Add(entityID, newobjectproperties); //need to check if new id overlaps temp id + what type of id?
            AnchorSystem.AnchorManager.CreateObject(newobjectproperties, null, anchorIDs[chunkID]);
        }
        else
        {
            if (objecttype != 1)  //NOT DONE: chunk ID and chunk local position
            {
                Debug.Log(chunkID + " " + anchorIDs[chunkID] + " " + anchorIDs[chunkID].chunkobject);
                newobject = Instantiate(buildlist[objecttype - 1], chunkposition[0], chunkrotation[0], anchorIDs[chunkID].chunkobject);
                Rigidbody objectrigidbody = newobject.GetComponent<Rigidbody>();

                newobject.transform.localPosition = chunkposition[0];
                newobject.transform.localRotation = chunkrotation[0];
                objectrigidbody.MovePosition(newobject.transform.position);
                objectrigidbody.MoveRotation(newobject.transform.rotation);
                /* foreach (Renderer bprender in blueprintobject.GetComponentsInChildren<Renderer>())
                 {
                     if (bprender.transform.tag == "Glass")
                     {
                         bprender.material = glassbpmat;
                     }
                     else
                     {
                         bprender.material = blueprintmaterial;
                     }
                 }*/
                //Debug.Log(blueprintobject.name + " at: " + blueprintobject.transform.position);
                //blueprintobject = Instantiate(buildlist.blueprintlist[objecttype - 1], chunkposition[0], chunkrotation[0], GameManager.anchorchunkIDs[chunkID].chunkobject);
            }
            else
            {
                newobject = Instantiate(buildlist[0], chunkposition[0], chunkrotation[0], anchorIDs[chunkID].chunkobject); //, GameManager.anchorIDs[chunkID].chunkobject);
                GameObject pipemiddle = Instantiate(buildlist[1], chunkposition[1], chunkrotation[1], newobject.transform);
                GameObject pipeend = Instantiate(buildlist[0], chunkposition[2], chunkrotation[2], newobject.transform);

                newobject.transform.localPosition = chunkposition[0];
                newobject.transform.localRotation = chunkrotation[0];
                pipemiddle.transform.localPosition = chunkposition[1];
                pipemiddle.transform.localRotation = chunkrotation[1];
                pipeend.transform.localPosition = chunkposition[2];
                pipeend.transform.localRotation = chunkrotation[2];

                Blueprint bpscript = newobject.GetComponent<Blueprint>();

                bpscript.blueprintobject[0] = newobject;
                bpscript.blueprintrenderer[0] = bpscript.blueprintobject[0].GetComponent<Renderer>();


                bpscript.blueprintmat.color = new Color32(0, 0, 0, 128);


                bpscript.blueprintobject[1] = pipemiddle;
                bpscript.blueprintobject[2] = pipeend;

                bpscript.blueprintrenderer[1] = pipemiddle.GetComponentInChildren<Renderer>();
                bpscript.blueprintrenderer[2] = pipeend.GetComponent<Renderer>();

                pipemiddle.GetComponentInChildren<Renderer>().material = bpscript.blueprintmat;
                pipeend.GetComponent<Renderer>().material = bpscript.blueprintmat;

                Transform pipebone = pipemiddle.transform.GetChild(0).transform.GetChild(0).transform.GetChild(0);
                pipebone.transform.position = pipeend.transform.GetChild(0).position;
                Bounds newbound = new Bounds(new Vector3(pipebone.localPosition.x / 2, pipebone.localPosition.y / 2, pipebone.localPosition.z / 2),
                    new Vector3(Mathf.Abs(pipebone.localPosition.x) + 0.0033f, Mathf.Abs(pipebone.localPosition.y) + 0.0033f, Mathf.Abs(pipebone.localPosition.z) + 0.0033f));
                pipemiddle.GetComponentInChildren<SkinnedMeshRenderer>().localBounds = newbound;

                BoxCollider col = pipemiddle.AddComponent<BoxCollider>();
                col.center = new Vector3(0, 0, -pipebone.localPosition.y / 2 * 100f);
                col.size = new Vector3(0.33f * 1, 0.33f * 1, Mathf.Abs(pipebone.localPosition.y * 100f));
                Debug.Log(col.size);
                Destroy(pipeend.GetComponent<Rigidbody>());
                pipeend.GetComponent<EntityProperties>().enabled = false;
                Destroy(pipeend.GetComponent<EntityProperties>());
                Destroy(pipeend.GetComponent<Blueprint>());
                /*
                //PlaceObject(blueprintlist[0], blueprintdisplay.transform.position, blueprintdisplay.transform.rotation, 1, snapping);
                //PlaceObject(blueprintlist[0], buildinglist[0], blueprintdisplay.transform.position, blueprintdisplay.transform.rotation, 1, snapping);
                //pipemiddle = Instantiate(blueprintlist[1], parentpipe.transform.GetChild(0).position, player.rotation);
                pipebone = pipemiddle.transform.GetChild(0).transform.GetChild(0).transform.GetChild(0);
                piping = true;*/

                bpscript.ispipe = true;
            }
            Blueprint blueprintscript = newobject.GetComponent<Blueprint>();
            EntityProperties blueprintproperties = newobject.GetComponent<EntityProperties>();

            foreach (Transform child in newobject.GetComponentsInChildren<Transform>())
            {
                if (child.tag == "Check" || child.tag == "WeldingBox")
                {
                    Destroy(child.gameObject);
                }
                foreach (BoxCollider snap in child.GetComponents<BoxCollider>())
                {
                    if (snap.isTrigger && snap.tag == "Snap")
                    {
                        blueprintscript.snappingfrom.Add(child);
                        //Debug.Log("add snap");
                        break;
                    }
                }
                child.gameObject.layer = 8;
            }

            for (int j = 0; j < snappedfrom.Count; j++) //snap
            {
                if (snappedfrom[j] != -1)
                {
                    Debug.Log("from " + snappedfrom[j] + " to " + snappedto[j] + ", id: " + snapobjectIDs[j]);
                    //Debug.Log(GameManager.entityIDs[snapobjectIDs[j]].gameObject.name);
                    blueprintscript.snappedto[snappedfrom[j]] = GameManager.entityIDs[snapobjectIDs[j]].snappingfrom[snappedto[j]];
                    //Debug.Log(blueprintscript.snappedto[snappedfrom[j]]);
                    blueprintscript.snappedto[snappedfrom[j]].gameObject.SetActive(false);
                    blueprintscript.snappingfrom[snappedfrom[j]].gameObject.SetActive(false);

                    if (entityIDs[snapobjectIDs[j]].GetComponent<Blueprint>())
                    {
                        entityIDs[snapobjectIDs[j]].snappedto[snappedto[j]] = blueprintscript.snappingfrom[snappedfrom[j]];
                    }
                }
                else
                {
                    blueprintscript.altsnappingto = GameManager.entityIDs[snapobjectIDs[j]].altsnappingfrom;
                    blueprintscript.altsnappingfrom.gameObject.SetActive(false);
                    blueprintscript.altsnappingto.gameObject.SetActive(false);
                }
            }
            blueprintproperties.teamid = teamID;
            blueprintproperties.buildingid = objecttype;
            blueprintscript.power = .01f;
            if(!sync)
            {
                List<AnchorSystem.Object> connectingobjects = new List<AnchorSystem.Object>();
                foreach (int anchorobject in anchorobjectIDs)//get all anchors
                {
                    Debug.Log(GameManager.entityIDs[anchorobject].name);
                    connectingobjects.Add(GameManager.entityIDs[anchorobject].anchordata);
                }
                blueprintscript.Place(connectingobjects, anchorIDs[chunkID]);
            }
            else
            {
                blueprintscript.Place(null, anchorIDs[chunkID]);
            }
            //AnchorSystem.AnchorManager.RemoveObject(blueprintproperties.anchordata);
            //AnchorSystem.AnchorManager.AddObject(anchorIDs[chunkID], blueprintproperties.anchordata);
            if (isServer)
            {
                GenerateID(blueprintproperties, entityID, 1);
                serverID = blueprintproperties.entityID;
                //newobjects.Add(serverID);
            }
            else
            {
                blueprintproperties.entityID = entityID;
                entityIDs.Add(entityID, blueprintproperties); //need to check if new id overlaps temp id + what type of id?
            }
        }
    }

    public void ServerRB(int chunkentityid, int tempid)//, out NetworkIdentity unassignedchunk)
    {
        //if RB is needed, function is called

        //generate id
        //List<uint> chunknetid = new List<uint>();
        //for (int i = 0; i < chunkentityid.Count; i++)
        //{
       /* EntityProperties chunkentity = GameManager.entityIDs[chunkentityid];
        if (tempid != -1)
        {
            GameManager.GenerateID(chunkentity, tempid, 3);
        }
        GameSystem.GameManager.anchorprefab
        GameObject serverRB = Instantiate(anchorprefab);
        NetworkServer.Spawn(serverRB);
        //chunknetid.Add(serverRB.GetComponent<NetworkIdentity>().netId);
        unassignedchunk = serverRB.GetComponent<NetworkIdentity>();*/
        //AnchorSystem.AnchorManager.unassignedchunks.Add(serverRB.GetComponent<NetworkIdentity>());
        //}


        if (isServer)
        {
            // AnchorSystem.AnchorManager.unassignedchunks.Add(serverRB.GetComponent<NetworkIdentity>());
        }
        //spawn rigidbody

        //switch out rigidbody

        //pass rb netids with damage rpc function

        //NetworkServer.Spawn(bulletClone);





        //spawn rb, list of netids






        //receive damage
        //anchor system
        //spawn rb
        //send network id with temp id to original
            //switch out temp chunk with network chunk
        //send damage + network id to others


        //send damage + temp ids
        //receive rb, switch out chunk

        //receive damage + network ids
    }


    /*public void NetworkFinishBlueprint()
    {

    }*/
    IEnumerator InitializeMatch()
    {
        if (GameManager == null)
        {
            GameManager = this;
            //StartCoroutine("InitializeMatch");
            //Debug.Log("Setting Up Server");
            //TerrainSystem.TerrainManager.StartMatch();
        }

        yield return new WaitUntil(() => TerrainSystem.TerrainManager != null);
        TerrainSystem.TerrainManager.StartGeneration();

        teamdata = new List<Team>();
        Team defaultteam = new Team { };
        defaultteam.players = new List<EntityProperties>();
        teamdata.Add(defaultteam);
        yield return new WaitUntil(() => !TerrainSystem.TerrainManager.generatingterrain);

        Random.seed = Random.Range(0, 100000);
        //Vector3 spawnposition = new Vector3(Mathf.Round((TerrainSystem.TerrainManager.mapsize + 5) * 2 * Mathf.Cos((float)0 / 6 * 2 * Mathf.PI)), 1, Mathf.Round((TerrainSystem.TerrainManager.mapsize + 5) * 2 * Mathf.Sin((float)0 / 6 * 2 * Mathf.PI))) * TerrainSystem.TerrainManager.unitsize;
        //Destroy(defaultcamera);
        //defaultteam.players.Add(Instantiate(playerprefab, spawnposition, Quaternion.identity).GetComponentInChildren<EntityProperties>());

        //PlayerUI.LocalPlayerUI.gameObject.SetActive(true);
    }



    /*void Start()
{
    //load up terrain
    //place spawns and players
    //game start, check for alive players and spawns
    //eliminate teams
    //if 1 team left, win
    if(GameManager == null)
    {
        GameManager = this;
        StartCoroutine("InitializeMatch");
        //TerrainSystem.TerrainManager.StartMatch();
    }
    //teamdata = new List<Team>();
    //Team defaultteam = new Team { };
    //defaultteam.players = new List<EntityProperties>();
    //defaultteam.players.Add(Instantiate(playerprefab).GetComponent<EntityProperties>());
}*/




    /*public void OnClientConnect(NetworkConnection conn)
    {
        Debug.Log("player join");
        ClientLoading(conn, TerrainSystem.TerrainManager.universalseed);
        //StartCoroutine("ClientLoading");
    }*/

   /* [TargetRpc]
    public void ClientLoading(NetworkConnection target, int universalseed)
    {
        TerrainSystem.TerrainManager.universalseed = universalseed;
        StartCoroutine("ClientTerrain");
    }*/
   /* IEnumerator ClientTerrain()
    {
        //TerrainSystem.TerrainManager.universalseed = universalseed;
        //yield return new WaitUntil(() => TerrainSystem.TerrainManager.universalseed != 0);

        TerrainSystem.TerrainManager.StartGeneration();

        teamdata = new List<Team>();
        Team defaultteam = new Team { };
        defaultteam.players = new List<EntityProperties>();

        yield return new WaitUntil(() => !TerrainSystem.TerrainManager.generatingterrain);

        ClientLoaded(playerprefab);
    }

    [Command]
    public void ClientLoaded(GameObject playerobject)
    {
        Vector3 spawnposition = new Vector3(Mathf.Round((TerrainSystem.TerrainManager.mapsize + 5) * 2 * Mathf.Cos((float)0 / 6 * 2 * Mathf.PI)), 1, Mathf.Round((TerrainSystem.TerrainManager.mapsize + 5) * 2 * Mathf.Sin((float)0 / 6 * 2 * Mathf.PI))) * TerrainSystem.TerrainManager.unitsize;
        Destroy(defaultcamera);
        teamdata[0].players.Add(Instantiate(playerprefab, spawnposition, Quaternion.identity).GetComponentInChildren<EntityProperties>());
        //teamdata.Add(defaultteam);
        SpawnPlayer(spawnposition);
    }*/

   /* [ClientRpc]
    public void SpawnPlayer(Vector3 spawnposition)
    {
        Destroy(defaultcamera);
        PlayerUI.LocalPlayerUI.gameObject.SetActive(true);
    }*/
        // Update is called once per frame
        void Update()
    {
        /*for (int i = 0; i < eliminated.Count; i++) //check surviving teams
        {
            if (!eliminated[i])
            {
                Team teamcheck = teamdata[i];
                if (teamcheck.players.Count > 0)
                {
                    continue;
                }
                else if (teamcheck.spawns.Count > 0)
                {
                    bool noteliminated = false;
                    for (int j = 0; j < eliminated.Count; j++) //check spawns for eligibility
                    {
                        if (teamcheck.spawns[j].contain.network.power >= 100 || teamcheck.spawns[j].contain.network.powerstabledelta > 0)
                        {
                            noteliminated = true;
                            break;
                        }
                    }
                    if (noteliminated)
                    {
                        continue;
                    }
                }
                eliminated[i] = true;
            }
        }*/
    }


    public class Team
    {
        public List<EntityProperties> players;
        public List<EntityProperties> spawns;
    }
}
