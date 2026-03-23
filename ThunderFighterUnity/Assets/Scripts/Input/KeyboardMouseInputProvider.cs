using UnityEngine;

namespace ThunderFighter.InputSystem
{
    public class KeyboardMouseInputProvider : MonoBehaviour, IInputProvider
    {
        [SerializeField] private KeyCode fireKey = KeyCode.J;
        [SerializeField] private KeyCode dashKey = KeyCode.Space;
        [SerializeField] private KeyCode skillOneKey = KeyCode.K;
        [SerializeField] private KeyCode skillTwoKey = KeyCode.L;
        [SerializeField] private KeyCode pauseKey = KeyCode.Escape;

        public Vector2 GetMoveVector()
        {
            float x = Input.GetAxisRaw("Horizontal");
            float y = Input.GetAxisRaw("Vertical");
            return new Vector2(x, y).normalized;
        }

        public bool IsFirePressed()
        {
            return Input.GetKey(fireKey) || Input.GetMouseButton(0);
        }

        public bool IsDashPressed()
        {
            return Input.GetKeyDown(dashKey);
        }

        public bool IsSkillOnePressed()
        {
            return Input.GetKeyDown(skillOneKey);
        }

        public bool IsSkillTwoPressed()
        {
            return Input.GetKeyDown(skillTwoKey);
        }

        public bool IsPausePressed()
        {
            return Input.GetKeyDown(pauseKey);
        }
    }
}
