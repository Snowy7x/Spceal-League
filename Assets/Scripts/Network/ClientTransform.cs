﻿using Unity.Netcode.Components;
using UnityEngine;

namespace Network
{
    [DisallowMultipleComponent]
    public class ClientTransform : NetworkTransform
    {
        protected override bool OnIsServerAuthoritative()
        {
            return false;
        }
    }
}