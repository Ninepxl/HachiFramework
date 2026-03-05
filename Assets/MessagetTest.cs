using UnityEngine;
using UnityEngine.InputSystem;
using HachiFramework;
using ActGame;

public class MessagetTest : MonoBehaviour
{
    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            MessageBroker<ChangeVelocityEvent>.Default.Publish(new ChangeVelocityEvent
            {
                Velocity = 10f
            });
        }
    }
}
