echo off
set programfiles=%PROGRAMFILES(X86)%
md "..\bin\Plugins\CSScriptNpp"
md "..\bin\Plugins\CSScriptNpp\Mdbg"
md "..\bin\Plugins\CSScriptNpp\Roslyn"
md "%programfiles%\Notepad++\plugins\CSScriptNpp"
md "%programfiles%\Notepad++\plugins\CSScriptNpp\Mdbg"
md "%programfiles%\Notepad++\plugins\CSScriptNpp\Roslyn"

rem CSScriptIntellisense.dll cannot be copied from build events as it would copy the assembly before DllExport is performed
rem so it needs to be done manually.

set plugins=%programfiles%\Notepad++\plugins

copy "CSScriptIntellisense\bin\Release\CSScriptIntellisense.dll" "%plugins%\CSScriptNpp\CSScriptIntellisense.dll"
copy "CSScriptIntellisense\bin\Release\CSharpIntellisense\*.dll" "%plugins%\CSScriptNpp"

copy "CSScriptNpp\bin\Release\CSScriptNpp.dll" "%plugins%\CSScriptNpp.dll"
copy "CSScriptNpp\bin\Release\CSScriptNpp\*.exe" "%plugins%\CSScriptNpp"
copy "CSScriptNpp\bin\release\CSScriptNpp\*.pdb" "%plugins%\CSScriptNpp"
copy "CSScriptNpp\CSScriptNpp\Updater.exe" "%plugins%\CSScriptNpp\Updater.exe"
copy "CSScriptNpp\CSScriptNpp\npp_jit.exe" "%plugins%\CSScriptNpp\npp_jit.exe"
copy "CSScriptNpp\CSScriptNpp\7z.exe" "%plugins%\CSScriptNpp\7z.exe"
copy "CSScriptNpp\CSScriptNpp\7z.dll" "%plugins%\CSScriptNpp\7z.dll"
copy "CSScriptNpp\CSScriptNpp\Mdbg\mdbghost*.exe" "%plugins%\CSScriptNpp\Mdbg"
copy "CSScriptNpp\bin\Release\CSScriptNpp\Mdbg\*.dll" "%plugins%\CSScriptNpp\Mdbg"
copy "CSScriptNpp\bin\Release\CSScriptNpp\Mdbg\*.exe" "%plugins%\CSScriptNpp\Mdbg"

copy "CSScriptNpp\CSScriptNpp\roslyn\csc.exe" "%plugins%\CSScriptNpp\Roslyn\csc.exe"
copy "CSScriptNpp\CSScriptNpp\roslyn\CSSCodeProvider.v4.6.dll" "%plugins%\CSScriptNpp\Roslyn\CSSCodeProvider.v4.6.dll"
copy "CSScriptNpp\CSScriptNpp\roslyn\Formatter.exe" "%plugins%\CSScriptNpp\Roslyn\Formatter.exe"

copy "CSScriptNpp\CSScriptNpp\roslyn\Microsoft.Build.Tasks.CodeAnalysis.dll" "%plugins%\CSScriptNpp\Roslyn\Microsoft.Build.Tasks.CodeAnalysis.dll"
copy "CSScriptNpp\CSScriptNpp\roslyn\Microsoft.CodeAnalysis.CSharp.Workspaces.dll" "%plugins%\CSScriptNpp\Roslyn\Microsoft.CodeAnalysis.CSharp.Workspaces.dll"
copy "CSScriptNpp\CSScriptNpp\roslyn\Microsoft.CodeAnalysis.VisualBasic.dll" "%plugins%\CSScriptNpp\Roslyn\Microsoft.CodeAnalysis.VisualBasic.dll"
copy "CSScriptNpp\CSScriptNpp\roslyn\Microsoft.CodeAnalysis.VisualBasic.Workspaces.dll" "%plugins%\CSScriptNpp\Roslyn\Microsoft.CodeAnalysis.VisualBasic.Workspaces.dll"
copy "CSScriptNpp\CSScriptNpp\roslyn\Microsoft.CodeAnalysis.Workspaces.Desktop.dll" "%plugins%\CSScriptNpp\Roslyn\Microsoft.CodeAnalysis.Workspaces.Desktop.dll"
copy "CSScriptNpp\CSScriptNpp\roslyn\Microsoft.CodeAnalysis.Workspaces.dll" "%plugins%\CSScriptNpp\Roslyn\Microsoft.CodeAnalysis.Workspaces.dll"
copy "CSScriptNpp\CSScriptNpp\roslyn\System.Composition.AttributedModel.dll" "%plugins%\CSScriptNpp\Roslyn\System.Composition.AttributedModel.dll"
copy "CSScriptNpp\CSScriptNpp\roslyn\System.Composition.Convention.dll" "%plugins%\CSScriptNpp\Roslyn\System.Composition.Convention.dll"
copy "CSScriptNpp\CSScriptNpp\roslyn\System.Composition.Hosting.dll" "%plugins%\CSScriptNpp\Roslyn\System.Composition.Hosting.dll"
copy "CSScriptNpp\CSScriptNpp\roslyn\System.Composition.Runtime.dll" "%plugins%\CSScriptNpp\Roslyn\System.Composition.Runtime.dll"
copy "CSScriptNpp\CSScriptNpp\roslyn\System.Composition.TypedParts.dll" "%plugins%\CSScriptNpp\Roslyn\System.Composition.TypedParts.dll"

