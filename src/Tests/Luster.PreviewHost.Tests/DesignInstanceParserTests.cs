using Luster.PreviewHost;
using Xunit;

namespace Luster.PreviewHost.Tests
{
    public class DesignInstanceParserTests
    {
        [Fact]
        public void Parse_SimpleDesignInstance_ReturnsTypeName()
        {
            string xaml = "<UserControl d:DesignInstance=\"ClrNs.Foo.DesignVm\" xmlns:d=\"http://...\"/>";
            var info = DesignInstanceParser.Parse(xaml);
            Assert.NotNull(info);
            Assert.Equal("ClrNs.Foo.DesignVm", info.TypeName);
        }

        [Fact]
        public void Parse_TypeMarkup_ReturnsTypeName()
        {
            string xaml = "<UserControl d:DesignInstance=\"{Type ClrNs.Foo.DesignVm}\"/>";
            var info = DesignInstanceParser.Parse(xaml);
            Assert.NotNull(info);
            Assert.Equal("ClrNs.Foo.DesignVm", info.TypeName);
        }

        [Fact]
        public void Parse_NoDesignInstance_ReturnsNull()
        {
            string xaml = "<UserControl xmlns=\"http://...\"/>";
            Assert.Null(DesignInstanceParser.Parse(xaml));
        }

        [Fact]
        public void Parse_IsDesignDataCreatableTrueByDefault()
        {
            string xaml = "<UserControl d:DesignInstance=\"ClrNs.Foo.DesignVm\"/>";
            var info = DesignInstanceParser.Parse(xaml);
            Assert.True(info.IsDesignDataCreatable);
        }
    }
}
