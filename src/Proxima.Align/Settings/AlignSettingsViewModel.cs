using Microsoft.VisualStudio.Extensibility.UI;
using Microsoft.Win32;
using System.Runtime.Serialization;

namespace Proxima.Align;

/// <summary>
/// View model for the alignment settings tool window, providing data-bindable properties
/// for operator alignment configuration, spacing options, theme colors, and commands.
/// </summary>
/// <remarks>
/// This view model manages the state of alignment settings including:
/// - Individual toggles for supported operators (=, +=, -=, *=, /=, %=, &amp;=, |=, ^=, &lt;&lt;=, &gt;&gt;=, =&gt;)
/// - Spacing configuration before and after operators
/// - Theme-aware color properties for UI rendering
/// - Save and Restore command implementations
/// The view model is serializable via data contract and implements property change notifications
/// for data binding in the Visual Studio Extensibility UI framework.
/// </remarks>
[DataContract]
internal sealed class AlignSettingsViewModel : NotifyPropertyChangedObject
{
    // ──────────────────────────────────────────────────────────────────────────
    // Operator map: single source of truth for operator ↔ property binding.
    // To add a new operator just add one entry here — nothing else changes.
    // ──────────────────────────────────────────────────────────────────────────
    private static readonly (string Op, Func<AlignSettingsViewModel, bool> Get, Action<AlignSettingsViewModel, bool> Set)[] OperatorMap =
    [
        ("=",   vm => vm.OpAssign,  (vm, v) => vm.OpAssign  = v),
        ("+=",  vm => vm.OpPlusEq,  (vm, v) => vm.OpPlusEq  = v),
        ("-=",  vm => vm.OpMinusEq, (vm, v) => vm.OpMinusEq = v),
        ("*=",  vm => vm.OpMulEq,   (vm, v) => vm.OpMulEq   = v),
        ("/=",  vm => vm.OpDivEq,   (vm, v) => vm.OpDivEq   = v),
        ("%=",  vm => vm.OpModEq,   (vm, v) => vm.OpModEq   = v),
        ("&=",  vm => vm.OpAndEq,   (vm, v) => vm.OpAndEq   = v),
        ("|=",  vm => vm.OpOrEq,    (vm, v) => vm.OpOrEq    = v),
        ("^=",  vm => vm.OpXorEq,   (vm, v) => vm.OpXorEq   = v),
        ("<<=", vm => vm.OpShlEq,   (vm, v) => vm.OpShlEq   = v),
        (">>=", vm => vm.OpShrEq,   (vm, v) => vm.OpShrEq   = v),
        ("=>",  vm => vm.OpArrow,   (vm, v) => vm.OpArrow   = v),
    ];

    private readonly AlignSettingsService _service;

    private bool _opAssign;
    private bool _opPlusEq;
    private bool _opMinusEq;
    private bool _opMulEq;
    private bool _opDivEq;
    private bool _opModEq;
    private bool _opAndEq;
    private bool _opOrEq;
    private bool _opXorEq;
    private bool _opShlEq;
    private bool _opShrEq;
    private bool _opArrow;
    private bool _enableLog;

    [DataMember] public bool EnableLog  { get => _enableLog;  set => SetProperty(ref _enableLog,  value); }
    [DataMember] public bool OpAssign   { get => _opAssign;   set => SetProperty(ref _opAssign,   value); }
    [DataMember] public bool OpPlusEq   { get => _opPlusEq;   set => SetProperty(ref _opPlusEq,   value); }
    [DataMember] public bool OpMinusEq  { get => _opMinusEq;  set => SetProperty(ref _opMinusEq,  value); }
    [DataMember] public bool OpMulEq    { get => _opMulEq;    set => SetProperty(ref _opMulEq,    value); }
    [DataMember] public bool OpDivEq    { get => _opDivEq;    set => SetProperty(ref _opDivEq,    value); }
    [DataMember] public bool OpModEq    { get => _opModEq;    set => SetProperty(ref _opModEq,    value); }
    [DataMember] public bool OpAndEq    { get => _opAndEq;    set => SetProperty(ref _opAndEq,    value); }
    [DataMember] public bool OpOrEq     { get => _opOrEq;     set => SetProperty(ref _opOrEq,     value); }
    [DataMember] public bool OpXorEq    { get => _opXorEq;    set => SetProperty(ref _opXorEq,    value); }
    [DataMember] public bool OpShlEq    { get => _opShlEq;    set => SetProperty(ref _opShlEq,    value); }
    [DataMember] public bool OpShrEq    { get => _opShrEq;    set => SetProperty(ref _opShrEq,    value); }
    [DataMember] public bool OpArrow    { get => _opArrow;    set => SetProperty(ref _opArrow,    value); }

    private bool _spaceBefore;
    private bool _spaceAfter;

    [DataMember] public bool SpaceBefore { get => _spaceBefore; set => SetProperty(ref _spaceBefore, value); }
    [DataMember] public bool SpaceAfter  { get => _spaceAfter;  set => SetProperty(ref _spaceAfter,  value); }

