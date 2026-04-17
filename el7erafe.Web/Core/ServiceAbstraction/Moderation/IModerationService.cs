
using Shared.DataTransferObject.Moderation;

namespace ServiceAbstraction.Moderation
{
    public interface IModerationService
    {
        Task<ModerationResult> CheckMessageAsync(string content);
    }
}
