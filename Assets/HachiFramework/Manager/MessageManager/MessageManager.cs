using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HachiFramework
{
    public class MessageManager : MonoBehaviour, IGameManager
    {
        public void OnAwake()
        {
            Debug.Log("MessageManager Inital");
        }

        public void OnDispose()
        {
        }

        public void OnStart()
        {
        }
    }
}
