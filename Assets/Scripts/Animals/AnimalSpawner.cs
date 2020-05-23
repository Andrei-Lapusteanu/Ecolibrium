using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalSpawner : MonoBehaviour
{
    private const string ANIMALS_PATH = "Animals/";

    private const string RABBIT_MALE = "Rabbits/Prefabs/Rabbit_male";
    private const string RABBIT_FEMALE = "Rabbits/Prefabs/Rabbit_female";
    private const string RABBIT_CUB = "Rabbits/Prefabs/Rabbit_cub";
    private const string WOLF_MALE = "Wolf/Wolf_male";
    private const string WOLF_FEMALE = "Wolf/Wolf_female";
    private const string WOLF_CUB = "Wolf/Wolf_cub";

    private const int MIN_RABBIT_GROUPS = 12;
    private const int MAX_RABBIT_GROUPS = 15;
    private const int MIN_WOLF_GROUPS = 7;
    private const int MAX_WOLF_GROUPS = 8;

    private const int MIN_RABBITS_PER_GROUP = 7;
    private const int MAX_RABBITS_PER_GROUP = 9;
    private const int MIN_WOLVES_PER_GROUP = 4;
    private const int MAX_WOLVES_PER_GROUP = 5;

    private const float POSITION_OFFSET = 2.0f;

    List<Vector3> rabbitGroupPos = new List<Vector3>();
    List<Vector3> wolfGroupPos = new List<Vector3>();

    GameObject rabbit_male;
    GameObject rabbit_female;
    GameObject rabbit_cub;
    GameObject wolf_male;
    GameObject wolf_female;
    GameObject wolf_cub;


    // Start is called before the first frame update
    void Start()
    {
        // TODO
        LoadAnimalPrefabs();
        GenerateGroups();
        GenerateAnimals();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void LoadAnimalPrefabs()
    {
        rabbit_male = Resources.Load(ANIMALS_PATH + RABBIT_MALE) as GameObject;
        rabbit_female = Resources.Load(ANIMALS_PATH + RABBIT_FEMALE) as GameObject;
        rabbit_cub = Resources.Load(ANIMALS_PATH + RABBIT_CUB) as GameObject;
        wolf_male = Resources.Load(ANIMALS_PATH + WOLF_MALE) as GameObject;
        wolf_female = Resources.Load(ANIMALS_PATH + WOLF_FEMALE) as GameObject;
        wolf_cub = Resources.Load(ANIMALS_PATH + WOLF_CUB) as GameObject;
    }

    public Vector3 GenerateRabbitGroupPos()
    {
        //return new Vector3(
        //    Random.Range(-WorldLimits.WORLD_LIMIT_X + POSITION_OFFSET, 0),
        //    0,
        //    Random.Range(-WorldLimits.WORLD_LIMIT_Z + POSITION_OFFSET, WorldLimits.WORLD_LIMIT_Z - POSITION_OFFSET));\

        return new Vector3(
              Random.Range(-WorldLimits.WORLD_LIMIT_X + POSITION_OFFSET, WorldLimits.WORLD_LIMIT_X - POSITION_OFFSET),
              0,
              Random.Range(-WorldLimits.WORLD_LIMIT_Z + POSITION_OFFSET, WorldLimits.WORLD_LIMIT_Z - POSITION_OFFSET));
    }
    public Vector3 GenerateWolfGroupPos()
    {
        //return new Vector3(
        //    Random.Range(0, WorldLimits.WORLD_LIMIT_X - POSITION_OFFSET),
        //    0,
        //    Random.Range(-WorldLimits.WORLD_LIMIT_Z + POSITION_OFFSET, WorldLimits.WORLD_LIMIT_Z - POSITION_OFFSET));

        return new Vector3(
            Random.Range(-WorldLimits.WORLD_LIMIT_X + POSITION_OFFSET, WorldLimits.WORLD_LIMIT_X - POSITION_OFFSET),
            0,
            Random.Range(-WorldLimits.WORLD_LIMIT_Z + POSITION_OFFSET, WorldLimits.WORLD_LIMIT_Z - POSITION_OFFSET));
    }

    public void GenerateGroups()
    {
        int rabbitGroups = Random.Range(MIN_RABBIT_GROUPS, MAX_RABBIT_GROUPS + 1);
        int wolfGroups = Random.Range(MIN_WOLF_GROUPS, MAX_WOLF_GROUPS + 1);

        for (int i = 0; i < rabbitGroups; i++)
            rabbitGroupPos.Add(GenerateRabbitGroupPos());

        for (int i = 0; i < wolfGroups; i++)
            wolfGroupPos.Add(GenerateWolfGroupPos());
    }

    public Vector3 GenerateGroupPosOffset()
    {
        return new Vector3(
            Random.Range(-1f, 1f),
            0,
            Random.Range(-1f, 1f));
    }

    public Gender GenerateGender()
    {
        int gender = Random.Range(0, 2);

        if (gender == 0)
            return Gender.Male;
        else return Gender.Female;
    }

    public void GenerateAnimals()
    {
        //int wolfCounter = 0, rabbitCounter = 0;
        for (int i = 0; i < wolfGroupPos.Count; i++)
        {
            int wolvesPerGroup = Random.Range(MIN_WOLVES_PER_GROUP, MAX_WOLVES_PER_GROUP + 1);

            for (int j = 0; j < wolvesPerGroup; j++)
            {
                Gender thisGender = GenerateGender();
                if (thisGender == Gender.Male)
                {
                    GameObject wolfMaleInst = Instantiate(wolf_male, wolfGroupPos[i] + GenerateGroupPosOffset(), Quaternion.identity);
                    wolfMaleInst.GetComponent<Wolf>().CreateAnimalRandAttributes(AnimalType.Wolf, thisGender, AnimalCounter.WolvesCounter++);
                }
                else
                {
                    GameObject wolfFemaleInst = Instantiate(wolf_female, wolfGroupPos[i] + GenerateGroupPosOffset(), Quaternion.identity);
                    wolfFemaleInst.GetComponent<Wolf>().CreateAnimalRandAttributes(AnimalType.Wolf, thisGender, AnimalCounter.WolvesCounter++);
                }
            } 
        }

        for (int i = 0; i < rabbitGroupPos.Count; i++)
        {
            int rabbitsPerGroup = Random.Range(MIN_RABBITS_PER_GROUP, MAX_RABBITS_PER_GROUP + 1);

            for (int j = 0; j < rabbitsPerGroup; j++)
            {
                Gender thisGender = GenerateGender();
                if (thisGender == Gender.Male)
                {
                    GameObject rabbitMaleInst = Instantiate(rabbit_male, rabbitGroupPos[i] + GenerateGroupPosOffset(), Quaternion.identity);
                    rabbitMaleInst.GetComponent<Rabbit>().CreateAnimalRandAttributes(AnimalType.Rabbit, thisGender, AnimalCounter.RabbitCounter++);
                }
                else
                {
                    GameObject rabbitFemaleInst = Instantiate(rabbit_female, rabbitGroupPos[i] + GenerateGroupPosOffset(), Quaternion.identity);
                    rabbitFemaleInst.GetComponent<Rabbit>().CreateAnimalRandAttributes(AnimalType.Rabbit, thisGender, AnimalCounter.RabbitCounter++);
                }
            }
        }
    }

    public void BirthCub(AnimalController child, Transform mother, AnimalType animalType)
    {
        switch (animalType)
        {
            case AnimalType.Rabbit:
                GameObject childInst = Instantiate(rabbit_cub, mother.position, Quaternion.identity);
                childInst.GetComponent<Rabbit>().SetNewAnimalAttributes(child, GenerateGender());
                childInst.GetComponent<Rabbit>().PlaySpawnEffect();
                AnimalCounter.RabbitCounter++;
                break;

            case AnimalType.Wolf:
                childInst = Instantiate(wolf_cub, mother.position, Quaternion.identity);
                childInst.GetComponent<Wolf>().SetNewAnimalAttributes(child, GenerateGender());
                childInst.GetComponent<Wolf>().PlaySpawnEffect();
                AnimalCounter.WolvesCounter++;
                break;
        }

        // TODO testing
        //rabbit_male.GetComponent<Rabbit>().testing = false;
        //rabbit_female.GetComponent<Rabbit>().testing = false;
        //wolf_male.GetComponent<Wolf>().testing = false;
        //wolf_female.GetComponent<Wolf>().testing = false;

    }

    // TODO - testing
    private void OnDestroy()
    {
        //rabbit_male.GetComponent<Rabbit>().testing = true;
        //rabbit_female.GetComponent<Rabbit>().testing = true;
        //wolf_male.GetComponent<Wolf>().testing = true;
        //wolf_female.GetComponent<Wolf>().testing = true;
    }

    public void GrowUpAnimal(AnimalController child)
    {
        switch(child.Species)
        {
            case AnimalType.Rabbit:

                if(child.Gender == Gender.Male)
                {
                    GameObject rabbitMaleInst = Instantiate(rabbit_male, child.transform.position, Quaternion.identity);
                    rabbitMaleInst.GetComponent<Rabbit>().SetGrownUpAnimalAttributes(child, child.Gender);
                    AnimalCounter.RabbitCounter++;
                }
                else
                {
                    GameObject rabbitFemaleInst = Instantiate(rabbit_female, child.transform.position, Quaternion.identity);
                    rabbitFemaleInst.GetComponent<Rabbit>().SetGrownUpAnimalAttributes(child, child.Gender);
                    AnimalCounter.RabbitCounter++;
                }

                break;

            case AnimalType.Wolf:

                if (child.Gender == Gender.Male)
                {
                    GameObject wolfMaleInst = Instantiate(wolf_male, child.transform.position, Quaternion.identity);
                    wolfMaleInst.GetComponent<Wolf>().SetGrownUpAnimalAttributes(child, child.Gender);
                    AnimalCounter.WolvesCounter++;
                }
                else
                {
                    GameObject wolfFemaleInst = Instantiate(wolf_female, child.transform.position, Quaternion.identity);
                    wolfFemaleInst.GetComponent<Wolf>().SetGrownUpAnimalAttributes(child, child.Gender);
                    AnimalCounter.WolvesCounter++;
                }
                break;
        }
    }
}
