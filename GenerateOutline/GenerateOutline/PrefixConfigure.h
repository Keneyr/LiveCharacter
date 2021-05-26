#ifndef UNITY_PREFIX_CONFIGURE_H
#define UNITY_PREFIX_CONFIGURE_H

// Processor extensions (SSE/VMX/VFP/NEON)
#define UNITY_AUTO_DETECT_VECTOR_UNIT 0
#define UNITY_SUPPORTS_SSE 0
#define UNITY_SUPPORTS_VMX 0
#define UNITY_SUPPORTS_VFP 0
#define UNITY_SUPPORTS_NEON 0
#define UNITY_DISABLE_NEON_SKINNING 0

// Renderer types potentially available based on platform
#define GFX_SUPPORTS_OPENGL 0
#define GFX_SUPPORTS_D3D9 0
#define GFX_SUPPORTS_D3D11 0
#define GFX_SUPPORTS_NULL 0
#define GFX_SUPPORTS_XENON 0
#define GFX_SUPPORTS_GCM 0
#define GFX_SUPPORTS_HOLLYWOOD 0
#define GFX_SUPPORTS_OPENGLES20 0
#define GFX_SUPPORTS_OPENGLES30 0
#define GFX_SUPPORTS_MOLEHILL 0
#define UNITY_ENABLE_ACCOUNTING_ALLOCATORS (!MASTER_BUILD)

// set per-platform defines

/*
Macros that need to be defined to be either 1 or 0 based on the target you are building.

 DEBUGMODE - Are asserts enabled or completely disabled? (The player we ship to customers has them disabled)
 UNITY_RELEASE - Enabled for all targets that we ship to unity-customers. Removes some development asserts.
 GAMERELEASE - Enable for everything that is not the editor
*/

// PCH handling in cw is fucked so we disable those warnings! Just remove it it when we throw out CW completely.

#if defined (UNITY) || defined (UNITY_EDITOR) || defined (DEPLOY_OPTIMIZED) \
	|| defined (UNITY_OSX) || defined (UNITY_WIN) || defined(UNITY_WII) \
	|| defined(UNITY_PS3) || defined(UNITY_XENON) || defined(UNITY_IPHONE) \
	|| defined(UNITY_ANDROID) || defined(UNITY_LINUX) || defined(UNITY_BB10) \
	|| defined(UNITY_TIZEN)
#error "These defines shall only be configured with this header"
#endif

#define UNITY 1

#if GAMERELEASE && !DEBUGMODE
#define DEPLOY_OPTIMIZED 1
#else
#define DEPLOY_OPTIMIZED 0
#endif

//UNITY_FLASH is actually passed on the cmdline and not determined from some system compiler define
#ifndef UNITY_FLASH
#define UNITY_FLASH 0
#endif

#ifndef TARGET_IPHONE_SIMULATOR
#define TARGET_IPHONE_SIMULATOR 0
#endif


// Platform
#if defined(__native_client__)
#	define UNITY_NACL 1
#	ifndef UNITY_PEPPER
#		define UNITY_PEPPER 1
#	endif
#	if !(defined(UNITY_NACL_CHROME) || defined(UNITY_NACL_WEBPLAYER))
#		define UNITY_NACL_CHROME 1
#	endif
#elif UNITY_FLASH
//then it's just flash, no need to set further define
#elif defined(ANDROID)
#	define UNITY_ANDROID 1
#elif defined(__APPLE__)
#	if defined(__arm__) || (TARGET_IPHONE_SIMULATOR)
#		define UNITY_IPHONE 1
#	else
#		define UNITY_OSX 1
#	endif
#define UNITY_XENON 0
#elif defined(GEKKO)
#define UNITY_WII 1
#elif defined(_WIN32) || defined(__WIN32__)
#define UNITY_WIN 1
#elif defined(SN_TARGET_PS3)
#define UNITY_PS3 1
#elif defined(SN_TARGET_PS3_SPU)
#define UNITY_SPU 1
#elif defined(__QNXNTO__)
#	define UNITY_BB10 1
#elif defined(TIZEN)
#	define UNITY_TIZEN 1
#elif defined(linux) || defined(__linux__)
#	define UNITY_LINUX 1
#endif


#define UNITY_EDITOR (!UNITY_EXTERNAL_TOOL && !GAMERELEASE && (UNITY_WIN || UNITY_OSX || UNITY_LINUX))