copy "CSScriptNpp\CSScriptNpp\roslyn\Microsoft.CodeAnalysis.CSharp.dll" "%plugins%\CSScriptNpp\Roslyn\Microsoft.CodeAnalysis.CSharp.dll"
copy "CSScriptNpp\CSScriptNpp\roslyn\Microsoft.CodeAnalysis.dll" "%plugins%\CSScriptNpp\Roslyn\Microsoft.CodeAnalysis.dll"
copy "CSScriptNpp\CSScriptNpp\roslyn\Microsoft.CodeDom.Providers.DotNetCompilerPlatform.dll" "%plugins%\CSScriptNpp\Roslyn\Microsoft.CodeDom.Providers.DotNetCompilerPlatform.dll"
copy "CSScriptNpp\CSScriptNpp\roslyn\System.Collections.Immutable.dll" "%plugins%\CSScriptNpp\Roslyn\System.Collections.Immutable.dll"
copy "CSScriptNpp\CSScriptNpp\roslyn\System.Reflection.Metadata.dll" "%plugins%\CSScriptNpp\Roslyn\System.Reflection.Metadata.dll"

set bin=..\bin\Plugins

copy "CSScriptIntellisense\bin\Release\CSScriptIntellisense.dll"  "%bin%\CSScriptNpp"
copy "CSScriptIntellisense\bin\Release\CSharpIntellisense\*.dll" "%bin%\CSScriptNpp"

copy "CSScriptNpp\bin\release\CSScriptNpp.dll" "%bin%\CSScriptNpp.dll"
copy "CSScriptNpp\bin\release\CSScriptNpp\*.exe" "%bin%\CSScriptNpp"
copy "CSScriptNpp\bin\release\CSScriptNpp\*.pdb" "%bin%\CSScriptNpp"
copy "CSScriptNpp\bin\Release\CSScriptNpp\Mdbg\*.dll" "%bin%\CSScriptNpp\Mdbg"
copy "CSScriptNpp\bin\Release\CSScriptNpp\Mdbg\*.exe" "%bin%\CSScriptNpp\Mdbg"
rem copy "CSScriptNpp\CSScriptNpp\roslyn\*.*" "%bin%\CSScriptNpp\Roslyn\"

copy "CSScriptNpp\CSScriptNpp\roslyn\csc.exe" "%bin%\CSScriptNpp\Roslyn\csc.exe"
copy "CSScriptNpp\CSScriptNpp\roslyn\CSSCodeProvider.v4.6.dll" "%bin%\CSScriptNpp\Roslyn\CSSCodeProvider.v4.6.dll"
copy "CSScriptNpp\CSScriptNpp\roslyn\Formatter.exe" "%bin%\CSScriptNpp\Roslyn\Formatter.exe"

