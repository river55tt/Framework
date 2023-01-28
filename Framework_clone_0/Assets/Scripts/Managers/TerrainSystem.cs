using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TerrainSystem : NetworkBehaviour
{
    public static TerrainSystem TerrainManager;
    public GameObject[] basecubes;
    public GameObject laddercube;
    public GameObject pumpcube;
    public GameObject beaconcube;
    public GameObject[] ruincubes;

    public bool generatingterrain;
    int frameobjects;
    int totalobjects;

    float loadingfps = 30;

    bool generatesphere = false;
    public int maptype;

    public int islandheight = 15;

    public float mapsize = 30; //radius

    int beaconcount = 8; //not including middle beacon
    float beaconspacing = 20;

    private float unitsize = 3f;
    public float noisescale = 10f;
    public float stemscale = 2f;
    public float spirescale = 40f;

    public int universalseed;
    float terrainseedx;
    float terrainseedz;
    public float noisethreshold = .095f;

    System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
    System.Diagnostics.Stopwatch totalstopwatch = new System.Diagnostics.Stopwatch();

    int terrainID = 1;
    // Start is called before the first frame update

    public override void OnStartServer()
    {
        if (TerrainManager == null)
        {
            TerrainManager = this;
            Debug.Log("Initialized Terrain Manager");
        }
    }
    public override void OnStartClient()
    {
        if (TerrainManager == null)
        {
            TerrainManager = this;
            Debug.Log("Initialized Terrain Manager");
        }
    }
    void Start()
    {
        /*if (TerrainManager == null)
        {
            TerrainManager = this;
            Debug.Log("Initialized Terrain Manager");
        }*/
            /*if (TerrainManager == null)
            {
                TerrainManager = this;
                generatingterrain = true;
                Debug.Log("Initialized Terrain Manager");
                StartCoroutine("GenerateTerrain");
            }*/

            /*
            AnchorManager.CreateObject(terrain.GetComponent<EntityProperties>());
            terrain.GetComponent<EntityProperties>().anchordata.anchor = true;
            AnchorManager.UpdateAnchor(terrain.GetComponent<EntityProperties>().anchordata.chunk);
            //Debug.Log(terrain.GetComponent<EntityProperties>().anchordata.chunk.contents);

            new Vector3 placementposition = Vector3.zero;
            new GameObject blocktype;
            new GameObject placingblock = Instantiate(blocktype, placementposition, Vector3.zero);
            AnchorSystem.AnchorManager.CreateObject(placingblock);
            if (blocktype == pumpcube || blocktype == beaconcube || placementposition.y < -10 * unitsize)
            {
                placingblock.GetComponent<EntityProperties>().anchordata.anchor = true;
            }
            AnchorManager.UpdateAnchor(placingblock.GetComponent<EntityProperties>().anchordata.chunk);
            */
        }

    // Update is called once per frame
    /* void Update()
     {
         if (AnchorSystem.AnchorManager != null && generatingterrain)
         {
             System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
             stopwatch.Start();

             stopwatch.Stop();
             Debug.Log("Time taken: " + (stopwatch.Elapsed));
             stopwatch.Reset();

             Vector3 placementposition = Vector3.zero;
             GameObject blocktype;
             GameObject placingblock = Instantiate(blocktype, placementposition, Quaternion.identity);
             AnchorSystem.AnchorManager.CreateObject(placingblock);
             if (blocktype == pumpcube || blocktype == beaconcube || placementposition.y < -10 * unitsize)
             {
                 placingblock.GetComponent<EntityProperties>().anchordata.anchor = true;
             }
             AnchorSystem.AnchorManager.UpdateAnchor(placingblock.GetComponent<EntityProperties>().anchordata.chunk);
         }
     }*/
    public void StartGeneration()
    {
        generatingterrain = true;
        StartCoroutine("GenerateTerrain");
    }
    IEnumerator GenerateTerrain()
    {
        yield return new WaitUntil(() => AnchorSystem.AnchorManager != null);
        //System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        //System.Diagnostics.Stopwatch totalstopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        totalstopwatch.Start();
        frameobjects = 0;
        totalobjects = 0;
        // while (generatingterrain)
        //{
        //yield return StartCoroutine(TimeCheck(stopwatch));
        Vector3 Gravity = Physics.gravity;
        Physics.gravity = Vector3.zero;

        Debug.Log(Gravity);

        int beaconcount = 0;
        float beaconspacing = 0;
        if (generatesphere)
        {
            beaconcount = 5;
            beaconspacing = 5;
        }
        else if (!generatesphere)
        {
            beaconcount = 9;
            beaconspacing = 20;
        }
        /*
        for (int i = 0; i < beaconcount; i++) //beacons
        {
            Vector3 position;
            GameObject blocktype = beaconcube;
            bool anchored = true;

            if (i == 0)
            {
                position = new Vector3(0, 0, 0);
                PlaceTerrain(position, Vector3.zero, blocktype, anchored);
                if (generatesphere) //SPHERE
                {
                    position = new Vector3(0, -beaconspacing, 0);
                    PlaceTerrain(position, Vector3.zero, blocktype, anchored);
                    position = new Vector3(0, beaconspacing, 0);
                    PlaceTerrain(position, Vector3.zero, blocktype, anchored);
                    position = new Vector3(0, -beaconspacing * 5, 0);
                    PlaceTerrain(position, Vector3.zero, blocktype, anchored);
                    position = new Vector3(0, beaconspacing * 5, 0);
                    PlaceTerrain(position, Vector3.zero, blocktype, anchored);
                }
                continue;
            }
            else
            {
                //Debug.Log(Mathf.Cos((float)i / ((float)beaconcount - 1) * 2 * Mathf.PI));
                position = new Vector3(Mathf.Round(beaconspacing * Mathf.Cos((float)i / ((float)beaconcount - 1) * 2 * Mathf.PI)), 0, Mathf.Round(beaconspacing * Mathf.Sin((float)i / ((float)beaconcount - 1) * 2 * Mathf.PI)));
            }
            //yield return PlaceTerrain(position, blocktype, anchored, stopwatch);
            PlaceTerrain(position, Vector3.zero, blocktype, anchored);
            if (generatesphere) //SPHERE
            {
                //PlaceTerrain(position *5, blocktype, anchored);
            }
            if ((float)stopwatch.Elapsed.Milliseconds / 1000 > 1 / loadingfps && frameobjects > 0)//next frame
            {
                yield return WaitFrame(stopwatch);
            }
        }

        if (generatesphere) //SPHERE
        {
            int sphereradius = 25;
            float samplingrate = 1000;
            for (float i = 0; i < samplingrate; i++) //beacons
            {
                float theta = i / samplingrate * 2 * Mathf.PI;
                GameObject blocktype;
                Vector3 position;
                bool anchored = false;
                for (float j = 0; j < samplingrate; j++) //height
                {
                    float phi = j / samplingrate * Mathf.PI;
                    position = new Vector3(Mathf.Round(Mathf.Sin(phi) * Mathf.Cos(theta) * sphereradius), Mathf.Round(Mathf.Cos(phi) * sphereradius), Mathf.Round(Mathf.Sin(phi) * Mathf.Sin(theta) * sphereradius));
                    blocktype = basecubes[Random.Range(0, basecubes.Length)];
                    PlaceTerrain(position, Vector3.zero, blocktype, anchored);
                    if ((float)stopwatch.Elapsed.Milliseconds / 1000 > 1 / loadingfps && frameobjects > 0)//next frame
                    {
                        yield return WaitFrame(stopwatch);
                    }
                }
            }
        }

        if (!generatesphere) //NORMAL TERRAIN
        {
            int islandheight = 15;

            int islandcount = 50;
            float mapsize = 30; //radius

            float islandminsize = 5;
            float islandmaxsize = 10;

            int ruincount = 2;
            int ruinsize;
            float ruinheight = 3;
            //place beaconspots
            for (int i = 0; i < beaconcount; i++) //beacons
            {
                //Debug.Log(i);
                GameObject blocktype;
                Vector3 position;
                bool anchored = false;

                if (i == 0)
                {
                    position = new Vector3(0, 0, 0);
                }
                else
                {
                    //Debug.Log(Mathf.Cos((float)i / ((float)beaconcount - 1) * 2 * Mathf.PI));
                    position = new Vector3(Mathf.Round(beaconspacing * Mathf.Cos((float)i / ((float)beaconcount - 1) * 2 * Mathf.PI)), 0, Mathf.Round(beaconspacing * Mathf.Sin((float)i / ((float)beaconcount - 1) * 2 * Mathf.PI)));
                }
                for (int h = 1; h < islandheight + 1; h++)
                {
                    position = new Vector3(position.x, h - islandheight, position.z);
                    blocktype = basecubes[Random.Range(0, basecubes.Length)];
                    if (h == 1)
                    {
                        anchored = true;
                    }
                    else if (h == islandheight)
                    {
                        //Debug.Log("beacon");
                        blocktype = beaconcube;
                        anchored = true;
                    }
                    else
                    {
                        anchored = false;
                    }
                    //yield return PlaceTerrain(position, blocktype, anchored, stopwatch);
                    PlaceTerrain(position, Vector3.zero, blocktype, anchored);
                    if ((float)stopwatch.Elapsed.Milliseconds / 1000 > 1 / loadingfps && frameobjects > 0)//next frame
                    {
                        yield return WaitFrame(stopwatch);
                    }
                }
            }

            //generate island locations
            List<Vector3> islands = new List<Vector3>();
            for (int j = 0; j < islandcount; j++)
            {
                GameObject blocktype;
                Vector3 position = new Vector3(Mathf.Round(Random.Range(-mapsize, mapsize)), 0, Mathf.Round(Random.Range(-mapsize, mapsize)));
                bool anchored = false;
                islands.Add(new Vector3(position.x, 0, position.z));
                for (int h = 1; h < islandheight; h++)
                {
                    position = new Vector3(position.x, h - islandheight, position.z);
                    blocktype = basecubes[Random.Range(0, basecubes.Length)];
                    if (h == 1)
                    {
                        anchored = true;
                    }
                    else
                    {
                        anchored = false;
                    }
                    //yield return PlaceTerrain(position, blocktype, anchored, stopwatch);
                    PlaceTerrain(position, Vector3.zero, blocktype, anchored);
                    if ((float)stopwatch.Elapsed.Milliseconds / 1000 > 1 / loadingfps && frameobjects > 0)//next frame
                    {
                        yield return WaitFrame(stopwatch);
                    }
                }
            }

            //generate surface for islands
            for (int k = 0; k < islandcount; k++)
            {
                int xsize = Mathf.RoundToInt(Random.Range(islandminsize, islandmaxsize + 1));
                int zsize = Mathf.RoundToInt(Random.Range(islandminsize, islandmaxsize + 1));
                //yield return StartCoroutine(TimeCheck(stopwatch));
                //Debug.Log(islands[k]);
                islands[k] = islands[k] + new Vector3(Mathf.Round(Random.Range(-xsize + 1, 0)), 0, Mathf.Round(Random.Range(-zsize + 1, 0)));
                //Debug.Log(islands[k]);
                for (int x = 0; x < xsize; x++)
                {
                    for (int z = 0; z < zsize; z++)
                    {
                        GameObject blocktype = basecubes[Random.Range(0, basecubes.Length)];
                        Vector3 position = new Vector3(x, 0, z) + islands[k];
                        bool anchored = false;
                        //yield return PlaceTerrain(position, blocktype, anchored, stopwatch);
                        PlaceTerrain(position, Vector3.zero, blocktype, anchored);
                        if ((float)stopwatch.Elapsed.Milliseconds / 1000 > 1 / loadingfps && frameobjects > 0)//next frame
                        {
                            yield return WaitFrame(stopwatch);
                        }
                    }
                }
            }

            //generate ruins on top
            for (int l = 0; l < ruincount; l++)
            {
                //yield return StartCoroutine(TimeCheck(stopwatch));
            }









        }

        */


        islandheight = 20;

        //islandcount = 50;
       // mapsize = 30; //radius

        beaconcount = 8; //not including middle beacon
        beaconspacing = 20;

        //islandminsize = 5;
        //islandmaxsize = 10;

        //ruincount = 2;
        //ruinsize;
        //ruinheight = 3;


        if (universalseed == 0)
        {
            universalseed = Random.Range(-10000, 10000);
        }
        Random.InitState(universalseed);

        terrainseedx = Random.Range(-100000f, 100000f);
        terrainseedz = Random.Range(-100000f, 100000f);

        if (isServer)
        { 

        }

        for (int i = 0; i < beaconcount; i++) //beacons
        {
            Vector3 position;

            if (i == 0)
            {
                position = new Vector3(0, 0, 0);
                CreateStem(0, position);
            }
            position = new Vector3(Mathf.Round(beaconspacing * Mathf.Cos((float)i / ((float)beaconcount) * 2 * Mathf.PI)), 0, Mathf.Round(beaconspacing * Mathf.Sin((float)i / ((float)beaconcount) * 2 * Mathf.PI)));
            CreateStem(0, position);
        }

        for (int j = 0; j < 6; j++) //spawn islands
        {
            Vector3 position = new Vector3(Mathf.Round((mapsize + 5) * 2 * Mathf.Cos((float)j / 6 * 2 * Mathf.PI)), 0, Mathf.Round((mapsize + 5) * 2 * Mathf.Sin((float)j / 6 * 2 * Mathf.PI)));
            //Debug.Log(position);
            CreateStem(1, position);
            CreateStem(2, position + new Vector3(1,0,0));
            CreateStem(3, position + new Vector3(-1, 0, 0));
            CreateStem(3, position + new Vector3(0, 0, 1));
            CreateStem(3, position + new Vector3(0, 0, -1));
            yield return StartCoroutine(FillCircle(position, 5, false, 1));
        }

        yield return StartCoroutine(FillCircle(Vector3.zero, (int)mapsize * 2, true, 2));






        //generatingterrain = false;
        Physics.gravity = Gravity;

        totalobjects += frameobjects;
        AnchorSystem anchormanager = AnchorSystem.AnchorManager;
        List<AnchorSystem.Chunk> unfreezechunks = anchormanager.allchunks;
        for (int n = 0; n < unfreezechunks.Count; n++)
        {
            //Debug.Log(unfreezechunks[n].contents.Count);
            Debug.Log(unfreezechunks[n].anchorID);
            //unfreezechunks[n].physicsproperties.isKinematic = false;
            //unfreezechunks[n].physicsproperties.useGravity = true;
            unfreezechunks[n].anchored = true;
            unfreezechunks[n].anchorsource = true;
            anchormanager.UpdateAnchor(unfreezechunks[n]);
            unfreezechunks[n].physicsproperties.WakeUp();
            //Debug.Log(unfreezechunks[n].physicsproperties.isKinematic);
        }
        Debug.Log("Total placed: " + totalobjects + " in " + string.Format("{0:0}.{1:00}", totalstopwatch.Elapsed.Seconds, totalstopwatch.Elapsed.Milliseconds / 10) + " seconds");
        generatingterrain = false;
        stopwatch.Stop();
    }

    IEnumerator WaitFrame(System.Diagnostics.Stopwatch stopwatch)
    {
        //Debug.Log((float)stopwatch.Elapsed.Milliseconds / 1000);
        Debug.Log("New Frame, " + frameobjects + " placed in: " + (float)stopwatch.Elapsed.Milliseconds / 1000 + " s");
        totalobjects += frameobjects;
        frameobjects = 0;
        stopwatch.Reset();
        yield return null;
        stopwatch.Start();
    }

    void PlaceTerrain(Vector3 location, Vector3 rotation, GameObject blocktype, bool anchored)
    {
        //Debug.Log(stopwatch.Elapsed.Milliseconds);

        int mask2 = (1 << 6) | (1 << 10); //building and ground
        Vector3 worldCenter = location * unitsize;
        Vector3 worldHalfExtents = Vector3.one * unitsize * 0.4f;
        Collider[] overlapcol = Physics.OverlapBox(worldCenter, worldHalfExtents, Quaternion.identity, mask2, QueryTriggerInteraction.Ignore);
        if (overlapcol.Length == 0)
        {
            Vector3 placementposition = location * unitsize;

            GameObject placingblock = Instantiate(blocktype, placementposition, Quaternion.Euler(rotation)); //place tile
            EntityProperties blockproperties = placingblock.GetComponent<EntityProperties>();
            blockproperties.entityID = terrainID;
            GameSystem.GameManager.entityIDs.Add(terrainID, blockproperties);
            AnchorSystem.AnchorManager.CreateObject(blockproperties); //connect to adjacent terrain
            blockproperties.anchordata.anchor = anchored;
            blockproperties.anchordata.chunk.physicsproperties.isKinematic = true;
            //blockproperties.anchordata.chunk.physicsproperties.useGravity = false;
            //AnchorSystem.AnchorManager.UpdateAnchor(blockproperties.anchordata.chunk);

            terrainID++;
            frameobjects++;
        }
    }
    //freeze physics for terrain


    //find where to place it
    //decide what type of tile
    //place it
    //connect to adjacent terrain

    //make pillar groups (islands)
    //platforms on pillars
    //ruins on top

    //unfreeze terrain

    IEnumerator FillCircle(Vector3 position, int size, bool noise, int layers)
    {
        for (int x = -size; x <= size; x++)
        {
            for (int z = -size; z <= size; z++)
            {
                if(x * x + z * z < (size * size))
                {
                    //yield return StartCoroutine(WaitFrame(stopwatch));
                    if ((float)stopwatch.Elapsed.Milliseconds / 1000 > 1 / loadingfps && frameobjects > 0)//next frame
                    {
                        //Debug.Log("wait frame");
                        yield return StartCoroutine(WaitFrame(stopwatch));
                    }
                    Vector3 blockposition = new Vector3(position.x + x, position.y, position.z + z);
                    //GameObject blocktype = basecubes[Random.Range(0, basecubes.Length)];
                    if (noise)
                    {
                        float noisevalue = Mathf.PerlinNoise((x + terrainseedx) / noisescale, (z + terrainseedz) / noisescale);
                        if(.5f + noisethreshold < noisevalue)
                        {
                            if(Mathf.PerlinNoise((x + terrainseedx) * unitsize / spirescale, (z + terrainseedz) * unitsize / spirescale) > .65f)
                            {
                                //make spire
                                Vector3 spireposition = blockposition;
                                for (int y = 1; y <= Random.Range(2,9); y++)
                                {
                                    spireposition += new Vector3(0, 1, 0);
                                    PlaceTerrain(spireposition,Vector3.zero, ruincubes[Random.Range(0, ruincubes.Length)], false);
                                }

                            }
                            if (Mathf.PerlinNoise((x + terrainseedx) / stemscale, (z + terrainseedz) / stemscale) > .20f)
                            {
                                for (int l = 0; l < layers; l++)
                                {                                   
                                    PlaceTerrain(blockposition - new Vector3(0,l,0), new Vector3(0, 90 * Random.Range(0, 4), 0), basecubes[Random.Range(0, basecubes.Length)], false);
                                }
                                if (0.3f - noisethreshold < noisevalue && noisevalue < 0.5f + noisethreshold)
                                {
                                    //not grass
                                    continue;
                                }
                                else
                                {
                                    //grass
                                }
                            }
                            else
                            {
                                float stemtype = Random.Range(0, 100f);
                                if(stemtype < 45f) //pump
                                {
                                    CreateStem(1, blockposition);
                                }
                                else if(stemtype < 70f) //ladder
                                {
                                    CreateStem(2, blockposition);
                                }
                                else //other
                                {
                                    CreateStem(3, blockposition);
                                }
                            }
                        }
                    }
                    else
                    {
                        PlaceTerrain(blockposition, new Vector3(0, 90 * Random.Range(0, 4), 0), basecubes[Random.Range(0, basecubes.Length)], false);
                    }
                }
            }
        }
        yield return null;
    }

    void CreateStem(int type, Vector3 position)
    {
        Vector3 rotation = Vector3.zero;
        if(type == 0) //beacon
        {
            PlaceTerrain(position, Vector3.zero, beaconcube, true);
        }
        else if(type == 1) //pump
        {
            PlaceTerrain(position, Vector3.zero, pumpcube, false);
        }
        else if (type == 2) //ladder
        {
            rotation = new Vector3(0, 90 * Random.Range(0, 4), 0);
            PlaceTerrain(position, rotation, laddercube, false);
        }
        else //other
        {
            PlaceTerrain(position, rotation, basecubes[Random.Range(0, basecubes.Length)], false);
            /*type = Random.Range(2, 5);// 33%
            if(type == 2) //ladder
            {
                rotation = new Vector3(0, 90 * Random.Range(0, 4), 0);
            }*/
        }

        for (int h = 1; h < islandheight; h++)
        {
            bool anchored;
            GameObject blocktype;

            if(type == 0 || type == 1) //blocktype
            {
                blocktype = pumpcube;
            }
            else if(type == 2)
            {
                blocktype = laddercube;
            }
            else
            {
                blocktype = basecubes[Random.Range(0, basecubes.Length)];
            }

            if(type != 2)//if not ladder
            {
                rotation = new Vector3(0, 90 * Random.Range(0, 4), 0);
            }

            position = new Vector3(position.x, h - islandheight, position.z);
            if (h == 1)
            {
                anchored = true;
            }
            else
            {
                anchored = false;
            }
            PlaceTerrain(position, rotation, blocktype, anchored);
        }
    }

   /* public struct GenerationSeedMessage : NetworkMessage
    {
        public int serverseed;
    }*/
}
