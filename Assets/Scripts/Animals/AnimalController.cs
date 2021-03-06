﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public enum Gender
{
    Male, Female
}

public enum AnimalType
{
    Rabbit, Wolf
}

public enum StatusType
{
    HP, Satiety, Energy, Speed, Age, Reproduce, FoodSight
}

public enum HungerState
{
    NotHungry, Hungry, VeryHungry
}

public enum TirednessState
{
    NotTired, Tired, VeryTired
}

public enum ReproduceState
{
    ReproduceFalse, ReproduceTrue
}

public enum AttribType
{
    MaxHP, MaxSatiety, MaxEnergy, MaxSpeed, MaxAge, MaxFoodSight
}

public enum AdultState
{
    Adult, NotAdult
}

public enum AnimalStates
{
    STATE_IDLE,                          // 0
    STATE_WALK,                          // 1
    STATE_RUN,                           // 2
    RABBIT_STATE_FLEE_FROM_PREDATOR,     // 3
    STATE_LOOK_FOR_FOOD,                 // 4
    STATE_GO_AFTER_FOOD,                 // 5
    STATE_EAT,                           // 6
    STATE_LOOK_FOR_MATE,                 // 7
    STATE_GO_AFTER_MATE,                 // 8
    STATE_BREED,                         // 9
    STATE_SLEEP,                         // 10
    STATE_DIE,                           // 11
    WOLF_STATE_SIT_DOWN,                 // 12
    WOLF_STATE_CREEP                     // 13
}

public abstract class AnimalController : MonoBehaviour
{
    //HP
    private const float BASE_HP_WOLF = 80f;
    private const float BASE_HP_OFFSET_WOLF = 10f;
    private const float BASE_HP_RABBIT = 40f;
    private const float BASE_HP_OFFSET_RABBIT = 5f;

    // Satiety
    private const float BASE_SATIETY_WOLF = 50f;
    private const float BASE_SATIETY_OFFSET_WOLF = 5f;
    private const float BASE_SATIETY_RABBIT = 30f;
    private const float BASE_SATIETY_OFFSET_RABBIT = 2.5f;

    // Energy
    private const float BASE_ENERGY_WOLF = 100f;
    private const float BASE_ENERGY_OFFSET_WOLF = 10f;
    private const float BASE_ENERGY_RABBIT = 30f;
    private const float BASE_ENERGY_OFFSET_RABBIT = 2.5f;

    // Speed
    private const float BASE_SPEED_WOLF = 7.75f;
    private const float BASE_SPEED_RABBIT = 6f;
    private const float BASE_SPEED_OFFSET_WOLF = 0.25f;
    private const float BASE_SPEED_OFFSET_RABBIT = 0.25f;

    // Age
    private const float MAX_AGE_WOLF = 7f;
    private const float MAX_AGE_RABBIT = 3f;
    private const float GROW_UP_AGE_WOLF = 1.7f;
    private const float GROW_UP_AGE_RABBIT = 1f;

    // Reproduce desire
    private const float BASE_REPRODUCE = 1f;

    // Reproduce threshold
    private const float BASE_REPRODUCE_THRESH = 0.7f;

    // Reproduse speed
    public static float BASE_REPRODUCE_SPEED_RABBIT = 0.05f;
    public static float BASE_REPRODUCE_SPEED_WOLF = 0.035f;

    // Reproduce offspring count
    private const int MIN_OFFSPRING_COUNT_WOLF = 2;
    private const int MAX_OFFSPRING_COUNT_WOLF = 3;
    private const int MIN_OFFSPRING_COUNT_RABBIT = 3;
    private const int MAX_OFFSPRING_COUNT_RABBIT = 5;

    // Food sight distance
    private const float BASE_FOOD_SIGHT_WOLF = 25f;
    private const float BASE_FOOD_SIGHT_RABBIT = 30f;
    private const float BASE_FOOD_SIGHT_OFFSET_WOLF = 5f;
    private const float BASE_FOOD_SIGHT_OFFSET_RABBIT = 5f;

    private Gender gender;
    private float healthPoints;
    private float age;
    private float satiety;
    private float energy;
    private float speed;
    private float reproduce;
    private float reproduceSpeed;
    private float foodSight;
    private float reproduceThreshold;
    private float fitness;
    private AnimalType type;
    private int id;

    private float maxHealthPoints;
    private float maxAge;
    private float maxSatiety;
    private float maxEnergy;
    private float maxSpeed;
    private float maxReproduce;
    private float maxFoodSight;

    protected float ageModifyRate = 0.0025f * 20;
    protected float satietyModifyRate_Rabbit = 0.05f * 20;
    protected float satietyModifyRate_Wolf = 0.075f * 20;
    protected float energyModifyRate = 0.025f * 20;
    protected float hpModifyRate = 0.1f * 20;

    private float statsPanelUpdateRate = 0.25f;
    private float nextPanelUpdate = 0.0f;

    protected GameObject foodTarget = null;
    protected GameObject mateTarget = null;
    protected GameObject predatorTarget = null;               // Rabbit only

    private Coroutine lookForFoodRoutine = null;
    private Coroutine isFoodInRangeRoutine = null;
    private Coroutine goAfterFoodRoutine = null;
    private Coroutine lookForMateRoutine = null;
    private Coroutine isMateInRangeRoutine = null;
    private Coroutine goAfterMateRoutine = null;
    private Coroutine fleeFromPredatorRoutine = null;       // Rabbit only

