using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransceiverSystem : MonoBehaviour
{
    public static TransceiverSystem TransceiverManager;
    public List<Team> teamdata = new List<Team>();
    // Start is called before the first frame update
    void Awake()
    {
        if (TransceiverManager == null)
        {
            TransceiverManager = this;
            Debug.Log("Initialized Transceiver Manager");

            Team newteam = new Team { };
            newteam.transceivers = new List<Transceiver>();
            //newteam.blueprints = new List<Blueprint>();
            teamdata.Add(newteam);
        }
    }

    // Update is called once per frame

    public class Team
    {
        public List<Transceiver> transceivers;
    }

    public void EditTransceiverList(Transceiver selectedobject, bool adding)
    {
        int team = selectedobject.GetComponent<EntityProperties>().teamid;
        if (adding)
        {
            teamdata[team].transceivers.Add(selectedobject);
        }
        else
        {
            teamdata[team].transceivers.Remove(selectedobject);
        }
    }
}
