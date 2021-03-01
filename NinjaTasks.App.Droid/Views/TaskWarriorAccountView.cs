using System;
using System.Security.Cryptography.X509Certificates;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Java.IO;
using Java.Net;
using NinjaTasks.Core.ViewModels;
using NinjaTasks.Core.ViewModels.Sync;
using NinjaTools.Droid.MvvmCross;
using String = System.String;
using StringBuilder = System.Text.StringBuilder;

#if DOT42
using Dot42.Manifest;
#endif

namespace NinjaTasks.App.Droid.Views
{
    [Activity(Label = "Add Account", Icon="@drawable/ic_launcher")]
    //[IntentFilter(new[] { ".activities.EditTaskWarriorAccount"}, 
    // Categories= new[] { "android.intent.category.DEFAULT" })]
    public class TaskWarriorAccountView : BaseView
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.TaskWarriorAccountView);

            FindViewById<Button>(Resource.Id.selectserver).Click += OnSelectServerFile;
            FindViewById<Button>(Resource.Id.selectclient).Click += OnSelectClientFile;
            FindViewById<Button>(Resource.Id.import_taskdconfig).Click += OnImportTaskdConf;
            //var presenter = (DroidPresenter)Mvx.IoCProvider.Resolve<IMvxAndroidViewPresenter>();

            ////var vm = Mvx.IoCProvider.IoCConstruct<TaskWarriorAccountViewModel>();
            //var initialFragment = new TaskWarriorAccountFragment { ViewModel = (TaskWarriorAccountViewModel)ViewModel };

            //presenter.RegisterFragmentManager(FragmentManager, initialFragment);
        }
        
        private void OnImportTaskdConf(object sender, EventArgs e)
        {
            Intent intent = new Intent(Intent.ActionGetContent);
            intent.SetType("file/*");
            StartActivityForResult(intent, 12);
        }

        private void OnSelectClientFile(object sender, EventArgs e)
        {
            Intent intent = new Intent(Intent.ActionGetContent);
            intent.SetType("file/*");
            StartActivityForResult(intent, 10);
        }

        private void OnSelectServerFile(object sender, EventArgs e)
        {
            Intent intent = new Intent(Intent.ActionGetContent);
            intent.SetType("file/*");
            StartActivityForResult(intent, 11);
        }

    

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            var vm = ViewModel as TaskWarriorAccountViewModel;
            if (vm == null) return;
            if (requestCode == 10 && resultCode == Result.Ok)
            {
                //byte[] readData = ReadUrlBinary(data);
                //ViewModel.Account.ClientCertificateAndKeyPem = readData;
                vm.ClientCertificateAndKeyPfxFile = data.Data.Path;
                vm.RaiseAllPropertiesChanged();
            }
            if (requestCode == 11 && resultCode == Result.Ok)
            {
                //byte[] readData 
                //byte[] readData = ReadUrl(data);
                vm.ServerCertificateCrtFile = data.Data.Path;
                vm.RaiseAllPropertiesChanged();
            }
            if (requestCode == 12 && resultCode == Result.Ok)
            {
                string readData = ReadUrl(data);
                vm.ImportTaskdConfig(readData, true);
            }
        }

        private static string ReadUrl(Intent data)
        {
            StringBuilder bld = new StringBuilder();
            // Read all the text returned by the server
            using(BufferedReader br = new BufferedReader(new InputStreamReader(new URL(data.DataString).OpenStream())))
            {
                String str;
                while ((str = br.ReadLine()) != null)
                    // str is one line of text; readLine() strips the newline character(s)
                    bld.AppendLine(str);
            }
            
            var readData = bld.ToString();
            return readData;
        }

        private static byte[] ReadUrlBinary(Intent data)
        {
            ByteArrayOutputStream buffer = new ByteArrayOutputStream();
            // Read all the text returned by the server
            using (var ir = new URL(data.DataString).OpenStream())
            {
                int nRead;
                byte[] b = new byte[16384];

                while ((nRead = ir.Read(b, 0, b.Length)) != -1) 
                    buffer.Write(b, 0, nRead);
            }

            buffer.Flush();
            return buffer.ToByteArray();
        }

    }
}