    private bool isScanningForFood = false;
    private bool isLookingForFood = false;
    private bool isGoingAfterFood = false;
    private bool isLookingForMate = false;
    private bool isScanningForMate = false;
    private bool isGoingAfterMate = false;
    private bool isFleeingFromPredator = false;             // Rabbit inly
    protected bool isSleeping = false;
    private bool isAlive = true;
    private bool hasLostFood = false;
    private bool hasLostMate = false;

    protected abstract IEnumerator MainAI();

    protected virtual void Idle(NavMeshAgent agent, bool isStopped, Animator anim, string animVarName, int state)
    {
        NavAgentCommand(agent, true, agent.speed, anim, animVarName, state);

        Debug.DrawLine(transform.position + Vector3.up, agent.destination, Color.green, 1);
    }

    protected virtual void Walk(NavMeshAgent agent, bool isStopped, Animator anim, string animVarName, int state)
    {
        NavAgentCommand(agent, false, GetWalkSpeed(), anim, animVarName, state);

        WalkToRandPos(agent, GetWalkSpeed(), 20.0f);

        Debug.DrawLine(transform.position + Vector3.up, agent.destination, Color.green, 1);
    }

    protected virtual void Run(NavMeshAgent agent, bool isStopped, Animator anim, string animVarName, int state)
    {
        NavAgentCommand(agent, false, Speed, anim, animVarName, state);

        WalkToRandPos(agent, Speed, 20.0f);

        Debug.DrawLine(transform.position + Vector3.up, agent.destination, Color.green, 1);
    }

    protected virtual void Eat(Action<int> currentStateCallback, int stateToBeSet, float satietyIncrease, string targetTag)
    {
        if (foodTarget != null)
        {
            // If rabbit
            if (Species == AnimalType.Rabbit && IsAdult() == AdultState.NotAdult)
                satietyIncrease /= 2;

            if (targetTag == Tags.Wolf)
            {
                AnimalCounter.RabbitDeathsTotal++;

                if (foodTarget.GetComponent<Rabbit>().IsAlive == true)
                    AnimalCounter.RabbitDeathsByBeingEaten++;

                Destroy(foodTarget, 0f);
            }
            else if (targetTag == Tags.Rabbit)
                foodTarget.GetComponent<PlantGrowController>().GetEaten(IsAdult());

            foodTarget = null;

            if (Satiety + satietyIncrease > maxSatiety)
                Satiety = MaxSatiety;
            else
                Satiety += satietyIncrease;
        }

        currentStateCallback(stateToBeSet);
    }

    protected virtual void Sleep(NavMeshAgent agent, bool isStopped, Animator anim, string animVarName, int state)
    {
        isSleeping = true;
        NavAgentCommand(agent, true, agent.speed, anim, animVarName, state);
    }

    protected virtual void Breed(Action<int> currentStateCallback)
    {
        // If target mate (partner) wishes to reproduce
        if (mateTarget != null && mateTarget.GetComponent<AnimalController>().GetReproduceState() == ReproduceState.ReproduceTrue)
        {
            // Reset parents reproduce desires
            this.Reproduce = 0f;
            mateTarget.GetComponent<AnimalController>().Reproduce = 0f;

            // If this animal is a rabbit
            if (Species == AnimalType.Rabbit)
            {
                int cubsCount = Random.Range(MIN_OFFSPRING_COUNT_RABBIT, MAX_OFFSPRING_COUNT_RABBIT + 1);

                // Run genetical algorithm for each offspring 
                for (int i = 0; i < cubsCount; i++)
                    GeneticalAlgorithm<Rabbit>.GenerateOffspring(GetComponent<Rabbit>(), mateTarget.GetComponent<Rabbit>());
            }

            // If this animal is a wolf
            else if (Species == AnimalType.Wolf)
            {
                int cubsCount = Random.Range(MIN_OFFSPRING_COUNT_WOLF, MAX_OFFSPRING_COUNT_WOLF + 1);

                // Run genetical algorithm for each offspring 
                for (int i = 0; i < cubsCount; i++)
                    GeneticalAlgorithm<Wolf>.GenerateOffspring(GetComponent<Wolf>(), mateTarget.GetComponent<Wolf>());
            }
        }

        currentStateCallback((int)AnimalStates.STATE_IDLE);
    }

    protected abstract void Die();

    protected virtual void StateLookForFood(Action<int> currentStateCallback, NavMeshAgent agent, Animator anim, string animVarName, string targetTag)
    {

        if (isLookingForFood == false)
        {
            if (Species == AnimalType.Wolf)
                lookForFoodRoutine = StartCoroutine(TryLookingForFood(agent, anim, animVarName, (int)AnimalStates.STATE_WALK));
            else if (Species == AnimalType.Rabbit)
                lookForFoodRoutine = StartCoroutine(TryLookingForFood(agent, anim, animVarName, (int)AnimalStates.STATE_WALK));

            isLookingForFood = true;
        }

        if (isScanningForFood == false)
        {
            isFoodInRangeRoutine = StartCoroutine(IsFoodInRange(targetTag));
            isScanningForFood = true;
        }

        if (IsTired() == TirednessState.VeryTired && IsHungry() != HungerState.VeryHungry)
        {
            currentStateCallback((int)AnimalStates.STATE_SLEEP);
            return;
        }

        else if (foodTarget != null)
            currentStateCallback((int)AnimalStates.STATE_GO_AFTER_FOOD);
    }

