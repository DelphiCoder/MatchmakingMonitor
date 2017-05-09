﻿using MatchMakingMonitor.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MatchMakingMonitor.Models.Replay;
using MatchMakingMonitor.Models.ResponseTypes;

namespace MatchMakingMonitor.Services
{
	public class ApiService
	{
		private readonly Settings _settings;
		private readonly ILogger _logger;

		private List<WgShip> _wgShips;
		public IReadOnlyList<WgShip> WgShips => _wgShips.AsReadOnly();

		private HttpClient _httpClient;
		public ApiService(ILogger logger, Settings settings)
		{
			_settings = settings;
			_logger = logger;
#pragma warning disable 4014
			Ships();
#pragma warning restore 4014
		}

		private string ApplicationId => _settings.Get<string>("appId" + _settings.Region);

		private static readonly Regex DateStart = new Regex("\"data\":{");
		private static readonly Regex ShipIdRegex = new Regex("\"(\\d){10}\":");
		private static readonly Regex DataEnd = new Regex("}}$");

		private async void Ships()
		{
			try
			{
				var baseUrl = _settings.Get<string>("baseUrl" + _settings.Region);
				var client = new HttpClient { BaseAddress = new Uri(baseUrl) };

				var result = await client.PostAsync($"/wows/encyclopedia/ships/?application_id={ApplicationId}&fields=name%2C+tier%2C+type%2C+ship_id", null);
				if (result.StatusCode != HttpStatusCode.OK) return;
				var shipsJson = await result.Content.ReadAsStringAsync();
				shipsJson = DateStart.Replace(shipsJson, "\"data\":[");
				shipsJson = ShipIdRegex.Replace(shipsJson, string.Empty);
				shipsJson = DataEnd.Replace(shipsJson, "]}");
				_wgShips = (await Task.Run(() => JsonConvert.DeserializeObject<WgShipResponse>(shipsJson)))?.Data;
			}
			catch (Exception e)
			{
				_logger.Error("Exception occurred while retrieving ships", e);
				_wgShips = new List<WgShip>();
			}
		}

		public async Task<IEnumerable<PlayerShip>> Players(Replay replay)
		{
			try
			{
				while (_wgShips == null)
				{
					await Task.Delay(1000);
				}
				var baseUrl = _settings.Get<string>("baseUrl" + _settings.Region);
				_httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };

				var tasks = replay.Vehicles.Select(GetAsync).ToList();
				var list = await Task.WhenAll(tasks);
				return list.Where(p => p != null);
			}
			catch (Exception e)
			{
				_logger.Error("Error occured while fetching players", e);
				return new List<PlayerShip>();
			}
		}

		[SuppressMessage("ReSharper", "InvertIf")]
		private async Task<PlayerShip> GetAsync(ReplayVehicle replayVehicle)
		{
			var wgShip = WgShips.SingleOrDefault(s => s.ShipId == replayVehicle.ShipId);

			var playerResponse = await _httpClient.GetAsync($"wows/account/list/?application_id={ApplicationId}&search={replayVehicle.Name}");
			if (playerResponse.StatusCode == HttpStatusCode.OK)
			{
				var playerJson = await playerResponse.Content.ReadAsStringAsync();
				var players = await Task.Run(() => JsonConvert.DeserializeObject<WgPlayerSearchResult>(playerJson));

				var player = players.Data.SingleOrDefault(p => p.Nickname == replayVehicle.Name);
				if (player != null)
				{
					var shipStatsResponse = await _httpClient.GetAsync($"wows/ships/stats/?application_id={ApplicationId}&account_id={player.AccountId}&ship_id={replayVehicle.ShipId}");
					if (shipStatsResponse.StatusCode == HttpStatusCode.OK)
					{
						var shipStatsJson = await shipStatsResponse.Content.ReadAsStringAsync();
						shipStatsJson = shipStatsJson.Replace("\"" + player.AccountId + "\":", "\"Ships\":");
						var shipStats = await Task.Run(() => JsonConvert.DeserializeObject<WgPlayerShipsStatsResult>(shipStatsJson));
						if (shipStats?.Data?.Ships != null && shipStats.Data.Ships.Any())
						{
							var ship = shipStats.Data.Ships.First();
							return new PlayerShip(ship, player, wgShip, replayVehicle.Relation);
						}
					}
				}
			}
			return new PlayerShip(wgShip)
			{
				Nickname = replayVehicle.Name,
				Relation = replayVehicle.Relation,
				IsPrivateOrHidden = true
			};
		}
	}
}
