using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayGauge : MonoBehaviour
{
    public Text DisplayText;
    public Text persecondText;
    public PowerNetworks.Contain thiscontain;
    public PowerNetworks.Network thisnetwork;
    // Start is called before the first frame update
    void Start()
    {
        thiscontain = gameObject.GetComponent<EntityProperties>().contain;
        thisnetwork = thiscontain.network;
    }

    // Update is called once per frame
    void Update()
    {
        thisnetwork = thiscontain.network;
        DisplayText.text = Mathf.RoundToInt(thisnetwork.power) + "/" + Mathf.RoundToInt(thisnetwork.powerstorage);
        persecondText.text = Mathf.RoundToInt(thisnetwork.powerstabledelta - thisnetwork.leaklist.Count * 5f) + "/s";
    }
}
