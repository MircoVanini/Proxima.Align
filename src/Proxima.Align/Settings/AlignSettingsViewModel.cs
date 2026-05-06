using Microsoft.VisualStudio.Extensibility.ToolWindows;
using Microsoft.VisualStudio.Extensibility.UI;
using Microsoft.VisualStudio.RpcContracts.RemoteUI;
using Microsoft.Win32;
using System.Runtime.Serialization;
using System.Text;

namespace Proxima.Align;

[DataContract]
internal sealed class AlignSettingsViewModel : NotifyPropertyChangedObject
{
    private readonly AlignSettingsService _service;

    // ── Operator toggles ──────────────────────────────────────────────────────

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

    [DataMember] public bool OpAssign { get => _opAssign; set => SetProperty(ref _opAssign, value); }
    [DataMember] public bool OpPlusEq { get => _opPlusEq; set => SetProperty(ref _opPlusEq, value); }
    [DataMember] public bool OpMinusEq { get => _opMinusEq; set => SetProperty(ref _opMinusEq, value); }
    [DataMember] public bool OpMulEq { get => _opMulEq; set => SetProperty(ref _opMulEq, value); }
    [DataMember] public bool OpDivEq { get => _opDivEq; set => SetProperty(ref _opDivEq, value); }
    [DataMember] public bool OpModEq { get => _opModEq; set => SetProperty(ref _opModEq, value); }
    [DataMember] public bool OpAndEq { get => _opAndEq; set => SetProperty(ref _opAndEq, value); }
    [DataMember] public bool OpOrEq { get => _opOrEq; set => SetProperty(ref _opOrEq, value); }
    [DataMember] public bool OpXorEq { get => _opXorEq; set => SetProperty(ref _opXorEq, value); }
    [DataMember] public bool OpShlEq { get => _opShlEq; set => SetProperty(ref _opShlEq, value); }
    [DataMember] public bool OpShrEq { get => _opShrEq; set => SetProperty(ref _opShrEq, value); }
    [DataMember] public bool OpArrow { get => _opArrow; set => SetProperty(ref _opArrow, value); }

    // ── General options ───────────────────────────────────────────────────────

    private bool _spaceBefore;
    private bool _spaceAfter;

    [DataMember] public bool SpaceBefore { get => _spaceBefore; set => SetProperty(ref _spaceBefore, value); }
    [DataMember] public bool SpaceAfter { get => _spaceAfter; set => SetProperty(ref _spaceAfter, value); }

    // ── Theme colors (read-only, computed once at construction) ───────────────

    [DataMember] public bool IsDarkTheme { get; }
    [DataMember] public string ColorForeground { get; }
    [DataMember] public string ColorBackground { get; }
    [DataMember] public string ColorGroupBg { get; }
    [DataMember] public string ColorGroupBorder { get; }
    [DataMember] public string ColorBoxBorder { get; }   // CheckBox border
    [DataMember] public string ColorBoxCheck { get; }   // CheckBox glyph
    [DataMember] public string ColorBoxBg { get; }   // CheckBox inner bg
    [DataMember] public string ColorBtnBg { get; }
    [DataMember] public string ColorBtnBorder { get; }

    // ── Commands ──────────────────────────────────────────────────────────────

    [DataMember] public AsyncCommand SaveCommand { get; }
    [DataMember] public AsyncCommand RestoreCommand { get; }

    // ── Ctor ──────────────────────────────────────────────────────────────────

