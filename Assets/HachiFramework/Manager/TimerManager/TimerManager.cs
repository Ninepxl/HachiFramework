using System;
using System.Collections.Generic;
using UnityEngine;

namespace HachiFramework
{
    public class TimerController
    {
        private float leadTime;
        private float interval;
        private float inovkeCount;
        private Action function;
        private float curTime;
        private bool isFinished;
        public bool IsFinished => isFinished;
        public TimerController(float leadTime, float interval, Action function, int count)
        {
            this.leadTime = leadTime;
            this.interval = interval;
            this.function = function;
            inovkeCount = count;
            isFinished = false;
            curTime = 0;
        }
        public void Update()
        {
            if (isFinished) return;
            curTime += Time.unscaledDeltaTime;
            if (leadTime > 0)
            {
                if (curTime >= leadTime)
                {
                    curTime -= leadTime;
                    leadTime = 0;
                }
                return;
            }
            if (curTime >= interval)
            {
                curTime -= interval;
                function();
                inovkeCount--;
                if (inovkeCount > 0)
                {
                    if (inovkeCount == 0)
                    {
                        isFinished = true;
                    }
                }
            }
        }
    }
    public class TimerManager : MonoBehaviour, IGameManager, IUpdate
    {
        private List<int> m_TimerIdList = new List<int>();
        private Dictionary<int, TimerController> m_TimerDic = new Dictionary<int, TimerController>();
        private int m_TimerId = 1;
        #region Public       

        /// 添加计时器，原则上不超过50个
        /// </summary>
        /// <param name="leadTime">前置时间（单位s）</param>
        /// <param name="interval">循环时间（单位s）</param>
        /// <param name="function">回调</param>
        /// <param name="count">触发次数, -1为一直触发</param>
        /// <returns></returns>
        public int AddTimer(float leadTime, float interval, Action function, int count = 1)
        {
            int timerId = m_TimerId;
            m_TimerId++;
            TimerController timer = new(leadTime, interval, function, count);
            m_TimerIdList.Add(timerId);
            m_TimerDic[timerId] = timer;
            return timerId;
        }

        public void KillTimer(int timerId)
        {
            if (m_TimerDic.ContainsKey(timerId))
            {
                m_TimerDic.Remove(timerId);
                m_TimerIdList.Remove(timerId);
            }
        }

        #endregion
        #region 生命周期
        public void OnAwake()
        {
        }

        public void OnDispose()
        {
        }

        public void OnStart()
        {
        }

        public void OnUpdate()
        {
            for (int i = 0; i < m_TimerIdList.Count; i++)
            {
                m_TimerDic[m_TimerIdList[i]].Update();
            }
        }
        #endregion

    }
}