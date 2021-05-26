#pragma once

#include "Texture.h"

class ColorRGBAf;
class ColorRGBA32;


class EXPORT_COREMODULE Texture2D : public Texture
{
public:
	// Should be called by MAIN thread in order to upload texture data immediately after texture asset was loaded
	// On some platforms helps to avoid memory peak during level load
	static void IntegrateLoadedImmediately();

protected:

	// See comments at Texture2D.cpp
	struct TextureRepresentation {
		TextureRepresentation();

		UInt8*	data; // data for all image frames
		int		width;
		int		height;
		int		format;
		int		imageSize; // size in bytes of one image frames, including mip levels
	};
	TextureRepresentation	m_TexData;	// original data
	TextureID m_UnscaledTexID;

	static bool s_ScreenReadAllowed;

protected:
	int		m_ImageCount;
	int		m_TextureDimension;

	int		m_glWidth;
	int		m_glHeight;

	int		m_InitFlags;
	bool	m_MipMap;
	bool	m_PowerOfTwo;
	bool    m_TextureUploaded;
	bool    m_UnscaledTextureUploaded;
	bool    m_IsReadable;
	bool	m_ReadAllowed;
	bool	m_IsUnreloadable;

#if UNITY_EDITOR
	bool	m_EditorDontWriteTextureData;
	bool	m_IgnoreMasterTextureLimit;
	bool	m_AlphaIsTransparency;
#endif

protected:
	void DestroyTexture();
	void DeleteGfxTexture();

	virtual void UploadTexture(bool dontUseSubImage);
	virtual void UnloadFromGfxDevice(bool forceUnloadAll);
	virtual void UploadToGfxDevice();


private:

	void	DestroyTextureRepresentation(TextureRepresentation* rep);
	void	DestroyTextureRepresentations(TextureRepresentation* scaled, TextureRepresentation* padded, bool freeSourceImage = true);

	void	InitTextureRepresentation(TextureRepresentation* rep, int format, const char* tag);
	void	InitTextureRepresentations(TextureRepresentation* scaled, TextureRepresentation* padded);

	void	UpdatePOTStatus();


	void ExtractMipLevel(TextureRepresentation* dst, int frame, int mipLevel, bool checkCompression, bool scaleToSize);
	bool ExtractImageInternal(ImageReference* image, bool scaleToSize, int imageIndex) const;
	void ExtractCompressedImageInternal(UInt8* dst, int dstWidth, int dstHeight, int imageIndex) const;

	bool GetImageReferenceInternal(ImageReference* image, int frame, int miplevel) const;

	bool CheckHasPixelData() const;

	MemLabelId GetTextureDataMemoryLabel() const { return (GetMemoryLabel().label == kMemTextureCacheId ? GetMemoryLabel() : MemLabelId(kMemTextureId, GetMemoryLabel().GetRootHeader())); }
public:
	REGISTER_DERIVED_CLASS(Texture2D, Texture)
		DECLARE_OBJECT_SERIALIZE(Texture2D)

	Texture2D(MemLabelId label, ObjectCreationMode mode);
	// ~Texture2D (); declared-by-macro

	virtual bool MainThreadCleanup();

	virtual void Reset();
	virtual void AwakeFromLoadThreaded();
	virtual void AwakeFromLoad(AwakeFromLoadMode awakeMode);

	virtual TextureDimension GetDimension() const { return static_cast<TextureDimension>(m_TextureDimension); }

	virtual int GetGLWidth() const { return m_glWidth; }
	virtual int GetGLHeight() const { return m_glHeight; }
	virtual void ApplySettings();

	virtual int GetDataWidth() const;
	virtual int GetDataHeight() const;

	virtual int GetRuntimeMemorySize() const;
#if ENABLE_PROFILER || UNITY_EDITOR
	virtual int GetStorageMemorySize() const { return m_TexData.imageSize*m_ImageCount; }
#endif

	virtual TextureID GetUnscaledTextureID() const;
	int CountDataMipmaps() const;
	bool IsNonPowerOfTwo() const { return !m_PowerOfTwo; }

