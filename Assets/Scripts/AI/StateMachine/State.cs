using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State
{
    public readonly Action AssignedAction;
    public readonly StateMachine.States StateType;
    public List<State> NextStates { get; private set; }
    public List<StateTrigger> TriggerConditions { get; private set; }
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
        TriggerConditions = new List<StateTrigger>();
    }

    public void AddNextState(State state, Func<bool> condition = null)
    {
        if (condition == null) {
            NextStates.Add(state);
        }
        else
        {
            StateTrigger conditionalState = new StateTrigger(state, condition);
            TriggerConditions.Add(conditionalState);
        }
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

public class StateTrigger
{
    public readonly State ResultingState;
    public Func<bool> Trigger { get; private set; }

    public StateTrigger(State resultingState, Func<bool> condition)
    {
        ResultingState = resultingState;
        Trigger = condition;
    }
}
