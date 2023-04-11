using System;
using System.Collections.Generic;

namespace BehaviorTreeLibrary.Core
{
    public class Node
    {
        public State CurrentState = State.NotExecuted;
        protected List<Node> Noeuds = new List<Node>();
        public Condition Condition = new ConditionAlwaysTrue();
        public Action Action = null;

        protected int Index = 0;

        public void Add(Node toAdd)
        {
            Noeuds.Add(toAdd);
        }

        public virtual void Execute()
        {
            CurrentState = State.Running;
            if (Noeuds.Count > 0 && Index < Noeuds.Count)
            {
                Noeuds[Index].Execute();
                CurrentState = Noeuds[Index].CurrentState;
                Index++;
            }
            else
            {
                throw new NotSupportedException();
                /*
                var res = Condition.Check();
                if (res)
                {
                    Action?.Invoke();
                }
                CurrentState = res ? State.Success : State.Failure;
                 */
            }
        }
    }
}
