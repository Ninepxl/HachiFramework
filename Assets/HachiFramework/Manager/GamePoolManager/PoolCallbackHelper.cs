using System;
using System.Collections.Generic;
using UnityEngine;

namespace HachiFramework
{
    public static class PoolCallbackHelper
    {
        static readonly List<IPoolCallbackReceiver> componentBuffer = new();

        /// <summary>
        /// 调用Root和下面所有子物体的OnRent方法
        /// </summary>
        /// <param name="root">根物体/param>
        public static void InvokeOnRent(GameObject root)
        {
            root.GetComponentsInChildren(componentBuffer);
            foreach (var comp in componentBuffer)
            {
                if (comp is IPoolCallbackReceiver)
                {
                    comp.OnRent();
                }
            }
        }


        /// <summary>
        /// 调用Root和下面所有子物体的OnReturn方法
        /// </summary>
        /// <param name="root">根物体</param>
        public static void InvokeOnReturn(GameObject root)
        {
            root.GetComponentsInChildren(componentBuffer);
            foreach (var comp in componentBuffer)
            {
                if (comp is IPoolCallbackReceiver)
                {
                    comp.OnReturn();
                }
            }
        }
    }
}