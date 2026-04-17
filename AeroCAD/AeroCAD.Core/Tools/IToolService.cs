using System;
using System.Collections.Generic;
using Primusz.AeroCAD.Core.Drawing;

namespace Primusz.AeroCAD.Core.Tools
{
    public interface IToolService : IServiceProvider
    {
        IViewport Viewport { get; }

        IReadOnlyCollection<ITool> Tools { get; }

        void RegisterTool(ITool tool);

        void UnregisterTool(ITool tool);

        void SuspendAll();

        void SuspendAll(ITool exclude);

        void UnsuspendAll();

        ITool GetTool(Guid id);

        ITool GetTool(string name);

        TTool GetTool<TTool>() where TTool : class, ITool;

        bool ActivateTool(Guid id);

        bool ActivateTool(ITool tool);

        void DeactivateAll();

        bool DeactivateTool(ITool tool);

        T GetService<T>() where T : class;
    }
}
