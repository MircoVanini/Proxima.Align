# Proxima Align Assignments

[![GitHub](https://img.shields.io/badge/GitHub-MircoVanini%2FProxima.Align-blue?logo=github)](https://github.com/MircoVanini/Proxima.Align)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE.txt)
[![Visual Studio Marketplace Version](https://img.shields.io/badge/Marketplace-Proxima%20Align%20Assignments-brightgreen)](https://marketplace.visualstudio.com)

## Overview

**Proxima Align Assignments** is a Visual Studio extension that automatically aligns assignment operators in your selected code blocks, improving code readability and consistency.

### Key Features

- **Automatic Alignment**: Align multiple assignment operators in selected code with a single command
- **Supported Operators**: Supports all major assignment operators:
  - Basic: `=`
  - Arithmetic: `+=`, `-=`, `*=`, `/=`, `%=`
  - Bitwise: `&=`, `|=`, `^=`, `<<=`, `>>=`
  - Lambda: `=>`
- **Smart Formatting**: Preserves code structure while improving visual alignment
- **Productivity Focus**: Enhances code readability and consistency across your codebase
- **Seamless Integration**: Works directly within Visual Studio

## Installation

1. Open Visual Studio Extension Manager (__Tools > Extensions and Updates__)
2. Search for "Proxima Align Assignments"
3. Click **Download** and follow the installation prompts
4. Restart Visual Studio when prompted

Alternatively, install from the [Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=MircoVanini.ProximaAlignAssignments)

## Usage

### Basic Workflow

1. Select the code block containing assignment operators you want to align
2. Access the alignment command through the Visual Studio menu or context menu
3. The extension automatically aligns all assignment operators in your selection
4. Enjoy improved code readability!

### Example


**Before:**
```
var x = 10; 
var longVariableName = 20; 
var y = 30; 
var anotherLongName = 40;
```

**After:**
```
var x                  = 10; 
var longVariableName   = 20; 
var y                  = 30; 
var anotherLongName    = 40;
```

## Supported Visual Studio Versions

- Visual Studio 2026 (All editions)

## Settings & Configuration

The extension provides configurable settings through the **__Extensions > Proxima Align__ > Align Assignment-Settings... ** menu for:

- Alignment patterns and behavior
- Operator spacing preferences

## Support & Documentation

- **GitHub Repository**: [MircoVanini/Proxima.Align](https://github.com/MircoVanini/Proxima.Align)
- **Issue Tracker**: [Report Issues](https://github.com/MircoVanini/Proxima.Align/issues)
- **Release Notes**: See [RELEASE-NOTES.txt](RELEASE-NOTES.txt) for version history

## License

This extension is distributed under the MIT License. See [LICENSE.txt](LICENSE.txt) for details.

## Publisher

**Mirco Vanini**

---

## FAQ

**Q: Does this extension support custom operators?**  
A: Currently, the extension supports standard C# assignment operators. Custom support may be added in future releases.

**Q: Will this affect my code logic?**  
A: No. The extension only modifies whitespace and does not change any code logic or functionality.

**Q: Can I undo the alignment?**  
A: Yes! Simply use Visual Studio's standard __Edit > Undo__ command (__Ctrl+Z__) if needed.

---

## Feedback & Contributing

We welcome feedback and contributions! Please visit our [GitHub repository](https://github.com/MircoVanini/Proxima.Align) to:
- Report bugs or request features
- Contribute code improvements
- Participate in discussions

---

**Enjoy more consistent, readable code with Proxima Align Assignments!**