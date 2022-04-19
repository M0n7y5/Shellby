using Keystone;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Shellby.ShellFormatters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Shellby
{
    public class MainWindowViewModel : ReactiveObject
    {
        public static List<string> platforms = Enum.GetNames<AssPlatform>().ToList();

        public ReactiveCommand<Unit, Unit> Compile { get; }

        [Reactive]
        public string AssemblyCode { get; set; } = "";

        [Reactive]
        public string CompiledBinary { get; set; } = "";

        [Reactive]
        public AssPlatform SelectedPlatform { get; set; }

        [Reactive]
        public bool IsAutoCompilationEnabled { get; set; }

        [Reactive]
        public string CompilationError { get; set; } = "";

        [Reactive]
        public IShellFormatter SelectedFormatter { get; set; }

        public List<IShellFormatter> Formatters { get; set; }

        private int _LineError { get; set; }

        public MainWindowViewModel()
        {
            Formatters = new List<IShellFormatter>()
            {
                new JustBytesFormatter(),
                new CSharpFormatter(),
                new CPPFormatter(),
                new PythonFormatter(),
            };

            SelectedFormatter = Formatters[0];

            Compile = ReactiveCommand.CreateFromTask(CompilationHandler);

            Compile.ThrownExceptions.Subscribe(e =>
            {
                if (e is KeystoneException)
                {
                    var ke = e as KeystoneException;
                    var errMsg = Engine.ErrorToString(ke!.Error);

                    CompilationError = $"Ln {_LineError}, {errMsg}";
                }
                else
                {
                    MessageBox.Show(e.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                CompiledBinary = "";
            });

            this.WhenAnyValue(
                x => x.IsAutoCompilationEnabled,
                x => x.AssemblyCode,
                x => x.SelectedFormatter,
                x => x.SelectedPlatform,
                (enabled, code, formatter, platform) =>
                new { enabled, code, formatter, platform })
                .Where(x => x.enabled)
                .Select(x => Unit.Default)
                .InvokeCommand(Compile);
        }

        private async Task CompilationHandler()
        {
            await Task.Run(() =>
            {
                CompilationError = "";

                if (string.IsNullOrWhiteSpace(AssemblyCode) || string.IsNullOrEmpty(AssemblyCode))
                {
                    CompiledBinary = "";
                    return;
                }

                var lines = AssemblyCode.Split(Environment.NewLine);
                var output = new StringBuilder(500);

                using (Engine engine = new Engine(
                    Architecture.X86, SelectedPlatform == AssPlatform.x32 ? Mode.X32 : Mode.X64)
                {
                    ThrowOnError = true
                })
                {
                    _LineError = 0;

                    var data = new List<(List<byte>, string)>();

                    foreach (var line in lines)
                    {
                        _LineError++;

                        if (string.IsNullOrWhiteSpace(line))
                        {
                            output.AppendLine();
                            continue;
                        }

                        var bytes = engine.Assemble(line, 0).Buffer;

                        data.Add((bytes.ToList(), line));
                    }

                    var formatted = SelectedFormatter.Format(data);

                    CompiledBinary = formatted;
                }
            });
        }
    }
}