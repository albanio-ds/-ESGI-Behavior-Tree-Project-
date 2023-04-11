using System;

namespace BehaviorTreeLibrary.Core
{
    public class Condition
    {
        public Func<bool> ToCheck = null;

        public Condition(Func<bool> toCheck)
        {
            ToCheck = toCheck;
        }

        public virtual bool Check()
        {
            return ToCheck.Invoke();
        }
    }
}
