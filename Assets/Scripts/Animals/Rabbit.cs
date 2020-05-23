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

    // Testing stuff
    public Gender genderUI;
    private bool testing = false;

    public GameObject spawnParticleEffect;
    private Animator anim;
    private NavMeshAgent agent;
    private StatusPanelController spc;
    private List<KeyValuePair<int, string>> states;
    private int currentState = 0;

    void Start()
    {
        // Get components
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        // Set animal type
        base.Species = AnimalType.Rabbit;

        // TODO - TESTING
        if (testing == true)
        {
            base.Gender = this.genderUI;

            // TODO - remove param
            base.SetAttributesLimits(AnimalType.Rabbit);

            // TODO - remove param
            base.CreateAnimalRandAttributes(AnimalType.Rabbit, base.Gender, AnimalCounter.RabbitCounter++);
        }

        // Set init anim state
        SetAnimState(anim, Globals.RabbitAnimName, STATE_RUN);

        // Start coroutines
        StartCoroutine(StopAgentRoutine(agent, anim, Globals.RabbitAnimName, STATE_IDLE));
        StartCoroutine(ModifyAttribsOverTime(AnimalType.Rabbit));
        StartCoroutine(SearchForPredator());
        StartCoroutine(MainAI());

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
            // TODO - Testing 
            this.genderUI = base.Gender;

            // Top priority is running away from predators, second is food, third sleep
            if (currentState == STATE_DIE)
                break;

            else if (predatorTarget != null)
                currentState = STATE_FLEE_FROM_PREDATOR;

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

                    base.Idle(agent, true, anim, Globals.RabbitAnimName, STATE_IDLE);

                    yield return new WaitForSeconds(0.5f);
                    break;

                case STATE_WALK:

                    base.Walk(agent, false, anim, Globals.RabbitAnimName, STATE_RUN);

                    yield return new WaitForSeconds(0.5f);
                    break;

                case STATE_RUN:

                    base.Run(agent, false, anim, Globals.RabbitAnimName, STATE_RUN);

                    yield return new WaitForSeconds(0.5f);
                    break;

                case STATE_FLEE_FROM_PREDATOR:

                    base.StateFleeFromPredator(SetCurrentState, agent, anim);
                    break;

                case STATE_LOOK_FOR_FOOD:

                    base.StateLookForFood(SetCurrentState, agent, anim, Globals.RabbitAnimName, Tags.Plant);
                    break;

                case STATE_GO_AFTER_FOOD:

                    base.StateGoAfterFood(SetCurrentState, agent, anim, Globals.RabbitAnimName);
                    break;

                case STATE_EAT:

                    base.Eat(SetCurrentState, STATE_IDLE, 10.0f, Tags.Rabbit);
                    break;

                case STATE_LOOK_FOR_MATE:

                    base.StateLookForMate(SetCurrentState, agent, anim, Globals.RabbitAnimName);
                    break;

                case STATE_GO_AFTER_MATE:

                    base.StateGoAfterMate(SetCurrentState, agent, anim, Globals.RabbitAnimName);
                    break;

                case STATE_BREED:

                    Breed(SetCurrentState);
                    break;

                case STATE_SLEEP:

                    Sleep(agent, true, anim, Globals.RabbitAnimName, STATE_IDLE);
                    break;

                case STATE_DIE:

                    currentState = STATE_DIE;
                    break;
            }

            // Disable looking for food coroutine
            if (currentState != STATE_LOOK_FOR_FOOD)
                base.DisableLookForFoodRoutine();

            // Disable going after food coroutine
            if (currentState != STATE_GO_AFTER_FOOD)
                base.DisableGoAfterFoodRoutine();

            // Disable scanning for food coroutine
            if (currentState != STATE_LOOK_FOR_FOOD && currentState != STATE_GO_AFTER_FOOD)
                base.DisableIsFoodInRangeRoutine();

            // Disable looking for mate coroutine
            if (currentState != STATE_LOOK_FOR_MATE)
                base.DisableLookForMateRoutine();

            // Disable going after mate coroutine
            if (currentState != STATE_GO_AFTER_MATE)
                base.DisableGoAfterMateRoutine();

            // Disable scanning for mate coroutine
            if (currentState != STATE_LOOK_FOR_MATE && currentState != STATE_GO_AFTER_MATE)
                base.DisableIsMateInRangeRoutine();

            // Disable running from predator coroutine
            if (currentState != STATE_FLEE_FROM_PREDATOR)
                base.DisableFleeingFromPredatorRoutine();

            // Disable sleeping state 
            if (currentState != STATE_SLEEP)
                base.isSleeping = false;

            if (predatorTarget != null)
                Debug.DrawLine(transform.position + Vector3.up, predatorTarget.transform.position, Color.red, 1);

            yield return new WaitForSeconds(0.25f);
        }
    }

    protected override void Die()
    {
        // Set current state
        currentState = STATE_DIE;
        base.IsAlive = false;
        AnimalCounter.RabbitDeathsTotal++;

        // Stop all coroutines
        StopAllCoroutines();

        // Disable agent behavior
        agent.enabled = false;

        // Stop agent
        NavAgentCommand(agent, true, 0f, anim, Globals.RabbitAnimName, STATE_DIE);

        // Start decay routine
        StartCoroutine(base.DecayAnimal());

        // Destroy after a few seconds
        Destroy(this.gameObject, 50f);
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
            new KeyValuePair<int, string>(STATE_BREED, "Breeding"),
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

    public void SetCurrentState(int newState)
    {
        currentState = newState;
    }

    public void ActivatePanel()
    {
        spc.Unhide();
    }
}