    protected virtual void StateGoAfterFood(Action<int> currentStateCallback, NavMeshAgent agent, Animator anim, string animVarName)
    {
        if (IsTired() == TirednessState.VeryTired && IsHungry() != HungerState.VeryHungry)
        {
            currentStateCallback((int)AnimalStates.STATE_SLEEP);
            return;
        }

        if (isGoingAfterFood == false)
        {
            goAfterFoodRoutine = StartCoroutine(GoAfterFood(agent, anim, animVarName, (int)AnimalStates.STATE_RUN));
            isGoingAfterFood = true;
        }

        if (foodTarget != null)
        {
            if (Species == AnimalType.Rabbit)
            {
                if (foodTarget.GetComponent<PlantGrowController>().IsEatable == true)
                {
                    if (CanEatFood(foodTarget, transform, 2f) == true)
                        currentStateCallback((int)AnimalStates.STATE_EAT);
                }
            }
            else if (Species == AnimalType.Wolf)
            {
                if (CanEatFood(foodTarget, transform, 4f) == true)
                    currentStateCallback((int)AnimalStates.STATE_EAT);
            }
            else foodTarget = null;
        }

        else if (foodTarget == null)
            currentStateCallback((int)AnimalStates.STATE_LOOK_FOR_FOOD);

        if (hasLostFood == true)
            currentStateCallback((int)AnimalStates.STATE_LOOK_FOR_FOOD);
    }

    protected virtual void StateLookForMate(Action<int> currentStateCallback, NavMeshAgent agent, Animator anim, string animVarName)
    {
        if (isLookingForMate == false)
        {
            lookForMateRoutine = StartCoroutine(TryLookingForMate(agent, anim, animVarName, (int)AnimalStates.STATE_RUN));
            isLookingForMate = true;
        }

        if (isScanningForMate == false)
        {
            // TODO simply with tostring()?
            if (Species == AnimalType.Rabbit)
                isMateInRangeRoutine = StartCoroutine(IsMateInRange());
            else if (Species == AnimalType.Wolf)
                isMateInRangeRoutine = StartCoroutine(IsMateInRange());

            isScanningForMate = true;
        }

        if (IsTired() == TirednessState.VeryTired && IsHungry() != HungerState.VeryHungry)
        {
            currentStateCallback((int)AnimalStates.STATE_RUN);
            return;
        }

        else if (mateTarget != null)
            currentStateCallback((int)AnimalStates.STATE_GO_AFTER_MATE);
    }

    protected virtual void StateGoAfterMate(Action<int> currentStateCallback, NavMeshAgent agent, Animator anim, string animVarName)
    {
        if (IsTired() == TirednessState.VeryTired && IsHungry() != HungerState.VeryHungry)
        {
            currentStateCallback((int)AnimalStates.STATE_SLEEP);
            return;
        }

        if (isGoingAfterMate == false)
        {
            goAfterMateRoutine = StartCoroutine(GoAfterMate(agent, anim, animVarName, (int)AnimalStates.STATE_RUN));
            isGoingAfterMate = true;
        }

        // TODO - need to check if it works
        if (mateTarget != null)
        {
            if (mateTarget.GetComponent<AnimalController>().GetReproduceState() == ReproduceState.ReproduceTrue)
            {
                if (Species == AnimalType.Rabbit)
                {
                    if (CanBreed(mateTarget, transform, 2.5f) == true)
                        currentStateCallback((int)AnimalStates.STATE_BREED);
                }
                else if (Species == AnimalType.Wolf)
                {
                    if (CanBreed(mateTarget, transform, 5f) == true)
                        currentStateCallback((int)AnimalStates.STATE_BREED);
                }
            }
            else mateTarget = null;
        }

        else if (mateTarget == null)
            currentStateCallback((int)AnimalStates.STATE_IDLE);

        if (hasLostMate == true)
            currentStateCallback((int)AnimalStates.STATE_IDLE);
    }

    protected virtual void StateFleeFromPredator(Action<int> currentStateCallback, NavMeshAgent agent, Animator anim)
    {
        if (isFleeingFromPredator == false)
            fleeFromPredatorRoutine = StartCoroutine(FleeFromPredator(agent, anim));

        if (predatorTarget == null)
            currentStateCallback((int)AnimalStates.STATE_IDLE);

    }

    protected void DisableLookForFoodRoutine()
    {
        if (lookForFoodRoutine != null)
        {
            isLookingForFood = false;
            StopCoroutine(lookForFoodRoutine);
        }
    }

    protected void DisableGoAfterFoodRoutine()
    {
        if (goAfterFoodRoutine != null)
        {
            isGoingAfterFood = false;
            hasLostFood = true;
            StopCoroutine(goAfterFoodRoutine);
        }
    }

    protected void DisableIsFoodInRangeRoutine()
    {
        if (isFoodInRangeRoutine != null)
        {
            isScanningForFood = false;
            StopCoroutine(isFoodInRangeRoutine);
        }
    }

    protected void DisableLookForMateRoutine()
    {
        if (lookForMateRoutine != null)
        {
            isLookingForMate = false;
            StopCoroutine(lookForMateRoutine);
        }
    }

    protected void DisableGoAfterMateRoutine()
    {
        if (goAfterMateRoutine != null)
        {
            isGoingAfterMate = false;
            hasLostMate = true;
            StopCoroutine(goAfterMateRoutine);
        }
    }

    protected void DisableIsMateInRangeRoutine()
    {
        if (isMateInRangeRoutine != null)
        {
            isScanningForMate = false;
            StopCoroutine(isMateInRangeRoutine);
        }
    }

    protected void DisableFleeingFromPredatorRoutine()
    {
        if (fleeFromPredatorRoutine != null)
        {
            isFleeingFromPredator = false;
            StopCoroutine(fleeFromPredatorRoutine);
        }
    }

