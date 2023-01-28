using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Mirror;

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem SaveManager;
    public bool loadingdata;
    NetworkManager networkmanager;
    [SerializeField] public static EntityObjectList CurrentSaveData = new EntityObjectList(new List<EntityObject>());

    public const string SaveDirectory = "/SaveData/";
    public const string FileName = "SaveGame.sav";


    public void Start()
    {
        networkmanager = NetworkManager.singleton;
    }
    public void Awake()
    {
        SaveManager = this;
        Debug.Log(CurrentSaveData);
    }


    void OnGUI()
    {
        if (GUILayout.Button("Load Save"))
        {
            loadingdata = true;
            networkmanager.StartHost();
        }
    }

    public void SaveData()
    {
        StartCoroutine(Save());
    }
    IEnumerator Save()
    {
        List<Vector3> chunkposition = new List<Vector3>();
        List<Quaternion> chunkrotation = new List<Quaternion>();
        List<int> snappedfrom = new List<int>();
        List<int> snappedto = new List<int>();
        List<int> snapobjectIDs = new List<int>();
        List<int> anchorobjectIDs = new List<int>();

        List<EntityObject> Objects = new List<EntityObject>();

        int k = 0;
        for (int i = 0; i < AnchorSystem.AnchorManager.allchunks.Count; i++) //snap
        {
            for (int j = 0; j < AnchorSystem.AnchorManager.allchunks[i].contents.Count; j++)
            {
                EntityObject objdata = new EntityObject(AnchorSystem.AnchorManager.allchunks[i].contents[j]);
                Objects.Add(objdata);
                k++;

                if (k > 20)
                {
                    k = 0;
                    yield return null;
                }
            }
        }
        CurrentSaveData = new EntityObjectList(Objects);

        var dir = Application.persistentDataPath + SaveDirectory;
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);


        string json = JsonUtility.ToJson(CurrentSaveData, true);
        File.WriteAllText(dir + FileName, json);

        GUIUtility.systemCopyBuffer = dir + FileName;
        //Debug.Log(CurrentSaveData);
        //Debug.Log(CurrentSaveData.ToString());
    }


    public void LoadData()
    {
        StartCoroutine(Load());
    }
    IEnumerator Load()
    {
        var dir = Application.persistentDataPath + SaveDirectory;
        GUIUtility.systemCopyBuffer = dir + FileName;
        if (!File.Exists(dir + FileName))
        {
            Debug.Log("No Save Found"); 
            yield break;
        }

        string json = File.ReadAllText(dir + FileName);
        EntityObjectList tempdata = JsonUtility.FromJson<EntityObjectList>(json);

        int k = 0;
        for (int i = 0; i < tempdata.AllEntityObjects.Count; i++) //snap
        {
            EntityObject tempobject = tempdata.AllEntityObjects[i];
            List<Vector3> positions = new List<Vector3>();
            List<Quaternion> rotations = new List<Quaternion>();

            for (int p = 0; p < tempobject.position.Count; p++)
            {
                positions.Add(new Vector3(tempobject.position[p].floatdata[0], tempobject.position[p].floatdata[1], tempobject.position[p].floatdata[2]));
            }
            for (int p = 0; p < tempobject.rotation.Count; p++)
            {
                rotations.Add(new Quaternion(tempobject.rotation[p].floatdata[1], tempobject.rotation[p].floatdata[2], tempobject.rotation[p].floatdata[3], tempobject.rotation[p].floatdata[0]));
            }



            GameSystem.GameManager.NetworkObject(tempobject.teamID, tempobject.objecttype, positions, rotations, -1, new List<int>(), new List<int>(), new List<int>(), new List<int>(), tempobject.entityID, false, out _);





            k++;
            if (k > 20)
            {
                k = 0;
                yield return null;
            }
        }
        TerrainSystem.TerrainManager.generatingterrain = false;
        yield return null;
    }
}

