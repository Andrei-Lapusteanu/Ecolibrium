using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

public class Rabbit : AnimalController
{
    private const int STATE_IDLE = 0;
    private const int STATE_WALK = 1;
    private const int STATE_RUN = 2;

    private const int STATE_FLEE_FROM_PREDATOR = 3;
    private const int STATE_LOOK_FOR_FOOD = 4;
    private const int STATE_GO_AFTER_FOOD = 5;
    private const int STATE_EAT = 6;
    private const int STATE_LOOK_FOR_MATE = 7;
    private const int STATE_GO_AFTER_MATE = 8;
    private const int STATE_BREED = 9;
    private const int STATE_SLEEP = 10;
    private const int STATE_DIE = 11;

    private const string animVarName = "rabbit_state";

    public Gender genderUI;

    public GameObject spawnParticleEffect;
    public bool testing = false;

    Animator anim;
    NavMeshAgent agent;
    StatusPanelController spc;
    List<KeyValuePair<int, string>> states;
    int currentState = 0;

    Coroutine mainAI = null;
    Coroutine fleeFromPredatorRoutine = null;
    Coroutine lookForFoodRoutine = null;
    Coroutine isFoodInRangeRoutine = null;
    Coroutine goAfterFoodRoutine = null;

    Coroutine lookForMateRoutine = null;
    Coroutine isMateInRangeRoutine = null;
    Coroutine goAfterMateRoutine = null;

    GameObject predatorTarget = null;
    GameObject foodTarget = null;
    GameObject mateTarget = null;

    bool isFleeingFromPredator = false;
    bool isLookingForFood = false;
    bool isScanningForFood = false;
    bool isGoingAfterFood = false;

    bool isLookingForMate = false;
    bool isScanningForMate = false;
    bool isGoingAfterMate = false;

    bool hasLostFood = false;
    bool hasLostMate = false;

    // Start is called before the first frame update
    void Start()
    {
        // Get components
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        // TESTING
        if (testing == true)
        {
            base.Gender = this.genderUI;
            base.SetAttributesLimits(AnimalType.Rabbit);
            base.CreateAnimalRandAttributes(AnimalType.Rabbit, base.Gender, WorldLimits.RabbitCounter++);
        }

        // Set init anim state
        SetAnimState(anim, animVarName, STATE_RUN);

        // Start coroutines
        StartCoroutine(StopAgentRoutine(agent, anim, animVarName, STATE_IDLE));
        StartCoroutine(ModifyAttribsOverTime(AnimalType.Rabbit));
        StartCoroutine(SearchForPredator());
        mainAI = StartCoroutine(MainAI());

        // Make a kvp list of all states
        states = GroupStates();

        // Init status panel
        spc = StatusPanelController.InstantiateStatsPanel();
        spc.Hide();

        // If left at default (100), agent.SetDestination() would be called too slow
        NavMesh.pathfindingIterationsPerFrame = 10000;
    }

