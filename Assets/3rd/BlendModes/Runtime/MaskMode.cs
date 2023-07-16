// Copyright 2014-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.


namespace BlendModes
{
    /// <summary>
    /// Represents the way in which <see cref="BlendModeEffect"/> is applied when interacting with the masked areas.
    /// </summary>
    public enum MaskMode
    {
        /// <summary>
        /// Apply blend effect over the entire object (ignore masks).
        /// </summary>
        Disabled,
        /// <summary>
        /// Apply blend effect over the areas affected by mask.
        /// </summary>
        NothingButMask,
        /// <summary>
        /// Apply blend effect over the areas not affected by mask.
        /// </summary>
        EverythingButMask
    }
}
