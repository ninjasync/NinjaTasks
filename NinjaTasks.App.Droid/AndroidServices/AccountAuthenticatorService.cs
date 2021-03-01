using System;
using Android.Accounts;
using Android.App;
using Android.Content;
using Android.OS;
using NinjaTasks.App.Droid.Views;

namespace NinjaTasks.App.Droid.AndroidServices
{
    [Service(Enabled = true, Exported = false)]
    [IntentFilter(new[] { "android.accounts.AccountAuthenticator" })]
    [MetaData("android.accounts.AccountAuthenticator", Resource="@xml/account_authenticator")]
    public class AccountAuthenticatorService : Service
    {
        private static AccountAuthenticator _accountAuthenticator;

        private AccountAuthenticator Authenticator
        {
            get
            {
                if (_accountAuthenticator != null)
                    return _accountAuthenticator;
                return _accountAuthenticator = new AccountAuthenticator(this);
            }
        }

        public override IBinder OnBind(Intent intent)
        {
            if (intent.Action == AccountManager.ActionAuthenticatorIntent)
                return Authenticator.IBinder;
            return null;
        }


        private class AccountAuthenticator : AbstractAccountAuthenticator
        {
            private readonly Context _context;

            public AccountAuthenticator(Context context)
                : base(context)
            {
                this._context = context;
            }

            public override Bundle AddAccount(AccountAuthenticatorResponse response, String accountType, String authTokenType,
                    String[] requiredFeatures, Bundle options)
            {
                Intent intent = new Intent(_context, typeof(ConfigureAccountsView));
                intent.PutExtra(AccountManager.KeyAccountAuthenticatorResponse, response);
                Bundle bundle = new Bundle();
                bundle.PutParcelable(AccountManager.KeyIntent, intent);
                return bundle;
            }

            public override Bundle ConfirmCredentials(AccountAuthenticatorResponse response, Account account, Bundle options)
            {
                return null;
            }


            public override Bundle EditProperties(AccountAuthenticatorResponse response, String accountType)
            {
                return null;
            }

            public override Bundle GetAuthToken(AccountAuthenticatorResponse response, Account account, String authTokenType, Bundle options)
            {
                return null;
            }

            public override String GetAuthTokenLabel(String authTokenType)
            {
                return null;
            }

            public override Bundle HasFeatures(AccountAuthenticatorResponse response, Account account, String[] features)
            {
                return null;
            }

            public override Bundle UpdateCredentials(AccountAuthenticatorResponse response, Account account, String authTokenType, Bundle options)
            {
                return null;
            }
        }
    }
}