#ifndef UNITY_OSX
#define UNITY_OSX 0
#endif
#ifndef UNITY_WIN
#define UNITY_WIN 0
#endif
#ifndef UNITY_IPHONE
#define UNITY_IPHONE 0
#endif
#ifndef UNITY_XENON
#define UNITY_XENON 0
#endif
#undef UNITY_PS3
#define UNITY_PS3 0
#undef UNITY_WII
#define UNITY_WII 0
#ifndef UNITY_ANDROID
#define UNITY_ANDROID 0
#endif
#ifndef UNITY_NACL
#define UNITY_NACL 0
#endif
#undef UNITY_LINUX
#define UNITY_LINUX 0
#ifndef UNITY_PLUGIN
#define UNITY_PLUGIN 0
#endif
#ifndef UNITY_WEBGL
#define UNITY_WEBGL 0
#endif
#ifndef UNITY_METRO
#define UNITY_METRO 0
#endif
#ifndef UNITY_WP8
#define UNITY_WP8 0
#endif
#ifndef UNITY_WEBPLAYER
#define UNITY_WEBPLAYER 0
#endif
#ifndef UNITY_PEPPER
#define UNITY_PEPPER 0
#endif
#ifndef UNITY_BB10
#define UNITY_BB10 0
#endif
#ifndef UNITY_TIZEN
#define UNITY_TIZEN 0
#endif

#if defined(_AMD64_) || defined(__LP64__)
#define UNITY_64 1
#define UNITY_32 0
#else
#define UNITY_64 0
#define UNITY_32 1
#endif

#if (!UNITY_EXTERNAL_TOOL && !UNITY_EDITOR && !UNITY_WEBPLAYER && (UNITY_WIN || UNITY_OSX || UNITY_LINUX))
#define UNITY_STANDALONE_OSX UNITY_OSX
#define UNITY_STANDALONE_WIN UNITY_WIN
#define UNITY_STANDALONE_LINUX UNITY_LINUX
#else
#define UNITY_STANDALONE_OSX 0
#define UNITY_STANDALONE_WIN 0
#define UNITY_STANDALONE_LINUX 0
#endif

// Processor extensions (SSE/VMX/VFP/NEON)

#if UNITY_PEPPER
#undef UNITY_SUPPORTS_SSE
#define UNITY_SUPPORTS_SSE 0
#elif UNITY_WIN && !defined(__arm__)
#undef UNITY_AUTO_DETECT_VECTOR_UNIT
#undef UNITY_SUPPORTS_SSE
#define UNITY_SUPPORTS_SSE 1
#define UNITY_AUTO_DETECT_VECTOR_UNIT (!UNITY_64)
#elif UNITY_OSX && defined(__i386__)
#undef UNITY_SUPPORTS_SSE
#define UNITY_SUPPORTS_SSE 1
#elif UNITY_XENON || UNITY_PS3
#undef UNITY_SUPPORTS_VMX
#define UNITY_SUPPORTS_VMX 1
#elif defined(__ARM_NEON__) || \
	defined(UNITY_ANDROID) && (defined(__ARM_ARCH_7__) || defined(__ARM_ARCH_7A__) || defined(__ARM_ARCH_7R__))
	// Android uses runtime checks for NEON in the ARMv7 codepath.
	// That means UNITY_SUPPORTS_NEON must ALWAYS be used in conjunction with CPUInfo::HasNEONSupport()
#undef UNITY_SUPPORTS_NEON
#define UNITY_SUPPORTS_NEON 1
#undef UNITY_SUPPORTS_VFP
#define UNITY_SUPPORTS_VFP 1
#elif defined(__ARM_ARCH_6__) || defined(__ARM_ARCH_6J__) || \
	defined(__ARM_ARCH_6K__) || defined(__ARM_ARCH_6Z__) || \
	defined(__ARM_ARCH_6ZK__) || defined(__ARM_ARCH_6T2__) || \
	defined(ARM_ARCH_VFP)
#undef UNITY_SUPPORTS_VFP
#define UNITY_SUPPORTS_VFP 1
#elif UNITY_LINUX && (defined(__i386__) || UNITY_64)
#undef UNITY_SUPPORTS_SSE
#define UNITY_SUPPORTS_SSE 1
#elif UNITY_WINRT && defined(__arm__)
#undef UNITY_SUPPORTS_NEON
#define UNITY_SUPPORTS_NEON 1
#undef UNITY_DISABLE_NEON_SKINNING
// NEON skinning code doesn't normalize tangents and normals, so if you're skinning a scaled mesh, it will look incorrect with NEON
// Did some profiling - and disabling NEON didn't add any significant performance loss, so for the sake of correct rendering, let's keep it disabled for now
// At least until we'll fix an issue where all the normalization will occur only in shaders
#define UNITY_DISABLE_NEON_SKINNING 1
#endif

