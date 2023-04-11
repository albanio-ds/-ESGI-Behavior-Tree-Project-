namespace BehaviorTreeLibrary.Core
{
    public class ConditionNode : Node
    {
        public ConditionNode(Condition cond)
        {
            Condition = cond;
        }

        public override void Execute()
        {
            bool result = Condition.Check();
            CurrentState = result ? State.Success : State.Failure;
        }
    }
}
