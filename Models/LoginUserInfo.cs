using System.ComponentModel.DataAnnotations; 

namespace PronptModel_ver2.Models
{
    // ログイン画面のためのモデルクラス
    public class LoginUserInfo
    {

        [Display(Name = "ユーザー名")]
        public string Username { get; set; } = "";

        [Display(Name = "パスワード")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";
    }
}