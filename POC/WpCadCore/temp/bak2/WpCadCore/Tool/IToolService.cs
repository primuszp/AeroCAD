using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WpCadCore.Controls;

namespace WpCadCore.Tool
{
    interface IToolService : IServiceProvider
    {
        IModelSpace ModelSpaceView { get; }

        void RegisterTool(ITool tool);
        void UnregisterTool(ITool tool);

        void SuspendAll();
        void SuspendAll(ITool exclude);
        void UnsuspendAll();

        ITool GetTool(Guid id);
        ITool GetTool(string name);

        bool ActivateTool(Guid id);
        bool ActivateTool(ITool tool);

        bool DeactivateTool(ITool tool);
        void DeactivateAll();
    }
}
