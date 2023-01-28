using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Artillery : MonoBehaviour
{
    public bool active;
    EntityProperties artyproperties;
    WeaponBehavior artyshot;
    public Transform artyturret;
    public Transform artygun;
    public float turnspeed = 20f;
    //public GameObject gunbarrel;
    // Start is called before the first frame update
    void Start()
    {
        artyproperties = GetComponent<EntityProperties>();
        artyshot = GetComponentInChildren<WeaponBehavior>();
    }

    // Update is called once per frame
    void Update()
    {
        if (active)
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = -Input.GetAxisRaw("Vertical");
            bool fire = Input.GetButtonDown("Fire1");

            if (horizontal != 0 || vertical != 0)
            {
                if (Input.GetButton("Sprint"))
                {
                    horizontal = horizontal / 2f;
                    vertical = vertical / 2f;
                }
                artyturret.Rotate(0, horizontal * turnspeed * Time.deltaTime, 0);
                artygun.Rotate(vertical * turnspeed * Time.deltaTime, 0, 0);
            }

            if (fire && artyproperties.contain.network.power > artyshot.cost)
            {
                //artyshot.Shoot(artygun.GetChild(0).forward);
                artyshot.Shoot(artyshot.transform.position + artyshot.transform.forward*10f);
            }
        }
    }
}
