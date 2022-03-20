using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DokoTable.ViewModels;

public interface IViewModel : INotifyPropertyChanged
{
    bool IsSynchronized { get; }
    void Invoke(Action action);
    void BeginInvoke(Action action);
}
