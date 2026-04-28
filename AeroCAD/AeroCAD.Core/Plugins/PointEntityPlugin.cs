using System.Collections.Generic;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Rendering;
using Primusz.AeroCAD.Core.Spatial;

namespace Primusz.AeroCAD.Core.Plugins
{
    public sealed class PointEntityPlugin : EntityPluginBase
    {
        protected override string PluginName => "AeroCAD.Point";
        protected override EntityPluginCapability Capabilities => EntityPluginCapability.Render | EntityPluginCapability.Bounds | EntityPluginCapability.InteractiveCommand;
        protected override IEntityRenderStrategy RenderStrategy => new PointEntityRenderStrategy();
        protected override IEntityBoundsStrategy BoundsStrategy => new PointBoundsStrategy();

        protected override IEnumerable<InteractiveCommandRegistration> CreateInteractiveCommands()
        {
            yield return InteractiveCommandRegistrationBuilder
                .Create("POINT")
                .WithAliases("PO", "PUNKT")
                .WithDescription("Create point object.")
                .WithInitialStep(new CommandStep("Point", "Specify a point:"))
                .InMenu("Draw", "_Point")
                .CreateEntityOnPoint((context, point) => new PointEntity(point), "POINT created.")
                .Build();

            yield return CreateSystemVariableCommand("PDMODE", SystemVariableService.PdMode, true);
            yield return CreateSystemVariableCommand("PDSIZE", SystemVariableService.PdSize, false);
        }

        private static InteractiveCommandRegistration CreateSystemVariableCommand(string commandName, string variableName, bool integer)
        {
            return InteractiveCommandRegistrationBuilder
                .Create(commandName)
                .WithDescription($"Set {variableName}.")
                .WithInitialStep(new CommandStep(commandName, $"Enter new value for {variableName}:"))
                .OnActivated(context =>
                {
                    var variables = context.GetService<ISystemVariableService>();
                    context.Feedback?.SetPrompt($"Enter new value for {variableName} <{variables?.Get<object>(variableName)}>:"); 
                })
                .OnToken((context, token) =>
                {
                    if (!token.ScalarValue.HasValue)
                        return context.Unhandled();

                    var variables = context.GetService<ISystemVariableService>();
                    if (integer)
                        variables?.Set(variableName, (int)token.ScalarValue.Value);
                    else
                        variables?.Set(variableName, token.ScalarValue.Value);

                    context.LogInput(token.RawText);
                    return context.End($"{variableName} = {variables?.Get<object>(variableName)}");
                })
                .Build();
        }
    }
}
