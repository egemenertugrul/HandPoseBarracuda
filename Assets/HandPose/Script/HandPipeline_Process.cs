using System.Linq;
using UnityEngine;
using Unity.Mathematics;

namespace MediaPipe.HandPose {

//
// Image processing part of the hand pipeline class
//

partial class HandPipeline
{
    // Hand region tracker
    HandRegion _handRegion = new HandRegion();

    void RunPipeline(Texture input)
    {
        // hand detection
        _detector.palm.ProcessImage(input);

        // Cancel if the hand detection score is too low.
        var palm = _detector.palm.Detections.FirstOrDefault();
        if (palm.score < 0.5f) return;

        // Update the hand region.
        _handRegion.Update(palm);

        // Hand region cropping
        _preprocess.SetMatrix("_Xform", _handRegion.CropMatrix);
        Graphics.Blit(input, _cropRT, _preprocess, 0);

        // Hand landmark detection
        _detector.landmark.ProcessImage(_cropRT);

        // Postprocess for hand mesh construction
        var post = _resources.postprocessCompute;

        post.SetMatrix("_fx_xform", _handRegion.CropMatrix);
        post.SetBuffer(0, "_fx_input", _detector.landmark.OutputBuffer);
        post.SetBuffer(0, "_fx_output", _postBuffer);
        post.Dispatch(0, 1, 1, 1);
    }
}

} // namespace MediaPipe.HandPose
