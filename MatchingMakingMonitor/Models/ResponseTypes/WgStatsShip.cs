using Newtonsoft.Json;

namespace MatchMakingMonitor.Models.ResponseTypes
{
	public class WgStatsShip
	{
		[JsonProperty("distance")]
		public int Distance { get; set; }
		[JsonProperty("last_battle_time")]
		public int LastBattleTime { get; set; }
		[JsonProperty("account_id")]
		public long AccountId { get; set; }
		[JsonProperty("pvp")]
		public WgStatsPvp Pvp { get; set; }
		[JsonProperty("updated_at")]
		public int UpdatedAt { get; set; }
		[JsonProperty("battles")]
		public int Battles { get; set; }
		[JsonProperty("ship_id")]
		public long ShipId { get; set; }
		[JsonProperty("private")]
		public string Private { get; set; }
	}
}