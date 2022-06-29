using System;
using UnityEngine;

namespace Habby.Account.Process
{
    public class ProcessCallEvent<T> : ProcessBase<T> where T : class
    {
        protected override void OnStart()
        {
            try
            {
                code = 0;
            }
            catch (Exception e)
            {
                AccountLog.LogError(e);
            }

            OnEnd();
        }
    }
}