    public void SetAttributesLimits(AnimalType animalType)
    {
        switch (animalType)
        {
            case AnimalType.Rabbit:
                this.maxHealthPoints = Random.Range
                    (BASE_HP_RABBIT - BASE_HP_OFFSET_RABBIT, BASE_HP_RABBIT + BASE_HP_OFFSET_RABBIT);

                this.maxAge = MAX_AGE_RABBIT;

                this.maxSatiety = Random.Range
                    (BASE_SATIETY_RABBIT - BASE_SATIETY_OFFSET_RABBIT, BASE_SATIETY_RABBIT + BASE_SATIETY_OFFSET_RABBIT);

                this.maxEnergy = Random.Range
                    (BASE_ENERGY_RABBIT - BASE_ENERGY_OFFSET_RABBIT, BASE_ENERGY_RABBIT + BASE_ENERGY_OFFSET_RABBIT);

                this.maxSpeed = Random.Range
                    (BASE_SPEED_RABBIT - BASE_SPEED_OFFSET_RABBIT, BASE_SPEED_RABBIT + BASE_SPEED_OFFSET_RABBIT);

                this.maxReproduce = BASE_REPRODUCE;

                this.maxFoodSight = Random.Range
                    (BASE_FOOD_SIGHT_RABBIT - BASE_FOOD_SIGHT_OFFSET_RABBIT, BASE_FOOD_SIGHT_RABBIT + BASE_FOOD_SIGHT_OFFSET_RABBIT);

                break;

            case AnimalType.Wolf:
                this.maxHealthPoints = Random.Range
                    (BASE_HP_WOLF - BASE_HP_OFFSET_WOLF, BASE_HP_WOLF + BASE_HP_OFFSET_WOLF);

                this.maxAge = MAX_AGE_WOLF;

                this.maxSatiety = Random.Range
                    (BASE_SATIETY_WOLF - BASE_SATIETY_OFFSET_WOLF, BASE_SATIETY_WOLF + BASE_SATIETY_OFFSET_WOLF);

                this.maxEnergy = Random.Range
                    (BASE_ENERGY_WOLF - BASE_ENERGY_OFFSET_WOLF, BASE_ENERGY_WOLF + BASE_ENERGY_OFFSET_WOLF);

                this.maxSpeed = Random.Range
                    (BASE_SPEED_WOLF - BASE_SPEED_OFFSET_WOLF, BASE_SPEED_WOLF + BASE_SPEED_OFFSET_WOLF);

                this.maxReproduce = BASE_REPRODUCE;

                this.maxFoodSight = Random.Range
                    (BASE_FOOD_SIGHT_WOLF - BASE_FOOD_SIGHT_OFFSET_WOLF, BASE_FOOD_SIGHT_WOLF + BASE_FOOD_SIGHT_OFFSET_WOLF);

                break;
        }
    }

    public void CreateAnimalRandAttributes(AnimalType animalType, Gender gender, int id)
    {
        this.gender = gender;
        SetAttributesLimits(animalType);

        switch (animalType)
        {
            case AnimalType.Rabbit:
                this.healthPoints = this.maxHealthPoints;
                this.age = Random.Range(1f, 1.1f);
                this.satiety = this.maxSatiety;
                this.energy = this.maxEnergy;
                this.speed = this.maxSpeed;
                this.reproduce = 0f;
                this.reproduceSpeed = BASE_REPRODUCE_SPEED_RABBIT;
                this.reproduceThreshold = BASE_REPRODUCE_THRESH;
                this.foodSight = this.maxFoodSight;
                this.type = AnimalType.Rabbit;
                this.id = id;
                break;

            case AnimalType.Wolf:
                this.healthPoints = this.maxHealthPoints;
                this.age = Random.Range(2f, 2.1f);
                this.satiety = maxSatiety;
                this.energy = maxEnergy;
                this.speed = this.maxSpeed;
                this.reproduce = 0f;
                this.reproduceSpeed = BASE_REPRODUCE_SPEED_WOLF;
                this.reproduceThreshold = BASE_REPRODUCE_THRESH;
                this.foodSight = this.maxFoodSight;
                this.type = AnimalType.Wolf;
                this.id = id;
                break;
        }

        //Debug.Log(animalType.ToString() + " " + gender.ToString() + " " + healthPoints.ToString() + " " + age.ToString() + " " + satiety.ToString() + " " + energy.ToString() + " " + speed.ToString());
    }

    public string GetGenderString(Gender gender)
    {
        if (gender == Gender.Male)
            return "male";
        else return "female";
    }

    public void SetAnimState(Animator anim, string varName, int value)
    {
        anim.SetInteger(varName, value);
    }

    public IEnumerator StopAgentRoutine(NavMeshAgent agent, Animator anim, string animName, int state)
    {
        for (; ; )
        {
            if (Vector3.Distance(agent.transform.position, agent.destination) < 1f)
            {
                agent.isStopped = true;
                SetAnimState(anim, animName, state);
            }

            yield return new WaitForSeconds(0.25f);
        }
    }

