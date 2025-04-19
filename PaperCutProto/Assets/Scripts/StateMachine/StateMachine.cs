using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    [SerializeField] private State _currentState;
    private Dictionary<string, State> _states = new Dictionary<string, State>();

    private void Awake()
    {
        var states = GetComponentsInChildren<State>();

        foreach (State state in states)
        {
            state.SwitchStateRequested += OnSwitchStateRequested;
            _states.Add(state.name, state);
        }

        if (_currentState == null && states.Length > 0)
        {
            _currentState = states[0];
        }
    }

    private void Update()
    {
        if (_currentState)
        {
            _currentState.OnTick();
        }
    }

    public void OnSwitchStateRequested(string stateName)
    {
        if (_states.ContainsKey(stateName))
        {
            if (_currentState != null)
            {
                _currentState.OnExit();
            }
            var newState = _states[stateName];
            _currentState = newState;
            _currentState.OnEnter();
        }
        else
        {
            Debug.LogError("State ${stateName} not found");
        }
    }

}
