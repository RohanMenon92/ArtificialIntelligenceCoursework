using MLAgents;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAgent : Agent
{
    [Header("Pig Agent Settings")]
    public float moveSpeed = 1f;
    public float rotateSpeed = 2f;
    public float nostrilWidth = .5f;

    private EnemyAcademy agentAcademy;
    private PlaySpace agentArea;
    private Rigidbody agentRigidbody;
    private RayPerception rayPerception;

    private int trufflesCollected = 0;
    public override void InitializeAgent()
    {
        base.InitializeAgent();
        agentAcademy = FindObjectOfType<EnemyAcademy>();
        agentArea = transform.parent.GetComponent<PlaySpace>();
        agentRigidbody = GetComponent<Rigidbody>();
        rayPerception = GetComponent<RayPerception>();
    }

    public override void CollectObservations()
    {

        // Add raycast perception observations for stumps and walls
        float rayDistance = 20f;
        float[] rayAngles = { 90f };
        string[] detectableObjects = { "stump", "wall" };
        AddVectorObs(rayPerception.Perceive(rayDistance, rayAngles, detectableObjects, 0f, 0f));

        // Sniff for truffles
        //AddVectorObs(GetNostrilStereo());

        // Add velocity observation
        Vector3 localVelocity = transform.InverseTransformDirection(agentRigidbody.velocity);
        AddVectorObs(localVelocity.x);
        AddVectorObs(localVelocity.z);
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        base.AgentAction(vectorAction, textAction);

        if(vectorAction[0] == 1)
        {
            Debug.Log("Forward");
        }
    }
}
