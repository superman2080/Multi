using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMath : MonoBehaviour
{
    public static float GetAngle(Vector2 origin, Vector2 moveTo)
    {
        Vector2 dir = (origin - moveTo).normalized;
        return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
    }

}
