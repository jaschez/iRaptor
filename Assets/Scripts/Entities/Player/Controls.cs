using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Mantenimiento y actualización de controle para el juego, módulo de la parte del control del juego
public class Controls
{
    static private Axis joyAxis;
    static private KeyCode move;
    static private KeyCode attack;
    static private KeyCode interact;
    static private KeyCode dash;
    static private KeyCode gadget;

    static private bool trigger = false;
    static private bool triggerUp = true;

    static private bool enabled = true;

    //Actualiza o inicializa los controles para el juego
    public static void SetupControls(Axis axis, KeyCode mov, KeyCode att, KeyCode das, KeyCode inter, KeyCode gadg)
    {
        joyAxis = axis;
        attack = att;
        move = mov;
        dash = das;
        interact = inter;
        gadget = gadg;
    }

    //TODO: Actualiza los controles desde un archivo
    public static void ReadControlsFromFile()
    {

    }

    public static void AllowControls(bool enable)
    {
        enabled = enable;
    }

    public static bool IsAllowed()
    {
        return enabled;
    }

    //Funciones de deteccion de teclas y ejes de joystick
    public static Axis GetJoystickAxis()
    {
        return joyAxis;
    }

    //Detección para tecla ATTACK
    public static bool GetAttackKey()
    {
        return (Input.GetKey(attack) || Input.GetButton("Fire")) && enabled;
    }

    public static bool GetAttackKeyUp()
    {
        return Input.GetKeyUp(attack) && enabled;
    }

    public static bool GetAttackKeyDown()
    {
        return (Input.GetKeyDown(attack) || Input.GetButtonDown("Fire")) && enabled;
    }

    //Detección para tecla INTERACT
    public static bool GetInteractKey()
    {
        return Input.GetKey(interact) && enabled;
    }

    public static bool GetInteractKeyUp()
    {
        return Input.GetKeyUp(interact) && enabled;
    }

    public static bool GetInteractKeyDown()
    {
        return Input.GetKeyDown(interact) && enabled;
    }

    //Detección para tecla MOVE
    public static bool GetMoveKey()
    {
        return (Input.GetKey(move) || Input.GetAxisRaw("Boost") != 0 || GetJoystick2X() != 0 || GetJoystick2Y() != 0) && enabled;
    }

    public static bool GetMoveKeyUp()
    {
        if (enabled)
        {
            if ((Input.GetAxis("Boost") != 0) || GetJoystick2X() == 0 && GetJoystick2Y() == 0 && !triggerUp)
            {
                triggerUp = true;
                return true;
            }
            else if (Input.GetAxis("Boost") != 0 || GetJoystick2X() != 0 || GetJoystick2Y() != 0)
            {
                triggerUp = false;
            }
        }

        return false;
    }

    public static bool GetMoveKeyDown()
    {
        if (enabled) {
            if ((Input.GetAxis("Boost") != 0 || GetJoystick2X() != 0 || GetJoystick2Y() != 0) && !trigger)
            {
                trigger = true;
                return true;
            }
            else if (Input.GetAxis("Boost") == 0 && GetJoystick2X() == 0 && GetJoystick2Y() == 0)
            {
                trigger = false;
            }

            return Input.GetKeyDown(move);
        }
        else
        {
            return false;
        }
    }

    //Detección para tecla DASH
    public static bool GetDashKey()
    {
        return (Input.GetKey(dash) || Input.GetButton("Dash")) && enabled;
    }

    public static bool GetDashKeyUp()
    {
        return Input.GetKeyUp(dash) && enabled;
    }

    public static bool GetDashKeyDown()
    {
        return (Input.GetKeyDown(dash) || Input.GetButtonDown("Dash")) && enabled;
    }

    //Detección para tecla GADGET
    public static bool GetGadgetKeyDown()
    {
        return (Input.GetKeyDown(gadget) || Input.GetButtonDown("Gadget")) && enabled;
    }

    //Detección para ejes de joystick
    public static int GetJoystick1X()
    {
        int value = 0;

        if (enabled) {
            if (Input.GetKey(joyAxis.left) || Input.GetAxis("Horizontal1") <= -.1f)
            {
                value += 1;
            }

            if (Input.GetKey(joyAxis.right) || Input.GetAxis("Horizontal1") >= .1f)
            {
                value -= 1;
            }
        }

        return value;
    }

    public static int GetJoystick1Y()
    {
        int value = 0;

        if (enabled)
        {
            if (Input.GetKey(joyAxis.up) || Input.GetAxisRaw("Vertical1") >= .1f)
            {
                value += 1;
            }

            if (Input.GetKey(joyAxis.down) || Input.GetAxisRaw("Vertical1") <= -.1f)
            {
                value -= 1;
            }
        }

        return value;
    }

    public static int GetJoystick2X()
    {
        int value = 0;

        if (Input.GetKey(joyAxis.left))
        {
            value += 1;
        }

        if (Input.GetKey(joyAxis.right))
        {
            value -= 1;
        }

        return value;
    }

    public static int GetJoystick2Y()
    {
        int value = 0;

        if (Input.GetKey(joyAxis.up))
        {
            value += 1;
        }

        if (Input.GetKey(joyAxis.down))
        {
            value -= 1;
        }

        return value;
    }

}

//Struct que contiene las teclas asociadas a cada direccion de la palanca
public struct Axis
{
    public KeyCode up;
    public KeyCode down;
    public KeyCode left;
    public KeyCode right;

    public Axis(KeyCode u, KeyCode d, KeyCode l, KeyCode r)
    {
        up = u;
        down = d;
        left = l;
        right = r;
    }
}
