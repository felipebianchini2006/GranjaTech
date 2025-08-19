using GranjaTech.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GranjaTech.Application.Services.Interfaces
{
    public interface IAuthService
    {
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDetailDto?> GetUserByIdAsync(int id);
        Task<bool> RegistrarAsync(RegisterDto registerDto);
        Task<LoginResponseDto?> LoginAsync(LoginDto loginDto);
        Task<bool> UpdateUserAsync(int id, UpdateUserDto updateUserDto);
        Task<bool> DeleteUserAsync(int id);

        // Métodos de Perfil
        Task<UpdateProfileDto?> GetProfileAsync();
        Task<bool> UpdateProfileAsync(UpdateProfileDto profileDto);
        Task<bool> ChangePasswordAsync(ChangePasswordDto passwordDto);
    }
}
