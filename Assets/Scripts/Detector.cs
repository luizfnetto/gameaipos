using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Detector : MonoBehaviour
{
    public GameObject roomStateMachine;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        RoomStateMachine scriptRef = roomStateMachine.GetComponent<RoomStateMachine>();
        if (scriptRef.GetCurrentState() == RoomStateMachine.StateId.NotStarted)
            scriptRef.SetState(RoomStateMachine.StateId.Hacking);
    }
}
