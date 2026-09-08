using System.Numerics;
using ImGuiNET;

namespace EmDbg.ImGuiUI;

public class DisassemblyWindow
{
    public static uint startAddress = 0;

    public enum InstructionHighlightState
    {
        NONE,
        EXCEPTION,
        BREAK
    }
    public static InstructionHighlightState highlightState = InstructionHighlightState.NONE;
    public static uint highlightAddr = 0;

    private static string UIJumpAddr = "80000000";

    public static void ShowWindow()
    {
        if (ImGui.Begin("Disassembly", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.MenuBar))
        {
            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("Jump"))
                {
                    ImGui.InputText("Address", ref UIJumpAddr, 8);
                    if (ImGui.Button("Jump"))
                    {
                        try
                        {
                            startAddress = Convert.ToUInt32(UIJumpAddr, 16);
                        } catch (Exception e) {}
                    }
                    ImGui.EndMenu();
                }
                ImGui.EndMenuBar();
            }
            if (ImGui.GetIO().MouseWheel != 0 && ImGui.IsWindowHovered())
            {
                var diff = (int)(ImGui.GetIO().MouseWheel * 4);
                // hack: just do it with signed longs lmao
                var newScroll = (long)startAddress;
                newScroll -= diff;
                if (newScroll < 0)
                {
                    newScroll = 0;
                }
                startAddress = (uint)newScroll;
            }
            //ImGui.InputInt("Start Address", ref startAddress);
            var drawlist = ImGui.GetWindowDrawList();

            if (ImGui.BeginTable("Instructions", 3, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg))
            {
                ImGui.TableSetupColumn("Actions", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("Address", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("Instruction", ImGuiTableColumnFlags.WidthStretch);

                var contentRegionAvail = ImGui.GetContentRegionAvail();
                for (uint i = 0; ImGui.GetCursorPosY() < contentRegionAvail.Y+12; i++)
                {
                    var addr = startAddress + i * 4;
                    var data = MemoryAccess.GetMemory((uint)addr, 4);
                    
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    var start = ImGui.GetCursorScreenPos();
                    if (addr == highlightAddr && highlightState != InstructionHighlightState.NONE)
                    {
                        uint color = 0x5000ffff;
                        if (highlightState == InstructionHighlightState.EXCEPTION)
                        {
                            color = 0x500000ff;
                        }
                        drawlist.AddRectFilled(new Vector2(start.X, start.Y), new Vector2(contentRegionAvail.X+start.X-10, start.Y+ImGui.GetTextLineHeight()), color);
                    }

                    if (ImGui.InvisibleButton("Breakpoint", new Vector2(ImGui.GetTextLineHeight())))
                    {
                        
                    }
                    ImGui.TableSetColumnIndex(1);
                    ImGui.Text(addr.ToString("X8"));
                    ImGui.TableSetColumnIndex(2);
                    if (data != null)
                    {
                        ImGui.Text(PPCDisassembler.DisassembleToStrings(data, 0, true)[0]);
                    }
                    else
                    {
                        var block = MemoryAccess.GetBlocksForAddressRange((uint)addr, (uint)addr)[0];
                        if (block.status == MemoryAccess.MemoryBlock.Status.REQUESTED)
                        {
                            ImGui.Text("Loading...");
                        }

                        if (block.status == MemoryAccess.MemoryBlock.Status.ERROR)
                        {
                            ImGui.Text("Error");
                        }
                    }
                }
                
                ImGui.EndTable();
            }
        }
        ImGui.End();
    }
}