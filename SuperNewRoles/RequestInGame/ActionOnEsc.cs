using System;
using UnityEngine;

namespace SuperNewRoles.RequestInGame;

public class ActionOnEsc : MonoBehaviour
{
    private Action onClick;
    public void Init(Action onClick)
    {
        this.onClick = onClick;
    }
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            onClick?.Invoke();
    }
}