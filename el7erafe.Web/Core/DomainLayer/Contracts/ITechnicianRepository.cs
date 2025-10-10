﻿using DomainLayer.Models.IdentityModule;
using DomainLayer.Models.IdentityModule.Enums;

namespace DomainLayer.Contracts
{
    public interface ITechnicianRepository
    {
        Task<Technician> CreateAsync(Technician technician); //Create technician 
        Task<Technician?> GetByIdAsync(int id); //Get technician by id
        Task<Technician?> GetByUserIdAsync(string userId); //Get technician by userId
        Task<IEnumerable<Technician>?> GetAllAsync(); //Get all technicians
        Task<IEnumerable<Technician>?> GetAllByStatusAsync(TechnicianStatus status); //Get all technicians by status
        Task<int> UpdateAsync(Technician technician); //Update technician
        Task<int> DeleteAsync(int id); //Delete technician
        Task<bool> ExistsAsync(int id); //Check if technician exists by id
        Task<bool> ExistsAsync(string phoneNumber); //Check if technician exists by phone number
        Task<bool> ExistsByNationalIdAsync(string nationalId); //Check if technician exists by national id

    }
}
