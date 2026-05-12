using System.Globalization;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AnimationEditor.Core.ViewModels;

public sealed class TimelineFrameVm : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private void Notify([CallerMemberName] string p = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));

    public int Index { get; }
    public double Width { get; }

    public string IndexLabel => (Index + 1).ToString(CultureInfo.InvariantCulture);

    private bool _isCurrent;
    public bool IsCurrent
    {
        get => _isCurrent;
        set
        {
            if (_isCurrent != value)
            {
                _isCurrent = value;
                Notify();
            }
        }
    }

    public TimelineFrameVm(int index, double width)
    {
        Index = index;
        Width = width;
    }
}
