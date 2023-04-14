using AI_BehaviorTree_AIGameUtility;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using BehaviorTreeLibrary.Core;
using Random = UnityEngine.Random;
using Windows.UI.Xaml.Media;

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
        public string GetName() { return "Tim V.II"; }

        public void SetAIGameWorldUtils(GameWorldUtils parGameWorldUtils) { AIGameWorldUtils = parGameWorldUtils; }

        //Fin du bloc de fonction nécessaire (Attention ComputeAIDecision en fait aussi partit)


        private float BestDistanceToFire = 8.5f;
        private List<AIAction> actionList;
        private PlayerInformations myPlayerInfo;
        private PlayerInformations PreviousPosmyP;
        private PlayerInformations target = null;
        private int targetID = -1;
        private BonusInformations Primarybonus;
        private List<PlayerInformations> playerInfos;
        private LayerMask NotWall = (1 << 19);
        private int toMuchTime = 0;


        public float sens = 5f;


        #region DistanceCheck

        private bool TOOFAR()
        {
            return Vector3.Distance(myPlayerInfo.Transform.Position, target.Transform.Position) > BestDistanceToFire;
        }
        private bool OPTIMALDISTANCE()
        {
            return Vector3.Distance(myPlayerInfo.Transform.Position, target.Transform.Position) < BestDistanceToFire;
        }

        private bool TOOCLOSE()
        {
            return Vector3.Distance(myPlayerInfo.Transform.Position, target.Transform.Position) < 4;
        }

        private void IsNearWall()
        {
            int directionray = 3;
            RaycastHit hit;
            if (Physics.Raycast(myPlayerInfo.Transform.Position,(myPlayerInfo.Transform.Rotation * Vector3.right).normalized,out hit, directionray, ~NotWall)){
                sens = -sens;
                directionray = -directionray;
            }
        }

        #endregion

        #region PreservationAction
        private bool LOWHP()
        {
            return (myPlayerInfo.CurrentHealth<myPlayerInfo.MaxHealth*0.3);
        }

        private bool IsHealPack()
        {
            return Primarybonus.Type == EBonusType.Health;
        }

        private bool AnyBonus()
        {
            List<BonusInformations> bonusInfos = AIGameWorldUtils.GetBonusInfosList();
     

            Primarybonus = null;
            foreach (BonusInformations bonusInfo in bonusInfos)
            {
                if (bonusInfo.Type == EBonusType.Health)
                {
                    if (Primarybonus != null)
                    {
                        if (Vector3.Distance(bonusInfo.Position, myPlayerInfo.Transform.Position) < Vector3.Distance(Primarybonus.Position, myPlayerInfo.Transform.Position))
                            Primarybonus = bonusInfo;
                    }
                    else
                    {
                        Primarybonus = bonusInfo;
                    }
                }
            }
            if (Primarybonus == null)
            {
                float ClosestBonus = 1000f;
                foreach (BonusInformations bonusInfo in bonusInfos)
                {
                    if (Vector3.Distance(bonusInfo.Position, myPlayerInfo.Transform.Position) < ClosestBonus)
                    {
                        Primarybonus = bonusInfo;
                        ClosestBonus = Vector3.Distance(bonusInfo.Position, myPlayerInfo.Transform.Position);
                    }
                }
            }
            return bonusInfos.Count>0;
        }

        private bool NearestBonus()
        {
            List<BonusInformations> bonusInfos = AIGameWorldUtils.GetBonusInfosList();

            Primarybonus = null;
            float ClosestBonus = 1000f;

            foreach (BonusInformations bonusInfo in bonusInfos)
            {
                if (Primarybonus == null)
                {
                    Primarybonus = bonusInfo;
                }
                else
                {
                    if (Vector3.Distance(bonusInfo.Position, myPlayerInfo.Transform.Position) < ClosestBonus && (bonusInfo.Type != EBonusType.Health))
                    {
                        Primarybonus = bonusInfo;
                        ClosestBonus = Vector3.Distance(bonusInfo.Position, myPlayerInfo.Transform.Position);
                    }
                }

            }

            if (Vector3.Distance(Primarybonus.Position, myPlayerInfo.Transform.Position) < Vector3.Distance(target.Transform.Position, myPlayerInfo.Transform.Position))
            {
                AIActionMoveToDestination MoveTowardBonus = new AIActionMoveToDestination();
                MoveTowardBonus.Position = Primarybonus.Position;
                actionList.Add(MoveTowardBonus);
                return true;
            }
            return false;
        }


        private bool GetBonus()
        {
            List<BonusInformations> bonusInfos = AIGameWorldUtils.GetBonusInfosList();
            if (bonusInfos.Count <= 0) return false;

            Primarybonus = null;
            float ClosestBonus = 1000f;

            foreach (BonusInformations bonusInfo in bonusInfos)
            {
                if (Primarybonus == null)
                {
                    Primarybonus = bonusInfo;
                }
                else
                {
                    if (Vector3.Distance(bonusInfo.Position, myPlayerInfo.Transform.Position) < ClosestBonus && (bonusInfo.Type != EBonusType.Health))
                    {
                        Primarybonus = bonusInfo;
                        ClosestBonus = Vector3.Distance(bonusInfo.Position, myPlayerInfo.Transform.Position);
                    }
                }
            }

            AIActionMoveToDestination MoveTowardBonus = new AIActionMoveToDestination();
            MoveTowardBonus.Position = Primarybonus.Position;
            actionList.Add(MoveTowardBonus);
            AIActionDash MoveTowardEnnemi = new AIActionDash();
            MoveTowardEnnemi.Direction = Primarybonus.Position - myPlayerInfo.Transform.Position;
            actionList.Add(MoveTowardEnnemi);
            
            return true;
        }
        #endregion

        #region Mouvement
        private void FocusRotate()
        {
            
            AIActionMoveToDestination RotateAround = new AIActionMoveToDestination();

            Vector3 point = target.Transform.Position;
            Vector3 axis = new Vector3(0, -sens, 0);
            float angle = 365 * Time.deltaTime;

            Vector3 vector = myPlayerInfo.Transform.Position;
            Quaternion quaternion = Quaternion.AngleAxis(angle, axis);
            Vector3 vector2 = vector - point;
            vector2 = quaternion * vector2;
            RotateAround.Position = (myPlayerInfo.Transform.Position = point + vector2);

            actionList.Add(RotateAround);
        }

        private void GetInRangeDash()
        {
            //float RandomSens = Random.Range(0, 3);
            //if (RandomSens == 3) sens = -sens;
            Vector3 forward = (target.Transform.Rotation * Vector3.right).normalized * sens + target.Transform.Position;

            
            AIActionMoveToDestination actionMove = new AIActionMoveToDestination();
            actionMove.Position = forward;
            actionList.Add(actionMove);
            AIActionDash MoveTowardEnnemi = new AIActionDash();
            MoveTowardEnnemi.Direction = forward - myPlayerInfo.Transform.Position;
            actionList.Add(MoveTowardEnnemi);
            
        }

        private void RetreatTowardBonus()
        {
            Vector3 forward = (target.Transform.Rotation * Vector3.right).normalized * sens + target.Transform.Position;

            AIActionMoveToDestination MoveTowardBonus = new AIActionMoveToDestination();
            MoveTowardBonus.Position = Primarybonus.Position;
            actionList.Add(MoveTowardBonus);
            if (PreviousPosmyP != null)
            {
            AIActionDash aIActionDash = new AIActionDash();
            aIActionDash.Direction = myPlayerInfo.Transform.Position - PreviousPosmyP.Transform.Position; ;
            actionList.Add(aIActionDash);
            }
        }

        #endregion
        private bool FocusAndShootFunction()
        {
            if(target==null)return false;
            AIActionLookAtPosition actionLookAt = new AIActionLookAtPosition();
            actionLookAt.Position = target.Transform.Position;
            actionList.Add(actionLookAt);
            actionList.Add(new AIActionFire());
            return true;
        }


        private bool Targeting()
        { 
            if(targetID != -1)
            {
                if (!playerInfos[targetID].IsActive || !(playerInfos[targetID].CurrentHealth < playerInfos[targetID].MaxHealth * 0.3))
                {
                    target = null;
                    targetID = -1;
                    sens = -sens;
                }

                else
                {
                    target = playerInfos[targetID];
                }
            }
            else
            {
                float distance = 100000;
                foreach (PlayerInformations playerInfo in playerInfos)
                {
                    if (!playerInfo.IsActive || playerInfo.PlayerId == AIId)
                        continue;


                    if (Vector3.Distance(playerInfo.Transform.Position, myPlayerInfo.Transform.Position) < distance){
                        targetID = playerInfo.PlayerId;
                        target = playerInfo;
                        distance = Vector3.Distance(playerInfo.Transform.Position, myPlayerInfo.Transform.Position);
                    }

                }
            }
    

            return (target != null);

        }

        public List<AIAction> ComputeAIDecision()
        {
            actionList = new List<AIAction>();
            playerInfos = AIGameWorldUtils.GetPlayerInfosList();
            myPlayerInfo = GetPlayerInfos(AIId, playerInfos);


            //================================================================

            Sequence mainSequence = new Sequence();
            Selector WHERETOGO = new Selector();
            Selector AnyoneAlive = new Selector();

            //================================================================|HUNT|
            #region Hunt
            ActionNode GetInRangeFast = new ActionNode(GetInRangeDash);
            ActionNode FocusRotation = new ActionNode(FocusRotate);

            Selector HUNT = new Selector();
            Sequence HUNT_F = new Sequence();
            Sequence HUNT_O = new Sequence();
            Sequence HUNT_C = new Sequence();


            HUNT_O.Add(new ConditionNode(new Condition(OPTIMALDISTANCE)));     //Condition
            HUNT_O.Add(FocusRotation);

            HUNT_F.Add(new ConditionNode(new Condition(TOOFAR)));     //Condition
            HUNT_F.Add(GetInRangeFast);

            HUNT_C.Add(new ConditionNode(new Condition(TOOCLOSE)));     //Condition
            HUNT_C.Add(GetInRangeFast);

            HUNT.Add(HUNT_F);
            HUNT.Add(HUNT_C);
            HUNT.Add(HUNT_O);
            #endregion
            
            //================================================================|PRESERVATION|
            #region PRESERVATION
            Sequence PRESERVATION = new Sequence();
            //ActionNode shootAction = new ActionNode(FocusAndShootFunction);
            ActionNode Retreat = new ActionNode(RetreatTowardBonus);

            PRESERVATION.Add(new ConditionNode(new Condition(LOWHP)));     //Condition
            PRESERVATION.Add(new ConditionNode(new Condition(AnyBonus)));  //Condition
            PRESERVATION.Add(new ConditionNode(new Condition(IsHealPack)));//Condition
            PRESERVATION.Add(Retreat);                                      // SI ALL YE
            #endregion

            //================================================================|TARGETING|
            ActionNode WallCheck = new ActionNode(IsNearWall);

            Sequence HuntWho = new Sequence();
            Sequence Hunting = new Sequence();

            Hunting.Add(WallCheck);
            Hunting.Add(HUNT);
            
            Sequence NearestBonusAvailable = new Sequence();
            NearestBonusAvailable.Add(new ConditionNode(new Condition(AnyBonus)));  //Condition
            NearestBonusAvailable.Add(new ConditionNode(new Condition(NearestBonus)));


            Selector WHATCLOSER = new Selector();
            WHATCLOSER.Add(NearestBonusAvailable);
            WHATCLOSER.Add(Hunting);





            HuntWho.Add(new ConditionNode(new Condition(Targeting)));//Condition
            HuntWho.Add(WHATCLOSER);

            WHERETOGO.Add(PRESERVATION);
            WHERETOGO.Add(HuntWho);


            AnyoneAlive.Add(WHERETOGO);
            AnyoneAlive.Add(new ConditionNode(new Condition(GetBonus)));//Condition


            mainSequence.Add(AnyoneAlive);
            mainSequence.Add(new ConditionNode(new Condition(FocusAndShootFunction)));
            mainSequence.Execute();

            PreviousPosmyP = GetPlayerInfos(AIId, playerInfos);
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