// Endian
#ifndef UNITY_BIG_ENDIAN
#if (defined (__APPLE__) && defined(__BIG_ENDIAN__)) || UNITY_WII || UNITY_XENON || UNITY_PS3
#define UNITY_BIG_ENDIAN 1
#define UNITY_LITTLE_ENDIAN 0
#else
#define UNITY_BIG_ENDIAN 0
#define UNITY_LITTLE_ENDIAN 1
#endif
#endif

// On intel macs we are statically linking ogg vorbis and use the built in OpenAL
#if defined(__APPLE__) && defined(__i386__)
#define UNITY_VORBIS_STATICLIB 1
#else
#define UNITY_VORBIS_STATICLIB 0
#endif

// PhysX setup
#if UNITY_OSX || UNITY_NACL
// "check" macro from AssertMacros.h in OS X SDK breaks PhysX headers.
#undef check
#endif


#ifndef ENABLE_GFXDEVICE_REMOTE_PROCESS
#define ENABLE_GFXDEVICE_REMOTE_PROCESS 0
#endif

#ifndef ENABLE_GFXDEVICE_REMOTE_PROCESS_CLIENT
#define ENABLE_GFXDEVICE_REMOTE_PROCESS_CLIENT 0
#endif

#ifndef ENABLE_GFXDEVICE_REMOTE_PROCESS_WORKER
#define ENABLE_GFXDEVICE_REMOTE_PROCESS_WORKER 0
#endif

