using System;
using UnityEngine;

public class State : MonoBehaviour
{
    public event Action<string> SwitchStateRequested;

    public virtual void OnEnter()
    {

    }

    public virtual void OnTick()
    {

    }

    public virtual void OnExit()
    {

    }

    protected void SwitchState(string stateName)
    {
        SwitchStateRequested?.Invoke(stateName);
    }
}
