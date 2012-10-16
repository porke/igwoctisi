using System;
namespace Client
{
    static class Program
    {
        static void Main(string[] args)
        {
            try
            {
                using (IGWOCTISI game = new IGWOCTISI())
                {
                    game.Run();
                }
            }
            catch (Exception exc)
            {
                NLog.LogManager.GetCurrentClassLogger().Fatal(exc);
                throw;
            }
        }
    }
}