#if UNITY_PEPPER
#if ENABLE_GFXDEVICE_REMOTE_PROCESS_CLIENT
#undef GFX_SUPPORTS_OPENGL
#define GFX_SUPPORTS_OPENGL 1
#define ENABLE_MULTITHREADED_CODE 1
#else
#undef GFX_SUPPORTS_OPENGLES20
#define GFX_SUPPORTS_OPENGLES20 1
#define ENABLE_MULTITHREADED_CODE 1
#endif
#elif UNITY_OSX
#undef GFX_SUPPORTS_OPENGL
#define GFX_SUPPORTS_OPENGL 1
#define ENABLE_MULTITHREADED_CODE 1
#elif UNITY_WIN
#undef GFX_SUPPORTS_OPENGL
#define GFX_SUPPORTS_OPENGL (!UNITY_WEBPLAYER && !UNITY_WINRT) // OpenGL not supported in web player
#undef GFX_SUPPORTS_D3D9
#define GFX_SUPPORTS_D3D9 (!UNITY_WINRT)
#undef GFX_SUPPORTS_D3D11
#define GFX_SUPPORTS_D3D11 1
#undef GFX_SUPPORTS_NULL
#define GFX_SUPPORTS_NULL (!UNITY_WEBPLAYER && !UNITY_WINRT) // null device supported in editor & standalone
#undef GFX_SUPPORTS_OPENGLES20
#define GFX_SUPPORTS_OPENGLES20 (!UNITY_EDITOR && !UNITY_WEBPLAYER && !UNITY_WINRT) // GLES20 only standalone
#undef GFX_SUPPORTS_OPENGLES30
#define GFX_SUPPORTS_OPENGLES30 (!UNITY_EDITOR && !UNITY_WEBPLAYER && !UNITY_WINRT) // GLES30 only standalone
#define ENABLE_MULTITHREADED_CODE 1
#elif UNITY_XENON
#undef GFX_SUPPORTS_XENON
#define GFX_SUPPORTS_XENON 1
#define ENABLE_MULTITHREADED_CODE 1
#elif UNITY_WII
#undef GFX_SUPPORTS_HOLLYWOOD
#define GFX_SUPPORTS_HOLLYWOOD 1
#undef UNITY_ENABLE_ACCOUNTING_ALLOCATORS
#define UNITY_ENABLE_ACCOUNTING_ALLOCATORS (!MASTER_BUILD)
#define ENABLE_MULTITHREADED_CODE 0
#elif UNITY_PS3
#undef GFX_SUPPORTS_GCM
#define GFX_SUPPORTS_GCM 1
#define ENABLE_MULTITHREADED_CODE 1
#elif UNITY_IPHONE
#undef GFX_SUPPORTS_OPENGLES20
#if !TARGET_IPHONE_SIMULATOR
#include "arm/arch.h" // in order to determine arm architecture type
#endif
#if defined(_ARM_ARCH_7) || TARGET_IPHONE_SIMULATOR
#define GFX_SUPPORTS_OPENGLES20 1
#else
#define GFX_SUPPORTS_OPENGLES20 0
#endif
#undef UNITY_ENABLE_ACCOUNTING_ALLOCATORS
#define UNITY_ENABLE_ACCOUNTING_ALLOCATORS 0
#define ENABLE_MULTITHREADED_CODE 1
#elif UNITY_ANDROID
#undef GFX_SUPPORTS_OPENGLES20
#undef GFX_SUPPORTS_OPENGLES30
#define GFX_SUPPORTS_OPENGLES20 1
#define GFX_SUPPORTS_OPENGLES30 1
#undef UNITY_ENABLE_ACCOUNTING_ALLOCATORS
#define UNITY_ENABLE_ACCOUNTING_ALLOCATORS 0
#define ENABLE_MULTITHREADED_CODE 1
#elif UNITY_BB10
#undef GFX_SUPPORTS_OPENGLES20
#define GFX_SUPPORTS_OPENGLES20 1
#undef UNITY_ENABLE_ACCOUNTING_ALLOCATORS
#define UNITY_ENABLE_ACCOUNTING_ALLOCATORS 0
#define ENABLE_MULTITHREADED_CODE 1
#elif UNITY_TIZEN
#undef GFX_SUPPORTS_OPENGLES20
#define GFX_SUPPORTS_OPENGLES20 1
#undef UNITY_ENABLE_ACCOUNTING_ALLOCATORS
#define UNITY_ENABLE_ACCOUNTING_ALLOCATORS 0
#define ENABLE_MULTITHREADED_CODE 1
#elif UNITY_LINUX
#if defined(__arm__)
#undef GFX_SUPPORTS_OPENGLES20
#define GFX_SUPPORTS_OPENGLES20 1
#undef UNITY_ENABLE_ACCOUNTING_ALLOCATORS
#define UNITY_ENABLE_ACCOUNTING_ALLOCATORS 0
#define ENABLE_MULTITHREADED_CODE 0
#else
#undef GFX_SUPPORTS_OPENGL
#define GFX_SUPPORTS_OPENGL SUPPORT_X11
#undef GFX_SUPPORTS_NULL
#define GFX_SUPPORTS_NULL (!UNITY_WEBPLAYER)
#define ENABLE_MULTITHREADED_CODE 1
#endif
#elif UNITY_FLASH
#undef GFX_SUPPORTS_MOLEHILL
#define GFX_SUPPORTS_MOLEHILL 1
#define ENABLE_MULTITHREADED_CODE 0
#elif UNITY_WEBGL
#undef GFX_SUPPORTS_OPENGLES20
#define GFX_SUPPORTS_OPENGLES20 1
#define ENABLE_MULTITHREADED_CODE 0
#endif


#define ENABLE_MULTITHREADED_SKINNING ENABLE_MULTITHREADED_CODE

#define GFX_OPENGLESxx_ONLY ((GFX_SUPPORTS_OPENGLES20) && !(GFX_SUPPORTS_D3D9 || GFX_SUPPORTS_D3D11 || GFX_SUPPORTS_OPENGL || GFX_SUPPORTS_XENON || GFX_SUPPORTS_GCM))

#define SUBSTANCE_PLATFORM_BLEND 1

#ifndef UNITY_ASSEMBLER

typedef signed short SInt16;
typedef unsigned short UInt16;
typedef unsigned char UInt8;
typedef signed char SInt8;
typedef signed int SInt32;
typedef unsigned int UInt32;
typedef unsigned long long UInt64;
typedef signed long long SInt64;

#if UNITY_64
typedef UInt64 UIntPtr;
#else
typedef UInt32 UIntPtr;
#endif

#endif


#if !defined(UNITY_BIG_ENDIAN) || !defined(UNITY_LITTLE_ENDIAN)
#	error make sure PlatformPrefixConfigure.h defines UNITY_LITTLE_ENDIAN & UNITY_BIG_ENDIAN
#endif


#endif
