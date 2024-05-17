
using FPTU_Starter.Application.ITokenService;
using FPTU_Starter.Application.Services.IService;
using FPTU_Starter.Application.ViewModel.AuthenticationDTO;
using FPTU_Starter.Domain.Entity;
using System.Net;

namespace FPTU_Starter.Application.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenGenerator _tokenGenerator;

        public AuthenticationService(IUnitOfWork unitOfWork, ITokenGenerator tokenGenerator )
        {
            _unitOfWork = unitOfWork;
            _tokenGenerator = tokenGenerator;
        }
        public async Task<ResponseToken> LoginAsync(LoginDTO loginDTO)
        {
            try
            {
                var getUser = await _unitOfWork.UserRepository.GetAsync(x => x.Email == loginDTO.Email);
                if (getUser == null)
                {
                    return new ResponseToken
                    {
                        Token = "can not found user"
                    };
                }

                bool checkPassword = BCrypt.Net.BCrypt.Verify(loginDTO.Password, getUser.Password);
                if (checkPassword)
                {
                    return new ResponseToken
                    {
                        Token = _tokenGenerator.GenerateToken(getUser)
                    };

                }
                else
                {
                    return new ResponseToken
                    {
                        Token = "can not found user"
                    };
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<ResponseToken> RegisterUserAsync(RegisterModel registerModel)
        {
            try
            {
                var getUser  = await _unitOfWork.UserRepository
                    .GetAsync(x => x.Email == registerModel.Email);
                if (getUser != null)
                    return new ResponseToken
                    {
                        Token = "No user found!!"
                    };
                var newUser = new ApplicationUser
                {
                    Email = registerModel.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(registerModel.Password),
                    Address = registerModel.Address,
                    Name = registerModel.Name,
                    Phone = registerModel.Phone,
                    Id = Guid.NewGuid(),
                };
                _unitOfWork.UserRepository.Add(newUser);
                await _unitOfWork.CommitAsync();
                return new ResponseToken
                {
                    Token = _tokenGenerator.GenerateToken(newUser)
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
    }
}
