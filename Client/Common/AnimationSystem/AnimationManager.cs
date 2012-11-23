using System.Collections.Generic;


namespace Client.Common.AnimationSystem
{
	public class AnimationManager
	{
		#region Protected members

		protected List<Animation> _animations;
		protected Queue<Animation> _animationsToAdd;

		#endregion

		public AnimationManager()
		{
			_animations = new List<Animation>();
			_animationsToAdd = new Queue<Animation>();
		}
		public void Update(double delta)
		{
			lock (_animations)
			{
				lock (_animationsToAdd)
				{
					foreach (var newAnim in _animationsToAdd)
					{
						_animations.Add(newAnim);
					}
					_animationsToAdd.Clear();
				}

				for (int i = 0; i < _animations.Count; )
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
			lock (_animationsToAdd)
			{
				animation.Begin();
				_animationsToAdd.Enqueue(animation);
			}
		}
	}
}
