namespace Client.View
{
	using System;

	public static class EventHandlerExtensions
	{
		public static EventArgs<T> CreateArgs<T>(this EventHandler<EventArgs<T>> handler, T arg)
		{
			return new EventArgs<T>(arg);
		}

		public static EventArgs<T, U> CreateArgs<T, U>(this EventHandler<EventArgs<T, U>> handler, T arg1, U arg2)
		{
			return new EventArgs<T, U>(arg1, arg2);
		}

		public static EventArgs<T, U, V> CreateArgs<T, U, V>(this EventHandler<EventArgs<T, U, V>> handler, T arg1, U arg2, V arg3)
		{
			return new EventArgs<T, U, V>(arg1, arg2, arg3);
		}
	}

	public class EventArgs<T> : EventArgs
	{
		public EventArgs(T arg)
		{
			_args = new Tuple<T>(arg);
		}
		
		public T Item
		{
			get
			{
				return _args.Item1;
			}
		}

		private Tuple<T> _args;	
	}

	public class EventArgs<T, U> : EventArgs
	{
		public EventArgs(T arg1, U arg2)
		{
			_args = new Tuple<T, U>(arg1, arg2);
		}
		
		public T Item1
		{
			get
			{
				return _args.Item1;
			}
		}
		public U Item2
		{
			get
			{
				return _args.Item2;
			}
		}

		private Tuple<T, U> _args;
	}

	public class EventArgs<T, U, V> : EventArgs
	{
		public EventArgs(T arg1, U arg2, V arg3)
		{
			_args = new Tuple<T, U, V>(arg1, arg2, arg3);
		}
		
		public T Item1
		{
			get
			{
				return _args.Item1;
			}
		}
		public U Item2
		{
			get
			{
				return _args.Item2;
			}
		}
		public V Item3
		{
			get
			{
				return _args.Item3;
			}
		}

		private Tuple<T, U, V> _args;
	}
}
