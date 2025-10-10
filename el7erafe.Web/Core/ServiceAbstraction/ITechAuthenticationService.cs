using Shared.DataTransferObject.TechnicianIdentityDTOs;

namespace ServiceAbstraction
{
    public interface ITechAuthenticationService
    {
        //Register 
        //Take name, PhoneNo, Password, NationalIdNo 
        //Return Token, Display Name
        Task<TechDTO> techRegisterAsync(TechRegisterDTO techRegisterDTO);

    }
}
