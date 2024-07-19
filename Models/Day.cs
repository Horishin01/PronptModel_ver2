using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace PronptModel_ver2.Models
{
    public enum DayName
    {
        [Display(Name = "月曜")]
        月曜,
        [Display(Name = "火曜")]
        火曜,
        [Display(Name = "水曜")]
        水曜,
        [Display(Name = "木曜")]
        木曜,
        [Display(Name = "金曜")]
        金曜,
        [Display(Name = "土曜")]
        土曜,
        [Display(Name = "日曜")]
        日曜,
    }
    // 「公開状態」を表す列挙型
    public enum PublicationStateType
    {
        [Display(Name = "下書き")]
        Draft,                        // 下書き

        [Display(Name = "公開済み")]
        Published,                    // 公開済み
    }

    public class Day
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "曜日")]
        public DayName DayNo { get; set; }

        [Display(Name = "持ち物登録")]
        public string PersonaLeffects { get; set; } = "";

        [Display(Name = "公開状態")]
        public PublicationStateType PublicationState { get; set; } // 公開状態


        [Display(Name = "学生Id")]
        public string StudentUserId { get; set; } = ""; // 外部キー(BlogUserのIdを参照) // 変更箇所

        public StudentUser? StudentUser { get; set; }      // 参照ナビゲーションプロパティ    }
    }
}
