using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HachiFramework
{
    public class InputManager : MonoBehaviour, IGameManager, IUpdate, IFixedUpdate
    {
        private GameInputs m_inputs;
        public Vector2 Movement => m_inputs.GameInput.Movement.ReadValue<Vector2>();
        public Vector2 CameraLook => m_inputs.GameInput.CameraLookAt.ReadValue<Vector2>();
        public bool HasInput => Movement.x != 0 || Movement.y != 0;
        public bool Run => m_inputs.GameInput.Run.triggered;
        public void OnAwake()
        {
            Debug.Log("InputManager Initialize");
            m_inputs = new GameInputs();
        }

        public void OnDispose()
        {
            m_inputs.GameInput.Disable();
        }

        public void OnFixedUpdate()
        {
        }

        public void OnStart()
        {
            m_inputs.GameInput.Enable();
        }

        public void OnUpdate()
        {
        }
    }
}