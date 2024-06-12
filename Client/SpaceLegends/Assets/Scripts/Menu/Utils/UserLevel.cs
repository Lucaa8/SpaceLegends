using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserLevel
{

    public int Level { get; private set; }
    public int Current { get; private set; }
    public int Max { get; private set; }

    public UserLevel(JArray json)
    {
        this.Level = json[0].Value<int>();
        this.Current = json[1].Value<int>();
        this.Max = json[2].Value<int>();
    }

    public float getRatio()
    {
        return this.Current*1.0F / this.Max;
    }

}
