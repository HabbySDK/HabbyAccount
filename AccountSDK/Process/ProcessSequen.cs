using System;
using System.Collections.Generic;
using UnityEngine;

namespace Habby.Account.Process
{
    public class ProcessSequen : ProcessBase<object>
    {
        public int Index { get; private set; }
        private List<IProcess> processList = new List<IProcess>(5);

        public T Add<T>(T pObj) where T : IProcess
        {
            if(this.Equals(pObj)) return pObj;
            pObj.onComplete += OnOneComplete;
            processList.Add(pObj);

            return pObj;
        }
        
        override protected void OnStart()
        {
            if (processList.Count == 0)
            {
                code = 0;
                OnEnd();
                return;
            }
                
            Index = -1;
            var tprocess = GoNextProcess();
            
            if (tprocess == null)
            {
                code = 0;
                OnEnd();
            }
        }


        IProcess GoNextProcess()
        {
            Index++;
            if (Index >= processList.Count) return null;

            for (int i = Index,max = processList.Count; i < max; i++)
            {
                var item = processList[Index];
                if (!item.Bypass)
                {
                    item.Start();
                    return item;
                }
            }
            
            return null;
        }

        void OnOneComplete(IProcess sender)
        {
            try
            {
                if (sender.code != 0)
                {
                    code = sender.code;
                    errorMsg = sender.errorMsg;
                    OnEnd();
                }
                else
                {
                    var tprocess = GoNextProcess();
                    if (tprocess == null)
                    {
                        code = 0;
                        OnEnd();
                    }
                }

            }
            catch (Exception e)
            {
                AccountLog.LogError(e);
            }

        }
        
    }
}