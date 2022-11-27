
skipStartVolumes = 5;
searchScansBeforePeak = 2;
searchScansAfterPeak = 3;
discardTrialsBelow = 0.5;
bad_trials = [];

% retrieve the number of scans
numberOfVols = size(volumeData, 4);

%
% HRF
%

% generate the HRF
hrf = generateHRF(tr, onsets_Seconds, numberOfVols);

% retrieve the valleys and peaks of each trial from the HRF
[valleyIndices, peakIndices, indicesConditions] = cls_getFeatureScansFromHrf(conditions, hrf, skipStartVolumes);


% update the HRF peaks to peaks based on the (average masked) signal
% note: masking, normalization and detrending also takes place in this function
[signalPeakIndices, signalPeakIndicesConditions, signal, signalMean] = cls_getFeaturePeaks(logicalMask, volumes, peakIndices, indicesConditions);
%logicalMask;
%logicalGlobalMask;

% exclude bad trials by low template correlation
[signalPeakIndices, signalPeakIndicesConditions] = cls_excludeTrialsByCorrelation(signal, signalPeakIndices, signalPeakIndicesConditions);

% display signal + hrf with peaks
%figure
%hrfDisplay = hrf / std(hrf) * (std(signalMean) * 1.6);
%hrfDisplay = hrfDisplay - (((max(hrfDisplay) - min(hrfDisplay)) / 2 + min(hrfDisplay)) - ((max(signalMean) - min(signalMean)) / 2 + min(signalMean)));
%signalDisplayPeak = nan(1, length(signalMean));
%signalDisplayPeak(signalPeakIndices) = signalMean(signalPeakIndices);
%plot(hrfDisplay,'DisplayName','hrfDisplay', 'color', [1 .7 .4]);
%hold on;
%plot(signalMean,'DisplayName','signalMean', 'color', [0 0 1]);
%plot(signalDisplayPeak,'o','DisplayName','signalDisplayPeak', 'color', [1 0 0]);
%hold off;


% loop through the trials
numCorrect = 0;
for i=1:size(signalPeakValues, 2)

	% copy the trials
	oneOutTrials =  signal(:,signalPeakIndices);
	oneOutTrialsConditions = signalPeakIndicesConditions;

	% retrieve the test trial
	testTrial = oneOutTrials(:,i);
	testTrialCondition = signalPeakIndicesConditions(i);

	% remove the test trial from the matrix
	oneOutTrials(:,i) = [];
	oneOutTrialsConditions(:,i) = [];

	% create the templates
	sym0Template = oneOutTrials(:,(oneOutTrialsConditions == 0));
	sym1Template = oneOutTrials(:,(oneOutTrialsConditions == 1));
	sym2Template = oneOutTrials(:,(oneOutTrialsConditions == 2));
	sym0Template = mean(sym0Template, 2);
	sym1Template = mean(sym1Template, 2);
	sym2Template = mean(sym2Template, 2);
	
	
	% correlate the trial with all templates
	crosscorr_sym0 = corr(testTrial, sym0Template);
	crosscorr_sym1 = corr(testTrial, sym1Template);
	crosscorr_sym2 = corr(testTrial, sym2Template);
	
	if (crosscorr_sym0 < 0)
		crosscorr_sym0 = crosscorr_sym0 * -1;
	end
	if (crosscorr_sym1 < 0)
		crosscorr_sym1 = crosscorr_sym1 * -1;
	end    
	if (crosscorr_sym2 < 0)
		crosscorr_sym2 = crosscorr_sym2 * -1;
	end
	
	[~,predictedLabel] = max([crosscorr_sym0, crosscorr_sym1, crosscorr_sym2]);
	predictedLabel = predictedLabel - 1;

	% count correct
	if (testTrialCondition == predictedLabel)
		numCorrect = numCorrect + 1;
	end
	
end
percCorrect = numCorrect / size(signalPeakValues, 2) * 100;
disp(['accuracy: ', num2str(percCorrect), ' %']);

%{
% display signal + hrf with peaks
hrfDisplay = hrf / std(hrf) * (std(signalMean) * 1.6);
hrfDisplay = hrfDisplay - (((max(hrfDisplay) - min(hrfDisplay)) / 2 + min(hrfDisplay)) - ((max(signalMean) - min(signalMean)) / 2 + min(signalMean)));
hrfDisplayPeak = nan(1, length(hrfDisplay));
hrfDisplayPeak(hrfPeakIndices) = hrfDisplay(hrfPeakIndices);
signalDisplayPeak = nan(1, length(signal));
signalDisplayPeak(signalPeakIndices) = signalMean(signalPeakIndices);
plot(hrfDisplay,'DisplayName','hrfDisplay', 'color', [1 .7 .4]);
hold on;
plot(hrfDisplayPeak,'o','DisplayName','hrfDisplayPeak');
plot(signalMean,'DisplayName','signalMean', 'color', [0 0 1]);
plot(signalDisplayPeak,'o','DisplayName','signalDisplayPeak');
hold off;
%}



% create the templates
trainTrials = signal(:,signalPeakIndices);
sym0Template = trainTrials(:,(signalPeakIndicesConditions == 0));
sym1Template = trainTrials(:,(signalPeakIndicesConditions == 1));
sym2Template = trainTrials(:,(signalPeakIndicesConditions == 2));
sym0Template = mean(sym0Template, 2);
sym1Template = mean(sym1Template, 2);
sym2Template = mean(sym2Template, 2);

