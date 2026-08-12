using Microsoft.VisualStudio.Extensibility.UI;
using System.Runtime.Serialization;
using System.Security;
using System.Text.Json;

namespace Proxima.Align;

/// <summary>
/// View model for the alignment settings tool window, providing data-bindable properties
/// for operator alignment configuration, spacing options, theme colors, and commands.
/// </summary>
/// <remarks>
/// This view model manages the state of alignment settings including:
/// - Individual toggles for supported operators (=, +=, -=, *=, /=, %=, &=, |=, ^=, <<=, >>=, =>)
/// - Spacing configuration before and after operators
/// - Theme-aware color properties for UI rendering
/// - Save and Restore command implementations
/// The view model is serializable via data contract and implements property change notifications
/// for data binding in the Visual Studio Extensibility UI framework.
/// </remarks>
[DataContract]
internal sealed class AlignSettingsViewModel : NotifyPropertyChangedObject
{
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

    [DataMember] public bool EnableLog  { get => _enableLog;    set => SetProperty(ref _enableLog, value); }
    [DataMember] public bool OpAssign   { get => _opAssign;     set => SetProperty(ref _opAssign, value); }
    [DataMember] public bool OpPlusEq   { get => _opPlusEq;     set => SetProperty(ref _opPlusEq, value); }
    [DataMember] public bool OpMinusEq  { get => _opMinusEq;    set => SetProperty(ref _opMinusEq, value); }
    [DataMember] public bool OpMulEq    { get => _opMulEq;      set => SetProperty(ref _opMulEq, value); }
    [DataMember] public bool OpDivEq    { get => _opDivEq;      set => SetProperty(ref _opDivEq, value); }
    [DataMember] public bool OpModEq    { get => _opModEq;      set => SetProperty(ref _opModEq, value); }
    [DataMember] public bool OpAndEq    { get => _opAndEq;      set => SetProperty(ref _opAndEq, value); }
    [DataMember] public bool OpOrEq     { get => _opOrEq;       set => SetProperty(ref _opOrEq, value); }
    [DataMember] public bool OpXorEq    { get => _opXorEq;      set => SetProperty(ref _opXorEq, value); }
    [DataMember] public bool OpShlEq    { get => _opShlEq;      set => SetProperty(ref _opShlEq, value); }
    [DataMember] public bool OpShrEq    { get => _opShrEq;      set => SetProperty(ref _opShrEq, value); }
    [DataMember] public bool OpArrow    { get => _opArrow;      set => SetProperty(ref _opArrow, value); }

    private bool _spaceBefore;
    private bool _spaceAfter;
    private string _saveStatusMessage = string.Empty;
    private bool _hasSaveStatus;

    [DataMember] public bool SpaceBefore { get => _spaceBefore; set => SetProperty(ref _spaceBefore, value); }
    [DataMember] public bool SpaceAfter { get => _spaceAfter; set => SetProperty(ref _spaceAfter, value); }
    [DataMember] public string SaveStatusMessage { get => _saveStatusMessage; private set => SetProperty(ref _saveStatusMessage, value); }
    [DataMember] public bool HasSaveStatus { get => _hasSaveStatus; private set => SetProperty(ref _hasSaveStatus, value); }

    [DataMember] public AsyncCommand SaveCommand { get; }
    [DataMember] public AsyncCommand RestoreCommand { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AlignSettingsViewModel"/> class.
    /// </summary>
    /// <param name="service">The alignment settings service used to load and save configuration.</param>
    public AlignSettingsViewModel(AlignSettingsService service)
    {
        _service = service;
        LoadFromSettings(_service.Current);

        SaveCommand = new AsyncCommand(ExecuteSaveAsync);
        RestoreCommand = new AsyncCommand(ExecuteRestoreAsync);
    }

    /// <summary>
    /// Loads operator and spacing settings from an <see cref="AlignSettings"/> instance
    /// into the view model properties.
    /// </summary>
    /// <param name="s">The settings object to load from.</param>
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
        EnableLog   = s.EnableLog;
    }

    /// <summary>
    /// Converts the current view model state into an <see cref="AlignSettings"/> instance.
    /// </summary>
    /// <returns>A new <see cref="AlignSettings"/> object containing the current operator and spacing configuration.</returns>
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

        return _service.Current.WithAlignmentPreferences(ops, SpaceBefore, SpaceAfter, EnableLog);
    }

    /// <summary>
    /// Executes the save command, persisting the current view model settings to storage.
    /// </summary>
    /// <param name="parameter">Optional command parameter (unused).</param>
    /// <param name="ct">Cancellation token for async operation.</param>
    /// <returns>A completed task.</returns>
    private Task ExecuteSaveAsync(object? parameter, CancellationToken ct)
    {
        try
        {
            _service.Save(ToSettings());
            SetSaveStatus("Settings saved.");
        }
        catch (IOException ex)
        {
            SetSaveStatus($"Unable to save settings: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            SetSaveStatus($"Unable to save settings: {ex.Message}");
        }
        catch (JsonException ex)
        {
            SetSaveStatus($"Unable to serialize settings: {ex.Message}");
        }
        catch (NotSupportedException ex)
        {
            SetSaveStatus($"Unable to save settings: {ex.Message}");
        }
        catch (SecurityException ex)
        {
            SetSaveStatus($"Unable to save settings: {ex.Message}");
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Executes the restore command, resetting all settings to their default values.
    /// </summary>
    /// <param name="parameter">Optional command parameter (unused).</param>
    /// <param name="ct">Cancellation token for async operation.</param>
    /// <returns>A completed task.</returns>
    private Task ExecuteRestoreAsync(object? parameter, CancellationToken ct)
    {
        LoadFromSettings(new AlignSettings());

        SaveStatusMessage = string.Empty;
        HasSaveStatus     = false;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Sets the save status message and updates the HasSaveStatus flag to indicate that a status message is present.
    /// </summary>
    /// <param name="message">The message to display as the save status.</param>
    private void SetSaveStatus(string message)
    {
        SaveStatusMessage = message;
        HasSaveStatus = true;
    }
}