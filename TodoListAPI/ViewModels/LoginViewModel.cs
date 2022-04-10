using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TodoListAPI.ViewModels
{
    public class LoginViewModel
    {
        [DisplayName("Username")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "The username is required!")]
        [StringLength(maximumLength: 50, MinimumLength = 8, ErrorMessage = "The username has to be between 8 and 50 characters+")]
        public string Username { get; set; }

        [DisplayName("Password")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "The password is required!")]
        [StringLength(maximumLength: 50, MinimumLength = 8, ErrorMessage = "The password has to be between 8 and 50 characters+")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

    }
}
