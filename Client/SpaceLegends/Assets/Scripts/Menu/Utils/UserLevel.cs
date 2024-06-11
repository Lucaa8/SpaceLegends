using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserLevel
{

    public int Level { get; private set; }
    public int Current { get; private set; }
    public int Max { get; private set; }

    public UserLevel(int[] info)
    {
        this.Level = info[0];
        this.Current = info[1];
        this.Max = info[2];
    }

    public float getRatio()
    {
        return this.Current*1.0F / this.Max;
    }

}
