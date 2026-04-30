using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using AutoMetal.Infrastructure;

namespace AutoMetal.Services
{
    internal class ModelConversionService : IModelConversionService
    {
        private readonly Action<string> _log;
        private readonly ProcessRunner _runner;

        public ModelConversionService(Action<string> logCallback)
        {
            _log = logCallback ?? (_ => { });
            _runner = new ProcessRunner(_log);
        }

        public async Task<(bool success, string enginePath, string errorMessage)> ConvertPtToEngineAsync(
            string ptPath,
            string enginePath,
            string projectPath,
            string pythonExePath)
        {
            if (string.IsNullOrWhiteSpace(ptPath) || !File.Exists(ptPath))
            {
                return (false, string.Empty, "请先选择有效的最优权重(.pt/.pth)路径");
            }

            if (string.IsNullOrWhiteSpace(projectPath) || !Directory.Exists(projectPath))
            {
                return (false, string.Empty, $"DeepLab工程目录不存在：{projectPath}");
            }

            string onnxPath = Path.Combine(
                Path.GetDirectoryName(ptPath),
                Path.GetFileNameWithoutExtension(ptPath) + ".onnx");
            Directory.CreateDirectory(Path.GetDirectoryName(onnxPath));

            string helperScriptPath = EnsureOnnxExportHelperScript(projectPath);
            string args = $"\"{helperScriptPath}\" --pt \"{ptPath}\" --onnx \"{onnxPath}\" --num-classes 2 --backbone mobilenet --downsample 16";

            _log($"开始导出ONNX：{ptPath} -> {onnxPath}");
            int exitCode = await RunPythonWithOptionalVenvAsync(projectPath, pythonExePath, args);
            if (exitCode != 0 || !File.Exists(onnxPath))
            {
                return (false, string.Empty, "ONNX导出失败，请检查日志输出");
            }
            _log($"ONNX导出完成：{onnxPath}");

            if (string.IsNullOrWhiteSpace(enginePath))
            {
                enginePath = Path.Combine(Path.GetDirectoryName(onnxPath), Path.GetFileNameWithoutExtension(onnxPath) + ".engine");
            }
            Directory.CreateDirectory(Path.GetDirectoryName(enginePath));

            string trtWorkingDir = Directory.Exists(projectPath) ? projectPath : AppDomain.CurrentDomain.BaseDirectory;
            string trtArgs = $"--onnx=\"{onnxPath}\" --saveEngine=\"{enginePath}\"";
            _log($"开始TensorRT转换：{onnxPath} -> {enginePath}");
            int trtExitCode = await _runner.RunAsync("cmd.exe", $"/c trtexec {trtArgs}", trtWorkingDir);
            if (trtExitCode != 0 || !File.Exists(enginePath))
            {
                return (false, string.Empty, "Engine转换失败，请确认trtexec已安装并在PATH中");
            }

            _log($"Engine转换完成：{enginePath}");
            return (true, enginePath, string.Empty);
        }

        private string EnsureOnnxExportHelperScript(string projectPath)
        {
            string scriptPath = Path.Combine(projectPath, "export_onnx_helper.py");
            string scriptContent = @"
import argparse
from deeplab import DeeplabV3

if __name__ == '__main__':
    parser = argparse.ArgumentParser(description='Export DeepLab weights to ONNX')
    parser.add_argument('--pt', required=True)
    parser.add_argument('--onnx', required=True)
    parser.add_argument('--num-classes', type=int, default=2)
    parser.add_argument('--backbone', default='mobilenet')
    parser.add_argument('--downsample', type=int, default=16)
    args = parser.parse_args()

    deeplab = DeeplabV3(
        model_path=args.pt,
        num_classes=args.num_classes,
        backbone=args.backbone,
        downsample_factor=args.downsample,
        cuda=False
    )
    deeplab.convert_to_onnx(True, args.onnx)
";
            File.WriteAllText(scriptPath, scriptContent, Encoding.UTF8);
            return scriptPath;
        }

        private async Task<int> RunPythonWithOptionalVenvAsync(string projectPath, string pythonExePath, string pythonScriptArgs)
        {
            string activateBat = Path.Combine(projectPath, ".venv", "Scripts", "activate.bat");
            string cmdArguments;
            if (File.Exists(activateBat))
            {
                cmdArguments = $"/c \"call \"\"{activateBat}\"\" && python {pythonScriptArgs}\"";
            }
            else
            {
                string pythonExe = File.Exists(pythonExePath) ? pythonExePath : "python";
                cmdArguments = $"/c \"\"{pythonExe}\"\" {pythonScriptArgs}";
            }

            return await _runner.RunAsync("cmd.exe", cmdArguments, projectPath);
        }
    }
}
