namespace Client
{
    static class Program
    {
        static void Main(string[] args)
        {
            using (IGWOCTISI game = new IGWOCTISI())
            {
                game.Run();
            }
        }
    }
}

