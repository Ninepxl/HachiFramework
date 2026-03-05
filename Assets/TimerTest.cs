using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActGame
{
    public class TimerTest : MonoBehaviour
    {

        void Start()
        {
            GameEntry.timerManager.AddTimer(0, 1, () =>
            {
                Debug.Log("触发定时器");
            }, -1);
        }

        void Update()
        {

        }
    }

}