using System;
namespace Client
{
    static class Program
    {
        static void Main(string[] args)
        {
#if !DEBUG
            try
            {
#endif
                using (var game = new IGWOCTISI())
                {
                    game.Run();
                }
#if !DEBUG
            }
            catch (Exception exc)
            {
                NLog.LogManager.GetCurrentClassLogger().Fatal(exc);
                throw;
            }
#endif
        }
    }
}

