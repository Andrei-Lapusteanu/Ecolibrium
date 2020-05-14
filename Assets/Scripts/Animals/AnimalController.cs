using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public enum Gender
{
    Male,
    Female
}

public enum AnimalType
{
    Rabbit,
    Wolf
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

public abstract class AnimalController : MonoBehaviour
{
    //HP
    private const float BASE_HP_WOLF = 100f;
    private const float BASE_HP_OFFSET_WOLF = 10f;
    private const float BASE_HP_RABBIT = 50f;
    private const float BASE_HP_OFFSET_RABBIT = 5f;

    // Satiety
    private const float BASE_SATIETY_WOLF = 50f;
    private const float BASE_SATIETY_OFFSET_WOLF = 5f;
    private const float BASE_SATIETY_RABBIT = 20f;
    private const float BASE_SATIETY_OFFSET_RABBIT = 2.5f;

    // Energy
    private const float BASE_ENERGY_WOLF = 100f;
    private const float BASE_ENERGY_OFFSET_WOLF = 10f;
    private const float BASE_ENERGY_RABBIT = 30f;
    private const float BASE_ENERGY_OFFSET_RABBIT = 2.5f;

    // Speed
    private const float BASE_SPEED_WOLF = 5f;
    private const float BASE_SPEED_RABBIT = 4f;
    private const float BASE_SPEED_OFFSET_WOLF = 0.5f;
    private const float BASE_SPEED_OFFSET_RABBIT = 0.25f;

    // Age
    private const float MAX_AGE_WOLF = 8f;
    private const float MAX_AGE_RABBIT = 3f;
    private const float GROW_UP_AGE_WOLF = 2f;
    private const float GROW_UP_AGE_RABBIT = 1f;

    // Reproduce desire
    private const float BASE_REPRODUCE = 1f;

    // Reproduce threshold
    private const float BASE_REPRODUCE_THRESH = 0.7f;

    // Food sight distance
    private const float BASE_FOOD_SIGHT_WOLF = 20f;
    private const float BASE_FOOD_SIGHT_RABBIT = 20f;
    private const float BASE_FOOD_SIGHT_OFFSET_WOLF = 5f;
    private const float BASE_FOOD_SIGHT_OFFSET_RABBIT = 5f;

    private Gender gender;
    private float healthPoints;
    private float age;
    private float satiety;
    private float energy;
    private float speed;
    private float reproduce;
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

    //protected float ageModifyRate = 0.0025f;
    //protected float satietyModifyRate = 0.05f; //0.03f
    //protected float energyModifyRate = 0.025f;
    //protected float hpModifyRate = 0.1f;
    //protected float reproduceModifyRate_Wolf = 0.0025f;
    //protected float reproduceModifyRate_Rabbit = 0.005f;

    protected float ageModifyRate = 0.0025f * 10;
    protected float satietyModifyRate = 0.05f * 10; //0.03f
    protected float energyModifyRate = 0.025f * 10;
    protected float hpModifyRate = 0.1f * 10;
    protected float reproduceModifyRate_Wolf = 0.0025f * 10;
    protected float reproduceModifyRate_Rabbit = 0.005f * 10;

    private float statsPanelUpdateRate = 0.25f;
    private float nextPanelUpdate = 0.0f;

    protected bool isSleeping = false;
    private bool isAlive = true;

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
    public AnimalType Type { get => type; set => type = value; }
    protected bool IsAlive { get => isAlive; set => isAlive = value; }

    protected abstract IEnumerator MainAI();

    protected virtual void Idle(NavMeshAgent agent, bool isStopped, Animator anim, string animVarName, int state)
    {
        NavAgentCommand(agent, true, agent.speed, anim, animVarName, state);
    }

    protected virtual void Walk(NavMeshAgent agent, bool isStopped, Animator anim, string animVarName, int state)
    {
        NavAgentCommand(agent, false, GetWalkSpeed(), anim, animVarName, state);

        WalkToRandPos(agent, GetWalkSpeed(), 20.0f);
    }

    protected virtual void Run(NavMeshAgent agent, bool isStopped, Animator anim, string animVarName, int state)
    {
        NavAgentCommand(agent, false, GetSpeed(), anim, animVarName, state);

        WalkToRandPos(agent, GetSpeed(), 20.0f);
    }

