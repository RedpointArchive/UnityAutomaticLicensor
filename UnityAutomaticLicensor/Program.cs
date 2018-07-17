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

        [Option("--unity-version <version>", Description = "'v5.x' for 5.x series, 'lic' for 2017.0 and later")]
        public string UnityVersion { get; set; } = "v5.x";

        private async Task OnExecute()
        {
            var licensor = new UnityLicensor(new UnityLicensorRequest
            {
                Username = this.Username,
                Password = this.Password,
                UnityExecutablePath = this.UnityPath,
                UnityVersion = this.UnityVersion,
            });
            await licensor.Run();
        }
    }
}
