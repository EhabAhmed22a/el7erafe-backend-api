using Microsoft.AspNetCore.Http;
using Shared.DataTransferObject.TechnicianIdentityDTOs;

namespace ServiceAbstraction
{
    public interface ITechnicianFileService
    {
        Task<TechRegisterToReturnDTO> ProcessTechnicianFilesAsync(TechRegisterDTO techRegisterDTO);
    }
}