    protected virtual void Eat(GameObject foodTarget, Action<GameObject> foodtargetCallback, Action<int> currentStateCallback, int stateToBeSet, float satietyIncrease, string targetTag)
    {
        if (foodTarget != null)
        {
            if (IsAdult() == AdultState.NotAdult)
                satietyIncrease /= 2;

            if (targetTag == "Wolf")
                Destroy(foodTarget, 0f);
            else if (targetTag == "Rabbit")
                foodTarget.GetComponent<PlantGrowController>().GetEaten(IsAdult());

            foodtargetCallback(null);

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

    protected abstract void Breed();

    protected abstract void Die();

    public float Remap(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    public void CreateAnimal(
            Gender gender,
            float hp,
            float age,
            float satiety,
            float energy,
            float speed,
            int id)
    {
        this.gender = gender;
        this.healthPoints = hp;
        this.age = age;
        this.satiety = satiety;
        this.energy = energy;
        this.speed = speed;
        this.id = id;
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
                this.age = Random.Range(1f, 1.5f);
                this.satiety = this.maxSatiety;
                this.energy = this.maxEnergy;
                this.speed = this.maxSpeed;
                this.reproduce = 0f;
                this.reproduceThreshold = BASE_REPRODUCE_THRESH;
                this.foodSight = this.maxFoodSight;
                this.type = AnimalType.Rabbit;
                this.id = id;
                break;

            case AnimalType.Wolf:
                this.healthPoints = this.maxHealthPoints;
                this.age = Random.Range(2f, 4f);
                this.satiety = maxSatiety;
                this.energy = maxEnergy;
                this.speed = this.maxSpeed;
                this.reproduce = 0f;
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

    public int GetAnimState(Animator anim, string varName)
    {
        return anim.GetInteger(varName);
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

            // If HP is zero or negative, kill it
            if (healthPoints <= 0f)
            {
                AnimalCounter.RabbitDeathsTotal++;
                AnimalCounter.RabbitDeathsByHunger++;
                Die();
            }

            // If aniaml is old enough, kill it
            if (age >= maxAge)
            {
                AnimalCounter.RabbitDeathsTotal++;
                AnimalCounter.RabbitDeathsByAge++;
                Die();
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
                speed = BASE_SPEED_WOLF * hpContrib * satietyContrib * energyContrib * ageContrib;
                maxSpeed = speed;

                // Modify wolf reproduction desire
                if (reproduce < maxReproduce && IsAdult() == AdultState.Adult)
                    reproduce += reproduceModifyRate_Wolf;

                // If true, cub should grow up
                if (IsAdult() == AdultState.NotAdult && (age + GetAgeModifyRate()) >= GROW_UP_AGE_WOLF)
                {
                    GameObject.Find("AnimalSpawnerController").GetComponent<AnimalSpawner>().GrowUpAnimal(this);
                    Destroy(gameObject);
                }
            }
            else if (animalType == AnimalType.Rabbit)
            {
                // Modify rabbit speed
                speed = BASE_SPEED_RABBIT * hpContrib * satietyContrib * energyContrib * ageContrib;
                maxSpeed = speed;

                // Modify rabbit reproduction desire
                if (reproduce < maxReproduce && IsAdult() == AdultState.Adult)
                    reproduce += reproduceModifyRate_Rabbit;

                // If true, cub should grow up
                if (IsAdult() == AdultState.NotAdult && (age + GetAgeModifyRate()) >= GROW_UP_AGE_RABBIT)
                {
                    GameObject.Find("AnimalSpawnerController").GetComponent<AnimalSpawner>().GrowUpAnimal(this);
                    Destroy(gameObject);
                }
            }

            // Modify satiety
            if (satiety >= 0)
                satiety -= GetSatietyModifyRate();

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

            //Debug.Log("Rabbit: " + id + ", " + Fitness);

            //var timeScale = Time.timeScale;

            //var deltaTime = Time.deltaTime;
            //var unscaledDeltaTime = Time.unscaledDeltaTime;

            //var fixedDeltaTime = Time.fixedDeltaTime;
            //var fixedUnscaledDeltaTime = Time.fixedUnscaledDeltaTime;

            //Debug.Log("timeScale : " + timeScale);
            //Debug.Log("deltaTime: " + deltaTime);
            //Debug.Log("unscaledDeltaTime: " + unscaledDeltaTime);
            //Debug.Log("fixedDeltaTime: " + fixedDeltaTime);
            //Debug.Log("fixedUnscaledDeltaTime: " + fixedUnscaledDeltaTime);

            //yield return new WaitForSeconds(Time.fixedUnscaledDeltaTime * 5f);
            yield return new WaitForSeconds(1f);

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
                NavAgentCommand(agent, false, GetSpeed(), anim, animVarName, state, navHit.position);

            else if (IsHungry() == HungerState.Hungry)
                NavAgentCommand(agent, false, GetWalkSpeed(), anim, animVarName, state, navHit.position);

            yield return new WaitForSeconds(5f);
        }
    }

    protected IEnumerator IsFoodInRange(string colliderTag, Action<GameObject> foodtargetCallback)
    {
        for (; ; )
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, GetFoodSight(), -1, QueryTriggerInteraction.Collide);
            float currentDist = float.MaxValue;
            float closestFoodDist = float.MaxValue;
            GameObject target = null;

            foreach (Collider collider in colliders)
                if (collider.tag == colliderTag)
                {
                    //Rabbits only
                    //if (colliderTag == "Plant" && (collider.gameObject.GetComponent<PlantGrowController>().IsEatable == false))
                    //    continue;

                    if (colliderTag == "Plant" && IsAdult() == AdultState.Adult && (collider.gameObject.GetComponent<PlantGrowController>().CanAdultEat == false))
                        continue;

                    if (colliderTag == "Plant" && IsAdult() == AdultState.NotAdult && (collider.gameObject.GetComponent<PlantGrowController>().CanCubEat == false))
                        continue;

                    currentDist = Vector3.Distance(transform.position, collider.transform.position);

                    if (currentDist < closestFoodDist)
                    {
                        closestFoodDist = currentDist;
                        target = collider.gameObject;
                    }

                }

            if (target != null)
                Debug.DrawLine(transform.position + Vector3.up, target.transform.position, Color.yellow, 1.5f);

            foodtargetCallback(target);

            //foodTarget = target;

            yield return new WaitForSeconds(0.25f);
        }
    }

    protected IEnumerator GoAfterFood(GameObject foodTarget,
                                      Action<GameObject> foodtargetCallback,
                                      Action<bool> hasLostFoodCallback,
                                      NavMeshAgent agent,
                                      Animator anim,
                                      string animVarName,
                                      int state)
    {
        for (; ; )
        {
            // Both
            if (foodTarget == null)
                yield return null;

            //Rabbit adults only 
            else if (animVarName == "rabbit_state" && IsAdult() == AdultState.Adult && foodTarget.GetComponent<PlantGrowController>().CanAdultEat == false)
            {
                foodtargetCallback(null);
                yield return null;
            }

            //Rabbit cubs only 
            else if (animVarName == "rabbit_state" && IsAdult() == AdultState.NotAdult && foodTarget.GetComponent<PlantGrowController>().CanCubEat == false)
            {
                foodtargetCallback(null);
                yield return null;
            }

            else if (Vector3.Distance(transform.position, foodTarget.transform.position) < GetFoodSight())
            {
                hasLostFoodCallback(false);
                NavAgentCommand(agent, false, GetSpeed(), anim, animVarName, state, foodTarget.transform.position);
            }
            else
            {
                hasLostFoodCallback(true);
                foodtargetCallback(null);
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

            NavAgentCommand(agent, false, GetSpeed(), anim, animVarName, state, navHit.position);

            yield return new WaitForSeconds(5f);
        }
    }

    // TODO INEFFICIENT
    protected IEnumerator IsMateInRange(string colliderTag, Action<GameObject> mateTargetCallback)
    {
        for (; ; )
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, GetFoodSight(), -1, QueryTriggerInteraction.Collide);
            float bestMateFitness = float.MinValue;
            GameObject mateTarget = null;

            if (colliderTag == "Rabbit")
            {
                foreach (Collider collider in colliders)
                    if (collider.tag == colliderTag &&                                                        // Found another animal of same species
                         GetComponent<Rabbit>().GetGender() != collider.GetComponent<Rabbit>().GetGender() && // Other is of opposite gender
                         collider.GetComponent<Rabbit>().IsAdult() == AdultState.Adult &&                     // Other is an adult
                         collider.GetComponent<Rabbit>().GetID() != GetID() &&                                // Is not herself/himself
                         collider.GetComponent<Rabbit>().GetReproduceState() == ReproduceState.ReproduceTrue) // Other wants to reproduce
                    {
                        // Get current target's fitness value
                        float currentMateFitness = collider.GetComponent<Rabbit>().Fitness;

                        if (currentMateFitness > bestMateFitness)
                        {
                            // Set mate target the one which has the biggest fitness value
                            bestMateFitness = currentMateFitness;
                            mateTarget = collider.gameObject;
                        }

                        Debug.DrawLine(transform.position + Vector3.up, collider.transform.position, Color.magenta, 4);
                    }

                mateTargetCallback(mateTarget);

                yield return new WaitForSeconds(0.25f);
            }
            else if (colliderTag == "Wolf")
            {
                foreach (Collider collider in colliders)
                    if (collider.tag == colliderTag &&                                                      // Found another animal of same species
                         GetComponent<Wolf>().GetGender() != collider.GetComponent<Wolf>().GetGender() &&   // Other is of opposite gender
                         collider.GetComponent<Wolf>().IsAdult() == AdultState.Adult &&                     // Other is an adult
                         collider.GetComponent<Wolf>().GetID() != GetID() &&                                // Is not herself/himself
                         collider.GetComponent<Wolf>().GetReproduceState() == ReproduceState.ReproduceTrue) // Other wants to reproduce
                    {
                        // Get current target's fitness value
                        float currentMateFitness = collider.GetComponent<Wolf>().Fitness;

                        if (currentMateFitness > bestMateFitness)
                        {
                            // Set mate target the one which has the biggest fitness value
                            bestMateFitness = currentMateFitness;
                            mateTarget = collider.gameObject;
                        }

                        Debug.DrawLine(transform.position + Vector3.up, collider.transform.position, Color.magenta, 4);
                    }

                mateTargetCallback(mateTarget);

                yield return new WaitForSeconds(0.25f);
            }
        }
    }

