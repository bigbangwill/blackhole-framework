using UnityEngine;

namespace Blackhole.GlobalDialogue.Contract
{
    public interface IPositionConvertor
    {
        Vector2 WorldPositionToUI(Vector2 worldPosition);
        Vector2 UIToWorld(Vector2 uiPosition);
    }
}