using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravitationalAnchor : MonoBehaviour, IInteract
{
    public EntityProperties anchorproperties;
    public GameObject powersphere;
    private AnchorSystem.Object anchorobject;
    bool stabilized;
    bool switchon;
    // Start is called before the first frame update
    /* void Start()
     {

     }*/

    // Update is called once per frame
    void Update()
    {
        if(switchon && anchorproperties.contain.network.power > 5f * Time.deltaTime)
        {
            anchorproperties.contain.network.power -= 5f * Time.deltaTime;
            if(!stabilized)
            {
                anchorobject = anchorproperties.anchordata;
                anchorobject.anchor = true;
                AnchorSystem.AnchorManager.UpdateAnchor(anchorobject.chunk);
                powersphere.SetActive(true);
            }
            powersphere.transform.Rotate(0, 7f * Time.deltaTime, 0, Space.Self);
            stabilized = true;
        }
        else
        {
            if (stabilized)
            {
                anchorobject = anchorproperties.anchordata;
                anchorobject.anchor = false;
                AnchorSystem.AnchorManager.UpdateAnchor(anchorobject.chunk);
                powersphere.SetActive(false);
            }
            stabilized = false;
        }
    }

    public void interactbutton(int buttonid, int entityID)
    {
        switchon = !switchon;
    }
}
