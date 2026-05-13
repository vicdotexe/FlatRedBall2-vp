using System.Collections.Generic;
using System.IO;
using System.Linq;
using FlatRedBall2.Animation.Content;

namespace AnimationEditor.Core.ViewModels;

/// <summary>
/// Builds <see cref="TreeNodeVm"/> trees from animation data objects and routes
/// tree-selection events to <see cref="SelectedState"/>.
/// All methods are pure (no Avalonia references).
/// </summary>
public static class TreeBuilder
{
    // ── Tree construction ─────────────────────────────────────────────────────

    /// <summary>
    /// Builds a full tree from an <see cref="AnimationChainListSave"/>.
    /// Chain nodes whose names appear in <paramref name="expandedChainNames"/> get
    /// <see cref="TreeNodeVm.IsExpanded"/> = <c>true</c>.
    /// When <paramref name="expandedChainNames"/> is <c>null</c> every chain defaults
    /// to expanded (preserves existing behaviour when no saved state is available).
    /// </summary>
    public static List<TreeNodeVm> BuildTree(
        AnimationChainListSave acls,
        IEnumerable<string>? expandedChainNames = null)
    {
        var expanded = expandedChainNames is null
            ? null
            : new HashSet<string>(expandedChainNames, System.StringComparer.Ordinal);

        var result = new List<TreeNodeVm>(acls.AnimationChains.Count);
        foreach (var chain in acls.AnimationChains)
        {
            var node = BuildChainNode(chain);
            node.IsExpanded = expanded is null || expanded.Contains(chain.Name);
            result.Add(node);
        }
        return result;
    }

    /// <summary>Builds a single chain node with all its frame children.</summary>
    public static TreeNodeVm BuildChainNode(AnimationChainSave chain)
    {
        var node = new TreeNodeVm
        {
            Header = chain.Name,
            Data = chain,
            IsExpanded = true,
            IsChainNode = true,
            Kind = NodeKind.Chain,
            Meta = $"{chain.Frames.Count} fr",
        };
        for (int i = 0; i < chain.Frames.Count; i++)
            node.Children.Add(BuildFrameNode(chain.Frames[i], i));
        return node;
    }

    /// <summary>
    /// Builds a single frame node with any shape children from
    /// <see cref="AnimationFrameSave.ShapesSave"/>.
    /// </summary>
    public static TreeNodeVm BuildFrameNode(AnimationFrameSave frame, int index = 0)
    {
        var node = new TreeNodeVm
        {
            Header = BuildFrameHeader(frame, index),
            Data = frame,
            Kind = NodeKind.Frame,
            IsFrameNode = true,
            Meta = $"{frame.FrameLength:0.00}s",
        };
        if (frame.ShapesSave is not null)
        {
            foreach (var r in frame.ShapesSave!.AARectSaves)
            {
                node.Children.Add(new TreeNodeVm
                {
                    Header = r.Name,
                    Data = r,
                    Kind = NodeKind.RectShape,
                    IsRectNode = true,
                });
            }
            foreach (var c in frame.ShapesSave!.CircleSaves)
            {
                node.Children.Add(new TreeNodeVm
                {
                    Header = c.Name,
                    Data = c,
                    Kind = NodeKind.CircleShape,
                    IsCircleNode = true,
                });
            }
        }
        return node;
    }

    /// <summary>Returns the display label for a frame node.</summary>
    public static string BuildFrameHeader(AnimationFrameSave frame, int index = 0)
    {
        if (string.IsNullOrEmpty(frame.TextureName))
            return $"Frame {index + 1}";
        return Path.GetFileName(frame.TextureName);
    }

    // ── Diff-update helpers ───────────────────────────────────────────────────

