﻿using System;
using System.Threading.Tasks;

namespace Sma5hMusic.GUI.Interfaces
{
    public interface IBuildDialog
    {
        Task Init(Func<bool, Task> callbackSuccess = null, Func<Exception, Task> callbackError = null);
        Task Build(bool useCache, Func<bool, Task> callbackSuccess = null, Func<Exception, Task> callbackError = null);
    }
}
