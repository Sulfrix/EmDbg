using EmDbg.Types;
using ImGuiNET;

namespace EmDbg.ImGuiUI;

public class ExceptionWindow
{
    public static ExceptionInfo? prevException;
    
    public static void OnException(ExceptionInfo exceptionInfo)
    {
        prevException = exceptionInfo;
        DisassemblyWindow.highlightState = DisassemblyWindow.InstructionHighlightState.EXCEPTION;
        DisassemblyWindow.highlightAddr = prevException.exceptAddress;
    }

    public static void ShowWindow()
    {
        if (prevException != null)
        {
            if (ImGui.Begin("Exception"))
            {
                ImGui.Text("An exception has occurred!");
                ImGui.Text("0x" + prevException.exceptAddress.ToString("X"));
                ImGui.Text("Type: 0x" + prevException.exceptType.ToString("X"));
                ImGui.Text("Thread: 0x" + prevException.thread.ToString("X"));

                if (ImGui.Button("Jump"))
                {
                    DisassemblyWindow.startAddress = (prevException.exceptAddress - 0x20);
                }
                ImGui.SameLine();
                if (ImGui.Button("Dismiss"))
                {
                    prevException = null;
                }
            }
        }
    }
}