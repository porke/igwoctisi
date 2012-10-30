using System.Collections.Generic;


namespace Client.View
{
	public class AnimationManager
	{
		#region Protected members

		protected List<Animation> _animations;

		#endregion

		public AnimationManager()
		{
			_animations = new List<Animation>();
		}
		public void Update(double delta)
		{
			lock (_animations)
			{
				for (var i = 0; i < _animations.Count; )
				{
					var animation = _animations[i];
					if (animation.IsCompleted)
					{
						animation.End();
						_animations.RemoveAt(i);
					}
					else
					{
						animation.Update(delta);
						++i;
					}
				}
			}
		}
		public void AddAnimation(Animation animation)
		{
			lock (_animations)
			{
				animation.Begin();
				_animations.Add(animation);
			}
		}
	}
}
