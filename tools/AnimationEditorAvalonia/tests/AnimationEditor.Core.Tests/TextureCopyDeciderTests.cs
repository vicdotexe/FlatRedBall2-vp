using AnimationEditor.Core.IO;
using Xunit;

namespace AnimationEditor.Core.Tests;

public class TextureCopyDeciderTests
{
    private static readonly string Folder  = TestPaths.Abs("project", "Content");
    private static readonly string Inside  = TestPaths.Abs("project", "Content", "hero.png");
    private static readonly string SubDir  = TestPaths.Abs("project", "Content", "Textures", "run.png");
    private static readonly string Outside = TestPaths.Abs("other", "assets", "enemy.png");

    // ── Null / empty texture path ─────────────────────────────────────────────

    [Fact]
    public void NullTexturePath_ReturnsFalse()
        => Assert.False(TextureCopyDecider.ShouldPromptToCopy(null, Folder));

    [Fact]
    public void EmptyTexturePath_ReturnsFalse()
        => Assert.False(TextureCopyDecider.ShouldPromptToCopy("", Folder));

    // ── Null / empty project folder ───────────────────────────────────────────

    [Fact]
    public void NullProjectFolder_ReturnsTrue()
        => Assert.True(TextureCopyDecider.ShouldPromptToCopy(Inside, null));

    [Fact]
    public void EmptyProjectFolder_ReturnsTrue()
        => Assert.True(TextureCopyDecider.ShouldPromptToCopy(Inside, ""));

    // ── Texture inside project folder ─────────────────────────────────────────

    [Fact]
    public void TextureDirectlyInFolder_ReturnsFalse()
        => Assert.False(TextureCopyDecider.ShouldPromptToCopy(Inside, Folder));

    [Fact]
    public void TextureInSubDirectory_ReturnsFalse()
        => Assert.False(TextureCopyDecider.ShouldPromptToCopy(SubDir, Folder));

    [Fact]
    public void FolderWithTrailingSeparator_TextureInside_ReturnsFalse()
        => Assert.False(TextureCopyDecider.ShouldPromptToCopy(Inside, Folder + Path.DirectorySeparatorChar));

    // ── Texture outside project folder ────────────────────────────────────────

    [Fact]
    public void TextureOutsideFolder_ReturnsTrue()
        => Assert.True(TextureCopyDecider.ShouldPromptToCopy(Outside, Folder));

    [Fact]
    public void TextureWithSamePrefix_ButNotSubPath_ReturnsTrue()
    {
        // "...projectExtra\..." starts with "...project" but is NOT inside "...project\"
        string notInside = TestPaths.Abs("projectExtra", "Content", "hero.png");
        Assert.True(TextureCopyDecider.ShouldPromptToCopy(notInside, Folder));
    }

    // ── Case-insensitive comparison ───────────────────────────────────────────

    [Fact]
    public void UpperCaseTexturePath_TextureInside_ReturnsFalse()
        => Assert.False(TextureCopyDecider.ShouldPromptToCopy(Inside.ToUpper(), Folder));

    [Fact]
    public void UpperCaseProjectFolder_TextureInside_ReturnsFalse()
        => Assert.False(TextureCopyDecider.ShouldPromptToCopy(Inside, Folder.ToUpper()));

    // ── Forward-slash normalisation ───────────────────────────────────────────

    [Fact]
    public void ForwardSlashTexturePath_TextureInside_ReturnsFalse()
    {
        string fwdTexture = Inside.Replace('\\', '/');
        Assert.False(TextureCopyDecider.ShouldPromptToCopy(fwdTexture, Folder));
    }

    [Fact]
    public void ForwardSlashProjectFolder_TextureInside_ReturnsFalse()
    {
        string fwdFolder = Folder.Replace('\\', '/');
        Assert.False(TextureCopyDecider.ShouldPromptToCopy(Inside, fwdFolder));
    }
}
