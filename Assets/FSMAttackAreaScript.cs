using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSMAttackAreaScript : MonoBehaviour
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
        if (other.tag == playerTag || other.tag == MLTag)
        {
            parentAgent.OnAttackAreaEnter();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == playerTag || other.tag == MLTag)
        {
            parentAgent.OnAttackAreaExit();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == playerTag || other.tag == MLTag)
        {
            parentAgent.OnAttackAreaStay();
        }
    }
}
