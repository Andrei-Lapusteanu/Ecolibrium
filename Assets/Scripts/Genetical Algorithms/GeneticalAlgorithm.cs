using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GeneticalAlgorithm<T>
{
    private static T parent_1;
    private static T parent_2;
    private static AnimalController child;
    private static AnimalSpawner animalSpawner;

    private static int mutationChance = 5;         // %
    private static float minNegativeMutationOffset = 0.5f;
    private static float maxNegativeMutationOffset = 0.8f;
    private static float minPositiveMutationOffset = 1.2f;
    private static float maxPositiveMutationOffset = 1.5f;

    public static void GenerateOffspring(T p1, T p2)
    {
        parent_1 = p1;
        parent_2 = p2;
        animalSpawner = GameObject.Find("AnimalSpawnerController").GetComponent<AnimalSpawner>();

        if ((parent_1 as AnimalController).Type == AnimalType.Rabbit)
            child = new Rabbit();
        else
            child = new Wolf();

        List<KeyValuePair<float, AttribType>> p1_attribs = (parent_1 as AnimalController).GetPackedAttribs();
        List<KeyValuePair<float, AttribType>> p2_attribs = (parent_2 as AnimalController).GetPackedAttribs();

        // Crossover
        for(int i = 0; i < p1_attribs.Count; i++)
        {
            bool shouldMutate = Crossover(p1_attribs[i].Key, p2_attribs[i].Key, p1_attribs[i].Value);

            // Mutation
            if (shouldMutate == true)
                Mutate(p1_attribs[i].Value);
        }

        // Spawn cub
        if ((parent_1 as AnimalController).Gender == Gender.Female)
            animalSpawner.BirthCub(child, (parent_1 as AnimalController).transform, (parent_1 as AnimalController).Type);
        else
            animalSpawner.BirthCub(child, (parent_2 as AnimalController).transform, (parent_2 as AnimalController).Type);
    }

    private static bool Crossover(float attrib_p1, float attrib_p2, AttribType attribType)
    {
        // Get a random weight (how much each parent influences child's gene value
        float weight = UnityEngine.Random.Range(0.0f, 1.0f);

        // Apply crossover
        float attrib_child = attrib_p1 * weight + attrib_p2 * (1 - weight);

        // Apply new value to child
        switch (attribType)
        {
            case AttribType.MaxHP:
                child.MaxHealthPoints = attrib_child;
                break;

            case AttribType.MaxSatiety:
                child.MaxSatiety = attrib_child;
                break;

            case AttribType.MaxEnergy:
                child.MaxEnergy = attrib_child;
                break;

            case AttribType.MaxSpeed:
                child.MaxSpeed = attrib_child;
                break;

            case AttribType.MaxAge:
                child.MaxAge = attrib_child;
                break;

            case AttribType.MaxFoodSight:
                child.MaxFoodSight = attrib_child;
                break;
        }

        // Should mutate?
        return UnityEngine.Random.Range(0, 100) < mutationChance ? true : false ;
    }

    private static void Mutate(AttribType attribType)
    {
        float mutationOffset = 0;

        // Calculate mutation outcome
        int randResult = UnityEngine.Random.Range(0, 2);
        bool mutationOutcome = (randResult == 0) ? true : false;

        if (mutationOutcome == true)
            // Positive mutation outcome yields positive mutation offset
            mutationOffset = UnityEngine.Random.Range(minPositiveMutationOffset, maxPositiveMutationOffset);
        else
            // Negative mutation outcome yields negative mutation offset
            mutationOffset = UnityEngine.Random.Range(minNegativeMutationOffset, maxNegativeMutationOffset);

        // Apply new value to child
        switch (attribType)
        {
            case AttribType.MaxHP:
                // Attenuate mutation offset for HP (if not, it will cause imbalance after may generations)
                // Because HP has a high base value, it influences in a big proportion the fitness calculation
                mutationOffset = (mutationOffset + 1f) / 2f;

                child.MaxHealthPoints *= mutationOffset;
                break;

            case AttribType.MaxSatiety:
                child.MaxSatiety *= mutationOffset;
                break;

            case AttribType.MaxEnergy:
                child.MaxEnergy *= mutationOffset;
                break;

            case AttribType.MaxSpeed:
                child.MaxSpeed *= mutationOffset;
                break;

            case AttribType.MaxAge:
                child.MaxAge *= mutationOffset;
                break;

            case AttribType.MaxFoodSight:
                // Attenuate mutation offset for food sight (if not, it will cause imbalance after may generations)
                // Because food sight has a high base value, it influences in a big proportion the fitness calculation
                mutationOffset = (mutationOffset + 1f) / 2f;

                child.MaxFoodSight *= mutationOffset;
                break;
        }
    }
}
