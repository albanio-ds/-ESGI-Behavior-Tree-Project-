using AI_BehaviorTree_AIGameUtility;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using AI_BehaviorTree_AIImplementation_3.Data;
using BehaviorTreeLibrary.Core;
using System.Linq;

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
        public string GetName() { return "Soulkiller-v1.7"; }

        public void SetAIGameWorldUtils(GameWorldUtils parGameWorldUtils) { AIGameWorldUtils = parGameWorldUtils; }

        //Fin du bloc de fonction nécessaire (Attention ComputeAIDecision en fait aussi partit)


        private float BestDistanceToFire = 10.0f;

        private bool IsTargetClose()
        {
            return Vector3.Distance(myPlayerInfo.Transform.Position, target.Transform.Position) < BestDistanceToFire;
        }

        private void GetClosestBonus(out Vector3? closest)
        {
            closest = null;
            if (myPlayerInfo == null)
            {
                return;
            }
            var bonus = AIGameWorldUtils.GetBonusInfosList();
            foreach (var item in bonus)
            {
                if (closest == null)
                {
                    closest = item.Position;
                    continue;
                }
                var dist = Vector3.Magnitude(item.Position - myPlayerInfo.Transform.Position);
                var dist2 = Vector3.Magnitude(closest.Value - myPlayerInfo.Transform.Position);
                if (dist < dist2)
                {
                    closest = item.Position;
                }
            }
            LastPosition = null;
        }


        private PlayerInformations myPlayerInfo;
        PlayerInformations target = null;
        private Vector3? LastPosition = null;

        private void LastPositionSet()
        {
            var pos = AIGameWorldUtils.GetProjectileInfosList();
            if (pos != null)
            {
                if (pos.Count != 0)
                {
                    LastPosition = pos[pos.Count - 1].Transform.Position;
                    return;
                }
            }
            LastPosition = Vector3.zero;
        }

        public PlayerData LastPlayerData = null;
        public PlayerData LastTargetData = null;
        private List<AIAction> actionList;
        private Vector3? PlayerCurrPos;
        private void GetPlayerAndClosestTarget()
        {
            playerInfos = AIGameWorldUtils.GetPlayerInfosList();
            myPlayerInfo = GetPlayerInfos(AIId, playerInfos);
            PlayerCurrPos = myPlayerInfo?.Transform.Position;
            foreach (PlayerInformations playerInfo in playerInfos)
            {
                if (!playerInfo.IsActive)
                    continue;

                if (playerInfo.PlayerId == AIId)
                    continue;

                if (!playerInfo.IsActive)
                    continue;
                if (target != null && myPlayerInfo != null)
                {
                    float newDist = Vector3.Magnitude(PlayerCurrPos.Value - playerInfo.Transform.Position);
                    float oldDist = Vector3.Magnitude(PlayerCurrPos.Value - target.Transform.Position);
                    if (newDist < BestDistanceToFire * 2)
                    {
                        if (oldDist < BestDistanceToFire * 2)
                        {
                            if (target.CurrentHealth < playerInfo.CurrentHealth)
                            {
                                continue;
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                target = playerInfo;
                //ComputeBestTarget(targets);
                TargetVisibilityTest();
                //break;
            }
        }

        private bool TargetVisible = false;

        private void TargetVisibilityTest()
        {
            Vector3 targetPos = target.Transform.Position;
            if (Physics.Raycast(PlayerCurrPos.Value, targetPos - PlayerCurrPos.Value, out RaycastHit info, 100))
            {
                TargetVisible = (Vector3.Magnitude(info.point - targetPos) < 1.2f);
            }
        }

        private void ComputeBestTarget(List<PlayerInformations> targets)
        {
            List<int> reachablesIndexes = new List<int>();
            // cibles atteignables
            for (int i = 0; i < targets.Count; i++)
            {
                Vector3 targetPos = targets[i].Transform.Position;
                if (Physics.Raycast(PlayerCurrPos.Value, targetPos - PlayerCurrPos.Value, out RaycastHit info, 100))
                {
                    if (Vector3.Magnitude(info.point - targetPos) < 1.2f)
                    {
                        reachablesIndexes.Add(i);
                    }
                }
            }
            if (reachablesIndexes.Count == 1)
            {
                target = targets[reachablesIndexes[0]];
                return;
            }

            // cible la + faible
            if (AIGameWorldUtils.GetBonusInfosList().Count != 0)
            {
                target = null;
            }
            foreach (var index in reachablesIndexes)
            {
                float val = targets[index].CurrentHealth;
                if (target == null)
                {
                    target = targets[index];
                    continue;
                }
                if (val < target.CurrentHealth)
                {
                    target = targets[index];
                }
            }
        }

        private List<PlayerInformations> playerInfos;

        public List<AIAction> ComputeAIDecision()
        {
            Vector3? closest = null;
            Sequence mainSequence = new Sequence();
            mainSequence.Add(new ActionNode(ResetValues));
            mainSequence.Add(new ActionNode(GetPlayerAndClosestTarget));

            // continuer si player != null
            mainSequence.Add(new ConditionNode(new Condition(() => myPlayerInfo != null)));
            mainSequence.Add(new ActionNode(() => GetClosestBonus(out closest)));

            Selector TargetSelector = new Selector();
            mainSequence.Add(TargetSelector);
            Sequence nullTargetSeq = new Sequence();
            Sequence notNullTargetSeq = new Sequence();
            TargetSelector.Add(nullTargetSeq);
            TargetSelector.Add(notNullTargetSeq);

            nullTargetSeq.Add(new ConditionNode(new Condition(() => target == null)));
            Selector nullClosestInNullTargetSelector = new Selector();
            nullTargetSeq.Add(nullClosestInNullTargetSelector);



            Sequence notNullClosestInNullTargetSequence = new Sequence();
            notNullClosestInNullTargetSequence.Add(new ConditionNode(new Condition(() => closest != null)));
            notNullClosestInNullTargetSequence.Add(new ActionNode(new Action(() => MoveTo(closest.Value))));
            notNullClosestInNullTargetSequence.Add(new ActionNode(new Action(() => DashFunction(closest.Value - myPlayerInfo.Transform.Position))));

            nullClosestInNullTargetSelector.Add(notNullClosestInNullTargetSequence);
            nullClosestInNullTargetSelector.Add(new ConditionNode(new ConditionAlwaysTrue()));
            //null sequence


            notNullTargetSeq.Add(new ActionNode(() =>
            {
                int score = 0;
                myPlayerInfo.BonusOnPlayer.TryGetValue(EBonusType.CooldownReduction, out int value);
                score += value;
                myPlayerInfo.BonusOnPlayer.TryGetValue(EBonusType.Damage, out value);
                score += value;
                if (closest != null && score < 3)
                {
                    LastPositionSet();
                    float targetDist = Vector3.Magnitude(PlayerCurrPos.Value - target.Transform.Position);
                    if (targetDist > Vector3.Magnitude(PlayerCurrPos.Value - closest.Value) || !TargetVisible)
                    {
                        MoveTo(closest.Value);
                    }
                    else
                    {
                        if (myPlayerInfo.CurrentHealth < myPlayerInfo.MaxHealth * .8f)
                        {
                            LastPositionMoveFunction();
                        }
                        else
                        {
                            MoveTo(target.Transform.Position + (targetDist > BestDistanceToFire ? 18 : 4) * new Vector3(-target.Transform.Position.z, 0, target.Transform.Position.x).normalized);
                        }
                        //if (Vector3.Magnitude(PlayerCurrPos.Value - target.Transform.Position) < BestDistanceToFire)
                        //{
                        //    LastPositionMoveFunction();
                        //}
                        //else
                        //{
                        //    //MoveTo(LastTargetData == null ? target.Transform.Position : LastTargetData.Position);
                        //    MoveTo(target.Transform.Position +  3 * new Vector3(-target.Transform.Position.z, 0, target.Transform.Position.x).normalized);
                        //}
                    }
                }
                else
                {
                    if (LastPosition == null)
                    {
                        LastPositionMoveFunction();
                    }
                    else
                    {
                        if (Vector3.Magnitude(LastPosition.Value - myPlayerInfo.Transform.Position) < 1.0f)
                        {
                            LastPositionMoveFunction();
                        }
                    }
                }

                if (myPlayerInfo.CurrentHealth < (myPlayerInfo.MaxHealth * .4f) || HealthChanged())
                {
                    DashFunction(closest != null && myPlayerInfo.CurrentHealth < myPlayerInfo.MaxHealth * 0.8f ? (PlayerCurrPos.Value - closest.Value) : myPlayerInfo.Transform.Position - LastPlayerData.Position);
                }


                FireFunction(target);
                SaveLastFrameInfo();
            }));

            mainSequence.Execute();
            return actionList;
        }

        private void ResetValues()
        {
            actionList = new List<AIAction>();
            target = null;
            TargetVisible = false;
        }

        private void LastPositionMoveFunction()
        {
            LastPositionSet();
            MoveTo(LastPosition.Value);
        }

        private void MoveTo(Vector3 pos)
        {
            AIActionMoveToDestination actionMove = new AIActionMoveToDestination();
            actionMove.Position = pos;
            actionList.Add(actionMove);
        }

        private void SaveLastFrameInfo()
        {
            LastPlayerData.Health = myPlayerInfo.CurrentHealth;
            LastPlayerData.Position = myPlayerInfo.Transform.Position;
            if (LastTargetData == null)
            {
                LastTargetData = new PlayerData();
            }
            LastTargetData.Position = target.Transform.Position;
            LastTargetData.Id = target.PlayerId;
        }

        private void FireFunction(PlayerInformations target)
        {
            AIActionLookAtPosition actionLookAt = new AIActionLookAtPosition();
            PreshotFonction(out bool success, out Vector3 targetPos);
            actionLookAt.Position = success ? targetPos : target.Transform.Position;
            actionList.Add(actionLookAt);
            actionList.Add(new AIActionFire());
        }

        private void PreshotFonction(out bool success, out Vector3 targetPos)
        {
            success = false;
            targetPos = Vector3.zero;
            if (LastTargetData != null)
            {
                if (LastTargetData.Id == target.PlayerId)
                {
                    Vector3 currPos = target.Transform.Position;
                    Vector3 lastPos = LastTargetData.Position;
                    float dist = Vector3.Magnitude(currPos - lastPos);
                    if (dist > 0.1f && dist < 8)
                    {
                        //float angle = Vector3.Angle((currPos - PlayerCurrPos.Value), (lastPos - PlayerCurrPos.Value));
                        Vector3 dir = -(lastPos - currPos);
                        success = true;
                        targetPos = currPos + (dir * ((currPos - PlayerCurrPos.Value).magnitude * 1.75f - 1.5f));
                    }
                }
            }
        }

        private void DashFunction(Vector3 dir)
        {
            if (myPlayerInfo.IsDashAvailable && !Physics.Raycast(PlayerCurrPos.Value - (Vector3.up * .5f), dir, 1.0f))
            {
                AIActionDash actionMove = new AIActionDash();
                actionMove.Direction = dir;
                actionList.Add(actionMove);
            }
        }

        private bool HealthChanged()
        {
            if (LastPlayerData != null)
            {
                if (LastPlayerData.Health > myPlayerInfo.CurrentHealth)
                {
                    return true;
                }
            }
            else
            {
                LastPlayerData = new PlayerData();
            }
            return false;
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
