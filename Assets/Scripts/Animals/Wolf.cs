using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Wolf : AnimalController
{
    private const int STATE_CREEP = -1;
    private const int STATE_IDLE = 0;
    private const int STATE_WALK = 1;
    private const int STATE_RUN = 2;
    private const int STATE_LOOK_FOR_FOOD = 4;
    private const int STATE_GO_AFTER_FOOD = 5;
    private const int STATE_EAT = 6;
    private const int STATE_LOOK_FOR_MATE = 7;
    private const int STATE_GO_AFTER_MATE = 8;
    private const int STATE_BREED = 9;
    private const int STATE_SLEEP = 10;
    private const int STATE_DIE = 11;
    private const int STATE_SIT_DOWN = 12;


    public Gender genderUI;
    private bool testing = false;

    public GameObject spawnParticleEffect;
    Animator anim;
    NavMeshAgent agent;
    StatusPanelController spc;
    List<KeyValuePair<int, string>> states;
    int currentState = 0;

    // Start is called before the first frame update
    void Start()
    {
        // Get components
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        // Set animal type
        base.Species = AnimalType.Wolf;

        if(testing == true)
        {
            base.Gender = this.genderUI;

            // TODO - remove param
            base.SetAttributesLimits(AnimalType.Wolf);

            // TODO - remove param
            base.CreateAnimalRandAttributes(AnimalType.Wolf, base.Gender, AnimalCounter.WolvesCounter++);
        }

        // Set init anim state
        SetAnimState(anim, Globals.WolfAnimName, STATE_IDLE);

        // Start coroutines
        StartCoroutine(StopAgentRoutine(agent, anim, Globals.WolfAnimName, STATE_IDLE));
        StartCoroutine(ModifyAttribsOverTime(AnimalType.Wolf));
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
            StatusPanelController.UpdateStaticElementsOnce(spc, Resources.Load<Sprite>("_Mine/Images/wolf_icon"), "Wolf " + GetGenderString(this.Gender));

            // Update dynamic panel elements
            UpdateStatusPanelValues(spc, states, currentState, Vector3.Magnitude(agent.velocity), foodTarget);
        }
        else spc.Hide();
    }

    protected override IEnumerator MainAI()
    {
        for (; ; )
        {
            // TODO - Testing 
            this.genderUI = base.Gender;

            if (currentState == STATE_DIE)
                break;

            if (currentState == STATE_IDLE || currentState == STATE_WALK || currentState == STATE_RUN || currentState == STATE_SIT_DOWN || currentState == STATE_CREEP || currentState == STATE_SLEEP || currentState == STATE_LOOK_FOR_MATE)
            {
                if (IsHungry() == HungerState.VeryHungry || (IsHungry() == HungerState.Hungry && IsTired() != TirednessState.VeryTired))
                    currentState = STATE_LOOK_FOR_FOOD;

                else if (base.Gender == Gender.Male && GetReproduceState() == ReproduceState.ReproduceTrue && GetReproduceState() == ReproduceState.ReproduceTrue && (IsHungry() != HungerState.VeryHungry && IsTired() != TirednessState.VeryTired))
                    currentState = STATE_LOOK_FOR_MATE;

                else if (IsTired() == TirednessState.VeryTired || (IsTired() == TirednessState.Tired && IsHungry() == HungerState.NotHungry))
                    currentState = STATE_SLEEP;

                else if (IsHungry() == HungerState.NotHungry && IsTired() == TirednessState.NotTired)
                    // TODO - adjust
                    currentState = Random.Range(-1, 2);
            }

            switch (currentState)
            {
                case STATE_IDLE:

                    base.Idle(agent, true, anim, Globals.WolfAnimName, STATE_IDLE);

                    yield return new WaitForSeconds(Random.Range(2f, 4f));
                    break;

                case STATE_SIT_DOWN:

                    SitDown();
                    Debug.DrawLine(transform.position + Vector3.up, agent.destination, Color.green, 4);

                    yield return new WaitForSeconds(Random.Range(2f, 4f));
                    break;

                case STATE_WALK:

                    base.Walk(agent, false, anim, Globals.WolfAnimName, STATE_WALK);

                    yield return new WaitForSeconds(Random.Range(2f, 4f));
                    break;

                case STATE_RUN:

                    base.Run(agent, false, anim, Globals.WolfAnimName, STATE_RUN);

                    yield return new WaitForSeconds(Random.Range(2f, 4f));
                    break;

                case STATE_CREEP:

                    Creep();
                    Debug.DrawLine(transform.position + Vector3.up, agent.destination, Color.green, 4);

                    yield return new WaitForSeconds(Random.Range(2f, 4f));
                    break;

                case STATE_LOOK_FOR_FOOD:

                    base.StateLookForFood(SetCurrentState, agent, anim, Globals.WolfAnimName, Tags.Rabbit);
                    break;

                case STATE_GO_AFTER_FOOD:

                    base.StateGoAfterFood(SetCurrentState, agent, anim, Globals.WolfAnimName);
                    break;

                case STATE_EAT:

                    if(base.foodTarget != null && base.foodTarget.GetComponent<Rabbit>().IsAdult() == AdultState.Adult)
                        base.Eat(SetCurrentState, STATE_IDLE, 20.0f, Tags.Wolf);
                    else if (base.foodTarget != null && base.foodTarget.GetComponent<Rabbit>().IsAdult() == AdultState.NotAdult)
                        base.Eat(SetCurrentState, STATE_IDLE, 10.0f, Tags.Wolf);

                    break;

                case STATE_LOOK_FOR_MATE:

                    base.StateLookForMate(SetCurrentState, agent, anim, Globals.WolfAnimName);
                    break;


                case STATE_GO_AFTER_MATE:

                    base.StateGoAfterMate(SetCurrentState, agent, anim, Globals.WolfAnimName);
                    break;

                case STATE_BREED:

                    Breed(SetCurrentState);
                    break;

                case STATE_SLEEP:

                    Sleep(agent, true, anim, Globals.WolfAnimName, STATE_SIT_DOWN);
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

            if (currentState != STATE_SLEEP)
                base.isSleeping = false;

            Debug.DrawLine(agent.transform.position + Vector3.up, agent.destination, Color.green, 2);

            if (foodTarget != null)
                Debug.DrawLine(transform.position + Vector3.up, foodTarget.transform.position, Color.red, 2);

            yield return new WaitForSeconds(0.25f);
        }
    }

    private void SitDown()
    {
        NavAgentCommand(agent, true, agent.speed, anim, Globals.WolfAnimName, STATE_SIT_DOWN);
    }

    protected void Creep()
    {
        NavAgentCommand(agent, false, GetWalkSpeed(), anim, Globals.WolfAnimName, STATE_CREEP);

        WalkToRandPos(agent, GetWalkSpeed(), 20.0f);
    }

    protected override void Die()
    {
        // Set current state
        currentState = STATE_DIE;
        base.IsAlive = false;
        AnimalCounter.WolfDeathsTotal++;

        // Stop all coroutines
        StopAllCoroutines();

        // Disable agent behavior
        agent.enabled = false;
        
        // TODO - anim state
        // Stop agent
        NavAgentCommand(agent, true, 0f, anim, Globals.RabbitAnimName, STATE_SIT_DOWN);

        // Start decay routine
        StartCoroutine(base.DecayAnimal());

        // Destroy after a few seconds
        Destroy(this.gameObject, 0f);
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
        Destroy(partileInst, 2.0f);
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
