using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantWolfScanner : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(ScanForWolves());
    }

    private IEnumerator ScanForWolves()
    {
        float searchRadius = 10.0f;

        for(; ; )
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, searchRadius, -1, QueryTriggerInteraction.Collide);
            GameObject wolfTarget = null;

            foreach (Collider collider in colliders)
                if (collider.tag == Tags.Wolf && collider.gameObject.GetComponent<Wolf>().IsAlive == true)
                {
                    GetComponent<PlantGrowController>().IsEatable = false;
                    wolfTarget = collider.gameObject;
                }

            if(wolfTarget == null)
                GetComponent<PlantGrowController>().IsEatable = true;

            yield return new WaitForSeconds(1f);
        }

    }
}
