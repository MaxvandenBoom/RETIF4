# RETIF4
Real-time fMRI neurofeedback software 4.0

Open-source C# software that is used to provide feedback on brain activity during functional Magnetic Resonance Imaging (fMRI).

This software currently allows researchers to:
- Obtain and intelligently synchronize functional scan volumes and scan triggers from a fMRI scanner (supports 3T and 7T Philips Achieva scanners)
- Optionally correct incoming function scan volumes to a template
- Realign incoming functional scan volumes in real-time
- Define a general region-of-interest (ROI) for feedback based on a pre-defined MNI mask and apply this ROI in real-time to incoming scan volumes in native space (after automatic co-registration)
- Perform voxel/feature selection within a region-of-interest (ROI) for feedback, based on a localizer task (and automated GLM analysis)
- Calibrate and normalize the incoming signal to a range of activation and/or perform classification
- Smooth the incoming signal over time
- Provide neurofeedback with accurately timed processing steps and real-time updates to the task presentation

## Build and Run

1. Clone or download (and extract) this repository

2. Unpack `/RETIF4/libs/SimpleITKCSharp_libs.zip`
which should extract to:
```
/RETIF4/libs/win32/SimpleITKCSharpManaged.dll
/RETIF4/libs/win32/SimpleITKCSharpNative.dll
/RETIF4/libs/win64/SimpleITKCSharpManaged.dll
/RETIF4/libs/win64/SimpleITKCSharpNative.dll
```

3. Make sure Matlab (>= 2018) is installed, and place a copy of SPM12 in `/RETIF4/matlab/spm12`

4. Open the solution file `RETIF4.sln` in Visual Studio (tested in 2017), build the `RETIF4` project and execute


## Studies
- Feedback on dorsolateral prefrontal cortex (DLPFC): Van den Boom, M. A., Jansma, J. M., & Ramsey, N. F. (2018). Rapid acquisition of dynamic control over DLPFC using real-time fMRI feedback. European Neuropsychopharmacology, 28(11), 1194-1205.


## Acknowledgements

- Written by Max van den Boom (University Medical Center Utrecht, NL; Multimodal Neuroimaging Lab, Mayo Clinic, USA)
- Contributors: Mark Bruurmijn, Tim Varkevisser, Nadia Leen (University Medical Center Utrecht, Utrecht, NL)
