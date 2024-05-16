using FPTU_Starter.Application.ViewModel.AuthenticationDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPTU_Starter.Application.Services.IService
{
    public interface IAuthenticationService
    {
        Task<ResponseToken> RegisterUserAsync(RegisterModel registerModel);
        Task<ResponseToken> LoginAsync(LoginDTO loginDTO);
    }
}
