using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity; 
using Microsoft.AspNetCore.Mvc;
using PronptModel_ver2.Data;   
using PronptModel_ver2.Models;


namespace PronptModel_ver2.Controllers
{
    public class AccountsController : Controller
    {
        private readonly UserManager<StudentUser> _userManager;
        private readonly SignInManager<StudentUser> _signInManager;

        public AccountsController(UserManager<StudentUser> userManager, SignInManager<StudentUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // Indexアクション(GETのみ)，管理者のみアクセス可
        [Authorize(Roles = IdentityDataSeeder.AdminRoleName)]
        public async Task<IActionResult> Index()
        {
            // 「通常ユーザー」のリストを取得してビューに渡す．
            var users = await _userManager.GetUsersInRoleAsync(IdentityDataSeeder.NormalRoleName);

            return View(users);
        }
        //=========================================================Login処理==================================================================
        // Loginアクション(GET用)
        public IActionResult Login()
        {
            return View(new LoginUserInfo());
        }

        // Loginアクション(POST用)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginUserInfo loginUserInfo)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var res = await _signInManager.PasswordSignInAsync(loginUserInfo.Username, loginUserInfo.Password, false, false);
                    if (!res.Succeeded) throw new SignInOperationException(res);

                    return RedirectToAction(nameof(LoginRoute)); // ログインに成功したら，いったん LoginRoute に転送する．
                }
                catch (CompositeMessagesException ex)
                {
                    ViewBag.ErrorMessages = ex.ErrorMessages;
                }
            }//if

