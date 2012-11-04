namespace Client.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Client.Renderer;
    using Microsoft.Xna.Framework;

	public class Scene
	{
		public Map Map { get; private set; }
		public int HoveredPlanet { get; set; }
		public int SelectedPlanet { get; set; }
		public PlanetLink HoveredLink { get; set; }

		public SceneVisual Visual { get; set; }

		private List<Player> _players;

		#region Events for SceneVisual

        /// <summary>
        /// Arguments: targetPlanet, newFleetsCount, onEndCallback.
        /// </summary>
		internal event Action<Planet, int, Action> AnimDeploy;

		#endregion

		public Scene(Map map, List<Player> playerList)
		{
			Map = map;
			HoveredPlanet = SelectedPlanet = 0;
			HoveredLink = null;
			_players = playerList;

			// Assign planets
			for (int i = 0; i < _players.Count; i++)
			{
				var startingData = Map.PlayerStartingData[i];
				var startingPlanet = Map.StartingPositions[i];
				startingPlanet.Owner = _players[i];

				// Translate the color from hex to enum
				int color = (startingData.Color << 8) >> 8;
				_players[i].Color = (PlayerColor) Enum.Parse(typeof(PlayerColor), Convert.ToString(color));
				_players[i].AddPlanet(startingPlanet);

				_players[i].DeployableFleets = startingPlanet.BaseUnitsPerTurn;
			}
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
			foreach (var link in Map.Links.Where(x => SelectedPlanet == x.SourcePlanet || SelectedPlanet == x.TargetPlanet))
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
		internal void ImplementChange(SimulationResult simResult)
		{
			var sourcePlanet = Map.GetPlanetById(simResult.SourceId);
			var targetPlanet = Map.GetPlanetById(simResult.TargetId);

			if (simResult.Type == SimulationResult.MoveType.Attack)
			{
				sourcePlanet.NumFleetsPresent = simResult.SourceLeft;
				targetPlanet.NumFleetsPresent = simResult.TargetLeft;

				if (simResult.TargetOwnerChanged)
				{
					targetPlanet.Owner = _players.Find(player => player.Username.Equals(simResult.TargetOwner));
				}
			}
			else if (simResult.Type == SimulationResult.MoveType.Move)
			{
				sourcePlanet.NumFleetsPresent = simResult.SourceLeft;
				targetPlanet.NumFleetsPresent = simResult.TargetLeft;
			}
			else if (simResult.Type == SimulationResult.MoveType.Deploy)
			{
                int newFleetsCount = simResult.TargetLeft;
                AnimDeploy(targetPlanet, newFleetsCount, () =>
                {
                    targetPlanet.NumFleetsPresent = simResult.TargetLeft;
                });
			}
		}
	}
}
