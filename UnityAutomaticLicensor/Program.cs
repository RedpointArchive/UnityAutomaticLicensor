using McMaster.Extensions.CommandLineUtils;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace UnityAutomaticLicensor
{
    public class Program
    {
        public static Task<int> Main(string[] args)
            => CommandLineApplication.ExecuteAsync<Program>(args);

        [Required]
        [Option("--username <username>", Description = "Unity account username")]
        public string Username { get; }

        [Required]
        [Option("--password <password>", Description = "Unity account password")]
        public string Password { get; }

        [Required]
        [Option("--unity-path <path-to-Unity.exe>", Description = "Path to Unity executable")]
        public string UnityPath { get; set; }

        [Option("--unity-license-path <path-to-directory-with-Unity.ulf>", Description =
            "Path to directory containing Unity license file")]
        public string UnityLicensePath { get; set; } = "C:/ProgramData/Unity";

        [Option("--unity-version <version>", Description = "Unity version number (e.g. '2018.3.4f1')")]
        public string UnityVersion { get; set; } = "5.4.1f1";

        [Option("--unity-changeset <changeset>", Description = "Unity version changeset")]
        public string UnityChangeset { get; set; } = "649f48bbbf0f";

        [Option("--nocheck", Description = "Indicates that unity should not be started again to verify the obtained license.")]
        public bool CheckSuccess { get; set; } = true;

        private async Task OnExecute()
        {
            var licensor = new UnityLicensor(new UnityLicensorRequest
            {
                Username = this.Username,
                Password = this.Password,
                UnityExecutablePath = this.UnityPath,
                UnityVersion = this.UnityVersion,
                UnityChangeset = this.UnityChangeset,
                UnityLicensePath =  this.UnityLicensePath,
                CheckSuccess = this.CheckSuccess
            });
            await licensor.Run();
        }
    }
}
