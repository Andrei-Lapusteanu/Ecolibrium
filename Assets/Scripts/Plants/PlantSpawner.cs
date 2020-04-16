using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantSpawner : MonoBehaviour
{
    private const string GRASS_PATH = "Plants/Grass";
    private const string PLANTAIN_PATH = "Plants/Plantain";
    private const string DANDELION_PATH = "Plants/Dandelion";

    private const int MIN_PLANT_GROUPS = 75;
    private const int MAX_PLANT_GROUPS = 100;

    private const int MIN_PLANTS_PER_GROUP = 3;
    private const int MAX_PLANTS_PER_GROUP = 10;

    private const float GROUP_OFFSET = 8.0f;
    private const float PLANT_OFFSET = 3.0f;

    List<GameObject> plants;
    List<Vector3> plantGroupPos = new List<Vector3>();

    GameObject grass;
    GameObject plantain;
    GameObject dandelion;

    public void LoadPlantPrefabs()
    {
        grass = Resources.Load(GRASS_PATH) as GameObject;
        plantain = Resources.Load(PLANTAIN_PATH) as GameObject;
        dandelion = Resources.Load(DANDELION_PATH) as GameObject;

        plants = new List<GameObject>() { grass, plantain, dandelion };
    }

    public Vector3 GeneratePlantGroupPos()
    {
        return new Vector3(
            Random.Range(-WorldLimits.WORLD_LIMIT_X + GROUP_OFFSET, WorldLimits.WORLD_LIMIT_X - GROUP_OFFSET),
            0,
            Random.Range(-WorldLimits.WORLD_LIMIT_Z + GROUP_OFFSET, WorldLimits.WORLD_LIMIT_Z - GROUP_OFFSET));
    }

    public void GenerateGroups()
    {
        int plantsGroups = Random.Range(MIN_PLANT_GROUPS, MAX_PLANT_GROUPS + 1);

        for (int i = 0; i < plantsGroups; i++)
            plantGroupPos.Add(GeneratePlantGroupPos());
    }

    public Vector3 GenerateGroupPosOffset()
    {
        return new Vector3(
            Random.Range(-PLANT_OFFSET, PLANT_OFFSET),
            0,
            Random.Range(-PLANT_OFFSET, PLANT_OFFSET));
    }

    public void GeneratePlants()
    {
        for (int i = 0; i < plantGroupPos.Count; i++)
        {
            int plantsPerGroup = Random.Range(MIN_PLANTS_PER_GROUP, MAX_PLANTS_PER_GROUP + 1);
        
            for(int j = 0; j < plantsPerGroup; j++)
                Instantiate(plants[Random.Range(0, plants.Count)], plantGroupPos[i] + GenerateGroupPosOffset(), Quaternion.identity);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        LoadPlantPrefabs();
        GenerateGroups();
        GeneratePlants();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
