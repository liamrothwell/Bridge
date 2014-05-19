﻿using System;

namespace Bridge.CLR
{
    /// <summary>
    /// 
    /// </summary>
    [Bridge.CLR.Ignore]
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Struct | AttributeTargets.Interface)]
    public sealed class ModuleAttribute : Attribute
    {
        public ModuleAttribute()
        {
        }

        public ModuleAttribute(string moduleName) 
        { 
        }
    }
}