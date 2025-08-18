using GranjaTech.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GranjaTech.Application.Services.Interfaces
{
    public interface IAuthService
    {
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<bool> RegistrarAsync(RegisterDto registerDto);
        Task<LoginResponseDto?> LoginAsync(LoginDto loginDto);
        Task<bool> UpdateUserAsync(int id, UpdateUserDto updateUserDto); // ADICIONE ESTE
        Task<bool> DeleteUserAsync(int id); // ADICIONE ESTE
    }
}