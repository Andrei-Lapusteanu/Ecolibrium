using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AnimalCounter : MonoBehaviour
{
    static int rabbitDeathsTotalCounter = 0;
    static int rabbitDeathsByAgeCounter = 0;
    static int rabbitDeathsByHungerCounter = 0;
    static int rabbitDeathsByBeingEatenCounter = 0;

    public static int RabbitDeathsTotal { get => rabbitDeathsTotalCounter; set => rabbitDeathsTotalCounter = value; }
    public static int RabbitDeathsByAge { get => rabbitDeathsByAgeCounter; set => rabbitDeathsByAgeCounter = value; }
    public static int RabbitDeathsByHunger { get => rabbitDeathsByHungerCounter; set => rabbitDeathsByHungerCounter = value; }
    public static int RabbitDeathsByBeingEaten { get => rabbitDeathsByBeingEatenCounter; set => rabbitDeathsByBeingEatenCounter = value; }

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

            int rabbitMaleCounter = 0;
            int rabbitFemaleCounter = 0;
            float rabbitMalePercentage = 0f;
            float rabbitFemalePercentage = 0f;

            float rabbitMedianHPCounter = 0f;
            float rabbitMedianSatietyCounter = 0f;
            float rabbitMedianEnergyCounter = 0f;
            float rabbitMedianSpeedCounter = 0f;
            float rabbitMedianAgeCounter = 0f;
            float rabbitMedianFoodSightCounter = 0f;

            float rabbitsKilledByAgePercentage = 0f;
            float rabbitsKilledByHungerPercentage = 0f;
            float rabbitsKilledByBeingEatenPercentage = 0f;

            // Get all game objects of ype Rabbit
            Rabbit[] rabbits = FindObjectsOfType<Rabbit>();

            // Get all game objects of ype Rabbit
            Wolf[] wolves = FindObjectsOfType<Wolf>();

            // Set alive rabbits counter
            aliveRabbitsCounter = rabbits.Length;

            // Set total rabbits counter
            totalRabbitsCounter = WorldLimits.RabbitCounter;

            // Foreach rabbit in scene
            foreach (Rabbit rabbit in rabbits)
            {
                if (rabbit.Gender == Gender.Female)
                    rabbitFemaleCounter++;
                else
                    rabbitMaleCounter++;

                // Accumulate values from each rabbit
                if(rabbit.IsAdult() == AdultState.Adult)
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
                rabbitMedianFoodSightCounter += rabbit.MaxFoodSight;
            }

            // Divide statistic values by number of alive rabbits
            // to get median values
            rabbitMedianHPCounter /= aliveRabbitsCounter;
            rabbitMedianSatietyCounter /= aliveRabbitsCounter;
            rabbitMedianEnergyCounter /= aliveRabbitsCounter;
            rabbitMedianSpeedCounter /= aliveRabbitsCounter;
            rabbitMedianAgeCounter /= aliveRabbitsCounter;
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
            GameObject.Find("TextFoodSightRabbits").GetComponent<TextMeshProUGUI>().text = rabbitMedianFoodSightCounter.ToString("0.0");

            GameObject.Find("TextDeathByAgeCounterRabbits").GetComponent<TextMeshProUGUI>().text = rabbitsKilledByAgePercentage.ToString("0.0") + "%";
            GameObject.Find("TextDeathByHungerCounterRabbits").GetComponent<TextMeshProUGUI>().text = rabbitsKilledByHungerPercentage.ToString("0.0") + "%";
            GameObject.Find("TextDeathByBeingEatenCounterRabbits").GetComponent<TextMeshProUGUI>().text = rabbitsKilledByBeingEatenPercentage.ToString("0.0") + "%";


            yield return new WaitForSeconds(1f);
        }
    }
}
