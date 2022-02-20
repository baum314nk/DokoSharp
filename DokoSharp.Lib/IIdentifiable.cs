using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DokoSharp.Lib;

/// <summary>
/// An interface for uniquely identifiable objects.
/// </summary>
public interface IIdentifiable
{
    string Identifier { get; }
}
