using Microsoft.AspNetCore.Identity; // 追記
using PronptModel_ver2.Models;                    // 追記


namespace PronptModel_ver2.Data
{
    // 初期ユーザー作成のためのクラス
    public class IdentityDataSeeder
    {
        public const string AdminRoleName = "administrators";  // アプリケーション内で用いる管理者グループの名前(決め打ち)
        public const string AdminUserName = "admin";           // アプリケーション内で用いる管理者ユーザーの名前(決め打ち)
        public const string NormalRoleName = "users";          // アプリケーション内で用いる通常ユーザーのグループの名前(決め打ち)
        public const string AllRoles = "administrators,users"; // すべてのグループをカンマでつないだもの(認証の設定で使う)

        //                              ┌─ ここが前回と異なり IdentityUser ではなく 
        //                              │   StudentUser となっている点に注意
        //                              ↓
        private readonly UserManager<StudentUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        // コンストラクタ
        public IdentityDataSeeder(UserManager<StudentUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // 初期ユーザー作成のためのメソッド(非同期メソッド版)
        public async Task EnsureDefaultCredentialsAsync()
        {
            if (!await _roleManager.RoleExistsAsync(AdminRoleName))                 // 管理者グループが存在していなければ
                await _roleManager.CreateAsync(new IdentityRole(AdminRoleName));    // それを作成する．
            if (!await _roleManager.RoleExistsAsync(NormalRoleName))                // 通常ユーザーのグループが存在していなければ
                await _roleManager.CreateAsync(new IdentityRole(NormalRoleName));   // それを作成する．

            var user = await _userManager.FindByNameAsync(AdminUserName);           // 管理者ユーザーが存在するかどうかを確認する．
            if (user == null)  // 存在しない場合．
            {
                user = new StudentUser()
                {
                    UserName = AdminUserName,
                    SecurityStamp = Guid.NewGuid().ToString("D"),
                    Nickname = AdminUserName,
                    Email = "admin@example.com",
                    Registered = DateTime.Now
                };

                await _userManager.CreateAsync(user, "p@55W0rD");                   // 管理者の初期パスワードは
                await _userManager.AddToRoleAsync(user, AdminRoleName);             // 決め打ちにする．
            }// if
        }

        // 初期ユーザー作成のためのメソッド(同期メソッド版)
        public void EnsureDefaultCredentials()
        {
            EnsureDefaultCredentialsAsync().Wait();
        }

        // Program.cs から呼び出して認証情報を作成するための静的メソッド
        public static void SeedData(IHost app)
        {
            var factory = app.Services.GetService<IServiceScopeFactory>();

            using (var scope = factory!.CreateScope())
            {
                var dataSeeder = scope.ServiceProvider.GetService<IdentityDataSeeder>();
                dataSeeder!.EnsureDefaultCredentials();
            }
        }
    }
}