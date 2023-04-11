using System;

namespace BehaviorTreeLibrary.Core
{
    public class ActionNode : Node
    {
        public ActionNode(Action action)
        {
            Action = action;
        }
        public override void Execute()
        {
            Action.Invoke();
            CurrentState = State.Success;
        }
    }
}
