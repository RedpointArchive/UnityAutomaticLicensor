using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UnityAutomaticLicensor
{
    public class UnityExecutorRequest
    {
        public List<string> ArgumentList { get; set; } = new List<string>();

        public Func<string, Task<UnityExecutorResponseResult?>> CustomBufferHandler { get; set; }
    }
}
