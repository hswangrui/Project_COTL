// Copyright 2014-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.


namespace BlendModes
{
    /// <summary>
    /// Represents what should happen with the pixels affected by the mask.
    /// </summary>
    public enum MaskBehaviour
    {
        /// <summary>
        /// Pixels over the affected (masked) areas will be discarded.
        /// </summary>
        Cutout,
        /// <summary>
        /// Pixels over the affected (masked) areas will be rendered using default material pass (normal blend mode).
        /// </summary>
        Normal
    }
}
