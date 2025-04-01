#ifndef RECTOR_AUDIO_WAVEFORM
#define RECTOR_AUDIO_WAVEFORM

StructuredBuffer<float> _RectorAudioWaveform;
int _RectorAudioWaveformSize = 512;

float RectorAudioWaveform(in uint index)
{
    return _RectorAudioWaveform[index % _RectorAudioWaveformSize];
}

uint RectorAudioWaveformSize()
{
    return _RectorAudioWaveformSize;
}

#endif