            return View(loginUserInfo);
        }

        // LoginRouteアクション(遷移先の振り分け用)，認証済みなら誰でもアクセス可
        [Authorize(Roles = IdentityDataSeeder.AllRoles)]
        public async Task<IActionResult> LoginRoute()
        {
            // ログイン中のユーザーに対応する StudentUser オブジェクトを取得する．
            var user = await _userManager.GetUserAsync(User);

            if (await IsAdminUserAsync(user))           　                                 // 管理者ユーザーの場合：
                return RedirectToAction(nameof(Index)); 　                                 //     Indexアクションに転送する．
            else                                                                          // それ以外の場合(通常ユーザーの場合)：
                return RedirectToAction(nameof(UserDetails), new { id = user!.UserName }); // ユーザー詳細画面へ遷移する．                                                                                           //     まだ作成していないのでトップページ / に遷移する．
        }
        //=========================================================Login処理==================================================================








        //=========================================================Logout処理==================================================================
        // Logoutアクション(POSTのみ)，認証済みなら誰でもアクセス可
        [Authorize(Roles = IdentityDataSeeder.AllRoles)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Redirect("/");
        }
        //=========================================================Logout処理==================================================================






        //=========================================================AccessDenied処理==================================================================
        // AccessDeniedアクション(GETのみ)
        public IActionResult AccessDenied()
        {
            return View();
        }

        // 指定されたユーザーが管理者ユーザー(⇔ adminロールに所属しているかどうか)を
        // チェックするヘルパーメソッド
        private async Task<bool> IsAdminUserAsync(StudentUser? StudentUser)
            => StudentUser != null && await _userManager.IsInRoleAsync(StudentUser, IdentityDataSeeder.AdminRoleName);

        //=========================================================AccessDenied処理==================================================================






        //=========================================================AddUser処理==================================================================
        // AddUserアクション(GET用)
        [Authorize(Roles = IdentityDataSeeder.AdminRoleName)]
        public IActionResult AddUser()
        {
            return View(new NewUserInfo());
        }


        // AddUserアクション(POST用)
        [Authorize(Roles = IdentityDataSeeder.AdminRoleName)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUser(NewUserInfo newUserInfo)
        {
            if (ModelState.IsValid)
            {
                var user = new StudentUser()
                {
                    UserName = newUserInfo.Username,
                    Email = newUserInfo.Email,
                    Sex = newUserInfo.Sex,
                    Registered = DateTime.Now,
                    SecurityStamp = Guid.NewGuid().ToString("D")
                };

                try
                {
                    var res1 = await _userManager.CreateAsync(user, newUserInfo.Password);
                    if (!res1.Succeeded) throw new IdentityOperationFailedException(res1);

                    var res2 = await _userManager.AddToRoleAsync(user, IdentityDataSeeder.NormalRoleName);
                    if (!res2.Succeeded) throw new IdentityOperationFailedException(res2);

                    return RedirectToAction(nameof(UserDetails), new { id = user.UserName }); // ユーザー詳細画面へ遷移する．
                }
                catch (CompositeMessagesException ex)
                {
                    ViewBag.ErrorMessages = ex.ErrorMessages;
                }
            }

            return View(newUserInfo);
        }
        //=========================================================AddUser処理==================================================================






        //=========================================================UserDetails処理==================================================================
        // UserDetailsアクション(GETのみ)
        [Authorize(Roles = IdentityDataSeeder.AllRoles)]
        public async Task<IActionResult> UserDetails(string? id)           // 引数名はルーティングパラメーター名と一致している必要がある．
        {
            if (string.IsNullOrEmpty(id))                                  // ルーティングパラメーターが指定されていなければ
                return NotFound();                                         // 404 Not Found を返す．

            var userName = id;                                             // ルーティングパラメーターには実際には
                                                                           // ユーザー名が格納されている(Idではない)

            var targetUser = await _userManager.FindByNameAsync(userName); // 指定されたユーザー名から，表示対象のユーザー情報を検索する．

            if (targetUser == null)                                        // 見つからなければ
                return NotFound();                                         // 404 Not Found を返す．

            var currentUser = await _userManager.GetUserAsync(User);       // 現在ログイン中のユーザー情報を取得して
            bool isAdminUser = await IsAdminUserAsync(currentUser);        // それが管理者ユーザーかどうかを調べる．

            if (!(isAdminUser || targetUser.Id == currentUser!.Id))        // 管理者ユーザーであるか，もしくは
                return Forbid();                                           // 表示対象がログインしているユーザー自身である場合以外は，
                                                                           // アクセスを拒否する．

            return View(targetUser);
        }
        //=========================================================UserDetails処理==================================================================







        //=========================================================EditUser処理==================================================================
        // EditUserアクション(GET用)
        [Authorize(Roles = IdentityDataSeeder.AllRoles)]
        public async Task<IActionResult> EditUser(string? id)
        {
            if (string.IsNullOrEmpty(id))                                  // ルーティングパラメーターが指定されていなければ
                return NotFound();                                         // 404 Not Found を返す．

            var userName = id;                                             // ルーティングパラメーターには実際には
                                                                           // ユーザー名が格納されている(Idではない)

            var targetUser = await _userManager.FindByNameAsync(userName); // 指定されたユーザー名から，操作対象のユーザー情報を検索する．

            if (targetUser == null)                                        // 見つからなければ
                return NotFound();                                         // 404 Not Found を返す．

            var currentUser = await _userManager.GetUserAsync(User);       // 現在ログイン中のユーザー情報を取得して
            bool isAdminUser = await IsAdminUserAsync(currentUser);        // それが管理者ユーザーかどうかを調べる．

            if (!(isAdminUser || targetUser.Id == currentUser!.Id))         // 管理者ユーザーであるか，もしくは
                return Forbid();                                           // 編集対象が自分自身である場合以外は，
                                                                           // アクセスを拒否する．

            ViewBag.TargetUser = targetUser;                               // 操作対象のユーザー情報をViewBagに入れておく(表示に使う)．
            ViewBag.IsAdminUser = isAdminUser;                             // 現在のユーザーが管理者かどうかもViewBagに入れておく(表示に使う)．

            return View(new EditorialUserInfo()                            //
            {                                                              // EditorialUserInfoオブジェクトを生成して，
                Nickname = targetUser.Nickname,                            // 操作対象のユーザーの情報の一部を転記してビューに渡す．
                Email = targetUser.Email,                                  //
            });
        }

        // EditUserアクション(POST用)
        [Authorize(Roles = IdentityDataSeeder.AllRoles)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(string? id, EditorialUserInfo editorialUserInfo)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var userName = id;

            var targetUser = await _userManager.FindByNameAsync(userName);

            if (targetUser == null)
                return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            bool isAdminUser = await IsAdminUserAsync(currentUser);

            if (!(isAdminUser || targetUser.Id == currentUser!.Id))
                return Forbid();

            ViewBag.TargetUser = targetUser;
            ViewBag.IsAdminUser = isAdminUser;

            // ↑ここまではGET用の EditUser() メソッドと同じ

            if (ModelState.IsValid)
            {
                try
                {
                    // 「ニックネーム」もしくは「メールアドレス」が現在の値とは異なる値である場合：
                    if ((targetUser.Nickname != editorialUserInfo.Nickname)
                        || (targetUser.Email != editorialUserInfo.Email))
                    {

                        targetUser.Nickname = editorialUserInfo.Nickname;               // ニックネームは空にできる(=削除)ので強制代入
                        targetUser.Email = editorialUserInfo.Email ?? targetUser.Email; // メールアドレスの削除はさせない                

                        // ユーザー情報を更新する．
                        var res = await _userManager.UpdateAsync(targetUser);
                        if (!res.Succeeded) throw new IdentityOperationFailedException(res);
                    }// if

                    // 「新しいパスワード」の入力欄が空ではない場合：
                    if (!string.IsNullOrEmpty(editorialUserInfo.NewPassword))
                    {
                        if (isAdminUser) // 管理者ユーザーとしてログインしている場合
                        {
                            // 操作対象のユーザーのパスワードを強制リセットする．
                            var token = await _userManager.GeneratePasswordResetTokenAsync(targetUser);
                            var res = await _userManager.ResetPasswordAsync(targetUser, token, editorialUserInfo.NewPassword);
                            if (!res.Succeeded) throw new IdentityOperationFailedException(res);
                        }
                        else // 通常ユーザーとしてログインしている場合
                        {
                            // パスワードの変更を試みる．
                            var res = await _userManager.ChangePasswordAsync(targetUser, editorialUserInfo.CurrentPassword!, editorialUserInfo.NewPassword);
                            if (!res.Succeeded) throw new IdentityOperationFailedException(res);
                        }
                    }//if

                    return RedirectToAction(nameof(UserDetails), new { id = targetUser.UserName }); // ユーザー詳細画面へ遷移する．
                }
                catch (CompositeMessagesException ex)
                {
                    ViewBag.ErrorMessages = ex.ErrorMessages;
                }
            }// if

            return View(editorialUserInfo);
        }
        
        //=========================================================EditUser処理==================================================================





        //=========================================================DeleteUser処理==================================================================
        // DeleteUserアクション(GET用)
        [Authorize(Roles = IdentityDataSeeder.AllRoles)]
        public async Task<IActionResult> DeleteUser(string? id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var userName = id;

            var targetUser = await _userManager.FindByNameAsync(userName);

            if (targetUser == null)
                return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            bool isAdminUser = await IsAdminUserAsync(currentUser);

            if ((isAdminUser && await IsAdminUserAsync(targetUser))   // 管理者ユーザーが同じ管理者ユーザーを削除しようとしている場合や
                || (!isAdminUser && targetUser.Id != currentUser!.Id))// 通常ユーザーが自分自身以外を削除しようとしている場合は
                return Forbid();                                      // アクセスを拒否する．

            ViewBag.CurrentUser = currentUser;                        // 現在のユーザー情報をViewBagに入れておく(表示に使う)．
            ViewBag.IsAdminUser = isAdminUser;                        // 現在のユーザーが管理者かどうかもViewBagに入れておく(表示に使う)．

            return View(targetUser);
        }

        // DeleteUserアクション(POST用)
        [Authorize(Roles = IdentityDataSeeder.AllRoles)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string? id, [Bind("UserName")] StudentUser StudentUser)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var userName = id;

            if (userName != StudentUser.UserName)
                return NotFound();

            var targetUser = await _userManager.FindByNameAsync(userName);

            if (targetUser == null)
                return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            bool isAdminUser = await IsAdminUserAsync(currentUser);

            if ((isAdminUser && await IsAdminUserAsync(targetUser))
                || (!isAdminUser && targetUser.Id != currentUser!.Id))
                return Forbid();

            ViewBag.CurrentUser = currentUser;
            ViewBag.IsAdminUser = isAdminUser;

            if (targetUser.Id == currentUser!.Id)           // 自分自身を削除しようとしている場合(通常ユーザーの退会処理)：
            {                                               //
                await _signInManager.SignOutAsync();        //    サインアウトする．
                await _userManager.DeleteAsync(targetUser); //    そのうえでユーザー情報を削除する．
                return Redirect("/");                       //    トップページ( / )にリダイレクトする．
            }                                               //
            else                                            // それ以外の場合(管理者ユーザーによるアカウントの削除処理)：
            {                                               //
                await _userManager.DeleteAsync(targetUser); //    ユーザー情報を削除する．
                return RedirectToAction(nameof(Index));     //    ユーザー一覧画面(Index)にリダイレクトする．
            }                                               //
        }

        //=========================================================DeleteUser処理==================================================================

    }
}