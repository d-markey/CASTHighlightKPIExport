using System.Text;

namespace HighlightKPIExport.Technical {
    public interface IArgument {
        bool Flag { get; }
        string Name { get; }
        string ShortName { get; }
        string Description { get; }
        void SetValue(string value);
        StringBuilder AppendUsage(StringBuilder sb);
    }
}