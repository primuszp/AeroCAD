using System;

namespace Primusz.AeroCAD.Core.Editor
{
    public interface ISystemVariableService
    {
        event EventHandler<SystemVariableChangedEventArgs> VariableChanged;

        void Register(SystemVariableDefinition definition);

        bool TryGet<T>(string name, out T value);

        T Get<T>(string name, T fallback = default);

        void Set<T>(string name, T value);
    }
}
