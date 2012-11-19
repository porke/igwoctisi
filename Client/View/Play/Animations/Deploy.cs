using System.Linq;

namespace Client.View.Play.Animations
{
    using System;
    using Client.Common.AnimationSystem;
    using Client.Model;
    using Client.Renderer;
    using System.Collections.Generic;
    using System.Threading;
	using Microsoft.Xna.Framework;

	using DeployFunction = System.Action<Client.Model.Planet, int, System.Action, Client.Model.Player,
		Client.Renderer.SceneVisual, Client.Common.AnimationSystem.AnimationManager, Client.Model.SimpleCamera>;

    public static class DeployAnimation
    {
        private static Random rand = new Random();

        public static void AnimateDeploys(this SceneVisual scene, AnimationManager animationManager, SimpleCamera camera,
            IList<Tuple<Planet, int, Action>> deploys)
        {
			ThreadPool.QueueUserWorkItem(obj =>
			{
				foreach (var deploy in deploys)
				{
					Planet targetPlanet = deploy.Item1;
					int newFleetsCount = deploy.Item2;
					Action onDeployEnd = deploy.Item3;
					Player player = targetPlanet.Owner;
					
					var deployFuncton = GetDeployFunction(newFleetsCount);
					deployFuncton.Invoke(targetPlanet, newFleetsCount, onDeployEnd, player, scene, animationManager, camera);
				}
			});
        }
		
		private static DeployFunction GetDeployFunction(int newFleetsCount)
		{
			int index = Array.BinarySearch(_deployFunctionsMinimums, newFleetsCount);
			if (index < 0) //the exact value wasn't found
			{
				index = ~index;
				index -= 1;
			}

			return _deployFunctions[index];
		}


		static DeployAnimation()
		{
			var deployFunctionsMinimums = new List<int>();
			var deployFunctions = new List<DeployFunction>();

			foreach (var pair in _deployFunctionsWithMinimums)
			{
				int minimumFleetsCount = pair.Key;
				DeployFunction function = pair.Value;

				deployFunctionsMinimums.Add(minimumFleetsCount);
				deployFunctions.Add(function);
			}

			_deployFunctionsMinimums = deployFunctionsMinimums.ToArray();
			_deployFunctions = deployFunctions.ToArray();

			Array.Sort(_deployFunctionsMinimums, _deployFunctions);
		}

		private static readonly int[] _deployFunctionsMinimums;
		private static readonly DeployFunction[] _deployFunctions;

