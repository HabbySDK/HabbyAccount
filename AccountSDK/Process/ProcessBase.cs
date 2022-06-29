using System;
using UnityEngine;

namespace Habby.Account.Process
{
    public interface IProcess
    {
        ProcessState state { get; }
        void Start();
        void Start(Action callBack);
        void SetBypass(bool pBypass);
        event System.Action<IProcess> onComplete;
        event System.Action onStart;

        bool Bypass { get; }
        int code { get; }
        string errorMsg { get; }
        object data { get; }
    }

    public enum ProcessState
    {
        none = 0,
        start,
        end,
    }
    public abstract class ProcessBase<T> : IProcess where T : class
    {
        public event Action<IProcess> onComplete;
        public event Action onStart;

        public bool Bypass { get; protected set; }
        public int code { get; protected set; }
        public string errorMsg { get; protected set; }
        public object data { get; protected set; }

        public ProcessState state { get; protected set; }
        
        
        public event HabbyAccountEvent<T> OnDoneEvent;
        event Action onProcessEndEvent;
        public void Start(Action callBack)
        {
            onProcessEndEvent += callBack;
            Start();
        }

        public void Start()
        {
            if(state == ProcessState.start) return;

            AccountLog.Log($"Process {this.GetType().Name} Start.");
            state = ProcessState.start;
            onStart?.Invoke();
            Reset();

            OnStart();
        }
        virtual protected void OnStart()
        {
            
        }

        void IProcess.SetBypass(bool pBypass)
        {
            if (state == ProcessState.start)
            {
                AccountLog.Log("SetBypass cannot be set after startup");
                return;
            }
            Bypass = pBypass;
        }

        virtual protected void Reset()
        {
            code = -1;
            errorMsg = null;
            data = null;
        }

        private void OnDone()
        {
            try
            {
                OnDoneEvent?.Invoke(code, data as T, errorMsg);
            }
            catch (Exception e)
            {
                AccountLog.LogError(e);
            }
            
        }

        private void OnProcessEnd()
        {
            try
            {
                var tendevt = onProcessEndEvent;
                onProcessEndEvent = null;
                tendevt?.Invoke();
            }
            catch (Exception e)
            {
                AccountLog.LogError(e);
            }
        }
        
        virtual protected void OnEnd()
        {
            AccountLog.LogFormat("Process {0} end. code={1},errormsg={2}", this.GetType().Name, code, errorMsg);

            state = ProcessState.end;

            OnDone();
            OnProcessEnd();
            
            try
            {
                onComplete?.Invoke(this);
            }
            catch (Exception e)
            {
                AccountLog.LogError(e);
            }
            
        }
    }
}