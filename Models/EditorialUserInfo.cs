using System.ComponentModel.DataAnnotations; 

namespace PronptModel_ver2.Models
{
    // ユーザー情報の編集のためのモデルクラス
    public class EditorialUserInfo : IValidatableObject
    {

        [Display(Name = "ユーザー名")]
        public string? Username { get; set; }


        [Display(Name = "氏名（フルネーム）")]
        public string Nickname { get; set; }

        [Display(Name = "メールアドレス")]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [Display(Name = "性別")]
        public SexType Sex { get; set; }

        [Display(Name = "現在のパスワード")]
        [DataType(DataType.Password)]
        public string? CurrentPassword { get; set; }

        [Display(Name = "新しいパスワード")]
        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }

        [Display(Name = "新しいパスワード(確認)")]
        [DataType(DataType.Password)]
        public string? NewPasswordConfirmation { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrEmpty(NewPassword))
            {
                if (string.IsNullOrEmpty(CurrentPassword))
                    yield return new ValidationResult("現在のパスワードを入力して下さい．");

                if (string.IsNullOrEmpty(NewPasswordConfirmation))
                    yield return new ValidationResult("新しいパスワード(確認)を入力して下さい．");

                if (NewPassword != NewPasswordConfirmation)
                    yield return new ValidationResult("入力されたパスワードが一致しません．");
            }
        }
    }
}