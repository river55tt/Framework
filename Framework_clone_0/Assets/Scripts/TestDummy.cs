using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDummy : MonoBehaviour
{
    //public CharacterController controller;
    public Animator animController;
    public Renderer PlayerRenderer;
    public Renderer BackpackRenderer;
    private WeaponSelection weaponscript;
    private EntityProperties playerproperties;

    //public SkinnedMeshRenderer trenderer;
    Material transmaterial;

    private Rigidbody[] ragdoll;
    public BoxCollider[] boxbody;
    private BoxCollider[] hitboxbody;
    private SphereCollider[] spherebody;
    private CapsuleCollider[] capsulebody;

    public float speed;
    public float topspeed = 15f;
    public float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;
    float targetAngle;
    float targetAngle2;
    public float gravity = -9.81f;
    public float jumppower = 5f;
    public float jetpackpower = 6f;

    public Transform groundCheck;
    public float groundDistance = 0.03f;
    public LayerMask groundMask;

    Vector3 velocity;
    bool Grounded;
    bool Jetpack;
    bool CombatMode;
    bool Alive;
    public bool sprint;
    public bool shooting;




    // Start is called before the first frame update
    void Start()
    {
        /*weaponscript = this.gameObject.GetComponent<WeaponSelection>();
        playerproperties = this.gameObject.GetComponent<EntityProperties>();

        hitboxbody = this.gameObject.GetComponentsInChildren<BoxCollider>();
        spherebody = this.gameObject.GetComponentsInChildren<SphereCollider>();
        capsulebody = this.gameObject.GetComponentsInChildren<CapsuleCollider>();
        ragdoll = this.gameObject.GetComponentsInChildren<Rigidbody>();

        for (int i = 0; i < hitboxbody.Length; i++)
        {
            hitboxbody[i].enabled = true;
        }

        for (int i = 0; i < boxbody.Length; i++)
        {
            boxbody[i].enabled = false;
        }
        for (int i = 0; i < spherebody.Length; i++)
        {
            spherebody[i].enabled = false;
        }
        for (int i = 0; i < capsulebody.Length; i++)
        {
            capsulebody[i].enabled = false;
        }
        for (int i = 0; i < ragdoll.Length; i++)
        {
            ragdoll[i].isKinematic = true;
            ragdoll[i].useGravity = false;
        }
        ragdoll[0].isKinematic = false;
        ragdoll[0].useGravity = true;
        capsulebody[0].enabled = true;*/
    }

    // Update is called once per frame
    void Update()
    {
        /*Grounded = Physics.CheckCapsule(GetComponent<Collider>().bounds.center, new Vector3(GetComponent<Collider>().bounds.center.x, GetComponent<Collider>().bounds.min.y - groundDistance, GetComponent<Collider>().bounds.center.z), 0.15f, groundMask);

        if (playerproperties.dead)
        {
            Dead();
            Alive = false;
        }
        else
        {
            Alive = true;
        }

        if (Alive)
        {
            if (Grounded && velocity.y < 0)
            {
                velocity.y = -1f;
                Jetpack = false;
            }


            Jetpack = false;
            velocity.y += gravity * Time.deltaTime;
        }


        animController.SetFloat("Speed", 0);
        animController.SetFloat("Strafe", 0);
        animController.SetBool("Grounded", Grounded);
        animController.SetInteger("Aim", 0);*/
    }

    void Dead()
    {
        //controller.enabled = false;
        animController.enabled = false;
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
        Destroy(playerproperties);
        //SetKinematic(false);
    }
}
