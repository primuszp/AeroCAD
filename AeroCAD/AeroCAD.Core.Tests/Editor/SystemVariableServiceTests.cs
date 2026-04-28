using Primusz.AeroCAD.Core.Editor;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.Editor
{
    public class SystemVariableServiceTests
    {
        [Fact]
        public void Defaults_ReturnAutoCadPointDisplayDefaults()
        {
            var service = new SystemVariableService();

            Assert.Equal(0, service.Get<int>(SystemVariableService.PdMode));
            Assert.Equal(0d, service.Get<double>(SystemVariableService.PdSize));
        }

        [Fact]
        public void Set_RaisesVariableChanged()
        {
            var service = new SystemVariableService();
            string changed = null;

            service.VariableChanged += (s, e) => changed = e.Name;
            service.Set(SystemVariableService.PdMode, 3);

            Assert.Equal(SystemVariableService.PdMode, changed);
            Assert.Equal(3, service.Get<int>(SystemVariableService.PdMode));
        }

        [Fact]
        public void Set_SanitizesNegativePdMode()
        {
            var service = new SystemVariableService();

            service.Set(SystemVariableService.PdMode, -1);

            Assert.Equal(0, service.Get<int>(SystemVariableService.PdMode));
        }
    }
}
