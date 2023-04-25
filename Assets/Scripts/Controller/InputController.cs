using System;
using UnityEngine;

internal class Repeater
{
    private const float THRESHOLD = 0.5f;
    private const float RATE = 0.25f;

    private float _next;
    private bool _hold;
    private string _axis;

    public Repeater(string axisName)
    {
        _axis = axisName;
    }

    public int Update()
    {
        int retValue = 0;
        int value = Mathf.RoundToInt(Input.GetAxisRaw(_axis));

        if (value != 0)
        {
            if (Time.time > _next)
            {
                retValue = value;
                _next = Time.time + (_hold ? RATE : THRESHOLD);
                _hold = true;
            }
        }
        else
        {
            _hold = false;
            _next = 0;
        }

        return retValue;
    }
}

public class InputController : MonoBehaviour
{
    public static event EventHandler<InfoEventArgs<Point>> moveEvent;
    public static event EventHandler<InfoEventArgs<int>> fireEvent;

    private Repeater _hor = new("Horizontal");
    private Repeater _ver = new("Vertical");

    private string[] _buttons = new string[] { "Fire1", "Fire2", "Fire3" };

    public void Update()
    {
        int x = _hor.Update();
        int y = _ver.Update();

        if (x != 0 || y != 0)
            moveEvent?.Invoke(this, new InfoEventArgs<Point>(new Point(x, y)));

        for (int i = 0; i < 3; i++)
        {
            if (Input.GetButtonUp(_buttons[i]))
                fireEvent?.Invoke(this, new InfoEventArgs<int>(i));
        }
    }
}

