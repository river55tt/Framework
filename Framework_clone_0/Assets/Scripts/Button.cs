using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : MonoBehaviour
{
    public int buttonid;
    public IInteract connectedscript;
    public GameObject scriptobject;
    //IInteract script = (IUpgradeable)GetComponent(typeof(IUpgradeable));
    // Start is called before the first frame update

    void Start()
    {
        connectedscript = (IInteract)scriptobject.GetComponent(typeof(IInteract));
    }
    // Update is called once per frame
    public void interact(int entityID)
    {
        connectedscript.interactbutton(buttonid, entityID);
    }
}
