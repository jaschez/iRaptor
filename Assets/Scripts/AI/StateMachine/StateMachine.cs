using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    public State[] EntryStates { get; private set; }
    public State CurrentState { get; private set; }

    //System.Action OnStateStart;

    public delegate void StartedState(State state);
    // Event declaration
    public event StartedState OnStateStart;

    Coroutine currentCoroutine;

    float stateTime;
    float timer;

    public void Init(List<State> entryStates)
    {
        EntryStates = entryStates.ToArray();

        if (EntryStates.Length > 0) {
            State selectedState = EntryStates[Random.Range(0, EntryStates.Length - 1)];
            currentCoroutine = StartCoroutine(DoState(selectedState));
        }
    }

    private void ResetTimer(float min, float max)
    {
        stateTime = Random.Range(min, max);
        timer = Time.time + stateTime;
    }

    IEnumerator DoState(State state)
    {
        while (state == null) yield return null;

        StartState(state);

        if (state.AssignedAction != null)
        {
            while (timer > Time.time)
            {
                state.AssignedAction.Invoke();

                if (state.TriggerState == null) {
                    foreach (StateTrigger stateTrigger in state.TriggerConditions)
                    {
                        if (stateTrigger.Trigger.Invoke())
                        {
                            State resultingState = stateTrigger.ResultingState;
                            ResetTimer(0, 0);
                            state.SetTriggerState(resultingState);
                        }
                    }
                }

                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(timer);
        }

        NextState();
    }

    void StartState(State state)
    {
        CurrentState = state;
        stateTime = Random.Range(state.MinCompletionTime, state.MaxCompletionTime);
        timer = Time.time + stateTime;

        OnStateStart?.Invoke(state);
    }

    void NextState()
    {
        State selectedState;

        if (CurrentState.TriggerState == null) {
            State[] possibleStates = CurrentState.NextStates.ToArray();
            selectedState = possibleStates[Random.Range(0, possibleStates.Length - 1)];
        }
        else
        {
            selectedState = CurrentState.TriggerState;
            CurrentState.ResetTriggerState();
        }

        currentCoroutine = StartCoroutine(DoState(selectedState));
    }

    private void OnEnable()
    {
        currentCoroutine = StartCoroutine(DoState(CurrentState));
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public enum States
    {
        Idle,
        Wandering,
        Attack
    }
}
