using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnInteract : MonoBehaviour, IInteract
{
    // Start is called before the first frame update
    public void interactbutton(int buttonid, int entityID)
    {
        SpawnSystem.SpawnManager.usingspawn = true;
        //SpawnSystem.SpawnManager.player = this.transform;
        SpawnSystem.SpawnManager.activespawn = transform.GetComponentInParent<EntityProperties>();
    }
}
