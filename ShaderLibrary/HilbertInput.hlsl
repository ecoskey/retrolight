#ifndef RETROLIGHT_HILBERT_INPUT_INCLUDED
#define RETROLIGHT_HILBERT_INPUT_INCLUDED

ByteAddressBuffer HilbertIndices;

uint GetHilbertIndex(uint2 positionSS) {
    positionSS %= 64;
    const uint dispatchIndex = positionSS.y * 64 + positionSS.x;
    const uint bufferIndex = dispatchIndex / 2;
    const bool bufferOffset = dispatchIndex % 2;
    const uint rawIndex = HilbertIndices.Load(bufferIndex);
    return bufferOffset ? rawIndex >> 16 : rawIndex & 0xFFFF;
}

#endif