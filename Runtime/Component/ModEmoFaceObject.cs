﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Numeira;

[AddComponentMenu(ComponentMenuPrefix + "Face Object Marker")]
[RequireComponent(typeof(SkinnedMeshRenderer))]
internal sealed class ModEmoFaceObject : ModEmoTagComponent
{
    public SkinnedMeshRenderer Renderer => GetComponent<SkinnedMeshRenderer>();

    protected override void CalculateContentHash(ref HashCode hashCode)
    {
        hashCode.Add(Renderer);
    }
}
