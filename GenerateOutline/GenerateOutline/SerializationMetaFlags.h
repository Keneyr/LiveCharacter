
#ifndef SERIALIZATIONMETAFLAGS_H
#define SERIALIZATIONMETAFLAGS_H

#include "EnumFlags.h"
#include "PrefixConfigure.h"
/// Meta flags can be used like this:
/// transfer.Transfer (someVar, "varname", kHideInEditorMask);
/// The proxytransfer for example reads the metaflag mask and stores it in the TypeTree
enum TransferMetaFlags
{
	kNoTransferFlags = 0,
	/// Putting this mask in a transfer will make the variable be hidden in the property editor
	kHideInEditorMask = 1 << 0,

	/// Makes a variable not editable in the property editor
	kNotEditableMask = 1 << 4,

	/// There are 3 types of PPtrs: kStrongPPtrMask, default (weak pointer)
	/// a Strong PPtr forces the referenced object to be cloned.
	/// A Weak PPtr doesnt clone the referenced object, but if the referenced object is being cloned anyway (eg. If another (strong) pptr references this object)
	/// this PPtr will be remapped to the cloned object
	/// If an  object  referenced by a WeakPPtr is not cloned, it will stay the same when duplicating and cloning, but be NULLed when templating
	kStrongPPtrMask = 1 << 6,
	// unused  = 1 << 7,

	/// kEditorDisplaysCheckBoxMask makes an integer variable appear as a checkbox in the editor
	kEditorDisplaysCheckBoxMask = 1 << 8,

	// unused = 1 << 9,
	// unused = 1 << 10,

	/// Show in simplified editor
	kSimpleEditorMask = 1 << 11,

	/// When the options of a serializer tells you to serialize debug properties kSerializeDebugProperties
	/// All debug properties have to be marked kDebugPropertyMask
	/// Debug properties are shown in expert mode in the inspector but are not serialized normally
	kDebugPropertyMask = 1 << 12,

	kAlignBytesFlag = 1 << 14,
	kAnyChildUsesAlignBytesFlag = 1 << 15,
	kIgnoreWithInspectorUndoMask = 1 << 16,

	// unused = 1 << 18,

	// Ignore this property when reading or writing .meta files
	kIgnoreInMetaFiles = 1 << 19,

	// When reading meta files and this property is not present, read array entry name instead (for backwards compatibility).
	kTransferAsArrayEntryNameInMetaFiles = 1 << 20,

	// When writing YAML Files, uses the flow mapping style (all properties in one line, with "{}").
	kTransferUsingFlowMappingStyle = 1 << 21,

	// Tells SerializedProperty to generate bitwise difference information for this field.
	kGenerateBitwiseDifferences = 1 << 22,

	kDontAnimate = 1 << 23,

};
ENUM_FLAGS(TransferMetaFlags);

enum TransferInstructionFlags
{
	kNoTransferInstructionFlags = 0,

	kNeedsInstanceIDRemapping = 1 << 0, // Should we convert PPtrs into pathID, fileID using the PerisistentManager or should we just store the memory InstanceID in the fileID?
	kAssetMetaDataOnly = 1 << 1, // Only serialize data needed for .meta files
	kYamlGlobalPPtrReference = 1 << 2,
#if UNITY_EDITOR
	kLoadAndUnloadAssetsDuringBuild = 1 << 3,
	kSerializeDebugProperties = 1 << 4, // Should we serialize debug properties (eg. Serialize mono private variables)
#endif
	kIgnoreDebugPropertiesForIndex = 1 << 5, // Should we ignore Debug properties when calculating the TypeTree index
#if UNITY_EDITOR
	kBuildPlayerOnlySerializeBuildProperties = 1 << 6, // Used by eg. build player to make materials cull any properties are aren't used anymore !
#endif
	kWorkaround35MeshSerializationFuckup = 1 << 7,