    protected IEnumerator GoAfterMate(GameObject mateTarget,
                                      Action<GameObject> mateTargetCallback,
                                      Action<bool> hasLostMateCallback,
                                      NavMeshAgent agent,
                                      Animator anim,
                                      string animVarName,
                                      int state)
    {
        for (; ; )
        {
            if (mateTarget == null)
            {
                hasLostMateCallback(true);
                mateTargetCallback(null);
                yield return new WaitForEndOfFrame();
            }

            else if (Vector3.Distance(transform.position, mateTarget.transform.position) < GetFoodSight())
            {
                hasLostMateCallback(false);
                NavAgentCommand(agent, false, GetSpeed(), anim, animVarName, state, mateTarget.transform.position);
            }
            else
            {
                hasLostMateCallback(true);
                mateTargetCallback(null);
            }

            yield return new WaitForSeconds(0.25f);
        }
    }

    protected IEnumerator DecayAnimal()
    {
        for (; ; )
        {
            transform.position -= Vector3.up / 100f;

            yield return new WaitForSeconds(0.5f);
        }
    }

    //protected IEnumerator GoAfterFood(Action<GameObject> foodtargetCallback, Action<bool> hasLostFoodCallback)
    //{

    //    for (; ; )
    //    {
    //        if (foodTarget == null)
    //            yield return null;