[System.Serializable]
public class EntityObjectList
{
    public List<EntityObject> AllEntityObjects;
    public EntityObjectList(List<EntityObject> allobjects)
    {
        AllEntityObjects = new List<EntityObject>();
        AllEntityObjects = allobjects;
        Debug.Log(AllEntityObjects.Count);
    }
}

[System.Serializable]
public class EntityObject
{
    public int teamID;
    public int objecttype;
    public int entityID;
    public float health;
    public bool isbp;

    //[SerializeField] public List<float[]> position;
    public List<FloatArray> position;
    //[SerializeField] public List<float[]> rotation;
    public List<FloatArray> rotation;
    public List<int> snappedfrom;
    public List<int> snappedto;
    public List<int> snapobjectIDs;
    public List<int> anchorobjectIDs;

    public EntityObject(AnchorSystem.Object objectdata)
    {
        isbp = false;

        position = new List<FloatArray>();
        rotation = new List<FloatArray>();
        //List<float> tempposition = new List<float>();
        //List<float> temprotation = new List<float>();
        snappedfrom = new List<int>();
        snappedto = new List<int>();
        snapobjectIDs = new List<int>();
        anchorobjectIDs = new List<int>();



        EntityProperties syncobject = objectdata.objectproperties;
        Blueprint parentBP = null;

        if (syncobject.GetComponent<Blueprint>())
        {
            parentBP = syncobject.GetComponent<Blueprint>();
            isbp = true;
            health = parentBP.power;
        }
        else
        {
            health = syncobject.health;
        }
        teamID = syncobject.teamid;
        objecttype = syncobject.buildingid;
        entityID = syncobject.entityID;

        for (int n = 0; n < syncobject.snappedto.Length; n++) //weld to snapped
        {
            if (syncobject.snappedto[n] != null)
            {
                snappedfrom.Add(n);
                List<Transform> othersnapto = new List<Transform>();
                if (syncobject.snappedto[n].GetComponentInParent<Blueprint>())
                {
                    othersnapto = syncobject.snappedto[n].GetComponentInParent<Blueprint>().snappingfrom;
                }
                else
                {
                    othersnapto = syncobject.snappedto[n].GetComponentInParent<EntityProperties>().snappingfrom;
                }
                snappedto.Add(othersnapto.FindIndex(x => x == syncobject.snappedto[n]));
                snapobjectIDs.Add(syncobject.snappedto[n].GetComponentInParent<EntityProperties>().entityID);
            }
        }
        foreach (AnchorSystem.Object anchorobjectid in syncobject.anchordata.connectedto)
        {
            anchorobjectIDs.Add(anchorobjectid.objectproperties.entityID);
        }

        if (syncobject.buildingid != 1)
        {
            ObjectTransform(syncobject.transform);
        }
        else
        {
            if (isbp)
            {
                ObjectTransform(parentBP.blueprintobject[0].transform);
                ObjectTransform(parentBP.blueprintobject[1].transform);
                ObjectTransform(parentBP.blueprintobject[2].transform);
            }
            else
            {
                ObjectTransform(syncobject.transform);
                ObjectTransform(syncobject.transform.GetChild(1));
                ObjectTransform(syncobject.transform.GetChild(2));
            }
        }
    }

    void ObjectTransform(Transform transformdata)
    {
        List<float> pos = new List<float> {transformdata.localPosition.x,
            transformdata.localPosition.y,
            transformdata.localPosition.z};

        List<float> rot = new List<float> {transformdata.localRotation.w,
            transformdata.localRotation.x,
            transformdata.localRotation.y,
            transformdata.localRotation.z};
        position.Add(new FloatArray(pos));
        rotation.Add(new FloatArray(rot));
        // position.Add(pos);
        //rotation.Add(rot);
    }
}

[System.Serializable]
public class FloatArray
{
    public List<float> floatdata;
    public FloatArray(List<float> floats)
    {
        floatdata = floats;
    }
}