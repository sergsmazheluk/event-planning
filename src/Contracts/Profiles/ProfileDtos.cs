namespace Contracts.Profiles
{
    public record ProfileDto(bool ProfileCompleted, string? ExtraProfileJson);
    public record CompleteProfileRequest(string ExtraProfileJson);
}
