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

        private async Task OnExecute()
        {
            var licensor = new UnityLicensor(new UnityLicensorRequest
            {
                Username = this.Username,
                Password = this.Password,
                UnityExecutablePath = this.UnityPath,
            });
            await licensor.Run();
        }
    }
}
