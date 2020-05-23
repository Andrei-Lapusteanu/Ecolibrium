using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AnimalCounter : MonoBehaviour
{
    public static int RabbitCounter = 0;
    public static int WolvesCounter = 0;
    public static int RabbitsAlive = 0;
    public static int WolvesAlive = 0;

    static int rabbitDeathsTotalCounter = 0;
    static int rabbitDeathsByAgeCounter = 0;
    static int rabbitDeathsByHungerCounter = 0;
    static int rabbitDeathsByBeingEatenCounter = 0;
    static int wolfDeathsTotalCounter = 0;
    static int wolfDeathsByAgeCounter = 0;
    static int wolfDeathsByHungerCounter = 0;

    public static int RabbitDeathsTotal { get => rabbitDeathsTotalCounter; set => rabbitDeathsTotalCounter = value; }
    public static int RabbitDeathsByAge { get => rabbitDeathsByAgeCounter; set => rabbitDeathsByAgeCounter = value; }
    public static int RabbitDeathsByHunger { get => rabbitDeathsByHungerCounter; set => rabbitDeathsByHungerCounter = value; }
    public static int RabbitDeathsByBeingEaten { get => rabbitDeathsByBeingEatenCounter; set => rabbitDeathsByBeingEatenCounter = value; }
    public static int WolfDeathsTotal { get => wolfDeathsTotalCounter; set => wolfDeathsTotalCounter = value; }
    public static int WolfDeathsByAge { get => wolfDeathsByAgeCounter; set => wolfDeathsByAgeCounter = value; }
    public static int WolfDeathsByHunger { get => wolfDeathsByHungerCounter; set => wolfDeathsByHungerCounter = value; }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(UpdateAnimalCount());
    }

    public IEnumerator UpdateAnimalCount()
    {
        for (; ; )
        {
            int aliveRabbitsCounter = 0;
            int totalRabbitsCounter = 0;
            int aliveWolvesCounter = 0;
            int totalWolvesCounter = 0;

            int rabbitMaleCounter = 0;
            int rabbitFemaleCounter = 0;
            float rabbitMalePercentage = 0f;
            float rabbitFemalePercentage = 0f;
            int wolfMaleCounter = 0;
            int wolfFemaleCounter = 0;
            float wolfMalePercentage = 0f;
            float wolfFemalePercentage = 0f;

            float rabbitMedianHPCounter = 0f;
            float rabbitMedianSatietyCounter = 0f;
            float rabbitMedianEnergyCounter = 0f;
            float rabbitMedianSpeedCounter = 0f;
            float rabbitMedianAgeCounter = 0f;
            float rabbitMedianReproduceSpeedCounter = 0f;
            float rabbitMedianFoodSightCounter = 0f;
            float wolfMedianHPCounter = 0f;
            float wolfMedianSatietyCounter = 0f;
            float wolfMedianEnergyCounter = 0f;
            float wolfMedianSpeedCounter = 0f;
            float wolfMedianAgeCounter = 0f;
            float wolfMedianReproduceSpeedCounter = 0f;
            float wolfMedianFoodSightCounter = 0f;

            float rabbitsKilledByAgePercentage = 0f;
            float rabbitsKilledByHungerPercentage = 0f;
            float rabbitsKilledByBeingEatenPercentage = 0f;
            float wolvesKilledByAgePercentage = 0f;
            float wolvesKilledByHungerPercentage = 0f;

            // Get all game objects of ype Rabbit
            Rabbit[] rabbits = FindObjectsOfType<Rabbit>();

            // Get all game objects of ype Rabbit
            Wolf[] wolves = FindObjectsOfType<Wolf>();

            // Set alive rabbits counter
            aliveRabbitsCounter = rabbits.Length;
            aliveWolvesCounter = wolves.Length;
            RabbitsAlive = aliveRabbitsCounter;
            WolvesAlive = aliveWolvesCounter;

            // Set total rabbits counter
            totalRabbitsCounter = RabbitCounter;
            totalWolvesCounter = WolvesCounter;

            // Foreach rabbit in scene
            foreach (Rabbit rabbit in rabbits)
            {
                if (rabbit.Gender == Gender.Female)
                    rabbitFemaleCounter++;
                else
                    rabbitMaleCounter++;

                // Accumulate values from each rabbit
                if (rabbit.IsAdult() == AdultState.Adult)
                {
                    rabbitMedianHPCounter += rabbit.MaxHealthPoints;
                    rabbitMedianSatietyCounter += rabbit.MaxSatiety;
                    rabbitMedianSpeedCounter += rabbit.MaxSpeed;
                }
                else
                {
                    // Adjust medial values of rabbits are not adults (for consistency)
                    rabbitMedianHPCounter += (rabbit.MaxHealthPoints * 2);
                    rabbitMedianSatietyCounter += (rabbit.MaxSatiety * 2);
                    rabbitMedianSpeedCounter += (rabbit.MaxSpeed / 0.75f);
                }

                // Common attribs for adult and cub
                rabbitMedianEnergyCounter += rabbit.MaxEnergy;
                rabbitMedianAgeCounter += rabbit.MaxAge;
                rabbitMedianReproduceSpeedCounter += rabbit.ReproduceSpeed;
                rabbitMedianFoodSightCounter += rabbit.MaxFoodSight;
            }

            // Divide statistic values by number of alive rabbits
            // to get median values
            rabbitMedianHPCounter /= aliveRabbitsCounter;
            rabbitMedianSatietyCounter /= aliveRabbitsCounter;
            rabbitMedianEnergyCounter /= aliveRabbitsCounter;
            rabbitMedianSpeedCounter /= aliveRabbitsCounter;
            rabbitMedianAgeCounter /= aliveRabbitsCounter;
            rabbitMedianReproduceSpeedCounter /= aliveRabbitsCounter;
            rabbitMedianFoodSightCounter /= aliveRabbitsCounter;

            // Get percentage of male rabbits
            rabbitMalePercentage = ((float)rabbitMaleCounter / (float)aliveRabbitsCounter) * 100f;

            // Get percentage of female rabbits
            rabbitFemalePercentage = ((float)rabbitFemaleCounter / (float)aliveRabbitsCounter) * 100f;

            // Get percentage of animals rabbits killed by old age
            rabbitsKilledByAgePercentage = ((float)rabbitDeathsByAgeCounter / (float)rabbitDeathsTotalCounter) * 100f;

            // Get percentage of animals rabbits killed by hunger
            rabbitsKilledByHungerPercentage = ((float)rabbitDeathsByHungerCounter / (float)rabbitDeathsTotalCounter) * 100f;

            // Get percentage of animals rabbits killed by being eaten
            rabbitsKilledByBeingEatenPercentage = ((float)rabbitDeathsByBeingEatenCounter / (float)rabbitDeathsTotalCounter) * 100f;

            // Disaply values on UI panel
            GameObject.Find("TextAliveCounterRabbits").GetComponent<TextMeshProUGUI>().text = aliveRabbitsCounter.ToString();
            GameObject.Find("TextTotalCounterRabbits").GetComponent<TextMeshProUGUI>().text = totalRabbitsCounter.ToString();
            GameObject.Find("TextMaleCounterRabbits").GetComponent<TextMeshProUGUI>().text = rabbitMalePercentage.ToString("0.0") + "%";
            GameObject.Find("TextFemaleCounterRabbits").GetComponent<TextMeshProUGUI>().text = rabbitFemalePercentage.ToString("0.0") + "%";

            GameObject.Find("TextHPRabbits").GetComponent<TextMeshProUGUI>().text = rabbitMedianHPCounter.ToString("0.0");
            GameObject.Find("TextSatietyRabbits").GetComponent<TextMeshProUGUI>().text = rabbitMedianSatietyCounter.ToString("0.0");
            GameObject.Find("TextEnergyRabbits").GetComponent<TextMeshProUGUI>().text = rabbitMedianEnergyCounter.ToString("0.0");
            GameObject.Find("TextSpeedRabbits").GetComponent<TextMeshProUGUI>().text = rabbitMedianSpeedCounter.ToString("0.00");
            GameObject.Find("TextAgeRabbits").GetComponent<TextMeshProUGUI>().text = rabbitMedianAgeCounter.ToString("0.00");
            GameObject.Find("TextReproduceSpeedRabbits").GetComponent<TextMeshProUGUI>().text = rabbitMedianReproduceSpeedCounter.ToString("0.00");
            GameObject.Find("TextFoodSightRabbits").GetComponent<TextMeshProUGUI>().text = rabbitMedianFoodSightCounter.ToString("0.0");

            GameObject.Find("TextDeathByAgeCounterRabbits").GetComponent<TextMeshProUGUI>().text = rabbitsKilledByAgePercentage.ToString("0.0") + "%";
            GameObject.Find("TextDeathByHungerCounterRabbits").GetComponent<TextMeshProUGUI>().text = rabbitsKilledByHungerPercentage.ToString("0.0") + "%";
            GameObject.Find("TextDeathByBeingEatenCounterRabbits").GetComponent<TextMeshProUGUI>().text = rabbitsKilledByBeingEatenPercentage.ToString("0.0") + "%";

            // Foreach wolf in scene
            foreach (Wolf wolf in wolves)
            {
                if (wolf.Gender == Gender.Female)
                    wolfFemaleCounter++;
                else
                    wolfMaleCounter++;

                // Accumulate values from each wolf
                if (wolf.IsAdult() == AdultState.Adult)
                {
                    wolfMedianHPCounter += wolf.MaxHealthPoints;
                    wolfMedianSatietyCounter += wolf.MaxSatiety;
                    wolfMedianSpeedCounter += wolf.MaxSpeed;
                }
                else
                {
                    // Adjust medial values of wolves are not adults (for consistency)
                    wolfMedianHPCounter += (wolf.MaxHealthPoints * 2);
                    wolfMedianSatietyCounter += (wolf.MaxSatiety * 2);
                    wolfMedianSpeedCounter += (wolf.MaxSpeed / 0.75f);
                }

                // Common attribs for adult and cub
                wolfMedianEnergyCounter += wolf.MaxEnergy;
                wolfMedianAgeCounter += wolf.MaxAge;
                wolfMedianReproduceSpeedCounter += wolf.ReproduceSpeed;
                wolfMedianFoodSightCounter += wolf.MaxFoodSight;
            }

            // Divide statistic values by number of alive wolves
            // to get median values
            wolfMedianHPCounter /= aliveWolvesCounter;
            wolfMedianSatietyCounter /= aliveWolvesCounter;
            wolfMedianEnergyCounter /= aliveWolvesCounter;
            wolfMedianSpeedCounter /= aliveWolvesCounter;
            wolfMedianAgeCounter /= aliveWolvesCounter;
            wolfMedianReproduceSpeedCounter /= aliveWolvesCounter;
            wolfMedianFoodSightCounter /= aliveWolvesCounter;

            // Get percentage of male wolves
            wolfMalePercentage = ((float)wolfMaleCounter / (float)aliveWolvesCounter) * 100f;

            // Get percentage of female wolves
            wolfFemalePercentage = ((float)wolfFemaleCounter / (float)aliveWolvesCounter) * 100f;

            // Get percentage of animals wolves killed by old age
            wolvesKilledByAgePercentage = ((float)wolfDeathsByAgeCounter / (float)wolfDeathsTotalCounter) * 100f;

            // Get percentage of animals wolves killed by hunger
            wolvesKilledByHungerPercentage = ((float)wolfDeathsByHungerCounter / (float)wolfDeathsTotalCounter) * 100f;

            // Disaply values on UI panel
            GameObject.Find("TextAliveCounterWolves").GetComponent<TextMeshProUGUI>().text = aliveWolvesCounter.ToString();
            GameObject.Find("TextTotalCounterWolves").GetComponent<TextMeshProUGUI>().text = totalWolvesCounter.ToString();
            GameObject.Find("TextMaleCounterWolves").GetComponent<TextMeshProUGUI>().text = wolfMalePercentage.ToString("0.0") + "%";
            GameObject.Find("TextFemaleCounterWolves").GetComponent<TextMeshProUGUI>().text = wolfFemalePercentage.ToString("0.0") + "%";

            GameObject.Find("TextHPWolves").GetComponent<TextMeshProUGUI>().text = wolfMedianHPCounter.ToString("0.0");
            GameObject.Find("TextSatietyWolves").GetComponent<TextMeshProUGUI>().text = wolfMedianSatietyCounter.ToString("0.0");
            GameObject.Find("TextEnergyWolves").GetComponent<TextMeshProUGUI>().text = wolfMedianEnergyCounter.ToString("0.0");
            GameObject.Find("TextSpeedWolves").GetComponent<TextMeshProUGUI>().text = wolfMedianSpeedCounter.ToString("0.00");
            GameObject.Find("TextAgeWolves").GetComponent<TextMeshProUGUI>().text = wolfMedianAgeCounter.ToString("0.00");
            GameObject.Find("TextReproduceSpeedWolves").GetComponent<TextMeshProUGUI>().text = wolfMedianReproduceSpeedCounter.ToString("0.00");
            GameObject.Find("TextFoodSightWolves").GetComponent<TextMeshProUGUI>().text = wolfMedianFoodSightCounter.ToString("0.0");

            GameObject.Find("TextDeathByAgeCounterWolves").GetComponent<TextMeshProUGUI>().text = wolvesKilledByAgePercentage.ToString("0.0") + "%";
            GameObject.Find("TextDeathByHungerCounterWolves").GetComponent<TextMeshProUGUI>().text = wolvesKilledByHungerPercentage.ToString("0.0") + "%";

            yield return new WaitForSeconds(1f);
        }
    }
}
