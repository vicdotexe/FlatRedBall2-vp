using AnimationEditor.Core.Data;
using FlatRedBall2.Animation.Content;
using System;
using FilePath = AnimationEditor.Core.Paths.FilePath;

namespace AnimationEditor.Core.IO
{
    public interface IIoManager
    {
        event Action<string, Exception> SaveFailed;
        event Action<AESettingsSave> SettingsLoaded;
        string RecoveryFilePath { get; set; }

        void SaveCompanionFileFor(FilePath fileName, AESettingsSave settings);
        void LoadAndApplyCompanionFileFor(string achxFile);
        void WriteRecoveryFile(AnimationChainListSave? animationChainListSave);
        void DeleteRecoveryFile();
        bool RecoveryFileExists();
    }
}
