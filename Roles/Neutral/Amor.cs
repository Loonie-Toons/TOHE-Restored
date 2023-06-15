﻿using System.Collections.Generic;
using System.Linq;
using Hazel;
using UnityEngine;
using static TOHE.Options;

namespace TOHE.Roles.Neutral
{
    public static class Amor
    {
        private static readonly int Id = 7772269;
        private static List<byte> playerIdList = new();
        private static int MatchmakeLimit = new();
        public static List<byte> Lovers = new();

        private static OptionItem MatchmakeCooldown;
        private static OptionItem MatchmakeMax;
        public static OptionItem KnowTargetRole;
        public static OptionItem TargetKnowOtherTarget;

        public static void SetupCustomOption()
        {
            SetupSingleRoleOptions(Id, TabGroup.NeutralRoles, CustomRoles.Amor, 1, zeroOne: false);
            MatchmakeCooldown = FloatOptionItem.Create(Id + 10, "AmorMatchmakeCooldown", new(0f, 990f, 2.5f), 30f, TabGroup.NeutralRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Amor])
                .SetValueFormat(OptionFormat.Seconds);
            MatchmakeMax = IntegerOptionItem.Create(Id + 11, "VirusInfectMax", new(2, 4, 1), 2, TabGroup.NeutralRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Amor])
                .SetValueFormat(OptionFormat.Times);
            KnowTargetRole = BooleanOptionItem.Create(Id + 12, "VirusKnowTargetRole", true, TabGroup.NeutralRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Amor]);
            TargetKnowOtherTarget = BooleanOptionItem.Create(Id + 13, "VirusTargetKnowOtherTarget", true, TabGroup.NeutralRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Amor]);
        }

        public static void Init()
        {
            playerIdList = new();
            Lovers = new();
        }

        public static void Add(byte playerId)
        {
            playerIdList.Add(playerId);
            MatchmakeLimit = MatchmakeMax.GetInt();
        }

        public static void SetCooldown(byte id) => Main.AllPlayerKillCooldown[id] = MatchmakeCooldown.GetFloat();

        private static void SendRPC()
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetAmorMatchmakeLimit, SendOption.Reliable, -1);
            writer.Write(MatchmakeLimit);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
        public static void ReceiveRPC(MessageReader reader)
        {
            MatchmakeLimit = reader.ReadInt32();
        }

        public static void OnCheckMurder(PlayerControl target)
        {
            if (Lovers.Count == MatchmakeMax.GetInt())
            {
                // inform amor..
                return;
            }

            if (target.Is(CustomRoles.Lovers))
            {
                // inform amor..
                return;
            }

            Lovers.Add(target.PlayerId);

            var aliveLovers = Main.AllAlivePlayerControls.Where(a => Lovers.Contains(a.PlayerId)).ToList();

            MatchmakeLimit = MatchmakeMax.GetInt() - aliveLovers.Count;
            SendRPC();

            if (aliveLovers.Count == MatchmakeMax.GetInt())
            {
                foreach (var lover in aliveLovers)
                {
                    lover.RpcSetCustomRole(CustomRoles.Lovers);
                    
                    // inform lovers
                }

                Utils.NotifyRoles();
            }
        }

        public static bool KnowRole(PlayerControl player, PlayerControl target)
        {
            if (Lovers.Contains(player.PlayerId) && target.Is(CustomRoles.Amor)) return true;
            if (KnowTargetRole.GetBool() && player.Is(CustomRoles.Amor) && Lovers.Contains(target.PlayerId)) return true;
            if (TargetKnowOtherTarget.GetBool() && Lovers.Contains(player.PlayerId) && Lovers.Contains(target.PlayerId)) return true;
            return false;
        }

        public static string GetMatchmakeLimit() => Utils.ColorString(MatchmakeLimit >= 1 ? Utils.GetRoleColor(CustomRoles.Amor) : Color.gray, $"({MatchmakeLimit})");
    }
}
