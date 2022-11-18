using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class SpawnUI : MonoBehaviour, IPointerDownHandler//, IPointerClickHandler
{
    // Start is called before the first frame update
    public int buttontype; //0 > button, 1 > spawn, 2 > leave

    // Update is called once per frame
    /*void Update()
    {
        if (EventSystem.current.is)
        {
            Debug.Log("button id: " + buttontype);
        }
    }*/
    private void OnMouseDown()
    {
        
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        //eventData.p
        Debug.Log(this.gameObject.name + " Was Clicked.");
        if (buttontype == 0)
        {
            Debug.Log(SpawnSystem.SpawnManager.spawnicons.FindIndex(x => x == this.gameObject) - 3);
            SpawnSystem.SpawnManager.selectedspawn = SpawnSystem.SpawnManager.spawnentities[SpawnSystem.SpawnManager.spawnicons.FindIndex(x => x == this.gameObject)-3];
        }
        else if(buttontype == 1)
        {
            if (SpawnSystem.SpawnManager.cantele == true)
            {
                SpawnSystem.SpawnManager.Teleport();
            }
        }
        else if (!SpawnSystem.SpawnManager.respawning)
        {
            SpawnSystem.SpawnManager.usingspawn = false;
            SpawnSystem.SpawnManager.selectedspawn = null;
            SpawnSystem.SpawnManager.activespawn = null;
        }
    }
}