    public AlignSettingsViewModel(AlignSettingsService service)
    {
        _service = service;
        LoadFromSettings(_service.Current);

        IsDarkTheme = false;

        if (IsDarkTheme)
        {
            ColorForeground  = "#FFD4D4D4";
            ColorBackground  = "#FF1E1E1E";
            ColorGroupBg     = "#FF252526";
            ColorGroupBorder = "#FF3F3F46";
            ColorBoxBorder   = "#FF999999";
            ColorBoxCheck    = "#FFD4D4D4";
            ColorBoxBg       = "#FF3C3C3C";
            ColorBtnBg       = "#FF3F3F46";
            ColorBtnBorder   = "#FF555558";
        }
        else
        {
            ColorForeground  = "#FF1E1E1E";
            ColorBackground  = "#FFF5F5F5";
            ColorGroupBg     = "#FFFFFFFF";
            ColorGroupBorder = "#FFCCCCCC";
            ColorBoxBorder   = "#FF717171";
            ColorBoxCheck    = "#FF1E1E1E";
            ColorBoxBg       = "#FFFFFFFF";
            ColorBtnBg       = "#FFE1E1E1";
            ColorBtnBorder   = "#FFACACAC";
        }

        SaveCommand = new AsyncCommand(ExecuteSaveAsync);
        RestoreCommand = new AsyncCommand(ExecuteRestoreAsync);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void LoadFromSettings(AlignSettings s)
    {
        OpAssign    = s.EnabledOperators.Contains("=");
        OpPlusEq    = s.EnabledOperators.Contains("+=");
        OpMinusEq   = s.EnabledOperators.Contains("-=");
        OpMulEq     = s.EnabledOperators.Contains("*=");
        OpDivEq     = s.EnabledOperators.Contains("/=");
        OpModEq     = s.EnabledOperators.Contains("%=");
        OpAndEq     = s.EnabledOperators.Contains("&=");
        OpOrEq      = s.EnabledOperators.Contains("|=");
        OpXorEq     = s.EnabledOperators.Contains("^=");
        OpShlEq     = s.EnabledOperators.Contains("<<=");
        OpShrEq     = s.EnabledOperators.Contains(">>=");
        OpArrow     = s.EnabledOperators.Contains("=>");
        SpaceBefore = s.SpaceBeforeOperator;
        SpaceAfter  = s.SpaceAfterOperator;
    }

    private AlignSettings ToSettings()
    {
        var ops = new List<string>();
        if (OpAssign) ops.Add("=");
        if (OpPlusEq) ops.Add("+=");
        if (OpMinusEq) ops.Add("-=");
        if (OpMulEq) ops.Add("*=");
        if (OpDivEq) ops.Add("/=");
        if (OpModEq) ops.Add("%=");
        if (OpAndEq) ops.Add("&=");
        if (OpOrEq) ops.Add("|=");
        if (OpXorEq) ops.Add("^=");
        if (OpShlEq) ops.Add("<<=");
        if (OpShrEq) ops.Add(">>=");
        if (OpArrow) ops.Add("=>");

        return new AlignSettings
        {
            EnabledOperators = ops,
            SpaceBeforeOperator = SpaceBefore,
            SpaceAfterOperator = SpaceAfter,
        };
    }

    private Task ExecuteSaveAsync(object? parameter, CancellationToken ct)
    {
        _service.Save(ToSettings());
        return Task.CompletedTask;
    }

    private Task ExecuteRestoreAsync(object? parameter, CancellationToken ct)
    {
        LoadFromSettings(new AlignSettings());
        return Task.CompletedTask;
    }

    
    private static void SearchKeyRecursive(RegistryKey key, string path, StringBuilder sb,
        int depth, int maxDepth, Func<string, bool> filter)
    {
        if (depth > maxDepth) return;

        // Controlla i valori di questa chiave
        foreach (var valueName in key.GetValueNames())
        {
            if (filter(valueName) || filter(key.GetValue(valueName)?.ToString() ?? ""))
            {
                var val = key.GetValue(valueName);
                sb.AppendLine($"  {path}\\{valueName} = {val}");
            }
        }

        // Ricerca nei sotto-nomi
        foreach (var subName in key.GetSubKeyNames())
        {
            try
            {
                using var sub = key.OpenSubKey(subName);
                if (sub != null)
                    SearchKeyRecursive(sub, $"{path}\\{subName}", sb, depth + 1, maxDepth, filter);
            }
            catch { }
        }
    }
}