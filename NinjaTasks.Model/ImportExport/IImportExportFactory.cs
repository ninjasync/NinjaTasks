namespace NinjaTasks.Model.ImportExport
{
    public interface IImportExportFactory
    {
        IFileImportExport CreateFileImportExport(string fileExtension, SerializationOptions options);
    }
}