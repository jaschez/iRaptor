using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State
{
    public readonly Action AssignedAction;
    public readonly StateMachine.States StateType;
    public List<State> NextStates { get; private set; }
    public State TriggerState { get; private set; } = null;

    public readonly float MinCompletionTime;
    public readonly float MaxCompletionTime;

    public State(Action assignedAction, StateMachine.States stateType, float minCompletionTime, float maxCompletionTime)
    {
        AssignedAction = assignedAction;
        StateType = stateType;
        MinCompletionTime = minCompletionTime;
        MaxCompletionTime = maxCompletionTime;
        NextStates = new List<State>();
    }

    public void AddNextState(State state)
    {
        NextStates.Add(state);
    }

    public void SetTriggerState(State state)
    {
        TriggerState = state;
    }

    public void ResetTriggerState()
    {
        TriggerState = null;
    }
}
