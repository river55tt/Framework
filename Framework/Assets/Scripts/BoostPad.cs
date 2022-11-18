using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostPad : MonoBehaviour
{
    EntityProperties boostproperties;
    public Collider boostcollider;
    float nextboost;
    public float booststrength = 15f;
    float boostdelay = .5f;
    // Start is called before the first frame update
    void Start()
    {
        boostproperties = GetComponentInParent<EntityProperties>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision other)
    {
        //Debug.Log("collision");
        //if (other.GetContact(0).otherCollider)
           // Debug.Log(other.GetContact(0).thisCollider.gameObject.name);
        /*if(other.GetContact(0).thisCollider == boostcollider)
        {

        }*/
        if (Time.time >= nextboost && other.GetContact(0).thisCollider == boostcollider)// && boostproperties.contain.network.power > 20f)
        {
            Transform colliderobject = other.collider.transform;
            EntityProperties colliderproperties = colliderobject.GetComponentInParent<EntityProperties>();
            //Debug.Log(colliderobject.name);
            if (colliderproperties != null)
            {
                if (colliderproperties.entitytype == 3 && colliderproperties.teamid == boostproperties.teamid)
                {
                    other.rigidbody.velocity = transform.up * booststrength;
                    //boostproperties.contain.network.power -= 20f;
                    nextboost = Time.time + boostdelay;
                }
            }
        }
    }
}