		private static Dictionary<int, DeployFunction> _deployFunctionsWithMinimums = new Dictionary<int, DeployFunction>()
		{
			#region Deploy 1-5 fleets
			{1, (Planet targetPlanet, int newFleetsCount, Action onDeployEnd, Player player, SceneVisual scene, AnimationManager animationManager, SimpleCamera camera) =>
			{
				bool performShipRotate = rand.Next() % 3 == 0;

				// Camera quake 5 arena 2
				if (performShipRotate)
				camera.Animate(animationManager)
					.Wait(0.25);
					//.Shake(0.4);

				float deploySpeedFactor = camera.Z > 4500 ? 1.75f : 1.25f;
				float deployDuration = deploySpeedFactor * Math.Abs(camera.Min.Z / 2000);

				var ship = Spaceship.Acquire(SpaceshipModelType.LittleSpaceship, player.Color);
				scene.AddSpaceship(ship);

				ship.Animate(animationManager)
					.Compound(deployDuration, c =>
					{
						// Set start position to the camera position
						ship.SetPosition(camera.GetPosition());
						ship.LookAt(targetPlanet.Visual.GetPosition(), camera.GetUpVector());

						// Move spaceship to the target deploy planet
						c.MoveTo(targetPlanet.Position, deployDuration, Interpolators.Decelerate());

						// Rotating that happens randomly...
						if (performShipRotate)
							c.Rotate(ship.GetLook(), 0, 360, 1, Interpolators.OvershootInterpolator());

						// Fade in and fade out
						c.InterpolateTo(1, deployDuration / 5, Interpolators.OvershootInterpolator(),
							(s) => 0,
							(s, o) => { s.Opacity = (float)o; })
						.Wait(deployDuration / 5 * 2)
						.InterpolateTo(0, deployDuration / 5 * 2, Interpolators.Decelerate(1.4),
							(s) => 1,
							(s, o) => { s.Opacity = (float)o; });

					})
					.AddCallback(s =>
					{
						onDeployEnd.Invoke();
						Spaceship.Recycle(s);
					});
			}},
			#endregion

			#region Deploy 6-15 fleets
			{6, (Planet targetPlanet, int newFleetsCount, Action onDeployEnd, Player player, SceneVisual scene, AnimationManager animationManager, SimpleCamera camera) =>
			{
				_deployFunctionsWithMinimums[1].Invoke(targetPlanet, newFleetsCount, onDeployEnd, player, scene, animationManager, camera);
			}},
			#endregion

			#region Deploy 16-20 fleets
			{16, (Planet targetPlanet, int newFleetsCount, Action onDeployEnd, Player player, SceneVisual scene, AnimationManager animationManager, SimpleCamera camera) =>
			{
				_deployFunctionsWithMinimums[1].Invoke(targetPlanet, newFleetsCount, onDeployEnd, player, scene, animationManager, camera);
			}},
			#endregion

			#region Deploy 21-30 fleets
			{21, (Planet targetPlanet, int newFleetsCount, Action onDeployEnd, Player player, SceneVisual scene, AnimationManager animationManager, SimpleCamera camera) =>
			{
				_deployFunctionsWithMinimums[1].Invoke(targetPlanet, newFleetsCount, onDeployEnd, player, scene, animationManager, camera);
			}},
			#endregion

			#region Deploy 31-50 fleets
			{31, (Planet targetPlanet, int newFleetsCount, Action onDeployEnd, Player player, SceneVisual scene, AnimationManager animationManager, SimpleCamera camera) =>
			{
				_deployFunctionsWithMinimums[1].Invoke(targetPlanet, newFleetsCount, onDeployEnd, player, scene, animationManager, camera);
			}},
			#endregion

			#region Deploy 51-70 fleets
			{51, (Planet targetPlanet, int newFleetsCount, Action onDeployEnd, Player player, SceneVisual scene, AnimationManager animationManager, SimpleCamera camera) =>
			{
				_deployFunctionsWithMinimums[1].Invoke(targetPlanet, newFleetsCount, onDeployEnd, player, scene, animationManager, camera);
			}},
			#endregion

			#region Deploy 71-80 fleets
			{71, (Planet targetPlanet, int newFleetsCount, Action onDeployEnd, Player player, SceneVisual scene, AnimationManager animationManager, SimpleCamera camera) =>
			{
				_deployFunctionsWithMinimums[1].Invoke(targetPlanet, newFleetsCount, onDeployEnd, player, scene, animationManager, camera);
			}},
			#endregion

			#region Deploy 81-100 fleets
			{81, (Planet targetPlanet, int newFleetsCount, Action onDeployEnd, Player player, SceneVisual scene, AnimationManager animationManager, SimpleCamera camera) =>
			{
				_deployFunctionsWithMinimums[1].Invoke(targetPlanet, newFleetsCount, onDeployEnd, player, scene, animationManager, camera);
			}},
			#endregion

			#region Deploy 101-120 fleets
			{101, (Planet targetPlanet, int newFleetsCount, Action onDeployEnd, Player player, SceneVisual scene, AnimationManager animationManager, SimpleCamera camera) =>
			{
				_deployFunctionsWithMinimums[1].Invoke(targetPlanet, newFleetsCount, onDeployEnd, player, scene, animationManager, camera);
			}},
			#endregion

			#region Deploy 121->+inf fleets
			{121, (Planet targetPlanet, int newFleetsCount, Action onDeployEnd, Player player, SceneVisual scene, AnimationManager animationManager, SimpleCamera camera) =>
			{
				_deployFunctionsWithMinimums[1].Invoke(targetPlanet, newFleetsCount, onDeployEnd, player, scene, animationManager, camera);
			}},
			#endregion
		};
	}
}
