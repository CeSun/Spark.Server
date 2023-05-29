using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frame.Interfaces;

public interface ILifeCycle
{
    void OnStart();
    void OnStop();

}
