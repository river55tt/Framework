using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour, IInteract
{
    int teamid;
    public EntityProperties controllerproperties;
    public EntityProperties shieldproperties;
    public Transform shieldobject;
    public float[] shieldsizes = { 10, 15, 20 };
    public float shieldhp;
    float shieldmaxhp = 3000;
    float brokentimer;
    int shieldselect;
    bool shieldon;
    bool switchon;

    public int shieldintensity;
    public Material shieldmat;
    // Start is called before the first frame update
    void Start()
    {
        //shieldproperties = GetComponent<EntityProperties>();
        teamid = controllerproperties.teamid;
        shieldmat = Instantiate(shieldmat);
        shieldobject.GetComponent<Renderer>().material = shieldmat;
        shieldintensity = Shader.PropertyToID("ShieldIntensity");

        shieldmaxhp = shieldproperties.maxhealth;
    }

    // Update is called once per frame
    void Update()
    {
        brokentimer -= Time.deltaTime;
        if (shieldproperties.dead && shieldon)
        {
            brokentimer = 10;
        }

        float cost = shieldsizes[shieldselect] / 5f * Time.deltaTime;
        if (brokentimer <= 0 && controllerproperties.contain.network.power > cost && switchon)
        {
            shieldhp = shieldproperties.health;
            //shieldmaxhp = shieldproperties.maxhealth;

            controllerproperties.contain.network.power -= cost;
            if (!shieldon)
            {
                shieldon = true;
                shieldproperties.dead = false;
                shieldobject.gameObject.SetActive(true);
                shieldobject.localScale = Vector3.one * shieldsizes[shieldselect];
            }
            if(shieldhp < shieldmaxhp)
            {
                shieldhp += 10 * Time.deltaTime;
                shieldproperties.health = shieldhp;
                shieldmat.SetFloat(shieldintensity, 1f + (shieldhp/shieldmaxhp) * 2f);
                //shieldmat.SetFloat("ShieldIntensity", 1f + shieldhp / shieldmaxhp * 2f);
                //Debug.Log(shieldmat.GetFloat("ShieldIntensity"));
            }
        }
        else
        {
            shieldon = false;
            shieldobject.gameObject.SetActive(false);
            shieldhp = 0;
            shieldproperties.health = shieldhp;
            shieldobject.localScale = Vector3.zero;
        }
    }

    public void interactbutton(int buttonid, int entityID)
    {
        switch (buttonid)
        {
            case 0:
                switchon = !switchon;
                if(shieldon)
                {
                    shieldon = false;
                    shieldobject.gameObject.SetActive(false);
                    shieldobject.localScale = Vector3.zero;
                }
                break;
            case 1:
                shieldselect = 0;
                if(shieldon)
                {
                    shieldobject.localScale = Vector3.one * shieldsizes[shieldselect];
                }
                break;
            case 2:
                shieldselect = 1;
                if (shieldon)
                {
                    shieldobject.localScale = Vector3.one * shieldsizes[shieldselect];
                }
                break;
            case 3:
                shieldselect = 2;
                if (shieldon)
                {
                    shieldobject.localScale = Vector3.one * shieldsizes[shieldselect];
                }
                break;
        }
    }
}
