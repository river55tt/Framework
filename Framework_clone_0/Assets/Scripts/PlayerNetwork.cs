using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class PlayerNetwork : NetworkBehaviour
{
    public kcp2k.KcpTransport KcpTransport;

    public EntityProperties playerproperties;
    public bool loaded;
    public Text nametext;
    Camera cam;
    //public BuildSystem buildlist;
    //public static PlayerNetwork serverplayer;

    [SyncVar]
    public bool playergrounded;
    [SyncVar]
    public float playerspeed;
    [SyncVar]
    public int playerselect;
    [SyncVar]
    public Vector3 playerbeam;

    List<int> anchorID = new List<int>();
    List<uint> anchornetID = new List<uint>();
    List<int> destroyedterrainID = new List<int>();
    List<int> joinsyncID = new List<int>();
    List<List<int>> connectionsyncID = new List<List<int>>();
    List<List<int>> snapfromsync = new List<List<int>>();
    List<List<int>> snaptosync = new List<List<int>>();
    List<List<int>> snapsyncID = new List<List<int>>();
    List<float> healthsync = new List<float>();
    List<bool> isbpsync = new List<bool>();
    List<int> anchorsource = new List<int>();
   // List<AnchorSystem.Chunk> unfreezechunks;// = AnchorSystem.AnchorManager.allchunks;
    void LateUpdate()
    {
        if (cam)
        {
            nametext.transform.parent.forward = cam.transform.forward;
        }
        //Test();
    }

    [SyncVar]
    public string playerName;

    /*public override void OnStartServer()
    {
        if (serverplayer == null)
        {
            serverplayer = this;
            //Debug.Log("Initialized Terrain Manager");
        }
    }*/

    // Start is called before the first frame update
    /* [ClientRpc]
     public void RpcSpawn(string message)
     {
         //OnMessage?.Invoke(this, message);
     }*/
    [Command]
    public void CmdPlayername(string name)
    {
        playerName = name;
    }

    [Command]
    public void Test()
    {
        Test2(connectionToClient);
    }

    [TargetRpc]
    public void Test2(NetworkConnection target)
    {
        //Debug.Log("im " + target);
        //Debug.Log(target.identity.gameObject);
        //TerrainSystem.TerrainManager.universalseed = universalseed;
        //StartCoroutine("ClientTerrain");
    }

    /*public void OnClientConnect()
    {
        Debug.Log("I connected");
        //NetworkClient.connection.
        Debug.Log(NetworkClient.spawned);
        /*foreach (NetworkClient.network playeridentity in NetworkClient.spawned)
        {
            //networkid
        }
        //NetworkIdentity.spawned
        //connection
        //connection.identity.transform.GetChild(0).gameObject.SetActive(false);
    }*/
    [Command]
    public void CmdPlayerAnimations(bool grounded, float speed, int weapon, Vector3 fabbeam)
    {
        playergrounded = grounded;
        playerspeed = speed;
        playerselect = weapon;
        playerbeam = fabbeam;
    }

    [TargetRpc]
    public void RpcClientLoading(NetworkConnection target, int universalseed, float mapsize, int islandheight, float noisescale, float stemscale, float spirescale, float noisethreshold)
    {
        //noisescale
        //stemscale
        //spirescale
        //noisethreshold

        /*TerrainSystem.TerrainManager.universalseed = universalseed;
        TerrainSystem.TerrainManager.mapsize = mapsize;
        TerrainSystem.TerrainManager.islandheight = islandheight;
        TerrainSystem.TerrainManager.noisescale = noisescale;
        TerrainSystem.TerrainManager.stemscale = stemscale;
        TerrainSystem.TerrainManager.spirescale = spirescale;
        TerrainSystem.TerrainManager.noisethreshold = noisethreshold;*/
       // GameSystem.GameManager.teamUI.SetActive(true);
        if (!isServer)
        {
           // TerrainSystem.TerrainManager.StartGeneration();
        }
        else
        {

        }
        StartCoroutine(ClientTerrain(connectionToServer));
    }

    IEnumerator ClientTerrain(NetworkConnection sender)
    {
        //TerrainSystem.TerrainManager.universalseed = universalseed;
        //yield return new WaitUntil(() => TerrainSystem.TerrainManager.universalseed != 0);
        /*if (!isServer)
        {
            TerrainSystem.TerrainManager.StartGeneration();
        }*/
        GameSystem.GameManager.teamdata = new List<GameSystem.Team>();
        for (int t = 0; t < 6; t++)
        {
            GameSystem.Team newteam = new GameSystem.Team { };
            newteam.players = new List<EntityProperties>();
            GameSystem.GameManager.teamdata.Add(newteam);
        }

        yield return new WaitUntil(() => !TerrainSystem.TerrainManager.generatingterrain);
        if(isServer)
        {
            AnchorSystem.AnchorManager.netIDs.Clear();
        }

        if (!isServer)
        {
            CmdRetroactiveSync();

            yield return new WaitUntil(() => loaded);      //LOAD NEW OBJECTS

            int yieldcount = 0;
            for (int i = 0; i < joinsyncID.Count; i++)
            {
                if (isbpsync[i])
                {
                    Blueprint tempbp = GameSystem.GameManager.entityIDs[joinsyncID[i]].GetComponent<Blueprint>();
                    for (int j = 0; j < snapfromsync[i].Count; j++) //snap
                    {
                        if (snapfromsync[i][j] != -1)
                        {
                            //Debug.Log("from " + snappedfrom[j] + " to " + snappedto[j] + ", id: " + snapobjectIDs[j]);
                            //Debug.Log(GameManager.entityIDs[snapobjectIDs[j]].gameObject.name);
                            tempbp.snappedto[snapfromsync[i][j]] = GameSystem.GameManager.entityIDs[snapsyncID[i][j]].snappingfrom[snaptosync[i][j]];
                            //Debug.Log(blueprintscript.snappedto[snappedfrom[j]]);
                            tempbp.snappedto[snapfromsync[i][j]].gameObject.SetActive(false);
                            tempbp.snappingfrom[snapfromsync[i][j]].gameObject.SetActive(false);

                            if (GameSystem.GameManager.entityIDs[snapsyncID[i][j]].GetComponent<Blueprint>())
                            {
                                GameSystem.GameManager.entityIDs[snapsyncID[i][j]].snappedto[snaptosync[i][j]] = tempbp.snappingfrom[snapfromsync[i][j]];
                            }
                        }
                        else
                        {
                            tempbp.altsnappingto = GameSystem.GameManager.entityIDs[snapsyncID[i][j]].altsnappingfrom;
                            tempbp.altsnappingfrom.gameObject.SetActive(false);
                            tempbp.altsnappingto.gameObject.SetActive(false);
                        }
                    }

                    yieldcount++;
                    if (yieldcount == 10)
                    {
                        yieldcount = 0;
                        yield return null;
                    }
                }
            }

            for (int i = 0; i < joinsyncID.Count; i++)
            {
                Blueprint tempbp = GameSystem.GameManager.entityIDs[joinsyncID[i]].GetComponent<Blueprint>();
                if (isbpsync[i])
                {
                    tempbp.power = healthsync[i];
                    tempbp.BlueprintFill(0f);
                }
                else if(tempbp)
                {
                    tempbp.power = tempbp.maxpower;
                    tempbp.BlueprintFill(10000f, GameSystem.GameManager.entityIDs[joinsyncID[i]].anchordata.chunk);
                }
                EntityProperties newproperties = GameSystem.GameManager.entityIDs[joinsyncID[i]];

                //AnchorSystem.AnchorManager.CreateObject(newproperties, GameSystem.GameManager.anchorIDs[anchorID[i]]);
                //newproperties.anchordata.chunk.physicsproperties.isKinematic = true;

                if (!isbpsync[i])
                {
                    newproperties.health = healthsync[i];
                    newproperties.TakeDamage(0, 0, Vector3.zero, null, true, out _, out _); //damage cracks sync
                }

                List<AnchorSystem.Object> connectingobjects = new List<AnchorSystem.Object>();
                foreach (int anchorobject in connectionsyncID[i])//get all anchors
                {
                    connectingobjects.Add(GameSystem.GameManager.entityIDs[anchorobject].anchordata);
                }
               // Debug.Log("addconnecions");
                AnchorSystem.AnchorManager.AddConnections(GameSystem.GameManager.entityIDs[joinsyncID[i]].anchordata, connectingobjects);

                if(tempbp)
                {
                    //Debug.Log(newproperties.anchordata.chunk.anchored);
                    newproperties.anchordata.chunk.physicsproperties.isKinematic = true;
                }
                yieldcount++;
                if (yieldcount == 20)
                {
                    yieldcount = 0;
                    yield return null;
                }
            }

            AnchorSystem anchormanager = AnchorSystem.AnchorManager;
            List<AnchorSystem.Chunk> unfreezechunks = anchormanager.allchunks;

            for (int n = 0; n < anchorsource.Count; n++)
            {
                GameSystem.GameManager.entityIDs[anchorsource[n]].anchordata.anchor = true;
                //Debug.Log(unfreezechunks[n].contents.Count);
               // yield return null;
            }

            for (int n = 0; n < unfreezechunks.Count; n++)
            {
                Debug.Log(unfreezechunks[n].anchorID);
                unfreezechunks[n].anchored = true;
                unfreezechunks[n].anchorsource = true;
                anchormanager.UpdateAnchor(unfreezechunks[n]); //?
                unfreezechunks[n].physicsproperties.WakeUp();
                //Debug.Log(n);
            }

            /*unfreezechunks = AnchorSystem.AnchorManager.allchunks;
            Debug.Log(unfreezechunks.Count);
            yield return null;
            yield return new WaitForSeconds(15);
            for (int n = 0; n < unfreezechunks.Count; n++)
            {
                unfreezechunks[n].anchored = true;
                unfreezechunks[n].anchorsource = true;
                AnchorSystem.AnchorManager.UpdateAnchor(unfreezechunks[n]);
                unfreezechunks[n].physicsproperties.WakeUp();
                Debug.Log(n);
                yield return null;
                if (unfreezechunks[n].contents.Count > 50)
                {
                    yield return new WaitForSeconds(1);
                }
                /* yieldcount++;
                 if (yieldcount == 10)
                 {
                     yieldcount = 0;
                     yield return null;
                 }*/
            //}*/



            /*for (int k = 0; k < destroyedterrainID.Count; k++)
            {
                EntityProperties destroy = GameSystem.GameManager.entityIDs[destroyedterrainID[k]];
                destroy.health = 0;
                destroy.TakeDamage(10000, 10000, Vector3.zero, null, true, out _, out _);
                yieldcount++;
                if (yieldcount == 10)
                {
                    yieldcount = 0;
                    yield return null;
                }
            }*/
        }
        else
        {
            loaded = true;
        }

        //unfreezechunks = null;
        joinsyncID = null;
        connectionsyncID = null;
        snapfromsync = null;
        snaptosync = null;
        snapsyncID = null;
        healthsync = null;
        isbpsync = null;

        Debug.Log("FINISHED LOADING");
        GameSystem.GameManager.teamUI.SetActive(true);
        yield return new WaitUntil(() => GameSystem.GameManager.chosenteam != 0);
        GameSystem.GameManager.teamUI.SetActive(false);
        CmdClientLoaded(GameSystem.GameManager.playername, GameSystem.GameManager.chosenteam - 1);

        /*Debug.Log("Merging Meshes!");
        foreach(AnchorSystem.Chunk mergechunk in AnchorSystem.AnchorManager.allchunks)
        {
            Transform meshcombine = mergechunk.chunkobject;
            MeshFilter[] meshFilters = meshcombine.GetComponentsInChildren<MeshFilter>();
            CombineInstance[] combine = new CombineInstance[meshFilters.Length];

            int i = 0;
            while (i < meshFilters.Length)
            {
                combine[i].mesh = meshFilters[i].sharedMesh;
                combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
                meshFilters[i].gameObject.SetActive(false);

                i++;
            }
            meshcombine.gameObject.AddComponent<MeshRenderer>();
            meshcombine.gameObject.AddComponent<MeshFilter>();
            meshcombine.GetComponent<MeshRenderer>().material = meshFilters[0].GetComponent<MeshRenderer>().material;
            meshcombine.GetComponent<MeshFilter>().mesh = new Mesh();
            meshcombine.GetComponent<MeshFilter>().mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            meshcombine.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
        }*/
        //GameSystem.GameManager.playerprefab
    }

    [Command]
    public void CmdRetroactiveSync()
    {
        StartCoroutine(PlayerRetroactiveSync(connectionToClient));
    }
    IEnumerator PlayerRetroactiveSync(NetworkConnection target)
    {
        if(!KcpTransport)
        {
            KcpTransport = GameNetworkManager.ServerObject.GetComponent<kcp2k.KcpTransport>();
        }
        //fo
        //List<int> newobjects = GameSystem.GameManager.newobjects;
        //List<int> destroyterrain = GameSystem.GameManager.destroyedterrain;
        int k = 0;

        List<int> teamIDs = new List<int>();
        List<int> objecttypes = new List<int>();
        List<List<Vector3>> chunkpositions = new List<List<Vector3>>();
        List<List<Quaternion>> chunkrotations = new List<List<Quaternion>>();
        List<int> chunkIDs = new List<int>();
        List<List<int>> snappedfroms = new List<List<int>>();
        List<List<int>> snappedtos = new List<List<int>>();
        List<List<int>> snapobjectIDss = new List<List<int>>();
        List<List<int>> anchorobjectIDss = new List<List<int>>();
        List<int> entityIDs = new List<int>();
        List<float> healths = new List<float>();
        List<bool> isbp = new List<bool>();

        List<Vector3> chunkposition = new List<Vector3>();
        List<Quaternion> chunkrotation = new List<Quaternion>();
        List<int> snappedfrom = new List<int>();
        List<int> snappedto = new List<int>();
        List<int> snapobjectIDs = new List<int>();
        List<int> anchorobjectIDs = new List<int>();

        List<int> anchorIDs = new List<int>();
        List<uint> anchornetIDs = new List<uint>();
        List<int> anchorsourceIDs = new List<int>();
        Debug.Log(AnchorSystem.AnchorManager.allchunks.Count);
        for (int i = 0; i < AnchorSystem.AnchorManager.allchunks.Count; i++) //snap
        {
            anchorIDs.Add(AnchorSystem.AnchorManager.allchunks[i].anchorID);
            anchornetIDs.Add(AnchorSystem.AnchorManager.allchunks[i].chunkobject.GetComponent<NetworkIdentity>().netId);
            if (k == 10)
            {
                k = 0;
                RpcSyncAnchor(target, anchorIDs, anchornetIDs, new List<int> { });

                anchorIDs = new List<int>();
                anchornetIDs = new List<uint>();
                yield return null;
            }
            k++;
        }
        RpcSyncAnchor(target, anchorIDs, anchornetIDs, new List<int> { });
        k = 0;
        yield return null;

        for (int i = 0; i < AnchorSystem.AnchorManager.allchunks.Count; i++) //snap
        {
            for (int j = 0; j < AnchorSystem.AnchorManager.allchunks[i].contents.Count; j++)
            {
                //Debug.Log(KcpTransport.GetBufferSize());
                if (KcpTransport.GetBufferSize() > 500)//server.connections.Values.Sum(conn => conn.SendBufferCount);)
                {
                    yield return new WaitUntil(() => KcpTransport.GetBufferSize() < 5000);
                }

                snappedfrom = new List<int>();
                snappedto = new List<int>();
                snapobjectIDs = new List<int>();
                anchorobjectIDs = new List<int>();
                chunkposition = new List<Vector3>();
                chunkrotation = new List<Quaternion>();

                EntityProperties syncobject = AnchorSystem.AnchorManager.allchunks[i].contents[j].objectproperties;
                Blueprint parentBP = null;
                if (syncobject.GetComponent<Blueprint>())
                {
                    parentBP = syncobject.GetComponent<Blueprint>();
                    isbp.Add(true);
                    healths.Add(parentBP.power);
                }
                else
                {
                    isbp.Add(false);
                    healths.Add(syncobject.health);
                }
                teamIDs.Add(syncobject.teamid);
                objecttypes.Add(syncobject.buildingid);
                chunkIDs.Add(syncobject.anchordata.chunk.anchorID);
                entityIDs.Add(syncobject.entityID);

                for (int n = 0; n < syncobject.snappedto.Length; n++) //weld to snapped
                {
                    if (syncobject.snappedto[n] != null)
                    {
                        snappedfrom.Add(n);
                        List<Transform> othersnapto = new List<Transform>();
                        if (syncobject.snappedto[n].GetComponentInParent<Blueprint>())
                        {
                            othersnapto = syncobject.snappedto[n].GetComponentInParent<Blueprint>().snappingfrom;
                        }
                        else
                        {
                            othersnapto = syncobject.snappedto[n].GetComponentInParent<EntityProperties>().snappingfrom;
                        }
                        snappedto.Add(othersnapto.FindIndex(x => x == syncobject.snappedto[n]));
                        snapobjectIDs.Add(syncobject.snappedto[n].GetComponentInParent<EntityProperties>().entityID);
                    }
                }

                if(syncobject.anchordata.anchor)
                {
                    anchorsourceIDs.Add(syncobject.entityID);
                }
                foreach (AnchorSystem.Object anchorobjectid in syncobject.anchordata.connectedto)
                {
                    anchorobjectIDs.Add(anchorobjectid.objectproperties.entityID);
                }

                snappedfroms.Add(snappedfrom);
                snappedtos.Add(snappedto);
                snapobjectIDss.Add(snapobjectIDs);
                anchorobjectIDss.Add(anchorobjectIDs);
                if (syncobject.buildingid != 1)
                {
                    chunkposition.Add(syncobject.transform.localPosition);
                    chunkrotation.Add(syncobject.transform.localRotation);
                }
                else
                {
                    if (isbp[isbp.Count-1])
                    {
                        chunkposition.Add(parentBP.blueprintobject[0].transform.localPosition);
                        chunkrotation.Add(parentBP.blueprintobject[0].transform.localRotation);
                        chunkposition.Add(parentBP.blueprintobject[1].transform.localPosition);
                        chunkrotation.Add(parentBP.blueprintobject[1].transform.localRotation);
                        chunkposition.Add(parentBP.blueprintobject[2].transform.localPosition);
                        chunkrotation.Add(parentBP.blueprintobject[2].transform.localRotation);
                    }
                    else
                    {
                        chunkposition.Add(syncobject.transform.localPosition);
                        chunkrotation.Add(syncobject.transform.localRotation);

                        chunkposition.Add(syncobject.transform.GetChild(1).localPosition);
                        chunkrotation.Add(syncobject.transform.GetChild(1).localRotation);
                        chunkposition.Add(syncobject.transform.GetChild(2).localPosition);
                        chunkrotation.Add(syncobject.transform.GetChild(2).localRotation);
                    }
                }
                chunkpositions.Add(chunkposition);
                chunkrotations.Add(chunkrotation);

                if (k == 8)
                {
                    k = 0;
                    RpcRetroactiveSync(target, teamIDs, objecttypes, chunkpositions, chunkrotations, chunkIDs,
                        snappedfroms, snappedtos, snapobjectIDss, anchorobjectIDss, entityIDs, healths, isbp, false);

                    teamIDs = new List<int>();
                    objecttypes = new List<int>();
                    chunkpositions = new List<List<Vector3>>();
                    chunkrotations = new List<List<Quaternion>>();
                    chunkIDs = new List<int>();
                    snappedfroms = new List<List<int>>();
                    snappedtos = new List<List<int>>();
                    snapobjectIDss = new List<List<int>>();
                    anchorobjectIDss = new List<List<int>>();
                    entityIDs = new List<int>();
                    healths = new List<float>();
                    isbp = new List<bool>();
                    yield return null;
                }
                k++;
            }
        }
        RpcSyncAnchor(target, new List<int> { }, new List<uint> { }, anchorsourceIDs);
        yield return null;
        RpcRetroactiveSync(target, teamIDs, objecttypes, chunkpositions, chunkrotations, chunkIDs,
    snappedfroms, snappedtos, snapobjectIDss, anchorobjectIDss, entityIDs, healths, isbp, true);
        Debug.Log("done");
        /* List<int> destroyedIDs = new List<int>();
         while (i < destroyterrain.Count)
         {
             destroyedIDs.Add(destroyterrain[i]);
             if (j == 10)
             {
                 j = 0;
                 RpcSyncDestroyed(target, destroyedIDs);

                 destroyedIDs = new List<int>();
                 yield return null;
             }
             i++; j++;
         }
         i = 0;
         j = 0;
         RpcSyncDestroyed(target, destroyedIDs);*/
        yield return null;

        /*for (int j = 0; j < AnchorSystem.AnchorManager.allchunks[i].contents.Count; j++)
        {

            List<Vector3> chunkposition = new List<Vector3>();
            List<Quaternion> chunkrotation = new List<Quaternion>();
            List<int> snappedfrom = new List<int>();
            List<int> snappedto = new List<int>();
            List<int> snapobjectIDs = new List<int>();
            List<int> anchorobjectIDs = new List<int>();

            if (!GameSystem.GameManager.entityIDs.ContainsKey(newobjects[i]))
            {
                Debug.Log("MISSING ID: " + newobjects[i]);
            }
            EntityProperties syncobject = GameSystem.GameManager.entityIDs[newobjects[i]];
            Blueprint parentBP = null;
            if (syncobject.GetComponent<Blueprint>())
            {
                parentBP = syncobject.GetComponent<Blueprint>();
                isbp.Add(true);
                healths.Add(parentBP.power);
            }
            else
            {
                isbp.Add(false);
                healths.Add(syncobject.health);
            }
            teamIDs.Add(syncobject.teamid);
            objecttypes.Add(syncobject.buildingid);
            chunkIDs.Add(0);
            entityIDs.Add(syncobject.entityID);

            for (int n = 0; n < syncobject.snappedto.Length; n++) //weld to snapped
            {
                if (syncobject.snappedto[n] != null)
                {
                    snappedfrom.Add(n);
                    List<Transform> othersnapto = new List<Transform>();
                    if (syncobject.snappedto[n].GetComponentInParent<Blueprint>())
                    {
                        othersnapto = syncobject.snappedto[n].GetComponentInParent<Blueprint>().snappingfrom;
                    }
                    else
                    {
                        othersnapto = syncobject.snappedto[n].GetComponentInParent<EntityProperties>().snappingfrom;
                    }
                    snappedto.Add(othersnapto.FindIndex(x => x == syncobject.snappedto[n]));
                    snapobjectIDs.Add(syncobject.snappedto[n].GetComponentInParent<EntityProperties>().entityID);
                }
            }

            foreach (AnchorSystem.Object anchorobjectid in syncobject.anchordata.connectedto)
            {
                anchorobjectIDs.Add(anchorobjectid.objectproperties.entityID);
            }

            snappedfroms.Add(snappedfrom);
            snappedtos.Add(snappedto);
            snapobjectIDss.Add(snapobjectIDs);
            anchorobjectIDss.Add(anchorobjectIDs);
            if (syncobject.buildingid != 1)
            {
                chunkposition.Add(syncobject.transform.localPosition);
                chunkrotation.Add(syncobject.transform.localRotation);
            }
            else
            {
                if (isbp[j])
                {
                    chunkposition.Add(parentBP.blueprintobject[0].transform.localPosition);
                    chunkrotation.Add(parentBP.blueprintobject[0].transform.localRotation);
                    chunkposition.Add(parentBP.blueprintobject[1].transform.localPosition);
                    chunkrotation.Add(parentBP.blueprintobject[1].transform.localRotation);
                    chunkposition.Add(parentBP.blueprintobject[2].transform.localPosition);
                    chunkrotation.Add(parentBP.blueprintobject[2].transform.localRotation);
                }
                else
                {
                    chunkposition.Add(syncobject.transform.localPosition);
                    chunkrotation.Add(syncobject.transform.localRotation);

                    chunkposition.Add(syncobject.transform.GetChild(1).localPosition);
                    chunkrotation.Add(syncobject.transform.GetChild(1).localRotation);
                    chunkposition.Add(syncobject.transform.GetChild(1).GetChild(2).localPosition);
                    chunkrotation.Add(syncobject.transform.GetChild(1).GetChild(2).localRotation);
                }
            }
            chunkpositions.Add(chunkposition);
            chunkrotations.Add(chunkrotation);

            if (j == 10)
            {
                j = 0;
                RpcRetroactiveSync(target, teamIDs, objecttypes, chunkpositions, chunkrotations, chunkIDs,
                    snappedfroms, snappedtos, snapobjectIDss, anchorobjectIDss, entityIDs, healths, isbp, false);

                teamIDs = new List<int>();
                objecttypes = new List<int>();
                chunkpositions = new List<List<Vector3>>();
                chunkrotations = new List<List<Quaternion>>();
                chunkIDs = new List<int>();
                snappedfroms = new List<List<int>>();
                snappedtos = new List<List<int>>();
                snapobjectIDss = new List<List<int>>();
                anchorobjectIDss = new List<List<int>>();
                entityIDs = new List<int>();
                healths = new List<float>();
                yield return null;
            }
            i++; j++;
        }*/
     /*   RpcRetroactiveSync(target, teamIDs, objecttypes, chunkpositions, chunkrotations, chunkIDs,
    snappedfroms, snappedtos, snapobjectIDss, anchorobjectIDss, entityIDs, healths, isbp, true);*/
        //yield return null;
    }
    [TargetRpc]
    public void RpcRetroactiveSync(NetworkConnection target, List<int> teamIDs, List<int> objecttypes, List<List<Vector3>> chunkpositions, List<List<Quaternion>> chunkrotations, List<int> chunkIDs,
    List<List<int>> snappedfroms, List<List<int>> snappedtos, List<List<int>> snapobjectIDss, List<List<int>> anchorobjectIDss, List<int> entityIDs, List<float> healths, List<bool> isbp, bool done)
    {
        for (int i = 0; i < teamIDs.Count; i++) //snap
        {
            /*if (GameSystem.GameManager.entityIDs.ContainsKey(entityIDs[i]))
            {
                EntityProperties properties = GameSystem.GameManager.entityIDs[entityIDs[i]];
                properties.health = healths[i];
                properties.TakeDamage(0, 0, Vector3.zero, null, true, out _, out _);
                continue;
            }*/
            //int i = 0;
            //Debug.Log("receive blueprint: " + entityIDs);
            GameSystem.GameManager.NetworkObject(teamIDs[i], objecttypes[i], chunkpositions[i], chunkrotations[i], chunkIDs[i], new List<int>(), new List<int>(), new List<int>(), new List<int>(), entityIDs[i], true, out _);
            //GameSystem.GameManager.entityIDs[entityIDs[i]].anchordata.chunk.physicsproperties.isKinematic = true;

            joinsyncID.Add(entityIDs[i]);
            connectionsyncID.Add(anchorobjectIDss[i]);
            snapfromsync.Add(snappedfroms[i]);
            snaptosync.Add(snappedtos[i]);
            snapsyncID.Add(snapobjectIDss[i]);
            healthsync.Add(healths[i]);
            isbpsync.Add(isbp[i]);
        }
        if (done)
        {
            loaded = true;
        }
    }
    [TargetRpc]
    public void RpcSyncAnchor(NetworkConnection target, List<int> anchorIDs, List<uint> anchornetIDs, List<int> sourceanchorIDs)
    {
        if(isServer)
        {
            return;
        }

        anchorsource.AddRange(sourceanchorIDs);
        if (!loaded)
        {
            anchorID.AddRange(anchorIDs);
            anchornetID.AddRange(anchornetIDs);
            //chunkRB = Instantiate(GameSystem.GameManager.anchorprefab);
            //NetworkServer.Spawn(chunkRB, NetworkClient.localPlayer.connectionToClient);
            //GameNetworkManager.
            for (int i = 0; i < anchornetIDs.Count; i++) //snap
            {
                AnchorSystem.Chunk chunkdata = new AnchorSystem.Chunk();
                chunkdata.connectedto = new List<AnchorSystem.Chunk>();
                chunkdata.contents = new List<AnchorSystem.Object>();

                GameObject chunkRB = null;

                foreach (NetworkIdentity networkchunk in AnchorSystem.AnchorManager.unassignedchunks)
                {
                    //Debug.Log(networkchunk.netId + ", " + anchornetIDs[i]);
                    if (anchornetIDs.Count > 0 && networkchunk.netId == anchornetIDs[i]) //search unassigned chunks for netid in order
                    {
                        chunkRB = networkchunk.gameObject;
                        //anchornetIDs.RemoveAt(0);
                        break;
                    }
                }

                if (chunkRB == null)
                {
                    //chunkRB = new GameObject("Chunk");
                    //chunkRB.AddComponent<Rigidbody>();
                    Debug.LogWarning("no chunk");
                    continue;
                }
                else
                {
                    Debug.Log("New chunk, remove unassigned");
                    AnchorSystem.AnchorManager.unassignedchunks.Remove(chunkRB.GetComponent<NetworkIdentity>());
                    Debug.Log(chunkRB.GetComponent<NetworkIdentity>().netId);
                    AnchorSystem.AnchorManager.netIDs.Remove(chunkRB.GetComponent<NetworkIdentity>().netId);
                    GameSystem.GameManager.anchorIDs.Add(anchorIDs[i], chunkdata);
                    chunkdata.anchorID = anchorIDs[i];


                    chunkdata.rigidbodysource = true;
                    chunkdata.chunkobject = chunkRB.transform;
                    chunkdata.physicsproperties = chunkRB.GetComponent<Rigidbody>();

                    chunkdata.anchored = true;
                    chunkdata.anchorsource = true;
                    AnchorSystem.AnchorManager.allchunks.Add(chunkdata);
                }
            }
        }
        else
        {
            for (int i = 0; i < anchorIDs.Count; i++) //snap
            {
                Debug.Log(anchorIDs[i]);
                AnchorSystem.Chunk chunkdata = GameSystem.GameManager.anchorIDs[AnchorSystem.AnchorManager.tempchunkIDs[0]];
                GameObject chunkRB = null;
                GameSystem.GameManager.anchorIDs.Remove(AnchorSystem.AnchorManager.tempchunkIDs[0]);
                GameSystem.GameManager.anchorIDs.Add(anchorIDs[i], chunkdata);
                chunkdata.anchorID = anchorIDs[i];

                foreach (NetworkIdentity networkchunk in AnchorSystem.AnchorManager.unassignedchunks)
                {
                    if (anchornetIDs.Count > 0 && networkchunk.netId == anchornetIDs[i]) //search unassigned chunks for netid in order
                    {
                        chunkRB = networkchunk.gameObject;
                        chunkdata.chunkobject = chunkRB.transform;
                        chunkdata.physicsproperties = chunkRB.GetComponent<Rigidbody>();
                        break;
                    }
                }

                if (chunkRB == null)
                {
                    Debug.LogWarning("no chunk");
                    continue;
                }
                else
                {
                    Debug.Log("Replace chunk, remove unassigned");
                    AnchorSystem.AnchorManager.unassignedchunks.Remove(chunkRB.GetComponent<NetworkIdentity>());

                    Transform oldchunkobject = chunkdata.chunkobject;

                    chunkdata.rigidbodysource = true;
                    chunkdata.chunkobject = chunkRB.transform;
                    chunkdata.physicsproperties = chunkRB.GetComponent<Rigidbody>();

                    chunkdata.anchored = true;
                    chunkdata.anchorsource = true;


                    chunkdata.contents[0].objecttransform.parent.position = chunkdata.physicsproperties.position;
                    chunkdata.contents[0].objecttransform.parent.rotation = chunkdata.physicsproperties.rotation;

                    foreach (AnchorSystem.Object assemble in chunkdata.contents) //finalize network using searched
                    {
                        assemble.objecttransform.SetParent(chunkdata.chunkobject);
                    }

                    AnchorSystem.AnchorManager.UpdateAnchor(chunkdata);
                    AnchorSystem.AnchorManager.allchunks.Add(chunkdata);

                    Destroy(oldchunkobject);
                }

                AnchorSystem.AnchorManager.tempchunkIDs.RemoveAt(0);
            }
        }
    }

    [TargetRpc]
    public void RpcSyncDestroyed(NetworkConnection target, List<int> destroyedIDs)
    {
        destroyedterrainID.AddRange(destroyedIDs);
    }

    [Command]
    public void CmdClientLoaded(string name, int team)
    {
        //ConnectionToClient conn = connectionToClient.identity; //what?
        Vector3 spawnposition = new Vector3(Mathf.Round((TerrainSystem.TerrainManager.mapsize + 5) * 2 * Mathf.Cos((float)team / 6 * 2 * Mathf.PI)), 1, Mathf.Round((TerrainSystem.TerrainManager.mapsize + 5) * 2 * Mathf.Sin((float)team / 6 * 2 * Mathf.PI))) * 3;
        //Destroy(GameSystem.GameManager.defaultcamera);
        // GameSystem.GameManager.teamdata[0].players.Add(Instantiate(GameSystem.GameManager.playerprefab, spawnposition, Quaternion.identity).GetComponentInChildren<EntityProperties>());
        //Instantiate(GameSystem.GameManager.playerprefab, spawnposition, Quaternion.identity, transform);
        //teamdata.Add(defaultteam);
        Debug.Log(connectionToClient.identity.transform.GetChild(0).gameObject);
        playerproperties = connectionToClient.identity.transform.GetChild(0).GetComponent<EntityProperties>();
        //Debug.Log(conn.identity.transform.GetChild(0));
        // Debug.Log(conn.identity.transform);
        playerName = name;
        connectionToClient.identity.name = connectionToClient.identity.name + ": " + name;
        int playerID = Random.Range(10000, 1000000);
        while (GameSystem.GameManager.entityIDs.ContainsKey(playerID))
        {
            playerID = Random.Range(10000, 1000000);
        }

        //Debug.Log(newID + ", already exists: " + entityIDs.ContainsKey(newID));
        /*if (isServerOnly)
        {
            GameSystem.GameManager.entityIDs.Add(playerID, connectionToClient.identity.GetComponentInChildren<EntityProperties>());
        }*/
        connectionToClient.identity.transform.GetChild(0).position = spawnposition;
        connectionToClient.identity.transform.GetChild(0).GetComponent<Rigidbody>().MovePosition(spawnposition);
        connectionToClient.identity.transform.GetChild(0).gameObject.SetActive(true);
        RpcSpawnPlayer(connectionToClient, connectionToClient.identity.gameObject, spawnposition, playerID, team);

        foreach (KeyValuePair<int, NetworkConnectionToClient> item in NetworkServer.connections)
        {
            //Debug.Log("Connections--->:" + item.Key + "-->" + item.Value.connectionId);
            if (item.Key != connectionToClient.connectionId)
            {
                RpcSpawnPlayer(item.Value, connectionToClient.identity.gameObject, spawnposition, playerID, team);
                //Debug.Log(item.Value.identity.gameObject);
                //Debug.Log(item.Value.identity.gameObject.transform.GetChild(0));
                //Debug.Log(item.Value.identity.transform.GetChild(0).GetComponent<EntityProperties>());
                RpcSpawnPlayer(connectionToClient, item.Value.identity.gameObject, spawnposition, item.Value.identity.transform.GetChild(0).GetComponent<EntityProperties>().entityID, item.Value.identity.transform.GetChild(0).GetComponent<EntityProperties>().teamid);
                //RpcPlaceBlueprint(item.Value, playerobject, objecttype, chunkposition, chunkrotation, chunkID, snappedfrom, snappedto, snapobjectIDs, anchorobjectIDs, serverID);
            }
            else
            {
                //RpcSpawnPlayer(item.Value, connectionToClient.identity.gameObject, spawnposition, playerID);
            }

            //RpcReplaceTempID(item.Value, serverID, tempID, 1);

        }
        foreach(NetworkTransformChild networktransform in connectionToClient.identity.GetComponentsInChildren<NetworkTransformChild>())
        {
            networktransform.clientAuthority = true;
        }
        connectionToClient.identity.GetComponentInChildren<Mirror.Experimental.NetworkRigidbody>().clientAuthority = true;
        connectionToClient.identity.AssignClientAuthority(connectionToClient);
    }

    [TargetRpc]
    public void RpcSpawnPlayer(NetworkConnection target, GameObject player, Vector3 spawnposition, int entityID, int team)
    {
        Debug.Log(entityID);
        Debug.Log(player.GetComponent<PlayerNetwork>());
        GameObject playerbody = player.transform.GetChild(0).gameObject;
        playerbody.transform.position = spawnposition;
        playerbody.GetComponent<Rigidbody>().MovePosition(spawnposition);
        playerbody.GetComponent<Rigidbody>().velocity = Vector3.zero;
        player.GetComponent<PlayerNetwork>().playerproperties = playerbody.GetComponent<EntityProperties>();
        Debug.Log(GameSystem.GameManager.entityIDs.ContainsKey(entityID));
        Debug.Log(GameSystem.GameManager.entityIDs.ContainsValue(playerbody.GetComponent<EntityProperties>()));
        if (!GameSystem.GameManager.entityIDs.ContainsKey(entityID))
        {
            GameSystem.GameManager.entityIDs.Add(entityID, playerbody.GetComponent<EntityProperties>());
        }
        playerbody.GetComponent<EntityProperties>().entityID = entityID;
        playerbody.GetComponent<EntityProperties>().teamid = team;

        if (player.GetComponent<PlayerNetwork>().isLocalPlayer)
        {
            foreach (GameObject players in GameObject.FindGameObjectsWithTag("PlayerNetwork"))
            {
                //Debug.Log(players.name);
                //players.transform.GetChild(0).gameObject.SetActive(true);
            }
            //Destroy(GameSystem.GameManager.defaultcamera);
            GameSystem.GameManager.defaultcamera.GetComponent<AudioListener>().enabled = false;
            //player.GetComponent<PlayerNetwork>().buildlist = playerbody.GetComponent<BuildSystem>();            
            PlayerUI.LocalPlayerUI.gameObject.SetActive(true);
            playerbody.GetComponentInChildren<Camera>().gameObject.AddComponent<AudioListener>();
            playerbody.GetComponentInChildren<Camera>().gameObject.tag = "MainCamera";
            playerbody.GetComponentInChildren<Camera>().depth = 2;
            //Destroy(othercamera.GetComponent<AudioListener>());
            player.GetComponent<PlayerNetwork>().nametext.gameObject.SetActive(false);
            cam = Camera.main;//playerbody.GetComponentInChildren<Camera>();
        }
        else
        {
            playerbody.SetActive(true);
            //player.transform.GetChild(0).GetComponentInChildren<Camera>().gameObject.tag = "Untagged";
            //Destroy(playerbody.GetComponentInChildren<Camera>().gameObject);
            // Destroy(othercamera.GetComponent<AudioListener>());
            //othercamera.tag = null;
        }
        //player.transform.GetChild(0).transform.position = spawnposition;
        foreach (NetworkTransformChild networktransform in player.GetComponent<PlayerNetwork>().GetComponentsInChildren<NetworkTransformChild>())
        {
            networktransform.clientAuthority = true;
        }
        player.GetComponent<PlayerNetwork>().GetComponentInChildren<Mirror.Experimental.NetworkRigidbody>().clientAuthority = true;
        player.GetComponent<PlayerNetwork>().nametext.text = player.GetComponent<PlayerNetwork>().playerName;
        player.GetComponent<PlayerNetwork>().cam = Camera.main;
        playerbody.SetActive(true);
    }

    [TargetRpc]
    public void RpcPlayerDisconnect(NetworkConnection target, int entityID)
    {
        if (!isServer)
        {
            EntityProperties playerproperties = GameSystem.GameManager.entityIDs[entityID];
            DistributorSystem.DistributorManager.EditReceiver(playerproperties, false);
            GameSystem.GameManager.entityIDs.Remove(playerproperties.entityID);
        }
    }

    [Command]
    public void CmdPlaceBlueprint(GameObject playerobject, int objecttype, List<Vector3> chunkposition, List<Quaternion> chunkrotation, int chunkID,
        List<int> snappedfrom, List<int> snappedto, List<int> snapobjectIDs, List<int> anchorobjectIDs, int tempID)
    {
        if(anchorobjectIDs.Count > 0)
        {
            Debug.Log("anchor id: " + chunkID + ", " + GameSystem.GameManager.anchorIDs.ContainsKey(chunkID));
        }


        //Debug.Log("server blueprint from: " + connectionToClient.connectionId);
        //GameObject blueprintobject = null;
        int serverID;
        int teamID = playerobject.GetComponentInChildren<EntityProperties>().teamid;
        if (NetworkClient.localPlayer.gameObject != playerobject)
        {
            GameSystem.GameManager.NetworkObject(teamID, objecttype, chunkposition, chunkrotation, chunkID, snappedfrom, snappedto, snapobjectIDs, anchorobjectIDs, tempID, false, out serverID);
        }
        else
        {
            serverID = tempID;
        }
       // GameSystem.GameManager.newobjects.Add(serverID);

        foreach (KeyValuePair<int, NetworkConnectionToClient> item in NetworkServer.connections)
        {
            //Debug.Log("Connections--->:" + item.Key + "-->" + item.Value.connectionId);
            if (item.Key != connectionToClient.connectionId)
            {
                RpcPlaceBlueprint(item.Value, teamID, objecttype, chunkposition, chunkrotation, chunkID, snappedfrom, snappedto, snapobjectIDs, anchorobjectIDs, serverID);
            }
            else
            {
                RpcReplaceTempID(item.Value, serverID, tempID, 1);
            }
        }
        /*for (int i = 0; i < NetworkServer.connections.Count; i++)
        {
           // Debug.Log(NetworkServer.connections.Count + " " + i);
            foreach (KeyValuePair<int, NetworkConnectionToClient> item in NetworkServer.connections)
            {
                if (item.Key != connectionToClient.connectionId)
                {
                    RpcPlaceBlueprint(connectionToClient, playerobject, objecttype, chunkposition, chunkrotation, chunkID, snappedfrom, snappedto, snapobjectIDs, anchorobjectIDs, serverID);
                }
                else
                {
                    RpcReplaceTempID(connectionToClient, serverID, tempID);
                }
            }
            Debug.Log(NetworkServer.connections[i].connectionId);
            Debug.Log(connectionToClient.connectionId);
            if (NetworkServer.connections[i].connectionId != connectionToClient.connectionId)
            {
                //NetworkServer.connections[i].SendByChannel(messageType, netMsg, channelID);
                Debug.Log("hi4");
                RpcPlaceBlueprint(connectionToClient, playerobject, objecttype, chunkposition, chunkrotation, chunkID, snappedfrom, snappedto, snapobjectIDs, anchorobjectIDs, serverID);
                Debug.Log("hi6");
            }
            else
            {
                Debug.Log("hi5");
                RpcReplaceTempID(connectionToClient, serverID, tempID);
            }
            Debug.Log("hi7");
        }
        Debug.Log("hi3");*/
    }

    [TargetRpc]
    public void RpcPlaceBlueprint(NetworkConnection target, int teamID, int objecttype, List<Vector3> chunkposition, List<Quaternion> chunkrotation, int chunkID,
        List<int> snappedfrom, List<int> snappedto, List<int> snapobjectIDs, List<int> anchorobjectIDs, int entityID)
    {
        if (!isServer)
        {
            Debug.Log("receive blueprint: " + entityID + " " + chunkID);
            if (!GameSystem.GameManager.anchorIDs.ContainsKey(chunkID))
            {
                Debug.LogWarning("Missing chunk!");
            }
            GameSystem.GameManager.NetworkObject(teamID, objecttype, chunkposition, chunkrotation, chunkID, snappedfrom, snappedto, snapobjectIDs, anchorobjectIDs, entityID, false, out _);
        }
    }

    [Command]
    public void CmdPowerExchange(int sendentityID, int receiveentityID, float power, bool finished)
    {
        //GameSystem.GameManager.GenerateID(playerobject, entityID);
        if (GameSystem.GameManager.entityIDs.ContainsKey(receiveentityID))
        {
            if (!isLocalPlayer)
            {
                EntityProperties objectproperties = GameSystem.GameManager.entityIDs[receiveentityID];
                Blueprint blueprintscript = objectproperties.GetComponent<Blueprint>();

                if (blueprintscript)
                {
                    if (finished)
                    {
                        blueprintscript.power = blueprintscript.maxpower;
                        blueprintscript.BlueprintFill(1000f);
                    }
                    else
                    {
                        //blueprintscript.power += power;
                        blueprintscript.BlueprintFill(power);
                        //Debug.Log(power);
                    }
                }
                else
                {
                    if (objectproperties.contain != null)
                    {
                        objectproperties.contain.network.power += power;
                    }
                    else
                    {
                        objectproperties.power += power;
                    }
                }
            }
        }
        else if (receiveentityID != 0)
        {
            Debug.LogWarning("SERVER MISSING RECEIVING ENTITY ID: " + receiveentityID);
        }

        if (GameSystem.GameManager.entityIDs.ContainsKey(sendentityID))
        {
            if (!isLocalPlayer)
            {
                EntityProperties objectproperties = GameSystem.GameManager.entityIDs[sendentityID];
                if (objectproperties.contain != null)
                {
                    objectproperties.contain.network.power -= power;
                }
                else
                {
                    objectproperties.power -= power;
                }
            }
        }
        else if (sendentityID != 0)
        {
            Debug.LogWarning("SERVER MISSING SENDING ENTITY ID: " + sendentityID);
        }

        foreach (KeyValuePair<int, NetworkConnectionToClient> item in NetworkServer.connections)
        {
            //Debug.Log("Connections--->:" + item.Key + "-->" + item.Value.connectionId);
            if (item.Key != connectionToClient.connectionId)
            {
                RpcPowerExchange(item.Value, sendentityID, receiveentityID, power, finished);
            }
        }
    }

    [TargetRpc]
    public void RpcPowerExchange(NetworkConnection target, int sendentityID, int receiveentityID, float power, bool finished)
    {
        if (!NetworkClient.localPlayer.GetComponent<PlayerNetwork>().loaded)
        {
            return;
        }
        if (!isServer)
        {
            if (GameSystem.GameManager.entityIDs.ContainsKey(receiveentityID))
            {
                EntityProperties objectproperties = GameSystem.GameManager.entityIDs[receiveentityID];
                Blueprint blueprintscript = objectproperties.GetComponent<Blueprint>();
                if (blueprintscript)
                {
                    if (finished)
                    {
                        blueprintscript.power = blueprintscript.maxpower;
                        blueprintscript.BlueprintFill(1000f);
                    }
                    else
                    {
                        //blueprintscript.power += power;
                        blueprintscript.BlueprintFill(power);
                        Debug.Log(power);
                    }
                }
                else
                {
                    if (objectproperties.contain != null)
                    {
                        objectproperties.contain.network.power += power;
                    }
                    else
                    {
                        objectproperties.power += power;
                    }
                }
            }
            else
            {
                Debug.LogWarning("MISSING RECEIVING ENTITY ID: " + receiveentityID);
            }

            if (GameSystem.GameManager.entityIDs.ContainsKey(sendentityID))
            {
                EntityProperties objectproperties = GameSystem.GameManager.entityIDs[sendentityID];
                if (objectproperties.contain != null)
                {
                    objectproperties.contain.network.power -= power;
                }
                else
                {
                    objectproperties.power -= power;
                }
            }
            else
            {
                Debug.LogWarning("MISSING SENDING ENTITY ID: " + sendentityID);
            }
        }
    }

    [Command]
    public void CmdDamageObject(List<int> entityID, List<float> damage, List<float> armorpen, List<Vector3> force, List<List<int>> partindex, List<bool> dead)
    {
        //Debug.Log("hi, " + entityID);
        for (int n = 0; n < entityID.Count; n++)
        {
            if (GameSystem.GameManager.entityIDs.ContainsKey(entityID[n]))
            {
                if (!isLocalPlayer)
                {
                    EntityProperties damageproperties = GameSystem.GameManager.entityIDs[entityID[n]];
                    GameObject part;
                    if (partindex.Count == 0)
                    {
                        part = null;
                    }
                    else
                    {
                        part = damageproperties.gameObject;
                        for (int i = 0; i < partindex[n].Count; i++)
                        {
                            Debug.Log(part.name + ", " + partindex[n][i]);
                            part = part.transform.GetChild(partindex[n][i]).gameObject;
                        }
                    }

                    if (!dead[n])
                    {
                        damageproperties.TakeDamage(damage[n], armorpen[n], force[n], part, true, out _, out _);
                    }
                    else
                    {
                        damageproperties.health = 0;
                        damageproperties.TakeDamage(10000, 10000, force[n], part, true, out _, out _);
                    }
                }
            }
            else
            {
                Debug.LogWarning("SERVER MISSING ENTITY ID: " + entityID);
            }
        }

        foreach (KeyValuePair<int, NetworkConnectionToClient> item in NetworkServer.connections)
        {
            //Debug.Log("Connections--->:" + item.Key + "-->" + item.Value.connectionId);
            if (item.Key != connectionToClient.connectionId)
            {
                RpcDamageObject(item.Value, entityID, damage, armorpen, force, partindex, dead, AnchorSystem.AnchorManager.replaceIDs ,AnchorSystem.AnchorManager.netIDs);
            }
        }
        if(AnchorSystem.AnchorManager.netIDs.Count > 0)
        {
            RpcSyncAnchor(connectionToClient, AnchorSystem.AnchorManager.replaceIDs, AnchorSystem.AnchorManager.netIDs, new List<int>());
            //tempchunk.physicsproperties.
        }
        AnchorSystem.AnchorManager.netIDs.RemoveRange(0, AnchorSystem.AnchorManager.netIDs.Count);
        AnchorSystem.AnchorManager.replaceIDs.RemoveRange(0, AnchorSystem.AnchorManager.replaceIDs.Count);
    }

    [TargetRpc]
    public void RpcDamageObject(NetworkConnection target, List<int> entityID, List<float> damage, List<float> armorpen, 
        List<Vector3> force, List<List<int>> partindex, List<bool> dead, List<int> anchorid, List<uint> anchornetID)
    {
        if (!NetworkClient.localPlayer.GetComponent<PlayerNetwork>().loaded)
        {
            return;
        }
        if (!isServer)
        {
            AnchorSystem.AnchorManager.replaceIDs.AddRange(anchorid);
            AnchorSystem.AnchorManager.netIDs.AddRange(anchornetID);
            /*if(AnchorSystem.AnchorManager.netIDs.Count > 0)
            {
                Debug.Log(AnchorSystem.AnchorManager.netIDs[0]);
               //Debug.Log(AnchorSystem.AnchorManager.replaceIDs[0]);
            }*/

            for (int j = 0; j < anchorid.Count; j++) //clean up welding box
            {
                Debug.Log("anchor ids: " + anchorid[j] + " " + anchornetID[j]);
            }
            if (AnchorSystem.AnchorManager.netIDs.Count > 0)
            {
                Debug.Log(AnchorSystem.AnchorManager.netIDs[0]);
            }

            for (int n = 0; n < entityID.Count; n++)
            {
                if (GameSystem.GameManager.entityIDs.ContainsKey(entityID[n]))
                {
                    EntityProperties damageproperties = GameSystem.GameManager.entityIDs[entityID[n]];
                    GameObject part;
                    if (partindex.Count == 0)
                    {
                        part = null;
                    }
                    else
                    {
                        part = damageproperties.gameObject;
                        for (int i = 0; i < partindex[n].Count; i++)
                        {
                            part = part.transform.GetChild(partindex[n][i]).gameObject;
                        }
                    }

                    if (!dead[n])
                    {
                        damageproperties.TakeDamage(damage[n], armorpen[n], force[n], part, true, out _, out _); //need to check if new id overlaps temp id + what type of id?
                    }
                    else
                    {
                        damageproperties.health = 0;
                        damageproperties.TakeDamage(10000, 10000, force[n], part, true, out _, out _);
                    }
                }
                else
                {
                    Debug.LogWarning("MISSING ENTITY ID: " + entityID);
                }
            }
        }
    }

    [Command]
    public void CmdBullet(int fireid, List<Quaternion> rotation, List<float> distance, int weapon)
    {
        foreach (KeyValuePair<int, NetworkConnectionToClient> item in NetworkServer.connections)
        {
            if (item.Key != connectionToClient.connectionId)
            {
                RpcBullet(item.Value, fireid, rotation, distance, weapon);
            }
        }
    }

    [TargetRpc]
    public void RpcBullet(NetworkConnection target, int fireid, List<Quaternion> rotation, List<float> distance, int weapon)
    {
        if (!NetworkClient.localPlayer.GetComponent<PlayerNetwork>().loaded)
        {
            return;
        }

        WeaponBehavior wep = null;
        Vector3 position = Vector3.zero;
        if (weapon != -1)
        {
            wep = playerproperties.GetComponent<WeaponSelection>().GunStats[weapon];
            position = GameSystem.GameManager.entityIDs[fireid].GetComponent<WeaponSelection>().GunStats[weapon].gun.position;
        }
        else
        {
            GameObject objectweapon = GameSystem.GameManager.entityIDs[fireid].gameObject;
            wep = objectweapon.GetComponentInChildren<WeaponBehavior>();
            if(objectweapon.GetComponentInChildren <Turret>())
            {
                //objectweapon.GetComponentInChildren<Turret>().ServerShoot(rotation[0]);
            }
            position = wep.gun.position;
        }
        for (int i = 0; i < rotation.Count; i++)
        {
            //Debug.Log(Quaternion.LookRotation(rotation[i].eulerAngles, Vector3.up).eulerAngles);
            //Debug.Log(rotation[i].eulerAngles);

            GameObject shotprojectile = Instantiate(wep.projectile, position, rotation[i]);
            shotprojectile.transform.Rotate(-90, 0, 0);
            shotprojectile.transform.position += -shotprojectile.transform.up * distance[i];
            shotprojectile.transform.localScale = new Vector3(400, 100 * distance[i] / .01f, 400);
            shotprojectile.GetComponent<Renderer>().material.color = wep.projectilecolor;
            shotprojectile.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            shotprojectile.GetComponent<Renderer>().receiveShadows = false;
            HitscanBullet shotscript = shotprojectile.AddComponent<HitscanBullet>();
            shotscript.size = 100 * distance[i] / .01f;
            shotscript.speed = wep.bulletspeed * 10000;
            shotscript.bullet = shotprojectile.transform;
            shotscript.bulletobject = shotprojectile;
        }
    }

    [ClientRpc]
    public void RpcSyncPowerNetworks(GameObject player, Vector3 spawnposition)
    {
        //loop through networks
        //sync power
    }

    [Command] 
    public void CmdSpawnAnchorObject(int entityobject, int tempId)
    {
       /* GameObject anchorobject = Instantiate(GameNetworkManager.anchorprefab);
        Rigidbody tempanchor = GameSystem.GameManager.entityIDs[entityobject].anchordata.chunk.physicsproperties;
        Rigidbody anchorRB = anchorobject.GetComponent<Rigidbody>();
        anchorRB.isKinematic = tempanchor.isKinematic;
        anchorRB.mass = tempanchor.mass;
        anchorRB.velocity = tempanchor.velocity;
        anchorRB.angularVelocity = tempanchor.angularVelocity;
        anchorRB.MovePosition(tempanchor.position);

        NetworkServer.Spawn(anchorobject);*/
        //NetworkServer.Spawn(GameNetworkManager.anchorprefab);
    }

    [TargetRpc]
    public void RpcReplaceTempID(NetworkConnection target, int serverID, int tempID, int type)
    {
        if (!isServer)
        {
            if (type == 1 && GameSystem.GameManager.entityIDs.ContainsKey(tempID))
            {
                Debug.Log("replace id >>> new: " + serverID + ", old: " + tempID + ", " + GameSystem.GameManager.entityIDs.ContainsKey(serverID));
                EntityProperties entityobject = GameSystem.GameManager.entityIDs[tempID];
                GameSystem.GameManager.entityIDs.Add(serverID, entityobject); //need to check if new id overlaps temp id + what type of id?
                entityobject.entityID = serverID;
                GameSystem.GameManager.entityIDs.Remove(tempID);
            }
            else if (type == 2 && GameSystem.GameManager.powerIDs.ContainsKey(serverID))
            {
                Debug.Log("replace id >>> new: " + serverID + ", old: " + tempID + ", " + GameSystem.GameManager.powerIDs.ContainsKey(serverID));
                PowerNetworks.Network powernetwork = GameSystem.GameManager.powerIDs[tempID];
                GameSystem.GameManager.powerIDs.Add(serverID, powernetwork);
                powernetwork.powerID = serverID;
                GameSystem.GameManager.powerIDs.Remove(tempID);
            }
            else if (type == 3 && GameSystem.GameManager.anchorIDs.ContainsKey(serverID))
            {
                Debug.Log("replace id >>> new: " + serverID + ", old: " + tempID + ", " + GameSystem.GameManager.anchorIDs.ContainsKey(serverID));
                AnchorSystem.Chunk anchorchunk = GameSystem.GameManager.anchorIDs[tempID];
                GameSystem.GameManager.anchorIDs.Add(serverID, anchorchunk);
                anchorchunk.anchorID = serverID;
                //anchorchunk.chunkobject
                GameSystem.GameManager.anchorIDs.Remove(tempID);
            }
            else
            {
                Debug.LogWarning("MISSING ENTITY ID: " + serverID);
            }
        }
    }

    [Command]
    public void CmdButton(int entityid, int userid, int button)
    {
        if (!isLocalPlayer)
        {
            EntityProperties buildingobject = GameSystem.GameManager.entityIDs[entityid];
            //EntityProperties playerobject = GameSystem.GameManager.entityIDs[userid];

            IInteract connectedscript = (IInteract)buildingobject.GetComponentInChildren(typeof(IInteract));//.interactbutton(button, entityid);
                                                                                                            //.GetComponentInChildren(typeof(IInteract)).interactbutton(button, entityid);
            connectedscript.interactbutton(button, userid);
        }
        foreach (KeyValuePair<int, NetworkConnectionToClient> item in NetworkServer.connections)
        {
            if (item.Key != connectionToClient.connectionId)
            {
                RpcButton(item.Value, entityid, userid, button);
            }
        }
    }

    [TargetRpc]
    public void RpcButton(NetworkConnection target, int entityid, int userid, int button)
    {
        if (!NetworkClient.localPlayer.GetComponent<PlayerNetwork>().loaded)
        {
            return;
        }
        if (!isServer)
        {
            Debug.Log("hi, " + entityid);
            EntityProperties buildingobject = GameSystem.GameManager.entityIDs[entityid];
            //EntityProperties playerobject = GameSystem.GameManager.entityIDs[userid];

            IInteract connectedscript = (IInteract)buildingobject.GetComponentInChildren(typeof(IInteract));//.interactbutton(button, entityid);
                                                                                                            //.GetComponentInChildren(typeof(IInteract)).interactbutton(button, entityid);
            connectedscript.interactbutton(button, userid);
        }
    }

    [Command]
    public void CmdRespawn(int playerid, int spawnid)
    {
        if (!isLocalPlayer)
        {
            EntityProperties player = GameSystem.GameManager.entityIDs[playerid];
            PlayerMovement movement = player.GetComponent<PlayerMovement>();
            player.health = player.maxhealth;
            player.power = 0;
            player.dead = false;
            movement.ragdoll.gameObject.SetActive(false);
            movement.animController.enabled = true;
            movement.capsulebody[0].enabled = true;
            movement.playerRB.isKinematic = false;
            movement.Target.SetParent(movement.transform);
            player.transform.GetChild(0).gameObject.SetActive(true);
            player.transform.position = GameSystem.GameManager.entityIDs[spawnid].transform.GetChild(5).position;
        }
        foreach (KeyValuePair<int, NetworkConnectionToClient> item in NetworkServer.connections)
        {
            if (item.Key != connectionToClient.connectionId)
            {
                RpcRespawn(item.Value, playerid, spawnid);
            }
        }
    }

    [TargetRpc]
    public void RpcRespawn(NetworkConnection target, int playerid, int spawnid)
    {
        if (!NetworkClient.localPlayer.GetComponent<PlayerNetwork>().loaded)
        {
            return;
        }
        if (!isServer)
        {
            EntityProperties player = GameSystem.GameManager.entityIDs[playerid];
            PlayerMovement movement = player.GetComponent<PlayerMovement>();
            player.health = player.maxhealth;
            player.power = 0;
            player.dead = false;
            movement.ragdoll.gameObject.SetActive(false);
            movement.animController.enabled = true;
            movement.capsulebody[0].enabled = true;
            movement.playerRB.isKinematic = false;
            movement.Target.SetParent(movement.transform);
            player.transform.GetChild(0).gameObject.SetActive(true);
            player.transform.position = GameSystem.GameManager.entityIDs[spawnid].transform.GetChild(5).position;
        }
    }
   /* [Command]
    public void RpcPlayerRB(int entityid)
    {
       // networkrigi
    }*/

    [TargetRpc]
    public void RpcRB(NetworkConnection target, int newchunkid, int oldchunkid)
    {

    }
}