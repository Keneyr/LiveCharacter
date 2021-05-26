#pragma once

#include <algorithm>

class Vector2f
{
public:
	float x, y;

	DECLARE_SERIALIZE_OPTIMIZE_TRANSFER(Vector2f)

	Vector2f() : x(0.f), y(0.f) {}
	Vector2f(float inX, float inY) { x = inX; y = inY; }
	explicit Vector2f(const float* p) { x = p[0]; y = p[1]; }

	void Set(float inX, float inY) { x = inX; y = inY; }

	float* GetPtr() { return &x; }
	const float* GetPtr()const { return &x; }
	float& operator[] (int i) { DebugAssertIf(i < 0 || i > 1); return (&x)[i]; }
	const float& operator[] (int i)const { DebugAssertIf(i < 0 || i > 1); return (&x)[i]; }

	Vector2f& operator += (const Vector2f& inV) { x += inV.x; y += inV.y; return *this; }
	Vector2f& operator -= (const Vector2f& inV) { x -= inV.x; y -= inV.y; return *this; }
	Vector2f& operator *= (const float s) { x *= s; y *= s; return *this; }
	Vector2f& operator /= (const float s) { DebugAssertIf(CompareApproximately(s, 0.0F)); x /= s; y /= s; return *this; }
	bool operator == (const Vector2f& v)const { return x == v.x && y == v.y; }
	bool operator != (const Vector2f& v)const { return x != v.x || y != v.y; }


	Vector2f operator - () const { return Vector2f(-x, -y); }

	Vector2f& Scale(const Vector2f& inV) { x *= inV.x; y *= inV.y; return *this; }

	static const float		epsilon;
	static const float		infinity;
	static const Vector2f	infinityVec;
	static EXPORT_COREMODULE const Vector2f	zero;
	static const Vector2f	xAxis;
	static const Vector2f	yAxis;
};