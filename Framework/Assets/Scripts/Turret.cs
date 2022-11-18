using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour
{
    public float range;
    public SphereCollider rangeindicator;
    public List<Transform> targets;
    public List<Transform> removetargets;
    public EntityProperties turretstats;
    WeaponBehavior turretweapon;
    public Transform turretbase;
    public Transform turretgun;
    public Transform turretaim;

    public float serverrange;

    private Transform thistransform;
    private PowerNetworks.Contain thiscontain;
    private PowerNetworks.Network thisnetwork;
    // Start is called before the first frame update
    void Start()
    {
        targets = new List<Transform>();
        turretstats = this.GetComponentInParent<EntityProperties>();
        turretweapon = this.GetComponentInParent<WeaponBehavior>();
        rangeindicator.radius = range;
        thistransform = this.transform.parent;
        thiscontain = turretstats.contain;
        thisnetwork = thiscontain.network;
    }

    // Update is called once per frame
    void Update()
    {
        if (Mirror.NetworkClient.localPlayer.isServer)
        {
            thisnetwork = thiscontain.network;
            //Debug.Log(targets.Count);
            //(trget.position - meTransform.position).sqrMagnitude < maxSeeDistanceSqrd
            if (targets.Count > 0 && (!Mirror.NetworkClient.localPlayer.isServer || thisnetwork.power >= turretweapon.cost))//Time.time >= turretweapon.nextFire && thisnetwork.power >= turretweapon.cost) // && Time.time >= turretweapon.nextFire
            {
                RaycastHit[] hits;
                //LayerMask mask2 = LayerMask.GetMask("Blueprint");
                if (targets.Count > 1)
                {
                    targets.Sort((t1, t2) => ((t1.position - thistransform.position).sqrMagnitude).CompareTo((t2.position - thistransform.position).sqrMagnitude));
                }

                bool shot = false;
                foreach (Transform selectedtarget in targets)
                {
                    if (selectedtarget == null || selectedtarget.GetComponent<EntityProperties>() == null)
                    {
                        removetargets.Add(selectedtarget);
                        continue;
                    }

                    hits = Physics.RaycastAll(turretgun.position, (selectedtarget.position - turretgun.position).normalized, range);//, QueryTriggerInteraction.Collide);
                    foreach (RaycastHit hit in hits)
                    {
                        //Debug.Log(hit.transform);
                        if (hit.transform == selectedtarget)
                        {
                            //Debug.Log("attempt fire");
                            shot = true;

                            var distanceToPlane = Vector3.Dot(thistransform.up, selectedtarget.position - turretbase.position);
                            var planePoint = selectedtarget.position - thistransform.up * distanceToPlane;
                            turretbase.LookAt(planePoint, thistransform.up);

                            Physics.SyncTransforms();

                            distanceToPlane = Vector3.Dot(turretbase.right, selectedtarget.position - turretgun.position);
                            planePoint = selectedtarget.position - turretbase.right * distanceToPlane;
                            turretgun.LookAt(planePoint, -turretbase.right);

                            /*Vector3 worldLookDirection = turretbase.position - transform.position;
                            Vector3 localLookDirection = turretbase.InverseTransformDirection(worldLookDirection);
                            localLookDirection.y = 0;
                            localLookDirection.x = 0;

                            transform.forward = transform.rotation * localLookDirection;

                            worldLookDirection = turretgun.position - transform.position;
                            localLookDirection = turretgun.InverseTransformDirection(worldLookDirection);*/

                            //turretbase.Rotate();
                            //turretgun.Rotate();
                            //turretweapon.Shoot((selectedtarget.position - turretgun.position).normalized);
                            if (Mirror.NetworkClient.localPlayer.isServer)
                            {
                                turretweapon.Shoot(selectedtarget.position);
                            }
                            break;
                        }
                    }
                    if(shot)
                    {
                        break;
                    }
                }
                foreach (Transform remove in removetargets)
                {
                    targets.Remove(remove);
                }
            }
        }
        
        //THIS WORKS, FIGURE IT OUT LATER

       /* Quaternion rotation = turretgun.rotation; 
        turretbase.localRotation = Quaternion.identity;
        turretgun.localRotation = Quaternion.identity;
        Vector3 something = thistransform.position + rotation * Vector3.forward * 10f;
        float distancething = Vector3.Dot(thistransform.up, something - turretbase.position);
        something = something - thistransform.up * distancething;
        turretbase.LookAt(something, thistransform.up);
        turretgun.rotation = rotation;*/
        //turretbase.rotation = Quaternion.Euler(rotation * transform.up);
        //turretbase.localRotation = Quaternion.Euler(0, rotation.eulerAngles.y, 0);
        //turretgun.localRotation = Quaternion.Euler(rotation.eulerAngles.x, 0, 0);
    }

    public void ServerShoot(Quaternion rotation) //temporary, needs testing
    {
        /*Vector3 selectedtarget = turretgun.position + rotation * transform.forward * serverrange;
        var distanceToPlane = Vector3.Dot(thistransform.up, selectedtarget - turretbase.position);
        var planePoint = selectedtarget - thistransform.up * distanceToPlane;
        turretbase.LookAt(planePoint, thistransform.up);

        Physics.SyncTransforms();

        distanceToPlane = Vector3.Dot(turretbase.right, selectedtarget - turretgun.position);
        planePoint = selectedtarget - turretbase.right * distanceToPlane;
        turretgun.LookAt(planePoint, -turretbase.right);*/

        //turretbase.rotation = Quaternion.Euler(rotation * turretbase.up);
       /* float angle = Quaternion.Dot(turretbase.rotation, rotation);
        turretbase.rotation = Quaternion.AngleAxis(angle, turretbase.up);
        //turretbase.rotation = Quaternion.FromToRotation(turretbase.up, rotation.eulerAngles) * turretbase.rotation;
        Physics.SyncTransforms();
        turretgun.rotation = Quaternion.FromToRotation(turretgun.right, rotation.eulerAngles) * turretgun.rotation;
        //turretgun.rotation = Quaternion.FromToRotation(turretbase.right, rotation.eulerAngles);
       */

        /*Vector3 something = turretgun.position + rotation * Vector3.forward * 10f;
        float distancething = Vector3.Dot(thistransform.up, something - turretbase.position);
        turretbase.LookAt(something - thistransform.up * distancething, thistransform.up);
        turretgun.rotation = rotation;

        Physics.SyncTransforms();

        distancething = Vector3.Dot(turretbase.right, something - turretgun.position);
        something = something - turretbase.right * distancething;
        turretgun.LookAt(something - turretbase.right * distancething, -turretbase.right);*/
    }

    void OnTriggerEnter(Collider other)
    {
        //Debug.Log("hi");
        if (!other.isTrigger)// && other.GetContact(0).thisCollider == rangeindicator)
        {

            EntityProperties otherstats = other.GetComponentInParent<EntityProperties>();
            if (turretstats != null)
            {
                if (otherstats != null && otherstats.teamid != turretstats.teamid && otherstats.entitytype == 3 && !targets.Contains(otherstats.transform))
                {
                    Debug.Log("hi");
                    targets.Add(otherstats.transform);
                }
            }
        }
    }
    /*private void OnCollisionEnter(Collision other)
    {

        Debug.Log("hi");
        EntityProperties otherstats = other.transform.GetComponentInParent<EntityProperties>();
        if (otherstats != null & otherstats.teamnumber != turretstats.teamnumber && otherstats.buildingid == -1)
        {
            targets.Add(otherstats.transform);
        }
    }*/
    private void OnTriggerExit(Collider other)
    {
        if (!other.isTrigger)
        {
            EntityProperties otherstats = other.GetComponentInParent<EntityProperties>();
            if (otherstats != null && targets.Contains(otherstats.transform))
            {
                targets.Remove(otherstats.transform);
            }
        }
    }
    private void OnDrawGizmos()
    {
        foreach (Transform selectedtarget in targets)
        {
            
            Debug.DrawLine(turretgun.position, turretgun.position+(selectedtarget.position - turretgun.position).normalized * range, Color.green);
        }
    }
}
