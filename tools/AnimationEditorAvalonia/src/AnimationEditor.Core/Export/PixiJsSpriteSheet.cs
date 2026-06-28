using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AnimationEditor.Core.Export;

/// <summary>
/// Serializable shape of a PixiJS spritesheet manifest (<c>SpriteSheetJson</c>). JSON property
/// names match PixiJS exactly — see
/// https://pixijs.download/release/docs/assets.SpriteSheetJson.html. Only <see cref="Frames"/>
/// and <see cref="Meta"/> are required by PixiJS; <see cref="Animations"/> is optional but always
/// emitted by the exporter (one entry per FRB animation chain).
/// </summary>
public sealed class PixiJsSpriteSheet
{
    /// <summary>Frame-key → frame rect/metadata. The one required map for PixiJS.</summary>
    [JsonPropertyName("frames")]
    public Dictionary<string, PixiJsFrameData> Frames { get; set; } = new();

    /// <summary>Animation name → ordered list of frame keys.</summary>
    [JsonPropertyName("animations")]
    public Dictionary<string, List<string>> Animations { get; set; } = new();

    [JsonPropertyName("meta")]
    public PixiJsMeta Meta { get; set; } = new();
}

/// <summary>Per-frame entry in <see cref="PixiJsSpriteSheet.Frames"/> (PixiJS <c>SpritesheetFrameData</c>).</summary>
public sealed class PixiJsFrameData
{
    /// <summary>Pixel rect of the frame within the sheet. The only field PixiJS requires.</summary>
    [JsonPropertyName("frame")]
    public PixiJsRect Frame { get; set; } = new();

    /// <summary>Always <c>false</c>: FRB frames are not atlas-packed/rotated.</summary>
    [JsonPropertyName("rotated")]
    public bool Rotated { get; set; }

    /// <summary>Always <c>false</c>: FRB frames are not trimmed.</summary>
    [JsonPropertyName("trimmed")]
    public bool Trimmed { get; set; }

    [JsonPropertyName("sourceSize")]
    public PixiJsSize SourceSize { get; set; } = new();

    [JsonPropertyName("spriteSourceSize")]
    public PixiJsRect SpriteSourceSize { get; set; } = new();
}

/// <summary>Pixel rectangle (PixiJS uses <c>w</c>/<c>h</c>, not <c>width</c>/<c>height</c>).</summary>
public sealed class PixiJsRect
{
    [JsonPropertyName("x")] public int X { get; set; }
    [JsonPropertyName("y")] public int Y { get; set; }
    [JsonPropertyName("w")] public int W { get; set; }
    [JsonPropertyName("h")] public int H { get; set; }
}

/// <summary>Pixel size (PixiJS <c>{ w, h }</c>).</summary>
public sealed class PixiJsSize
{
    [JsonPropertyName("w")] public int W { get; set; }
    [JsonPropertyName("h")] public int H { get; set; }
}

/// <summary>Sheet-level metadata (PixiJS <c>meta</c>). <see cref="Image"/> and <see cref="Scale"/> are required.</summary>
public sealed class PixiJsMeta
{
    /// <summary>File name of the source sheet image.</summary>
    [JsonPropertyName("image")] public string Image { get; set; } = string.Empty;

    /// <summary>Sheet scale as a string ("1"); PixiJS parses it as a number.</summary>
    [JsonPropertyName("scale")] public string Scale { get; set; } = "1";

    /// <summary>Optional sibling sheets for multi-pack atlases. Omitted from JSON when null.</summary>
    [JsonPropertyName("related_multi_packs")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? RelatedMultiPacks { get; set; }
}
