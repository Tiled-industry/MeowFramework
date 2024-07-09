using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Panty
{
    public class DelayTask
    {
        private Action mTask;

        public float DelayTime;
        private float mRemTime;
        public bool Loop;

        private enum E_State : byte { Start, Pause, Finished }
        private E_State mState = E_State.Finished;
        public bool IsEnd() => mState == E_State.Finished;
        public void Skip(float time) => mRemTime -= time;
        public float RemTime() => mRemTime;

        public void AddTask(Action task) => mTask += task;
        public DelayTask SetTask(Action task) { mTask = task; return this; }
        public void Execute() => mTask?.Invoke();
        public void Clear() => mTask = null;
        /// <summary>
        /// ��ʼ������ �ý׶λ��Զ����� Reset
        /// </summary>
        public DelayTask Init(float delayTime, bool isLoop)
        {
            mRemTime = DelayTime = delayTime;
            Loop = isLoop;
            return this;
        }
        public void Reset() => mRemTime = DelayTime;
        public void Start() => mState = E_State.Start;
        public void Pause() => mState = E_State.Pause;
        public void Stop() => mState = E_State.Finished;
        public void Complete() => mRemTime = 0f;

        public void Continue()
        {
            if (mState == E_State.Pause)
            {
                if (Loop) Reset();
                Start();
            }
        }
        public void Update(float delta)
        {
            if (mState == E_State.Start)
            {
                if (mRemTime <= 0f)
                {
                    if (Loop) Reset();
                    else Stop();
                    Execute();
                }
                else Skip(delta);
            }
        }
    }
    public interface ITaskScheduler : IModule
    {
        DelayTask AddDelayTask(float duration, Action onFinished, bool isLoop = false, bool ignoreTimeScale = false);
        DelayTask AddTemporaryTask(float duration, Action onUpdate, bool ignoreTimeScale = false);
        TaskScheduler.Step Sequence(bool ignoreTimeScale = false);
        void StopSequence(TaskScheduler.Step step);
    }
    public class TaskScheduler : AbsModule, ITaskScheduler
    {
        public interface IAction
        {
            bool IsExit();
            void Reset();
            void Update(float delta);
        }
        private class Sequence
        {
            private bool mExit;
            public bool IsExit() => mExit;
            public void Exit() => mExit = true;
            private Queue<IAction> actions = new Queue<IAction>();
            public void Enqueue(IAction e) => actions.Enqueue(e);
            public void Update(float delta)
            {
                var a = actions.Peek();
                if (a.IsExit())
                {
                    actions.Dequeue();
                    if (actions.Count == 0)
                        mExit = true;
                }
                else a.Update(delta);
            }
        }
        // �����ȴ���������
        private class WaitAction : IAction
        {
            private Func<bool> exitCondition;
            public WaitAction(Func<bool> exit)
            {
#if UNITY_EDITOR
                ThrowEx.EmptyCallback(exit);
#endif
                exitCondition = exit;

            }
            public bool IsExit() => exitCondition();
            public void Reset() { }
            public void Update(float delta) { }
        }
        // �����ӳ�N�봥��
        private class DelayAction : IAction
        {
            private float duration, cur;
            public DelayAction(float duration) =>
                this.duration = duration < 0f ? 0f : duration;
            public bool IsExit() => cur >= duration;
            public void Reset() => cur = 0;
            public void Update(float delta) => cur += delta;
        }
        // ָ���������ظ�ִ�е�����
        private class RepeatAction : IAction
        {
            private Action call;
            private byte repeatCount, currentCount;
            public RepeatAction(byte count, Action call)
            {
#if UNITY_EDITOR
                ThrowEx.EmptyCallback(call);
#endif
                this.repeatCount = count == 0 ? (byte)1 : count;
                this.call = call;
            }
            public bool IsExit() => currentCount >= repeatCount;
            public void Reset() => currentCount = 0;
            public void Update(float delta)
            {
                call.Invoke();
                currentCount++;
            }
        }
        // ���������ظ�ִ��
        private class UntilConditionAction : IAction
        {
            private Action call;
            private Func<bool> exit;
            public UntilConditionAction(Func<bool> exit, Action call)
            {
#if UNITY_EDITOR
                ThrowEx.EmptyCallback(call);
                ThrowEx.EmptyCallback(exit);
#endif
                this.exit = exit;
                this.call = call;
            }
            public bool IsExit() => exit();
            public void Reset() { }
            public void Update(float delta) => call.Invoke();
        }
        // �������ظ�ִ������
        private class PeriodicAction : IAction
        {
            private Action call;
            private float duration, cur;
            public PeriodicAction(Action call, float duration)
            {
#if UNITY_EDITOR
                ThrowEx.EmptyCallback(call);
#endif
                this.call = call;
                this.duration = duration < 0f ? 0f : duration;
            }
            public bool IsExit() => cur >= duration;
            public void Reset() => cur = 0;
            public void Update(float delta)
            {
                cur += delta;
                call?.Invoke();
            }
        }
        // ֱ��ִ��һ���߼�
        private class DoAction : IAction
        {
            private Action call;
            public DoAction(Action call)
            {
#if UNITY_EDITOR
                ThrowEx.EmptyCallback(call);
#endif
                this.call = call;
            }
            public bool IsExit()
            {
                call.Invoke();
                return true;
            }
            public void Reset() { }
            public void Update(float delta) { }
        }
        // ���ִ���� ÿ�δ��������һ������ִ��
        private class RandomGroup : IAction
        {
            private readonly PArray<IAction> actions;
            private IAction select;
            public RandomGroup(PArray<IAction> actions)
            {
                this.actions = actions;
                select = actions.RandomGet();
            }
            public bool IsExit() => select.IsExit();
            public void Update(float delta) => select.Update(delta);
            public void Reset()
            {
                select = actions.RandomGet();
                select.Reset();
            }
        }
        // �ظ��� �����ڵ��߼� �ظ�ִ��N��
        private class RepeatGroup : IAction
        {
            private readonly PArray<IAction> actions;
            private byte repeatCount, current;
            private int cur;
            public RepeatGroup(PArray<IAction> actions, byte count)
            {
                this.repeatCount = count == 0 ? (byte)1 : count;
                this.actions = actions;
            }
            public bool IsExit() => current >= repeatCount;
            public void ResetAll() { foreach (var s in actions) s.Reset(); }
            public void Reset()
            {
                cur = 0;
                current = 0;
                ResetAll();
            }
            public void Update(float delta)
            {
                var sq = actions[cur];
                if (sq.IsExit())
                {
                    actions.LoopPosN(ref cur);
                    if (cur == 0)
                    {
                        current++;
                        ResetAll();
                    }
                }
                else sq.Update(delta);
            }
        }
        // ������ �����ڵ��߼�ͬ��ִ�� ֱ�������
        private class ParallelGroup : IAction
        {
            private readonly PArray<IAction> actions = new PArray<IAction>();
            public ParallelGroup(PArray<IAction> actions) => this.actions = actions;
            public bool IsExit() => actions.All(action => action.IsExit());
            public void Reset() { foreach (var s in actions) s.Reset(); }
            public void Update(float delta) { foreach (var s in actions) s.Update(delta); }
        }
        // ѭ���� ������δ�����ʱ�� ˳��ִ�����ڶ���
        private class LoopGroup : IAction
        {
            private int cur = 0;
            private readonly PArray<IAction> actions;
            private Func<bool> exitCondition;
            public LoopGroup(PArray<IAction> actions, Func<bool> exit)
            {
#if UNITY_EDITOR
                ThrowEx.EmptyCallback(exit);
#endif
                this.actions = actions;
                this.exitCondition = exit;
            }
            public bool IsExit() => exitCondition();
            public void Reset() { foreach (var s in actions) s.Reset(); }
            public void Update(float delta)
            {
                var s = actions[cur];
                if (s.IsExit())
                {
                    s.Reset();
                    actions.LoopPosN(ref cur);
                }
                else s.Update(delta);
            }
        }
        private enum E_Type : byte
        {
            Loop, Parallel, Repeat, Random
        }
        private class Group
        {
            private Group father;
            private E_Type type;
            public Group Father => father;
            public bool IsRoot => father == null;
            public Group(E_Type type, Group father)
            {
                this.type = type;
                this.father = father;
            }
            private PArray<IAction> cache = new PArray<IAction>();
            public void Push(IAction action) => cache.Push(action);
            public IAction GetAction() => type switch
            {
                E_Type.Repeat => new RepeatGroup(cache, mCounter),
                E_Type.Loop => new LoopGroup(cache, mOnExit),
                E_Type.Parallel => new ParallelGroup(cache),
                E_Type.Random => new RandomGroup(cache),
                _ => throw new Exception("δ֪����"),
            };
        }
        public class Step
        {
            private static TaskScheduler mScheduler;
            public Action evt;
            public bool ignoreTimeScale;
            public Step(TaskScheduler scheduler, bool ignoreTimeScale)
            {
                mScheduler = scheduler;
                this.ignoreTimeScale = ignoreTimeScale;
            }
            /// <summary>
            /// ����һ������ ����һ�ε���ǰ �ᴩ��ǰ����ʹ��
            /// </summary>
            public Step Cache(Action evt)
            { this.evt = evt; return this; }
            /// <summary>
            /// ����һ���ȴ��¼� �ȴ�����Ҫִ������
            /// </summary>
            public Step Wait(Func<bool> onExit)
            {
                mScheduler.ToSequence(this, new WaitAction(onExit));
                return this;
            }
            /// <summary>
            /// ����һ���ӳ��¼� 
            /// </summary>
            public Step Delay(float duration)
            {
                mScheduler.ToSequence(this, new DelayAction(duration));
                return this;
            }
            /// <summary>
            /// ����һ���¼�
            /// </summary>
            public Step Event(Action call)
            {
                mScheduler.ToSequence(this, new DoAction(call));
                return this;
            }
            /// <summary>
            /// ����һ����ִ���¼� ʹ��ȫ���¼�
            /// </summary>
            public Step Event() => Event(evt);
            /// <summary>
            /// ����һ���ظ������¼�
            /// </summary>
            public Step Repeat(byte repeatCount, Action call)
            {
                mScheduler.ToSequence(this, new RepeatAction(repeatCount, call));
                return this;
            }
            /// <summary>
            /// ����һ���ظ������¼� ʹ��ȫ���¼�
            /// </summary>
            public Step Repeat(byte repeatCount) => Repeat(repeatCount, evt);
            /// <summary>
            /// ����һ��������δ����ʱ�ظ�ִ�е��¼�
            /// </summary>
            public Step Until(Func<bool> exit, Action call)
            {
                mScheduler.ToSequence(this, new UntilConditionAction(exit, call));
                return this;
            }
            /// <summary>
            /// ����һ��������δ����ʱ�ظ�ִ�е��¼� ʹ��ȫ�ֵ��¼�
            /// </summary>
            public Step Until(Func<bool> exit) => Until(exit, evt);
            /// <summary>
            /// ����һ���������ظ�ִ������
            /// </summary>
            public Step Periodic(float duration, Action call)
            {
                mScheduler.ToSequence(this, new PeriodicAction(call, duration));
                return this;
            }
            /// <summary>
            /// ����һ���������ظ�ִ������ ʹ��ȫ���¼�
            /// </summary>
            public Step Periodic(float duration) => Periodic(duration, evt);
            /// <summary>
            /// ��ʼ��������� ��End����ǰ �����Ի���������ÿ�ε�����
            /// </summary>
            public Step RandomGroup()
            {
                mScheduler.NextGroup(E_Type.Random);
                return this;
            }
            /// <summary>
            /// ��������ѭ��ģʽ ��End����ǰ �����Ի���������ÿ�ε�����
            /// ע�� ��onExit����Ϊnull
            /// </summary>
            public Step LoopGroup(Func<bool> onExit)
            {
                mOnExit = onExit;
                mScheduler.NextGroup(E_Type.Loop);
                return this;
            }
            /// <summary>
            /// ���ô���ѭ��ģʽ ��End����ǰ �����Ի���������ÿ�ε�����
            /// </summary>
            /// <param name="repeatCount">ѭ��������������� 0</param>
            public Step RepeatGroup(byte repeatCount)
            {
                mCounter = repeatCount == 0 ? (byte)1 : repeatCount;
                mScheduler.NextGroup(E_Type.Repeat);
                return this;
            }
            /// <summary>
            /// ���ò�����ģʽ ��End����ǰ �����Ի���������ÿ�ε�����
            /// </summary>
            public Step ParallelGroup()
            {
                mScheduler.NextGroup(E_Type.Parallel);
                return this;
            }
            /// <summary>
            /// ���ڷ�ն�����
            /// </summary>
            public Step End()
            {
                mScheduler.EndGroup(this);
                return this;
            }
        }
        private static byte mCounter;
        private static Func<bool> mOnExit;

        private Group stepGroup = null;
        private PArray<Step> mRmvStep;
        private PArray<DelayTask> mAvailable, mDelayTasks, mUnScaledTasks;
        private Dictionary<Step, Sequence> mSequenceGroup;
        private Dictionary<Step, Sequence> mUnscaledSequence;

        protected override void OnInit()
        {
            mSequenceGroup = new Dictionary<Step, Sequence>();
            mUnscaledSequence = new Dictionary<Step, Sequence>();
            mAvailable = new PArray<DelayTask>();
            mDelayTasks = new PArray<DelayTask>();
            mUnScaledTasks = new PArray<DelayTask>();

            mRmvStep = new PArray<Step>();
            MonoKit.OnUpdate += Update;
        }
        void ITaskScheduler.StopSequence(Step step)
        {
            GetSequence(step).Exit();
        }
        public DelayTask AddDelayTask(float duration, Action onFinished, bool isLoop, bool ignoreTimeScale)
        {
            var task = GetTask().Init(duration, isLoop);
            task.SetTask(onFinished).Start();

            if (ignoreTimeScale)
                mUnScaledTasks.Push(task);
            else
                mDelayTasks.Push(task);
            return task;
        }
        DelayTask ITaskScheduler.AddTemporaryTask(float duration, Action onUpdate, bool ignoreTimeScale)
        {
#if DEBUG
            ThrowEx.EmptyCallback(onUpdate);
#endif
            MonoKit.OnUpdate += onUpdate;
            return AddDelayTask(duration, () => MonoKit.OnUpdate -= onUpdate, false, ignoreTimeScale);
        }
        Step ITaskScheduler.Sequence(bool ignoreTimeScale)
        {
#if UNITY_EDITOR
            if (stepGroup != null)
                throw new Exception($"����δ�������{stepGroup}");
#endif
            return new Step(this, ignoreTimeScale);
        }
        private DelayTask GetTask() => mAvailable.IsEmpty ? new DelayTask() : mAvailable.Pop();
        private void NextGroup(E_Type type)
        {
            if (stepGroup == null)
            {
                stepGroup = new Group(type, null);
            }
            else
            {
                var cur = stepGroup;
                stepGroup = new Group(type, cur);
                cur.Push(stepGroup.GetAction());
            }
        }
        private void EndGroup(Step step)
        {
#if UNITY_EDITOR
            if (stepGroup == null)
                throw new Exception($"�����End����");
#endif
            if (stepGroup.IsRoot)
            {
                GetSequence(step).Enqueue(stepGroup.GetAction());
                stepGroup = null;
            }
            else stepGroup = stepGroup.Father;
        }
        private void ToSequence(Step step, IAction action)
        {
            if (stepGroup == null)
                GetSequence(step).Enqueue(action);
            else stepGroup.Push(action);
        }
        private Sequence GetSequence(Step step)
        {
            Sequence q = null;
            if (step.ignoreTimeScale)
            {
                if (!mUnscaledSequence.TryGetValue(step, out q))
                {
                    q = new Sequence();
                    mUnscaledSequence.Add(step, q);
                }
            }
            else if (!mSequenceGroup.TryGetValue(step, out q))
            {
                q = new Sequence();
                mSequenceGroup.Add(step, q);
            }
            return q;
        }
        private void Update()
        {
            float delta = Time.unscaledDeltaTime;
            Update(mUnScaledTasks, delta);
            Update(mUnscaledSequence, delta);
            if (Time.timeScale <= 0f) return;
            Update(mDelayTasks, Time.deltaTime);
            Update(mSequenceGroup, Time.deltaTime);
        }
        private void Update(Dictionary<Step, Sequence> dic, float delta)
        {
            if (dic.Count == 0) return;
            mRmvStep.ToFirst();
            foreach (var pair in dic)
            {
                var q = pair.Value;
                if (q.IsExit())
                    mRmvStep.Push(pair.Key);
                else
                    q.Update(delta);
            }
            while (mRmvStep.Count > 0)
            {
                dic.Remove(mRmvStep.Pop());
            }
        }
        private void Update(PArray<DelayTask> tasks, float delta)
        {
            int i = 0;
            while (i < tasks.Count)
            {
                var task = tasks[i];
                task.Update(delta);
                if (task.IsEnd())
                {
                    mAvailable.Push(task);
                    tasks.RmvAt(i);
                }
                else i++;
            }
        }
    }
}