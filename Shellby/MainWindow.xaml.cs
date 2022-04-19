using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Keystone;
using ReactiveUI;

namespace Shellby
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IViewFor<MainWindowViewModel>
    {
        public MainWindowViewModel? ViewModel { get; set; }

        object? IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (MainWindowViewModel?)value;
        }

        public MainWindow()
        {
            // this is cancer smh
            HighlightingManager.Instance.RegisterHighlighting("Asm", new[] { ".s", ".asm" }, delegate
            {
                using (var stream = new MemoryStream(Shellby.Resources.Asm_Mode_Dark))
                using (XmlTextReader reader = new XmlTextReader(stream))
                {
                    return HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            });

            HighlightingManager.Instance.RegisterHighlighting("CSharpDark", new[] { ".cs" }, delegate
            {
                using (var stream = new MemoryStream(Shellby.Resources.CSharp_Mode_Dark))
                using (XmlTextReader reader = new XmlTextReader(stream))
                {
                    return HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            });

            InitializeComponent();

            this.ViewModel = new MainWindowViewModel();

            this.cmbPlatform.ItemsSource = MainWindowViewModel.platforms;
            this.cmbPlatform.SelectedIndex = 0;

            this.cmbFormatter.ItemsSource = this.ViewModel.Formatters;
            this.cmbFormatter.SelectedIndex = 0;

            WPFUI.Appearance.Theme.Set(
              WPFUI.Appearance.ThemeType.Dark,      // Theme type
              WPFUI.Appearance.BackgroundType.Auto, // Background type
              true                                  // Whether to change accents automatically
            );

            this.txtInputAss.Document = new ICSharpCode.AvalonEdit.Document.TextDocument();
            this.txtOutputShell.Document = new ICSharpCode.AvalonEdit.Document.TextDocument();

            this.txtInputAss.TextArea.Caret.PositionChanged += (s, e) =>
            {
                this.lblStatus.Text =
                    $"Ln {txtInputAss.TextArea.Caret.Line}, Col {txtInputAss.TextArea.Caret.Column}";
            };

            // init
            this.lblStatus.Text = $"Ln {txtInputAss.TextArea.Caret.Line}, Col {txtInputAss.TextArea.Caret.Column}";

            this.WhenActivated(d =>
            {
                this.Bind(ViewModel,
                    vm => vm.AssemblyCode,
                    v => v.txtInputAss.Document.Text)
                .DisposeWith(d);

                this.Bind(ViewModel,
                    vm => vm.SelectedFormatter,
                    v => v.cmbFormatter.SelectedItem)
                .DisposeWith(d);

                this.WhenAnyValue(x => x.ViewModel!.CompiledBinary)
                    .ObserveOnDispatcher()
                    .SubscribeOnDispatcher()
                    .BindTo(this, x => x.txtOutputShell.Document.Text)
                .DisposeWith(d);

                this.WhenAnyValue(x => x.ViewModel!.CompilationError)
                    .ObserveOnDispatcher()
                    .SubscribeOnDispatcher()
                    .BindTo(this, x => x.lblError.Text)
                .DisposeWith(d);

                this.Bind(ViewModel,
                    vm => vm.SelectedPlatform,
                    v => v.cmbPlatform.SelectedIndex,
                    x => (int)x,
                    x => (AssPlatform)x)
                .DisposeWith(d);

                this.BindCommand(ViewModel,
                    vm => vm.Compile,
                    v => v.btnCompile)
                .DisposeWith(d);

                this.Bind(ViewModel,
                    vm => vm.IsAutoCompilationEnabled,
                    v => v.tglAutoCompilation.IsChecked)
                .DisposeWith(d);
            });
        }
    }
}