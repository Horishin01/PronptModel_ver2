using Microsoft.AspNetCore.Identity;         // 追記
using System.ComponentModel.DataAnnotations; // 追記

namespace PronptModel_ver2.Models
{

    // ISO/IEC 5218 に基づく性別型
    public enum SexType
    {
        [Display(Name = "不明")]
        NotKnown = 0, // 不明
        [Display(Name = "男性")]
        Male = 1, // 男性
        [Display(Name = "女性")]
        Female = 2, // 女性
        [Display(Name = "適用不可")]
        NotApplicable = 9, // 適用不可
    }


    // 「ユーザー」クラス
    public class StudentUser : IdentityUser
    {
        [Display(Name = "ユーザー名")]
        public override string UserName { get => base.UserName; set => base.UserName = value; }

        [Display(Name = "メールアドレス")]
        [EmailAddress]
        public override string Email { get => base.Email; set => base.Email = value; }

        [Display(Name = "氏名（フルネーム）")]
        public string? Nickname { get; set; }

        [Display(Name = "性別")]
        public SexType Sex { get; set; }


        [Display(Name = "登録日時")]
        [DataType(DataType.DateTime)]
        public DateTime Registered { get; set; }

        public List<Day>? Days { get; set; } // コレクションナビゲーションプロパティ

    }
}
