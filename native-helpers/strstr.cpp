#include <intrin.h>
#include <memory.h>

// Source: https://github.com/WojciechMula/sse4-strstr (licensed under BSD 2-clause)
namespace bits {
    template <typename T>
    T clear_leftmost_set(const T value) {
        return value & (value - 1);
    }

    unsigned get_first_bit_set(const unsigned value) {
		unsigned long r = 0;
		_BitScanReverse(&r, value);
		return r;
    }
} // namespace bits

extern "C" __declspec(dllexport) int str_str(char* haystack, char* needle, int hs_size, int needle_size)
{
    const __m256i first = _mm256_set1_epi8(needle[0]);
    const __m256i last  = _mm256_set1_epi8(needle[needle_size - 1]);

	for (size_t i = 0; i < hs_size; i += 32) {

        const __m256i block_first = _mm256_loadu_si256(reinterpret_cast<const __m256i*>(haystack + i));
        const __m256i block_last  = _mm256_loadu_si256(reinterpret_cast<const __m256i*>(haystack + i + needle_size - 1));

        const __m256i eq_first = _mm256_cmpeq_epi8(first, block_first);
        const __m256i eq_last  = _mm256_cmpeq_epi8(last, block_last);

        unsigned mask = _mm256_movemask_epi8(_mm256_and_si256(eq_first, eq_last));

        while (mask != 0) {
            const auto bitpos = bits::get_first_bit_set(mask);
            if (memcmp(haystack + i + bitpos + 1, needle + 1, needle_size - 2) == 0) {
				return (int)(i + bitpos);
            }
            mask = bits::clear_leftmost_set(mask);
        }
    }

	return -1;
}

