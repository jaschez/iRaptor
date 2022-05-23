using System.Collections;
using System.Collections.Generic;
using Aarthificial.Reanimation;
using UnityEngine;

public class NumbrianRenderer : MonoBehaviour
{
    private static class Drivers
    {
        public const string IsAlert = "isAlert";
        public const string ActionState = "actionState";
    }

    private Reanimator _reanimator;
    private EnemyModule _controller;

    private void Awake()
    {
        _reanimator = GetComponent<Reanimator>();
        _controller = GetComponent<EnemyModule>();
    }

    private void Update()
    {
        _reanimator.Flip = _controller.MoverComponent.Direction.x < 0;
        _reanimator.Set(Drivers.IsAlert, _controller.PlayerSeen);
        _reanimator.Set(Drivers.ActionState, (int)_controller.StateMachine.CurrentState.StateType);
    }
}