	kSerializeGameRelease = 1 << 8, // Should Transfer classes use optimized reading. Allowing them to read memory directly that normally has a type using ReadDirect.
	kSwapEndianess = 1 << 9, // Should we swap endianess when reading / writing a file
	kSaveGlobalManagers = 1 << 10, // Should global managers be saved when writing the game build
	kDontReadObjectsFromDiskBeforeWriting = 1 << 11,
	kSerializeMonoReload = 1 << 12, // Should we backupmono mono variables for an assembly reload?
	kDontRequireAllMetaFlags = 1 << 13, // Can we fast path calculating all meta data. This lets us skip a bunch of code when serializing mono data.
	kSerializeForPrefabSystem = 1 << 14,
#if UNITY_EDITOR
	kWarnAboutLeakedObjects = 1 << 15,
	// Unused = 1 << 16,
	// Unused = 1 << 17,
	kEditorPlayMode = 1 << 18,
	kBuildResourceImage = 1 << 19,
	kSerializeEditorMinimalScene = 1 << 21,
	kGenerateBakedPhysixMeshes = 1 << 22,
#endif
	kThreadedSerialization = 1 << 23,
	kIsBuiltinResourcesFile = 1 << 24,
	kPerformUnloadDependencyTracking = 1 << 25,
	kDisableWriteTypeTree = 1 << 26,
	kAutoreplaceEditorWindow = 1 << 27,// Editor only
	kSerializeForInspector = 1 << 29,
	kSerializedAssetBundleVersion = 1 << 30, // When writing (typetrees disabled), allow later Unity versions an attempt to read SerializedFile.
	kAllowTextSerialization = 1 << 31
};
ENUM_FLAGS(TransferInstructionFlags);

enum BuildAssetBundleOptions
{
	kAssetBundleUncompressed = 1 << 11,
	kAssetBundleCollectDependencies = 1 << 20,
	kAssetBundleIncludeCompleteAssets = 1 << 21,
	kAssetBundleDisableWriteTypeTree = 1 << 26,
	kAssetBundleDeterministic = 1 << 28,
};
ENUM_FLAGS(BuildAssetBundleOptions);


enum ActiveResourceImage
{
	kResourceImageNotSupported = -2,
	kResourceImageInactive = -1,
	kGPUResourceImage = 0,
	kResourceImage = 1,
	kStreamingResourceImage = 2,
	kNbResourceImages = 3
};

/// This needs to be in Sync with BuildTarget in C#
enum BuildTargetPlatform
{
	kBuildNoTargetPlatform = -2,
	kBuildAnyPlayerData = -1,
	kBuildValidPlayer = 1,

	// We don't support building for these any more, but we still need the constants for asset bundle
	// backwards compatibility.
	kBuildStandaloneOSXPPC = 3,

	kBuildStandaloneOSXIntel = 4,
	kBuildStandaloneOSXIntel64 = 27,
	kBuildStandaloneOSXUniversal = 2,
	kBuildStandaloneWinPlayer = 5,
	kBuildWebPlayerLZMA = 6,
	kBuildWebPlayerLZMAStreamed = 7,
	kBuildWii = 8,
	kBuild_iPhone = 9,
	kBuildPS3 = 10,
	kBuildXBOX360 = 11,
	// was kBuild_Broadcom = 12,
	kBuild_Android = 13,
	kBuildWinGLESEmu = 14,
	// was kBuildWinGLES20Emu = 15,
	kBuildNaCl = 16,
	kBuildStandaloneLinux = 17,
	kBuildFlash = 18,
	kBuildStandaloneWin64Player = 19,
	kBuildWebGL = 20,
	kBuildMetroPlayerX86 = 21,
	kBuildMetroPlayerX64 = 22,
	kBuildMetroPlayerARM = 23,
	kBuildStandaloneLinux64 = 24,
	kBuildStandaloneLinuxUniversal = 25,
	kBuildWP8Player = 26,
	kBuildBB10 = 28,
	kBuildTizen = 29,
	kBuildPlayerTypeCount = 30,
};

struct BuildUsageTag
{
	bool   forceTextureReadable;
	bool   strippedPrefabObject;
	UInt32 meshUsageFlags;
	UInt32 meshSupportedChannels;

