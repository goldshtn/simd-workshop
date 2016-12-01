#include <nmmintrin.h>

extern "C" __declspec(dllexport) void set_intersect(short* a, short* b, int size, short* c)
{
	// __m128i va, vb;
	// __m128i res = _mm_cmpestrm(va, 8, vb, 8,
	//	_SIDD_UWORD_OPS | _SIDD_CMP_EQUAL_ANY | _SIDD_BIT_MASK);
	// Shuffle common elements, write out common elements, keep going:
	//	https://highlyscalable.wordpress.com/2012/06/05/fast-intersection-sorted-lists-sse/
}