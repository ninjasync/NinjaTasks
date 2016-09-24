using System;

namespace NinjaTools.Progress
{
    [Flags]
    public enum ProgressOptions
    {
        Plain,
        AllowCancel = 0x1,
        DelayPopup = 0x02,
        AllowPause = 0x4,
        IsIndeterminate = 0x8,
        Default = Plain
    };

    public interface IProgressFactory
    {
        IProgressDisposable Use(string title = "", ProgressOptions options = ProgressOptions.Default);
    }

    public static class ProgressExtentions
    {
        /// <summary>
        /// either calls factory.Use(), or returns new NullProgress() if factory is null.
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="title"> </param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IProgressDisposable SafeUse(this IProgressFactory factory, string title="", ProgressOptions options = ProgressOptions.Default)
        {
            if(factory == null) return new NullProgress();
            return factory.Use(title, options);
        }
    }
}
