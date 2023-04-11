namespace BehaviorTreeLibrary.Core
{
    public class ConditionAlwaysTrue : Condition
    {
        public ConditionAlwaysTrue() : base(null){}

        public override bool Check()
        {
            return true;
        }
    }
}
