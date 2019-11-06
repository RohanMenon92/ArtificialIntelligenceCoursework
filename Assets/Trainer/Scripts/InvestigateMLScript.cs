using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvestigateMLScript : MonoBehaviour
{
    MLContolAgent parentAgent;
    List<string> playerTags = new List<string>() { "Player" , "FSMPlayer", "MLPlayer"};

    // Start is called before the first frame update
    void Start()
    {
        parentAgent = transform.GetComponentInParent<MLContolAgent>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (playerTags.Contains(other.tag))
        {
            // Test if it is the same agent
            if (other.transform != transform.parent)
            {
                // Enter investigate area
                parentAgent.OnInvestigateAreaEnter(other.gameObject);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (playerTags.Contains(other.tag))
        {
            if (other.transform != transform.parent)
            {
                // Enter continually update target
                parentAgent.SetTarget(other.gameObject);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (playerTags.Contains(other.tag))
        {
            // Test if it is the same agent
            if (other.transform != transform.parent)
            {
                // On Investigate are exit
                parentAgent.OnInvestigateAreaExit(other.gameObject);
            }
        }
    }
}
