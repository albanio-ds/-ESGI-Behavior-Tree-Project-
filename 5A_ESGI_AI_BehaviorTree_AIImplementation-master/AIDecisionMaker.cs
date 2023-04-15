using AI_BehaviorTree_AIGameUtility;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using BehaviorTreeLibrary.Core;
using Random = UnityEngine.Random;

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
        public string GetName() { return "Jadrix"; }

        public void SetAIGameWorldUtils(GameWorldUtils parGameWorldUtils) { AIGameWorldUtils = parGameWorldUtils; }

        //Fin du bloc de fonction nécessaire (Attention ComputeAIDecision en fait aussi partit)


        private float BestDistanceToFire = 5.0f;
        private List<AIAction> actionList;
        private PlayerInformations myPlayerInfo;
        private PlayerInformations target;
        private BonusInformations bonus;
        List<BonusInformations> bonusInfos;

        private void StopMovingFunction()
        {
            AIActionStopMovement actionStop = new AIActionStopMovement();
            actionList.Add(actionStop);
        }

        private void StartMovingFunction()
        {
            AIActionMoveToDestination actionMove = new AIActionMoveToDestination();

            Vector3 directionTotarget = (target.Transform.Position - myPlayerInfo.Transform.Position).normalized;
            //float distanceToTarget = Vector3.Distance(target.Transform.Position, myPlayerInfo.Transform.Position);

            Vector3 forward = target.Transform.Position - myPlayerInfo.Transform.Position;
            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
            Vector3 back = target.Transform.Position + myPlayerInfo.Transform.Position;
            Vector3 left = Vector3.Cross(Vector3.down, back).normalized;
            Vector3 up = Vector3.Cross(forward, right).normalized;
            Vector3 down = Vector3.Cross(back, left).normalized;


            Quaternion rotation = Quaternion.LookRotation(forward, up);



            Vector3 rotatedDirection = rotation * directionTotarget;


            // Distance minimale avant que l'IA commence à reculer
            float minDistance = 5f;

            // Distance actuelle entre l'IA et l'ennemi
            float distanceToTarget = Vector3.Distance(myPlayerInfo.Transform.Position, target.Transform.Position);

            if (distanceToTarget < minDistance)
            {
                // Reculez de la même distance que la distance minimale, mais en direction opposée
                actionMove.Position = myPlayerInfo.Transform.Position - rotatedDirection * minDistance;
            }
            else
            {
                // Avancez vers l'ennemi
                actionMove.Position = myPlayerInfo.Transform.Position + rotatedDirection * 5f;
            }
            
            
            actionList.Add(actionMove);
            AIActionDash aIActionDash = new AIActionDash();
            int random;
            random=Random.Range(0, 2);
            if(random==0)
            {
                aIActionDash.Direction = Quaternion.AngleAxis(45, up) * rotatedDirection * -1f;
                actionList.Add(aIActionDash);
            } 
            else
            {
                aIActionDash.Direction = Quaternion.AngleAxis(45, down) * rotatedDirection * 1f;
                actionList.Add(aIActionDash);
            }
            

        }
        private void DefendToEnemy()
        {
                AIActionMoveToDestination actionMove = new AIActionMoveToDestination();

                Vector3 directionTotarget = (target.Transform.Position - myPlayerInfo.Transform.Position).normalized;
                //float distanceToTarget = Vector3.Distance(target.Transform.Position, myPlayerInfo.Transform.Position);

                Vector3 forward = target.Transform.Position - myPlayerInfo.Transform.Position;
                Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
                Vector3 back = target.Transform.Position + myPlayerInfo.Transform.Position;
                Vector3 left = Vector3.Cross(Vector3.down, back).normalized;
                Vector3 up = Vector3.Cross(forward, right).normalized;
                Vector3 down = Vector3.Cross(back, left).normalized;


                Quaternion rotation = Quaternion.LookRotation(forward, up);



                Vector3 rotatedDirection = rotation * directionTotarget;


                // Distance minimale avant que l'IA commence à reculer
                float minDistance = 5f;

                // Distance actuelle entre l'IA et l'ennemi
                float distanceToTarget = Vector3.Distance(myPlayerInfo.Transform.Position, target.Transform.Position);

                if (distanceToTarget < minDistance)
                {
                    // Reculez de la même distance que la distance minimale, mais en direction opposée
                    actionMove.Position = myPlayerInfo.Transform.Position - rotatedDirection * minDistance;
                }
                else
                {
                    // Avancez vers l'ennemi
                    actionMove.Position = myPlayerInfo.Transform.Position + rotatedDirection * 5f;
                }


                actionList.Add(actionMove);
                AIActionDash aIActionDash = new AIActionDash();
                int random;
                random = Random.Range(0, 2);
                if (random == 0)
                {
                    aIActionDash.Direction = Quaternion.AngleAxis(45, up) * rotatedDirection * -1f;
                    actionList.Add(aIActionDash);
                }
                else
                {
                    aIActionDash.Direction = Quaternion.AngleAxis(45, down) * rotatedDirection * 1f;
                    actionList.Add(aIActionDash);
                }


            
        }

        private void StartMovingToBonusFunction()
        {
            if (bonusInfos == null || bonusInfos.Count == 0) // check if there are no more bonuses
            {
                return; // exit function
            }

            // get the closest bonus to the AI
            //bonus = bonusInfos.OrderBy(b => Vector3.Distance(b.Position, myPlayerInfo.Transform.Position)).FirstOrDefault();
            if (bonus.Type == EBonusType.Health && myPlayerInfo.CurrentHealth<=50)
            {
                AIActionMoveToDestination actionMove = new AIActionMoveToDestination();
                actionMove.Position = bonus.Position;
                actionList.Add(actionMove);
                AIActionDash aIActionDash = new AIActionDash();
                aIActionDash.Direction = actionMove.Position - myPlayerInfo.Transform.Position;
                actionList.Add(aIActionDash);

            }
            else if (bonus.Type==EBonusType.Speed )
            {
                AIActionMoveToDestination actionMove = new AIActionMoveToDestination();
                actionMove.Position = bonus.Position;
                actionList.Add(actionMove);
                AIActionDash aIActionDash = new AIActionDash();
                aIActionDash.Direction = actionMove.Position - myPlayerInfo.Transform.Position;
                actionList.Add(aIActionDash);

            }
            else if (bonus.Type == EBonusType.CooldownReduction)
            {
                AIActionMoveToDestination actionMove = new AIActionMoveToDestination();
                actionMove.Position = bonus.Position;
                actionList.Add(actionMove);
                AIActionDash aIActionDash = new AIActionDash();
                aIActionDash.Direction = actionMove.Position - myPlayerInfo.Transform.Position;
                actionList.Add(aIActionDash);
            }
            else if(bonus.Type == EBonusType.Damage)
            {
                AIActionMoveToDestination actionMove = new AIActionMoveToDestination();
                actionMove.Position = bonus.Position;
                actionList.Add(actionMove);
                AIActionDash aIActionDash = new AIActionDash();
                aIActionDash.Direction = actionMove.Position - myPlayerInfo.Transform.Position;
                actionList.Add(aIActionDash);
            }

          

        }
        private bool IsPlayerEnoughClose()
        {
            return Vector3.Distance(myPlayerInfo.Transform.Position, target.Transform.Position) >10;
        }
        private bool IsThereBonus()
        {
            if(bonusInfos!=null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        private bool NoMoreEnemies(List<PlayerInformations> players)
        {
            foreach (PlayerInformations player in players)
            {
                if (player.IsActive && player.PlayerId != myPlayerInfo.PlayerId)
                {
                    // Un ennemi est encore actif
                    return false;
                }
            }

            // Aucun ennemi trouvé
            return true;
        }

        private void FocusAndShootFunction()
        {  

            // Récupérer la dernière position connue de l'ennemi
            Vector3 enemyLastPosition = target.Transform.Position;

            // Calculer la distance entre les deux joueurs
            float distanceToEnemy = Vector3.Distance(myPlayerInfo.Transform.Position, enemyLastPosition);

            // Estimer le temps de déplacement nécessaire pour atteindre l'ennemi
            float timeToEnemy = distanceToEnemy / 5f;

            Vector3 previousPosition = enemyLastPosition;
            Vector3 currentPosition = target.Transform.Position;
            Vector3 currentDirection = (currentPosition - previousPosition).normalized;
            // Calculer la position future de l'ennemi
            Vector3 enemyFuturePosition = enemyLastPosition + currentDirection * timeToEnemy;

            // Tourner la caméra vers la position future de l'ennemi
            AIActionLookAtPosition actionLookAt = new AIActionLookAtPosition();
            actionLookAt.Position = enemyFuturePosition;
            actionList.Add(actionLookAt);

            // Tirer sur la position future de l'ennemi
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

            
            bonusInfos=new List<BonusInformations>();
            bonusInfos = AIGameWorldUtils.GetBonusInfosList();
            bonus = null;
            foreach (BonusInformations bonusInfo in bonusInfos)
            {
                bonus = bonusInfo;
                break;
            }

            if (bonus == null)
                return actionList;


            myPlayerInfo = GetPlayerInfos(AIId, playerInfos);
            if (myPlayerInfo == null)
                return actionList;
            /*
            Selector mainSelector = new Selector();
            Sequence checkDistanceBonusSequence = new Sequence();
            Selector  checkBonusIDSelector= new Selector();
            ActionNode LookBonusAction = new ActionNode(LookBonusFunction);
            ActionNode  goToBonusAction = new ActionNode(StartMovingToBonusFunction);
            Selector attackOrMoveSelector = new Selector();
            Sequence attackTargetSequence = new Sequence();
            ActionNode shootAction = new ActionNode(FocusAndShootFunction);
            Selector moveToTargetOrIdleSelector = new Selector();
            Sequence moveToTargetSequence = new Sequence();
            ActionNode startmoveAction = new ActionNode(StartMovingFunction);
            Sequence stopMovingSequence = new Sequence();
            ActionNode stopMoveAction = new ActionNode(StopMovingFunction);
            */

            /*
            mainSelector.Add(checkDistanceBonusSequence);
            checkDistanceBonusSequence.Add(new ConditionNode(new Condition(IsPlayerCloseToBonus)));
            checkDistanceBonusSequence.Add(goToBonusAction);
            goToBonusAction.Add(checkBonusIDSelector);
            checkBonusIDSelector.Add(goToBonusAction);
            */

            /*
            mainSelector.Add(shootAction);
            mainSelector.Add(moveToTargetOrIdleSelector);
            
            moveToTargetOrIdleSelector.Add(new ConditionNode(new Condition(IsPlayerEnoughClose)));
            moveToTargetOrIdleSelector.Add(moveToTargetSequence);
            moveToTargetOrIdleSelector.Add(startmoveAction);
           
            mainSelector.Execute();
            */


           

            //move shoot
            Sequence mainSequence = new Sequence();
            Selector StartMoveSelector = new Selector();
            Sequence stopMoveIfCloseSequence = new Sequence();
            Sequence MoveCloseSequence = new Sequence();
            Sequence attackEnemySequence = new Sequence();
            Sequence defendObjectiveSequence = new Sequence();

            ActionNode startmoveAction = new ActionNode(StartMovingFunction);
            ActionNode stopMoveAction = new ActionNode(StopMovingFunction);
            ActionNode shootAction = new ActionNode(FocusAndShootFunction);
            ActionNode Defend = new ActionNode(DefendToEnemy);

            //bonus
            Selector StartMoveToBonusSelector = new Selector();
            Sequence checkDistanceBonusSequence = new Sequence();
            Sequence checkBonusIDSelector = new Sequence();
            Sequence noMoreBonusSequence = new Sequence();

            ActionNode goToBonusAction = new ActionNode(StartMovingToBonusFunction);

           
            
            mainSequence.Add(shootAction);
            mainSequence.Add(StartMoveToBonusSelector);
            mainSequence.Add(StartMoveSelector);
            
            StartMoveSelector.Add(MoveCloseSequence);
            MoveCloseSequence.Add(startmoveAction);


            StartMoveToBonusSelector.Add(new ConditionNode(new Condition(IsThereBonus)));
            StartMoveToBonusSelector.Add(noMoreBonusSequence);
            noMoreBonusSequence.Add(goToBonusAction);
            

            //defendObjectiveSequence.Add(new ConditionNode(new Condition(IsPlayerEnoughClose)));
            //defendObjectiveSequence.Add(Defend);

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
