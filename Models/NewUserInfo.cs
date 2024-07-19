using System.ComponentModel.DataAnnotations;

namespace PronptModel_ver2.Models
{
    public class NewUserInfo
    {
        [Required]
        [Display(Name = "ユーザー名")]
        public string Username { get; set; }

        [Display(Name = "氏名（フルネーム）")]
        public string Nickname { get; set; }

        [Required]
        [Display(Name = "メールアドレス")]
        [EmailAddress]
        public string Email { get; set; }


        [Required]
        [Display(Name = "性別")]
        public SexType Sex { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "パスワード")]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "パスワード確認")]
        [Compare("Password", ErrorMessage = "パスワードと確認パスワードが一致しません。")]
        public string PasswordConfirmation { get; set; }
    }
}
