using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    public State[] EntryStates { get; private set; }
    State currentState;

    System.Action OnStateStart;

    Coroutine currentCoroutine;

    float stateTime;
    float timer;

    public void Init(List<State> entryStates, System.Action StateStart)
    {
        EntryStates = entryStates.ToArray();
        State selectedState = EntryStates[Random.Range(0, EntryStates.Length - 1)];
        OnStateStart = StateStart;

        currentCoroutine = StartCoroutine(DoState(selectedState));
    }

    public void ResetTimer(float min, float max)
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
        currentState = state;
        stateTime = Random.Range(state.MinCompletionTime, state.MaxCompletionTime);
        timer = Time.time + stateTime;

        OnStateStart.Invoke();
    }

    void NextState()
    {
        State selectedState;

        if (currentState.TriggerState == null) {
            State[] possibleStates = currentState.NextStates.ToArray();
            selectedState = possibleStates[Random.Range(0, possibleStates.Length - 1)];
        }
        else
        {
            selectedState = currentState.TriggerState;
            currentState.ResetTriggerState();
        }

        currentCoroutine = StartCoroutine(DoState(selectedState));
    }

    private void OnEnable()
    {
        currentCoroutine = StartCoroutine(DoState(currentState));
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
