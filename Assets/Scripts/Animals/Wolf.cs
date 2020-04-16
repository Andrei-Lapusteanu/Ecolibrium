using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Wolf : AnimalController
{
    private const int STATE_IDLE = 0;
    private const int STATE_SIT_DOWN = 1;
    private const int STATE_WALK = 2;
    private const int STATE_CREEP = 3;
    private const int STATE_RUN = 4;

    private const int STATE_LOOK_FOR_FOOD = 5;
    private const int STATE_GO_AFTER_FOOD = 6;
    private const int STATE_EAT = 7;
    private const int STATE_SEARCH_FOR_MATE = 8;
    private const int STATE_BREED = 9;
    private const int STATE_SLEEP = 10;

    private const string animVarName = "wolf_state";

    Animator anim;
    NavMeshAgent agent;
    StatusPanelController spc;
    List<KeyValuePair<int, string>> states;
    int currentState = 0;

    Coroutine mainAI = null;
    Coroutine lookForFoodRoutine = null;
    Coroutine isFoodInRangeRoutine = null;
    Coroutine goAfterFoodRoutine = null;

    GameObject foodTarget = null;
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
        SetAnimState(anim, animVarName, STATE_IDLE);

        // Start coroutines
        StartCoroutine(StopAgentRoutine(agent, anim, animVarName, STATE_IDLE));
        StartCoroutine(ModifyAttribsOverTime());
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
        if (spc.IsActive == true)
        {
            // Update static panel elements once
            StatusPanelController.UpdateStaticElementsOnce(spc, Resources.Load<Sprite>("_Mine/Images/wolf_icon"), "Wolf " + GetGenderString(this.gender));

            // Update dynamic panel elements
            UpdateStatusPanelValues(spc, states, currentState, Vector3.Magnitude(agent.velocity), foodTarget);
        }
        else spc.Hide();
    }

    protected override IEnumerator MainAI()
    {
        for (; ; )
        {
            if (currentState == STATE_IDLE || currentState == STATE_WALK || currentState == STATE_SIT_DOWN || currentState == STATE_CREEP || currentState == STATE_SLEEP)
            {
                if (IsHungry() == HungerState.VeryHungry || (IsHungry() == HungerState.Hungry && IsTired() != TirednessState.VeryTired))
                    currentState = STATE_LOOK_FOR_FOOD;

                else if (IsTired() == TirednessState.VeryTired || (IsTired() == TirednessState.Tired && IsHungry() == HungerState.NotHungry))
                    currentState = STATE_SLEEP;

                else if (IsHungry() == HungerState.NotHungry && IsTired() == TirednessState.NotTired)
                    currentState = Random.Range(0, 4);
            }

            switch (currentState)
            {
                case STATE_IDLE:
                    Idle();
                    Debug.DrawLine(transform.position + Vector3.up, agent.destination, Color.green, 4);
                    yield return new WaitForSeconds(Random.Range(2f, 10f));

                    break;

                case STATE_SIT_DOWN:
                    SitDown();
                    Debug.DrawLine(transform.position + Vector3.up, agent.destination, Color.green, 4);
                    yield return new WaitForSeconds(Random.Range(2f, 10f));

                    break;

                case STATE_WALK:
                    Walk();
                    Debug.DrawLine(transform.position + Vector3.up, agent.destination, Color.green, 4);
                    yield return new WaitForSeconds(Random.Range(2f, 10f));

                    break;

                case STATE_RUN:
                    Run();
                    Debug.DrawLine(transform.position + Vector3.up, agent.destination, Color.green, 4);
                    yield return new WaitForSeconds(Random.Range(2f, 10f));

                    break;

                case STATE_CREEP:
                    Creep();
                    Debug.DrawLine(transform.position + Vector3.up, agent.destination, Color.green, 4);
                    yield return new WaitForSeconds(Random.Range(2f, 10f));

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
                        if (CanEatFood(foodTarget, transform, 4f) == true)
                            currentState = STATE_EAT;
                    }
                    else if(foodTarget == null)
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

            if (currentState != STATE_LOOK_FOR_FOOD)
            {
                if (lookForFoodRoutine != null)
                {
                    isLookingForFood = false;
                    StopCoroutine(lookForFoodRoutine);
                }
            }

            if (currentState != STATE_GO_AFTER_FOOD)
            {
                if (goAfterFoodRoutine != null)
                {
                    isGoingAfterFood = false;
                    hasLostFood = true;
                    StopCoroutine(goAfterFoodRoutine);
                }
            }

            if (currentState != STATE_LOOK_FOR_FOOD && currentState != STATE_GO_AFTER_FOOD)
            {
                if (isFoodInRangeRoutine != null)
                {
                    isScanningForFood = false;
                    StopCoroutine(isFoodInRangeRoutine);
                }
            }

            if(currentState != STATE_SLEEP)
                base.isSleeping = false;

            Debug.DrawLine(agent.transform.position + Vector3.up, agent.destination, Color.green, 4);

            if (foodTarget != null)
                Debug.DrawLine(transform.position + Vector3.up, foodTarget.transform.position, Color.red, 4);

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

            else if(IsHungry() == HungerState.Hungry)
                NavAgentCommand(agent, false, GetWalkSpeed(), anim, animVarName, STATE_WALK, navHit.position);

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
                if (collider.tag == "Rabbit")
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

    private void SitDown()
    {
        NavAgentCommand(agent, true, agent.speed, anim, animVarName, STATE_SIT_DOWN);
    }

    protected override void Walk()
    {
        NavAgentCommand(agent, false, GetWalkSpeed(), anim, animVarName, STATE_WALK);

        WalkToRandPos(agent, GetWalkSpeed(), 20.0f);
    }

    protected override void Run()
    {
        NavAgentCommand(agent, false, GetSpeed(), anim, animVarName, STATE_RUN);

        WalkToRandPos(agent, GetSpeed(), 20.0f);
    }

    protected void Creep()
    {
        NavAgentCommand(agent, false, GetWalkSpeed(), anim, animVarName, STATE_CREEP);

        WalkToRandPos(agent, GetWalkSpeed(), 20.0f);
    }

    protected override void Eat()
    {
        if (foodTarget != null)
        {
            Destroy(foodTarget, 0f);
            foodTarget = null;
            satiety += 20f;
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
        NavAgentCommand(agent, true, agent.speed, anim, animVarName, STATE_SIT_DOWN);
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
            new KeyValuePair<int, string>(STATE_SIT_DOWN, "Seated"),
            new KeyValuePair<int, string>(STATE_WALK, "Walking"),
            new KeyValuePair<int, string>(STATE_CREEP, "Creeping"),
            new KeyValuePair<int, string>(STATE_RUN, "Running"),
            new KeyValuePair<int, string>(STATE_LOOK_FOR_FOOD, "Searching for food"),
            new KeyValuePair<int, string>(STATE_GO_AFTER_FOOD, "Going after food"),
            new KeyValuePair<int, string>(STATE_EAT, "Eating"),
            new KeyValuePair<int, string>(STATE_SEARCH_FOR_MATE, "Searching for mate"),
            new KeyValuePair<int, string>(STATE_BREED, "Breeding <3"),
            new KeyValuePair<int, string>(STATE_SLEEP, "Sleeping")
        };
    }

    public void ActivatePanel()
    {
        spc.Unhide();
    }
}
