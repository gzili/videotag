using System.Diagnostics;
using System.Text;

namespace VideoTag.Server.Helpers;

public static class ProcessAsyncHelper
{
    public static async Task RunProcessAsync(string command, string arguments)
    {
        using var process = new Process();
        process.StartInfo.FileName = command;
        process.StartInfo.Arguments = arguments;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.CreateNoWindow = true;

        process.Start();

        var stdOutTask = process.StandardOutput.ReadToEndAsync();
        var stdErrTask = process.StandardError.ReadToEndAsync();

        await Task.WhenAll(process.WaitForExitAsync(), stdOutTask, stdErrTask);

        if (process.ExitCode != 0)
        {
            throw new Exception($"Process finished with exit code {process.ExitCode}. StdOut: <{stdOutTask.Result}>. StdErr: <{stdErrTask.Result}>");
        }
    }
    
    public static async Task<string> RunProcessAndReadStringAsync(string command, string arguments)
    {
        using var process = new Process();
        process.StartInfo.FileName = command;
        process.StartInfo.Arguments = arguments;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.CreateNoWindow = true;

        process.Start();

        var stdOutTask = process.StandardOutput.ReadToEndAsync();
        var stdErrTask = process.StandardError.ReadToEndAsync();

        await Task.WhenAll(process.WaitForExitAsync(), stdOutTask, stdErrTask);

        if (process.ExitCode != 0)
        {
            throw new Exception($"Process finished with exit code {process.ExitCode}. StdOut: <{stdOutTask.Result}>. StdErr: <{stdErrTask.Result}>");
        }

        return stdOutTask.Result.Trim();
    }
    
    public static async Task<byte[]> RunProcessAndReadByteArrayAsync(string command, string arguments)
    {
        using var process = new Process();
        process.StartInfo.FileName = command;
        process.StartInfo.Arguments = arguments;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.CreateNoWindow = true;

        process.Start();
        
        using var memoryStream = new MemoryStream();
        var stdOutTask = process.StandardOutput.BaseStream.CopyToAsync(memoryStream);
        var stdErrTask = process.StandardError.ReadToEndAsync();

        await Task.WhenAll(process.WaitForExitAsync(), stdOutTask, stdErrTask);

        if (process.ExitCode != 0)
        {
            throw new Exception(
                $"Process finished with exit code {process.ExitCode}. " +
                $"StdOut: <{Encoding.UTF8.GetString(memoryStream.ToArray())}> " +
                $"StdErr: <{stdErrTask.Result}>");
        }

        return memoryStream.ToArray();
    }
}