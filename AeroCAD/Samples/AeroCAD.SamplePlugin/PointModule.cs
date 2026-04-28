using System.Collections.Generic;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Plugins;

namespace Primusz.AeroCAD.SamplePlugin
{
    public sealed class PointModule : CadModuleBase
    {
        public override string Name => "AeroCAD.SamplePlugin.Point";

        public override string Version => "1.0.0";

        public override IEnumerable<IEntityPlugin> Plugins
        {
            get
            {
                yield return EntityPluginBuilder
                    .Create("AeroCAD.SamplePlugin.Point")
                    .WithRenderStrategy(new PointRenderStrategy())
                    .WithBoundsStrategy(new PointBoundsStrategy())
                    .WithGripPreviewStrategy(new PointGripPreviewStrategy())
                    .WithInteractiveCommand(CreatePointCommand())
                    .WithInteractiveCommand(CreatePdModeCommand())
                    .WithInteractiveCommand(CreatePdSizeCommand())
                    .BuildPlugin();
            }
        }

        private static InteractiveCommandRegistration CreatePointCommand()
        {
            var pointStep = new CommandStep("Point", "Specify a point:");

            return InteractiveCommandRegistrationBuilder
                .Create("POINT")
                .WithAliases("PO", "PUNKT")
                .WithDescription("Create point object.")
                .WithInitialStep(pointStep)
                .InMenu("Draw", "_Point")
                .CreateEntityOnPoint((context, point) => new PointEntity(point), "POINT created.")
                .Build();
        }

        private static InteractiveCommandRegistration CreatePdModeCommand()
        {
            var step = new CommandStep("PDMODE", "Enter new value for PDMODE <0>:");

            return InteractiveCommandRegistrationBuilder
                .Create("PDMODE")
                .WithDescription("Set point display mode.")
                .WithInitialStep(step)
                .OnActivated(context => context.Feedback?.SetPrompt($"Enter new value for PDMODE <{PointDisplaySettings.GetPdMode(context.GetService<ISystemVariableService>())}>:"))
                .OnToken((context, token) =>
                {
                    if (!token.ScalarValue.HasValue)
                        return context.Unhandled();

                    var variables = context.GetService<ISystemVariableService>();
                    variables?.Set(SystemVariableService.PdMode, (int)token.ScalarValue.Value);
                    context.LogInput(token.RawText);
                    return context.End($"PDMODE = {PointDisplaySettings.GetPdMode(variables)}");
                })
                .Build();
        }

        private static InteractiveCommandRegistration CreatePdSizeCommand()
        {
            var step = new CommandStep("PDSIZE", "Enter new value for PDSIZE <0>:");

            return InteractiveCommandRegistrationBuilder
                .Create("PDSIZE")
                .WithDescription("Set point display size.")
                .WithInitialStep(step)
                .OnActivated(context => context.Feedback?.SetPrompt($"Enter new value for PDSIZE <{PointDisplaySettings.GetPdSize(context.GetService<ISystemVariableService>())}>:"))
                .OnToken((context, token) =>
                {
                    if (!token.ScalarValue.HasValue)
                        return context.Unhandled();

                    var variables = context.GetService<ISystemVariableService>();
                    variables?.Set(SystemVariableService.PdSize, token.ScalarValue.Value);
                    context.LogInput(token.RawText);
                    return context.End($"PDSIZE = {PointDisplaySettings.GetPdSize(variables)}");
                })
                .Build();
        }
    }
}
