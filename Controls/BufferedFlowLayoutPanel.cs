namespace TichuWinForms_Smooth.Controls;

public sealed class BufferedFlowLayoutPanel : FlowLayoutPanel
{
    public BufferedFlowLayoutPanel()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
    }
}
