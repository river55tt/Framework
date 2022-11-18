using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSelection : MonoBehaviour
{
    public Transform player;
    public PlayerMovement playermovement;
    public Transform targetTransform;
    public Transform targetRotate;
    public Transform targetRotate2;
    public Transform[] aimTransform;
    public Transform midsection;
    public Transform chest;
    public Transform cam;

    public Animator animController;
    public bool combatmode;
    public bool sprinting;

    public int aim = 1;
    [Range(0, 1)]
    public float[] weight;
    public int iterations = 3;

    public float gunrange;
    float turnSmoothVelocity;

    public bool[] heldweapons;
    public GameObject[] Weapons;
    public WeaponBehavior[] GunStats;
    public GameObject Pistol;
    public GameObject AR;
    public GameObject Shotgun;
    public GameObject SMG;
    public GameObject RocketL;
    // Start is called before the first frame update
    void Start()
    {
        GunStats = new WeaponBehavior[Weapons.Length];
        int teamid = GetComponent<EntityProperties>().teamid;
        for (int i = 0; i < Weapons.Length; i++)
        {
            GunStats[i] = Weapons[i].GetComponent<WeaponBehavior>();
            GunStats[i].teamid = teamid;
            GunStats[i].weaponid = i;
        }
    }
    void Update()
    {
        if (GetComponentInParent<PlayerNetwork>().isLocalPlayer)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                if (aim != 1)
                {
                    aim = 1;
                }
                else
                {
                    aim = 0;
                }
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                if (aim != 2)
                {
                    aim = 2;
                }
                else
                {
                    aim = 0;
                }
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                if (aim != 3)
                {
                    aim = 3;
                }
                else
                {
                    aim = 0;
                }
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                if (aim != 4)
                {
                    aim = 4;
                }
                else
                {
                    aim = 0;
                }
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                if (aim != 5)
                {
                    aim = 5;
                }
                else
                {
                    aim = 0;
                }
            }

            if (!combatmode)
            {
                aim = 0;
            }

            Pistol.SetActive(false);
            AR.SetActive(false);
            Shotgun.SetActive(false);
            SMG.SetActive(false);
            RocketL.SetActive(false);
            if (aim == 0 || !heldweapons[aim - 1])
            {
                gunrange = 100f;
                aim = 0;
            }
            else if (aim == 1)
            {
                Pistol.SetActive(true);
                gunrange = 30f;
            }
            else if (aim == 2)
            {
                AR.SetActive(true);
                gunrange = 45f;
            }
            else if (aim == 3)
            {
                SMG.SetActive(true);
                gunrange = 30f;
            }
            else if (aim == 4)
            {
                Shotgun.SetActive(true);
                gunrange = 20f;
            }
            else if (aim == 5)
            {
                RocketL.SetActive(true);
                gunrange = 30f;
            }
            animController.SetInteger("Aim", aim);
            //AimGun();
        }
        //animController.SetInteger("Aim", aim);
    }
    // Update is called once per frame
    void LateUpdate()
    {
        if (GetComponentInParent<PlayerNetwork>().isLocalPlayer)
        {
            if (combatmode)
            {
                Vector3 targetPosition;
                RaycastHit hit;
                LayerMask mask = LayerMask.GetMask("Player");
                if (Physics.Raycast(targetRotate2.position, targetRotate2.forward, out hit, gunrange, ~mask))
                {
                    //Debug.Log(hit.transform.name);
                    if (hit.distance > 10f)
                    {
                        targetTransform.localPosition = new Vector3(0, 0, hit.distance);
                    }
                    else
                    {
                        targetTransform.localPosition = new Vector3(0, 0, 10);
                    }
                }
                //else
                //{
                targetPosition = targetTransform.position;
                //}
                for (int i = 0; i < iterations; i++)
                {
                    AimAtTarget(midsection, targetPosition, aimTransform[aim], weight[0]);
                    AimAtTarget(chest, targetPosition, aimTransform[aim], weight[1]);
                }
                /*Vector3 targetPosition = targetTransform.position;
                 for (int i = 0; i < iterations; i++)
                 {
                     AimAtTarget(midsection, targetPosition, aimTransform[aim], weight);
                     AimAtTarget(chest, targetPosition, aimTransform[aim], weight);
                 }*/
            }
            else
            {
                midsection.localRotation = Quaternion.Euler(Vector3.zero);
            }
            AdjustAim();

            if (Input.GetButton("Fire1") && aim != 0 && combatmode)
            {
                targetRotate.localEulerAngles = new Vector3(0, 0, 0);
                playermovement.shooting = true;
                playermovement.sprint = false;
                Vector3 targetPosition;
                RaycastHit hit;
                LayerMask mask = LayerMask.GetMask("Player");
                if (Physics.Raycast(targetRotate2.position, targetRotate2.forward, out hit, gunrange, ~mask))
                {
                    //Debug.Log(hit.transform.name);
                    if (hit.distance > 10)
                    {
                        targetTransform.localPosition = new Vector3(0, 0, hit.distance);
                    }
                    else
                    {
                        targetTransform.localPosition = new Vector3(0, 0, 10);
                    }
                }
                //else
                //{
                targetPosition = targetTransform.position;
                //}
                for (int i = 0; i < iterations; i++)
                {
                    //AimAtTarget(midsection, targetPosition, aimTransform[aim], weight[0]);
                    AimAtTarget(chest, targetPosition, aimTransform[aim], weight[1]);
                }
                GunStats[aim - 1].Shoot(targetTransform.position);
            }
            else
            {
                playermovement.shooting = false;
            }
        }
    }

    private void AimAtTarget(Transform bone, Vector3 targetPosition, Transform aimTransform, float weight)
    {
        Vector3 aimDirection = aimTransform.forward;
        if (aim != 0)
        {
            aimDirection = aimTransform.up * -1;
        }
        //Debug.Log(bone.position - aimTransform.position);
        //targetTransform.position = targetPosition + bone.position - aimTransform.position;
        //Debug.Log(bone.InverseTransformPoint(aimTransform.position));
        Vector3 targetDirection = targetPosition + bone.position - aimTransform.position;
        //Vector3 targetDirection = targetPosition - aimTransform.position;
        Vector3 targetRelative = bone.InverseTransformPoint(targetDirection);

        float angleYtotarget = Mathf.Atan2(targetRelative.x, targetRelative.z) * Mathf.Rad2Deg;
        float angleXtotarget = Mathf.Atan2(targetRelative.y, targetRelative.z) * Mathf.Rad2Deg;
       
        //Debug.Log(targetDirection);
        //Debug.Log(angleYtotarget);
        //Debug.Log(angleXtotarget);
        //Debug.Log(bone.eulerAngles.y);

        // Quaternion aimTowards = Quaternion.FromToRotation(aimDirection, targetDirection);
        //Quaternion blendedRotation = Quaternion.Slerp(Quaternion.identity, aimTowards, weight);
        //bone.localEulerAngles = new Vector3(0, bone.localEulerAngles.y + angleYtotarget * weight, 0);
        //bone.localEulerAngles = new Vector3(-angleXtotarget, 0, 0);
        bone.localEulerAngles = new Vector3(bone.localEulerAngles.x - angleXtotarget*weight, bone.localEulerAngles.y + angleYtotarget*weight, 0);
        // bone.Rotate(-angleXtotarget,0,0);
        // bone.Rotate(0,angleYtotarget, 0);
        // bone.Rotate(0,angleYtotarget, 0);
        //Quaternion blah = Quaternion.(aimTransform.rotation - bone.rotation);
        //bone.Rotate(new Vector3(aimTransform.rotation - bone.rotation)));
        //bone.localEulerAngles = new Vector3(bone.eulerAngles.x - angleXtotarget * weight, 0, 0);
        //bone.rotation = blendedRotation * bone.rotation;

        //bone.rotation = (aimTransform.rotation * Quaternion.Inverse(bone.rotation)) * bone.rotation;
        //bone.Rotate(bone.eulerAngles.x - aimTransform.eulerAngles.x, bone.eulerAngles.y - aimTransform.eulerAngles.y, bone.eulerAngles.z - aimTransform.eulerAngles.z);
        //Debug.Log(bone.forward + aimTransform.up);
        //if (Input.GetKey(KeyCode.Alpha4))
        //{
            bone.LookAt(chest.position + chest.forward * 3f * gunrange + aimTransform.up * gunrange);
        //}
        //Debug.Log(-aimTransform.up);
        //Debug.Log(new Vector3(bone.eulerAngles.x - aimTransform.eulerAngles.x, bone.eulerAngles.y - aimTransform.eulerAngles.y, bone.eulerAngles.z - aimTransform.eulerAngles.z));
    }

    private void AdjustAim()
    {
        targetTransform.localPosition = new Vector3(0f, 0f, gunrange);

        float targetangle;
        if (targetRotate2.localEulerAngles.y < 180f)
        {
            //targetRotate.localEulerAngles = new Vector3(0, Mathf.Clamp(targetRotate2.localEulerAngles.y, 0, 60f) - targetRotate2.localEulerAngles.y, 0);
            targetangle = Mathf.Clamp(targetRotate2.localEulerAngles.y, 0, 60f) - targetRotate2.localEulerAngles.y;
        }
        else
        {
            //targetRotate.localEulerAngles = new Vector3(0, Mathf.Clamp(targetRotate2.localEulerAngles.y, 300f, 360f) - targetRotate2.localEulerAngles.y, 0);
            targetangle = Mathf.Clamp(targetRotate2.localEulerAngles.y, 300f, 360f) - targetRotate2.localEulerAngles.y;
        }

        //Debug.Log(targetangle);
        if (!sprinting)
        {
            float angle = Mathf.SmoothDampAngle(targetRotate.localEulerAngles.y, 0, ref turnSmoothVelocity, .1f, 600f);
            if (targetangle > 0f)
            {
               // angle = Mathf.Clamp(angle, targetangle, 180f);
            }
            else if (targetangle < 0f)
            {
                //angle = Mathf.Clamp(angle, -240f, targetangle);
            }
            targetRotate.localEulerAngles = new Vector3(0, angle, 0);
        }
        else
        {
            //Debug.Log(Mathf.Clamp(Quaternion.eulerAngles(0,targetRotate2.localRotation.y,0).y, -45f, 45f));
            //Debug.Log(targetRotate2.localEulerAngles.y);
            //Debug.Log(Mathf.Clamp(targetRotate2.localEulerAngles.y, 0, 60f));
            //Debug.Log(Mathf.Clamp(targetRotate2.localEulerAngles.y, 300f, 360f));
            float angle = Mathf.SmoothDampAngle(targetRotate.localEulerAngles.y, targetangle, ref turnSmoothVelocity, .1f, 600f);
            if (targetangle > 0f)
            {
                //angle = Mathf.Clamp(angle, targetangle, 240f);
            }
            else if (targetangle < 0f)
            {
                //angle = Mathf.Clamp(angle, -240f, targetangle);
            }
            targetRotate.localEulerAngles = new Vector3(0, angle, 0);
        }
    }

    private void OnDrawGizmos()
    {
       /* Debug.DrawLine(targetRotate2.position, targetRotate2.position + targetRotate2.forward * gunrange, Color.red);
        Debug.DrawLine(chest.position, chest.position + chest.forward * gunrange, Color.green);
        Debug.DrawLine(chest.position, chest.position + chest.forward * 2 * gunrange + aimTransform[aim].up*gunrange, Color.black);
    */}

}
