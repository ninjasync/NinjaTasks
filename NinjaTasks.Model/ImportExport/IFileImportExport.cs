namespace NinjaTasks.Model.ImportExport
{
    public interface IFileImportExport
    {
        /// <summary>
        /// returns a statics message.
        /// </summary>
        string Import(string filename);

        /// <summary>
        /// returns a statics message.
        /// </summary>
        string ExportTo(string filename);
    }
}