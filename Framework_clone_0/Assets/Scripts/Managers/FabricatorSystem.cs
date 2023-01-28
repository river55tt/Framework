using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FabricatorSystem : MonoBehaviour
{
    public static FabricatorSystem FabricatorManager;
    public List<Team> teamdata = new List<Team>();
    //public List<Fabricator> fabricators = new List<Fabricator>();
    //public List<Blueprint> blueprints = new List<Blueprint>();

    // Start is called before the first frame update
    void Awake()
    {
        if (FabricatorManager == null)
        {
            FabricatorManager = this;
            Debug.Log("Initialized Fabricator Manager");

            for (int t = 0; t < 6; t++)
            {
                Team newteam = new Team { };
                newteam.fabricators = new List<Fabricator>();
                newteam.blueprints = new List<Blueprint>();
                teamdata.Add(newteam);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        //thisnetwork = thiscontain.network;
        /*foreach(Fabricator fab in fabricators)
        {
            if(blueprints[0] != null)
            {

            }
        }*/
    }
    public class Team
    {
        public List<Fabricator> fabricators;
        public List<Blueprint> blueprints;
    }

    public void EditBPList(Blueprint selectedbp, bool adding)
    {
        int team = selectedbp.GetComponent<EntityProperties>().teamid;
        if (adding)
        {
            teamdata[team].blueprints.Add(selectedbp);
        }
        else
        {
            teamdata[team].blueprints.Remove(selectedbp);
        }
    }
   /* public void RemoveBP(Blueprint removedbp)
    {
        blueprints.Remove(removedbp);
    }*/
}