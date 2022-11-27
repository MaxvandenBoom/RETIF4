
% correct data, realign
regData = ITKAlignImage(realignmentVolumeSingle, in, correctionVolumeReorient);

% store the global data
globalData = regData(logicalMask);
rtGlobalData = horzcat(rtData, globalData);

% store the roi data
roiData = regData(logicalMask);
rtData = horzcat(rtData, roiData);

% classify
%[predictedSampleLabel, ~] = multisvmclassify(SVMModel, regData');
%rtDataSampleClassifications = horzcat(rtDataSampleClassifications, predictedSampleLabel);
