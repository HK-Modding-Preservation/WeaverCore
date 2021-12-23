﻿using System;

namespace WeaverCore.Attributes
{
    /// <summary>
    /// Called before WeaverCore begins its build process. Can be attached to a static method
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class BeforeBuildAttribute : PriorityAttribute
    {
        public BeforeBuildAttribute(int priority = 0) : base(priority) { }
    }
}