	enum {
		kNoMipmap = 0,
		kMipmapMask = 1 << 0,
		kThreadedInitialize = 1 << 2,
		kOSDrawingCompatible = 1 << 3,
	};
	virtual bool InitTexture(int width, int height, TextureFormat format, int flags = kMipmapMask, int imageCount = 1, intptr_t nativeTex = 0);

	void   InitTextureInternal(int width, int height, TextureFormat format, int imageSize, UInt8* buffer, int options, int imageCount);
	UInt8* AllocateTextureData(int imageSize, TextureFormat format, bool initMemory = false);
	void   DeallocateTextureData(UInt8* memory);

	virtual bool HasMipMap() const;

	void SetIsReadable(bool readable) { m_IsReadable = readable; }
	bool GetIsReadable() const { return m_IsReadable; }
	void SetIsUnreloadable(bool value) { m_IsUnreloadable = value; }
	bool GetIsUploaded() const { return m_TextureUploaded; }

#if UNITY_EDITOR
	void SetEditorDontWriteTextureData(bool value) { m_EditorDontWriteTextureData = value; }
	// directly load from an image, used in editor for gizmos/icons
	void SetImage(const ImageReference& image, int flags = kMipmapMask);
	virtual void WarnInstantiateDisallowed();
	virtual bool IgnoreMasterTextureLimit() const;
	void SetIgnoreMasterTextureLimit(bool ignore);

	bool GetAlphaIsTransparency() const;
	void SetAlphaIsTransparency(bool is);

	virtual TextureFormat GetEditorUITextureFormat() const { return GetTextureFormat(); }

#endif

	virtual void UpdateImageData();
	virtual void UpdateImageDataDontTouchMipmap();

	// Returns the original (may be NPOT) data
	UInt8 *GetRawImageData(int frame = 0) { return m_TexData.data + frame * m_TexData.imageSize; }
	int GetRawImageDataSize() const { return m_TexData.imageSize; }

	bool GetWriteImageReference(ImageReference* image, int frame, int miplevel);

	int GetImageCount() const { return m_ImageCount; }

	virtual bool ExtractImage(ImageReference* image, int imageIndex = 0) const;

	bool ResizeWithFormat(int width, int height, TextureFormat format, int flags);
	bool Resize(int width, int height) { return ResizeWithFormat(width, height, GetTextureFormat(), HasMipMap() ? kMipmapMask : kNoMipmap); }

	int GetTextureFormat() const { return m_TexData.format; }

	void Compress(bool dither);

	virtual void RebuildMipMap();

	virtual int CountMipmaps() const;

	ColorRGBAf GetPixelBilinear(int image, float u, float v) const;
	ColorRGBAf GetPixel(int image, int x, int y) const;
	void SetPixel(int image, int x, int y, const ColorRGBAf& c);

	// Read pixels. Set reversed when reading into a cubemap
	void ReadPixels(int frame, int left, int bottom, int width, int height, int destX, int destY, bool reversed, bool computeMipMap);

	bool GetPixels(int x, int y, int width, int height, int mipLevel, ColorRGBAf* data, int frame = 0) const;
	void SetPixels(int x, int y, int width, int height, int pixelCount, const ColorRGBAf* pixels, int miplevel, int frame = 0);


	// always whole mip level, into/from 32 bit RGBA colors
	// GetPixels32 also supports getting pixels from DXT textures.
	// For DXT textures the output width/height must have minimum of 4.
	bool GetPixels32(int mipLevel, ColorRGBA32* data) const;
	void SetPixels32(int mipLevel, const ColorRGBA32* pixels, const int pixelCount);

	// Encodes to PNG bytes
	bool EncodeToPNG(dynamic_array<UInt8>& outBuffer);

	// Is reading the data from this texture allowed by webplayer security
	void SetReadAllowed(bool allowed) { m_ReadAllowed = allowed; if (!allowed) s_ScreenReadAllowed = false; }
	bool GetReadAllowed() const { return m_ReadAllowed; }

	void Apply(bool updateMipmaps, bool makeNoLongerReadable);

	static bool GetScreenReadAllowed() { return s_ScreenReadAllowed; }


	friend struct TemporaryTextureSerializationRevert;
};

void ConvertTextureEndianessWrite(int format, UInt8* src, UInt8* dst, int size, bool bBigEndianGPU);
void ConvertTextureEndianessRead(int format, UInt8* src, int size);