    [DataMember] public bool   IsDarkTheme    { get; }
    [DataMember] public string ColorForeground  { get; }
    [DataMember] public string ColorBackground  { get; }
    [DataMember] public string ColorGroupBg     { get; }
    [DataMember] public string ColorGroupBorder { get; }
    [DataMember] public string ColorBoxBorder   { get; }  // CheckBox border
    [DataMember] public string ColorBoxCheck    { get; }  // CheckBox glyph
    [DataMember] public string ColorBoxBg       { get; }  // CheckBox inner bg
    [DataMember] public string ColorBtnBg       { get; }
    [DataMember] public string ColorBtnBorder   { get; }

    [DataMember] public AsyncCommand SaveCommand    { get; }
    [DataMember] public AsyncCommand RestoreCommand { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AlignSettingsViewModel"/> class.
    /// </summary>
    /// <param name="service">The alignment settings service used to load and save configuration.</param>
    public AlignSettingsViewModel(AlignSettingsService service)
    {
        _service = service;
        LoadFromSettings(_service.Current);

        IsDarkTheme = DetectDarkTheme();

        (ColorForeground, ColorBackground, ColorGroupBg, ColorGroupBorder,
         ColorBoxBorder,  ColorBoxCheck,   ColorBoxBg,
         ColorBtnBg,      ColorBtnBorder) = IsDarkTheme
            ? ("#FFD4D4D4", "#FF1E1E1E", "#FF252526", "#FF3F3F46",
               "#FF999999", "#FFD4D4D4", "#FF3C3C3C",
               "#FF3F3F46", "#FF555558")
            : ("#FF1E1E1E", "#FFF5F5F5", "#FFFFFFFF", "#FFCCCCCC",
               "#FF717171", "#FF1E1E1E", "#FFFFFFFF",
               "#FFE1E1E1", "#FFACACAC");

        SaveCommand    = new AsyncCommand(ExecuteSaveAsync);
        RestoreCommand = new AsyncCommand(ExecuteRestoreAsync);
    }

    /// <summary>
    /// Detects whether the current Visual Studio theme is Dark by reading the registry.
    /// Dynamically resolves the installed VS version instead of hardcoding it.
    /// Falls back to <c>false</c> (Light) if the value cannot be read.
    /// </summary>
    private static bool DetectDarkTheme()
    {
        try
        {
            using var vsKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\VisualStudio");
            if (vsKey is null) return false;

            // ✅ MaxBy è O(n) vs OrderByDescending O(n log n), un solo Parse per elemento
            var version = vsKey.GetSubKeyNames()
                           .Where(n => double.TryParse(n,
                                       System.Globalization.NumberStyles.Any,
                                       System.Globalization.CultureInfo.InvariantCulture, out _))
                           .MaxBy(n => double.Parse(n,
                                       System.Globalization.CultureInfo.InvariantCulture));

            if (version is null) return false;

            using var key = vsKey.OpenSubKey($@"{version}\General");
            var theme = key?.GetValue("CurrentTheme") as string;

            return string.Equals(theme, "1ded0138-47ce-435e-84ef-9ec1f439b749",
                                  StringComparison.OrdinalIgnoreCase);
        }
        catch { return false; }
    }

    /// <summary>
    /// Loads operator and spacing settings from an <see cref="AlignSettings"/> instance
    /// into the view model properties using <see cref="OperatorMap"/>.
    /// </summary>
    private void LoadFromSettings(AlignSettings s)
    {
        foreach (var (op, _, set) in OperatorMap)
            set(this, s.EnabledOperators.Contains(op));

        SpaceBefore = s.SpaceBeforeOperator;
        SpaceAfter  = s.SpaceAfterOperator;
        EnableLog   = s.EnableLog;
    }

    /// <summary>
    /// Converts the current view model state into an <see cref="AlignSettings"/> instance
    /// using <see cref="OperatorMap"/> as single source of truth.
    /// </summary>
    private AlignSettings ToSettings() => new()
    {
        EnabledOperators    = OperatorMap.Where(x => x.Get(this))
                                         .Select(x => x.Op)
                                         .ToList(),
        SpaceBeforeOperator = SpaceBefore,
        SpaceAfterOperator  = SpaceAfter,
        EnableLog           = EnableLog,
        // Preserva AlignComments: non è esposta in UI ma non va persa al salvataggio
        AlignComments       = _service.Current.AlignComments,
    };

    /// <summary>
    /// Executes the save command, persisting the current view model settings to storage.
    /// </summary>
    private Task ExecuteSaveAsync(object? parameter, CancellationToken ct)
    {
        _service.Save(ToSettings());
        return Task.CompletedTask;
    }

    /// <summary>
    /// Executes the restore command, resetting all settings to their default values.
    /// </summary>
    private Task ExecuteRestoreAsync(object? parameter, CancellationToken ct)
    {
        LoadFromSettings(new AlignSettings());
        return Task.CompletedTask;
    }
}