using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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
    HP, Satiety, Energy, Speed, Age
}

public enum HungerState
{
    NotHungry, Hungry, VeryHungry
}

public enum TirednessState
{
    NotTired, Tired, VeryTired
}

public abstract class AnimalController : MonoBehaviour
{
    private const float BASE_HP_WOLF = 100f;
    private const float BASE_HP_OFFSET_WOLF = 10f;
    private const float BASE_HP_RABBIT = 50f;
    private const float BASE_HP_OFFSET_RABBIT = 5f;

    private const float BASE_SATIETY_WOLF = 50f;
    private const float BASE_SATIETY_OFFSET_WOLF = 5f;
    private const float BASE_SATIETY_RABBIT = 20f;
    private const float BASE_SATIETY_OFFSET_RABBIT = 2.5f;

    private const float BASE_ENERGY_WOLF = 100f;
    private const float BASE_ENERGY_OFFSET_WOLF = 10f;
    private const float BASE_ENERGY_RABBIT = 30f;
    private const float BASE_ENERGY_OFFSET_RABBIT = 2.5f;

    private const float BASE_SPEED_WOLF = 4.3f;
    private const float BASE_SPEED_RABBIT = 4f;
    private const float BASE_SPEED_OFFSET_WOLF = 0.5f;
    private const float BASE_SPEED_OFFSET_RABBIT = 0.25f;

    private const float MAX_AGE_WOLF = 8f;
    private const float MAX_AGE_RABBIT = 3f;

    protected Gender gender;
    protected float healthPoints;
    protected float age;
    protected float satiety;
    protected float energy;
    protected float speed;
    protected int id;

    protected float maxHealthPoints;
    protected float maxAge;
    protected float maxSatiety;
    protected float maxEnergy;
    protected float maxSpeed;

    protected float ageModifyRate = 0.001f;
    protected float satietyModifyRate = 0.03f;
    protected float energyModifyRate = 0.025f;

    private float statsPanelUpdateRate = 0.25f;
    private float nextPanelUpdate = 0.0f;

    protected bool isSleeping = false;

    protected abstract IEnumerator MainAI();
    protected abstract IEnumerator TryLookingForFood();
    protected abstract IEnumerator IsFoodInRange();
    protected abstract IEnumerator GoAfterFood();
    protected abstract void Idle();
    protected abstract void Walk();
    protected abstract void Run();
    protected abstract void Eat();
    protected abstract void SearchForMate();
    protected abstract void Breed();
    protected abstract void Sleep();
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
                this.id = id;
                break;

            case AnimalType.Wolf:
                this.healthPoints = this.maxHealthPoints;
                this.age = Random.Range(2f, 4f);
                this.satiety = maxSatiety;
                this.energy = maxEnergy;
                this.speed = this.maxSpeed;
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

    public IEnumerator ModifyAttribsOverTime()
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

            speed = BASE_SPEED_WOLF * hpContrib * satietyContrib * energyContrib * ageContrib;
            maxSpeed = speed;

            if (satiety >= 0)
                satiety -= GetSatietyModifyRate();

            if (isSleeping == false)
            {
                if (energy >= 0)
                    energy -= GetEnergyModifyRate();
            }
            else if(isSleeping == true)
            {
                if (energy <= maxEnergy)
                    energy += (GetEnergyModifyRate() * 3f);
            }

            if (age < maxAge)
                age += GetAgeModifyRate();

            yield return new WaitForSeconds(0.1f);
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
            StatusPanelController.UpdatePanelStatus(spc, StatusType.HP, GetHP(), GetMaxHP());
            StatusPanelController.UpdatePanelStatus(spc, StatusType.Satiety, GetSatiety(), GetMaxSatiety());
            StatusPanelController.UpdatePanelStatus(spc, StatusType.Energy, GetEnergy(), GetMaxEnergy());
            StatusPanelController.UpdatePanelStatus(spc, StatusType.Speed, velocity, GetMaxSpeed());
            StatusPanelController.UpdatePanelStatus(spc, StatusType.Age, GetAge(), GetMaxAge());

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

    public bool IsRested()
    {
        float tirednessLevel = energy / maxEnergy;

        return tirednessLevel > 0.7f ? true : false;
    }

    public Gender GetGender() { return this.gender; }
    public float GetHP() { return this.healthPoints; }
    public float GetAge() { return this.age; }
    public float GetSatiety() { return this.satiety; }
    public float GetEnergy() { return this.energy; }
    public float GetSpeed() { return this.speed; }

    public float GetMaxHP() { return this.maxHealthPoints; }
    public float GetMaxAge() { return this.maxAge; }
    public float GetMaxSatiety() { return this.maxSatiety; }
    public float GetMaxEnergy() { return this.maxEnergy; }
    public float GetMaxSpeed() { return this.maxSpeed; }

    public float GetSatietyModifyRate() { return satietyModifyRate; }
    public float GetEnergyModifyRate() { return energyModifyRate; }
    public float GetAgeModifyRate() { return ageModifyRate; }

    public float GetWalkSpeed() { return speed * 0.4f; }
}
