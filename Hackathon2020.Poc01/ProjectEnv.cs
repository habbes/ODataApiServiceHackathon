using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hackathon2020.Poc01
{
    public static class ProjectEnv
    {
        public static bool IsRemoveEnv()
        {
            return Environment.GetEnvironmentVariable("IS_REMOTE_ENV") == "true";
        }

        public static bool ShouldSeedData()
        {
            return Environment.GetEnvironmentVariable("SEED_DATA") == "true"
                || Environment.GetCommandLineArgs().Contains("--seed_data")
                || Environment.GetCommandLineArgs().Contains("-seed_data");
        }
    }
}
