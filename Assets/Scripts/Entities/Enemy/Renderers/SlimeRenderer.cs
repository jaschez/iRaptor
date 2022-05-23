using System.Collections;
using System.Collections.Generic;
using Aarthificial.Reanimation;
using UnityEngine;

public class SlimeRenderer : MonoBehaviour
{

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
    }
}
