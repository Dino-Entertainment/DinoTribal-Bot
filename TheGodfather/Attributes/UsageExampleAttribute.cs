﻿#region USING_DIRECTIVES
using System;
#endregion

namespace TheGodfather.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class UsageExampleAttribute : Attribute
    {
        public string Example { get; private set; }


        public UsageExampleAttribute(string example)
        {
            Example = example;
        }
    }
}
