using System.Threading.Tasks;

namespace AutoMetal.Services
{
    internal interface IModelConversionService
    {
        Task<(bool success, string enginePath, string errorMessage)> ConvertPtToEngineAsync(
            string ptPath,
            string enginePath,
            string projectPath,
            string pythonExePath);
    }
}
