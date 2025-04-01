#ifndef RECTOR_AUDIO_SPECTRUM
#define RECTOR_AUDIO_SPECTRUM

StructuredBuffer<float> _RectorAudioSpectrum;
#define RECTOR_AUDIO_SPECTRUM_BUFFER_SIZE 512

float RectorAudioSpectrum(in uint index)
{
    return _RectorAudioSpectrum[index % RECTOR_AUDIO_SPECTRUM_BUFFER_SIZE];
}

#endif
