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
        public string GetName() { return "Apagnan 1.0"; }

        public void SetAIGameWorldUtils(GameWorldUtils parGameWorldUtils) { AIGameWorldUtils = parGameWorldUtils; }

        //Fin du bloc de fonction nécessaire (Attention ComputeAIDecision en fait aussi partit)


        private float BestDistanceToFire = 10.0f;

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
        }


        private PlayerInformations myPlayerInfo;
        PlayerInformations target = null;
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
            //nullClosestInNullTargetSelector.Add(new ConditionNode(new ConditionAlwaysTrue()));
            Sequence nullClosestInNullTargetSeq = new Sequence();
            nullClosestInNullTargetSeq.Add(new ActionNode(() => MoveTo(MapData.BonusSpawns[0] == null ? Vector3.zero : MapData.BonusSpawns[0])));
            nullClosestInNullTargetSelector.Add(nullClosestInNullTargetSeq);
            //null sequence
            notNullTargetSeq.Add(new ActionNode(InitMapData));
            int score = 0;
            notNullTargetSeq.Add(new ActionNode(() => GetOffensiveScore(out score)));
            Selector bonusPresenceSelector = new Selector();
            Selector healthChangedSelector = new Selector();
            notNullTargetSeq.Add(bonusPresenceSelector);
            notNullTargetSeq.Add(healthChangedSelector);
            notNullTargetSeq.Add(new ActionNode(() => FireFunction(target)));
            notNullTargetSeq.Add(new ActionNode(SaveLastFrameInfo));

            Sequence yesBonus = new Sequence();
            Sequence noBonus = new Sequence();
            bonusPresenceSelector.Add(yesBonus);
            bonusPresenceSelector.Add(noBonus);

            yesBonus.Add(new ConditionNode(new Condition(() => closest != null && score < 3)));
            yesBonus.Add(new ActionNode(() => MoveTo(closest.Value)));

            noBonus.Add(new ActionNode(UpdateCheckpoint));
            noBonus.Add(new ActionNode(() => MoveTo(circlesCheckpoints[indexCheckpoint])));

            Sequence heathSequence = new Sequence();
            healthChangedSelector.Add(heathSequence);
            healthChangedSelector.Add(new ConditionNode(new ConditionAlwaysTrue()));

            heathSequence.Add(new ConditionNode(new Condition(() => (myPlayerInfo.CurrentHealth < (myPlayerInfo.MaxHealth * .6f) && myPlayerInfo.WeaponIsOnCooldown) || HealthChanged())));
            heathSequence.Add(new ActionNode(() => DashFunction(closest != null && myPlayerInfo.CurrentHealth < myPlayerInfo.MaxHealth * 0.8f ? (PlayerCurrPos.Value - closest.Value) : myPlayerInfo.Transform.Position - LastPlayerData.Position)));
            mainSequence.Execute();
            return actionList;
        }

        private void UpdateCheckpoint()
        {
            if ((PlayerCurrPos.Value - circlesCheckpoints[indexCheckpoint]).magnitude < 5f || (Time.time - CheckpointUpdated > 15))
            {
                CheckpointUpdated = Time.time;
                indexCheckpoint = (indexCheckpoint + 1) % circlesCheckpoints.Length;
            }
            for (int i = 0; i < 10; i++)
            {
                if (Physics.CheckSphere(circlesCheckpoints[indexCheckpoint], 1))
                {
                    indexCheckpoint = (indexCheckpoint + 1) % circlesCheckpoints.Length;
                }
                if (i == 9)
                {
                    //Debug.LogError("NO VALID POS");
                }
            }
        }

        private void GetOffensiveScore(out int score)
        {
            score = 0;
            if (myPlayerInfo.BonusOnPlayer.TryGetValue(EBonusType.CooldownReduction, out int value))
                score += value;
            if (myPlayerInfo.BonusOnPlayer.TryGetValue(EBonusType.Damage, out value))
                score += value;
        }

        private float CheckpointUpdated = 0;

        private void InitMapData()
        {
            if (MapData.BonusSpawns != null)
            {
                return;
            }
            if (!myPlayerInfo.WeaponIsOnCooldown)
            {
                return;
            }
            var bonus = AIGameWorldUtils.GetBonusInfosList();
            MapData.BonusSpawns = new Vector3[bonus.Count];
            Vector3 center = Vector3.zero;
            for (int i = 0; i < bonus.Count; i++)
            {
                MapData.BonusSpawns[i] = bonus[i].Position;
            }
            var players = AIGameWorldUtils.GetPlayerInfosList();
            MapData.PlayerSpawns = new Vector3[players.Count];
            for (int i = 0; i < players.Count; i++)
            {
                MapData.PlayerSpawns[i] = players[i].Transform.Position;
                center += MapData.PlayerSpawns[i];
            }
            center /= MapData.PlayerSpawns.Length;
            circlesCheckpoints = CreateCircleFromPoint(center, (center - MapData.PlayerSpawns[0]).magnitude * .7f, 8);
        }

        private Vector3[] circlesCheckpoints = null;
        private int indexCheckpoint = 0;

        private void ResetValues()
        {
            actionList = new List<AIAction>();
            target = null;
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

        public Vector3[] CreateCircleFromPoint(Vector3 center, float radius, int segments = 32)
        {
            Vector3[] points = new Vector3[segments];

            float angle = 0f;
            float angleStep = 2f * Mathf.PI / segments;

            for (int i = 0; i < segments; i++)
            {
                float x = center.x + radius * Mathf.Cos(angle);
                float z = center.z + radius * Mathf.Sin(angle);
                float y = center.y;

                Vector3 point = new Vector3(x, y, z);
                points[i] = point;

                angle += angleStep;
            }

            return points;
        }
    }
}

public static class MapData
{
    public static Vector3[] BonusSpawns = null;
    public static Vector3[] PlayerSpawns = null;
}