copy "CSScriptNpp\CSScriptNpp\roslyn\Microsoft.Build.Tasks.CodeAnalysis.dll" "%bin%\CSScriptNpp\Roslyn\Microsoft.Build.Tasks.CodeAnalysis.dll"
copy "CSScriptNpp\CSScriptNpp\roslyn\Microsoft.CodeAnalysis.CSharp.Workspaces.dll" "%bin%\CSScriptNpp\Roslyn\Microsoft.CodeAnalysis.CSharp.Workspaces.dll"
copy "CSScriptNpp\CSScriptNpp\roslyn\Microsoft.CodeAnalysis.VisualBasic.dll" "%bin%\CSScriptNpp\Roslyn\Microsoft.CodeAnalysis.VisualBasic.dll"
copy "CSScriptNpp\CSScriptNpp\roslyn\Microsoft.CodeAnalysis.VisualBasic.Workspaces.dll" "%bin%\CSScriptNpp\Roslyn\Microsoft.CodeAnalysis.VisualBasic.Workspaces.dll"
copy "CSScriptNpp\CSScriptNpp\roslyn\Microsoft.CodeAnalysis.Workspaces.Desktop.dll" "%bin%\CSScriptNpp\Roslyn\Microsoft.CodeAnalysis.Workspaces.Desktop.dll"
copy "CSScriptNpp\CSScriptNpp\roslyn\Microsoft.CodeAnalysis.Workspaces.dll" "%bin%\CSScriptNpp\Roslyn\Microsoft.CodeAnalysis.Workspaces.dll"
copy "CSScriptNpp\CSScriptNpp\roslyn\System.Composition.AttributedModel.dll" "%bin%\CSScriptNpp\Roslyn\System.Composition.AttributedModel.dll"
copy "CSScriptNpp\CSScriptNpp\roslyn\System.Composition.Convention.dll" "%bin%\CSScriptNpp\Roslyn\System.Composition.Convention.dll"
copy "CSScriptNpp\CSScriptNpp\roslyn\System.Composition.Hosting.dll" "%bin%\CSScriptNpp\Roslyn\System.Composition.Hosting.dll"
copy "CSScriptNpp\CSScriptNpp\roslyn\System.Composition.Runtime.dll" "%bin%\CSScriptNpp\Roslyn\System.Composition.Runtime.dll"
copy "CSScriptNpp\CSScriptNpp\roslyn\System.Composition.TypedParts.dll" "%bin%\CSScriptNpp\Roslyn\System.Composition.TypedParts.dll"

copy "CSScriptNpp\CSScriptNpp\roslyn\Microsoft.CodeAnalysis.CSharp.dll" "%bin%\CSScriptNpp\Roslyn\Microsoft.CodeAnalysis.CSharp.dll"
copy "CSScriptNpp\CSScriptNpp\roslyn\Microsoft.CodeAnalysis.dll" "%bin%\CSScriptNpp\Roslyn\Microsoft.CodeAnalysis.dll"
copy "CSScriptNpp\CSScriptNpp\roslyn\Microsoft.CodeDom.Providers.DotNetCompilerPlatform.dll" "%bin%\CSScriptNpp\Roslyn\Microsoft.CodeDom.Providers.DotNetCompilerPlatform.dll"
copy "CSScriptNpp\CSScriptNpp\roslyn\System.Collections.Immutable.dll" "%bin%\CSScriptNpp\Roslyn\System.Collections.Immutable.dll"
copy "CSScriptNpp\CSScriptNpp\roslyn\System.Reflection.Metadata.dll" "%bin%\CSScriptNpp\Roslyn\System.Reflection.Metadata.dll"


copy "CSScriptNpp\CSScriptNpp\Mdbg\mdbghost*.exe" "%bin%\CSScriptNpp\Mdbg"
copy "CSScriptNpp\CSScriptNpp\Mdbg\mdbghost*.exe" "%plugins%\CSScriptNpp\Mdbg"
copy "CSScriptNpp\CSScriptNpp\Updater.exe" "%bin%\CSScriptNpp\Updater.exe"
copy "CSScriptNpp\CSScriptNpp\npp_jit.exe" "%bin%\CSScriptNpp\npp_jit.exe"
copy "CSScriptNpp\CSScriptNpp\7z.exe" "%bin%\CSScriptNpp\7z.exe"
copy "CSScriptNpp\CSScriptNpp\7z.dll" "%bin%\CSScriptNpp\7z.dll"

copy "..\readme.txt" "..\bin\readme.txt"
copy "..\license.txt" "..\bin\license.txt"

echo ----------------------------
rem need to keep it last so copy errors (if any) are visible
copy "CSScriptNpp\bin\Release\CSScriptNpp.dll" "%plugins%\CSScriptNpp.dll"

pause