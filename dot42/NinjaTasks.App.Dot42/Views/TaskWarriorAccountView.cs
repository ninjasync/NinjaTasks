using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Java.IO;
using Java.Net;
using NinjaTasks.Core.ViewModels.Sync;
using NinjaTools.Droid.MvvmCross;
using String = System.String;
using StringBuilder = System.Text.StringBuilder;

#if DOT42
using Dot42.Manifest;
#endif

namespace NinjaTasks.App.Droid.Views
{
    [Activity(Label = "Add Account", Icon="@drawable/ic_launcher", VisibleInLauncher = false)]
    //[IntentFilter(new[] { ".activities.EditTaskWarriorAccount"}, 
    // Categories= new[] { "android.intent.category.DEFAULT" })]
    public class TaskWarriorAccountView : BaseView
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(R.Layout.TaskWarriorAccountView);

            FindViewById<Button>(R.Id.selectserver).Click += OnSelectServerFile;
            FindViewById<Button>(R.Id.selectclient).Click += OnSelectClientFile;
            FindViewById<Button>(R.Id.import_taskdconfig).Click += OnImportTaskdConf;
            //var presenter = (DroidPresenter)Mvx.Resolve<IMvxAndroidViewPresenter>();

            ////var vm = Mvx.IocConstruct<TaskWarriorAccountViewModel>();
            //var initialFragment = new TaskWarriorAccountFragment { ViewModel = (TaskWarriorAccountViewModel)ViewModel };

            //presenter.RegisterFragmentManager(FragmentManager, initialFragment);
        }
        
        private void OnImportTaskdConf(object sender, EventArgs e)
        {
            Intent intent = new Intent(Intent.ACTION_GET_CONTENT);
            intent.SetType("file/*");
            StartActivityForResult(intent, 12);
        }

        private void OnSelectClientFile(object sender, EventArgs e)
        {
            Intent intent = new Intent(Intent.ACTION_GET_CONTENT);
            intent.SetType("file/*");
            StartActivityForResult(intent, 10);
        }

        private void OnSelectServerFile(object sender, EventArgs e)
        {
            Intent intent = new Intent(Intent.ACTION_GET_CONTENT);
            intent.SetType("file/*");
            StartActivityForResult(intent, 11);
        }

    

        protected override void OnActivityResult(int requestCode, int resultCode, Intent data)
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
            BufferedReader br = new BufferedReader(new InputStreamReader(new URL(data.DataString).OpenStream()));
            {
                String str;
                while ((str = br.ReadLine()) != null)
                    // str is one line of text; readLine() strips the newline character(s)
                    bld.AppendLine(str);
            }
            br.Close();
            
            var readData = bld.ToString();
            return readData;
        }

        private static byte[] ReadUrlBinary(Intent data)
        {
            ByteArrayOutputStream buffer = new ByteArrayOutputStream();
            // Read all the text returned by the server
            var ir = new URL(data.DataString).OpenStream();
            {
                int nRead;
                byte[] b = new byte[16384];

                while ((nRead = ir.Read(b, 0, b.Length)) != -1) 
                    buffer.Write(b, 0, nRead);
            }

            ir.Close();

            buffer.Flush();
            return buffer.ToByteArray();
        }

    }
}