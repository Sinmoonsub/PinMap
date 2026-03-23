using System.Collections.Generic;
using System.Windows.Media;
using PinMap.Models;

namespace PinMap
{
    public interface IPointDisplayControl
    {
        IEnumerable<ChannelPoint> Points { get; set; }
        Dictionary<int, Brush> ChannelColors { get; set; }
        void ResetView();
    }
}