    public IEnumerator ModifyAttribsOverTime(AnimalType animalType)
    {
        for (; ; )
        {
            float hpNorm = healthPoints / maxHealthPoints;
            float satieryNorm = satiety / maxSatiety;
            float energyNorm = energy / maxEnergy;
            float ageNorm = age / maxAge;

            float hpContrib = Remap(hpNorm, 0f, 1f, 0.85f, 1.0f);
            float satietyContrib = Remap(satieryNorm, 0f, 1f, 0.85f, 1.0f);
            float energyContrib = Remap(energyNorm, 0f, 1f, 0.85f, 1.0f);
            float ageContrib = Remap(ageNorm, 1f, 0f, 0.85f, 1.0f);

            // Modify age
            if (age < maxAge)
                age += GetAgeModifyRate();

            if (Species == AnimalType.Rabbit)
            {
                // If HP is zero or negative, kill it
                if (healthPoints <= 0f)
                {
                    AnimalCounter.RabbitDeathsByHunger++;
                    Die();
                    break;
                }

                // If aniaml is old enough, kill it
                if (age >= maxAge)
                {
                    AnimalCounter.RabbitDeathsByAge++;
                    Die();
                    break;
                }
            }
            else if (Species == AnimalType.Wolf)
            {
                // If HP is zero or negative, kill it
                if (healthPoints <= 0f)
                {
                    AnimalCounter.WolfDeathsByHunger++;
                    Die();
                }

                // If animal is old enough, kill it
                if (age >= maxAge)
                {
                    AnimalCounter.WolfDeathsByAge++;
                    Die();
                }
            }

            // Drain HP is very hungry
            if (IsHungry() == HungerState.VeryHungry)
                healthPoints -= hpModifyRate;

            // Boost HP drain if famished
            if (satiety <= 0f)
                healthPoints -= (hpModifyRate * 2);

            // Replenish HP if not very hungry and not very tired
            if (IsHungry() != HungerState.VeryHungry && IsTired() != TirednessState.VeryTired)
                if (healthPoints < maxHealthPoints)
                    healthPoints += hpModifyRate;

            // Boost HP replenish if not very hungry and not very tired
            if (IsHungry() == HungerState.NotHungry && IsTired() == TirednessState.NotTired)
                if (healthPoints < maxHealthPoints)
                    healthPoints += (hpModifyRate * 2);

            // Hacky
            if (healthPoints > maxHealthPoints)
                healthPoints = maxHealthPoints;

            if (animalType == AnimalType.Wolf)
            {
                // Modify wolf speed
                //speed = BASE_SPEED_WOLF * hpContrib * satietyContrib * energyContrib * ageContrib;
                maxSpeed = speed;

                // Modify wolf reproduction desire
                if (reproduce < maxReproduce && IsAdult() == AdultState.Adult)
                    reproduce += ReproduceSpeed;

                // If true, cub should grow up
                if (IsAdult() == AdultState.NotAdult && (age + GetAgeModifyRate()) >= GROW_UP_AGE_WOLF)
                {
                    GameObject.Find("AnimalSpawnerController").GetComponent<AnimalSpawner>().GrowUpAnimal(this);
                    Destroy(gameObject);
                }

                // Modify satiety
                if (satiety >= 0)
                    satiety -= satietyModifyRate_Wolf;
            }
            else if (animalType == AnimalType.Rabbit)
            {
                // Modify rabbit speed
                //speed = BASE_SPEED_RABBIT * hpContrib * satietyContrib * energyContrib * ageContrib;
                maxSpeed = speed;

                // Modify rabbit reproduction desire
                if (reproduce < maxReproduce && IsAdult() == AdultState.Adult)
                    reproduce += ReproduceSpeed;

                // If true, cub should grow up
                if (IsAdult() == AdultState.NotAdult && (age + GetAgeModifyRate()) >= GROW_UP_AGE_RABBIT)
                {
                    GameObject.Find("AnimalSpawnerController").GetComponent<AnimalSpawner>().GrowUpAnimal(this);
                    Destroy(gameObject);
                }

                // Modify satiety
                if (satiety >= 0)
                    satiety -= satietyModifyRate_Rabbit;
            }


            if (isSleeping == false)
            {
                // Modify energy
                if (energy >= 0)
                    energy -= GetEnergyModifyRate();
            }
            else if (isSleeping == true)
            {
                // Modify energy
                if (energy <= maxEnergy)
                    energy += (GetEnergyModifyRate() * 3f);
            }

            // Calculare current fitness value. Multiply age to increase its weight in calculation
            Fitness = healthPoints + speed + satiety + energy + foodSight - (5 * age);

            yield return new WaitForSeconds(2f);

        }
    }

    protected IEnumerator TryLookingForFood(NavMeshAgent agent, Animator anim, string animVarName, int state)
    {
        for (; ; )
        {
            NavMeshHit navHit = new NavMeshHit();
            Vector3 randDir = Random.insideUnitSphere * 100f;

            randDir += transform.position;
            NavMesh.SamplePosition(randDir, out navHit, 100f, -1);

            if (IsHungry() == HungerState.VeryHungry)
                NavAgentCommand(agent, false, Speed, anim, animVarName, (int)AnimalStates.STATE_RUN, navHit.position);

            else if (IsHungry() == HungerState.Hungry)
                NavAgentCommand(agent, false, GetWalkSpeed(), anim, animVarName, (int)AnimalStates.STATE_WALK, navHit.position);

            yield return new WaitForSeconds(5f);
        }
    }

    protected IEnumerator IsFoodInRange(string colliderTag)
    {
        for (; ; )
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, FoodSight, -1, QueryTriggerInteraction.Collide);
            float currentDist = float.MaxValue;
            float closestFoodDist = float.MaxValue;
            GameObject target = null;

            foreach (Collider collider in colliders)
                if (collider.tag == colliderTag)
                {
                    // Rabbits only check
                    if (Species == AnimalType.Rabbit)
                    {
                        if (colliderTag == Tags.Plant &&
                            IsAdult() == AdultState.Adult &&
                            (collider.gameObject.GetComponent<PlantGrowController>().IsEatable == false ||
                            collider.gameObject.GetComponent<PlantGrowController>().CanAdultEat == false))
                            continue;

                        if (colliderTag == Tags.Plant &&
                            IsAdult() == AdultState.NotAdult &&
                            (collider.gameObject.GetComponent<PlantGrowController>().IsEatable == false ||
                            collider.gameObject.GetComponent<PlantGrowController>().CanCubEat == false))
                            continue;
                    }

                    currentDist = Vector3.Distance(transform.position, collider.transform.position);

                    if (currentDist < closestFoodDist)
                    {
                        closestFoodDist = currentDist;
                        target = collider.gameObject;
                    }

                }

