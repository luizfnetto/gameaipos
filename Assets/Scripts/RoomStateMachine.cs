using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class RoomStateMachine : MonoBehaviour
{
    public GameObject entranceDoor;
    public GameObject terminal;
    public GameObject player;
    public GameObject gameOverPanel;
    public GameObject gameWinPanel;

    private StateId _currentState;
    private bool _dead = false;

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
                Win();
                break;
            case StateId.GameOver:
                Die();
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

    private void Win()
    {
        if (!gameWinPanel.active)
        {
            gameWinPanel.SetActive(true);
        }
    }

    private void Die()
    {
        if (!_dead)
        {
            _dead = true;
            FirstPersonController control = player.GetComponent<FirstPersonController>();
            if (control.enabled)
            {
                control.enabled = false;
            }
            if (!gameOverPanel.active)
            {
                gameOverPanel.SetActive(true);
            }
            Cursor.visible = true;
            Screen.lockCursor = false;
        }
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
