using WinChecker.Core.Models;

namespace WinChecker.Core.Services;

public interface IPeParser
{
    Task<PeMetadata> ParseMetadataAsync(string filePath);
}
