namespace Client.Common
{
    using System;
    using Microsoft.Xna.Framework.Content;

    public static class XnaExtensions
    {
        public static IAsyncResult BeginLoad<T>(this ContentManager contentMgr, string assetName, AsyncCallback asyncCallback, object asyncState)
        {
            var asyncResult = new AsyncResult<T>(asyncCallback, asyncState);
            asyncResult.BeginInvoke(() => contentMgr.Load<T>(assetName));
            return asyncResult;
        }

        public static T EndLoad<T>(this ContentManager contentMgr, IAsyncResult ar)
        {
            var asyncResult = (AsyncResult<T>) ar;
            return asyncResult.EndInvoke();
        }
    }
}
