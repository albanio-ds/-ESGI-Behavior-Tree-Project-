namespace BehaviorTreeLibrary.Core
{
    public class Sequence : Node
    {
        public override void Execute()
        {
            if (CurrentState != State.Failure && Index < Noeuds.Count)
            {
                base.Execute();
                Execute();
            }
            else
            {
                // nothing
            }
        }
    }
}
