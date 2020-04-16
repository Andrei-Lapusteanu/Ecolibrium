using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Rabbit : AnimalController
{
    private const int STATE_IDLE = 0;
    private const int STATE_WALK = 1;
    private const int STATE_RUN = 2;

    private const int STATE_FLEE_FROM_PREDATOR = 3;
    private const int STATE_LOOK_FOR_FOOD = 4;
    private const int STATE_GO_AFTER_FOOD = 5;
    private const int STATE_EAT = 6;
    private const int STATE_SEARCH_FOR_MATE = 7;
    private const int STATE_BREED = 8;
    private const int STATE_SLEEP = 9;
    private const int STATE_DIE = 10;


    private const string animVarName = "rabbit_state";

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

    GameObject predatorTarget = null;
    GameObject foodTarget = null;
    bool isFleeingFromPredator = false;
    bool isLookingForFood = false;
    bool isScanningForFood = false;
    bool isGoingAfterFood = false;
    bool hasLostFood = false;

    // Start is called before the first frame update
    void Start()
    {
        // Get components
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        // Set init anim state
        SetAnimState(anim, animVarName, STATE_RUN);

        // Start coroutines
        StartCoroutine(StopAgentRoutine(agent, anim, animVarName, STATE_IDLE));
        StartCoroutine(ModifyAttribsOverTime());
        StartCoroutine(SearchForPredator());
        mainAI = StartCoroutine(MainAI());

        // Make a kvp list of all states
        states = GroupStates();

        // Init status panel
        spc = StatusPanelController.InstantiateStatsPanel();

        // If left at default (100), agent.SetDestination() would be called too slow
        NavMesh.pathfindingIterationsPerFrame = 10000;
    }

    private void LateUpdate()
    {
        if (spc.IsActive == true)
            // Update panel's position
            spc.transform.position = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 3f);
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(Time.timeScale);

        if (spc.IsActive == true)
        {
            // Update static panel elements once
            StatusPanelController.UpdateStaticElementsOnce(spc, Resources.Load<Sprite>("_Mine/Images/rabbit_icon"), "Rabbit " + GetGenderString(this.gender));

            // Update dynamic panel elements
            UpdateStatusPanelValues(spc, states, currentState, Vector3.Magnitude(agent.velocity));
        }
        else spc.Hide();
    }

    protected override IEnumerator MainAI()
    {
        for (; ; )
        {
            // Top priority is running away from predators, second is food, third sleep
            if (predatorTarget != null)
                currentState = STATE_FLEE_FROM_PREDATOR;

            else if (currentState == STATE_IDLE || currentState == STATE_WALK || currentState == STATE_RUN || currentState == STATE_SLEEP)
            {
                if (IsHungry() == HungerState.VeryHungry || (IsHungry() == HungerState.Hungry && IsTired() != TirednessState.VeryTired))
                    currentState = STATE_LOOK_FOR_FOOD;

                else if (IsTired() == TirednessState.VeryTired || (IsTired() == TirednessState.Tired && IsHungry() == HungerState.NotHungry))
                    currentState = STATE_SLEEP;

                else if (IsHungry() == HungerState.NotHungry && IsTired() == TirednessState.NotTired)
                    currentState = Random.Range(0, 3);
            }

            switch (currentState)
            {
                case STATE_IDLE:
                    Idle();
                    Debug.DrawLine(transform.position + Vector3.up, agent.destination, Color.green, 4);
                    yield return new WaitForSeconds(0.5f);

                    break;

                case STATE_WALK:
                    Walk();
                    Debug.DrawLine(transform.position + Vector3.up, agent.destination, Color.green, 4);
                    yield return new WaitForSeconds(0.5f);

                    break;

                case STATE_RUN:
                    Run();
                    Debug.DrawLine(transform.position + Vector3.up, agent.destination, Color.green, 4);
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
                        lookForFoodRoutine = StartCoroutine(TryLookingForFood());

                    if (isScanningForFood == false)
                        isFoodInRangeRoutine = StartCoroutine(IsFoodInRange());

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
                        goAfterFoodRoutine = StartCoroutine(GoAfterFood());

                    if (foodTarget != null)
                    {
                        if (CanEatFood(foodTarget, transform, 2f) == true)
                            currentState = STATE_EAT;
                    }
                    else if (foodTarget == null)
                        currentState = STATE_LOOK_FOR_FOOD;

                    if (hasLostFood == true)
                        currentState = STATE_LOOK_FOR_FOOD;

                    break;

                case STATE_EAT:
                    Eat();
                    break;

                case STATE_SEARCH_FOR_MATE:
                    SearchForMate();
                    break;

                case STATE_BREED:
                    Breed();
                    break;

                case STATE_SLEEP:
                    Sleep();
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

            // Disable sleeping state 
            if (currentState != STATE_SLEEP)
                base.isSleeping = false;

            if (foodTarget != null)
                Debug.DrawLine(transform.position + Vector3.up, foodTarget.transform.position, Color.green, 2);

            if (predatorTarget != null)
                Debug.DrawLine(transform.position + Vector3.up, predatorTarget.transform.position, Color.red, 2);

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
                    NavAgentCommand(agent, false, GetSpeed(), anim, animVarName, STATE_RUN, runToTarget);
                }
                else
                    predatorTarget = null;

            yield return new WaitForSeconds(0.25f);
        }
    }

    protected override IEnumerator TryLookingForFood()
    {
        isLookingForFood = true;

        for (; ; )
        {
            NavMeshHit navHit = new NavMeshHit();
            Vector3 randDir = Random.insideUnitSphere * 100f;

            randDir += transform.position;
            NavMesh.SamplePosition(randDir, out navHit, 100f, -1);

            if (IsHungry() == HungerState.VeryHungry)
                NavAgentCommand(agent, false, GetSpeed(), anim, animVarName, STATE_RUN, navHit.position);

            else if (IsHungry() == HungerState.Hungry)
                NavAgentCommand(agent, false, GetWalkSpeed(), anim, animVarName, STATE_RUN, navHit.position);

            yield return new WaitForSeconds(5f);
        }
    }

    protected override IEnumerator IsFoodInRange()
    {
        isScanningForFood = true;

        for (; ; )
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, 20.0f, -1, QueryTriggerInteraction.Collide);
            float currentDist = float.MaxValue;
            float closestFoodDist = float.MaxValue;
            GameObject target = null;

            foreach (Collider collider in colliders)
                if (collider.tag == "Plant")
                {
                    currentDist = Vector3.Distance(transform.position, collider.transform.position);

                    if (currentDist < closestFoodDist)
                    {
                        closestFoodDist = currentDist;
                        target = collider.gameObject;
                    }

                    Debug.DrawLine(transform.position + Vector3.up, collider.transform.position, Color.yellow, 4);
                }

            foodTarget = target;

            yield return new WaitForSeconds(0.25f);
        }
    }

    protected override IEnumerator GoAfterFood()
    {
        isGoingAfterFood = true;

        for (; ; )
        {
            if (foodTarget == null)
                yield return null;

            else if (Vector3.Distance(transform.position, foodTarget.transform.position) < 20.0f)
            {
                hasLostFood = false;
                NavAgentCommand(agent, false, GetSpeed(), anim, animVarName, STATE_RUN, foodTarget.transform.position);
            }
            else
            {
                hasLostFood = true;
                foodTarget = null;
            }

            yield return new WaitForSeconds(0.25f);
        }
    }

    protected override void Idle()
    {
        NavAgentCommand(agent, true, agent.speed, anim, animVarName, STATE_IDLE);
    }

    protected override void Walk()
    {
        NavAgentCommand(agent, false, GetWalkSpeed(), anim, animVarName, STATE_RUN);

        WalkToRandPos(agent, GetWalkSpeed(), 20.0f);
    }

    protected override void Run()
    {
        NavAgentCommand(agent, false, GetSpeed(), anim, animVarName, STATE_RUN);

        WalkToRandPos(agent, GetSpeed(), 20.0f);
    }

    protected override void Eat()
    {
        if (foodTarget != null)
        {
            Destroy(foodTarget, 0f);
            foodTarget = null;
            satiety += 10f;
        }

        currentState = STATE_IDLE;
    }

    protected override void SearchForMate()
    {
        throw new System.NotImplementedException();
    }

    protected override void Breed()
    {
        throw new System.NotImplementedException();
    }

    protected override void Sleep()
    {
        base.isSleeping = true;
        NavAgentCommand(agent, true, agent.speed, anim, animVarName, STATE_IDLE);
    }

    protected override void Die()
    {
        throw new System.NotImplementedException();
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
            new KeyValuePair<int, string>(STATE_SEARCH_FOR_MATE, "Searching for mate"),
            new KeyValuePair<int, string>(STATE_BREED, "Breeding <3"),
            new KeyValuePair<int, string>(STATE_SLEEP, "Sleeping"),
            new KeyValuePair<int, string>(STATE_DIE, "Dying :(")
        };
    }

    public void ActivatePanel()
    {
        spc.Unhide();
    }
}
