using DokoLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace DokoTable;

public class DokoViewModel
{
    public Game Game { get; protected set; }

    public DokoViewModel()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Error()
            .WriteTo.Debug()
            .CreateLogger();

        Game = new Game("Player1", "Player2", "Player3", "Player4", SpecialRule.GetDefaults(), 100);
    }
}
