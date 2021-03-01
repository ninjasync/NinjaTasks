namespace NinjaTools.Connectivity.Streams
{
    public interface IAbortableStream
    {
        /// <summary>
        /// aborts a connection , i.e. error condition, instead of 
        /// gracefully closing it with Close() or Dispose()
        /// </summary>
        void Abort();
    }
}
