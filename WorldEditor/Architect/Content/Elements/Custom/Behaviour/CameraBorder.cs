using System;
using System.Collections.Generic;
using UnityEngine;

namespace Architect.Content.Elements.Custom.Behaviour;

public class CameraBorder : MonoBehaviour
{
    private static readonly List<CameraBorder> Borders = [];

    internal static void Init()
    {
        On.CameraController.LateUpdate += (orig, self) =>
        {
            orig(self);
            foreach (var bord in Borders)
            {
                switch (bord.type)
                {
                    case 0:
                    {
                        if (self.transform.position.x < bord.transform.position.x) self.transform.SetPositionX(bord.transform.position.x);
                        break;
                    }
                    case 1:
                    {
                        if (self.transform.position.x > bord.transform.position.x) self.transform.SetPositionX(bord.transform.position.x);
                        break;
                    }
                    case 2:
                    {
                        if (self.transform.position.y > bord.transform.position.y) self.transform.SetPositionY(bord.transform.position.y);
                        break;
                    }
                    case 3:
                    {
                        if (self.transform.position.y < bord.transform.position.y) self.transform.SetPositionY(bord.transform.position.y);
                        break;
                    }
                }
            }
        };
    }

    private void OnEnable()
    {
        Borders.Add(this);
    }

    private void OnDisable()
    {
        Borders.Remove(this);
    }

    public int type;
}