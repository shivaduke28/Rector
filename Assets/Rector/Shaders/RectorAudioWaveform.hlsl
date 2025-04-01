#ifndef RECTOR_AUDIO_WAVEFORM
#define RECTOR_AUDIO_WAVEFORM

StructuredBuffer<float> _RectorWaveform;
int _RectorWaveformSize = 512;

float RectorAudioWaveform(in uint index)
{
    return _RectorWaveform[index % _RectorWaveformSize];
}

uint RectorAudioWaveformSize()
{
    return _RectorWaveformSize;
}

#endif