    /// <summary>
    /// Diff-updates the shape children of <paramref name="frameNode"/> to match
    /// <paramref name="shapesSave"/> without replacing existing VMs.
    /// <para>
    /// VMs whose <see cref="TreeNodeVm.Data"/> still appears in
    /// <paramref name="shapesSave"/> are reused and their
    /// <see cref="TreeNodeVm.Header"/> is resynced (so a renamed shape is reflected).
    /// VMs for removed shapes are deleted; new VMs are created for added shapes.
    /// Order is: rects first, then circles.
    /// </para>
    /// </summary>
    public static void SyncShapesInto(TreeNodeVm frameNode, ShapesSave? shapesSave)
    {
            var rects   = shapesSave?.AARectSaves  ?? new System.Collections.Generic.List<AARectSave>();
            var circles = shapesSave?.CircleSaves  ?? new System.Collections.Generic.List<CircleSave>();

            // Remove shape VMs that no longer exist in the data lists.
            for (int i = frameNode.Children.Count - 1; i >= 0; i--)
            {
                var child = frameNode.Children[i];
                bool keep = (child.Data is AARectSave r && rects.Contains(r))
                         || (child.Data is CircleSave  c && circles.Contains(c));
                if (!keep) frameNode.Children.RemoveAt(i);
            }

            // Ensure every desired shape has a VM at the correct index.
            int pos = 0;
            foreach (var r in rects)
            {
                var vm = frameNode.Children.FirstOrDefault(n => ReferenceEquals(n.Data, r));
                if (vm is null)
                {
                    vm = new TreeNodeVm { Header = r.Name, Data = r, Kind = NodeKind.RectShape, IsRectNode = true };
                    frameNode.Children.Insert(pos, vm);
                }
                else
                {
                    vm.Header = r.Name;
                    int cur = frameNode.Children.IndexOf(vm);
                    if (cur != pos) { frameNode.Children.RemoveAt(cur); frameNode.Children.Insert(pos, vm); }
                }
                pos++;
            }
            foreach (var c in circles)
            {
                var vm = frameNode.Children.FirstOrDefault(n => ReferenceEquals(n.Data, c));
                if (vm is null)
                {
                    vm = new TreeNodeVm { Header = c.Name, Data = c, Kind = NodeKind.CircleShape, IsCircleNode = true };
                    frameNode.Children.Insert(pos, vm);
                }
                else
                {
                    vm.Header = c.Name;
                    int cur = frameNode.Children.IndexOf(vm);
                    if (cur != pos) { frameNode.Children.RemoveAt(cur); frameNode.Children.Insert(pos, vm); }
                }
                pos++;
            }
    }

    /// <summary>
    /// Diff-updates the frame children of <paramref name="chainNode"/> to match
    /// <paramref name="frames"/> without replacing existing VMs.
    /// <para>
    /// Frame VMs are matched by <see cref="TreeNodeVm.Data"/> reference.
    /// Surviving VMs have their header and meta resynced and their shape children
    /// recursively diff-updated via <see cref="SyncShapesInto"/>.
    /// <see cref="TreeNodeVm.IsExpanded"/> is preserved on retained VMs.
    /// </para>
    /// </summary>
    public static void SyncFramesInto(TreeNodeVm chainNode, System.Collections.Generic.IList<AnimationFrameSave> frames)
    {
        // Remove frame VMs whose data is no longer in the list.
        for (int i = chainNode.Children.Count - 1; i >= 0; i--)
        {
            var child = chainNode.Children[i];
            if (child.Data is AnimationFrameSave f && !frames.Contains(f))
                chainNode.Children.RemoveAt(i);
        }

        // Ensure every desired frame has a VM at the correct index.
        for (int i = 0; i < frames.Count; i++)
        {
            var frame = frames[i];
            var vm    = chainNode.Children.FirstOrDefault(n => ReferenceEquals(n.Data, frame));
            if (vm is null)
            {
                chainNode.Children.Insert(i, BuildFrameNode(frame, i));
            }
            else
            {
                // Only update header for named frames — unnamed "Frame N" labels stay
                // stable across reorder so the user can visually track which frame moved.
                if (!string.IsNullOrEmpty(frame.TextureName))
                    vm.Header = BuildFrameHeader(frame, i);
                vm.Meta   = $"{frame.FrameLength:0.00}s";
                int cur   = chainNode.Children.IndexOf(vm);
                if (cur != i) { chainNode.Children.RemoveAt(cur); chainNode.Children.Insert(i, vm); }
                SyncShapesInto(vm, frame.ShapesSave);
            }
        }
    }

    // ── Expand-state persistence ──────────────────────────────────────────────

