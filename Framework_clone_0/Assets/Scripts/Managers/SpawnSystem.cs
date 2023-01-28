using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SpawnSystem : MonoBehaviour
{
    public static SpawnSystem SpawnManager;
    public List<GameObject> spawnicons;
    public List<SpawnUI> spawnui;
    public List<EntityProperties> spawnentities;
    public bool usingspawn;
    public bool respawning;
    public int totalspawns = 0;

    public EntityProperties player;
    public GameObject spawnUI;
    public EntityProperties activespawn;
    public EntityProperties selectedspawn;

    //public bool canspawn;
    public bool cantele;
    // Start is called before the first frame update
    void Start()
    {
        if (SpawnManager == null)
        {
            SpawnManager = this;
            Debug.Log("Initialized Spawn Manager");
            //spawnicons = new List<GameObject>();
            //spawnui = new List<SpawnUI>();
            spawnentities = new List<EntityProperties>();
        }
        else if (SpawnManager != this)
        {
            totalspawns++;
            SpawnManager.spawnentities.Add(this.transform.GetComponent<EntityProperties>());
            SpawnManager.spawnicons.Add(Instantiate(SpawnManager.spawnicons[2]));
            SpawnManager.spawnicons[SpawnManager.spawnicons.Count - 1].GetComponentInChildren<Text>().text = (SpawnManager.totalspawns).ToString();
            SpawnManager.spawnicons[SpawnManager.spawnicons.Count - 1].transform.SetParent(SpawnManager.spawnUI.transform);

            //GetComponentInChildren<Button>().connectedscript = SpawnManager;
            //GetComponentInChildren<Button>().buttonid = SpawnManager.spawnentities.FindIndex(x => x == this.transform.GetComponent<EntityProperties>());
            //Instantiate(spawnicons[0]);
            Destroy(this);
            //Debug.Log("Bye");
        }
    }

    // Update is called once per frame
    void Update()
    {
        cantele = false;
        if (usingspawn || respawning) //acount for destroyed spawns
        {
            List<int> removeat = new List<int>();
            for (int i = 0; i < spawnentities.Count; i++)
            {
                if (spawnentities[i] == null)
                {
                    removeat.Add(i);
                }
            }
            for (int n = 0; n < removeat.Count; n++)
            {
                spawnentities.RemoveAt(removeat[n]);
                Destroy(spawnicons[n + 3]);
                spawnicons.RemoveAt(n + 3);
            }

            float cost = 100f;
            if(respawning)
            {
                cost = 200f;
            }

            spawnUI.SetActive(true);
            spawnicons[0].SetActive(true);
            spawnicons[1].SetActive(true);
            int j = 0;
            //update power level of each spawn
            for (int i = 0; i < spawnentities.Count; i++)
            {
                if (activespawn == spawnentities[i] && !respawning)
                {
                    spawnicons[i + 3].SetActive(false);
                    continue;
                }
                float powerscale = 1;
                if (respawning || spawnentities[i].contain.network == activespawn.contain.network)
                {
                    if (spawnentities[i].contain.network.power < cost)
                    {
                        powerscale = spawnentities[i].contain.network.power / cost;
                    }
                }
                else if (spawnentities[i].contain.network.power < cost/2f)
                {
                    powerscale = spawnentities[i].contain.network.power / cost/2f;
                }
                spawnicons[i + 3].transform.GetChild(1).localScale = new Vector3(1, powerscale, 1);
                spawnicons[i + 3].SetActive(true);
                Vector3 tempposition = spawnicons[2].transform.localPosition;
                tempposition.x = j * 120 - spawnentities.Count * 60 + 120;
                if (respawning)
                {
                    tempposition.x -= 60;
                }
                //Debug.Log(tempposition.x);
                spawnicons[i + 3].transform.localPosition = tempposition;
                j++;
            }
            if (selectedspawn != null)
            {
                if (respawning && selectedspawn.contain.network.power >= cost)
                {
                    cantele = true;
                }
                else if (activespawn != null && selectedspawn.contain.network.power >= cost / 2f)
                {
                    if (activespawn.contain.network.power >= cost)
                    {
                        cantele = true;
                    }
                    else if (!respawning && activespawn.contain.network != selectedspawn.contain.network && activespawn.contain.network.power >= cost / 2f)
                    {
                        cantele = true;
                    }
                }
            }
        }
        else
        {
            spawnUI.SetActive(false);



            for (int i = 0; i < spawnicons.Count; i++)
            {
                spawnicons[i].SetActive(false);
            }
        }
    }


    public void Teleport()
    {
        if (respawning)
        {
            PlayerMovement movement = player.GetComponent<PlayerMovement>();
            player.health = player.maxhealth;
            player.power = 0;
            player.dead = false;
            movement.ragdoll.gameObject.SetActive(false);
            movement.animController.enabled = true;
            movement.capsulebody[0].enabled = true;
            movement.playerRB.isKinematic = false;
            movement.Target.SetParent(movement.transform);
            //Debug.Log(player.name);
            //Debug.Log(player.transform.GetChild(0));
            player.transform.GetChild(0).gameObject.SetActive(true);
            selectedspawn.contain.network.power -= 200f;
            player.GetComponentInParent<PlayerNetwork>().CmdRespawn(player.entityID, selectedspawn.entityID);
            player.GetComponentInParent<PlayerNetwork>().CmdPowerExchange(selectedspawn.entityID, 0, 200f, false);
        }
        else
        {
            player.GetComponentInParent<PlayerNetwork>().CmdPowerExchange(activespawn.entityID, 0, 50f, false);
            player.GetComponentInParent<PlayerNetwork>().CmdPowerExchange(selectedspawn.entityID, 0, 50f, false);
            activespawn.contain.network.power -= 50f;
            selectedspawn.contain.network.power -= 50f;
        }
        player.transform.position = selectedspawn.transform.GetChild(5).position;
        usingspawn = false;
        respawning = false;
        //player.GetComponent<Collider>().bounds.min.y
    }
}