    private void LateUpdate()
    {
        if (spc.IsActive == true)
            // Update panel's position
            spc.transform.position = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 3f);
    }

    private void OnDestroy()
    {
        StopAllCoroutines();

        if (spc != null)
            Destroy(spc.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (spc.IsActive == true)
        {
            // Update static panel elements once
            StatusPanelController.UpdateStaticElementsOnce(spc, Resources.Load<Sprite>("_Mine/Images/rabbit_icon"), "Rabbit " + GetGenderString(this.Gender));

            // Update dynamic panel elements
            UpdateStatusPanelValues(spc, states, currentState, Vector3.Magnitude(agent.velocity));
        }
        else spc.Hide();
    }

    protected override IEnumerator MainAI()
    {
        for (; ; )
        {
            // DE STERS
            this.genderUI = base.Gender;

            // Top priority is running away from predators, second is food, third sleep
            if (currentState == STATE_DIE)
                break;

            if (predatorTarget != null)
                currentState = STATE_FLEE_FROM_PREDATOR;

            // added currentState == STATE_LOOK_FOR_MATE
            else if (currentState == STATE_IDLE || currentState == STATE_WALK || currentState == STATE_RUN || currentState == STATE_SLEEP || currentState == STATE_LOOK_FOR_MATE)
            {
                if (IsHungry() == HungerState.VeryHungry || (IsHungry() == HungerState.Hungry && IsTired() != TirednessState.VeryTired))
                    currentState = STATE_LOOK_FOR_FOOD;

                else if (base.Gender == Gender.Male && GetReproduceState() == ReproduceState.ReproduceTrue && (IsHungry() != HungerState.VeryHungry && IsTired() != TirednessState.VeryTired))
                    currentState = STATE_LOOK_FOR_MATE;

                else if (IsTired() == TirednessState.VeryTired || (IsTired() == TirednessState.Tired && IsHungry() == HungerState.NotHungry))
                    currentState = STATE_SLEEP;

                else if (IsHungry() == HungerState.NotHungry && IsTired() == TirednessState.NotTired)
                    currentState = Random.Range(0, 3);
            }

            switch (currentState)
            {
                case STATE_IDLE:
                    Idle(agent, true, anim, animVarName, STATE_IDLE);
                    Debug.DrawLine(transform.position + Vector3.up, agent.destination, Color.green, 1);
                    yield return new WaitForSeconds(0.5f);

                    break;

                case STATE_WALK:
                    Walk(agent, false, anim, animVarName, STATE_RUN);
                    Debug.DrawLine(transform.position + Vector3.up, agent.destination, Color.green, 1);
                    yield return new WaitForSeconds(0.5f);

                    break;

                case STATE_RUN:
                    Run(agent, false, anim, animVarName, STATE_RUN);
                    Debug.DrawLine(transform.position + Vector3.up, agent.destination, Color.green, 1);
                    yield return new WaitForSeconds(0.5f);

                    break;

                case STATE_FLEE_FROM_PREDATOR:

                    if (isFleeingFromPredator == false)
                        fleeFromPredatorRoutine = StartCoroutine(FleeFromPredator());

                    if (predatorTarget == null)
                        currentState = STATE_IDLE;

                    break;

                case STATE_LOOK_FOR_FOOD:

                    if (isLookingForFood == false)
                    {
                        lookForFoodRoutine = StartCoroutine(TryLookingForFood(agent, anim, animVarName, STATE_RUN));
                        isLookingForFood = true;
                    }

                    if (isScanningForFood == false)
                    {
                        isFoodInRangeRoutine = StartCoroutine(IsFoodInRange("Plant", SetFoodTarget));
                        isScanningForFood = true;
                    }

                    if (IsTired() == TirednessState.VeryTired && IsHungry() != HungerState.VeryHungry)
                    {
                        currentState = STATE_SLEEP;
                        break;
                    }

                    else if (foodTarget != null)// && currentState == STATE_LOOK_FOR_FOOD)
                        currentState = STATE_GO_AFTER_FOOD;

                    break;

                case STATE_GO_AFTER_FOOD:

                    if (IsTired() == TirednessState.VeryTired && IsHungry() != HungerState.VeryHungry)
                    {
                        currentState = STATE_SLEEP;
                        break;
                    }

                    if (isGoingAfterFood == false)
                    {
                        goAfterFoodRoutine = StartCoroutine(GoAfterFood(foodTarget, SetFoodTarget, SetHasLostFood, agent, anim, animVarName, STATE_RUN));
                        isGoingAfterFood = true;
                    }

                    if (foodTarget != null)
                    {
                        if (foodTarget.GetComponent<PlantGrowController>().IsEatable == true)
                        {
                            if (CanEatFood(foodTarget, transform, 2f) == true)
                                currentState = STATE_EAT;
                        }
                        else foodTarget = null;
                    }

                    else if (foodTarget == null)
                        currentState = STATE_LOOK_FOR_FOOD;

                    if (hasLostFood == true)
                        currentState = STATE_LOOK_FOR_FOOD;

                    break;

                case STATE_EAT:
                    Eat(foodTarget, SetFoodTarget, SetCurrentState, STATE_IDLE, 10.0f, "Rabbit");
                    break;

                case STATE_LOOK_FOR_MATE:
                    if (isLookingForMate == false)
                    {
                        lookForMateRoutine = StartCoroutine(TryLookingForMate(agent, anim, animVarName, STATE_RUN));
                        isLookingForMate = true;
                    }

                    if (isScanningForMate == false)
                    {
                        isMateInRangeRoutine = StartCoroutine(IsMateInRange("Rabbit", SetMateTarget));
                        isScanningForMate = true;
                    }

                    if (IsTired() == TirednessState.VeryTired && IsHungry() != HungerState.VeryHungry)
                    {
                        currentState = STATE_SLEEP;
                        break;
                    }

                    else if (mateTarget != null)// && currentState == STATE_LOOK_FOR_FOOD)
                        currentState = STATE_GO_AFTER_MATE;

                    break;

                case STATE_GO_AFTER_MATE:
                    if (IsTired() == TirednessState.VeryTired && IsHungry() != HungerState.VeryHungry)
                    {
                        currentState = STATE_SLEEP;
                        break;
                    }

                    if (isGoingAfterMate == false)
                    {
                        goAfterMateRoutine = StartCoroutine(GoAfterMate(mateTarget, SetMateTarget, SetHasLostMate, agent, anim, animVarName, STATE_RUN));
                        isGoingAfterMate = true;
                    }

                    if (mateTarget != null)
                    {
                        if (mateTarget.GetComponent<Rabbit>().GetReproduceState() == ReproduceState.ReproduceTrue)
                        {
                            if (CanBreed(mateTarget, transform, 2.5f) == true)
                                currentState = STATE_BREED;
                        }
                        else mateTarget = null;
                    }

                    else if (mateTarget == null)
                        currentState = STATE_IDLE;

                    if (hasLostMate == true)
                        currentState = STATE_IDLE;
                    break;

                case STATE_BREED:
                    Breed();
                    break;

                case STATE_SLEEP:
                    Sleep(agent, true, anim, animVarName, STATE_IDLE);
                    break;

                case STATE_DIE:
                    // Keep animal dead
                    currentState = STATE_DIE;
                    break;
            }

            // Disable running from predator coroutine
            if (currentState != STATE_FLEE_FROM_PREDATOR)
            {
                if (fleeFromPredatorRoutine != null)
                {
                    isFleeingFromPredator = false;
                    //predatorTarget = null;
                    StopCoroutine(fleeFromPredatorRoutine);
                }
            }

            // Disable looking for food coroutine
            if (currentState != STATE_LOOK_FOR_FOOD)
            {
                if (lookForFoodRoutine != null)
                {
                    isLookingForFood = false;
                    StopCoroutine(lookForFoodRoutine);
                }
            }

            // Disable going after food coroutine
            if (currentState != STATE_GO_AFTER_FOOD)
            {
                if (goAfterFoodRoutine != null)
                {
                    isGoingAfterFood = false;
                    hasLostFood = true;
                    StopCoroutine(goAfterFoodRoutine);
                }
            }

            // Disable scanning for food coroutine
            if (currentState != STATE_LOOK_FOR_FOOD && currentState != STATE_GO_AFTER_FOOD)
            {
                if (isFoodInRangeRoutine != null)
                {
                    isScanningForFood = false;
                    StopCoroutine(isFoodInRangeRoutine);
                }
            }

            // Disable looking for mate coroutine
            if (currentState != STATE_LOOK_FOR_MATE)
            {
                if (lookForMateRoutine != null)
                {
                    isLookingForMate = false;
                    StopCoroutine(lookForMateRoutine);
                }
            }

            // Disable going after mate coroutine
            if (currentState != STATE_GO_AFTER_MATE)
            {
                if (goAfterMateRoutine != null)
                {
                    isGoingAfterMate = false;
                    hasLostMate = true;
                    StopCoroutine(goAfterMateRoutine);
                }
            }

            // Disable scanning for mate coroutine
            if (currentState != STATE_LOOK_FOR_MATE && currentState != STATE_GO_AFTER_MATE)
            {
                if (isMateInRangeRoutine != null)
                {
                    isScanningForMate = false;
                    StopCoroutine(isMateInRangeRoutine);
                }
            }

            // Disable sleeping state 
            if (currentState != STATE_SLEEP)
                base.isSleeping = false;

            if (foodTarget != null)
                //Debug.DrawLine(transform.position + Vector3.up, foodTarget.transform.position, Color.green, 1);

                if (predatorTarget != null)
                    Debug.DrawLine(transform.position + Vector3.up, predatorTarget.transform.position, Color.red, 1);

            yield return new WaitForSeconds(0.25f);
        }
    }

    IEnumerator SearchForPredator()
    {
        float searchRadius = 10.0f;

        for (; ; )
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, searchRadius, -1, QueryTriggerInteraction.Collide);
            float currentDistance = float.MaxValue;
            float closestWolfDistance = float.MaxValue;
            GameObject target = null;

            foreach (Collider collider in colliders)
                if (collider.tag == "Wolf")
                {
                    currentDistance = Vector3.Distance(transform.position, collider.transform.position);

                    if (currentDistance < closestWolfDistance)
                    {
                        closestWolfDistance = currentDistance;
                        target = collider.gameObject;
                    }

                    Debug.DrawLine(transform.position + Vector3.up, collider.transform.position, Color.white, 4);
                }

            predatorTarget = target;
            yield return new WaitForSeconds(0.2f);
        }
    }

    IEnumerator FleeFromPredator()
    {
        isFleeingFromPredator = true;

        for (; ; )
        {
            //if (predatorTarget == null)
            //    yield return null;

            if (predatorTarget != null)
                if (Vector3.Distance(transform.position, predatorTarget.transform.position) < 50f)
                {
                    // Run in front of predator (using predator's position, its forward vector and an offset)
                    Vector3 direction = Vector3.Normalize(transform.position - predatorTarget.transform.position);
                    Vector3 runToTarget = direction * 40f;
                    //Vector3 runToTarget = (predatorTarget.transform.forward * 25f) + predatorTarget.transform.position;
                    NavAgentCommand(agent, false, base.GetSpeed(), anim, animVarName, STATE_RUN, runToTarget);
                }
                else
                    predatorTarget = null;

            yield return new WaitForSeconds(0.25f);
        }
    }

    protected override void Breed()
    {
        // alg genetic
        if (mateTarget.GetComponent<Rabbit>().GetReproduceState() == ReproduceState.ReproduceTrue)
        {
            // Reset parents reproduce desire
            mateTarget.GetComponent<Rabbit>().Reproduce = 0f;
            base.Reproduce = 0f;

            for (int i = 0; i < UnityEngine.Random.Range(2, 4); i++)
                GeneticalAlgorithm<Rabbit>.GenerateOffspring(gameObject.GetComponent<Rabbit>(), mateTarget.GetComponent<Rabbit>());
        }

        currentState = STATE_IDLE;
    }

    protected override void Die()
    {
        // Set current state
        currentState = STATE_DIE;
        base.IsAlive = false;

        // Stop all coroutines
        StopAllCoroutines();

        // Disable box collider ??
        //GetComponent<BoxCollider>().enabled = false;
        agent.enabled = false;

        // Stop agent
        NavAgentCommand(agent, true, 0f, anim, animVarName, STATE_DIE);

        // Start decay routine
        StartCoroutine(base.DecayAnimal());

        // Destroy after a few seconds
        Destroy(this.gameObject, 30f);
    }

    List<KeyValuePair<int, string>> GroupStates()
    {
        return new List<KeyValuePair<int, string>>()
        {
            new KeyValuePair<int, string>(STATE_IDLE, "Idling"),
            new KeyValuePair<int, string>(STATE_WALK, "Walking"),
            new KeyValuePair<int, string>(STATE_RUN, "Running"),
            new KeyValuePair<int, string>(STATE_FLEE_FROM_PREDATOR, "Fleeing from wolf"),
            new KeyValuePair<int, string>(STATE_LOOK_FOR_FOOD, "Searching for food"),
            new KeyValuePair<int, string>(STATE_GO_AFTER_FOOD, "Going after food"),
            new KeyValuePair<int, string>(STATE_EAT, "Eating"),
            new KeyValuePair<int, string>(STATE_LOOK_FOR_MATE, "Searching for mate"),
            new KeyValuePair<int, string>(STATE_GO_AFTER_MATE, "Going after mate"),
            new KeyValuePair<int, string>(STATE_BREED, "Breeding <3"),
            new KeyValuePair<int, string>(STATE_SLEEP, "Sleeping"),
            new KeyValuePair<int, string>(STATE_DIE, "Dead")
        };
    }

    public void PlaySpawnEffect()
    {
        // Play particle effect
        GameObject partileInst = Instantiate(spawnParticleEffect, transform);
        partileInst.transform.GetChild(0).GetComponent<ParticleSystem>().Play();
        partileInst.transform.GetChild(1).GetComponent<ParticleSystem>().Play();

        // Destroy effect
        Destroy(partileInst, 3.0f);
    }

    public void SetFoodTarget(GameObject newfoodTarget)
    {
        foodTarget = newfoodTarget;
    }

    public void SetHasLostFood(bool newBoolValue)
    {
        hasLostFood = newBoolValue;
    }

    public void SetMateTarget(GameObject newMateTarget)
    {
        mateTarget = newMateTarget;
    }

    public void SetHasLostMate(bool newBoolValue)
    {
        hasLostMate = newBoolValue;
    }

    public void SetCurrentState(int newState)
    {
        currentState = newState;
    }

    public void ActivatePanel()
    {
        spc.Unhide();
    }
}
