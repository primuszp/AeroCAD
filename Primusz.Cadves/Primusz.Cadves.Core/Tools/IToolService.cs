using System;
using Primusz.Cadves.Core.Drawing;

namespace Primusz.Cadves.Core.Tools
{
    public interface IToolService : IServiceProvider
    {
        IViewport Viewport { get; }

        void RegisterTool(ITool tool);

        void UnregisterTool(ITool tool);

        void SuspendAll();

        void SuspendAll(ITool exclude);

        void UnsuspendAll();

        ITool GetTool(Guid id);

        ITool GetTool(string name);

        bool ActivateTool(Guid id);

        bool ActivateTool(ITool tool);

        void DeactivateAll();

        bool DeactivateTool(ITool tool);
    }
}
