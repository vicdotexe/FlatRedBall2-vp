using System.IO;
using AnimationEditor.Core.IO;
using Xunit;

namespace AnimationEditor.Core.Tests;

public class TexturePathHelperTests
{
    // ── ComputeStorePath ──────────────────────────────────────────────────────

    [Fact]
    public void ComputeStorePath_TextureInAchxFolder_ReturnsSimpleRelative()
    {
        string storePath = TexturePathHelper.ComputeStorePath(
            TestPaths.Abs("Project", "Animations", "Hero.png"),
            TestPaths.AbsDir("Project", "Animations"));

        Assert.Equal("Hero.png", storePath);
    }

    [Fact]
    public void ComputeStorePath_TextureInSubfolder_ReturnsSubfolderRelative()
    {
        string storePath = TexturePathHelper.ComputeStorePath(
            TestPaths.Abs("Project", "Animations", "Sprites", "Hero.png"),
            TestPaths.AbsDir("Project", "Animations"));

        Assert.Equal("Sprites/Hero.png", storePath);
    }

    [Fact]
    public void ComputeStorePath_TextureInSiblingFolder_ReturnsDotDotRelative()
    {
        string storePath = TexturePathHelper.ComputeStorePath(
            TestPaths.Abs("Project", "Content", "Hero.png"),
            TestPaths.AbsDir("Project", "Animations"));

        // Key fix: sibling-folder textures must be stored as "../Content/Hero.png",
        // not as the absolute path.
        Assert.Equal("../Content/Hero.png", storePath);
    }

    [Fact]
    public void ComputeStorePath_TextureInGrandparentSiblingFolder_ReturnsDotDotRelative()
    {
        string storePath = TexturePathHelper.ComputeStorePath(
            TestPaths.Abs("OtherProject", "Content", "Hero.png"),
            TestPaths.AbsDir("Project", "Animations"));

        Assert.Equal("../../OtherProject/Content/Hero.png", storePath);
    }

    [Fact]
    public void ComputeStorePath_EmptyAchxFolder_ReturnsAbsoluteUnchanged()
    {
        var absolute = TestPaths.Abs("Project", "Content", "Hero.png");
        string storePath = TexturePathHelper.ComputeStorePath(absolute, string.Empty);

        Assert.Equal(absolute, storePath);
    }

    // ── ComputeDisplayPath ────────────────────────────────────────────────────

    [Fact]
    public void ComputeDisplayPath_AlreadyRelative_ReturnedUnchanged()
    {
        string display = TexturePathHelper.ComputeDisplayPath(
            "../Content/Hero.png",
            TestPaths.Abs("Project", "Animations", "Player.achx"));

        Assert.Equal("../Content/Hero.png", display);
    }

    [Fact]
    public void ComputeDisplayPath_AbsolutePathInSiblingFolder_ReturnsRelative()
    {
        string display = TexturePathHelper.ComputeDisplayPath(
            TestPaths.Abs("Project", "Content", "Hero.png"),
            TestPaths.Abs("Project", "Animations", "Player.achx"));

        // Absolute paths stored in the .achx should be displayed as relative.
        Assert.Equal("../Content/Hero.png", display);
    }

    [Fact]
    public void ComputeDisplayPath_AbsolutePathInAchxFolder_ReturnsSimpleRelative()
    {
        string display = TexturePathHelper.ComputeDisplayPath(
            TestPaths.Abs("Project", "Animations", "Hero.png"),
            TestPaths.Abs("Project", "Animations", "Player.achx"));

        Assert.Equal("Hero.png", display);
    }

    [Fact]
    public void ComputeDisplayPath_NullPath_ReturnsEmpty()
    {
        string display = TexturePathHelper.ComputeDisplayPath(
            null,
            TestPaths.Abs("Project", "Animations", "Player.achx"));

        Assert.Equal(string.Empty, display);
    }

    [Fact]
    public void ComputeDisplayPath_EmptyPath_ReturnsEmpty()
    {
        string display = TexturePathHelper.ComputeDisplayPath(
            string.Empty,
            TestPaths.Abs("Project", "Animations", "Player.achx"));

        Assert.Equal(string.Empty, display);
    }

    [Fact]
    public void ComputeDisplayPath_NullAchxPath_ReturnsAbsoluteUnchanged()
    {
        var absolute = TestPaths.Abs("Project", "Content", "Hero.png");
        string display = TexturePathHelper.ComputeDisplayPath(absolute, null);

        Assert.Equal(absolute, display);
    }

    // ── ResolveDisplayPath ────────────────────────────────────────────────────

    [Fact]
    public void ResolveDisplayPath_EmptyDisplayPath_ReturnsEmpty()
    {
        string resolved = TexturePathHelper.ResolveDisplayPath(string.Empty, TestPaths.AbsDir("Project", "Animations"));

        Assert.Equal(string.Empty, resolved);
    }

    [Fact]
    public void ResolveDisplayPath_AbsolutePath_ReturnedUnchanged()
    {
        var absolute = TestPaths.Abs("Project", "Content", "Hero.png");
        string resolved = TexturePathHelper.ResolveDisplayPath(absolute, TestPaths.AbsDir("Project", "Animations"));

        Assert.Equal(absolute, resolved);
    }

    [Fact]
    public void ResolveDisplayPath_RelativePathEmptyAchxFolder_ReturnedUnchanged()
    {
        string resolved = TexturePathHelper.ResolveDisplayPath("Hero.png", string.Empty);

        Assert.Equal("Hero.png", resolved);
    }

    [Fact]
    public void ResolveDisplayPath_SimpleRelativePath_ResolvesToAbsolute()
    {
        string resolved = TexturePathHelper.ResolveDisplayPath("Hero.png", TestPaths.AbsDir("Project", "Animations"));

        Assert.Equal(
            Path.GetFullPath(Path.Combine(TestPaths.AbsDir("Project", "Animations"), "Hero.png")),
            resolved);
    }

    [Fact]
    public void ResolveDisplayPath_DotDotRelativePath_ResolvesToAbsolute()
    {
        string resolved = TexturePathHelper.ResolveDisplayPath("../Content/Hero.png", TestPaths.AbsDir("Project", "Animations"));

        Assert.Equal(
            Path.GetFullPath(Path.Combine(TestPaths.AbsDir("Project", "Animations"), "../Content/Hero.png")),
            resolved);
    }
}
