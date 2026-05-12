using FlatRedBall2.Animation.Content;
using System;

namespace AnimationEditor.Core.CommandsAndState
{
    public interface IApplicationEvents
    {
        event Action AfterZoomChange;
        event Action WireframePanning;
        event Action WireframeTextureChange;
        /// <summary>Fired by <c>AppCommands.OpenAchxWorkflow</c> after the .achx is fully loaded.</summary>
        event Action<string> AchxLoaded;
        event Action<AARectSave> AfterAxisAlignedRectangleChanged;
        event Action<CircleSave> AfterCircleChanged;
        event Action AnimationChainsChanged;
        /// <summary>Fired after the active file path changes (open or save-as). Carries the new path.</summary>
        event Action<string> CurrentFileChanged;
        /// <summary>Fired when texture references in the loaded project may have changed.</summary>
        event Action AvailableTexturesChanged;

        void RaiseAfterAxisAlignedRectangleChanged(AARectSave rectangle);
        void RaiseAfterCircleChanged(CircleSave circle);
        void RaiseAnimationChainsChanged();
        void CallAchxLoaded(string newFileName);
        void CallAfterZoomChange();
        void CallAfterWireframePanning();
        void CallWireframeTextureChange();
        void RaiseCurrentFileChanged(string path);
        void RaiseAvailableTexturesChanged();
    }
}
