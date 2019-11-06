using MLAgents;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatAcademy : Academy
{
    public override void AcademyReset()
    {
        Vector3 resetPosition1 = new Vector3(-39.6f, -0.5f, 56.5f);
        Vector3 resetPosition2 = new Vector3(-8.8f, -0.5f, 41f);

        // Reset ML ControlAgents Spawn with randomized positions between resetPosition1 and resetPosition2
        foreach (MLContolAgent targetAgent in FindObjectsOfType<MLContolAgent>())
        {
            // Randomize Spawn locations


            if (targetAgent.transform.GetSiblingIndex() % 2 == 1)
            {
                targetAgent.transform.localPosition = new Vector3(resetPosition1.x - Random.Range(-2f, 2f), resetPosition1.y, resetPosition1.z - Random.Range(-2f, 2f));
            }
            else
            {
                targetAgent.transform.localPosition = new Vector3(resetPosition2.x - Random.Range(-2f, 2f), resetPosition2.y, resetPosition2.z - Random.Range(-2f, 2f));
            }


            // ResetHealth
            targetAgent.health = 50;
            targetAgent.Reset();
        }
        // Reset FSM ControlAgents
        foreach (PlayerControlScript targetAgent in FindObjectsOfType<PlayerControlScript>())
        {
            // Randomize Spawn locations


            if (targetAgent.transform.GetSiblingIndex() % 2 == 1)
            {
                targetAgent.transform.localPosition = new Vector3(resetPosition1.x - Random.Range(-2f, 2f), resetPosition1.y, resetPosition1.z - Random.Range(-2f, 2f));

            }
            else
            {
                targetAgent.transform.localPosition = new Vector3(resetPosition2.x - Random.Range(-2f, 2f), resetPosition2.y, resetPosition2.z - Random.Range(-2f, 2f));
            }


            // ResetHealth
            targetAgent.health = 50;
            targetAgent.Reset();
        }
        // Reset ML ControlAgents
        foreach (FSMControlAgent targetAgent in FindObjectsOfType<FSMControlAgent>())
        {
            // Randomize Spawn locations
            if (targetAgent.transform.GetSiblingIndex() % 2 == 1)
            {
                targetAgent.transform.localPosition = new Vector3(resetPosition1.x - Random.Range(-2f, 2f), resetPosition1.y, resetPosition1.z - Random.Range(-2f, 2f));

            }
            else
            {
                targetAgent.transform.localPosition = new Vector3(resetPosition2.x - Random.Range(-2f, 2f), resetPosition2.y, resetPosition2.z - Random.Range(-2f, 2f));
            }

            targetAgent.transform.localRotation = Quaternion.Euler(new Vector3(0f, Random.Range(0,359) , 0f));

            // ResetHealth
            targetAgent.health = 50;
            targetAgent.Reset();
        }
    }


    // Start is called before the first frame update
    //void Start()
    //{

    //}

    //// Update is called once per frame
    //void Update()
    //{

    //}
}
