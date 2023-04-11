namespace BehaviorTreeLibrary.Core
{
    public class Selector : Node
    {
        public Selector() { }

        public override void Execute()
        {
            if (CurrentState != State.Success && Index < Noeuds.Count)
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
