using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DC.Authorization
{
    public interface IActiveHook
    {
        bool IsActive { get; }
        void Reset();
        void Start();
    }
}
