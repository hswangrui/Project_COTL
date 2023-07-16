// Copyright 2014-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.


namespace BlendModes
{
    /// <summary>
    /// Represents the way in which <see cref="BlendModeEffect"/> is applied to the affected object.
    /// </summary>
    public enum RenderMode
    {
        /// <summary>
        /// Blend object's texture with anything that was drawn before the object on screen.
        /// </summary>
        SelfWithScreen,
        /// <summary>
        /// Blend specified overlay color and texture with the object's texture.
        /// </summary>
        TextureWithSelf
    }
}
