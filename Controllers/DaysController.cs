using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PronptModel_ver2.Data;
using PronptModel_ver2.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace PronptModel_ver2.Controllers
{
    public class DaysController : Controller
    {
        private readonly PronptContext _context;
        private readonly UserManager<StudentUser> _userManager;
        private readonly SignInManager<StudentUser> _signInManager;

        public DaysController(PronptContext context, UserManager<StudentUser> userManager, SignInManager<StudentUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // 指定されたユーザーが管理者ユーザー(⇔ adminロールに所属しているかどうか)を
        // チェックするヘルパーメソッド
        private async Task<bool> IsAdminUserAsync(StudentUser? studentUser)
            => studentUser != null && await _userManager.IsInRoleAsync(studentUser, IdentityDataSeeder.AdminRoleName);

        // 指定されたユーザーが指定された記事を閲覧することができるかどうかを
        // チェックするヘルパーメソッド
        private async Task<bool> IsReadableAsync(StudentUser? studentUser, Day day)
        {
            bool isAdminUser = await IsAdminUserAsync(studentUser);
            bool isLoggedIn = _signInManager.IsSignedIn(User) && studentUser != null;

            return day.PublicationState == PublicationStateType.Published
                      || (isLoggedIn && (isAdminUser || day.StudentUserId == studentUser!.Id));
        }

        // 指定されたユーザーが指定された記事を編集もしくは削除することができるかどうかを
        // チェックするヘルパーメソッド
        private async Task<bool> IsModifiableAsync(StudentUser? studentUser, Day day)
        {
            bool isAdminUser = await IsAdminUserAsync(studentUser);
            bool isLoggedIn = _signInManager.IsSignedIn(User) && studentUser != null;

            return isLoggedIn && (isAdminUser || day.StudentUserId == studentUser!.Id);
        }

        // GET: Days
        public async Task<IActionResult> Index()
        {
            // ログイン中のユーザー情報とそれが管理者ユーザーかどうかを取得する
            StudentUser? currentUser = await _userManager.GetUserAsync(User);
            bool isAdminUser = currentUser != null && await IsAdminUserAsync(currentUser);

            // ログイン済みかどうか
            bool isLoggedIn = _signInManager.IsSignedIn(User) && currentUser != null;

            // クエリの構築
            IQueryable<Day> daysQuery = _context.Days.Include(a => a.StudentUser);

            if (isLoggedIn)
            {
                // ログイン中の場合は、ログインユーザーのデータまたは管理者の場合
                daysQuery = daysQuery.Where(a => a.StudentUserId == currentUser!.Id || isAdminUser);
            }
            else
            {
                // ログインしていない場合は公開されているデータのみ
                daysQuery = daysQuery.Where(a => a.PublicationState == PublicationStateType.Published);
            }

            // ViewBagに情報を設定
            ViewBag.CurrentUser = currentUser; // ↑でチェックした
            ViewBag.IsAdminUser = isAdminUser; // ユーザー関連の情報を
            ViewBag.IsLoggedIn = isLoggedIn;   // ViewBagに入れておく(表示に使う).

            // クエリを実行して結果を取得
            var days = await daysQuery.ToListAsync();

            return View(days);
        }

        // GET: days/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var day = await _context.Days
                .Include(a => a.StudentUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (day == null)
            {
                return NotFound();
            }

            // ログイン中のユーザー情報を取得して閲覧可能かどうかを調べる．
            StudentUser? currentUser = await _userManager.GetUserAsync(User);
            if (!await IsReadableAsync(currentUser, day)) return Forbid();

            // ログイン中の場合その記事を編集可能かどうかをViewDataに記憶しておく．
            ViewBag.IsModifiable = currentUser != null && await IsModifiableAsync(currentUser, day);

            return View(day);
        }

        // GET: Days/Create
        [Authorize]
        public IActionResult Create()
        {
            // ViewData["BlogUserId"] = new SelectList(_context.Users, "Id", "Id"); // ←削除
            return View(new Day()); // Articleクラスのインスタンスを作成して渡している

        }

        // POST: Days/Create
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DayNo,PersonaLeffects,PublicationState")] Day day)
        {
            var currentUser = await _userManager.GetUserAsync(User); // ユーザー情報を取得する

            // 認証済みのはずなのでユーザー情報を取得できないことはあり得ないがいちおうチェックしておく
            if (currentUser == null) throw new InvalidOperationException(nameof(currentUser));

            if (ModelState.IsValid)
            {
                day.StudentUserId = currentUser.Id; // 日の所有者のIDを現在ログイン中のユーザーに設定する // 変更箇所
                _context.Add(day);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id = day.Id }); // 記事の作成が成功したら Details に遷移する
            }
            return View(day);
        }

        // GET: Articles/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var day = await _context.Days.FindAsync(id);
            if (day == null)
            {
                return NotFound();
            }
            // ViewData["BlogUserId"] = new SelectList(_context.Users, "Id", "Id", article.BlogUserId); // ←削除

            // ログイン中のユーザー情報を取得して編集可能かどうかを調べる．
            StudentUser? currentUser = await _userManager.GetUserAsync(User);
            if (!await IsModifiableAsync(currentUser, day)) return Forbid();

            return View(day);
        }

        // POST: Days/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DayNo,PersonaLeffects,PublicationState,StudentUserId")] Day day)
        {
            if (id != day.Id)
            {
                return NotFound();
            }
            // ログイン中のユーザー情報を取得して編集可能かどうかを調べる．
            StudentUser? currentUser = await _userManager.GetUserAsync(User);
            if (!await IsModifiableAsync(currentUser, day)) return Forbid();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(day);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DayExists(day.Id)) // ここを修正
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Details), new { id = day.Id }); // 編集が成功したら Details アクションに戻す
            }
            // ViewData["BlogUserId"] = new SelectList(_context.Users, "Id", "Id", article.BlogUserId); // ←削除
            return View(day);
        }

        // GET: Articles/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var day = await _context.Days
                .Include(a => a.StudentUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (day == null)
            {
                return NotFound();
            }

            // ログイン中のユーザー情報を取得して削除可能かどうかを調べる．
            StudentUser? currentUser = await _userManager.GetUserAsync(User);
            if (!await IsModifiableAsync(currentUser, day)) return Forbid();

            return View(day);
        }

        // POST: Articles/Delete/5
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var day = await _context.Days.FindAsync(id);
            if (day == null)   // 要求されたIDの記事が
                return NotFound(); // 存在しなければ 404 Not Found

            // ログイン中のユーザー情報を取得して削除可能かどうかを調べる．
            StudentUser? currentUser = await _userManager.GetUserAsync(User);
            if (!await IsModifiableAsync(currentUser, day)) return Forbid();

            _context.Days.Remove(day); // 指定された記事を削除する．
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DayExists(int id)
        {
            return _context.Days.Any(e => e.Id == id);
        }
    }
}
