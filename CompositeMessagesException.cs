using Microsoft.AspNetCore.Identity; // 追記

namespace PronptModel_ver2
{
    // 複合メッセージをもつ抽象例外クラス
    public abstract class CompositeMessagesException : Exception
    {
        public abstract IEnumerable<string> ErrorMessages { get; }
    }

    // サインインに関する例外クラス
    public class SignInOperationException : CompositeMessagesException
    {
        public SignInResult Result { get; }

        public SignInOperationException(SignInResult res)
        {
            Result = res;
        }

        public override IEnumerable<string> ErrorMessages
        {
            get
            {
                if (!Result.Succeeded)
                {
                    yield return "ログインに失敗しました．";
                    if (Result.IsLockedOut)
                        yield return "ロックアウトされています．";
                    if (Result.IsNotAllowed)
                        yield return "許可されていません";
                    if (Result.RequiresTwoFactor)
                        yield return "二要素認証が必要です．";
                }
            }
        }
    }

    // アカウント操作に関する例外クラス
    public class IdentityOperationFailedException : CompositeMessagesException
    {
        public IdentityResult Result { get; }
        public IdentityOperationFailedException(IdentityResult res)
        {
            Result = res;
        }
        public override string Message => string.Join(", ", ErrorMessages);
        public override IEnumerable<string> ErrorMessages => from e in Result.Errors select e.Description;
    }
}