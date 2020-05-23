using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantGrowController : MonoBehaviour
{
    //private const float GROW_RATE = 0.001f;
    private const float GROW_RATE = 0.0325f;

    private Coroutine growBackRoutine;
    private Vector3 targetScale;
    private float growBackRate;
    private bool isEatable;

    private bool canAdultEat;
    private bool canCubEat;

    // Start is called before the first frame update
    void Start()
    {
        targetScale = transform.localScale;
        growBackRate = GROW_RATE * targetScale.x;
        isEatable = true;

        canAdultEat = true;
        canCubEat = true;
    }

    public IEnumerator GrowBack()
    {
        for (; ; )
        {
            // If plant isn't fully grows
            //if (Vector3.Distance(transform.localScale, targetScale) >= 0.05f)
            if (transform.localScale.x <= targetScale.x)
            {
                // Animate plant growth
                transform.localScale += new Vector3(growBackRate, growBackRate, growBackRate);

                // If plant is half grown, cub can eat it
                if (transform.localScale.x >= (targetScale.x / 2f))
                    canCubEat = true;
                    
            }
            // Plant fully grown
            else
            {
                transform.localScale = targetScale;

                // Adult can eat it (if plan fully grown)
                canAdultEat = true;
                yield break;
            }

            //yield return new WaitForSeconds(1f / (Time.timeScale * 10));
            yield return new WaitForSeconds(1f);
        }
    }

    public void GetEaten(AdultState adultState)
    {
        if(adultState == AdultState.Adult)
        {
            if (canAdultEat == true)
            {
                if (growBackRoutine != null)
                    StopCoroutine(growBackRoutine);

                transform.localScale = Vector3.zero;
                growBackRoutine = StartCoroutine(GrowBack());
                canAdultEat = false;
                canCubEat = false;
            }
        }
        else if(adultState == AdultState.NotAdult)
        {
            if (canCubEat == true)
            {
                if (growBackRoutine != null)
                    StopCoroutine(growBackRoutine);

                transform.localScale -= (targetScale / 2f);
                growBackRoutine = StartCoroutine(GrowBack());
                canAdultEat = false;
                canCubEat = false;
            }
        }
    }

    public bool IsEatable { get => isEatable; set => isEatable = value; }
    public bool CanAdultEat { get => canAdultEat; set => canAdultEat = value; }
    public bool CanCubEat { get => canCubEat; set => canCubEat = value; }
}
