using System.IO;
using Luster.VisualReviewer;
using Xunit;

namespace Luster.VisualReviewer.Tests
{
    public class ContractReaderTests
    {
        [Fact]
        public void Read_ReturnsFileContent()
        {
            var path = Path.GetTempFileName();
            File.WriteAllText(path, "# 契约\n- 控件库: HandyControl");
            try
            {
                Assert.Equal("# 契约\n- 控件库: HandyControl", ContractReader.Read(path));
            }
            finally { File.Delete(path); }
        }

        [Fact]
        public void Read_MissingFile_ReturnsEmpty()
        {
            Assert.Equal("", ContractReader.Read(Path.Combine(Path.GetTempPath(), "no_such_contract.md")));
        }
    }
}
