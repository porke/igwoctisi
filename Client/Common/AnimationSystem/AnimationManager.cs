using System.Collections.Generic;
using System.Collections.Concurrent;


namespace Client.Common.AnimationSystem
{
	public class AnimationManager
	{
		#region Protected members

		protected List<Animation> _animations;
		protected ConcurrentQueue<Animation> _animationsToAdd;

		#endregion

		public AnimationManager()
		{
			_animations = new List<Animation>();
			_animationsToAdd = new ConcurrentQueue<Animation>();
		}
		public void Update(double delta)
		{
			lock (_animations)
			{
				Animation newAnim = null;
				while (_animationsToAdd.TryDequeue(out newAnim))
				{
					newAnim.Begin();
					_animations.Add(newAnim);
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
			_animationsToAdd.Enqueue(animation);
		}
	}
}
