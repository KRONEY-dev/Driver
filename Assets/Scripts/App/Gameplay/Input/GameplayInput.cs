using UnityEngine;
using UnityEngine.InputSystem;

namespace Driver.Gameplay.Input
{
    public interface IGameplayInput
    {
        bool FireHeld { get; }

        Vector2 PointerScreenPosition { get; }
    }

    public class GameplayInput : IGameplayInput
    {
        public bool FireHeld
        {
            get
            {
                Pointer pointer = Pointer.current;

                return pointer != null && pointer.press.isPressed;
            }
        }

        public Vector2 PointerScreenPosition
        {
            get
            {
                Pointer pointer = Pointer.current;

                return pointer != null ? pointer.position.ReadValue() : Vector2.zero;
            }
        }
    }
}