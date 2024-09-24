using UnityEngine;

namespace VEOController
{
    public class InputsHandler : MonoBehaviour
    {

        private Controller player;
        private PlayerInputs inputs;

        void Awake()
        {
            player = GetComponent<Controller>();
            inputs = player.inputs;

            SetInputs();
        }

        void SetInputs()
        {
            inputs.AddAction("Jump", player.JumpPressed, player.JumpReleased);
            inputs.AddAction("Dash", player.DashPressed, null);
            inputs.AddAction("Attack", player.AttackPressed, null);
            inputs.AddAction("HeavyAttack", player.HeavyAttackPressed, player.HeavyAttackReleased);
        }
    }

}
