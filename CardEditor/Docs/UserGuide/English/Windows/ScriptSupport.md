# Script Support

Script Support is a reference library designed to help users access and explore Card Information, Card Scripts, Scrapi Books, and other resources related to the Yu-Gi-Oh! Card Game Engine.

For project setup and build instructions, see the [Script Support README](../../../../../ScriptSupport/README.md).

For a complete guide to its features and configuration, see the [Script Support User Guide](../../../../../ScriptSupport/Docs/UserGuide/English/GettingStarted.md).

## Overview

![Script Support](../../../Images/English/Windows/ScriptSupport/ScriptSupport.png)


Script Support supports two operating modes:

- Standalone Mode: Run Script Support as an independent application.
- Embedded Mode: Access Script Support as an integrated module within CardEditorX.

Both modes provide access to the same reference resources and functionality, allowing users to choose the operating environment that best fits their workflow.

## Embedded Mode

Script Support can be accessed directly from CardEditorX in Embedded Mode.

To open Script Support, navigate to:

```
Menu → Window → Script Support
```

Script Support will appear as a floating pop-up within CardEditor's MainWindow, allowing users to access its features without leaving the main application.

## Troubleshooting

CardEditorX and Script Support are built on different .NET runtimes:

- CardEditorX uses .NET Framework 4.8.1.
- Script Support uses .NET 8.

When Script Support runs in Embedded Mode, integrating components built on these different runtimes may theoretically introduce UI interaction issues.

Potential issues may include:

- Focus Management: Unexpected focus behavior when switching between Script Support and CardEditor's MainWindow.

- Keyboard Input: Keyboard events may not be handled or routed as expected.

- Mouse Input: Mouse interactions may behave unexpectedly within the embedded interface.

- Window Interaction: Unexpected behavior when activating or interacting with the embedded window.

These potential issues have been carefully addressed during development and testing. No such issues have been observed in the current implementation.

If you encounter unexpected behavior while using Embedded Mode, please report the issue with the steps required to reproduce it.

---

[← Previous: CodeEditor](./CodeEditor.md)

[← Getting Started](../GettingStarted.md)