    //        else if (Vector3.Distance(transform.position, foodTarget.transform.position) < base.GetFoodSight())
    //        {
    //            hasLostFood = false;
    //            NavAgentCommand(agent, false, base.GetSpeed(), anim, animVarName, STATE_RUN, foodTarget.transform.position);
    //        }
    //        else
    //        {
    //            hasLostFood = true;
    //            foodTarget = null;
    //        }

    //        yield return new WaitForSeconds(0.25f);
    //    }
    //}

    //protected IEnumerator IsMateInRange()
    //{
    //    isScanningForMate = true;

    //    for (; ; )
    //    {
    //        Collider[] colliders = Physics.OverlapSphere(transform.position, base.GetFoodSight(), -1, QueryTriggerInteraction.Collide);
    //        float bestMateFitness = float.MinValue;
    //        GameObject mateTarget = null;

    //        foreach (Collider collider in colliders)
    //            if (collider.tag == "Rabbit" &&                                                           // Found another rabbit
    //                GetComponent<Rabbit>().GetGender() != collider.GetComponent<Rabbit>().GetGender() &&  // Other is of opposite gender
    //                 collider.GetComponent<Rabbit>().IsAdult() == AdultState.Adult &&                     // Other is an adult
    //                 collider.GetComponent<Rabbit>().GetID() != base.GetID() &&                           // Is not herself/himself
    //                 collider.GetComponent<Rabbit>().GetReproduceState() == ReproduceState.ReproduceTrue) // Other wants to reproduce
    //            {
    //                // Get current target's fitness value
    //                float currentMateFitness = collider.GetComponent<Rabbit>().Fitness;

    //                if (currentMateFitness > bestMateFitness)
    //                {
    //                    // Set mate target the one which has the biggest fitness value
    //                    bestMateFitness = currentMateFitness;
    //                    mateTarget = collider.gameObject;
    //                }

    //                Debug.DrawLine(transform.position + Vector3.up, collider.transform.position, Color.magenta, 4);
    //            }

    //        this.mateTarget = mateTarget;

    //        yield return new WaitForSeconds(0.25f);
    //    }
    //}

    //protected IEnumerator GoAfterMate()
    //{
    //    isGoingAfterMate = true;

