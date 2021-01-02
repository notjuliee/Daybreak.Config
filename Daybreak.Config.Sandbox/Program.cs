using System;

namespace Daybreak.Config.Sandbox
{
    class BazConfig
    {
        [CfgField] public bool MakeBugs = false;
    }

    class TestConfig : BaseConfig
    {
        [CfgField] [Validation.LengthBetween(0, 32)]
        public string FooBar = "Default Foo";

        [CfgSection] public BazConfig BazConfig = new BazConfig();
    }

    class Program
    {
        static void Main(string[] args)
        {
            var cfg = new TestConfig();
            try
            {
                cfg.LoadFromFile("test.cfg");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Parse Error: {e}");
            }

            //Console.WriteLine($"FooBar = {cfg.FooBar}");
            //Console.WriteLine($"BazConfig.MakeBugs = {cfg.BazConfig.MakeBugs}");
            
            Console.WriteLine(cfg.DumpToString());
        }
    }
}