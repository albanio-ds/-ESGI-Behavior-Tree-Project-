using System;
using System.Collections.Generic;
using System.Text;

namespace Behavior_Tree.Core
{

    public class Selector : Noeud
    {
        private Queue<Selector> ActionToExecute;

        public Selector()
        {
            ActionToExecute = new Queue<Selector>();
            CurrentState = State.NotExecuted;
        }

        public void AddAction(Selector toAdd)
        {
            ActionToExecute.Enqueue(toAdd);
        }

        public void Exectute()
        {
            try
            {

            }
            catch (Exception e)
            {

            }
        }
    }
}
