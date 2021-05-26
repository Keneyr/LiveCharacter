#ifndef SERIALIZEUTILITY_H
#define SERIALIZEUTILITY_H


#include "SerializationMetaFlags.h"

#define TRANSFER(x) transfer.Transfer (x, #x)
#define TRANSFER_SIMPLE(x) transfer.Transfer (x, #x, kSimpleEditorMask)

#if UNITY_EDITOR
#define TRANSFER_EDITOR_ONLY(x) if (!transfer.IsSerializingForGameRelease()) { transfer.Transfer (x, #x, kDontAnimate); }
#else
#define TRANSFER_EDITOR_ONLY(x) {  }
#endif

#if UNITY_EDITOR
#define TRANSFER_EDITOR_ONLY_HIDDEN(x) if (!transfer.IsSerializingForGameRelease()) { transfer.Transfer (x, #x, kHideInEditorMask); }
#else
#define TRANSFER_EDITOR_ONLY_HIDDEN(x) {  }
#endif

#define TRANSFER_WITH_CUSTOM_GET_SET(TYPE, STR_NAME, GET, SET, OPTIONS) \
	{ \
		TYPE value;  \
		if (transfer.IsWriting ()) { GET ; }  \
		transfer.Transfer(value, STR_NAME, OPTIONS);  \
		if (transfer.DidReadLastProperty ()) { SET ;	}  \
	}

#define TRANSFER_PROPERTY(TYPE,NAME,GET,SET)\
	TRANSFER_WITH_CUSTOM_GET_SET(TYPE, #NAME,  value = GET (), SET (value), kNoTransferFlags)

#define TRANSFER_ENUM(x) { Assert(sizeof(x) == sizeof(int)); transfer.Transfer ((int&)x, #x); }

#if UNITY_EDITOR
#define TRANSFER_DEBUG(x) {	if (transfer.GetFlags () & kSerializeDebugProperties) transfer.Transfer (x, #x, kDebugPropertyMask | kNotEditableMask); }
#else
#define TRANSFER_DEBUG(x) 
#endif

template<class T>
inline bool SerializePrefabIgnoreProperties(T& transfer)
{
	return (transfer.GetFlags() & kSerializeForPrefabSystem) == 0;
}

/// Usage: TRANSFER_PROPERTY_DEBUG(bool, m_Enabled, data->GetEnabled)
#define TRANSFER_PROPERTY_DEBUG(TYPE,NAME,GET) \
if (transfer.GetFlags () & kSerializeDebugProperties){\
	TYPE NAME;\
	if (transfer.IsWriting ())\
		NAME = GET ();\
	transfer.Transfer (NAME, #NAME, kDebugPropertyMask | kNotEditableMask);\
}


#define DEFINE_GET_TYPESTRING(x)						\
	inline static const char* GetTypeString ()	{ return #x; } \
	inline static bool IsAnimationChannel ()	{ return false; } \
	inline static bool MightContainPPtr ()	{ return true; } \
	inline static bool AllowTransferOptimization ()	{ return false; }

#define DEFINE_GET_TYPESTRING_IS_ANIMATION_CHANNEL(x)		\
	inline static const char* GetTypeString ()	{ return #x; } \
	inline static bool IsAnimationChannel ()	{ return true; } \
	inline static bool MightContainPPtr ()	{ return false; } \
	inline static bool AllowTransferOptimization ()	{ return true; }


#define DECLARE_SERIALIZE(x) \
	inline static const char* GetTypeString ()	{ return #x; }	\
	inline static bool IsAnimationChannel ()	{ return false; } \
	inline static bool MightContainPPtr ()	{ return true; } \
	inline static bool AllowTransferOptimization ()	{ return false; } \
	template<class TransferFunction> \
	void Transfer (TransferFunction& transfer);

#define DECLARE_SERIALIZE_NO_PPTR(x) \
	inline static const char* GetTypeString ()	{ return #x; }	\
	inline static bool IsAnimationChannel ()	{ return false; } \
	inline static bool MightContainPPtr ()	{ return false; } \
	inline static bool AllowTransferOptimization ()	{ return false; } \
	template<class TransferFunction> \
	void Transfer (TransferFunction& transfer);

#define DECLARE_SERIALIZE_OPTIMIZE_TRANSFER(x) \
	inline static const char* GetTypeString ()	{ return #x; }	\
	inline static bool IsAnimationChannel ()	{ return false; } \
	inline static bool MightContainPPtr ()	{ return false; } \
	inline static bool AllowTransferOptimization ()	{ return true; } \
	template<class TransferFunction> \
	void Transfer (TransferFunction& transfer);


#endif
