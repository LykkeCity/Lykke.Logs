// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace  Lykke.Logs
{
    internal struct LykkeLogMessageEntry
    {
        public string LevelString;
        public ConsoleColor? MessageBackground;
        public ConsoleColor? MessageForeground;
        public string Message;
    }
}
