using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public Vector3 closedPos;
    public Vector3 openPos;
    public float speed = 10;

    private bool opening = false;
    private bool closing = false;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (opening || closing)
        {
            float step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, GetTarget(), step);
            Check();
        }
    }

    Vector3 GetTarget()
    {
        if (closing)
            return closedPos;
        if (opening)
            return openPos;
        return transform.position;
    }

    void Check ()
    {
        if(transform.position == GetTarget())
        {
            opening = false;
            closing = false;
        }
    }

    public void Close()
    {
        opening = false;
        closing = true;
    }

    public void Open()
    {
        opening = true;
        closing = false;
    }

    public bool IsClosed ()
    {
        return transform.position == closedPos;
    }
}
