#if !DOT42
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cirrious.MvvmCross.Plugins.Messenger;
using NinjaTasks.Core.Messages;
using NinjaTasks.Model;
using NinjaTasks.Model.ImportExport;
using NinjaTasks.Model.Storage;
using NinjaTools.MVVM;

namespace NinjaTasks.Core.ViewModels.Sync
{
    public class ImportExportViewModel : BaseViewModel
    {
        private readonly IImportExportFactory _importExport;
        private readonly ITodoStorage _storage;
        private readonly IMvxMessenger _messenger;

        public string FileName { get; set; }
        
        public IList<string> SupportedTypes { get; private set; }
        public string SelectedType { get; set; }
        public string ResultMessage { get; set; }

        public string SelectedListId { get; set; }
        public string SelectedListName { get; private set; }     // not used atm
        public bool ImportUnsetListToSelectedList { get; set; }  // not used atm

        public bool CanTreatCategoriesAsLists { get { return SelectedType == "ics"; } }
        public bool TreatCategoriesAsList { get; set; }

        public bool IsExport { get; set; }

        public ImportExportViewModel(IImportExportFactory importExport, ITodoStorage storage, IMvxMessenger messenger)
        {
            _importExport = importExport;
            _storage = storage;
            _messenger = messenger;
            
            TreatCategoriesAsList = true;

            AddToAutoBundling(() => IsExport);
            AddToAutoBundling(() => SelectedListId);
            AddToAutoBundling(() => TreatCategoriesAsList);
            SupportedTypes = new[] {"ics", "txt"};
        }

        public override void Start()
        {
            base.Start();

            TodoList selectedList = _storage.GetLists(SelectedListId).FirstOrDefault();
            SelectedListName = selectedList == null?"":selectedList.Description;
        }

        private void OnFileNameChanged()
        {
            ResultMessage = null;
            string ext = Path.GetExtension(FileName ?? "").Replace(".", "");
            
            if (string.IsNullOrWhiteSpace(SelectedType) && !string.IsNullOrEmpty(ext))
                SelectedType = SupportedTypes.FirstOrDefault(p => p.Equals(ext, StringComparison.OrdinalIgnoreCase));
        }

        public bool CanImportExport
        {
            get
            {
                return !string.IsNullOrWhiteSpace(FileName) 
                     && !string.IsNullOrEmpty(SelectedType)
                     && (ResultMessage == null || IsExport);
            }
        }
        public void ImportExport()
        {
            var imp = _importExport.CreateFileImportExport(SelectedType, new SerializationOptions { TreatCategoriesAsList = TreatCategoriesAsList});
            if (IsExport)
                ResultMessage = imp.ExportTo(FileName);
            else
            {
                ResultMessage = imp.Import(FileName);
                _messenger.Publish(new TrackableStoreModifiedMessage(this, ModificationSource.ImportExport));
            }
        }
    }

    public class MockImportExportViewModel : ImportExportViewModel
    {
        public MockImportExportViewModel() : base(null, null, null)
        {
        }
    }
}
#endif