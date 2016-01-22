using CommandLine;

namespace cic
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new Options();

            if (Parser.Default.ParseArguments(args, options))
            {
                Run(options);
            }
        }

        private static void Run(Options options)
        {
            var flow = new ProgramFlow(options);
            flow.Run();
        }
    }
}