    //    for (; ; )
    //    {
    //        if (mateTarget == null)
    //        {
    //            hasLostMate = true;
    //            mateTarget = null;
    //            yield return new WaitForEndOfFrame();
    //        }

    //        else if (Vector3.Distance(transform.position, mateTarget.transform.position) < base.GetFoodSight())
    //        {
    //            hasLostMate = false;
    //            NavAgentCommand(agent, false, base.GetSpeed(), anim, animVarName, STATE_RUN, mateTarget.transform.position);
    //        }
    //        else
    //        {
    //            hasLostMate = true;
    //            mateTarget = null;
    //        }

    //        yield return new WaitForSeconds(0.25f);
    //    }
    //}

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
            StatusPanelController.UpdatePanelStatus(spc, StatusType.HP, GetHP(), GetMaxHP());
            StatusPanelController.UpdatePanelStatus(spc, StatusType.Satiety, GetSatiety(), GetMaxSatiety());
            StatusPanelController.UpdatePanelStatus(spc, StatusType.Energy, GetEnergy(), GetMaxEnergy());
            StatusPanelController.UpdatePanelStatus(spc, StatusType.Speed, velocity, GetMaxSpeed());
            StatusPanelController.UpdatePanelStatus(spc, StatusType.Age, GetAge(), GetMaxAge());
            StatusPanelController.UpdatePanelStatus(spc, StatusType.Reproduce, GetReproduce(), GetMaxReproduce());
            StatusPanelController.UpdatePanelStatus(spc, StatusType.FoodSight, GetFoodSight(), GetMaxFoodSight());

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

        this.Id = WorldLimits.RabbitCounter;
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
        this.ReproduceThreshold = BASE_REPRODUCE_THRESH;
        this.FoodSight = this.MaxFoodSight;
    }

    public void SetGrownUpAnimalAttributes(AnimalController animal, Gender g)
    {
        this.Gender = g;

        this.Id = WorldLimits.RabbitCounter;
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
        this.ReproduceThreshold = BASE_REPRODUCE_THRESH;
        this.FoodSight = animal.MaxFoodSight;
    }

    public Gender GetGender() { return this.gender; }
    public float GetHP() { return this.healthPoints; }
    public float GetAge() { return this.age; }
    public float GetSatiety() { return this.satiety; }
    public float GetEnergy() { return this.energy; }
    public float GetSpeed() { return this.speed; }
    public float GetReproduce() { return this.reproduce; }
    public float GetReproduceThreshold() { return this.reproduceThreshold; }
    public float GetFoodSight() { return this.foodSight; }
    public int GetID() { return this.id; }

    public float GetMaxHP() { return this.maxHealthPoints; }
    public float GetMaxAge() { return this.maxAge; }
    public float GetMaxSatiety() { return this.maxSatiety; }
    public float GetMaxEnergy() { return this.maxEnergy; }
    public float GetMaxSpeed() { return this.maxSpeed; }
    public float GetMaxReproduce() { return this.maxReproduce; }
    public float GetMaxFoodSight() { return this.maxFoodSight; }

    public float GetSatietyModifyRate() { return satietyModifyRate; }
    public float GetEnergyModifyRate() { return energyModifyRate; }
    public float GetAgeModifyRate() { return ageModifyRate; }

    public float GetWalkSpeed() { return speed * 0.4f; }
}

//public void SetAnimalAttributes(AnimalController animal, Gender g)
//{
//    this.Gender = g;

//    this.Id = WorldLimits.RabbitCounter;
//    this.MaxHealthPoints = animal.MaxHealthPoints;
//    this.MaxSatiety = animal.MaxSatiety;
//    this.MaxEnergy = animal.MaxEnergy;
//    this.MaxSpeed = animal.MaxSpeed;
//    this.MaxAge = animal.MaxAge;
//    this.MaxReproduce = BASE_REPRODUCE;
//    this.MaxFoodSight = animal.MaxFoodSight;

//    this.Speed = animal.MaxSpeed;
//    this.Reproduce = 0f;
//    this.ReproduceThreshold = BASE_REPRODUCE_THRESH;
//    this.FoodSight = animal.MaxFoodSight;

//    if (animal.Age != 0)
//    {
//        this.HealthPoints = animal.HealthPoints;
//        this.Satiety = animal.Satiety;
//        this.Energy = animal.Energy;
//        this.Age = animal.Age + 0.01f;
//    }
//    else
//    {
//        this.HealthPoints = animal.MaxHealthPoints;
//        this.Satiety = animal.MaxSatiety;
//        this.Energy = animal.MaxEnergy;
//        this.Age = animal.Age + 0.01f;
//    }
//}