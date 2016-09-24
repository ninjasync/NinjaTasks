using System;
using NinjaTasks.Model.ImportExport;
using NinjaTasks.Model.Storage;

namespace NinjaTasks.Sync.ImportExport
{
    public class ImportExportFactory : IImportExportFactory
    {
        private readonly ITodoStorage _storage;

        public ImportExportFactory(ITodoStorage storage)
        {
            _storage = storage;
        }

        public IFileImportExport CreateFileImportExport(string fileExtension, SerializationOptions options)
        {
            ITasksSerializer serializer = null;
            if (fileExtension.ToLowerInvariant() == "ics")
            {
                var s = new IcsSerializerDDay();
                s.TreatCategoriesAsList = options.TreatCategoriesAsList;
                serializer = s;
            }
            else if(fileExtension.ToLowerInvariant() == "txt")
                serializer = new TodoTxtSerializer();

            if(serializer == null)
                throw new Exception("unsupported file type: " + fileExtension);

            return new FileImportExport(_storage, serializer);

            
        }
    }
}
