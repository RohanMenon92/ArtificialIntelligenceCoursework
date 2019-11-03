using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvestigteAreaScript : MonoBehaviour
{
    FSMControlAgent parentAgent;
    string playerTag = "Player";
    string MLTag = "MLPlayer";

    // Start is called before the first frame update
    void Start()
    {
        parentAgent = transform.GetComponentInParent<FSMControlAgent>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" || other.tag == "MLPlayer")
        {
            parentAgent.OnInvestigateAreaEnter();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.tag == "Player" || other.tag == "MLPlayer")
        {
            parentAgent.SetTarget(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player" || other.tag == "MLPlayer")
        {
            parentAgent.OnInvestigateAreaExit();
        }
    }
}
