namespace PinMap.Models;

public struct ChannelPoint
{
    public int Channel { get; }
    public double X { get; }
    public double Y { get; }

    public ChannelPoint(int channel, double x, double y)
    {
        Channel = channel;
        X = x;
        Y = y;
    }
}