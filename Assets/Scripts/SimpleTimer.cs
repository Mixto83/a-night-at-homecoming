using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleTimer 
{
    private float count;
    private float runningCount;
    private bool isStarted;

    public SimpleTimer(float count)
    {
        this.count = count;
        runningCount = count;
        isStarted = false;
}
    // Update is called once per frame
    public void Update()
    {
        if (!isFinished() && isStarted)
        { 
            runningCount -= Time.deltaTime;
        } else if (isFinished())
        {
            isStarted = false;
        }
    }

    public bool isFinished()
    {
       if(runningCount > 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public void start()
    {
        isStarted = true;
    }

    public void reset(float newCount = -1)
    {
        if(newCount != -1) count = newCount;
        runningCount = count;
        start();
    }

}
