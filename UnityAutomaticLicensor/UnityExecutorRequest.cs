using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UnityAutomaticLicensor
{
    public class UnityExecutorRequest
    {
        public string UnityExecutablePath { get; set; }

        public List<string> ArgumentList { get; set; } = new List<string>();

        public Func<string, Task<UnityExecutorResponseResult?>> CustomBufferHandler { get; set; }
    }
}
