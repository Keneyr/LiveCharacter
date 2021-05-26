#pragma once


extern "C"
{
	void _DllExport GenerateOutline(float detail, unsigned char alphaTolerance, bool holeDetection,
		std::vector<dynamic_array<Vector2f> >& outVertices, int extrudeOverride);
}