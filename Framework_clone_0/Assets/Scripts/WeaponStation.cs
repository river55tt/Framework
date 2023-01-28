using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponStation : MonoBehaviour, IInteract
{
    public EntityProperties thisproperties;
    // Start is called before the first frame update

    // Update is called once per frame


    public void interactbutton(int buttonid, int entityID)
    {
        switch (buttonid)
        {
            case 0:
                if (thisproperties.contain.network.power > 50) //ar
                {
                    //Debug.Log(GameSystem.GameManager.entityIDs[entityID]);
                    //Debug.Log(GameSystem.GameManager.teamdata[0].players[0]);
                   // GameSystem.GameManager.teamdata[0].players[0].GetComponent<WeaponSelection>().heldweapons[1] = true;
                    GameSystem.GameManager.entityIDs[entityID].GetComponent<WeaponSelection>().heldweapons[1] = true;
                    thisproperties.contain.network.power -= 50f;
                }
                break;
            case 1:
                if (thisproperties.contain.network.power > 40) //smg
                {
                    GameSystem.GameManager.entityIDs[entityID].GetComponent<WeaponSelection>().heldweapons[2] = true;
                    thisproperties.contain.network.power -= 40f;
                }
                break;
            case 2:
                if (thisproperties.contain.network.power > 40) //shotgun
                {
                    GameSystem.GameManager.entityIDs[entityID].GetComponent<WeaponSelection>().heldweapons[3] = true;
                    thisproperties.contain.network.power -= 40f;
                }
                break;
            case 3:
                if (thisproperties.contain.network.power > 70) //rocket launcher
                {
                    GameSystem.GameManager.entityIDs[entityID].GetComponent<WeaponSelection>().heldweapons[4] = true;
                    thisproperties.contain.network.power -= 70f;
                }
                break;
            case 4:

                break;
        }
    }
}
