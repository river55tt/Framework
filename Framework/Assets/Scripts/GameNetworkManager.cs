using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
public class GameNetworkManager : NetworkManager
{
    public static GameObject ServerObject;
    public override void OnStartServer()
    {
        Debug.Log("Setting Up Server");
        ServerObject = gameObject;
        //GameSystem.GameManager.StartCoroutine("")
    }

    //public override void 
    public override void OnServerConnect(NetworkConnection conn)
    {
        //Debug.Log("h");
       // base.OnClientConnect(conn);
        //conn.
        StartCoroutine(PlayerSendTerrain(conn));

        /*Debug.Log("player join");
        Debug.Log(conn);
        if (conn.GetType().Name != "LocalConnectionToServer")
        {
            Debug.Log(conn);
            Debug.Log(conn.identity);
            Debug.Log(conn.identity.gameObject);
            //NetworkManager.
            //conn.clientOwnedObjects[0]
            //NetworkConnection.
            conn.identity.GetComponent<PlayerNetwork>().ClientLoading(conn, TerrainSystem.TerrainManager.universalseed);
            //GameSystem.GameManager.ClientLoading(conn, TerrainSystem.TerrainManager.universalseed);
            //PlayerNetwork.ClientLoading(conn, TerrainSystem.TerrainManager.universalseed);
        }
        else
        {
            //TerrainSystem.TerrainManager.universalseed = universalseed;
            //GameSystem.GameManager.ClientLoading(conn, TerrainSystem.TerrainManager.universalseed);
            TerrainSystem.TerrainManager.StartCoroutine("ClientTerrain");
        }*/
        //GameSystem.GameManager.ClientLoading(conn, TerrainSystem.TerrainManager.universalseed);
        //StartCoroutine("ClientLoading");
    }
    IEnumerator PlayerSendTerrain(NetworkConnection connection)
    {
        Debug.Log("player join, " + connection);
        // Debug.Log(connection);
        if(connection.identity == null)
        {
            yield return new WaitUntil(() => connection.identity != null);
        }
        Debug.Log(connection.identity);
        connection.identity.transform.GetChild(0).gameObject.SetActive(false);
        //yield return new WaitUntil(() => connection.identity != null);
        //connection.identity.transform.GetChild(0).gameObject.SetActive(false);
        connection.identity.GetComponent<PlayerNetwork>().RpcClientLoading(connection, TerrainSystem.TerrainManager.universalseed, TerrainSystem.TerrainManager.mapsize, TerrainSystem.TerrainManager.islandheight,
            TerrainSystem.TerrainManager.noisescale, TerrainSystem.TerrainManager.stemscale, TerrainSystem.TerrainManager.spirescale, TerrainSystem.TerrainManager.noisethreshold);
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        Debug.Log("I connected");
        base.OnClientConnect(conn);
    }
    //public void Spawn
    public override void OnServerDisconnect(NetworkConnection conn)
    {
        EntityProperties playerproperties = conn.identity.GetComponent<PlayerNetwork>().playerproperties;

        foreach (KeyValuePair<int, NetworkConnectionToClient> item in NetworkServer.connections)
        {
            item.Value.identity.gameObject.GetComponent<PlayerNetwork>().RpcPlayerDisconnect(item.Value, playerproperties.entityID);
            //Debug.Log("Connections--->:" + item.Key + "-->" + item.Value.connectionId);
        }

        if (GameSystem.GameManager.entityIDs != null && playerproperties != null)
        {
            DistributorSystem.DistributorManager.EditReceiver(playerproperties, false);
            GameSystem.GameManager.entityIDs.Remove(playerproperties.entityID);
        }

        base.OnServerDisconnect(conn);
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);

        base.OnClientDisconnect(conn);
    }
}
