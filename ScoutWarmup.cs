using System;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Listeners;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Timers;
using System.Text.RegularExpressions;

namespace ScoutWarmup
{
    public class ScoutWarmup : BasePlugin
    {
        public override string ModuleName => "Warmup Scout";
        public override string ModuleVersion => "v.1";
        public override string ModuleAuthor => "audio";
        public override string ModuleDescription => "Enable Scout, Zeus, and Bhop during warmup";

        private CCSGameRules? _gameRules = null;

        public bool IsWarmup
        {
            get
            {
                if (_gameRules is null)
                    InitializeGameRules();

                return _gameRules is not null && _gameRules.WarmupPeriod;
            }
        }

        private void InitializeGameRules() => _gameRules = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").FirstOrDefault()?.GameRules;

        public override void Load(bool hotReload)
        {
            InitializeGameRules();
            RegisterListener<OnMapStart>(OnMapStart);
        }

        private void OnMapStart(string mapName)
        {
            _gameRules = null;
            AddTimer(1.0F, InitializeGameRules);
        }

        [GameEventHandler]
        public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
        {
            if (IsWarmup)
            {
                foreach (var player in Utilities.GetPlayers())
                {
                    if (player == null || !player.IsValid || player.Connected != PlayerConnectedState.PlayerConnected)
                    {
                        // Skip invalid players
                        continue;
                    }

                    player.GiveNamedItem("weapon_ssg08");
                    player.GiveNamedItem("weapon_taser");
                }

                Server.ExecuteCommand("sv_autobunnyhopping true");
                Server.ExecuteCommand("sv_enablebunnyhopping true");

                WriteColor("[SCOUTWARMUP] - Warmup Started - Scout+Bhop Enabled", ConsoleColor.Green);
            }

            return HookResult.Continue;
        }

        [GameEventHandler]
        public HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
        {
            if (IsWarmup)
            {
                Server.ExecuteCommand("sv_autobunnyhopping false");
                Server.ExecuteCommand("sv_enablebunnyhopping false");

                WriteColor("[SCOUTWARMUP] - Warmup Ended - Scout+Bhop Disabled", ConsoleColor.Red);
            }

            return HookResult.Continue;
        }

        private static void WriteColor(string message, ConsoleColor color)
        {
            var pieces = Regex.Split(message, @"(\[[^\]]*\])");

            foreach (var piece in pieces)
            {
                if (piece.StartsWith("[") && piece.EndsWith("]"))
                {
                    Console.ForegroundColor = color;
                    Console.Write(piece.Substring(1, piece.Length - 2));
                    Console.ResetColor();
                }
                else
                {
                    Console.Write(piece);
                }
            }

            Console.WriteLine();
        }
    }
}
