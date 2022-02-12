using DokoLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DokoTable;

internal class DokoViewModel
{
    public ObservableCollection<Card> Cards { get; protected set; }

    public DokoViewModel()
    {
        Cards = new ObservableCollection<Card>(Card.GetDeckOfCards());
    }
}
