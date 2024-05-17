using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPTU_Starter.Application.ViewModel.AuthenticationDTO
{
    public class RegisterModel
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        [Compare(nameof(Password))]
        public string? ConfirmPassword { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
    }
}
