using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BuildSystem : MonoBehaviour
{
    public Transform player;
    public PlayerMovement playermovement;
    public EntityProperties playerproperties;
    public Transform cam;
    public Transform cursor;
    public bool combatmode;
   // public GameObject[] buildinglist;
    public GameObject[] blueprintlist;
    
    GameObject blueprintdisplay;

    bool reselect;
    int buildingcategory;
    int buildingselection;

    public Material blueprintmaterial;

    public bool piping;
    public List<Transform> bpsnap;
    public int activebpsnap;
    public Transform snapping;
    EntityProperties buildingon;

    bool interacting;
    public WeaponBehavior destroybeam;

    GameObject parentpipe;
    Blueprint parentBP;
    GameObject pipemiddle;
    Transform pipebone;
    Transform savedsnap;

    public List<BoxCollider> checkbox;

    public float rotationoffset;

    public Color32 validplacecolor;
    public Color32 invalidplacecolor;
    public Color32 glass;
    public Material glassbpmat;
    bool validplace;

    public GameObject buildmenu;
    public GameObject handfab;
    public LineRenderer handfabbeam;

    bool altsnap;

    //public List<GameObject> cube;
    // Start is called before the first frame update
    void Start()
    {
        handfabbeam = handfab.GetComponentInChildren<LineRenderer>();
        handfabbeam.startWidth = .05f;
        handfabbeam.endWidth = .2f;
        handfabbeam.enabled = false;

        buildmenu = PlayerUI.LocalPlayerUI.buildMenu;
        cursor = PlayerUI.LocalPlayerUI.cursor;
    }

    // Update is called once per frame
    void Update()
    {
        if (GetComponentInParent<PlayerNetwork>().isLocalPlayer && Camera.main)
        {
            Ray ray = Camera.main.ScreenPointToRay(cursor.position);
            bool interact = Input.GetButton("Interact");
            bool interactdown = Input.GetButtonDown("Interact");
            bool lmouse = Input.GetMouseButton(0);
            bool lmousedown = Input.GetMouseButtonDown(0);
            bool destroydown = Input.GetButtonDown("Destroy");
            if (!combatmode)
            {
                handfab.SetActive(true);
                if (Input.GetButtonDown("BuildingCancel"))
                {
                    //buildingselection = 0;
                    buildingcategory = 0;
                    /*reselect = false;
                    if (blueprintdisplay != null)
                    {
                        Destroy(blueprintdisplay);
                        Destroy(parentpipe);
                        Destroy(pipemiddle);
                        Destroy(blueprintdisplay);
                        pipebone = null;
                        piping = false;
                        checkbox.Clear();
                        if (savedsnap != null)
                        {
                            savedsnap.gameObject.SetActive(true);
                            savedsnap = null;
                        }
                    }*/
                }
                if (Input.GetKeyDown(KeyCode.Alpha1)) //index + 1
                {
                    if (buildingcategory == 0)
                    {
                        buildingcategory = 1;
                    }
                    else if (buildingcategory == 1)
                    {
                        buildingselection = 1;
                    }
                    else if (buildingcategory == 2)
                    {
                        buildingselection = 7;
                    }
                    else if (buildingcategory == 3)
                    {
                        buildingselection = 9;
                    }
                    else if (buildingcategory == 4)
                    {
                        buildingselection = 13;
                    }
                    else if (buildingcategory == 5)
                    {
                        buildingselection = 18;
                    }
                    reselect = true;
                }
                else if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    if (buildingcategory == 0)
                    {
                        buildingcategory = 2;
                    }
                    else if (buildingcategory == 1)
                    {
                        buildingselection = 3;
                    }
                    else if (buildingcategory == 2)
                    {
                        buildingselection = 6;
                    }
                    else if (buildingcategory == 3)
                    {
                        buildingselection = 10;
                    }
                    else if (buildingcategory == 4)
                    {
                        buildingselection = 16;
                    }
                    else if (buildingcategory == 5)
                    {
                        buildingselection = 19;
                    }
                    reselect = true;
                }
                else if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    if (buildingcategory == 0)
                    {
                        buildingcategory = 3;
                    }
                    else if (buildingcategory == 1)
                    {
                        buildingselection = 4;
                    }
                    else if (buildingcategory == 2)
                    {

                    }
                    else if (buildingcategory == 3)
                    {
                        buildingselection = 14;
                    }
                    else if (buildingcategory == 4)
                    {
                        buildingselection = 17;
                    }
                    else if (buildingcategory == 5)
                    {
                        buildingselection = 20;
                    }
                    reselect = true;
                }
                else if (Input.GetKeyDown(KeyCode.Alpha4))
                {
                    if (buildingcategory == 0)
                    {
                        buildingcategory = 4;
                    }
                    else if (buildingcategory == 1)
                    {
                        buildingselection = 5;
                    }
                    else if (buildingcategory == 2)
                    {

                    }
                    else if (buildingcategory == 3)
                    {
                        buildingselection = 15;
                    }
                    else if (buildingcategory == 4)
                    {
                        buildingselection = 26;
                    }
                    else if (buildingcategory == 5)
                    {
                        buildingselection = 21;
                    }
                    reselect = true;
                }
                else if (Input.GetKeyDown(KeyCode.Alpha5))
                {
                    if (buildingcategory == 0)
                    {
                        buildingcategory = 5;
                    }
                    if (buildingcategory == 1)
                    {
                        buildingselection = 11;
                    }
                    else if (buildingcategory == 2)
                    {

                    }
                    else if (buildingcategory == 3)
                    {
                        buildingselection = 24;
                    }
                    else if (buildingcategory == 4)
                    {
                        buildingselection = 27;
                    }
                    else if (buildingcategory == 5)
                    {
                        buildingselection = 22;
                    }
                    reselect = true;
                }
                else if (Input.GetKeyDown(KeyCode.Alpha6))
                {
                    if (buildingcategory == 1)
                    {
                        buildingselection = 12;
                    }
                    else if (buildingcategory == 2)
                    {

                    }
                    else if (buildingcategory == 3)
                    {
                        buildingselection = 28;
                    }
                    else if (buildingcategory == 5)
                    {
                        buildingselection = 23;
                    }
                    reselect = true;
                }
                else if (Input.GetKeyDown(KeyCode.Alpha7))
                {
                    if (buildingcategory == 1)
                    {
                        buildingselection = 8;
                    }
                    else if (buildingcategory == 2)
                    {

                    }
                    else if (buildingcategory == 3)
                    {

                    }
                    else if (buildingcategory == 5)
                    {
                        buildingselection = 25;
                    }
                    reselect = true;
                }

                buildmenu.transform.GetChild(0).gameObject.SetActive(false);
                buildmenu.transform.GetChild(1).gameObject.SetActive(false);
                buildmenu.transform.GetChild(2).gameObject.SetActive(false);
                buildmenu.transform.GetChild(3).gameObject.SetActive(false);
                buildmenu.transform.GetChild(4).gameObject.SetActive(false);
                buildmenu.transform.GetChild(5).gameObject.SetActive(false);
                if (buildingcategory == 0)
                {
                    buildmenu.transform.GetChild(5).gameObject.SetActive(true); //categories
                }
                else if (buildingcategory == 1)
                {
                    buildmenu.transform.GetChild(0).gameObject.SetActive(true); //pipes
                }
                else if (buildingcategory == 2)
                {
                    buildmenu.transform.GetChild(1).gameObject.SetActive(true); //power
                }
                else if (buildingcategory == 3)
                {
                    buildmenu.transform.GetChild(2).gameObject.SetActive(true); //tech
                }
                else if (buildingcategory == 4)
                {
                    buildmenu.transform.GetChild(3).gameObject.SetActive(true); //military
                }
                else if (buildingcategory == 5)
                {
                    buildmenu.transform.GetChild(4).gameObject.SetActive(true); //military
                }
                /*else if (Input.GetKeyDown(KeyCode.Alpha7))
                {
                    buildingselection = 8;
                    reselect = true;
                }*/
                if (interactdown)
                {
                    if (buildingselection > 0)
                    {
                        Destroy(blueprintdisplay);
                        Destroy(parentpipe);
                        Destroy(pipemiddle);
                        Destroy(blueprintdisplay);

                        checkbox.Clear();
                        bpsnap.Clear();
                        pipebone = null;
                        piping = false;
                        if (savedsnap != null)
                        {
                            savedsnap.gameObject.SetActive(true);
                            savedsnap = null;
                        }
                    }
                    buildingselection = 0;
                }

                if (buildingselection > 0 && reselect)
                {
                    //Debug.Log("Switch");
                    Destroy(blueprintdisplay);
                    Destroy(parentpipe);
                    Destroy(pipemiddle);
                    Destroy(blueprintdisplay);

                    checkbox.Clear();
                    bpsnap.Clear();
                    pipebone = null;
                    piping = false;
                    if (savedsnap != null)
                    {
                        savedsnap.gameObject.SetActive(true);
                        savedsnap = null;
                    }

                    blueprintdisplay = Instantiate(blueprintlist[buildingselection - 1], player.position, player.rotation);
                    foreach (Renderer bprender in blueprintdisplay.GetComponentsInChildren<Renderer>())
                    {
                        // bprender.material = blueprintmaterial;
                        if (bprender.transform.tag != "Glass")
                        {
                            //Debug.Log("not glass");
                            //Debug.Log(bprender);
                            bprender.material = blueprintmaterial;
                        }
                        else
                        {
                            //Debug.Log("glass");
                            bprender.material = glassbpmat;
                        }
                        //blueprintdisplay.GetComponent<Renderer>().material = blueprintmaterial;
                    }
                    blueprintdisplay.SetActive(false);
                    reselect = false;
                    rotationoffset = 0;
                    blueprintdisplay.GetComponent<Blueprint>().disabled = true;

                    foreach (Transform child in blueprintdisplay.GetComponentsInChildren<Transform>())
                    {
                        foreach (BoxCollider snap in child.GetComponents<BoxCollider>())
                        {
                            if (snap.isTrigger && snap.enabled && snap.transform.tag == "Snap")
                            {
                                bpsnap.Add(child);
                                break;
                            }
                            else if (snap.transform.tag == "Check")
                            {
                                checkbox.Add(snap);
                            }
                        }
                    }
                    if (bpsnap.Count == 0 && blueprintdisplay.GetComponent<EntityProperties>().altsnappingfrom == null)
                    {
                        activebpsnap = -1;
                    }
                    else
                    {
                        activebpsnap = 0;
                    }
                }
                bool beamenabled = false;
                //Vector3[] beampositionsreset = new Vector3[2];
                handfabbeam.SetPosition(1, Vector3.zero);

                if (buildingselection == 0 && (lmouse || interact))
                {
                    if (!interacting && (lmousedown || interactdown))
                    {
                        interacting = true;
                    }
                    if (interacting)
                    {
                        RaycastHit hit2;
                        //LayerMask mask2 = LayerMask.GetMask("Blueprint");
                        int mask3 = (1 << 8) | (1 << 10);
                        if (Physics.Raycast(ray, out hit2, 500f, mask3, QueryTriggerInteraction.Collide))
                        {
                            //Debug.Log(hit2.collider.gameObject);
                            Blueprint tempfillblueprint = hit2.collider.GetComponentInParent<Blueprint>();
                            if ((tempfillblueprint != null && !tempfillblueprint.disabled) || (hit2.transform.gameObject.layer == 10 && hit2.transform.gameObject.tag == "AccessPoint"))
                            {
                                float powerfill;
                                bool finished = false;
                                float maxpower;
                                float power;
                                EntityProperties tempproperties = hit2.collider.GetComponentInParent<EntityProperties>();

                                powerfill = 30f * Time.deltaTime; //output or intake
                                if (tempfillblueprint != null)
                                {
                                    maxpower = tempfillblueprint.maxpower;
                                    power = tempfillblueprint.power;
                                }
                                else
                                {
                                    maxpower = tempproperties.contain.network.powerstorage;
                                    power = tempproperties.contain.network.power;
                                    powerfill *= 2;
                                }

                                if (lmouse)
                                {
                                    if (powerfill > playerproperties.power) //output more than current power
                                    {
                                        powerfill = playerproperties.power;
                                    }
                                    if (powerfill > maxpower - power)
                                    {
                                        powerfill = maxpower - power;
                                        finished = true;
                                    }
                                }
                                else if (interact)
                                {
                                    if (powerfill > playerproperties.maxpower - playerproperties.power) //intake more than empty storage
                                    {
                                        powerfill = playerproperties.maxpower - playerproperties.power;
                                    }
                                    if (powerfill > power)
                                    {
                                        powerfill = power;
                                    }
                                    powerfill *= -1;
                                }
                                if (tempfillblueprint != null)
                                {
                                    NetworkClient.localPlayer.GetComponent<PlayerNetwork>().CmdPowerExchange(playerproperties.entityID, tempfillblueprint.GetComponent<EntityProperties>().entityID, powerfill, finished);
                                    tempfillblueprint.BlueprintFill(powerfill);
                                   // NetworkClient.localPlayer.GetComponent<PlayerNetwork>().CmdPowerExchange(playerproperties.entityID, tempfillblueprint.GetComponent<EntityProperties>().entityID, powerfill, finished);
                                }
                                else
                                {
                                    tempproperties.contain.network.power += powerfill;
                                }
                                playerproperties.power -= powerfill;

                                Vector3[] beampositions = new Vector3[2];
                                beampositions[0] = handfab.transform.position;
                                beampositions[1] = hit2.point;
                                handfabbeam.SetPositions(beampositions);
                                beamenabled = true;
                            }
                            else if(interactdown && hit2.collider.gameObject.tag == "Spawn")
                            {
                                SpawnSystem.SpawnManager.usingspawn = true;
                                SpawnSystem.SpawnManager.player = playerproperties;
                                SpawnSystem.SpawnManager.activespawn = hit2.collider.GetComponentInParent<EntityProperties>();
                            }
                            /*else if(hit2.transform.gameObject.layer == 10 && hit2.transform.gameObject.tag == "AccessPoint")
                            {
                                EntityProperties tempproperties = hit2.transform.GetComponentInParent<EntityProperties>();\
                                                            float powerfill;
                                powerfill = 30f * Time.deltaTime; //output or intake
                                if (lmouse)
                                {
                                    if (powerfill > playerproperties.power) //output more than current power
                                    {
                                        powerfill = playerproperties.power;
                                    }
                                }
                                else if (interact)
                                {
                                    if (powerfill > playerproperties.maxpower - playerproperties.power) //intake more than empty storage
                                    {
                                        powerfill = playerproperties.maxpower - playerproperties.power;
                                    }
                                    powerfill *= -1;
                                }
                            }*/
                        }
                    }
                }
                else
                {
                    interacting = false;
                }

                if (destroydown)
                {
                    RaycastHit hit3;
                    int mask3 = (1 << 8) | (1 << 10);
                    if (Physics.Raycast(ray, out hit3, 500f, mask3, QueryTriggerInteraction.Ignore))
                    {
                        //Debug.Log(hit2.collider.gameObject);
                        Blueprint tempblueprint = hit3.collider.GetComponentInParent<Blueprint>();
                        EntityProperties tempbuilding = hit3.collider.GetComponentInParent<EntityProperties>();
                        destroybeam.Shoot(hit3.point);
                        if (tempblueprint != null && !tempblueprint.disabled && tempbuilding.teamid == playerproperties.teamid)
                        {
                            tempbuilding.health = 0;
                            tempbuilding.TakeDamage(tempbuilding.maxhealth, 100f, Vector3.zero, null, false, out List<int> partindex, out _);
                            NetworkClient.localPlayer.GetComponent<PlayerNetwork>().CmdDamageObject(new List<int> { tempbuilding.entityID }, new List<float> { 100000f }, 
                                new List<float> { 100f }, new List<Vector3> { Vector3.zero }, new List<List<int>> { partindex }, new List<bool> { true });

                            //FabricatorSystem.FabricatorManager.EditBPList(tempblueprint, false);
                            //Destroy(tempblueprint.gameObject);
                        }
                        else if (tempbuilding != null && tempbuilding.teamid == playerproperties.teamid)
                        {
                            tempbuilding.TakeDamage(tempbuilding.maxhealth / 2, 100f, Vector3.zero, null, false, out List<int> partindex, out bool dead);
                            NetworkClient.localPlayer.GetComponent<PlayerNetwork>().CmdDamageObject(new List<int> { tempbuilding.entityID }, new List<float> { tempbuilding.maxhealth / 2 },
                                new List<float> { 100f }, new List<Vector3> { Vector3.zero }, new List<List<int>> { partindex }, new List<bool> { dead });
                        }
                    }
                }

                handfabbeam.enabled = beamenabled;
                RaycastHit hit;
                LayerMask mask = LayerMask.GetMask("Ground");
                // LayerMask mask2 = LayerMask.GetMask("Blueprint");
                int buildmask = (1 << 6) | (1 << 8) | (1 << 10);
                if (buildingselection > 0 && blueprintdisplay != null)
                {
                    int snaptype = blueprintdisplay.GetComponent<EntityProperties>().snappingtype;
                    if (Input.GetButtonDown("Rotate"))
                    {
                        if (Input.GetButton("Sprint"))
                        {
                            rotationoffset += 90;
                        }
                        else
                        {
                            rotationoffset += -90;
                        }
                    }
                    if (Input.GetButtonDown("Snap") && activebpsnap != -1)
                    {
                        if (Input.GetButton("Sprint"))
                        {
                            activebpsnap = -2;
                        }
                        else
                        {
                            if (activebpsnap == -2 || activebpsnap == bpsnap.Count - 1)
                            {
                                activebpsnap = 0;
                            }
                            else
                            {
                                activebpsnap++;
                            }
                        }
                    }
                    if (Input.GetButton("Sprint"))
                    {
                        rotationoffset += -Input.mouseScrollDelta.y * 15;
                        rotationoffset = Mathf.Round(rotationoffset / 15) * 15;
                        playermovement.buildrotate = true;
                    }
                    else
                    {
                        playermovement.buildrotate = false;
                    }
                    //Debug.Log(rotationoffset);

                    if (Physics.Raycast(ray, out hit, 500f, buildmask, QueryTriggerInteraction.Collide))
                    {
                        //Debug.Log(hit.transform.gameObject.layer);
                        altsnap = false;

                        blueprintdisplay.SetActive(true);
                        EntityProperties snapproperties = hit.collider.GetComponentInParent<EntityProperties>();

                        if (activebpsnap >= 0 && hit.collider.isTrigger && snapproperties != null && snapproperties.snappingtype == snaptype) //snap to object
                        {
                            List<Transform> testsnap;
                            buildingon = null;
                            if (hit.collider.GetComponentInParent<Blueprint>() != null)
                            {
                                testsnap = hit.collider.GetComponentInParent<Blueprint>().snappingfrom;
                                foreach (Transform snapcol in testsnap)
                                {
                                    if (snapcol == hit.collider.transform)
                                    {
                                        //Debug.Log("snap");
                                        //snap to object
                                        // Debug.Log(bpsnap[activebpsnap].localPosition);
                                        //blueprintdisplay.transform.position = snapcol.position - bpsnap[activebpsnap].localPosition*1;
                                        blueprintdisplay.transform.rotation = snapcol.rotation * Quaternion.Euler(0, 180 - bpsnap[activebpsnap].localEulerAngles.y, 0); //* bpsnap[activebpsnap].localRotation
                                                                                                                                                                        //blueprintdisplay.transform.Rotate(-Vector3.forward, Mathf.Round((cam.eulerAngles.y-snapcol.eulerAngles.y) / 15) * 15 + rotationoffset + 180);

                                        //blueprintdisplay.transform.Rotate(blueprintdisplay.transform.rotation * -snapcol.right, Mathf.Round((cam.eulerAngles.y - snapcol.eulerAngles.y) / 15) * 15 + rotationoffset + 180);

                                        blueprintdisplay.transform.Rotate(bpsnap[activebpsnap].localRotation * -Vector3.forward, Mathf.Round((cam.eulerAngles.y - snapcol.eulerAngles.y) / 15) * 15 + rotationoffset + 180);

                                        blueprintdisplay.transform.position = snapcol.position - blueprintdisplay.transform.rotation * bpsnap[activebpsnap].localPosition * 1;
                                        //blueprintdisplay.transform.Rotate(Vector3.forward, Mathf.Round(cam.eulerAngles.y / 15) * 15 + rotationoffset);
                                        //blueprintdisplay.transform.rotation *= bpsnap[activebpsnap].localRotation;
                                        //blueprintdisplay.transform.rotation = Quaternion.FromToRotation(blueprintdisplay.transform.forward, -bpsnap[activebpsnap].forward);
                                        //blueprintdisplay.transform.rotation *= Quaternion.Euler(0, 180, 0);
                                        snapping = snapcol;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                testsnap = snapproperties.snappingfrom;
                                foreach (Transform snapcol in testsnap)
                                {
                                    if (snapcol == hit.collider.transform)
                                    {
                                        //Debug.Log("snap");
                                        //snap to object
                                        //blueprintdisplay.transform.position = snapcol.position;
                                        //blueprintdisplay.transform.rotation = snapcol.rotation;
                                        //blueprintdisplay.transform.position = snapcol.position - bpsnap[activebpsnap].localPosition*1;
                                        blueprintdisplay.transform.rotation = snapcol.rotation * Quaternion.Euler(0, 180 - bpsnap[activebpsnap].localEulerAngles.y, 0); //* bpsnap[activebpsnap].localRotation
                                                                                                                                                                        //blueprintdisplay.transform.Rotate(-Vector3.forward, Mathf.Round((cam.eulerAngles.y - snapcol.eulerAngles.y) / 15) * 15 + rotationoffset + 180);

                                        //blueprintdisplay.transform.Rotate(blueprintdisplay.transform.rotation * -snapcol.right, Mathf.Round((cam.eulerAngles.y - snapcol.eulerAngles.y) / 15) * 15 + rotationoffset + 180);

                                        blueprintdisplay.transform.Rotate(bpsnap[activebpsnap].localRotation * -Vector3.forward, Mathf.Round((cam.eulerAngles.y - snapcol.eulerAngles.y) / 15) * 15 + rotationoffset + 180);

                                        blueprintdisplay.transform.position = snapcol.position - blueprintdisplay.transform.rotation * bpsnap[activebpsnap].localPosition * 1;

                                        snapping = snapcol;
                                        break;
                                    }
                                }
                                Transform testsnap2 = hit.collider.GetComponentInParent<EntityProperties>().altsnappingfrom;
                                Transform altsnaptransform = blueprintdisplay.GetComponent<EntityProperties>().altsnappingfrom;

                                if (testsnap2 == hit.collider.transform)
                                {
                                    blueprintdisplay.transform.rotation = testsnap2.rotation * Quaternion.Euler(0, 180 - altsnaptransform.localEulerAngles.y, 0);
                                    blueprintdisplay.transform.Rotate(altsnaptransform.localRotation * -Vector3.forward, Mathf.Round((cam.eulerAngles.y - testsnap2.eulerAngles.y) / 15) * 15 + rotationoffset + 180);
                                    blueprintdisplay.transform.position = testsnap2.position - blueprintdisplay.transform.rotation * altsnaptransform.localPosition * 1;

                                    altsnap = true;
                                    snapping = testsnap2;
                                }
                            }
                        }
                        else //dont snap to object
                        {
                            snapping = null;
                            blueprintdisplay.transform.position = hit.point;
                            blueprintdisplay.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                            blueprintdisplay.transform.Rotate(Vector3.up, Mathf.Round(cam.eulerAngles.y / 15) * 15 + rotationoffset);
                            //Debug.Log(Mathf.Round(cam.eulerAngles.y/15)*15);
                            //Debug.Log(cam.eulerAngles.y/15);
                            if (piping)
                            {
                                blueprintdisplay.transform.Rotate(Vector3.up, 180);
                            }
                            buildingon = snapproperties;
                        }
                        if (piping)
                        {
                            pipemiddle.SetActive(true);
                            if (pipemiddle.transform.position != blueprintdisplay.transform.GetChild(0).position)
                            {
                                pipemiddle.transform.rotation = Quaternion.LookRotation(pipemiddle.transform.position - blueprintdisplay.transform.GetChild(0).position);
                            }
                            pipebone.transform.position = blueprintdisplay.transform.GetChild(0).position;
                            Bounds newbound = new Bounds(new Vector3(pipebone.localPosition.x / 2, pipebone.localPosition.y / 2, pipebone.localPosition.z / 2),
                                new Vector3(Mathf.Abs(pipebone.localPosition.x) + 0.33f, Mathf.Abs(pipebone.localPosition.y) + 0.33f, Mathf.Abs(pipebone.localPosition.z) + 0.33f));
                            pipemiddle.GetComponentInChildren<SkinnedMeshRenderer>().localBounds = newbound;

                            foreach (Transform child in pipemiddle.GetComponentsInChildren<Transform>())
                            {
                                if (child.tag == "Check")
                                {
                                    BoxCollider col = child.GetComponent<BoxCollider>();
                                    col.center = new Vector3(0, 0, -pipebone.localPosition.y / 2 * 100f);
                                    col.size = new Vector3(0.200f * 1, 0.200f * 1, Mathf.Abs(pipebone.localPosition.y * 100f));
                                    break;
                                }
                            }
                        }



                        //CHECK FOR PLACEMENT
                        int mask2 = buildmask | (1 << 9);
                        validplace = true;
                        Physics.SyncTransforms();
                        /*foreach (GameObject cubedelete in cube)
                        {
                            Destroy(cubedelete);
                        }
                        cube.Clear();*/
                        foreach (BoxCollider checkhitbox in checkbox)
                        {
                            //Collider[] overlapcol = Physics.OverlapBox(checkhitbox.transform.TransformPoint(checkhitbox.center), checkhitbox.size/2, checkhitbox.transform.rotation, mask2, QueryTriggerInteraction.Ignore);
                            //if (checkhitbox != checkbox[1])
                            if (true)
                            {
                                Vector3 worldCenter = checkhitbox.transform.TransformPoint(checkhitbox.center);
                                Vector3 worldHalfExtents = checkhitbox.size * 0.5f; // only necessary when collider is scaled by non-uniform transform
                                Collider[] overlapcol = Physics.OverlapBox(worldCenter, worldHalfExtents, checkhitbox.transform.rotation, mask2, QueryTriggerInteraction.Ignore);

                                //Debug.Log(checkbox[1].size);

                                /*GameObject cube1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                cube1.transform.position = worldCenter;
                                cube1.transform.localScale = worldHalfExtents * 2;
                                cube1.transform.rotation = checkhitbox.transform.rotation;
                                cube1.GetComponent<Renderer>().material.color = Color.red;
                                cube.Add(cube1);*/

                                foreach (Collider intersect in overlapcol)
                                {
                                    BoxCollider boxintersect = intersect as BoxCollider;
                                    if (boxintersect != checkhitbox && intersect.transform.gameObject.layer != 9 && intersect.tag != "WeldingBox")
                                    {
                                        // Debug.Log(checkhitbox.transform.parent);
                                        //Debug.Log(intersect.transform.gameObject);
                                        validplace = false;
                                        break;
                                    }
                                    else if (boxintersect != null && checkbox.Contains(boxintersect) && boxintersect != checkhitbox && intersect.tag == "Check")
                                    {
                                        // Debug.Log(checkhitbox.transform.parent);
                                        // Debug.Log(intersect.transform.parent);
                                        validplace = false;
                                        break;
                                    }
                                }
                            }
                        }
                        if (blueprintdisplay.GetComponent<EntityProperties>().altsnappingfrom != null && !altsnap)
                        {
                            validplace = false;
                        }


                        if (validplace)
                        {
                            blueprintmaterial.color = validplacecolor;
                            blueprintdisplay.GetComponent<Renderer>().material.color = validplacecolor;

                            glass = validplacecolor;
                            glass.a = 40;
                            glassbpmat.color = glass;
                            if (piping)
                            {
                                parentpipe.GetComponent<Renderer>().material.color = validplacecolor;
                                pipemiddle.GetComponentInChildren<Renderer>().material.color = validplacecolor;
                            }
                        }
                        else
                        {
                            blueprintmaterial.color = invalidplacecolor;
                            blueprintdisplay.GetComponent<Renderer>().material.color = invalidplacecolor;

                            glass = invalidplacecolor;
                            glass.a = 40;
                            glassbpmat.color = glass;
                            if (piping)
                            {
                                parentpipe.GetComponent<Renderer>().material.color = invalidplacecolor;
                                pipemiddle.GetComponentInChildren<Renderer>().material.color = invalidplacecolor;
                            }
                        }

                        if (lmousedown && validplace)
                        {
                            if (!piping)
                            {
                                if (buildingselection != 1)
                                {
                                    PlaceObject(blueprintlist[buildingselection - 1], blueprintdisplay.transform.position, blueprintdisplay.transform.rotation, 0, snapping);
                                    //PlaceObject(blueprintlist[buildingselection - 1], buildinglist[buildingselection - 1], blueprintdisplay.transform.position, blueprintdisplay.transform.rotation, 0, snapping);
                                    buildingselection = 0;
                                    reselect = false;
                                    Destroy(blueprintdisplay);
                                }
                                else
                                {
                                    PlaceObject(blueprintlist[0], blueprintdisplay.transform.position, blueprintdisplay.transform.rotation, 1, snapping);
                                    //PlaceObject(blueprintlist[0], buildinglist[0], blueprintdisplay.transform.position, blueprintdisplay.transform.rotation, 1, snapping);
                                    pipemiddle = Instantiate(blueprintlist[1], parentpipe.transform.GetChild(0).position, player.rotation);
                                    foreach (Transform boxchild in pipemiddle.GetComponentsInChildren<Transform>())
                                    {
                                        if (boxchild.tag == "Check")
                                        {
                                            checkbox.Add(boxchild.gameObject.AddComponent<BoxCollider>());
                                        }
                                    }
                                    foreach (BoxCollider boxchild in parentBP.GetComponentsInChildren<BoxCollider>())
                                    {
                                        if (boxchild.transform.tag == "Check")
                                        {
                                            checkbox.Add(boxchild);
                                        }
                                    }
                                    pipebone = pipemiddle.transform.GetChild(0).transform.GetChild(0).transform.GetChild(0);
                                    piping = true;
                                }
                            }
                            else
                            {
                                PlaceObject(blueprintlist[1], pipemiddle.transform.position, pipemiddle.transform.rotation, 2, null);
                                PlaceObject(blueprintlist[0], blueprintdisplay.transform.position, blueprintdisplay.transform.rotation, 3, snapping);
                                //PlaceObject(blueprintlist[1], buildinglist[1], pipemiddle.transform.position, pipemiddle.transform.rotation, 2, null);
                                //PlaceObject(blueprintlist[0], buildinglist[0], blueprintdisplay.transform.position, blueprintdisplay.transform.rotation, 3, snapping);
                                buildingselection = 0;
                                reselect = false;
                                Destroy(blueprintdisplay);
                                Destroy(pipemiddle);
                                parentpipe = null;
                                pipebone = null;
                                piping = false;
                                checkbox.Clear();
                            }
                        }
                    }
                    /*    else if (Physics.Raycast(ray, out hit, 500f, mask, QueryTriggerInteraction.Ignore))
                        {
                            blueprintdisplay.SetActive(true);
                            blueprintdisplay.transform.position = hit.point;
                            blueprintdisplay.transform.rotation = cam.rotation;
                            if (Input.GetMouseButton(0))
                            {
                                PlaceObject(blueprintlist[buildingselection - 1], buildinglist[buildingselection - 1], hit.point, cam.rotation);
                                buildingselection = 0;
                                currentselection = 0;
                                Destroy(blueprintdisplay);
                            }
                        }*/
                    else
                    {
                        blueprintdisplay.SetActive(false);
                        if (piping)
                        {
                            pipemiddle.SetActive(false);
                        }
                    }
                }
                else
                {
                    playermovement.buildrotate = false;
                }
                /*if(destroypress)
                {
                    RaycastHit hit2;
                    LayerMask mask2 = LayerMask.GetMask("Blueprint");
                    if (Physics.Raycast(ray, out hit2, 500f, mask2, QueryTriggerInteraction.Collide))
                    {
                        Blueprint tempfillblueprint = hit2.transform.GetComponentInParent<Blueprint>();
                        if (tempfillblueprint != null && !tempfillblueprint.disabled)
                        {
                            FabricatorSystem.FabricatorManager.EditBPList(tempfillblueprint,false);
                            Destroy(tempfillblueprint.gameObject);
                        }
                    }
                }*/
            }
            else
            {
                handfab.SetActive(false);
                buildmenu.transform.GetChild(0).gameObject.SetActive(false);
                buildmenu.transform.GetChild(1).gameObject.SetActive(false);
                buildmenu.transform.GetChild(2).gameObject.SetActive(false);
                buildmenu.transform.GetChild(3).gameObject.SetActive(false);
                buildmenu.transform.GetChild(4).gameObject.SetActive(false);

                playermovement.buildrotate = false;
                buildingselection = 0;
                buildingcategory = 0;
                reselect = false;
                Destroy(blueprintdisplay);
                Destroy(parentpipe);
                Destroy(pipemiddle);
                Destroy(blueprintdisplay);
                pipebone = null;
                piping = false;
            }
        }
    }
    // void PlaceObject(GameObject placedbuilding, GameObject finishedbuilding, Vector3 placedposition, Quaternion rotation, int pipestage, Transform snappedto)
    void PlaceObject(GameObject placedbuilding, Vector3 placedposition, Quaternion rotation, int pipestage, Transform snappedto)
    {
        //Debug.Log(buildingon);
        GameObject blueprintobject;
        Transform snappedfrom = null;
        blueprintobject = Instantiate(placedbuilding, placedposition, rotation);
        Blueprint bpscript = blueprintobject.GetComponent<Blueprint>();
        if(pipestage == 3)
        {
            Destroy(blueprintobject.GetComponent<EntityProperties>());
            Destroy(bpscript);
            bpscript = parentBP;
        }
        //EntityProperties propertyscript = blueprintobject.GetComponentInParent<EntityProperties>();

        foreach (Transform child in blueprintobject.GetComponentsInChildren<Transform>())
        {
            if (pipestage == 0)
            {
                child.gameObject.layer = 8;
                if (child.tag == "Check")
                {
                    Destroy(child.gameObject);
                }
            }
            foreach (BoxCollider snap in child.GetComponents<BoxCollider>())
            {
               /* if (snappedfrom != null && snap.transform == snappedfrom)
                {
                    break;
                }*/
                if (snap.isTrigger && snap.tag == "Snap")
                {
                    bpscript.snappingfrom.Add(child);
                    break;
                }
            }
        }
        if (snappedto != null && (altsnap || bpsnap[activebpsnap] != null) && pipestage != 3)
        {
            //Debug.Log("working");
            if (!altsnap)
            {
                snappedfrom = bpscript.snappingfrom[activebpsnap];
            }
            else
            {
                snappedfrom = bpscript.altsnappingfrom;
            }
            //snappedfrom.gameObject.SetActive(false);
            //snappedto.gameObject.SetActive(false);
            if (!piping)
            {
                if (!altsnap)
                {
                    bpscript.snappedto[activebpsnap] = snappedto;
                }
                else
                {
                    bpscript.altsnappingto = snappedto;
                }
            }
            //Debug.Log(bpscript.snappingfrom.FindIndex(x => x == snappedfrom));
            //bpscript.snappedto[bpscript.snappingfrom.FindIndex(x => x == snappedfrom)] = snappedto;
            Blueprint snappedbp = snappedto.GetComponentInParent<Blueprint>();
            if (snappedbp != null && !altsnap)
            {
                snappedbp.snappedto[snappedbp.snappingfrom.FindIndex(x => x == snappedto)] = snappedfrom.transform;
            }
            else if(snappedbp != null && altsnap)
            {
                snappedbp.altsnappingto = snappedto;
            }
            snappedfrom.gameObject.SetActive(false);
            snappedto.gameObject.SetActive(false);
            savedsnap = snappedto;

        }

        if (pipestage == 1)
        {
            parentpipe = blueprintobject;
            parentBP = bpscript;
            parentBP.disabled = true;

            Destroy(blueprintobject.GetComponent<Rigidbody>());
            
            if(buildingon != null)
            {
                parentpipe.transform.SetParent(buildingon.transform);
            }
            else if(snappedto != null)
            {
                parentpipe.transform.SetParent(snappedto.parent.transform);
            }
            
            if (snappedto != null)
            {
                parentBP.snappedto[0] = snappedto;
            }
            else
            {

            }
        }
        else if(pipestage == 2)
        {
            parentBP.blueprintobject[1] = blueprintobject;
            blueprintobject.transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).position = pipebone.position;
            blueprintobject.transform.SetParent(parentpipe.transform);

            parentBP.blueprintrenderer[1] = blueprintobject.GetComponentInChildren<Renderer>();
            blueprintobject.GetComponentInChildren<Renderer>().material = parentBP.blueprintmat;
            Bounds newbound = new Bounds(new Vector3(pipebone.localPosition.x / 2, pipebone.localPosition.y / 2, pipebone.localPosition.z / 2),
                new Vector3(Mathf.Abs(pipebone.localPosition.x) + 0.0033f, Mathf.Abs(pipebone.localPosition.y) + 0.0033f, Mathf.Abs(pipebone.localPosition.z) + 0.0033f));
            blueprintobject.GetComponentInChildren<SkinnedMeshRenderer>().localBounds = newbound;

            BoxCollider col = blueprintobject.AddComponent<BoxCollider>();
            col.center = new Vector3(0, 0, -pipebone.localPosition.y / 2 * 100f);
            col.size = new Vector3(0.33f *1, 0.33f*1, Mathf.Abs(pipebone.localPosition.y * 100f));
            //Debug.Log(col.size);
            //blueprintobject.layer = 8;
        }
        else if (pipestage == 3)
        {            
            parentBP.blueprintobject[2] = blueprintobject;
            parentBP.disabled = false;
            Rigidbody pipeRB = parentpipe.AddComponent<Rigidbody>();
            Rigidbody copyRB = blueprintobject.GetComponent<Rigidbody>();
            pipeRB.mass = copyRB.mass;
            pipeRB.drag = copyRB.drag;
            pipeRB.angularDrag = copyRB.angularDrag;
            pipeRB.useGravity = copyRB.useGravity;
            pipeRB.isKinematic = copyRB.isKinematic;
            pipeRB.velocity = copyRB.velocity;

            Destroy(copyRB);
            List<AnchorSystem.Object> connectingobjects = new List<AnchorSystem.Object>();
            connectingobjects.Add(parentBP.transform.parent.GetComponentInParent<EntityProperties>().anchordata);
            blueprintobject.transform.SetParent(parentpipe.transform); //parentpipe

            parentBP.blueprintrenderer[2] = blueprintobject.GetComponent<Renderer>();
            blueprintobject.GetComponent<Renderer>().material = parentBP.blueprintmat;

            foreach (Transform child in parentpipe.GetComponentsInChildren<Transform>())
            {
                if (child.tag == "Check")
                {
                    Destroy(child.gameObject);
                }
                child.gameObject.layer = 8;  // add any layer you want. 
            }
            savedsnap = null;
            if (snappedto != null)
            {
                //Debug.Log("stuff");
                bpscript.snappedto[1] = snappedto;
                //snappedto.gameObject.SetActive(false);
                bpscript.snappingfrom[1].gameObject.SetActive(false);
                snappedto.gameObject.SetActive(false);
            }
            bpscript.power = .01f;

            //AnchorSystem.AnchorManager.RemoveObject(parentBP.GetComponent<EntityProperties>().anchordata);
            bpscript.GetComponent<EntityProperties>().teamid = playerproperties.teamid;
            bpscript.GetComponent<EntityProperties>().buildingid = 1;
            bpscript.ispipe = true;

            if (buildingon != null)
            {
                //Debug.Log("connected to something");
                connectingobjects.Add(buildingon.anchordata);
            }
            for (int j = 0; j < bpscript.snappedto.Length; j++) //weld to snapped
            {
                if (bpscript.snappedto[j] != null)
                {
                    connectingobjects.Add(bpscript.snappedto[j].GetComponentInParent<EntityProperties>().anchordata);
                }
            }

            bpscript.Place(connectingobjects);

            //AnchorSystem.AnchorManager.AddConnections(bpscript.GetComponent<EntityProperties>().anchordata, connectingobjects);
            // NetworkClient.localPlayer
            List<Vector3> chunkpositions = new List<Vector3>();
            List<Quaternion> chunkrotations = new List<Quaternion>();
            List<int> networksnapfrom = new List<int>();
            List<int> networksnapto = new List<int>();
            List<int> networksnaptoIDs = new List<int>();
            List<int> anchorobjectIDs = new List<int>();

            // parentBP.blueprintobject[0].transform.position
            //   parentBP.blueprintobject[0].transform.rotation
            chunkpositions.Add(parentBP.blueprintobject[0].transform.localPosition);
            chunkrotations.Add(parentBP.blueprintobject[0].transform.localRotation);
            chunkpositions.Add(parentBP.blueprintobject[1].transform.localPosition);
            chunkrotations.Add(parentBP.blueprintobject[1].transform.localRotation);
            chunkpositions.Add(parentBP.blueprintobject[2].transform.localPosition);
            chunkrotations.Add(parentBP.blueprintobject[2].transform.localRotation);



            for (int n = 0; n < bpscript.snappedto.Length; n++) //weld to snapped
            {
                if (bpscript.snappedto[n] != null)
                {
                    networksnapfrom.Add(n);
                    List<Transform> othersnapto = new List<Transform>();
                    if (bpscript.snappedto[n].GetComponentInParent<Blueprint>())
                    {
                        othersnapto = bpscript.snappedto[n].GetComponentInParent<Blueprint>().snappingfrom;
                    }
                    else
                    {
                        othersnapto = bpscript.snappedto[n].GetComponentInParent<EntityProperties>().snappingfrom;
                    }
                    networksnapto.Add(othersnapto.FindIndex(x => x == bpscript.snappedto[n]));
                    Debug.Log(n + ", " + othersnapto.FindIndex(x => x == bpscript.snappedto[n]));
                    networksnaptoIDs.Add(bpscript.snappedto[n].GetComponentInParent<EntityProperties>().entityID);
                }
            }

            foreach (AnchorSystem.Object anchorobjectid in bpscript.GetComponent<EntityProperties>().anchordata.connectedto)
            {
                anchorobjectIDs.Add(anchorobjectid.objectproperties.entityID);
            }

            GameSystem.GameManager.GenerateID(bpscript.GetComponent<EntityProperties>(), 0, 1);
            Debug.Log("anchor id: " + bpscript.GetComponent<EntityProperties>().anchordata.chunk.anchorID);
            NetworkClient.localPlayer.GetComponent<PlayerNetwork>().CmdPlaceBlueprint(NetworkClient.localPlayer.gameObject, 1, chunkpositions, chunkrotations, 
                bpscript.GetComponent<EntityProperties>().anchordata.chunk.anchorID, networksnapfrom, networksnapto, networksnaptoIDs, anchorobjectIDs, bpscript.GetComponent<EntityProperties>().entityID);

            //          public void RpcPlaceBlueprint(NetworkConnection target, GameObject playerobject, int objecttype, List<Vector3> chunkposition, List<Quaternion> chunkrotation, int chunkID,
            //  List<int> snappedfrom, List<int> snappedto, List<int> snapobjectIDs, List<int> anchorobjectIDs, int entityID)
        }
        else
        {
            savedsnap = null;
            bpscript.GetComponent<EntityProperties>().teamid = playerproperties.teamid;
            bpscript.GetComponent<EntityProperties>().buildingid = buildingselection;

            List<AnchorSystem.Object> connectingobjects = new List<AnchorSystem.Object>();
            if (buildingon != null)
            {
                //Debug.Log("connected to something");
                connectingobjects.Add(buildingon.anchordata);
            }
            for (int j = 0; j < bpscript.snappedto.Length; j++) //weld to snapped
            {
                if (bpscript.snappedto[j] != null)
                {
                    connectingobjects.Add(bpscript.snappedto[j].GetComponentInParent<EntityProperties>().anchordata);
                }
            }
            if (altsnap)
            {
                connectingobjects.Add(bpscript.altsnappingto.GetComponentInParent<EntityProperties>().anchordata);
            }

            bpscript.Place(connectingobjects);
            bpscript.power = .01f;
            bpscript.disabled = false;


            //AnchorSystem.AnchorManager.AddConnections(bpscript.GetComponent<EntityProperties>().anchordata, connectingobjects);

            // NetworkClient.localPlayer
            List<Vector3> chunkpositions = new List<Vector3>();
            List<Quaternion> chunkrotations = new List<Quaternion>();
            List<int> networksnapfrom = new List<int>();
            List<int> networksnapto = new List<int>();
            List<int> networksnaptoIDs = new List<int>();
            List<int> anchorobjectIDs = new List<int>();

            chunkpositions.Add(bpscript.transform.localPosition);
            chunkrotations.Add(bpscript.transform.localRotation);



            for (int n = 0; n < bpscript.snappedto.Length; n++) //weld to snapped
            {
                if (bpscript.snappedto[n] != null)
                {
                    networksnapfrom.Add(n);
                    List<Transform> othersnapto = new List<Transform>();
                    if(snappedto.GetComponentInParent<Blueprint>())
                    {
                        othersnapto = snappedto.GetComponentInParent<Blueprint>().snappingfrom;
                    }
                    else
                    {
                        othersnapto = snappedto.GetComponentInParent<EntityProperties>().snappingfrom;
                    }
                    networksnapto.Add(othersnapto.FindIndex(x => x == snappedto));
                    Debug.Log(n + ", " + othersnapto.FindIndex(x => x == snappedto));
                    networksnaptoIDs.Add(bpscript.snappedto[n].GetComponentInParent<EntityProperties>().entityID);
                }
            }
            if(altsnap)
            {
                networksnapfrom.Add(-1);
                networksnapto.Add(-1);
                networksnaptoIDs.Add(snappedto.GetComponentInParent<EntityProperties>().entityID);
            }

            foreach(AnchorSystem.Object anchorobjectid in bpscript.GetComponent<EntityProperties>().anchordata.connectedto)
            {
                anchorobjectIDs.Add(anchorobjectid.objectproperties.entityID);
            }

            GameSystem.GameManager.GenerateID(bpscript.GetComponent<EntityProperties>(), 0, 1);
            Debug.Log("anchor id: " + bpscript.GetComponent<EntityProperties>().anchordata.chunk.anchorID);
            NetworkClient.localPlayer.GetComponent<PlayerNetwork>().CmdPlaceBlueprint(NetworkClient.localPlayer.gameObject, buildingselection, chunkpositions, chunkrotations, 
                bpscript.GetComponent<EntityProperties>().anchordata.chunk.anchorID, networksnapfrom, networksnapto, networksnaptoIDs, anchorobjectIDs, bpscript.GetComponent<EntityProperties>().entityID);

           // (int chunkID,
       // List<int> snappedfrom, List<int> snappedto, List<int> snapobjectIDs, List<int> anchorobjectIDs, int tempID)
        }
    }



    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        //Check that it is being run in Play Mode, so it doesn't try to draw this in Editor mode
        if (piping)
        {

            foreach (BoxCollider checkhitbox in checkbox)
            {                

                //Gizmos.DrawWireCube(checkhitbox.transform.TransformPoint(checkhitbox.center), checkhitbox.size);
            }
        }
            //Draw a cube where the OverlapBox is (positioned where your GameObject is as well as a size)
            //Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}
