using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using Rust;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("AntiRaidHeli", "SeesAll", "0.3.2")]
    [Description("Deploys escalating patrol helicopters over active player raids.")]
    public class AntiRaidHeli : RustPlugin
    {
        [PluginReference] private Plugin SmartRecon, Clans, Friends;

        private const string AdminPermission = "antiraidheli.admin";
        private const string HelicopterPrefab =
            "assets/prefabs/npc/patrol helicopter/patrolhelicopter.prefab";
        private const string LabelMarkerPrefab =
            "assets/prefabs/deployable/vendingmachine/vending_mapmarker.prefab";
        private const string RadiusMarkerPrefab =
            "assets/prefabs/tools/map/genericradiusmarker.prefab";
        private const ulong EventHelicopterSkin = 94610420261005UL;
        private const float MarkerRadiusScale = 350f;
        private const string HostilityUiName = "AntiRaidHeli.Hostility";
        private const string HostilityIconBase64 =
            "iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAfnSURBVHhe7VlrrF1FFQYfUHxEBWs1UBVEBaM1WlJpiI/EapBnCUpRtFaLlaKJKJZHS2J80VYQAV8pAvpLEGsQbREEa9NWMalFqo02tdqkVi0xmPb23t5z9vq+b8y3z+zT3U2vld7Q3Bv3l5ycvWc9Zq2Z2WtmrTniiBYtWrRo0aJFixYtWrRo0aLF/z0GBgZeLOniTqfzupTSs5v08YZNmzYdpU7nVEmzBgYGJjbp+yGl9FKSG1JKCYBI/oXkvQAWRsS7Jb2sKTPWIOl4SbZ1Eckf2wf7Yp9IPmofmzJ9MPgNM44EgoMEHyV5B4B5qUhTJT2/qedwwX0XKk5LKc0jeSeA3xEcatpdR0R8s6mnj5TS8yS9KiLOlHQFyWUkfwHgrwQ7TWUGwSfMQ/L6iJiZUjqpqbcJScd1u903SpqibneKJD8f1+RrYnh4+KSIuEDUYpKrAPy7aY8BoFPa3LNrGYAr7NPw8PDJ9rGp96BwLEgpvVzS2yXNJbmE5I9I/h7AYNMAkjtMB3ClpDNSSi+s6wOw+Uky4OY6T0rpBUVRnCHgM7mvHU0ZAHsAbMz0JZI+KumttvWwxa+U0ou63e4bIuI89Iy9jeSv67NDsvBgkfwqgAUAUBLKL7PPE6ZF8CaSdqpb0awLwCMkb/eguq9utzvFfTftGTNIKT3LAakoitMFzCZ16z53eyCZmMei0f51AHNUFNOtw7qa+sclcoQ+leRPSd4KYDcCg34m+RPTJJ3QlBsFjmw2HDaklI5s/ipaRCyWdCaAx728Jb2HEUtHku0rHY9oOiLptSSXk/wCInY7kEXEFx3IUkqnNGXG5QCMZLSk05vfexOOGU25cYuU0jPz1vR5ANeQfFDSXgIPEXwIwBP59zDJn5Mc8r95s8xc62jqHVcg+cOU0m8J/tkzDOCzFQ3Ag3a49v7pzLO1lCGX9xWNN+zdu3eyCk2T9Aq/k/wSyeH60ZkoZ3tV9e5TWl4FZUBMKb1S0jTrqnjGPJx95VNZeUKUJIIr7TyD19d5y6VfGwCjDI5AQXCF1DsplYMSXJpSOqrOOyYBYEUV0CoHshMekAkN3p8BeKDeJulokgN9HWRNB1bUecckAGy3sYiQ09AqFQXwuU4vL1+QgGsAXE1gK8mtAq52yi3pKvMAuC7LNHVsb/ZXYaSd57DDaWl/yjIQCIK/YZUD/Bc4TyD5CIBo0iLizmZ/YxJlyip5a1uNwG6f+SvUZ/VAv4rPMuVRGVhtXdbZ7Odpx549eyYVRTGtKIrpRVG8LSLe6cpQRJwVEeemlGYq4kIAswB8AMAl/klyZujnOQT+7lhQVpgOvgDMV/JmmX+klOZYV4o0E8AH/RPkvt7vfiVd6DqE7fHxOlLMSJHe4VS6KIq3DA4OjlwBOhiKTrGwaeBTRelIhJ2vylJ3pJTeJ+nciLDx782p9EWZ5gxRiN4gjBYAFjX9+p8haVKeyXlVRQjAY5IudRuAywF8EsB8kmsB7ABwGYBPSLrc+7mdqD6B6mAj6RjT/C5peWJaUm1x+eBURv8suzTrcz+XEfgbwXXuX5L7mg/g4wAuJfhY2Q84LGCupEtGtQLqYNBlqO5IZ3WSXyO5vt4m6cTS8zyTCp3pdgC/2jdHPRBcnWXO6jH3Z3+/EhuA9e6r3lbBttlGBpc0aaNCVvwAyX+JvEHScysayS9LWuOsrme31rqqY5qkN9ed8VJPKT2jWtqNLU5Z5oK6TFEUU3M/7ndt5h30s/uu7MinyJsI7sw5x/SKNiq4iJFrfPcXRXFamcyQ36n2XUYsy0b57L7OzyTvNq3T6by6Kn/l5fwHSRMJrqnaqoNOjvaTSG6uDZADwcllP+T3S93guqp0z+BtptkWkt/1iVHSm0StJLhjaGho9MUVf+MefQCfIumoPIPkwxrS8aZ7V8jOvMYO5OeLTPNKQUS5MqqZ9gXFXmkyyf6W4Hqg7xwAXFznBeAT4XOsy4Ezy09Mnc4pfo6Id+V+TnBOEREzJO10FZjkjba94c5TR0ScT9L78WwvMQDXAthVfQY+95deIM2rjCT5rUpeKIPhAMnKqav8GQD4p7etshoE7DBvplnelzIDDn41O76d5b3lzs98i8s+PNC9M4NT7q8I+BBJF1rOr+QPGdu3bz8GgOt1d/vbyvn6gopOcFU25vaIcMJiIzfWdVQDk/nu9+2Md5OKDmCDpJc4aar4vBrqOghuzPI3MPi98hn8ZUWXdKVrDLYxpXQPgHubucchY9u2bRM8O5JucSBzWxUDAHxMVBER55QVYKnrmajLk1xRnvmlWZ6VXLdPEtc4gPaeNVnSOeYhucUDVdchyCX3roObpLMlFd4K9+ORzpN0s8vzW7ZsObpOe1qh4V6gMnz74n0+pTTVl5GKONtLWtLrK578CSwUy1slV4QWua2vr1c5tpAHZKIkX39N8K1Vjaff55iDL0DyMt4AwFH7Rrc3i5wk73J0r97rdEY4iHl7XZ+X/gH3/gpjJhs0ANyXv9HHfYdH8pYn82j2vu89zana+wNA3kyWdcOdeTDv20/BWAbJe7LR3t7WC/pIk6fK+TPfdQegf9iyVVpsnU2eMQsHvWy0v/1jm3SjKIprawOwX9CssGvXrmNrZ4KVTfqYRbfbdSLyx4j4wUj3esPDwyc6+/P26ecm3XBJPCLukvQnJztNeosWLVq0aNGiRYsWLVq0OET8B0X7lVaA6QBZAAAAAElFTkSuQmCC";

        private static readonly int HelicopterSightMask = Layers.Mask.Terrain
            | Layers.Mask.World | Layers.Mask.Construction | Layers.Mask.Deployed;
        private static readonly FieldInfo UseNapalmField = typeof(PatrolHelicopterAI)
            .GetField("useNapalm", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo PassNapalmField = typeof(PatrolHelicopterAI)
            .GetField("passNapalm", BindingFlags.Instance | BindingFlags.Public
                | BindingFlags.NonPublic);

        private PluginConfiguration _config;
        private readonly List<RaidIncident> _incidents = new List<RaidIncident>();
        private readonly HashSet<PatrolHelicopter> _retiringHelicopters =
            new HashSet<PatrolHelicopter>();
        private readonly Dictionary<uint, RecentConstructionRecord> _recentConstruction =
            new Dictionary<uint, RecentConstructionRecord>();
        private Timer _maintenanceTimer;
        private float _nextConstructionPruneAt;
        private float _nextConstructionSaveAt;
        private bool _constructionDataDirty;
        private bool _unloading;
        private uint _hostilityIconCrc;

        #region Oxide lifecycle

        private void Init()
        {
            permission.RegisterPermission(AdminPermission, this);
            RegisterMessages();
        }

        private void OnServerInitialized()
        {
            RegisterHostilityIcon();
            LoadConstructionHistory();
            LoadRaidProgress();
            _maintenanceTimer = timer.Every(1f, MaintainIncidents);
        }

        private void OnServerSave()
        {
            SaveConstructionHistory();
            SaveRaidProgress();
        }

        private void Unload()
        {
            _unloading = true;
            _maintenanceTimer?.Destroy();
            _maintenanceTimer = null;
            SaveConstructionHistory();
            SaveRaidProgress();

            foreach (BasePlayer player in BasePlayer.activePlayerList)
                CuiHelper.DestroyUi(player, HostilityUiName);

            RemoveHostilityIcon();

            for (int i = _incidents.Count - 1; i >= 0; i--)
                EndIncident(_incidents[i], false, true);

            _incidents.Clear();
            foreach (PatrolHelicopter helicopter in _retiringHelicopters)
            {
                if (helicopter != null && !helicopter.IsDestroyed)
                    helicopter.Kill();
            }
            _retiringHelicopters.Clear();
        }

        private void RegisterHostilityIcon()
        {
            if (FileStorage.server == null || CommunityEntity.ServerInstance == null)
                return;

            try
            {
                _hostilityIconCrc = FileStorage.server.Store(
                    Convert.FromBase64String(HostilityIconBase64),
                    FileStorage.Type.png, CommunityEntity.ServerInstance.net.ID);
            }
            catch (Exception exception)
            {
                _hostilityIconCrc = 0;
                PrintWarning("Unable to register the embedded hostility icon; "
                    + "using the built-in fallback silhouette instead: "
                    + exception.Message);
            }
        }

        private void RemoveHostilityIcon()
        {
            if (_hostilityIconCrc == 0 || FileStorage.server == null
                || CommunityEntity.ServerInstance == null)
                return;

            try
            {
                FileStorage.server.Remove(_hostilityIconCrc, FileStorage.Type.png,
                    CommunityEntity.ServerInstance.net.ID);
            }
            catch (Exception exception)
            {
                PrintWarning("Unable to remove the embedded hostility icon from "
                    + "FileStorage during unload: " + exception.Message);
            }
            _hostilityIconCrc = 0;
        }

        private void OnNewSave(string filename)
        {
            StopAllIncidents(false);
            _recentConstruction.Clear();
            _constructionDataDirty = false;
            Interface.Oxide.DataFileSystem.WriteObject(Name + "_RaidProgress",
                new RaidProgressData());
            Interface.Oxide.DataFileSystem.WriteObject(
                Name + "_ConstructionHistory", new ConstructionHistoryData());
        }

        private void OnPlayerDisconnected(BasePlayer player, string reason)
        {
            if (player != null)
                CuiHelper.DestroyUi(player, HostilityUiName);
        }

        private void OnEntityDeath(BasePlayer player, HitInfo info)
        {
            if (player != null)
                CuiHelper.DestroyUi(player, HostilityUiName);
        }

        protected override void LoadDefaultConfig()
        {
            _config = PluginConfiguration.CreateDefault();
            SaveConfig();
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();

            try
            {
                _config = Config.ReadObject<PluginConfiguration>();
                if (_config == null)
                    throw new JsonException("The configuration was empty.");
            }
            catch (Exception exception)
            {
                PrintError("Configuration could not be read; defaults will be used: "
                    + exception.Message);
                _config = PluginConfiguration.CreateDefault();
            }

            ValidateConfiguration();
            SaveConfig();
        }

        protected override void SaveConfig()
        {
            Config.WriteObject(_config, true);
        }

        #endregion

        #region Raid detection and aggression

        private object OnEntityTakeDamage(BaseCombatEntity entity, HitInfo info)
        {
            if (!_config.Enabled || entity == null || info == null)
                return null;

            RaidIncident attackingIncident = ResolveIncidentFromDamage(info);
            if (attackingIncident != null)
            {
                if (entity is BasePlayer)
                {
                    ResponseProfile profile = attackingIncident.GetCurrentProfile(_config);
                    if (profile != null
                        && info.damageTypes.GetMajorityDamageType() == DamageType.Bullet
                        && profile.BulletAccuracyPercent < 100f
                        && UnityEngine.Random.Range(0f, 100f) >= profile.BulletAccuracyPercent)
                    {
                        info.damageTypes.Clear();
                        return true;
                    }
                }
                else if (IsPotentialPlayerAsset(entity)
                    && !CanHelicopterDamageAsset(attackingIncident, entity))
                {
                    info.damageTypes.Clear();
                    return true;
                }

                return null;
            }

            if (entity is PatrolHelicopter)
            {
                RaidIncident incident = FindIncident(entity as PatrolHelicopter);
                BasePlayer helicopterAttacker = ResolveAttackingPlayer(info);
                if (incident != null && helicopterAttacker != null
                    && IsInsideDangerZone(incident, helicopterAttacker.transform.position))
                {
                    RecordCombatHostility(incident, helicopterAttacker,
                        Time.realtimeSinceStartup);
                }

                return null;
            }

            BasePlayer attackedPlayer = entity as BasePlayer;
            if (attackedPlayer != null)
            {
                BasePlayer playerAttacker = ResolveAttackingPlayer(info);
                if (playerAttacker != null && !playerAttacker.IsNpc
                    && attackedPlayer != playerAttacker)
                    RecordAggressionNear(playerAttacker, playerAttacker.transform.position);

                return null;
            }

            if (!IsQualifyingRaidStructure(entity) || !entity.OwnerID.IsSteamId())
                return null;

            BasePlayer attacker = ResolveAttackingPlayer(info);
            if (attacker == null || attacker.IsNpc)
                return null;

            float damage = info.damageTypes.Total();
            if (damage <= 0f || info.damageTypes.GetMajorityDamageType() == DamageType.Decay)
                return null;

            if (TryRefreshAdminTestRaidActivity(attacker, entity))
                return null;

            if (IsFriendlyStructureDamage(attacker, entity)
                && !IsKnownRaidAggressorContinuingRaid(attacker, entity))
                return null;

            RegisterRaidDamage(attacker, entity, damage);
            return null;
        }

        private bool TryRefreshAdminTestRaidActivity(BasePlayer attacker,
            BaseCombatEntity target)
        {
            if (attacker == null || target == null)
                return false;

            uint buildingId = GetBuildingId(target);
            if (buildingId == 0)
                return false;

            float now = Time.realtimeSinceStartup;
            for (int i = 0; i < _incidents.Count; i++)
            {
                RaidIncident incident = _incidents[i];
                if (incident == null || !incident.IsAdminTest || !incident.Qualified
                    || !incident.ProtectedBuildingIds.Contains(buildingId)
                    || !IsAggressorOwner(incident, attacker.userID))
                    continue;

                // Admin tests commonly use two structures owned by the same account.
                // Refresh the simulated raid without reclassifying that protected
                // structure (or its owner) as a real victim/aggressor asset.
                incident.LastRaidDamageAt = now;
                incident.LastRaidDamageUtc = UtcNowSeconds();
                incident.RecordRaidDamage(now,
                    _config.AdaptivePressure.ActivityWindowSeconds);
                RecordRaidAggressor(incident, attacker, now);
                ReactivateFromRaidDamage(incident);
                return true;
            }

            return false;
        }

        private void RegisterRaidDamage(BasePlayer attacker, BaseCombatEntity target, float damage)
        {
            float now = Time.realtimeSinceStartup;
            Vector3 position = target.transform.position;
            RaidIncident incident = FindIncident(position, _config.RaidDetection.MergeRadius);

            // Final-response clearance belongs only to the same raiding party
            // continuing against the same victim. A different group or a nearby
            // unrelated base must begin its own response chain.
            if (incident != null && incident.ResponseCompleted
                && (!IsVictimAsset(incident, target)
                    || !IsAggressorOwner(incident, attacker.userID)))
                incident = null;

            if (incident == null)
            {
                if (CountLiveIncidents() >= _config.RaidDetection.MaximumConcurrentRaidZones)
                    return;

                incident = new RaidIncident
                {
                    Center = position,
                    CreatedAt = now,
                    LastRaidDamageAt = now,
                    QualificationWindowStartedAt = now
                };
                _incidents.Add(incident);
            }

            if (now - incident.QualificationWindowStartedAt
                > _config.RaidDetection.QualificationWindowSeconds)
            {
                incident.QualificationWindowStartedAt = now;
                incident.QualifyingHits = 0;
                incident.QualifyingDamage = 0f;
            }

            RecordVictimAsset(incident, target);
            incident.LastRaidDamageAt = now;
            incident.LastRaidDamageUtc = UtcNowSeconds();
            incident.QualifyingHits++;
            incident.QualifyingDamage += damage;
            incident.RecordRaidDamage(now,
                _config.AdaptivePressure.ActivityWindowSeconds);
            RecordRaidAggressor(incident, attacker, now);

            // Let the hotspot follow nearby structural damage gradually without allowing one hit
            // near the merge boundary to teleport an established event away from its participants.
            if (!incident.Qualified)
                incident.Center = Vector3.Lerp(incident.Center, position, 0.35f);

            if (incident.Qualified)
            {
                ReactivateFromRaidDamage(incident);
                return;
            }

            bool enoughCumulativeDamage = incident.QualifyingDamage
                >= _config.RaidDetection.MinimumAccumulatedDamage;
            bool enoughHits = incident.QualifyingHits
                >= _config.RaidDetection.MinimumQualifyingHits;
            bool heavySingleHit = damage
                >= _config.RaidDetection.SingleHitDamageThreshold;
            if (!enoughCumulativeDamage || (!enoughHits && !heavySingleHit))
                return;

            QualifyIncident(incident);
        }

        private void QualifyIncident(RaidIncident incident)
        {
            incident.Qualified = true;
            incident.ResponseLevel = 1;
            CaptureVictimProtection(incident);
            CreateOrUpdateMarkers(incident);
            BroadcastRaidAlert(incident, "InitialAlert", incident.GetCurrentProfile(_config)?.Name);
            ScheduleHelicopter(incident,
                _config.RaidDetection.InitialResponseDelaySeconds);
        }

        private void ReactivateFromRaidDamage(RaidIncident incident)
        {
            if (incident == null || !incident.Qualified || incident.ResponseCompleted)
                return;

            if (!incident.PausedForInactivity && !incident.AwaitingRenewedRaidDamage)
                return;

            bool escalating = incident.AwaitingRenewedRaidDamage;
            incident.PausedForInactivity = false;
            incident.AwaitingRenewedRaidDamage = false;
            CreateOrUpdateMarkers(incident);
            BroadcastRaidAlert(incident, escalating ? "EscalationDeployed" : "ResponseResumed",
                incident.GetCurrentProfile(_config)?.Name);
            SpawnHelicopter(incident);
        }

        private void RecordAggressionNear(BasePlayer attacker, Vector3 position)
        {
            float now = Time.realtimeSinceStartup;
            for (int i = 0; i < _incidents.Count; i++)
            {
                RaidIncident incident = _incidents[i];
                if (!incident.Qualified || !IsInsideDangerZone(incident, position))
                    continue;

                RecordCombatHostility(incident, attacker, now);
            }
        }

        private static BasePlayer ResolveAttackingPlayer(HitInfo info)
        {
            if (info == null)
                return null;

            BasePlayer player = info.InitiatorPlayer;
            if (player != null)
                return player;

            ulong ownerId = info.Initiator?.OwnerID ?? 0UL;
            return ownerId.IsSteamId() ? BasePlayer.FindByID(ownerId) : null;
        }

        private static bool IsQualifyingRaidStructure(BaseCombatEntity entity)
        {
            return entity is BuildingBlock || entity is Door || entity is SimpleBuildingBlock
                || entity is BuildingPrivlidge;
        }

        private bool IsFriendlyStructureDamage(BasePlayer attacker, BaseCombatEntity entity)
        {
            ulong ownerId = entity?.OwnerID ?? 0UL;
            if (attacker == null || !ownerId.IsSteamId())
                return false;

            if (attacker.userID == ownerId)
                return true;

            if (_config.RaidDetection.IgnoreToolCupboardAuthorizedDamage
                && entity.GetBuildingPrivilege()?.IsAuthed(attacker) == true)
                return true;

            if (_config.RaidDetection.IgnoreNativeTeamDamage
                && attacker.currentTeam != 0UL)
            {
                RelationshipManager.PlayerTeam team = RelationshipManager.ServerInstance
                    ?.FindTeam(attacker.currentTeam);
                if (team != null && team.members.Contains(ownerId))
                    return true;
            }

            if (_config.RaidDetection.IgnoreClanDamage && Clans != null && Clans.IsLoaded)
            {
                string attackerClan = Clans.Call("GetClanOf", attacker.userID) as string;
                string ownerClan = Clans.Call("GetClanOf", ownerId) as string;
                if (!string.IsNullOrEmpty(attackerClan)
                    && string.Equals(attackerClan, ownerClan,
                        StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            if (_config.RaidDetection.IgnoreFriendDamage && Friends != null
                && Friends.IsLoaded)
            {
                object result = Friends.Call("AreFriends", attacker.UserIDString,
                    ownerId.ToString());
                if (result is bool && (bool)result)
                    return true;
            }

            return false;
        }

        private bool IsKnownRaidAggressorContinuingRaid(BasePlayer attacker,
            BaseCombatEntity target)
        {
            if (attacker == null || target == null)
                return false;

            Vector3 position = target.transform.position;
            float mergeRadiusSquared = _config.RaidDetection.MergeRadius
                * _config.RaidDetection.MergeRadius;
            for (int i = 0; i < _incidents.Count; i++)
            {
                RaidIncident incident = _incidents[i];
                if (incident == null || !incident.Qualified
                    || HorizontalDistanceSquared(incident.Center, position)
                        > mergeRadiusSquared
                    || !IsAggressorOwner(incident, attacker.userID)
                    || !IsVictimAsset(incident, target))
                    continue;

                return true;
            }

            return false;
        }

        private bool IsVictimAsset(RaidIncident incident,
            BaseCombatEntity target)
        {
            if (incident == null || target == null)
                return false;

            uint buildingId = GetBuildingId(target);
            return (buildingId != 0
                    && incident.ProtectedBuildingIds.Contains(buildingId))
                || (target.OwnerID.IsSteamId()
                    && IsVictimOwner(incident, target.OwnerID));
        }

        private void RecordCombatHostility(RaidIncident incident, BasePlayer player,
            float time)
        {
            if (incident == null || player == null || !player.userID.IsSteamId())
                return;

            incident.RecordAggressor(player.userID, time);
        }

        private void RecordRaidAggressor(RaidIncident incident, BasePlayer player,
            float time)
        {
            if (incident == null || player == null || !player.userID.IsSteamId())
                return;

            RecordCombatHostility(incident, player, time);
            incident.AggressorOwnerIds.Add(player.userID);

            if (_config.RaidDetection.IgnoreNativeTeamDamage && player.currentTeam != 0UL)
            {
                RelationshipManager.PlayerTeam team = RelationshipManager.ServerInstance
                    ?.FindTeam(player.currentTeam);
                if (team?.members != null)
                {
                    foreach (ulong memberId in team.members)
                    {
                        if (memberId.IsSteamId())
                            incident.AggressorOwnerIds.Add(memberId);
                    }
                }
            }
        }

        private void RecordVictimAsset(RaidIncident incident, BaseCombatEntity target)
        {
            if (incident == null || target == null)
                return;

            if (target.OwnerID.IsSteamId())
                incident.VictimOwnerIds.Add(target.OwnerID);

            uint buildingId = GetBuildingId(target);
            if (buildingId != 0)
                incident.ProtectedBuildingIds.Add(buildingId);

            BuildingPrivlidge privilege = target as BuildingPrivlidge
                ?? target.GetBuildingPrivilege();
            if (privilege == null || privilege.authorizedPlayers == null)
                return;

            if (privilege.OwnerID.IsSteamId())
                incident.VictimOwnerIds.Add(privilege.OwnerID);
            foreach (ulong authorizedId in privilege.authorizedPlayers)
            {
                if (authorizedId.IsSteamId())
                    incident.VictimOwnerIds.Add(authorizedId);
            }
        }

        private void CaptureVictimProtection(RaidIncident incident)
        {
            if (incident == null || incident.VictimOwnerIds.Count == 0)
                return;

            ulong[] owners = new ulong[incident.VictimOwnerIds.Count];
            incident.VictimOwnerIds.CopyTo(owners);
            for (int i = 0; i < owners.Length; i++)
            {
                RelationshipManager.PlayerTeam team = RelationshipManager.ServerInstance
                    ?.FindPlayersTeam(owners[i]);
                if (team?.members == null)
                    continue;

                foreach (ulong memberId in team.members)
                {
                    if (memberId.IsSteamId())
                        incident.VictimOwnerIds.Add(memberId);
                }
            }
        }

        private RaidIncident ResolveIncidentFromDamage(HitInfo info)
        {
            BaseEntity source = info?.Initiator as BaseEntity;
            for (int depth = 0; source != null && depth < 4; depth++)
            {
                PatrolHelicopter helicopter = source as PatrolHelicopter;
                if (helicopter != null)
                    return FindIncident(helicopter);

                source = source.creatorEntity;
            }

            return null;
        }

        private static bool IsPotentialPlayerAsset(BaseCombatEntity entity)
        {
            if (entity == null || entity is BasePlayer || entity is PatrolHelicopter)
                return false;

            return entity.OwnerID.IsSteamId() || GetBuildingId(entity) != 0
                || entity.GetBuildingPrivilege() != null;
        }

        private bool CanHelicopterDamageAsset(RaidIncident incident,
            BaseCombatEntity entity)
        {
            if (!_config.RaidBaseIdentification.Enabled || incident == null || entity == null)
                return false;

            ulong ownerId = entity.OwnerID;
            if (ownerId.IsSteamId() && IsVictimOwner(incident, ownerId))
                return false;

            uint buildingId = GetBuildingId(entity);
            if (buildingId != 0 && incident.ProtectedBuildingIds.Contains(buildingId))
                return false;

            BuildingPrivlidge privilege = entity as BuildingPrivlidge
                ?? entity.GetBuildingPrivilege();
            if (IsAggressorAuthorized(incident, privilege))
                return true;

            if (ownerId.IsSteamId() && IsAggressorOwner(incident, ownerId))
            {
                incident.AggressorOwnerIds.Add(ownerId);
                return true;
            }

            if (buildingId != 0 && incident.HostileBuildingIds.Contains(buildingId))
                return true;

            return false;
        }

        private bool IsAggressorAuthorized(RaidIncident incident,
            BuildingPrivlidge privilege)
        {
            if (incident == null || privilege?.authorizedPlayers == null)
                return false;

            foreach (ulong authorizedId in privilege.authorizedPlayers)
            {
                if (authorizedId.IsSteamId()
                    && IsAggressorOwner(incident, authorizedId))
                    return true;
            }

            return false;
        }

        private bool IsVictimOwner(RaidIncident incident, ulong ownerId)
        {
            if (incident.VictimOwnerIds.Contains(ownerId))
                return true;

            foreach (ulong victimId in incident.VictimOwnerIds)
            {
                if (!ArePlayersAssociated(victimId, ownerId))
                    continue;
                return true;
            }

            return false;
        }

        private bool IsAggressorOwner(RaidIncident incident, ulong ownerId)
        {
            if (incident == null || !ownerId.IsSteamId())
                return false;
            if (incident.AggressorOwnerIds.Contains(ownerId))
                return true;

            foreach (ulong aggressorId in incident.AggressorOwnerIds)
                if (ArePlayersAssociated(aggressorId, ownerId))
                    return true;
            return false;
        }

        private bool ArePlayersAssociated(ulong firstId, ulong secondId)
        {
            if (!firstId.IsSteamId() || !secondId.IsSteamId())
                return false;
            if (firstId == secondId)
                return true;

            if (_config.RaidDetection.IgnoreNativeTeamDamage)
            {
                RelationshipManager.PlayerTeam team = RelationshipManager.ServerInstance
                    ?.FindPlayersTeam(firstId);
                if (team?.members?.Contains(secondId) == true)
                    return true;
            }

            if (_config.RaidDetection.IgnoreClanDamage && Clans != null && Clans.IsLoaded)
            {
                string firstClan = Clans.Call("GetClanOf", firstId) as string;
                string secondClan = Clans.Call("GetClanOf", secondId) as string;
                if (!string.IsNullOrEmpty(firstClan)
                    && string.Equals(firstClan, secondClan,
                        StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            if (_config.RaidDetection.IgnoreFriendDamage && Friends != null
                && Friends.IsLoaded)
            {
                object result = Friends.Call("AreFriends", firstId.ToString(),
                    secondId.ToString());
                if (result is bool && (bool)result)
                    return true;
            }

            return false;
        }

        private static uint GetBuildingId(BaseEntity entity)
        {
            DecayEntity decayEntity = entity as DecayEntity;
            if (decayEntity != null && decayEntity.buildingID != 0)
                return decayEntity.buildingID;

            BuildingPrivlidge privilege = entity?.GetBuildingPrivilege();
            return privilege?.buildingID ?? 0U;
        }

        private void OnEntityBuilt(Planner planner, GameObject gameObject)
        {
            if (!_config.RaidBaseIdentification.Enabled)
                return;

            BuildingBlock block = gameObject?.ToBaseEntity() as BuildingBlock;
            if (block == null)
                return;

            BasePlayer builder = planner?.GetOwnerPlayer();
            NextTick(() => RecordRecentConstruction(block, builder?.userID ?? block.OwnerID));
        }

        private void RecordRecentConstruction(BuildingBlock block, ulong builderId)
        {
            if (block == null || block.IsDestroyed || block.buildingID == 0)
                return;

            RecentConstructionRecord record;
            if (!_recentConstruction.TryGetValue(block.buildingID, out record))
            {
                record = new RecentConstructionRecord
                {
                    BuildingId = block.buildingID,
                    Position = block.transform.position
                };
                _recentConstruction[block.buildingID] = record;
            }

            record.AddPiece(block.transform.position, builderId, UtcNowSeconds());
            _constructionDataDirty = true;
        }

        private void MaintainRaidBaseCandidates(RaidIncident incident, float now)
        {
            RaidBaseIdentificationConfiguration settings = _config.RaidBaseIdentification;
            if (!settings.Enabled || incident == null || !incident.Qualified)
                return;

            double currentUtc = UtcNowSeconds();
            double maximumAgeSeconds = settings.ConstructionHistoryMinutes * 60d;
            double fastAgeSeconds = settings.FastClassificationWindowMinutes * 60d;
            float maximumDistanceSquared = settings.MaximumDistanceFromRaid
                * settings.MaximumDistanceFromRaid;

            foreach (KeyValuePair<uint, RecentConstructionRecord> pair
                in _recentConstruction)
            {
                RecentConstructionRecord record = pair.Value;
                if (record == null || record.PieceCount < settings.MinimumRecentPieces
                    || currentUtc - record.LastBuiltUtc > maximumAgeSeconds
                    || HorizontalDistanceSquared(record.Position, incident.Center)
                        > maximumDistanceSquared)
                    continue;

                bool victimOwned = false;
                bool aggressorOwned = false;
                for (int ownerIndex = 0; ownerIndex < record.OwnerIds.Count; ownerIndex++)
                {
                    ulong ownerId = record.OwnerIds[ownerIndex];
                    if (IsVictimOwner(incident, ownerId))
                    {
                        victimOwned = true;
                        break;
                    }
                    if (IsAggressorOwner(incident, ownerId))
                        aggressorOwned = true;
                }

                if (victimOwned)
                {
                    incident.CandidateUseStartedAt.Remove(record.BuildingId);
                    continue;
                }

                if (aggressorOwned)
                {
                    incident.HostileBuildingIds.Add(record.BuildingId);
                    incident.CandidateUseStartedAt.Remove(record.BuildingId);
                    continue;
                }

                if (!IsAggressorUsingRaidBase(incident, record.Position,
                    settings.AggressorUseProximity))
                {
                    incident.CandidateUseStartedAt.Remove(record.BuildingId);
                    continue;
                }

                float startedAt;
                if (!incident.CandidateUseStartedAt.TryGetValue(record.BuildingId,
                    out startedAt))
                {
                    incident.CandidateUseStartedAt[record.BuildingId] = now;
                    continue;
                }

                float requiredSeconds = currentUtc - record.LastBuiltUtc <= fastAgeSeconds
                    ? settings.FastClassificationUseSeconds
                    : settings.OlderConstructionUseSeconds;
                if (now - startedAt >= requiredSeconds)
                {
                    incident.HostileBuildingIds.Add(record.BuildingId);
                    incident.CandidateUseStartedAt.Remove(record.BuildingId);
                }
            }
        }

        private bool IsAggressorUsingRaidBase(RaidIncident incident, Vector3 position,
            float proximity)
        {
            float proximitySquared = proximity * proximity;
            foreach (BasePlayer player in BasePlayer.activePlayerList)
            {
                if (player == null || !player.IsConnected || player.IsDead()
                    || HorizontalDistanceSquared(player.transform.position, position)
                        > proximitySquared)
                    continue;

                if (IsAggressorOwner(incident, player.userID))
                    return true;
            }

            return false;
        }

        private void LoadConstructionHistory()
        {
            _recentConstruction.Clear();
            if (!_config.RaidBaseIdentification.Enabled
                || !_config.RaidBaseIdentification.PersistHistory)
                return;

            try
            {
                ConstructionHistoryData data = Interface.Oxide.DataFileSystem
                    .ReadObject<ConstructionHistoryData>(Name + "_ConstructionHistory");
                if (data?.Records == null)
                    return;

                double oldestAllowed = UtcNowSeconds()
                    - _config.RaidBaseIdentification.ConstructionHistoryMinutes * 60d;
                for (int i = 0; i < data.Records.Count; i++)
                {
                    RecentConstructionRecord record = data.Records[i];
                    if (record != null && record.BuildingId != 0
                        && record.LastBuiltUtc >= oldestAllowed)
                        _recentConstruction[record.BuildingId] = record;
                    else
                        _constructionDataDirty = true;
                }
            }
            catch (Exception exception)
            {
                PrintWarning("Recent construction history could not be loaded: "
                    + exception.Message);
            }
        }

        private void SaveConstructionHistory()
        {
            if (!_config.RaidBaseIdentification.Enabled
                || !_config.RaidBaseIdentification.PersistHistory
                || !_constructionDataDirty)
                return;

            try
            {
                PruneConstructionHistory();
                ConstructionHistoryData data = new ConstructionHistoryData();
                data.Records.AddRange(_recentConstruction.Values);
                Interface.Oxide.DataFileSystem.WriteObject(
                    Name + "_ConstructionHistory", data);
                _constructionDataDirty = false;
            }
            catch (Exception exception)
            {
                PrintWarning("Recent construction history could not be saved: "
                    + exception.Message);
            }
        }

        private void PruneConstructionHistory()
        {
            double oldestAllowed = UtcNowSeconds()
                - _config.RaidBaseIdentification.ConstructionHistoryMinutes * 60d;
            List<uint> expired = null;
            foreach (KeyValuePair<uint, RecentConstructionRecord> pair
                in _recentConstruction)
            {
                if (pair.Value != null && pair.Value.LastBuiltUtc >= oldestAllowed)
                    continue;

                if (expired == null)
                    expired = new List<uint>();
                expired.Add(pair.Key);
            }

            if (expired == null)
                return;
            for (int i = 0; i < expired.Count; i++)
                _recentConstruction.Remove(expired[i]);
            _constructionDataDirty = true;
        }

        private static double UtcNowSeconds()
        {
            return (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0,
                DateTimeKind.Utc)).TotalSeconds;
        }

        private void LoadRaidProgress()
        {
            try
            {
                RaidProgressData data = Interface.Oxide.DataFileSystem
                    .ReadObject<RaidProgressData>(Name + "_RaidProgress");
                if (data?.Incidents == null)
                    return;

                double nowUtc = UtcNowSeconds();
                double maximumAge = _config.RaidDetection.ResponseMemoryMinutes > 0f
                    ? _config.RaidDetection.ResponseMemoryMinutes * 60d
                    : double.MaxValue;
                for (int i = 0; i < data.Incidents.Count; i++)
                {
                    RaidProgressRecord record = data.Incidents[i];
                    if (record == null || record.ResponseLevel < 1
                        || nowUtc - record.LastRaidDamageUtc > maximumAge)
                        continue;

                    RaidIncident incident = new RaidIncident
                    {
                        Center = record.Center,
                        CreatedAt = Time.realtimeSinceStartup,
                        LastRaidDamageAt = Time.realtimeSinceStartup
                            - _config.RaidDetection.RaidInactivitySeconds - 1f,
                        LastRaidDamageUtc = record.LastRaidDamageUtc,
                        QualificationWindowStartedAt = Time.realtimeSinceStartup,
                        Qualified = true,
                        ResponseLevel = Mathf.Clamp(record.ResponseLevel, 1,
                            _config.ResponseProfiles.Count),
                        AwaitingRenewedRaidDamage = record.AwaitingRenewedRaidDamage,
                        PausedForInactivity = true,
                        ResponseCompleted = record.ResponseCompleted,
                        SavedHelicopterHealth = record.SavedHelicopterHealth,
                        SavedMainRotorHealth = record.SavedMainRotorHealth,
                        SavedTailRotorHealth = record.SavedTailRotorHealth
                    };
                    incident.VictimOwnerIds.UnionWith(record.VictimOwnerIds
                        ?? new List<ulong>());
                    incident.AggressorOwnerIds.UnionWith(record.AggressorOwnerIds
                        ?? new List<ulong>());
                    incident.ProtectedBuildingIds.UnionWith(record.ProtectedBuildingIds
                        ?? new List<uint>());
                    incident.HostileBuildingIds.UnionWith(record.HostileBuildingIds
                        ?? new List<uint>());
                    _incidents.Add(incident);
                }
            }
            catch (Exception exception)
            {
                PrintWarning("Raid response progress could not be loaded: "
                    + exception.Message);
            }
        }

        private void SaveRaidProgress()
        {
            try
            {
                var data = new RaidProgressData();
                double nowUtc = UtcNowSeconds();
                double maximumAge = _config.RaidDetection.ResponseMemoryMinutes > 0f
                    ? _config.RaidDetection.ResponseMemoryMinutes * 60d
                    : double.MaxValue;
                for (int i = 0; i < _incidents.Count; i++)
                {
                    RaidIncident incident = _incidents[i];
                    if (incident == null || !incident.Qualified || incident.IsAdminTest
                        || nowUtc - incident.LastRaidDamageUtc > maximumAge)
                        continue;

                    float health = incident.SavedHelicopterHealth;
                    float mainRotor = incident.SavedMainRotorHealth;
                    float tailRotor = incident.SavedTailRotorHealth;
                    PatrolHelicopter helicopter = incident.Helicopter;
                    if (helicopter != null && !helicopter.IsDestroyed)
                    {
                        health = Mathf.Max(1f, helicopter.Health());
                        if (helicopter.weakspots != null
                            && helicopter.weakspots.Length >= 2)
                        {
                            mainRotor = Mathf.Max(1f, helicopter.weakspots[0].health);
                            tailRotor = Mathf.Max(1f, helicopter.weakspots[1].health);
                        }
                    }

                    data.Incidents.Add(new RaidProgressRecord
                    {
                        Center = incident.Center,
                        LastRaidDamageUtc = incident.LastRaidDamageUtc,
                        ResponseLevel = incident.ResponseLevel,
                        AwaitingRenewedRaidDamage = incident.AwaitingRenewedRaidDamage,
                        ResponseCompleted = incident.ResponseCompleted,
                        SavedHelicopterHealth = health,
                        SavedMainRotorHealth = mainRotor,
                        SavedTailRotorHealth = tailRotor,
                        VictimOwnerIds = new List<ulong>(incident.VictimOwnerIds),
                        AggressorOwnerIds = new List<ulong>(incident.AggressorOwnerIds),
                        ProtectedBuildingIds = new List<uint>(incident.ProtectedBuildingIds),
                        HostileBuildingIds = new List<uint>(incident.HostileBuildingIds)
                    });
                }

                Interface.Oxide.DataFileSystem.WriteObject(
                    Name + "_RaidProgress", data);
            }
            catch (Exception exception)
            {
                PrintWarning("Raid response progress could not be saved: "
                    + exception.Message);
            }
        }

        #endregion

        #region Helicopter lifecycle

        private void ScheduleHelicopter(RaidIncident incident, float delaySeconds)
        {
            incident.SpawnTimer?.Destroy();
            incident.SpawnTimer = null;

            float delay = Mathf.Max(0f, delaySeconds);
            if (delay <= 0f)
            {
                SpawnHelicopter(incident);
                return;
            }

            incident.SpawnTimer = timer.Once(delay, () =>
            {
                incident.SpawnTimer = null;
                if (_unloading || !_incidents.Contains(incident) || !incident.Qualified)
                    return;

                SpawnHelicopter(incident);
            });
        }

        private void SpawnHelicopter(RaidIncident incident)
        {
            if (incident.Helicopter != null && !incident.Helicopter.IsDestroyed)
                return;

            ResponseProfile profile = incident.GetCurrentProfile(_config);
            if (profile == null)
            {
                EndIncident(incident, true, false);
                return;
            }

            Vector3 spawnPosition = GetHelicopterSpawnPosition(incident);
            Vector3 approachDirection = incident.Center - spawnPosition;
            approachDirection.y = 0f;
            Quaternion spawnRotation = approachDirection.sqrMagnitude > 0.01f
                ? Quaternion.LookRotation(approachDirection.normalized)
                : Quaternion.identity;

            PatrolHelicopter helicopter = GameManager.server.CreateEntity(
                HelicopterPrefab, spawnPosition, spawnRotation, true) as PatrolHelicopter;
            if (helicopter == null)
            {
                PrintError("Unable to create the AntiRaidHeli patrol helicopter.");
                ScheduleHelicopter(incident, 15f);
                return;
            }

            helicopter.enableSaving = false;
            helicopter.skinID = EventHelicopterSkin + (ulong)incident.ResponseLevel;
            helicopter._name = "AntiRaidHeli-Level-" + incident.ResponseLevel;
            incident.SuppressReplacement = false;
            incident.HelicopterReachedZone = false;
            incident.Helicopter = helicopter;
            incident.HelicopterSpawnedAt = Time.realtimeSinceStartup;
            helicopter.Spawn();

            // PatrolHelicopter's native initialization replaces the requested creation
            // point with a vanilla map-edge entry. Reapply our calculated position after
            // Spawn, matching the proven custom-helicopter sequence used by HeliSignals.
            helicopter.transform.SetPositionAndRotation(spawnPosition, spawnRotation);
            helicopter.UpdateNetworkGroup();
            helicopter.SendNetworkUpdateImmediate();

            NextTick(() => ConfigureHelicopter(incident, helicopter, profile));
            CreateOrUpdateMarkers(incident);
        }

        private Vector3 GetHelicopterSpawnPosition(RaidIncident incident)
        {
            if (string.Equals(_config.Helicopter.SpawnMode, "OverRaidArea",
                StringComparison.OrdinalIgnoreCase))
            {
                Vector3 overhead = incident.Center;
                overhead.y = GetSurfaceHeight(overhead)
                    + _config.Helicopter.SpawnHeight;
                return overhead;
            }

            float minimumDistance = _config.Targeting.DangerZoneRadius + 100f;
            float preferredDistance = Mathf.Max(
                _config.Helicopter.LandSpawnDistanceFromRaid, minimumDistance);
            Vector3 spawnPosition;
            if (!TryFindNearbyDryLand(incident.Center, preferredDistance,
                out spawnPosition))
                spawnPosition = incident.Center;

            spawnPosition.y = GetSurfaceHeight(spawnPosition)
                + _config.Helicopter.SpawnHeight;
            return spawnPosition;
        }

        private static bool TryFindNearbyDryLand(Vector3 center,
            float preferredDistance, out Vector3 position)
        {
            position = Vector3.zero;
            if (TerrainMeta.HeightMap == null || TerrainMeta.WaterMap == null)
                return false;

            const int directionSamples = 72;
            const float distanceStep = 25f;
            float halfX = TerrainMeta.Size.x * 0.5f - 20f;
            float halfZ = TerrainMeta.Size.z * 0.5f - 20f;
            float rotationOffset = UnityEngine.Random.Range(0f, Mathf.PI * 2f);

            // Prefer the configured stand-off, then work inward only when a small
            // island or coastal raid provides no dry point at that radius.
            for (float distance = preferredDistance; distance >= 25f;
                distance -= distanceStep)
            {
                for (int directionIndex = 0; directionIndex < directionSamples;
                    directionIndex++)
                {
                    float angle = rotationOffset + Mathf.PI * 2f
                        * directionIndex / directionSamples;
                    Vector3 candidate = center + new Vector3(Mathf.Cos(angle), 0f,
                        Mathf.Sin(angle)) * distance;
                    if (Mathf.Abs(candidate.x) >= halfX || Mathf.Abs(candidate.z) >= halfZ
                        || !IsDryLand(candidate))
                        continue;

                    position = candidate;
                    return true;
                }
            }

            if (!IsDryLand(center))
                return false;

            position = center;
            return true;
        }

        private static bool IsDryLand(Vector3 position)
        {
            float terrainHeight = TerrainMeta.HeightMap.GetHeight(position);
            float waterHeight = TerrainMeta.WaterMap.GetHeight(position);
            return terrainHeight > WaterSystem.OceanLevel + 0.25f
                && waterHeight <= terrainHeight + 0.25f;
        }

        private static float GetSurfaceHeight(Vector3 position)
        {
            float height = position.y;
            if (TerrainMeta.HeightMap != null)
                height = Mathf.Max(height, TerrainMeta.HeightMap.GetHeight(position));
            if (TerrainMeta.WaterMap != null)
                height = Mathf.Max(height, TerrainMeta.WaterMap.GetHeight(position));
            return height;
        }

        private void ConfigureHelicopter(RaidIncident incident, PatrolHelicopter helicopter,
            ResponseProfile profile)
        {
            if (helicopter == null || helicopter.IsDestroyed || incident.Helicopter != helicopter)
                return;

            float startingHealth = incident.SavedHelicopterHealth > 0f
                ? Mathf.Min(incident.SavedHelicopterHealth, profile.Health)
                : profile.Health;
            helicopter._maxHealth = profile.Health;
            helicopter.startHealth = profile.Health;
            helicopter.InitializeHealth(startingHealth, profile.Health);
            helicopter.bulletDamage = profile.BulletDamage;
            helicopter.bulletSpeed = profile.BulletSpeed;

            if (helicopter.weakspots != null && helicopter.weakspots.Length >= 2)
            {
                helicopter.weakspots[0].maxHealth = profile.MainRotorHealth;
                helicopter.weakspots[0].health = incident.SavedMainRotorHealth > 0f
                    ? Mathf.Min(incident.SavedMainRotorHealth, profile.MainRotorHealth)
                    : profile.MainRotorHealth;
                helicopter.weakspots[1].maxHealth = profile.TailRotorHealth;
                helicopter.weakspots[1].health = incident.SavedTailRotorHealth > 0f
                    ? Mathf.Min(incident.SavedTailRotorHealth, profile.TailRotorHealth)
                    : profile.TailRotorHealth;
            }

            PatrolHelicopterAI ai = helicopter.myAI;
            if (ai != null)
            {
                ai.maxSpeed = profile.MaximumSpeed;
                ai.maxRotationSpeed = profile.MaximumRotationSpeed;
                ai.hasInterestZone = true;
                ai.interestZoneOrigin = incident.Center;
                ai.timeBetweenRockets = profile.SecondsBetweenRockets;
                ai.numRocketsLeft = profile.EnableRockets
                    ? profile.MaximumRocketsPerAttack : 0;
                ai.lastStrafeTime = Time.realtimeSinceStartup;

                if (ai.leftGun != null)
                {
                    ai.leftGun.fireRate = profile.GunFireRate;
                    ai.leftGun.burstLength = profile.BurstLengthSeconds;
                    ai.leftGun.timeBetweenBursts = profile.SecondsBetweenBursts;
                    ai.leftGun.maxTargetRange = profile.MaximumTargetRange;
                    ai.leftGun.loseTargetAfter = Mathf.Max(12f,
                        profile.SecondsBetweenBursts + profile.BurstLengthSeconds + 2f);
                }

                if (ai.rightGun != null)
                {
                    ai.rightGun.fireRate = profile.GunFireRate;
                    ai.rightGun.burstLength = profile.BurstLengthSeconds;
                    ai.rightGun.timeBetweenBursts = profile.SecondsBetweenBursts;
                    ai.rightGun.maxTargetRange = profile.MaximumTargetRange;
                    ai.rightGun.loseTargetAfter = Mathf.Max(12f,
                        profile.SecondsBetweenBursts + profile.BurstLengthSeconds + 2f);
                }

                ai.ExitCurrentState();
                ai.State_Move_Enter(GetHelicopterDestination(incident));
            }

            helicopter.UpdateNetworkGroup();
            helicopter.SendNetworkUpdateImmediate();
            incident.SavedHelicopterHealth = 0f;
            incident.SavedMainRotorHealth = 0f;
            incident.SavedTailRotorHealth = 0f;
        }

        private void OnEntityKill(PatrolHelicopter helicopter)
        {
            if (helicopter != null && _retiringHelicopters.Remove(helicopter))
                return;

            RaidIncident incident = FindIncident(helicopter);
            if (incident == null)
                return;

            incident.Helicopter = null;
            ClearHelicopterTargets(helicopter?.myAI);

            if (_unloading || incident.SuppressReplacement)
                return;

            if (incident.ResponseLevel >= _config.ResponseProfiles.Count)
            {
                incident.ResponseCompleted = true;
                BroadcastRaidAlert(incident, "FinalDefeated", null);
                DestroyMarker(ref incident.LabelMarker);
                DestroyMarker(ref incident.RadiusMarker);
                SaveRaidProgress();
                return;
            }

            incident.ResponseLevel++;
            incident.AwaitingRenewedRaidDamage = true;
            incident.SavedHelicopterHealth = 0f;
            incident.SavedMainRotorHealth = 0f;
            incident.SavedTailRotorHealth = 0f;
            CreateOrUpdateMarkers(incident);
            BroadcastRaidAlert(incident, "EscalationAlert",
                incident.GetCurrentProfile(_config)?.Name);
            SaveRaidProgress();
        }

        private void OnEntitySpawned(TimedExplosive explosive)
        {
            if (explosive == null)
                return;

            NextTick(() =>
            {
                if (explosive == null || explosive.IsDestroyed)
                    return;

                PatrolHelicopter helicopter = explosive.creatorEntity as PatrolHelicopter;
                RaidIncident incident = FindIncident(helicopter);
                ResponseProfile profile = incident?.GetCurrentProfile(_config);
                if (profile == null || Mathf.Approximately(profile.RocketDamageScale, 1f))
                    return;

                foreach (DamageTypeEntry entry in explosive.damageTypes)
                    entry.amount *= profile.RocketDamageScale;
            });
        }

        private object CanHelicopterTarget(PatrolHelicopterAI ai, BasePlayer player)
        {
            PatrolHelicopter helicopter = ai?.helicopterBase as PatrolHelicopter;
            RaidIncident incident = FindIncident(helicopter);
            if (incident == null)
                return helicopter != null && _retiringHelicopters.Contains(helicopter)
                    ? (object)false : null;

            return IsEligibleTarget(incident, ai, player) ? null : (object)false;
        }

        private object OnHelicopterTarget(HelicopterTurret turret, BasePlayer player)
        {
            PatrolHelicopterAI ai = turret?._heliAI;
            PatrolHelicopter helicopter = ai?.helicopterBase as PatrolHelicopter;
            RaidIncident incident = FindIncident(helicopter);
            if (incident == null)
                return helicopter != null && _retiringHelicopters.Contains(helicopter)
                    ? (object)false : null;

            return IsEligibleTarget(incident, ai, player) ? null : (object)false;
        }

        private object CanHelicopterStrafe(PatrolHelicopterAI ai)
        {
            PatrolHelicopter helicopter = ai?.helicopterBase as PatrolHelicopter;
            RaidIncident incident = FindIncident(helicopter);
            if (incident == null)
                return helicopter != null && _retiringHelicopters.Contains(helicopter)
                    ? (object)false : null;

            ResponseProfile profile = incident.GetCurrentProfile(_config);
            if (profile == null || !profile.EnableRockets
                || profile.MaximumRocketsPerAttack <= 0)
                return false;

            if (Time.realtimeSinceStartup - ai.lastStrafeTime
                < GetAdaptiveRocketCooldown(incident, profile))
                return false;

            return true;
        }

        private object CanHelicopterStrafeTarget(PatrolHelicopterAI ai, BasePlayer player)
        {
            PatrolHelicopter helicopter = ai?.helicopterBase as PatrolHelicopter;
            RaidIncident incident = FindIncident(helicopter);
            if (incident == null)
                return helicopter != null && _retiringHelicopters.Contains(helicopter)
                    ? (object)false : null;

            return IsEligibleTarget(incident, ai, player)
                && IsRecentAggressor(incident, player) ? null : (object)false;
        }

        private object OnHelicopterStrafeEnter(PatrolHelicopterAI ai,
            Vector3 strafePosition, BasePlayer strafeTarget)
        {
            PatrolHelicopter helicopter = ai?.helicopterBase as PatrolHelicopter;
            RaidIncident incident = FindIncident(helicopter);
            if (incident == null)
                return helicopter != null && _retiringHelicopters.Contains(helicopter)
                    ? (object)false : null;

            ResponseProfile profile = incident.GetCurrentProfile(_config);
            bool approvedPlayerStrafe = strafeTarget != null
                && IsEligibleTarget(incident, ai, strafeTarget)
                && IsRecentAggressor(incident, strafeTarget);
            bool approvedShelterStrafe = strafeTarget != null
                && IsRecentAggressor(incident, strafeTarget)
                && IsAggressorOwner(incident, strafeTarget.userID)
                && Time.realtimeSinceStartup <= incident.ApprovedStructureStrafeUntil
                && HorizontalDistance(strafeTarget.transform.position,
                    incident.ApprovedStructureStrafePosition) <= 15f;
            if (profile == null || !profile.EnableRockets
                || (!approvedPlayerStrafe && !approvedShelterStrafe))
                return false;

            ai.lastStrafeTime = Time.realtimeSinceStartup;
            ai.lastRocketTime = 0f;
            // Native code decrements the counter before firing the first projectile.
            ai.numRocketsLeft = GetAdaptiveRocketCount(incident, profile) + 1;
            ai.timeBetweenRockets = profile.SecondsBetweenRockets;

            bool useNapalm = profile.EnableNapalm
                && UnityEngine.Random.Range(0f, 100f)
                    < GetAdaptiveNapalmChance(incident, profile);
            if (UseNapalmField != null)
                UseNapalmField.SetValue(ai, useNapalm);
            if (PassNapalmField != null)
                PassNapalmField.SetValue(ai, useNapalm);

            return null;
        }

        private int GetAdaptivePressureStage(RaidIncident incident)
        {
            if (!_config.AdaptivePressure.Enabled || incident == null)
                return 1;

            int hits = incident.CountRecentRaidHits(Time.realtimeSinceStartup,
                _config.AdaptivePressure.ActivityWindowSeconds);
            if (hits >= _config.AdaptivePressure.HeavyRaidHitThreshold)
                return 3;
            if (hits >= _config.AdaptivePressure.SustainedRaidHitThreshold)
                return 2;
            return hits > 0 ? 1 : 0;
        }

        private int GetAdaptiveRocketCount(RaidIncident incident,
            ResponseProfile profile)
        {
            int stage = GetAdaptivePressureStage(incident);
            float multiplier = stage >= 3
                ? _config.AdaptivePressure.HeavyRocketCountMultiplier
                : stage >= 2
                    ? _config.AdaptivePressure.SustainedRocketCountMultiplier : 1f;
            return Math.Max(0, Mathf.RoundToInt(profile.MaximumRocketsPerAttack
                * multiplier));
        }

        private float GetAdaptiveRocketCooldown(RaidIncident incident,
            ResponseProfile profile)
        {
            int stage = GetAdaptivePressureStage(incident);
            float multiplier = stage >= 3
                ? _config.AdaptivePressure.HeavyCooldownMultiplier
                : stage >= 2
                    ? _config.AdaptivePressure.SustainedCooldownMultiplier : 1f;
            return Mathf.Max(5f, profile.RocketAttackCooldownSeconds * multiplier);
        }

        private float GetAdaptiveNapalmChance(RaidIncident incident,
            ResponseProfile profile)
        {
            int stage = GetAdaptivePressureStage(incident);
            float bonus = stage >= 3
                ? _config.AdaptivePressure.HeavyNapalmBonusPercent
                : stage >= 2
                    ? _config.AdaptivePressure.SustainedNapalmBonusPercent : 0f;
            return Mathf.Clamp(profile.NapalmChancePercent + bonus, 0f, 100f);
        }

        #endregion

        #region Incident maintenance and targeting

        private void MaintainIncidents()
        {
            float now = Time.realtimeSinceStartup;
            if (now >= _nextConstructionPruneAt)
            {
                _nextConstructionPruneAt = now + 60f;
                PruneConstructionHistory();
            }
            if (now >= _nextConstructionSaveAt)
            {
                _nextConstructionSaveAt = now + 300f;
                SaveConstructionHistory();
            }

            _retiringHelicopters.RemoveWhere(helicopter => helicopter == null
                || helicopter.IsDestroyed);
            for (int i = _incidents.Count - 1; i >= 0; i--)
            {
                RaidIncident incident = _incidents[i];
                MaintainHostilityNotices(incident, now);

                if (!incident.Qualified)
                {
                    if (now - incident.CreatedAt
                        > _config.RaidDetection.QualificationWindowSeconds)
                        _incidents.RemoveAt(i);
                    continue;
                }

                if (now - incident.LastRaidDamageAt
                    > _config.RaidDetection.RaidInactivitySeconds)
                {
                    if (!incident.PausedForInactivity)
                        PauseIncident(incident, true);
                    else if (_config.RaidDetection.ResponseMemoryMinutes > 0f
                        && UtcNowSeconds() - incident.LastRaidDamageUtc
                            > _config.RaidDetection.ResponseMemoryMinutes * 60d)
                        EndIncident(incident, false, true);
                    continue;
                }

                if (incident.PausedForInactivity || incident.ResponseCompleted
                    || incident.AwaitingRenewedRaidDamage)
                    continue;

                MaintainRaidBaseCandidates(incident, now);

                PatrolHelicopter helicopter = incident.Helicopter;
                if (helicopter == null || helicopter.IsDestroyed || helicopter.myAI == null)
                    continue;

                if (_config.Helicopter.MaximumLifetimeSeconds > 0f
                    && now - incident.HelicopterSpawnedAt
                    >= _config.Helicopter.MaximumLifetimeSeconds)
                {
                    PauseIncident(incident, false);
                    continue;
                }

                MaintainHelicopterPosition(incident, helicopter.myAI);
                RefreshEligibleTargets(incident, helicopter.myAI);
                TryStartAggressorShelterStrafe(incident, helicopter.myAI);
            }
        }

        private void PauseIncident(RaidIncident incident, bool announce)
        {
            if (incident == null || incident.PausedForInactivity)
                return;

            incident.PausedForInactivity = true;
            incident.SpawnTimer?.Destroy();
            incident.SpawnTimer = null;
            DestroyMarker(ref incident.LabelMarker);
            DestroyMarker(ref incident.RadiusMarker);
            foreach (KeyValuePair<ulong, HostilityRecord> pair
                in incident.GetHostilityRecords())
            {
                BasePlayer player = BasePlayer.FindByID(pair.Key);
                if (player != null && player.IsConnected)
                    CuiHelper.DestroyUi(player, HostilityUiName);
            }

            PatrolHelicopter helicopter = incident.Helicopter;
            incident.Helicopter = null;
            if (helicopter != null && !helicopter.IsDestroyed)
            {
                incident.SavedHelicopterHealth = Mathf.Max(1f, helicopter.Health());
                if (helicopter.weakspots != null && helicopter.weakspots.Length >= 2)
                {
                    incident.SavedMainRotorHealth = Mathf.Max(1f,
                        helicopter.weakspots[0].health);
                    incident.SavedTailRotorHealth = Mathf.Max(1f,
                        helicopter.weakspots[1].health);
                }

                incident.SuppressReplacement = true;
                ClearHelicopterTargets(helicopter.myAI);
                _retiringHelicopters.Add(helicopter);
                helicopter.myAI?.Retire();
            }

            if (announce)
                BroadcastRaidAlert(incident, "AllClear");
            if (!_unloading)
                SaveRaidProgress();
        }

        private void MaintainHelicopterPosition(RaidIncident incident, PatrolHelicopterAI ai)
        {
            float distance = HorizontalDistance(ai.transform.position, incident.Center);
            if (!incident.HelicopterReachedZone
                && distance <= _config.Helicopter.OrbitRadius + 40f
                && ai._currentState != PatrolHelicopterAI.aiState.DEATH
                && ai._currentState != PatrolHelicopterAI.aiState.STRAFE
                && ai._currentState != PatrolHelicopterAI.aiState.ORBITSTRAFE)
            {
                incident.HelicopterReachedZone = true;
                ai.hasInterestZone = true;
                ai.interestZoneOrigin = incident.Center;
                ai.ExitCurrentState();
                ai.State_Orbit_Enter(_config.Helicopter.OrbitRadius);
                return;
            }

            float maximumDistance = _config.Targeting.DangerZoneRadius
                + _config.Helicopter.MaximumPatrolOvershoot;
            if (distance <= maximumDistance)
                return;

            if (ai._currentState == PatrolHelicopterAI.aiState.DEATH
                || ai._currentState == PatrolHelicopterAI.aiState.STRAFE
                || ai._currentState == PatrolHelicopterAI.aiState.ORBITSTRAFE)
                return;

            ai.hasInterestZone = true;
            ai.interestZoneOrigin = incident.Center;
            incident.HelicopterReachedZone = false;
            ai.ExitCurrentState();
            ai.State_Move_Enter(GetHelicopterDestination(incident));
        }

        private void RefreshEligibleTargets(RaidIncident incident, PatrolHelicopterAI ai)
        {
            for (int i = ai._targetList.Count - 1; i >= 0; i--)
            {
                PatrolHelicopterAI.targetinfo target = ai._targetList[i];
                BasePlayer player = target?.ply;
                if (player != null && IsEligibleTarget(incident, ai, player))
                {
                    // Rust's gun turrets abandon a target when this native visibility
                    // timestamp expires, even if it remains in _targetList.
                    ai.UpdateTargetLineOfSightTime(target);
                    continue;
                }

                ai._targetList.RemoveAt(i);
                if (ai.leftGun?._target == player)
                    ai.leftGun.ClearTarget();
                if (ai.rightGun?._target == player)
                    ai.rightGun.ClearTarget();
            }

            foreach (BasePlayer player in BasePlayer.activePlayerList)
            {
                if (!IsEligibleTarget(incident, ai, player) || HasTarget(ai, player))
                    continue;

                // Preserve the native acquisition bookkeeping used by both guns.
                ai.TryAddTarget(player);
            }

            if (ai.leftGun?._target is BasePlayer left
                && !IsEligibleTarget(incident, ai, left))
                ai.leftGun.ClearTarget();
            if (ai.rightGun?._target is BasePlayer right
                && !IsEligibleTarget(incident, ai, right))
                ai.rightGun.ClearTarget();

            BasePlayer fallbackTarget = null;
            for (int i = 0; i < ai._targetList.Count; i++)
            {
                BasePlayer candidate = ai._targetList[i]?.ply;
                if (candidate != null)
                {
                    fallbackTarget = candidate;
                    break;
                }
            }

            RefreshGunTarget(ai.leftGun, ai._targetList, fallbackTarget);
            RefreshGunTarget(ai.rightGun, ai._targetList, fallbackTarget);
        }

        private static void RefreshGunTarget(HelicopterTurret gun,
            List<PatrolHelicopterAI.targetinfo> targets, BasePlayer fallbackTarget)
        {
            if (gun == null)
                return;

            if (targets == null || targets.Count == 0)
            {
                if (gun.HasTarget())
                    gun.ClearTarget();
                return;
            }

            gun.UpdateTargetFromList(targets);
            if (!gun.HasTarget() && fallbackTarget != null)
                gun.SetTarget(fallbackTarget);
            gun.UpdateTargetVisibility();
        }

        private bool IsRecentAggressor(RaidIncident incident, BasePlayer player)
        {
            return incident != null && player != null
                && incident.IsRecentAggressor(player.userID,
                    Time.realtimeSinceStartup - _config.Targeting.HostilitySeconds);
        }

        private void MaintainHostilityNotices(RaidIncident incident, float now)
        {
            if (incident == null)
                return;

            float duration = _config.Targeting.HostilitySeconds;
            List<ulong> expiredRecords = null;
            foreach (KeyValuePair<ulong, HostilityRecord> pair
                in incident.GetHostilityRecords())
            {
                HostilityRecord record = pair.Value;
                if (record == null)
                {
                    (expiredRecords ??= new List<ulong>()).Add(pair.Key);
                    continue;
                }

                float remaining = duration - (now - record.LastHostileAt);
                float globalRemaining = GetHostilityRemaining(pair.Key);
                bool isNewestRecord = remaining >= globalRemaining - 0.5f;
                int stage = remaining <= 0f ? 0
                    : remaining <= 60f ? 1
                    : remaining <= 120f ? 2 : 3;
                BasePlayer player = BasePlayer.FindByID(pair.Key);
                if (_config.HostilityUi.Enabled && isNewestRecord
                    && remaining > 0f && player != null && player.IsConnected
                    && player.IsAlive())
                    DrawHostilityUi(player, remaining, false);

                if (stage >= record.LastNoticeStage)
                {
                    if (stage == 0)
                        (expiredRecords ??= new List<ulong>()).Add(pair.Key);
                    continue;
                }

                // Only the most recent incident may display countdown messages
                // for a player. This prevents an older overlapping raid from
                // announcing that hostility cleared while a newer timer is active.
                if (isNewestRecord && player != null && player.IsConnected
                    && player.IsAlive())
                {
                    if (_config.Targeting.SendPrivateHostilityCountdown)
                    {
                        string key = stage == 0 ? "HostilityExpired"
                            : stage == 1 ? "HostilityOneMinute"
                            : stage == 2 ? "HostilityTwoMinutes"
                            : "HostilityThreeMinutes";
                        Reply(player, key);
                    }

                    if (_config.HostilityUi.Enabled)
                    {
                        if (stage == 0 && globalRemaining <= 0f)
                        {
                            DrawHostilityUi(player, 0f, true);
                            timer.Once(_config.HostilityUi.ClearedDisplaySeconds, () =>
                            {
                                if (player != null && player.IsConnected
                                    && GetHostilityRemaining(player.userID) <= 0f)
                                    CuiHelper.DestroyUi(player, HostilityUiName);
                            });
                        }
                    }
                }
                record.LastNoticeStage = stage;
                if (stage == 0)
                    (expiredRecords ??= new List<ulong>()).Add(pair.Key);
            }

            if (expiredRecords == null)
                return;
            for (int i = 0; i < expiredRecords.Count; i++)
                incident.RemoveHostilityRecord(expiredRecords[i]);
        }

        private float GetHostilityRemaining(ulong userId)
        {
            float newestHostileAt = float.MinValue;
            for (int i = 0; i < _incidents.Count; i++)
            {
                HostilityRecord record;
                if (_incidents[i].TryGetHostilityRecord(userId, out record)
                    && record.LastHostileAt > newestHostileAt)
                    newestHostileAt = record.LastHostileAt;
            }

            return newestHostileAt == float.MinValue ? 0f
                : _config.Targeting.HostilitySeconds
                    - (Time.realtimeSinceStartup - newestHostileAt);
        }

        private void DrawHostilityUi(BasePlayer player, float remainingSeconds,
            bool cleared)
        {
            if (player == null || !player.IsConnected)
                return;

            CuiHelper.DestroyUi(player, HostilityUiName);
            int seconds = Mathf.Max(0, Mathf.CeilToInt(remainingSeconds));
            string countdown = string.Format("{0}M {1:00}S", seconds / 60,
                seconds % 60);
            var elements = new CuiElementContainer();
            string panel = elements.Add(new CuiPanel
            {
                Image = { Color = cleared ? _config.HostilityUi.ClearedColor
                    : _config.HostilityUi.HostileColor },
                RectTransform =
                {
                    AnchorMin = _config.HostilityUi.AnchorMin,
                    AnchorMax = _config.HostilityUi.AnchorMax
                }
            }, "Hud", HostilityUiName);

            AddHelicopterIndicatorIcon(elements, panel, cleared);

            elements.Add(new CuiLabel
            {
                RectTransform = { AnchorMin = "0.15 0", AnchorMax = "0.72 1" },
                Text =
                {
                    Text = cleared ? "HOSTILITY CLEARED" : "HELI HOSTILE",
                    FontSize = 11,
                    Align = TextAnchor.MiddleLeft,
                    Color = "1 1 1 1"
                }
            }, panel);
            elements.Add(new CuiLabel
            {
                RectTransform = { AnchorMin = "0.70 0", AnchorMax = "0.98 1" },
                Text =
                {
                    Text = cleared ? "SAFE" : countdown,
                    FontSize = 12,
                    Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                }
            }, panel);
            CuiHelper.AddUi(player, elements);
        }

        private void AddHelicopterIndicatorIcon(
            CuiElementContainer elements, string parent, bool cleared)
        {
            // Matches NoEscape's 13%-wide icon slot and dark silhouette. The
            // embedded PNG is registered in Rust FileStorage; CUI primitives
            // remain as a dependency-free fallback if registration ever fails.
            string color = cleared ? "0 0 0 0.38"
                : "0 0 0 0.42";

            if (_hostilityIconCrc != 0)
            {
                elements.Add(new CuiElement
                {
                    Parent = parent,
                    Components =
                    {
                        new CuiRawImageComponent
                        {
                            Png = _hostilityIconCrc.ToString(),
                            Color = color
                        },
                        new CuiRectTransformComponent
                        {
                            AnchorMin = "0.006 0.05",
                            AnchorMax = "0.136 0.95"
                        }
                    }
                });
                return;
            }

            AddIconPart(elements, parent, color, "0.022 0.37", "0.082 0.66");
            AddIconPart(elements, parent, color, "0.074 0.46", "0.112 0.55");
            AddIconPart(elements, parent, color, "0.106 0.35", "0.117 0.66");
            AddIconPart(elements, parent, color, "0.099 0.47", "0.124 0.54");
            AddIconPart(elements, parent, color, "0.057 0.64", "0.064 0.78");
            AddIconPart(elements, parent, color, "0.009 0.77", "0.108 0.83");
            AddIconPart(elements, parent, color, "0.030 0.27", "0.076 0.32");
            AddIconPart(elements, parent, color, "0.026 0.30", "0.032 0.39");
            AddIconPart(elements, parent, color, "0.074 0.30", "0.080 0.39");
        }

        private static void AddIconPart(CuiElementContainer elements,
            string parent, string color, string anchorMin, string anchorMax)
        {
            elements.Add(new CuiPanel
            {
                Image = { Color = color },
                RectTransform =
                {
                    AnchorMin = anchorMin,
                    AnchorMax = anchorMax
                }
            }, parent);
        }

        private void TryStartAggressorShelterStrafe(RaidIncident incident,
            PatrolHelicopterAI ai)
        {
            ResponseProfile profile = incident?.GetCurrentProfile(_config);
            if (profile == null || !profile.EnableRockets
                || profile.MaximumRocketsPerAttack <= 0 || ai == null
                || !incident.HelicopterReachedZone || ai._targetList.Count > 0
                || Time.realtimeSinceStartup - incident.LastRaidDamageAt
                    > _config.AdaptivePressure.StructurePressureSeconds
                || ai._currentState == PatrolHelicopterAI.aiState.STRAFE
                || ai._currentState == PatrolHelicopterAI.aiState.ORBITSTRAFE
                || Time.realtimeSinceStartup - ai.lastStrafeTime
                    < GetAdaptiveRocketCooldown(incident, profile))
                return;

            float oldestAllowed = Time.realtimeSinceStartup
                - _config.Targeting.HostilitySeconds;
            foreach (ulong userId in incident.GetAggressorIds())
            {
                if (!incident.IsRecentAggressor(userId, oldestAllowed))
                    continue;

                if (!IsAggressorOwner(incident, userId))
                    continue;

                BasePlayer player = BasePlayer.FindByID(userId);
                if (player == null || !player.IsConnected || !player.IsAlive()
                    || player.IsSleeping() || player.InSafeZone()
                    || IsPlayerInvisible(player)
                    || !IsInsideDangerZone(incident, player.transform.position))
                    continue;

                Vector3 shelterPosition;
                if (!TryFindDamageableAggressorShelter(incident, player.transform.position,
                    out shelterPosition))
                    continue;

                incident.ApprovedStructureStrafePosition = shelterPosition;
                incident.ApprovedStructureStrafeUntil = Time.realtimeSinceStartup + 5f;
                ai.lastRocketTime = 0f;
                ai.numRocketsLeft = GetAdaptiveRocketCount(incident, profile) + 1;
                ai.timeBetweenRockets = profile.SecondsBetweenRockets;

                bool useNapalm = profile.EnableNapalm
                    && UnityEngine.Random.Range(0f, 100f)
                        < GetAdaptiveNapalmChance(incident, profile);
                UseNapalmField?.SetValue(ai, useNapalm);
                PassNapalmField?.SetValue(ai, useNapalm);

                ai.ExitCurrentState();
                ai.State_Strafe_Enter(player);
                ai.lastStrafeTime = Time.realtimeSinceStartup;
                return;
            }
        }

        private bool TryFindDamageableAggressorShelter(RaidIncident incident,
            Vector3 playerPosition, out Vector3 shelterPosition)
        {
            shelterPosition = Vector3.zero;
            float bestDistance = float.MaxValue;
            List<BuildingBlock> blocks = Facepunch.Pool.Get<List<BuildingBlock>>();
            try
            {
                Vis.Entities(playerPosition, 15f, blocks, Layers.Mask.Construction,
                    QueryTriggerInteraction.Ignore);
                for (int i = 0; i < blocks.Count; i++)
                {
                    BuildingBlock block = blocks[i];
                    if (block == null || block.IsDestroyed
                        || !CanHelicopterDamageAsset(incident, block))
                        continue;

                    float distance = (block.transform.position - playerPosition).sqrMagnitude;
                    if (distance >= bestDistance)
                        continue;

                    bestDistance = distance;
                    shelterPosition = block.CenterPoint();
                }
            }
            finally
            {
                Facepunch.Pool.FreeUnmanaged(ref blocks);
            }

            return bestDistance < float.MaxValue;
        }

        private bool IsEligibleTarget(RaidIncident incident, PatrolHelicopterAI ai,
            BasePlayer player)
        {
            if (incident == null || ai == null || player == null || player.IsNpc
                || player.IsDestroyed || !player.IsConnected || !player.IsAlive()
                || player.IsSleeping() || player.IsWounded() || player.InSafeZone()
                || player.IsInTutorial || IsPlayerInvisible(player)
                || !IsInsideDangerZone(incident, player.transform.position))
                return false;

            ResponseProfile profile = incident.GetCurrentProfile(_config);
            if (profile == null)
                return false;

            bool recentlyAggressive = incident.IsRecentAggressor(player.userID,
                Time.realtimeSinceStartup - _config.Targeting.HostilitySeconds);

            float maxRange = Mathf.Min(profile.MaximumTargetRange,
                _config.Targeting.DangerZoneRadius + _config.Helicopter.MaximumPatrolOvershoot);
            if ((player.transform.position - ai.transform.position).sqrMagnitude
                > maxRange * maxRange)
                return false;

            if (!recentlyAggressive && !_config.Targeting.AllowVanillaThreatTargeting)
                return false;

            if (_config.Targeting.RequireArmedOrThreateningPlayer && !recentlyAggressive
                && !IsArmedOrThreatening(player))
                return false;

            return HasHelicopterLineOfSight(ai, player);
        }

        private bool IsArmedOrThreatening(BasePlayer player)
        {
            if (player == null)
                return false;
            if (player.GetThreatLevel() > _config.Targeting.MinimumNativeThreatLevel)
                return true;

            List<Item> beltItems = player.inventory?.containerBelt?.itemList;
            if (beltItems == null)
                return false;
            for (int i = 0; i < beltItems.Count; i++)
            {
                Item item = beltItems[i];
                if (item?.info != null && item.info.category == ItemCategory.Weapon)
                    return true;
            }
            return false;
        }

        private bool HasHelicopterLineOfSight(PatrolHelicopterAI ai, BasePlayer target)
        {
            if (ai?.helicopterBase == null || target == null)
                return false;

            Vector3 center = target.CenterPoint();
            Vector3 eyes = target.eyes != null ? target.eyes.position : center;
            bool initialized = false;

            if (ai.leftGun != null)
            {
                initialized = true;
                if (HasClearSight(ai.leftGun.transform.position, eyes)
                    || HasClearSight(ai.leftGun.transform.position, center))
                    return true;
            }

            if (ai.rightGun != null)
            {
                initialized = true;
                if (HasClearSight(ai.rightGun.transform.position, eyes)
                    || HasClearSight(ai.rightGun.transform.position, center))
                    return true;
            }

            if (initialized)
                return false;

            return HasClearSight(ai.helicopterBase.transform.position, eyes)
                || HasClearSight(ai.helicopterBase.transform.position, center);
        }

        private static bool HasClearSight(Vector3 origin, Vector3 destination)
        {
            return !Physics.Linecast(origin, destination, HelicopterSightMask,
                QueryTriggerInteraction.Ignore);
        }

        private bool IsPlayerInvisible(BasePlayer player)
        {
            if (player == null)
                return false;

            object result = SmartRecon?.Call("IsInvisible", player);
            if (result is bool && (bool)result)
                return true;

            return player.isInvisible || player._limitedNetworking;
        }

        private Vector3 GetHelicopterDestination(RaidIncident incident)
        {
            return incident.Center + Vector3.up * _config.Helicopter.PatrolHeight;
        }

        private static void ClearHelicopterTargets(PatrolHelicopterAI ai)
        {
            if (ai == null)
                return;

            ai._targetList.Clear();
            ai.leftGun?.ClearTarget();
            ai.rightGun?.ClearTarget();
        }

        private static bool HasTarget(PatrolHelicopterAI ai, BasePlayer player)
        {
            if (ai == null || player == null)
                return false;

            for (int i = 0; i < ai._targetList.Count; i++)
            {
                PatrolHelicopterAI.targetinfo target = ai._targetList[i];
                if (target != null && target.ply == player)
                    return true;
            }

            return false;
        }

        #endregion

        #region Markers and announcements

        private void CreateOrUpdateMarkers(RaidIncident incident)
        {
            DestroyMarker(ref incident.LabelMarker);
            DestroyMarker(ref incident.RadiusMarker);

            if (!_config.MapMarker.Enabled)
                return;

            ResponseProfile profile = incident.GetCurrentProfile(_config);
            string levelName = profile?.Name ?? "Raid Zone";

            if (_config.MapMarker.ShowLabel)
            {
                VendingMachineMapMarker label = GameManager.server.CreateEntity(
                    LabelMarkerPrefab, incident.Center) as VendingMachineMapMarker;
                if (label != null)
                {
                    label.enableSaving = false;
                    label.markerShopName = FormatMarkerLabel(incident.ResponseLevel, levelName);
                    label.Spawn();
                    incident.LabelMarker = label;
                }
            }

            if (_config.MapMarker.ShowDangerRadius)
            {
                MapMarkerGenericRadius radius = GameManager.server.CreateEntity(
                    RadiusMarkerPrefab, incident.Center) as MapMarkerGenericRadius;
                if (radius != null)
                {
                    MarkerColor color = _config.MapMarker.GetColor(incident.ResponseLevel);
                    radius.enableSaving = false;
                    radius.alpha = Mathf.Clamp01(color.Alpha);
                    radius.radius = Mathf.Clamp(
                        _config.Targeting.DangerZoneRadius / MarkerRadiusScale, 0.05f, 2f);
                    radius.color1 = new Color(Mathf.Clamp01(color.Red),
                        Mathf.Clamp01(color.Green), Mathf.Clamp01(color.Blue));
                    radius.Spawn();
                    radius.SendUpdate();
                    incident.RadiusMarker = radius;
                }
            }
        }

        private void BroadcastRaidAlert(RaidIncident incident, string key,
            params object[] arguments)
        {
            if (!_config.Announcements.Enabled)
                return;

            if (key == "InitialAlert" && !_config.Announcements.BroadcastInitialResponse)
                return;
            if ((key == "EscalationAlert" || key == "EscalationDeployed"
                    || key == "ResponseResumed")
                && !_config.Announcements.BroadcastEscalations)
                return;
            if ((key == "AllClear" || key == "FinalDefeated")
                && !_config.Announcements.BroadcastAllClear)
                return;

            string grid = MapHelper.GridToString(MapHelper.PositionToGrid(incident.Center));
            var fullArguments = new List<object> { grid };
            if (arguments != null)
                fullArguments.AddRange(arguments);

            string message = string.Format(lang.GetMessage(key, this), fullArguments.ToArray());
            PrintToChat(message);
        }

        private static void DestroyMarker<T>(ref T marker) where T : BaseNetworkable
        {
            if (marker != null && !marker.IsDestroyed)
                marker.Kill();
            marker = null;
        }

        private string FormatMarkerLabel(int responseLevel, string responseName)
        {
            try
            {
                return string.Format(_config.MapMarker.LabelFormat,
                    responseLevel, responseName);
            }
            catch (FormatException)
            {
                return "Anti-Raid Heli - Level " + responseLevel + ": " + responseName;
            }
        }

        #endregion

        #region Commands

        [ChatCommand("antiraidhelistop")]
        private void CommandStop(BasePlayer player, string command, string[] args)
        {
            if (!HasAdminAccess(player))
            {
                Reply(player, "NoPermission");
                return;
            }

            int count = StopAllIncidents(true);
            Reply(player, "Stopped", count);
        }

        [ChatCommand("antiraidhelistart")]
        private void CommandStart(BasePlayer player, string command, string[] args)
        {
            if (!HasAdminAccess(player))
            {
                Reply(player, "NoPermission");
                return;
            }

            int level = 1;
            if (args != null && args.Length > 0)
                int.TryParse(args[0], out level);
            level = Mathf.Clamp(level, 1, _config.ResponseProfiles.Count);

            StartManualIncident(player.transform.position, level, player.userID);
            Reply(player, "Started", level,
                MapHelper.GridToString(MapHelper.PositionToGrid(player.transform.position)));
        }

        [ChatCommand("antiraidhelitest")]
        private void CommandTest(BasePlayer player, string command, string[] args)
        {
            if (!HasAdminAccess(player))
            {
                Reply(player, "NoPermission");
                return;
            }

            RaycastHit hit;
            if (!Physics.Raycast(player.eyes.HeadRay(), out hit, 200f,
                Layers.Mask.Construction | Layers.Mask.Deployed,
                QueryTriggerInteraction.Ignore))
            {
                Reply(player, "TestTargetRequired");
                return;
            }

            BaseCombatEntity target = hit.GetEntity() as BaseCombatEntity;
            uint buildingId = GetBuildingId(target);
            if (target == null || buildingId == 0)
            {
                Reply(player, "TestTargetRequired");
                return;
            }

            int level = 2;
            if (args != null && args.Length > 0)
                int.TryParse(args[0], out level);
            level = Mathf.Clamp(level, 1, _config.ResponseProfiles.Count);

            RaidIncident incident = StartManualIncident(target.transform.position,
                level, player.userID);
            incident.IsAdminTest = true;
            incident.ProtectedBuildingIds.Add(buildingId);
            Reply(player, "TestStarted", level,
                MapHelper.GridToString(MapHelper.PositionToGrid(target.transform.position)));
        }

        private RaidIncident StartManualIncident(Vector3 center, int level,
            ulong aggressorId = 0)
        {
            level = Mathf.Clamp(level, 1, _config.ResponseProfiles.Count);

            RaidIncident incident = new RaidIncident
            {
                Center = center,
                CreatedAt = Time.realtimeSinceStartup,
                LastRaidDamageAt = Time.realtimeSinceStartup,
                LastRaidDamageUtc = UtcNowSeconds(),
                QualificationWindowStartedAt = Time.realtimeSinceStartup,
                Qualified = true,
                ResponseLevel = level
            };
            if (aggressorId != 0)
            {
                incident.RecordAggressor(aggressorId, Time.realtimeSinceStartup);
                incident.AggressorOwnerIds.Add(aggressorId);
            }
            _incidents.Add(incident);
            CreateOrUpdateMarkers(incident);
            BroadcastRaidAlert(incident, "InitialAlert",
                incident.GetCurrentProfile(_config)?.Name);
            SpawnHelicopter(incident);
            return incident;
        }

        [ConsoleCommand("antiraidheli.stop")]
        private void ConsoleStop(ConsoleSystem.Arg arg)
        {
            BasePlayer player = arg.Player();
            if (player != null && !HasAdminAccess(player))
            {
                arg.ReplyWith(lang.GetMessage("NoPermission", this, player.UserIDString));
                return;
            }

            int count = StopAllIncidents(true);
            arg.ReplyWith(string.Format(lang.GetMessage("Stopped", this,
                player?.UserIDString), count));
        }

        [ConsoleCommand("antiraidheli.start")]
        private void ConsoleStart(ConsoleSystem.Arg arg)
        {
            BasePlayer player = arg.Player();
            if (player != null && !HasAdminAccess(player))
            {
                arg.ReplyWith(lang.GetMessage("NoPermission", this, player.UserIDString));
                return;
            }

            int level;
            if (!int.TryParse(arg.GetString(0, "1"), out level))
                level = 1;
            level = Mathf.Clamp(level, 1, _config.ResponseProfiles.Count);

            Vector3 center;
            ulong aggressorId = 0;
            if (player != null)
            {
                center = player.transform.position;
                aggressorId = player.userID;
            }
            else
            {
                BasePlayer onlinePlayer = null;
                foreach (BasePlayer activePlayer in BasePlayer.activePlayerList)
                {
                    onlinePlayer = activePlayer;
                    break;
                }
                if (onlinePlayer != null)
                {
                    center = onlinePlayer.transform.position;
                    aggressorId = onlinePlayer.userID;
                }
                else
                {
                    center = Vector3.zero;
                    if (TerrainMeta.HeightMap != null)
                        center.y = TerrainMeta.HeightMap.GetHeight(center);
                }
            }

            RaidIncident incident = StartManualIncident(center, level, aggressorId);
            arg.ReplyWith(string.Format(lang.GetMessage("Started", this,
                player?.UserIDString), level,
                MapHelper.GridToString(MapHelper.PositionToGrid(incident.Center))));
        }

        private int StopAllIncidents(bool announce)
        {
            int count = _incidents.Count;
            RaidIncident[] incidents = _incidents.ToArray();
            foreach (RaidIncident incident in incidents)
                EndIncident(incident, announce, true);
            return count;
        }

        private bool HasAdminAccess(BasePlayer player)
        {
            return player != null && (player.IsAdmin
                || permission.UserHasPermission(player.UserIDString, AdminPermission));
        }

        #endregion

        #region Helpers and cleanup

        private void EndIncident(RaidIncident incident, bool announce, bool killHelicopter)
        {
            if (incident == null || !_incidents.Remove(incident))
                return;

            incident.SpawnTimer?.Destroy();
            incident.SpawnTimer = null;
            DestroyMarker(ref incident.LabelMarker);
            DestroyMarker(ref incident.RadiusMarker);
            RefreshHostilityUiAfterIncidentRemoval(incident);

            PatrolHelicopter helicopter = incident.Helicopter;
            incident.Helicopter = null;
            if (helicopter != null && !helicopter.IsDestroyed)
            {
                incident.SuppressReplacement = true;
                ClearHelicopterTargets(helicopter.myAI);
                if (killHelicopter)
                    helicopter.Kill();
                else if (helicopter.myAI != null)
                {
                    _retiringHelicopters.Add(helicopter);
                    helicopter.myAI.Retire();
                }
            }

            if (announce)
                BroadcastRaidAlert(incident, "AllClear");
            if (!_unloading)
                SaveRaidProgress();
        }

        private void RefreshHostilityUiAfterIncidentRemoval(RaidIncident removed)
        {
            foreach (KeyValuePair<ulong, HostilityRecord> pair
                in removed.GetHostilityRecords())
            {
                BasePlayer player = BasePlayer.FindByID(pair.Key);
                if (player == null || !player.IsConnected)
                    continue;

                float remaining = GetHostilityRemaining(pair.Key);
                if (_config.HostilityUi.Enabled && remaining > 0f)
                    DrawHostilityUi(player, remaining, false);
                else
                    CuiHelper.DestroyUi(player, HostilityUiName);
            }
        }

        private RaidIncident FindIncident(Vector3 position, float radius)
        {
            float bestDistance = radius * radius;
            RaidIncident best = null;
            for (int i = 0; i < _incidents.Count; i++)
            {
                RaidIncident incident = _incidents[i];
                float distance = HorizontalDistanceSquared(incident.Center, position);
                if (distance > bestDistance)
                    continue;

                bestDistance = distance;
                best = incident;
            }
            return best;
        }

        private RaidIncident FindIncident(PatrolHelicopter helicopter)
        {
            if (helicopter == null)
                return null;

            for (int i = 0; i < _incidents.Count; i++)
            {
                if (ReferenceEquals(_incidents[i].Helicopter, helicopter))
                    return _incidents[i];
            }
            return null;
        }

        private int CountLiveIncidents()
        {
            int count = 0;
            for (int i = 0; i < _incidents.Count; i++)
            {
                if (_incidents[i].Qualified && !_incidents[i].PausedForInactivity
                    && !_incidents[i].ResponseCompleted)
                    count++;
            }
            return count;
        }

        private bool IsInsideDangerZone(RaidIncident incident, Vector3 position)
        {
            return HorizontalDistanceSquared(incident.Center, position)
                <= _config.Targeting.DangerZoneRadius * _config.Targeting.DangerZoneRadius;
        }

        private static float HorizontalDistance(Vector3 first, Vector3 second)
        {
            return Mathf.Sqrt(HorizontalDistanceSquared(first, second));
        }

        private static float HorizontalDistanceSquared(Vector3 first, Vector3 second)
        {
            float x = first.x - second.x;
            float z = first.z - second.z;
            return x * x + z * z;
        }

        private void Reply(BasePlayer player, string key, params object[] args)
        {
            player.ChatMessage(string.Format(lang.GetMessage(key, this,
                player.UserIDString), args));
        }

        private void RegisterMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["InitialAlert"] = "<color=#ffb347><b>Raid Alert:</b></color> Anti-Raid Helicopter en route to an active raid at <color=#ffd479>{0}</color>. Response: <color=#ffffff>{1}</color>. The danger zone is marked on the map.",
                ["EscalationAlert"] = "<color=#ff6b35><b>Raid Alert:</b></color> The helicopter at <color=#ffd479>{0}</color> was destroyed. <color=#ffffff>{1}</color> is standing by and will deploy the moment structural raiding resumes.",
                ["EscalationDeployed"] = "<color=#ff6b35><b>Raid Alert:</b></color> Structural raiding resumed at <color=#ffd479>{0}</color>. <color=#ffffff>{1}</color> has been deployed.",
                ["ResponseResumed"] = "<color=#ffb347><b>Raid Alert:</b></color> Raiding resumed at <color=#ffd479>{0}</color>. The paused <color=#ffffff>{1}</color> response is returning.",
                ["FinalDefeated"] = "<color=#8cff98><b>Raid Alert:</b></color> The final Anti-Raid Helicopter at <color=#ffd479>{0}</color> has been defeated.",
                ["AllClear"] = "<color=#8cff98><b>Raid Alert Cleared:</b></color> The Anti-Raid Heli danger zone at <color=#ffd479>{0}</color> is no longer active.",
                ["HostilityThreeMinutes"] = "<color=#ff6b35><b>Anti-Raid Heli:</b></color> You are marked hostile. Cease all hostile activity for <color=#ffffff>3 minutes</color> to surrender.",
                ["HostilityTwoMinutes"] = "<color=#ffb347><b>Anti-Raid Heli:</b></color> <color=#ffffff>2 minutes</color> until your hostility expires. Any hostile action resets the timer.",
                ["HostilityOneMinute"] = "<color=#ffd479><b>Anti-Raid Heli:</b></color> <color=#ffffff>1 minute</color> until your hostility expires. Any hostile action resets the timer.",
                ["HostilityExpired"] = "<color=#8cff98><b>Anti-Raid Heli:</b></color> You are no longer raid-hostile. Disarm, strip down, and get out of here before the heli changes its mind.",
                ["NoPermission"] = "You do not have permission to use this command.",
                ["Stopped"] = "Stopped and cleaned up {0} AntiRaidHeli event(s).",
                ["Started"] = "Started AntiRaidHeli response level {0} at {1}.",
                ["TestTargetRequired"] = "Look directly at a building belonging to the simulated victim base and try again.",
                ["TestStarted"] = "Started AntiRaidHeli protection test level {0} at {1}. The targeted building is protected; your other buildings are treated as aggressor structures."
            }, this);
        }

        #endregion

        #region Configuration

        private void ValidateConfiguration()
        {
            _config.RaidDetection ??= new RaidDetectionConfiguration();
            _config.RaidBaseIdentification ??= new RaidBaseIdentificationConfiguration();
            _config.Targeting ??= new TargetingConfiguration();
            _config.HostilityUi ??= new HostilityUiConfiguration();
            _config.AdaptivePressure ??= new AdaptivePressureConfiguration();
            _config.Helicopter ??= new HelicopterConfiguration();
            _config.MapMarker ??= new MapMarkerConfiguration();
            _config.Announcements ??= new AnnouncementConfiguration();
            _config.ResponseProfiles ??= PluginConfiguration.CreateDefaultProfiles();

            if (_config.ResponseProfiles.Count == 0)
                _config.ResponseProfiles = PluginConfiguration.CreateDefaultProfiles();

            _config.RaidDetection.MinimumQualifyingHits = Math.Max(1,
                _config.RaidDetection.MinimumQualifyingHits);
            _config.RaidDetection.MinimumAccumulatedDamage = Mathf.Max(1f,
                _config.RaidDetection.MinimumAccumulatedDamage);
            _config.RaidDetection.SingleHitDamageThreshold = Mathf.Max(1f,
                _config.RaidDetection.SingleHitDamageThreshold);
            _config.RaidDetection.QualificationWindowSeconds = Mathf.Max(1f,
                _config.RaidDetection.QualificationWindowSeconds);
            _config.RaidDetection.MergeRadius = Mathf.Max(25f,
                _config.RaidDetection.MergeRadius);
            _config.RaidDetection.InitialResponseDelaySeconds = Mathf.Max(0f,
                _config.RaidDetection.InitialResponseDelaySeconds);
            _config.RaidDetection.RaidInactivitySeconds = Mathf.Max(30f,
                _config.RaidDetection.RaidInactivitySeconds);
            _config.RaidDetection.MaximumConcurrentRaidZones = Math.Max(1,
                _config.RaidDetection.MaximumConcurrentRaidZones);
            _config.RaidBaseIdentification.ConstructionHistoryMinutes = Mathf.Max(60f,
                _config.RaidBaseIdentification.ConstructionHistoryMinutes);
            _config.RaidBaseIdentification.FastClassificationWindowMinutes = Mathf.Clamp(
                _config.RaidBaseIdentification.FastClassificationWindowMinutes, 1f,
                _config.RaidBaseIdentification.ConstructionHistoryMinutes);
            _config.RaidBaseIdentification.MaximumDistanceFromRaid = Mathf.Max(25f,
                _config.RaidBaseIdentification.MaximumDistanceFromRaid);
            _config.RaidBaseIdentification.MinimumRecentPieces = Math.Max(1,
                _config.RaidBaseIdentification.MinimumRecentPieces);
            _config.RaidBaseIdentification.AggressorUseProximity = Mathf.Max(3f,
                _config.RaidBaseIdentification.AggressorUseProximity);
            _config.RaidBaseIdentification.FastClassificationUseSeconds = Mathf.Max(1f,
                _config.RaidBaseIdentification.FastClassificationUseSeconds);
            _config.RaidBaseIdentification.OlderConstructionUseSeconds = Mathf.Max(
                _config.RaidBaseIdentification.FastClassificationUseSeconds,
                _config.RaidBaseIdentification.OlderConstructionUseSeconds);
            _config.Targeting.DangerZoneRadius = Mathf.Max(25f,
                _config.Targeting.DangerZoneRadius);
            _config.Targeting.HostilitySeconds = Mathf.Max(60f,
                _config.Targeting.HostilitySeconds);
            _config.Targeting.MinimumNativeThreatLevel = Mathf.Clamp01(
                _config.Targeting.MinimumNativeThreatLevel);
            _config.HostilityUi.ClearedDisplaySeconds = Mathf.Max(1f,
                _config.HostilityUi.ClearedDisplaySeconds);
            if (string.IsNullOrWhiteSpace(_config.HostilityUi.AnchorMin))
                _config.HostilityUi.AnchorMin = "0.87 0.35";
            if (string.IsNullOrWhiteSpace(_config.HostilityUi.AnchorMax))
                _config.HostilityUi.AnchorMax = "0.99 0.38";
            _config.HostilityUi.HostileColor = NormalizeCuiColor(
                _config.HostilityUi.HostileColor, "0.95 0 0.02 0.78");
            _config.HostilityUi.ClearedColor = NormalizeCuiColor(
                _config.HostilityUi.ClearedColor, "0.10 0.65 0.20 0.82");
            _config.AdaptivePressure.ActivityWindowSeconds = Mathf.Max(10f,
                _config.AdaptivePressure.ActivityWindowSeconds);
            _config.AdaptivePressure.StructurePressureSeconds = Mathf.Max(5f,
                _config.AdaptivePressure.StructurePressureSeconds);
            _config.AdaptivePressure.SustainedRaidHitThreshold = Math.Max(2,
                _config.AdaptivePressure.SustainedRaidHitThreshold);
            _config.AdaptivePressure.HeavyRaidHitThreshold = Math.Max(
                _config.AdaptivePressure.SustainedRaidHitThreshold + 1,
                _config.AdaptivePressure.HeavyRaidHitThreshold);
            _config.AdaptivePressure.SustainedRocketCountMultiplier = Mathf.Max(1f,
                _config.AdaptivePressure.SustainedRocketCountMultiplier);
            _config.AdaptivePressure.HeavyRocketCountMultiplier = Mathf.Max(
                _config.AdaptivePressure.SustainedRocketCountMultiplier,
                _config.AdaptivePressure.HeavyRocketCountMultiplier);
            _config.AdaptivePressure.SustainedCooldownMultiplier = Mathf.Clamp(
                _config.AdaptivePressure.SustainedCooldownMultiplier, 0.1f, 1f);
            _config.AdaptivePressure.HeavyCooldownMultiplier = Mathf.Clamp(
                _config.AdaptivePressure.HeavyCooldownMultiplier, 0.1f,
                _config.AdaptivePressure.SustainedCooldownMultiplier);
            _config.AdaptivePressure.SustainedNapalmBonusPercent = Mathf.Clamp(
                _config.AdaptivePressure.SustainedNapalmBonusPercent, 0f, 100f);
            _config.AdaptivePressure.HeavyNapalmBonusPercent = Mathf.Clamp(
                _config.AdaptivePressure.HeavyNapalmBonusPercent,
                _config.AdaptivePressure.SustainedNapalmBonusPercent, 100f);
            _config.RaidDetection.ResponseMemoryMinutes = Mathf.Max(0f,
                _config.RaidDetection.ResponseMemoryMinutes);
            _config.Helicopter.LandSpawnDistanceFromRaid = Mathf.Max(100f,
                _config.Helicopter.LandSpawnDistanceFromRaid);
            if (string.Equals(_config.Helicopter.SpawnMode, "NearestShoreline",
                    StringComparison.OrdinalIgnoreCase))
                _config.Helicopter.SpawnMode = "NearbyLand";
            if (!string.Equals(_config.Helicopter.SpawnMode, "NearbyLand",
                    StringComparison.OrdinalIgnoreCase)
                && !string.Equals(_config.Helicopter.SpawnMode, "OverRaidArea",
                    StringComparison.OrdinalIgnoreCase))
                _config.Helicopter.SpawnMode = "NearbyLand";
            else if (string.Equals(_config.Helicopter.SpawnMode, "NearbyLand",
                StringComparison.OrdinalIgnoreCase))
                _config.Helicopter.SpawnMode = "NearbyLand";
            else
                _config.Helicopter.SpawnMode = "OverRaidArea";
            _config.Helicopter.SpawnHeight = Mathf.Max(30f,
                _config.Helicopter.SpawnHeight);
            _config.Helicopter.PatrolHeight = Mathf.Max(30f,
                _config.Helicopter.PatrolHeight);
            _config.Helicopter.OrbitRadius = Mathf.Clamp(
                _config.Helicopter.OrbitRadius, 40f, 200f);
            _config.Helicopter.MaximumPatrolOvershoot = Mathf.Max(50f,
                _config.Helicopter.MaximumPatrolOvershoot);
            _config.Helicopter.MaximumLifetimeSeconds = Mathf.Max(0f,
                _config.Helicopter.MaximumLifetimeSeconds);
            if (string.IsNullOrWhiteSpace(_config.MapMarker.LabelFormat))
                _config.MapMarker.LabelFormat = "Anti-Raid Heli - Level {0}: {1}";
            ValidateMarkerColor(_config.MapMarker.Level1);
            ValidateMarkerColor(_config.MapMarker.Level2);
            ValidateMarkerColor(_config.MapMarker.Level3);
            ValidateMarkerColor(_config.MapMarker.Level4);

            _config.ResponseProfiles.RemoveAll(profile => profile == null);
            if (_config.ResponseProfiles.Count == 0)
                _config.ResponseProfiles = PluginConfiguration.CreateDefaultProfiles();
            _config.ResponseProfiles.Sort((left, right) => left.Level.CompareTo(right.Level));

            if (_config.ConfigurationVersion < 1)
            {
                ApplyRocketEscalationDefaults(_config.ResponseProfiles);
                _config.ConfigurationVersion = 1;
            }
            if (_config.ConfigurationVersion < 2)
            {
                ApplyAccuracyEscalationDefaults(_config.ResponseProfiles);
                _config.ConfigurationVersion = 2;
            }
            if (_config.ConfigurationVersion < 3)
            {
                ApplyShelterPressureDefaults(_config.ResponseProfiles);
                _config.ConfigurationVersion = 3;
            }
            if (_config.ConfigurationVersion < 4)
            {
                _config.Targeting.HostilitySeconds = 180f;
                _config.Targeting.AllowVanillaThreatTargeting = true;
                _config.Targeting.SendPrivateHostilityCountdown = true;
                _config.Helicopter.MaximumLifetimeSeconds = 0f;
                _config.RaidDetection.ResponseMemoryMinutes = 360f;
                _config.ConfigurationVersion = 4;
            }
            if (_config.ConfigurationVersion < 5)
                _config.ConfigurationVersion = 5;

            foreach (ResponseProfile profile in _config.ResponseProfiles)
                profile.Validate();
            for (int i = 0; i < _config.ResponseProfiles.Count; i++)
                _config.ResponseProfiles[i].Level = i + 1;
        }

        private static string NormalizeCuiColor(string value, string fallback)
        {
            string[] components = value?.Split(new[] { ' ' },
                StringSplitOptions.RemoveEmptyEntries);
            if (components == null || components.Length != 4)
                return fallback;

            float[] parsed = new float[4];
            for (int i = 0; i < components.Length; i++)
            {
                if (!float.TryParse(components[i], NumberStyles.Float,
                    CultureInfo.InvariantCulture, out parsed[i]))
                    return fallback;
                parsed[i] = Mathf.Clamp01(parsed[i]);
            }

            return string.Join(" ", Array.ConvertAll(parsed, component =>
                component.ToString("0.###", CultureInfo.InvariantCulture)));
        }

        private static void ValidateMarkerColor(MarkerColor color)
        {
            if (color == null)
                return;
            color.Red = Mathf.Clamp01(color.Red);
            color.Green = Mathf.Clamp01(color.Green);
            color.Blue = Mathf.Clamp01(color.Blue);
            color.Alpha = Mathf.Clamp01(color.Alpha);
        }

        private static void ApplyRocketEscalationDefaults(List<ResponseProfile> profiles)
        {
            foreach (ResponseProfile profile in profiles)
            {
                if (profile == null)
                    continue;

                switch (profile.Level)
                {
                    case 1:
                        profile.EnableRockets = true;
                        profile.MaximumRocketsPerAttack = 3;
                        profile.RocketAttackCooldownSeconds = 55f;
                        profile.EnableNapalm = true;
                        profile.NapalmChancePercent = 10f;
                        break;
                    case 2:
                        profile.EnableRockets = true;
                        profile.MaximumRocketsPerAttack = 6;
                        profile.RocketAttackCooldownSeconds = 45f;
                        profile.EnableNapalm = true;
                        profile.NapalmChancePercent = 20f;
                        break;
                    case 3:
                        profile.EnableRockets = true;
                        profile.MaximumRocketsPerAttack = 10;
                        profile.RocketAttackCooldownSeconds = 35f;
                        profile.EnableNapalm = true;
                        profile.NapalmChancePercent = 35f;
                        break;
                    default:
                        profile.EnableRockets = true;
                        profile.MaximumRocketsPerAttack = 16;
                        profile.RocketAttackCooldownSeconds = 25f;
                        profile.EnableNapalm = true;
                        profile.NapalmChancePercent = 50f;
                        break;
                }
            }
        }

        private static void ApplyAccuracyEscalationDefaults(
            List<ResponseProfile> profiles)
        {
            foreach (ResponseProfile profile in profiles)
            {
                if (profile == null)
                    continue;

                switch (profile.Level)
                {
                    case 1:
                        profile.BulletAccuracyPercent = 65f;
                        profile.BulletSpeed = 300;
                        break;
                    case 2:
                        profile.BulletAccuracyPercent = 75f;
                        profile.BulletSpeed = 350;
                        break;
                    case 3:
                        profile.BulletAccuracyPercent = 85f;
                        profile.BulletSpeed = 400;
                        break;
                    default:
                        profile.BulletAccuracyPercent = 92f;
                        profile.BulletSpeed = 450;
                        break;
                }
            }
        }

        private static void ApplyShelterPressureDefaults(
            List<ResponseProfile> profiles)
        {
            foreach (ResponseProfile profile in profiles)
            {
                if (profile == null)
                    continue;

                switch (profile.Level)
                {
                    case 1:
                        profile.RocketAttackCooldownSeconds = 30f;
                        break;
                    case 2:
                        profile.RocketAttackCooldownSeconds = 25f;
                        break;
                    case 3:
                        profile.RocketAttackCooldownSeconds = 20f;
                        break;
                    default:
                        profile.RocketAttackCooldownSeconds = 15f;
                        break;
                }
            }
        }

        private sealed class PluginConfiguration
        {
            [JsonProperty("Configuration version")]
            public int ConfigurationVersion = 5;

            [JsonProperty("Enabled")]
            public bool Enabled = true;

            [JsonProperty("Raid detection")]
            public RaidDetectionConfiguration RaidDetection = new RaidDetectionConfiguration();

            [JsonProperty("Raid base identification and structure protection")]
            public RaidBaseIdentificationConfiguration RaidBaseIdentification =
                new RaidBaseIdentificationConfiguration();

            [JsonProperty("Player targeting")]
            public TargetingConfiguration Targeting = new TargetingConfiguration();

            [JsonProperty("Raid hostility screen indicator")]
            public HostilityUiConfiguration HostilityUi =
                new HostilityUiConfiguration();

            [JsonProperty("Adaptive anti-raid pressure")]
            public AdaptivePressureConfiguration AdaptivePressure =
                new AdaptivePressureConfiguration();

            [JsonProperty("Helicopter patrol")]
            public HelicopterConfiguration Helicopter = new HelicopterConfiguration();

            [JsonProperty("Map marker")]
            public MapMarkerConfiguration MapMarker = new MapMarkerConfiguration();

            [JsonProperty("Announcements")]
            public AnnouncementConfiguration Announcements = new AnnouncementConfiguration();

            [JsonProperty("Escalating response profiles",
                ObjectCreationHandling = ObjectCreationHandling.Replace)]
            public List<ResponseProfile> ResponseProfiles = CreateDefaultProfiles();

            public static PluginConfiguration CreateDefault()
            {
                return new PluginConfiguration();
            }

            public static List<ResponseProfile> CreateDefaultProfiles()
            {
                return new List<ResponseProfile>
                {
                    new ResponseProfile
                    {
                        Level = 1,
                        Name = "Suppression",
                        Health = 100000f,
                        MainRotorHealth = 9000f,
                        TailRotorHealth = 5000f,
                        BulletDamage = 20f,
                        BulletSpeed = 300,
                        BulletAccuracyPercent = 65f,
                        MaximumTargetRange = 300f,
                        EnableRockets = true,
                        MaximumRocketsPerAttack = 3,
                        RocketAttackCooldownSeconds = 30f,
                        EnableNapalm = true,
                        NapalmChancePercent = 10f
                    },
                    new ResponseProfile
                    {
                        Level = 2,
                        Name = "Escalation",
                        Health = 250000f,
                        MainRotorHealth = 22500f,
                        TailRotorHealth = 12500f,
                        BulletDamage = 30f,
                        BulletSpeed = 350,
                        BulletAccuracyPercent = 75f,
                        MaximumTargetRange = 320f,
                        EnableRockets = true,
                        MaximumRocketsPerAttack = 6,
                        RocketAttackCooldownSeconds = 25f,
                        EnableNapalm = true,
                        NapalmChancePercent = 20f
                    },
                    new ResponseProfile
                    {
                        Level = 3,
                        Name = "Maximum Response",
                        Health = 500000f,
                        MainRotorHealth = 45000f,
                        TailRotorHealth = 25000f,
                        BulletDamage = 40f,
                        BulletSpeed = 400,
                        BulletAccuracyPercent = 85f,
                        MaximumTargetRange = 340f,
                        EnableRockets = true,
                        MaximumRocketsPerAttack = 10,
                        RocketAttackCooldownSeconds = 20f,
                        EnableNapalm = true,
                        NapalmChancePercent = 35f
                    },
                    new ResponseProfile
                    {
                        Level = 4,
                        Name = "Final Response",
                        Health = 1000000f,
                        MainRotorHealth = 90000f,
                        TailRotorHealth = 50000f,
                        BulletDamage = 50f,
                        BulletSpeed = 450,
                        BulletAccuracyPercent = 92f,
                        MaximumTargetRange = 350f,
                        GunFireRate = 0.1f,
                        BurstLengthSeconds = 4f,
                        SecondsBetweenBursts = 2f,
                        EnableRockets = true,
                        MaximumRocketsPerAttack = 16,
                        SecondsBetweenRockets = 0.2f,
                        RocketDamageScale = 1.25f,
                        RocketAttackCooldownSeconds = 15f,
                        EnableNapalm = true,
                        NapalmChancePercent = 50f
                    }
                };
            }
        }

        private sealed class RaidDetectionConfiguration
        {
            [JsonProperty("Minimum qualifying hits within the qualification window")]
            public int MinimumQualifyingHits = 2;

            [JsonProperty("Minimum accumulated structure damage within the qualification window")]
            public float MinimumAccumulatedDamage = 100f;

            [JsonProperty("Single structure hit damage that can satisfy the hit-count requirement")]
            public float SingleHitDamageThreshold = 100f;

            [JsonProperty("Qualification window seconds")]
            public float QualificationWindowSeconds = 20f;

            [JsonProperty("Merge nearby raid damage into the same event within meters")]
            public float MergeRadius = 125f;

            [JsonProperty("Initial helicopter response delay seconds")]
            public float InitialResponseDelaySeconds = 15f;

            [JsonProperty("End event after no qualifying structure raid damage for seconds")]
            public float RaidInactivitySeconds = 300f;

            [JsonProperty("Remember paused response progress minutes (0 = until wipe)")]
            public float ResponseMemoryMinutes = 360f;

            [JsonProperty("Maximum concurrent raid zones")]
            public int MaximumConcurrentRaidZones = 2;

            [JsonProperty("Ignore structure damage by players authorized on the Tool Cupboard")]
            public bool IgnoreToolCupboardAuthorizedDamage = true;

            [JsonProperty("Ignore structure damage between native Rust teammates")]
            public bool IgnoreNativeTeamDamage = true;

            [JsonProperty("Ignore structure damage between members of the same Clans plugin clan")]
            public bool IgnoreClanDamage = true;

            [JsonProperty("Ignore structure damage between Friends plugin friends")]
            public bool IgnoreFriendDamage = true;
        }

        private sealed class RaidBaseIdentificationConfiguration
        {
            [JsonProperty("Enabled")]
            public bool Enabled = true;

            [JsonProperty("Construction history minutes")]
            public float ConstructionHistoryMinutes = 360f;

            [JsonProperty("Fast classification window minutes")]
            public float FastClassificationWindowMinutes = 60f;

            [JsonProperty("Maximum distance from raided base meters")]
            public float MaximumDistanceFromRaid = 100f;

            [JsonProperty("Minimum recently placed building pieces")]
            public int MinimumRecentPieces = 3;

            [JsonProperty("Aggressor use proximity meters")]
            public float AggressorUseProximity = 20f;

            [JsonProperty("Fast classification required use seconds")]
            public float FastClassificationUseSeconds = 5f;

            [JsonProperty("Older construction required use seconds")]
            public float OlderConstructionUseSeconds = 20f;

            [JsonProperty("Persist construction history across reloads")]
            public bool PersistHistory = true;
        }

        private sealed class TargetingConfiguration
        {
            [JsonProperty("Danger zone radius meters")]
            public float DangerZoneRadius = 125f;

            [JsonProperty("Allow vanilla threat targeting of armed non-aggressors in the danger zone")]
            public bool AllowVanillaThreatTargeting = true;

            [JsonProperty("Require players to satisfy Rust's armed or threatening check")]
            public bool RequireArmedOrThreateningPlayer = true;

            [JsonProperty("Minimum native threat level")]
            public float MinimumNativeThreatLevel = 0.5f;

            [JsonProperty("Raid and combat hostility duration seconds")]
            public float HostilitySeconds = 180f;

            [JsonProperty("Send private hostility countdown messages")]
            public bool SendPrivateHostilityCountdown = true;
        }

        private sealed class AdaptivePressureConfiguration
        {
            [JsonProperty("Enabled")]
            public bool Enabled = true;

            [JsonProperty("Raid activity measurement window seconds")]
            public float ActivityWindowSeconds = 60f;

            [JsonProperty("Stop attacking hostile structures after no raid damage for seconds")]
            public float StructurePressureSeconds = 30f;

            [JsonProperty("Sustained raid hit threshold per activity window")]
            public int SustainedRaidHitThreshold = 4;

            [JsonProperty("Heavy raid hit threshold per activity window")]
            public int HeavyRaidHitThreshold = 8;

            [JsonProperty("Sustained raid rocket count multiplier")]
            public float SustainedRocketCountMultiplier = 1.5f;

            [JsonProperty("Heavy raid rocket count multiplier")]
            public float HeavyRocketCountMultiplier = 2f;

            [JsonProperty("Sustained raid rocket cooldown multiplier")]
            public float SustainedCooldownMultiplier = 0.7f;

            [JsonProperty("Heavy raid rocket cooldown multiplier")]
            public float HeavyCooldownMultiplier = 0.45f;

            [JsonProperty("Sustained raid napalm bonus percent")]
            public float SustainedNapalmBonusPercent = 10f;

            [JsonProperty("Heavy raid napalm bonus percent")]
            public float HeavyNapalmBonusPercent = 25f;
        }

        private sealed class HostilityUiConfiguration
        {
            [JsonProperty("Enabled")]
            public bool Enabled = true;

            [JsonProperty("Anchor minimum (positioned below NoEscape by default)")]
            public string AnchorMin = "0.87 0.35";

            [JsonProperty("Anchor maximum (positioned below NoEscape by default)")]
            public string AnchorMax = "0.99 0.38";

            [JsonProperty("Hostile panel color (RGBA)")]
            public string HostileColor = "0.95 0 0.02 0.78";

            [JsonProperty("Cleared panel color (RGBA)")]
            public string ClearedColor = "0.10 0.65 0.20 0.82";

            [JsonProperty("Seconds to show HOSTILITY CLEARED")]
            public float ClearedDisplaySeconds = 5f;
        }

        private sealed class HelicopterConfiguration
        {
            [JsonProperty("Spawn mode (NearbyLand or OverRaidArea)")]
            public string SpawnMode = "NearbyLand";

            [JsonProperty("Nearby-land spawn distance from raid meters")]
            public float LandSpawnDistanceFromRaid = 300f;

            [JsonProperty("Spawn height meters")]
            public float SpawnHeight = 90f;

            [JsonProperty("Patrol height above raid meters")]
            public float PatrolHeight = 75f;

            [JsonProperty("Orbit radius around raid meters")]
            public float OrbitRadius = 75f;

            [JsonProperty("Maximum patrol overshoot beyond danger zone meters")]
            public float MaximumPatrolOvershoot = 175f;

            [JsonProperty("Maximum lifetime for each helicopter seconds (0 = no limit)")]
            public float MaximumLifetimeSeconds;
        }

        private sealed class MapMarkerConfiguration
        {
            [JsonProperty("Enabled")]
            public bool Enabled = true;

            [JsonProperty("Show label marker")]
            public bool ShowLabel = true;

            [JsonProperty("Show danger zone radius")]
            public bool ShowDangerRadius = true;

            [JsonProperty("Label format ({0} = response level, {1} = response name)")]
            public string LabelFormat = "Anti-Raid Heli - Level {0}: {1}";

            [JsonProperty("Level 1 color")]
            public MarkerColor Level1 = new MarkerColor(1f, 0.8f, 0f, 0.35f);

            [JsonProperty("Level 2 color")]
            public MarkerColor Level2 = new MarkerColor(1f, 0.45f, 0f, 0.4f);

            [JsonProperty("Level 3 color")]
            public MarkerColor Level3 = new MarkerColor(1f, 0.05f, 0.05f, 0.45f);

            [JsonProperty("Level 4 color")]
            public MarkerColor Level4 = new MarkerColor(0.45f, 0f, 0f, 0.5f);

            public MarkerColor GetColor(int level)
            {
                if (level <= 1) return Level1 ?? new MarkerColor();
                if (level == 2) return Level2 ?? Level1 ?? new MarkerColor();
                if (level == 3) return Level3 ?? Level2 ?? new MarkerColor();
                return Level4 ?? Level3 ?? new MarkerColor();
            }
        }

        private sealed class MarkerColor
        {
            [JsonProperty("Red (0-1)")]
            public float Red = 1f;

            [JsonProperty("Green (0-1)")]
            public float Green = 0.8f;

            [JsonProperty("Blue (0-1)")]
            public float Blue;

            [JsonProperty("Alpha (0-1)")]
            public float Alpha = 0.35f;

            public MarkerColor() { }

            public MarkerColor(float red, float green, float blue, float alpha)
            {
                Red = red;
                Green = green;
                Blue = blue;
                Alpha = alpha;
            }
        }

        private sealed class AnnouncementConfiguration
        {
            [JsonProperty("Enabled")]
            public bool Enabled = true;

            [JsonProperty("Broadcast initial response")]
            public bool BroadcastInitialResponse = true;

            [JsonProperty("Broadcast escalation warnings")]
            public bool BroadcastEscalations = true;

            [JsonProperty("Broadcast when the event clears")]
            public bool BroadcastAllClear = true;
        }

        private sealed class ResponseProfile
        {
            [JsonProperty("Level")]
            public int Level = 1;

            [JsonProperty("Name")]
            public string Name = "Suppression";

            [JsonProperty("Health")]
            public float Health = 100000f;

            [JsonProperty("Main rotor health")]
            public float MainRotorHealth = 9000f;

            [JsonProperty("Tail rotor health")]
            public float TailRotorHealth = 5000f;

            [JsonProperty("Bullet damage")]
            public float BulletDamage = 20f;

            [JsonProperty("Bullet speed")]
            public int BulletSpeed = 250;

            [JsonProperty("Bullet accuracy percent")]
            public float BulletAccuracyPercent = 45f;

            [JsonProperty("Gun fire rate seconds")]
            public float GunFireRate = 0.125f;

            [JsonProperty("Burst length seconds")]
            public float BurstLengthSeconds = 3f;

            [JsonProperty("Seconds between bursts")]
            public float SecondsBetweenBursts = 3f;

            [JsonProperty("Maximum target range meters")]
            public float MaximumTargetRange = 300f;

            [JsonProperty("Maximum speed")]
            public float MaximumSpeed = 42f;

            [JsonProperty("Maximum rotation speed scale")]
            public float MaximumRotationSpeed = 1f;

            [JsonProperty("Enable rockets")]
            public bool EnableRockets;

            [JsonProperty("Maximum rockets per attack")]
            public int MaximumRocketsPerAttack = 6;

            [JsonProperty("Seconds between rockets")]
            public float SecondsBetweenRockets = 0.2f;

            [JsonProperty("Rocket damage scale")]
            public float RocketDamageScale = 1f;

            [JsonProperty("Minimum seconds between rocket attacks")]
            public float RocketAttackCooldownSeconds = 35f;

            [JsonProperty("Enable napalm")]
            public bool EnableNapalm;

            [JsonProperty("Napalm chance percent per rocket attack")]
            public float NapalmChancePercent;

            public void Validate()
            {
                Level = Math.Max(1, Level);
                Name = string.IsNullOrWhiteSpace(Name) ? "Response " + Level : Name;
                Health = Mathf.Max(1000f, Health);
                MainRotorHealth = Mathf.Max(1f, MainRotorHealth);
                TailRotorHealth = Mathf.Max(1f, TailRotorHealth);
                BulletDamage = Mathf.Max(0f, BulletDamage);
                BulletSpeed = Math.Max(1, BulletSpeed);
                BulletAccuracyPercent = Mathf.Clamp(BulletAccuracyPercent, 0f, 100f);
                GunFireRate = Mathf.Max(0.05f, GunFireRate);
                BurstLengthSeconds = Mathf.Max(0.1f, BurstLengthSeconds);
                SecondsBetweenBursts = Mathf.Max(0.1f, SecondsBetweenBursts);
                MaximumTargetRange = Mathf.Max(25f, MaximumTargetRange);
                MaximumSpeed = Mathf.Max(1f, MaximumSpeed);
                MaximumRotationSpeed = Mathf.Max(0.1f, MaximumRotationSpeed);
                MaximumRocketsPerAttack = Math.Max(0, MaximumRocketsPerAttack);
                SecondsBetweenRockets = Mathf.Max(0.1f, SecondsBetweenRockets);
                RocketDamageScale = Mathf.Max(0f, RocketDamageScale);
                RocketAttackCooldownSeconds = Mathf.Max(1f, RocketAttackCooldownSeconds);
                NapalmChancePercent = Mathf.Clamp(NapalmChancePercent, 0f, 100f);
            }
        }

        #endregion

        #region Runtime models

        private sealed class RaidIncident
        {
            public Vector3 Center;
            public float CreatedAt;
            public float LastRaidDamageAt;
            public double LastRaidDamageUtc;
            public float QualificationWindowStartedAt;
            public int QualifyingHits;
            public float QualifyingDamage;
            public bool Qualified;
            public bool IsAdminTest;
            public int ResponseLevel;
            public bool AwaitingRenewedRaidDamage;
            public bool PausedForInactivity;
            public bool ResponseCompleted;
            public PatrolHelicopter Helicopter;
            public float HelicopterSpawnedAt;
            public bool HelicopterReachedZone;
            public bool SuppressReplacement;
            public float SavedHelicopterHealth;
            public float SavedMainRotorHealth;
            public float SavedTailRotorHealth;
            public Vector3 ApprovedStructureStrafePosition;
            public float ApprovedStructureStrafeUntil;
            public Timer SpawnTimer;
            public VendingMachineMapMarker LabelMarker;
            public MapMarkerGenericRadius RadiusMarker;
            public readonly HashSet<ulong> VictimOwnerIds = new HashSet<ulong>();
            public readonly HashSet<ulong> AggressorOwnerIds = new HashSet<ulong>();
            public readonly HashSet<uint> ProtectedBuildingIds = new HashSet<uint>();
            public readonly HashSet<uint> HostileBuildingIds = new HashSet<uint>();
            public readonly Dictionary<uint, float> CandidateUseStartedAt =
                new Dictionary<uint, float>();
            private readonly Dictionary<ulong, HostilityRecord> _aggressors =
                new Dictionary<ulong, HostilityRecord>();
            private readonly List<float> _recentRaidDamage = new List<float>();

            public ResponseProfile GetCurrentProfile(PluginConfiguration config)
            {
                int index = ResponseLevel - 1;
                return config?.ResponseProfiles != null && index >= 0
                    && index < config.ResponseProfiles.Count
                    ? config.ResponseProfiles[index]
                    : null;
            }

            public void RecordAggressor(ulong userId, float time)
            {
                if (!userId.IsSteamId())
                    return;

                HostilityRecord record;
                if (!_aggressors.TryGetValue(userId, out record))
                {
                    record = new HostilityRecord { LastNoticeStage = 4 };
                    _aggressors[userId] = record;
                }
                else if (record.LastNoticeStage < 3)
                {
                    record.LastNoticeStage = 4;
                }

                record.LastHostileAt = time;
            }

            public bool IsRecentAggressor(ulong userId, float oldestAllowedTime)
            {
                HostilityRecord record;
                return _aggressors.TryGetValue(userId, out record)
                    && record.LastHostileAt >= oldestAllowedTime;
            }

            public IEnumerable<ulong> GetAggressorIds()
            {
                return _aggressors.Keys;
            }

            public IEnumerable<KeyValuePair<ulong, HostilityRecord>> GetHostilityRecords()
            {
                return _aggressors;
            }

            public bool TryGetHostilityRecord(ulong userId,
                out HostilityRecord record)
            {
                return _aggressors.TryGetValue(userId, out record);
            }

            public void RemoveHostilityRecord(ulong userId)
            {
                _aggressors.Remove(userId);
            }

            public void RecordRaidDamage(float time, float windowSeconds)
            {
                _recentRaidDamage.Add(time);
                PruneRaidDamage(time - windowSeconds);
            }

            public int CountRecentRaidHits(float time, float windowSeconds)
            {
                PruneRaidDamage(time - windowSeconds);
                return _recentRaidDamage.Count;
            }

            private void PruneRaidDamage(float oldestAllowedTime)
            {
                int removeCount = 0;
                while (removeCount < _recentRaidDamage.Count
                    && _recentRaidDamage[removeCount] < oldestAllowedTime)
                    removeCount++;
                if (removeCount > 0)
                    _recentRaidDamage.RemoveRange(0, removeCount);
            }
        }

        private sealed class HostilityRecord
        {
            public float LastHostileAt;
            public int LastNoticeStage;
        }

        private sealed class RaidProgressData
        {
            [JsonProperty("Remembered raid responses")]
            public List<RaidProgressRecord> Incidents =
                new List<RaidProgressRecord>();
        }

        private sealed class RaidProgressRecord
        {
            [JsonProperty("Raid center")]
            public Vector3 Center;

            [JsonProperty("Last raid damage UTC")]
            public double LastRaidDamageUtc;

            [JsonProperty("Response level")]
            public int ResponseLevel;

            [JsonProperty("Waiting for renewed raid damage")]
            public bool AwaitingRenewedRaidDamage;

            [JsonProperty("All response helicopters defeated")]
            public bool ResponseCompleted;

            [JsonProperty("Saved helicopter health")]
            public float SavedHelicopterHealth;

            [JsonProperty("Saved main rotor health")]
            public float SavedMainRotorHealth;

            [JsonProperty("Saved tail rotor health")]
            public float SavedTailRotorHealth;

            [JsonProperty("Victim owner IDs")]
            public List<ulong> VictimOwnerIds = new List<ulong>();

            [JsonProperty("Aggressor owner IDs")]
            public List<ulong> AggressorOwnerIds = new List<ulong>();

            [JsonProperty("Protected building IDs")]
            public List<uint> ProtectedBuildingIds = new List<uint>();

            [JsonProperty("Hostile building IDs")]
            public List<uint> HostileBuildingIds = new List<uint>();
        }

        private sealed class ConstructionHistoryData
        {
            [JsonProperty("Recent buildings")]
            public List<RecentConstructionRecord> Records =
                new List<RecentConstructionRecord>();
        }

        private sealed class RecentConstructionRecord
        {
            [JsonProperty("Building ID")]
            public uint BuildingId;

            [JsonProperty("Approximate center")]
            public Vector3 Position;

            [JsonProperty("Recently placed pieces")]
            public int PieceCount;

            [JsonProperty("Most recent construction UTC")]
            public double LastBuiltUtc;

            [JsonProperty("Builders")]
            public List<ulong> OwnerIds = new List<ulong>();

            public void AddPiece(Vector3 position, ulong ownerId, double utcNow)
            {
                PieceCount++;
                Position += (position - Position) / Mathf.Max(1, PieceCount);
                LastBuiltUtc = utcNow;
                if (ownerId.IsSteamId() && !OwnerIds.Contains(ownerId))
                    OwnerIds.Add(ownerId);
            }
        }

        #endregion
    }
}
