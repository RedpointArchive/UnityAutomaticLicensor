using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace UnityAutomaticLicensor
{
    public class UnityExecutor
    {
        public async Task<UnityExecutorResponse> ExecuteAsync(UnityExecutorRequest request)
        {
            var unityPath = @"C:\Program Files\Unity\Editor\Unity.exe";
            var logPath = Path.Combine(Path.GetTempPath(), "UnityLog-" + DateTimeOffset.UtcNow.ToUnixTimeSeconds() + ".log");

            Console.WriteLine("Executing Unity...");
            var processStartInfo = new ProcessStartInfo
            {
                FileName = unityPath,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            foreach (var arg in request.ArgumentList)
            {
                processStartInfo.ArgumentList.Add(arg);
            }
            processStartInfo.ArgumentList.Add("-logFile");
            processStartInfo.ArgumentList.Add(logPath);
            var process = Process.Start(processStartInfo);

            Console.WriteLine("Unity process has been launched...");
            var buffer = string.Empty;
            var offset = 0;
            UnityExecutorResponseResult? outcome = null;
            var running = true;
            var startTime = DateTimeOffset.UtcNow;
            DateTimeOffset? cleanupTime = null;
            StreamReader reader = null;
            try
            {
                while (running)
                {
                    if (!File.Exists(logPath))
                    {
                        if ((DateTimeOffset.UtcNow - startTime).TotalSeconds > 30)
                        {
                            Console.WriteLine("Unity didn't start in time... killing and retrying...");
                            await KillProcess(process.Id);
                            return new UnityExecutorResponse
                            {
                                Output = string.Empty,
                                Result = UnityExecutorResponseResult.Retry,
                            };
                        }
                        else
                        {
                            Console.WriteLine("Waiting for Unity to start...");
                            await Task.Delay(1000);
                            continue;
                        }
                    }
                    else
                    {
                        if (reader == null)
                        {
                            Console.WriteLine("Opening log file...");
                            reader = new StreamReader(new FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                        }

                        buffer += await reader.ReadToEndAsync();

                        if (buffer.Length < offset)
                        {
                            var monoResult = await HandleMonoIsStalled(cleanupTime, process, buffer);
                            if (monoResult != null)
                            {
                                return monoResult;
                            }
                        }

                        var newContent = buffer.Substring(offset);

                        if (newContent.Length == 0)
                        {
                            var monoResult = await HandleMonoIsStalled(cleanupTime, process, buffer);
                            if (monoResult != null)
                            {
                                return monoResult;
                            }
                        }

                        Console.Write(newContent);

                        if (cleanupTime.HasValue)
                        {
                            var monoResult = await HandleMonoIsStalled(cleanupTime, process, buffer);
                            if (monoResult != null)
                            {
                                return monoResult;
                            }
                        }
                        
                        if (newContent.Contains("Cleanup mono") || newContent.Contains("Failed to build player"))
                        {
                            cleanupTime = DateTimeOffset.UtcNow;
                        }
                        if (newContent.Contains("Failed to start Unity Package Manager: operation timed out"))
                        {
                            Console.WriteLine("Package manager timeout - Unity has stalled!");
                            await KillProcess(process.Id);
                            return new UnityExecutorResponse
                            {
                                Output = buffer,
                                Result = UnityExecutorResponseResult.Retry
                            };
                        }
                        if (newContent.Contains("Canceling DisplayDialog: Updating license failed Failed to update license within 60 seconds"))
                        {
                            Console.WriteLine("Licensing timeout - Unity has stalled!");
                            await KillProcess(process.Id);
                            return new UnityExecutorResponse
                            {
                                Output = buffer,
                                Result = UnityExecutorResponseResult.Retry
                            };
                        }
                        if (newContent.Contains("Exiting batchmode successfully"))
                        {
                            outcome = UnityExecutorResponseResult.Success;
                            running = false;
                            break;
                        }
                        if (newContent.Contains("cubemap not supported"))
                        {
                            // Intermittent failure? :/
                            outcome = UnityExecutorResponseResult.Retry;
                            running = false;
                            break;
                        }
                        if (newContent.Contains("Exiting batchmode") || newContent.Contains("Aborting batchmode"))
                        {
                            outcome = UnityExecutorResponseResult.Error;
                            running = false;
                            break;
                        }
                        if (request.CustomBufferHandler != null)
                        {
                            outcome = await request.CustomBufferHandler(buffer);
                            if (outcome != null)
                            {
                                running = false;
                                break;
                            }
                        }

                        offset += newContent.Length;
                        await Task.Delay(100);
                    }
                }

                await KillProcess(process.Id);

                return new UnityExecutorResponse
                {
                    Output = buffer,
                    Result = outcome ?? UnityExecutorResponseResult.Error
                };
            }
            finally
            {
                if (reader != null)
                {
                    reader.Dispose();
                }
            }
        }

        private async Task<UnityExecutorResponse> HandleMonoIsStalled(DateTimeOffset? cleanupTime, Process process, string buffer)
        {
            if (cleanupTime != null)
            {
                if ((DateTimeOffset.UtcNow - cleanupTime.Value).TotalSeconds > 20)
                {
                    Console.WriteLine("Mono cleanup took longer than 20 seconds - Unity is stalled!");
                    await KillProcess(process.Id);
                    return new UnityExecutorResponse
                    {
                        Output = buffer,
                        Result = UnityExecutorResponseResult.Retry
                    };
                }
            }

            return null;
        }

        private async Task KillProcess(int processId)
        {
            while (!(Process.GetProcessById(processId)?.HasExited ?? true))
            {
                try
                {
                    Console.WriteLine("Sending kill signal to Unity and waiting for it to exit...");
                    Process.GetProcessById(processId).Kill();
                }
                catch
                {
                }
                
                await Task.Delay(1000);
            }
        }
    }
}
