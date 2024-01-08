using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using static CounterStrikeSharp.API.Core.Listeners;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Timers;
using System.ComponentModel;
using System.Drawing;

namespace ScoutWarmup
{

	public class ScoutWarmup : BasePlugin
	{
		public override string ModuleName => "Warmup Scout";
		public override string ModuleVersion => "v.1";
		public override string ModuleAuthor => "audio";
		public override string ModuleDescription => "Scout + Zeus + Bhop in warmup";

		CCSGameRules? _gameRules = null;   // ty Abner <3

		public bool WarmupPeriod
		{
			get
			{
				if (_gameRules is null)
					SetGameRules();

				return _gameRules is not null && _gameRules.WarmupPeriod;
			}
		}
		void SetGameRules() => _gameRules = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules!;

		public override void Load(bool hotReload)
		{
			OnMapStart(Server.MapName);
			RegisterListener<OnMapStart>(OnMapStart);
		}

		void OnMapStart(string _mapname)
        {
            _gameRules = null;
            AddTimer(1.0F, SetGameRules);
        }

		[GameEventHandler]
		public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
        {
			if (WarmupPeriod)
			{
				foreach (var player in Utilities.GetPlayers())
                {
                    if(player == null && !player.IsValid || player.Connected != PlayerConnectedState.PlayerConnected)
                    {
                        // Skip invalid players
                        return HookResult.Continue;
                    }
					player.GiveNamedItem("weapon_ssg08");
					player.GiveNamedItem("weapon_taser");
				}
				Server.ExecuteCommand("sv_autobunnyhopping true");
                Server.ExecuteCommand("sv_enablebunnyhopping true");
				WriteColor($"[SCOUTWARMUP] - Warmup Started - Scout+Bhop Enabled", ConsoleColor.Green);
			}
		}

		[GameEventHandler]
		public HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
		{  
			if (WarmupPeriod)
			{
				Server.ExecuteCommand("sv_autobunnyhopping false");
                Server.ExecuteCommand("sv_enablebunnyhopping false");
				WriteColor($"[SCOUTWARMUP] - Warmup Ended - Scout+Bhop Disabled", ConsoleColor.Red);
			}
		}

		static void WriteColor(string message, ConsoleColor color)
        {
            var pieces = Regex.Split(message, @"(\[[^\]]*\])");

            for (int i = 0; i < pieces.Length; i++)
            {
                string piece = pieces[i];

                if (piece.StartsWith("[") && piece.EndsWith("]"))
                {
                    Console.ForegroundColor = color;
                    piece = piece.Substring(1, piece.Length - 2);
                }

                Console.Write(piece);
                Console.ResetColor();
            }

            Console.WriteLine();
        }
	}
}
