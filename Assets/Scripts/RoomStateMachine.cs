using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomStateMachine : MonoBehaviour
{
    public GameObject entranceDoor;
    public GameObject terminal;

    private StateId _currentState; 

    public enum StateId
    {
        NotStarted,
        Hacking,
        TerminalHacked,
        GameOver
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        switch (_currentState)
        {
            case StateId.NotStarted:
                break;
            case StateId.Hacking:
                Hacking();
                break;
            case StateId.TerminalHacked:
                break;
            case StateId.GameOver:
                break;
        }
    }

    public StateId GetCurrentState ()
    {
        return _currentState;
    }

    public void SetState (StateId state)
    {
        _currentState = state;
    }

    private void Hacking ()
    {
        Door doorSpt = entranceDoor.GetComponent<Door>();
        if (!doorSpt.IsClosed())
            doorSpt.Close();

        Terminal terminalSpt = terminal.GetComponent<Terminal>();
        terminalSpt.StartHacking();
    }
}
