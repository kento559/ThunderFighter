using UnityEngine;

namespace ThunderFighter.InputSystem
{
    public interface IInputProvider
    {
        Vector2 GetMoveVector();
        bool IsFirePressed();
        bool IsDashPressed();
        bool IsSkillOnePressed();
        bool IsSkillTwoPressed();
        bool IsPausePressed();
    }
}
