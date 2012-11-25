namespace Client.Model
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Client.Renderer;
	using Microsoft.Xna.Framework;
	using System.Threading;
	using System.ComponentModel;

	public class Scene
	{		
		public int HoveredPlanet { get; set; }
		public PlanetLink HoveredLink { get; set; }
		public SceneVisual Visual { get; set; }

		public int SelectedPlanet { get; private set; }
		public Map Map { get; private set; }
		public Player ClientPlayer { get; private set; }

		private List<Player> _players;

		#region Events for SceneVisual

		/// <summary>
		/// Arguments: none
		/// </summary>
		internal event Action SaveCameraPosition;

		/// <summary>
		/// Arguments: [{targetPlanet, newFleetsCount, onEndCallback}]
		/// </summary>
		internal event Action<List<Tuple<Planet, int, Action>>> AnimDeploys;
		
		/// <summary>
		/// Arguments: [{simResult, onEndCallback}]
		/// </summary>
		internal event Action<List<Tuple<Planet, Planet, SimulationResult, Action<SimulationResult>>>> AnimMovesAndAttacks;

		/// <summary>
		/// Arguments: none
		/// </summary>
		internal event Action AnimCameraBack;

		#endregion

		public Scene(Map map)
		{
			Map = map;
			HoveredPlanet = SelectedPlanet = 0;
			HoveredLink = null;
		}
		public void Update(double delta, double time)
		{
			Map.Update(delta, time);
		}
		public Planet PickPlanet(Vector2 clickPosition, IRenderer renderer)
		{
			foreach (var item in Map.Planets)
			{
				if (renderer.RaySphereIntersection(Map.Camera, clickPosition, new Vector3(item.X, item.Y, item.Z), item.Radius))
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

				if (renderer.RayLinkIntersection(Map.Camera, clickPosition, sourcePlanet, targetPlanet))
				{
					return link;
				}
				else if (renderer.RayLinkIntersection(Map.Camera, clickPosition, targetPlanet, sourcePlanet))
				{
					return link.OppositeLink;
				}
			}
			return null;
		}
		public void AnimateChanges(IList<SimulationResult> simResults, Action endCallback)
		{
			CountdownEvent deployAnimsCounter = null;
			CountdownEvent moveAndAttackAnimsCounter = null;

			// Collect data
			var deploys = simResults
				.Where(sr => sr.Type == SimulationResult.MoveType.Deploy
					&& sr.ShouldPlayerSeeAnimation(this)
				)
				.Select(sr =>
				{
					var targetPlanet = Map.GetPlanetById(sr.TargetId);
					return Tuple.Create<Planet, int, Action>(targetPlanet, sr.FleetCount,
						() => //action called when one deploy animation ends
						{
							targetPlanet.NumFleetsPresent = sr.TargetLeft;
							deployAnimsCounter.Signal();
						});
				})
				.ToList();

			var movesAndAttacks = simResults
				.Where(sr => (sr.Type == SimulationResult.MoveType.Move || sr.Type == SimulationResult.MoveType.Attack)
					&& sr.ShouldPlayerSeeAnimation(this)
				)
				.Select(sr =>
				{
					var sourcePlanet = Map.GetPlanetById(sr.SourceId);
					var targetPlanet = Map.GetPlanetById(sr.TargetId);
					return Tuple.Create<Planet, Planet, SimulationResult, Action<SimulationResult>>(sourcePlanet, targetPlanet, sr,
						(srDone) => //action called when one move or attack animation ends
						{
							// TODO add animation changes
							sourcePlanet.NumFleetsPresent = sr.SourceLeft;
							targetPlanet.NumFleetsPresent = sr.TargetLeft;

							moveAndAttackAnimsCounter.Signal();
						});
				})
				.ToList();

			// Start playing animations and call callback when all of them are done.
			deployAnimsCounter = new CountdownEvent(deploys.Count);
			moveAndAttackAnimsCounter = new CountdownEvent(movesAndAttacks.Count);

			var bw = new BackgroundWorker();
			bw.DoWork += new DoWorkEventHandler((sender, workArgs) =>
			{
				SaveCameraPosition();

				// First deploy all fleets
				if (deploys.Count > 0)
				{
					AnimDeploys(deploys);
					deployAnimsCounter.Wait();
				}

				// Then show moves and attacks
				if (movesAndAttacks.Count > 0)
				{
					AnimMovesAndAttacks(movesAndAttacks);
					moveAndAttackAnimsCounter.Wait();
				}

				// Move camera to the old position
				AnimCameraBack();

				endCallback.Invoke();
			});

			// Don't try to animate if nothing happens
			if (deploys.Count + movesAndAttacks.Count > 0)
			{
				bw.RunWorkerAsync();
			}
		}
		public void Initialize(NewRoundInfo roundInfo, List<Player> players, Player clientPlayer)
		{
			_players = players;
			ClientPlayer = clientPlayer;

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
