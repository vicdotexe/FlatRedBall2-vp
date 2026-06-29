using System.IO;
using Shouldly;
using Xunit;

namespace FlatRedBall2.Tests.Packaging;

// Issue #504. Precompiled apos-shapes.xnb must be built with the same MonoGame MGCB line
// that template consumers resolve — not a newer preview MGCB that emits a higher MGFX version.
public class PrecompiledShaderCompatibilityTests
{
    private const string ExpectedMonoGameVersion = "3.8.4.1";

    [Theory]
    [InlineData("src/PrecompiledShaders/DesktopGL/apos-shapes.xnb")]
    [InlineData("templates/frb2-desktop/build/DesktopGL/apos-shapes.xnb")]
    [InlineData("templates/frb2-multiplatform/build/DesktopGL/apos-shapes.xnb")]
    public void DesktopAposShapesXnb_MatchesPinnedMonoGameToolchain(string relativePath)
    {
        var bytes = File.ReadAllBytes(Path.Combine(RepoRoot, relativePath));
        var mgfxVersion = PrecompiledAposShapesXnbReader.GetMgfxVersion(bytes);
        var monoGameAssemblyVersion = PrecompiledAposShapesXnbReader.GetMonoGameAssemblyVersion(bytes);
        var runtimeMaxMgfx = PrecompiledAposShapesXnbReader.GetMaxMgfxVersionAcceptedByRuntime();

        monoGameAssemblyVersion.ShouldBe(
            ExpectedMonoGameVersion,
            $"Rebuild apos-shapes.xnb with MGCB {ExpectedMonoGameVersion} and recopy into {relativePath}.");

        mgfxVersion.ShouldBeLessThanOrEqualTo(
            runtimeMaxMgfx,
            customMessage: $"MGFX v{mgfxVersion} in {relativePath} exceeds MonoGame.Framework {ExpectedMonoGameVersion} (max MGFX v{runtimeMaxMgfx}).");
    }

    private static string RepoRoot => TemplatePackageReferenceTests.RepoRootForTests;
}
