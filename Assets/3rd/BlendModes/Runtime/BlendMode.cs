// Copyright 2014-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.


namespace BlendModes
{
    /// <summary>
    /// Represents an algorithm to be used when blending render layers.
    /// </summary>
    public enum BlendMode
    {
        /// <summary>
        /// Looks at the color information in each channel and selects the base or blend color—whichever is darker—as the result color. Pixels lighter than the blend color are replaced, and pixels darker than the blend color do not change. 
        /// </summary>
        Darken,
        /// <summary>
        /// Looks at the color information in each channel and multiplies the base color by the blend color. The result color is always a darker color. Multiplying any color with black produces black. Multiplying any color with white leaves the color unchanged.
        /// </summary>
        Multiply,
        /// <summary>
        /// Looks at the color information in each channel and darkens the base color to reflect the blend color by increasing the contrast between the two. Blending with white produces no change. The effect is similar to drawing on the image with multiple marking pens. 
        /// </summary>
        ColorBurn,
        /// <summary>
        /// Looks at the color information in each channel and darkens the base color to reflect the blend color by decreasing the brightness. Blending with white produces no change.
        /// </summary>
        LinearBurn,
        /// <summary>
        /// Compares the total of all channel values for the blend and base color and displays the lower value color. Darker Color does not produce a third color, which can result from the Darken blend, because it chooses the lowest channel values from both the base and the blend color to create the result color.
        /// </summary>
        DarkerColor,
        /// <summary>
        /// Looks at the color information in each channel and selects the base or blend color—whichever is lighter—as the result color. Pixels darker than the blend color are replaced, and pixels lighter than the blend color do not change. 
        /// </summary>
        Lighten,
        /// <summary>
        /// Looks at each channel’s color information and multiplies the inverse of the blend and base colors. The result color is always a lighter color. Screening with black leaves the color unchanged. Screening with white produces white. The effect is similar to projecting multiple photographic slides on top of each other. 
        /// </summary>
        Screen,
        /// <summary>
        /// Looks at the color information in each channel and brightens the base color to reflect the blend color by decreasing contrast between the two. Blending with black produces no change.
        /// </summary>
        ColorDodge,
        /// <summary>
        /// Looks at the color information in each channel and brightens the base color to reflect the blend color by increasing the brightness. Blending with black produces no change.
        /// </summary>
        LinearDodge,
        /// <summary>
        /// Compares the total of all channel values for the blend and base color and displays the higher value color. Lighter Color does not produce a third color, which can result from the Lighten blend, because it chooses the highest channel values from both the base and blend color to create the result color. 
        /// </summary>
        LighterColor,
        /// <summary>
        /// Multiplies or screens the colors, depending on the base color. Patterns or colors overlay the existing pixels while preserving the highlights and shadows of the base color. The base color is not replaced, but mixed with the blend color to reflect the lightness or darkness of the original color.
        /// </summary>
        Overlay,
        /// <summary>
        /// Darkens or lightens the colors, depending on the blend color. The effect is similar to shining a diffused spotlight on the image. If the blend color (light source) is lighter than 50% gray, the image is lightened as if it were dodged. If the blend color is darker than 50% gray, the image is darkened as if it were burned in.
        /// </summary>
        SoftLight,
        /// <summary>
        /// Multiplies or screens the colors, depending on the blend color. The effect is similar to shining a harsh spotlight on the image. If the blend color (light source) is lighter than 50% gray, the image is lightened, as if it were screened. This is useful for adding highlights to an image. If the blend color is darker than 50% gray, the image is darkened, as if it were multiplied. This is useful for adding shadows to an image. Painting with pure black or white results in pure black or white.
        /// </summary>
        HardLight,
        /// <summary>
        /// Burns or dodges the colors by increasing or decreasing the contrast, depending on the blend color. If the blend color (light source) is lighter than 50% gray, the image is lightened by decreasing the contrast. If the blend color is darker than 50% gray, the image is darkened by increasing the contrast. 
        /// </summary>
        VividLight,
        /// <summary>
        /// Burns or dodges the colors by decreasing or increasing the brightness, depending on the blend color. If the blend color (light source) is lighter than 50% gray, the image is lightened by increasing the brightness. If the blend color is darker than 50% gray, the image is darkened by decreasing the brightness. 
        /// </summary>
        LinearLight,
        /// <summary>
        /// Replaces the colors, depending on the blend color. If the blend color (light source) is lighter than 50% gray, pixels darker than the blend color are replaced, and pixels lighter than the blend color do not change. If the blend color is darker than 50% gray, pixels lighter than the blend color are replaced, and pixels darker than the blend color do not change. This is useful for adding special effects to an image.
        /// </summary>
        PinLight,
        /// <summary>
        /// Adds the red, green and blue channel values of the blend color to the RGB values of the base color. If the resulting sum for a channel is 255 or greater, it receives a value of 255; if less than 255, a value of 0. Therefore, all blended pixels have red, green, and blue channel values of either 0 or 255. This changes all pixels to primary additive colors (red, green, or blue), white, or black.
        /// </summary>
        HardMix,
        /// <summary>
        /// Looks at the color information in each channel and subtracts either the blend color from the base color or the base color from the blend color, depending on which has the greater brightness value. Blending with white inverts the base color values; blending with black produces no change.
        /// </summary>
        Difference,
        /// <summary>
        /// Creates an effect similar to but lower in contrast than the Difference mode. Blending with white inverts the base color values. Blending with black produces no change.
        /// </summary>
        Exclusion,
        /// <summary>
        /// Looks at the color information in each channel and subtracts the blend color from the base color. In 8- and 16-bit images, any resulting negative values are clipped to zero.
        /// </summary>
        Subtract,
        /// <summary>
        /// Looks at the color information in each channel and divides the blend color from the base color.
        /// </summary>
        Divide,
        /// <summary>
        /// Creates a result color with the luminance and saturation of the base color and the hue of the blend color. 
        /// </summary>
        Hue,
        /// <summary>
        /// Creates a result color with the luminance and hue of the base color and the saturation of the blend color. Painting with this mode in an area with no (0) saturation (gray) causes no change. 
        /// </summary>
        Saturation,
        /// <summary>
        /// Creates a result color with the luminance of the base color and the hue and saturation of the blend color. This preserves the gray levels in the image and is useful for coloring monochrome images and for tinting color images.
        /// </summary>
        Color,
        /// <summary>
        /// Creates a result color with the hue and saturation of the base color and the luminance of the blend color. This mode creates the inverse effect of Color mode.
        /// </summary>
        Luminosity
    }
}
