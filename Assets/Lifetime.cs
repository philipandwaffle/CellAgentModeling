using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Lifetime : MonoBehaviour
{
    Timer timer;

    [SerializeField]
    int dueTime;

    // Start is called before the first frame update
    void Start()
    {
        timer = new Timer(new TimerCallback(TimeUp));
        timer.Change(dueTime, 0);
    }
    private void TimeUp(object state)
    {        
        Timer t = (Timer)state;
        t.Dispose();
        Debug.Log("Timer Up, destroying");
        Destroy(this);
    }
}
