using Microsoft.VisualStudio.TestTools.UnitTesting;
using BehaviorTreeLibrary.Core;
using System;

namespace BehaviorTreeTest
{
    [TestClass]
    public class UnitTest1
    {

        private bool motivated;
        private bool haveTime;

        private ActionNode orderFoodAction;
        private ActionNode cookAction;

        private void orderFoodFunction()
        {
            Console.WriteLine("order food");
        }
        private void cookFunction()
        {
            Console.WriteLine("cook food");
        }
        private bool IsgotTime()
        {
            return haveTime;
        }
        private bool IsgotMotivation()
        {
            return motivated;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mot">motivation value</param>
        /// <param name="timed">have time to cook value</param>
        private void SetValues(bool mot, bool timed)
        {
            motivated = mot;
            haveTime = timed;
        }

        private void TestProcess()
        {
            orderFoodAction = new ActionNode(orderFoodFunction);
            cookAction = new ActionNode(cookFunction);
            Selector mainSelector = new Selector();
            Sequence sequence = new Sequence();
            sequence.Add(new ConditionNode(new Condition(IsgotMotivation)));
            sequence.Add(new ConditionNode(new Condition(IsgotTime)));
            sequence.Add(cookAction);
            mainSelector.Add(sequence);
            mainSelector.Add(orderFoodAction);
            mainSelector.Execute();
        }

        [TestMethod]
        public void TestNotMotivated()
        {
            SetValues(false, true);
            TestProcess();
            Assert.IsTrue(State.Success == orderFoodAction.CurrentState);
            Assert.IsTrue(State.NotExecuted == cookAction.CurrentState);
            SetValues(true, false);
            TestProcess();
            Assert.IsTrue(State.Success == orderFoodAction.CurrentState);
            Assert.IsTrue(State.NotExecuted == cookAction.CurrentState);
        }

        [TestMethod]
        public void TestMotivatedAndHaveTime()
        {
            SetValues(true, true);
            TestProcess();
            Assert.IsTrue(State.NotExecuted == orderFoodAction.CurrentState);
            Assert.IsTrue(State.Success == cookAction.CurrentState);
        }

    }
}