	BuildUsageTag()
	{
		forceTextureReadable = false;
		meshUsageFlags = 0;
		meshSupportedChannels = 0;
		strippedPrefabObject = false;
	}
};


struct BuildTargetSelection
{
	BuildTargetPlatform platform;
	int subTarget;

	BuildTargetSelection() : platform(kBuildNoTargetPlatform), subTarget(0) { }
	BuildTargetSelection(BuildTargetPlatform platform_, int subTarget_) : platform(platform_), subTarget(subTarget_) {}

	bool operator == (const BuildTargetSelection& rhs) const
	{
		if (platform != rhs.platform)
			return false;
		if (subTarget != rhs.subTarget)
			return false;

		return true;
	}
	bool operator != (const BuildTargetSelection& rhs) const
	{
		return !operator == (rhs);
	}

	static BuildTargetSelection NoTarget() { return BuildTargetSelection(kBuildNoTargetPlatform, 0); }
};


enum WebPlayerBuildSubTarget
{
	kWebBuildSubtargetDefault = 0,
	kWebBuildSubtargetDirect3D = 1, // windows only (D3D9 & D3D11)
	kWebBuildSubtargetOpenGL = 2, // non-windows only (OpenGL)
};


/// This needs to be in Sync with XboxRunMethod in C#
enum XboxBuildSubtarget
{
	kXboxBuildSubtargetDevelopment = 0,
	kXboxBuildSubtargetMaster = 1,
	kXboxBuildSubtargetDebug = 2
};

/// This needs to be in Sync with WiiBuildDebugLevel in C#
enum WiiBuildDebugLevel
{
	kWiiBuildDebugLevel_Full = 0,
	kWiiBuildDebugLevel_Minimal = 1,
	kWiiBuildDebugLevel_None = 2,
};

/// This needs to be in Sync with XboxRunMethod in C#
enum XboxRunMethod
{
	kXboxRunMethodHDD = 0,
	kXboxRunMethodDiscEmuFast = 1,
	kXboxRunMethodDiscEmuAccurate = 2
};

/// This needs to be in Sync with AndroidBuildSubtarget in C#
enum AndroidBuildSubtarget
{
	kAndroidBuildSubtarget_Generic = 0,
	kAndroidBuildSubtarget_DXT = 1,
	kAndroidBuildSubtarget_PVRTC = 2,
	kAndroidBuildSubtarget_ATC = 3,
	kAndroidBuildSubtarget_ETC = 4,
	kAndroidBuildSubtarget_ETC2 = 5,
	kAndroidBuildSubtarget_ASTC = 6,
};

/// This needs to be in Sync with BB10BuildSubtarget in C#
enum BlackBerryBuildSubtarget
{
	kBlackBerryBuildSubtarget_Generic = 0,
	kBlackBerryBuildSubtarget_PVRTC = 1,
	kBlackBerryBuildSubtarget_ATC = 2,
	kBlackBerryBuildSubtarget_ETC = 3
};

/// This needs to be in Sync with BB10BuildType in C#
enum BlackBerryBuildType
{
	kBlackBerryBuildType_Debug = 0,
	kBlackBerryBuildType_Submission = 1
};

/// This needs to be in Sync with BuildOptions in C#
enum BuildPlayerOptions
{
	kBuildPlayerOptionsNone = 0,
	kDevelopmentBuild = 1 << 0,
	kAutoRun = 1 << 2,
	kSelectBuiltPlayer = 1 << 3,
	kBuildAdditionalStreamedScenes = 1 << 4,
	kAcceptExternalModificationsToPlayer = 1 << 5,
	kInstallInBuildsFolder = 1 << 6,
	kWebPlayerOfflineDeployment = 1 << 7,
	kConnectWithProfiler = 1 << 8,
	kAllowDebugging = 1 << 9,
	kSymlinkLibraries = 1 << 10,
	kBuildPlayerUncompressed = 1 << 11,
	kConnectToHost = 1 << 12,
	kDeployOnline = 1 << 13,
	kHeadlessModeEnabled = 1 << 14
};

#endif
