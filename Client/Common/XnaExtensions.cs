namespace Client.Common
{
    using System;
    using Microsoft.Xna.Framework.Content;
	using Microsoft.Xna.Framework;

    public static class XnaExtensions
    {
        public static IAsyncResult BeginLoad<T>(this ContentManager contentMgr, string assetName, AsyncCallback asyncCallback, object asyncState)
        {
            var asyncResult = new AsyncResult<T>(asyncCallback, asyncState);
            asyncResult.BeginInvoke(() => contentMgr.Load<T>(assetName));
            return asyncResult;
        }
		public static Vector3 ParseVector3(string s)
		{
			var components = s.Split(';');
			return new Vector3(
				float.Parse(components[0]),
				float.Parse(components[1]),
				float.Parse(components[2]));
		}

        public static T EndLoad<T>(this ContentManager contentMgr, IAsyncResult ar)
        {
            var asyncResult = (AsyncResult<T>) ar;
            return asyncResult.EndInvoke();
        }
    }
}
