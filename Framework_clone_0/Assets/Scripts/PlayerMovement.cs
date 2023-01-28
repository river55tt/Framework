using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Drawing;
using Mirror;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody playerRB;
    //public CharacterController controller;
    public Animator animController;
    public Renderer PlayerRenderer;
    public Renderer BackpackRenderer;
    private WeaponSelection weaponscript;
    private EntityProperties playerproperties;
    private BuildSystem buildsystem;
    private PlayerUI playerUI;
    public GameObject prefabLocalPlayer;

    //public SkinnedMeshRenderer trenderer;
    public Material transmaterial;
    public Material opaquematerial;

    public GameObject ragdoll;
    private List<List<int>> ragdollindex;
    public BoxCollider[] boxbody;
    private BoxCollider[] hitboxbody;
    private SphereCollider[] spherebody;
    public CapsuleCollider[] capsulebody;
    private Transform backpack;
    CapsuleCollider capsulecol;

    public float speed;
    public float topspeed = 15f;
    public float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;
    float targetAngle;
    float targetAngle2;
    public float gravity = -9.81f;
    public float jumppower = 5f;
    public float jetpackpower = 6f;

    Transform groundCheck;
    public float groundDistance = 0.03f;
    public LayerMask groundMask;

    Vector3 velocity;
    bool Grounded;
    bool Jetpack;
    bool CombatMode;
    bool Alive;
    public bool sprint;
    public bool shooting;
    public bool buildrotate;
    public GameObject handfab;

    List<Transform> seatcollision = new List<Transform>();
    List<float> seatdelay = new List<float>();
    GameObject vehicleobject;
    Transform seatobject;
    int vehicleid;

    [SerializeField]
    public Transform cam;
    public Transform Target;
    public Transform reticle;
    public Transform cursor;
    float mouseX, mouseY;
    float savedmouseX, savedmouseY;
    bool rotatingcam;
    float targetzoom;
    float zoom;
    float zoomvelocity;
    public float sensitivity = 1f;

    public float setalpha;
    private byte alp;

    [DllImport("user32.dll")]
    public static extern bool SetCursorPos(int X, int Y);
    [DllImport("user32.dll")]
    public static extern bool GetCursorPos(out Point pos);
    Point cursorPos = new Point();
    Point savedcursorPos = new Point();

    // Start is called before the first frame update
    void Start()
    {
        playerproperties = GetComponent<EntityProperties>();
        if (GetComponentInParent<PlayerNetwork>().isLocalPlayer)
        {           
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            GetCursorPos(out cursorPos);
            //transmaterial = PlayerRenderer.material;           
            buildsystem = GetComponent<BuildSystem>();
            playerUI = PlayerUI.LocalPlayerUI;
            playerUI.maxhealth = playerproperties.maxhealth;
            playerUI.maxpower = playerproperties.maxpower;
            playerUI.playerMovement = this;
            playerUI.playerStats = playerproperties;
            cursor = playerUI.cursor;
            reticle = playerUI.reticle;
        }

        weaponscript = GetComponent<WeaponSelection>();
        playerRB = GetComponent<Rigidbody>();
        hitboxbody = this.gameObject.GetComponentsInChildren<BoxCollider>();
        spherebody = this.gameObject.GetComponentsInChildren<SphereCollider>();
        capsulebody = this.gameObject.GetComponentsInChildren<CapsuleCollider>();
        //ragdoll = this.gameObject.GetComponentsInChildren<Rigidbody>();

        ragdollindex = new List<List<int>>();
        foreach (Rigidbody ragdollRB in ragdoll.GetComponentsInChildren<Rigidbody>())
        {
            Transform RBtransform = ragdollRB.transform;
            List<int> tempindex = new List<int>();
            if (ragdollRB != null)
            {
                while (!RBtransform.GetComponent<TestDummy>() && RBtransform.parent)
                {
                    tempindex.Add(RBtransform.GetSiblingIndex());
                    RBtransform = RBtransform.parent;
                }
            }
            tempindex.Reverse();
            /*string indexstring = "";
            for (int n = 0; n < tempindex.Count; n++)
            {
                indexstring = indexstring + tempindex[n].ToString();
                //Debug.Log(indexstring + " " + tempindex[n].ToString());
            }*/
            //Debug.Log(ragdollRB.name);
            ragdollindex.Add(tempindex);
        }


        //ragdoll[0] = this.gameObject.GetComponent<Rigidbody>();
        capsulecol = transform.GetComponent<CapsuleCollider>();
        groundCheck = this.transform;
        for (int i = 0; i < hitboxbody.Length; i++)
        {
            hitboxbody[i].enabled = true;
        }

        for (int i = 0; i < boxbody.Length; i++)
        {
            //boxbody[i].enabled = false;
        }
        for (int i = 0;i < spherebody.Length; i++)
        {
            spherebody[i].enabled = false;
        }
        for (int i = 0; i < capsulebody.Length; i++)
        {
            capsulebody[i].enabled = false;
        }
       /* for (int i = 0; i < ragdoll.Length; i++)
        {
            ragdoll[i].isKinematic = true;
            ragdoll[i].useGravity = false;
        }
        ragdoll[0].isKinematic = false;
        ragdoll[0].useGravity = true;*/
        capsulebody[0].enabled = true;


        foreach (Transform alltransforms in GetComponentsInChildren<Transform>())
        {
            if (alltransforms.tag == "BeamPoint")
            {
                backpack = alltransforms;
                break;
            }
        }
        if(backpack == null)
        {
            backpack = playerproperties.backpack;
        }

        if(GetComponentInParent<PlayerNetwork>().isLocalPlayer)
        {
            //GameObject temp = Instantiate(prefabLocalPlayer);
            //transform.SetParent(temp.transform); //set parent, set up UI
        }

        LineRenderer handfabbeam = handfab.GetComponentInChildren<LineRenderer>();
        handfabbeam.startWidth = .05f;
        handfabbeam.endWidth = .2f;
        handfabbeam.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (playerproperties.dead && Alive == true)
        {
            Dead();
            Alive = false;
        }
        else if (!playerproperties.dead)
        {
            Alive = true;
        }

        if (GetComponentInParent<PlayerNetwork>().isLocalPlayer)
        {
            int mask = (1 << 7) | (1 << 8) | (1 << 9) | (1 << 12);
            int groundmask2 = (1 << 6) | (1 << 10);
            //CapsuleCollider capsulecol = GetComponent<CapsuleCollider>();       

            playerproperties.power += 8f * Time.deltaTime;
            if (playerproperties.power > playerproperties.maxpower)
            {
                playerproperties.power = playerproperties.maxpower;
            }

            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            sprint = Input.GetButton("Sprint");
            bool jump = Input.GetButton("Jump");
            bool jumpdown = Input.GetButtonDown("Jump");
            bool combatswitch = Input.GetButtonDown("Combat");
            bool RMouse = Input.GetMouseButton(1);
            bool RMousedown = Input.GetMouseButtonDown(1);

            bool networkballtest = Input.GetMouseButtonDown(2);
            if(networkballtest && GameSystem.GameManager.isServer)
            {
                /*GameObject ball = Instantiate(GameSystem.GameManager.testball, transform.position + transform.up * 5f, Quaternion.identity);
                NetworkServer.Spawn(ball, NetworkClient.localPlayer.connectionToClient);
                */

                SaveSystem.SaveManager.SaveData();

                Debug.Log("Merging Meshes!");
                foreach (AnchorSystem.Chunk mergechunk in AnchorSystem.AnchorManager.allchunks)
                {
                    //GameSystem.GameManager.UpdateChunkMesh(mergechunk.chunkobject);




                   /* Transform meshcombine = mergechunk.chunkobject;
                    Destroy(meshcombine.GetComponent<MeshFilter>().mesh);
                    meshcombine.GetComponent<MeshFilter>().mesh = new Mesh();

                    MeshFilter[] meshFilters = meshcombine.GetComponentsInChildren<MeshFilter>();
                    CombineInstance[] combine = new CombineInstance[meshFilters.Length];

                    //Debug.Log(meshFilters.Length);

                    int i = 1;
                    while (i < meshFilters.Length)
                    {
                        if(meshFilters[i].GetComponent<EntityProperties>() && meshFilters[i].GetComponent<EntityProperties>().damagealpha != 0)
                        {
                            meshFilters[i].gameObject.GetComponent<MeshRenderer>().enabled = true;
                            i++;
                            continue;
                        }
                        combine[i].mesh = meshFilters[i].sharedMesh;
                        combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
                        meshFilters[i].gameObject.GetComponent<MeshRenderer>().enabled = false;
                        i++;
                    }

                   // meshcombine.GetComponent<MeshRenderer>().material = meshFilters[1].GetComponent<MeshRenderer>().material;
                    //meshcombine.GetComponent<MeshFilter>().mesh = new Mesh();
                    meshcombine.GetComponent<MeshFilter>().mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                    meshcombine.GetComponent<MeshFilter>().mesh.CombineMeshes(combine, true);*/
                }
            }
            if (!buildrotate)
            {
                targetzoom -= Input.GetAxis("Mouse ScrollWheel");
            }

            if (Alive)
            {
                velocity = Vector3.zero;
                if (vehicleid != 0)
                {
                    //Debug.Log(transform.position);
                    //Debug.Log(playerproperties.anchordata.connectedto[0].objectproperties.transform.GetChild(2).transform.position);
                    if (vehicleobject != null)
                    {
                        transform.rotation = seatobject.rotation;
                        transform.position = seatobject.position + transform.up * 1.2f;
                        playerRB.velocity = seatobject.GetComponentInParent<Rigidbody>().velocity;
                        playerRB.angularVelocity = seatobject.GetComponentInParent<Rigidbody>().angularVelocity;
                    }

                    if (jumpdown || vehicleobject == null)
                    {
                        if (vehicleid == 2)
                        {
                            vehicleobject.GetComponent<Artillery>().active = false;
                        }
                        vehicleid = 0;
                        Vector3 tempvelocity = Vector3.zero;
                        if (vehicleobject != null)
                        {
                            tempvelocity = vehicleobject.GetComponentInParent<Rigidbody>().velocity;
                        }
                        vehicleobject = null;
                        //Vector3 tempvelocity = vehicleobject.GetComponentInParent<Rigidbody>().velocity;
                        //AnchorSystem.AnchorManager.RemoveObject(playerproperties.anchordata);

                        //this.transform.SetParent(null);
                        seatcollision.Add(seatobject);
                        seatdelay.Add(Time.time + 1f);

                        //transform.rotation.z = 0;
                        //playerRB = gameObject.GetComponent<Rigidbody>();
                        //playerRB = gameObject.AddComponent<Rigidbody>();
                        //playerRB.rotation = Quaternion.FromToRotation(playerRB.transform.rotation.eulerAngles, transform.up);
                        //Quaternion upquat = new Quaternion(0, 1, 0, 1);
                        //playerRB.rotation = Quaternion.Dot(playerRB.rotation, upquat);
                        //Debug.Log(playerRB.rotation.eulerAngles);

                        //IEnumerator coroutine = RotateTest(playerRB.rotation.eulerAngles);
                        //StartCoroutine(coroutine);

                        //playerRB.rotation = new Quaternion(0, playerRB.rotation.y, 0, playerRB.rotation.w).normalized;
                        //playerRB.isKinematic = false
                        //transform.rotation = Quaternion.FromToRotation(playerRB.transform.rotation.eulerAngles, transform.up);
                        transform.rotation = Quaternion.identity;
                        playerRB.velocity = tempvelocity;
                        //playerRB.mass = 10;
                        //playerRB.angularDrag = 0;
                        //playerRB.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
                    }
                    else
                    {
                        Grounded = true;
                    }
                }
                else
                {
                    Grounded = Physics.CheckCapsule(new Vector3(capsulecol.bounds.center.x, capsulecol.bounds.min.y + capsulecol.radius, capsulecol.bounds.center.z),
                new Vector3(capsulecol.bounds.center.x, capsulecol.bounds.min.y - groundDistance, capsulecol.bounds.center.z),
                capsulecol.radius - .02f, groundmask2, QueryTriggerInteraction.Ignore);
                    velocity = playerRB.velocity;
                }
                //velocity = playerRB.velocity;
                if (Grounded && velocity.y < 0)
                {
                    //velocity.y = -1f;
                    Jetpack = false;
                }

                if (shooting)
                {
                    sprint = false;
                    weaponscript.sprinting = sprint;
                }
                weaponscript.sprinting = sprint;

                if (Grounded && jumpdown) //grounded + jump code
                {
                    Grounded = false;
                    Jetpack = false;
                    velocity.y = jumppower;
                }
                else if (!Grounded && playerproperties.power > 0 && (jumpdown || (jump && Jetpack))) //jetpack
                {
                    Jetpack = true;
                    //playerproperties.fuel
                    float partialuse;
                    if (playerproperties.power - 15f * Time.deltaTime > 0)
                    {
                        playerproperties.power -= 15f * Time.deltaTime;
                        partialuse = Time.deltaTime;
                    }
                    else
                    {
                        partialuse = playerproperties.power / 15f;
                        //velocity.y -= gravity * (partialuse);
                        playerproperties.power = 0f;
                    }

                    if (velocity.y < 0)
                    {
                        velocity.y += (jetpackpower * 3f + 9.81f) * partialuse;
                    }
                    else if (velocity.y < jetpackpower)
                    {
                        velocity.y += (jetpackpower + 9.81f) * partialuse;
                    }
                    else
                    {
                        velocity.y += (jetpackpower * .5f + 9.81f) * partialuse;
                    }
                }
                else
                {
                    Jetpack = false;
                    //velocity.y += gravity * Time.deltaTime;
                }
                //controller.Move(velocity * Time.deltaTime);


                bool interactdown = Input.GetButtonDown("Interact");
                if (interactdown)
                {
                    Ray ray = Camera.main.ScreenPointToRay(cursor.position);
                    RaycastHit hit2;
                    //LayerMask mask2 = LayerMask.GetMask("Blueprint");
                    int mask3 = (1 << 10);
                    if (Physics.Raycast(ray, out hit2, 500f, mask3, QueryTriggerInteraction.Collide))
                    {
                        if (hit2.collider.GetComponent<Button>() != null)
                        {
                            Debug.Log("button");
                            hit2.collider.GetComponent<Button>().interact(playerproperties.entityID);
                            NetworkClient.localPlayer.GetComponent<PlayerNetwork>().CmdButton(hit2.collider.GetComponentInParent<EntityProperties>().entityID, playerproperties.entityID, hit2.collider.GetComponent<Button>().buttonid);
                        }
                    }
                }
            }
            animController.SetBool("Grounded", Grounded);


            targetzoom = Mathf.Clamp(targetzoom, 0f, 1f);
            zoom = Mathf.SmoothDamp(zoom, targetzoom, ref zoomvelocity, .1f); //CAMERA
            /*RaycastHit hit;
            //int mask = (1 << 7) | (1 << 8) | (1 << 9);
            //LayerMask mask = LayerMask.GetMask("Player");
            Target.localPosition = new Vector3(0, .326f, 0);
            Vector3 raypos = Target.position;
            Target.localPosition = new Vector3(0, -1.3f + .626f + 1f * Mathf.Pow(2.2f, zoom), 0);
            cam.localPosition = new Vector3(0, 0, 9.99f - 10f * Mathf.Pow(2f, zoom));

            //Vector3 raypos = groundCheck.position + new Vector3(0, -.3f, 0);
            //raypos = groundCheck.position + new Vector3(0, .326f, 0);
            //raypos = Target.position;
            if (Physics.SphereCast(raypos, .1f, cam.position - raypos, out hit, Vector3.Distance(cam.position, raypos), ~mask, QueryTriggerInteraction.Ignore))
            {
                //Target.localPosition = new Vector3(0, -1.3f + .626f + 1f, 0);
                cam.localPosition = new Vector3(0, 0, -hit.distance + .0001f);
                cam.position = hit.point;
            }*/


           /* if (Physics.SphereCast(groundCheck.position + new Vector3(0, -.3f, 0), .1f, groundCheck.up, out hit, -1.3f + .626f + 1f * Mathf.Pow(2.2f, zoom) + .3f, ~mask, QueryTriggerInteraction.Ignore))
            {
                Target.localPosition = new Vector3(0, hit.distance - .3f, 0);
            }
            else
            {
                Target.localPosition = new Vector3(0, -1.3f + .626f + 1f * Mathf.Pow(2.2f, zoom), 0);
            }
            if (Physics.SphereCast(Target.position, .05f, -Target.forward, out hit, -9.99f + 10f * Mathf.Pow(2f, zoom), ~mask, QueryTriggerInteraction.Ignore))
            {
                //Debug.Log("hit");
                cam.localPosition = new Vector3(0, 0, -hit.distance + .0001f);
            }
            else
            {
                cam.localPosition = new Vector3(0, 0, 9.99f - 10f * Mathf.Pow(2f, zoom));
            }*/

            if (zoom <= .15f)  //player transparency
            {
                PlayerRenderer.enabled = true;
                PlayerRenderer.material = transmaterial;
                //transmaterial.SetFloat("_Mode", 2);                              //  0 = Opaque   1 = Cutout   2 = Fade   3 = Transparent
                //Material.
                transmaterial.SetFloat("_Mode", 0);
                transmaterial.SetOverrideTag("RenderType", "Transparent"); //"Fade"
                /*Color32 col = PlayerRenderer.material.GetColor("_Color");
                alp = (byte)(zoom / .2f * 255f);
                alp = (byte)0;
                col.a = alp;
                transmaterial.SetColor("_Color", col);*/
                transmaterial.color = new UnityEngine.Color(transmaterial.color.r, transmaterial.color.g, transmaterial.color.b, zoom / .2f);

                /*transmaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha); //);  (int)UnityEngine.Rendering.BlendMode.One);
                transmaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                transmaterial.SetInt("_ZWrite", 0);
                transmaterial.DisableKeyword("_ALPHATEST_ON");
                transmaterial.DisableKeyword("_ALPHABLEND_ON");
                transmaterial.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                transmaterial.renderQueue = 3000;*/

                transmaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                transmaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                transmaterial.SetInt("_ZWrite", 0);
                transmaterial.DisableKeyword("_ALPHATEST_ON");
                transmaterial.EnableKeyword("_ALPHABLEND_ON");
                transmaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                transmaterial.renderQueue = 3000;
            }
            else //opaque
            {
                //if (Physics.SphereCast(cam.transform.position, .1f, cam.transform.forward, out hit, 100f, mask))
                if (!true)
                {
                    PlayerRenderer.enabled = true;
                    transmaterial.SetFloat("_Mode", 3);
                    Color32 col = PlayerRenderer.material.GetColor("_Color");
                    alp = (byte)(.5f * 255f);
                    col.a = alp;
                    transmaterial.SetColor("_Color", col);

                    transmaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    transmaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    transmaterial.SetInt("_ZWrite", 0);
                    transmaterial.DisableKeyword("_ALPHATEST_ON");
                    transmaterial.DisableKeyword("_ALPHABLEND_ON");
                    transmaterial.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                    transmaterial.renderQueue = 3000;
                }
                else
                {
                    PlayerRenderer.enabled = true;
                    transmaterial.SetFloat("_Mode", 0);
                    transmaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    transmaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    transmaterial.SetInt("_ZWrite", 1);
                    transmaterial.DisableKeyword("_ALPHATEST_ON");
                    transmaterial.DisableKeyword("_ALPHABLEND_ON");
                    transmaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    transmaterial.renderQueue = -1;

                    PlayerRenderer.material = opaquematerial;
                }
            }
            BackpackRenderer.material = PlayerRenderer.material;


            if (combatswitch) //combatswitch code
            {
                CombatMode = !CombatMode;
                reticle.gameObject.SetActive(!reticle.gameObject.activeSelf);
                cursor.gameObject.SetActive(!reticle.gameObject.activeSelf);
                reticle.position = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);
                cursor.position = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);
                //    SetCursorPos(Screen.width / 2 , Screen.height / 2);
            }
            weaponscript.combatmode = CombatMode;
            buildsystem.combatmode = CombatMode;
            if (CombatMode && !RMouse)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                if (RMousedown)
                {
                    GetCursorPos(out savedcursorPos);
                    rotatingcam = true;
                    cursor.position = Input.mousePosition;
                }
                if (RMouse)
                {
                    //CamControl();
                }
            }

            if (Alive && vehicleid == 0) //player movement
            {
                Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;  //move code
                float clampspeed = new Vector3(playerRB.velocity.x, 0f, playerRB.velocity.z).magnitude;
                //speed = new Vector3(playerRB.velocity.x, 0f, playerRB.velocity.z).magnitude;

                /*if (clampspeed > topspeed)
                {
                    speed = clampspeed - (clampspeed - topspeed) * 2f * Time.deltaTime;
                }*/

                Vector3 moveDir = Vector3.zero;
                if (sprint)
                {
                    topspeed = 6;
                }
                else
                {
                    topspeed = 3;
                }

                speed = new Vector3(velocity.x, 0f, velocity.z).magnitude; //before input

                if (direction.magnitude >= 0.1f)
                {
                    targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
                    if (!CombatMode || sprint)
                    {
                        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
                        transform.rotation = Quaternion.Euler(0f, angle, 0f);
                    }

                    moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                    moveDir = moveDir.normalized * 20 * Time.deltaTime;
                    if (!Grounded)
                    {
                        moveDir /= 1.3f;
                    }
                    if (sprint && Grounded)
                    {
                        moveDir *= 2f;
                    }
                }
                else
                {
                    moveDir = new Vector3(velocity.x, 0, velocity.z);
                    if (Grounded)
                    {
                        if (speed >= 40 * Time.deltaTime)
                        {
                            moveDir = moveDir.normalized * -40f * Time.deltaTime;
                        }
                        else
                        {
                            moveDir = Vector3.zero;
                            speed = 0;
                            velocity.x = 0;
                            velocity.z = 0;
                        }
                    }
                    else
                    {
                        if (speed >= 10 * Time.deltaTime)
                        {
                            moveDir = moveDir.normalized * -10f * Time.deltaTime;
                        }
                        else
                        {
                            moveDir = Vector3.zero;
                            speed = 0;
                            velocity.x = 0;
                            velocity.z = 0;
                        }
                    }
                }
                velocity.x += moveDir.x;
                velocity.z += moveDir.z;

                speed = new Vector3(velocity.x, 0f, velocity.z).magnitude; //after input

                if (speed > 0)
                {
                    velocity.x = velocity.x / speed;
                    velocity.z = velocity.z / speed;
                }

                if (speed > topspeed) //if too fast, slow down
                {
                    if (Grounded)
                    {
                        speed -= (speed - topspeed) * 15f * Time.deltaTime;
                    }
                    else
                    {
                        speed -= (speed - topspeed) * 4f * Time.deltaTime;
                    }

                    if (speed < topspeed)
                    {
                        speed = topspeed;
                    }
                }

                velocity.x *= speed; //normalized
                velocity.z *= speed;



                /* if (speed > 0 || direction.magnitude >= 0.1f)
                 {
                     if (direction.magnitude >= 0.1f)
                     {
                         if (sprint)
                         {
                             turnSmoothTime = .07f;
                             if (speed < topspeed)
                             {
                                 speed += 30f * Time.deltaTime;
                             }
                         }
                         else
                         {
                             turnSmoothTime = .15f;
                             if (speed < 4f && speed < topspeed)
                             {
                                 if (speed + 15f * Time.deltaTime < 4f)
                                 {
                                     speed += 15f * Time.deltaTime;
                                 }
                                 else
                                 {
                                     speed = 4f;
                                 }
                             }
                             else if (speed > 4f)
                             {
                                 if (speed - 25f * Time.deltaTime > 4f)
                                 {
                                     speed -= 25f * Time.deltaTime;
                                 }
                                 else
                                 {
                                     speed = 4f;
                                 }
                             }
                         }

                         targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
                         if (!CombatMode || sprint)
                         {
                             float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
                             transform.rotation = Quaternion.Euler(0f, angle, 0f);
                         }
                     }
                     else
                     {
                         speed -= 25f * Time.deltaTime;
                     }
                     moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                     moveDir = moveDir.normalized * speed;
                     velocity.x = moveDir.x;
                     velocity.z = moveDir.z;
                     //controller.Move(moveDir.normalized * speed * Time.deltaTime);
                 }
                 else
                 {
                     moveDir = new Vector3(velocity.x, 0, velocity.z) ;
                     moveDir = moveDir.normalized * speed;
                     velocity.x = moveDir.x;
                     velocity.z = moveDir.z;
                     if (speed - 40f * Time.deltaTime > 0f)
                     {
                         speed -= 40f * Time.deltaTime;
                     }
                     else
                     {
                         speed = 0f;
                     }
                 }*/


                if (CombatMode && !sprint) //combat switch aim
                {
                    float angle2 = Mathf.SmoothDampAngle(transform.eulerAngles.y, cam.eulerAngles.y, ref turnSmoothVelocity, turnSmoothTime);
                    transform.rotation = Quaternion.Euler(0f, angle2, 0f);
                    //animController.SetFloat("Strafe", speed * direction.x / 8f);   STRAFE LATER
                    /*if (vertical < 0)
                    {
                        animController.SetFloat("Speed", -speed * Mathf.Abs(direction.z) / 8f);
                    }
                    else
                    {
                        animController.SetFloat("Speed", speed * Mathf.Abs(direction.z) / 8f);
                    }*/
                    animController.SetFloat("Speed", speed / 8f);
                }
                else
                {
                    animController.SetFloat("Speed", speed / 8f);
                    animController.SetFloat("Strafe", 0f);
                }
                playerRB.velocity = velocity;
            }
            else if (Alive && vehicleid == 2) //artillery
            {

            }
            playerUI.SetStats(playerproperties.health, playerproperties.power);

            //Debug.Log(playerproperties.localbeampoints.Count);
            if (playerproperties.localbeampoints.Count > 0)
            {
                Vector3 tempposition =
                playerproperties.localbeampoints[0] = transform.InverseTransformPoint(backpack.position);
            }

            if (seatcollision.Count > 0 && Time.time > seatdelay[0])
            {
                seatcollision.RemoveAt(0);
                seatdelay.RemoveAt(0);
            }

            CamControl();
            int weaponselect = weaponscript.aim;

            Vector3 fabbeam = Vector3.zero;
            if (buildsystem.handfabbeam)
            {
                fabbeam = buildsystem.handfabbeam.GetPosition(1);
            }
            if(!CombatMode)
            {
                weaponselect = -1;
            }
            /*if(!CombatMode)
            {
                weaponselect = -1;
            }
            else
            {
                weaponselect = weaponscript.aim;
            } */
            GetComponentInParent<PlayerNetwork>().CmdPlayerAnimations(Grounded, speed, weaponselect, fabbeam);
        }
        else
        {
            PlayerNetwork thisplayer = GetComponentInParent<PlayerNetwork>();
            animController.SetBool("Grounded", thisplayer.playergrounded);
            animController.SetFloat("Speed", thisplayer.playerspeed / 8f);
            animController.SetInteger("Aim", thisplayer.playerselect);
            for (int i = 0; i < weaponscript.Weapons.Length; i++)
            {
                weaponscript.Weapons[i].SetActive(false);
            }
            if(thisplayer.playerselect > 0)
            {
                weaponscript.Weapons[thisplayer.playerselect - 1].SetActive(true);
            }
            
            if (thisplayer.playerselect == -1)
            {
                handfab.SetActive(true);
            }
            else
            {
                handfab.SetActive(false);
            }

            if(thisplayer.playerbeam != Vector3.zero)
            {
                Vector3[] beampositions = new Vector3[2];
                beampositions[0] = handfab.transform.position;
                beampositions[1] = thisplayer.playerbeam;
                handfab.GetComponentInChildren<LineRenderer>().SetPositions(beampositions);
                handfab.GetComponentInChildren<LineRenderer>().enabled = true;
            }
            else
            {
                handfab.GetComponentInChildren<LineRenderer>().enabled = false;
            }
        }
    }

    void LateUpdate()
    {
        //CamControl();
    }
    void CamControl()
    {
        bool RMouse = Input.GetMouseButton(1);
        bool RMousedown = Input.GetMouseButtonDown(1);
        if (CombatMode || RMouse)
        {
            mouseX += Input.GetAxis("Mouse X") * sensitivity;
            mouseY -= Input.GetAxis("Mouse Y") * sensitivity;
            mouseY = Mathf.Clamp(mouseY, -80, 80);
            if (RMousedown)
            {
                GetCursorPos(out savedcursorPos);
                rotatingcam = true;
            }
        }
        else
        {
            //cursor.transform.position = cursor.transform.position + new Vector3(Input.GetAxisRaw("Mouse X") * sensitivity * 4, Input.GetAxisRaw("Mouse Y") * sensitivity * 4, 0);
            if (rotatingcam)
            {
                SetCursorPos(savedcursorPos.X, savedcursorPos.Y);
                rotatingcam = false;
            }
            else
            {
                cursor.position = Input.mousePosition;
            }
        }

        Target.rotation = Quaternion.Euler(mouseY, mouseX, 0);

       // cam.LookAt(Target);
        //Physics.SyncTransforms();

        RaycastHit hit;
        RaycastHit hit2;
        int mask = (1 << 7) | (1 << 8) | (1 << 9) | (1 << 12);
        //LayerMask mask = LayerMask.GetMask("Player");
        Target.localPosition = new Vector3(0, .324f, 0);
        Vector3 raypos = Target.position;
        Target.localPosition = new Vector3(0, -1.3f + .626f + 1f * Mathf.Pow(2.2f, zoom), 0);
        cam.localPosition = new Vector3(0, 0, 9.99f - 10f * Mathf.Pow(2f, zoom));

        cam.LookAt(Target);

        Quaternion camrot = cam.rotation;
        Vector3 campos = cam.position;
        //Vector3 raypos = groundCheck.position + new Vector3(0, -.3f, 0);
        //raypos = groundCheck.position + new Vector3(0, .326f, 0);
        //raypos = Target.position;
        if(Physics.SphereCast(raypos, .03f, cam.position - raypos, out hit, Vector3.Distance(cam.position, raypos), ~mask, QueryTriggerInteraction.Ignore))
        {
            if (hit.distance > 1f)
            {
                if (Physics.SphereCast(raypos + (cam.position - raypos).normalized * hit.distance / 4, .13f, cam.position - raypos, out hit2, 3 * hit.distance / 4, ~mask, QueryTriggerInteraction.Ignore))
                {
                    cam.position = raypos + (cam.position - raypos).normalized * (hit.distance / 4 + hit2.distance);
                }
                else
                {
                    cam.position = raypos + (cam.position - raypos).normalized * hit.distance;
                }
            }
            else if(Physics.SphereCast(raypos, .1f, cam.position - raypos, out hit2, hit.distance, ~mask, QueryTriggerInteraction.Ignore))
            {
                cam.position = raypos + (cam.position - raypos).normalized * (hit.distance);
            }
        }

        Physics.SyncTransforms();
    }

    void Dead()
    {
        //controller.enabled = false;

        /*
                ragdoll[0].isKinematic = true;
                ragdoll[0].useGravity = false;

                for (int i = 0; i < hitboxbody.Length; i++)
                {
                    hitboxbody[i].enabled = false;
                }

                for (int i = 0; i < boxbody.Length; i++)
                {
                    boxbody[i].enabled = true;
                }
                for (int i = 0; i < spherebody.Length; i++)
                {
                    spherebody[i].enabled = true;
                }
                for (int i = 0; i < capsulebody.Length; i++)
                {
                    capsulebody[i].enabled = true;
                }
                capsulebody[0].enabled = false;
                for (int i = 0; i < ragdoll.Length; i++)
                {
                    ragdoll[i].isKinematic = false;
                    ragdoll[i].useGravity = true;
                }
                //SetKinematic(false);*/

        transform.GetChild(0).gameObject.SetActive(false);

        ragdoll.gameObject.SetActive(true);
        Rigidbody[] ragdollRB = ragdoll.GetComponentsInChildren<Rigidbody>();
        ragdoll.transform.position = transform.position;
        ragdoll.transform.rotation = transform.rotation;
        for (int i = 0; i < ragdollindex.Count; i++)
        {
            //Debug.Log(ragdollRB[i].name);
            Transform part = transform;
            for (int j = 0; j < ragdollindex[i].Count; j++)
            {
                //Debug.Log(part.name + ", " + ragdollindex[i][j]);
                part = part.GetChild(ragdollindex[i][j]);
            }
            //Debug.Log(part.name + ": " + part.position + " " + part.rotation);
            ragdollRB[i].transform.position = part.position;
            ragdollRB[i].transform.rotation = part.rotation;
            ragdollRB[i].velocity = playerRB.velocity;
        }
        capsulebody[0].enabled = false;
        playerRB.isKinematic = true;
        animController.enabled = false;
        //ragdoll.gameObject.SetActive(true);
        if(GetComponentInParent<PlayerNetwork>().isLocalPlayer)
        {
            Target.SetParent(ragdoll.transform.transform.GetChild(0));
            SpawnSystem.SpawnManager.respawning = true;
            SpawnSystem.SpawnManager.player = playerproperties;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (vehicleid == 0 && other.tag == "Ladder")
        {
            playerRB.velocity = new Vector3(playerRB.velocity.x, 8, playerRB.velocity.z);
        }
    }

    void OnCollisionEnter(Collision other)
    {
        //Debug.Log("collision");
        Transform colliderobject = other.collider.transform;
        //Debug.Log(colliderobject.name);
        if (vehicleid == 0 && colliderobject.tag == "Seat")
        {
            Debug.Log("touched seat");
            if (seatcollision.Count > 0)
            {
                Debug.Log(seatdelay[0] - Time.time + " left");
            }
            EntityProperties seatproperties = colliderobject.GetComponentInParent<EntityProperties>();
            if (seatcollision.Count > 0 && seatcollision.Contains(seatproperties.transform))
            {
                Debug.Log("return");
                return;
            }
            else
            {
                vehicleobject = colliderobject.GetComponentInParent<EntityProperties>().gameObject;
                seatobject = colliderobject;
                //AnchorSystem.AnchorManager.CreateObject(playerproperties);
                //List<AnchorSystem.Object> connectingobjects = new List<AnchorSystem.Object>();
                //connectingobjects.Add(colliderobject.GetComponentInParent<EntityProperties>().anchordata);
                //AnchorSystem.AnchorManager.AddConnections(playerproperties.anchordata, connectingobjects);

                transform.rotation = colliderobject.rotation;
                transform.position = colliderobject.position + transform.up * 1.2f;

                //Destroy(playerRB);
                //playerRB = null;
                if (colliderobject.GetComponentInParent<Artillery>())
                {
                    vehicleid = 2; //artillery
                    colliderobject.GetComponentInParent<Artillery>().active = true;
                }
                else if (colliderobject.GetComponentInParent<Corrosion>())
                {
                    vehicleid = 3; //barge
                }
                else
                {
                    vehicleid = 1; //seat
                }
                //Debug.Log(transform.position - colliderobject.position);
            }
            //Destroy(playerRB);




        }
    }

    IEnumerator RotateTest(Vector3 rotationtest)
    {
        transform.position += new Vector3(0, 2, 0);
        transform.rotation = Quaternion.identity;
        playerRB.isKinematic = true;
        yield return new WaitForSeconds(2);
        playerRB.transform.rotation = Quaternion.Euler(0, 0, rotationtest.z) * playerRB.transform.rotation;
        yield return new WaitForSeconds(1);
        playerRB.transform.rotation = Quaternion.Euler(rotationtest.x, 0, 0) * playerRB.transform.rotation;
        yield return new WaitForSeconds(1);
        playerRB.transform.rotation = Quaternion.Euler(0, rotationtest.y, 0) * playerRB.transform.rotation;
        yield return new WaitForSeconds(2);
        playerRB.transform.rotation = Quaternion.Euler(0, -rotationtest.y, 0) * playerRB.transform.rotation;
        yield return new WaitForSeconds(1);
        playerRB.transform.rotation = Quaternion.Euler(-rotationtest.x, 0, 0) * playerRB.transform.rotation;
        yield return new WaitForSeconds(1);
        playerRB.transform.rotation = Quaternion.Euler(0, 0, -rotationtest.z) * playerRB.transform.rotation;
        yield return new WaitForSeconds(2);
        transform.rotation = Quaternion.identity;
        playerRB.isKinematic = false;
    }
}