namespace Client.View.Play.Animations
{
	using Client.Common.AnimationSystem;
	using Client.Common.AnimationSystem.DefaultAnimations;
	using Client.Model;
	using Microsoft.Xna.Framework;

	public static class CameraMovementExtensions
	{
		public static void Shake(this Animation<SimpleCamera> animation, double duration, float maxDistance)
		{
			//TODO implement camera shaking	
		}

		public static Animation<SimpleCamera> MoveToDeploy(this Animation<SimpleCamera> animation, Planet planet)
		{
			var camera = animation.Context;
			var startingPosition = camera.GetPosition();
			var targetPosition1 = new Vector3(planet.Visual.X, planet.Visual.Y, camera.Z);
			var targetPosition2 = new Vector3(planet.Visual.X, planet.Visual.Y, camera.Max.Z);
			var firstMoveDiff = targetPosition1 - startingPosition;

			const float CameraSpeedFactor = 1.0f;
			float duration1 = CameraSpeedFactor * (targetPosition1 - startingPosition).Length() / 1000.0f;
			float duration2 = CameraSpeedFactor * (targetPosition2 - targetPosition1).Length() / 1000.0f;

			// Don't make a move of camera when it's centered on the planet.
			// This cause that camera won't be zoomed in.
			if ((targetPosition1 - startingPosition).Length() > 10)
			{
				return animation
					// Firstly, move on XY axis
					.Interpolate<SimpleCamera>(duration1, Interpolators.Accelerate(),
					(sc, percent) =>
					{
						var newPos = Vector3.Lerp(startingPosition, targetPosition1, (float)percent);
						sc.Force = Vector3.Zero;
						sc.InstantMoveTo(newPos);
					})

					// Secondly, zoom in
					.Interpolate<SimpleCamera>(duration2, Interpolators.Decelerate(),
					(sc, percent) =>
					{
						var newPos = Vector3.Lerp(targetPosition1, targetPosition2, (float)percent);
						sc.Force = Vector3.Zero;
						sc.InstantMoveTo(newPos);
					});
			}

			return animation;
		}
	}
}
