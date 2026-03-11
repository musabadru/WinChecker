namespace WinChecker.Core.Services;

public interface IDllResolver
{
    string? ResolveDllPath(string dllName, string appDirectory, Architecture appArchitecture);
}
