using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoMusicCenter.Utilities
{
    public interface ICloseable
    {
        event EventHandler? CloseRequest;
    }
}
