using System;
using System.Collections.Generic;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Tools;

namespace Primusz.AeroCAD.Core.Plugins
{
    public sealed class BuiltInModifyModule : CadModuleBase
    {
        public override string Name => "AeroCAD.BuiltInModify";

        public override IEnumerable<IEntityPlugin> Plugins
        {
            get { yield break; }
        }

        public override IEnumerable<InteractiveCommandRegistration> InteractiveCommands
        {
            get
            {
                yield return new InteractiveCommandRegistration(
                    "MOVE",
                    CreateControllerFactory(() => new MoveSelectionCommandController()),
                    aliases: new[] { "M" },
                    description: "Move selected entities.",
                    policy: new EditorCommandPolicy(CommandSelectionRequirement.Any, selectionFailureMessage: "MOVE requires a preselection."),
                    menuGroup: "Modify",
                    menuLabel: "_Move");

                yield return new InteractiveCommandRegistration(
                    "COPY",
                    CreateControllerFactory(() => new CopySelectionCommandController()),
                    aliases: new[] { "CO", "CP" },
                    description: "Copy selected entities.",
                    policy: new EditorCommandPolicy(CommandSelectionRequirement.Any, selectionFailureMessage: "COPY requires a preselection."),
                    menuGroup: "Modify",
                    menuLabel: "_Copy");

                yield return new InteractiveCommandRegistration(
                    "OFFSET",
                    CreateControllerFactory(() => new OffsetCommandController()),
                    aliases: new[] { "O", "OF" },
                    description: "Offset a selected line, polyline, circle or arc.",
                    policy: new EditorCommandPolicy(
                        CommandSelectionRequirement.Single,
                        new[] { typeof(Line), typeof(Polyline), typeof(Circle), typeof(Arc) },
                        "OFFSET requires exactly one preselected entity.",
                        "OFFSET currently supports line, polyline, circle and arc."),
                    menuGroup: "Modify",
                    menuLabel: "_Offset");

                yield return new InteractiveCommandRegistration(
                    "TRIM",
                    CreateControllerFactory(() => new TrimCommandController()),
                    aliases: new[] { "TR" },
                    description: "Trim an entity to a selected boundary.",
                    menuGroup: "Modify",
                    menuLabel: "_Trim");

                yield return new InteractiveCommandRegistration(
                    "EXTEND",
                    CreateControllerFactory(() => new ExtendCommandController()),
                    aliases: new[] { "EX" },
                    description: "Extend an entity to a selected boundary.",
                    menuGroup: "Modify",
                    menuLabel: "_Extend");
            }
        }

        private static Func<Func<Drawing.Layers.Layer>, IInteractiveCommandController> CreateControllerFactory(Func<IInteractiveCommandController> factory)
        {
            return _ => factory();
        }
    }
}
