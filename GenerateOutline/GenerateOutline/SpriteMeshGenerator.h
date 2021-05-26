#pragma once
#include "dynamic_bitset.h"
#include <vector>
#include "Vector2.h"

class SpriteMeshGenerator
{
public:
	struct s_cost
	{
		s_cost() {};
		s_cost(float _c, float _w)
		{
			c = _c;
			w = _w;
		};
		float  c;
		float  w;
	};

	struct vertex
	{
		vertex() {};
		vertex(Vector2f pos)
		{
			p = pos;
		};
		Vector2f p;
		Vector2f n;    // normal
		int  s;        // sign -> indicating concavity 曲线凹度
		float c;
		struct s_cost cost;
	};

	class path_segment
	{
	public:
		path_segment(std::vector<vertex> path, int i0, int i1)
		{
			m_i0 = i0;
			m_i1 = i1;
			m_mx = max_distance(path, i0, i1);
		};

		int m_i0;
		int m_i1;
		int m_mx;
		int m_cnt;
	private:
		int max_distance(std::vector<vertex> path, int i0, int i1);
	};

	class compare_path_segment {
	public:
		bool operator()(path_segment& s0, path_segment& s1)
		{
			return (s0.m_cnt < s1.m_cnt);
		}
	};

	class path
	{
	public:
		path() {};
		path(const std::vector<vertex>& p, int w, int h, int sign, float area, float bias)
		{
			m_bx = w;
			m_by = h;
			m_area = area;
			m_sign = sign;
			m_path = p;
			opt(bias);
			bbox();
			m_path0 = m_path;
		}

		std::vector<vertex>   m_path;

		void   bbox();
		void   simplify(float q, int mode);

		bool   isHole() const { return m_sign == '-'; }

		const Vector2f& GetMin() const { return m_min; }
		const Vector2f& GetMax() const { return m_max; }

	private:
		int find_max_distance(int i0);

		void   fit(std::vector<int>& ci, int i0, int i1);
		bool   opt(float bias);

		bool   dec(int i);
		bool   inf(int i);
		bool   select();
		bool   cvx_cost(int i);
		bool   cve_cost(int i);
		int    min_cost();
		int    self_intersect(Vector2f p0, Vector2f p1);

		void     clip();
		void     clip_edge(int e);
		bool     clip_test(Vector2f p, int side);
		Vector2f clip_isec(Vector2f p, Vector2f q, int e);

		int                   m_bx;
		int                   m_by;
		int                   m_sign;
		float                 m_area;
		Vector2f              m_min;
		Vector2f              m_max;
		std::vector<vertex>   m_path0;
		std::vector<int>      m_invalid;
	};

	struct mask
	{
		mask() {};
		//根据alpha通道进行二值化+膨胀
		mask(ColorRGBA32 *img, int width, int height, unsigned char acut, unsigned int dn)
		{
			w = width;
			h = height;
			int n = w * h;
			dynamic_bitset bv;
			bv.resize(n);
			for (int y = 0; y < height; y++)
				for (int x = 0; x < width; x++) {
					if (img[x + y * width].a > acut)
						bv.set(x + y * w);
				}

			if (dn)
				this->dilate(dn, bv);

			w += 1;
			h += 1;
			m_bv.resize(w*h);
			for (int y = 0; y < height; y++)
				for (int x = 0; x < width; x++) {
					if (bv.test(x + y * width)) {
						m_bv.set((x + 0) + (y + 0)*w);
						m_bv.set((x + 1) + (y + 1)*w);
						m_bv.set((x + 0) + (y + 1)*w);
						m_bv.set((x + 1) + (y + 0)*w);
					}
				}
		}

		bool tst(int x, int y)
		{
			if ((x < 0) || (x >= w) ||
				(y < 0) || (y >= h))
				return 0;
			int i = x + y * w;
			return m_bv.test(i);
		}

		void set(int x, int y)
		{
			if ((x < 0) || (x >= w) ||
				(y < 0) || (y >= h))
				return;
			int i = x + y * w;
			m_bv.set(i);
		}

		void rst(int x, int y)
		{
			if ((x < 0) || (x >= w) ||
				(y < 0) || (y >= h))
				return;
			int i = x + y * w;
			m_bv[i] = 0;
		}

		void inv(int x, int y)
		{
			if ((x < 0) || (x >= w) ||
				(y < 0) || (y >= h))
				return;
			int i = x + y * w;
			m_bv[i].flip();
		}

		int first()
		{
			int n = (int)m_bv.size();
			for (int i = 0; i < n; i++)
				if (m_bv.test(i))
					return i;
			return -1;
		}

		bool dilate(int n, dynamic_bitset &bv)
		{
			if ((w == 0) || (h == 0))
				return false;
			UInt32 *md = new UInt32[w*h];
			if (!mdist(md, bv))
				return false;

			for (int i = 0; i < w*h; i++) {
				if (md[i] <= n)
					bv.set(i);
			}
			delete md;
			return true;
		}
		int w;
		int h;
		dynamic_bitset m_bv;
	private:
		bool mdist(UInt32 *md, dynamic_bitset& bv)
		{
			if (!md)
				return false;

			for (int y = 0; y < h; y++)
				for (int x = 0; x < w; x++) {
					int i = x + y * w;
					if (bv.test(i))
						md[i] = 0;
					else {
						md[i] = w + h;
						if (y > 0) md[i] = min(md[i], md[i - w] + 1);
						if (x > 0) md[i] = min(md[i], md[i - 1] + 1);
					}
				}

			for (int y = h - 1; y >= 0; y--)
				for (int x = w - 1; x >= 0; x--) {
					int i = x + y * w;
					if ((y + 1) < h) md[i] = min(md[i], md[i + w] + 1);
					if ((x + 1) < w) md[i] = min(md[i], md[i + 1] + 1);
				}
			return true;
		}
	};

public:
	void Decompose(std::vector<Vector2f>* vertices, std::vector<int>* indices);
	void MakeShape(ColorRGBA32* image, int imageWidth, int imageHeight, float hullTolerance, unsigned char alphaTolerance, bool holeDetection, unsigned int extrude, float bias, int mode);
	bool FindBounds(Rectf& bounds);

	const std::vector<path>& GetPaths() const { return m_paths; }


private:
	bool trace(Vector2f p0, Vector2f p1, Vector2f &p);
	bool invmask(std::vector<vertex>& outline);
	bool contour(std::vector<vertex>& outline, int &sign, float &area);

	std::vector<path> m_paths;

	float evaluateLOD(const float areaHint, float area);

	struct mask       m_mask_org;
	struct mask       m_mask_cur;

	//int width,height;
	void SaveOutlineAsImageToFile(std::vector<vertex>& outline, int imageWidth, int imageHeight, const std::string& path);
	void SavePathsAsImageToFile(std::vector<path>& m_paths, int imageWidth, int imageHeight, const std::string& path);
};
