using FPTU_Starter.Application.ViewModel;
using FPTU_Starter.Application.ViewModel.UserDTO;

namespace FPTU_Starter.Application.Services.IService
{
    public interface IUserManagementService
    {
        Task<ResultDTO<UserInfoResponse>> GetUserInfo();
        Task<ResultDTO<string>> UpdateUser(UserUpdateRequest userUpdateRequest);
        Task<bool> CheckIfUserExistByEmail(string email);
        Task<ResultDTO<UserInfoResponse>> GetUserInfoById(Guid id);
        Task<ResultDTO<List<UserInfoResponse>>> GetAllUsers(string? search, string? roleName);
        Task<ResultDTO<string>> UpdatePassword(string newPassword, string confirmPassword, string userEmail);
    }
}
