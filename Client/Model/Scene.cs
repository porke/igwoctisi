namespace Client.Model
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Client.Renderer;
	using Microsoft.Xna.Framework;
	using System.Threading;

	public class Scene
	{		
		public int HoveredPlanet { get; set; }
		public PlanetLink HoveredLink { get; set; }
		public SceneVisual Visual { get; set; }

		public int SelectedPlanet { get; private set; }
		public Map Map { get; private set; }		

		private List<Player> _players;

		#region Events for SceneVisual

		/// <summary>
		/// Arguments: [{targetPlanet, newFleetsCount, onEndCallback}]
		/// </summary>
		internal event Action<List<Tuple<Planet, int, Action>>> AnimDeploys;

		/// <summary>
		/// Arguments: [{sourcePlanet, targetPlanet, numFleetsCount, onEndCallback}]
		/// </summary>
		internal event Action<List<Tuple<Planet, Planet, int, Action>>> AnimMoves;

		/// <summary>
		/// Arguments: [{simResult, onEndCallback}]
		/// </summary>
		internal event Action<List<Tuple<SimulationResult, Action>>> AnimAttacks;

		#endregion

		public Scene(Map map)
		{
			Map = map;
			HoveredPlanet = SelectedPlanet = 0;
			HoveredLink = null;
		}
		public Planet PickPlanet(Vector2 clickPosition, IRenderer renderer)
		{
			foreach (var item in Map.Planets)
			{
				if (renderer.RaySphereIntersection(clickPosition, new Vector3(item.X, item.Y, item.Z), item.Radius))
				{
					return item;
				}
			}

			return null;
		}
		public PlanetLink PickLink(Vector2 clickPosition, IRenderer renderer)
		{
			var selectedLinks = Map.Links.Where(x => SelectedPlanet == x.SourcePlanet || SelectedPlanet == x.TargetPlanet);
			foreach (var link in selectedLinks)
			{
				var sourcePlanet = Map.GetPlanetById(link.SourcePlanet);
				var targetPlanet = Map.GetPlanetById(link.TargetPlanet);
				var sourcePos = new Vector3(sourcePlanet.X, sourcePlanet.Y, sourcePlanet.Z);
				var targetPos = new Vector3(targetPlanet.X, targetPlanet.Y, targetPlanet.Z);

				if (renderer.RayLinkIntersection(clickPosition, sourcePos, targetPos))
				{
					return link;
				}
			}
			return null;
		}
		internal void AnimateChanges(IList<SimulationResult> simResults, Action endCallback)
		{
            CountdownEvent deployAnimsCounter = null;
            CountdownEvent moveAnimsCounter = null;
            CountdownEvent attackAnimsCounter = null;

			// Collect data
			var deploys = simResults
				.Where(sr => sr.Type == SimulationResult.MoveType.Deploy)
				.Select(sr =>
							{
								var targetPlanet = Map.GetPlanetById(sr.TargetId);
								return Tuple.Create<Planet, int, Action>(targetPlanet, sr.FleetCount,
									() => //action called when one deploy animation ends
										{
											targetPlanet.NumFleetsPresent += 1;
                                            deployAnimsCounter.Signal();
										});
							})
				.ToList();

			var moves = simResults
				.Where(sr => sr.Type == SimulationResult.MoveType.Move)
				.Select(sr =>
							{
								var sourcePlanet = Map.GetPlanetById(sr.SourceId);
								var targetPlanet = Map.GetPlanetById(sr.TargetId);
								return Tuple.Create<Planet, Planet, int, Action>(sourcePlanet, targetPlanet, sr.FleetCount,
									() => //action called when one move animation ends
										{
                                            moveAnimsCounter.Signal();
										});
							})
				.ToList();

			var attacks = simResults
				.Where(sr => sr.Type == SimulationResult.MoveType.Attack)
				.Select(sr =>
							{
								return Tuple.Create<SimulationResult, Action>(sr,
									() => //action called when one attack animation ends
										{
                                            attackAnimsCounter.Signal();
										});
							})
				.ToList();

			// Start playing animations and call callback when all of them are done.
			deployAnimsCounter = new CountdownEvent(deploys.Count);
			moveAnimsCounter = new CountdownEvent(moves.Count);
			attackAnimsCounter = new CountdownEvent(attacks.Count);

			AnimDeploys(deploys);

			ThreadPool.QueueUserWorkItem(new WaitCallback(obj =>
			{
				deployAnimsCounter.Wait();

                AnimMoves(moves);
                AnimAttacks(attacks);
				moveAnimsCounter.Wait();
				attackAnimsCounter.Wait();

				endCallback.Invoke();
			}));
		}

		public void Initialize(NewRoundInfo roundInfo, List<Player> players)
		{
			_players = players;

			// Assign planets to the players and vice versa.
			foreach (var data in Map.PlayerStartingData)
			{
				var planet = Map.GetPlanetById(data.PlanetId);
				var newPlanetState = roundInfo.FindPlanetState(planet.Id);
				string ownerName = null;

				// Planet may be not assigned to anyone.
				if (roundInfo.TryFindPlanetOwner(planet.Id, ref ownerName))
				{
					var player = _players.Find(p => p.Username.Equals(ownerName));
					player.Color = Map.GetColorById(data.ColorId);
					player.TryAssignPlanet(planet);
					planet.NumFleetsPresent = newPlanetState.Fleets;

					// Translate the color from hex to enum
					player.Color = Map.GetColorById(data.ColorId);
				}
			}
		}

		public bool CanSelectPlanet(Planet planet, Player clientPlayer)
		{
			if (planet.Owner == null) return false;
			if (!planet.Owner.Username.Equals(clientPlayer.Username)) return false;

			return true;
		}

		public void SelectPlanet(Planet planet)
		{
			SelectedPlanet = planet.Id;
		}
	}
}
