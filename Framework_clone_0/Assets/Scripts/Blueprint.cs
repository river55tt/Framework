using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Blueprint : MonoBehaviour
{
    public float maxpower;
    public float power;
    public GameObject[] finishedobject;
    public GameObject[] blueprintobject;
    public List <Transform> snappingfrom;
    //public List <Transform> snappedto;
    public Transform[] snappedto;

    public Transform altsnappingfrom; //beacon or pump
    public Transform altsnappingto; //beacon or pump

    public bool ispipe;
    public bool disabled;

    bool added;

    public Material blueprintmat;
    public Renderer[] blueprintrenderer;
    public Color32 basecolor = new Color32(0,0,0,128);
    public Color32 filledcolor = new Color32(40,230,255,230);
    // Start is called before the first frame update
    void Start()
    {
        blueprintobject[0] = this.gameObject;
        blueprintrenderer[0] = blueprintobject[0].GetComponent<Renderer>();
        //blueprintmat = blueprintrenderer[0].material;
       // blueprintrenderer[0].material = blueprintmat;

        blueprintmat.color = basecolor;

       /* foreach (Renderer bprender in this.transform.GetComponentsInChildren<Renderer>())
        {
            bprender.material = blueprintmat;
        }*/
    }
    public void Place(List<AnchorSystem.Object> optionalconnections = null, AnchorSystem.Chunk optionalchunk = null)
    {
        blueprintmat = blueprintrenderer[0].material;
        EntityProperties thisproperties = this.transform.GetComponent<EntityProperties>();

        foreach (Renderer bprender in this.transform.GetComponentsInChildren<Renderer>())
        {
            bprender.material = blueprintmat;
        }
        thisproperties.snappingfrom = snappingfrom;
        thisproperties.snappedto = snappedto;

        if (altsnappingfrom != null)
        {
            thisproperties.altsnappingfrom = altsnappingfrom;
            thisproperties.altsnappingto = altsnappingto;
        }

        if (thisproperties.snappingtype == -1 || thisproperties.snappingtype == 2)
        {
            thisproperties.snappingtype = 1;
        }
        //if (buildingid == 0)
        //{
        foreach (Transform verticalsnap in snappingfrom)
        {
            if (verticalsnap.tag == "VerticalSnap")
            {
                verticalsnap.Rotate(180, 0, 0, Space.Self);
            }
        }

        //Debug.Log("PLACE");
        AnchorSystem.AnchorManager.CreateObject(thisproperties, optionalconnections, optionalchunk);
       // Debug.Log(thisproperties.anchordata)

        //GameSystem.GameManager.GenerateID(thisproperties, 0);

       // NetworkClient.localPlayer.GetComponent<PlayerNetwork>().CmdPlaceBlueprint(NetworkClient.localPlayer.gameObject, thisproperties.entityID);

        //}
        //else
        //{
        //    AnchorSystem.AnchorManager.CreateObject(this.transform.parent.GetComponentInParent<EntityProperties>());
        //}
    }
    // Update is called once per frame
    void Update()
    {
        if (disabled)
        {
            added = false;
        }
        else
        {
            if(!added)
            {
                added = true;
                FabricatorSystem.FabricatorManager.EditBPList(this, true);
            }
            if (power < maxpower)
            {
                if (power <= 0)
                {
                    GetComponent<EntityProperties>().TakeDamage(100000f, 100f, Vector3.zero, null, false, out List<int> partindex, out _);
                    NetworkClient.localPlayer.GetComponent<PlayerNetwork>().CmdDamageObject(new List<int> { GetComponent<EntityProperties>().entityID }, new List<float> { 100000f }, 
                        new List<float> { 100f }, new List<Vector3> { Vector3.zero }, new List<List<int>> { partindex }, new List<bool> { true });

                    //FabricatorSystem.FabricatorManager.EditBPList(this, false);
                    for (int i = blueprintobject.Length; i > 0; i--)
                    {
                        
                       // Destroy(blueprintobject[i - 1]);
                    }
                }
                blueprintmat.color = Color32.Lerp(basecolor, filledcolor, power / maxpower);
                /*Color32 col = blueprintrenderer.material.GetColor("_Color");
                byte alp = (byte)(.5f + currentiridium / maxiridium * 255f);
                col.a = alp;
                blueprintmat.SetColor("_Color", col);

                blueprintmat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                blueprintmat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                blueprintmat.SetInt("_ZWrite", 0);
                blueprintmat.DisableKeyword("_ALPHATEST_ON");
                blueprintmat.DisableKeyword("_ALPHABLEND_ON");
                blueprintmat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                blueprintmat.renderQueue = 3000;*/
            }
            else
            {

            }
        }
    }

    public void BlueprintFill(float powerchange, AnchorSystem.Chunk optionalchunk = null)//, bool networkaction)
    {
        power += powerchange;

       /* if (networkaction == false)
        {
            PlayerNetwork localplayer = NetworkClient.localPlayer.GetComponent<PlayerNetwork>();
            if (power < maxpower)
            {
                localplayer.CmdPowerExchange(null, transform.GetComponent<EntityProperties>().entityID, power, false);
            }
            else
            {
                localplayer.CmdPowerExchange(null, transform.GetComponent<EntityProperties>().entityID, power, true);
            }
        }*/
        if (power < maxpower)
        {
            blueprintmat.color = Color32.Lerp(basecolor, filledcolor, power / maxpower);
            /*Color32 col = blueprintrenderer.material.GetColor("_Color");
            byte alp = (byte)(.5f + currentiridium / maxiridium * 255f);
            col.a = alp;
            blueprintmat.SetColor("_Color", col);

            blueprintmat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            blueprintmat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            blueprintmat.SetInt("_ZWrite", 0);
            blueprintmat.DisableKeyword("_ALPHATEST_ON");
            blueprintmat.DisableKeyword("_ALPHABLEND_ON");
            blueprintmat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            blueprintmat.renderQueue = 3000;*/
        }
        else
        {
            GameObject buildobject = Instantiate(finishedobject[0], blueprintobject[0].transform.position, blueprintobject[0].transform.rotation);
            EntityProperties buildproperties = buildobject.GetComponent<EntityProperties>();

            List<AnchorSystem.Object> connectingobjects = new List<AnchorSystem.Object>();
            connectingobjects = this.GetComponent<EntityProperties>().anchordata.connectedto;


           // AnchorSystem.AnchorManager.RemoveObject(this.transform.GetComponent<EntityProperties>().anchordata);

            //finalize

            if (!ispipe)
            {
                //Instantiate(finishedobject[0], blueprintobject[0].transform.position, blueprintobject[0].transform.rotation);
                Destroy(blueprintobject[0]);
            }
            else
            {
                //GameObject pipe = Instantiate(finishedobject[0], blueprintobject[0].transform.position, blueprintobject[0].transform.rotation);
                GameObject pipemiddle = Instantiate(finishedobject[1], blueprintobject[1].transform.position, blueprintobject[1].transform.rotation);
                Transform pipebone = blueprintobject[1].transform.GetChild(0).transform.GetChild(0).transform.GetChild(0);
                pipemiddle.transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).position = pipebone.position;
                GameObject pipeend = Instantiate(finishedobject[2], blueprintobject[2].transform.position, blueprintobject[2].transform.rotation);

                pipemiddle.transform.SetParent(buildobject.transform);
                //pipemiddle.GetComponentInChildren<MeshFilter>().mesh.RecalculateBounds();
                pipeend.transform.SetParent(buildobject.transform);
                Bounds newbound = new Bounds(new Vector3(pipebone.localPosition.x / 2, pipebone.localPosition.y / 2, pipebone.localPosition.z / 2),
                    new Vector3(Mathf.Abs(pipebone.localPosition.x) + 0.33f, Mathf.Abs(pipebone.localPosition.y) + 0.33f, Mathf.Abs(pipebone.localPosition.z) + 0.33f));
                pipemiddle.GetComponentInChildren<SkinnedMeshRenderer>().localBounds = newbound;
                BoxCollider col = pipemiddle.AddComponent<BoxCollider>();
                col.center = new Vector3(0, 0, -pipebone.localPosition.y / 2 * 100f);
                col.size = new Vector3(0.33f * 1, 0.33f * 1, Mathf.Abs(pipebone.localPosition.y * 100f));

                //pipeend.name = "gamer";

                //pipe.GetComponent<EntityProperties>().snappingfrom = snappingfrom;
                pipeend.GetComponent<EntityProperties>().enabled = false;
                Destroy(pipeend.GetComponent<EntityProperties>());
                Destroy(pipeend.GetComponent<Rigidbody>());
                //Destroy(blueprintobject[0]);
            }
            buildproperties.snappedto = snappedto;
            if (altsnappingto != null)
            {
                buildproperties.altsnappingto = altsnappingto;
                altsnappingto.GetComponentInParent<EntityProperties>().altsnappingto = altsnappingfrom;
            }

            int i = 0;
            foreach (Transform child in buildobject.GetComponentsInChildren<Transform>())   //update my snap points
            {
                if(child.gameObject.tag == "Snap") //maybe issue
                {
                    //Debug.Log("do things");
                    buildproperties.snappingfrom.Add(child);
                    child.gameObject.SetActive(snappingfrom[i].gameObject.activeSelf);
                    i++;
                }
                /*foreach (BoxCollider snap in child.GetComponents<BoxCollider>())
                {
                    if (snap.isTrigger && snap.gameObject.tag == "Snap")//.activeSelf)
                    {
                        buildproperties.snappingfrom.Add(child);
                        child.gameObject.SetActive(snappingfrom[i].gameObject.activeSelf);
                        i++;
                        break;
                    }
                }*/
                //snappingfrom.Find(x => x == child).
            }
            //Debug.Log(i);
            for (int j = 0; j < snappedto.Length; j++) //update others snap points
            {
                Transform snapupdate = snappedto[j];
                if (snapupdate != null)
                {
                    Blueprint bp = snapupdate.GetComponentInParent<Blueprint>();
                    EntityProperties properties = snapupdate.GetComponentInParent<EntityProperties>();
                    if (bp == null)
                    {
                        int foundindex = properties.snappingfrom.FindIndex(x => x == snapupdate);
                        properties.snappedto[foundindex] = buildproperties.snappingfrom[j];
                    }
                    else
                    {
                        int foundindex = bp.snappingfrom.FindIndex(x => x == snapupdate);
                        bp.snappedto[foundindex] = buildproperties.snappingfrom[j];
                    }
                }
            }
            if(ispipe)
            {
                buildproperties.snappingfrom[1].transform.SetParent(buildproperties.transform);
            }
            buildproperties.teamid = blueprintobject[0].GetComponent<EntityProperties>().teamid;
            buildproperties.entityID = blueprintobject[0].GetComponent<EntityProperties>().entityID;
            buildproperties.buildingid = blueprintobject[0].GetComponent<EntityProperties>().buildingid;

            GameSystem.GameManager.entityIDs.Remove(buildproperties.entityID);
            GameSystem.GameManager.entityIDs.Add(buildproperties.entityID, buildproperties);

            if (buildproperties.powerscale != null)
            {
                //
                //buildproperties
            }
            PowerNetworks.PowerManager.AddObject(buildproperties);
            if (buildobject.transform.parent != null) //does this do anything? IMPOPRTANT
            {
               // buildobject.transform.SetParent(buildobject.transform.parent.GetComponentInParent<EntityProperties>().transform);
            }
            blueprintobject[0].SetActive(false);
            //Physics.SyncTransforms();

            AnchorSystem.AnchorManager.CreateObject(buildproperties, connectingobjects, optionalchunk);
            AnchorSystem.AnchorManager.RemoveObject(this.transform.GetComponent<EntityProperties>().anchordata);
            foreach (AnchorSystem.Object connections in connectingobjects) //buildproperties.anchordata.connectedto)
            {
                //Debug.Log(connections.objectproperties.gameObject.name);
            }
            //AnchorSystem.AnchorManager.AddConnections(buildproperties.anchordata, connectingobjects);

            /*foreach (AnchorSystem.Object connections in connectingobjects) //buildproperties.anchordata.connectedto)
            {
                Debug.Log(connections.objectproperties.gameObject.name);
            }*/

            FabricatorSystem.FabricatorManager.EditBPList(this,false);
            /*Debug.Log("hi");
            Debug.Log(blueprintobject[0]);
            //Debug.Log("hi2");*/
            blueprintobject[0].GetComponent<EntityProperties>().entityID = -1;
            Destroy(blueprintobject[0]);
            //Debug.Log("hi2");
        }
    }

    /*void OnTriggerEnter(Collider other)  //check for physical connections
    {
        //Debug.Log("hi");
        if (!other.isTrigger)
        {

            EntityProperties otherstats = other.GetComponentInParent<EntityProperties>();
            if (otherstats != null && otherstats.teamnumber != turretstats.teamnumber && otherstats.buildingid == -1 && !targets.Contains(otherstats.transform))
            {
                Debug.Log("hi");
                targets.Add(otherstats.transform);
            }
        }
    }*/
}