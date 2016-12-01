#include <nmmintrin.h>
#include <memory.h>

// Adapted from https://highlyscalable.wordpress.com/2012/06/05/fast-intersection-sorted-lists-sse/

static __m128i shuffle_mask[16];

int getBit(int value, int position) {
    return ( ( value & (1 << position) ) >> position);
}

void prepare_shuffling_dictionary() {
    for(int i = 0; i < 16; i++) {
        int counter = 0;
        char permutation[16];
        memset(permutation, 0xFF, sizeof(permutation));
        for(char b = 0; b < 4; b++) {
            if(getBit(i, b)) {
                permutation[counter++] = 4*b;
                permutation[counter++] = 4*b + 1;
                permutation[counter++] = 4*b + 2;
                permutation[counter++] = 4*b + 3;
            }
        }
        __m128i mask = _mm_loadu_si128((const __m128i*)permutation);
        shuffle_mask[i] = mask;
    }
}
 
extern "C" __declspec(dllexport) size_t set_intersect(short* A, short* B, int size, short* C)
{
    size_t count = 0;
    size_t i_a = 0, i_b = 0;
 
    while(i_a < size && i_b < size) {
        __m128i v_a = _mm_loadu_si128((__m128i*)&A[i_a]);
        __m128i v_b = _mm_loadu_si128((__m128i*)&B[i_b]);
 
        __m128i res_v = _mm_cmpestrm(v_b, 8, v_a, 8,
                _SIDD_UWORD_OPS|_SIDD_CMP_EQUAL_ANY|_SIDD_BIT_MASK);
        int r = _mm_extract_epi32(res_v, 0);
        __m128i p = _mm_shuffle_epi8(v_a, shuffle_mask[r]); // TODO Verify shuffle mask
        _mm_storeu_si128((__m128i*)&C[count], p);
        count += _mm_popcnt_u32(r);
 
        short a_max = _mm_extract_epi16(v_a, 7);
        short b_max = _mm_extract_epi16(v_b, 7);
        i_a += (a_max <= b_max) * 4;
        i_b += (a_max >= b_max) * 4;
    }
 
	// TODO Take care of the tail 
    return count;
}
