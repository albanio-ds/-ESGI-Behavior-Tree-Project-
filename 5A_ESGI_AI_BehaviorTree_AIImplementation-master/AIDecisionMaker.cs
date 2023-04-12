using AI_BehaviorTree_AIGameUtility;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using BehaviorTreeLibrary.Core;

namespace AI_BehaviorTree_AIImplementation
{
    public class AIDecisionMaker
    {

        /// <summary>
        /// Ne pas supprimer des fonctions, ou changer leur signature sinon la DLL ne fonctionnera plus
        /// Vous pouvez unitquement modifier l'intérieur des fonctions si nécessaire (par exemple le nom)
        /// ComputeAIDecision en fait partit
        /// </summary>
        private int AIId = -1;
        public GameWorldUtils AIGameWorldUtils = new GameWorldUtils();

        // Ne pas utiliser cette fonction, elle n'est utile que pour le jeu qui vous Set votre Id, si vous voulez votre Id utilisez AIId
        public void SetAIId(int parAIId) { AIId = parAIId; }

        // Vous pouvez modifier le contenu de cette fonction pour modifier votre nom en jeu
        public string GetName() { return "BehaviorTreeTestv01"; }

        public void SetAIGameWorldUtils(GameWorldUtils parGameWorldUtils) { AIGameWorldUtils = parGameWorldUtils; }

        //Fin du bloc de fonction nécessaire (Attention ComputeAIDecision en fait aussi partit)


        private float BestDistanceToFire = 10.0f;
        private List<AIAction> actionList;
        private PlayerInformations myPlayerInfo;
        private PlayerInformations target;

        private void StopMovingFunction()
        {
            AIActionStopMovement actionStop = new AIActionStopMovement();
            actionList.Add(actionStop);
        }

        private void StartMovingFunction()
        {
            AIActionMoveToDestination actionMove = new AIActionMoveToDestination();
            actionMove.Position = target.Transform.Position;
            actionList.Add(actionMove);
            AIActionDash aIActionDash = new AIActionDash();
            aIActionDash.Direction = actionMove.Position - myPlayerInfo.Transform.Position;
            actionList.Add(aIActionDash);
        }

        private bool IsPlayerEnoughClose()
        {
            return Vector3.Distance(myPlayerInfo.Transform.Position, target.Transform.Position) < BestDistanceToFire;
        }

        private void FocusAndShootFunction()
        {
            AIActionLookAtPosition actionLookAt = new AIActionLookAtPosition();
            actionLookAt.Position = target.Transform.Position;
            actionList.Add(actionLookAt);
            actionList.Add(new AIActionFire());
        }

        public List<AIAction> ComputeAIDecision()
        {
            actionList = new List<AIAction>();

            List<PlayerInformations> playerInfos = AIGameWorldUtils.GetPlayerInfosList();

            target = null;
            foreach (PlayerInformations playerInfo in playerInfos)
            {
                if (!playerInfo.IsActive)
                    continue;

                if (playerInfo.PlayerId == AIId)
                    continue;

                target = playerInfo;
                break;
            }

            if (target == null)
                return actionList;

            myPlayerInfo = GetPlayerInfos(AIId, playerInfos);
            if (myPlayerInfo == null)
                return actionList;

            Sequence mainSequence = new Sequence();
            Selector stopOrStartMoveSelector = new Selector();
            Sequence stopMoveIfCloseSequence = new Sequence();
            ActionNode startmoveAction = new ActionNode(StartMovingFunction);
            ActionNode stopMoveAction = new ActionNode(StopMovingFunction);
            ActionNode shootAction = new ActionNode(FocusAndShootFunction);

            stopMoveIfCloseSequence.Add(new ConditionNode(new Condition(IsPlayerEnoughClose)));
            stopMoveIfCloseSequence.Add(stopMoveAction);
            stopOrStartMoveSelector.Add(stopMoveIfCloseSequence);
            stopOrStartMoveSelector.Add(startmoveAction);
            mainSequence.Add(stopOrStartMoveSelector);
            mainSequence.Add(shootAction);
            mainSequence.Execute();

            return actionList;
        }

        public PlayerInformations GetPlayerInfos(int parPlayerId, List<PlayerInformations> parPlayerInfosList)
        {
            foreach (PlayerInformations playerInfo in parPlayerInfosList)
            {
                if (playerInfo.PlayerId == parPlayerId)
                    return playerInfo;
            }

            Assert.IsTrue(false, "GetPlayerInfos : PlayerId not Found");
            return null;
        }
    }
}