    /// <summary>
    /// Returns the names of chain nodes that currently have
    /// <see cref="TreeNodeVm.IsExpanded"/> = <c>true</c>, for persistence in
    /// <c>AESettingsSave.ExpandedNodes</c>.
    /// </summary>
    public static IEnumerable<string> GetExpandedChainNames(IEnumerable<TreeNodeVm> roots) =>
        roots
            .Where(n => n.Data is AnimationChainSave && n.IsExpanded)
            .Select(n => ((AnimationChainSave)n.Data!).Name);

    // ── Selection routing ─────────────────────────────────────────────────────

    /// <summary>
    /// Maps a selected data object to the appropriate
    /// <see cref="ISelectedState"/> property.
    /// Returns <c>true</c> when the data object matched a known type.
    /// <para>
    /// Only applies the selection when the data object is reachable from
    /// <paramref name="acls"/>'s current chain list, preventing stale tree
    /// nodes from overwriting live selection state.
    /// </para>
    /// </summary>
    public static bool RouteNodeSelection(object? data, ISelectedState selectedState, AnimationChainListSave? acls)
    {
        if (data is null) return false;
        switch (data)
        {
            case AnimationChainSave chain:
                if (acls is null || !acls.AnimationChains.Contains(chain))
                    return true; // stale — recognised type, but don't corrupt state
                // Re-assign even if chain is the same object when a frame is selected,
                // so that SelectedChain setter clears SelectedFrame and fires SelectionChanged.
                if (selectedState.SelectedChain != chain || selectedState.SelectedFrame != null)
                    selectedState.SelectedChain = chain;
                return true;
            case AnimationFrameSave frame:
            {
                if (acls is null || !acls.AnimationChains.Any(c => c.Frames.Contains(frame)))
                    return true; // stale — recognised type, but don't corrupt state
                // Re-assign even when the same frame is already selected if a shape is
                // selected, so that SelectedFrame.set clears SelectedCircle/SelectedRectangle.
                if (selectedState.SelectedFrame != frame || selectedState.SelectedShape != null)
                    selectedState.SelectedFrame = frame;
                return true;
            }
            case AARectSave rect:
            {
                var parentFrame = FindParentFrameFor(rect, acls);
                if (parentFrame is null) return true; // stale — shape not reachable from live project
                if (selectedState.SelectedFrame != parentFrame)
                    selectedState.SelectedFrame = parentFrame; // stops playback; clears previous shape
                if (selectedState.SelectedRectangle != rect)
                    selectedState.SelectedRectangle = rect;
                return true;
            }
            case CircleSave circle:
            {
                var parentFrame = FindParentFrameFor(circle, acls);
                if (parentFrame is null) return true; // stale — shape not reachable from live project
                if (selectedState.SelectedFrame != parentFrame)
                    selectedState.SelectedFrame = parentFrame; // stops playback; clears previous shape
                if (selectedState.SelectedCircle != circle)
                    selectedState.SelectedCircle = circle;
                return true;
            }
            default:
                return false;
        }
    }

    /// <summary>
    /// Searches the full animation chain list for the <see cref="AnimationFrameSave"/>
    /// that owns <paramref name="shape"/> via its <c>ShapesSave</c>.
    /// Returns <c>null</c> when the shape cannot be found (e.g. stale node after a test reset).
    /// </summary>
    private static AnimationFrameSave? FindParentFrameFor(object shape, AnimationChainListSave? acls)
    {
        if (acls is null) return null;
        foreach (var chain in acls.AnimationChains)
            foreach (var frame in chain.Frames)
                if (frame.ShapesSave is { } scs)
                    if ((shape is CircleSave c && scs.CircleSaves.Contains(c)) ||
                        (shape is AARectSave r && scs.AARectSaves.Contains(r)))
                        return frame;
        return null;
    }

    // ── Node search ───────────────────────────────────────────────────────────

    /// <summary>
    /// Recursively searches <paramref name="roots"/> and returns the first node
    /// whose <see cref="TreeNodeVm.Data"/> is the same reference as
    /// <paramref name="target"/>.  Returns <c>null</c> when not found.
    /// </summary>
    public static TreeNodeVm? FindNodeForData(IEnumerable<TreeNodeVm> roots, object target)
    {
        foreach (var node in roots)
        {
            if (ReferenceEquals(node.Data, target)) return node;
            var found = FindNodeForData(node.Children, target);
            if (found is not null) return found;
        }
        return null;
    }
}
