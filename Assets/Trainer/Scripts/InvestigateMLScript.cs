using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvestigateMLScript : MonoBehaviour
{
    MLContolAgent parentAgent;
    string playerTag = "Player";
    string MLTag = "FSMPlayer";

    // Start is called before the first frame update
    void Start()
    {
        parentAgent = transform.GetComponentInParent<MLContolAgent>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == parentAgent.tagToTarget)
        {
            parentAgent.OnInvestigateAreaEnter(other.gameObject);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == parentAgent.tagToTarget)
        {
            parentAgent.SetTarget(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == parentAgent.tagToTarget)
        {
            parentAgent.OnInvestigateAreaExit(other.gameObject);
        }
    }
}
