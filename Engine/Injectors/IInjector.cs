using System;
using Engine.ProcessCore;

namespace Engine.Injectors
{
    public interface IInjector
    {
        string SelfFileName { get; }
        string UniqueId { get; }
        string UniqueName { get; }
        string About { get; }
        Module InjectedModule { get; set; }
        IntPtr Inject(Core targetProcess, string filePath);
    }
}