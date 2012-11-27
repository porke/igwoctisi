using System.Collections.Generic;
using System;
using System.Runtime.CompilerServices;


namespace Client.Common.AnimationSystem
{
	public abstract class Animation
	{
		public double Duration { get; protected set; }
		public double Elapsed { get; protected set; }
		public double Progress
		{
			get { return Interpolator(Elapsed / Duration); }
		}
		public bool IsCompleted
		{
			get { return Elapsed >= Duration; }
		}
		private Func<double, double> Interpolator { get; set; }

		public Animation(double duration, Func<double, double> interpolator)
		{
			Duration = duration;
			Interpolator = interpolator != null ? interpolator : Interpolators.Linear();
		}
		[MethodImpl(MethodImplOptions.Synchronized)]
		public virtual void Begin()
		{
			Elapsed = 0;
		}
		[MethodImpl(MethodImplOptions.Synchronized)]
		public virtual void Update(double delta)
		{
			Elapsed += delta;
		}
		[MethodImpl(MethodImplOptions.Synchronized)]
		public virtual void End()
		{
		}
	}

	public abstract class Animation<T> : Animation
	{
		#region Protected members

		protected List<Action<T>> _callbacks;

		#endregion

		#region Animation members

		[MethodImpl(MethodImplOptions.Synchronized)]
		public override void End()
		{
			base.End();

			_callbacks.ForEach(x => x(Context));
			_callbacks = null;
		}

		#endregion

		public T Context { get; protected set; }
		public AnimationManager AnimationMgr { get; protected set; }

		public Animation(T context, AnimationManager animationMgr, double duration, Func<double, double> interpolator = null)
			: base(duration, interpolator)
		{
			Context = context;
			AnimationMgr = animationMgr;
			_callbacks = new List<Action<T>>();
		}
		[MethodImpl(MethodImplOptions.Synchronized)]
		public void AddCallback(Action<T> callback)
		{
			if (_callbacks != null)
			{
				_callbacks.Add(callback);
			}
			else
			{
				callback.Invoke(Context);
			}
		}
		public Animation<S> AddAfter<S>(Animation<S> animation)
		{
			AddCallback(context => AnimationMgr.AddAnimation(animation));
			return animation;
		}
		public static Animation<T> Dummy(T context, AnimationManager animationManager)
		{
		    var animation = new DummyAnimation(context, animationManager);
            return animation;
		}

		private class DummyAnimation : Animation<T>
		{
			public DummyAnimation(T context, AnimationManager animationManager)
				: base(context, animationManager, 0, null)
			{
			}
		}
	}
}
