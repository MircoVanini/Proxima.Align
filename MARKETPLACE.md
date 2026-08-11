# Proxima Align Assignments

![Proxima Align Assignments](https://raw.githubusercontent.com/MircoVanini/Proxima.Align/develop/docs/images/marketplace-banner.png)

Make related assignments easier to scan by aligning their operators with one
command.

## Highlights

- Align assignment and compound-assignment operators in a selected code block.
- Correctly calculate visual columns when indentation contains tabs.
- Leave strings, character literals, and comments untouched.
- Configure enabled operators and spacing from a themed Visual Studio dialog.
- Use the menu command or the **Ctrl+Alt+\\** shortcut.

## Supported operators

`=`, `+=`, `-=`, `*=`, `/=`, `%=`, `&=`, `|=`, `^=`, `<<=`, `>>=`, `=>`

## Example

Before:

```csharp
var id = 1;
var displayName = "Proxima";
var isEnabled = true;
```

After:

```csharp
var id          = 1;
var displayName = "Proxima";
var isEnabled   = true;
```

## Compatibility

Requires Visual Studio 17.14 or later on Windows. Supports x64 and Arm64.

## Source and support

Source code and issue tracking are available at
[github.com/MircoVanini/Proxima.Align](https://github.com/MircoVanini/Proxima.Align).
