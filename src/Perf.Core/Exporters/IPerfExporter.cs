using System.Threading.Tasks;
using Perf.Core.Tracing;

namespace Perf.Core.Exporters;

public interface IPerfExporter
{
    Task ExportAsync(PerfTrace trace);
}