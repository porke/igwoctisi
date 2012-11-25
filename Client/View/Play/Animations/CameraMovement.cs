namespace Client.View.Play.Animations
{
	using Client.Common.AnimationSystem;
	using Client.Common.AnimationSystem.DefaultAnimations;
	using Client.Model;
	using Microsoft.Xna.Framework;
	using Client.Renderer;

	public static class CameraMovementExtensions
	{
		public static void AnimateCameraBack(this SceneVisual scene, AnimationManager animationManager, SimpleCamera camera, Vector3 oldPosition)
		{
			const float duration = 0.5f;

			var startingPosition = camera.GetPosition();
			camera.Animate(animationManager)
				.Interpolate<SimpleCamera>(duration, Interpolators.Decelerate(),
				(sc, percent) =>
				{
					var newPos = Vector3.Lerp(startingPosition, oldPosition, (float)percent);
					sc.Force = Vector3.Zero;
					sc.InstantMoveTo(newPos);
				});
		}

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

			const float cameraSpeedFactor = 0.001f;
			float duration1 = cameraSpeedFactor * (targetPosition1 - startingPosition).Length();
			float duration2 = cameraSpeedFactor * (targetPosition2 - targetPosition1).Length();

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

		public static Animation<SimpleCamera> MoveToAttack(this Animation<SimpleCamera> animation, Planet sourcePlanet, Planet targetPlanet)
		{
			var camera = animation.Context;
			var p1 = sourcePlanet.Visual.GetPosition();
			var p2 = targetPlanet.Visual.GetPosition();
			float desiredFov = camera.FieldOfView + MathHelper.ToRadians(5);

			var p1ToP2 = Vector3.Normalize(p2 - p1);
			var p1ToCamRotation = Matrix.CreateRotationY(desiredFov);
			var p1ToCam = Vector3.Transform(p1ToP2, p1ToCamRotation);

			var p2ToP1 = -p1ToP2;
			var p2ToCamRotation = Matrix.Invert(p1ToCamRotation);
			var p2ToCam = Vector3.Transform(p2ToP1, p2ToCamRotation);

			float a1, a2, b1, b2;
			var p1TowardsCam = p1 + p1ToCam;
			var p2TowardsCam = p2 + p2ToCam;
			a1 = (p1TowardsCam.Z - p1.Z) / (p1TowardsCam.X - p1.X);
			b1 = (p1TowardsCam.Z - p1.Z / p1.X) / (1 - p1TowardsCam.X / p1.X);
			a2 = (p2TowardsCam.Z - p2.Z) / (p2TowardsCam.X - p2.X);
			b2 = (p2TowardsCam.Z - p2.Z / p2.X) / (1 - p2TowardsCam.X / p2.X);

			bool anyNaN = float.IsNaN(a1) || float.IsNaN(a2) || float.IsNaN(b1) || float.IsNaN(b2);
			var pCamOld = camera.GetPosition();
			var pCam = new Vector3(
				0.5f * (p1.X + p2.X),
				0.5f * (p1.Y + p2.Y), 
				anyNaN ? camera.Max.Z : MathHelper.Min(-(b1 - a1/a2*b2)/(1-a1/a2), camera.Max.Z)
			);

			animation.Interpolate<SimpleCamera>(0.5f, Interpolators.Decelerate(),
			(sc, percent) =>
			{
				var newPos = Vector3.Lerp(pCamOld, pCam, (float)percent);
				sc.Force = Vector3.Zero;
				sc.InstantMoveTo(newPos);
			});

			return animation;
		}
	}
}
