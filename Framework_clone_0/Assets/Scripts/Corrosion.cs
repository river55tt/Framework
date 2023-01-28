using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Corrosion : NetworkBehaviour
{
    // Start is called before the first frame update
    List<EntityProperties> underwater;
    void Start()
    {
        underwater = new List<EntityProperties>();
    }

    // Update is called once per frame
    void Update()
    {
        List<EntityProperties> removelist = new List<EntityProperties>();
        foreach (EntityProperties unit in underwater)
        {
            unit.TakeDamage(15f * Time.deltaTime, 100f, Vector3.zero, null, false, out List<int> partindex, out bool dead);
            NetworkClient.localPlayer.GetComponent<PlayerNetwork>().CmdDamageObject(new List<int> { unit.entityID }, new List<float> { 15f * Time.deltaTime },
    new List<float> { 100f }, new List<Vector3> { Vector3.zero }, new List<List<int>> { partindex }, new List<bool> { dead });
            if (unit.health <= 0)
            {
                removelist.Add(unit);                
            }
        }
        foreach (EntityProperties remove in removelist)
        {
            underwater.Remove(remove);
        }
    }

    // void OnCollisionStay(Collision other)
    //{
    void OnTriggerEnter(Collider other)
    {
        //Debug.Log("collision");
        EntityProperties colliderproperties = other.transform.GetComponentInParent<EntityProperties>();
        Rigidbody colliderRB = other.transform.GetComponentInParent<Rigidbody>();
        //Debug.Log(colliderobject.name);
        if (colliderproperties != null && colliderRB != null)
        {
            if (!colliderRB.isKinematic)
            {
                if (colliderproperties.entitytype == 3 && !underwater.Contains(colliderproperties) && colliderproperties.GetComponentInParent<PlayerNetwork>().isLocalPlayer)
                {
                    //colliderproperties.TakeDamage(10f * Time.deltaTime, 100, Vector3.zero, null);
                    underwater.Add(colliderproperties);
                }
                else if(colliderproperties.entitytype != 3 && isServer)
                {
                    colliderproperties.health = 0;
                    colliderproperties.TakeDamage(100000f, 100, Vector3.zero, null, false, out List<int> partindex, out _);
                    NetworkClient.localPlayer.GetComponent<PlayerNetwork>().CmdDamageObject(new List<int> { colliderproperties.entityID }, new List<float> { 100000f },
                        new List<float> { 100f }, new List<Vector3> { Vector3.zero }, new List<List<int>> { partindex }, new List<bool> { true });
                }
            }
        }
    }
    void OnTriggerExit(Collider other)
    {
        EntityProperties colliderproperties = other.transform.GetComponentInParent<EntityProperties>();
        //Debug.Log(colliderobject.name);
        if (colliderproperties != null && underwater.Contains(colliderproperties))
        {
            underwater.Remove(colliderproperties);
        }
    }
}
