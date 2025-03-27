#ifndef RECTOR_AUDIO_SPECTRUM
#define RECTOR_AUDIO_SPECTRUM

StructuredBuffer<float> _AudioSpectrum;
#define RECTOR_AUDIO_SPECTRUM_BUFFER_SIZE 512

float AudioSpectrum(in uint index)
{
    return _AudioSpectrum[index % RECTOR_AUDIO_SPECTRUM_BUFFER_SIZE];
}

#endif
