using UnityEngine;

namespace Player
{
    public class CarInput
    {
        float forwardInput;
        float reverseInput;
        Vector2 rollInput;
        
        public float DriveInput => forwardInput + reverseInput;
        public float SteerInput => rollInput.x;
        public Vector2 RollInput => rollInput;
        
        public event System.Action<bool, bool, bool> OnJumpEvent;
        
        public void SetForwardInput(float value) => forwardInput = value;
        public void SetReverseInput(float value) => reverseInput = -value;
        public void SetRollInput(Vector2 value) => rollInput = value;

        public void OnJump(bool press, bool hold, bool release)
        {
            OnJumpEvent?.Invoke(press, hold, release);
        }
    }
}