            if (target != null)
                Debug.DrawLine(transform.position + Vector3.up, target.transform.position, Color.yellow, 1.5f);

            foodTarget = target;

            yield return new WaitForSeconds(0.25f);
        }
    }

    protected IEnumerator GoAfterFood(NavMeshAgent agent, Animator anim, string animVarName, int state)
    {
        for (; ; )
        {
            // Both
            if (foodTarget == null)
                yield return null;

            //Rabbit adults only check
            else if (Species == AnimalType.Rabbit &&
                     IsAdult() == AdultState.Adult &&
                     (foodTarget.GetComponent<PlantGrowController>().IsEatable == false ||
                     foodTarget.GetComponent<PlantGrowController>().CanAdultEat == false))
            {
                foodTarget = null;
                yield return null;
            }

            //Rabbit cubs only check
            else if (Species == AnimalType.Rabbit &&
                     IsAdult() == AdultState.NotAdult &&
                     (foodTarget.GetComponent<PlantGrowController>().IsEatable == false ||
                     foodTarget.GetComponent<PlantGrowController>().CanCubEat == false))
            {
                foodTarget = null;
                yield return null;
            }

            else if (Vector3.Distance(transform.position, foodTarget.transform.position) < FoodSight)
            {
                hasLostFood = false;
                NavAgentCommand(agent, false, Speed, anim, animVarName, state, foodTarget.transform.position);
            }

            else
            {
                hasLostFood = true;
                foodTarget = null;
            }

            yield return new WaitForSeconds(0.25f);
        }
    }

    protected IEnumerator TryLookingForMate(NavMeshAgent agent, Animator anim, string animVarName, int state)
    {
        for (; ; )
        {
            NavMeshHit navHit = new NavMeshHit();
            Vector3 randDir = Random.insideUnitSphere * 100f;

            randDir += transform.position;
            NavMesh.SamplePosition(randDir, out navHit, 100f, -1);

            NavAgentCommand(agent, false, Speed, anim, animVarName, state, navHit.position);

            yield return new WaitForSeconds(5f);
        }
    }

    // TODO INEFFICIENT
    protected IEnumerator IsMateInRange()
    {
        for (; ; )
        {
            Collider[] colliders;

            if (Species == AnimalType.Wolf)
                colliders = Physics.OverlapSphere(transform.position, 120, -1, QueryTriggerInteraction.Collide);
            else
                colliders = Physics.OverlapSphere(transform.position, FoodSight, -1, QueryTriggerInteraction.Collide);

            float bestMateFitness = float.MinValue;
            GameObject newMateTarget = null;

            if (Species == AnimalType.Rabbit)
            {
                foreach (Collider collider in colliders)
                    if (collider.tag == Tags.Rabbit &&                                                        // Found another animal of same species
                         GetComponent<Rabbit>().Gender != collider.GetComponent<Rabbit>().Gender &&           // Other is of opposite gender
                         collider.GetComponent<Rabbit>().IsAdult() == AdultState.Adult &&                     // Other is an adult
                         collider.GetComponent<Rabbit>().Id != Id &&                                          // Is not herself/himself
                         collider.GetComponent<Rabbit>().GetReproduceState() == ReproduceState.ReproduceTrue) // Other wants to reproduce
                    {
                        // Get current target's fitness value
                        float currentMateFitness = collider.GetComponent<Rabbit>().Fitness;

                        if (currentMateFitness > bestMateFitness)
                        {
                            // Set mate target the one which has the biggest fitness value
                            bestMateFitness = currentMateFitness;
                            newMateTarget = collider.gameObject;
                        }

                        Debug.DrawLine(transform.position + Vector3.up, collider.transform.position, Color.magenta, 4);
                    }

                mateTarget = newMateTarget;

                yield return new WaitForSeconds(0.25f);
            }

            if (Species == AnimalType.Wolf)
            {
                foreach (Collider collider in colliders)
                    if (collider.tag == Tags.Wolf &&                                                        // Found another animal of same species
                         GetComponent<Wolf>().Gender != collider.GetComponent<Wolf>().Gender &&             // Other is of opposite gender
                         collider.GetComponent<Wolf>().IsAdult() == AdultState.Adult &&                     // Other is an adult
                         collider.GetComponent<Wolf>().Id != Id &&                                          // Is not herself/himself
                         collider.GetComponent<Wolf>().GetReproduceState() == ReproduceState.ReproduceTrue) // Other wants to reproduce
                    {
                        // Get current target's fitness value
                        float currentMateFitness = collider.GetComponent<Wolf>().Fitness;

                        if (currentMateFitness > bestMateFitness)
                        {
                            // Set mate target the one which has the biggest fitness value
                            bestMateFitness = currentMateFitness;
                            newMateTarget = collider.gameObject;
                        }

                        Debug.DrawLine(transform.position + Vector3.up, collider.transform.position, Color.magenta, 4);
                    }

                mateTarget = newMateTarget;

                yield return new WaitForSeconds(0.25f);
            }
        }
    }

    protected IEnumerator GoAfterMate(NavMeshAgent agent, Animator anim, string animVarName, int state)
    {
        for (; ; )
        {
            float mateSight = 0f;

            // TODO - hack
            if (Species == AnimalType.Wolf)
                mateSight = 120;
            else
                mateSight = FoodSight;

            if (mateTarget == null)
            {
                hasLostMate = true;
                mateTarget = null;
                yield return new WaitForEndOfFrame();
            }

            else if (Vector3.Distance(transform.position, mateTarget.transform.position) < mateSight)
            {
                hasLostMate = false;
                NavAgentCommand(agent, false, Speed, anim, animVarName, state, mateTarget.transform.position);
            }
            else
            {
                hasLostMate = true;
                mateTarget = null;
            }

            yield return new WaitForSeconds(0.25f);
        }
    }

    protected IEnumerator SearchForPredator()
    {
        float searchRadius = 15.0f;

        for (; ; )
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, searchRadius, -1, QueryTriggerInteraction.Collide);
            float currentDistance = float.MaxValue;
            float closestWolfDistance = float.MaxValue;
            GameObject target = null;

            foreach (Collider collider in colliders)
                if (collider.tag == Tags.Wolf && collider.gameObject.GetComponent<Wolf>().IsAlive == true)
                {
                    currentDistance = Vector3.Distance(transform.position, collider.transform.position);

                    if (currentDistance < closestWolfDistance)
                    {
                        closestWolfDistance = currentDistance;
                        target = collider.gameObject;
                    }

                    //Debug.DrawLine(transform.position + Vector3.up, collider.transform.position, Color.white, 4);
                }

            predatorTarget = target;
            yield return new WaitForSeconds(0.2f);
        }
    }

    private IEnumerator FleeFromPredator(NavMeshAgent agent, Animator anim)
    {
        isFleeingFromPredator = true;

        for (; ; )
        {
            if (predatorTarget != null)
                if (Vector3.Distance(transform.position, predatorTarget.transform.position) < 50f)
                {
                    // Run in front of predator (using predator's position, its forward vector and an offset)
                    Vector3 direction = Vector3.Normalize(transform.position - predatorTarget.transform.position);
                    Vector3 runToTarget = transform.position + (direction * 15f);

                    Debug.DrawLine(transform.position, runToTarget, Color.blue, 1);

                    NavAgentCommand(agent, false, Speed, anim, Globals.RabbitAnimName, (int)AnimalStates.STATE_RUN, runToTarget);
                }
                else
                {
                    predatorTarget = null;
                    isFleeingFromPredator = false;
                }

            yield return new WaitForSeconds(0.2f);
        }
    }

    protected IEnumerator DecayAnimal()
    {
        for (; ; )
        {
            transform.position -= Vector3.up / 200f;

            yield return new WaitForSeconds(0.5f);
        }
    }

    public void WalkToRandPos(NavMeshAgent agent, float agentSpeed, float searchDistance)
    {
        NavMeshHit navHit;
        Vector3 randDir = Random.insideUnitSphere * searchDistance;

        randDir += transform.position;
        NavMesh.SamplePosition(randDir, out navHit, searchDistance, -1);

        agent.speed = agentSpeed;
        agent.SetDestination(navHit.position);
    }

    public void UpdateStatusPanelValues(StatusPanelController spc, List<KeyValuePair<int, string>> states, int currentState, float velocity, GameObject obj = null)
    {
        if (Time.time > nextPanelUpdate)
        {
            // Update panel's position
            spc.transform.position = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 1);

            string currentStateStr = null;

            // Get string corresponding to current state
            foreach (KeyValuePair<int, string> kvp in states)
                if (kvp.Key == currentState)
                    currentStateStr = kvp.Value;

            // Update panels
            StatusPanelController.UpdatePanelAnimalState(spc, currentStateStr);
            StatusPanelController.UpdatePanelStatus(spc, StatusType.HP, HealthPoints, MaxHealthPoints);
            StatusPanelController.UpdatePanelStatus(spc, StatusType.Satiety, Satiety, MaxSatiety);
            StatusPanelController.UpdatePanelStatus(spc, StatusType.Energy, Energy, MaxEnergy);
            StatusPanelController.UpdatePanelStatus(spc, StatusType.Speed, velocity, MaxSpeed);
            StatusPanelController.UpdatePanelStatus(spc, StatusType.Age, Age, MaxAge);
            StatusPanelController.UpdatePanelStatus(spc, StatusType.Reproduce, Reproduce, MaxReproduce);
            StatusPanelController.UpdatePanelStatus(spc, StatusType.FoodSight, FoodSight, MaxFoodSight);

            nextPanelUpdate = Time.time + statsPanelUpdateRate;
        }
    }

    public void NavAgentCommand(NavMeshAgent agent, bool isStopped, float agentSpeed,
                                Animator anim, string animVarName, int animState, Vector3 destination = new Vector3())
    {
        agent.isStopped = isStopped;
        agent.speed = agentSpeed;
        SetAnimState(anim, animVarName, animState);

        if (destination != Vector3.zero)
            agent.SetDestination(destination);
    }

    public bool CanEatFood(GameObject foodTarget, Transform tranf, float distance)
    {
        if (foodTarget != null)
            return Vector3.Distance(tranf.position, foodTarget.transform.position) < distance ? true : false;
        else return false;
    }

    public bool CanBreed(GameObject mateTarget, Transform transf, float distance)
    {
        if (mateTarget != null)
            return Vector3.Distance(transf.position, mateTarget.transform.position) < distance ? true : false;
        else return false;
    }

    public HungerState IsHungry()
    {
        float hungerLevel = satiety / maxSatiety;

        if (hungerLevel > 0.55f)
            return HungerState.NotHungry;

        else if (hungerLevel > 0.25 && hungerLevel <= 0.55f)
            return HungerState.Hungry;

        else
            return HungerState.VeryHungry;
    }

    public TirednessState IsTired()
    {
        float tirednessLevel = energy / maxEnergy;

        if (tirednessLevel > 0.55f)
            return TirednessState.NotTired;

        else if (tirednessLevel > 0.25 && tirednessLevel <= 0.55f)
            return TirednessState.Tired;

        else
            return TirednessState.VeryTired;
    }

    public AdultState IsAdult()
    {
        switch (type)
        {
            case AnimalType.Rabbit:
                if (age >= GROW_UP_AGE_RABBIT)
                    return AdultState.Adult;
                else
                    return AdultState.NotAdult;

            case AnimalType.Wolf:
                if (age >= GROW_UP_AGE_WOLF)
                    return AdultState.Adult;
                else
                    return AdultState.NotAdult;
        }

        return AdultState.Adult;
    }

    public bool IsRested()
    {
        float tirednessLevel = energy / maxEnergy;

        return tirednessLevel > 0.7f ? true : false;
    }

    public ReproduceState GetReproduceState()
    {
        if (this.reproduce > this.reproduceThreshold && IsAlive == true)
            return ReproduceState.ReproduceTrue;
        else
            return ReproduceState.ReproduceFalse;
    }

    public List<KeyValuePair<float, AttribType>> GetPackedAttribs()
    {
        return new List<KeyValuePair<float, AttribType>>()
        {
            new KeyValuePair<float, AttribType>(maxHealthPoints, AttribType.MaxHP),
            new KeyValuePair<float, AttribType>(maxSatiety, AttribType.MaxSatiety),
            new KeyValuePair<float, AttribType>(maxEnergy, AttribType.MaxEnergy),
            new KeyValuePair<float, AttribType>(MaxSpeed, AttribType.MaxSpeed),
            new KeyValuePair<float, AttribType>(maxAge, AttribType.MaxAge),
            new KeyValuePair<float, AttribType>(maxFoodSight, AttribType.MaxFoodSight),
        };
    }

    public void SetNewAnimalAttributes(AnimalController animal, Gender g)
    {
        // AGE == 0

        this.Gender = g;

        this.Id = AnimalCounter.RabbitCounter;
        this.MaxHealthPoints = animal.MaxHealthPoints / 2;
        this.MaxSatiety = animal.MaxSatiety / 2;
        this.MaxEnergy = animal.MaxEnergy;
        this.MaxSpeed = animal.MaxSpeed * 0.75f;
        this.MaxAge = animal.MaxAge;
        this.MaxReproduce = BASE_REPRODUCE;
        this.MaxFoodSight = animal.MaxFoodSight;

        this.HealthPoints = this.MaxHealthPoints;
        this.Satiety = this.MaxSatiety;
        this.Energy = this.MaxEnergy;
        this.Speed = this.MaxSpeed;
        this.Age = this.Age + 0.01f;
        this.Reproduce = 0f;
        this.ReproduceSpeed = animal.ReproduceSpeed;
        this.ReproduceThreshold = BASE_REPRODUCE_THRESH;
        this.FoodSight = this.MaxFoodSight;
    }

    public void SetGrownUpAnimalAttributes(AnimalController animal, Gender g)
    {
        this.Gender = g;

        this.Id = AnimalCounter.RabbitCounter;
        this.MaxHealthPoints = animal.MaxHealthPoints * 2;
        this.MaxSatiety = animal.MaxSatiety * 2;
        this.MaxEnergy = animal.MaxEnergy;
        this.MaxSpeed = animal.MaxSpeed / 0.75f;
        this.MaxAge = animal.MaxAge;
        this.MaxReproduce = BASE_REPRODUCE;
        this.MaxFoodSight = animal.MaxFoodSight;

        this.HealthPoints = animal.HealthPoints * 2;
        this.Satiety = animal.Satiety * 2;
        this.Energy = animal.Energy;
        this.Speed = animal.MaxSpeed / 0.75f;
        this.Age = animal.Age + 0.01f;
        this.Reproduce = 0f;
        this.ReproduceSpeed = animal.ReproduceSpeed;
        this.ReproduceThreshold = BASE_REPRODUCE_THRESH;
        this.FoodSight = animal.MaxFoodSight;
    }

    public float Remap(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    public Gender Gender { get => gender; set => gender = value; }
    public float HealthPoints { get => healthPoints; set => healthPoints = value; }
    public float Age { get => age; set => age = value; }
    public float Satiety { get => satiety; set => satiety = value; }
    public float Energy { get => energy; set => energy = value; }
    public float Speed { get => speed; set => speed = value; }
    public float Reproduce { get => reproduce; set => reproduce = value; }
    public float FoodSight { get => foodSight; set => foodSight = value; }
    public float ReproduceThreshold { get => reproduceThreshold; set => reproduceThreshold = value; }
    public int Id { get => id; set => id = value; }
    public float Fitness { get => fitness; set => fitness = value; }

    public float MaxHealthPoints { get => maxHealthPoints; set => maxHealthPoints = value; }
    public float MaxAge { get => maxAge; set => maxAge = value; }
    public float MaxSatiety { get => maxSatiety; set => maxSatiety = value; }
    public float MaxEnergy { get => maxEnergy; set => maxEnergy = value; }
    public float MaxSpeed { get => maxSpeed; set => maxSpeed = value; }
    public float MaxReproduce { get => maxReproduce; set => maxReproduce = value; }
    public float MaxFoodSight { get => maxFoodSight; set => maxFoodSight = value; }
    public AnimalType Species { get => type; set => type = value; }
    public bool IsAlive { get => isAlive; set => isAlive = value; }
    public float ReproduceSpeed { get => reproduceSpeed; set => reproduceSpeed = value; }

    public float GetEnergyModifyRate() { return energyModifyRate; }
    public float GetAgeModifyRate() { return ageModifyRate; }

    public float GetWalkSpeed() { return speed * 0.4f; }
}